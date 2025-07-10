using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.ChangeDataCapture.Common.Testing;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ChangeDataCapture.Service.Testing
{
	class CleanupTaskSpecificTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestRunTaskCdcDisabled_WithDisableCdcFlagTrue()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				if (CdcDatabase.IsEnabled(testConnection, Db.DatabaseName))
				{
					CdcDatabase.Disable(testConnection, Db.DatabaseName);
				}

				using (SystemDataRegistry.Instance.BiDisableChangeDataCapture.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var cleanupTask = new CleanupTaskForTesting();
					var logger = new TestServiceLogger();
					cleanupTask.ServiceLogger = logger;
					cleanupTask.RunTask();

					Assert("Run task should do nothing", logger.Count == 0);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestDisableCdcRegistryItem_CdcAlreadyDisabled()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				if (CdcDatabase.IsEnabled(testConnection, Db.DatabaseName))
				{
					CdcDatabase.Disable(testConnection, Db.DatabaseName);
				}
				CdcDatabase.Enable(testConnection, Db.DatabaseName);

				using (SystemDataRegistry.Instance.BiDisableChangeDataCapture.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var cleanupTask = new CleanupTaskForTesting();
					cleanupTask.RunTask();
					var cdcTable = new CdcTableForTesting("dbo", "GlbStaff");

					Assert("CDC should not be enabled on test table after CDN", !cdcTable.IsCdcEnabled(testConnection));
					Assert("Disable CDC flag should still be true after CDN", DbRegistry.BiDisableChangeDataCapture.LoadValue(Db.Connection));

					cdcTable.EnableCdc(testConnection);
					cleanupTask.RunTask();
					Assert("If CDC is enabled on the table during another function, it should be disabled on next CDN", !cdcTable.IsCdcEnabled(testConnection));
					Assert("Disable CDC flag should still be true after CDN", DbRegistry.BiDisableChangeDataCapture.LoadValue(Db.Connection));
				}
			}
		}

		[UseSnapshotProtection]
		public void TestDisableCdcRegistryItem_GlowEnabled()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				if (CdcDatabase.IsEnabled(testConnection, Db.DatabaseName))
				{
					CdcDatabase.Disable(testConnection, Db.DatabaseName);
				}
				CdcDatabase.Enable(testConnection, Db.DatabaseName);

				using (SystemDataRegistry.Instance.BiDisableChangeDataCapture.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/"))
				{
					var cleanupTask = new CleanupTaskForTesting();
					var cdcTable = new CdcTableForTesting("dbo", "GlbStaff");
					cdcTable.EnableCdc(testConnection);

					cleanupTask.RunTask();

					Assert("CDC should not be enabled on test table after CDN", !cdcTable.IsCdcEnabled(testConnection));
					Assert("Disable CDC flag should still be true after CDN", DbRegistry.BiDisableChangeDataCapture.LoadValue(Db.Connection));
					Assert("When GLOW is enabled, Change Tracking should be enabled", IsChangeTrackingEnabled(testConnection));
				}
			}
		}

		bool IsChangeTrackingEnabled(AdminConnection testConnection)
		{
			var sqlText = @"
				IF EXISTS (
					select * from sys.change_tracking_tables where object_id = OBJECT_ID(N'dbo.GlbStaff')
				)
				SELECT 1 ELSE SELECT 0
			";
			var changeTrackingIsEnabled = Convert.ToBoolean(testConnection.ExecuteScalar<int>(sqlText));
			return changeTrackingIsEnabled;
		}

		[UseSnapshotProtection]
		public void TestIncreaseTimeoutWhenTimeoutErrorOccursButProgressIsMade()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				if (CdcDatabase.IsEnabled(testConnection, Db.DatabaseName))
				{
					CdcDatabase.Disable(testConnection, Db.DatabaseName);
				}
				CdcDatabase.Enable(testConnection, Db.DatabaseName);

				var scanner = new OnlineCdcScanner(new LoggerForTest());
				var cleanupTask = new CleanupTaskForTesting();
				cleanupTask.ThrowRequestTimeoutException = true;
				cleanupTask.UnprocessedRowCountDecreasesForTest = true;
				cleanupTask.FakeUnprocessedRowCount = 10;

				string sqlText = @"
					CREATE TABLE dbo.CleanupTaskSnapshotTest$Table (ColA int PRIMARY KEY);
					EXEC sys.sp_cdc_enable_table
						@source_schema = 'dbo',
						@source_name = 'CleanupTaskSnapshotTest$Table',
						@role_name = null;
					INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (1);";
				testConnection.ExecuteNonQuery(sqlText);

				AssertCdcTableContents(testConnection);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 1);

				cleanupTask.RunTask();
				AssertEquals("Timeout should increase on retry", cleanupTask.defaultCommandTimeoutInSeconds * 3, cleanupTask.CmdTimeoutInSeconds);
			}
		}

		[UseSnapshotProtection]
		public void TestCdcCleanupRetry()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				CdcDatabase.Enable(testConnection, Db.DatabaseName);
				var scanner = new OnlineCdcScanner(new LoggerForTest());
				var cleanupTask = new CleanupTaskForTesting();
				cleanupTask.FakeUnprocessedRowCount = 10;
				cleanupTask.ThrowFailedCaptureInstanceException = true;

				string sqlText = @"
					CREATE TABLE dbo.CleanupTaskSnapshotTest$Table (ColA int PRIMARY KEY);
					EXEC sys.sp_cdc_enable_table
						@source_schema = 'dbo',
						@source_name = 'CleanupTaskSnapshotTest$Table',
						@role_name = null;
					INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (1);";
				testConnection.ExecuteNonQuery(sqlText);
				scanner.ScanUntilNoTransactionsToProcess();

				try
				{
					cleanupTask.RunTask();
					Fail("CdcException expected, but was not thrown.");
				}
				catch (CdcException ex)
				{
					AssertEquals("Should try to continue before throwing exception", 4, cleanupTask.NumberOfTimesCleanupProcedureCalled);
					AssertContains("Exception message", "Cleanup procedure has failed with an exception 3 times", ex.Message);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestWhenCleanupFailedParameterNotSupportedCallProcedureWithoutCleanupFailedParameter()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				var cleanupTask = new CleanupTaskForTesting();
				cleanupTask.ThrowIncorrectNumberOfParametersException = true;
				var lowWaterMark = new byte[] { 0x00000000000000000000 };
				cleanupTask.ExecuteCleanupProcedure_Exposed("dbo_CleanupTaskSnapshotTest$DummyTable", lowWaterMark);
				AssertEquals("Cleanup Procedure with CleanupFailedParameter is called", false, cleanupTask.SupportCleanupFailedParameter);
				AssertEquals("Cleanup Procedure without CleanupFailedParameter is called", true, cleanupTask.IsCleanupProcedureWithoutCleanupFailedParameterCalled);
			}
		}

		[UseSnapshotProtection]
		public void TestRetentionForCleanupWithGLOWEnabledSystems()
		{
			var cleanupTask = new CleanupTaskForTesting(isRetentionPeriodEnforcedForTesting: true);
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/"))
			{
				AssertEquals("When GLOW is enabled, CDC Schema retention should be 1 day", 1, cleanupTask.CDCRetentionPeriodExposed.Days);
			}

			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				AssertEquals("GLOW registry is cached, when GLOW is not enabled, CDC Schema retention should still be 1 day", 1, cleanupTask.CDCRetentionPeriodExposed.Days);
			}

			cleanupTask = new CleanupTaskForTesting(isRetentionPeriodEnforcedForTesting: true);
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				AssertEquals("New CleanupTask, when GLOW is not enabled, CDC Schema retention should be 0 days", 0, cleanupTask.CDCRetentionPeriodExposed.Days);
			}
		}

		[UseSnapshotProtection]
		public void TestCleanupCdcWithGLOW()
		{
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/"))
			using (var testConnection = Db.NewAdminConnection())
			{
				CdcDatabase.Enable(testConnection, Db.DatabaseName);
				var scanner = new OnlineCdcScanner(new LoggerForTest());
				var cleanupTask = new CleanupTaskForTesting(isRetentionPeriodEnforcedForTesting: true);

				string sqlText = @"
					CREATE TABLE dbo.CleanupTaskSnapshotTest$Table (ColA int PRIMARY KEY);
					EXEC sys.sp_cdc_enable_table
						@source_schema = 'dbo',
						@source_name = 'CleanupTaskSnapshotTest$Table',
						@role_name = null;
					INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (1);";
				testConnection.ExecuteNonQuery(sqlText);

				AssertCdcTableContents(testConnection);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 1);

				var fetchLowWatermarkSqlText = "SELECT ISNULL((SELECT Convert(nvarchar(max),sys.fn_cdc_get_max_lsn(),1)), '0x00000000000000000000')";
				var lowWaterMark = (string)testConnection.ExecuteScalar(fetchLowWatermarkSqlText);

				var updateTranEndTimeSqlText = "UPDATE CDC.lsn_time_mapping set tran_end_time = DATEADD(day, -2, tran_end_time) WHERE start_lsn = " + lowWaterMark;
				testConnection.ExecuteNonQuery(updateTranEndTimeSqlText);

				var deleteResidualLSNsSqlText = "DELETE FROM CDC.lsn_time_mapping Where Start_lsn < " + lowWaterMark;
				testConnection.ExecuteNonQuery(deleteResidualLSNsSqlText);

				sqlText = "INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (2);";
				testConnection.ExecuteNonQuery(sqlText);
				scanner.ScanUntilNoTransactionsToProcess();

				fetchLowWatermarkSqlText = "SELECT ISNULL((SELECT Convert(nvarchar(max),sys.fn_cdc_get_max_lsn(),1)), '0x00000000000000000000')";
				lowWaterMark = (string)testConnection.ExecuteScalar(fetchLowWatermarkSqlText);

				AssertCdcTableContents(testConnection, 1, 2);
				cleanupTask.RunTask();
				AssertCdcTableContents(testConnection, 2);

				sqlText = "INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (3);";
				testConnection.ExecuteNonQuery(sqlText);
				scanner.ScanUntilNoTransactionsToProcess();

				AssertCdcTableContents(testConnection, 2, 3);
				cleanupTask.RunTask();
				AssertCdcTableContents(testConnection, 2, 3);

				updateTranEndTimeSqlText = "UPDATE CDC.lsn_time_mapping set tran_end_time = DATEADD(day, -2, tran_end_time) WHERE start_lsn = " + lowWaterMark;
				testConnection.ExecuteNonQuery(updateTranEndTimeSqlText);

				AssertCdcTableContents(testConnection, 2, 3);
				cleanupTask.RunTask();
				AssertCdcTableContents(testConnection, 3);
			}
		}

		[UseSnapshotProtection]
		public void TestCleanupCdcWithoutGLOW()
		{
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			using (var testConnection = Db.NewAdminConnection())
			{
				CdcDatabase.Enable(testConnection, Db.DatabaseName);
				var scanner = new OnlineCdcScanner(new LoggerForTest());
				var cleanupTask = new CleanupTaskForTesting();

				string sqlText = @"
					CREATE TABLE dbo.CleanupTaskSnapshotTest$Table (ColA int PRIMARY KEY);
					EXEC sys.sp_cdc_enable_table
						@source_schema = 'dbo',
						@source_name = 'CleanupTaskSnapshotTest$Table',
						@role_name = null;
					INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (1);";
				testConnection.ExecuteNonQuery(sqlText);

				AssertCdcTableContents(testConnection);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 1);

				cleanupTask.RunTask();
				AssertCdcTableContents(testConnection, 1);

				sqlText = "INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (2);";
				testConnection.ExecuteNonQuery(sqlText);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 1, 2);

				sqlText = "INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (3);";
				testConnection.ExecuteNonQuery(sqlText);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 1, 2, 3);

				cleanupTask.RunTask();
				AssertCdcTableContents(testConnection, 3);
			}
		}

		[UseSnapshotProtection]
		public void TestCleanupCdc_NoDataWarehouseServerSet()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				CdcDatabase.Enable(testConnection, Db.DatabaseName);
				var scanner = new OnlineCdcScanner(new LoggerForTest());
				var cleanupTask = new CleanupTaskForTesting();
				cleanupTask.IsAuditServerSet = true;
				cleanupTask.IsDataWarehouseServerSet = false;

				string sqlText = @"
					CREATE TABLE dbo.CleanupTaskSnapshotTest$Table (ColA int PRIMARY KEY);
					EXEC sys.sp_cdc_enable_table
						@source_schema = 'dbo',
						@source_name = 'CleanupTaskSnapshotTest$Table',
						@role_name = null;
					INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (1);";
				testConnection.ExecuteNonQuery(sqlText);

				AssertCdcTableContents(testConnection);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 1);

				cleanupTask.RunTask();
				AssertCdcTableContents(testConnection, 1);

				sqlText = "INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (2);";
				testConnection.ExecuteNonQuery(sqlText);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 1, 2);

				cleanupTask.RunTask();
				AssertCdcTableContents(testConnection, 2);

				sqlText = "INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (3);";
				testConnection.ExecuteNonQuery(sqlText);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 2, 3);

				cleanupTask.RunTask();
				AssertCdcTableContents(testConnection, 3);
			}
		}

		[UseSnapshotProtection]
		public void TestCleanupCdc_NoAuditServerSet()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				CdcDatabase.Enable(testConnection, Db.DatabaseName);
				var scanner = new OnlineCdcScanner(new LoggerForTest());
				var cleanupTask = new CleanupTaskForTesting();
				cleanupTask.IsAuditServerSet = false;
				cleanupTask.IsDataWarehouseServerSet = true;

				string sqlText = @"
					CREATE TABLE dbo.CleanupTaskSnapshotTest$Table (ColA int PRIMARY KEY);
					EXEC sys.sp_cdc_enable_table
						@source_schema = 'dbo',
						@source_name = 'CleanupTaskSnapshotTest$Table',
						@role_name = null;
					INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (1);";
				testConnection.ExecuteNonQuery(sqlText);

				AssertCdcTableContents(testConnection);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 1);

				cleanupTask.RunTask();
				AssertCdcTableContents(testConnection, 1);

				sqlText = "INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (2);";
				testConnection.ExecuteNonQuery(sqlText);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 1, 2);

				cleanupTask.RunTask();
				AssertCdcTableContents(testConnection, 2);

				sqlText = "INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (3);";
				testConnection.ExecuteNonQuery(sqlText);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 2, 3);

				cleanupTask.RunTask();
				AssertCdcTableContents(testConnection, 3);
			}
		}

		[UseSnapshotProtection]
		public void TestCleanupCdc_NoBiDatabases()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				CdcDatabase.Enable(testConnection, Db.DatabaseName);
				var scanner = new OnlineCdcScanner(new LoggerForTest());
				var cleanupTask = new CleanupTaskForTesting();
				cleanupTask.AuditDbExists = false;
				cleanupTask.EdwDbExists = false;

				string sqlText = @"
					CREATE TABLE dbo.CleanupTaskSnapshotTest$Table (ColA int PRIMARY KEY);
					EXEC sys.sp_cdc_enable_table
						@source_schema = 'dbo',
						@source_name = 'CleanupTaskSnapshotTest$Table',
						@role_name = null;
					INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (1);";
				testConnection.ExecuteNonQuery(sqlText);

				AssertCdcTableContents(testConnection);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 1);

				cleanupTask.RunTask();
				AssertCdcTableContents(testConnection, 1);

				sqlText = "INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (2);";
				testConnection.ExecuteNonQuery(sqlText);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 1, 2);

				cleanupTask.RunTask();
				AssertCdcTableContents(testConnection, 2);

				sqlText = "INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (3);";
				testConnection.ExecuteNonQuery(sqlText);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 2, 3);

				cleanupTask.RunTask();
				AssertCdcTableContents(testConnection, 3);
			}
		}

		[UseSnapshotProtection]
		public void TestCleanupCdc_EtlNotRunning()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				CdcDatabase.Enable(testConnection, Db.DatabaseName);
				var scanner = new OnlineCdcScanner(new LoggerForTest());
				var cleanupTask = new CleanupTaskForTesting();
				cleanupTask.AuditEtlRunning = false;
				cleanupTask.EdwEtlRunning = false;

				string sqlText = @"
					CREATE TABLE dbo.CleanupTaskSnapshotTest$Table (ColA int PRIMARY KEY);
					EXEC sys.sp_cdc_enable_table
						@source_schema = 'dbo',
						@source_name = 'CleanupTaskSnapshotTest$Table',
						@role_name = null;
					INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (1);";
				testConnection.ExecuteNonQuery(sqlText);

				AssertCdcTableContents(testConnection);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 1);

				cleanupTask.RunTask();
				AssertCdcTableContents(testConnection, 1);

				sqlText = "INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (2);";
				testConnection.ExecuteNonQuery(sqlText);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 1, 2);

				cleanupTask.RunTask();
				AssertCdcTableContents(testConnection, 1, 2);

				sqlText = "INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (3);";
				testConnection.ExecuteNonQuery(sqlText);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 1, 2, 3);

				cleanupTask.RunTask();
				AssertCdcTableContents(testConnection, 1, 2, 3);
			}
		}

		[UseSnapshotProtection]
		public void TestCleanupCdc_AuditDbDoesNotExist()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				CdcDatabase.Enable(testConnection, Db.DatabaseName);
				var scanner = new OnlineCdcScanner(new LoggerForTest());
				var cleanupTask = new CleanupTaskForTesting();
				cleanupTask.AuditDbExists = false;
				cleanupTask.EdwEtlRunning = true;

				string sqlText = @"
					CREATE TABLE dbo.CleanupTaskSnapshotTest$Table (ColA int PRIMARY KEY);
					EXEC sys.sp_cdc_enable_table
						@source_schema = 'dbo',
						@source_name = 'CleanupTaskSnapshotTest$Table',
						@role_name = null;
					INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (1);";
				testConnection.ExecuteNonQuery(sqlText);

				AssertCdcTableContents(testConnection);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 1);

				cleanupTask.RunTask();
				AssertCdcTableContents(testConnection, 1);

				sqlText = "INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (2);";
				testConnection.ExecuteNonQuery(sqlText);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 1, 2);

				cleanupTask.RunTask();
				AssertCdcTableContents(testConnection, 2);

				sqlText = "INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (3);";
				testConnection.ExecuteNonQuery(sqlText);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 2, 3);

				cleanupTask.RunTask();
				AssertCdcTableContents(testConnection, 3);
			}
		}

		[UseSnapshotProtection]
		public void TestCleanupCdc_EdwDbDoesNotExist()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				CdcDatabase.Enable(testConnection, Db.DatabaseName);
				var scanner = new OnlineCdcScanner(new LoggerForTest());
				var cleanupTask = new CleanupTaskForTesting();
				cleanupTask.AuditEtlRunning = true;
				cleanupTask.EdwDbExists = false;

				string sqlText = @"
					CREATE TABLE dbo.CleanupTaskSnapshotTest$Table (ColA int PRIMARY KEY);
					EXEC sys.sp_cdc_enable_table
						@source_schema = 'dbo',
						@source_name = 'CleanupTaskSnapshotTest$Table',
						@role_name = null;
					INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (1);";
				testConnection.ExecuteNonQuery(sqlText);

				AssertCdcTableContents(testConnection);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 1);

				cleanupTask.RunTask();
				AssertCdcTableContents(testConnection, 1);

				sqlText = "INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (2);";
				testConnection.ExecuteNonQuery(sqlText);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 1, 2);

				cleanupTask.RunTask();
				AssertCdcTableContents(testConnection, 2);

				sqlText = "INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (3);";
				testConnection.ExecuteNonQuery(sqlText);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 2, 3);

				cleanupTask.RunTask();
				AssertCdcTableContents(testConnection, 3);
			}
		}

		[UseSnapshotProtection]
		public void TestCleanupCdc_CdcDisabled()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				if (CdcDatabase.IsEnabled(testConnection, Db.DatabaseName))
				{
					CdcDatabase.Disable(testConnection, Db.DatabaseName);
				}

				var cleanupTask = new CleanupTask();
				var logger = new LoggerForTest();
				cleanupTask.ServiceLogger = logger;

				using (SystemDataRegistry.Instance.BiDisableChangeDataCapture.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					cleanupTask.RunTask();
					var expectedMessage = "Database is not enabled for change data capture. Check if Data Warehouse and Audit registry items are set and perform database upgrade.";
					AssertCollectionContains(logger.ToString(), expectedMessage, logger.LogEntries);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestCleanupCdc_LockRequestTimeout()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				if (CdcDatabase.IsEnabled(testConnection, Db.DatabaseName))
				{
					CdcDatabase.Disable(testConnection, Db.DatabaseName);
				}
				CdcDatabase.Enable(testConnection, Db.DatabaseName);

				var scanner = new OnlineCdcScanner(new LoggerForTest());
				var cleanupTask = new CleanupTaskForTesting();
				cleanupTask.AuditDbExists = false;
				cleanupTask.EdwDbExists = false;
				cleanupTask.ThrowLockRequestTimeoutException = true;
				cleanupTask.FakeUnprocessedRowCount = 1;
				scanner.ScanUntilNoTransactionsToProcess();

				string sqlText = @"
					CREATE TABLE dbo.CleanupTaskSnapshotTest$Table (ColA int PRIMARY KEY);
					EXEC sys.sp_cdc_enable_table
						@source_schema = 'dbo',
						@source_name = 'CleanupTaskSnapshotTest$Table',
						@role_name = null;
					INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (1);";
				testConnection.ExecuteNonQuery(sqlText);

				AssertCdcTableContents(testConnection);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 1);

				cleanupTask.RunTask();
				AssertArrayEqualsByElements(
					new string[] {
						"Cleaning up capture instance [dbo_CleanupTaskSnapshotTest$Table].",
						"Lock request timeout. Retrying in the next run.\r\nError Number: 1222, Message: Lock request time out period exceeded.\r\n"
					},
					cleanupTask.Logs.ToArray());
				AssertEquals(4, cleanupTask.NumberOfTimesCleanupProcedureCalled);
			}
		}

		[UseSnapshotProtection]
		public void TestCleanupCdc_OnlyLockTimeoutStops()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				if (CdcDatabase.IsEnabled(testConnection, Db.DatabaseName))
				{
					CdcDatabase.Disable(testConnection, Db.DatabaseName);
				}
				CdcDatabase.Enable(testConnection, Db.DatabaseName);

				var scanner = new OnlineCdcScanner(new LoggerForTest());
				var cleanupTask = new CleanupTaskForTesting();
				cleanupTask.AuditDbExists = false;
				cleanupTask.EdwDbExists = false;
				cleanupTask.ThrowLockRequestTimeoutException = true;
				cleanupTask.FakeUnprocessedRowCount = 0;
				scanner.ScanUntilNoTransactionsToProcess();

				string sqlText = @"
					CREATE TABLE dbo.CleanupTaskSnapshotTest$Table (ColA int PRIMARY KEY);
					CREATE TABLE dbo.CleanupTaskSnapshotTest$TableTwo (ColA int PRIMARY KEY);
					EXEC sys.sp_cdc_enable_table
						@source_schema = 'dbo',
						@source_name = 'CleanupTaskSnapshotTest$Table',
						@role_name = null;
					EXEC sys.sp_cdc_enable_table
						@source_schema = 'dbo',
						@source_name = 'CleanupTaskSnapshotTest$TableTwo',
						@role_name = null;
					INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (1);
					INSERT dbo.CleanupTaskSnapshotTest$TableTwo VALUES (1);";
				testConnection.ExecuteNonQuery(sqlText);

				AssertCdcTableContents(testConnection);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 1);

				cleanupTask.RunTask();
				// There are two cdc tables, after retrying and failing to cleanup the first, it should not continue on the other
				// so should call the procedure once + 3 retry's
				AssertEquals(4, cleanupTask.NumberOfTimesCleanupProcedureCalled);
			}
		}

		[UseSnapshotProtection]
		public void TestCleanupCdc_LockRequestTimeoutMoreThan3Days()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				if (CdcDatabase.IsEnabled(testConnection, Db.DatabaseName))
				{
					CdcDatabase.Disable(testConnection, Db.DatabaseName);
				}
				CdcDatabase.Enable(testConnection, Db.DatabaseName);

				var scanner = new OnlineCdcScanner(new LoggerForTest());
				var cleanupTask = new CleanupTaskForTesting();
				cleanupTask.ThrowLockRequestTimeoutException = true;
				cleanupTask.FakeUnprocessedRowCount = 1;

				string sqlText = @"insert into cdc.lsn_time_mapping (start_lsn, tran_end_time)
