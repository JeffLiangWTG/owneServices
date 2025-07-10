using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion26Test : USReferenceDbUpgraderVersionTest
	{
		protected override void AssertUpgradeResult()
		{
			string sqlText = string.Format(@"
				select count(1)
					from [{0}].INFORMATION_SCHEMA.COLUMNS
					where TABLE_NAME = 'USCQuota'
					and COLUMN_NAME = 'UT_LastTrasactionDate'
					and IS_NULLABLE = 'YES'", refDbUpgrader.DbName);

			AssertEquals("Column 'UT_LastTrasactionDate' should be nullable", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sqlText));
		}

		protected override int LatestVersionNumber
		{
			get { return 26; }
		}
	}
}
