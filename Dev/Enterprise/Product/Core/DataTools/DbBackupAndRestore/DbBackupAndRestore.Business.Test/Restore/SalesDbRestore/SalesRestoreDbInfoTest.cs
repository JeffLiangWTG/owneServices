using System;
using System.IO;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class SalesRestoreDbInfoTest : TestCase
	{
		public void TestGetRestoreDbInfo()
		{
			SalesRestoreDbInfo testSalesRestoreDbInfo = GetRestoreDbInfoFromEdiLoadInfoFile(Db.ServerName, string.Empty, Db.DatabaseName);

			AssertEquals("ServerName", Db.ServerName, testSalesRestoreDbInfo.ServerName);
			AssertEquals("DatabaseName", Db.DatabaseName, testSalesRestoreDbInfo.DatabaseName);
			Assert("ServerExists", testSalesRestoreDbInfo.ServerExists);
			Assert("DatabaseExists", testSalesRestoreDbInfo.DatabaseExists);
			AssertEquals("DBFile count", Db.Connection.GetDbFiles(Db.DatabaseName).Length, testSalesRestoreDbInfo.DbFiles.Count);
		}

		public void TestGetRestoreDbInfoWhenDbDoesNotExist()
		{
			string testDbName = "SalesDbTest" + Guid.NewGuid().ToString();
			SalesRestoreDbInfo testSalesRestoreDbInfo = GetRestoreDbInfoFromEdiLoadInfoFile(Db.ServerName, string.Empty, testDbName);

			AssertEquals("ServerName", Db.ServerName, testSalesRestoreDbInfo.ServerName);
			AssertEquals("DatabaseName", testDbName, testSalesRestoreDbInfo.DatabaseName);
			Assert("ServerExists", testSalesRestoreDbInfo.ServerExists);
			Assert("DatabaseExists", !testSalesRestoreDbInfo.DatabaseExists);
			AssertEquals("DBFiles", 0, testSalesRestoreDbInfo.DbFiles.Count);
		}

		public void TestGetRestoreDbInfoWhenLoadInfoFileIsEmpty()
		{
			SalesRestoreDbInfo testSalesRestoreDbInfo = GetRestoreDbInfoFromEdiLoadInfoFile(string.Empty, string.Empty, string.Empty);

			AssertEquals("ServerName", Db.ServerName, testSalesRestoreDbInfo.ServerName);
			AssertEquals("DatabaseName", Db.DatabaseName, testSalesRestoreDbInfo.DatabaseName);
			Assert("ServerExists", testSalesRestoreDbInfo.ServerExists);
			Assert("DatabaseExists", testSalesRestoreDbInfo.DatabaseExists);
			AssertEquals("DBFile count", Db.Connection.GetDbFiles(Db.DatabaseName).Length, testSalesRestoreDbInfo.DbFiles.Count);
		}

		SalesRestoreDbInfo GetRestoreDbInfoFromEdiLoadInfoFile(string dbServer, string instance, string dbName)
		{
			SalesRestoreDbInfo dbInfo;
			using (TempFile loadInfoFile = TempFile.NewWithExtension("ini"))
			{
				string loadInfo = string.Format("[CONFIGURATION]\r\nSERVER={0}\r\nINSTANCE={1}\r\nDATABASE={2}\r\nOTHERPARAMETERS=\r\nMAINTENANCEMESSAGE=\r\nLOCALDIROVERRIDE=",
						dbServer, instance, dbName);
				File.WriteAllText(loadInfoFile.Filename, loadInfo);
				dbInfo = SalesRestoreDbInfo.GetRestoreDbInfo(loadInfoFile.Filename);
			}
			return dbInfo;
		}
	}
}
