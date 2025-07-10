using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgaradeToVersion98_TariffRuleExceptionUpdateTest : USReferenceDbUpgraderVersionTest
	{
		protected override void PrepareTestData(DbConnection conn)
		{
			TestHelper.CreateTables(conn, new ITableScript[] {
				new USCTariffRule(),
				new USCTariffRuleException()
				});

			var insertTariffRuleSQL = @"
IF NOT EXISTS (SELECT * FROM USCTariffRule WHERE U1_RuleCode = 'I99' AND U1_Tariff = '9903')
BEGIN
	INSERT INTO USCTariffRule (U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom) VALUES (NEWID(), 'I99', '9903', '2000-01-01')
END;"
;
			conn.ExecuteNonQuery(insertTariffRuleSQL);
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRuleException WHERE U2_Tariff = '99038501'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRuleException WHERE U2_Tariff = '99034005'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRuleException WHERE U2_Tariff = '99034105'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRuleException WHERE U2_Tariff = '99034110'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRuleException WHERE U2_Tariff = '99038001'"));
		}

		protected override int LatestVersionNumber
		{
			get { return 98; }
		}
	}
}
