using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion57AddMissingSchDPortsTest : USReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber
		{
			get { return 57; }
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(*) FROM USCRegionDistrictPort WHERE UR_Code = '1113'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(*) FROM USCRegionDistrictPort WHERE UR_Code = '4117'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(*) FROM USCRegionDistrictPort WHERE UR_Code = '4121'"));
		}
	}
}
