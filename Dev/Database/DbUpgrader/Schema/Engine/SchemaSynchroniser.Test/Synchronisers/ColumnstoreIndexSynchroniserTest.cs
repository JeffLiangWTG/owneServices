using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class ColumnstoreIndexSynchroniserTest : TestCaseWithMockMainDbAndTemplateDbTransactional
	{
		public void TestSynchroniseAll()
		{
			AssertIndexExistsInTestMainDb("TableColumnstoreIndexAddTest", "NewIndex_DBO_TableColumnstoreIndexAddTest", expected: false);
			AssertIndexExistsInTestMainDb("TableColumnstoreIndexRemoveTest", "OldIndex_DBO_TableColumnstoreIndexRemoveTest", expected: true);
			AssertIndexExistsInTestMainDb("TableColumnstoreIndexRemoveAddTest", "OldIndex_DBO_TableColumnstoreIndexRemoveAddTest", expected: true);
			AssertIndexExistsInTestMainDb("TableColumnstoreIndexRemoveAddTest", "NewIndex_DBO_TableColumnstoreIndexRemoveAddTest", expected: false);
			AssertIndexExistsInTestMainDb("TableColumnstoreIndexIgnoreTest", "Index_DBO_TableColumnstoreIndexIgnoreTest", expected: true);

			AssertIndexExistsInTestTemplateDb("TableColumnstoreIndexAddTest", "NewIndex_DBO_TableColumnstoreIndexAddTest", expected: true);
			AssertIndexExistsInTestTemplateDb("TableColumnstoreIndexRemoveTest", "OldIndex_DBO_TableColumnstoreIndexRemoveTest", expected: false);
			AssertIndexExistsInTestTemplateDb("TableColumnstoreIndexRemoveAddTest", "OldIndex_DBO_TableColumnstoreIndexRemoveAddTest", expected: false);
			AssertIndexExistsInTestTemplateDb("TableColumnstoreIndexRemoveAddTest", "NewIndex_DBO_TableColumnstoreIndexRemoveAddTest", expected: true);
			AssertIndexExistsInTestTemplateDb("TableColumnstoreIndexIgnoreTest", "Index_DBO_TableColumnstoreIndexIgnoreTest", expected: false);

			var testScriptRunner = new ColumnstoreIndexSynchroniser(TestConnection, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(testScriptRunner.SynchroniseAll);

			// In Main DB and not in the Template - should be removed
			AssertIndexExistsInTestMainDb("TableColumnstoreIndexRemoveTest", "OldIndex_DBO_TableColumnstoreIndexRemoveTest", expected: false);
			AssertIndexExistsInTestMainDb("TableColumnstoreIndexRemoveAddTest", "OldIndex_DBO_TableColumnstoreIndexRemoveAddTest", expected: false);

			// Not in Main DB and in the Template - should be added
			AssertIndexExistsInTestMainDb("TableColumnstoreIndexAddTest", "NewIndex_DBO_TableColumnstoreIndexAddTest", expected: true);
			AssertIndexExistsInTestMainDb("TableColumnstoreIndexRemoveAddTest", "NewIndex_DBO_TableColumnstoreIndexRemoveAddTest", expected: true);

			// Table not in Template DB - should be ignored
			AssertIndexExistsInTestMainDb("TableColumnstoreIndexIgnoreTest", "Index_DBO_TableColumnstoreIndexIgnoreTest", expected: true);
		}

		#region Implementation

		protected override IAuxiliaryDbCreator GetMockMainDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockMainDb, createTestMainDbObjectsScript);
		}

		protected override IAuxiliaryDbCreator GetMockTemplateDbCreator()
		{
			return new AuxiliaryDbCreatorForTesting(mockTemplateDb, createTestTemplateDbObjectsScript);
		}

		void AssertIndexExistsInTestMainDb(string tableName, string indexName, bool expected)
		{
			if (expected)
			{
				AssertClusteredColumnstoreIndexExistsInDb(TestConnection, mockMainDb, tableName, indexName);
			}
			else
			{
				AssertClusteredColumnstoreIndexDoesNotExistInDb(TestConnection, mockMainDb, indexName);
			}
		}

		void AssertIndexExistsInTestTemplateDb(string tableName, string indexName, bool expected)
		{
			if (expected)
			{
				AssertClusteredColumnstoreIndexExistsInDb(TestConnection, mockTemplateDb, tableName, indexName);
			}
			else
			{
				AssertClusteredColumnstoreIndexDoesNotExistInDb(TestConnection, mockTemplateDb, indexName);
			}
		}

		void AssertClusteredColumnstoreIndexExistsInDb(DbConnection connection, string dbName, string tableName, string indexName)
		{
			string sqlText = String.Format(@"
				SELECT COUNT(*)
				FROM
					[{0}].sys.schemas sch
					INNER JOIN [{0}].sys.tables tab ON tab.schema_id = sch.schema_id
					INNER JOIN [{0}].sys.indexes ind ON ind.object_id = tab.object_id
				WHERE
					sch.name = 'dbo'
					AND tab.name = '{1}'
					AND ind.type = 5
					AND ind.name = '{2}'",
				dbName, tableName, indexName);

			int qtyRows = Convert.ToInt32(connection.ExecuteScalar(sqlText));
			string assertMessage = String.Format("Index: dbo.{0}.{1} exists?", tableName, indexName);

			AssertEquals(assertMessage, true, qtyRows == 1);
		}

		void AssertClusteredColumnstoreIndexDoesNotExistInDb(DbConnection connection, string dbName, string indexName)
		{
			string sqlText = String.Format(
				"SELECT COUNT(*) FROM [{0}].sys.indexes WHERE type = 5 AND name = '{1}'",
				dbName, indexName);

			int qtyRows = Convert.ToInt32(connection.ExecuteScalar(sqlText));
			string assertMessage = String.Format("Index: {0} exists?", indexName);

			AssertEquals(assertMessage, false, qtyRows > 0);
		}

		#region Scripts

		const string createTestMainDbObjectsScript = @"
			-- This table will have a columnstore index added
			CREATE TABLE [dbo].[TableColumnstoreIndexAddTest] (Col1 INT NOT NULL);

			-- This table will have a columnstore index removed
			CREATE TABLE [dbo].[TableColumnstoreIndexRemoveTest] (Col1 VARCHAR(30) NULL);
			CREATE CLUSTERED COLUMNSTORE INDEX [OldIndex_DBO_TableColumnstoreIndexRemoveTest] ON [dbo].[TableColumnstoreIndexRemoveTest];

			-- This table will have a columnstore index removed and a new one added
			CREATE TABLE [dbo].[TableColumnstoreIndexRemoveAddTest] (Col1 DATETIME NULL);
			CREATE CLUSTERED COLUMNSTORE INDEX [OldIndex_DBO_TableColumnstoreIndexRemoveAddTest] ON [dbo].[TableColumnstoreIndexRemoveAddTest];

			-- This table does not exist in the template database. It will be ignored
			CREATE TABLE [dbo].[TableColumnstoreIndexIgnoreTest] (Col1 BIT NOT NULL);
			CREATE CLUSTERED COLUMNSTORE INDEX [Index_DBO_TableColumnstoreIndexIgnoreTest] ON [dbo].[TableColumnstoreIndexIgnoreTest];
			";

		const string createTestTemplateDbObjectsScript = @"
			-- This table had a columnstore index added
			CREATE TABLE [dbo].[TableColumnstoreIndexAddTest] (Col1 INT NOT NULL);
			CREATE CLUSTERED COLUMNSTORE INDEX [NewIndex_DBO_TableColumnstoreIndexAddTest] ON [dbo].[TableColumnstoreIndexAddTest];

			-- This table had a columnstore index removed
			CREATE TABLE [dbo].[TableColumnstoreIndexRemoveTest] (Col1 VARCHAR(30) NULL);

			-- This table had a columnstore index removed and a new one added
			CREATE TABLE [dbo].[TableColumnstoreIndexRemoveAddTest] (Col1 DATETIME NULL);
			CREATE CLUSTERED COLUMNSTORE INDEX [NewIndex_DBO_TableColumnstoreIndexRemoveAddTest] ON [dbo].[TableColumnstoreIndexRemoveAddTest];
			";

		#endregion

		#endregion
	}
}