values (0x00000000000000000001, dateadd(yy, -1, getdate()))";
				testConnection.ExecuteNonQuery(sqlText);

				sqlText = @"
					CREATE TABLE dbo.CleanupTaskSnapshotTest$Table (ColA int PRIMARY KEY);
					EXEC sys.sp_cdc_enable_table
						@source_schema = 'dbo',
						@source_name = 'CleanupTaskSnapshotTest$Table',
						@role_name = null;
					INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (1);";
				testConnection.ExecuteNonQuery(sqlText);

				AssertCdcTableContents(testConnection);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, 1);

				try
				{
					cleanupTask.RunTask();
					Fail("SqlException expected, but was not thrown.");
				}
				catch (Exception ex)
				{
					Assert(ex.InnerException is SqlException);
					AssertEquals("SQL exception type", DbErrorType.LockTimeoutExpired, new DbErrorMatch((SqlException)ex.InnerException).ExceptionType);
					AssertEquals("Exception message", "CDC Cleanup has failed to run for greater than 3 days.", ex.Message);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestCleanupCdc_ExecutionTimeout()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				if (CdcDatabase.IsEnabled(testConnection, Db.DatabaseName))
				{
					CdcDatabase.Disable(testConnection, Db.DatabaseName);
				}
				CdcDatabase.Enable(testConnection, Db.DatabaseName);

				var scanner = new OnlineCdcScanner(new LoggerForTest());
				var cleanupTask = new CleanupTaskForTesting();
				scanner.ScanUntilNoTransactionsToProcess();

				string sqlText = @"
					CREATE TABLE dbo.CleanupTaskSnapshotTest$Table (ColA int PRIMARY KEY);
					EXEC sys.sp_cdc_enable_table
						@source_schema = 'dbo',
						@source_name = 'CleanupTaskSnapshotTest$Table',
						@role_name = null;
					INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (1);";
				testConnection.ExecuteNonQuery(sqlText);

				AssertCdcTableContents(testConnection);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, new[] { 1 });

				sqlText = @"INSERT dbo.CleanupTaskSnapshotTest$Table VALUES (2);";
				testConnection.Command(sqlText).ExecuteNonQuery();

				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableContents(testConnection, new[] { 1, 2 });

				try
				{
					using (var lockConnection = Db.NewAdminConnection())
					{
						lockConnection.BeginTransaction();
						lockConnection.ExecuteNonQuery("SELECT * FROM cdc.dbo_CleanupTaskSnapshotTest$Table_CT with (TABLOCK, HOLDLOCK)");
						cleanupTask.RunTask();
					}
					Fail("Expected CdcException to be thrown");
				}
				catch (CdcException ex)
				{
					CombineAssertions(() =>
					{
						AssertEquals("Exception message", "Failed to clean up capture instance [dbo_CleanupTaskSnapshotTest$Table].", ex.Message);
						AssertEquals("Inner exception type", typeof(SqlException), ex.InnerException.GetType());
						if (ex.InnerException is SqlException)
						{
							AssertEquals("Inner exception error code", DbErrorType.TimeoutExpired, new DbErrorMatch(ex.InnerException as SqlException).ExceptionType);
						}
					});
				}
			}
		}

		[UseSnapshotProtection]
		public void TestCleanupThresholdValue()
		{
			using (var testConnection = Db.NewAdminConnection())
			using (var mainDbConnection = CleanupTaskForTesting.GetMainDbConnecionForTest())
			{
				if (!CdcDatabase.IsEnabled(testConnection, Db.DatabaseName))
				{
					CdcDatabase.Enable(testConnection, Db.DatabaseName);
				}

				var scanner = new OnlineCdcScanner(new LoggerForTest());
				var cleanupTask = new CleanupTaskForTesting();
				cleanupTask.testMainDbconnection = mainDbConnection;
				var sqlText = @"
					CREATE TABLE dbo.CleanupTaskSnapshotTest$Table (ColA int PRIMARY KEY);
					EXEC sys.sp_cdc_enable_table
						@source_schema = 'dbo',
						@source_name = 'CleanupTaskSnapshotTest$Table',
						@role_name = null;";
				testConnection.ExecuteNonQuery(sqlText);

				var sqlTextBuilder = new StringBuilder();
				for (var counter = 1; counter <= 10000; counter++)
				{
					sqlTextBuilder.AppendLine($"INSERT INTO dbo.CleanupTaskSnapshotTest$Table(ColA) VALUES({counter})");
				}
				testConnection.ExecuteNonQuery(sqlTextBuilder.ToString());

				scanner.ScanUntilNoTransactionsToProcess();

				sqlText = @"CREATE TABLE dbo.TestTableLog (IDC INT IDENTITY(1,1) PRIMARY KEY, NumRowsDeleted BIGINT)";
				testConnection.ExecuteNonQuery(sqlText);

				sqlText = @"
					CREATE TRIGGER cdc.TG_CleanupTaskSnapshotTest$Table_CT_Delete ON  cdc.dbo_CleanupTaskSnapshotTest$Table_CT AFTER DELETE
					AS
					BEGIN
						SET NOCOUNT ON;
									INSERT INTO dbo.TestTableLog(NumRowsDeleted)
						SELECT COUNT(*) FROM Deleted
					END";
				testConnection.ExecuteNonQuery(sqlText);

				var captureInstanceRowCountQuery = "SELECT COUNT(*) FROM cdc.dbo_CleanupTaskSnapshotTest$Table_CT";
				var initialRowCount = Convert.ToInt32(testConnection.ExecuteScalar(captureInstanceRowCountQuery));
				AssertEquals("Capture Instance row count", 10000, initialRowCount);

				sqlText = "SELECT ISNULL((SELECT sys.fn_cdc_get_max_lsn()), 0x00000000000000000000)";
				var lowWaterMark = (byte[])testConnection.ExecuteScalar(sqlText);

				cleanupTask.ExecuteCleanupProcedure_Exposed("dbo_CleanupTaskSnapshotTest$Table", lowWaterMark);

				var deletedRowsDuringFirstBatch = Convert.ToInt32(testConnection.ExecuteScalar("SELECT NumRowsDeleted FROM dbo.TestTableLog WHERE IDC = 1"));
				AssertEquals("Capture Instance row count", 4999, deletedRowsDuringFirstBatch);
			}
		}

		void AssertCdcTableContents(DbConnection testConnection, params int[] colAValues)
		{
			AssertCdcTableCount(testConnection, colAValues.Length);

			// Expected
			var expectedContents = new StringBuilder();
			expectedContents.AppendLine("Oper - ColA");
			expectedContents.AppendLine("-----------");

			for (int i = 0; i < colAValues.Length; i++)
			{
				expectedContents.AppendFormat("   2 -    {0}", colAValues[i].ToString()).AppendLine();
			}

			// Actual
			var actualContents = new StringBuilder();
			actualContents.AppendLine("Oper - ColA");
			actualContents.AppendLine("-----------");

			using (var cmd = testConnection.Command("SELECT [__$operation], ColA FROM cdc.dbo_CleanupTaskSnapshotTest$Table_CT ORDER BY __$start_lsn"))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					actualContents.AppendFormat("   {0} -    {1}", reader[0].ToString(), reader[1].ToString()).AppendLine();
				}
			}

			AssertEquals("CDC Table Contents", expectedContents.ToString(), actualContents.ToString());
		}

		void AssertCdcTableCount(DbConnection testConnection, int expectedCount)
		{
			string sqlText = "SELECT count(*) FROM cdc.dbo_CleanupTaskSnapshotTest$Table_CT;";
			AssertEquals("CDC table row count", expectedCount, testConnection.ExecuteScalar(sqlText));
		}

		public void TestGetLsnByTimeOffsetHandlesNullMinLsn()
		{
			var cleanupTask = new CleanupTaskForTesting();
			using (var testConnection = CleanupTaskForTesting.GetMainDbConnecionForTest())
			{
				cleanupTask.testMainDbconnection = testConnection;
				var lsnResult = cleanupTask.GetLsnByTimeOffset_Exposed(minLSN: null, timespan: TimeSpan.Zero);
				AssertEquals("Result when minLSN is null", new byte[10], lsnResult);
			}
		}

		[UseSnapshotProtection]
		public void TestCdnServiceTaskBacklog_NotNegativeAfterCleanup()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				var cleanupTask = new CleanupTaskForTesting();
				var cleanupTaskQueue = new CleanupTaskQueue();
				IHostedServiceQueueProvider provider = new CleanupTaskQueue();
				CdcDatabase.Enable(testConnection, Db.DatabaseName);
				var scanner = new OnlineCdcScanner(new LoggerForTest());

				int offset = Convert.ToInt32((DateTime.UtcNow - DateTime.Now).TotalDays);
				string sqlText = @"INSERT INTO cdc.lsn_time_mapping (start_lsn, tran_end_time)
                                VALUES (0x00000000200000000001, DATEADD(DAY, 630, GETDATE()))";
				testConnection.Command(sqlText).ExecuteNonQuery();

				cleanupTask.RunTask();
				var queueSize = provider.QueueResult.QueueSize;

				AssertGreaterThanOrEqualTo(queueSize, 0);
			}
		}

		class CleanupTaskForTesting : CleanupTask
		{
			public CleanupTaskForTesting(bool isRetentionPeriodEnforcedForTesting = false)
			{
				logger = new LoggerForTest();
				ServiceLogger = logger;
				IsAuditServerSet = true;
				IsDataWarehouseServerSet = true;
				AuditDbExists = true;
				EdwDbExists = true;
				AuditEtlRunning = true;
				EdwEtlRunning = true;
				SupportCleanupFailedParameter = true;
				IsCleanupProcedureWithoutCleanupFailedParameterCalled = false;
				UnprocessedRowCountDecreasesForTest = false;
				ThrowLockRequestTimeoutException = false;
				ThrowRequestTimeoutException = false;
				ThrowFailedCaptureInstanceException = false;
				ThrowIncorrectNumberOfParametersException = false;

				IsRetentionPeriodEnforcedForTesting = isRetentionPeriodEnforcedForTesting;
			}
			public bool IsAuditServerSet;
			public bool IsDataWarehouseServerSet;
			public bool AuditDbExists;
			public bool EdwDbExists;
			public bool AuditEtlRunning;
			public bool EdwEtlRunning;
			public bool UnprocessedRowCountDecreasesForTest;
			public bool ThrowLockRequestTimeoutException;
			public bool ThrowRequestTimeoutException;
			public bool ThrowFailedCaptureInstanceException;
			public bool ThrowIncorrectNumberOfParametersException;
			public bool SupportCleanupFailedParameter;
			public bool IsCleanupProcedureWithoutCleanupFailedParameterCalled;
			public DbConnection testMainDbconnection { get => base.MainDbConnection; set => base.MainDbConnection = value; }
			public static DbConnection GetMainDbConnecionForTest() => Db.NewExtraUnrestrictedWriterConnection(Db.ServerName, Db.DatabaseName);

			readonly LoggerForTest logger;
			readonly bool IsRetentionPeriodEnforcedForTesting;

			public IEnumerable<string> Logs
			{
				get
				{
					return logger.LogEntries;
				}
			}

			public byte[] GetLsnByTimeOffset_Exposed(byte[] minLSN, TimeSpan timespan)
			{
				return GetLsnByTimeOffset(minLSN, timespan);
			}

			protected override string AuditServer
			{
				get
				{
					return IsAuditServerSet ? base.AuditServer : null;
				}
			}

			protected override string DataWarehouseServer
			{
				get
				{
					return IsDataWarehouseServerSet ? base.DataWarehouseServer : null;
				}
			}

			internal override TimeSpan CDCRetentionPeriod
			{
				get
				{
					return IsRetentionPeriodEnforcedForTesting ? base.CDCRetentionPeriod : TimeSpan.Zero;
				}
			}

			public TimeSpan CDCRetentionPeriodExposed => CDCRetentionPeriod;
			internal override int defaultCommandTimeoutInSeconds => 1;

			public int NumberOfTimesCleanupProcedureCalled;

			protected override bool ExecuteCleanupProcedure(string captureInstance, byte[] lowWaterMark, int deleteThreshold)
			{
				NumberOfTimesCleanupProcedureCalled++;
				if (ThrowLockRequestTimeoutException)
				{
					throw AdoTestUtils.GetSqlException(1222, "Lock request time out period exceeded.", Db.Connection);
				}
				else if (ThrowFailedCaptureInstanceException)
				{
					base.ExecuteCleanupProcedure(captureInstance, lowWaterMark, deleteThreshold);
					throw new CdcCleanupProcedureFailedException("", null, 0, 0);
				}
				else
				{
					bool result = base.ExecuteCleanupProcedure(captureInstance, lowWaterMark, deleteThreshold);
					if (ThrowRequestTimeoutException)
					{
						throw AdoTestUtils.GetSqlException(-2, "Server is taking too long to respond. Please try again.", Db.Connection);
					}
					return result;
				}
			}

			protected override bool ExecuteCleanupProcedure_Unsafe(string captureInstance, byte[] lowWaterMark, int deleteThreshold, bool withCleanupFailedParameter)
			{
				if (ThrowIncorrectNumberOfParametersException)
				{
					if (withCleanupFailedParameter)
					{
						SupportCleanupFailedParameter = false;
						throw AdoTestUtils.GetSqlException(8144, "Procedure or function sp_cdc_cleanup_change_table has too many arguments specified.", Db.Connection);
					}
					else
					{
						IsCleanupProcedureWithoutCleanupFailedParameterCalled = true;
						return true;
					}
				}
				else
				{
					return base.ExecuteCleanupProcedure_Unsafe(captureInstance, lowWaterMark, deleteThreshold, withCleanupFailedParameter);
				}
			}

			public int? FakeUnprocessedRowCount;
			protected override long GetUnprocessedRowCount(string captureInstance, byte[] waterMark)
			{
				if (UnprocessedRowCountDecreasesForTest && FakeUnprocessedRowCount > 0)
				{
					FakeUnprocessedRowCount--;
				}

				return FakeUnprocessedRowCount ?? base.GetUnprocessedRowCount(captureInstance, waterMark);
			}

			public void ExecuteCleanupProcedure_Exposed(string captureInstance, byte[] lowWaterMark)
			{
				ExecuteCleanupProcedure(captureInstance, lowWaterMark, defaultDeleteThreshold);
			}

			protected override byte[] GetAuditProcessedLsn(DbConnection auditConnection)
			{
				return AuditDbExists ? (AuditEtlRunning ? GetMaxLsn() : zeroLsn) : null;
			}

			protected override byte[] GetEdwProcessedLsn(DbConnection dwConnection)
			{
				return EdwDbExists ? (EdwEtlRunning ? GetMaxLsn() : zeroLsn) : null;
			}

			byte[] GetMaxLsn()
			{
				var sqlText = @"SELECT sys.fn_cdc_get_max_lsn()";
				return (byte[])Db.Connection.ExecuteScalar(sqlText);
			}

			readonly byte[] zeroLsn = new byte[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 };
		}
	}
}
