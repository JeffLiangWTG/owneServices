using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.CA.Testing
{
	sealed class UpgradeToVersion151Test : CAReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber => 151;
		protected override void AssertUpgradeResult()
		{
			AssertEquals("Document Types of 8031, 8030", 2,
				UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(1) FROM CACDocumentTypes WHERE FR_GovAgencyIDCode = 'ECCC' AND FR_Code in ('8030', '8031')"));
		}
	}
}
