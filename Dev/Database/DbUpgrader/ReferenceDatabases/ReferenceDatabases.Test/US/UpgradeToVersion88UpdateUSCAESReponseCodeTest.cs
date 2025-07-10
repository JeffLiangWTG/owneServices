using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion88UpdateUSCAESReponseCodeTest : USReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber
		{
			get { return 88; }
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals(607, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(*) FROM USCAESResponseCode"));
		}
	}
}
