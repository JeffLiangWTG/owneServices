using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion78_ModifySTNTariffRuleFor99119700 : USReferenceDbUpgraderVersionTest
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

			string insertScript = @"IF NOT EXISTS (SELECT null FROM USCTariffRule WHERE U1_PK = '2EA18AB8-A362-43B0-8745-7F99129BE432')
				INSERT INTO USCTariffRule (U1_PK, U1_RuleCode, U1_Tariff, U1_DateFrom) VALUES ('2EA18AB8-A362-43B0-8745-7F99129BE432', 'STN', '99119700', '2000-01-01 00:00:00.000')";

			conn.ExecuteNonQuery(insertScript);
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff where U3_TariffFrom = '2008979030' and U3_U1 = '2EA18AB8-A362-43B0-8745-7F99129BE432'"));
		}

		protected override int LatestVersionNumber
		{
			get { return 78; }
		}
	}
}
