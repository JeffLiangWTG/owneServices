using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion115_CreateIndexForUSCACCaseAndUSCACCaseTariff : USReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber => 115;

		protected override void AssertUpgradeResult()
		{
			AssertEquals("USCACCase.NR_RX__U5_CaseStatus_U5_CaseNumber_U5_ISOCountryCode exists", true, DbObjectCreator.IndexExists(testConnection, "USCACCase", "NR_RX__U5_CaseStatus_U5_CaseNumber_U5_ISOCountryCode"));
			AssertEquals("USCACCaseTariff.NR_RX__U9_TariffNumber_U9_CaseNumber exists", true, DbObjectCreator.IndexExists(testConnection, "USCACCaseTariff", "NR_RX__U9_TariffNumber_U9_CaseNumber"));
			AssertEquals("USCTariffQuantity.FK_RX__UQ_UE exists", true, DbObjectCreator.IndexExists(testConnection, "USCTariffQuantity", "FK_RX__UQ_UE"));
			AssertEquals("USCTariffValue.FK_RX__UA_UE exists", true, DbObjectCreator.IndexExists(testConnection, "USCTariffValue", "FK_RX__UA_UE"));

			foreach (var table in refDbUpgrader.Tables)
			{
				foreach (var index in table.CreateIndexScripts)
				{
					Assert(index.IndexName + " exists", DbObjectCreator.IndexExists(testConnection, table.TableName, index.IndexName));
				}
			}
		}

		protected override void PrepareTestData(DbConnection conn)
		{
			TestHelper.CreateTables(conn, new ITableScript[] { new USCACCase(), new USCACCaseTariff(), new USCTariffQuantity(), new USCTariffValue() });

			foreach (var table in refDbUpgrader.Tables)
			{
				foreach (var index in table.CreateIndexScripts)
				{
					conn.Command(DbSchemaChange.GetDropIndexIfExistsScript(table.TableName, index.IndexName)).ExecuteNonQuery();
				}
			}
		}
	}
}
