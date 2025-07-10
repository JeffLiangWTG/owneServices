using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion84_AddTTBPriceRule : USReferenceDbUpgraderVersionTest
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
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCRule where U0_Code = 'TPR'"));
			AssertEquals(4, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "SELECT count(1) FROM USCTariffRule WHERE U1_RuleCode =  'TPR'"));
		}

		protected override int LatestVersionNumber
		{
			get { return 84; }
		}
	}
}
