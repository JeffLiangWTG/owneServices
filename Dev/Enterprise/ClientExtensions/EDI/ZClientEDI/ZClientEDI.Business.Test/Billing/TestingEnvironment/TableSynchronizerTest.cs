using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using ZClientEDI.Business.Billing.TestingEnvironment;
namespace ZClientEDI.Business.Test.Billing.TestingEnvironment
{
	public class TableSynchronizerTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestSynchronise()
		{
			var sql1 = @"
CREATE TABLE TableA
(
	A_ID INT NOT NULL PRIMARY KEY WITH (ALLOW_PAGE_LOCKS = OFF)
);

CREATE TABLE TableB
(
	B_A_ID INT NOT NULL PRIMARY KEY WITH (ALLOW_PAGE_LOCKS = OFF)
	, CONSTRAINT FK_B_A_ID FOREIGN KEY (B_A_ID) REFERENCES TableA (A_ID)
);

CREATE TABLE TableC
(
	C_B_A_ID INT NOT NULL PRIMARY KEY WITH (ALLOW_PAGE_LOCKS = OFF)
	, CONSTRAINT FK_C_B_A_ID FOREIGN KEY (C_B_A_ID) REFERENCES TableB (B_A_ID)
);

INSERT TableA VALUES(1);
INSERT TableB VALUES(1);
INSERT TableC VALUES(1);

INSERT TableA VALUES(2);
INSERT TableB VALUES(2);
INSERT TableC VALUES(2);

INSERT TableA VALUES(3);
INSERT TableB VALUES(3);
INSERT TableC VALUES(3);

";
			Db.Connection.ExecuteNonQuery(sql1);

			var sql2 = @"
CREATE TRIGGER TG_TableA
ON dbo.TableA
INSTEAD OF DELETE
AS
BEGIN
	RAISERROR (15600, -1, -1, 'TG_TableA !');
END
";
			Db.Connection.ExecuteNonQuery(sql2);

			var logger = new SimpleLogger();
			new TableSynchronizerForTest(logger).Synchronise();
			var logs = logger.ToString();
			AssertEquals(@"Generating Staging Table TableB ...
Generating Staging Table TableA ...
Generating Table Dependencies ...
Deleting dbo.TableB ... (1)
Deleting dbo.TableC ... (1)
Deleting dbo.TableB ... (2)
Deleting dbo.TableA ... (1)
Copying TableA ...
Copying TableB ...
", logs);

			AssertTableRows("TableA", "1|2|9");
			AssertTableRows("TableB", "2");
			AssertTableRows("TableC", "");
		}

		void AssertTableRows(string tableName, string expectedRows)
		{
			var rowsAsString = string.Join("|", DataUtils.GetDataTableFromQuery(Db.Connection, $"SELECT * FROM {tableName} ORDER BY 1")
				.Rows.OfType<DataRow>().Select(x => x[0].ToString()));
			AssertEquals(tableName, expectedRows, rowsAsString);
		}

		public void TestValidation()
		{
			var logger = new SimpleLogger();
			new TableSynchronizerForValidationTest(logger).Synchronise();
			var logs = logger.ToString();
			AssertEquals(@"Error: The source and destination databases must be different.
", logs);
		}

		public void TestAppLock()
		{
			using (var mutex = new ZGlobalMutex(new MutexID(typeof(TableSynchronizerForValidationTest).FullName, "Ensure only one TableSynchronizer execution")))
			{
				mutex.Lock();
				var logger = new SimpleLogger();
				new TableSynchronizerForValidationTest(logger).Synchronise();
				var logs = logger.ToString();
				AssertEquals(@"Error: Unable to obtain the table synchronizer lock as it is currently held by another session.
", logs);
			}
		}

		class TableSynchronizerForValidationTest : TableSynchronizer
		{
			public TableSynchronizerForValidationTest(ILogger logger) : base(logger)
			{
			}

			protected override IEnumerable<DatabaseTable> GetTablesToSync()
			{
				return new[]
				{
					new DatabaseTable("TableB"),
					new DatabaseTable("TableA")
				};
			}

			protected override DbConnection NewTargetServerConnection() => Db.NewAdminConnection();
		}

		class TableSynchronizerForTest : TableSynchronizer
		{
			public TableSynchronizerForTest(ILogger logger) : base(logger)
			{
			}

			protected override bool Validate() => true;

			protected override DbConnection NewTargetServerConnection()
			{
				return Db.NewAdminConnection();
			}

			protected override string GetStagingTableColumnSelectQuery(TableColumn tableColumn)
			{
				if (tableColumn.ColumnName == "A_ID")
				{
					return "A_ID = IIF(A_ID = 3, 9, A_ID)";
				}
				else
				{
					return base.GetStagingTableColumnSelectQuery(tableColumn);
				}
			}

			protected override string GetStagingTableAdditionalWhereClause(DatabaseTable table)
			{
				if (table.TableName == "TableB")
				{
					return "B_A_ID = 2";
				}
				else
				{
					return base.GetStagingTableAdditionalWhereClause(table);
				}
			}

			protected override void OnSynchronisingTargetTables(List<DatabaseTable> tablesToDelete)
			{
				TargetServerConnection.ExecuteNonQuery("INSERT TableA VALUES(4);");
			}

			protected override IEnumerable<DatabaseTable> GetTablesToSync()
			{
				return new[]
				{
					new DatabaseTable("TableB"),
					new DatabaseTable("TableA")
				};
			}
		}
	}
}
