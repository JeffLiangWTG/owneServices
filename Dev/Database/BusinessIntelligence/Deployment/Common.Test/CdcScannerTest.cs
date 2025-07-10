using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace Enterprise.ChangeDataCapture.Common.Testing
{
	class CdcScannerTest : TestCase
	{
		public void TestLogStatus()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				var loggerForTest = new CdcScannerLoggerForTest();
				var scanner = new CdcScannerForTest(loggerForTest);

				var timer = new Stopwatch();
				timer.Start();
				var expectedLogs = new List<string>();

				scanner.LogStatus_Exposed(timer, "XXX", new DateTime(2000, 1, 1));
				AssertEquals("Expect no logs as 5sec has not elapsed", 0, loggerForTest.Logs.Count);
				AssertContainsExactElementsInExactOrder(expectedLogs, loggerForTest.Logs);

				Thread.Sleep((int)scanner.GetLogInterval().TotalMilliseconds + 1000);
				scanner.LogStatus_Exposed(timer, "XXX", new DateTime(2000, 1, 1));
				expectedLogs.Add("Captured changes for transactions up to LSN: 0xXXX, Transaction Date (UTC): 1/01/2000 12:00:00 AM");
				expectedLogs.Add("Number of transaction in the log awaiting CDC Scan: 0 @ 0 transactions per second");
				AssertEquals("Expect 2 logs as 5sec has elapsed", 2, loggerForTest.Logs.Count);
				AssertContainsExactElementsInExactOrder(expectedLogs, loggerForTest.Logs);
			}
		}

		[UseSnapshotProtection]
		public void TestScanUntilNoTransactionsToProcess()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				CdcDatabase.Enable(testConnection, Db.DatabaseName);
				var scanner = new CdcScannerForTest();

				string sqlText = @"
					CREATE TABLE dbo.CdcScannerTest$Table (ColA int PRIMARY KEY, ColB char(1));
					EXEC sys.sp_cdc_enable_table
						@source_schema = 'dbo',
						@source_name = 'CdcScannerTest$Table',
						@role_name = null;";
				testConnection.ExecuteNonQuery(sqlText);

				var processedTranCount = scanner.ScanUntilNoTransactionsToProcess();
				CombineAssertions(() =>
				{
					Assert("Initial scanned transaction count", processedTranCount > 0);

					AssertSourceTableContents(testConnection, Array.Empty<int>(), Array.Empty<string>());
					AssertCdcTableContents(testConnection, Array.Empty<int>(), Array.Empty<int>(), Array.Empty<string>());
				});

				sqlText = "INSERT dbo.CdcScannerTest$Table VALUES (1,'X'), (2,'Y');";
				testConnection.ExecuteNonQuery(sqlText);

				AssertSourceTableContents(testConnection, new int[] { 1, 2 }, new string[] { "X", "Y" });
				AssertCdcTableContents(testConnection, Array.Empty<int>(), Array.Empty<int>(), Array.Empty<string>());

				processedTranCount = scanner.ScanUntilNoTransactionsToProcess();
				CombineAssertions(() =>
				{
					Assert("Scanned transaction count", processedTranCount > 0);
					AssertCdcTableContents(testConnection, new int[] { 2, 2 }, new int[] { 1, 2 }, new string[] { "X", "Y" });
				});

				sqlText = @"
					UPDATE dbo.CdcScannerTest$Table SET ColB = 'Z' WHERE ColA = 1;
					DELETE dbo.CdcScannerTest$Table WHERE ColA = 2;";
				testConnection.ExecuteNonQuery(sqlText);

				AssertSourceTableContents(testConnection, new int[] { 1 }, new string[] { "Z" });
				AssertCdcTableContents(testConnection, new int[] { 2, 2 }, new int[] { 1, 2 }, new string[] { "X", "Y" });

				processedTranCount = scanner.ScanUntilNoTransactionsToProcess();
				CombineAssertions(() =>
				{
					Assert("Scanned transaction count", processedTranCount > 0);
					AssertCdcTableContents(testConnection, new int[] { 2, 2, 3, 4, 1 }, new int[] { 1, 2, 1, 1, 2 }, new string[] { "X", "Y", "X", "Z", "Y" });
				});

				processedTranCount = scanner.ScanUntilNoTransactionsToProcess();
				AssertEquals("Final scanned transaction count", 0, processedTranCount);
			}
		}

		[UseSnapshotProtection]
		public void TestCDCScanWithDedicatedNonPooledConnection()
		{
			var scannerForTest = new CdcScannerForTest();
			DedicatedConnectionForCDCScanForTest dedicatedConnection;
			using (dedicatedConnection = new DedicatedConnectionForCDCScanForTest())
			{
				AssertEquals(true, scannerForTest.DedicatedNonPooledCDCConnectionExposed.GetType() == typeof(DedicatedConnectionForCDCScan));
				AssertEquals(false, dedicatedConnection.ConnectionPoolingValueExposed.IsPooling);
			}
			AssertEquals(System.Data.ConnectionState.Closed, dedicatedConnection.ConnectionStateExposed);
		}

		public void TestSystemDatabaseReplicaStatesSchema()
		{
			AssertNoExceptionThrown(() =>
			{
				using (var testConnection = Db.NewAdminConnection())
				{
					var sqlText = "SELECT is_commit_participant, synchronization_health FROM sys.dm_hadr_database_replica_states";
					testConnection.ExecuteNonQuery(sqlText);
				}
			});
		}

		public void TestTraceStatusSchema()
		{
			using (var testConnection = Db.NewAdminConnection())
			using (var cmd = testConnection.Command("DBCC TRACESTATUS(1448)"))
			using (var reader = cmd.ExecuteReader())
			{
				AssertEquals("Field count", 4, reader.FieldCount);
				Assert("DBCC TRACESTATUS returned empty result set.", reader.Read());

				var traceFlag = reader["TraceFlag"];
				var status = reader["Status"];
				var global = reader["Global"];
				var session = reader["Session"];

				CombineAssertions(() =>
				{
					AssertEquals("Data type for TraceFlag", typeof(short), traceFlag.GetType());
					AssertEquals("Data type for Status", typeof(short), status.GetType());
					AssertEquals("Data type for Global", typeof(short), global.GetType());
					AssertEquals("Data type for Session", typeof(short), session.GetType());
				});
			}
		}

		[UseSnapshotProtection]
		public void TestCdcUpdateMaskIsVarbinary128()
		{
			using (var connection = Db.NewAdminConnection())
			{
				if (!CdcDatabase.IsEnabled(connection, Db.DatabaseName))
				{
					CdcDatabase.Enable(connection, Db.DatabaseName);
				}

				connection.ExecuteNonQuery("CREATE TABLE [dbo].[TestTable] (c1 int)");

				var cdcTable = new CdcTableForTesting("dbo", "TestTable");
				cdcTable.EnableCdc(connection);

				var sqlText = @"select t.name as dataType, c.max_length as maxLength
from sys.columns c
inner join sys.types t
	on c.user_type_id = t.user_type_id
where
	c.object_id = OBJECT_ID('cdc.dbo_TestTable_CT') and
	c.name = '__$update_mask'";

				using (var reader = connection.Command(sqlText).ExecuteReader())
				{
					if (reader.Read())
					{
						var dataType = reader["dataType"].ToString();
						var maxLength = Convert.ToInt32(reader["maxLength"], CultureInfo.InvariantCulture);

						CombineAssertions("Column __$update_mask should be varbinary(128).", () =>
						{
							AssertEquals(dataType, "varbinary");
							AssertEquals(maxLength, 128);
						});
					}
					else
					{
						Fail("Column __$update_mask not found in CDC table.");
					}
				}
			}
		}

		public void TestOrMaskFunction()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var sqlText = @"select case when sys.ORMask(Mask) = 0xFF then 1 else 0 end
