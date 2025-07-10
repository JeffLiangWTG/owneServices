using System.IO;
using CargoWise.IO;
using NUnit.Framework;

namespace CargoWise.BuildTools.Testing
{
	sealed class CSharpSolutionTest : TestCase
	{
		public CSharpSolutionTest() : base()
		{
		}

		public void TestSolutionParsesProjectsProperly()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string testSolutionFile = resourceRetriever.SaveResourceToFile("TestSolutionFile.txt");
				CSharpSolution testSolution = new CSharpSolution(testSolutionFile);
				AssertEquals("Not all projects were loaded", 13, testSolution.Projects.Count); // should exclude the deployment project

				string baseDir = new FileInfo(testSolutionFile).DirectoryName + @"\";

				AssertProjectInSolution(@"ReportEngine\ReportEngine.csproj", baseDir, testSolution);
				AssertProjectInSolution(@"..\NUnitCore\NUnitCore.csproj", baseDir, testSolution);
				AssertProjectInSolution(@"..\UnitTest\UnitTest.csproj", baseDir, testSolution);
				AssertProjectInSolution(@"Enterprise\Enterprise.csproj", baseDir, testSolution);
				AssertProjectInSolution(@"DocumentPrinting\DocumentPrinting.csproj", baseDir, testSolution);
				AssertProjectInSolution(@"Core\Core.csproj", baseDir, testSolution);
				AssertProjectInSolution(@"ContainerManager\ContainerManager.csproj", baseDir, testSolution);
				AssertProjectInSolution(@"SharedComponents\SharedComponents.csproj", baseDir, testSolution);
				AssertProjectInSolution(@"SGCustoms\SGCustoms.csproj", baseDir, testSolution);
				AssertProjectInSolution(@"Customs\Customs.csproj", baseDir, testSolution);
				AssertProjectInSolution(@"LCLBooking\LCLBooking.csproj", baseDir, testSolution);
				AssertProjectInSolution(@"Setup\Setup.vdproj", baseDir, testSolution);
			}
		}

		#region Implementation

		void AssertProjectInSolution(string projectName, string baseDir, CSharpSolution solution)
		{
			string fullProjectPath = baseDir + projectName;
			bool projectFound = solution.Projects.Contains(fullProjectPath);
			Assert("Did not contain expected project - " + fullProjectPath, projectFound);
		}

		#endregion
	}
}
