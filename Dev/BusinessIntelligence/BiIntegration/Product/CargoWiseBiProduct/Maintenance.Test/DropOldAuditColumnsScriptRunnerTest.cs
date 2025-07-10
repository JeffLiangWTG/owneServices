using System;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace CargoWise.Bi.Maintenance.Testing
{
	class DropOldAuditColumnsScriptRunnerTest : TestCase
	{
		readonly string TestTable1 = "TestTable1";
		readonly string TestTable2 = "TestTable2";

		#region Tests

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestDropIfLegacyColumnsExist()
		{
			var auditDbName = Db.AuditDatabaseName;
			using (var connection = Db.NewAdminConnection())
			using (SystemDataRegistry.Instance.AuditRetentionPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 6))
			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				var testLogger = new LoggerForTest();
				var dropOldAuditColumnsScriptRunnerForTest = new DropOldAuditColumnsScriptRunner(connection, testLogger);

				CreateTestTables(connection);
				InsertTestSchemaMapping(connection);

				CombineAssertions("Columns should exist in TestTable1 and TestTable2", () =>
				{
					AssertNumberOfColumnsInTestTable(connection, auditDbName, TestTable1, 8);
					AssertNumberOfColumnsInTestTable(connection, auditDbName, TestTable2, 2);
				});

				dropOldAuditColumnsScriptRunnerForTest.Run();

				CombineAssertions("Columns should be successfully dropped in TestTable1", () =>
				{
					AssertNumberOfColumnsInTestTable(connection, auditDbName, TestTable1, 6);
					AssertNumberOfColumnsInTestTable(connection, auditDbName, TestTable2, 2);

					AssertEquals("Successfully dropped legacy columns in Audit Database", testLogger.LogEntries.ElementAt(0));
				});
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestDropIfNoLegacyColumnsExist()
		{
			var auditDbName = Db.AuditDatabaseName;
			using (var connection = Db.NewAdminConnection())
			using (SystemDataRegistry.Instance.AuditRetentionPeriod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 24))
			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				var testLogger = new LoggerForTest();
				var dropOldAuditColumnsScriptRunnerForTest = new DropOldAuditColumnsScriptRunner(connection, testLogger);

				CreateTestTables(connection);
				InsertTestSchemaMapping(connection);

				CombineAssertions("Columns should exist in TestTable1 and TestTable2", () =>
				{
					AssertNumberOfColumnsInTestTable(connection, auditDbName, TestTable1, 8);
					AssertNumberOfColumnsInTestTable(connection, auditDbName, TestTable2, 2);
				});

				dropOldAuditColumnsScriptRunnerForTest.Run();

				CombineAssertions("No columns should be dropped", () =>
				{
					AssertNumberOfColumnsInTestTable(connection, auditDbName, TestTable1, 8);
					AssertNumberOfColumnsInTestTable(connection, auditDbName, TestTable2, 2);

					AssertEquals("No legacy columns to drop", testLogger.LogEntries.ElementAt(0));
				});
			}
		}

		#endregion

		#region Implementation

		#region Test Table Creation

		void CreateTestTables(AdminConnection connection)
		{
			var sqlText = @"
CREATE TABLE [dbo].[TestTable1]
(
	Test_PK uniqueidentifier NOT NULL PRIMARY KEY,
	Column1_Renamed varchar(100),
	Column2_Renamed varchar(100),
	Column2_Legacy_TestDropCondition varchar(100),
	Column3_Renamed varchar(100),
	Column3_Legacy_TestDropCondition varchar(100),
	Column4_Renamed varchar(100),
	Column4_Legacy_TestDropCondition varchar(100),
)

CREATE TABLE [dbo].[TestTable2]
(
	Test_PK uniqueidentifier NOT NULL PRIMARY KEY,
	TT2_Column5_Renamed varchar(100)
)"
;
			connection.ExecuteNonQuery(sqlText);
		}

		#endregion

		#region Test Data Insertion

		void InsertTestSchemaMapping(AdminConnection connection)
		{
			var sqlText = $@"
INSERT INTO [biadmin].[SchemaMappingSummary]
(MaxLsn, MaxLsnTimeUTC, EffectiveSchemaVersion, TableName, RenamedColumn, MappedColumn)
VALUES (0x01, DATEADD(month, 1, GETUTCDATE()), '6000.0', 'TestTable1', 'Column1_Renamed', 'Column1_Legacy_DoNotDrop')

INSERT INTO [biadmin].[SchemaMappingSummary]
(MaxLsn, MaxLsnTimeUTC, EffectiveSchemaVersion, TableName, RenamedColumn, MappedColumn)
VALUES (0x02, DATEADD(month, 6, GETUTCDATE()), '6000.0', 'TestTable1', 'Column2_Renamed', 'Column2_Legacy_TestDropCondition')

INSERT INTO [biadmin].[SchemaMappingSummary]
(MaxLsn, MaxLsnTimeUTC, EffectiveSchemaVersion, TableName, RenamedColumn, MappedColumn)
VALUES (0x03, DATEADD(month, 12, GETUTCDATE()), '6000.0', 'TestTable1', 'Column3_Renamed', 'Column3_Legacy_TestDropCondition')

INSERT INTO [biadmin].[SchemaMappingSummary]
(MaxLsn, MaxLsnTimeUTC, EffectiveSchemaVersion, TableName, RenamedColumn, MappedColumn)
VALUES (0x04, DATEADD(month, 8, GETUTCDATE()), '6000.0', 'TestTable1', 'Column4_Renamed', 'Column4_Legacy_TestDropCondition')

INSERT INTO [biadmin].[SchemaMappingSummary]
(MaxLsn, MaxLsnTimeUTC, EffectiveSchemaVersion, TableName, RenamedColumn, MappedColumn)
VALUES (0x05, DATEADD(month, 6, GETUTCDATE()), '6000.0', 'TestTable2', 'TT2_Column5_Renamed', 'TT2_Column5_Legacy_DoNotDrop')
";

			connection.ExecuteNonQuery(sqlText);
		}

		#endregion

		#region Assertions

		void AssertNumberOfColumnsInTestTable(AdminConnection connection, string auditDbName, string testTableName, int expectedNumberOfColumns)
		{
			var queryTestTable = string.Format(CultureInfo.InvariantCulture,
					@"SELECT COUNT(*)
					  FROM INFORMATION_SCHEMA.COLUMNS
					  WHERE TABLE_CATALOG = '{0}'
					  AND TABLE_NAME = '{1}'", auditDbName, testTableName);

			AssertEquals(expectedNumberOfColumns, (int)connection.ExecuteScalar(queryTestTable));
		}

		#endregion

		#endregion
	}
}
