using System;
using System.Data;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.ExtendedProperties;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.ChangeDataCapture.Common.Testing;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ChangeDataCapture.Service.Testing
{
	class CaptureTaskSnapshotTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestCaptureChanges()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				CdcDatabase.Enable(testConnection, Db.DatabaseName);
				var captureTask = new CaptureTaskForTesting();

				string sqlText = @"
					CREATE TABLE dbo.CaptureTaskSnapshotTest$Table (ColA int PRIMARY KEY);
					EXEC sys.sp_cdc_enable_table
						@source_schema = 'dbo',
						@source_name = 'CaptureTaskSnapshotTest$Table',
						@role_name = null;";
				testConnection.ExecuteNonQuery(sqlText);

				captureTask.CaptureChangesCore_Exposed();
				AssertCdcTableCount(testConnection, 0);

				sqlText = "INSERT dbo.CaptureTaskSnapshotTest$Table VALUES (34);";
				testConnection.ExecuteNonQuery(sqlText);

				// Run capture task after insert operation
				long processedTranCount = captureTask.CaptureChangesCore_Exposed();
				AssertEquals("Process Transaction Count > 0?", true, processedTranCount > 0);
				AssertCdcTableCount(testConnection, 1);
				// Assert specific change (insert) captured
				sqlText = "SELECT count(*) FROM cdc.dbo_CaptureTaskSnapshotTest$Table_CT WHERE __$operation = 2 AND ColA = 34;";
				AssertEquals("Insert operation captured", 1, testConnection.ExecuteScalar(sqlText));

				// Run capture task again => no changes to capture
				processedTranCount = captureTask.CaptureChangesCore_Exposed();
				AssertEquals("Process Transaction Count (no further changes)", 0, processedTranCount);
				AssertCdcTableCount(testConnection, 1);
			}
		}

		[UseSnapshotProtection]
		public void TestCaptureChangesLogsCdcScan()
		{
			using (var connection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				if (!CdcDatabase.IsEnabled(connection, Db.DatabaseName))
				{
					CdcDatabase.Enable(connection, Db.DatabaseName);
				}

				connection.ExecuteNonQuery("CREATE TABLE [dbo].[TestTable] (c1 int, c2 int, c3 int)");
				var cdcTable = new CdcTableForTesting("dbo", "TestTable");
				cdcTable.EnableCdc(connection);

				connection.ExecuteNonQuery("INSERT INTO [dbo].[TestTable] (c1, c2, c3) VALUES (1, 1, 1)");

				var captureTask = new CaptureTaskForTesting();
				var testLogger = new LoggerForTest();

				captureTask.ServiceLogger = testLogger;
				captureTask.RunTask(CancellationToken.None);

				var endDate = (DateTime)connection.ExecuteScalar("SELECT TOP 1 end_time FROM sys.dm_cdc_log_scan_sessions WHERE last_commit_lsn <> 0x0 ORDER BY session_id");
				var endLsn = (string)connection.ExecuteScalar("SELECT TOP 1 last_commit_lsn FROM sys.dm_cdc_log_scan_sessions WHERE last_commit_lsn <> 0x0 ORDER BY session_id");
				endLsn = endLsn.Replace(":", string.Empty);

				var endScanMessage = $"Completed capturing changes for transactions up to LSN: 0x{endLsn}, Transaction Date (UTC): {endDate.ToUniversalTime()}";

				AssertCollectionContains($"Logs should contain {endScanMessage}", endScanMessage, testLogger.LogEntries);
			}
		}

		[UseSnapshotProtection]
		public void TestIsFirstCdcRunExtPropertyName()
		{
			using (var connection = Db.NewAdminConnection())
			{
				if (!CdcDatabase.IsEnabled(connection, Db.DatabaseName))
				{
					CdcDatabase.Enable(connection, Db.DatabaseName);
				}
				ExtProperty.Database.Update(connection, CdcDatabase.IsFirstCdcRunExtPropertyName, "1");
				AssertIsFirstCdcRunExtValue(connection, "1");

				var captureTask = new CaptureTaskForTesting();
				var testLogger = new LoggerForTest();
				captureTask.ServiceLogger = testLogger;
				captureTask.RunTask(CancellationToken.None);

				AssertIsFirstCdcRunExtValue(connection, null);
			}
		}

		[UseSnapshotProtection]
		public void TestNudgingAuditEtlTask()
		{
			using (var connection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var captureTask = new CaptureTaskForTesting();
				var testLogger = new LoggerForTest();
				captureTask.ServiceLogger = testLogger;

				if (!CdcDatabase.IsEnabled(connection, Db.DatabaseName))
				{
					CdcDatabase.Enable(connection, Db.DatabaseName);
					var cdcTable = new CdcTableForTesting("dbo", "GlbStaff");
					cdcTable.EnableCdc(connection);

					captureTask.RunTask(CancellationToken.None);
					AssertCollectionContains("Initial CDC scan should nudge Audit ETL task", "Nudging Audit ETL Execution Task", testLogger.LogEntries);
				}
				else
				{
					captureTask.RunTask(CancellationToken.None);
					AssertCollectionNotContains("CDC scan should not nudge Audit ETL task", "Nudging Audit ETL Execution Task", testLogger.LogEntries);
				}

				connection.ExecuteNonQuery("INSERT INTO dbo.GlbStaff(GS_PK, GS_Code, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES(newid(), 'TST', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
				testLogger.ClearLog();
				captureTask.RunTask(CancellationToken.None);
				AssertCollectionContains("CDC scan should nudge Audit ETL task", "Nudging Audit ETL Execution Task", testLogger.LogEntries);
			}
		}

		[UseSnapshotProtection]
		public void TestCaptureChangesFlushReplicationArticles()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				if (!CdcDatabase.IsEnabled(testConnection, Db.DatabaseName))
				{
					CdcDatabase.Enable(testConnection, Db.DatabaseName);
				}

				var captureTask = new CaptureTask();
				var testLogger = new LoggerForTest();
				captureTask.ServiceLogger = testLogger;
				captureTask.RunTask(CancellationToken.None);

				using (var cmd = testConnection.Command("sp_cdc_scan"))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@continuous", SqlDbType.Bit, 0);
					cmd.ExecuteNonQuery();
				}
				Assert(true);
			}
		}

		void AssertCdcTableCount(DbConnection testConnection, int expectedCount)
		{
			string sqlText = "SELECT count(*) FROM cdc.dbo_CaptureTaskSnapshotTest$Table_CT;";
			AssertEquals("CDC table row count", expectedCount, testConnection.ExecuteScalar(sqlText));
		}

		void AssertIsFirstCdcRunExtValue(DbConnection connection, string expectedValue)
		{
			var actualValue = ExtProperty.Database.Select(connection, CdcDatabase.IsFirstCdcRunExtPropertyName);
			AssertEquals("IsFirstCdcRunExtPropertyName", expectedValue, actualValue);
		}

		class CaptureTaskForTesting : CaptureTask
		{
			public long CaptureChangesCore_Exposed()
			{
				if (ServiceLogger == null)
				{
					ServiceLogger = new LoggerForTest();
				}
				return Scanner.ScanUntilNoTransactionsToProcess();
			}
		}
	}
}
