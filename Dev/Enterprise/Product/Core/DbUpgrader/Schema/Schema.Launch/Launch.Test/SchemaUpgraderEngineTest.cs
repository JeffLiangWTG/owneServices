using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	[UseSnapshotProtection]
	sealed class SchemaUpgraderEngineTest : BaseUpgraderTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			testConnection = ((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade();
			Db.Connection.EnsureIsOpen();
			originalServerName = Db.ServerName;
			originalDbName = Db.DatabaseName;
			Db.ClearServerDetails();
			Db.InitializeDatabaseDetails(originalServerName, SchemaUpgraderForTest.TestMainDb);
			Db.Connection.IgnoreCommitTracker = true;

			using (Db.DisableSchemaVersionCheck())
			{
				SetupTestResources();
			}
		}

		protected override void TearDown()
		{
			try
			{
				TearDownTestResources();
				testConnection.Dispose();
			}
			finally
			{
				Db.ClearServerDetails();
				Db.InitializeDatabaseDetails(originalServerName, originalDbName);
				Db.Connection.IgnoreCommitTracker = false;
			}
			base.TearDown();
		}

		void SetupTestResources()
		{
			testSchemaUpgrader = new SchemaUpgraderForTest(Db.Connection);
			testSchemaUpgrader.DoUpgradeExposed();
		}

		void TearDownTestResources()
		{
			testSchemaUpgrader.CleanTestResources();
			testSchemaUpgrader = null;
		}

		string originalServerName;
		string originalDbName;
		SchemaUpgraderForTest testSchemaUpgrader;
		IDisposable testConnection;

		#region XML Schemas

		public void TestXmlSchemasSynchronised()
		{
			AssertXmlSchemaCollectionExists("XsdEql", true);
			AssertXmlSchemaCollectionExists("XsdMod", true);
			AssertXmlSchemaCollectionExists("XsdDel", false);
			AssertXmlSchemaCollectionExists("XsdNew", true);

			AssertColumnXmlSchema("XmlTypeIssues", "Col2", "XsdEql");
			AssertColumnXmlSchema("XmlTypeIssues", "Col3", "XsdMod");
			AssertColumnXmlSchema("XmlTypeIssues", "Col4", null);
			AssertColumnXmlSchema("XmlTypeIssues", "Col5", "XsdNew");
			AssertColumnXmlSchema("XmlTypeIssues", "Col6", "XsdNew");
		}

		void AssertXmlSchemaCollectionExists(string xsdName, bool expectedToExist)
		{
			string sqlText = String.Format(
				"SELECT count(*) FROM [{0}].sys.xml_schema_collections WHERE name = '{1}';",
				SchemaUpgraderForTest.TestMainDb, xsdName);
			int rowCount = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(xsdName + " exists?", expectedToExist, (rowCount == 1));
		}

		void AssertColumnXmlSchema(string tableName, string columnName, string expectedXsd)
		{
			string columnXmlSchema = XmlSchemaSynchroniserForTesting.GetColumnXmlSchema(Db.Connection, SchemaUpgraderForTest.TestMainDb, tableName, columnName);
			AssertEquals(tableName + "." + columnName + " XML schema", expectedXsd, columnXmlSchema);
		}

		#endregion

		#region Tables and Columns

		public void TestTablesAndColumnsSynchronised()
		{
			AssertNewTableCreated();
			AssertOldTableRemoved();
			AssertColumnSynchronised();
			AssertColumnWithIncompatibleTypeChangeSynchronised();
			AssertColumnWithReducedSizeSynchronised();
			AssertColumnWithSmallDateTimeConversionSynchronised();
			AssertColumnDropped();
			AssertNewColumnsAdded();
			AssertNewColumnsCreatedWithDefaultValuesInExistingRows();
			AssertDropAllColumnsIssue();

			// Character Case Changes
			AssertTableNameCaseDiff();
			AssertDiffCaseColumn();
		}

		void AssertNewTableCreated()
		{
			string sqlText = String.Format(
				"SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'New'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		void AssertOldTableRemoved()
		{
			string sqlText = String.Format(
				"SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Old'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(0, qtyRows);
		}

		/// <summary>
		/// Test if the Col3 (Match_ColDiff table) has been changed to VARCHAR(40)
		/// </summary>
		void AssertColumnSynchronised()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = 'Match_ColDiff' AND COLUMN_NAME = 'Col3'
				AND UPPER(DATA_TYPE) = 'VARCHAR' AND CHARACTER_MAXIMUM_LENGTH = 40
				AND UPPER(IS_NULLABLE) = 'NO'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		/// <summary>
		/// Test if the Col4 has been changed to money.
		/// If the column type was changed, it means the DROP / RE-ADD mechanism worked. Otherwise it would have blown up.
		/// </summary>
		void AssertColumnWithIncompatibleTypeChangeSynchronised()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = 'Match_ColDiff' AND COLUMN_NAME = 'Col4'
				AND UPPER(DATA_TYPE) = 'MONEY'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		/// <summary>
		/// Test if the Col5 has been changed from varchar(10) to varchar(5).
		/// If the column type was changed, it means the explicit conversion (w/ reduction) worked. Otherwise it would have blown up.
		/// </summary>
		void AssertColumnWithReducedSizeSynchronised()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = 'Match_ColDiff' AND COLUMN_NAME = 'Col5'
				AND UPPER(DATA_TYPE) = 'VARCHAR' AND CHARACTER_MAXIMUM_LENGTH = 5",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		/// <summary>
		/// Test if the Col6 has been changed to smalldatetime.
		/// If the column type was changed, it means the smalldatetime range conversion worked. Otherwise it would have blown up.
		/// </summary>
		void AssertColumnWithSmallDateTimeConversionSynchronised()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = 'Match_ColDiff' AND COLUMN_NAME = 'Col6'
				AND UPPER(DATA_TYPE) = 'SMALLDATETIME'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		/// <summary>
		/// Test if Col7 (Match_ColDiff table) has been removed
		/// </summary>
		void AssertColumnDropped()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = 'Match_ColDiff' AND COLUMN_NAME = 'Col7'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = (int)Db.Connection.ExecuteScalar(sqlText);
			AssertEquals(0, qtyRows);
		}

		/// <summary>
		/// Test if Col8 (Match_ColDiff table) has been added
		/// </summary>
		void AssertNewColumnsAdded()
		{
			string sqlTextMask = @"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = '{1}' AND COLUMN_NAME = '{2}'
				AND UPPER(DATA_TYPE) = '{3}'
				AND UPPER(IS_NULLABLE) = '{4}'";

			string sqlText = String.Format(sqlTextMask,
				SchemaUpgraderForTest.TestMainDb,
				"Match_ColDiff",
				"Col8",
				"SMALLINT",
				"NO");
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Match_ColDiff.Col8 created", 1, qtyRows);

			sqlText = String.Format(sqlTextMask,
				SchemaUpgraderForTest.TestMainDb,
				"XmlTypeWithPkChange",
				"Col3",
				"INT",
				"YES");
			qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("XmlTypeIssues.Col3 created", 1, qtyRows);
		}

		void AssertNewColumnsCreatedWithDefaultValuesInExistingRows()
		{
			string sqlText = String.Format(
				"SELECT COUNT(*) FROM {0}..Match_ColDiff WHERE Col3 = 'Col3 Default'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		void AssertDropAllColumnsIssue()
		{
			string sqlText = String.Format(
				"SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'DropAllColumnsIssue'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = (int)Db.Connection.ExecuteScalar(sqlText);
			AssertEquals("DropAllColumnsIssue table should still be there", true, qtyRows == 1);

			sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = 'DropAllColumnsIssue' AND COLUMN_NAME = 'ColOld'",
				SchemaUpgraderForTest.TestMainDb);
			qtyRows = (int)Db.Connection.ExecuteScalar(sqlText);
			AssertEquals("DropAllColumnsIssue.ColOld should have been deleted", true, qtyRows == 0);

			sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = 'DropAllColumnsIssue' AND COLUMN_NAME = 'ColNew'",
				SchemaUpgraderForTest.TestMainDb);
			qtyRows = (int)Db.Connection.ExecuteScalar(sqlText);
			AssertEquals("DropAllColumnsIssue.ColNew should have been created", true, qtyRows == 1);

			sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.TABLE_CONSTRAINTS
				WHERE TABLE_NAME = 'DropAllColumnsIssue' AND CONSTRAINT_NAME = 'PK_DropAllColumnsIssue'
				AND CONSTRAINT_TYPE = 'PRIMARY KEY'",
				SchemaUpgraderForTest.TestMainDb);
			qtyRows = (int)Db.Connection.ExecuteScalar(sqlText);
			AssertEquals("PK_DropAllColumnsIssue should have been re-created", true, qtyRows == 1);
		}

		#region Character Case Changes

		void AssertTableNameCaseDiff()
		{
			DbObjectCreatorTest.AssertTableExistCaseSensitive(Db.Connection, SchemaUpgraderForTest.TestMainDb, Db.SqlDbOwnerSchema, "MatcH_TableNameCaseDiff", expected: false);
			DbObjectCreatorTest.AssertTableExistCaseSensitive(Db.Connection, SchemaUpgraderForTest.TestMainDb, Db.SqlDbOwnerSchema, "Match_TableNameCaseDiff", expected: true);
			AssertTableNameCaseDiffDataPreserved();
		}

		void AssertTableNameCaseDiffDataPreserved()
		{
			string sqlText = String.Format(
				"SELECT Col1 FROM {0}..Match_TableNameCaseDiff",
				SchemaUpgraderForTest.TestMainDb);
			DataTable testTable = Utilities.GetDataTableFromQuery(Db.Connection, sqlText);

			AssertEquals("Row Count", 1, testTable.Rows.Count);
			AssertEquals("Value should have been preserved", 1, Convert.ToInt32(testTable.Rows[0]["Col1"]));
		}

		void AssertDiffCaseColumn()
		{
			DbObjectCreatorTest.AssertColumnExistCaseSensitive(Db.Connection, SchemaUpgraderForTest.TestMainDb, Db.SqlDbOwnerSchema, "Match_ColCaseDiff", "col2", expected: false);
			DbObjectCreatorTest.AssertColumnExistCaseSensitive(Db.Connection, SchemaUpgraderForTest.TestMainDb, Db.SqlDbOwnerSchema, "Match_ColCaseDiff", "Col2", expected: true);
			AssertDiffCaseColumnDataPreserved();
		}

		void AssertDiffCaseColumnDataPreserved()
		{
			string sqlText = String.Format(
				"SELECT Col2 FROM {0}..Match_ColCaseDiff",
				SchemaUpgraderForTest.TestMainDb);
			DataTable testTable = Utilities.GetDataTableFromQuery(Db.Connection, sqlText);

			AssertEquals("Row Count", 2, testTable.Rows.Count);
			AssertEquals("Value in 1st row", "Row One", testTable.Rows[0]["Col2"].ToString());
			AssertEquals("Value in 2nd row", DBNull.Value, testTable.Rows[1]["Col2"]);
		}

		#endregion

		#endregion

		#region Client-specific and Unmanaged Tables

		public void TestClientSpecificAndUnmanagedTables()
		{
			// Unmatching client-specific table
			AssertUnmatchingClientSpecificTablesAreNotRemoved();
			AssertPksOfUnmatchingTablesAreNotRemoved();
			AssertDefaultsOfUnmatchingTablesAreNotRemoved();
			AssertCheckConstraintsOfUnmatchingTablesAreNotRemoved();
			AssertIndexesOfUnmatchingTablesAreNotRemoved();
			AssertFksOfUnmatchingTablesAreRemoved();

			// Objects created using ClientHook scripts
			AssertClientSpecificNewTableCreated();

			// Matching client-specific table
			AssertClientSpecificMatchTableColumnsSynchronised();
			AssertClientSpecificMatchTableIndexSynchronised();
			AssertClientSpecificMatchTableDefaultSynchronised();
			AssertClientSpecificMatchTableCheckConstraintsSynchronised();
			AssertClientSpecificMatchTableFkSynchronised();
			AssertClientSpecificTableDataPreserved();

			// Non dbo schema tables (Alien test schema)
			AssertNonDboTablesRemoved();
			AssertFkToAlienTableRemoved();
		}

		#region Unmatching client-specific table

		void AssertUnmatchingClientSpecificTablesAreNotRemoved()
		{
			string sqlText = String.Format(
				"SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ClientTable'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		void AssertPksOfUnmatchingTablesAreNotRemoved()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.TABLE_CONSTRAINTS
				WHERE TABLE_NAME = 'ClientTable' AND CONSTRAINT_NAME = 'PK_ClientTable'
				AND CONSTRAINT_TYPE = 'PRIMARY KEY'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		void AssertDefaultsOfUnmatchingTablesAreNotRemoved()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = 'ClientTable' AND COLUMN_NAME = 'Col2'
				AND COLUMN_DEFAULT = '(''A'')'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		void AssertCheckConstraintsOfUnmatchingTablesAreNotRemoved()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.TABLE_CONSTRAINTS
				WHERE TABLE_NAME = 'ClientTable' AND CONSTRAINT_NAME = 'CK_ClientTable_Col2'
				AND CONSTRAINT_TYPE = 'CHECK'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		void AssertIndexesOfUnmatchingTablesAreNotRemoved()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.sys.objects tab
				INNER JOIN {0}..sysindexes ind ON ind.id = tab.object_id
				WHERE tab.type = 'U' AND tab.name = 'ClientTable'
				AND ind.name = 'IX_ClientTable_01'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		/// <summary>
		/// It is possible to preserve the FKs of unmatching tables.
		/// But we don't support it to avoid some issues (like in the following scenario).
		///  1/ If an UNMANAGED client table reference Enterprise tables;
		///  2/ Referenced Enterprise table is removed from schema;
		///  3/ If referencing FK is not removed the Enterprise table can't be removed.
		/// In summary: we DO NOT want unmanaged tables to reference Enterprise tables.
		///
		/// In case we decide to keep unmanaged table FKs,
		/// here is the code to put in the "synchroniseAllForeignKeysSqlText" on ConstraintScriptRunner
		/// -- Main Database FKs - View (derived table)
		/// -- Ignore TABLES that are not in the schema (unmanaged)
		/// WHERE
		///   EXISTS (SELECT curobj.name
		///           FROM [DbBeingUpgraded].sys.objects curobj
		///           INNER JOIN [TemplateDb].sys.objects newobj ON curobj.name = newobj.name
		///           WHERE curobj.object_id = oRef.fkeyid)
		/// </summary>
		void AssertFksOfUnmatchingTablesAreRemoved()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
				WHERE CONSTRAINT_NAME = 'FK_ClientTable_TO_ClientTable01'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(0, qtyRows);
		}

		#endregion

		#region Objects created using ClientHook scripts

		void AssertClientSpecificNewTableCreated()
		{
			string sqlText = String.Format(
				"SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ClientNew'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		#endregion

		#region Matching client-specific table

		void AssertClientSpecificMatchTableColumnsSynchronised()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = 'ClientMatch' AND COLUMN_NAME = 'Col3'
				AND UPPER(DATA_TYPE) = 'CHAR' AND CHARACTER_MAXIMUM_LENGTH = 10
				AND UPPER(IS_NULLABLE) = 'YES'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);

			sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = 'ClientMatch' AND COLUMN_NAME = 'Col4'
				AND UPPER(DATA_TYPE) = 'INT' AND UPPER(IS_NULLABLE) = 'YES'",
				SchemaUpgraderForTest.TestMainDb);
			qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		/// <summary>
		/// Test if columns Col3 and Col4 are both part of the index
		/// </summary>
		void AssertClientSpecificMatchTableIndexSynchronised()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.sys.index_columns keys
				INNER JOIN {0}.sys.tables tab ON tab.object_id = keys.object_id
				INNER JOIN {0}.sys.indexes ind ON ind.object_id = keys.object_id AND ind.index_id = keys.index_id
				INNER JOIN {0}.sys.columns cols ON cols.object_id = keys.object_id AND cols.column_id = keys.column_id
				WHERE tab.name = 'ClientMatch'
				AND ind.name = 'IX_ClientMatch_01'
				AND cols.name IN ('Col3','Col4')",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(2, qtyRows);
		}

		/// <summary>
		/// Test if the default of column [Col2] has been changed to ('B')
		/// NOTE: In SQL Server 2005, the numeric default values are encapsulated by an extra pair of parentheses.
		///       e.g. ((1)) instead of just (1).
		/// </summary>
		void AssertClientSpecificMatchTableDefaultSynchronised()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = 'ClientMatch' AND COLUMN_NAME = 'Col4'
				AND COLUMN_DEFAULT IN ('(1)', '((1))')",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		/// <summary>
		/// Test if the old CHECK constraint was removed and new created
		/// </summary>
		void AssertClientSpecificMatchTableCheckConstraintsSynchronised()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.TABLE_CONSTRAINTS
				WHERE TABLE_NAME = 'ClientMatch' AND CONSTRAINT_NAME = 'CK_ClientMatch_Col2'
				AND CONSTRAINT_TYPE = 'CHECK'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(0, qtyRows);

			sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.TABLE_CONSTRAINTS
				WHERE TABLE_NAME = 'ClientMatch' AND CONSTRAINT_NAME = 'CK_ClientMatch_Col3'
				AND CONSTRAINT_TYPE = 'CHECK'",
				SchemaUpgraderForTest.TestMainDb);
			qtyRows = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		/// <summary>
		/// Test if the old FK has been removed and the new created
		/// </summary>
		void AssertClientSpecificMatchTableFkSynchronised()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
				WHERE CONSTRAINT_NAME = 'FK_ClientMatch_Col3_TO_ClientMatch_Col1'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(0, qtyRows);

			sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
				WHERE CONSTRAINT_NAME = 'FK_ClientMatch_Col4_TO_ClientMatch_Col1'
				AND UNIQUE_CONSTRAINT_NAME = 'PK_ClientMatch'",
				SchemaUpgraderForTest.TestMainDb);
			qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		void AssertClientSpecificTableDataPreserved()
		{
			string sqlText = String.Format("SELECT COUNT(*) FROM {0}..ClientTable", SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(3, qtyRows);

			sqlText = String.Format("SELECT COUNT(*) FROM {0}..ClientMatch", SchemaUpgraderForTest.TestMainDb);
			qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(3, qtyRows);
		}

		#endregion

		#region Non-dbo schema tables

		void AssertNonDboTablesRemoved()
		{
			string sqlText = String.Format(
				"SELECT COUNT(*) FROM {0}.sys.tables WHERE schema_id != 1",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Non dbo table count", 0, qtyRows);

			sqlText = String.Format(
				"SELECT COUNT(*) FROM {0}.sys.tables WHERE name in ('TabAlien1', 'TabAlien2')",
				SchemaUpgraderForTest.TestMainDb);
			qtyRows = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Number of Alien tables", 0, qtyRows);

			sqlText = String.Format(
				"SELECT COUNT(*) FROM {0}.sys.tables WHERE name = 'StmData'",
				SchemaUpgraderForTest.TestMainDb);
			qtyRows = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Number of StmData tables", 1, qtyRows);
		}

		void AssertFkToAlienTableRemoved()
		{
			string sqlText = String.Format(
				"SELECT COUNT(*) FROM {0}.sys.foreign_keys WHERE name = 'ClientTable_2_TabAlien1'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Number of FKs to alien table", 0, qtyRows);
		}

		#endregion

		#endregion

		#region Constraints and Indexes

		public void TestConstraintsAndIndexesSynchronised()
		{
			AssertPrimaryKeyChanges();
			AssertUniqueConstraintRecreated();
			AssertUniqueConstraintColumnSynchronised();
			AssertOldIndexesRemoved();
			AssertModifiedIndexesSynchronised();
			AssertNewIndexesCreated();
			AssertXmlIndexChanges();
			AssertSpatialIndexesSynchronised();
			AssertDefaultDiffTableSynchronised();
			AssertNewCheckConstraintCreated();
			AssertOldFKRemoved();
			AssertNewFKCreated();
			AssertFKCascadeActionSynchronised();
		}

		void AssertPrimaryKeyChanges()
		{
			AssertPrimaryKeyRecreated("Match_PKDiff", "PK_Match_PKDiff");
			AssertPrimaryKeySynchronised("PK_Match_PKDiff", true);

			AssertPrimaryKeyRecreated("XmlTypeWithPkChange", "PK_XmlTypeWithPkChange");
			AssertPrimaryKeySynchronised("PK_XmlTypeWithPkChange", false);
		}

		void AssertPrimaryKeyRecreated(string tableName, string pkName)
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.TABLE_CONSTRAINTS
				WHERE TABLE_NAME = '{1}' AND CONSTRAINT_NAME = '{2}'
				AND CONSTRAINT_TYPE = 'PRIMARY KEY'",
				SchemaUpgraderForTest.TestMainDb,
				tableName,
				pkName);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(tableName + "." + pkName, 1, qtyRows);
		}

		void AssertPrimaryKeySynchronised(string pkName, bool expectedClustered)
		{
			string clusteredOrNonclustered = expectedClustered ? "CLUSTERED" : "NONCLUSTERED";
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.sys.indexes
				WHERE name = '{1}' AND is_primary_key = 1 AND type_desc = '{2}'",
				SchemaUpgraderForTest.TestMainDb,
				pkName,
				clusteredOrNonclustered);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(pkName + " " + clusteredOrNonclustered, 1, qtyRows);
		}

		/// <summary>
		/// Test if the Unique Constraint is there
		/// </summary>
		void AssertUniqueConstraintRecreated()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.TABLE_CONSTRAINTS
				WHERE TABLE_NAME = 'Match_PKDiff' AND CONSTRAINT_NAME = 'UK_Match_PKDiff'
				AND CONSTRAINT_TYPE = 'UNIQUE'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		/// <summary>
		/// Test if the Unique Constraint keys is now "(Col3)"
		/// </summary>
		void AssertUniqueConstraintColumnSynchronised()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.KEY_COLUMN_USAGE
				WHERE TABLE_NAME = 'Match_PKDiff' AND CONSTRAINT_NAME = 'UK_Match_PKDiff'
				AND COLUMN_NAME = 'Col3'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		void AssertOldIndexesRemoved()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.sys.objects tab
				INNER JOIN {0}..sysindexes ind ON ind.id = tab.object_id
				WHERE tab.type = 'U' AND tab.name = 'Match_IndexDiff'
				AND ind.name = 'IX_Match_IndexDiff_03'
				AND (ind.status & 64) = 0
				AND (ind.status & 2048) = 0
				AND (ind.status & 4096) = 0
				AND ind.indid > 0 AND ind.indid < 255",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Match_IndexDiff.IX_Match_IndexDiff_03 should have been removed", 0, qtyRows);
		}

		void AssertModifiedIndexesSynchronised()
		{
			string baseSqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.sys.objects tab
				INNER JOIN {0}..sysindexes ind ON ind.id = tab.object_id
				WHERE tab.type = 'U' AND tab.name = 'Match_IndexDiff'
				AND ind.name = '{1}'
				AND (ind.status & 64) = 0
				AND (ind.status & 2048) = 0
				AND (ind.status & 4096) = 0
				AND ind.indid > 0 AND ind.indid < 255",
				SchemaUpgraderForTest.TestMainDb, "{0}");

			string ind01SqlText = String.Format(baseSqlText, "IX_Match_IndexDiff_01") + " AND (ind.status & 2) = 0 AND (ind.status & 16) > 0";
			int qtyRows = Convert.ToInt32(Db.Connection.ExecuteScalar(ind01SqlText));
			AssertEquals("Match_IndexDiff.IX_Match_IndexDiff_01 should have been re-created as Not-Unique and Clustered", 1, qtyRows);

			string ind04SqlText = String.Format(baseSqlText, "IX_Match_IndexDiff_04") + " AND (ind.status & 2) = 0 AND (ind.status & 16) = 0";
			qtyRows = Convert.ToInt32(Db.Connection.ExecuteScalar(ind04SqlText));
			AssertEquals("Match_IndexDiff.IX_Match_IndexDiff_04 should have been re-created as NonClustered", 1, qtyRows);

			string ind02SqlText = String.Format(baseSqlText, "IX_Match_IndexDiff_02") + " AND (ind.status & 2) = 0 AND (ind.status & 16) = 0";
			qtyRows = Convert.ToInt32(Db.Connection.ExecuteScalar(ind02SqlText));
			AssertEquals("Match_IndexDiff.IX_Match_IndexDiff_02 should have been re-created", 1, qtyRows);

			string sqlText = String.Format(@"
				SELECT cols.name FROM {0}.sys.index_columns keys
				INNER JOIN {0}.sys.tables tab ON tab.object_id = keys.object_id
				INNER JOIN {0}.sys.indexes ind ON ind.object_id = keys.object_id AND ind.index_id = keys.index_id
				INNER JOIN {0}.sys.columns cols ON cols.object_id = keys.object_id AND cols.column_id = keys.column_id
				WHERE tab.name = 'Match_IndexDiff'
				AND ind.name = 'IX_Match_IndexDiff_02'
				ORDER BY keys.key_ordinal",
				SchemaUpgraderForTest.TestMainDb);

			DataTable indexColumns = Utilities.GetDataTableFromQuery(Db.Connection, sqlText);
			AssertEquals("Match_IndexDiff.IX_Match_IndexDiff_02 key count", 2, indexColumns.Rows.Count);
			AssertEquals("1st key column of Match_IndexDiff.IX_Match_IndexDiff_02", "Col3", indexColumns.Rows[0][0].ToString());
			AssertEquals("2nd key column of Match_IndexDiff.IX_Match_IndexDiff_02", "Col2", indexColumns.Rows[1][0].ToString());
		}

		void AssertNewIndexesCreated()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.sys.objects tab
				INNER JOIN {0}..sysindexes ind ON ind.id = tab.object_id
				WHERE tab.type = 'U' AND tab.name = 'Match_IndexDiff'
				AND ind.name = 'IX_Match_IndexDiff_05'
				AND (ind.status & 2) > 0
				AND (ind.status & 16) = 0
				AND (ind.status & 64) = 0
				AND (ind.status & 2048) = 0
				AND (ind.status & 4096) = 0
				AND ind.indid > 0 AND ind.indid < 255",
				SchemaUpgraderForTest.TestMainDb);

			int qtyRows = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals("Match_IndexDiff.IX_Match_IndexDiff_05 should have been created", 1, qtyRows);

			sqlText = String.Format(@"
				SELECT cols.name FROM {0}.sys.index_columns keys
				INNER JOIN {0}.sys.tables tab ON tab.object_id = keys.object_id
				INNER JOIN {0}.sys.indexes ind ON ind.object_id = keys.object_id AND ind.index_id = keys.index_id
				INNER JOIN {0}.sys.columns cols ON cols.object_id = keys.object_id AND cols.column_id = keys.column_id
				WHERE tab.name = 'Match_IndexDiff'
				AND ind.name = 'IX_Match_IndexDiff_05'",
				SchemaUpgraderForTest.TestMainDb);

			DataTable indexColumns = Utilities.GetDataTableFromQuery(Db.Connection, sqlText);
			AssertEquals("Match_IndexDiff.IX_Match_IndexDiff_05 key count", 1, indexColumns.Rows.Count);
			AssertEquals("Match_IndexDiff.IX_Match_IndexDiff_05 key column name", "Col3", indexColumns.Rows[0][0].ToString());
		}

		void AssertXmlIndexChanges()
		{
			IndexTestHelper.AssertIndexNotExists(Db.Connection, SchemaUpgraderForTest.TestMainDb, "XmlTypeWithPkChange", "IX_XmlTypeWithPkChange_01");
			IndexTestHelper.AssertIndexNotExists(Db.Connection, SchemaUpgraderForTest.TestMainDb, "XmlTypeWithPkChange", "IX_XmlTypeWithPkChange_01_Path");

			IndexTestHelper.AssertXmlIndexExists(Db.Connection, SchemaUpgraderForTest.TestMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2", null, null);
			IndexTestHelper.AssertIndexNotExists(Db.Connection, SchemaUpgraderForTest.TestMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_Old");
			IndexTestHelper.AssertXmlIndexExists(Db.Connection, SchemaUpgraderForTest.TestMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_New", "IX_XmlTypeIssues_Col2", "VALUE");
			IndexTestHelper.AssertXmlIndexExists(Db.Connection, SchemaUpgraderForTest.TestMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col2_Value2Property", "IX_XmlTypeIssues_Col2", "PROPERTY");

			IndexTestHelper.AssertIndexNotExists(Db.Connection, SchemaUpgraderForTest.TestMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col3_Old");
			IndexTestHelper.AssertIndexNotExists(Db.Connection, SchemaUpgraderForTest.TestMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col3_Old_Path");
			IndexTestHelper.AssertXmlIndexExists(Db.Connection, SchemaUpgraderForTest.TestMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col3_New", null, null);
			IndexTestHelper.AssertXmlIndexExists(Db.Connection, SchemaUpgraderForTest.TestMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Col3_New_Value", "IX_XmlTypeIssues_Col3_New", "VALUE");

			IndexTestHelper.AssertXmlIndexExists(Db.Connection, SchemaUpgraderForTest.TestMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Changing", null, null);
			IndexTestHelper.AssertXmlIndexExists(Db.Connection, SchemaUpgraderForTest.TestMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Changing_Col", "IX_XmlTypeIssues_Changing", "PATH");
			string[] colList = IndexTestHelper.GetIndexKeyColumnsInOrder(Db.Connection, SchemaUpgraderForTest.TestMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Changing");
			AssertEquals("XmlTypeIssues.IX_XmlTypeIssues_Changing should have only 1 column", 1, colList.Length);
			AssertEquals("XmlTypeIssues.IX_XmlTypeIssues_Changing should be on Col5 column", "Col5", colList[0]);

			IndexTestHelper.AssertXmlIndexExists(Db.Connection, SchemaUpgraderForTest.TestMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_NewPrimary", null, null);
			IndexTestHelper.AssertXmlIndexExists(Db.Connection, SchemaUpgraderForTest.TestMainDb, "XmlTypeIssues", "IX_XmlTypeIssues_Changing_Parent", "IX_XmlTypeIssues_NewPrimary", "VALUE");
		}

		void AssertSpatialIndexesSynchronised()
		{
			using (((ICurrentDbControl)Db.Connection).UseDatabase(SchemaUpgraderForTest.TestMainDb))
			{
				// 1. new pk and indexes
				var expected = new string[]
				{
					"SPATIAL INDEX [IX_Spatial_Geometry] ON [dbo].[Spatial_NewIndex] ([S_Geometry]) USING GEOMETRY_GRID WITH (CELLS_PER_OBJECT = 16, BOUNDING_BOX = (0, 0, 1, 1), GRIDS = (MEDIUM, MEDIUM, MEDIUM, MEDIUM))",
					"SPATIAL INDEX [IX_Spatial_Geometry_Auto] ON [dbo].[Spatial_NewIndex] ([S_Geometry]) USING GEOMETRY_AUTO_GRID WITH (CELLS_PER_OBJECT = 8, BOUNDING_BOX = (0, 0, 1, 1))",
					"SPATIAL INDEX [IX_Spatial_Geography] ON [dbo].[Spatial_NewIndex] ([S_Geography]) USING GEOGRAPHY_GRID WITH (CELLS_PER_OBJECT = 16, GRIDS = (MEDIUM, MEDIUM, MEDIUM, MEDIUM))",
					"SPATIAL INDEX [IX_Spatial_Geography_Auto] ON [dbo].[Spatial_NewIndex] ([S_Geography]) USING GEOGRAPHY_AUTO_GRID WITH (CELLS_PER_OBJECT = 12)",
				};

				AssertContainsExactElementsInAnyOrder<string>(expected
					, SpatialIndexLoader.Load(Db.Connection, null, "Spatial_NewIndex", null).Select(index => index.Definition));

				expected = new string[]
				{
					"SPATIAL INDEX [IX_Spatial] ON [dbo].[ClientSpatial] ([S_Geography]) USING GEOGRAPHY_AUTO_GRID WITH (CELLS_PER_OBJECT = 12)",
				};

				AssertContainsExactElementsInAnyOrder<string>(expected
					, SpatialIndexLoader.Load(Db.Connection, null, "ClientSpatial", null).Select(index => index.Definition));

				// 2. alter index def
				expected = new string[]
				{
					"SPATIAL INDEX [IX_Spatial_Geometry] ON [dbo].[Spatial_AlterIndex] ([S_Geometry]) USING GEOMETRY_GRID WITH (CELLS_PER_OBJECT = 16, BOUNDING_BOX = (1, 1, 2, 2), GRIDS = (MEDIUM, MEDIUM, MEDIUM, MEDIUM))",
					"SPATIAL INDEX [IX_Spatial_Geometry_Auto] ON [dbo].[Spatial_AlterIndex] ([S_Geometry]) USING GEOMETRY_AUTO_GRID WITH (CELLS_PER_OBJECT = 8, BOUNDING_BOX = (1, 1, 2, 2))",
					"SPATIAL INDEX [IX_Spatial_Geography] ON [dbo].[Spatial_AlterIndex] ([S_Geography]) USING GEOGRAPHY_GRID WITH (CELLS_PER_OBJECT = 16, GRIDS = (LOW, LOW, LOW, LOW))",
					"SPATIAL INDEX [IX_Spatial_Geography_Auto] ON [dbo].[Spatial_AlterIndex] ([S_Geography]) USING GEOGRAPHY_AUTO_GRID WITH (CELLS_PER_OBJECT = 2)",
					"SPATIAL INDEX [IX_Spatial_Geography_2] ON [dbo].[Spatial_AlterIndex] ([S_Geography]) USING GEOGRAPHY_AUTO_GRID WITH (CELLS_PER_OBJECT = 12, ALLOW_PAGE_LOCKS = OFF)",
				};

				AssertContainsExactElementsInAnyOrder<string>(expected
					, SpatialIndexLoader.Load(Db.Connection, null, "Spatial_AlterIndex", null).Select(index => index.Definition));

				// 3. alter pk
				expected = new string[]
				{
					"SPATIAL INDEX [IX_Spatial_Geometry] ON [dbo].[Spatial_AlterPK] ([S_Geometry]) USING GEOMETRY_GRID WITH (CELLS_PER_OBJECT = 16, BOUNDING_BOX = (0, 0, 1, 1), GRIDS = (MEDIUM, MEDIUM, MEDIUM, MEDIUM))",
					"SPATIAL INDEX [IX_Spatial_Geometry_Auto] ON [dbo].[Spatial_AlterPK] ([S_Geometry]) USING GEOMETRY_AUTO_GRID WITH (CELLS_PER_OBJECT = 8, BOUNDING_BOX = (0, 0, 1, 1))",
					"SPATIAL INDEX [IX_Spatial_Geography] ON [dbo].[Spatial_AlterPK] ([S_Geography]) USING GEOGRAPHY_GRID WITH (CELLS_PER_OBJECT = 16, GRIDS = (MEDIUM, MEDIUM, MEDIUM, MEDIUM))",
					"SPATIAL INDEX [IX_Spatial_Geography_Auto] ON [dbo].[Spatial_AlterPK] ([S_Geography]) USING GEOGRAPHY_AUTO_GRID WITH (CELLS_PER_OBJECT = 12)",
				};

				AssertContainsExactElementsInAnyOrder<string>(expected
					, SpatialIndexLoader.Load(Db.Connection, null, "Spatial_AlterPK", null).Select(index => index.Definition));

				// 4. alter column default
				expected = new string[]
				{
					"SPATIAL INDEX [IX_Spatial_Geometry] ON [dbo].[Spatial_AlterColumnDefault] ([S_Geometry]) USING GEOMETRY_GRID WITH (CELLS_PER_OBJECT = 16, BOUNDING_BOX = (0, 0, 1, 1), GRIDS = (MEDIUM, MEDIUM, MEDIUM, MEDIUM))",
					"SPATIAL INDEX [IX_Spatial_Geometry_Auto] ON [dbo].[Spatial_AlterColumnDefault] ([S_Geometry]) USING GEOMETRY_AUTO_GRID WITH (CELLS_PER_OBJECT = 8, BOUNDING_BOX = (0, 0, 1, 1))",
					"SPATIAL INDEX [IX_Spatial_Geography] ON [dbo].[Spatial_AlterColumnDefault] ([S_Geography]) USING GEOGRAPHY_GRID WITH (CELLS_PER_OBJECT = 16, GRIDS = (MEDIUM, MEDIUM, MEDIUM, MEDIUM))",
					"SPATIAL INDEX [IX_Spatial_Geography_Auto] ON [dbo].[Spatial_AlterColumnDefault] ([S_Geography]) USING GEOGRAPHY_AUTO_GRID WITH (CELLS_PER_OBJECT = 12)",
				};

				AssertContainsExactElementsInAnyOrder<string>(expected
					, SpatialIndexLoader.Load(Db.Connection, null, "Spatial_AlterColumnDefault", null).Select(index => index.Definition));

				expected = new string[]
				{
					"(CONVERT([geometry],'POINT EMPTY'))",
					"(CONVERT([geography],'POINT EMPTY'))",
				};

				AssertContainsExactElementsInAnyOrder<string>(expected
					, GetDefaultsByTable(Db.Connection, "Spatial_AlterColumnDefault"));
			}
		}

		IEnumerable<string> GetDefaultsByTable(DbConnection connection, string tableName)
		{
			var result = new List<string>();
			Db.Connection.ExecuteReader($"SELECT definition FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID(N'{tableName}')",
				(reader) =>
				{
					result.Add((string)reader["definition"]);
				});

			return result;
		}

		/// <summary>
		/// Test if the default of column [Col2] has been changed to ('B')
		/// </summary>
		void AssertDefaultDiffTableSynchronised()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = 'Match_DefaultDiff' AND COLUMN_NAME = 'Col2'
				AND COLUMN_DEFAULT = '(''B'')'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		/// <summary>
		/// Test if the new CHECK constraint was created
		/// </summary>
		void AssertNewCheckConstraintCreated()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.TABLE_CONSTRAINTS
				WHERE TABLE_NAME = 'Match_DefaultDiff' AND CONSTRAINT_NAME = 'CK_Match_DefaultDiff_Col2'
				AND CONSTRAINT_TYPE = 'CHECK'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Convert.ToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		/// <summary>
		/// Test if the old FK has been removed
		/// </summary>
		void AssertOldFKRemoved()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
				WHERE CONSTRAINT_NAME = 'FK_Match_PKDiff_TO_Old_01'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(0, qtyRows);
		}

		/// <summary>
		/// Test if the new FK has been created
		/// </summary>
		void AssertNewFKCreated()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
				WHERE CONSTRAINT_NAME = 'FK_New_TO_Match_PKDiff_01' AND UNIQUE_CONSTRAINT_NAME = 'PK_Match_PKDiff'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		/// <summary>
		/// Test if the FK [FK_Match_DefaultDiff_TO_Match_PKDiff_01] has been changed (on delete cascade)
		/// </summary>
		void AssertFKCascadeActionSynchronised()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
				WHERE CONSTRAINT_NAME = 'FK_Match_DefaultDiff_TO_Match_PKDiff_01'
				AND UNIQUE_CONSTRAINT_NAME = 'PK_Match_PKDiff' AND UPPER(DELETE_RULE) = 'CASCADE'",
				SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		#endregion // Constraints and Indexes

		#region DocManager

		public void TestDocManagerDbSynchronised()
		{
			AssertNewDocManagerTableCreated();
			AssertNewDocManagerTablePKCreated();
		}

		/// <summary>
		/// Test if the table was created
		/// </summary>
		void AssertNewDocManagerTableCreated()
		{
			string sqlText = String.Format(
				"SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TestTable'",
				SchemaUpgraderForTest.TestDocManagerDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		/// <summary>
		/// Test if the PK was created
		/// </summary>
		void AssertNewDocManagerTablePKCreated()
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*) FROM {0}.INFORMATION_SCHEMA.TABLE_CONSTRAINTS
				WHERE TABLE_NAME = 'TestTable' AND CONSTRAINT_NAME = 'PK_TestTable'
				AND CONSTRAINT_TYPE = 'PRIMARY KEY'",
				SchemaUpgraderForTest.TestDocManagerDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(1, qtyRows);
		}

		#endregion

		#region DataContents

		public void TestDataContents()
		{
			AssertDataPreserved();
		}

		void AssertDataPreserved()
		{
			string sqlText = String.Format("SELECT COUNT(*) FROM {0}..Match_ColDiff", SchemaUpgraderForTest.TestMainDb);
			int qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(3, qtyRows);

			sqlText = String.Format("SELECT COUNT(*) FROM {0}..Match_PKDiff", SchemaUpgraderForTest.TestMainDb);
			qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(3, qtyRows);

			sqlText = String.Format("SELECT COUNT(*) FROM {0}..Match_IndexDiff", SchemaUpgraderForTest.TestMainDb);
			qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(3, qtyRows);

			sqlText = String.Format("SELECT COUNT(*) FROM {0}..Match_DefaultDiff", SchemaUpgraderForTest.TestMainDb);
			qtyRows = Utilities.ConvertToInt32(Db.Connection.ExecuteScalar(sqlText));
			AssertEquals(3, qtyRows);
		}

		#endregion

		#region Task Numbers

		public override BaseUpgrader GetNewUpgrader(BaseUpgraderUpgradeManagerForTesting dummyUpgradeManager)
		{
			return new SchemaUpgraderForTest(dummyUpgradeManager, Db.Connection);
		}
		#endregion

		#region Test classes

		class SchemaUpgraderForTest : Schema.SchemaUpgrader
		{
			internal SchemaUpgraderForTest(DbConnection upgConnection)
				: base(new DummyUpgradeManager(), upgConnection, upgConnection, upgConnection)
			{
			}

			internal SchemaUpgraderForTest(BaseUpgraderUpgradeManagerForTesting upgradeManager, DbConnection upgConnection)
				: base(upgradeManager, upgConnection, upgConnection, upgConnection)
			{
			}

			public const string TestMainDb = UpgUtils.UpgraderPrefix + "TestMainDB";
			public const string TestDocManagerDb = TestMainDb + "_SD001";

			public void DoUpgradeExposed() => DoUpgrade();

			protected override void DoUpgrade()
			{
				CreateTestDbs();

				using (((ICurrentDbControl)upgConnection).UseDatabase(TestMainDb))
				{
					UpgradeMainDbSchema();
					UpgradeDocManagerDatabases();
				}

				DropUpgAuxDatabases();
			}
			public void CleanTestResources()
			{
				DropTestDbs();
			}

			protected void CreateTestDbs()
			{
				testDbCreator.CreateDropExisting();
				testDocManagerDbCreator.CreateDropExisting();
			}

			protected void DropTestDbs()
			{
				testDocManagerDbCreator.Drop();
				testDbCreator.Drop();
				syncSchemaWrapper?.DropTemplateDb();
				eDocsyncSchemaWrapper?.DropTemplateDb();
			}

			void DropUpgAuxDatabases()
			{
				UpgUtils.CleanupAuxDatabases((AdminConnection)upgConnection, TestMainDb);
			}

			protected override void UpgradeMainDbSchema()
			{
				syncSchemaWrapper = new UnitTestSchemaSynchronisationWrapper(Manager, TestMainDb, upgConnection);
				syncSchemaWrapper.Run();
			}

			protected override void UpgradeDocManagerDatabases()
			{
				eDocsyncSchemaWrapper = new DocManagerSchemaSynchroniserForTest(Manager, TestDocManagerDb, upgConnection);
				eDocsyncSchemaWrapper.Run();
			}

			UnitTestSchemaSynchronisationWrapper syncSchemaWrapper;
			DocManagerSchemaSynchroniserForTest eDocsyncSchemaWrapper;
			readonly IAuxiliaryDbCreator testDbCreator = new MainUpgradeDbCreatorForTesting(TestMainDb);
			readonly IAuxiliaryDbCreator testDocManagerDbCreator = new AuxiliaryDbCreatorForTesting(TestDocManagerDb);
		}

		#endregion

	}
}
