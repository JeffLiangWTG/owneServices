using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion121_UpdateUSCTariffRuleTest : USReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber => 121;

		protected override void PrepareTestData(DbConnection conn)
		{
			TestHelper.CreateTables(conn, new ITableScript[] {
				new USCTariffRule(),
				new USCRuleSecondaryTariff()
				});

			string insertScript = @"
				INSERT INTO USCTariffRule(U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom) VALUES ('8D48D4AA-0233-40FD-8F24-59C3418DFC74', 'STN', '8205900000', '2000-01-01 00:00:00.000')
				INSERT INTO USCTariffRule(U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom) VALUES ('12EA0C89-B3DB-4934-A59E-4E0F377BA90B', 'STN', '8215200000', '2000-01-01 00:00:00.000')
				";

			conn.ExecuteNonQuery(insertScript);
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule where U1_RuleCode = 'STN' and U1_Tariff = '8205900000' and U1_DateTo = '2012-02-02'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule where U1_RuleCode = 'STN' and U1_Tariff = '8205901000'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule where U1_RuleCode = 'STN' and U1_Tariff = '8205906000'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '7013322090' and U3_U1 = '12EA0C89-B3DB-4934-A59E-4E0F377BA90B'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '8205' and U3_U1 = 'A284107D-AB67-4A51-BA3A-7D2FDCCE7A85'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '8205' and U3_U1 = '2C85F7E6-D444-447A-B6D8-49A82A70DA1D'"));
		}
	}
}
