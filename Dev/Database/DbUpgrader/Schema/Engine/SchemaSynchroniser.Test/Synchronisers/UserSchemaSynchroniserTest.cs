using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	sealed class UserSchemaSynchroniserTest : TestCaseWithMockMainDbAndTemplateDbTransactional
	{
		public void TestCreateNewUserSchemas()
		{
			// PRE-CONDITION
			AssertUserSchemaExistsInTestMainDb("TestSchema", expected: true);
			AssertUserSchemaExistsInTestMainDb("NewSchema", expected: false);
			AssertUserSchemaExistsInTestMainDb("[NewBracketedNameSchema]", expected: false);
			AssertUserSchemaExistsInTestTemplateDb("TestSchema", expected: true);
			AssertUserSchemaExistsInTestTemplateDb("NewSchema", expected: true);
			AssertUserSchemaExistsInTestTemplateDb("[NewBracketedNameSchema]", expected: true);

			// Create new user schemas
			var testSynchroniser = new UserSchemaSynchroniser(TestConnection, mockMainDb, mockTemplateDb, new DummyUpgradeManager());
			RunActionOnMockMainDb(testSynchroniser.CreateNewUserSchemas);

			// Assert results
			AssertUserSchemaExistsInTestMainDb("TestSchema", expected: true);
			AssertUserSchemaExistsInTestMainDb("NewSchema", expected: true);
			AssertUserSchemaExistsInTestMainDb("[NewBracketedNameSchema]", expected: false);
		}

		public void TestDropOldUserSchemas()
		{
			// PRE-CONDITION
			AssertUserSchemaExistsInTestMainDb("TestSchema", expected: true);
			AssertUserSchemaExistsInTestMainDb("OldSchemaNoObjects", expected: true);
			AssertUserSchemaExistsInTestMainDb("OldSchemaWithObjects", expected: true);
			AssertUserSchemaExistsInTestMainDb("[OldBracketedNameSchema]", expected: true);
			AssertUserSchemaExistsInTestTemplateDb("TestSchema", expected: true);
			AssertUserSchemaExistsInTestTemplateDb("OldSchemaNoObjects", expected: false);
			AssertUserSchemaExistsInTestTemplateDb("OldSchemaWithObjects", expected: false);
			AssertUserSchemaExistsInTestTemplateDb("[OldBracketedNameSchema]", expected: false);

			// Drop empty old user schemas
			var testSynchroniser = new UserSchemaSynchroniser(TestConnection, mockMainDb, mockTemplateDb, new DummyUpgradeManager());
			RunActionOnMockMainDb(testSynchroniser.DropOldUserSchemas);

			// Assert results (OldSchemaWithObjects not removed as it contains objects)
			AssertUserSchemaExistsInTestMainDb("TestSchema", expected: true);
			AssertUserSchemaExistsInTestMainDb("OldSchemaNoObjects", expected: false);
			AssertUserSchemaExistsInTestMainDb("OldSchemaWithObjects", expected: true);
			AssertUserSchemaExistsInTestMainDb("[OldBracketedNameSchema]", expected: false);

			// Drop OldSchemaWithObjects child table and retry
			TestConnection.ExecuteNonQuery("DROP TABLE [" + mockMainDb + "].[OldSchemaWithObjects].[TestTable]");
			RunActionOnMockMainDb(testSynchroniser.DropOldUserSchemas);

			// Assert results (OldSchemaWithObjects not removed as it contains objects)
			AssertUserSchemaExistsInTestMainDb("TestSchema", expected: true);
			AssertUserSchemaExistsInTestMainDb("OldSchemaNoObjects", expected: false);
			AssertUserSchemaExistsInTestMainDb("OldSchemaWithObjects", expected: false);
			AssertUserSchemaExistsInTestMainDb("[OldBracketedNameSchema]", expected: false);
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

		void AssertUserSchemaExistsInTestMainDb(string schemaName, bool expected)
		{
			AssertEquals(
				String.Format("Schema [{0}] exists in [{1}]?", schemaName, mockMainDb),
				expected,
				DoesUserSchemaExistInDb(TestConnection, mockMainDb, schemaName));
		}

		void AssertUserSchemaExistsInTestTemplateDb(string schemaName, bool expected)
		{
			AssertEquals(
				String.Format("Schema [{0}] exists in [{1}]?", schemaName, mockTemplateDb),
				expected,
				DoesUserSchemaExistInDb(TestConnection, mockTemplateDb, schemaName));
		}

		bool DoesUserSchemaExistInDb(DbConnection connection, string dbName, string schemaName)
		{
			string sqlText = String.Format(
				"IF EXISTS (SELECT null FROM [{0}].sys.schemas WHERE name = '{1}') SELECT 1 ELSE SELECT 0",
				dbName, schemaName);
			return Convert.ToBoolean(connection.ExecuteScalar(sqlText));
		}

		#region Scripts

		const string createTestMainDbObjectsScript = @"
			EXEC ('CREATE SCHEMA [TestSchema]');
			EXEC ('CREATE SCHEMA [OldSchemaNoObjects]');
			EXEC ('CREATE SCHEMA [OldSchemaWithObjects]');
			EXEC ('CREATE SCHEMA [[OldBracketedNameSchema]]]');

			CREATE TABLE [OldSchemaWithObjects].[TestTable] (Col1 INT);
			";

		const string createTestTemplateDbObjectsScript = @"
			EXEC ('CREATE SCHEMA [TestSchema]');
			EXEC ('CREATE SCHEMA [NewSchema]');
			EXEC ('CREATE SCHEMA [[NewBracketedNameSchema]]]');
			";

		#endregion

		#endregion
	}
}
