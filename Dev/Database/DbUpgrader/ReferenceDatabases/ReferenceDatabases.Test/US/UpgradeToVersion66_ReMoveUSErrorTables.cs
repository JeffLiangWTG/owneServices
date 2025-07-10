using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion66_ReMoveUSErrorTables : USReferenceDbUpgraderVersionTest
	{
		protected override void AssertUpgradeResult()
		{
			AssertEquals(0, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(*) FROM sys.objects where name = 'USCABIError' and type_desc = 'USER_TABLE'"));
			AssertEquals(0, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(*) FROM sys.objects where name = 'USCACEError' and type_desc = 'USER_TABLE'"));
		}

		protected override int LatestVersionNumber
		{
			get { return 66; }
		}
	}
}
