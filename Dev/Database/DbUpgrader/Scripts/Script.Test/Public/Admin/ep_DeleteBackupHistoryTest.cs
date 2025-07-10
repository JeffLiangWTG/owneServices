using System;
using System.Data;
using System.Threading;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Admin;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Admin
{
	[TestedType(typeof(ep_DeleteBackupHistory))]
	class ep_DeleteBackupHistoryTest : DbCreateScriptTest
	{
		public void TestDeleteBackupHistory()
		{
			using var connection = Db.NewAdminConnection();

			// removing all exising history
			DeleteBackupHistoryByDateTime(DateTime.Today.AddDays(1));

			// there is no record in msdb.dbo.backupset
			int count = (int)connection.ExecuteScalar(string.Format(@"
				SELECT COUNT(*) FROM msdb.dbo.backupset
				WHERE database_name = '{0}'", tempDbName));
			AssertEquals(count, 0);

			DoOneBackup();
			DoOneBackup();
			Thread.Sleep(1000);
			DoOneBackup();

			// after 3 backups, there are 3 history records in msdb.dbo.backupset
			count = (int)connection.ExecuteScalar(string.Format(@"
				SELECT COUNT(*) FROM msdb.dbo.backupset
				WHERE database_name = '{0}'", tempDbName));
			AssertEquals(count, 3);

			DateTime lastBackupDateTime = (DateTime)connection.ExecuteScalar(string.Format(@"
				SELECT MAX(backup_finish_date) FROM msdb.dbo.backupset
				WHERE database_name = '{0}'", tempDbName));

			DeleteBackupHistoryByDateTime(lastBackupDateTime);

			// after removing 2 records, there is only one record in msdb.dbo.backupset
			count = (int)connection.ExecuteScalar(string.Format(@"
				SELECT COUNT(*) FROM msdb.dbo.backupset
				WHERE database_name = '{0}'", tempDbName));
			AssertEquals(count, 1);

			// removing all history
			DeleteBackupHistoryByDateTime(DateTime.Today.AddDays(1));
		}

		#region setup

		protected override void SetUp()
		{
			base.SetUp();
			CreateTestFolderIfNotExist();
			CreateTempDbIfNotExists();
		}

		protected override void TearDown()
		{
			DropTempDb();
			DeleteTestFolderIfExists();
			base.TearDown();
		}

		void CreateTestFolderIfNotExist()
		{
			bool folderExists = System.IO.Directory.Exists(tempFolderName);

			if (!folderExists)
			{
				System.IO.Directory.CreateDirectory(tempFolderName);
			}

			folderExists = System.IO.Directory.Exists(tempFolderName + "db\\");

			if (!folderExists)
			{
				System.IO.Directory.CreateDirectory(tempFolderName + "db\\");
			}
		}

		void CreateTempDbIfNotExists()
		{
			string createDbCommand = string.Format(@"
				IF NOT EXISTS (SELECT null FROM sys.databases WHERE name = '{0}')
					CREATE DATABASE [{0}]
						ON (NAME = {0}_Data, FILENAME = '{1}db\\{0}_Data.mdf')
						LOG ON (NAME = {0}_Log, FILENAME = '{1}db\\{0}_Log.ldf')",
				tempDbName, tempFolderName);

			using (var connection = Db.NewAdminConnection())
			{
				using (DbCommand createCmd = connection.Command(createDbCommand))
				{
					createCmd.CommandTimeout = 0; // No timeout
					createCmd.CommandType = CommandType.Text;
					createCmd.ExecuteNonQuery();
				}
			}

			using (var connection = Db.NewAdminConnection(tempDbName))
			{
				using (DbCommand addProcedure = connection.Command(ScriptToTest.Text))
				{
					addProcedure.CommandTimeout = 0; // No timeout
					addProcedure.CommandType = CommandType.Text;
					addProcedure.ExecuteNonQuery();
				}
			}
		}

		void DeleteBackupHistoryByDateTime(DateTime oldest_date)
		{
			using (var connection = Db.NewAdminConnection(tempDbName))
			{
				try
				{
					using (DbCommand deleteCmd = connection.Command("ep_DeleteBackupHistory"))
					{
						deleteCmd.CommandTimeout = 0; // No timeout
						deleteCmd.CommandType = CommandType.StoredProcedure;
						deleteCmd.AddParameter("@oldest_date", SqlDbType.DateTime, oldest_date);
						deleteCmd.ExecuteNonQuery();
					}
					connection.CloseConnection();
				}
				catch (SqlException ex)
				{
					connection.CloseConnection();
					Assert("Failed to delete backup history of DB " + tempDbName + ": " + ex.Number + " " + ex.Message, false);
				}
			}
		}

		void DoOneBackup()
		{
			try
			{
				using (var connection = Db.NewAdminConnection())
				{
					string backupCommandText = string.Format(@"BACKUP DATABASE [{0}] TO DISK = '{1}' WITH INIT;",
						tempDbName, tempFolderName + tempDbName);
					connection.ExecuteNonQuery(backupCommandText);
				}
			}
			catch (SqlException ex)
			{
				Assert("Failed to backup DB " + tempDbName + ": " + ex.Number + " " + ex.Message, false);
			}
		}

		void DropTempDb()
		{
			using (var connection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(connection, tempDbName);
			}
		}

		void DeleteTestFolderIfExists()
		{
			bool folderExists = System.IO.Directory.Exists(tempFolderName);

			if (folderExists)
			{
				System.IO.Directory.Delete(tempFolderName, true);
			}
		}

		const string tempDbName = "TempDbFor_ep_DeleteBackupHistory_test";
		readonly string tempFolderName = NUnit.Framework.TempForTest.TempPath + "EP_DELETEBACKUPHISTORY_TMP\\";
		#endregion
	}
}

