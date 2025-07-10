using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	sealed class UpgradeToVersion64_ChangeIndexOnUSCACCaseWhenUserHaveIndexAlreadyTest : USReferenceDbUpgraderVersionTest
	{
		protected override int LatestVersionNumber
		{
			get { return 64; }
		}

		protected override void PrepareTestData(DbConnection conn)
		{
			base.PrepareTestData(conn);

			TestHelper.CreateTables(conn, new ITableScript[] {
				new USCACCase(),
				new USCACCaseRate(),
				new USCACCaseEvent(),
				new USCACCaseBondCash(),
				new USCACCaseTariff(),
				new USCACCaseLiqSuspension()
			});

			DropIndex(conn, "USCACCase", "NR_UC__U5_CaseNumber");
			DropIndex(conn, "USCACCaseBondCash", "NR_RC__U8_CaseNumber");
			DropIndex(conn, "USCACCaseEvent", "NR_RC__U7_CaseNumber");
			DropIndex(conn, "USCACCaseLiqSuspension", "NR_RC__UN_CaseNumber");
			DropIndex(conn, "USCACCaseRate", "NR_RC__U6_CaseNumber");
			DropIndex(conn, "USCACCaseTariff", "NR_RC__U9_CaseNumber");

			conn.ExecuteNonQuery(DbSchemaChange.GetCreateIndexIfNotExistsScript("USCACCase", "NR_UC__U5_TestclusteredIndex", "CREATE clustered  INDEX NR_UC__U5_TestclusteredIndex ON USCACCase (U5_CaseNumber asc)"));

			conn.ExecuteNonQuery(DbSchemaChange.GetCreateIndexIfNotExistsScript("USCACCaseBondCash", "NR_RC__U8_CaseNumber", "CREATE NONCLUSTERED INDEX NR_UC__U8_CaseNumber ON USCACCaseBondCash (U8_CaseNumber asc)"));
			conn.ExecuteNonQuery(DbSchemaChange.GetCreateIndexIfNotExistsScript("USCACCaseBondCash", "NR_RC__U8_TestclusteredIndex", "CREATE clustered INDEX NR_UC__U8_TestclusteredIndex ON USCACCaseBondCash (U8_CaseNumber asc)"));

			conn.ExecuteNonQuery(DbSchemaChange.GetCreateIndexIfNotExistsScript("USCACCaseEvent", "NR_RC__U7_CaseNumber", "CREATE NONCLUSTERED INDEX NR_UC__U7_CaseNumber ON USCACCaseEvent (U7_CaseNumber asc)"));
			conn.ExecuteNonQuery(DbSchemaChange.GetCreateIndexIfNotExistsScript("USCACCaseEvent", "NR_RC__U7_TestclusteredIndex", "CREATE clustered INDEX NR_UC__U7_TestclusteredIndex ON USCACCaseEvent (U7_CaseNumber asc)"));

			conn.ExecuteNonQuery(DbSchemaChange.GetCreateIndexIfNotExistsScript("USCACCaseLiqSuspension", "NR_RC__UN_CaseNumber", "CREATE NONCLUSTERED INDEX NR_UC__UN_CaseNumber ON USCACCaseLiqSuspension (UN_CaseNumber asc)"));
			conn.ExecuteNonQuery(DbSchemaChange.GetCreateIndexIfNotExistsScript("USCACCaseLiqSuspension", "NR_RC__UN_TestclusteredIndex", "CREATE clustered INDEX NR_UC__UN_TestclusteredIndex ON USCACCaseLiqSuspension (UN_CaseNumber asc)"));

			conn.ExecuteNonQuery(DbSchemaChange.GetCreateIndexIfNotExistsScript("USCACCaseRate", "NR_RC__U6_CaseNumber", "CREATE NONCLUSTERED INDEX NR_UC__U6_CaseNumber ON USCACCaseRate (U6_CaseNumber asc)"));
			conn.ExecuteNonQuery(DbSchemaChange.GetCreateIndexIfNotExistsScript("USCACCaseRate", "NR_RC__U6_TestclusteredIndex", "CREATE clustered INDEX NR_UC__U6_TestclusteredIndex ON USCACCaseRate (U6_CaseNumber asc)"));

			conn.ExecuteNonQuery(DbSchemaChange.GetCreateIndexIfNotExistsScript("USCACCaseTariff", "NR_RC__U9_CaseNumber", "CREATE NONCLUSTERED INDEX NR_UC__U9_CaseNumber ON USCACCaseTariff (U9_CaseNumber asc)"));
			conn.ExecuteNonQuery(DbSchemaChange.GetCreateIndexIfNotExistsScript("USCACCaseTariff", "NR_RC__U9_TestclusteredIndex", "CREATE clustered INDEX NR_UC__U9_TestclusteredIndex ON USCACCaseTariff (U9_CaseNumber asc)"));
		}

		void DropIndex(DbConnection conn, string tableName, string indexName)
		{
			if (DbObjectCreator.IndexExists(conn, tableName, indexName))
			{
				conn.ExecuteNonQuery(string.Format("Drop index [{0}] ON [{1}]", indexName, tableName));
			}
		}

		protected override void AssertUpgradeResult()
		{
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from sys.indexes where object_name(object_id)='USCACCase' and name = 'NR_UC__U5_CaseNumber' and type_desc = 'CLUSTERED'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from sys.indexes where object_name(object_id)='USCACCaseBondCash' and name = 'NR_RC__U8_CaseNumber' and type_desc = 'CLUSTERED'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from sys.indexes where object_name(object_id)='USCACCaseEvent' and name = 'NR_RC__U7_CaseNumber' and type_desc = 'CLUSTERED'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from sys.indexes where object_name(object_id)='USCACCaseLiqSuspension' and name = 'NR_RC__UN_CaseNumber' and type_desc = 'CLUSTERED'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from sys.indexes where object_name(object_id)='USCACCaseRate' and name = 'NR_RC__U6_CaseNumber' and type_desc = 'CLUSTERED'"));
			AssertEquals(1, UpgCommandRunner.RunScalarCommandOnGivenDb(testConnection, refDbUpgrader.DbName, "select count(*) from sys.indexes where object_name(object_id)='USCACCaseTariff' and name = 'NR_RC__U9_CaseNumber' and type_desc = 'CLUSTERED'"));
		}
	}
}
