using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion106_UpdateSTN9902RuleSecondaryTariffFor99020116 : USReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber => 106;

		protected override void PrepareTestData(DbConnection conn)
		{
			TestHelper.CreateTables(conn, new ITableScript[] {
				new USCRule(),
				new USCTariffRule(),
				new USCTariffRuleException(),
				new USCRuleSecondaryTariff(),
				new USCRuleSecondaryTariffException()
				});

			string insertScript = @"
				IF NOT EXISTS (SELECT null FROM USCTariffRule WHERE U1_PK = 'E621D0FE-B399-454B-92A5-DE2C78DADAE1')
				INSERT INTO USCTariffRule (U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom) VALUES ('E621D0FE-B399-454B-92A5-DE2C78DADAE1', 'STN', '99020116', '2018-01-03 00:00:00.000')

				IF NOT EXISTS (SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = 'E2A6F3FE-BAE3-4D45-BAE3-07D73346EF09')
				INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom, U3_DateTo) VALUES('E2A6F3FE-BAE3-4D45-BAE3-07D73346EF09', 'E621D0FE-B399-454B-92A5-DE2C78DADAE1', '20098960', '2018-01-03 00:00:00.000', '2020-12-31 00:00:00.000')
				";

			conn.ExecuteNonQuery(insertScript);
		}

		protected override void AssertUpgradeResult()
		{
			CombineAssertions(() =>
			{
				AssertEquals("expired data", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '20098960' and U3_U1 = 'E621D0FE-B399-454B-92A5-DE2C78DADAE1' AND U3_DateFrom = '2018-01-03 00:00:00' AND U3_DateTo = '2018-10-31 00:00:00'"));
				AssertEquals("new data", 1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '20098970' and U3_U1 = 'E621D0FE-B399-454B-92A5-DE2C78DADAE1' AND U3_DateFrom = '2018-11-01 00:00:00' AND U3_DateTo = '2020-12-31 00:00:00'"));
			});
		}
	}
}
