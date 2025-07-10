using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Admin;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Admin
{
	[TestedType(typeof(ep_ShrinkLogFile))]
	class ep_ShrinkLogFileTest : DbCreateScriptTest
	{
		public void TestShrinkLogFileForFullDBs()
		{
			int logFileSizeBefore = 0;
			int logFileSizeNow = 0;
			int logFileSizeBeforeSD1 = 0;
			int logFileSizeBeforeSD2 = 0;
			int logFileSizeBeforeSD3 = 0;
			int logFileSizeBeforeAudit = 0;
			int logFileSizeBeforeEDW = 0;
			int logFileSizeBeforeUserRepository = 0;
			int logFileGrowthSizeBeforeSD1 = 0;
			int logFileGrowthSizeBeforeSD2 = 0;
			int logFileGrowthSizeBeforeSD3 = 0;
			int logFileGrowthSizeBeforeAudit = 0;
			int logFileGrowthSizeBeforeEDW = 0;
			int logFileGrowthSizeBeforeUserRepository = 0;
			int logFileSizeNowSD1 = 0;
			int logFileSizeNowSD2 = 0;
			int logFileSizeNowSD3 = 0;
			int logFileSizeNowAudit = 0;
			int logFileSizeNowEDW = 0;
			int logFileSizeNowUserRepository = 0;
			int logFileGrowthSizeNowSD1 = 0;
			int logFileGrowthSizeNowSD2 = 0;
			int logFileGrowthSizeNowSD3 = 0;
			int logFileGrowthSizeNowAudit = 0;
			int logFileGrowthSizeNowEDW = 0;
			int logFileGrowthSizeNowUserRepository = 0;
			int dataFileSize = 0;
			int logFileGrowthSize = 0;
			string message;

			SetToWriteMode(tempDbName);
			SetToWriteMode(tempDbSD001Name);
			SetToWriteMode(tempDbSD002Name);
			SetToWriteMode(tempDbSD003Name);
			SetToWriteMode(tempDbUserRepositoryName);
			SetToWriteMode(tempDbAuditName);
			SetToWriteMode(tempDbEDWName);

			//create backup to prevent no log backup issue.
			DoFullBackup();
			DoLogBackup();

			//shrink to initial state
			ShrinkLogFileTest();

			//increase log file size
			for (int i = 1; i <= 100; i++)
			{
				DoEnlargeLogFileToCreateVlfs(tempDbName);
				DoEnlargeLogFileToCreateVlfs(tempDbSD001Name);
				DoEnlargeLogFileToCreateVlfs(tempDbSD002Name);
				DoEnlargeLogFileToCreateVlfs(tempDbSD003Name);
				DoEnlargeLogFileToCreateVlfs(tempDbUserRepositoryName);
				DoEnlargeLogFileToCreateVlfs(tempDbAuditName);
				DoEnlargeLogFileToCreateVlfs(tempDbEDWName);
			}

			logFileSizeBefore = GetFileSizeInMb(tempDbName, false);
			logFileSizeBeforeSD1 = GetFileSizeInMb(tempDbSD001Name, false);
			logFileSizeBeforeSD2 = GetFileSizeInMb(tempDbSD002Name, false);
			logFileSizeBeforeSD3 = GetFileSizeInMb(tempDbSD003Name, false);
			logFileSizeBeforeAudit = GetFileSizeInMb(tempDbAuditName, false);
			logFileSizeBeforeEDW = GetFileSizeInMb(tempDbEDWName, false);
			logFileSizeBeforeUserRepository = GetFileSizeInMb(tempDbUserRepositoryName, false);

			logFileGrowthSizeBeforeSD1 = GetFileGrowthSize(tempDbSD001Name, false);
			logFileGrowthSizeBeforeSD2 = GetFileGrowthSize(tempDbSD002Name, false);
			logFileGrowthSizeBeforeSD3 = GetFileGrowthSize(tempDbSD003Name, false);
			logFileGrowthSizeBeforeAudit = GetFileGrowthSize(tempDbAuditName, false);
			logFileGrowthSizeBeforeEDW = GetFileGrowthSize(tempDbEDWName, false);
			logFileGrowthSizeBeforeUserRepository = GetFileGrowthSize(tempDbUserRepositoryName, false);

			message = ShrinkLogFileTest();
			var messages = message.Split(new char[] { '|' }).Select(x => x.Trim());
			Assert(!message.Contains("Operation failed"));

			CombineAssertions(() =>
			{
				AssertCollectionContains(tempDbName, messages);
				AssertCollectionContains(tempDbSD001Name, messages);
				AssertCollectionContains(tempDbSD002Name, messages);
				AssertCollectionContains(tempDbSD003Name, messages);
				AssertCollectionContains(tempDbAuditName, messages);
				AssertCollectionContains(tempDbEDWName, messages);
				AssertCollectionContains(tempDbUserRepositoryName, messages);
			});

			dataFileSize = GetFileSizeInMb(tempDbName, true);
			logFileGrowthSize = GetFileGrowthSize(tempDbName, false);

			if (dataFileSize > 51200)
			{
				Assert(@"If data file size>= 51200 MB, log file growth should be 4096MB", logFileGrowthSize == 4096);
			}
			else if (dataFileSize >= 10240)
			{
				Assert(@"If data file size >= 10240 and < 51200 MB, log file growth should be 1024MB", logFileGrowthSize == 1024);
			}
			else
			{
				Assert(@"If data file size < 10240 MB, log file growth should be 500MB", logFileGrowthSize == 500);
			}

			logFileSizeNow = GetFileSizeInMb(tempDbName, false);
			logFileSizeNowSD1 = GetFileSizeInMb(tempDbSD001Name, false);
			logFileSizeNowSD2 = GetFileSizeInMb(tempDbSD002Name, false);
			logFileSizeNowSD3 = GetFileSizeInMb(tempDbSD003Name, false);
			logFileSizeNowAudit = GetFileSizeInMb(tempDbAuditName, false);
			logFileSizeNowEDW = GetFileSizeInMb(tempDbEDWName, false);
			logFileSizeNowUserRepository = GetFileSizeInMb(tempDbUserRepositoryName, false);

			logFileGrowthSizeNowSD1 = GetFileGrowthSize(tempDbSD001Name, false);
			logFileGrowthSizeNowSD2 = GetFileGrowthSize(tempDbSD002Name, false);
			logFileGrowthSizeNowSD3 = GetFileGrowthSize(tempDbSD003Name, false);
			logFileGrowthSizeNowAudit = GetFileGrowthSize(tempDbAuditName, false);
			logFileGrowthSizeNowEDW = GetFileGrowthSize(tempDbEDWName, false);
			logFileGrowthSizeNowUserRepository = GetFileGrowthSize(tempDbUserRepositoryName, false);

			CombineAssertions(() =>
			{
				Assert(string.Format(CultureInfo.InvariantCulture, "Compare log file size between before shrink: {0}MB and after: {1}MB.", logFileSizeBefore, logFileSizeNow), logFileSizeNow <= logFileSizeBefore); //unknown active/open transactions sometimes prevent shrinking 
				Assert(string.Format(CultureInfo.InvariantCulture, "Compare eDocs log file size between before shrink: {0} MB and after {1}MB.", logFileSizeBeforeSD1, logFileSizeNowSD1), logFileSizeNowSD1 <= logFileSizeBeforeSD1);
				Assert(string.Format(CultureInfo.InvariantCulture, "Compare eDocs log file size between before shrink: {0} MB and after {1}MB.", logFileSizeBeforeSD2, logFileSizeNowSD2), logFileSizeNowSD2 <= logFileSizeBeforeSD2);
				Assert(string.Format(CultureInfo.InvariantCulture, "Compare eDocs log file size between before shrink: {0} MB and after {1}MB.", logFileSizeBeforeSD3, logFileSizeNowSD3), logFileSizeNowSD3 <= logFileSizeBeforeSD3);
				Assert(string.Format(CultureInfo.InvariantCulture, "Compare Audit log file size between before shrink: {0} MB and after {1}MB.", logFileSizeBeforeAudit, logFileSizeNowAudit), logFileSizeNowAudit <= logFileSizeBeforeAudit);
				Assert(string.Format(CultureInfo.InvariantCulture, "Compare EDW log file size between before shrink: {0} MB and after {1}MB.", logFileSizeBeforeEDW, logFileSizeNowEDW), logFileSizeNowEDW <= logFileSizeBeforeEDW);
				Assert(string.Format(CultureInfo.InvariantCulture, "Compare UserRepository log file size between before shrink: {0} MB and after {1}MB.", logFileSizeBeforeUserRepository, logFileSizeNowUserRepository), logFileSizeNowUserRepository <= logFileSizeBeforeUserRepository);

				int expectedBiLogFileGrowthSize = 1024;
				if (logFileGrowthSize * 0.1 < 64)
				{
					expectedBiLogFileGrowthSize = 64;
				}
				else if (logFileGrowthSize * 0.1 < 1024)
				{
					expectedBiLogFileGrowthSize = (int)(logFileGrowthSize * 0.1);
				}

				Assert(@"First eDocs database log growth size should be 10MB", logFileGrowthSizeNowSD1 == 10);
				Assert(@"Second eDocs database log growth size should be 10MB", logFileGrowthSizeNowSD2 == 10);
				Assert(@"Last eDocs database log growth size should be 500MB", logFileGrowthSizeNowSD3 == 500);
				AssertEquals(@"Audit database log growth size", expectedBiLogFileGrowthSize, logFileGrowthSizeNowAudit);
				AssertEquals(@"EDW database log growth size", expectedBiLogFileGrowthSize, logFileGrowthSizeNowEDW);
				Assert(@"UserRepository database log growth size should be 500MB", logFileGrowthSizeNowUserRepository == 500);
			});
		}

		public void TestShrinkLogFileForSimpleDBs()
		{
			int logFileSizeBefore = 0;
			int logFileSizeNow = 0;
			int logFileSizeBeforeSD1 = 0;
			int logFileSizeBeforeSD2 = 0;
			int logFileSizeBeforeSD3 = 0;
			int logFileSizeBeforeAudit = 0;
			int logFileSizeBeforeEDW = 0;
			int logFileSizeBeforeUserRepository = 0;
			int logFileGrowthSizeBeforeSD1 = 0;
			int logFileGrowthSizeBeforeSD2 = 0;
			int logFileGrowthSizeBeforeSD3 = 0;
			int logFileGrowthSizeBeforeAudit = 0;
			int logFileGrowthSizeBeforeEDW = 0;
			int logFileGrowthSizeBeforeUserRepository = 0;
			int logFileSizeNowSD1 = 0;
			int logFileSizeNowSD2 = 0;
			int logFileSizeNowSD3 = 0;
			int logFileSizeNowAudit = 0;
			int logFileSizeNowEDW = 0;
			int logFileSizeNowUserRepository = 0;
			int logFileGrowthSizeNowSD1 = 0;
			int logFileGrowthSizeNowSD2 = 0;
			int logFileGrowthSizeNowSD3 = 0;
			int logFileGrowthSizeNowAudit = 0;
			int logFileGrowthSizeNowEDW = 0;
			int logFileGrowthSizeNowUserRepository = 0;
			int dataFileSize = 0;
			int logFileGrowthSize = 0;
			string message;

			SetToSimpleMode(tempDbName);
			SetToSimpleMode(tempDbSD001Name);
			SetToSimpleMode(tempDbSD002Name);
			SetToSimpleMode(tempDbSD003Name);
			SetToSimpleMode(tempDbUserRepositoryName);
			SetToSimpleMode(tempDbAuditName);
			SetToSimpleMode(tempDbEDWName);
			SetToWriteMode(tempDbName);
			SetToWriteMode(tempDbSD001Name);
			SetToWriteMode(tempDbSD002Name);
			SetToWriteMode(tempDbSD003Name);
			SetToWriteMode(tempDbUserRepositoryName);
			SetToWriteMode(tempDbAuditName);
			SetToWriteMode(tempDbEDWName);

			//shrink to initial state
			ShrinkLogFileTest();

			//increase log file size
			for (int i = 1; i <= 100; i++)
			{
				DoEnlargeLogFileToCreateVlfs(tempDbName);
				DoEnlargeLogFileToCreateVlfs(tempDbSD001Name);
				DoEnlargeLogFileToCreateVlfs(tempDbSD002Name);
				DoEnlargeLogFileToCreateVlfs(tempDbSD003Name);
				DoEnlargeLogFileToCreateVlfs(tempDbUserRepositoryName);
				DoEnlargeLogFileToCreateVlfs(tempDbAuditName);
				DoEnlargeLogFileToCreateVlfs(tempDbEDWName);
			}

			logFileSizeBefore = GetFileSizeInMb(tempDbName, false);
			logFileSizeBeforeSD1 = GetFileSizeInMb(tempDbSD001Name, false);
			logFileSizeBeforeSD2 = GetFileSizeInMb(tempDbSD002Name, false);
			logFileSizeBeforeSD3 = GetFileSizeInMb(tempDbSD003Name, false);
			logFileSizeBeforeAudit = GetFileSizeInMb(tempDbAuditName, false);
			logFileSizeBeforeEDW = GetFileSizeInMb(tempDbEDWName, false);
			logFileSizeBeforeUserRepository = GetFileSizeInMb(tempDbUserRepositoryName, false);

			logFileGrowthSizeBeforeSD1 = GetFileGrowthSize(tempDbSD001Name, false);
			logFileGrowthSizeBeforeSD2 = GetFileGrowthSize(tempDbSD002Name, false);
			logFileGrowthSizeBeforeSD3 = GetFileGrowthSize(tempDbSD003Name, false);
			logFileGrowthSizeBeforeAudit = GetFileGrowthSize(tempDbAuditName, false);
			logFileGrowthSizeBeforeEDW = GetFileGrowthSize(tempDbEDWName, false);
			logFileGrowthSizeBeforeUserRepository = GetFileGrowthSize(tempDbUserRepositoryName, false);

			message = ShrinkLogFileTest();
			var messages = message.Split(new char[] { '|' }).Select(x => x.Trim());
			Assert(!message.Contains("Operation failed"));

			CombineAssertions(() =>
			{
				AssertCollectionContains(tempDbName, messages);
				AssertCollectionContains(tempDbSD001Name, messages);
				AssertCollectionContains(tempDbSD002Name, messages);
				AssertCollectionContains(tempDbSD003Name, messages);
				AssertCollectionContains(tempDbAuditName, messages);
				AssertCollectionContains(tempDbEDWName, messages);
				AssertCollectionContains(tempDbUserRepositoryName, messages);
			});

			dataFileSize = GetFileSizeInMb(tempDbName, true);
			logFileGrowthSize = GetFileGrowthSize(tempDbName, false);

			if (dataFileSize > 51200)
			{
				Assert(@"If data file size>= 51200 MB, log file growth should be 4096MB", logFileGrowthSize == 4096);
			}
			else if (dataFileSize >= 10240)
			{
				Assert(@"If data file size >= 10240 and < 51200 MB, log file growth should be 1024MB", logFileGrowthSize == 1024);
			}
			else
			{
				Assert(@"If data file size < 10240 MB, log file growth should be 500MB", logFileGrowthSize == 500);
			}

			logFileSizeNow = GetFileSizeInMb(tempDbName, false);
			logFileSizeNowSD1 = GetFileSizeInMb(tempDbSD001Name, false);
			logFileSizeNowSD2 = GetFileSizeInMb(tempDbSD002Name, false);
			logFileSizeNowSD3 = GetFileSizeInMb(tempDbSD003Name, false);
			logFileSizeNowAudit = GetFileSizeInMb(tempDbAuditName, false);
			logFileSizeNowEDW = GetFileSizeInMb(tempDbEDWName, false);
			logFileSizeNowUserRepository = GetFileSizeInMb(tempDbUserRepositoryName, false);

			logFileGrowthSizeNowSD1 = GetFileGrowthSize(tempDbSD001Name, false);
			logFileGrowthSizeNowSD2 = GetFileGrowthSize(tempDbSD002Name, false);
			logFileGrowthSizeNowSD3 = GetFileGrowthSize(tempDbSD003Name, false);
			logFileGrowthSizeNowAudit = GetFileGrowthSize(tempDbAuditName, false);
			logFileGrowthSizeNowEDW = GetFileGrowthSize(tempDbEDWName, false);
			logFileGrowthSizeNowUserRepository = GetFileGrowthSize(tempDbUserRepositoryName, false);

			CombineAssertions(() =>
			{
				Assert(string.Format(CultureInfo.InvariantCulture, "Compare log file size between before shrink: {0}MB and after: {1}MB.", logFileSizeBefore, logFileSizeNow), logFileSizeNow <= logFileSizeBefore);
				Assert(string.Format(CultureInfo.InvariantCulture, "Compare eDocs log file size between before shrink: {0} MB and after {1}MB.", logFileSizeBeforeSD1, logFileSizeNowSD1), logFileSizeNowSD1 <= logFileSizeBeforeSD1);
				Assert(string.Format(CultureInfo.InvariantCulture, "Compare eDocs log file size between before shrink: {0} MB and after {1}MB.", logFileSizeBeforeSD2, logFileSizeNowSD2), logFileSizeNowSD2 <= logFileSizeBeforeSD2);
				Assert(string.Format(CultureInfo.InvariantCulture, "Compare eDocs log file size between before shrink: {0} MB and after {1}MB.", logFileSizeBeforeSD3, logFileSizeNowSD3), logFileSizeNowSD3 <= logFileSizeBeforeSD3);
				Assert(string.Format(CultureInfo.InvariantCulture, "Compare Audit log file size between before shrink: {0} MB and after {1}MB.", logFileSizeBeforeAudit, logFileSizeNowAudit), logFileSizeNowAudit <= logFileSizeBeforeAudit);
				Assert(string.Format(CultureInfo.InvariantCulture, "Compare EDW log file size between before shrink: {0} MB and after {1}MB.", logFileSizeBeforeEDW, logFileSizeNowEDW), logFileSizeNowEDW <= logFileSizeBeforeEDW);
				Assert(string.Format(CultureInfo.InvariantCulture, "Compare UserRepository log file size between before shrink: {0} MB and after {1}MB.", logFileSizeBeforeUserRepository, logFileSizeNowUserRepository), logFileSizeNowUserRepository <= logFileSizeBeforeUserRepository);

				int expectedBiLogFileGrowthSize = 1024;
				if (logFileGrowthSize * 0.1 < 64)
				{
					expectedBiLogFileGrowthSize = 64;
				}
				else if (logFileGrowthSize * 0.1 < 1024)
				{
					expectedBiLogFileGrowthSize = (int)(logFileGrowthSize * 0.1);
				}

				Assert(@"First eDocs database log growth size should be 10MB", logFileGrowthSizeNowSD1 == 10);
				Assert(@"Second eDocs database log growth size should be 10MB", logFileGrowthSizeNowSD2 == 10);
				Assert(@"Last eDocs database log growth size should be 500MB", logFileGrowthSizeNowSD3 == 500);
				AssertEquals(@"Audit database log growth size", expectedBiLogFileGrowthSize, logFileGrowthSizeNowAudit);
				AssertEquals(@"EDW database log growth size", expectedBiLogFileGrowthSize, logFileGrowthSizeNowEDW);
				Assert(@"UserRepository database log growth size should be 500MB", logFileGrowthSizeNowUserRepository == 500);
			});
		}

		public void TestShrinksWhenExcessiveVLFsDefault()
		{
			// Arrange
			const long largeBackupSizePercentageTolerance = 1000000;
			CreateFullBackupAndLogBackup(tempDbName);
			SetToWriteMode(tempDbName);
			ShrinkLogFileTest();
			DoEnlargeLogFileToCreateVlfs(tempDbName, count: defaultMaximumAllowedVLFsCount + 1);

			var startVlfCount = GetVlfCount(tempDbName);
			AssertGreaterThanOrEqualTo(startVlfCount, defaultMaximumAllowedVLFsCount);
			var startFileSize = (long)GetFileSizeInMb(tempDbName, false);
			var maximumLogBackupSizeInTwoWeeks = GetMaximumLogBackupSizeInTwoWeeks(tempDbName);
			var backupSizeToleranceInMb = maximumLogBackupSizeInTwoWeeks * largeBackupSizePercentageTolerance / 100 / 1024 / 1024;
			AssertLessThan(startFileSize, backupSizeToleranceInMb);

			// Act
			ShrinkLogFileTest(defaultMaximumAllowedVLFsCount, largeBackupSizePercentageTolerance);

			// Assert
			var afterVlfCount = GetVlfCount(tempDbName);
			AssertLessThan(afterVlfCount, startVlfCount);
		}

		public void TestShrinksWhenLargeLogBackupDefault()
		{
			// Arrange
			const int largeVLFsTolerance = 10000;
			CreateFullBackupAndLogBackup(tempDbName);
			SetToWriteMode(tempDbName);
			ShrinkLogFileTest();
			DoEnlargeLogFileToCreateVlfs(tempDbName, count: defaultMaximumAllowedVLFsCount + 1);

			var startVlfCount = GetVlfCount(tempDbName);
			AssertLessThan(startVlfCount, largeVLFsTolerance);

			var startFileSize = (long)GetFileSizeInMb(tempDbName, false);
			var maximumLogBackupSizeInTwoWeeks = GetMaximumLogBackupSizeInTwoWeeks(tempDbName);
			var backupSizeToleranceInMb = maximumLogBackupSizeInTwoWeeks * defaultMaximumLogToTwoWeekBackupPercentage / 100 / 1024 / 1024;
			AssertGreaterThan(startFileSize, backupSizeToleranceInMb);

			// Act
			ShrinkLogFileTest(largeVLFsTolerance, defaultMaximumLogToTwoWeekBackupPercentage);

			// Assert
			var afterFileSize = (long)GetFileSizeInMb(tempDbName, false);
			AssertLessThan(afterFileSize, startFileSize);
		}

		public void TestDoesNotShrinkWhenVlfIsLowAndLogSizeIsSmall()
		{
			// Arrange
			const int largeVLFsTolerance = 10000;
			const long largeBackupSizePercentageTolerance = 1000000;
			CreateFullBackupAndLogBackup(tempDbName);
			SetToWriteMode(tempDbName);
			ShrinkLogFileTest();
			DoEnlargeLogFileToCreateVlfs(tempDbName, count: defaultMaximumAllowedVLFsCount + 1);

			var startVlfCount = GetVlfCount(tempDbName);
			AssertLessThan(startVlfCount, largeVLFsTolerance);

			var startFileSize = GetFileSizeInMb(tempDbName, false);
			var maximumLogBackupSizeInTwoWeeks = GetMaximumLogBackupSizeInTwoWeeks(tempDbName);
			var backupSizeToleranceInMb = maximumLogBackupSizeInTwoWeeks * largeBackupSizePercentageTolerance / 100 / 1024 / 1024;
			AssertLessThan(startFileSize, backupSizeToleranceInMb);

			// Act
			ShrinkLogFileTest(largeVLFsTolerance, largeBackupSizePercentageTolerance);

			// Assert
			var afterVlfCount = GetVlfCount(tempDbName);
			AssertEquals(afterVlfCount, startVlfCount);

			var afterFileSize = GetFileSizeInMb(tempDbName, false);
			AssertEquals(afterFileSize, startFileSize);
		}

		public void TestReadOnlyMode()
		{
			SetToReadOnlyMode(tempDbName);
			SetToReadOnlyMode(tempDbSD001Name);
			SetToReadOnlyMode(tempDbSD002Name);
			SetToReadOnlyMode(tempDbSD003Name);
			SetToReadOnlyMode(tempDbUserRepositoryName);
			SetToReadOnlyMode(tempDbAuditName);
			SetToReadOnlyMode(tempDbEDWName);

			string message = ShrinkLogFileTest();
			Assert(message.Contains(@"Database is in read-only mode. Shrinking procedure skipped"));
			Assert(!message.Contains(@"3rd time backup log"));

			SetToWriteMode(tempDbName);
			SetToWriteMode(tempDbSD001Name);
			SetToWriteMode(tempDbSD002Name);
			SetToWriteMode(tempDbSD003Name);
			SetToWriteMode(tempDbUserRepositoryName);
			SetToWriteMode(tempDbAuditName);
			SetToWriteMode(tempDbEDWName);
		}

		public void TestFolderNotExists()
		{
			//Increase log file size
			for (int i = 1; i <= 100; i++)
			{
				DoEnlargeLogFileToCreateVlfs(tempDbName);
			}

			//Test invalid folder
			string shrinkMSG;
			object message;
			using (var connection = Db.NewAdminConnection(tempDbName))
			{
				try
				{
					using (DbCommand shrinkCmd = connection.Command("ep_ShrinkLogFile"))
					{
						shrinkCmd.CommandTimeout = 0; // No timeout
						shrinkCmd.CommandType = CommandType.StoredProcedure;
						shrinkCmd.AddParameter("@BKPath", SqlDbType.NVarChar, @"Z:\ZZZ");
						shrinkCmd.AddOutputParameter("@MSG", SqlDbType.NVarChar, 65535, 0, 0, 0);
						shrinkCmd.ExecuteNonQuery();
						message = shrinkCmd.GetParameterValue("@MSG");
						shrinkMSG = (message == DBNull.Value) ? string.Empty : message.ToString();
					}
				}
				catch (SqlException ex)
				{
					Assert("Failed to shrink Log file for DB " + tempDbName + ": " + ex.Number + " " + ex.Message, false);
					shrinkMSG = string.Empty;
				}
			}

			Assert(shrinkMSG.Contains("Backup path specified does not exist. Shrinking procedure skipped."));
		}

		public void TestWhenAcquiringBackupLockTimesOut()
		{
			// Arrange
			CreateFullBackupAndLogBackup(tempDbName);

			using var otherConnection = Db.NewAdminConnection();
			Assert("Should acquire lock", otherConnection.TryGetLock("LogBackupRunnerLock", out var logBackupLock, dbName: tempDbName));
			AssertNotNull(nameof(logBackupLock), logBackupLock);

			using (logBackupLock)
			{
				// Act
				var message = ShrinkLogFileTest();

				// Assert
				CombineAssertions(() =>
				{
					AssertNotContains("Shrink log file", message);
					AssertContains("Could not acquire LogBackupRunnerLock after 60 second. Shrinking procedure skipped.", message);
				});
			}
		}

		public void TestWhenAcquiringBackupLockSucceeds()
		{
			// Arrange
			CreateFullBackupAndLogBackup(tempDbName);
			var acquired = new ManualResetEventSlim(false);

			Task.Run(() =>
			{
				using var threadConnection = Db.NewAdminConnection();
				threadConnection.TryGetLock("LogBackupRunnerLock", out var threadLock, dbName: tempDbName);
				using (threadLock)
				{
					acquired.Set();
					Thread.Sleep(TimeSpan.FromSeconds(20)); // LBK works for 20 seconds, then finishes
				}
			});

			if (!acquired.Wait(TimeSpan.FromSeconds(10)))
			{
				Assert("Thread lock failed to acquire", false);
			}

			// Act
			var message = ShrinkLogFileTest();

			// Assert
			CombineAssertions(() =>
			{
				AssertContains("Shrink log file", message);
				AssertNotContains("Could not acquire LogBackupRunnerLock after 60 second. Shrinking procedure skipped.", message);

				using var otherConnection = Db.NewAdminConnection();
				Assert("Should have released the lock", otherConnection.TryGetLock("LogBackupRunnerLock", out var logBackupLock, dbName: tempDbName));
				AssertNotNull(nameof(logBackupLock), logBackupLock);
				logBackupLock.Dispose();
			});
		}

		#region setup

		protected override void SetUp()
		{
			base.SetUp();
			tempFolderName = NUnit.Framework.TempForTest.TempPath + "EP_SHRINKLOGFILE_TMP\\";
			CreateTestFolderIfNotExist();
			CreateTempDbsDropExisting();
			DoFullBackup();
			ClearBackupHistory();
			LoadSPSourceCode(ScriptToTest.Text);
			LoadSPSourceCode(epBackupDBScript);
		}

		protected override void TearDown()
		{
			DropTempDb();
			DeleteTestFolderIfExists();
			base.TearDown();
		}

		int GetVlfCount(string dbName)
		{
			int vlfCount = 0;
			using (var connection = Db.NewAdminConnection())
			{
				vlfCount = (int)connection.ExecuteScalar($"select count(*) from sys.dm_db_log_info(DB_ID('{dbName}'));");
			}
			return vlfCount;
		}

		void LoadSPSourceCode(string spSourceCode)
		{
			using (var connection = Db.NewAdminConnection(tempDbName))
			{
				using (DbCommand addProcedure = connection.Command(spSourceCode))
				{
					addProcedure.CommandTimeout = 0; // No timeout
					addProcedure.CommandType = CommandType.Text;
					addProcedure.ExecuteNonQuery();
				}
			}
		}

		void ClearBackupHistory()
		{
			string query = string.Format("exec msdb.dbo.sp_delete_database_backuphistory '{0}'", tempDbName);
			using (var connection = Db.NewAdminConnection())
			{
				using (DbCommand queryCommand = connection.Command(query))
				{
					queryCommand.CommandTimeout = 0; // No timeout
					queryCommand.CommandType = CommandType.Text;
					queryCommand.ExecuteScalar();
				}
			}
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

		void CreateTempDbsDropExisting()
		{
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				AdoTestUtils.CreateDbDropExisting(connection, tempDbName, DbRecoveryModel.Full);
				AdoTestUtils.CreateDbDropExisting(connection, tempDbSD001Name, DbRecoveryModel.Full);
				AdoTestUtils.CreateDbDropExisting(connection, tempDbSD002Name, DbRecoveryModel.Full);
				AdoTestUtils.CreateDbDropExisting(connection, tempDbSD003Name, DbRecoveryModel.Full);
				AdoTestUtils.CreateDbDropExisting(connection, tempDbUserRepositoryName, DbRecoveryModel.Full);
				AdoTestUtils.CreateDbDropExisting(connection, tempDbAuditName, DbRecoveryModel.Full);
				AdoTestUtils.CreateDbDropExisting(connection, tempDbEDWName, DbRecoveryModel.Full);

				SetToWriteMode(tempDbName);
				SetToWriteMode(tempDbSD001Name);
				SetToWriteMode(tempDbSD002Name);
				SetToWriteMode(tempDbSD003Name);
				SetToWriteMode(tempDbUserRepositoryName);
				SetToWriteMode(tempDbAuditName);
				SetToWriteMode(tempDbEDWName);
			}
		}

		string ShrinkLogFileTest(ulong maxVlfs = defaultMaximumAllowedVLFsCount, ulong maxBackupPercentage = defaultMaximumLogToTwoWeekBackupPercentage)
		{
			string shrinkMSG;
			object message;
			using (var connection = Db.NewAdminConnection(tempDbName))
			{
				try
				{
					using (DbCommand shrinkCmd = connection.Command("ep_ShrinkLogFile"))
					{
						shrinkCmd.CommandTimeout = 0; // No timeout
						shrinkCmd.CommandType = CommandType.StoredProcedure;
						shrinkCmd.AddParameter("@MAXVLFs", SqlDbType.Int, maxVlfs);
						shrinkCmd.AddParameter("@MaximumLogToTwoWeekBackupPercentage", SqlDbType.BigInt, maxBackupPercentage);
						shrinkCmd.AddParameter("@BKPath", SqlDbType.NVarChar, tempFolderName);
						shrinkCmd.AddOutputParameter("@MSG", SqlDbType.NVarChar, 65535, 0, 0, 0);
						shrinkCmd.ExecuteNonQuery();
						message = shrinkCmd.GetParameterValue("@MSG");
						shrinkMSG = (message == DBNull.Value) ? string.Empty : message.ToString();
						return shrinkMSG;
					}
				}
				catch (SqlException ex)
				{
					Assert("Failed to shrink Log file for DB " + tempDbName + ": " + ex.Number + " " + ex.Message, false);
					return string.Empty;
				}
			}
		}

		int GetFileSizeInMb(string dbName, bool isDataType)
		{
			var fileSize = 0;
			using (var connection = Db.NewAdminConnection())
			{
				fileSize = (int)connection.ExecuteScalar(string.Format(@"
					select sum(size/128) from sys.master_files where type = {1} and DB_NAME(database_id) = '{0}'", dbName, isDataType ? 0 : 1));
			}
			return fileSize;
		}

		long GetMaximumLogBackupSizeInTwoWeeks(string dbName)
		{
			using (var connection = Db.NewAdminConnection())
			{
				var result = connection.ExecuteScalar($@"
SELECT
	Max(ISNULL(msdb.dbo.backupset.backup_size, 0))
FROM
	msdb.dbo.backupset
WHERE
	msdb.dbo.backupset.backup_finish_date > DATEADD(day, -14, GETDATE())
AND
	msdb.dbo.backupset.type = 'L'
AND
	msdb.dbo.backupset.database_name = '{dbName}'")
					.ToString();
				return Convert.ToInt64(result);
			}
		}

		int GetFileGrowthSize(string dbName, bool isDataType)
		{
			var fileGrowthSize = 0;
			using (var connection = Db.NewAdminConnection())
			{
				fileGrowthSize = (int)connection.ExecuteScalar(string.Format(@"
					select top 1 case when is_percent_growth = 1 then growth else growth/128 end from sys.master_files where type = {1} and DB_NAME(database_id) = '{0}' order by name desc", dbName, isDataType ? 0 : 1));
			}
			return fileGrowthSize;
		}

		void DoEnlargeLogFileToCreateVlfs(string dbName, int enlargeAmountMB = 1, int count = 1)
		{
			for (var i = 0; i < count; i++)
			{
				DoEnlargeLogFileToCreateVlfsCore();
			}

			void DoEnlargeLogFileToCreateVlfsCore()
			{
				try
				{
					using (var connection = Db.NewAdminConnection())
					{
						int logFileSize = (int)connection.ExecuteScalar(string.Format(@"
                            select sum(size/128) from sys.master_files where type = 1 and DB_NAME(database_id) = '{0}'", dbName));
						connection.ExecuteScalar(string.Format(@"
                            ALTER DATABASE [{0}] MODIFY FILE (NAME = N'{1}', SIZE = {2}MB)", dbName, dbName + "_Log", logFileSize + enlargeAmountMB));
					}
				}
				catch (SqlException ex)
				{
					Assert("Failed to Enlarge Log File " + dbName + ": " + ex.Number + " " + ex.Message, false);
				}
			}
		}

		void CreateFullBackupAndLogBackup(string dbName)
		{
			var forlder = tempFolderName;
			var sql = $@"
BACKUP DATABASE [{dbName}] TO  DISK = N'{tempFolderName}\\{dbName}.bak' WITH NOFORMAT, NOINIT,  NAME = N'CreateFullBackupAndLogBackup Test', SKIP, NOREWIND, NOUNLOAD,  STATS = 10
BACKUP LOG [{dbName}] TO  DISK = N'{tempFolderName}\\{dbName}.trn' WITH NOFORMAT, NOINIT,  NAME = N'CreateFullBackupAndLogBackup Test', SKIP, NOREWIND, NOUNLOAD,  STATS = 10";

			using (var connection = Db.NewAdminConnection())
			{
				using (DbCommand createCmd = connection.Command(sql))
				{
					createCmd.CommandTimeout = 0; // No timeout
					createCmd.CommandType = CommandType.Text;
					createCmd.ExecuteNonQuery();
				}
			}
		}

		void SetToSimpleMode(string dbName)
		{
			var sql = string.Format(@"ALTER DATABASE [{0}] SET RECOVERY SIMPLE WITH NO_WAIT", dbName);

			using (var connection = Db.NewAdminConnection())
			{
				using (DbCommand createCmd = connection.Command(sql))
				{
					createCmd.CommandTimeout = 0; // No timeout
					createCmd.CommandType = CommandType.Text;
					createCmd.ExecuteNonQuery();
				}
			}
		}
		void SetToWriteMode(string dbName)
		{
			var sql = string.Format(@"ALTER DATABASE [{0}] SET READ_WRITE WITH NO_WAIT", dbName);

			using (var connection = Db.NewAdminConnection())
			{
				using (DbCommand createCmd = connection.Command(sql))
				{
					createCmd.CommandTimeout = 0; // No timeout
					createCmd.CommandType = CommandType.Text;
					createCmd.ExecuteNonQuery();
				}
			}
		}
		void SetToReadOnlyMode(string dbName)
		{
			var sql = string.Format(@"ALTER DATABASE [{0}] SET READ_ONLY WITH NO_WAIT", dbName);

			using (var connection = Db.NewAdminConnection())
			{
				using (DbCommand createCmd = connection.Command(sql))
				{
					createCmd.CommandTimeout = 0; // No timeout
					createCmd.CommandType = CommandType.Text;
					createCmd.ExecuteNonQuery();
				}
			}
		}

		void MakeFullBackup(string dbName)
		{
			try
			{
				using (var connection = Db.NewAdminConnection())
				{
					string backupCommandText = string.Format(@"BACKUP DATABASE [{0}] TO DISK = '{1}' WITH INIT;",
							dbName, tempFolderName + dbName + ".bak");
					connection.ExecuteNonQuery(backupCommandText);
				}
			}
			catch (SqlException ex)
			{
				Assert("Failed to backup DB " + dbName + ": " + ex.Number + " " + ex.Message, false);
			}
		}

		void DoFullBackup()
		{
			MakeFullBackup(tempDbName);
			MakeFullBackup(tempDbSD001Name);
			MakeFullBackup(tempDbSD002Name);
			MakeFullBackup(tempDbSD003Name);
			MakeFullBackup(tempDbUserRepositoryName);
			MakeFullBackup(tempDbAuditName);
			MakeFullBackup(tempDbEDWName);
		}
		void MakeLogBackup(string dbName)
		{
			try
			{
				using (var connection = Db.NewAdminConnection())
				{
					string backupCommandText = string.Format(@"BACKUP LOG [{0}] TO DISK = '{1}' WITH INIT;",
							dbName, tempFolderName + dbName + ".trn");
					connection.ExecuteNonQuery(backupCommandText);
				}
			}
			catch (SqlException ex)
			{
				Assert("Failed to backup LOG " + dbName + ": " + ex.Number + " " + ex.Message, false);
			}
		}
		void DoLogBackup()
		{
			MakeLogBackup(tempDbName);
			MakeLogBackup(tempDbSD001Name);
			MakeLogBackup(tempDbSD002Name);
			MakeLogBackup(tempDbSD003Name);
			MakeLogBackup(tempDbUserRepositoryName);
			MakeLogBackup(tempDbAuditName);
			MakeLogBackup(tempDbEDWName);
		}

		void DropTempDb()
		{
			using (var connection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(connection, tempDbName);
				AdoTestUtils.DropDbIfExists(connection, tempDbSD001Name);
				AdoTestUtils.DropDbIfExists(connection, tempDbSD002Name);
				AdoTestUtils.DropDbIfExists(connection, tempDbSD003Name);
				AdoTestUtils.DropDbIfExists(connection, tempDbUserRepositoryName);
				AdoTestUtils.DropDbIfExists(connection, tempDbAuditName);
				AdoTestUtils.DropDbIfExists(connection, tempDbEDWName);
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

		const int defaultMaximumAllowedVLFsCount = 100;
		const int defaultMaximumLogToTwoWeekBackupPercentage = 120;
		readonly string tempDbName = "TempDbForepShrinkLogFiletest";
		readonly string tempDbSD001Name = "TempDbForepShrinkLogFiletest_SD001";
		readonly string tempDbSD002Name = "TempDbForepShrinkLogFiletest_SD002";
		readonly string tempDbSD003Name = "TempDbForepShrinkLogFiletest_SD003";
		readonly string tempDbUserRepositoryName = "TempDbForepShrinkLogFiletest_UserRepository";
		readonly string tempDbAuditName = "TempDbForepShrinkLogFiletest_Audit";
		readonly string tempDbEDWName = "TempDbForepShrinkLogFiletest_EDW";
		readonly string epBackupDBScript = new ep_BackupDb().Text;
		string tempFolderName;
		#endregion
	}
}
