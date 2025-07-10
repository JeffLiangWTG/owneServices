using System;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class DbFileInfoTest : TestCase
	{
		public void TestCreateAndGetValues()
		{
			DbFileInfo dbFile1 = new DbFileInfo("Db_Data", @"E:\MSSQL\Data\", "D");
			AssertEquals("(File 1) LogicalName", "Db_Data", dbFile1.LogicalName);
			AssertEquals("(File 1) FolderPath", @"E:\MSSQL\Data\", dbFile1.FolderPath);
			AssertEquals("(File 1) Type", "D", dbFile1.FileType);
			AssertEquals("(File 1) DbType", DbFileInfo.DbTypeMain, dbFile1.DbType);
			AssertEquals("(File 1) FileSequencePerType", 0, dbFile1.FileSequencePerType);

			DbFileInfo dbFile2 = new DbFileInfo("Db_Log", @"E:\MSSQL\Log", "L");
			AssertEquals("(File 2) LogicalName", "Db_Log", dbFile2.LogicalName);
			AssertEquals("(File 2) FolderPath", @"E:\MSSQL\Log", dbFile2.FolderPath);
			AssertEquals("(File 2) Type", "L", dbFile2.FileType);
			AssertEquals("(File 2) DbType", DbFileInfo.DbTypeMain, dbFile2.DbType);
			AssertEquals("(File 2) FileSequencePerType", 0, dbFile2.FileSequencePerType);
		}

		public void TestValidateFileTypeAndDbType()
		{
			DbFileInfo dbFile = new DbFileInfo("", "", "D");
			dbFile = new DbFileInfo("", "", "L");

			try
			{
				dbFile = new DbFileInfo("", "", "Q");
				Fail("An exception should have been thrown");
			}
			catch (ArgumentException e)
			{
				AssertEquals("Wrong exception thrown", "Wrong file type (Q). It must be (D) data or (L) log.\r\nParameter name: fileType", e.Message);
			}

			try
			{
				dbFile = new DbFileInfo("", "", "D", "", "Unknow DB");
				Fail("An exception should have been thrown");
			}
			catch (ArgumentException e)
			{
				AssertEquals("Wrong exception thrown", String.Format("Wrong db type ({0}). It must be ({1}), ({2}), ({3}), ({4}), ({5}), or ({6}).\r\nParameter name: dbType",
					"Unknow DB", DbFileInfo.DbTypeMain, DbFileInfo.DbTypeEDocs, DbFileInfo.DbTypeRefDB, DbFileInfo.DbTypeUserRepository, DbFileInfo.DbTypeAuditDB, DbFileInfo.DbTypeEdwDB), e.Message);
			}
		}
	}
}
