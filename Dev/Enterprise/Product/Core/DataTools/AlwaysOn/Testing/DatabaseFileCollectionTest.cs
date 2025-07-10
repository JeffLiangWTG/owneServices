using Enterprise.AlwaysOn.Setup;
using NUnit.Framework;

namespace Enterprise.AlwaysOn.Testing
{
	class DatabaseFileCollectionTest : TestCase
	{
		public void TestAddCountAndClear()
		{
			var dbFiles = new DatabaseFileCollection();
			AssertEquals("Count", 0, dbFiles.Count);
			dbFiles.Add(new DatabaseFile("DataFile01", "D:\\Physical\\Data.mdf", "D"));
			AssertEquals("Count", 1, dbFiles.Count);
			dbFiles.Add(new DatabaseFile("LogFile01", "D:\\Physical\\Log.ldf", "L"));
			AssertEquals("Count", 2, dbFiles.Count);
			dbFiles.Clear();
			AssertEquals("Count", 0, dbFiles.Count);
		}

		public void TestEnumerator()
		{
			var dbFiles = new DatabaseFileCollection();
			dbFiles.Add(new DatabaseFile("DataFile01", "D:\\Physical\\Data01.mdf", "D"));
			dbFiles.Add(new DatabaseFile("DataFile02", "D:\\Physical\\Data02.mdf", "D"));
			dbFiles.Add(new DatabaseFile("LogFile01", "D:\\Physical\\Log.ldf", "L"));

			int calculatedCount = 0;

			foreach (var dbFile in dbFiles)
			{
				calculatedCount++;
				AssertEquals("Type", dbFile.LogicalName.StartsWith("Data") ? DatabaseFile.FileType.Data : DatabaseFile.FileType.Log, dbFile.Type);
			}

			AssertEquals("Count", calculatedCount, dbFiles.Count);
			AssertEquals("Count", 3, calculatedCount);
		}

		public void TestCompareTo()
		{
			DatabaseFileCollection dbFiles01 = new DatabaseFileCollection();
			DatabaseFileCollection dbFiles02 = null;
			AssertEquals("dbFiles01 == dbFiles02 ?", false, dbFiles01.CompareTo(dbFiles02) == 0);

			dbFiles02 = new DatabaseFileCollection();
			AssertEquals("dbFiles01 == dbFiles02 ?", true, dbFiles01.CompareTo(dbFiles02) == 0);

			dbFiles01.Add(new DatabaseFile("DataFile01", "D:\\Folder\\Data01.mdf", "D"));
			AssertEquals("dbFiles01 == dbFiles02 ?", false, dbFiles01.CompareTo(dbFiles02) == 0);

			dbFiles02.Add(new DatabaseFile("DataFile01", "D:\\Folder\\Data01.mdf", "D"));
			AssertEquals("dbFiles01 == dbFiles02 ?", true, dbFiles01.CompareTo(dbFiles02) == 0);

			dbFiles02.Add(new DatabaseFile("LogFile01", "D:\\Folder\\Log01.ldf", "L"));
			AssertEquals("dbFiles01 == dbFiles02 ?", false, dbFiles01.CompareTo(dbFiles02) == 0);

			dbFiles01.Add(new DatabaseFile("LogFile01", "D:\\AnotherFolder\\Log01.ldf", "L"));
			AssertEquals("dbFiles01 == dbFiles02 ?", false, dbFiles01.CompareTo(dbFiles02) == 0);
		}
	}
}
