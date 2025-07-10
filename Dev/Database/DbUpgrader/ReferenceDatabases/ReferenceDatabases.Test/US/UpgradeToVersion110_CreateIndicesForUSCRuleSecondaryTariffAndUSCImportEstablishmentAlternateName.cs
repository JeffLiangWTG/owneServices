using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion110_CreateIndicesForUSCRuleSecondaryTariffAndUSCImportEstablishmentAlternateName : USReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber => 110;

		protected override void AssertUpgradeResult()
		{
			AssertEquals("USCImportEstablishmentAlternateName.IX_USCImportEstablishmentAlternateName_IA_IE exists", true, DbObjectCreator.IndexExists(testConnection, "USCImportEstablishmentAlternateName", "IX_USCImportEstablishmentAlternateName_IA_IE"));
			AssertEquals("USCRuleSecondaryTariff.IX_USCRuleSecondaryTariff_U3_U1 exists", true, DbObjectCreator.IndexExists(testConnection, "USCRuleSecondaryTariff", "IX_USCRuleSecondaryTariff_U3_U1"));

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
			TestHelper.CreateTables(conn, new ITableScript[] {
				new USCImportEstablishment(),
				new USCImportEstablishmentAlternateName(),
				new USCRuleSecondaryTariff()
				});

			foreach (var table in refDbUpgrader.Tables)
			{
				foreach (var index in table.CreateIndexScripts)
				{
					conn.ExecuteNonQuery(DbSchemaChange.GetDropIndexIfExistsScript(table.TableName, index.IndexName));
				}
			}
		}
	}
}
