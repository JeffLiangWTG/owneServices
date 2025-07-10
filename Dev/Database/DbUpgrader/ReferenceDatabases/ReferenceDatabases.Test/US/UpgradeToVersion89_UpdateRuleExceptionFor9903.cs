using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion89_UpdateRuleExceptionFor9903 : USReferenceDbUpgraderVersionTest
	{
		protected override void PrepareTestData(DbConnection conn)
		{
			TestHelper.CreateTables(conn, new ITableScript[] {
				new USCRule(),
				new USCTariffRule(),
				new USCTariffRuleException(),
				new USCTariffRuleException(),
				});

			var insertRuleSQL = @"
IF NOT EXISTS (SELECT * FROM USCRule WHERE U0_Code = 'I99')
BEGIN
	INSERT INTO USCRule (U0_PK, U0_Code) VALUES (NEWID(), 'I99')
END;";
			conn.ExecuteNonQuery(insertRuleSQL);

			var insertTariffRuleSQL = @"
IF NOT EXISTS (SELECT * FROM USCTariffRule WHERE U1_RuleCode = 'I99' AND U1_Tariff = '9903')
BEGIN
	INSERT INTO USCTariffRule (U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom) VALUES (NEWID(), 'I99', '9903', '2000-01-01')
END;";
			conn.ExecuteNonQuery(insertTariffRuleSQL);
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRule WHERE U0_Code = 'I99'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule WHERE U1_RuleCode = 'I99' AND U1_Tariff = '9903'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRuleException WHERE U2_Tariff = '990317'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRuleException WHERE U2_Tariff = '990353'"));
		}

		protected override int LatestVersionNumber
		{
			get { return 89; }
		}
	}
}
