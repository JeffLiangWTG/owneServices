using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion124_USCDataVersionTest : USReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber => 124;

		protected override void PrepareTestData(DbConnection conn)
		{
			TestHelper.CreateTables(conn, new ITableScript[] { new USCDataVersion() });
			conn.ExecuteNonQuery(@"UPDATE dbo.USCDataVersion SET UZ_Version = '2501', UZ_Note = 'Test!', UZ_UpdateTime = '2024-03-21 23:06:22.837' WHERE UZ_Name = 'LastHTSAttempt'");
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals(0, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(0) FROM dbo.USCDataVersion WHERE UZ_Version like '25__' AND UZ_Name = 'LastHTSAttempt'"));
		}
	}
}
