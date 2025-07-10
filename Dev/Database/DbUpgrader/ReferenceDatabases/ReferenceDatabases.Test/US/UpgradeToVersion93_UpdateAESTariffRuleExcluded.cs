using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion93_UpdateAESTariffRuleExcluded : USReferenceDbUpgraderVersionTest
	{
		protected override void PrepareTestData(DbConnection conn)
		{
			TestHelper.CreateTables(conn, new ITableScript[] {
				new USCRule(),
				new USCTariffRule()
				});

			var insertRuleSQL = @"
IF NOT EXISTS (SELECT * FROM USCRule WHERE U0_Code = 'AES')
BEGIN
	INSERT INTO USCRule (U0_PK, U0_Code) VALUES (NEWID(), 'AES')
END;";
			conn.ExecuteNonQuery(insertRuleSQL);

			var insertTariffRuleSQL = @"
IF NOT EXISTS (SELECT * FROM USCTariffRule WHERE U1_RuleCode = 'AES' AND U1_Tariff = '0206290000')
BEGIN
	INSERT INTO USCTariffRule (U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom) VALUES (NEWID(), 'AES', '0206290000', '2000-01-01')
END;"
;
			conn.ExecuteNonQuery(insertTariffRuleSQL);
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRule WHERE U0_Code = 'AES'"));
			AssertEquals(293, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule WHERE U1_RuleCode = 'AES'"));
		}

		protected override int LatestVersionNumber
		{
			get { return 93; }
		}
	}
}
