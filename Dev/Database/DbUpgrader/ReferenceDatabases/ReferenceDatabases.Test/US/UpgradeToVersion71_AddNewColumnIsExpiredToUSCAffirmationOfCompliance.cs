using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion71_AddNewColumnIsExpiredToUSCAffirmationOfCompliance : USReferenceDbUpgraderVersionTest
	{
		protected override void AssertUpgradeResult()
		{
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select 1 from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = 'USCAffirmationOfCompliance' and COLUMN_NAME = 'UL_IsExpired' and IS_NULLABLE = 'NO'"));
		}

		protected override int LatestVersionNumber
		{
			get { return 71; }
		}
	}
}
