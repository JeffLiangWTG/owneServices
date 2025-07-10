using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations
{
	sealed class SourceTableCollectionTest : TestWithTransformationDirectorCopyDb
	{
		public void TestAddAndCount()
		{
			SourceTable testTable1 = new SourceTable(Db.DatabaseName, "Table1", new SourceColumn[] { new SourceColumn("Col1", "int") }, null);
			SourceTable testTable2 = new SourceTable(Db.DatabaseName, "Table2", new SourceColumn[] { new SourceColumn("Col1", "int") }, null);

			SourceTableCollection testCollection = new SourceTableCollection();

			AssertEquals("Collection should be empty", 0, testCollection.Count);

			testCollection.Add(testTable1);
			testCollection.Add(testTable2);

			AssertEquals("Collection should have 2 tables", 2, testCollection.Count);
			AssertEquals("Table1 name", "Table1", testCollection[testTable1.OriginalName].OriginalName);
			AssertEquals("Table2 name", "Table2", testCollection[testTable2.OriginalName].OriginalName);
		}

		public void TestAreAllTablesCopied()
		{
			Db.Connection.ExecuteNonQuery("if (OBJECT_ID('Table1', 'U') is NOT NULL) DROP TABLE Table1; CREATE TABLE TAble1 (Col1 int);");
			Db.Connection.ExecuteNonQuery("if (OBJECT_ID('Table2', 'U') is NOT NULL) DROP TABLE Table2; CREATE TABLE TAble2 (Col1 int);");

			SourceTableForTesting testTable1 = new SourceTableForTesting(Db.DatabaseName, "Table1", new SourceColumn[] { new SourceColumn("Col1", "int") }, null);
			SourceTableForTesting testTable2 = new SourceTableForTesting(Db.DatabaseName, "Table2", new SourceColumn[] { new SourceColumn("Col1", "int") }, null);

			SourceTableCollection testCollection = new SourceTableCollection();
			testCollection.Add(testTable1);
			testCollection.Add(testTable2);

			AssertEquals("No table should be copied", false, testCollection.AreAllTablesCopied);
			testTable1.Create();
			AssertEquals("NOT all tables should be copied", false, testCollection.AreAllTablesCopied);
			testTable2.Create();
			AssertEquals("All tables should be copied", true, testCollection.AreAllTablesCopied);
		}

		public void TestAddThrowsAnExceptionIfAtLeastOneTableIsAlreadyCopied()
		{
			Db.Connection.ExecuteNonQuery("if (OBJECT_ID('Table1', 'U') is NOT NULL) DROP TABLE Table1; CREATE TABLE TAble1 (Col1 int);");

			SourceTableForTesting testTable1 = new SourceTableForTesting(Db.DatabaseName, "Table1", new SourceColumn[] { new SourceColumn("Col1", "int") }, null);
			SourceTableForTesting testTable2 = new SourceTableForTesting(Db.DatabaseName, "Table2", new SourceColumn[] { new SourceColumn("Col1", "int") }, null);

			SourceTableCollection testCollection = new SourceTableCollection();

			testCollection.Add(testTable1);
			testTable1.Create();

			try
			{
				testCollection.Add(testTable2);
				Fail("Should have thrown an InvalidOperationException");
			}
			catch (InvalidOperationException e)
			{
				AssertNotNull("Exception Thrown", e.Message);
			}

			testTable1.Drop();
			testCollection.Add(testTable2);
			AssertEquals("Collection should have 2 tables", 2, testCollection.Count);
		}
	}
}
