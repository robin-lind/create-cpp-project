using System;
using System.IO;
using System.Resources;
using System.Text;

namespace create_cpp_project
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.Write("Name: ");
			var projectName = Console.ReadLine();
			Console.Write("Vcpkg? ");
			var vcpkg = Console.ReadLine().ToLower().StartsWith('y');
			Console.Write("olcPGE? ");
			var olcPGE = Console.ReadLine().ToLower().StartsWith('y');

			//var projectName = "Test";
			//var vcpkg = true;
			//var olcPGE = true;

			var weirdGuid = Guid.NewGuid();
			var solutionGuid = Guid.NewGuid();
			var projectGuid = Guid.NewGuid();

			Directory.CreateDirectory($"{projectName}");
			Directory.CreateDirectory(@$"{projectName}\{projectName}");

			var sln = Resources.sln;
			sln = sln.Replace("{projectName}", projectName);
			sln = sln.Replace("{weirdGuid}", $"{{{weirdGuid.ToString().ToUpper()}}}");
			sln = sln.Replace("{projectGuid}", $"{{{projectGuid.ToString().ToUpper()}}}");
			sln = sln.Replace("{solutionGuid}", $"{{{solutionGuid.ToString().ToUpper()}}}");
			
			var projectNameClean = projectName.Replace('-', '_');
			var allowed = "_abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
			for (var i = 0; i < projectNameClean.Length; i++)
			{
				if (!allowed.Contains(projectNameClean[i]))
				{
					projectNameClean = projectNameClean.Replace(projectNameClean[i], '_');
				}
			}
			File.WriteAllText(@$"{projectName}\{projectName}.sln", sln);

			var vcxproj = Resources.vcxproj;
			vcxproj = vcxproj.Replace("{projectName}", projectName);
			vcxproj = vcxproj.Replace("{projectNameClean}", projectNameClean);
			vcxproj = vcxproj.Replace("{projectGuid}", $"{{{projectGuid.ToString().ToUpper()}}}");

			if (vcpkg)
			{
				vcxproj = vcxproj.Replace("VcpkgSplit", Resources.vcpkg);
				File.WriteAllText(@$"{projectName}\{projectName}\vcpkg.json", Resources.vcpkgManifest.Replace("{projectName}", projectName));
			}
			else
			{
				vcxproj = vcxproj.Replace("VcpkgSplit", "");
			}

			var cppBuilder = new StringBuilder();
			var hBuilder = new StringBuilder();
			var cppFilterBuilder = new StringBuilder();
			var hFilterBuilder = new StringBuilder();

			Action<string> addCpp = (name) =>
			{
				cppBuilder.AppendLine($"    <ClCompile Include=\"{name}.cpp\" />");
				cppFilterBuilder.AppendLine($"    <ClCompile Include=\"{name}.cpp\">");
				cppFilterBuilder.AppendLine("      <Filter>Source Files</Filter>");
				cppFilterBuilder.AppendLine("    </ClCompile>");
			};

			Action<string> addH = (name) =>
			{
				hBuilder.AppendLine($"    <ClCompile Include=\"{name}.h\" />");
				hFilterBuilder.AppendLine($"    <ClCompile Include=\"{name}.h\">");
				hFilterBuilder.AppendLine("      <Filter>Header Files</Filter>");
				hFilterBuilder.AppendLine("    </ClCompile>");
			};

			cppBuilder.AppendLine("  <ItemGroup>");
			hBuilder.AppendLine("  <ItemGroup>");
			cppFilterBuilder.AppendLine("  <ItemGroup>");
			hFilterBuilder.AppendLine("  <ItemGroup>");
			addCpp(projectName);
			addH("linalg");

			File.WriteAllText(@$"{projectName}\{projectName}\linalg.h", Resources.linalg);
			if (olcPGE)
			{
				var main = Resources.olcPGE_main.Replace("{projectName}", projectName);
				File.WriteAllText(@$"{projectName}\{projectName}\{projectName}.cpp", main);
				File.WriteAllText(@$"{projectName}\{projectName}\olcPixelGameEngine.h", Resources.olcPGE);
				File.WriteAllText(@$"{projectName}\{projectName}\olcPixelGameEngine.cpp", Resources.olcPGEcpp);
				
				addCpp("olcPixelGameEngine");
				addH("olcPixelGameEngine");
			}
			else
			{
				var main = Resources.main.Replace("{projectName}", projectName);
				File.WriteAllText(@$"{projectName}\{projectName}\{projectName}.cpp", main);
			}

			cppBuilder.Append("  </ItemGroup>");
			hBuilder.Append("  </ItemGroup>");
			cppFilterBuilder.Append("  </ItemGroup>");
			hFilterBuilder.Append("  </ItemGroup>");

			vcxproj = vcxproj.Replace("CppSplit", cppBuilder.ToString());
			vcxproj = vcxproj.Replace("HSplit", hBuilder.ToString());

			var filter = Resources.filters;
			filter = filter.Replace("{sourceGuid}", $"{{{Guid.NewGuid().ToString().ToUpper()}}}");
			filter = filter.Replace("{headerGuid}", $"{{{Guid.NewGuid().ToString().ToUpper()}}}");
			filter = filter.Replace("{resourceGuid}", $"{{{Guid.NewGuid().ToString().ToUpper()}}}");
			filter = filter.Replace("CppSplit", cppFilterBuilder.ToString());
			filter = filter.Replace("HSplit", hFilterBuilder.ToString());
			File.WriteAllText(@$"{projectName}\{projectName}\{projectName}.vcxproj.filters", filter);

			File.WriteAllText(@$"{projectName}\{projectName}\{projectName}.vcxproj", vcxproj);
		}
	}
}
