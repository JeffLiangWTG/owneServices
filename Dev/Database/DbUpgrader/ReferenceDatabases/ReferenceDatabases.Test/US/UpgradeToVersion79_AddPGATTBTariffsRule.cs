using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion79_AddPGATTBTariffsRule : USReferenceDbUpgraderVersionTest
	{
		protected override void PrepareTestData(DbConnection conn)
		{
			TestHelper.CreateTables(conn, new ITableScript[] {
				new USCRule(),
				new USCTariffRule(),
				new USCTariffRuleException(),
				new USCRuleSecondaryTariff(),
				new USCRuleSecondaryTariffException(),
				});
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRule where U0_Code = 'TBP'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRule where U0_Code = 'TBC'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRule where U0_Code = 'TBF'"));

			AssertEquals(34, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule WHERE U1_RuleCode =  'TBP'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule where U1_RuleCode = 'TBP' AND U1_Tariff = '2203' AND U1_TariffTo = '2207' AND U1_DateFrom = '2000-01-01'"));
			AssertEquals(30, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule WHERE U1_RuleCode =  'TBC'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule where U1_RuleCode = 'TBC' AND U1_Tariff = '2203' AND U1_TariffTo = '2206' AND U1_DateFrom = '2000-01-01'"));
			AssertEquals(16, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule WHERE U1_RuleCode =  'TBF'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule where U1_RuleCode = 'TBF' AND U1_Tariff = '2204' AND U1_TariffTo = '2206' AND U1_DateFrom = '2000-01-01'"));
		}

		protected override int LatestVersionNumber
		{
			get { return 79; }
		}
	}
}
