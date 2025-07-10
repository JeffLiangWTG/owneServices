using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion86_AddIndexesToUSCRuleSecondaryTariffExceptionAndUSCTariffRuleExceptionAndUSCVisaTariff : USReferenceDbUpgraderVersionTest
	{
		protected override void PrepareTestData(DbConnection conn)
		{
			TestHelper.CreateTables(conn, new ITableScript[] {
				new USCTariffRule(),
				new USCTariffRuleException(),
				new USCRuleSecondaryTariff(),
				new USCRuleSecondaryTariffException(),
				});
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals("USCRuleSecondaryTariffException.NR_IX__USCRuleSecondaryTariffException_U4_U3 exists", true, DbObjectCreator.IndexExists(testConnection, "USCRuleSecondaryTariffException", "NR_IX__USCRuleSecondaryTariffException_U4_U3"));
			AssertEquals("USCTariffRuleException.NR_IX__USCTariffRuleException_U2_U1 exists", true, DbObjectCreator.IndexExists(testConnection, "USCTariffRuleException", "NR_IX__USCTariffRuleException_U2_U1"));
			AssertEquals("USCVisaTariff.NR_IX__USCVisaTariff_UK_UO exists", true, DbObjectCreator.IndexExists(testConnection, "USCVisaTariff", "NR_IX__USCVisaTariff_UK_UO"));
		}

		protected override int LatestVersionNumber => 86;
	}
}
