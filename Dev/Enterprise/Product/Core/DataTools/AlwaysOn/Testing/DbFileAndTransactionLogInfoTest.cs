using Enterprise.AlwaysOn.Setup;
using NUnit.Framework;

namespace Enterprise.AlwaysOn.Testing
{
	class DbFileAndTransactionLogInfoTest : TestCase
	{
		public void TestAttributes()
		{
			var dbFiles = new DatabaseFileCollection();
			dbFiles.Add(new DatabaseFile("DataFile", "D:\\Folder\\Data.mdf", "D"));
			dbFiles.Add(new DatabaseFile("LogFile", "L:\\Folder\\Log.ldf", "L"));

			var dbFileInfo = new DbFileAndTransactionLogInfo("SomeDb", 45000000042400001m, dbFiles);
			AssertEquals("DbName", "SomeDb", dbFileInfo.DbName);
			AssertEquals("LogSequenceNumber", 45000000042400001m, dbFileInfo.LogSequenceNumber);
			AssertEquals("File count", 2, dbFileInfo.Files.Count);
		}
	}
}
