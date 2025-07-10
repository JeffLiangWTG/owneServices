using System;
using System.Globalization;
using System.IO;
using Dat.Integration;
using Dat.Integration.SpecialFileHandling;
using Dat.Integration.VersionControl;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Dat.SpecialFileHandling.Testing
{
	class BiVersionChangeRequestTest : TestCase
	{
		#region Power BI

		public void TestAnalyticsReportsTriggerPowerBiVersionChangeGit()
		{
			TestPowerBiReportsTriggerPowerBiVersionChange(
				RepositoryKey.ForGit("http://tfs.wtg.zone:8080/tfs/CargoWise/CW/_git/Dev", "master", "/").ForDat(),
				"/BusinessIntelligence/CargoWiseBi/CargoWiseBi.PBIRS/Reports/Logistics/Test Report.pbix",
				"/BusinessIntelligence/BiIntegration/Deployment/CargoWiseBiDeployment/ReportingServices/AnalyticsReport/AnalyticsReportProjectVersion.cs");
		}

		void TestPowerBiReportsTriggerPowerBiVersionChange(IRepositoryKey repositoryKey, string powerBiTestPbixFileServerPath, string powerBiVersionFileServerPath)
		{
			var formattedDate = DateTime.UtcNow.ToString("yyMMdd", CultureInfo.InvariantCulture);

			var schemaVersionFileContents = string.Format(Culture.Invariant, @"doesntmatterwhatishere Application = new VersionLabel({0}00, 0);	nor here", formattedDate);

			using (var powerBiProjectVersionFile = TempFile.New())
			{
				File.WriteAllText(powerBiProjectVersionFile.Filename, schemaVersionFileContents);

				var workspace = new Mock<IWorkspaceAccess>(MockBehavior.Strict);
				workspace.Setup(m => m.GetPendingChanges()).Returns(new[]
				{
					new MockPendingChange(TfsChangeType.Edit, powerBiTestPbixFileServerPath),
				});
				workspace
					.Setup(m => m.GetLocalItemForServerItem(powerBiVersionFileServerPath))
					.Returns(powerBiProjectVersionFile.Filename);

				var versionChangeRequest = new AnalyticsReportVersionChangeRequestForTesting(new SpecialFileHandlerContext(repositoryKey));
				versionChangeRequest.BumpVersion_Exposed(workspace.Object, powerBiVersionFileServerPath);

				var expectedFileContent = string.Format(CultureInfo.InvariantCulture, @"doesntmatterwhatishere Application = new VersionLabel({0}01, 0);	nor here", formattedDate);
				var actualFileContent = File.ReadAllText(powerBiProjectVersionFile.Filename);

				AssertEquals("Power BI Project version file", expectedFileContent, actualFileContent);
			}
		}

		public void TestPowerBiReportsTriggerPowerBiVersionChange_DifferentPathCase()
		{
			var powerBiTestPbixFile1ServerPath = "$/Dev/BusinessIntelligence/CargoWiseBi/CargoWiseBi.PBIRS/Reports/Logistics/Test Report 1.pbix"; // Edited
			var powerBiTestPbixFile2ServerPath = "$/Dev/BUSINESSINTELLIGENCE/CargoWiseBi/CargoWiseBi.PBIRS/Reports/Logistics/Test Report 2.pbix"; // Edited

			var versionChangeRequest = new AnalyticsReportVersionChangeRequestForTesting(new SpecialFileHandlerContext(RepositoryKey.ForGit("http://tfs.wtg.zone:8080/tfs/CargoWise/CW/_git/Dev", "master", "/").ForDat()));
			AssertEquals($"Trigger version change for [{powerBiTestPbixFile1ServerPath}]", true, versionChangeRequest.TriggersVersionChange_Exposed(powerBiTestPbixFile1ServerPath));
			AssertEquals($"Trigger version change for [{powerBiTestPbixFile2ServerPath}]", true, versionChangeRequest.TriggersVersionChange_Exposed(powerBiTestPbixFile2ServerPath));
		}

		public void TestPowerBiReportsTriggerPowerBiVersionDoesNotChange_AuditPath()
		{
			var powerBiTestPbixFile1ServerPath = "$/Dev/BusinessIntelligence/CargoWiseBi/CargoWiseBi.PBIRS/Reports/Audit/Test Report 1.pbix"; // Edited
			var powerBiTestPbixFile2ServerPath = "$/Dev/BUSINESSINTELLIGENCE/CargoWiseBi/CargoWiseBi.PBIRS/Reports/Audit/Test Report 2.pbix"; // Edited

			var versionChangeRequest = new AnalyticsReportVersionChangeRequestForTesting(new SpecialFileHandlerContext(RepositoryKey.ForGit("http://tfs.wtg.zone:8080/tfs/CargoWise/CW/_git/Dev", "master", "/").ForDat()));
			AssertEquals($"Version change not triggered for [{powerBiTestPbixFile1ServerPath}]", false, versionChangeRequest.TriggersVersionChange_Exposed(powerBiTestPbixFile1ServerPath));
			AssertEquals($"Version change not triggered for [{powerBiTestPbixFile2ServerPath}]", false, versionChangeRequest.TriggersVersionChange_Exposed(powerBiTestPbixFile2ServerPath));
		}

		class AnalyticsReportVersionChangeRequestForTesting : AnalyticsReportVersionChangeRequest
		{
			public AnalyticsReportVersionChangeRequestForTesting(SpecialFileHandlerContext context)
				: base(context)
			{
			}

			public void BumpVersion_Exposed(IWorkspaceAccess workspace, string versionFileServerPath)
			{
				BumpVersion(workspace, versionFileServerPath);
			}

			public bool TriggersVersionChange_Exposed(string serverPath)
			{
				return TriggersVersionChange(serverPath);
			}
		}

		#endregion
	}
}
