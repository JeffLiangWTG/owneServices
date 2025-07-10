using System;
using CargoWise.Data.SqlServer;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DataUtilsTransactionalTest : TransactionedTestCase
	{
		public void TestLoadAndSaveDbExtendedProperty()
		{
			var testProperty = "TestLoadAndSaveDbExtendedProperty";

			var testValue = DataUtils.LoadDbExtendedProperty(TestConnection, testProperty);
			AssertNull("Initial property value", DataUtils.LoadDbExtendedProperty(TestConnection, testProperty));

			DataUtils.SaveDbExtendedProperty(TestConnection, testProperty, "abc");
			AssertEquals("Property value", "abc", DataUtils.LoadDbExtendedProperty(TestConnection, testProperty));
			AssertEquals("Property value", "abc", DataUtils.LoadDbExtendedProperty(TestConnection, testProperty, Db.DatabaseName));

			DataUtils.SaveDbExtendedProperty(TestConnection, testProperty, "xyz", Db.DatabaseName);
			AssertEquals("Property value", "xyz", DataUtils.LoadDbExtendedProperty(TestConnection, testProperty));
			AssertEquals("Property value", "xyz", DataUtils.LoadDbExtendedProperty(TestConnection, testProperty, Db.DatabaseName));

			DataUtils.DropDbExtendedProperty(TestConnection, testProperty);
			AssertNull("Is Property dropped?", DataUtils.LoadDbExtendedProperty(TestConnection, testProperty));
		}
		public void TestLoadAndSaveTableExtendedProperty()
		{
			Db.Connection.ExecuteNonQuery("CREATE TABLE dbo._test(id int);");

			var testProperty = "TestLoadAndSaveTableExtendedProperty";
			var table = new DbSchemaTable(string.Format("[{0}]..{1}", Db.Connection.CurrentDatabase, "_test"));

			AssertEquals("Initial property value", null, DataUtils.LoadTableExtendedProperty(TestConnection, table, testProperty));

			DataUtils.UpdateTableExtendedProperty(TestConnection, table, testProperty, "abc");
			AssertEquals("Property value", "abc", DataUtils.LoadTableExtendedProperty(TestConnection, table, testProperty));

			DataUtils.UpdateTableExtendedProperty(TestConnection, table, testProperty, null);
			AssertEquals("Property value", null, DataUtils.LoadTableExtendedProperty(TestConnection, table, testProperty));

			DataUtils.UpdateTableExtendedProperty(TestConnection, table, testProperty, "xyz");
			AssertEquals("Property value", "xyz", DataUtils.LoadTableExtendedProperty(TestConnection, table, testProperty));

			DataUtils.DropTableExtendedProperty(TestConnection, table, testProperty);
			AssertEquals("Is Property dropped?", null, DataUtils.LoadTableExtendedProperty(TestConnection, table, testProperty));

			var table3 = new DbSchemaTable(string.Format("[{0}]..{1}", Db.Connection.CurrentDatabase, "NonExistentTable"));
			AssertEquals("Table does not exist => no property loaded", null, DataUtils.LoadTableExtendedProperty(TestConnection, table3, testProperty));

			var table2 = new DbSchemaTable(string.Format("[{0}]..{1}", "NonExistentDatabase", "_test"));
			AssertExceptionThrown(
				typeof(SqlException),
				"Database 'NonExistentDatabase' does not exist. Make sure that the name is entered correctly.",
				() => DataUtils.LoadTableExtendedProperty(TestConnection, table2, testProperty));
		}

		public void TestObjectExists()
		{
			string testTableName = "_test_" + Guid.NewGuid().ToString().Replace("-", "");
			AssertEquals(false, DataUtils.ObjectExists(TestConnection, testTableName));

			TestConnection.ExecuteNonQuery(string.Format("CREATE TABLE dbo.{0}(id int);", testTableName));

			AssertEquals(true, DataUtils.ObjectExists(TestConnection, testTableName));
			AssertEquals(true, DataUtils.ObjectExists(TestConnection, string.Format("[{0}]", testTableName)));
			AssertEquals(true, DataUtils.ObjectExists(TestConnection, string.Format("dbo.{0}", testTableName)));
			AssertEquals(true, DataUtils.ObjectExists(TestConnection, string.Format("dbo.[{0}]", testTableName)));
			AssertEquals(true, DataUtils.ObjectExists(TestConnection, string.Format("[dbo].{0}", testTableName)));
			AssertEquals(true, DataUtils.ObjectExists(TestConnection, string.Format("[dbo].[{0}]", testTableName)));
			AssertEquals(false, DataUtils.ObjectExists(TestConnection, string.Format("xxx.{0}", testTableName)));
			AssertEquals(true, DataUtils.ObjectExists(TestConnection, string.Format(TestConnection.CurrentDatabase + ".dbo.{0}", testTableName)));
			AssertEquals(true, DataUtils.ObjectExists(TestConnection, string.Format(TestConnection.CurrentDatabase + "..{0}", testTableName)));
		}

		public void TestGetTheMostPopularValueForTheColumn()
		{
			var sql = @"
if (OBJECT_ID('dbo._testStatistics', 'U') is NOT NULL) DROP TABLE dbo._testStatistics;
CREATE TABLE dbo._testStatistics
(
	Col_1 int NOT NULL,
	Col_2 int NOT NULL,
	Col_3 int     NULL,
);

INSERT dbo._testStatistics (Col_1, Col_2, Col_3) VALUES
	(1, 11, NULL),
	(1, 11, NULL),
	(1, 22, NULL),
	(2, 22, NULL),
	(2, 22, 333);
";

			Db.Connection.ExecuteNonQuery(sql);

			AssertEquals("1", DataUtils.GetTheMostPopularValueForTheColumn(Db.Connection, Db.DatabaseName, "dbo", "_testStatistics", "Col_1"));
			AssertEquals("22", DataUtils.GetTheMostPopularValueForTheColumn(Db.Connection, Db.DatabaseName, "dbo", "_testStatistics", "Col_2"));
			AssertEquals(null, DataUtils.GetTheMostPopularValueForTheColumn(Db.Connection, Db.DatabaseName, "dbo", "_testStatistics", "Col_3"));
		}

		public void TestBuildUniqueKeyList()
		{
			var createTestTableSql = @"
				CREATE TABLE dbo.TestBuildUniqueSingleKeyList
				(
					Col1 BIT,
					Col2 BIT,
					Col3 BIT,
					Col4 BIT,
					Col5 BIT,
					Col6 BIT,
					Col7 BIT,
					Col8 BIT,
					Col9 BIT,
					ColA BIT
				);
				CREATE UNIQUE NONCLUSTERED INDEX IndexSingleKeyNoInclude ON dbo.TestBuildUniqueSingleKeyList (Col1);
				CREATE UNIQUE NONCLUSTERED INDEX IndexSingleKeyWithInclude ON dbo.TestBuildUniqueSingleKeyList (Col2) INCLUDE (Col3);
				CREATE UNIQUE NONCLUSTERED INDEX IndexSingleKeyWithFilter ON dbo.TestBuildUniqueSingleKeyList (Col4) WHERE (Col5 = 1);
				CREATE UNIQUE NONCLUSTERED INDEX IndexMultiKeyNoInclude ON dbo.TestBuildUniqueSingleKeyList (Col6, Col7);
				CREATE UNIQUE NONCLUSTERED INDEX IndexMultiKeyWithInclude ON dbo.TestBuildUniqueSingleKeyList (Col8, Col9) INCLUDE (ColA);
				";

			TestConnection.ExecuteNonQuery(createTestTableSql);

			var indexes = DataUtils.BuildUniqueSingleKeyList();

			CombineAssertions(
				() =>
				{
					AssertCollectionContains("[Col1] Contains column in a single key index without include columns?", "TESTBUILDUNIQUESINGLEKEYLIST.COL1", indexes);
					AssertCollectionContains("[Col2] Contains column in a single key index with include columns?", "TESTBUILDUNIQUESINGLEKEYLIST.COL2", indexes);
					AssertCollectionNotContains("[Col3] Contains include only column?", "TESTBUILDUNIQUESINGLEKEYLIST.COL3", indexes);
					AssertCollectionContains("[Col4] Contains column in a single key index with filter?", "TESTBUILDUNIQUESINGLEKEYLIST.COL4", indexes);
					AssertCollectionNotContains("[Col5] Contains filter only column?", "TESTBUILDUNIQUESINGLEKEYLIST.COL5", indexes);
					AssertCollectionNotContains("[Col6] Contains column in a composite key index without include columns?", "TESTBUILDUNIQUESINGLEKEYLIST.COL6", indexes);
					AssertCollectionNotContains("[Col7] Contains column in a composite key index without include columns?", "TESTBUILDUNIQUESINGLEKEYLIST.COL7", indexes);
					AssertCollectionNotContains("[Col8] Contains column in a composite key index with include columns?", "TESTBUILDUNIQUESINGLEKEYLIST.COL8", indexes);
					AssertCollectionNotContains("[Col9] Contains column in a composite key index with include columns?", "TESTBUILDUNIQUESINGLEKEYLIST.COL9", indexes);
					AssertCollectionNotContains("[ColA] Contains include only column?", "TESTBUILDUNIQUESINGLEKEYLIST.COLA", indexes);
				}
			);
		}

		public void TestShouldDisableAutoStatisticsDuringUpgrade()
		{
			TestConnection.ExecuteNonQuery(@"DELETE FROM dbo.StmData WHERE SD_Name = 'ISU_DisableAutoStatisticsDuringUpgrade';");

			Assert(DataUtils.ShouldDisableAutoStatisticsDuringUpgrade(TestConnection, Db.DatabaseName));

			TestConnection.ExecuteNonQuery(@"
					INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Type, SD_BinaryValue)
					VALUES (newid(), 'ISU_DisableAutoStatisticsDuringUpgrade', 'BOL', NULL);
				");

			Assert(DataUtils.ShouldDisableAutoStatisticsDuringUpgrade(TestConnection, Db.DatabaseName));

			TestConnection.ExecuteNonQuery(@"
					UPDATE dbo.StmData 
					SET SD_BinaryValue = CONVERT(VARBINARY(MAX), N'False')
					WHERE SD_Name = 'ISU_DisableAutoStatisticsDuringUpgrade';
				");

			Assert(!DataUtils.ShouldDisableAutoStatisticsDuringUpgrade(TestConnection, Db.DatabaseName));

			TestConnection.ExecuteNonQuery(@"
					UPDATE dbo.StmData 
					SET SD_BinaryValue = CONVERT(VARBINARY(MAX), N'True')
					WHERE SD_Name = 'ISU_DisableAutoStatisticsDuringUpgrade';
				");

			Assert(DataUtils.ShouldDisableAutoStatisticsDuringUpgrade(TestConnection, Db.DatabaseName));
		}
	}
}
