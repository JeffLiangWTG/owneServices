using System.IO;
using System.Text;
using CargoWise.Data;
using CargoWise.IO;
using Enterprise.LogShipping.Setup;
using NUnit.Framework;

namespace Enterprise.LogShipping.Testing
{
	sealed class RestoreManagerTest : LogShippingTestFixture
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[SnailTest]
		public void TestRestore()
		{
			TempDirectory tempDir = null;

			try
			{
				using (var dbTestHelper = new DbTestHelper())
				{
					var info = new LogShippingInfo();
					info.PrimaryServer = new SqlServerInfoForTesting("Server", string.Empty);
					var databaseInfo = new MainDatabaseInfoForTesting(info, "PrimaryDb");
					databaseInfo.ShouldInitialise = true;
					databaseInfo.SecondaryDatabaseExistValue = false;
					var filePath = dbTestHelper.GetTestBackupFile(Db.Connection.ServerVersionNumber);
					info.BackupSourceDirectory = Path.GetDirectoryName(filePath);
					databaseInfo.BackupFileName = Path.GetFileName(filePath);

					info.SecondaryServer = new SqlServerInfoForTesting(dbTestHelper.TestConnection.ServerNameWithoutInstance, dbTestHelper.TestConnection.ServerInstanceName);
					info.MainDatabase = databaseInfo;

					tempDir = new TempDirectory();
					info.BackupLocalCopyDirectory = tempDir.DirectoryName;
					var manager = new RestoreManager();
					manager.OnShowMessage += new NotificationDelegate(Manager_OnShowMessage);

					try
					{
						var restoreResult = manager.Restore(info);
						AssertEquals("Restore Database failed. \r\nOutput text:\r\n" + managerOutput.ToString(), true, restoreResult);

						Assert("Database is in StandBy mode", IsDatabaseInStandByMode(dbTestHelper, databaseInfo.SecondaryDatabaseName));
						var masterDbPath = dbTestHelper.GetDatabasePhysicalPath(Db.SqlMasterDb);
						var secondaryDbDataFullPath = dbTestHelper.GetDatabasePhysicalFullPath(databaseInfo.SecondaryDatabaseName, BackupFileInfo.DataFileType);
						var secondaryDbLogFullPath = dbTestHelper.GetDatabasePhysicalFullPath(databaseInfo.SecondaryDatabaseName, BackupFileInfo.LogFileType);

						AssertEquals("Data file has been moved to master db folder", secondaryDbDataFullPath, Path.Combine(masterDbPath, databaseInfo.SecondaryDatabaseName + "_Data.mdf"));
						AssertEquals("Log file has been moved to master db folder", secondaryDbLogFullPath, Path.Combine(masterDbPath, databaseInfo.SecondaryDatabaseName + "_Log.ldf"));
						Assert("StandBy file exists", Directory.GetFiles(tempDir.DirectoryName, string.Format("{0}*.tuf", databaseInfo.SecondaryDatabaseName)).Length > 0);

						var secondaryDbPath = dbTestHelper.GetDatabasePhysicalPath(databaseInfo.SecondaryDatabaseName);
						secondaryDbDataFullPath = dbTestHelper.GetDatabasePhysicalFullPath(databaseInfo.SecondaryDatabaseName, BackupFileInfo.DataFileType);
						secondaryDbLogFullPath = dbTestHelper.GetDatabasePhysicalFullPath(databaseInfo.SecondaryDatabaseName, BackupFileInfo.LogFileType);
						restoreResult = manager.Restore(info);

						AssertEquals("Replacing database should be OK, but failed.\r\nOutput text:\r\n" + managerOutput.ToString(), true, restoreResult);
						AssertEquals("Data file has been moved to secondary db folder", secondaryDbDataFullPath, Path.Combine(secondaryDbPath, databaseInfo.SecondaryDatabaseName + "_Data.mdf"));
						AssertEquals("Log file has been moved to secondary db folder", secondaryDbLogFullPath, Path.Combine(secondaryDbPath, databaseInfo.SecondaryDatabaseName + "_Log.ldf"));
					}
					finally
					{
						dbTestHelper.DropDatabase(databaseInfo.SecondaryDatabaseName);
					}
				}
			}
			finally
			{
				if (tempDir != null)
				{
					var tempDirPath = tempDir.DirectoryName;
					try
					{ tempDir.Dispose(); }
					catch { DirectoryHelper.RetryDeletingTempDirectory(tempDirPath); }
				}
			}
		}

		bool IsDatabaseInStandByMode(DbTestHelper dbTestHelper, string dbName)
		{
			var sqlText = string.Format(@"
				SELECT is_in_standby 
				FROM sys.databases
				WHERE [name] = '{0}'", dbName);
			return (bool)dbTestHelper.TestConnection.ExecuteScalar(sqlText);
		}

		void Manager_OnShowMessage(string message)
		{
			managerOutput.AppendLine(message);
		}

		readonly StringBuilder managerOutput = new StringBuilder();
	}
}
