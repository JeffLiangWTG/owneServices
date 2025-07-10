using System;
using System.Data;
using System.Globalization;
using System.Threading;
using CargoWise.Application;
using CargoWise.Bi.Common.Integration;
using CargoWise.Bi.Product.DataLoad;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Foundation;
using CargoWise.Schema;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.ChangeDataCapture.Common.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	sealed class BusinessIntelligenceUpgradeEtlRunnerTest : TestCase
	{
		public void TestMonitorScansLog_WisecloudHosted()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var upgPreparation = new BusinessIntelligenceUpgradeEtlRunnerForTest(connection);
				using (upgPreparation.SetIsSelfHostedFlagForTest(false))
				{
					var logger = new DummyLoggerForTest();
					var scanner = new CdcScannerForTest_SkipsScan(logger);
					upgPreparation.CaptureAllChanges_Exposed(logger, scanner);
					Assert(
						$"Wisecloud hosted customers should NOT log a suggestion to monitor the scan.\r\nActual logs:\r\n{String.Join("\r\n\t", logger.Logs)}",
						!logger.Logs.Contains(
							"Please consider monitoring CDC scan progress with sys.dm_cdc_log_scan_sessions for more details."
						)
					);
				}
			}
		}

		public void TestMonitorScansLog_SelfHosted()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var upgPreparation = new BusinessIntelligenceUpgradeEtlRunnerForTest(connection);
				using (upgPreparation.SetIsSelfHostedFlagForTest(true))
				{
					var logger = new DummyLoggerForTest();
					var scanner = new CdcScannerForTest_SkipsScan(logger);
					upgPreparation.CaptureAllChanges_Exposed(logger, scanner);
					Assert(
						$"Self hosted customers should log a suggestion to monitor the scan.\r\nActual logs:\r\n{string.Join("\r\n\t", logger.Logs)}",
						logger.Logs.Contains(
							"Please consider monitoring CDC scan progress with sys.dm_cdc_log_scan_sessions for more details."
						)
					);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestCaptureAllChangesCoreSetsCdcScannerTimeoutToInfinite()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var scanner = new CdcScannerForTest_SkipsScan(new DummyLoggerForTest());
				AssertEquals("Before the scan, the timeout should be the default", scanner.CdcScanProvider.DefaultCommandTimeout, scanner.CdcScanProvider.CommandTimeout);

				var upgPreparation = new BusinessIntelligenceUpgradeEtlRunnerForTest(connection);
				upgPreparation.CaptureAllChangesCore_Exposed(new DummyLoggerForTest(), scanner);
				AssertEquals("Before the scan, the timeout should be infinite", TimeSpan.FromSeconds(DbCommand.Timeout.Infinite), scanner.CdcScanProvider.CommandTimeout);
			}
		}

		[UseSnapshotProtection]
		public void TestWaitForAnotherCdcScanToFinishRunningSingleScan()
		{
			using (var connection = Db.NewAdminConnection())
			using (var anotherConnection = Db.NewAdminConnection())
			{
				EnableCdc(connection);
				anotherConnection.ExecuteNonQuery("EXEC [sys].[sp_cdc_scan]");

				try
				{
					var upgPreparation = new BusinessIntelligenceUpgradeEtlRunnerForTest(connection);
					upgPreparation.CapturePendingCdcChangesAndFlushToAuditDatabase(new DummyLoggerForTest());
					Fail("Expected incomplete CDC scan exception.");
				}
				catch (OdysseyException ex)
				{
					AssertEquals("Failed to capture and process pending change data\r\nLog Reader has not captured all data changes within designed time interval. It may be in suspended, not running, or busy state. Check the log reader logs and retry upgrade.", ex.Message);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestWaitForAnotherCdcScanToFinishRunningContinuousScan()
		{
			using (var connection = Db.NewAdminConnection())
			{
				EnableCdc(connection);

				var thread = new Thread(() =>
				{
					using (var anotherConnection = Db.NewAdminConnection())
					{
						try
						{
							anotherConnection.ExecuteNonQuery("EXEC [sys].[sp_cdc_scan] @continuous = 1");
						}
						catch { }
					}
				});
				thread.Start();
				Thread.Sleep(1000);

				var upgPreparation = new BusinessIntelligenceUpgradeEtlRunnerForTest(connection);
				upgPreparation.RunAuditEtl = false;
				var logger = new DummyLoggerForTest();

				upgPreparation.CapturePendingCdcChangesAndFlushToAuditDatabase(logger);
				AssertCollectionContains("Log Reader has captured the all the data changes. It is now safe to proceed with DB upgrade.", logger.Logs);
			}
		}

		[UseSnapshotProtection]
		public void TestWaitForAnotherCdcScanToFinishRunningContinuousScan_TempTableExists()
		{
			using (var connection = Db.NewAdminConnection())
			{
				EnableCdc(connection);

				var thread = new Thread(() =>
				{
					using (var anotherConnection = Db.NewAdminConnection())
					{
						try
						{
							anotherConnection.ExecuteNonQuery("EXEC [sys].[sp_cdc_scan] @continuous = 1");
						}
						catch { }
					}
				});
				thread.Start();
				Thread.Sleep(1000);

				var upgPreparation = new BusinessIntelligenceUpgradeEtlRunnerForTest(connection);
				upgPreparation.RunAuditEtl = false;

				connection.ExecuteNonQuery(string.Format("IF NOT EXISTS(SELECT NULL FROM sys.tables WHERE name = 'DBUPG-PENDING-CDC-MARK') CREATE TABLE [{0}].[DBUPG-PENDING-CDC-MARK] (PKCOL UNIQUEIDENTIFIER NOT NULL PRIMARY KEY)", Db.SqlDbOwnerSchema));
				var sqlText = "IF EXISTS (SELECT NULL FROM sys.tables WHERE name = 'DBUPG-PENDING-CDC-MARK') SELECT 1 ELSE SELECT 0";
				Assert("Temp Table should exist before run.", Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));

				var logger = new DummyLoggerForTest();
				upgPreparation.CapturePendingCdcChangesAndFlushToAuditDatabase(logger);
				AssertCollectionContains("Log Reader has captured the all the data changes. It is now safe to proceed with DB upgrade.", logger.Logs);

				sqlText = "IF NOT EXISTS (SELECT NULL FROM sys.tables WHERE name = 'DBUPG-PENDING-CDC-MARK') SELECT 1 ELSE SELECT 0";
				Assert("Temp Table should not exist after run.", Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
			}
		}

		[UseSnapshotProtection]
		public void TestWaitForAnotherCdcScanToFinishRunningContinuousScan_CaptureInstanceExists()
		{
			using (var connection = Db.NewAdminConnection())
			{
				EnableCdc(connection);

				var thread = new Thread(() =>
				{
					using (var anotherConnection = Db.NewAdminConnection())
					{
						try
						{
							anotherConnection.ExecuteNonQuery("EXEC [sys].[sp_cdc_scan] @continuous = 1");
						}
						catch { }
					}
				});
				thread.Start();
				Thread.Sleep(1000);

				var upgPreparation = new BusinessIntelligenceUpgradeEtlRunnerForTest(connection);
				upgPreparation.RunAuditEtl = false;

				var sqlText = string.Format(@"
IF NOT EXISTS(SELECT NULL FROM sys.tables WHERE name = 'DBUPG-PENDING-CDC-MARK')
	CREATE TABLE [{0}].[DBUPG-PENDING-CDC-MARK] (PKCOL UNIQUEIDENTIFIER NOT NULL PRIMARY KEY)
EXEC sys.sp_cdc_enable_table @source_schema = '{0}', @source_name = 'DBUPG-PENDING-CDC-MARK', @capture_instance = '{0}_DBUPG-PENDING-CDC-MARK', @role_name = null
DROP TABLE [{0}].[DBUPG-PENDING-CDC-MARK]", Db.SqlDbOwnerSchema);
				connection.ExecuteNonQuery(sqlText);

				sqlText = "IF NOT EXISTS (SELECT NULL FROM sys.tables WHERE name = 'DBUPG-PENDING-CDC-MARK') SELECT 1 ELSE SELECT 0";
				Assert("Temp Table should not exist before run.", Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));

				sqlText = string.Format("IF EXISTS (SELECT NULL FROM sys.tables WHERE name = '{0}_DBUPG-PENDING-CDC-MARK_CT') SELECT 1 ELSE SELECT 0", Db.SqlDbOwnerSchema);
				Assert("Capture instance should exist before run.", Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));

				var logger = new DummyLoggerForTest();
				upgPreparation.CapturePendingCdcChangesAndFlushToAuditDatabase(logger);
				AssertCollectionContains("Log Reader has captured the all the data changes. It is now safe to proceed with DB upgrade.", logger.Logs);

				sqlText = "IF NOT EXISTS (SELECT NULL FROM sys.tables WHERE name = 'DBUPG-PENDING-CDC-MARK') SELECT 1 ELSE SELECT 0";
				Assert("Temp Table should should not exist after run.", Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));

				sqlText = string.Format("IF NOT EXISTS (SELECT NULL FROM sys.tables WHERE name = '{0}_DBUPG-PENDING-CDC-MARK_CT') SELECT 1 ELSE SELECT 0", Db.SqlDbOwnerSchema);
				Assert("Capture instance should not exist after run.", Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
			}
		}

		[UseSnapshotProtection]
		public void TestCdcScanLogsDuringUpgrade()
		{
			using (var connection = Db.NewAdminConnection())
			{
				EnableCdc(connection);
				var testLogger = new DummyLoggerForTest();
				var upgPreparation = new BusinessIntelligenceUpgradeEtlRunnerForTest(connection);

				connection.ExecuteNonQuery("CREATE TABLE [dbo].[TestTable] (c1 int, c2 int, c3 int)");
				var cdcTable = new CdcTableForTesting("dbo", "TestTable");
				cdcTable.EnableCdc(connection);

				connection.ExecuteNonQuery("INSERT INTO [dbo].[TestTable] (c1, c2, c3) VALUES (1, 1, 1)");
				upgPreparation.CapturePendingCdcChangesAndFlushToAuditDatabase(testLogger);

				var endDate = (DateTime)connection.ExecuteScalar("SELECT TOP 1 end_time FROM sys.dm_cdc_log_scan_sessions WHERE last_commit_lsn <> 0x0 ORDER BY session_id");
				var endLsn = (string)connection.ExecuteScalar("SELECT TOP 1 last_commit_lsn FROM sys.dm_cdc_log_scan_sessions WHERE last_commit_lsn <> 0x0 ORDER BY session_id");
				endLsn = endLsn.Replace(":", string.Empty);

				var endScanMessage = $"Completed capturing changes for transactions up to LSN: 0x{endLsn}, Transaction Date (UTC): {endDate.ToUniversalTime()}";

				AssertCollectionContains($"Logs should contain {endScanMessage}.\r\nLogs:\r\n{string.Join("\r\n", testLogger.Logs)}", endScanMessage, testLogger.Logs);
			}
		}

		[UseSnapshotProtection]
		public void TestAuditLockedByTupleMover_LogRowGroupState()
		{
			using (var connection = Db.NewAdminConnection())
			{
				EnableCdc(connection);
				var upgPreparation = new BusinessIntelligenceUpgradeEtlRunnerForTest(connection);
				upgPreparation.RunAuditEtl = false;
				var logger = new DummyLoggerForTest();

				// TupleMover is not running
				var mock = new Mock<IBiColumnstoreRowGroupHelper>();
				var dt = new DataTable();
				mock.Setup(b => b.GetColumnstoreRowGroupPhysicalState(connection)).Returns(dt);
				using (ObjectFactory.Substitute(mock.Object))
				{
					upgPreparation.CapturePendingCdcChangesAndFlushToAuditDatabase(logger);

					var firstMessage = $"The following tables residing in the Audit DB are pending deltastore rowgroup compression. If the upgrade fails, please try again:";
					AssertCollectionNotContains($"Logs should contain: '{firstMessage}'", firstMessage, logger.Logs);
				}

				// TupleMover is running
				dt.Columns.Add("TableName");
				dt.Columns.Add("RowGroupState");
				dt.Rows.Add(new object[] { "TupleMover_Test", "CLOSED" });
				dt.Rows.Add(new object[] { "TupleMover_Test2", "TUPLE_MOVER" });
				mock.Setup(b => b.GetColumnstoreRowGroupPhysicalState(connection)).Returns(dt);
				using (ObjectFactory.Substitute(mock.Object))
				{
					upgPreparation.CapturePendingCdcChangesAndFlushToAuditDatabase(logger);

					var firstMessage = $"The following tables residing in the Audit DB are pending deltastore rowgroup compression. If the upgrade fails, please try again:";
					AssertCollectionContains($"Logs should contain: '{firstMessage}'", firstMessage, logger.Logs);
					var secondMessage = $"TupleMover_Test has a rowgroup state of CLOSED";
					AssertCollectionContains($"Logs should contain: '{secondMessage}'", secondMessage, logger.Logs);
					var thirdMessage = $"TupleMover_Test2 has a rowgroup state of TUPLE_MOVER";
					AssertCollectionContains($"Logs should contain: '{thirdMessage}'", thirdMessage, logger.Logs);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestEnableTraceFlagLog()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					EnableCdc(connection);

					var testLogger = new DummyLoggerForTest();
					var upgPreparation = new BusinessIntelligenceUpgradeEtlRunnerForTest(connection);

					upgPreparation.ThrowSqlExceptionForTest = true;
					EnvProxy.SetHostedLocationForTest("NCW");

					upgPreparation.CapturePendingCdcChangesAndFlushToAuditDatabase(testLogger);

					var expectedMessage = "This database does not permit CDC Allow Alter Meta Data - Please enable TF15006. Please search for Technical Advisory Notes for more information.";
					AssertCollectionContains($"Logs should contain: '{expectedMessage}'", expectedMessage, testLogger.Logs);
				}
				finally
				{
					EnvProxy.SetHostedLocationForTest(null);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestMandatoryTraceFlagMissingException()
		{
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					var expectedMessage = "This database does not permit CDC Allow Alter Meta Data - Please enable TF15006. Please search for Technical Advisory Notes for more information.";
					EnableCdc(connection);

					var testLogger = new DummyLoggerForTest();
					var testScanner = new CdcScannerForTest_SkipsScan(testLogger);
					var upgPreparation = new BusinessIntelligenceUpgradeEtlRunnerForTest(connection);

					upgPreparation.ThrowSqlExceptionForTest = true;
					EnvProxy.SetHostedLocationForTest("SYD");

					Assert(EnvProxy.IsHostedWithCargowise);
					AssertExceptionThrown<MandatoryTraceFlagMissingException>(
						"MandatoryTraceFlagMissingException should be thrown when trace flag needs to be enabled",
						expectedMessage,
						() => upgPreparation.CaptureAllChanges_Exposed(testLogger, testScanner));

					EnvProxy.SetHostedLocationForTest("NCW");

					Assert(!EnvProxy.IsHostedWithCargowise);
					AssertNoExceptionThrown(() => upgPreparation.CaptureAllChanges_Exposed(testLogger, testScanner));
				}
				finally
				{
					EnvProxy.SetHostedLocationForTest(null);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestCdcScanWhenEndLsnIsNotNullable()
		{
			using (var connection = Db.NewAdminConnection())
			{
				EnableCdc(connection);
				var testLogger = new DummyLoggerForTest();
				var upgPreparation = new BusinessIntelligenceUpgradeEtlRunnerForTest(connection);

				connection.ExecuteNonQuery("CREATE TABLE [dbo].[TestTable] (c1 int, c2 int, c3 int)");
				var cdcTable = new CdcTableForTesting("dbo", "TestTable");
				cdcTable.EnableCdc(connection);
				connection.ExecuteNonQuery("ALTER TABLE cdc.dbo_TestTable_CT ALTER COLUMN __$end_lsn binary(10) NOT NULL");

				connection.ExecuteNonQuery("INSERT INTO dbo.TestTable SELECT 1, 2, 3");

				var scanner = new CdcScannerForTest(null, useAdminConnection: true);

				upgPreparation.CaptureAllChanges_Exposed(testLogger, scanner);

				Assert("Change data exists", connection.Exists("FROM cdc.dbo_TestTable_CT"));
				AssertEquals("__$end_lsn column should be nullable", true, Convert.ToBoolean(connection.ExecuteScalar("SELECT is_nullable FROM sys.columns WHERE object_name(object_id) = 'dbo_TestTable_CT' and name = '__$end_lsn'")));
			}
		}

		void EnableCdc(AdminConnection connection)
		{
			if (!CdcDatabase.IsEnabled(connection, Db.DatabaseName))
			{
				CdcDatabase.Enable(connection, Db.DatabaseName);
			}

			var cdcTable = new CdcTable(Db.SqlDbOwnerSchema, "StmData");
			if (!cdcTable.IsCdcEnabled(connection))
			{
				cdcTable.EnableCdc(connection);
			}
		}

		sealed class BusinessIntelligenceUpgradeEtlRunnerForTest : BusinessIntelligenceUpgradeEtlRunner
		{
			public BusinessIntelligenceUpgradeEtlRunnerForTest(AdminConnection connection)
				: base(connection, connection)
			{
				this.connection = connection;
				RunAuditEtl = true;
			}

			readonly AdminConnection connection;
			public bool ThrowSqlExceptionForTest;

			public IDisposable SetIsSelfHostedFlagForTest(bool value)
			{
				this.isSelfHosted = value;

				return new DisposableAction(new Action(() =>
				{
					isSelfHosted = null;
				}));
			}

			public bool RunAuditEtl { get; set; }

			internal override IEtlAuditExecution GetEtlExecutionManager(IUpgradeTaskWorkflowLogger logger)
			{
				if (RunAuditEtl)
				{
					var manager = EtlExecutionManagerFactory.NewForUpgrade(connection, connection, new LoggerWrapper(logger));
					return manager;
				}
				else
				{
					return null;
				}
			}

			public void CaptureAllChangesCore_Exposed(IUpgradeTaskWorkflowLogger logger, CdcScanner scanner)
			{
				this.CaptureAllChangesCore(logger, scanner);
			}

			internal void CaptureAllChanges_Exposed(IUpgradeTaskWorkflowLogger logger, CdcScanner scanner)
			{
				CaptureAllChanges(logger, scanner);
			}

			protected override void CaptureAllChangesCore(IUpgradeTaskWorkflowLogger logger, CdcScanner scanner)
			{
				if (ThrowSqlExceptionForTest)
				{
					var objectId = new Random().Next(1000000000, 2000000000).ToString();
					var enableTraceFlagException = SqlExceptionBuilder.CreateSqlError(3764, 1, 1, Db.Connection.ServerName, $"Cannot alter the procedure 'cdc.sp_batchinsert_{objectId}' because it is being used for Change Data Capture.", "", 1);
					throw SqlExceptionBuilder.CreateSqlException(enableTraceFlagException);
				}
				else
				{
					base.CaptureAllChangesCore(logger, scanner);
				}
			}

			protected override int WaitTimeInSeconds
			{
				get { return 5; }
			}
		}

		sealed class CdcScannerForTest_SkipsScan : CdcScanner
		{
			public CdcScannerForTest_SkipsScan(IUpgradeTaskWorkflowLogger logger)
			{
				CdcScannerLogger = new OfflineCdcScannerLogger(logger);
			}

			public override long ScanUntilNoTransactionsToProcess()
			{
				return ScanUntilNoTransactionsToProcess(shouldAbort: () => true);
			}
		}
	}
}
