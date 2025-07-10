using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion29UpdateUSCForeignCodeInxedTest : USReferenceDbUpgraderVersionTest
	{
		protected override void AssertUpgradeResult()
		{
			string sql = @"SELECT count(*)
											FROM sys.tables tab
											INNER JOIN sys.indexes ind ON tab.object_id = ind.object_id
										  WHERE tab.name = 'USCForeignPort' AND ind.name = 'NR_IX__UH_Code'
											AND is_unique = '1'";

			AssertEquals(1, (int)UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, sql));
		}

		protected override int LatestVersionNumber
		{
			get { return 29; }
		}
	}
}
