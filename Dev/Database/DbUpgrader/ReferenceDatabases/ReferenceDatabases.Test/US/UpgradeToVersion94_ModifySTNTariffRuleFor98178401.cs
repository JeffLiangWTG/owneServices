using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion94_ModifySTNTariffRuleFor98178401 : USReferenceDbUpgraderVersionTest
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

			string insertScript = @"IF NOT EXISTS (SELECT null FROM USCTariffRule WHERE U1_PK = 'E265613A-FED1-47D7-9298-F0825C9E3A21')
				INSERT INTO USCTariffRule (U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom) VALUES ('E265613A-FED1-47D7-9298-F0825C9E3A21', 'STN', '98178401', '2000-01-01 00:00:00.000')";

			conn.ExecuteNonQuery(insertScript);
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '84798994' and U3_U1 = 'E265613A-FED1-47D7-9298-F0825C9E3A21'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '903180' and U3_U1 = 'E265613A-FED1-47D7-9298-F0825C9E3A21'"));
		}

		protected override int LatestVersionNumber
		{
			get { return 94; }
		}
	}
}
