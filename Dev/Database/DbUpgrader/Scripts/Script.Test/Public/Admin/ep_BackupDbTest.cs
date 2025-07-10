using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Admin;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Admin
{
	[TestedType(typeof(ep_BackupDb))]
	class ep_BackupDbTest : DbCreateScriptTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			CreateTestFolderIfNotExist();

			using (var connection = Db.NewAdminConnection())
			{
				CreateTempDbIfNotExists(connection);
			}
		}

		protected override void TearDown()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DropTempDb(connection);
			}

			DeleteTestFolderIfExists();
			base.TearDown();
		}

		public void TestLogBackup_WithCheckSum()
		{
			TestLogBackup(true);
		}

		public void TestLogBackup_WithoutCheckSum()
		{
			TestLogBackup(false);
		}

		public void TestLogBackup(bool shouldHaveCheckSum)
		{
			using (var connection = Db.NewAdminConnection())
			{
				DeleteBackUpFiles();
				CleanHistory(connection, tempDbName);
				var previousCount = CheckSumCount();
				string backupFileName = tempDbName + DateTime.Now.ToString("yyyyMMddhhmmss");
				try
				{
					PerformFullBackUp(connection, tempDbName, enforceFullRecovery: true, useCheckSum: shouldHaveCheckSum);
					PerformLogBackUp(connection, tempDbName, backupFileName);
				}
				catch (SqlException ex)
				{
					Assert("Failed to back DB " + backupFileName + ": " + ex.Number + " " + ex.Message, false);
				}

				Assert("New (log) back-up file does not exist after the back-up process is over.", File.Exists(tempFolderName + backupFileName));
				Assert($"The back up should{(shouldHaveCheckSum ? string.Empty : " *not* ")} have a check sum.", shouldHaveCheckSum.Equals(previousCount != CheckSumCount()));

				DeleteBackUpFiles();
			}
		}

		public int CheckSumCount()
		{
			using (var connection = Db.NewAdminConnection())
			using (var cmd = connection.Command($"select count(*) from msdb..backupSet where has_backup_checksums = 1 and database_name = '{tempDbName}'"))
			{
				return (int)cmd.ExecuteScalar();
			}
		}

		public void TestDuplicatedLogBackUp()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DeleteBackUpFiles();
				CleanHistory(connection, tempDbName);

				string backupFileName = "LogBackupFile";

				try
				{
					PerformFullBackUp(connection, tempDbName, enforceFullRecovery: true);
					PerformLogBackUp(connection, tempDbName, backupFileName);

					var trnFilePath = Path.Combine(tempFolderName, backupFileName);
					Assert("Log backup file should exists.", File.Exists(trnFilePath));
					var lastWriteDate = new FileInfo(trnFilePath).LastWriteTimeUtc;
					Thread.Sleep(TimeSpan.FromSeconds(60));

					try
					{
						PerformLogBackUp(connection, tempDbName, backupFileName);
						Fail("Second backup should fail because previous backup should not be overriden");
					}
					catch (SqlException ex)
					{
						Assert("Second backup should fail with correct error type", new DbErrorMatch(ex).ExceptionType == DbErrorType.MediumOnDeviceHasNotExpiredAndCannotBeOverwritten);
					}
					finally
					{
						Assert("Log backup file should exists.", File.Exists(trnFilePath));
						Assert("Log backup is not overriden", lastWriteDate == new FileInfo(trnFilePath).LastWriteTimeUtc);
					}
				}
				finally
				{
					DeleteBackUpFiles();
					CleanHistory(connection, tempDbName);
				}
			}
		}

		public void TestSimpleRecoveryModelBackUp()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DeleteBackUpFiles();
				CleanHistory(connection, tempDbName);
				SetToSimpleMode(connection, tempDbName);

				string backupFileName = tempDbName + DateTime.Now.ToString("yyyyMMddhhmmss");

				AssertExceptionThrown(
					"Attempt to perform a log backup should fail if DB is set to simple recovery model.",
					typeof(SqlException), "The statement BACKUP LOG is not allowed while the recovery model is SIMPLE",
					() => PerformLogBackUp(connection, tempDbName, backupFileName),
					assertStartsWith: true);

				DeleteBackUpFiles();
			}
		}

		public void TestNewFullBackUp()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DeleteBackUpFiles();

				string backupFileName = tempFolderName + tempDbName;

				try
				{
					PerformFullBackUp(connection, tempDbName);
				}
				catch (SqlException ex)
				{
					Assert("Failed to back up to " + backupFileName + ": " + ex.Number + " " + ex.Message, false);
				}

				Assert("New back-up file does not exist after the back-up process is over.", File.Exists(backupFileName));

				var sqlText = @"RESTORE LABELONLY FROM DISK = '" + backupFileName + "'";
				var hasDescription = false;

				using (var conn = Db.NewAdminConnection())
				using (var cmd = conn.Command(sqlText))
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						if (!reader.IsDBNull(7))
						{
							hasDescription = true;
						}
					}
				}

				Assert("the backup file should have a media description in its label", hasDescription);

				DeleteBackUpFiles();
			}
		}

		public void TestDeleteCurrentFile()
		{
			using (var connection = Db.NewAdminConnection())
			{
				DeleteBackUpFiles();

				var backupFileName = tempFolderName + tempDbName;
				var currentFileName = backupFileName + ".CURRENT";

				try
				{
					PerformFullBackUp(connection, tempDbName);
				}
				catch (SqlException ex)
				{
					Assert("First backup: failed to back up to " + backupFileName + ": " + ex.Number + " " + ex.Message, false);
				}

				Assert(string.Format("{0} not deleted.", currentFileName), !File.Exists(currentFileName));
				Assert("New back-up file does not exist after the back-up process is over.", File.Exists(backupFileName));

				try
				{
					PerformFullBackUp(connection, tempDbName);
				}
				catch (SqlException ex)
				{
					Assert("Second backup: failed to back up to " + backupFileName + ": " + ex.Number + " " + ex.Message, false);
				}

				Assert(string.Format("{0} not deleted.", currentFileName), !File.Exists(currentFileName));
				Assert("New back-up file does not exist after the back-up process is over.", File.Exists(backupFileName));

				DropTempDb(connection);

				try
				{
					PerformFullBackUp(connection, tempDbName);
				}
				catch (SqlException)
				{
					Assert(string.Format("{0} not deleted.", currentFileName), !File.Exists(currentFileName));
				}

				Assert("Old back-up file does not exist after the back-up process failed.", File.Exists(backupFileName));

				DeleteBackUpFiles();
			}
		}

		public void TestOverridingFullBackUp()
		{
			string backupFileName = tempFolderName + tempDbName;
			string oldBackupFileName = backupFileName + "_Backup.old";

			DeleteBackUpFiles();
			CreateDummyBackUpFile(backupFileName);

			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					PerformFullBackUp(connection, tempDbName);
				}
				catch (SqlException ex)
				{
					Assert("Failed to back up to " + backupFileName + ": " + ex.Number + " " + ex.Message, false);
				}
			}

			Assert("New back-up file does not exist after the back-up process is over.", File.Exists(backupFileName));
			Assert("Old back-up file exists after the back-up process is over.", !File.Exists(oldBackupFileName));

			DeleteBackUpFiles();
		}

		public void TestFullBackupOnNotExistingDB()
		{
			DeleteBackUpFiles();
			const string nonExsistingDbName = "NonExistingDB";
			var backupFileName = tempFolderName + nonExsistingDbName;

			CreateDummyBackUpFile(backupFileName);

			File.Exists(backupFileName);
			var currentFileMD5 = GetMD5SumForFile(backupFileName);

			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					PerformFullBackUp(connection, nonExsistingDbName);
					Assert("Backup should fail on a non-existing database.", false);
				}
				catch (SqlException)
				{
				}
			}

			var backUpFileMD5 = GetMD5SumForFile(backupFileName);
			Assert("A back-up process for a non-existing database (" + nonExsistingDbName + ") changed the existing back-up file (" + backupFileName + ").", currentFileMD5.Equals(backUpFileMD5));

			DeleteBackUpFiles();
		}

		void CreateTestFolderIfNotExist()
		{
			var folderExists = Directory.Exists(tempFolderName);

			if (!folderExists)
			{
				Directory.CreateDirectory(tempFolderName);
			}
			folderExists = Directory.Exists(tempFolderName + "db\\");

			if (!folderExists)
			{
				Directory.CreateDirectory(tempFolderName + "db\\");
			}
		}

		void DeleteTestFolderIfExists()
		{
			var folderExists = Directory.Exists(tempFolderName);

			if (folderExists)
			{
				Directory.Delete(tempFolderName, true);
			}
		}

		void CreateDummyBackUpFile(string fileName)
		{
			if (!File.Exists(fileName))
			{
				var sw = File.CreateText(fileName);
				sw.WriteLine("dummy backup data");
				sw.Close();
			}
		}

		void DeleteBackUpFiles()
		{
			foreach (var f in new DirectoryInfo(tempFolderName).GetFiles(tempDbName + "*", SearchOption.TopDirectoryOnly))
			{
				f.Delete();
			}
		}

		void PerformLogBackUp(AdminConnection connection, string dbName, string fileName)
		{
			var time = (DateTime)connection.ExecuteScalar("SELECT getdate()");
			var backupStamp = time.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
			var serverName = connection.ServerNameReportedByDatabase.Trim().Replace('\\', '-');

			try
			{
				using (var backupCmd = connection.Command("ep_BackupDb"))
				{
					backupCmd.CommandTimeout = 0; // No timeout
					backupCmd.CommandType = CommandType.StoredProcedure;
					backupCmd.AddParameter("@DbName", SqlDbType.VarChar, dbName);
					backupCmd.AddParameter("@FolderPath", SqlDbType.VarChar, tempFolderName);
					backupCmd.AddParameter("@FileName", SqlDbType.VarChar, fileName);
					backupCmd.AddParameter("@BkpType", SqlDbType.VarChar, "LOG");
					backupCmd.AddParameter("@BackupDescription", SqlDbType.NVarChar, 255, "T-" + backupStamp + "_S-" + serverName);
					backupCmd.AddParameter("@IsCompressed", SqlDbType.Bit, true);
					backupCmd.ExecuteNonQuery();
				}
				connection.CloseConnection();
			}
			catch (SqlException)
			{
				connection.CloseConnection();
				throw;
			}
		}

		void PerformFullBackUp(AdminConnection connection, string dbName, bool enforceFullRecovery = false, bool useCheckSum = false)
		{
			if (enforceFullRecovery)
			{
				connection.ExecuteNonQuery($"ALTER DATABASE [{tempDbName}] SET RECOVERY FULL");
			}

			var time = (DateTime)connection.ExecuteScalar("SELECT getdate()");
			var backupStamp = time.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
			var serverName = connection.ServerNameReportedByDatabase.Trim().Replace('\\', '-');

			using (var backupCmd = connection.Command("ep_BackupDb"))
			{
				backupCmd.CommandTimeout = 0; // No timeout
				backupCmd.CommandType = CommandType.StoredProcedure;
				backupCmd.AddParameter("@DbName", SqlDbType.VarChar, dbName);
				backupCmd.AddParameter("@FolderPath", SqlDbType.VarChar, tempFolderName);
				backupCmd.AddParameter("@FileName", SqlDbType.VarChar, tempDbName);
				backupCmd.AddParameter("@BkpType", SqlDbType.VarChar, "FULL");
				backupCmd.AddParameter("@BackupDescription", SqlDbType.NVarChar, 255, "T-" + backupStamp + "_S-" + serverName);
				backupCmd.AddParameter("@IsCompressed", SqlDbType.Bit, true);
				backupCmd.AddParameter("@UseCheckSum", SqlDbType.Bit, useCheckSum);
				backupCmd.ExecuteNonQuery();
			}
		}

		void SetToSimpleMode(AdminConnection connection, string dbName)
		{
			AdoTestUtils.SetDbRecoveryModelForTest(connection, dbName, DbRecoveryModel.Simple);
		}

		void CreateTempDbIfNotExists(AdminConnection connection)
		{
			var sqlCommand = string.Format(@"
				IF NOT EXISTS (SELECT null FROM sys.databases WHERE name = '{0}')
					CREATE DATABASE [{0}]
						ON (NAME = {0}_Data, FILENAME = '{1}db\\{0}_Data.mdf')
						LOG ON (NAME = {0}_Log, FILENAME = '{1}db\\{0}_Log.ldf')",
				tempDbName, tempFolderName);

			using (DbCommand createCmd = connection.Command(sqlCommand))
			{
				createCmd.CommandTimeout = 0; // No timeout
				createCmd.CommandType = CommandType.Text;
				createCmd.ExecuteNonQuery();
			}
		}

		void DropTempDb(AdminConnection connection)
		{
			AdoTestUtils.DropDbIfExists(connection, tempDbName);
		}

		string GetMD5SumForFile(string fileName)
		{
			if (!File.Exists(fileName))
			{
				return "";
			}

			using (var md5 = MD5.Create())
			{
				using (var stream = File.OpenRead(fileName))
				{
					return BitConverter.ToString(md5.ComputeHash(stream));
				}
			}
		}

		void CleanHistory(AdminConnection connection, string dBName)
		{
			var sqlScript = $@"
-- Clean History
DECLARE @MediaSetId       TABLE (media_set_id INT);
DECLARE @BackupSetId      TABLE(backup_set_id INT);
DECLARE @RestoreHistoryId TABLE(restore_history_id INT);

INSERT INTO @MediaSetId(media_set_id)
	SELECT DISTINCT media_set_id
	FROM  msdb.dbo.backupmediafamily
	WHERE physical_device_name like '%{dBName}%'

INSERT INTO @BackupSetId(backup_set_id)
	SELECT DISTINCT bs.backup_set_id
	FROM msdb.dbo.backupset bs
	INNER JOIN @MediaSetId msi ON msi.media_set_id = bs.media_set_id

INSERT INTO @RestoreHistoryId(restore_history_id)
	SELECT DISTINCT rh.restore_history_id
	FROM msdb.dbo.restorehistory rh
	INNER JOIN @BackupSetId bsi ON bsi.backup_set_id = rh.backup_set_id

DELETE FROM msdb.dbo.restorefile
WHERE restore_history_id IN(SELECT restore_history_id FROM @RestoreHistoryId)

DELETE FROM msdb.dbo.restorefilegroup
WHERE restore_history_id IN(SELECT restore_history_id FROM @RestoreHistoryId)

DELETE FROM msdb.dbo.restorehistory
WHERE restore_history_id IN(SELECT restore_history_id FROM @RestoreHistoryId)

--Delete Backup History
DELETE FROM msdb.dbo.backupfile
WHERE backup_set_id IN(SELECT backup_set_id FROM @BackupSetId)

DELETE FROM msdb.dbo.backupfilegroup
WHERE backup_set_id IN(SELECT backup_set_id FROM @BackupSetId)

DELETE FROM msdb.dbo.backupset
WHERE backup_set_id IN(SELECT backup_set_id FROM @BackupSetId)

DELETE msdb.dbo.backupmediafamily
WHERE media_set_id IN(SELECT media_set_id FROM @MediaSetId)

DELETE msdb.dbo.backupmediaset
WHERE media_set_id IN(SELECT media_set_id FROM @MediaSetId)";
			RunNonQuery(connection, sqlScript);
			DbCommitTracker.Ignore("-- Clean History");
		}
		internal static void RunNonQuery(AdminConnection connection, string script)
		{
			Regex r = new Regex(@"^(\s|\t)*go(\s\t)?.*", RegexOptions.Multiline | RegexOptions.IgnoreCase);

			foreach (string statement in r.Split(script))
			{
				//Skip empty statements, in case of a GO and trailing blanks or something
				string thisStatement = statement.Trim();
				if (string.IsNullOrEmpty(thisStatement))
				{
					continue;
				}

				using (DbCommand createCmd = connection.Command(thisStatement))
				{
					createCmd.CommandTimeout = 0; // No timeout
					createCmd.CommandType = CommandType.Text;
					createCmd.ExecuteNonQuery();
				}
			}
		}

		const string tempDbName = "TempDbFor_ep_BackupDb_test";
		readonly string tempFolderName = NUnit.Framework.TempForTest.TempPath + "EP_BACKUP_TMP\\";
	}
}

