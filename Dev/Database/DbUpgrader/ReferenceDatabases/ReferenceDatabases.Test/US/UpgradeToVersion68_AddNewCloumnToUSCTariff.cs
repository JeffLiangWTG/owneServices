using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion68_AddNewCloumnToUSCTariff : USReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber
		{
			get { return 68; }
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from information_Schema.columns where column_Name = 'UE_PGACodes' and TABLE_NAME = 'USCTariff' "));
		}
	}
}
