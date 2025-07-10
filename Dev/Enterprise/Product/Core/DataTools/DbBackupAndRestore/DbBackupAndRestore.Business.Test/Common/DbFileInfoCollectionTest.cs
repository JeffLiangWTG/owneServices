using System;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class DbFileInfoCollectionTest : TestCase
	{
		public void TestIndexer()
		{
			DbFileInfoCollection collection = new DbFileInfoCollection();

			DbFileInfo dbFile1 = new DbFileInfo("Db_Data", @"E:\MSSQL\Data", "D");
			DbFileInfo dbFile2 = new DbFileInfo("Db_Log", @"L:\MSSQL\Log\", "L");

			AssertEquals("(File 1) FileSequencePerType - Before adding to collection", 0, dbFile1.FileSequencePerType);
			AssertEquals("(File 2) FileSequencePerType - Before adding to collection", 0, dbFile2.FileSequencePerType);

			collection.Add(dbFile1);
			collection.Add(dbFile2);

			AssertEquals("(File 1) FileSequencePerType - After adding to collection", 1, dbFile1.FileSequencePerType);
			AssertEquals("(File 2) FileSequencePerType - After adding to collection", 1, dbFile2.FileSequencePerType);

			AssertEquals("Indexer should return first element added", dbFile1, collection[0]);
			AssertEquals("Indexer should return second element added", dbFile2, collection[1]);

			AssertEquals("First element PhysicalName", @"E:\MSSQL\Data", collection[0].FolderPath);
			AssertEquals("Second element PhysicalName", @"L:\MSSQL\Log\", collection[1].FolderPath);
		}

		public void TestSetDbFileTypeVisible()
		{
			DbFileInfoCollection collection = new DbFileInfoCollection();

			DbFileInfo dbFile0 = new DbFileInfo("Db_Data0", @"E:\MSSQL\Data", "D", "", DbFileInfo.DbTypeMain);
			DbFileInfo dbFile1 = new DbFileInfo("Db_Data1", @"E:\MSSQL\Data", "D", "", DbFileInfo.DbTypeEDocs);
			DbFileInfo dbFile2 = new DbFileInfo("Db_Data2", @"E:\MSSQL\Data", "D", "", DbFileInfo.DbTypeRefDB);
			DbFileInfo dbFile3 = new DbFileInfo("Db_Data3", @"E:\MSSQL\Data", "D", "", DbFileInfo.DbTypeUserRepository);

			collection.Add(dbFile0);
			collection.Add(dbFile1);
			collection.Add(dbFile2);
			collection.Add(dbFile3);

			for (int i = 0; i < collection.Count; i++)
			{
				AssertEquals(collection[i].Visible, true);
			}

			collection.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, false);
			AssertEquals(collection[1].Visible, false);

			collection.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, false);
			AssertEquals(collection[2].Visible, false);

			collection.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, false);
			AssertEquals(collection[3].Visible, false);

			collection.SetDbFileTypeVisible(DbFileInfo.DbTypeEDocs, true);
			collection.SetDbFileTypeVisible(DbFileInfo.DbTypeRefDB, true);
			collection.SetDbFileTypeVisible(DbFileInfo.DbTypeUserRepository, true);

			for (int i = 0; i < collection.Count; i++)
			{
				AssertEquals(collection[i].Visible, true);
			}
		}

		public void TestAdd()
		{
			DbFileInfoCollection collection = new DbFileInfoCollection();

			AssertEquals("Precondition: list empty", 0, collection.Count);

			DbFileInfo dbFile1 = new DbFileInfo("", "", "D");
			AssertEquals("(File 1) FileSequencePerType - Before adding to collection", 0, dbFile1.FileSequencePerType);
			collection.Add(dbFile1);
			AssertEquals("Should now have one element after add", 1, collection.Count);
			AssertEquals("(File 1) FileSequencePerType - After adding to collection", 1, dbFile1.FileSequencePerType);
			AssertEquals("(File 1) FilePathSuffix - After adding to collection", "_Data.mdf", dbFile1.FilePathSuffix);

			DbFileInfo dbFile2 = new DbFileInfo("", "", "L");
			AssertEquals("(File 2) FileSequencePerType - Before adding to collection", 0, dbFile2.FileSequencePerType);
			collection.Add(dbFile2);
			AssertEquals("Should now have two elements after add", 2, collection.Count);
			AssertEquals("(File 2) FileSequencePerType - After adding to collection", 1, dbFile2.FileSequencePerType);
			AssertEquals("(File 2) FilePathSuffix - After adding to collection", "_Log.ldf", dbFile2.FilePathSuffix);

			DbFileInfo dbFile3 = new DbFileInfo("", "", "L");
			AssertEquals("(File 3) FileSequencePerType - Before adding to collection", 0, dbFile3.FileSequencePerType);
			collection.Add(dbFile3);
			AssertEquals("Should now have two elements after add", 3, collection.Count);
			AssertEquals("(File 3) FileSequencePerType - After adding to collection", 2, dbFile3.FileSequencePerType);
			AssertEquals("(File 3) FilePathSuffix - After adding to collection", "_Log2.ldf", dbFile3.FilePathSuffix);

			DbFileInfo dbFile4 = new DbFileInfo("", "", "L");
			AssertEquals("(File 4) FileSequencePerType - Before adding to collection", 0, dbFile4.FileSequencePerType);
			collection.Add(dbFile4);
			AssertEquals("Should now have two elements after add", 4, collection.Count);
			AssertEquals("(File 4) FileSequencePerType - After adding to collection", 3, dbFile4.FileSequencePerType);
			AssertEquals("(File 4) FilePathSuffix - After adding to collection", "_Log3.ldf", dbFile4.FilePathSuffix);

			DbFileInfo dbFile5 = new DbFileInfo("", "", "D");
			AssertEquals("(File 5) FileSequencePerType - Before adding to collection", 0, dbFile5.FileSequencePerType);
			collection.Add(dbFile5);
			AssertEquals("Should now have one element after add", 5, collection.Count);
			AssertEquals("(File 5) FileSequencePerType - After adding to collection", 2, dbFile5.FileSequencePerType);
			AssertEquals("(File 5) FilePathSuffix - After adding to collection", "_Data2.mdf", dbFile5.FilePathSuffix);
		}

		public void TestAddTwiceThrowsException()
		{
			DbFileInfoCollection collection = new DbFileInfoCollection();

			DbFileInfo dbFile1 = new DbFileInfo("", "", "D");
			collection.Add(dbFile1);

			try
			{
				collection.Add(dbFile1);
				Fail("An exception should have been thrown");
			}
			catch (InvalidOperationException e)
			{
				AssertEquals("Wrong exception thrown", "Attempt to add an existing element to the collection.", e.Message);
			}
		}

		public void TestGetDbFileInfoCollectionFromDb()
		{
			string sqlText = String.Format(@"
				SELECT
					s.name AS [LogicalName],
					s.physical_name AS [PhysicalName],
					CASE s.type WHEN 1 THEN 'L' ELSE 'D' END AS [Type]
				FROM sys.master_files AS s
				WHERE s.database_id = db_id('{0}')
				", Db.DatabaseName);

			using (var adminConnection = Db.NewAdminConnection())
			{
				var table = DataUtils.GetDataTableFromQuery(adminConnection, sqlText);

				var fileCollection = DbFileInfoCollection.GetDbFileInfoCollectionFromDb(Db.ServerName, Db.DatabaseName);
				AssertEquals("Sould be same count of files", table.Rows.Count, fileCollection.Count);

				for (int i = 0; i < fileCollection.Count; i++)
				{
					AssertEquals(string.Format("File {0} LogicalName", i), fileCollection[i].LogicalName, table.Rows[i]["LogicalName"].ToString());
					Assert(string.Format("File {0} PhysicalName", i), table.Rows[i]["PhysicalName"].ToString().StartsWith(fileCollection[i].FolderPath));
					AssertEquals(string.Format("File {0} Type", i), fileCollection[i].FileType, table.Rows[i]["Type"].ToString());
				}
			}
		}
	}
}
