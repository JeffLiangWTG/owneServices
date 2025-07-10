using System;
using System.Data;
using CargoWise.BuildTools.Testing;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.Builder.Generator
{
	sealed class AutoSchemaGeneratorTest : TransactionedTestCase
	{
		public void TestDatabaseCommaDelimitedBusinessObjectTableTextList()
		{
			AutoSchemaGeneratorForTesting testGenerator = new AutoSchemaGeneratorForTesting(outputDirectory);
			string mainDbCsvBizObjTableList = testGenerator.DatabaseCommaDelimitedBusinessObjectTableTextList_Exposed[""].CsvBizObjTableList;

			bool isStmEventTableOnTheList = (mainDbCsvBizObjTableList.IndexOf("StmEvent") >= 0);
			bool isJobShipmentTableOnTheList = (mainDbCsvBizObjTableList.IndexOf("JobShipment") >= 0);
			bool isStmNumberCacheTableOnTheList = (mainDbCsvBizObjTableList.IndexOf("StmNumberCache") >= 0);
			bool isDummyBizoTableOnTheList = (mainDbCsvBizObjTableList.IndexOf("DummyBizo") >= 0);

			AssertEquals("StmEvent table has a business object and should be on the main DB bizo table list", true, isStmEventTableOnTheList);
			AssertEquals("JobShipment table has a business object and should be on the main DB bizo table list", true, isJobShipmentTableOnTheList);
			AssertEquals("StmNumberCache table does NOT have a business object and should NOT be on the main DB bizo table list", false, isStmNumberCacheTableOnTheList);

			AssertEquals("DummyBizo table has a business object and should be on the main DB bizo table list", true, isDummyBizoTableOnTheList);
		}

		public void TestPersistantTablesColumns()
		{
			Db.Connection.ExecuteNonQuery("CREATE TABLE [" + TestNonBizoNoSchemaClassTable + "] (col1 tinyint)");

			var testGenerator = new AutoSchemaGeneratorForTesting(outputDirectory);
			var persistantTables = testGenerator.PersistantTablesColumns_Exposed;

			var isStmEventTableOnTheList = false;
			var isJobShipmentTableOnTheList = false;
			var isDummyBizoTableOnTheList = false;
			var isNonBizoTableWithoutSchemaClassOnTheList = false;

			foreach (DataTable table in persistantTables)
			{
				if (table.TableName == "StmEvent")
				{
					isStmEventTableOnTheList = true;
				}
				else if (table.TableName == "JobShipment")
				{
					isJobShipmentTableOnTheList = true;
				}
				else if (table.TableName == "DummyBizo")
				{
					isDummyBizoTableOnTheList = true;
				}
				else if (table.TableName == TestNonBizoNoSchemaClassTable)
				{
					isNonBizoTableWithoutSchemaClassOnTheList = true;
				}
			}

			AssertEquals("StmEvent table has a business object and should be on the list", true, isStmEventTableOnTheList);
			AssertEquals("JobShipment table has a business object and should be on the list", true, isJobShipmentTableOnTheList);

			AssertEquals(
				"DummyBizo is a DUMMY DEVELOPMENT ONLY table but it has a business object, so it should be on the list",
				true, isDummyBizoTableOnTheList);

			AssertEquals(
				"TestNonBizoNoSchemaClassTable does NOT have a business object but it will have a Schema class, so it should be on the list",
				true, isNonBizoTableWithoutSchemaClassOnTheList);
		}

		public void TestGenerateSchemaForNonBusinessObjectTables()
		{
			Db.Connection.ExecuteNonQuery("CREATE TABLE [" + TestNonBizoNoSchemaClassTable + "] (col1 tinyint)");

			AutoSchemaGeneratorForTesting testGenerator = new AutoSchemaGeneratorForTesting(outputDirectory);
			testGenerator.GenerateSchemaForNonBusinessObjectTables();

			bool isStmEventTableOnTheList = false;
			bool isJobShipmentTableOnTheList = false;
			bool isDummyBizoTableOnTheList = false;
			bool isNonBizoTableWithoutSchemaClassOnTheList = false;

			foreach (string dbAndTableName in testGenerator.GeneratedNonBizObjTableSchemaList)
			{
				if (dbAndTableName.Equals("StmEvent", StringComparison.OrdinalIgnoreCase))
				{
					isStmEventTableOnTheList = true;
				}
				else if (dbAndTableName.Equals("JobShipment", StringComparison.OrdinalIgnoreCase))
				{
					isJobShipmentTableOnTheList = true;
				}
				else if (dbAndTableName.Equals("DummyBizo", StringComparison.OrdinalIgnoreCase))
				{
					isDummyBizoTableOnTheList = true;
				}
				else if (dbAndTableName.Equals(TestNonBizoNoSchemaClassTable, StringComparison.OrdinalIgnoreCase))
				{
					isNonBizoTableWithoutSchemaClassOnTheList = true;
				}
			}

			AssertEquals("StmEvent table has a business object and so it should NOT be on the generated schema list", false, isStmEventTableOnTheList);
			AssertEquals("JobShipment table has a business object and so it should NOT be on the generated schema list", false, isJobShipmentTableOnTheList);

			AssertEquals(
				"DummyBizo has a business object and it is a DUMMY DEVELOPMENT ONLY table, so it should NOT be on the generated schema list",
				false, isDummyBizoTableOnTheList);

			// File is generated but not added to SourceControl or the Solution
			AssertEquals(
				"TestNonBizoNoSchemaClassTable does NOT have a business object nor a Schema class, but it should still be generated",
				true, isNonBizoTableWithoutSchemaClassOnTheList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			outputDirectory = new FauxGeneratorOutputDirectory(GeneratorOutputDirectory.SaveMode.CheckOut);
		}

		protected override void TearDown()
		{
			base.TearDown();
			outputDirectory.Dispose();
		}

		GeneratorOutputDirectory outputDirectory;

		const string TestNonBizoNoSchemaClassTable = "NonBizoNoSchemaClassTable842E3A4F07B34301861F92BD26C3B088";
	}
}
