using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.ReferenceDatabases.US.Testing
{
	static class TestHelper
	{
		public static void CreateTables(DbConnection connection, ITableScript[] scripts)
		{
			foreach (ITableScript script in scripts)
			{
				DbObjectCreator.CreateTableIfNotExists(connection, script.TableName, script.CreateTableScript);
				foreach (IndexScript index in script.CreateIndexScripts)
				{
					CreateIndexIfNotExists(connection, script.TableName, index.IndexName, index.CreateIndexScript);
				}
			}
		}

		static void CreateIndexIfNotExists(DbConnection conn, string tableName, string indexName, string createScript)
		{
			var sqlText = DbSchemaChange.GetCreateIndexIfNotExistsScript(tableName, indexName, createScript);
			conn.ExecuteNonQuery(sqlText);
		}
	}
}
