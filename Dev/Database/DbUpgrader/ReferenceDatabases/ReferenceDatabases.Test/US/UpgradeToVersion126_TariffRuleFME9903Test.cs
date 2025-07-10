using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion126_TariffRuleFME9903Test : USReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber => 124;

		protected override void PrepareTestData(DbConnection conn)
		{
			TestHelper.CreateTables(conn, new ITableScript[] { new USCTariffRule() });
			conn.ExecuteNonQuery(@"DELETE dbo.USCTariffRule WHERE U1_RuleCode = 'FME' and U1_Tariff = '9903' and U1_TariffTo = '9904'");
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals(0, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT COUNT(U1_PK) FROM dbo.USCTariffRule WHERE U1_RuleCode = 'FME' and U1_Tariff = '9903' and U1_TariffTo = '9904'"));
		}
	}
}
