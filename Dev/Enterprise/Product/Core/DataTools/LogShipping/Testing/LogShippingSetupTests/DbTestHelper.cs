#region Test
#if DEBUG

using System;
using System.IO;
using CargoWise.Data;
using Enterprise.LogShipping.Setup;
using NUnit.Framework;

namespace Enterprise.LogShipping.Testing
{
	class DbTestHelper : IDisposable
	{
		public string GetDatabasePhysicalPath(string databaseName)
		{
			string sqlText = string.Format(@"SELECT TOP 1 physical_name FROM sys.master_files where database_id = db_id('{0}')", databaseName);
			string result = (string)TestConnection.ExecuteScalar(sqlText);
			return Path.GetDirectoryName(result);
		}

		public string GetDatabasePhysicalFullPath(string databaseName, string fileType)
		{
			string sqlText = string.Format(@"SELECT TOP 1 physical_name FROM sys.master_files where database_id = db_id('{0}') and type = {1}", databaseName, fileType == BackupFileInfo.DataFileType ? 0 : 1);
			string result = (string)TestConnection.ExecuteScalar(sqlText);
			return result;
		}

		public void CreateDatabase(string database)
		{
			TestConnection.ExecuteNonQuery(string.Format("CREATE DATABASE [{0}]", database));
		}

		public void DropDatabase(string database)
		{
			TestConnection.ExecuteNonQuery(string.Format("IF DB_ID (N'{0}') IS NOT NULL DROP DATABASE [{0}]", database));
		}

		public DbConnection TestConnection
		{
			get { return testConnection ?? (testConnection = Db.NewAdminConnection()); }
		}
		DbConnection testConnection;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		public string GetTestBackupFile(SqlServerVersionNumber sqlVersion)
		{
			string backupFileName = GetTestBackupFileName(sqlVersion);
			return Path.Combine(Path.Combine(TestCase.BaseSourcePath, TestBackupFilePath), backupFileName);
		}

		public static string GetTestBackupFileName(SqlServerVersionNumber sqlVersion)
		{
			switch (sqlVersion.CompatibilityLevel)
			{
				case 160:
					return "TestDatabase2022.bak";
				case 150:
					return "TestDatabase2019.bak";
				case 140:
					return "TestDatabase2017.bak";
				case 130:
					return "TestDatabase2016.bak";
				default:
					throw new Exception("There are no backup files for SQL Server version " + sqlVersion.ToString());
			}
		}

		public const string TestBackupFilePath = @"Enterprise\Product\Core\DataTools\LogShipping\Testing\LogShippingSetupTests\TestFiles";
		public const string TestDatabaseDataFileName = "TestDatabase_DDBC83B3-4010-480d-9C5D-54E3F584F9B0_Data.mdf";
		public const string TestDatabaseLogFileName = "TestDatabase_DDBC83B3-4010-480d-9C5D-54E3F584F9B0_Log.ldf";

		public void Dispose()
		{
			if (testConnection != null)
			{
				((IDisposable)testConnection).Dispose();
			}
		}
	}

	static class DirectoryHelper
	{
		public static void RetryDeletingTempDirectory(string tempDirPath)
		{
			if (tempDirPath != null && Directory.Exists(tempDirPath))
			{
				int i = 3;
				do
				{
					try
					{
						Directory.Delete(tempDirPath, recursive: true);
						i = 0;
					}
#pragma warning disable ENT0001
					catch // Testing
#pragma warning restore ENT0001
					{
						System.Threading.Thread.Sleep(1000);
						i--;
					}
				} while (i > 0);
			}
		}
	}
}

#endif
#endregion
