using System;
using CargoWise.Data;
using Enterprise.ChangeDataCapture.Common;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Shared
{
	public sealed class DbColumnDependencyRemoverTest : TransactionedTestCase
	{
		protected override DbConnection TestConnection
		{
			get
			{
				if (adminConnection == null)
				{
					adminConnection = Db.NewAdminConnection();
				}
				return adminConnection;
			}
		}
		AdminConnection adminConnection;

		protected override void SetUp()
		{
			base.SetUp();
			if (CdcDatabase.IsEnabled((AdminConnection)TestConnection, Db.DatabaseName))
			{
				CdcDatabase.Disable((AdminConnection)TestConnection, Db.DatabaseName);
			}
			CdcDatabase.Enable((AdminConnection)TestConnection, Db.DatabaseName);
		}

		public void TestDropRelateObjects()
		{
			// create table, column and dependencies...
			var sqlText = @"
				CREATE TABLE [~DbColumnDependencyRemoverTest~] (
					ColToKeep int default 0,
					ColToDrop int default 1 check(ColToDrop > 0),
					ColPkToDrop int not null PRIMARY KEY);
				CREATE INDEX [IX~DbColumnDependencyRemoverTest~] ON [~DbColumnDependencyRemoverTest~] (ColToDrop);
				CREATE INDEX [IX~DbColumnIndexFilterDependencyRemoverTest~] ON [~DbColumnDependencyRemoverTest~] (ColToKeep) WHERE ColToDrop = 1;
				CREATE STATISTICS [s1] ON [dbo].[~DbColumnDependencyRemoverTest~]([ColToDrop], [ColPkToDrop]);
				CREATE STATISTICS [s2] ON [dbo].[~DbColumnDependencyRemoverTest~]([ColToDrop], [ColToKeep]);
				ALTER TABLE [~DbColumnDependencyRemoverTest~]
					ADD CONSTRAINT [FK~DbColumnDependencyRemoverTest01~] FOREIGN KEY (ColToDrop)
					REFERENCES [~DbColumnDependencyRemoverTest~] (ColPkToDrop);
				ALTER TABLE [~DbColumnDependencyRemoverTest~]
					ADD CONSTRAINT [FK~DbColumnDependencyRemoverTest02~] FOREIGN KEY (ColToKeep)
					REFERENCES [~DbColumnDependencyRemoverTest~] (ColPkToDrop);";
			TestConnection.ExecuteNonQuery(sqlText);

			var ct = new CdcTable("dbo", "~DbColumnDependencyRemoverTest~");
			ct.EnableCdc((AdminConnection)TestConnection);

			// create dependent triggers (must be in a separate batch)
			sqlText = @"
				CREATE TRIGGER [TG-INS~DbColumnDependencyRemoverTest~] ON [~DbColumnDependencyRemoverTest~]
					FOR INSERT AS IF UPDATE(ColToDrop) SELECT 1;";
			TestConnection.ExecuteNonQuery(sqlText);

			sqlText = @"
				CREATE TRIGGER [TG-UPD~DbColumnDependencyRemoverTest~] ON [~DbColumnDependencyRemoverTest~]
					FOR UPDATE AS IF UPDATE(ColToDrop) SELECT 1;";
			TestConnection.ExecuteNonQuery(sqlText);

			AssertEquals("[PRE-CONDITION] Column ColToDrop exists?", true, DbObjectCreator.ColumnExists(TestConnection, "~DbColumnDependencyRemoverTest~", "ColToDrop"));
			AssertEquals("[PRE-CONDITION] Index exists?", true, DbObjectCreator.IndexExists(TestConnection, "~DbColumnDependencyRemoverTest~", "IX~DbColumnDependencyRemoverTest~"));
			AssertEquals("[PRE-CONDITION] Filtered Index exists?", true, DbObjectCreator.IndexExists(TestConnection, "~DbColumnDependencyRemoverTest~", "IX~DbColumnIndexFilterDependencyRemoverTest~"));
			AssertEquals("[PRE-CONDITION] Statistic 1 exists?", true, StatExists("~DbColumnDependencyRemoverTest~", "s1"));
			AssertEquals("[PRE-CONDITION] Statistic 2 exists?", true, StatExists("~DbColumnDependencyRemoverTest~", "s2"));
			AssertEquals("[PRE-CONDITION] FK 1 exists?", true, FkExists("~DbColumnDependencyRemoverTest~", "FK~DbColumnDependencyRemoverTest01~"));
			AssertEquals("[PRE-CONDITION] FK 2 exists?", true, FkExists("~DbColumnDependencyRemoverTest~", "FK~DbColumnDependencyRemoverTest02~"));
			AssertEquals("[PRE-CONDITION] Insert Trigger exists?", true, ObjectExists("TG-INS~DbColumnDependencyRemoverTest~"));
			AssertEquals("[PRE-CONDITION] Update Trigger exists?", true, ObjectExists("TG-UPD~DbColumnDependencyRemoverTest~"));

			// remove ColToDrop dependencies
			var columnDependencyRemover = new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, "~DbColumnDependencyRemoverTest~", "ColToDrop");
			columnDependencyRemover.DropRelateObjects(TestConnection);
			AssertEquals("Column ColToDrop exists?", true, DbObjectCreator.ColumnExists(TestConnection, "~DbColumnDependencyRemoverTest~", "ColToDrop"));
			AssertEquals("Index exists?", false, DbObjectCreator.IndexExists(TestConnection, "~DbColumnDependencyRemoverTest~", "IX~DbColumnDependencyRemoverTest~"));
			AssertEquals("Filtered Index exists?", false, DbObjectCreator.IndexExists(TestConnection, "~DbColumnDependencyRemoverTest~", "IX~DbColumnIndexFilterDependencyRemoverTest~"));
			AssertEquals("Statistic 1 exists?", false, StatExists("~DbColumnDependencyRemoverTest~", "s1"));
			AssertEquals("Statistic 2 exists?", false, StatExists("~DbColumnDependencyRemoverTest~", "s2"));
			AssertEquals("FK 1 exists?", false, FkExists("~DbColumnDependencyRemoverTest~", "FK~DbColumnDependencyRemoverTest01~"));
			AssertEquals("FK 2 exists?", true, FkExists("~DbColumnDependencyRemoverTest~", "FK~DbColumnDependencyRemoverTest02~"));
			AssertEquals("Insert Trigger exists?", false, ObjectExists("TG-INS~DbColumnDependencyRemoverTest~"));
			AssertEquals("Update Trigger exists?", false, ObjectExists("TG-UPD~DbColumnDependencyRemoverTest~"));

			// remove ColToDrop... 
			sqlText = "ALTER TABLE [~DbColumnDependencyRemoverTest~] DROP COLUMN ColToDrop";
			TestConnection.ExecuteNonQuery(sqlText);
			AssertEquals("Column exists?", false, DbObjectCreator.ColumnExists(TestConnection, "~DbColumnDependencyRemoverTest~", "ColToDrop"));

			// remove ColPkToDrop dependencies
			columnDependencyRemover = new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, "~DbColumnDependencyRemoverTest~", "ColPkToDrop");
			columnDependencyRemover.DropRelateObjects(TestConnection);
			AssertEquals("Column ColPkToDrop exists?", true, DbObjectCreator.ColumnExists(TestConnection, "~DbColumnDependencyRemoverTest~", "ColPkToDrop"));
			AssertEquals("FK 2 exists?", false, FkExists("~DbColumnDependencyRemoverTest~", "FK~DbColumnDependencyRemoverTest02~"));

			// remove ColPkToDrop... 
			sqlText = "ALTER TABLE [~DbColumnDependencyRemoverTest~] DROP COLUMN ColPkToDrop";
			TestConnection.ExecuteNonQuery(sqlText);
			AssertEquals("Column ColPkToDrop exists?", false, DbObjectCreator.ColumnExists(TestConnection, "~DbColumnDependencyRemoverTest~", "ColPkToDrop"));
			Assert($"Table {ct.CaptureInstance} remains CDC enabled", ct.IsCdcEnabled(TestConnection));
		}

		public void TestDropRelateObjectsDoesNotDropColumnstoreIndexes()
		{
			// create table + cci + other dependencies (default, check, stats)
			var sqlText = @"
				CREATE TABLE [dbo].[~DbColumnDependencyRemoverTest~] (
					ColToKeep int default 0,
					ColToDrop int default 1 check(ColToDrop > 0)
				);
				CREATE CLUSTERED COLUMNSTORE INDEX [cci~DbColumnDependencyRemoverTest~] ON [dbo].[~DbColumnDependencyRemoverTest~];
				CREATE STATISTICS [s~DbColumnDependencyRemoverTest~] ON [dbo].[~DbColumnDependencyRemoverTest~]([ColToDrop]);";
			TestConnection.ExecuteNonQuery(sqlText);

			AssertEquals("[PRE-CONDITION] Column ColToDrop exists?", true, DbObjectCreator.ColumnExists(TestConnection, "~DbColumnDependencyRemoverTest~", "ColToDrop"));
			AssertEquals("[PRE-CONDITION] Clustered Columnstore Index exists?", true, DbObjectCreator.IndexExists(TestConnection, "~DbColumnDependencyRemoverTest~", "cci~DbColumnDependencyRemoverTest~"));
			AssertEquals("[PRE-CONDITION] Statistic exists?", true, StatExists("~DbColumnDependencyRemoverTest~", "s~DbColumnDependencyRemoverTest~"));

			// remove dependencies
			var columnDependencyRemover = new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, "~DbColumnDependencyRemoverTest~", "ColToDrop");
			columnDependencyRemover.DropRelateObjects(TestConnection);

			AssertEquals("Column ColToDrop exists?", true, DbObjectCreator.ColumnExists(TestConnection, "~DbColumnDependencyRemoverTest~", "ColToDrop"));
			AssertEquals("Clustered Columnstore Index exists?", true, DbObjectCreator.IndexExists(TestConnection, "~DbColumnDependencyRemoverTest~", "cci~DbColumnDependencyRemoverTest~"));
			AssertEquals("Statistic exists?", false, StatExists("~DbColumnDependencyRemoverTest~", "s~DbColumnDependencyRemoverTest~"));

			// assert a column can be removed
			sqlText = "ALTER TABLE [~DbColumnDependencyRemoverTest~] DROP COLUMN ColToDrop";
			TestConnection.ExecuteNonQuery(sqlText);
			AssertEquals("Column exists?", false, DbObjectCreator.ColumnExists(TestConnection, "~DbColumnDependencyRemoverTest~", "ColToDrop"));
			// assert CCI is still there
			AssertEquals("Clustered Columnstore Index exists?", true, DbObjectCreator.IndexExists(TestConnection, "~DbColumnDependencyRemoverTest~", "cci~DbColumnDependencyRemoverTest~"));
		}

		[ExpectNoExceptions]
		public void TestDropRelateObjectsHandlesLeadingSpaces()
		{
			// create table, column and dependencies with LEADING spaces...
			var sqlText = @"
				CREATE TABLE [~DbColumnDependencyRemoverTest~] (
					ColToKeep int default 0,
					ColToDrop int default 1 check(ColToDrop > 0),
					ColPkToDrop int not null PRIMARY KEY);
				CREATE INDEX [ IX~DbColumnDependencyRemoverTest~] ON [~DbColumnDependencyRemoverTest~] (ColToDrop);
				CREATE STATISTICS [ s1] ON [dbo].[~DbColumnDependencyRemoverTest~]([ColToDrop], [ColPkToDrop])
			";
			TestConnection.ExecuteNonQuery(sqlText);

			var ct = new CdcTable("dbo", "~DbColumnDependencyRemoverTest~");
			ct.EnableCdc((AdminConnection)TestConnection);

			// remove dependencies
			var columnDependencyRemover = new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, "~DbColumnDependencyRemoverTest~", "ColToDrop");
			columnDependencyRemover.DropRelateObjects(TestConnection);
			Assert($"Table {ct.CaptureInstance} remains CDC enabled", ct.IsCdcEnabled(TestConnection));
		}

		#region DropSchemaBoundReferencingObjects

		const string schemaBoundTableName = "~DbColumnDependencyRemoverTest~";
		const string schemaBoundTableFullName = "dbo.[~DbColumnDependencyRemoverTest~]";

		public void TestDropSchemaBoundReferencingObjects_NoDependencies()
		{
			CreateSchemaBoundObjects();

			AssertNoExceptionThrown("ColToDrop does not have any dependent objects so it can be dropped",
				() => TestConnection.ExecuteNonQuery(String.Format("ALTER TABLE {0} DROP COLUMN ColToDrop;", schemaBoundTableFullName)));
		}

		public void TestDropSchemaBoundReferencingObjects_AlterColumn()
		{
			CreateSchemaBoundObjects();

			AssertExceptionThrown("Can NOT alter the column because of dependencies",
				typeof(SqlException),
				() => TestConnection.ExecuteNonQuery(String.Format("ALTER TABLE {0} ALTER COLUMN ColToAlter int NULL;", schemaBoundTableFullName)));
		}

		public void TestDropSchemaBoundReferencingObjects_DropColumn()
		{
			CreateSchemaBoundObjects();

			AssertExceptionThrown("Can NOT drop the column because of dependencies",
				typeof(SqlException),
				() => TestConnection.ExecuteNonQuery(String.Format("ALTER TABLE {0} DROP COLUMN ColToAlter;", schemaBoundTableFullName)));
		}

		public void TestDropSchemaBoundReferencingObjects_DropTable()
		{
			CreateSchemaBoundObjects();

			AssertExceptionThrown("Can NOT drop the table because of dependencies",
				typeof(SqlException),
				() => TestConnection.ExecuteNonQuery(String.Format("DROP TABLE {0};", schemaBoundTableFullName)));
		}

		public void TestDropSchemaBoundReferencingObjects()
		{
			CreateSchemaBoundObjects();

			var columnDependencyRemover = new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, schemaBoundTableName, "ColToAlter");
			columnDependencyRemover.DropRelateObjects(TestConnection);

			AssertNoExceptionThrown("No dependencies - can alter the column",
				() => TestConnection.ExecuteNonQuery(String.Format("ALTER TABLE {0} ALTER COLUMN ColToAlter int NULL;", schemaBoundTableFullName)));

			AssertNoExceptionThrown("No dependencies - can drop the column",
				() => TestConnection.ExecuteNonQuery(String.Format("ALTER TABLE {0} DROP COLUMN ColToAlter;", schemaBoundTableFullName)));
		}

		void CreateSchemaBoundObjects()
		{
			// create table
			var sql = String.Format(@"
if (OBJECT_ID('{0}', 'U') is NOT NULL) DROP TABLE {0};
CREATE TABLE {0}
(
	ColToKeep  int NULL,
	ColToDrop  int NULL,
	ColToAlter int NULL,
);
",
				schemaBoundTableFullName);

			TestConnection.ExecuteNonQuery(sql);

			// Function
			var function_1 = String.Format("dbo.[{0}_IF_1]()", schemaBoundTableName);
			var function_2 = String.Format("dbo.[{0}_IF_2]()", schemaBoundTableName);
			var function_3 = String.Format("dbo.[{0}_IF_3]()", schemaBoundTableName);
			TestConnection.ExecuteNonQuery(String.Format("CREATE FUNCTION {0} RETURNS TABLE WITH SCHEMABINDING AS RETURN SELECT ColToAlter FROM {1};", function_1, schemaBoundTableFullName));
			TestConnection.ExecuteNonQuery(String.Format("CREATE FUNCTION {0} RETURNS TABLE WITH SCHEMABINDING AS RETURN SELECT ColToAlter FROM {1};", function_2, function_1));
			TestConnection.ExecuteNonQuery(String.Format("CREATE FUNCTION {0} RETURNS TABLE WITH SCHEMABINDING AS RETURN SELECT ColToAlter FROM {1};", function_3, function_2));

			// View
			var view_1 = String.Format("dbo.[{0}_V_1]", schemaBoundTableName);
			var view_2 = String.Format("dbo.[{0}_V_2]", schemaBoundTableName);
			var view_3 = String.Format("dbo.[{0}_V_3]", schemaBoundTableName);
			TestConnection.ExecuteNonQuery(String.Format("CREATE VIEW {0} WITH SCHEMABINDING AS SELECT ColToAlter FROM {1};", view_1, schemaBoundTableFullName));
			TestConnection.ExecuteNonQuery(String.Format("CREATE VIEW {0} WITH SCHEMABINDING AS SELECT ColToAlter FROM {1};", view_2, function_3));
			TestConnection.ExecuteNonQuery(String.Format("CREATE VIEW {0} WITH SCHEMABINDING AS SELECT ColToAlter FROM {1};", view_3, view_2));
		}

		#endregion // DropSchemaBoundReferencingObjects

		public void TestDropRelatedIndexes()
		{
			TestConnection.ExecuteNonQuery(@"
CREATE TABLE [_Parent]
(
	PK        int NOT NULL CONSTRAINT [PK_Parent] PRIMARY KEY,
	ColToDrop int NOT NULL,
)

CREATE UNIQUE INDEX [Ind_To_Drop] ON [_Parent] (PK) INCLUDE (ColToDrop)
CREATE NONCLUSTERED INDEX [Ind_To_Drop2] ON [_Parent] (ColToDrop)

CREATE TABLE [_Child]
(
	PK int NOT NULL CONSTRAINT [PK_Child] PRIMARY KEY,
	FK int NOT NULL,
)

ALTER TABLE [_Child] WITH CHECK ADD
	CONSTRAINT [FK~_Child~]
		FOREIGN KEY
			(FK)
		REFERENCES
			_Parent (PK)
");

			AssertEquals("[PRE-CONDITION] Column ColToDrop exists?", true, DbObjectCreator.ColumnExists(TestConnection, "_Parent", "ColToDrop"));
			AssertEquals("[PRE-CONDITION] Index exists?", true, DbObjectCreator.IndexExists(TestConnection, "_Parent", "PK_Parent"));
			AssertEquals("[PRE-CONDITION] Index exists?", true, DbObjectCreator.IndexExists(TestConnection, "_Parent", "Ind_To_Drop"));
			AssertEquals("[PRE-CONDITION] Index exists?", true, DbObjectCreator.IndexExists(TestConnection, "_Parent", "Ind_To_Drop2"));
			AssertEquals("[PRE-CONDITION] Index exists?", true, DbObjectCreator.IndexExists(TestConnection, "_Child", "PK_Child"));
			AssertEquals("[PRE-CONDITION] FK exists?", true, FkExists("_Child", "FK~_Child~"));

			var columnDependencyRemover = new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, "_Parent", "ColToDrop");

			// remove ColToDrop indexes
			// Act
			AssertNoExceptionThrown(() => columnDependencyRemover.DropRelatedIndexes(TestConnection));

			// Assert
			AssertEquals("Column ColToDrop exists?", true, DbObjectCreator.ColumnExists(TestConnection, "_Parent", "ColToDrop"));
			AssertEquals("Index exists?", true, DbObjectCreator.IndexExists(TestConnection, "_Parent", "PK_Parent"));
			AssertEquals("Index exists?", false, DbObjectCreator.IndexExists(TestConnection, "_Parent", "Ind_To_Drop"));
			AssertEquals("Index exists?", false, DbObjectCreator.IndexExists(TestConnection, "_Parent", "Ind_To_Drop2"));
			AssertEquals("Index exists?", true, DbObjectCreator.IndexExists(TestConnection, "_Child", "PK_Child"));
			AssertEquals("FK exists?", false, FkExists("_Child", "FK~_Child~"));
		}

		public void TestDropRelatedObjectsWithIndexRelatedForeignKeys()
		{
			// Arrange
			TestConnection.ExecuteNonQuery(@"
CREATE TABLE [_Parent]
(
	PK        int NOT NULL CONSTRAINT [PK_Parent] PRIMARY KEY,
	ColToDrop int NOT NULL,
)

CREATE UNIQUE INDEX [Ind_To_Drop] ON [_Parent] (PK) INCLUDE (ColToDrop)

CREATE TABLE [_Child]
(
	PK int NOT NULL CONSTRAINT [PK_Child] PRIMARY KEY,
	FK int NOT NULL,
)

ALTER TABLE [_Child] WITH CHECK ADD
	CONSTRAINT [FK~_Child~]
		FOREIGN KEY
			(FK)
		REFERENCES
			_Parent (PK)
");

			var ctParent = new CdcTable("dbo", "_Parent");
			ctParent.EnableCdc((AdminConnection)TestConnection);
			var ctChild = new CdcTable("dbo", "_Child");
			ctChild.EnableCdc((AdminConnection)TestConnection);

			AssertEquals("[PRE-CONDITION] Column ColToDrop exists?", true, DbObjectCreator.ColumnExists(TestConnection, "_Parent", "ColToDrop"));
			AssertEquals("[PRE-CONDITION] Index exists?", true, DbObjectCreator.IndexExists(TestConnection, "_Parent", "PK_Parent"));
			AssertEquals("[PRE-CONDITION] Index exists?", true, DbObjectCreator.IndexExists(TestConnection, "_Parent", "Ind_To_Drop"));
			AssertEquals("[PRE-CONDITION] Index exists?", true, DbObjectCreator.IndexExists(TestConnection, "_Child", "PK_Child"));
			AssertEquals("[PRE-CONDITION] FK exists?", true, FkExists("_Child", "FK~_Child~"));

			var columnDependencyRemover = new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, "_Parent", "ColToDrop");

			// remove ColToDrop dependencies
			// Act
			AssertNoExceptionThrown(() => columnDependencyRemover.DropRelateObjects(TestConnection));

			// Assert
			AssertEquals("[PRE-CONDITION] Column ColToDrop exists?", true, DbObjectCreator.ColumnExists(TestConnection, "_Parent", "ColToDrop"));
			AssertEquals("[PRE-CONDITION] Index exists?", true, DbObjectCreator.IndexExists(TestConnection, "_Parent", "PK_Parent"));
			AssertEquals("[PRE-CONDITION] Index exists?", false, DbObjectCreator.IndexExists(TestConnection, "_Parent", "Ind_To_Drop"));
			AssertEquals("[PRE-CONDITION] Index exists?", true, DbObjectCreator.IndexExists(TestConnection, "_Child", "PK_Child"));
			AssertEquals("[PRE-CONDITION] FK exists?", false, FkExists("_Child", "FK~_Child~"));

			// remove ColToDrop... 
			// Act
			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery("ALTER TABLE _Parent DROP COLUMN ColToDrop"));

			// Assert
			AssertEquals("[PRE-CONDITION] Column ColToDrop exists?", false, DbObjectCreator.ColumnExists(TestConnection, "_Parent", "ColToDrop"));
			AssertEquals("[PRE-CONDITION] Index exists?", true, DbObjectCreator.IndexExists(TestConnection, "_Parent", "PK_Parent"));
			AssertEquals("[PRE-CONDITION] Index exists?", false, DbObjectCreator.IndexExists(TestConnection, "_Parent", "Ind_To_Drop"));
			AssertEquals("[PRE-CONDITION] Index exists?", true, DbObjectCreator.IndexExists(TestConnection, "_Child", "PK_Child"));
			AssertEquals("[PRE-CONDITION] FK exists?", false, FkExists("_Child", "FK~_Child~"));

			Assert($"Table {ctParent.CaptureInstance} remains CDC enabled", ctParent.IsCdcEnabled(TestConnection));
			Assert($"Table {ctChild.CaptureInstance} remains CDC enabled", ctChild.IsCdcEnabled(TestConnection));
		}

		public void TestDropDependentTriggers()
		{
			var referencingTableName = "~DbColumnDependencyTestReferencingTable~";
			var referencedTableName = "~DbColumnDependencyTestReferencedTable~";
			var unrelatedTableName = "~DbColumnDependencyTestUnrelatedTable~";
			var extRefTriggerName = "TG_~DbColumnDependencyTestReferencingTable~_ExtRefTrigger";
			var tableVariableTriggerName = "TG_~DbColumnDependencyTestReferencingTable~_TableVariableTrigger";
			var sampleTriggerName = "TG_~DbColumnDependencyTestUnrelatedTable~_SampleTrigger";

			// Create tables
			var sqlText = $@"
				CREATE TABLE dbo.[{referencingTableName}] (ColPk UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, Col1 INT NULL DEFAULT 0);
				CREATE TABLE dbo.[{referencedTableName}] (ColPk UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, Col1 INT NULL DEFAULT 0);
				CREATE TABLE dbo.[{unrelatedTableName}] (ColPk UNIQUEIDENTIFIER NOT NULL PRIMARY KEY, Col1 INT NULL DEFAULT 0);";
			TestConnection.ExecuteNonQuery(sqlText);

			var ct1 = new CdcTable("dbo", referencingTableName);
			ct1.EnableCdc((AdminConnection)TestConnection);
			var ct2 = new CdcTable("dbo", referencedTableName);
			ct2.EnableCdc((AdminConnection)TestConnection);
			var ct3 = new CdcTable("dbo", unrelatedTableName);
			ct3.EnableCdc((AdminConnection)TestConnection);

			// Create triggers (must be in a separate batch)
			sqlText = $@"
				CREATE TRIGGER dbo.[{extRefTriggerName}]
				ON dbo.[{referencingTableName}]
				AFTER UPDATE
				AS
				BEGIN
				    INSERT INTO dbo.[{referencedTableName}](Col1)
				    SELECT Inserted.Col1 FROM inserted;
				END;";
			TestConnection.ExecuteNonQuery(sqlText);

			sqlText = $@"
				CREATE TRIGGER dbo.[{tableVariableTriggerName}]
				ON dbo.[{referencingTableName}]
				AFTER UPDATE
				AS
				BEGIN
				    DECLARE @PutawayJobPKs TABLE(Value UNIQUEIDENTIFIER NOT NULL);
				    INSERT INTO @PutawayJobPKs SELECT DISTINCT Inserted.ColPk FROM inserted;
				END;";
			TestConnection.ExecuteNonQuery(sqlText);

			sqlText = $@"
				CREATE TRIGGER dbo.[{sampleTriggerName}]
				ON dbo.[{unrelatedTableName}]
				AFTER INSERT
				AS
				BEGIN
				    INSERT INTO dbo.[{unrelatedTableName}](Col1)
				    SELECT Inserted.Col1 FROM inserted;
				END;";
			TestConnection.ExecuteNonQuery(sqlText);

			AssertTriggerExistenceStatus(referencingTableName, extRefTriggerName, true, true);
			AssertTriggerExistenceStatus(referencingTableName, tableVariableTriggerName, true, true);
			AssertTriggerExistenceStatus(unrelatedTableName, sampleTriggerName, true, true);

			var columnDependencyRemoverReffedTbl = new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, referencedTableName, "Col1");
			columnDependencyRemoverReffedTbl.DropRelateObjects(TestConnection);

			AssertTriggerExistenceStatus(referencingTableName, extRefTriggerName, false);
			AssertTriggerExistenceStatus(referencingTableName, tableVariableTriggerName, true);
			AssertTriggerExistenceStatus(unrelatedTableName, sampleTriggerName, true);

			var columnDependencyRemoverReffingTbl = new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, referencingTableName, "ColPk");
			columnDependencyRemoverReffingTbl.DropRelateObjects(TestConnection);

			AssertTriggerExistenceStatus(referencingTableName, extRefTriggerName, false);
			AssertTriggerExistenceStatus(referencingTableName, tableVariableTriggerName, false);
			AssertTriggerExistenceStatus(unrelatedTableName, sampleTriggerName, true);

			Assert($"Table {ct1.CaptureInstance} remains CDC enabled", ct1.IsCdcEnabled(TestConnection));
			Assert($"Table {ct2.CaptureInstance} remains CDC enabled", ct2.IsCdcEnabled(TestConnection));
			Assert($"Table {ct3.CaptureInstance} remains CDC enabled", ct3.IsCdcEnabled(TestConnection));
		}

		void AssertTriggerExistenceStatus(string tableName, string triggerName, bool expectedStatus, bool preCondition = false)
		{
			var msg = preCondition ? "[PRE-CONDITION] " : string.Empty;
			msg += $"Trigger {triggerName} on {tableName} should ";
			msg += expectedStatus ? string.Empty : "not ";
			msg += "exists.";
			AssertEquals(msg, expected: expectedStatus, DbObjectCreator.TriggerExists(TestConnection, tableName, triggerName));
		}

		#region Implementation

		bool StatExists(string tableName, string statName)
		{
			string sqlText = String.Format(@"
				SELECT
					count(*)
				FROM
					sys.tables tab
					INNER JOIN sys.stats st ON st.object_id = tab.object_id
				WHERE
					tab.name = '{0}'
					AND st.name = '{1}'",
				tableName, statName);
			var count = Convert.ToInt32(TestConnection.ExecuteScalar(sqlText));
			return (count == 1);
		}

		bool FkExists(string tableName, string fkName)
		{
			string sqlText = String.Format(@"
				SELECT
					count(*)
				FROM
					sys.tables tab
					INNER JOIN sys.foreign_keys fk ON fk.parent_object_id = tab.object_id
				WHERE
					tab.name = '{0}'
					AND fk.name = '{1}'",
				tableName, fkName);
			var count = Convert.ToInt32(TestConnection.ExecuteScalar(sqlText));
			return (count == 1);
		}

		bool ObjectExists(string objName)
		{
			string sqlText = String.Format("IF exists (SELECT null FROM sys.objects WHERE name = '{0}') SELECT 1 ELSE SELECT 0", objName);
			return Convert.ToBoolean(TestConnection.ExecuteScalar(sqlText));
		}

		#endregion // Implementation
	}
}
