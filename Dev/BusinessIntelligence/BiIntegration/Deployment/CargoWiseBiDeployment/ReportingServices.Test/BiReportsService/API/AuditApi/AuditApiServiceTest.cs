using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Deployment.ReportingServices.Testing
{
	public class AuditApiServiceTest : TestCase
	{
		protected override void SetUp()
		{
			fakeScriptRunner = new AuditApiScriptRunnerForTest();
			auditApiService = new AuditApiService(fakeScriptRunner);
		}

		protected override void TearDown()
		{
			BiServers.ClearBiServersCache();
		}

		AuditApiScriptRunnerForTest fakeScriptRunner;
		AuditApiService auditApiService;

		#region GetChangedTablesList

		[UseSnapshotProtection(new[] { DatabaseType.Audit })]
		public void TestGetChangedTablesList()
		{
			using (var auditConnection = Db.NewAdminConnection(Db.AuditDatabaseName))
			{
				var expectedMaxLsnHex = "0x0002183B00003E300003";
				var expectedMaxLsnBytes = new byte[] { 0x00, 0x02, 0x18, 0x3B, 0x00, 0x00, 0x3E, 0x30, 0x00, 0x03 };

				fakeScriptRunner.maxLsnResult = expectedMaxLsnBytes;

				var data = new List<object[]>()
				{
					new object[] { "dbo", "OrgHeader" },
					new object[] { "hrm", "SomeOtherTable" },
				};
				var expectedItems = new ChangedTable[] { new ChangedTable { schemaName = "dbo", tableName = "OrgHeader" }, new ChangedTable { schemaName = "hrm", tableName = "SomeOtherTable" } };

				fakeScriptRunner.changedTablesResult = CreateFakeTable(new() { ("SourceSchemaName", typeof(string)), ("SourceTableName", typeof(string)) }, data);

				var afterLsn = "0x0000ff00000000000000";

				var result = auditApiService.GetChangedTablesList(afterLsn);

				AssertEquals(afterLsn, result.afterLsn);
				AssertEquals(expectedMaxLsnHex, result.maxLsn);
				AssertArrayEqualsByElements(expectedItems, result.items);
			}
		}

		public void TestGetChangedTablesListNoResult()
		{
			using (var auditConnection = Db.NewAdminConnection(Db.AuditDatabaseName))
			{
				fakeScriptRunner.changedTablesResult = CreateFakeTable(new() { ("SourceSchemaName", typeof(string)), ("SourceTableName", typeof(string)) }, new List<object[]>());
				fakeScriptRunner.maxLsnResult = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

				var result = auditApiService.GetChangedTablesList("0x00000000000000000000");

				AssertArrayEqualsByElements(Array.Empty<ChangedTable>(), result.items);
				AssertEquals("MaxLsn should have a default when TableState does not have any AetHWMHistorySummaryLsn's", "0x00000000000000000000", result.maxLsn);
			}
		}

		public void TestGetChangedTablesListInvalidAfterLsn()
		{
			using (var auditConnection = Db.NewAdminConnection(Db.AuditDatabaseName))
			{
				fakeScriptRunner.changedTablesResult = CreateFakeTable(new() { ("SourceSchemaName", typeof(string)), ("SourceTableName", typeof(string)) }, new List<object[]>());
				fakeScriptRunner.maxLsnResult = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

				try
				{
					auditApiService.GetChangedTablesList("0x0002183B00003E300003");
					Fail("Should have thrown an exception");
				}
				catch (Exception e)
				{
					AssertEquals("Exception type", typeof(AuditAPIException), e.GetType());
					AssertEquals("Exception message", "Parameter is invalid: The after_lsn value (0x0002183B00003E300003) provided is greater than the maxLsn value for this server (0x00000000000000000000).", e.Message);
				}
			}
		}

		DataTable CreateFakeTable(List<(string, Type)> dataColumns, IEnumerable<object[]> values)
		{
			var table = new DataTable();
			foreach (var columnInfo in dataColumns)
			{
				var column = new DataColumn()
				{
					Caption = columnInfo.Item1,
					ColumnName = columnInfo.Item1,
					DataType = columnInfo.Item2,
					AllowDBNull = true,
				};
				table.Columns.Add(column);
			}

			foreach (var value in values)
			{
				var row = table.NewRow();
				row.ItemArray = value;
				table.Rows.Add(row);
			}

			return table;
		}

		#endregion

		#region GetChangeDetail

		[UseSnapshotProtection(new[] { DatabaseType.Audit })]
		public void TestGetChangeDetail()
		{
			using (var auditConnection = Db.NewAdminConnection(Db.AuditDatabaseName))
			{
				var guid = Guid.NewGuid().ToString();
				auditConnection.ExecuteNonQuery($@"
TRUNCATE TABLE biadmin.SchemaVersionHistory
TRUNCATE TABLE biadmin.SchemaMappingHistory

SET IDENTITY_INSERT biadmin.SchemaVersionHistory ON;

INSERT INTO biadmin.SchemaVersionHistory (SchemaVersionId, SchemaVersion)
VALUES (1, '8000.0')

INSERT INTO biadmin.SchemaMappingHistory (SchemaVersionId, SchemaName, TableName, ColumnName, ColumnOrdinal, DataType, MaxLength, Precision, Scale)
VALUES
	(1, 'dbo', 'StmData', 'SD_PK', 1, 'uniqueidentifier', 16, 0, 0),
	(1, 'dbo', 'StmData', 'SD_Name', 2, 'varchar', 300, 0, 0)

INSERT INTO dbo.StmData (__$start_lsn, __$seqval, __$operation, __$update_mask, __$lsn_period, __$command_id, SD_PK, SD_Name)
SELECT 0xFFFFFFFFFFFFFFFFFFFF, 0x0, 3, 0xFFFFFFFFFFFFFFFFFFFF, 0, 0, '{guid}', 'Test'");

				fakeScriptRunner.maxLsnResult = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
				BiMasterState.SetParameter(auditConnection, BiConstants.LastMaxLsnProcessed, "0xFFFFFFFFFFFFFFFFFFFF");

				var result = auditApiService.GetChangeDetail("dbo", "StmData", "0x00000000000000000000");

				CombineAssertions(() =>
				{
					AssertEquals("schemaName", "dbo", result.schemaName);
					AssertEquals("tableName", "StmData", result.tableName);
					AssertEquals("totalItems", 1, result.totalItems);

					var changeDetail = result.items[0];
					AssertEquals("version", "8000.0", changeDetail.version);
					AssertEquals("schemaChangeCount", 1, changeDetail.schemaChangeCount);

					AssertEquals("columns.Length", 2, changeDetail.columns.Length);
					var actualColumnList = changeDetail.columns.Select(c => c.name);
					AssertCollectionContains("Column list", "SD_PK", actualColumnList);
					AssertCollectionContains("Column list", "SD_Name", actualColumnList);

					var changeData = changeDetail.changes[0];
					AssertEquals("__$start_lsn", "0xFFFFFFFFFFFFFFFFFFFF", changeData.start_lsn);
					AssertEquals("__$seqval", "0x00000000000000000000", changeData.seqval);
					AssertEquals("__$operation", 3, changeData.operation);
					AssertEquals("__$update_mask", "0xFFFFFFFFFFFFFFFFFFFF", changeData.update_mask);
					AssertEquals("__$lsn_period", 0, changeData.lsn_period);
					AssertEquals("__$command_id", 0, changeData.command_id);

					var actualPkValue = changeData.data.FirstOrDefault(c => c.columnName == "SD_PK").value;
					AssertEquals("SD_PK", guid, actualPkValue);
					var actualNameValue = changeData.data.FirstOrDefault(c => c.columnName == "SD_Name").value;
					AssertEquals("SD_Name", "Test", actualNameValue);
				});
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Audit })]
		public void TestGetChangeDetail_WithPaging()
		{
			using (var auditConnection = Db.NewAdminConnection(Db.AuditDatabaseName))
			{
				var guid = Guid.NewGuid().ToString();
				auditConnection.ExecuteNonQuery($@"
TRUNCATE TABLE biadmin.SchemaVersionHistory
TRUNCATE TABLE biadmin.SchemaMappingHistory
TRUNCATE TABLE dbo.StmData

SET IDENTITY_INSERT biadmin.SchemaVersionHistory ON;

INSERT INTO biadmin.SchemaVersionHistory (SchemaVersionId, SchemaVersion)
VALUES (1, '8000.0')

INSERT INTO biadmin.SchemaMappingHistory (SchemaVersionId, SchemaName, TableName, ColumnName, ColumnOrdinal, DataType, MaxLength, Precision, Scale)
VALUES
	(1, 'dbo', 'StmData', 'SD_PK', 1, 'uniqueidentifier', 16, 0, 0),
	(1, 'dbo', 'StmData', 'SD_Name', 2, 'varchar', 300, 0, 0)

INSERT INTO dbo.StmData (__$start_lsn, __$seqval, __$operation, __$update_mask, __$lsn_period, __$command_id, SD_PK, SD_Name)
VALUES
	(0x01000000000000000000, 0x01000000000000000000, 3, 0xFFFFFFFFFFFFFFFFFFFF, 0, 0, '{guid}', 'Test1'),
	(0x01000000000000000000, 0x01000000000000000000, 4, 0xFFFFFFFFFFFFFFFFFFFF, 0, 0, '{guid}', 'Test2')");

				fakeScriptRunner.maxLsnResult = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
				BiMasterState.SetParameter(auditConnection, BiConstants.LastMaxLsnProcessed, "0xFFFFFFFFFFFFFFFFFFFF");

				var result = auditApiService.GetChangeDetail("dbo", "StmData", "0x00000000000000000000", pageSize: 1);

				CombineAssertions("First request", () =>
				{
					AssertEquals("schemaName", "dbo", result.schemaName);
					AssertEquals("tableName", "StmData", result.tableName);
					AssertEquals("totalItems", 1, result.totalItems);

					if (result.totalItems > 0)
					{
						var changeDetail = result.items[0];
						AssertEquals("version", "8000.0", changeDetail.version);
						AssertEquals("schemaChangeCount", 1, changeDetail.schemaChangeCount);

						AssertEquals("columns.Length", 2, changeDetail.columns.Length);
						var actualColumnList = changeDetail.columns.Select(c => c.name);
						AssertCollectionContains("Column list", "SD_PK", actualColumnList);
						AssertCollectionContains("Column list", "SD_Name", actualColumnList);

						var changeData = changeDetail.changes[0];
						AssertEquals("__$start_lsn", "0x01000000000000000000", changeData.start_lsn);
						AssertEquals("__$seqval", "0x01000000000000000000", changeData.seqval);
						AssertEquals("__$operation", 3, changeData.operation);
						AssertEquals("__$update_mask", "0xFFFFFFFFFFFFFFFFFFFF", changeData.update_mask);
						AssertEquals("__$lsn_period", 0, changeData.lsn_period);
						AssertEquals("__$command_id", 0, changeData.command_id);

						var actualPkValue = changeData.data.FirstOrDefault(c => c.columnName == "SD_PK").value;
						AssertEquals("SD_PK", guid, actualPkValue);
						var actualNameValue = changeData.data.FirstOrDefault(c => c.columnName == "SD_Name").value;
						AssertEquals("SD_Name", "Test1", actualNameValue);
					}
				});

				result = auditApiService.GetChangeDetail("dbo", "StmData", "0x01000000000000000000", "0x01000000000000000000", 0, 3, pageSize: 1);

				CombineAssertions("Second request", () =>
				{
					AssertEquals("schemaName", "dbo", result.schemaName);
					AssertEquals("tableName", "StmData", result.tableName);
					AssertEquals("totalItems", 1, result.totalItems);

					if (result.totalItems > 0)
					{
						var changeDetail = result.items[0];
						AssertEquals("version", "8000.0", changeDetail.version);
						AssertEquals("schemaChangeCount", 1, changeDetail.schemaChangeCount);

						AssertEquals("columns.Length", 2, changeDetail.columns.Length);
						var actualColumnList = changeDetail.columns.Select(c => c.name);
						AssertCollectionContains("Column list", "SD_PK", actualColumnList);
						AssertCollectionContains("Column list", "SD_Name", actualColumnList);

						var changeData = changeDetail.changes[0];
						AssertEquals("__$start_lsn", "0x01000000000000000000", changeData.start_lsn);
						AssertEquals("__$seqval", "0x01000000000000000000", changeData.seqval);
						AssertEquals("__$operation", 4, changeData.operation);
						AssertEquals("__$update_mask", "0xFFFFFFFFFFFFFFFFFFFF", changeData.update_mask);
						AssertEquals("__$lsn_period", 0, changeData.lsn_period);
						AssertEquals("__$command_id", 0, changeData.command_id);

						var actualPkValue = changeData.data.FirstOrDefault(c => c.columnName == "SD_PK").value;
						AssertEquals("SD_PK", guid, actualPkValue);
						var actualNameValue = changeData.data.FirstOrDefault(c => c.columnName == "SD_Name").value;
						AssertEquals("SD_Name", "Test2", actualNameValue);
					}
				});
			}
		}

		#endregion
	}

	class AuditApiScriptRunnerForTest : AuditApiScriptRunner
	{
		public DataTable changedTablesResult;
		public byte[] maxLsnResult;

		public override DataTable GetChangedTables(byte[] fromLsn)
		{
			return changedTablesResult;
		}

		public override byte[] GetMaxLsn()
		{
			return maxLsnResult;
		}
	}
}
