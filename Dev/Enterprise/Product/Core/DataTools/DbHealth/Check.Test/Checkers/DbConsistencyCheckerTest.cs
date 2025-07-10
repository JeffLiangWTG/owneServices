using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.Types;
using Enterprise.DbHealth.Shared;
using Enterprise.DbHealth.Shared.Test;
using Enterprise.Environment;
using Enterprise.Integration;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DbHealth.Check
{
	[TestedType(typeof(DbConsistencyCheckerForTest))]
	sealed class DbConsistencyCheckerTest : CheckerTestCaseBase
	{
		public void TestCheckProperStepping()
		{
			var testChecker = new DbConsistencyCheckerForTest();
			var logger = (TestServiceLogger)testChecker.Logger;
			testChecker.ShouldExecuteActualCmd = false;
			DbHealthWarningList warningList = new DbHealthWarningList();
			string testDbName = "DbForConsistencyTest-BFAD575548084627B6F71EB5690620CD";
			string testDbName2 = testDbName + "2";
			testChecker.DatabasesToCheckOverride = new string[] { testDbName, testDbName2 };

			try
			{
				using (((ICurrentDbControl)Db.Connection).UseDatabase(Db.Connection.CurrentDatabase))
				{
					DbWorker.CreateTestDb(testDbName);
					DbWorker.CreateTestDb(testDbName2);
					int totalTablesPerDb = DbWorker.GetNumberOfTables(testDbName);

					testChecker.RegistryDb = testDbName;
					testChecker.RegistryTableView = "[dbo].[Table2]";
					testChecker.RegistryStep = 2;
					(testChecker as IChecker).Check(Db.Connection, warningList, logger);

					// ALL COMMANDS
					int expectedTotalOfExecutedCommands = 1 + (totalTablesPerDb - 1 + totalTablesPerDb) + 2 + 2;
					AssertEquals("Total of DBCC Commands Executed",
						expectedTotalOfExecutedCommands, testChecker.DbccCommandsRunByWorker.Rows.Count);

					// DB1-STEP1
					DataRow[] cmdsDb1Step1 = testChecker.GetCommandsRunPerDbAndStep(testDbName, StepsOfDbCheck.Allocation);
					AssertEquals("DB1-STEP1: DBCC Commands Executed", 0, cmdsDb1Step1.Length);

					// DB1-STEP2
					DataRow[] cmdsDb1Step2 = testChecker.GetCommandsRunPerDbAndStep(testDbName, StepsOfDbCheck.Tables);
					AssertEquals("DB1-STEP2: DBCC Commands Executed", totalTablesPerDb - 1, cmdsDb1Step2.Length);
					AssertEquals("DB1-STEP2: 1st Object", "[dbo].[Table2]", cmdsDb1Step2[0][2].ToString());

					// DB1-STEP3
					DataRow[] cmdsDb1Step3 = testChecker.GetCommandsRunPerDbAndStep(testDbName, StepsOfDbCheck.Catalog);
					AssertEquals("DB1-STEP3: DBCC Commands Executed", 1, cmdsDb1Step3.Length);

					// DB1-STEP4
					DataRow[] cmdsDb1Step4 = testChecker.GetCommandsRunPerDbAndStep(testDbName, StepsOfDbCheck.Views);
					AssertEquals("DB1-STEP4: DBCC Commands Executed", 1, cmdsDb1Step4.Length);
					AssertEquals("DB1-STEP4: 1st Object", "[dbo].[View1]", cmdsDb1Step4[0][2].ToString());

					// DB2-STEP1
					DataRow[] cmdsDb2Step1 = testChecker.GetCommandsRunPerDbAndStep(testDbName2, StepsOfDbCheck.Allocation);
					AssertEquals("DB2-STEP1: DBCC Commands Executed", 1, cmdsDb2Step1.Length);

					// DB2-STEP2
					DataRow[] cmdsDb2Step2 = testChecker.GetCommandsRunPerDbAndStep(testDbName2, StepsOfDbCheck.Tables);
					AssertEquals("DB2-STEP2: DBCC Commands Executed", totalTablesPerDb, cmdsDb2Step2.Length);
					AssertEquals("DB2-STEP2: 1st Object", "[dbo].[Table1]", cmdsDb2Step2[0][2].ToString());

					// DB2-STEP3
					DataRow[] cmdsDb2Step3 = testChecker.GetCommandsRunPerDbAndStep(testDbName2, StepsOfDbCheck.Catalog);
					AssertEquals("DB2-STEP3: DBCC Commands Executed", 1, cmdsDb2Step3.Length);

					// DB2-STEP4
					DataRow[] cmdsDb2Step4 = testChecker.GetCommandsRunPerDbAndStep(testDbName2, StepsOfDbCheck.Views);
					AssertEquals("DB2-STEP4: DBCC Commands Executed", 1, cmdsDb2Step4.Length);
					AssertEquals("DB2-STEP4: 1st Object", "[dbo].[View1]", cmdsDb2Step4[0][2].ToString());
				}
			}
			finally
			{
				DbWorker.DropTestDbIfExists(testDbName);
				DbWorker.DropTestDbIfExists(testDbName2);
			}
		}

		[SnailTest]
		[RequiresLargeLogFile]
		public void TestUsingTablock()
		{
			// tweak these as machines get faster.
			// trying to cause the host process to detect a user is trying to run a statement
			// blocked by dbcc
			// this should cancel the dbcc task

			Env.Registry.DbccInitialTimeout = 300000;
			Env.Registry.DbccUserWaitThreshold = 500;
			Env.Registry.DbccServiceTaskWaitThreshold = 1000;
			Env.Registry.DbccPollingInterval = 500;

			var tableName = "TestTable";
			var fullTableName = string.Format(CultureInfo.InvariantCulture, "[{0}].[dbo].[{1}]", Db.DatabaseName, tableName);

			var userAction = new Action(() =>
			{
				Thread.Sleep(500);

				var sqlInsert = string.Format(CultureInfo.InvariantCulture, @"-- DBCC CHECKTABLE User activity
INSERT dbo.{0}(a, b, c) VALUES (1, 'aaa', 2.22);"
					, tableName
					);
				using (var userConnection = Db.NewAdminConnection())
				using (var userCommand = userConnection.Command(sqlInsert))
				{
					userCommand.ExecuteNonQuery();
				}
			});

			var checker = new DbConsistencyCheckerForTest();
			checker.UseTablockOption = true;

			var logger = (TestServiceLogger)checker.Logger;
			var cancelledMessage = string.Format(CultureInfo.InvariantCulture, "Information|{0} - The operation was cancelled due to blocking user process. Switched to using Snapshot.", fullTableName);

			long initialRowCount = 1000000;
			PrepareData(tableName, initialRowCount);
			AssertEquals("PRECONDITION: Initial row count", initialRowCount, GetRowCount(fullTableName));

			// Cancel
			checker.ActionForTest = userAction;
			CombineAssertions(() =>
			{
				AssertEquals("Check cancelled", true, !checker.TryUseTablock_Exposed(fullTableName));
				AssertEquals("Table must be unlocked for user insert.", cancelledMessage, logger[0]);
				AssertEquals("1 log only: Table must be unlocked for user insert.", 1, logger.Count);
				AssertEquals("User inserted new row.", initialRowCount + 1, GetRowCount(fullTableName));
			});

			// Complete
			checker.ActionForTest = null;
			logger.ClearLog();
			CombineAssertions(() =>
			{
				AssertEquals("Check cancelled", false, !checker.TryUseTablock_Exposed(fullTableName));
				AssertEquals("No locks detected.", 0, logger.Count);
				AssertEquals("No user activity.", initialRowCount + 1, GetRowCount(fullTableName));
			});
		}

		[UseSnapshotProtection]
		[SnailTest]
		public void TestUsingTablock_LockTimeout()
		{
			Env.Registry.DbccInitialTimeout = 300000;
			Env.Registry.DbccUserWaitThreshold = 1000;
			Env.Registry.DbccServiceTaskWaitThreshold = 1000;
			Env.Registry.DbccPollingInterval = 1000;

			var tableName = "TestTable";
			var fullTableName = string.Format(CultureInfo.InvariantCulture, "[{0}].[dbo].[{1}]", Db.DatabaseName, tableName);
			var manualEvent = new ManualResetEventSlim(false);

			Task blockerTask = null;
			var userAction = new Action(() =>
			{
				var sqlInsert = string.Format(CultureInfo.InvariantCulture, @"-- DBCC CHECKTABLE User activity
BEGIN TRAN
INSERT dbo.{0} (a, b, c) VALUES (1, 'aaa', 2.22);
WAITFOR DELAY '00:00:03';
ROLLBACK TRAN
"
					, tableName
					);
				blockerTask = Task.Run(() =>
				{
					manualEvent.Wait();
					using (var userConnection = Db.NewAdminConnection())
					using (var userCommand = userConnection.Command(sqlInsert))
					{
						userCommand.ExecuteNonQuery();
					}
				});
			});

			var checker = new DbConsistencyCheckerForTest();
			checker.UseTablockOption = true;

			var logger = (TestServiceLogger)checker.Logger;
			var cancelledMessage = string.Format(CultureInfo.InvariantCulture, "Information|{0} - The operation was cancelled due to lock timeout. Switched to using Snapshot.", fullTableName);

			long initialRowCount = 1;
			PrepareData(tableName, initialRowCount);
			AssertEquals("PRECONDITION: Initial row count", initialRowCount, GetRowCount(fullTableName));

			// Cancel - lock timeout is less then user lock (waitfor delay)
			checker.ActionForTest = null;
			var result = true;
			var testAction = Task.Run(() =>
			{
				manualEvent.Set();
				result = checker.TryUseTablock_Exposed(fullTableName);
			});
			userAction.Invoke();
			Task.WaitAll(blockerTask, testAction);

			CombineAssertions(() =>
			{
				AssertEquals("Check cancelled", true, !result);
				AssertEquals("1 log only: Switched to using Snapshot due to lock timeout.", 1, logger.Count);
				AssertEquals("Switched to using Snapshot due to lock timeout.", cancelledMessage, logger[0]);
			});

			// Complete - lock timeout is greater then user lock (waitfor delay)
			Env.Registry.DbccUserWaitThreshold = 5000;
			logger.ClearLog();
			result = false;
			manualEvent.Reset();
			testAction = Task.Run(() =>
			{
				manualEvent.Set();
				result = checker.TryUseTablock_Exposed(fullTableName);
			});
			userAction.Invoke();
			Task.WaitAll(blockerTask, testAction);

			CombineAssertions(() =>
			{
				AssertEquals("Check cancelled", false, !result);
				AssertEquals("No locks detected.", 0, logger.Count);
			});
		}

		public void TestObjectDisposedException_ReproduceProblem_Issue00918093()
		{
			var cancelationSource = new CancellationTokenSource();
			var cancellationToken = cancelationSource.Token;
			using (cancelationSource)
			{
				cancelationSource.Cancel();
				cancelationSource.Cancel();
			}
			AssertExceptionThrown("Expect the error throw when we check WaitHandle.", typeof(ObjectDisposedException), () => { var waitHandle = cancellationToken.WaitHandle; });
		}

		public void TestCheckSharedRefDbByDifferentClients()
		{
			var checker = new DbConsistencyCheckerForTest();

			SqlApplicationLock appLock;
			using (var conn = Db.NewExtraConnectionToMainDb())
			{
				Assert(conn.TryGetLock(DbConsistencyChecker.DatabaseConsistencyCheckLockKey, out appLock));
				try
				{
					checker.Run(Db.Connection, new[] { Db.Connection.CurrentDatabase });
					var logs = string.Join(System.Environment.NewLine, checker.Logger.ToString());
					AssertMultilineASCIIEquals("Text did not match"
						, expected: string.Join(System.Environment.NewLine, new[]
						{
							$"Information|Checking {Db.Connection.CurrentDatabase.QuoteName()} database - by parts",
							$"Information|Database {Db.Connection.CurrentDatabase} is already being processed.",
						})
						, actual: logs);
				}
				finally
				{
					appLock.Dispose();
				}
			}
		}

		public void TestSharedDbExtendedProperTyName_NoCustomerCode()
		{
			var sharedDbExtendedPropertyName = DbConsistencyChecker.GetSharedDbExtendedPropertyName(Db.Connection);
			AssertEquals("LatestConsistencyCheck", sharedDbExtendedPropertyName);
		}

		[TestDate(2020, 04, 20, 16, 04, 20)]
		public void TestSharedRefDb_LatestConsistencyCheck_ExtendedPropertyCreatedWithCorrectTime()
		{
			// Arrange
			var checker = new DbConsistencyCheckerForTest();
			var sharedRefDbName = "CW-RefDb-Xxx-ZZ-000000";
			var warningList = new DbHealthWarningList();
			var logger = checker.Logger;

			using (AdoTestUtils.CreateDbDropExistingDisposable(sharedRefDbName, Db.DatabaseName))
			{
				AssertNull(DataUtils.LoadDbExtendedProperty(Db.Connection, DbConsistencyChecker.GetSharedDbExtendedPropertyName(Db.Connection), sharedRefDbName));
				checker.DatabasesToCheckOverride = new string[] { sharedRefDbName };
				Db.Connection.AddSharedReferenceDatabase_ForTest(sharedRefDbName);

				// Act
				((IChecker)checker).Check(Db.Connection, warningList, logger);

				// Assert
				var propertyValue = DataUtils.LoadDbExtendedProperty(Db.Connection, DbConsistencyChecker.GetSharedDbExtendedPropertyName(Db.Connection), sharedRefDbName);
				AssertNotNullOrEmpty("Extended property value should have been set", propertyValue);
				AssertEquals("Extended property value should have been set to now", ZDateTimeOffset.UtcNow, DateTimeOffset.Parse(propertyValue));
			}
		}

		[UseSnapshotProtection]
		[TestDate(2020, 04, 20, 16, 04, 21)]
		public void TestUpdateLatestConsistencyCheckOnSecondarySingleShareRefDb()
		{
			// Arrange
			const string sharedRefDbName = "CW-RefDatabase-ForTest";
			DbRegistry.SingleRefDatabaseName.SaveValue(sharedRefDbName, Db.Connection);

			AssertEquals(sharedRefDbName, RefDbTableNameResolver.SingleRefDatabaseName);

			using (AdoTestUtils.CreateDbDropExistingDisposable(sharedRefDbName, Db.DatabaseName))
			{
				using var secondaryConnection = Db.NewAdminConnection(Db.ServerName, sharedRefDbName);
				AssertEquals(true, secondaryConnection.DatabaseExists(sharedRefDbName));

				var checker = new DbConsistencyCheckerForTest(Db.Connection, secondaryConnection);
				checker.ShouldExecuteActualCmd = false;
				checker.DatabasesToCheckOverride = [sharedRefDbName];

				Env.Registry.DbccRunSecondariesByParts = true;

				DataUtils.SaveDbExtendedProperty(secondaryConnection,
					DbConsistencyChecker.GetSharedDbExtendedPropertyName(secondaryConnection),
					ZDateTimeOffset.UtcNow.Add(-checker.MinimumTimeBetweenSharedRefDbChecks.Add(TimeSpan.FromDays(1)))
						.ToDateTimeOffset().ToString(), sharedRefDbName);

				var commandCollector = new ZStringBuilder();

				try
				{
					secondaryConnection.OnExecute += LogExecutedCommand;

					// Act
					((IChecker)checker).Check(secondaryConnection, new DbHealthWarningList(), checker.Logger);
				}
				finally
				{
					secondaryConnection.OnExecute -= LogExecutedCommand;
				}

				// Assert
				var propertyValue = DataUtils.LoadDbExtendedProperty(secondaryConnection,
					DbConsistencyChecker.GetSharedDbExtendedPropertyName(secondaryConnection), sharedRefDbName);
				AssertEquals("Extended property value should have been set by secondary connection",
					ZDateTimeOffset.UtcNow, DateTimeOffset.Parse(propertyValue));

				const string sqlTextForSaveDbExtendedProperty = @"
IF EXISTS(SELECT null FROM sys.extended_properties WHERE class = 0 AND name = @propertyName)
  EXEC sys.sp_updateextendedproperty @name = @propertyName, @value = @propertyValue;
ELSE
EXEC sys.sp_addextendedproperty @name = @propertyName, @value = @propertyValue;";

				AssertContains(sqlTextForSaveDbExtendedProperty, commandCollector.ToStringWithNewLineBetweenAppends());

				void LogExecutedCommand(DbCommand command)
				{
					commandCollector.Append(command.CommandText);
				}
			}
		}

		[TestDate(2020, 04, 20, 16, 04, 21)]
		public void TestSharedRefDb_DoesNotCheck_IfPreviousIsSoonerThanFrequency()
		{
			// Arrange
			var checker = new DbConsistencyCheckerForTest();
			var sharedRefDbName = "CW-RefDb-Xxx-ZZ-000001";
			var warningList = new DbHealthWarningList();
			var logger = checker.Logger;

			using (AdoTestUtils.CreateDbDropExistingDisposable(sharedRefDbName, Db.DatabaseName))
			{
				var latestRunTime = ZDateTimeOffset.UtcNow.AddDays(-1).ToDateTimeOffset();
				DataUtils.SaveDbExtendedProperty(Db.Connection, DbConsistencyChecker.GetSharedDbExtendedPropertyName(Db.Connection), latestRunTime.ToString(), sharedRefDbName);
				checker.DatabasesToCheckOverride = new string[] { sharedRefDbName };
				Db.Connection.AddSharedReferenceDatabase_ForTest(sharedRefDbName);

				// Act
				((IChecker)checker).Check(Db.Connection, warningList, logger);

				// Assert
				var propertyValue = DataUtils.LoadDbExtendedProperty(Db.Connection, DbConsistencyChecker.GetSharedDbExtendedPropertyName(Db.Connection), sharedRefDbName);
				AssertEquals("Extended property value should still have latestRunTime value", latestRunTime, DateTimeOffset.Parse(propertyValue));
			}
		}

		[TestDate(2020, 04, 20, 16, 04, 22)]
		public void TestSharedRefDb_DoesCheck_IfPreviousIsLaterThanFrequency()
		{
			// Arrange
			var checker = new DbConsistencyCheckerForTest();
			var sharedRefDbName = "CW-RefDb-Xxx-ZZ-000002";
			var warningList = new DbHealthWarningList();
			var logger = checker.Logger;

			using (AdoTestUtils.CreateDbDropExistingDisposable(sharedRefDbName, Db.DatabaseName))
			{
				var latestRunTime = ZDateTimeOffset.UtcNow.AddDays(-14).ToDateTimeOffset();
				DataUtils.SaveDbExtendedProperty(Db.Connection, DbConsistencyChecker.GetSharedDbExtendedPropertyName(Db.Connection), latestRunTime.ToString(), sharedRefDbName);
				checker.DatabasesToCheckOverride = new string[] { sharedRefDbName };
				Db.Connection.AddSharedReferenceDatabase_ForTest(sharedRefDbName);

				// Act
				((IChecker)checker).Check(Db.Connection, warningList, logger);

				// Assert
				var propertyValue = DataUtils.LoadDbExtendedProperty(Db.Connection, DbConsistencyChecker.GetSharedDbExtendedPropertyName(Db.Connection), sharedRefDbName);
				AssertEquals("Extended property value should have been set to now", ZDateTimeOffset.UtcNow, DateTimeOffset.Parse(propertyValue));
			}
		}

		[UseSnapshotProtection]
		public void TestRunCheckdbWithPhysicalOnly_Secondary()
		{
			var db_Main = "DccRunCheckdbWithPhysicalOnlyForTest";

			var warningList = new DbHealthWarningList();
			var logger = new LoggerForTest();

			var checker = new DbConsistencyCheckerForTest(Db.Connection);
			checker.DatabasesToCheckOverride = new string[] { db_Main };
			checker.UseTablockOption = true;
			checker.ShouldExecuteActualCmd = false;

			AlwaysOn.AlwaysOnDatabases_ForTest.Value = new List<string>(new[] { db_Main });
			Env.Registry.DbccRunCheckdbWithPhysicalOnly = true;

			using (AdoTestUtils.CreateDbDropExistingDisposable(db_Main))
			using (var connection = Db.NewAdminConnection(db_Main))
			{
				Env.Registry.DbccRunSecondariesByParts = true;

				logger.ClearLog();
				(checker as IChecker).Check(connection, warningList, logger);

				AssertArrayEqualsByElements("Primary - OFF, PhysicalOnly - ON, AlwaysOn - ON, RunByParts - ON => by parts"
					, expected: new[]
					{
						$"Checking [{db_Main}] database - by parts",
					}
					, actual: logger.LogEntries.ToArray());

				Env.Registry.DbccRunSecondariesByParts = false;

				logger.ClearLog();
				(checker as IChecker).Check(connection, warningList, logger);

				AssertArrayEqualsByElements("Primary - OFF, PhysicalOnly - ON, AlwaysOn - ON, RunByParts - OFF => using DBCC CHECKDB"
					, expected: new[]
					{
						$"Checking [{db_Main}] database - using DBCC CHECKDB",
					}
					, actual: logger.LogEntries.ToArray());
			}
		}

		[UseSnapshotProtection]
		public void TestRunCheckdbWithPhysicalOnly_Primary()
		{
			var db_Main = "DccRunCheckdbWithPhysicalOnlyForTest";
			var db_SD = db_Main + "_SD001";
			var db_Audit = db_Main + "_Audit";

			var warningList = new DbHealthWarningList();
			var logger = new LoggerForTest();

			var checker = new DbConsistencyCheckerForTest();
			checker.DatabasesToCheckOverride = new string[] { db_Main, db_SD, db_Audit };
			checker.UseTablockOption = true;
			checker.ShouldExecuteActualCmd = false;

			using (AdoTestUtils.CreateDbDropExistingDisposable(db_Main))
			using (AdoTestUtils.CreateDbDropExistingDisposable(db_SD))
			using (AdoTestUtils.CreateDbDropExistingDisposable(db_Audit))
			using (var connection = Db.NewAdminConnection(db_Main))
			{
				Env.Registry.DbccRunCheckdbWithPhysicalOnly = false;
				AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = false;

				logger.ClearLog();
				(checker as IChecker).Check(connection, warningList, logger);

				AssertArrayEqualsByElements("PhysicalOnly - OFF => parts"
					, expected: new[]
					{
						$"Checking [{db_Main}] database - by parts",
						$"Checking [{db_SD}] database - by parts",
						$"Checking [{db_Audit}] database - by parts",
					}
					, actual: logger.LogEntries.ToArray());

				Env.Registry.DbccRunCheckdbWithPhysicalOnly = true;

				logger.ClearLog();
				(checker as IChecker).Check(connection, warningList, logger);

				AssertArrayEqualsByElements("PhysicalOnly - ON, AlwaysOn - OFF => parts"
					, expected: new[]
					{
						$"Checking [{db_Main}] database - by parts",
						$"Checking [{db_SD}] database - by parts",
						$"Checking [{db_Audit}] database - by parts",
					}
					, actual: logger.LogEntries.ToArray());

				AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = null;
				AlwaysOn.AlwaysOnDatabases_ForTest.Value = new List<string>(new[] { db_Main, db_SD });

				logger.ClearLog();
				(checker as IChecker).Check(connection, warningList, logger);

				AssertArrayEqualsByElements("PhysicalOnly - ON, AlwaysOn - ON => physical only for AlwaysOn databases"
					, expected: new[]
					{
						$"Checking [{db_Main}] database - physical only",
						$"Checking [{db_SD}] database - physical only",
						$"Checking [{db_Audit}] database - by parts",
					}
					, actual: logger.LogEntries.ToArray());
			}
		}

		[UseSnapshotProtection]
		public void TestRunCheckdbWithPhysicalOnly_ActualRun()
		{
			var db_Main = "DccRunCheckdbWithPhysicalOnlyActualRunForTest";

			var warningList = new DbHealthWarningList();
			var logger = new LoggerForTest();

			var checker = new DbConsistencyCheckerForTest();
			checker.DatabasesToCheckOverride = new string[] { db_Main };
			checker.UseTablockOption = true;
			checker.ShouldExecuteActualCmd = true;

			using (AdoTestUtils.CreateDbDropExistingDisposable(db_Main))
			using (var connection = Db.NewAdminConnection(db_Main))
			{
				Env.Registry.DbccRunCheckdbWithPhysicalOnly = true;
				AlwaysOn.IsDbPartOfAlwaysOn_ForTest.Value = true;

				logger.ClearLog();
				(checker as IChecker).Check(connection, warningList, logger);

				AssertArrayEqualsByElements("PhysicalOnly - ON, AlwaysOn - ON => physical only for AlwaysOn databases"
					, expected: new[]
					{
						$"Checking [{db_Main}] database - physical only",
					}
					, actual: logger.LogEntries.ToArray());
			}
		}

		#region Implementation

		void PrepareData(string tableName, long rowCount)
		{
			var sql = string.Format(CultureInfo.InvariantCulture, @"-- DBCC CHECKTABLE Prepare data
if (OBJECT_ID(N'dbo.{0}', N'U') is NOT NULL) DROP TABLE dbo.{0};
CREATE TABLE dbo.{0}(id int IDENTITY(1,1) PRIMARY KEY NONCLUSTERED, a int, b varchar(max), c decimal(9, 3));
CREATE INDEX _1 ON dbo.{0}(a)
CREATE INDEX _2 ON dbo.{0}(a) include(b)
CREATE INDEX _3 ON dbo.{0}(c)
CREATE INDEX _4 ON dbo.{0}(id, a)
CREATE INDEX _5 ON dbo.{0}(id, c) include (b)
CREATE INDEX _6 ON dbo.{0}(id, c)
CREATE INDEX _7 ON dbo.{0}(id, a) include (b)

;with d as
(
	select row_number() over (order by (select null)) RN FROM dbo.StmNumberSequence A join dbo.StmNumberSequence B on 1=1 
)
INSERT dbo.{0}(a, b, c) SELECT 1, 'aaa', 2.22 FROM d WHERE RN <= {1};
;"
				, tableName
				, rowCount
				);

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		long GetRowCount(string fullObjectName)
		{
			var sqlCount = string.Format(CultureInfo.InvariantCulture, @"-- DBCC CHECKTABLE Rowcount
SELECT rows FROM sys.partitions WHERE object_id = OBJECT_ID(N'{0}', N'U') AND index_id in (0, 1);
"
				, fullObjectName
				);

			using (var cmd = Db.Connection.Command(sqlCount))
			{
				return (long)cmd.ExecuteScalar();
			}
		}

		#region overrides

		protected override IChecker GetNewCheckerInstance()
		{
			return new DbConsistencyCheckerForTest();
		}

		protected override void TearDown()
		{
			Db.Connection.ClearRefDbNameBuffers_ForTest();
			base.TearDown();
		}

		#endregion // overrides

		#endregion // Implementation
	}
}
