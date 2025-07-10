using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Accounting.Business.Testing
{
	sealed class AccountingReflectionTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoReferenceToNUnitInProductionProjects()
		{
			var solutions = CargoWise.BuildTools.BuildXml.Instance.GetAllSolutionFileNames().Where(p => p.Contains(RootDirectory));

			CombineAssertions(() =>
			{
				foreach (var solution in solutions)
				{
					try
					{
						var solutionFilePath = Path.Combine(BaseSourcePath, solution);
						var solutionFile = new SolutionFile(solutionFilePath);
						var solutionFolderPath = Directory.GetParent(solutionFilePath).FullName;
						var projectsToCheck = solutionFile.Projects.Where(x => IsProductionProject(x));

						foreach (var project in projectsToCheck)
						{
							var projectFilePath = Path.Combine(solutionFolderPath, project.ProjectPath);
							var content = File.ReadAllText(projectFilePath).ToUpperInvariant();
							AssertEquals($"Project {projectFilePath} should NOT refer to NUnitCore nor NUnit", false, content.Contains("NUNITCORE.") || content.Contains("NUNIT."));
						}
					}
					catch (Exception e)
					{
						Fail($"An exception occured during the test: {e.Message}");
					}
				}
			});
		}

		bool IsProductionProject(ProjectInformation project) => project.ProjectPath.EndsWith(".csproj", StringComparison.InvariantCultureIgnoreCase) && !project.ProjectName.ToUpperInvariant().Contains(".TESTING");
		const string RootDirectory = @"Enterprise\Product\Operations\Accounting\";
	}
}
