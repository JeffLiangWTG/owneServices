using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Deployment.ReportingServices.Testing
{
	internal class AuditApiScriptRunnerTest : TestCase
	{
		protected override void SetUp()
		{
			auditConnection = Db.NewAdminConnection(Db.AuditDatabaseName);
			auditConnection.ExecuteNonQuery(@"
CREATE TABLE [dbo].[TestAuditTable] (
	__$start_lsn binary(10),
	__$command_id int,
	__$seqval binary(10),
	__$lsn_period smallint,
	__$operation int,
	TT_PK int
);
");
			scriptRunner = new AuditApiScriptRunner();
		}

		protected override void TearDown()
		{
			auditConnection.Dispose();
		}

		AdminConnection auditConnection;
		AuditApiScriptRunner scriptRunner;

		[UseSnapshotProtection(new[] { DatabaseType.Audit })]
		public void TestGetChangedTables()
		{
			auditConnection.ExecuteNonQuery(@"
TRUNCATE TABLE biadmin.TableState;
INSERT INTO biadmin.TableState (SourceSchemaName, SourceTableName, AetHWMHistorySummaryLsn) VALUES
('dbo', 'table1', 0x01), -- last change before lsn
('dbo', 'table2', 0x02), -- last change was their hwm
('dbo', 'table3', 0x03),
('dbo', 'table4', 0x04),
('hrm', 'table4', 0x04), -- duplicate table name with different schema
('dbo', 'table5', NULL); -- never had a change
");

			var expectedResult = new List<string>() { "dbo.table3", "dbo.table4", "hrm.table4" };
			var actualResult = new List<string>();

			var resultTable = scriptRunner.GetChangedTables(new byte[] { 0x02 });
			foreach (DataRow row in resultTable.Rows)
			{
				actualResult.Add($"{row["SourceSchemaName"]}.{row["SourceTableName"]}");
			}

			AssertContainsExactElementsInAnyOrder("Should return a row for each table that has a HWM above the given fromLsn", expectedResult, actualResult);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Audit })]
		public void TestGetChangedTablesNoChanges()
		{
			auditConnection.ExecuteNonQuery(@"
TRUNCATE TABLE biadmin.TableState;
INSERT INTO biadmin.TableState (SourceSchemaName, SourceTableName, AetHWMHistorySummaryLsn) VALUES
('dbo', 'table1', 0x01),
('dbo', 'table4', 0x04),
('hrm', 'table4', 0x04),
('dbo', 'table5', NULL);
");

			var expectedResult = new List<string>() { };
			var actualResult = new List<string>();

			var resultTable = scriptRunner.GetChangedTables(new byte[] { 0x04 });
			AssertEquals("Should not get any rows back when the passed fromLsn is greater or equal to all table HWM's", 0, resultTable.Rows.Count);
		}

		readonly byte[] zeroLsn = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

		[UseSnapshotProtection(new[] { DatabaseType.Audit })]
		public void TestMaxLsnNullRecords()
		{
			auditConnection.ExecuteNonQuery(@$"
TRUNCATE TABLE biadmin.TableState;
insert into biadmin.TableState (AetHWMHistorySummaryLsn, SourceSchemaName, SourceTableName) VALUES (NULL, 'dbo', 'TableName');");

			var actualMaxLsn = scriptRunner.GetMaxLsn();

			AssertEquals("Should not assume that there is a maxLsn", actualMaxLsn, zeroLsn);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Audit })]
		public void TestMaxLsnNoRecords()
		{
			auditConnection.ExecuteNonQuery("TRUNCATE TABLE biadmin.TableState;");

			var actualMaxLsn = scriptRunner.GetMaxLsn();

			AssertEquals("Should not assume that there is a maxLsn", actualMaxLsn, zeroLsn);
		}
	}
}
