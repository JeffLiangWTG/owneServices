using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion117_UpdateUSCTariffRule : USReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber => 117;

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
				IF NOT EXISTS (SELECT null FROM USCTariffRule WHERE U1_PK = 'FDE9326C-EB9F-4C7B-A3FE-3A1A3FA9F14D')
				INSERT INTO USCTariffRule (U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom) VALUES ('FDE9326C-EB9F-4C7B-A3FE-3A1A3FA9F14D', 'STN', '98179501', '2020-01-01 00:00:00.000')

				IF NOT EXISTS (SELECT null FROM USCTariffRule WHERE U1_PK = '608B71CB-E944-4720-BDC9-3E32BB919F9C')
				INSERT INTO USCTariffRule (U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom) VALUES ('608B71CB-E944-4720-BDC9-3E32BB919F9C', 'STN', '98179505', '2020-01-01 00:00:00.000')

				IF NOT EXISTS (SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = '38E95538-F44F-4F2A-91E7-E1BFAEA9D480')
				INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom, U3_DateTo) VALUES('38E95538-F44F-4F2A-91E7-E1BFAEA9D480', '608B71CB-E944-4720-BDC9-3E32BB919F9C', '940520', '2020-01-01 00:00:00.000', NULL)

				IF NOT EXISTS (SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = 'D188CEB6-638A-491A-8A51-C4602EFDBACA')
				INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom, U3_DateTo) VALUES('D188CEB6-638A-491A-8A51-C4602EFDBACA', '608B71CB-E944-4720-BDC9-3E32BB919F9C', '940540', '2020-01-01 00:00:00.000', NULL)

				IF NOT EXISTS (SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = 'AB2AB343-42F8-4F6B-985C-CF0A4198FC60')
				INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom, U3_DateTo) VALUES('AB2AB343-42F8-4F6B-985C-CF0A4198FC60', 'FDE9326C-EB9F-4C7B-A3FE-3A1A3FA9F14D', '940520', '2020-01-01 00:00:00.000', NULL)

				IF NOT EXISTS (SELECT null FROM USCRuleSecondaryTariff WHERE U3_PK = '45C07832-91B3-4BA5-B3A3-0CA964C3E747')
				INSERT INTO USCRuleSecondaryTariff(U3_PK, U3_U1, U3_TariffFrom, U3_DateFrom, U3_DateTo) VALUES('45C07832-91B3-4BA5-B3A3-0CA964C3E747', 'FDE9326C-EB9F-4C7B-A3FE-3A1A3FA9F14D', '940540', '2020-01-01 00:00:00.000', NULL)
				";

			conn.ExecuteNonQuery(insertScript);
		}
		protected override void AssertUpgradeResult()
		{
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '940520' and U3_U1 = 'FDE9326C-EB9F-4C7B-A3FE-3A1A3FA9F14D' and U3_DateTo = '2021-12-31 00:00:00.000'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '940540' and U3_U1 = 'FDE9326C-EB9F-4C7B-A3FE-3A1A3FA9F14D' and U3_DateTo = '2021-12-31 00:00:00.000'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '940520' and U3_U1 = '608B71CB-E944-4720-BDC9-3E32BB919F9C' and U3_DateTo = '2021-12-31 00:00:00.000'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '940540' and U3_U1 = '608B71CB-E944-4720-BDC9-3E32BB919F9C' and U3_DateTo = '2021-12-31 00:00:00.000'"));

			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '853951' and U3_U1 = 'FDE9326C-EB9F-4C7B-A3FE-3A1A3FA9F14D'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '940521' and U3_U1 = 'FDE9326C-EB9F-4C7B-A3FE-3A1A3FA9F14D'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '940529' and U3_U1 = 'FDE9326C-EB9F-4C7B-A3FE-3A1A3FA9F14D'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '940541' and U3_U1 = 'FDE9326C-EB9F-4C7B-A3FE-3A1A3FA9F14D'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '940542' and U3_U1 = 'FDE9326C-EB9F-4C7B-A3FE-3A1A3FA9F14D'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '940549' and U3_U1 = 'FDE9326C-EB9F-4C7B-A3FE-3A1A3FA9F14D'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '853951' and U3_U1 = '608B71CB-E944-4720-BDC9-3E32BB919F9C'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '940521' and U3_U1 = '608B71CB-E944-4720-BDC9-3E32BB919F9C'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '940529' and U3_U1 = '608B71CB-E944-4720-BDC9-3E32BB919F9C'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '940541' and U3_U1 = '608B71CB-E944-4720-BDC9-3E32BB919F9C'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '940542' and U3_U1 = '608B71CB-E944-4720-BDC9-3E32BB919F9C'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '940549' and U3_U1 = '608B71CB-E944-4720-BDC9-3E32BB919F9C'"));
		}
	}
}
