using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion103_UpdateA99TariffRuleFor990380 : USReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber => 103;

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
			AssertEquals(1655, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule where U1_RuleCode = 'STN' AND U1_Tariff LIKE '9902%'"));
			AssertEquals(1739, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRuleSecondaryTariff JOIN USCTariffRule ON U3_U1 = U1_PK where U1_RuleCode = 'STN' AND U1_Tariff LIKE '9902%'"));
		}
	}
}
