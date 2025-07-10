using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Common.Testing;

sealed class ProjectReferenceTest : TestCase
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestProjectReference()
	{
		CombineAssertions(() =>
		{
			AssertProjectReference(new[] { "Enterprise.Customs.JP.Common" }, new[] { "Enterprise.Customs.JP.Business", "Enterprise.Customs.JP.Manifest" });
			AssertProjectReference(new[] { "Enterprise.Customs.JP.Business" }, new[] { "Enterprise.Customs.JP.AFR", "Enterprise.Customs.JP.Manifest" });
			AssertProjectReference(new[] { "Enterprise.Customs.JP.Manifest" }, new[] { "Enterprise.Customs.JP.AFR", "Enterprise.Customs.JP.Business" });
			AssertProjectReference(new[] { "Enterprise.Customs.JP.AFR" }, new[] { "Enterprise.Customs.JP.Business", "Enterprise.Customs.JP.Manifest" });
		});
	}

	void AssertProjectReference(string[] projectNames, string[] wrongProjectReferences)
	{
		var allSolutions = new[]
		{
			@"Enterprise\Product\Operations\Customs\JP\Common\JP.Common.sln",
			@"Enterprise\Product\Operations\Customs\JP\Core\JP.sln",
			@"Enterprise\Product\Operations\Customs\JP\Manifest\JP.Manifest.sln",
			@"Enterprise\Product\Operations\Customs\JP\AFR\JP.AFR.sln",
		};

		foreach (var solution in allSolutions)
		{
			var solutionPath = Path.Combine(BaseSourcePath, solution);

			foreach (var projectPath in GetProjectsFromSolution(solutionPath))
			{
				if (projectNames.Any(c => projectPath.Contains(c)))
				{
					try
					{
						var fileContent = File.ReadAllText(projectPath);

						foreach (var wrongProjectReference in wrongProjectReferences)
						{
							Assert($"Project {projectPath} contains wrong project reference {wrongProjectReference}", !fileContent.Contains(wrongProjectReference));
						}
					}
					catch (ArgumentException ex)
					{
						Assert($"Error processing project. Path: {projectPath}. Exception Message: {ex.Message}", false);
					}
				}
			}
		}
	}

	static IEnumerable<string> GetProjectsFromSolution(string solutionPath)
	{
		using (var reader = File.OpenText(solutionPath))
		{
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				if (line.StartsWith("Project"))
				{
					var projectPath = line.Split(',')[1].Trim(' ', '\t', '"');
					var fullPath = Path.Combine(Path.GetDirectoryName(solutionPath), projectPath);

					if (File.Exists(fullPath))
					{
						yield return fullPath;
					}
				}
			}
		}
	}
}
