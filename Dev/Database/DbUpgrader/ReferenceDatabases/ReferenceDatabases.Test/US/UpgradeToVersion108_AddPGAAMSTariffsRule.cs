using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion108_AddPGAAMSTariffsRule : USReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber => 108;

		protected override void PrepareTestData(DbConnection conn)
		{
			TestHelper.CreateTables(conn, new ITableScript[] {
				new USCRule(),
				new USCTariffRule(),
				new USCTariffRuleException(),
				new USCRuleSecondaryTariff(),
				new USCRuleSecondaryTariffException()
				});
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRule where U0_Code = 'EG1'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRule where U0_Code = 'EG2'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRule where U0_Code = 'MO1'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRule where U0_Code = 'MO2'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRule where U0_Code = 'MO4'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRule where U0_Code = 'MO5'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRule where U0_Code = 'MO6'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRule where U0_Code = 'MO7'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRule where U0_Code = 'MO8'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRule where U0_Code = 'PN1'"));

			AssertEquals(2, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule WHERE U1_RuleCode =  'EG1'"));
			AssertEquals(2, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule WHERE U1_RuleCode =  'EG2'"));
			AssertEquals(27, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule WHERE U1_RuleCode =  'MO1'"));
			AssertEquals(10, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule WHERE U1_RuleCode =  'MO2'"));
			AssertEquals(23, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule WHERE U1_RuleCode =  'MO4'"));
			AssertEquals(2, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule WHERE U1_RuleCode =  'MO5'"));
			AssertEquals(27, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule WHERE U1_RuleCode =  'MO6'"));
			AssertEquals(5, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule WHERE U1_RuleCode =  'MO7'"));
			AssertEquals(29, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule WHERE U1_RuleCode =  'MO8'"));
			AssertEquals(12, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule WHERE U1_RuleCode =  'PN1'"));
		}
	}
}
