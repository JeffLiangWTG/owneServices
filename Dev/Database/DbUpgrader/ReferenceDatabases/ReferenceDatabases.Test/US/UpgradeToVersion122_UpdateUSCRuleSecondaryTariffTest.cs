using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion122_UpdateUSCRuleSecondaryTariffTest : USReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber => 122;

		protected override void PrepareTestData(DbConnection conn)
		{
			TestHelper.CreateTables(conn, new ITableScript[] {
				new USCTariffRule(),
				new USCRuleSecondaryTariff()
				});

			string insertScript = @"
				INSERT INTO USCTariffRule(U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom) VALUES ('12EA0C89-B3DB-4934-A59E-4E0F377BA90B', 'STN', '8215200000', '2000-01-01 00:00:00.000')
				INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom, U3_DateTo) VALUES('63A0491E-D9E2-4326-8E2E-784617534761', '12EA0C89-B3DB-4934-A59E-4E0F377BA90B', '7013322090', '2000-01-01 00:00:00.000', NULL)
				";

			conn.ExecuteNonQuery(insertScript);
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '7013372090' and U3_U1 = '12EA0C89-B3DB-4934-A59E-4E0F377BA90B'"));
		}
	}
}
