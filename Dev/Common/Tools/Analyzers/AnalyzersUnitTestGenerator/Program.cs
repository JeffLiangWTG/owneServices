using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.BuildTools;
using CargoWise.StaticAnalysis;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

[assembly: AssemblyTitle("Analyzers Unit Test Generator")]

namespace AnalyzersUnitTestGenerator
{
	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer only tool")]
	class Program
	{
		static void Main(string[] args)
		{
			var excludedDirectories = new[]
			{
				"EdiFact",
				"ThirdParty",
				"FlexCel",

				//found under Enterprise\Architecture\GUI.UserControls
				Path.Combine(Path.DirectorySeparatorChar.ToString(), "Aga.")
			};

			var isAlpha = IsAlpha;
			var sourcePath = TestCase.BaseSourcePath;
			var winzorFolder = Path.Combine(WTGBranchRoot, "Winzor");
			var buildXml = new BuildXml(BuildXml.Instance.BuildXmlFileName);
			var projects = buildXml
				.GetAllSolutionFileNames()
				.SelectMany(solutionFileName => GetProjectFileNames(Path.Combine(sourcePath, solutionFileName)))
				.Where(projectFileFullPath =>
					!excludedDirectories.Any(directory => projectFileFullPath.IndexOf(directory, StringComparison.OrdinalIgnoreCase) > -1)
					//handled separately in RunAnalyzers.TestIntentionalFailures
					&& !projectFileFullPath.EndsWith(@"\AnalyzersRunner.FunctionalTestingTarget.csproj", StringComparison.OrdinalIgnoreCase)
				)
				.Select(p => p.Replace(sourcePath, string.Empty))
				.ToArray();
			Emit.GenerateTestMethods(args[0], "AnalyzersRunner.RunAnalyzers", "AnalyzersRunner.ProcessRunner", "AssertNoIssuesOnProject", projects);
		}

		static string[] GetProjectFileNames(string solutionFile)
		{
			var solution = new CSharpSolution(solutionFile);
			var solutionPath = Path.GetDirectoryName(solutionFile);
			return solution.Projects.Cast<string>().Select(p => Path.Combine(solutionPath, p)).ToArray();
		}

		static bool IsAlpha => new ReleaseInfo(ReleaseInfoXML).ReleaseRing == WTG.DevTools.Definitions.ReleaseRings.Codes.ALP;

		static string WTGBranchRoot => BuildConstants.GetLocalEnterprisePath(Assembly.GetExecutingAssembly().Location);

		static string ReleaseInfoXML => $"{WTGBranchRoot}{ReleaseInfo.XmlFileName}";
	}
}
