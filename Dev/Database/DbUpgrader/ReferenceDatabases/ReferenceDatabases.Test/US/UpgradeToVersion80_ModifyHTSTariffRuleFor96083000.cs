using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion80_ModifyHTSTariffRuleFor96083000 : USReferenceDbUpgraderVersionTest
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

			string insertScript = @"IF NOT EXISTS (SELECT null FROM USCTariffRule WHERE U1_PK = '006DA522-211D-4BA1-A8A2-27C80FEF0257')
				INSERT INTO USCTariffRule (U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom) VALUES ('006DA522-211D-4BA1-A8A2-27C80FEF0257', 'STN', '9608300039', '2000-01-01 00:00:00.000')";

			conn.ExecuteNonQuery(insertScript);
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '960830' and U3_U1 = '006DA522-211D-4BA1-A8A2-27C80FEF0257'"));
		}

		protected override int LatestVersionNumber
		{
			get { return 80; }
		}
	}
}