from 
	(select 0xF0 as Mask union select 0x0F) Q";

				Assert("sys.ORMask() changed functionality.", Convert.ToBoolean(connection.ExecuteScalar(sqlText), CultureInfo.InvariantCulture));
			}
		}

		[UseSnapshotProtection]
		public void TestCdcUpdateMaskFunctionality()
		{
			using (var connection = Db.NewAdminConnection())
			{
				if (!CdcDatabase.IsEnabled(connection, Db.DatabaseName))
				{
					CdcDatabase.Enable(connection, Db.DatabaseName);
				}

				connection.ExecuteNonQuery("CREATE TABLE [dbo].[TestTable] (c1 int, c2 int, c3 int)");

				var cdcTable = new CdcTableForTesting("dbo", "TestTable");
				cdcTable.EnableCdc(connection);
				var cdcScanner = new CdcScannerForTest();

				connection.ExecuteNonQuery("INSERT INTO [dbo].[TestTable] (c1, c2, c3) VALUES (1, 1, 1)");
				cdcScanner.ScanUntilNoTransactionsToProcess();
				var sqlText = "SELECT TOP 1 __$update_mask FROM cdc.dbo_TestTable_CT ORDER BY __$start_lsn DESC";
				var actualUpdateMask = (byte[])connection.ExecuteScalar(sqlText);
				AssertArrayEqualsByElements("All columns updated", new byte[] { 0x7 }, actualUpdateMask);

				connection.ExecuteNonQuery("UPDATE [dbo].[TestTable] SET c1 = 2");
				cdcScanner.ScanUntilNoTransactionsToProcess();
				actualUpdateMask = (byte[])connection.ExecuteScalar(sqlText);
				AssertArrayEqualsByElements("C1 updated", new byte[] { 0x1 }, actualUpdateMask);

				connection.ExecuteNonQuery("UPDATE [dbo].[TestTable] SET c2 = 2");
				cdcScanner.ScanUntilNoTransactionsToProcess();
				actualUpdateMask = (byte[])connection.ExecuteScalar(sqlText);
				AssertArrayEqualsByElements("C2 updated", new byte[] { 0x2 }, actualUpdateMask);

				connection.ExecuteNonQuery("UPDATE [dbo].[TestTable] SET c3 = 2");
				cdcScanner.ScanUntilNoTransactionsToProcess();
				actualUpdateMask = (byte[])connection.ExecuteScalar(sqlText);
				AssertArrayEqualsByElements("C3 updated", new byte[] { 0x4 }, actualUpdateMask);

				connection.ExecuteNonQuery("UPDATE [dbo].[TestTable] SET c1 = 3, c2 = 3");
				cdcScanner.ScanUntilNoTransactionsToProcess();
				actualUpdateMask = (byte[])connection.ExecuteScalar(sqlText);
				AssertArrayEqualsByElements("C1 and C2 updated", new byte[] { 0x3 }, actualUpdateMask);

				connection.ExecuteNonQuery("UPDATE [dbo].[TestTable] SET c2 = 4, c3 = 4");
				cdcScanner.ScanUntilNoTransactionsToProcess();
				actualUpdateMask = (byte[])connection.ExecuteScalar(sqlText);
				AssertArrayEqualsByElements("C2 and C3 updated", new byte[] { 0x6 }, actualUpdateMask);

				connection.ExecuteNonQuery("UPDATE [dbo].[TestTable] SET c1 = 5, c3 = 5");
				cdcScanner.ScanUntilNoTransactionsToProcess();
				actualUpdateMask = (byte[])connection.ExecuteScalar(sqlText);
				AssertArrayEqualsByElements("C1 and C3 updated", new byte[] { 0x5 }, actualUpdateMask);

				connection.ExecuteNonQuery("UPDATE [dbo].[TestTable] SET c1 = 6, c2 = 6, c3 = 6");
				cdcScanner.ScanUntilNoTransactionsToProcess();
				actualUpdateMask = (byte[])connection.ExecuteScalar(sqlText);
				AssertArrayEqualsByElements("All columns updated", new byte[] { 0x7 }, actualUpdateMask);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.BI })]
		public void TestHandleInvalidBeginLsnForCdcCommitRecord()
		{
			var auditDbName = Db.AuditDatabaseName;
			var edwDbName = Db.EdwDatabaseName;
			using (var connection = Db.NewAdminConnection())
			{
				if (!CdcDatabase.IsEnabled(connection, Db.DatabaseName))
				{
					CdcDatabase.Enable(connection, Db.DatabaseName);
				}
				connection.ExecuteNonQuery("CREATE TABLE [dbo].[TestTable] (c1 int)");
				var cdcTable = new CdcTableForTesting("dbo", "TestTable");
				cdcTable.EnableCdc(connection);
				connection.ExecuteNonQuery("INSERT INTO [dbo].[TestTable] VALUES(1)");

				connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "TRUNCATE TABLE [{0}].[{1}].[DataLossLog]", auditDbName, BiConstants.BiAdminSchemaName));
				connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "EXEC [{0}].[{1}].[usp_SetMasterStateParameter] @ParamName='{2}', @ParamValue='0x01'", edwDbName, BiConstants.BiAdminSchemaName, BiConstants.LastLsnProcessed));
				connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "UPDATE [{0}].[{1}].[StagingTableState] SET InitialLoadRequired = 0, CurrentState = 'Idle'", edwDbName, BiConstants.BiAdminSchemaName));

				AssertAuditCorruptedDataLossLog(connection, auditDbName, false);
				AssertEdwInitialLoadRequired(connection, edwDbName, false);

				var scanner = new CdcScannerForTest();
				scanner.HandleInvalidBeginLsnForCdcCommitRecord_Exposed();

				AssertAuditCorruptedDataLossLog(connection, auditDbName, true);
				AssertEdwInitialLoadRequired(connection, edwDbName, true);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.BI })]
		public void TestCDCScanDeadlockError()
		{
			var auditDbName = Db.AuditDatabaseName;
			var edwDbName = Db.EdwDatabaseName;
			using (var connection = Db.NewAdminConnection())
			{
				if (!CdcDatabase.IsEnabled(connection, Db.DatabaseName))
				{
					CdcDatabase.Enable(connection, Db.DatabaseName);
				}
				connection.ExecuteNonQuery("CREATE TABLE [dbo].[TestTable] (c1 int)");
				var cdcTable = new CdcTableForTesting("dbo", "TestTable");
				cdcTable.EnableCdc(connection);
				connection.ExecuteNonQuery("INSERT INTO [dbo].[TestTable] VALUES(1)");

				var scanner = new CdcScannerForTest();
				scanner.SetCdcScanError(DbErrorType.DeadlockError);
				AssertExceptionThrown(typeof(SqlException), "A deadlock error.", () => scanner.CallCdcScanProcedure_Exposed(0));
			}
		}

		[UseSnapshotProtection]
		public void TestCdcErrorLogs()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				CdcDatabase.Enable(testConnection, Db.DatabaseName);
				var scanner = new CdcScannerForTest();

				string sqlText = @"
					CREATE TABLE dbo.CdcScannerTest$Table (ColA int PRIMARY KEY, ColB char(1));
					EXEC sys.sp_cdc_enable_table
						@source_schema = 'dbo',
						@source_name = 'CdcScannerTest$Table',
						@role_name = null;";
				testConnection.ExecuteNonQuery(sqlText);

				sqlText = @"
					CREATE TRIGGER [cdc].[TR_II_dbo_TestTable_CT] ON [cdc].[dbo_CdcScannerTest$Table_CT]
					INSTEAD OF INSERT AS
					BEGIN
						DECLARE @val int = 1/0
					END";
				testConnection.ExecuteNonQuery(sqlText);

				sqlText = "INSERT dbo.CdcScannerTest$Table VALUES (1,'X'), (2,'Y');";
				testConnection.ExecuteNonQuery(sqlText);

				try
				{
					var processedTranCount = scanner.ScanUntilNoTransactionsToProcess();
					Fail("Expected CdcException to be thrown.");
				}
				catch (CdcException ex)
				{
					CombineAssertions($"Actual message: {ex.Message}", () =>
					{
						Assert("Expected message: (22863) Failed to insert rows into Change Data Capture change tables. Refer to previous errors in the current session to identify the cause and correct any associated problems.",
							ex.Message.Contains("(22863) Failed to insert rows into Change Data Capture change tables. Refer to previous errors in the current session to identify the cause and correct any associated problems."));
						Assert("Expected message: (8134) Divide by zero error encountered.",
							ex.Message.Contains("(8134) Divide by zero error encountered."));
					});
				}
			}
		}

		[UseSnapshotProtection]
		public void TestCdcScanWithIrrelevantTransactions()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				DbRegistry.BiCdcMaxTransactions.SaveValue(2, testConnection);
				if (CdcDatabase.IsEnabled(testConnection, Db.DatabaseName))
				{
					CdcDatabase.Disable(testConnection, Db.DatabaseName);
				}
				CdcDatabase.Enable(testConnection, Db.DatabaseName);
				new CdcTable("dbo", "StmData").EnableCdc(testConnection);

				string sqlText = @"
					CREATE TABLE dbo.CdcScannerTest$Table (ColA int PRIMARY KEY, ColB bit);
					INSERT INTO CdcScannerTest$Table (ColA, ColB) VALUES (1, 1)
					EXEC sys.sp_cdc_enable_table
						@source_schema = 'dbo',
						@source_name = 'CdcScannerTest$Table',
						@role_name = null;";
				testConnection.ExecuteNonQuery(sqlText);

				var logger = new CdcScannerLoggerForTest();
				var scanner = new CdcScannerForTest(logger);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableTransactionCount(testConnection, logger, expectedNumberOfRecords: 0);

				InsertIrrelevantTransaction(testConnection, numberOfTransactions: 10);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableTransactionCount(testConnection, logger, expectedNumberOfRecords: 0);

				InsertIrrelevantTransaction(testConnection, numberOfTransactions: 3);
				InsertRelevantTransaction(testConnection, numberOfTransactions: 1);
				scanner.ScanUntilNoTransactionsToProcess();
				AssertCdcTableTransactionCount(testConnection, logger, expectedNumberOfRecords: 2); // Update inserts 2 records to CDC table
			}
		}

		[UseSnapshotProtection]
		public void TestStatementHasBeenTerminated()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				if (!CdcDatabase.IsEnabled(testConnection, Db.DatabaseName))
				{
					CdcDatabase.Enable(testConnection, Db.DatabaseName);
				}

				var testLogger = new CdcScannerLoggerForTest();
				var cdcScanner = new CdcScannerForTest(testLogger);
				cdcScanner.SetCdcScanError(DbErrorType.StatementHasBeenTerminated);

				try
				{
					cdcScanner.ScanUntilNoTransactionsToProcess();
					Fail("Expected exception 'The statement has been terminated.'");
				}
				catch (CdcException e)
				{
					CombineAssertions($"Actual message: {e.Message}", () =>
					{
						Assert("Missing exception message 'The statement has been terminated.'", e.Message.Contains("The statement has been terminated."));
						Assert("Missing exception message 'Session ID:'", e.Message.Contains("Session ID:"));
					});
				}
			}
		}

		void InsertIrrelevantTransaction(DbConnection connection, int numberOfTransactions)
		{
			var sqlText = "UPDATE CdcScannerTest$Table SET ColB = ColB WHERE ColA = 1";
			for (var counter = 0; counter < numberOfTransactions; counter++)
			{
				connection.ExecuteNonQuery(sqlText);
			}
		}

		void InsertRelevantTransaction(DbConnection connection, int numberOfTransactions)
		{
			var sqlText = "UPDATE CdcScannerTest$Table SET ColB = ~ColB WHERE ColA = 1";
			for (var counter = 0; counter < numberOfTransactions; counter++)
			{
				connection.ExecuteNonQuery(sqlText);
			}
		}

		void AssertCdcTableTransactionCount(DbConnection testConnection, CdcScannerLoggerForTest logger, int expectedNumberOfRecords)
		{
			var actualCount = Convert.ToInt32(testConnection.ExecuteScalar("SELECT COUNT(*) FROM cdc.dbo_CdcScannerTest$Table_CT"));
			AssertEquals($"Logs:\r\n{string.Join("\r\n", logger.Logs)}\r\n\r\nNumber of records in CDC table", expectedNumberOfRecords, actualCount);
		}

		void AssertSourceTableContents(DbConnection testConnection, int[] colAValues, string[] colBValues)
		{
			Assert("Provided expected arrays must have the same length", colAValues.Length == colBValues.Length);

			// Expected
			var expectedContents = new StringBuilder();
			expectedContents.AppendLine("ColA - ColB");
			expectedContents.AppendLine("-----------");

			for (int i = 0; i < colAValues.Length; i++)
			{
				expectedContents.AppendFormat("   {0} -    {1}", colAValues[i].ToString(), colBValues[i]).AppendLine();
			}

			// Actual
			var actualContents = new StringBuilder();
			actualContents.AppendLine("ColA - ColB");
			actualContents.AppendLine("-----------");

			using (var cmd = testConnection.Command("SELECT ColA, ColB FROM dbo.CdcScannerTest$Table ORDER BY ColA"))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					actualContents.AppendFormat("   {0} -    {1}", reader[0].ToString(), reader[1].ToString()).AppendLine();
				}
			}

			AssertEquals("Source Table Contents", expectedContents.ToString(), actualContents.ToString());
		}

		void AssertCdcTableContents(DbConnection testConnection, int[] operations, int[] colAValues, string[] colBValues)
		{
			Assert("Provided expected arrays must have the same length", operations.Length == colAValues.Length && colAValues.Length == colBValues.Length);

			// Expected
			var expectedContents = new StringBuilder();
			expectedContents.AppendLine("Oper - ColA - ColB");
			expectedContents.AppendLine("------------------");

			for (int i = 0; i < operations.Length; i++)
			{
				expectedContents.AppendFormat("   {0} -    {1} -    {2}", operations[i].ToString(), colAValues[i].ToString(), colBValues[i]).AppendLine();
			}

			// Actual
			var actualContents = new StringBuilder();
			actualContents.AppendLine("Oper - ColA - ColB");
			actualContents.AppendLine("------------------");

			using (var cmd = testConnection.Command("SELECT [__$operation], ColA, ColB FROM cdc.dbo_CdcScannerTest$Table_CT ORDER BY __$start_lsn"))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					actualContents.AppendFormat("   {0} -    {1} -    {2}", reader[0].ToString(), reader[1].ToString(), reader[2].ToString()).AppendLine();
				}
			}

			AssertEquals("CDC Table Contents", expectedContents.ToString(), actualContents.ToString());
		}

		void AssertAuditCorruptedDataLossLog(DbConnection connection, string auditDbName, bool expectedValue)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "IF EXISTS (SELECT null FROM [{0}].[{1}].[DataLossLog] WHERE LossType = 'Corrupted') SELECT 1 ELSE SELECT 0", auditDbName, BiConstants.BiAdminSchemaName);
			AssertEquals("Audit database corrupted data loss", expectedValue, Convert.ToBoolean(connection.ExecuteScalar(sqlText)));
		}

		void AssertEdwInitialLoadRequired(DbConnection connection, string edwDbName, bool expectedValue)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "IF NOT EXISTS (SELECT null FROM [{0}].[{1}].[MasterState]) SELECT 1 ELSE SELECT 0", edwDbName, BiConstants.BiAdminSchemaName);
			AssertEquals("EDW database master state is empty?", expectedValue, Convert.ToBoolean(connection.ExecuteScalar(sqlText)));

			string.Format(CultureInfo.InvariantCulture, "IF EXISTS (SELECT null FROM [{0}].[{1}].[StagingTableState] WHERE InitialLoadRequired = 1) SELECT 1 ELSE SELECT 0", edwDbName, BiConstants.BiAdminSchemaName);
			AssertEquals("EDW database staging tables require initial load?", expectedValue, Convert.ToBoolean(connection.ExecuteScalar(sqlText)));
		}

		[UseSnapshotProtection]
		public void TestCDCScanWontWorkUnlessFirstConnectionIsClosed()
		{
			//This test is here to demonstrate that a dedicated connection (closable conneciton) is required in order for replcmds to be executed recursively in separate connections.
			//replflush SHOULD NOT BE CALLED in the normal execution of CDC Scanning unless dealing with CDC errors, this will lead to data loss.
			//CS00915583 - WISGLOSYD - UMP Sync Issue
			string sqlText = @"EXEC dbo.CdcScan;";
			using (var adminConnection = Db.NewAdminConnection())
			using (var testConnection = DedicatedConnectionForCDCScan.New())
			{
				var connectionPooling = DbEnv.Instance.ConnectionPooling;
				AssertEquals("Connection Pooling is off", false, connectionPooling.IsPooling);
				CdcDatabase.Enable(adminConnection, Db.DatabaseName);
				testConnection.ExecuteNonQuery(sqlText);
				AssertNoExceptionThrown("CDC Scan twice in same connection should not throw exception ", () =>
				{
					testConnection.ExecuteNonQuery(sqlText);
				});

				using (var anotherTestConnectionAttemptWhileOuterConnectionRemainsOpen = Db.NewAdminConnection())
				{
					try
					{
						anotherTestConnectionAttemptWhileOuterConnectionRemainsOpen.ExecuteNonQuery(sqlText);
						Fail("Additional CDC Scan when previous connection has not been closed should throw exception.");
					}
					catch (SqlException ex)
					{
						var errorType = new DbErrorMatch(ex).ExceptionType;
						AssertEquals("Expected SQL exception type", DbErrorType.AnotherConnectionIsRunningSpReplcmdsForCdc, errorType);
					}
				}
			}
		}

		[UseSnapshotProtection]
		public void TestCDCScanWillWorkInNewConnectionAfterREPLFLUSH()
		{
			//This test is here to demonstrate that replflush will allow subsequent replcmds to be executed after a connection remains open.
			//replflush SHOULD NOT BE CALLED in the normal execution of CDC Scanning unless dealing with CDC errors, this will lead to data loss.
			//CS00915583 - WISGLOSYD - UMP Sync Issue
			using (var adminConnection = Db.NewAdminConnection())
			using (var testConnection = DedicatedConnectionForCDCScan.New())
			{
				string sqlText = @"
					CREATE PROCEDURE dbo.CdcScanWithReplFlushForTest
					WITH EXECUTE AS OWNER AS
					BEGIN
						EXEC SYS.SP_CDC_SCAN @CONTINUOUS=0;
						EXEC SP_REPLFLUSH;
					END
				";
				var connectionPooling = DbEnv.Instance.ConnectionPooling;
				AssertEquals("Connection Pooling is off", false, connectionPooling.IsPooling);
				CdcDatabase.Enable(adminConnection, Db.DatabaseName);
				adminConnection.ExecuteNonQuery(sqlText);

				AssertNoExceptionThrown("Another CDC Scan in same connection should not throw exception ", () =>
				{
					using (var cmd = testConnection.Command("dbo.CdcScanWithReplFlushForTest"))
					{
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.ExecuteNonQuery();
					}
				});

				using (var anotherTestConnectionAttemptWhileOuterConnectionRemainsOpen = DedicatedConnectionForCDCScan.New())
				{
					AssertNoExceptionThrown("Additional CDC Scan when previous connection not closed should not throw exception with Replflush", () =>
					{
						using (var cmd = anotherTestConnectionAttemptWhileOuterConnectionRemainsOpen.Command("dbo.CdcScanWithReplFlushForTest"))
						{
							cmd.CommandType = CommandType.StoredProcedure;
							cmd.ExecuteNonQuery();
						}
					});
				}
			}
		}

		[UseSnapshotProtection]
		public void TestCDCScannerDeadlockPriority()
		{
			var scannerForTest = new CdcScannerForTest();
			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					var deadlockPriority = 0;
					if (!CdcDatabase.IsEnabled(connection, Db.DatabaseName))
					{
						CdcDatabase.Enable(connection, Db.DatabaseName);
					}

					connection.ExecuteNonQuery("CREATE TABLE [dbo].[TestingTable] (c1 int)");

					var cdcTable = new CdcTableForTesting("dbo", "TestingTable");
					cdcTable.EnableCdc(connection);

					var updateTableQuery = @"INSERT INTO dbo.TestingTable (c1) VALUES (1)";
					connection.ExecuteNonQuery(updateTableQuery);
					int sessionId = 0;

					var cdcTask = new Task(() =>
					{
						using (var secondConnection = new DedicatedConnectionForCDCScanForTest())
						{
							var sessionQuery = $@"SELECT @@SPID AS 'ID'";
							sessionId = Convert.ToInt32(secondConnection.ExecuteScalar(sessionQuery));
							scannerForTest.CallCdcScanProcedure_Exposed(1);
						}
					});

					var lockTask = new Task(() =>
					{
						using (var firstConnection = Db.NewAdminConnection())
						{
							var lockTableQuery = @"begin tran select TOP 0 * from cdc.dbo_TestingTable_CT ct with(TABLOCKX, HOLDLOCK)";
							firstConnection.ExecuteNonQuery(lockTableQuery);

							Thread.Sleep(1000);
							var deadlockPriorityQuery = $@"select deadlock_priority
														from sys.dm_exec_sessions
														where database_id = DB_ID('{Db.DatabaseName}')
														and session_id = {sessionId}";
							deadlockPriority = Convert.ToInt32(firstConnection.ExecuteScalar(deadlockPriorityQuery));
							AssertEquals(10, deadlockPriority);
							firstConnection.ExecuteNonQuery("ROLLBACK TRAN");
						}
					});
					lockTask.Start();
					cdcTask.Start();
					lockTask.Wait();
					cdcTask.Wait();
				}
				finally
				{
					CdcDatabase.Disable(connection, Db.DatabaseName);
					connection.ExecuteNonQuery("DROP TABLE [dbo].[TestingTable]");
				}
			}
		}

		[UseSnapshotProtection]
		public void TestCdcStoredProceduresReverted()
		{
			using (var connection = Db.NewAdminConnection())
			{
				if (CdcDatabase.IsEnabled(connection, Db.DatabaseName))
				{
					CdcDatabase.Disable(connection, Db.DatabaseName);
				}
				CdcDatabase.Enable(connection, Db.DatabaseName);

				new CdcTableForTesting("dbo", "GlbStaff").EnableCdc(connection, captureInstance: "dbo_GlbStaff");

				connection.ExecuteNonQuery("update dbo.glbstaff set GS_IsResource = 1, GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate()");

				var cdcScanner = new CdcScannerForTest();
				cdcScanner.ScanUntilNoTransactionsToProcess();

				var actualChangesCount = Convert.ToInt32(connection.ExecuteScalar("select count(*) from cdc.dbo_GlbStaff_CT"));
				AssertGreaterThan("CDC changes found?", actualChangesCount, 0);
			}
		}

		[UseSnapshotProtection]
		public void TestScanUntilNoTransactionsToProcessWithRepetitiveEndLsn_IdleSystem()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				CdcDatabase.Enable(testConnection, Db.DatabaseName);
				var loggerForTest = new CdcScannerLoggerForTest();
				var repeatScanner = new CdcScannerForRepetitiveEndLsnTest(loggerForTest);

				string sqlText = @"
					CREATE TABLE dbo.CdcScannerTest$Table (ColA int PRIMARY KEY, ColB char(1));
					EXEC sys.sp_cdc_enable_table
						@source_schema = 'dbo',
						@source_name = 'CdcScannerTest$Table',
						@role_name = null;";
				testConnection.ExecuteNonQuery(sqlText);
				var cdcScanner = new CdcScannerForTest();
				var processedTranCount = cdcScanner.ScanUntilNoTransactionsToProcess();

				repeatScanner.ScanUntilNoTransactionsToProcess();
				CombineAssertions(() =>
				{
					Assert("Initial scanned transaction count", processedTranCount > 0);

					AssertSourceTableContents(testConnection, Array.Empty<int>(), Array.Empty<string>());
					AssertCdcTableContents(testConnection, Array.Empty<int>(), Array.Empty<int>(), Array.Empty<string>());

					AssertGreaterThan("The value of MaxTransactions should be bigger than what is in registry.", repeatScanner.CdcScanProvider.MaxTransactions, DbRegistry.BiCdcMaxTransactions.LoadValue(testConnection));
				});

				var expectedLogs = new List<string>()
				{
					"Executing internal CDC Scan procedure",
					"Executing internal CDC Scan procedure",
				};

				AssertContainsExactElementsInExactOrder(expectedLogs, loggerForTest.Logs);
			}
		}

		[UseSnapshotProtection]
		public void TestScanUntilNoTransactionsToProcessWithRepetitiveEndLsn_ActiveSystem()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				CdcDatabase.Enable(testConnection, Db.DatabaseName);
				var loggerForTest = new CdcScannerLoggerForTest();
				var repeatScanner = new CdcScannerForRepetitiveEndLsnTest(loggerForTest);

				string sqlText = @"
					CREATE TABLE dbo.CdcScannerTest$Table (ColA int PRIMARY KEY, ColB char(1));
					EXEC sys.sp_cdc_enable_table
						@source_schema = 'dbo',
						@source_name = 'CdcScannerTest$Table',
						@role_name = null;";
				testConnection.ExecuteNonQuery(sqlText);
				var cdcScanner = new CdcScannerForTest();
				var processedTranCount = cdcScanner.ScanUntilNoTransactionsToProcess();

				testConnection.ExecuteNonQuery("INSERT INTO dbo.CdcScannerTest$Table (ColA, ColB) VALUES (1, 'A');");
				repeatScanner.ScanUntilNoTransactionsToProcess();

				CombineAssertions(() =>
				{
					Assert("Initial scanned transaction count", processedTranCount > 0);

					AssertSourceTableContents(testConnection, new int[1] { 1 }, new string[1] { "A" });
					AssertCdcTableContents(testConnection, new int[1] { 2 }, new int[1] { 1 }, new string[1] { "A" });

					AssertGreaterThan("The value of MaxTransactions should be bigger than what is in registry.", repeatScanner.CdcScanProvider.MaxTransactions, DbRegistry.BiCdcMaxTransactions.LoadValue(testConnection));
				});

				var expectedLogs = new List<string>()
				{
					"Executing internal CDC Scan procedure",
					"Repeating scan sessions for the same LSN range have been detected. The system has temporarily increased @maxtransactions to 1100 in order to progress the log reader. This message requires no action.",
					"Executing internal CDC Scan procedure",
					"Repeating scan sessions for the same LSN range have been detected. The system has temporarily increased @maxtransactions to 2100 in order to progress the log reader. This message requires no action.",
				};

				AssertContainsExactElementsInExactOrder(loggerForTest.Logs, expectedLogs);
			}
		}

		class CdcScannerForRepetitiveEndLsnTest : CdcScannerForTest
		{
			int endLsnMappingCount;
			LogScanResult repetitiveResult;

			public CdcScannerForRepetitiveEndLsnTest(CdcScannerLoggerForTest logger) : base(logger)
			{
			}

			internal override LogScanResult GetLastLogScanResult()
			{
				if (endLsnMappingCount == 1 && repetitiveResult != null)
				{
					endLsnMappingCount++;
					return repetitiveResult;
				}
				else
				{
					var result = base.GetLastLogScanResult();
					if (repetitiveResult == null)
					{
						repetitiveResult = result;
					}
					endLsnMappingCount++;
					return result;
				}
			}
		}

		class DedicatedConnectionForCDCScanForTest : DedicatedConnectionForCDCScan
		{
			public DedicatedConnectionForCDCScanForTest() : base()
			{
			}

			public IConnectionPooling ConnectionPoolingValueExposed => ConnectionPoolingValue;

			public ConnectionState ConnectionStateExposed => base.InternalConnectionUnsafe.State;
		}
	}
}
