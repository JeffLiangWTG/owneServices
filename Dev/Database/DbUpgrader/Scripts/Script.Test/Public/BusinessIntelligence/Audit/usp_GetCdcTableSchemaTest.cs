using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.Audit;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.Audit.Testing
{
	[TestedType(typeof(usp_GetCdcTableSchema))]
	internal class usp_GetCdcTableSchemaTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.AuditDatabaseName; }
		}

		[UseSnapshotProtection(new[] { DatabaseType.Audit })]
		public void TestGetCdcTableSchema()
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(ScriptDbName))
			{
				InsertTestTableSchema();

				AssertCdcSchemaList(new List<string> { "Test_PK", "Test_Name", "Test_Value" });
			}
		}

		void InsertTestTableSchema()
		{
			var sqlText = @"
DECLARE @SchemaVersionId INT = (SELECT TOP 1 SchemaVersionId FROM biadmin.SchemaVersionHistory ORDER BY SchemaVersionId ASC)

INSERT INTO [biadmin].SchemaMappingHistory (SchemaVersionId, SchemaName, TableName, ColumnName, ColumnOrdinal, DataType, MaxLength, Precision, Scale)
VALUES
	(@SchemaVersionId, 'TestSchema', 'TestTable', 'Test_PK', 1, 'uniqueidentifier', 16, 0, 0),
	(@SchemaVersionId, 'TestSchema', 'TestTable', 'Test_Name', 2, 'varchar', 300, 0, 0),
	(@SchemaVersionId, 'TestSchema', 'TestTable', 'Test_Value', 3, 'int', 4, 10, 0)";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		void AssertCdcSchemaList(IEnumerable<string> expectedColumnList)
		{
			var actualColumnList = new List<string>();
			using (var cmd = TestConnection.Command("biadmin.usp_GetCdcTableSchema"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@schemaName", SqlDbType.VarChar, "TestSchema");
				cmd.AddParameter("@tableName", SqlDbType.VarChar, "TestTable");
				cmd.AddParameter("@minLsn", SqlDbType.Binary, DBNull.Value);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						actualColumnList.Add(reader["ColumnName"].ToString());
					}
				}
			}

			AssertContainsExactElementsInExactOrder("Column list does not match expected.", expectedColumnList, actualColumnList);
		}

		public void TestSchemaVersionHistory()
		{
			var latestSchemaVersion = new Version(
				DbRegistry.DatabaseMajorSchemaVersion.LoadValue(TestConnection),
				DbRegistry.DatabaseMinorSchemaVersion.LoadValue(TestConnection));

			using (((ICurrentDbControl)TestConnection).UseDatabase(ScriptDbName))
			{
				Assert("Latest schema version not found in schema version history.",
					TestConnection.Exists($"FROM biadmin.SchemaVersionHistory WHERE SchemaVersion = '{latestSchemaVersion}'"));
			}
		}
	}
}
