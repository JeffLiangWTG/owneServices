using System;
using System.Data;
using CargoWise.Bi.Common;
using CargoWise.Data;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.ChangeDataCapture.Common.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.ServiceTask.Testing
{
	abstract class BaseAuditEtlExecutionTaskTest : TestCase
	{
		#region Implementation

		protected const string testSchemaName = "TestSchema";
		protected const string testTableName = "TestTable_AuditEtl";

		protected void TruncateTables(DbConnection connection, string databaseName)
		{
			connection.ExecuteNonQuery($"TRUNCATE TABLE [{databaseName}].[{BiConstants.BiAdminSchemaName}].[MasterState]");
			connection.ExecuteNonQuery($"TRUNCATE TABLE [{databaseName}].[{BiConstants.BiAdminSchemaName}].[TableState]");
			connection.ExecuteNonQuery($"TRUNCATE TABLE [{databaseName}].[{BiConstants.BiAdminSchemaName}].[TableConfiguration]");
			connection.ExecuteNonQuery($"TRUNCATE TABLE [{databaseName}].[{BiConstants.BiAdminSchemaName}].[LsnTimeMapping]");
			connection.ExecuteNonQuery($"TRUNCATE TABLE [{databaseName}].[{BiConstants.BiAdminSchemaName}].[DataLossLog]");
			connection.ExecuteNonQuery($"TRUNCATE TABLE [{databaseName}].[{BiConstants.BiAdminSchemaName}].[CdcHistorySummary]");
			connection.ExecuteNonQuery($"TRUNCATE TABLE [{databaseName}].[{BiConstants.BiAdminSchemaName}].[CdcHistorySummaryStaging]");
		}

		protected void CreateTestTable(AdminConnection connection, string auditDbName, string testTableName)
		{
			if (connection.DatabaseExists(auditDbName))
			{
				var cdcTable = new CdcTable(Db.SqlDbOwnerSchema, testTableName);
				if (cdcTable.IsCdcEnabled(connection))
				{
					cdcTable.DisableCdc(connection, string.Format("{0}_{1}", testSchemaName, testTableName));
				}
			}

			var sqlText = @"
IF NOT EXISTS(SELECT null FROM sys.schemas WHERE name = '{0}')
	EXEC ('CREATE SCHEMA [{0}]');

IF EXISTS (SELECT null FROM sys.tables t inner join sys.schemas s on t.schema_id = s.schema_id WHERE t.name = '{1}' and s.name = '{0}')
	DROP TABLE [{0}].[{1}]
CREATE TABLE [{0}].[{1}] (
	Column1ID uniqueidentifier,
	Column2Int int,
	Column3Char char(10),
	Column4Xml xml
)
";

			connection.ExecuteNonQuery(string.Format(sqlText, testSchemaName, testTableName));

			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				sqlText = @"
IF NOT EXISTS(SELECT null FROM sys.schemas WHERE name = '{0}')
	EXEC ('CREATE SCHEMA [{0}]');

IF EXISTS (SELECT null FROM sys.tables t inner join sys.schemas s on t.schema_id = s.schema_id WHERE t.name = '{1}' and s.name = '{0}')
	DROP TABLE [{0}].[{1}]
CREATE TABLE [{0}].[{1}] (
	[__$start_lsn] binary(10) NOT NULL,
	[__$seqval] binary(10) NOT NULL,
	[__$operation] int NOT NULL,
	[__$update_mask] varbinary(128) NOT NULL,
	[__$lsn_period] smallint DEFAULT (0) NOT NULL,
	Column1ID uniqueidentifier,
	Column2Int int,
	Column3Char char(10),
	Column4Xml nvarchar(max)
)

IF NOT EXISTS (SELECT NULL FROM [{2}].TableConfiguration WHERE SourceSchemaName = '{0}' AND SourceTableName = '{1}')
	INSERT INTO [{2}].TableConfiguration (SourceSchemaName, SourceTableName, TableColumnList,TableColumnConvertList)
	VALUES('{0}', '{1}', '__$start_lsn,__$seqval,__$operation,__$update_mask,Column1ID,Column2Int,Column3Char,Column4Xml', '__$start_lsn,__$seqval,__$operation,__$update_mask,Column1ID,Column2Int,Column3Char,CONVERT(NVARCHAR(MAX),Column4Xml) AS Column4Xml')

IF NOT EXISTS (SELECT NULL FROM [{2}].TableState WHERE SourceSchemaName = '{0}' AND SourceTableName = '{1}')
	INSERT INTO [{2}].TableState (SourceSchemaName, SourceTableName, CurrentState)
	VALUES('{0}', '{1}', 'New')
";
				connection.ExecuteNonQuery(string.Format(sqlText, testSchemaName, testTableName, BiConstants.BiAdminSchemaName));
			}
		}

		protected void EnableCdc(AdminConnection connection)
		{
			if (!CdcDatabase.IsEnabled(connection, Db.DatabaseName))
			{
				CdcDatabase.Enable(connection, Db.DatabaseName);
			}

			var cdcTable = new CdcTableForTesting(testSchemaName, testTableName);
			if (!cdcTable.IsCdcEnabled(connection))
			{
				cdcTable.EnableCdc(connection);
			}
		}

		protected void InsertRow(DbConnection connection, Guid id, string stringValue = null)
		{
			var query = string.Format(@"
INSERT INTO [{0}].[{1}] (Column1ID, Column2Int, Column3Char, Column4Xml) VALUES(@id, @int, @str,@xml)",
				testSchemaName,
				testTableName);

			using (var cmd = connection.Command(query))
			{
				cmd.AddParameter("@id", SqlDbType.UniqueIdentifier, id);
				cmd.AddParameter("@int", SqlDbType.Int, 1);
				cmd.AddParameter("@str", SqlDbType.VarChar, 128, stringValue ?? "Val1");
				cmd.AddParameter("@xml", SqlDbType.Xml, "<root><child>1</child></root>");

				cmd.ExecuteNonQuery();
			}
		}

		protected void UpdateRow(DbConnection connection, Guid id)
		{
			var query = string.Format(@"
UPDATE [{0}].[{1}]
SET Column2Int = @int, Column3Char = @str, Column4Xml = @xml
WHERE Column1ID = @id",
				testSchemaName,
				testTableName);

			using (var cmd = connection.Command(query))
			{
				cmd.AddParameter("@id", SqlDbType.UniqueIdentifier, id);
				cmd.AddParameter("@int", SqlDbType.Int, 2);
				cmd.AddParameter("@str", SqlDbType.VarChar, 128, "Val2");
				cmd.AddParameter("@xml", SqlDbType.Xml, "<root><child>2</child></root>");

				cmd.ExecuteNonQuery();
			}
		}

		protected void DeleteRow(DbConnection connection, Guid id)
		{
			var query = string.Format(@"
DELETE FROM [{0}].[{1}]
WHERE Column1ID = @id",
				testSchemaName,
				testTableName);

			using (var cmd = connection.Command(query))
			{
				cmd.AddParameter("@id", SqlDbType.UniqueIdentifier, id);

				cmd.ExecuteNonQuery();
			}
		}

		protected void DeferredUpdateRow(DbConnection connection, Guid id, string stringValue = null)
		{
			var query = string.Format(@"
DELETE FROM [{0}].[{1}] WHERE Column1ID = @id;
INSERT INTO [{0}].[{1}] (Column1ID, Column2Int, Column3Char, Column4Xml) VALUES(@id, @int, @str, @xml)",
				testSchemaName,
				testTableName);

			using (var cmd = connection.Command(query))
			{
				cmd.AddParameter("@id", SqlDbType.UniqueIdentifier, id);
				cmd.AddParameter("@int", SqlDbType.Int, 1);
				cmd.AddParameter("@str", SqlDbType.VarChar, 128, "Val3");
				cmd.AddParameter("@xml", SqlDbType.Xml, "<root><child>3</child></root>");

				cmd.ExecuteNonQuery();
			}
		}

		protected void CheckAuditTable(DbConnection connection, string auditDbName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				var query = string.Format(@"SELECT __$operation FROM [{0}].[{1}]", testSchemaName, testTableName);
				var operationList = DataUtils.GetListOfValuesFromQuery(connection, query);

				AssertCollectionContains("Audit table is missing Insert operation.", Convert.ToInt32(CdcOperation.Insert).ToString(), operationList);
				AssertCollectionContains("Audit table is missing Before Update operation.", Convert.ToInt32(CdcOperation.BeforeUpdate).ToString(), operationList);
				AssertCollectionContains("Audit table is missing After Update operation.", Convert.ToInt32(CdcOperation.AfterUpdate).ToString(), operationList);
				AssertCollectionContains("Audit table is missing Delete operation.", Convert.ToInt32(CdcOperation.Delete).ToString(), operationList);
			}
		}

		protected void CheckCdcHistorySummary(DbConnection connection, string auditDbName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				var query = $@"
					SELECT
						SUM(NumberOfRowsDelete) AS NumberOfRowsDelete,
						SUM(NumberOfRowsInsert) AS NumberOfRowsInsert,
						SUM(NumberOfRowsUpdate) AS NumberOfRowsUpdate
					FROM [{auditDbName}].[{BiConstants.BiAdminSchemaName}].[CdcHistorySummary]
					WHERE 
						SchemaName = '{testSchemaName}' 
						AND ChangedTableName = '{testTableName}'
				";
				var cdcHistory = DataUtils.GetDataTableFromQuery(connection, query).Rows[0];

				CombineAssertions("There should be:\r\n", () =>
				{
					Assert($"Two delete but there was {cdcHistory["NumberOfRowsDelete"]}", Convert.ToInt32(cdcHistory["NumberOfRowsDelete"]) == 2);
					Assert($"Two insert but there was {cdcHistory["NumberOfRowsInsert"]}", Convert.ToInt32(cdcHistory["NumberOfRowsInsert"]) == 2);
					Assert($"One updates but there was {cdcHistory["NumberOfRowsUpdate"]}", Convert.ToInt32(cdcHistory["NumberOfRowsUpdate"]) == 1);
				});
			}
		}

		enum CdcOperation
		{
			Delete = 1,
			Insert = 2,
			BeforeUpdate = 3,
			AfterUpdate = 4
		}

		#endregion
	}
}
