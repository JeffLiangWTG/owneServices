using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Bi.Common.Testing;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Bi.Maintenance;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.ChangeDataCapture.Common.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Moq;
using NUnit.Framework;
using static CargoWise.Bi.Product.DataLoad.TsqlScriptRunner;

namespace CargoWise.Bi.Product.DataLoad.Testing
{
	class AuditTsqlScriptRunnerTest : TestCase
	{
		public void TestErrorMessagesArePopulated()
		{
			var errorList = new List<EtlError>();
			var errorMessagesArePopulated = AuditTsqlScriptRunner.ErrorMessagesArePopulated(errorList);
			Assert("Empty list has no populated error messages", !errorMessagesArePopulated);

			errorList.Add(new EtlError("table1", "", DateTime.UtcNow));
			errorList.Add(new EtlError("table2", null, DateTime.UtcNow));
			errorMessagesArePopulated = AuditTsqlScriptRunner.ErrorMessagesArePopulated(errorList);
			Assert("List still has no populated error messages", !errorMessagesArePopulated);

			errorList.Add(new EtlError("table2", "error message", DateTime.UtcNow));
			errorMessagesArePopulated = AuditTsqlScriptRunner.ErrorMessagesArePopulated(errorList);
			Assert("List has been populated with error messages", errorMessagesArePopulated);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Audit })]
		public void TestGetErrorMessagesFromMasterState()
		{
			using (var mainDbConnection = Db.NewAdminConnection())
			{
				var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(mainDbConnection);

				using (var auditDbConnection = Db.NewAdminConnection(auditServer, Db.AuditDatabaseName))
				{
					using (BiTemporaryMasterState.SetParameterTemporaryValue(auditDbConnection, BiConstants.LastEtlErrorMessage, "bingbong"))
					{
						var sqlText = $"UPDATE [{BiConstants.BiAdminSchemaName}].TableState SET SqlErrorMessage = '', SqlErrorDatetimeUTC = GETUTCDATE()";
						auditDbConnection.ExecuteNonQuery(sqlText);

						var testLogger = new LoggerForTest();
						var auditScriptRunnerForTest = new AuditTsqlScriptRunnerForTest(auditDbConnection, testLogger);
						var errorList = auditScriptRunnerForTest.GetErrorList_Exposed();
						AssertEquals("Should only have the BiMasterStateError", 1, errorList.Count());
						AssertContains("bingbong", errorList.First());
					}
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestTablockLogger_QueryIsBlocked()
		{
			var auditDbName = Db.AuditDatabaseName;

			using (var mainDbConnection = Db.NewAdminConnection())
			{
				var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(mainDbConnection);

				using (var auditDbConnection = Db.NewAdminConnection(auditServer, auditDbName))
				{
					EnableCdc(mainDbConnection);
					TruncateTables(mainDbConnection, auditDbName);
					CreateTestTable(mainDbConnection, auditDbName, testTableName);

					var pk = Guid.NewGuid();

					InsertRow(mainDbConnection, pk);
					UpdateRow(mainDbConnection, pk);
					DeleteRow(mainDbConnection, pk);

					var scanner = new CdcScannerForTest();
					scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);

					using (var blockingConnection = Db.NewAdminConnection(auditServer, auditDbName))
					using (var transactionManager = blockingConnection.BeginTransactionWithManager())
					{
						try
						{
							var blockingQuery = $"SELECT top 1 * from {auditDbName}.{testSchemaName}.{testTableName} with(tablockx)";
							blockingConnection.ExecuteScalar(blockingQuery);

							using (var blockedConnection = Db.NewAdminConnection(auditServer, auditDbName))
							using (blockedConnection.TemporarySetLockTimeout(500))
							{
								var testLogger = new LoggerForTest();
								var auditScriptRunnerForTest = new AuditTsqlScriptRunnerForTest(blockedConnection, testLogger);
								auditScriptRunnerForTest.PerformMasterAuditLoad();
								AssertEquals("Actual Message:\r\n" + testLogger.LogEntries.ElementAt(2),
									$"Audit ETL failed due to a lock request timeout caused by the following query:\r\nSPID = {blockingConnection.SPID}\r\nQuery = {blockingQuery}",
									testLogger.LogEntries.ElementAt(2)
								);
							}
						}
						finally
						{
							transactionManager.RollbackTransaction();
						}
					}
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestNoTempTablesInAuditEtl()
		{
			var auditDbName = Db.AuditDatabaseName;

			using (var mainDbConnection = Db.NewAdminConnection())
			{
				var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(mainDbConnection);

				using (var auditDbConnection1 = Db.NewAdminConnection(auditServer, auditDbName))
				using (var auditDbConnection2 = Db.NewAdminConnection(auditServer, auditDbName))
				{
					EnableCdc(mainDbConnection);
					TruncateTables(mainDbConnection, auditDbName);
					CreateTestTable(mainDbConnection, auditDbName, testTableName);

					var scanner = new CdcScannerForTest();
					var testLogger = new LoggerForTest();

					#region First AET execution

					var pk = Guid.NewGuid();
					InsertRow(mainDbConnection, pk);
					scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);

					var auditScriptRunnerForTest1 = new AuditTsqlScriptRunnerForTest(auditDbConnection1, testLogger);
					auditScriptRunnerForTest1.PerformMasterAuditLoad();

					#endregion

					#region Second AET execution

					UpdateRow(mainDbConnection, pk);
					scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);

					var auditScriptRunnerForTest2 = new AuditTsqlScriptRunnerForTest(auditDbConnection2, testLogger);
					auditScriptRunnerForTest2.PerformMasterAuditLoad();

					#endregion

					AssertEquals("Cached plans for Audit ETL", 1, GetCachedPlansForAuditEtl(auditDbConnection1, auditDbName));
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestAetHWMHistorySummaryLsnIsForIndividualTable()
		{
			var auditDbName = Db.AuditDatabaseName;
			var testTableNameLatestChange = testTableName + "WithNoChange";

			using (var mainDbConnection = Db.NewAdminConnection())
			{
				var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(mainDbConnection);

				using (var auditDbConnection = Db.NewAdminConnection(auditServer, auditDbName))
				{
					EnableCdc(mainDbConnection);
					TruncateTables(mainDbConnection, auditDbName);
					CreateTestTable(mainDbConnection, auditDbName, testTableName);
					CreateTestTable(mainDbConnection, auditDbName, testTableNameLatestChange);

					var scanner = new CdcScannerForTest();
					var testLogger = new LoggerForTest();

					var pk = Guid.NewGuid();
					var pk2 = Guid.NewGuid();
					InsertRow(mainDbConnection, pk);
					InsertRow(mainDbConnection, pk2, testSchemaName, testTableNameLatestChange);
					scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);

					var cdcHWM = mainDbConnection.ExecuteScalar<string>($"SELECT CONVERT(nvarchar(max), Lsn, 1) FROM cdc.CdcTables WHERE CaptureInstance = '{testSchemaName}_{testTableName}'");
					var cdcHWM2 = mainDbConnection.ExecuteScalar<string>($"SELECT CONVERT(nvarchar(max), Lsn, 1) FROM cdc.CdcTables WHERE CaptureInstance = '{testSchemaName}_{testTableNameLatestChange}'");

					AssertLessThan("Make sure the setup produces different LSN's per table", cdcHWM, cdcHWM2);

					var auditScriptRunnerForTest = new AuditTsqlScriptRunnerForTest(auditDbConnection, testLogger);
					auditScriptRunnerForTest.PerformMasterAuditLoad();

					var aetHWM = GetAetHWMHistorySummaryLsn(auditDbConnection, testSchemaName, testTableName);
					var aetHWM2 = GetAetHWMHistorySummaryLsn(auditDbConnection, testSchemaName, testTableNameLatestChange);

					AssertLessThan("The AetHWM should be the maximum for the table, not for all tables", aetHWM, aetHWM2);
				}
			}
		}

		bool IsBlocked(DbConnection connectionToUse, int targetSpid, string on)
		{
			var query = @$"
declare @obj_id nvarchar(max) = '%' + cast(object_id('{on}') as nvarchar(max)) + '%';
select top 1 1 from sys.dm_os_waiting_tasks where session_id = {targetSpid} and resource_description like @obj_id
";
			var result = connectionToUse.ExecuteScalar(query);
			return result != null; // result will have be 1 when there is a matching lock
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestAetTableAndGlobalWatermarksMatchWithChangesDuringProc()
		{
			using (var mainDbConnection = Db.NewAdminConnection())
			{
				EnableCdc(mainDbConnection);
				TruncateTables(mainDbConnection, Db.AuditDatabaseName);
				CreateTestTable(mainDbConnection, Db.AuditDatabaseName, testTableName);

				var scanner = new CdcScannerForTest();
				var testLogger = new LoggerForTest();

				// make some cdc changes and scan into change tables
				var pk = Guid.NewGuid();
				InsertRow(mainDbConnection, pk);
				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);

				using (var blockingTransaction = mainDbConnection.BeginTransactionWithManager())
				{
					//take lock on cdc.CdcTables in main db
					mainDbConnection.ExecuteNonQuery(@"
-- Save contents of CdcTables into a temp table
CREATE TABLE #tempCdcTables (CaptureInstance SYSNAME NOT NULL, Lsn BINARY(10) NOT NULL, LastDUProcessedLsn BINARY(10) DEFAULT 0x0000000000, PkColumnList VARCHAR(MAX) DEFAULT NULL);
INSERT INTO #tempCdcTables SELECT * FROM cdc.CdcTables;

-- Truncate cdc.CdcTables to take a schema modification lock on it
TRUNCATE TABLE cdc.CdcTables;
");

					int? blockedSpid = null;
					// execute audit master load asynchronously
					var task = Task.Run(() =>
					{
						using (var auditConnection = Db.NewAdminConnection(Db.AuditDatabaseName))
						{
							blockedSpid = auditConnection.SPID; // assignment is atomic
							var auditScriptRunnerForTest = new AuditTsqlScriptRunnerForTest(auditConnection, testLogger);
							auditScriptRunnerForTest.PerformMasterAuditLoad();
						}
					});

					// wait until audit master load gets blocked
					int remainingWaits = 5;
					while (blockedSpid == null || !IsBlocked(mainDbConnection, blockedSpid ?? -1, "cdc.CdcTables"))
					{
						Thread.Sleep(1000);
						if (remainingWaits-- == 0)
						{
							if (task.IsFaulted)
							{
								throw new Exception("Audit master load failed: ", task.Exception);
							}
							else
							{
								Fail("This test expects MasterAuditLoad to read from cdc.CdcTables");
							}
							return;
						}
					}

					// insert into change table cdc.ChangeTables and lsn time mapping to simulate a change being made and scanned
					mainDbConnection.ExecuteNonQuery(@"
-- Copy the previous contents of CdcTables back into it
INSERT INTO cdc.CdcTables SELECT * FROM #tempCdcTables;
-- Update all the tables high water mark lsn's to be new
UPDATE cdc.CdcTables SET Lsn = sys.fn_cdc_increment_lsn(sys.fn_cdc_get_max_lsn())");
					using (var adminConnection = Db.NewAdminConnection())
					{
						adminConnection.ExecuteNonQuery(@"
INSERT INTO cdc.lsn_time_mapping (start_lsn, tran_begin_time, tran_end_time, tran_id) VALUES (sys.fn_cdc_increment_lsn(sys.fn_cdc_get_max_lsn()), GETDATE(), DATEADD(second, -1, GETDATE()), 0x00);
");
					}

					// unblock audit load
					blockingTransaction.CommitTransaction();

					// wait for audit load to finish
					task.Wait();

					// assert that max of AetHWM == LAST_MAX_LSN_PROCESSED
					using (var auditConnection = Db.NewAdminConnection(Db.AuditDatabaseName))
					{
						var maxLsnFromTablesAetHWM = auditConnection.ExecuteScalar<byte[]>("select top 1 AetHWMHistorySummaryLsn from biadmin.TableState order by AetHWMHistorySummaryLsn desc");
						var maxLsnFromMasterState = BiMasterState.GetParameterAsByteArray(auditConnection, BiConstants.LastMaxLsnProcessed);
						AssertEquals(maxLsnFromMasterState, maxLsnFromTablesAetHWM);
					}
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestAetHWMHistorySummaryLsnInEachRunOfMasterAuditLoad()
		{
			var auditDbName = Db.AuditDatabaseName;
			var testTableNameNoChange = testTableName + "WithNoChange";

			using (var mainDbConnection = Db.NewAdminConnection())
			{
				var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(mainDbConnection);

				using (var auditDbConnection = Db.NewAdminConnection(auditServer, auditDbName))
				{
					EnableCdc(mainDbConnection);
					TruncateTables(mainDbConnection, auditDbName);
					CreateTestTable(mainDbConnection, auditDbName, testTableName);
					CreateTestTable(mainDbConnection, auditDbName, testTableNameNoChange);

					var scanner = new CdcScannerForTest();
					var testLogger = new LoggerForTest();

					#region First AET execution

					var pk = Guid.NewGuid();
					var pk2 = Guid.NewGuid();
					InsertRow(mainDbConnection, pk);
					InsertRow(mainDbConnection, pk2, testSchemaName, testTableNameNoChange);
					scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);

					var auditScriptRunnerForTest = new AuditTsqlScriptRunnerForTest(auditDbConnection, testLogger);
					auditScriptRunnerForTest.PerformMasterAuditLoad();

					var aetHWM = GetAetHWMHistorySummaryLsn(auditDbConnection, testSchemaName, testTableName);
					var aetHWM2 = GetAetHWMHistorySummaryLsn(auditDbConnection, testSchemaName, testTableNameNoChange);

					#endregion

					#region Second AET execution

					UpdateRow(mainDbConnection, pk);
					scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
					auditScriptRunnerForTest.PerformMasterAuditLoad();

					#endregion

					var newAetHWM = GetAetHWMHistorySummaryLsn(auditDbConnection, testSchemaName, testTableName);
					var newAetHWM2 = GetAetHWMHistorySummaryLsn(auditDbConnection, testSchemaName, testTableNameNoChange);

					AssertNotEquals("newAetHWM should not equals aetHWM", aetHWM, newAetHWM);
					AssertEquals("AetHWM should not change when there is no change", aetHWM2, newAetHWM2);
				}
			}
		}

		string GetAetHWMHistorySummaryLsn(DbConnection auditConnection, string sourceSchemaName, string sourceTableName)
		{
			var sqlText = $"SELECT CONVERT(nvarchar(max), AetHWMHistorySummaryLsn, 1) FROM biadmin.TableState WHERE SourceSchemaName = '{sourceSchemaName}' AND SourceTableName = '{sourceTableName}'";
			var result = auditConnection.ExecuteScalar(sqlText);
			AssertNotEquals("result should not be DBNull", result, DBNull.Value);
			return (string)result;
		}

		int GetCachedPlansForAuditEtl(DbConnection connection, string auditDbName)
		{
			var sqlText = $@"
SELECT count(*)
FROM 
    sys.dm_exec_cached_plans PLANS
    CROSS APPLY sys.dm_exec_sql_text (PLANS.plan_handle) AS Q
where [text] like '(@FromLsn BINARY(10), @ToLsn BINARY(10))%9223372036854775807%'
	and dbid = DB_ID('{auditDbName}')";

			return Convert.ToInt32(connection.ExecuteScalar(sqlText));
		}

		public void TestTimeout()
		{
			using (var mainDbConnection = Db.NewAdminConnection())
			{
				var logger = new LoggerForTest();
				var auditTsqlScriptRunner = new AuditTsqlScriptRunnerForTest(mainDbConnection, logger);
				AssertEquals("6 hour AET timeout", (int)TimeSpan.FromHours(6).TotalSeconds, auditTsqlScriptRunner.timeOutExposed);
			}
		}

		public void TestIsLinkedServerCreationRequired()
		{
			CombineAssertions(() =>
			{
				Test(7202, true, "Could not find server 'LOCALHOST' in sys.servers");
				Test(7411, true, "Error message... Server '127.0.0.1-db' is not configured  for DATA ACCESS...continued error message");
				Test(7416, true, "Access to the remote server is denied because no login-mapping exists.");
				Test(7303, true, "Cannot initialize the data source object of OLE DB provider \"%ls\" for linked server \"%ls\".");
			});

			void Test(int sqlErrorNumber, bool expected, string exceptionMessage)
			{
				using (var mainDbConnection = Db.NewAdminConnection())
				{
					var logger = new LoggerForTest();
					var auditTsqlScriptRunner = new AuditTsqlScriptRunner(mainDbConnection, logger);
					AssertEquals($"Creation of LinkedServer is required for this error.{System.Environment.NewLine}sqlErrorNumber:{sqlErrorNumber}{System.Environment.NewLine}exceptionMessage:{exceptionMessage}", expected, auditTsqlScriptRunner.IsLinkedServerCreationRequired(SqlExceptionBuilder.CreateSqlException(sqlErrorNumber, exceptionMessage)));
				}
			}
		}

		public void TestOnlyCreateLinkedServerIfNotWTGInternalSystem()
		{
			// Create linked server
			var keyMock = new Mock<IProductRegistrationKey>();
			var productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.SetupGet(r => r.Key).Returns(keyMock.Object);
			productRegistrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(false);
			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			{
				keyMock.Setup(k => k.HostedLocation).Returns("SYD");
				using (var mainDbConnection = Db.NewAdminConnection())
				{
					var logger = new LoggerForTest();
					var auditTsqlScriptRunner = new AuditTsqlScriptRunner(mainDbConnection, logger);
					var retry = auditTsqlScriptRunner.CreateLinkedServerAndRetry();
					AssertEquals(true, retry);
					Assert("Expected log: 'Creating linked server'", logger.LogEntries.Any(l => l.Contains("Creating linked server")));
				}
			}

			// Skip linked server creation
			productRegistrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(true);

			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			{
				keyMock.Setup(k => k.HostedLocation).Returns("SYD");
				using (var mainDbConnection = Db.NewAdminConnection())
				{
					var logger = new LoggerForTest();
					var auditTsqlScriptRunner = new AuditTsqlScriptRunner(mainDbConnection, logger);
					var retry = auditTsqlScriptRunner.CreateLinkedServerAndRetry();
					AssertEquals(false, retry);
					Assert("Expected log: 'Skip linked server creation'", logger.LogEntries.Any(l => l.Contains("Skip linked server creation for WTG internal systems")));
				}
			}

			productRegistrationMock.Setup(r => r.IsWiseTechGlobalInternalSystem()).Returns(true);
			productRegistrationMock.Setup(r => r.IsWiseTechGlobalInternalEDISystem()).Returns(true);
			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			{
				keyMock.Setup(k => k.HostedLocation).Returns("SYD");
				using (var mainDbConnection = Db.NewAdminConnection())
				{
					var logger = new LoggerForTest();
					var auditTsqlScriptRunner = new AuditTsqlScriptRunner(mainDbConnection, logger);
					var retry = auditTsqlScriptRunner.CreateLinkedServerAndRetry();
					AssertEquals(true, retry);
					Assert("Expected log: 'Creating linked server'", logger.LogEntries.Any(l => l.Contains("Creating linked server")));
				}
			}
		}

		public void TestRecreateLinkedServerWhenPortNumberIncorrect()
		{
			var logger = new LoggerForTest();
			var sqlException = SqlExceptionBuilder.CreateSqlException(1225, "A network-related or instance-specific error has occurred while establishing a connection to SQL Server. Server is not found or not accessible. Check if instance name is correct and if SQL Server is configured to allow remote connections. For more information see SQL Server Books Online.");
			SqlException actualEx = null;
			using (var mainDbConnection = Db.NewAdminConnection())
			{
				var serverName = "127.0.0.1";
				var linkedServerCreator = new LinkedServerCreatorForTest(mainDbConnection, serverName, logger);

				try
				{
					linkedServerCreator.DropLinkedServer();
					var createLinkedServerQuery = $@"
EXEC sp_addlinkedserver
   @server=N'{serverName}',
   @srvproduct=N'SQL_Server',
   @provider=N'MSOLEDBSQL',
   @provstr = 'User ID=Odyssey_UnrestrictedWriterLogin',
   @datasrc=N'{serverName},1453';";
					mainDbConnection.ExecuteNonQuery(createLinkedServerQuery);

					var auditScriptRunner = new AuditTsqlScriptRunnerForTest(mainDbConnection, logger);
					auditScriptRunner.exception = sqlException;
					auditScriptRunner.LinkedServerName_Exposed = serverName;
					try
					{
						auditScriptRunner.Run();
					}
					catch (SqlException sqlEx)
					{
						actualEx = sqlEx;
					}

					CombineAssertions(() =>
					{
						Assert("Linked server recreation required", logger.LogEntries.Contains($"Linked server recreation required"));
						Assert(logger.LogEntries.Contains($"Creating linked server [{serverName}]"));
						AssertEquals("Linked server created with correct port", expected: LinkedServerCreator.GetMainDBServerPortNumber(), LinkedServerCreator.GetServerPortNumber(serverName));
						Assert("Retry failed will rethrow exception.", actualEx?.ErrorCode == sqlException.ErrorCode);
					});
				}
				finally
				{
					linkedServerCreator.DropLinkedServer();
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRun()
		{
			var auditDbName = Db.AuditDatabaseName;

			using (var mainDbConnection = Db.NewAdminConnection())
			{
				EnableCdc(mainDbConnection);
				TruncateTables(mainDbConnection, auditDbName);
				CreateTestTable(mainDbConnection, auditDbName, testTableName);

				#region Master Audit Load

				var pk = Guid.NewGuid();

				InsertRow(mainDbConnection, pk);
				UpdateRow(mainDbConnection, pk);
				DeleteRow(mainDbConnection, pk);

				var scanner = new CdcScannerForTest();
				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);

				var logger = new LoggerForTest();
				var auditTsqlScriptRunner = new AuditTsqlScriptRunner(mainDbConnection, logger);
				auditTsqlScriptRunner.Run();

				CheckAuditTable(mainDbConnection, auditDbName);
				Assert("AET log start lsn", logger.LogEntries.Any(l => l.StartsWith("Last max Lsn processed:")));
				Assert("AET log end lsn", logger.LogEntries.Any(l => l.StartsWith("Audit ETL completed at Lsn:")));
				#endregion
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestExceptionNotThrown_CallingSPWithFQNFormat()
		{
			var auditDbName = Db.AuditDatabaseName;

			using (var mainDbConnection = Db.NewAdminConnection())
			{
				EnableCdc(mainDbConnection);
				TruncateTables(mainDbConnection, auditDbName);
				CreateTestTable(mainDbConnection, auditDbName, testTableName);

				#region Master Audit Load

				var pk = Guid.NewGuid();

				InsertRow(mainDbConnection, pk);
				UpdateRow(mainDbConnection, pk);
				DeleteRow(mainDbConnection, pk);

				var scanner = new CdcScannerForTest();
				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);

				var logger = new LoggerForTest();
				var auditTsqlScriptRunner = new AuditTsqlScriptRunnerForTest(mainDbConnection, logger);
				using (Db.NewExtraRestrictedWriterConnection(Db.ServerName, Db.SqlMasterDb))
				{
					AssertNoExceptionThrown("Stored procedures should be called with FQN format", () => auditTsqlScriptRunner.PerformMasterAuditLoad());
				}

				CheckAuditTable(mainDbConnection, auditDbName);

				#endregion
			}
		}

		[UseSnapshotProtection]
		public void TestCheckLsnTimeMappingTableExists()
		{
			using (var mainDbConnection = Db.NewAdminConnection())
			{
				EnableCdc(mainDbConnection);

				var sqlText = "SELECT start_lsn, tran_end_time FROM cdc.lsn_time_mapping";
				AssertNoExceptionThrown(() => mainDbConnection.ExecuteNonQuery(sqlText));
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestLsnTimeMappingCopiedToAudit()
		{
			var auditDbName = Db.AuditDatabaseName;

			using (var mainDbConnection = Db.NewAdminConnection())
			{
				EnableCdc(mainDbConnection);
				TruncateTables(mainDbConnection, auditDbName);

				var sqlText = "SELECT COUNT(*) FROM cdc.lsn_time_mapping";
				var initialLsnTimeMappingCount = Convert.ToInt32(mainDbConnection.ExecuteScalar(sqlText));

				CreateTestTable(mainDbConnection, auditDbName, testTableName);

				var scanner = new CdcScannerForTest();
				scanner.ScanUntilNoTransactionsToProcess();

				InsertRow(mainDbConnection, Guid.NewGuid());
				scanner.ScanUntilNoTransactionsToProcess();

				var logger = new LoggerForTest();
				var auditTsqlScriptRunner = new AuditTsqlScriptRunner(mainDbConnection, logger);
				auditTsqlScriptRunner.Run();

				var newLsnTimeMappingCount = Convert.ToInt32(mainDbConnection.ExecuteScalar(sqlText));

				using (((ICurrentDbControl)mainDbConnection).UseDatabase(auditDbName))
				{
					sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT COUNT(*) FROM [{0}].LsnTimeMapping", BiConstants.BiAdminSchemaName);
					var auditLsnTimeMappingCount = Convert.ToInt32(mainDbConnection.ExecuteScalar(sqlText));

					AssertEquals("Number of lsn time mappings copied to audit database:", newLsnTimeMappingCount - initialLsnTimeMappingCount, auditLsnTimeMappingCount);
				}
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestAuditTableXMLtoNvarCharDataMove()
		{
			var auditDbName = Db.AuditDatabaseName;

			using (var mainDbConnection = Db.NewAdminConnection())
			using (var biConnection = Db.NewAdminConnection(auditDbName))
			{
				EnableCdc(mainDbConnection);
				TruncateTables(biConnection, auditDbName);
				CreateTestTable(mainDbConnection, auditDbName, testTableName);
				InsertRow(mainDbConnection, new Guid());

				var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT COUNT(*) FROM [{0}].[{1}] where Column4Xml is not NULL", testSchemaName, testTableName);
				AssertEquals("Initial count of records with XML data column", 0, Convert.ToInt32(biConnection.ExecuteScalar(sqlText)));

				var scanner = new CdcScannerForTest();
				scanner.ScanUntilNoTransactionsToProcess();

				var logger = new LoggerForTest();
				var auditTsqlScriptRunner = new AuditTsqlScriptRunner(mainDbConnection, logger);
				auditTsqlScriptRunner.Run();

				AssertEquals("Final count of records with XML data column", 1, Convert.ToInt32(biConnection.ExecuteScalar(sqlText)));
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestAuditTableDataLoss()
		{
			var auditDbName = Db.AuditDatabaseName;

			using (var mainDbConnection = Db.NewAdminConnection())
			using (var biConnection = Db.NewAdminConnection(auditDbName))
			{
				EnableCdc(mainDbConnection);
				TruncateTables(biConnection, auditDbName);
				CreateTestTable(mainDbConnection, auditDbName, testTableName);
				InsertRow(mainDbConnection, new Guid());

				var scanner = new CdcScannerForTest();
				scanner.ScanUntilNoTransactionsToProcess();

				var logger = new LoggerForTest();
				var auditTsqlScriptRunner = new AuditTsqlScriptRunner(mainDbConnection, logger);
				auditTsqlScriptRunner.Run();

				var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT COUNT(*) FROM [{0}].DataLossLog", BiConstants.BiAdminSchemaName);
				AssertEquals("Initial count for data loss log", 0, Convert.ToInt32(biConnection.ExecuteScalar(sqlText)));

				var cdcTable = new CdcTable(testSchemaName, testTableName);
				cdcTable.DisableCdc(mainDbConnection, string.Format("{0}_{1}", testSchemaName, testTableName));
				cdcTable.EnableCdc(mainDbConnection, string.Format("{0}_{1}", testSchemaName, testTableName));

				scanner.ScanUntilNoTransactionsToProcess();
				auditTsqlScriptRunner.Run();

				AssertEquals("Number data loss log records", 1, Convert.ToInt32(biConnection.ExecuteScalar(sqlText)));
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestLsnPeriodPopulated()
		{
			var auditDbName = Db.AuditDatabaseName;

			using (var mainDbConnection = Db.NewAdminConnection())
			{
				EnableCdc(mainDbConnection);
				TruncateTables(mainDbConnection, auditDbName);
				CreateTestTable(mainDbConnection, auditDbName, testTableName);

				#region Master Audit Load

				var pk = Guid.NewGuid();
				var utcNow = DateTime.UtcNow;

				while (utcNow.Month != utcNow.AddSeconds(30).Month)
				{
					System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
					utcNow = DateTime.UtcNow;
				}

				int expectedLsnPeriod = ((utcNow.Year - 2000) * 100) + utcNow.Month;

				InsertRow(mainDbConnection, pk);

				var scanner = new CdcScannerForTest();
				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);

				var logger = new LoggerForTest();
				var auditTsqlScriptRunner = new AuditTsqlScriptRunner(mainDbConnection, logger);
				auditTsqlScriptRunner.Run();

				CheckLatestLsnPeriod(mainDbConnection, auditDbName, pk, expectedLsnPeriod);

				#endregion
			}
		}

		public void TestLsnPeriodExists()
		{
			var auditTables = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig
				.Where(t => t.TableInAudit)
				.Select(t => t.SourceSchema + "_" + t.SourceTable);

			var sqlText = string.Format(CultureInfo.InvariantCulture,
@"select s.name + '_' + t.name
from sys.tables t
inner join sys.schemas s
on t.schema_id = s.schema_id
left join sys.columns c
on t.object_id = c.object_id and c.name = '__$lsn_period'
where c.name is NULL and (s.name + '_' + t.name) in
(
	'{0}'
)",
				string.Join("',\r\n\t'", auditTables));

			using (var mainDbConnection = Db.NewAdminConnection())
			{
				var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(mainDbConnection);
				using (var biConnection = Db.NewAdminConnection(auditServer, Db.AuditDatabaseName))
				{
					var tablesWithNoLsnPeriod = DataUtils.GetListOfValuesFromQuery(biConnection, sqlText);

					Assert(string.Format("The following tables don't have __$lsn_period column:\r\n{0}", string.Join("\r\n", tablesWithNoLsnPeriod)), !tablesWithNoLsnPeriod.Any());
				}
			}
		}

		public void TestEnsureIndexNameConvention()
		{
			var sqlText = string.Format(@"
select tc.SourceSchemaName, tc.SourceTableName, i.name
from [{0}].[TableConfiguration] tc
inner join sys.indexes i
	on i.object_id = object_id(tc.SourceSchemaName + '.' + tc.SourceTableName)
where i.type = 5",
				BiConstants.BiAdminSchemaName);

			using (var biConnection = Db.NewAdminConnection(Db.AuditDatabaseName))
			using (var cmd = biConnection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				CombineAssertions(() =>
				{
					Assert(true);

					while (reader.Read())
					{
						var schema = reader.GetString(0);
						var table = reader.GetString(1);
						var indexName = reader.GetString(2);

						AssertEquals(string.Format("Clustered columnstore index name for [{0}].[{1}]", schema, table), string.Format("cci_{0}_{1}", schema, table), indexName);
					}
				});
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestLsnTimeMappingPopulationError()
		{
			var auditDbName = Db.AuditDatabaseName;

			using (var mainDbConnection = Db.NewAdminConnection())
			{
				EnableCdc(mainDbConnection);
				TruncateTables(mainDbConnection, auditDbName);
				CreateTestColumnStoreTable(mainDbConnection, auditDbName, testTableName);

				InsertRow(mainDbConnection, Guid.NewGuid());

				var scanner = new CdcScannerForTest();
				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);

				using (((ICurrentDbControl)mainDbConnection).UseDatabase(auditDbName))
				{
					mainDbConnection.ExecuteNonQuery("DROP TABLE [biadmin].[LsnTimeMapping]");
				}

				var logger = new LoggerForTest();
				var auditTsqlScriptRunner = new AuditTsqlScriptRunnerForTest(mainDbConnection, logger);
				auditTsqlScriptRunner.Run();

				CombineAssertions(string.Format("Actual logs:\r\n{0}", string.Join("\r\n", logger.LogEntries)), () =>
				{
					Assert("Expected log: 'Error while populating LsnTimeMapping table:'", logger.LogEntries.Any(l => l.Contains("Error while populating LsnTimeMapping table:")));
					Assert("Expected log: 'Invalid object name 'biadmin.LsnTimeMapping'.'", logger.LogEntries.Any(l => l.Contains("Invalid object name 'biadmin.LsnTimeMapping'.")));
				});
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRun_CorruptedIndex()
		{
			var auditDbName = Db.AuditDatabaseName;

			using (var mainDbConnection = Db.NewAdminConnection())
			{
				EnableCdc(mainDbConnection);
				TruncateTables(mainDbConnection, auditDbName);
				CreateTestColumnStoreTable(mainDbConnection, auditDbName, testTableName);

				#region Before corrupted index

				InsertRow(mainDbConnection, Guid.NewGuid());

				var scanner = new CdcScannerForTest();
				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);

				var logger = new LoggerForTest();
				var auditTsqlScriptRunner = new AuditTsqlScriptRunnerForTest(mainDbConnection, logger);
				auditTsqlScriptRunner.Run();

				AssertColumnStoreRowGroup(mainDbConnection, auditDbName, testTableName, ColumnStoreRowGroupState.Open);

				#endregion

				#region After corrupted index

				ModifyLoadAuditStoredProcedure(mainDbConnection, auditDbName, testTableName);

				InsertRow(mainDbConnection, Guid.NewGuid());

				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
				auditTsqlScriptRunner.Run();

				AssertColumnStoreRowGroup(mainDbConnection, auditDbName, testTableName, ColumnStoreRowGroupState.Compressed);

				#endregion
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRunInBatchMode()
		{
			var auditDbName = Db.AuditDatabaseName;

			using (var mainDbConnection = Db.NewAdminConnection())
			{
				EnableCdc(mainDbConnection);
				TruncateTables(mainDbConnection, auditDbName);
				CreateTestTable(mainDbConnection, auditDbName, testTableName);

				#region Master Audit Load

				var pk = Guid.NewGuid();

				InsertRow(mainDbConnection, pk, "TestValue1");
				Thread.Sleep(500);
				InsertRow(mainDbConnection, pk, "TestValue2");
				Thread.Sleep(500);
				InsertRow(mainDbConnection, pk, "TestValue3");

				var scanner = new CdcScannerForTest();
				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);

				var logger = new LoggerForTest();
				var auditTsqlScriptRunner = new AuditTsqlScriptRunnerForTest(mainDbConnection, logger);
				auditTsqlScriptRunner.BatchModeSize_Exposed = 3;
				auditTsqlScriptRunner.MaxIntervalMinutes_Exposed = 50.0f / (1000 * 60);
				auditTsqlScriptRunner.Run();

				AssertCollectionContains($"Expected Log: Audit ETL was executed in 3 batches.\r\nActual Logs:\r\n{string.Join("\r\n", logger.LogEntries)}", "Audit ETL was executed in 3 batches.", logger.LogEntries);

				using (((ICurrentDbControl)mainDbConnection).UseDatabase(auditDbName))
				{
					var query = string.Format(@"SELECT LTRIM(RTRIM(Column3Char)) FROM [{0}].[{1}]", testSchemaName, testTableName);
					var actualValues = DataUtils.GetListOfValuesFromQuery(mainDbConnection, query);

					var expectedValues = new[] { "TestValue1", "TestValue2", "TestValue3" };

					AssertContainsExactElementsInAnyOrder("Filtered values.", expectedValues, actualValues);
				}

				#endregion
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRunWithAuditFilter()
		{
			var auditDbName = Db.AuditDatabaseName;

			using (var mainDbConnection = Db.NewAdminConnection())
			{
				EnableCdc(mainDbConnection);
				TruncateTables(mainDbConnection, auditDbName);
				CreateTestTable(mainDbConnection, auditDbName, testTableName);

				using (((ICurrentDbControl)mainDbConnection).UseDatabase(auditDbName))
				{
					string sqlText = $"UPDATE [{BiConstants.BiAdminSchemaName}].[TableConfiguration] SET AuditFilter = 'Column3Char LIKE ''Included%'''";
					mainDbConnection.ExecuteNonQuery(sqlText);
				}

				#region Master Audit Load

				var pk = Guid.NewGuid();

				InsertRow(mainDbConnection, pk, "Included1");
				InsertRow(mainDbConnection, pk, "Included2");
				InsertRow(mainDbConnection, pk, "Excluded");

				var scanner = new CdcScannerForTest();
				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);

				var logger = new LoggerForTest();
				var auditTsqlScriptRunner = new AuditTsqlScriptRunner(mainDbConnection, logger);
				auditTsqlScriptRunner.Run();

				using (((ICurrentDbControl)mainDbConnection).UseDatabase(auditDbName))
				{
					var query = string.Format(@"SELECT LTRIM(RTRIM(Column3Char)) FROM [{0}].[{1}]", testSchemaName, testTableName);
					var actualValues = DataUtils.GetListOfValuesFromQuery(mainDbConnection, query);

					var expectedValues = new[] { "Included1", "Included2" };

					AssertContainsExactElementsInAnyOrder("Filtered values.", expectedValues, actualValues);
				}

				#endregion
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRunWithAuditFilterBatchMode()
		{
			var auditDbName = Db.AuditDatabaseName;

			using (var mainDbConnection = Db.NewAdminConnection())
			{
				EnableCdc(mainDbConnection);
				TruncateTables(mainDbConnection, auditDbName);
				CreateTestTable(mainDbConnection, auditDbName, testTableName);

				using (((ICurrentDbControl)mainDbConnection).UseDatabase(auditDbName))
				{
					string sqlText = $"UPDATE [{BiConstants.BiAdminSchemaName}].[TableConfiguration] SET AuditFilter = 'Column3Char LIKE ''Included%'''";
					mainDbConnection.ExecuteNonQuery(sqlText);
				}

				#region Master Audit Load

				var pk = Guid.NewGuid();

				InsertRow(mainDbConnection, pk, "Included1");
				Thread.Sleep(500);
				InsertRow(mainDbConnection, pk, "Included2");
				Thread.Sleep(500);
				InsertRow(mainDbConnection, pk, "Included3");
				Thread.Sleep(500);
				InsertRow(mainDbConnection, pk, "Excluded1");
				Thread.Sleep(500);
				InsertRow(mainDbConnection, pk, "Excluded2");
				Thread.Sleep(500);
				InsertRow(mainDbConnection, pk, "Excluded3");
				Thread.Sleep(500);
				InsertRow(mainDbConnection, pk, "Excluded4");
				Thread.Sleep(500);
				InsertRow(mainDbConnection, pk, "Excluded5");
				Thread.Sleep(500);
				InsertRow(mainDbConnection, pk, "Excluded6");
				Thread.Sleep(500);
				InsertRow(mainDbConnection, pk, "Included4");

				var scanner = new CdcScannerForTest();
				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);

				var logger = new LoggerForTest();
				var auditTsqlScriptRunner = new AuditTsqlScriptRunnerForTest(mainDbConnection, logger);
				auditTsqlScriptRunner.BatchModeSize_Exposed = 3;
				auditTsqlScriptRunner.MaxIntervalMinutes_Exposed = 50.0f / (1000 * 60);
				auditTsqlScriptRunner.Run();

				AssertCollectionContains($"Expected Log: Audit ETL was executed in 10 batches.\r\nActual Logs:\r\n{string.Join("\r\n", logger.LogEntries)}", "Audit ETL was executed in 10 batches.", logger.LogEntries);

				using (((ICurrentDbControl)mainDbConnection).UseDatabase(auditDbName))
				{
					var query = string.Format(@"SELECT LTRIM(RTRIM(Column3Char)) FROM [{0}].[{1}]", testSchemaName, testTableName);
					var actualValues = DataUtils.GetListOfValuesFromQuery(mainDbConnection, query);

					var expectedValues = new[] { "Included1", "Included2", "Included3", "Included4" };

					AssertContainsExactElementsInAnyOrder("Filtered values.", expectedValues, actualValues);
				}

				#endregion
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestPopulateCdcHistorySummary()
		{
			var auditDbName = Db.AuditDatabaseName;

			using (var mainDbConnection = Db.NewAdminConnection())
			{
				EnableCdc(mainDbConnection);
				TruncateTables(mainDbConnection, auditDbName);
				CreateTestTable(mainDbConnection, auditDbName, testTableName);

				#region Master Audit Load

				var pk1 = Guid.NewGuid();
				InsertRow(mainDbConnection, pk1);
				UpdateRow(mainDbConnection, pk1);
				DeleteRow(mainDbConnection, pk1);

				var scanner = new CdcScannerForTest();
				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);

				var logger = new LoggerForTest();
				var auditTsqlScriptRunner = new AuditTsqlScriptRunner(mainDbConnection, logger);
				auditTsqlScriptRunner.Run();

				CheckCdcHistorySummaryTable(mainDbConnection, auditDbName, testTableName, 4);

				var pk2 = Guid.NewGuid();
				InsertRow(mainDbConnection, pk2);
				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
				auditTsqlScriptRunner.Run();
				CheckCdcHistorySummaryTable(mainDbConnection, auditDbName, testTableName, 5);

				UpdateRow(mainDbConnection, pk2);
				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
				auditTsqlScriptRunner.Run();
				CheckCdcHistorySummaryTable(mainDbConnection, auditDbName, testTableName, 7);

				DeleteRow(mainDbConnection, pk2);
				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
				auditTsqlScriptRunner.Run();
				CheckCdcHistorySummaryTable(mainDbConnection, auditDbName, testTableName, 8);

				RunCdcHistorySummaryPopulator(mainDbConnection, auditDbName);
				CheckCompleteCdcHistorySummaryTable(mainDbConnection, auditDbName, testTableName);

				#endregion
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGetErrorList()
		{
			var auditDbName = Db.AuditDatabaseName;

			using (var mainDbConnection = Db.NewAdminConnection())
			{
				EnableCdc(mainDbConnection);
				TruncateTables(mainDbConnection, auditDbName);
				CreateTestTable(mainDbConnection, auditDbName, testTableName);
				RemoveColumnFromAuditTable(mainDbConnection, auditDbName, testTableName);

				#region Master Audit Load

				var pk = Guid.NewGuid();
				InsertRow(mainDbConnection, pk);

				var scanner = new CdcScannerForTest();
				scanner.ScanUntilNoTransactionsToProcess(shouldAbort: () => false);

				var logger = new LoggerForTest();
				try
				{
					var auditTsqlScriptRunner = new AuditTsqlScriptRunner(mainDbConnection, logger);
					auditTsqlScriptRunner.Run();
					Fail("EtlExecutionException expected.");
				}
				catch (EtlExecutionException e)
				{
					Assert("Error message should be invalid column name.", e.Message.Contains("Invalid column name 'Column3Char'."));
				}

				#endregion
			}
		}

		#region Implementation

		const string testSchemaName = "TestSchema";
		const string testTableName = "TestTable_AuditEtl";

		#region Assertions

		void CheckAuditTable(DbConnection connection, string auditDbName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				var query = string.Format(@"SELECT __$operation FROM [{0}].[{1}]", testSchemaName, testTableName);
				var operationList = DataUtils.GetListOfValuesFromQuery(connection, query);

				AssertCollectionContains("Audit table is missing Insert operation.", Convert.ToInt32(CdcOperation.Insert).ToString(), operationList);
				AssertCollectionContains("Audit table is missing Before Update operation.", Convert.ToInt32(CdcOperation.BeforeUpdate).ToString(), operationList);
				AssertCollectionContains("Audit table is missing After Update operation.", Convert.ToInt32(CdcOperation.AfterUpdate).ToString(), operationList);
				AssertCollectionContains("Audit table is missing Delete operation.", Convert.ToInt32(CdcOperation.Delete).ToString(), operationList);
			}
		}

		void CheckLatestLsnPeriod(DbConnection connection, string auditDbName, Guid pk, int expectedLsnPeriod)
		{
			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				var query = string.Format(@"SELECT __$lsn_period FROM [{0}].[{1}] WHERE Column1ID = '{2}'", testSchemaName, testTableName, pk.ToString());
				var actualLsnPeriod = Convert.ToInt32(connection.ExecuteScalar(query));

				AssertEquals("Incorrect LSN period", actualLsnPeriod, expectedLsnPeriod);
			}
		}

		void AssertColumnStoreRowGroup(DbConnection connection, string auditDbName, string testTableName, ColumnStoreRowGroupState expectedState)
		{
			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				var query = string.Format(@"SELECT state FROM sys.column_store_row_groups WHERE object_id = object_id('{0}.{1}')", testSchemaName, testTableName);
				var actualState = (ColumnStoreRowGroupState)Convert.ToInt32(connection.ExecuteScalar(query));

				AssertEquals("Incorrect column store row group state.", expectedState, actualState);
			}
		}

		enum ColumnStoreRowGroupState
		{
			Open = 1,
			Compressed = 3
		}

		void CheckCdcHistorySummaryTable(DbConnection connection, string auditDbName, string testTableName, int expectedNumberOfRows)
		{
			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture, "SELECT ISNULL(SUM(NumberOfRows), 0) FROM [{0}].[CdcHistorySummary] WHERE ChangedTableName = '{1}'", BiConstants.BiAdminSchemaName, testTableName);
				AssertEquals("Total number of rows for CDC History Summary", expectedNumberOfRows, Convert.ToInt32(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
			}
		}

		void CheckCompleteCdcHistorySummaryTable(DbConnection connection, string auditDbName, string testTableName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture, @"
;WITH CTE AS
(
	SELECT __$start_lsn AS Lsn, count(*) AS RowCnt FROM [{0}].[{1}] group by __$start_lsn
)
SELECT ISNULL(SUM(cte.RowCnt), 0)
FROM CTE cte
LEFT JOIN [{2}].[CdcHistorySummary] s ON s.Lsn = cte.Lsn
WHERE cte.RowCnt <> s.NumberOfRows OR s.NumberOfRows IS NULL", testSchemaName, testTableName, BiConstants.BiAdminSchemaName, testTableName);

				var numberOfMissingRows = Convert.ToInt32(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture);
				AssertEquals("Missing CDC History Summary for the following LSN values.", 0, numberOfMissingRows);
			}
		}

		#endregion

		#region Row Manipulations

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Test Cases")]
		void InsertAdmSchedule(DbConnection connection, bool isActive, DateTime dailyStartTimeUtc)
		{
			var sqlText = @"
delete from dbo.StmScheduleTask where S5_ScheduleType = 'ADM'

insert into dbo.StmScheduleTask(S5_PK, S5_ScheduleDescription, S5_ScheduleType, S5_TypeOfDocument, S5_GB, S5_TaskPeriod, S5_DailyStartTime, S5_ParentTableCode, S5_ParentID, S5_IsActive)
select newid(), 'Audit Database Maintenance', 'ADM', 'BI', @BranchPk, 'D', @DailyStartTime, 'SH', null, @IsActive";

			var dailyStartTimeLocal = Env.Time.GetLocalTimeFromUtc(dailyStartTimeUtc);

			using (((ICurrentDbControl)connection).UseDatabase(Db.DatabaseName))
			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@BranchPk", SqlDbType.UniqueIdentifier, Env.CurrentBranchPK);
				cmd.AddParameter("@IsActive", SqlDbType.Bit, isActive);
				cmd.AddParameter("@DailyStartTime", SqlDbType.DateTime, dailyStartTimeLocal);

				cmd.ExecuteNonQuery();
			}
		}

		void CreateTestTable(AdminConnection connection, string auditDbName, string testTableName)
		{
			var sqlText = @"
IF NOT EXISTS(SELECT null FROM sys.schemas WHERE name = '{0}')
	EXEC ('CREATE SCHEMA [{0}]');

IF EXISTS (SELECT null FROM sys.tables t inner join sys.schemas s on t.schema_id = s.schema_id WHERE t.name = '{1}' and s.name = '{0}')
	DROP TABLE [{0}].[{1}]
CREATE TABLE [{0}].[{1}] (
	Column1ID uniqueidentifier,
	Column2Int int,
	Column3Char char(10),
	Column4Xml xml
)
";

			connection.ExecuteNonQuery(string.Format(sqlText, testSchemaName, testTableName));

			var cdcTable = new CdcTable(testSchemaName, testTableName);
			if (!cdcTable.IsCdcEnabled(connection))
			{
				cdcTable.EnableCdc(connection);
			}

			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				sqlText = @"
IF NOT EXISTS(SELECT null FROM sys.schemas WHERE name = '{0}')
	EXEC ('CREATE SCHEMA [{0}]');

IF EXISTS (SELECT null FROM sys.tables t inner join sys.schemas s on t.schema_id = s.schema_id WHERE t.name = '{1}' and s.name = '{0}')
	DROP TABLE [{0}].[{1}]
CREATE TABLE [{0}].[{1}] (
	[__$start_lsn] binary(10) NOT NULL,
	[__$seqval] binary(10) NOT NULL,
	[__$operation] int NOT NULL,
	[__$update_mask] varbinary(128) NOT NULL,
	[__$lsn_period] smallint DEFAULT (0) NOT NULL,
	Column1ID uniqueidentifier,
	Column2Int int,
	Column3Char char(10),
	Column4Xml Nvarchar(max)
)

IF NOT EXISTS (SELECT NULL FROM [{2}].TableConfiguration WHERE SourceSchemaName = '{0}' AND SourceTableName = '{1}')
	INSERT INTO [{2}].TableConfiguration (SourceSchemaName, SourceTableName, TableColumnList, TableColumnConvertList)
	VALUES('{0}', '{1}', '__$start_lsn,__$seqval,__$operation,__$update_mask,Column1ID,Column2Int,Column3Char,Column4Xml','__$start_lsn,__$seqval,__$operation,__$update_mask,Column1ID,Column2Int,Column3Char,CONVERT(NVARCHAR(MAX),Column4Xml) AS Column4Xml')

IF NOT EXISTS (SELECT NULL FROM [{2}].TableState WHERE SourceSchemaName = '{0}' AND SourceTableName = '{1}')
	INSERT INTO [{2}].TableState (SourceSchemaName, SourceTableName, CurrentState)
	VALUES('{0}', '{1}', 'New')
";
				connection.ExecuteNonQuery(string.Format(sqlText, testSchemaName, testTableName, BiConstants.BiAdminSchemaName));
			}
		}

		void RemoveColumnFromAuditTable(AdminConnection connection, string auditDbName, string testTableName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				var sqlText = @"ALTER TABLE [{0}].[{1}] DROP COLUMN Column3Char";
				connection.ExecuteNonQuery(string.Format(sqlText, testSchemaName, testTableName));
			}
		}

		void RunCdcHistorySummaryPopulator(DbConnection connection, string auditDbName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				var populator = new CdcHistorySummaryPopulator(connection, new LoggerForTest());
				populator.Run();
			}
		}

		void CreateTestColumnStoreTable(AdminConnection connection, string auditDbName, string testTableName)
		{
			var sqlText = @"
IF NOT EXISTS(SELECT null FROM sys.schemas WHERE name = '{0}')
	EXEC ('CREATE SCHEMA [{0}]');

IF EXISTS (SELECT null FROM sys.tables WHERE name = '{1}')
	DROP TABLE [{0}].[{1}]
CREATE TABLE [{0}].[{1}] (
	Column1ID uniqueidentifier,
	Column2Int int,
	Column3Char char(10),
	Column4Xml xml --varchar(Max) is not support for column store
)";

			connection.ExecuteNonQuery(string.Format(sqlText, testSchemaName, testTableName));

			var cdcTable = new CdcTable(testSchemaName, testTableName);
			if (!cdcTable.IsCdcEnabled(connection))
			{
				cdcTable.EnableCdc(connection);
			}

			using (((ICurrentDbControl)connection).UseDatabase(auditDbName))
			{
				sqlText = @"
IF NOT EXISTS(SELECT null FROM sys.schemas WHERE name = '{0}')
	EXEC ('CREATE SCHEMA [{0}]');

IF EXISTS (SELECT null FROM sys.tables WHERE name = '{1}')
	DROP TABLE [{0}].[{1}]
CREATE TABLE [{0}].[{1}] (
	[__$start_lsn] binary(10) NOT NULL,
	[__$seqval] binary(10) NOT NULL,
	[__$operation] int NOT NULL,
	[__$update_mask] varbinary(128) NOT NULL,
	[__$lsn_period] smallint DEFAULT (0) NOT NULL,
	Column1ID uniqueidentifier,
	Column2Int int,
	Column3Char char(10),
	Column4Xml nvarchar(max)
)

IF EXISTS (SELECT name FROM sys.indexes  
            WHERE name = N'cci_{0}_{1}')   
    DROP INDEX cci_{0}_{1} ON [{0}].[{1}];

CREATE CLUSTERED COLUMNSTORE INDEX cci_{0}_{1}
    ON [{0}].[{1}];

IF NOT EXISTS (SELECT NULL FROM [{2}].TableConfiguration WHERE SourceSchemaName = '{0}' AND SourceTableName = '{1}')
	INSERT INTO [{2}].TableConfiguration (SourceSchemaName, SourceTableName, TableColumnList,TableColumnConvertList)
	VALUES('{0}', '{1}', '__$start_lsn,__$seqval,__$operation,__$update_mask,Column1ID,Column2Int,Column3Char,Column4Xml', '__$start_lsn,__$seqval,__$operation,__$update_mask,Column1ID,Column2Int,Column3Char,CONVERT(NVARCHAR(MAX),Column4Xml) AS Column4Xml')

IF NOT EXISTS (SELECT NULL FROM [{2}].TableState WHERE SourceSchemaName = '{0}' AND SourceTableName = '{1}')
	INSERT INTO [{2}].TableState (SourceSchemaName, SourceTableName, CurrentState)
	VALUES('{0}', '{1}', 'New')
";
				connection.ExecuteNonQuery(string.Format(sqlText, testSchemaName, testTableName, BiConstants.BiAdminSchemaName));
			}
		}

		void ModifyLoadAuditStoredProcedure(AdminConnection mainDbConnection, string auditDbName, string testTableName)
		{
			using (((ICurrentDbControl)mainDbConnection).UseDatabase(auditDbName))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture,
					@"SELECT TableID FROM [{0}].[TableConfiguration] WHERE SourceSchemaName = '{1}' AND SourceTableName = '{2}'",
					BiConstants.BiAdminSchemaName,
					testSchemaName,
					testTableName);

				var tableId = Convert.ToInt32(mainDbConnection.ExecuteScalar(sqlText));

				sqlText = string.Format(CultureInfo.InvariantCulture, @"
ALTER PROCEDURE [{0}].[usp_LoadAuditTable]

@from_lsn BINARY(10),
@to_lsn BINARY(10),
@table_id INT,
@server_name VARCHAR(1000)

AS

BEGIN
	SET NOCOUNT ON;
	SET XACT_ABORT ON;

	declare @message NVARCHAR(MAX)
	select @message = [text]
	from sys.messages
	where language_id = 1033 and message_id = 601

	IF @table_id = {1}
		RAISERROR(@message /*'Could not continue scan with NOLOCK due to data movement'*/, 12, 1)

END",
				BiConstants.BiAdminSchemaName,
				tableId);
				mainDbConnection.ExecuteNonQuery(sqlText);
			}
		}

		void InsertRow(DbConnection connection, Guid id, string stringValue = null)
		{
			var query = string.Format(@"
INSERT INTO [{0}].[{1}] (Column1ID, Column2Int, Column3Char, Column4Xml) VALUES(@id, @int, @str, @xml)",
				testSchemaName,
				testTableName);

			using (var cmd = connection.Command(query))
			{
				cmd.AddParameter("@id", SqlDbType.UniqueIdentifier, id);
				cmd.AddParameter("@int", SqlDbType.Int, 1);
				cmd.AddParameter("@str", SqlDbType.VarChar, 128, stringValue ?? "Val1");
				cmd.AddParameter("@xml", SqlDbType.Xml, "<root><child>1</child></root>");

				cmd.ExecuteNonQuery();
			}
		}

		void InsertRow(DbConnection connection, Guid id, string testSchemaName, string testTableName, string stringValue = null)
		{
			var query = string.Format(@"
INSERT INTO [{0}].[{1}] (Column1ID, Column2Int, Column3Char, Column4Xml) VALUES(@id, @int, @str, @xml)",
				testSchemaName,
				testTableName);

			using (var cmd = connection.Command(query))
			{
				cmd.AddParameter("@id", SqlDbType.UniqueIdentifier, id);
				cmd.AddParameter("@int", SqlDbType.Int, 1);
				cmd.AddParameter("@str", SqlDbType.VarChar, 128, stringValue ?? "Val1");
				cmd.AddParameter("@xml", SqlDbType.Xml, "<root><child>1</child></root>");

				cmd.ExecuteNonQuery();
			}
		}

		void UpdateRow(DbConnection connection, Guid id)
		{
			var query = string.Format(@"
UPDATE [{0}].[{1}]
SET Column2Int = @int, Column3Char = @str , Column4Xml = @xml
WHERE Column1ID = @id",
				testSchemaName,
				testTableName);

			using (var cmd = connection.Command(query))
			{
				cmd.AddParameter("@id", SqlDbType.UniqueIdentifier, id);
				cmd.AddParameter("@int", SqlDbType.Int, 2);
				cmd.AddParameter("@str", SqlDbType.VarChar, "Val2");
				cmd.AddParameter("@xml", SqlDbType.Xml, "<root><child>1</child></root>");

				cmd.ExecuteNonQuery();
			}
		}

		void DeleteRow(DbConnection connection, Guid id)
		{
			var query = string.Format(@"
DELETE FROM [{0}].[{1}]
WHERE Column1ID = @id",
				testSchemaName,
				testTableName);

			using (var cmd = connection.Command(query))
			{
				cmd.AddParameter("@id", SqlDbType.UniqueIdentifier, id);

				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region CDC Enabling/Disabling

		void EnableCdc(AdminConnection connection)
		{
			if (CdcDatabase.IsEnabled(connection, Db.DatabaseName))
			{
				CdcTestHelper.TruncateLog(connection);
				CdcDatabase.Disable(connection, Db.DatabaseName);
			}
			CdcDatabase.Enable(connection, Db.DatabaseName);
		}

		void TruncateTables(DbConnection connection, string databaseName)
		{
			connection.ExecuteNonQuery(string.Format("TRUNCATE TABLE [{0}].[{1}].[MasterState]", databaseName, BiConstants.BiAdminSchemaName));
			connection.ExecuteNonQuery(string.Format("TRUNCATE TABLE [{0}].[{1}].[TableState]", databaseName, BiConstants.BiAdminSchemaName));
			connection.ExecuteNonQuery(string.Format("TRUNCATE TABLE [{0}].[{1}].[TableConfiguration]", databaseName, BiConstants.BiAdminSchemaName));
			connection.ExecuteNonQuery(string.Format("TRUNCATE TABLE [{0}].[{1}].[LsnTimeMapping]", databaseName, BiConstants.BiAdminSchemaName));
			connection.ExecuteNonQuery(string.Format("TRUNCATE TABLE [{0}].[{1}].[DataLossLog]", databaseName, BiConstants.BiAdminSchemaName));
		}

		#endregion

		class AuditTsqlScriptRunnerForTest : AuditTsqlScriptRunner
		{
			public AuditTsqlScriptRunnerForTest(DbConnection biConnection, ILogger logger)
				: base(biConnection, logger)
			{ }

			public int timeOutExposed => commandTimeout;

			protected override int BatchModeSize => BatchModeSize_Exposed;
			public int BatchModeSize_Exposed { get; set; }

			protected override float MaxIntervalMinutes => MaxIntervalMinutes_Exposed;
			public float MaxIntervalMinutes_Exposed { get; set; }

			public Exception exception { get; set; }
			public string LinkedServerName_Exposed { get; set; }

			protected override string LinkedServerName => LinkedServerName_Exposed ?? base.LinkedServerName;

			protected override void RunUnsafe()
			{
				if (exception is null)
				{
					base.RunUnsafe();
				}
				else
				{
					throw exception;
				}
			}

			protected override void RerunMasterAuditLoad()
			{
				// do nothing
			}

			internal IEnumerable<string> GetErrorList_Exposed()
			{
				return this.GetErrorList();
			}
		}

		enum CdcOperation
		{
			Delete = 1,
			Insert = 2,
			BeforeUpdate = 3,
			AfterUpdate = 4
		}

		#endregion
	}
}
