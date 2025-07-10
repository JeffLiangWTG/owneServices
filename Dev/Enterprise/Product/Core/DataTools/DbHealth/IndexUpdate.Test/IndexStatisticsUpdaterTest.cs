using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbHealth.Shared.Test;
using Enterprise.Integration;
using Moq;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static System.FormattableString;

namespace Enterprise.DbHealth.IndexUpdate.Testing
{
	class IndexStatisticsUpdaterTest : DbRunnerWithRegistryWorkerTest
	{
		class SwitchOffAutoStatsForLOBTest : IndexStatisticsUpdaterTest
		{
			protected override void TestCheckWithTimeoutsCore()
			{
				// ignore since already tested from parent IndexStatisticsUpdaterTest
			}

			public void TestLockRequestTimeoutExceptionDoesNotBlockISUFromContinuingOntoNextStatsObject()
			{
				const string testDbName = "SwitchOffAutoStatsForLOBTest_AAEA41F38E02471EAB8D34DFC80665BE";

				var testLogs = new StringBuilder();
				var loggerMock = new Mock<ILogger>();

				void Log(LogType type, string message, Exception ex)
				{
					var sb = new StringBuilder();
					_ = sb.Append(type.ToString());
					_ = sb.Append("|");
					_ = sb.Append(message);
					if (ex != null)
					{
						_ = sb.Append("|");
						_ = sb.Append(ex.ToString());
					}
					_ = testLogs.AppendLine(sb.ToString());
				}

				_ = loggerMock.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>()))
					.Callback((LogType logType, string message) => Log(logType, message, null));
				_ = loggerMock.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>(), It.IsAny<Exception>()))
					.Callback((LogType logType, string message, Exception exception) => Log(logType, message, exception));

				var testUpdater = new IndexStatisticsUpdaterForTest(loggerMock.Object);
				testUpdater.DaysOfLastUpdateToAcceptAsValid_Exposed = 0;

				// Prepare
				using (var adminConnection = Db.NewAdminConnection())
				{
					AdoTestUtils.DropDbIfExists(adminConnection, testDbName);
				}

				var testTables = new[] { "_Table1", "_Table2" };
				testTables.ForEach(testTable => PrepareTestData(testDbName, testTable));

				// Arrange (1 of 2)
				// turn auto stats ON as ISU will turn them OFF, and will fail if blocked
				GetAutoStatsName(testDbName).ForEach(x =>
				{
					using (((ICurrentDbControl)Db.Connection).UseDatabase(testDbName))
					{
						_ = Db.Connection.ExecuteNonQuery(Invariant($"EXEC sys.sp_autostats N'[dbo].{x.TableName.QuoteName()}', 'ON', {x.StatsName.QuoteName()}"));
					}
				});

				testTables.ForEach(testTable =>
				{
					GetLOBColNames(testDbName, testTable).ForEach(col =>
					{
						AssertStatisticsNoRecompute($"PRECONDITION: Is No_Recompute for the field {col} on?", expected: false, testDbName, testTable, fieldName: col);
					});
				});

				using var holdlockEvent = new ManualResetEvent(false);
				using var exitEvent = new ManualResetEvent(false);

				// Arrange (2 of 2)
				// hold lock on the blockedTable
				var blockedTable = testTables[0];
				var nonBlockedTable = testTables[1];
				var blockingQueryTask = ExecuteLongRunningQueryInTransaction($"select * from [dbo].{blockedTable.QuoteName()} with(holdlock);");

				try
				{
					// Act
					_ = holdlockEvent.WaitOne();
					AssertNoExceptionThrown(() => testUpdater.Run(Db.Connection, new string[] { testDbName }));

					// Assert
					var logMessages = testLogs.ToString();

					// non-blocked table should have LOB stats updated
					GetLOBColNames(testDbName, nonBlockedTable).ForEach(col =>
					{
						AssertStatisticsNoRecompute($"Is No_Recompute for the field {col} on?", expected: true, testDbName, nonBlockedTable, fieldName: col);
					});

					// blocked table should have LOB stats NOT updated
					GetLOBColNames(testDbName, blockedTable).ForEach(col =>
					{
						AssertStatisticsNoRecompute($"Is No_Recompute for the field {col} on?", expected: false, testDbName, blockedTable, fieldName: col);
					});

					// Add a warning to the log to show which LOB stats failed to be switched off
					AssertContains($@"Warning|Cannot apply the following changes: ""EXEC sys.sp_autostats N'[dbo].[{blockedTable}]', 'OFF',", logMessages);

					// ISU should continue to index update and not blocked by failed turning off LOB stats
					testTables.ForEach(testTable =>
					{
						AssertContains($"Information|Updating Index Statistics: DB [{testDbName}] - Table/View [dbo].[{testTable}]", logMessages);
					});
				}
				finally
				{
					_ = exitEvent.Set();
					blockingQueryTask?.Wait();

					DbWorker.DropTestDbIfExists(testDbName);
				}

				Task ExecuteLongRunningQueryInTransaction(string sql)
				{
					return Task.Run(() =>
					{
						using var connection = Db.NewAdminConnection(testDbName);
						connection.RunInTransaction(() =>
						{
							_ = connection.ExecuteNonQuery(sql);
							_ = holdlockEvent.Set();
							_ = exitEvent.WaitOne();
						});
					});
				}
			}
		}

		class IndexStatisticsUpdaterBaseTest : IndexStatisticsUpdaterTest
		{
			protected override void TestCheckWithTimeoutsCore()
			{
				// ignore since already tested from parent IndexStatisticsUpdaterTest
			}

			public void TestStatisticsIsUpdated_WithSpecialCharacters()
			{
				AssertStatisticsIsUpdated("TestDb-BFAD575548084627B6F71EB569062AAA");
				AssertStatisticsIsUpdated("TestDb-BFAD575548084!@#$$%71EB569062AAA");
			}

			public void TestStatisticsIsUpdated()
			{
				AssertStatisticsIsUpdated("TestDb_BFAD575548084627B6F71EB569062AAA");
			}

			void AssertStatisticsIsUpdated(string testDbName)
			{
				TestServiceLogger testLogger = new TestServiceLogger();
				IndexStatisticsUpdaterForTest testUpdater = new IndexStatisticsUpdaterForTest(testLogger);
				string sql = @"SELECT
				 max(ind.rowmodctr)
				FROM
				 [{0}].sys.objects obj
				 INNER JOIN [{0}].sys.sysindexes ind
				  ON ind.id = obj.object_id and ind.indid != 0
				WHERE
				 obj.Name = '{1}'";

				try
				{
					using (((ICurrentDbControl)Db.Connection).UseDatabase(Db.Connection.CurrentDatabase))
					{
						DbWorker.CreateTestDb(testDbName);
						int value = GetValue(sql, testDbName);
						AssertNotEquals(0, value);

						testUpdater.Run(Db.Connection, new string[] { testDbName });

						value = GetValue(sql, testDbName);
						AssertEquals(0, value);
						var fullLog = testLogger.ToString();
						AssertContains(string.Format("Information|Updating Index Statistics: DB [{0}]", testDbName), fullLog);
						AssertContains(string.Format("Information|Updating Index Statistics: DB [{0}] - Table/View [dbo].[Table1]", testDbName), fullLog);
						AssertContains(string.Format("Information|Updating Index Statistics: DB [{0}] - Table/View [dbo].[Table2]", testDbName), fullLog);
						AssertContains(string.Format("Information|Updating Index Statistics: DB [{0}] - Table/View [dbo].[View1]", testDbName), fullLog);
					}
				}
				finally
				{
					DbWorker.DropTestDbIfExists(testDbName);
				}
			}

			public void TestSqlExceptionInCallToCheckAndReturnRemainingTimeoutInSecondsIsLogged()
			{
				// Arrange
				var testLogger = new TestServiceLogger();
				var testUpdater = new IndexStatisticsUpdaterForTest(testLogger);
				testUpdater.SpoilExecuteCommand = true;

				const string testDbName = "TestSqlExceptionIsLogged";
				using (new DisposableAction(
					() => DbWorker.CreateTestDb(testDbName),
					() => DbWorker.DropTestDbIfExists(testDbName)))
				{
					// Act
					testUpdater.Run(Db.Connection, new string[] { testDbName });
				}

				// Assert
				var lastLogForException = testLogger[testLogger.Count - 1];
				AssertContains("Error", lastLogForException);
				AssertContains("muhaha", lastLogForException);
			}

			public void TestSkipsReadOnlyDatabases()
			{
				TestServiceLogger testLogger = new TestServiceLogger();
				IndexStatisticsUpdaterForTest testUpdater = new IndexStatisticsUpdaterForTest(testLogger);
				string testDbName = "TestDb_CB273A3271D74AB28BE9014C04175A2C";

				try
				{
					DbWorker.CreateTestDb(testDbName);

					Db.Connection.AlterDbWriteableState(testDbName, false);
					testLogger.ClearLog();
					testUpdater.Run(Db.Connection, new string[] { testDbName });
					AssertEquals(0, testLogger.Count);

					Db.Connection.AlterDbWriteableState(testDbName, true);
					testUpdater.Run(Db.Connection, new string[] { testDbName });
					var fullLog = testLogger.ToString();
					AssertContains("Information|Updating Index Statistics: DB [TestDb_CB273A3271D74AB28BE9014C04175A2C]", fullLog);
					AssertContains("Information|Updating Index Statistics: DB [TestDb_CB273A3271D74AB28BE9014C04175A2C] - Table/View [dbo].[Table1]", fullLog);
					AssertContains("Information|Updating Index Statistics: DB [TestDb_CB273A3271D74AB28BE9014C04175A2C] - Table/View [dbo].[Table2]", fullLog);
					AssertContains("Information|Updating Index Statistics: DB [TestDb_CB273A3271D74AB28BE9014C04175A2C] - Table/View [dbo].[View1]", fullLog);
				}
				finally
				{
					DbWorker.DropTestDbIfExists(testDbName);
				}
			}

			public void TestParticularStatisticsIsUpdated()
			{
				var testDbName = "DbForConsistencyTest_BFAD575548084627B6F71EB569062AAA";

				var testLogger = new TestServiceLogger();
				var testUpdater = new IndexStatisticsUpdaterForTest(testLogger);
				testUpdater.DaysOfLastUpdateToAcceptAsValid_Exposed = 0;

				PrepareTestData(testDbName);

				Thread.Sleep(1000);
				var now = DateTime.Now;

				try
				{
					AssertStatisticsForTheField("PRECONDITION: Is statistics for the field index_stats updated?", now, false, testDbName, Table1, fieldName: "index_stats");
					AssertStatisticsForTheField("PRECONDITION: Is statistics for the field user_stats updated?", now, false, testDbName, Table1, fieldName: "user_stats");
					AssertStatisticsForTheField("PRECONDITION: Is statistics for the field auto_stats_int updated?", now, false, testDbName, Table1, fieldName: "auto_stats_int");
					AssertStatisticsForTheField("PRECONDITION: Is statistics for the field auto_stats_date updated?", now, false, testDbName, Table1, fieldName: "auto_stats_date");
					AssertStatisticsForTheField("PRECONDITION: Is statistics for the field auto_stats_lob updated?", now, false, testDbName, Table1, fieldName: "auto_stats_lob");
					AssertStatisticsForTheField("PRECONDITION: Is statistics for the field stats_no_recompute updated?", now, false, testDbName, Table1, fieldName: "stats_no_recompute");

					testUpdater.Run(Db.Connection, new string[] { testDbName });
					now = DateTime.Now;

					// _Table1 was updated
					AssertContains("Information|Updating Index Statistics: DB [DbForConsistencyTest_BFAD575548084627B6F71EB569062AAA] - Table/View [dbo].[_Table1]", testLogger.ToString());

					AssertStatisticsForTheField("Is statistics for the field index_stats updated?", now, true, testDbName, Table1, fieldName: "index_stats");
					AssertStatisticsForTheField("Is statistics for the field user_stats updated?", now, true, testDbName, Table1, fieldName: "user_stats");
					AssertStatisticsForTheField("Is statistics for the field auto_stats_int updated?", now, true, testDbName, Table1, fieldName: "auto_stats_int");
					AssertStatisticsForTheField("Is statistics for the field auto_stats_date updated?", now, true, testDbName, Table1, fieldName: "auto_stats_date");
					AssertStatisticsForTheField("Is statistics for the field auto_stats_lob updated?", now, false, testDbName, Table1, fieldName: "auto_stats_lob");
					AssertStatisticsForTheField("Is statistics for the field stats_no_recompute updated?", now, false, testDbName, Table1, fieldName: "stats_no_recompute");

					testLogger.ClearLog();
					testUpdater.Run(Db.Connection, new string[] { testDbName });

					// No changes since last update - no update for _Table1
					AssertNotContains("Information|Updating Index Statistics: DB [DbForConsistencyTest_BFAD575548084627B6F71EB569062AAA] - Table/View [dbo].[_Table1]", testLogger.ToString());
				}
				finally
				{
					DbWorker.DropTestDbIfExists(testDbName);
				}
			}

			public void TestLockTimeout_GetData()
			{
				var testDbName = "DbForStatisticsGetDataTest_720DC3C76570417EA3093AC19D6E81CF";

				var logger = new TestServiceLogger();
				var updater = new IndexStatisticsUpdaterForTest(logger);
				updater.DaysOfLastUpdateToAcceptAsValid_Exposed = 0;

				DbWorker.DropTestDbIfExists(testDbName);
				PrepareTestData(testDbName);

				Thread.Sleep(1000);
				var now = DateTime.Now;

				Task task = null;

				updater.UserActionGetData_ForTest = () =>
				{
					task = Task.Run(() =>
					{
						using (Db.DisposableActionForDbConnection())
						using (var connection = Db.NewAdminConnection(testDbName))
						{
							connection.BeginTransaction();
							connection.ExecuteNonQuery("ALTER TABLE [dbo].[_Table1] ADD new_field bit");
							Thread.Sleep(500);
						}
					});

					Thread.Sleep(100);
				};

				try
				{
					AssertStatisticsForTheField("PRECONDITION: Is statistics for the field index_stats updated?", now, false, testDbName, Table1, fieldName: "index_stats");
					AssertStatisticsForTheField("PRECONDITION: Is statistics for the field user_stats updated?", now, false, testDbName, Table1, fieldName: "user_stats");
					AssertStatisticsForTheField("PRECONDITION: Is statistics for the field auto_stats_int updated?", now, false, testDbName, Table1, fieldName: "auto_stats_int");
					AssertStatisticsForTheField("PRECONDITION: Is statistics for the field auto_stats_date updated?", now, false, testDbName, Table1, fieldName: "auto_stats_date");
					AssertStatisticsForTheField("PRECONDITION: Is statistics for the field auto_stats_lob updated?", now, false, testDbName, Table1, fieldName: "auto_stats_lob");

					using (Db.Connection.TemporarySetLockTimeout(TimeSpan.FromMilliseconds(1)))
					{
						updater.Run(Db.Connection, new string[] { testDbName });
						now = DateTime.Now;
					}

					CombineAssertions(() =>
					{
						AssertContains($"Information|Updating Index Statistics: DB [{testDbName}]", logger.ToString());

						AssertStatisticsForTheField("Is statistics for the field index_stats updated?", now, false, testDbName, Table1, fieldName: "index_stats");
						AssertStatisticsForTheField("Is statistics for the field user_stats updated?", now, false, testDbName, Table1, fieldName: "user_stats");
						AssertStatisticsForTheField("Is statistics for the field auto_stats_int updated?", now, false, testDbName, Table1, fieldName: "auto_stats_int");
						AssertStatisticsForTheField("Is statistics for the field auto_stats_date updated?", now, false, testDbName, Table1, fieldName: "auto_stats_date");
						AssertStatisticsForTheField("Is statistics for the field auto_stats_lob updated?", now, false, testDbName, Table1, fieldName: "auto_stats_lob");
					});
				}
				finally
				{
					Task.WaitAll(task);
					DbWorker.DropTestDbIfExists(testDbName);
				}
			}

			public void TestLockTimeout_Update()
			{
				var testDbName = "DbForStatisticsUpdateTest_720DC3C76570417EA3093AC19D6E81CF";

				var logger = new TestServiceLogger();
				var updater = new IndexStatisticsUpdaterForTest(logger);
				updater.DaysOfLastUpdateToAcceptAsValid_Exposed = 0;

				DbWorker.DropTestDbIfExists(testDbName);
				PrepareTestData(testDbName);

				Thread.Sleep(1000);
				var now = DateTime.Now;

				Task task = null;

				updater.UserActionUpdate_ForTest = () =>
				{
					task = Task.Run(() =>
					{
						using (Db.DisposableActionForDbConnection())
						using (var connection = Db.NewAdminConnection(testDbName))
						{
							connection.BeginTransaction();
							connection.ExecuteNonQuery("ALTER TABLE [dbo].[_Table1] ADD new_field bit");
							Thread.Sleep(500);
						}
					});

					Thread.Sleep(100);
				};

				try
				{
					AssertStatisticsForTheField("PRECONDITION: Is statistics for the field index_stats updated?", now, false, testDbName, Table1, fieldName: "index_stats");
					AssertStatisticsForTheField("PRECONDITION: Is statistics for the field user_stats updated?", now, false, testDbName, Table1, fieldName: "user_stats");
					AssertStatisticsForTheField("PRECONDITION: Is statistics for the field auto_stats_int updated?", now, false, testDbName, Table1, fieldName: "auto_stats_int");
					AssertStatisticsForTheField("PRECONDITION: Is statistics for the field auto_stats_date updated?", now, false, testDbName, Table1, fieldName: "auto_stats_date");
					AssertStatisticsForTheField("PRECONDITION: Is statistics for the field auto_stats_lob updated?", now, false, testDbName, Table1, fieldName: "auto_stats_lob");

					using (Db.Connection.TemporarySetLockTimeout(TimeSpan.FromMilliseconds(1)))
					{
						updater.Run(Db.Connection, new string[] { testDbName });
						now = DateTime.Now;
					}

					CombineAssertions(() =>
					{
						var fullLog = logger.ToString();
						AssertContains($"Information|Updating Index Statistics: DB [{testDbName}]", fullLog);
						AssertContains($"Information|Updating Index Statistics: DB [{testDbName}] - Table/View [dbo].[_Table1]", fullLog);
						AssertContains($"Information|Lock request time out period exceeded.", fullLog);

						AssertStatisticsForTheField("Is statistics for the field index_stats updated?", now, false, testDbName, Table1, fieldName: "index_stats");
						AssertStatisticsForTheField("Is statistics for the field user_stats updated?", now, false, testDbName, Table1, fieldName: "user_stats");
						AssertStatisticsForTheField("Is statistics for the field auto_stats_int updated?", now, false, testDbName, Table1, fieldName: "auto_stats_int");
						AssertStatisticsForTheField("Is statistics for the field auto_stats_date updated?", now, false, testDbName, Table1, fieldName: "auto_stats_date");
						AssertStatisticsForTheField("Is statistics for the field auto_stats_lob updated?", now, false, testDbName, Table1, fieldName: "auto_stats_lob");
					});
				}
				finally
				{
					Task.WaitAll(task);
					DbWorker.DropTestDbIfExists(testDbName);
				}
			}

			public void TestAutoStatsNotChangedDuringUpdate()
			{
				var testDbName = "DbForStatisticsUpdateTest_E82AF4A831B243658187E087BF343EDE";

				var testLogger = new TestServiceLogger();
				var testUpdater = new IndexStatisticsUpdaterForTest(testLogger);
				testUpdater.DaysOfLastUpdateToAcceptAsValid_Exposed = 0;

				PrepareTestData(testDbName);

				StatisticsSwitch.Instance.SetDbStatisticsSettings(testDbName, true);

				try
				{
					AssertEquals("Before update: AutoStats is ON", "ON-ON-ON", StatisticsSwitch.Instance.GetStatisticsSettings(testDbName).Current.ToString());

					var statsStates = new List<string>();
					testUpdater.UserActionUpdate_ForTest = () => statsStates.Add(StatisticsSwitch.Instance.GetStatisticsSettings(testDbName).Current.ToString());

					testUpdater.Run(Db.Connection, new string[] { testDbName });

					AssertEquals("During update: AutoStats is OFF", "ON-ON-ON", string.Join(";", statsStates.Distinct()));
					AssertEquals("After update: AutoStats is ON", "ON-ON-ON", StatisticsSwitch.Instance.GetStatisticsSettings(testDbName).Current.ToString());
				}
				finally
				{
					DbWorker.DropTestDbIfExists(testDbName);
				}
			}

			public void TestNoRecomputeIsOnForLOB()
			{
				// switch no_recompute to ON for stats created for BLOB columns

				var testDbName = "DbForStatisticsUpdateTest_CC8D22AB140F47FD91ABA77B8CD94303";

				var testLogger = new TestServiceLogger();
				var testUpdater = new IndexStatisticsUpdaterForTest(testLogger);
				testUpdater.DaysOfLastUpdateToAcceptAsValid_Exposed = 0;

				PrepareTestData(testDbName);
				var lobCols = GetLOBColNames(testDbName, "_Table1");

				try
				{
					lobCols.ForEach(col =>
					{
						AssertStatisticsNoRecompute($"PRECONDITION: Is No_Recompute for the field {col} on?", false, testDbName, Table1, fieldName: col);
					});

					testUpdater.Run(Db.Connection, new string[] { testDbName });

					// _Table1 was updated
					AssertContains($"Information|Updating Index Statistics: DB [{testDbName}] - Table/View [dbo].[_Table1]", testLogger.ToString());

					lobCols.ForEach(col =>
					{
						AssertStatisticsNoRecompute($"PRECONDITION: Is No_Recompute for the field {col} on?", true, testDbName, Table1, fieldName: col);
					});
				}
				finally
				{
					DbWorker.DropTestDbIfExists(testDbName);
				}
			}

			public void TestCommonStatsCreatedForFilteredIndex()
			{
				// Create common stats with default sample, norecompute

				var testDbName = "DbForStatisticsUpdateTest_278B9EA87BD1414C8B4DF93DD7313D7D";

				var testLogger = new TestServiceLogger();
				var testUpdater = new IndexStatisticsUpdaterForTest(testLogger);
				testUpdater.DaysOfLastUpdateToAcceptAsValid_Exposed = 0;

				PrepareTestData(testDbName);
				Db.Connection.ExecuteNonQuery($"ALTER TABLE {testDbName.QuoteName()}.dbo._Table1 ADD filtered_index_stats int NULL;");
				Db.Connection.ExecuteNonQuery($"CREATE INDEX _filtered_index_stats on {testDbName.QuoteName()}.dbo._Table1(filtered_index_stats) WHERE filtered_index_stats is NOT NULL;");
				Db.Connection.ExecuteNonQuery($"INSERT {testDbName.QuoteName()}.dbo._Table1 DEFAULT VALUES;");

				try
				{
					AssertStatisticsName("PRECONDITION: No common stats for the field filtered_index_stats", "_filtered_index_stats", testDbName, Table1, fieldName: "filtered_index_stats");

					testUpdater.Run(Db.Connection, new string[] { testDbName });

					AssertStatisticsName("The stats for the field filtered_index_stats are index and common stats only", "_filtered_index_stats; WTG_filtered_index_stats", testDbName, Table1, fieldName: "filtered_index_stats");
				}
				finally
				{
					DbWorker.DropTestDbIfExists(testDbName);
				}
			}

			public void TestUpdateSample_EmptyFilteredIndex()
			{
				// No rows. filtered: fullscan, common: fullscan

				var testDbName = "DbForStatisticsUpdateTest_87A96AD90E414D0BAEBD0500A6F10F66";

				var testLogger = new TestServiceLogger();
				var testUpdater = new IndexStatisticsUpdaterForTest(testLogger);
				testUpdater.DaysOfLastUpdateToAcceptAsValid_Exposed = 0;

				PrepareTestData(testDbName);
				Db.Connection.ExecuteNonQuery($"ALTER TABLE {testDbName.QuoteName()}.dbo._Table1 ADD filtered_index_stats int NULL;");
				Db.Connection.ExecuteNonQuery($"CREATE INDEX _filtered_index_stats on {testDbName.QuoteName()}.dbo._Table1(filtered_index_stats) WHERE filtered_index_stats is NOT NULL;");
				Db.Connection.ExecuteNonQuery($"CREATE STATISTICS [WTG_filtered_index_stats] ON {testDbName.QuoteName()}.dbo._Table1 (filtered_index_stats)");
				Db.Connection.ExecuteNonQuery($"INSERT {testDbName.QuoteName()}.dbo._Table1 DEFAULT VALUES;");

				StatisticsSwitch.Instance.SetDbStatisticsSettings(testDbName, isON: false);

				try
				{
					AssertStatisticsName("PRECONDITION: The stats for the field filtered_index_stats are index and common stats only", "_filtered_index_stats; WTG_filtered_index_stats", testDbName, Table1, fieldName: "filtered_index_stats");
					AssertStatisticsSample("PRECONDITION: Filtered index stats", testUpdater, "_filtered_index_stats", 0, 0, true, testDbName, Table1);
					AssertStatisticsSample("PRECONDITION: Common stats for filtered index", testUpdater, "WTG_filtered_index_stats", 2, 2, true, testDbName, Table1);

					Db.Connection.ExecuteNonQuery($@"-- Populate table
;WITH
	L0 AS(SELECT n = NULL FROM(VALUES(NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL)) AS t(n)),
	L1 AS (SELECT n = NULL FROM L0 AS A CROSS JOIN L0 AS B),
	L2 AS (SELECT n = NULL FROM L1 AS A CROSS JOIN L1 AS B),
	L3 AS (SELECT n = NULL FROM L2 AS A CROSS JOIN L2 AS B)
INSERT
	{testDbName.QuoteName()}.dbo._Table1 WITH(TABLOCKX) (filtered_index_stats)
SELECT TOP (200000)
	filtered_index_stats = NULL
FROM
	L3;

"
						);

					testUpdater.Run(Db.Connection, new string[] { testDbName });

					AssertStatisticsSample("Filtered index stats", testUpdater, "_filtered_index_stats", 0, 0, true, testDbName, Table1);
					AssertStatisticsSample("Common stats for filtered index", testUpdater, "WTG_filtered_index_stats", 200003, 200003, false, testDbName, Table1);
				}
				finally
				{
					DbWorker.DropTestDbIfExists(testDbName);
				}
			}

			public void TestUpdateSample_SmallFilteredIndex()
			{
				// A few rows. filtered: fullscan, common: default sample (auto or even OFF)

				var testDbName = "DbForStatisticsUpdateTest_87A96AD90E414D0BAEBD0500A6F10F66";

				var testLogger = new TestServiceLogger();
				var testUpdater = new IndexStatisticsUpdaterForTest(testLogger);
				testUpdater.DaysOfLastUpdateToAcceptAsValid_Exposed = 0;

				PrepareTestData(testDbName);
				Db.Connection.ExecuteNonQuery($"ALTER TABLE {testDbName.QuoteName()}.dbo._Table1 ADD filtered_index_stats int NULL;");
				Db.Connection.ExecuteNonQuery($"CREATE INDEX _filtered_index_stats on {testDbName.QuoteName()}.dbo._Table1(filtered_index_stats) WHERE filtered_index_stats is NOT NULL;");
				Db.Connection.ExecuteNonQuery($"CREATE STATISTICS [WTG_filtered_index_stats] ON {testDbName.QuoteName()}.dbo._Table1 (filtered_index_stats)");
				Db.Connection.ExecuteNonQuery($"INSERT {testDbName.QuoteName()}.dbo._Table1 DEFAULT VALUES;");

				StatisticsSwitch.Instance.SetDbStatisticsSettings(testDbName, isON: false);

				try
				{
					AssertStatisticsName("PRECONDITION: The stats for the field filtered_index_stats are index and common stats only", "_filtered_index_stats; WTG_filtered_index_stats", testDbName, Table1, fieldName: "filtered_index_stats");
					AssertStatisticsSample("PRECONDITION: Filtered index stats", testUpdater, "_filtered_index_stats", 0, 0, true, testDbName, Table1);
					AssertStatisticsSample("PRECONDITION: Common stats for filtered index", testUpdater, "WTG_filtered_index_stats", 2, 2, true, testDbName, Table1);

					Db.Connection.ExecuteNonQuery($@"-- Populate table
;WITH
	L0 AS(SELECT n = NULL FROM(VALUES(NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL)) AS t(n)),
	L1 AS (SELECT n = NULL FROM L0 AS A CROSS JOIN L0 AS B),
	L2 AS (SELECT n = NULL FROM L1 AS A CROSS JOIN L1 AS B),
	L3 AS (SELECT n = NULL FROM L2 AS A CROSS JOIN L2 AS B)
INSERT
	{testDbName.QuoteName()}.dbo._Table1 WITH(TABLOCKX) (filtered_index_stats)
SELECT TOP (200000)
	filtered_index_stats = NULL
FROM
	L3;

INSERT {testDbName.QuoteName()}.dbo._Table1 (filtered_index_stats) VALUES (1);

"
						);

					testUpdater.Run(Db.Connection, new string[] { testDbName });

					AssertStatisticsSample("Filtered index stats", testUpdater, "_filtered_index_stats", 1, 200004, true, testDbName, Table1);
					AssertStatisticsSample("Common stats for filtered index", testUpdater, "WTG_filtered_index_stats", 200004, 200004, false, testDbName, Table1);
				}
				finally
				{
					DbWorker.DropTestDbIfExists(testDbName);
				}
			}

			public void TestUpdateSample_LargeFilteredIndex()
			{
				// A lot of rows. filtered: fullscan with fallback to default sample, common: default sample

				var testDbName = "DbForStatisticsUpdateTest_87A96AD90E414D0BAEBD0500A6F10F66";

				var testLogger = new TestServiceLogger();
				var testUpdater = new IndexStatisticsUpdaterForTest(testLogger);
				testUpdater.DaysOfLastUpdateToAcceptAsValid_Exposed = 0;

				PrepareTestData(testDbName);
				Db.Connection.ExecuteNonQuery($"ALTER TABLE {testDbName.QuoteName()}.dbo._Table1 ADD filtered_index_stats int NULL;");
				Db.Connection.ExecuteNonQuery($"CREATE INDEX _filtered_index_stats on {testDbName.QuoteName()}.dbo._Table1(filtered_index_stats) WHERE filtered_index_stats is NOT NULL;");
				Db.Connection.ExecuteNonQuery($"CREATE STATISTICS [WTG_filtered_index_stats] ON {testDbName.QuoteName()}.dbo._Table1 (filtered_index_stats)");
				Db.Connection.ExecuteNonQuery($"INSERT {testDbName.QuoteName()}.dbo._Table1 DEFAULT VALUES;");

				StatisticsSwitch.Instance.SetDbStatisticsSettings(testDbName, isON: false);

				try
				{
					AssertStatisticsName("PRECONDITION: The stats for the field filtered_index_stats are index and common stats only", "_filtered_index_stats; WTG_filtered_index_stats", testDbName, Table1, fieldName: "filtered_index_stats");
					AssertStatisticsSample("PRECONDITION: Filtered index stats", testUpdater, "_filtered_index_stats", 0, 0, true, testDbName, Table1);
					AssertStatisticsSample("PRECONDITION: Common stats for filtered index", testUpdater, "WTG_filtered_index_stats", 2, 2, true, testDbName, Table1);

					Db.Connection.ExecuteNonQuery($@"-- Populate table
;WITH
	L0 AS(SELECT n = NULL FROM(VALUES(NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL)) AS t(n)),
	L1 AS (SELECT n = NULL FROM L0 AS A CROSS JOIN L0 AS B),
	L2 AS (SELECT n = NULL FROM L1 AS A CROSS JOIN L1 AS B),
	L3 AS (SELECT n = NULL FROM L2 AS A CROSS JOIN L2 AS B)
INSERT
	{testDbName.QuoteName()}.dbo._Table1 WITH(TABLOCKX) (filtered_index_stats)
SELECT TOP (200000)
	filtered_index_stats = 1
FROM
	L3;

"
						);

					testUpdater.Run(Db.Connection, new string[] { testDbName });

					AssertStatisticsSample("Filtered index stats", testUpdater, "_filtered_index_stats", 200000, 200003, true, testDbName, Table1);
					AssertStatisticsSample("Common stats for filtered index", testUpdater, "WTG_filtered_index_stats", 200003, 200003, false, testDbName, Table1);
				}
				finally
				{
					DbWorker.DropTestDbIfExists(testDbName);
				}
			}

			public void TestUpdate_FilteredIndexesStats()
			{
				var testDbName = "DbForStatisticsUpdateTest_7F1983682EDB4DC9A5ACBBF74139AB58";
				var testLogger = new TestServiceLogger();
				var testUpdater = new IndexStatisticsUpdaterForTest(testLogger);
				testUpdater.DaysOfLastUpdateToAcceptAsValid_Exposed = 0;
				var guidForSmallIndex = Guid.NewGuid().ToString();
				var guidForBigIndex = Guid.NewGuid().ToString();

				PrepareTestData(testDbName);

				Db.Connection.ExecuteNonQuery($"ALTER TABLE {testDbName.QuoteName()}.dbo._Table1 ADD foreign_key uniqueidentifier NULL;");
				Db.Connection.ExecuteNonQuery($"CREATE INDEX _small_foreign_key_filtered_index on {testDbName.QuoteName()}.dbo._Table1(foreign_key) WHERE foreign_key = '{guidForSmallIndex}';");
				Db.Connection.ExecuteNonQuery($"CREATE INDEX _big_foreign_key_filtered_index on {testDbName.QuoteName()}.dbo._Table1(foreign_key) WHERE foreign_key = '{guidForBigIndex}';");
				using (((ICurrentDbControl)Db.Connection).UseDatabase(testDbName))
				{
					Db.Connection.ExecuteNonQuery($"EXEC sys.sp_autostats N'dbo._Table1', 'OFF', _big_foreign_key_filtered_index;");
				}

				StatisticsSwitch.Instance.SetDbStatisticsSettings(testDbName, isON: false);

				try
				{
					AssertStatisticsName("PRECONDITION: The stats for the field filtered_index_stats are 2 indexes", "_big_foreign_key_filtered_index; _small_foreign_key_filtered_index", testDbName, Table1, fieldName: "foreign_key");
					AssertStatisticsSample("PRECONDITION: Small filtered index stats", testUpdater, "_small_foreign_key_filtered_index", 0, 0, true, testDbName, Table1);
					AssertStatisticsSample("PRECONDITION: Big filtered index stats", testUpdater, "_big_foreign_key_filtered_index", 0, 0, true, testDbName, Table1);
					AssertAutoUpdateStatsIsOnOrOff("PRECONDITION: Small filtered index stats", "_small_foreign_key_filtered_index", true, testDbName);
					AssertAutoUpdateStatsIsOnOrOff("PRECONDITION: Big filtered index stats", "_big_foreign_key_filtered_index", false, testDbName);

					var smallIndexRows = testUpdater.EmptyStatisticsRowsThreshold - 10;
					var bigIndexRows = testUpdater.EmptyStatisticsRowsThreshold + 10;
					var totalRows = smallIndexRows + bigIndexRows + 2;

					Db.Connection.ExecuteNonQuery($@"-- Populate table
;WITH
	L0 AS(SELECT n = NULL FROM(VALUES(NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL)) AS t(n)),
	L1 AS (SELECT n = NULL FROM L0 AS A CROSS JOIN L0 AS B),
	L2 AS (SELECT n = NULL FROM L1 AS A CROSS JOIN L1 AS B),
	L3 AS (SELECT n = NULL FROM L2 AS A CROSS JOIN L2 AS B)
INSERT
	{testDbName.QuoteName()}.dbo._Table1 WITH(TABLOCKX) (foreign_key)
SELECT TOP ({smallIndexRows}) foreign_key = '{guidForSmallIndex}' FROM L3
union all
SELECT TOP ({bigIndexRows}) foreign_key = '{guidForBigIndex}' FROM L3;
"
						);

					testUpdater.Run(Db.Connection, new string[] { testDbName });
					AssertStatisticsSample("Small filtered index stats", testUpdater, "_small_foreign_key_filtered_index", smallIndexRows, totalRows, true, testDbName, Table1);
					AssertStatisticsSample("Big filtered index stats", testUpdater, "_big_foreign_key_filtered_index", bigIndexRows, totalRows, true, testDbName, Table1);
					AssertAutoUpdateStatsIsOnOrOff("Small filtered index stats changed", "_small_foreign_key_filtered_index", false, testDbName);
					AssertAutoUpdateStatsIsOnOrOff("Big filtered index stats changed", "_big_foreign_key_filtered_index", true, testDbName);

					Db.Connection.ExecuteNonQuery($@"-- Make Changes
INSERT into {testDbName.QuoteName()}.dbo._Table1 WITH(TABLOCKX) (foreign_key) VALUES ('{guidForSmallIndex}')
INSERT into {testDbName.QuoteName()}.dbo._Table1 WITH(TABLOCKX) (foreign_key) VALUES ('{guidForBigIndex}')
");

					testUpdater = new IndexStatisticsUpdaterForTest(testLogger) { DaysOfLastUpdateToAcceptAsValid_Exposed = 0, };

					testUpdater.Run(Db.Connection, new string[] { testDbName });
					AssertStatisticsSample("Small filtered index stats", testUpdater, "_small_foreign_key_filtered_index", smallIndexRows + 1, totalRows + 2, true, testDbName, Table1);
					AssertStatisticsSample("Big filtered index stats", testUpdater, "_big_foreign_key_filtered_index", bigIndexRows + 1, totalRows + 2, true, testDbName, Table1);
					AssertAutoUpdateStatsIsOnOrOff("Small filtered index stats not changed", "_small_foreign_key_filtered_index", false, testDbName);
					AssertAutoUpdateStatsIsOnOrOff("Big filtered index stats not changed", "_big_foreign_key_filtered_index", true, testDbName);
				}
				finally
				{
					DbWorker.DropTestDbIfExists(testDbName);
				}
			}

			void AssertAutoUpdateStatsIsOnOrOff(string message, string statsName, bool expected, string dbName)
			{
				using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
				{
					var actual = !Db.Connection.ExecuteScalar<bool>(
						$@"SELECT no_recompute FROM sys.stats WHERE name = @stats_name"
						, (cmd) =>
						{
							cmd.AddParameter("@stats_name", SqlDbType.NVarChar, 128, statsName);
						}
					);
					AssertEquals(message + "\r\nAuto Update Stats Incorrect", expected, actual);
				}
			}

			public void TestSetAutoStatsForFilteredIndexes()
			{
				// Arrange
				const string dbName = "TestSetAutoStatsForFilteredIndexes";
				using (AdoTestUtils.CreateDbDropExistingDisposable(dbName, Db.DatabaseName))
				using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
				{
					CreateTestTable2(Db.Connection, dbName);
					AssertEquals("ON", GetAutoStats(Db.Connection, "[dbo].[TestTable2]", "RX__Count"));

					const string statsToUpdate = "EXEC sys.sp_autostats N'[dbo].[TestTable2]', 'OFF', [RX__Count];";

					var logger = new Mock<ILogger>();
					logger.Setup(x => x.Log(LogType.Information, It.IsAny<string>())).Verifiable();

					var updater = new IndexStatisticsUpdaterForTest(logger.Object);

					// Act
					updater.SetAutoStatsForFilteredIndexes_Exposed(Db.Connection);

					// Assert
					CombineAssertions(() =>
					{
						logger.Verify(x => x.Log(LogType.Information, $"Checking autostats for filtered indexes (threshold: {updater.EmptyStatisticsRowsThreshold:N0} rows)"), Times.Once);
						logger.Verify(x => x.Log(LogType.Information, statsToUpdate), Times.Once);
						logger.VerifyNoOtherCalls();

						AssertEquals("OFF", GetAutoStats(Db.Connection, "[dbo].[TestTable2]", "RX__Count"));
					});
				}
			}

			public void TestSetAutoStatsForFilteredIndexes_LockTimeoutException()
			{
				// Arrange
				const string dbName = "TestSetAutoStatsForFilteredIndexes";
				using (AdoTestUtils.CreateDbDropExistingDisposable(dbName, Db.DatabaseName))
				using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
				{
					CreateTestTable2(Db.Connection, dbName);
					AssertEquals("ON", GetAutoStats(Db.Connection, "[dbo].[TestTable2]", "RX__Count"));

					const string statsToUpdate = "EXEC sys.sp_autostats N'[dbo].[TestTable2]', 'OFF', [RX__Count];";

					var logger = new Mock<ILogger>();
					logger.Setup(x => x.Log(LogType.Information, It.IsAny<string>())).Verifiable();
					logger.Setup(
						x => x.Log(
							LogType.Warning,
							$"Cannot apply the following changes: \"{statsToUpdate}\" due to blocking issue. Consider to run ISU at the most quiet time"))
						.Verifiable();

					using (MakeBlockingSituationOnTestTable2(dbName))
					{
						var updater = new IndexStatisticsUpdaterForTest(logger.Object);
						updater.UserActionSetAutoStatsUpdate_ForTest = (cmd) =>
						{
							AssertEquals(
								"Lock timeout used in connection is 4 seconds",
								4000,
								Db.Connection.ExecuteScalar<int>("SELECT @@LOCK_TIMEOUT"));
						};

						// Act
						updater.SetAutoStatsForFilteredIndexes_Exposed(Db.Connection);

						// Assert
						CombineAssertions(() =>
						{
							logger.Verify(x => x.Log(LogType.Information, $"Checking autostats for filtered indexes (threshold: {updater.EmptyStatisticsRowsThreshold:N0} rows)"), Times.Once);
							logger.Verify(x => x.Log(LogType.Information, statsToUpdate), Times.Exactly(1));
							logger.Verify(x => x.Log(LogType.Warning, $"Cannot apply the following changes: \"{statsToUpdate}\" due to blocking issue. Consider to run ISU at the most quiet time"), Times.Once);
							logger.VerifyNoOtherCalls();

							AssertEquals("ON", GetAutoStats(Db.Connection, "[dbo].[TestTable2]", "RX__Count"));
						});
					}
				}
			}

			public void TestSetAutoStatsForFilteredIndexes_CommandTimeoutException()
			{
				// Arrange
				const string dbName = "TestSetAutoStatsForFilteredIndexes";
				using (AdoTestUtils.CreateDbDropExistingDisposable(dbName, Db.DatabaseName))
				using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
				{
					CreateTestTable2(Db.Connection, dbName);
					AssertEquals("ON", GetAutoStats(Db.Connection, "[dbo].[TestTable2]", "RX__Count"));

					const string statsToUpdate = "EXEC sys.sp_autostats N'[dbo].[TestTable2]', 'OFF', [RX__Count];";

					var logger = new Mock<ILogger>();
					logger.Setup(x => x.Log(LogType.Information, It.IsAny<string>())).Verifiable();
					logger.Setup(
						x => x.Log(
							LogType.Warning,
							$"Cannot apply the following changes: \"{statsToUpdate}\" due to blocking issue. Consider to run ISU at the most quiet time"))
						.Verifiable();

					// adjust timeout to 1 second to make command timeout exception
					using (Db.Connection.TemporarySetDefaultCommandTimeOut(1))
					using (MakeBlockingSituationOnTestTable2(dbName))
					{
						var updater = new IndexStatisticsUpdaterForTest(logger.Object);

						// Act
						updater.SetAutoStatsForFilteredIndexes_Exposed(Db.Connection);

						// Assert
						CombineAssertions(() =>
						{
							logger.Verify(x => x.Log(LogType.Information, $"Checking autostats for filtered indexes (threshold: {updater.EmptyStatisticsRowsThreshold:N0} rows)"), Times.Once);
							logger.Verify(x => x.Log(LogType.Information, statsToUpdate), Times.Exactly(1));
							logger.Verify(x => x.Log(LogType.Warning, $"Cannot apply the following changes: \"{statsToUpdate}\" due to blocking issue. Consider to run ISU at the most quiet time"), Times.Once);
							logger.VerifyNoOtherCalls();

							AssertEquals("ON", GetAutoStats(Db.Connection, "[dbo].[TestTable2]", "RX__Count"));
						});
					}
				}
			}

			static string GetAutoStats(DbConnection connection, string tableName, string indexName)
			{
				return connection.ExecuteScalar<string>($@"
SELECT
	AUTOSTATS = IIF(s.no_recompute = 1, 'OFF', 'ON')
FROM
	sys.stats AS s
WHERE 1=1
	AND s.object_id = OBJECT_ID(@tableName, N'U')
	AND s.name = @indexName
",
					cmd =>
					{
						cmd.AddParameter("@tableName", SqlDbType.NVarChar, 400, tableName);
						cmd.AddParameter("@indexName", SqlDbType.NVarChar, 128, indexName);
					});
			}

			static IDisposable MakeBlockingSituationOnTestTable2(string dbName)
			{
				var cw1Connection = Db.NewAdminConnection(dbName);
				cw1Connection.BeginTransaction();
				cw1Connection.ExecuteNonQuery(@"
INSERT INTO [dbo].[TestTable2] VALUES (NEWID(), 1);
");

				return new DisposableAction(() =>
				{
					cw1Connection.RollbackTransaction();
					cw1Connection.Dispose();
				});
			}

			static void CreateTestTable2(DbConnection connection, string dbName)
			{
				using (((ICurrentDbControl)connection).UseDatabase(dbName))
				{
					var sql = @"
DROP TABLE IF EXISTS dbo.TestTable2;

CREATE TABLE dbo.TestTable2
(
	PK             UNIQUEIDENTIFIER NOT NULL,
	COUNT          INT,
);

CREATE NONCLUSTERED INDEX [RX__Count] ON [dbo].[TestTable2] (Count ASC)
WHERE Count > 0;
";

					connection.ExecuteNonQuery(sql);
				}
			}

			public void TestSetAutoStatsForFilteredIndexes_RetryWhenDeadlockException()
			{
				var statsToUpdate = "EXEC sys.sp_autostats N'[dbo].[TestTable]', 'OFF', [NR_RX__TT_Col];";
				var logger = new TestServiceLogger();
				var updater = new IndexStatisticsUpdaterForTest(logger);
				updater.UserActionSetAutoStatsGetData_ForTest = (cmd, stmts) =>
				{
					stmts.Enqueue(statsToUpdate);
				};
				var firstAttempt = true;
				updater.UserActionSetAutoStatsUpdate_ForTest = (cmd) =>
				{
					if (firstAttempt)
					{
						firstAttempt = false;
						throw SqlExceptionBuilder.CreateSqlException(
							SqlExceptionBuilder.CreateSqlErrorCollection(
								SqlExceptionBuilder.CreateSqlError(1205, 51, 13, Db.ServerName, $"Transaction (Process ID {Db.Connection.SPID}) was deadlocked on resources with another process and has been chosen as the deadlock victim. Rerun the transaction.", string.Empty, 0)));
					}
					else
					{
						throw new OperationCanceledException("Operation canceled by user");
					}
				};

				AssertExceptionThrown<OperationCanceledException>(() =>
				{
					updater.SetAutoStatsForFilteredIndexes_Exposed(Db.Connection);
				});

				CombineAssertions(() =>
				{
					AssertContains("Stats re-queued", "Re-queued due to deadlock: " + statsToUpdate, logger.ToString(), ignoreCase: true);
				});
			}

			public void TestSimultaneousUpdatesOnSharedDatabase()
			{
				string dbName = RefDbTableNameResolver.SharedDbPrefix + RefDbTableNameResolver.RefDbAffix + "_ISU_Stats";
				Task task = null;

				try
				{
					DbWorker.CreateTestDb(dbName, createTables: false);
					using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
					{
						Db.Connection.ExecuteNonQuery(ExtProperty.TableDefinition);
						Db.Connection.ExecuteNonQuery($"EXEC sys.sp_autostats N'{ExtProperty.TableName}', 'OFF';");
					}

					using (var readyToRun_1 = new AutoResetEvent(initialState: false))
					using (var hasRun_2 = new AutoResetEvent(initialState: false))
					{
						var updater_1_has_run = false;
						var logger_1 = new TestServiceLogger();
						var updater_1 = new IndexStatisticsUpdaterForTest(logger_1);
						updater_1.UserActionPreUpdate_ForTest = () =>
						{
							updater_1_has_run = true;
							readyToRun_1.Set();
							hasRun_2.WaitOne();
						};

						var updater_2_has_run = false;
						var logger_2 = new TestServiceLogger();
						var updater_2 = new IndexStatisticsUpdaterForTest(logger_2);
						updater_2.UserActionPreUpdate_ForTest = () =>
						{
							updater_2_has_run = true;
						};

						task = Task.Run(() =>
						{
							using (Db.DisposableActionForDbConnection())
							using (var connection_1 = Db.NewAdminConnection())
							{
								updater_1.Run(connection_1, new string[] { dbName });
							}
						});

						using (var connection_2 = Db.NewAdminConnection())
						{
							readyToRun_1.WaitOne();
							updater_2.Run(connection_2, new string[] { dbName });
							hasRun_2.Set();
						}

						AssertEquals("Has updater_1 run?", true, updater_1_has_run);
						AssertEquals("Has updater_2 run?", false, updater_2_has_run);
					}
				}
				finally
				{
					Task.WaitAll(task);
					DbWorker.DropTestDbIfExists(dbName);
				}
			}

			#region Supported Index Stats

			/* Summary
			// Supported Index TYPE:
			//   1 => Clustered
			//   2 => Nonclustered
			// Not supported Index TYPE:
			//   0 => Heap
			//   3 => XML                      -- does not generate stats
			//   4 => Spatial                  -- does not generate stats
			//   5 => Columnstore clustered    -- generates empty stats
			//   6 => Columnstore nonclustered -- generates empty stats
			*/

			public void TestSupported_1_Clustered()
			{
				var dbName = "DbForStatisticsUpdateTest_2D39B63109244DEFBA41A50B47DBB373";

				var sql = @"
if (OBJECT_ID(N'dbo._Table1', N'U') is NOT NULL) DROP TABLE dbo._Table1;
CREATE TABLE dbo._Table1
(
	index_stats int,
);

INSERT dbo._Table1 DEFAULT VALUES;

-- create index stats
CREATE CLUSTERED INDEX _index_stats ON dbo._Table1 (index_stats);

INSERT dbo._Table1 DEFAULT VALUES;
";

				AssertSupportedStats(dbName, Table1, sql, true);
			}

			public void TestSupported_2_Nonclustered()
			{
				var dbName = "DbForStatisticsUpdateTest_2E1D159157244F13809961A55E22B0A3";

				var sql = @"
if (OBJECT_ID(N'dbo._Table1', N'U') is NOT NULL) DROP TABLE dbo._Table1;
CREATE TABLE dbo._Table1
(
	index_stats int,
);

INSERT dbo._Table1 DEFAULT VALUES;

-- create index stats
CREATE NONCLUSTERED INDEX _index_stats ON dbo._Table1 (index_stats);

INSERT dbo._Table1 DEFAULT VALUES;
";

				AssertSupportedStats(dbName, Table1, sql, true);
			}

			public void TestSupported_5_Columnstore_Clustered()
			{
				var dbName = "DbForStatisticsUpdateTest_D68B488D46744266B8E35DDECB7105D6";

				var sql = @"
if (OBJECT_ID(N'dbo._Table1', N'U') is NOT NULL) DROP TABLE dbo._Table1;
CREATE TABLE dbo._Table1
(
	index_stats int,
);

INSERT dbo._Table1 DEFAULT VALUES;

-- create index stats
CREATE CLUSTERED COLUMNSTORE INDEX _index_stats ON dbo._Table1;

INSERT dbo._Table1 DEFAULT VALUES;
";

				AssertSupportedStats(dbName, Table1, sql, false);
			}

			public void TestSupported_6_Columnstore_Nonclustered()
			{
				var dbName = "DbForStatisticsUpdateTest_E2D4252653464420AA3B45A100745D1E";

				var sql = @"
if (OBJECT_ID(N'dbo._Table1', N'U') is NOT NULL) DROP TABLE dbo._Table1;
CREATE TABLE dbo._Table1
(
	index_stats int,
);

INSERT dbo._Table1 DEFAULT VALUES;

-- create index stats
CREATE NONCLUSTERED COLUMNSTORE INDEX _index_stats ON dbo._Table1 (index_stats);

INSERT dbo._Table1 DEFAULT VALUES;
";

				AssertSupportedStats(dbName, Table1, sql, false);
			}

			void AssertSupportedStats(string dbName, string tableName, string sql, bool expected)
			{
				var logger = new TestServiceLogger();
				var updater = new IndexStatisticsUpdaterForTest(logger)
				{
					DaysOfLastUpdateToAcceptAsValid_Exposed = 0
				};

				AdoTestUtils.CreateDbDropExisting(dbName, Db.DatabaseName);

				using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
				{
					Db.Connection.ExecuteNonQuery(sql);
				}

				Thread.Sleep(1000);
				var now = DateTime.Now;

				try
				{
					AssertStatisticsForTheField("PRECONDITION: Is statistics for the field index_stats updated?", now, false, dbName, Table1, fieldName: "index_stats");

					updater.Run(Db.Connection, new string[] { dbName });
					now = DateTime.Now;

					// _Table1 was updated
					if (expected)
					{
						AssertContains($"Information|Updating Index Statistics: DB [{dbName}] - Table/View [dbo].[_Table1]", logger.ToString());
					}
					else
					{
						AssertNotContains($"Information|Updating Index Statistics: DB [{dbName}] - Table/View [dbo].[_Table1]", logger.ToString());
					}

					AssertStatisticsForTheField("Is statistics for the field index_stats updated?", now, expected, dbName, Table1, fieldName: "index_stats");
				}
				finally
				{
					DbWorker.DropTestDbIfExists(dbName);
				}
			}

			#endregion // Supported Index Stats
		}

		#region Implementation

		void PrepareTestData(string dbName, string tableName = Table1)
		{
			using (var connection = Db.NewAdminConnection())
			{
				connection.CreateDatabase(dbName);

				// Ensure DB AutoCreateStats is ON
				StatisticsSwitch.Instance.SetDbStatisticsSettings(connection, dbName, autoCreateON: true, autoUpdateON: true, autoUpdateAsyncON: true);
			}

			using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
			{
				var sql = Invariant($@"
DROP TABLE IF EXISTS dbo.[{tableName}];
CREATE TABLE dbo.[{tableName}]
(
	index_stats          int,

	index_stats_M1       bigint,  -- index column included in multi columns index
	index_stats_M2       int,     -- index column included in multi columns index

	index_stats_Filtered date,    -- filtered index column

	user_stats           int,     -- WTG created stats on single column
	user_stats_M1        bigint,  -- WTG created stats on multi columns
	user_stats_M2        int,     -- WTG created stats on multi columns

	auto_stats_int       int,
	auto_stats_date      date,
	auto_stats_lob       varchar(max),
	auto_stats_lob2      nvarchar(max),

	stats_no_recompute   int,
);

INSERT dbo.[{tableName}] DEFAULT VALUES;

-- create index stats
CREATE INDEX _index_stats on dbo.[{tableName}](index_stats);
CREATE INDEX _index_stats_M on dbo.[{tableName}] (index_stats_M1, index_stats_M2);
CREATE INDEX _index_stats_Filtered on dbo.[{tableName}] (index_stats_Filtered) WHERE index_stats_Filtered is NULL;

-- create user stats
CREATE STATISTICS _user_stats on dbo.[{tableName}](user_stats);
CREATE STATISTICS _user_stats_M on dbo.[{tableName}](user_stats_M1, user_stats_M2);

-- create no_recompute stats
CREATE STATISTICS _stats_no_recompute on dbo.[{tableName}](stats_no_recompute) WITH FULLSCAN, NORECOMPUTE;

-- create auto stats for int
SELECT TOP(1) * FROM dbo.[{tableName}] WHERE auto_stats_int is NULL;

-- create auto stats for date
SELECT TOP(1) * FROM dbo.[{tableName}] WHERE auto_stats_date is NULL or index_stats_Filtered is NULL;

-- create auto stats for LOB
SELECT TOP(1) * FROM dbo.[{tableName}] WHERE auto_stats_lob is NULL;
SELECT TOP(1) * FROM dbo.[{tableName}] WHERE auto_stats_lob2 is NULL;

INSERT dbo.[{tableName}] DEFAULT VALUES;
");

				_ = Db.Connection.ExecuteNonQuery(sql);
			}
		}

		List<(string TableName, string StatsName)> GetAutoStatsName(string dbName)
		{
			const string sql = @"
SELECT 
    table_name = OBJECT_NAME(s.object_id)
    , stats_name = s.name
FROM
    sys.stats              AS s
    JOIN sys.objects       AS obj    ON s.object_id = obj.object_id
    INNER JOIN 
        sys.stats_columns  AS sc
            ON s.object_id = sc.object_id AND s.stats_id = sc.stats_id
    INNER JOIN 
        sys.columns        AS col
            ON sc.object_id = col.object_id AND sc.column_id = col.column_id
WHERE 1=1
    AND s.auto_created = 1
    AND obj.is_ms_shipped = 0
ORDER BY 
    table_name
";
			var autoStats = new List<(string TableName, string StatsName)>();
			using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
			{
				Db.Connection.ExecuteReader(sql, (reader) => autoStats.Add(new((string)reader[0], (string)reader[1])));
			}

			return autoStats;
		}
		List<string> GetLOBColNames(string dbName, string tableName)
		{
			var sql = Invariant($@"
select c.name
from
    sys.schemas             AS sch
    JOIN sys.objects        AS obj ON obj.schema_id = sch.schema_id
    JOIN sys.columns        AS c on c.object_id = obj.object_id
where 1=1
    AND obj.is_ms_shipped = 0
    AND obj.name = '{tableName}'
    AND c.max_length = -1
order by
    obj.name
;
");
			var cols = new List<string>();
			using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
			{
				Db.Connection.ExecuteReader(sql, (reader) => cols.Add((string)reader[0]));
			}

			return cols;
		}

		void AssertStatisticsForTheField(string message, DateTime now, bool expected, string dbName, string tableName, string fieldName)
		{
			var sql = $@"
SELECT
	column_name = COL_NAME(object_id, column_id),
	last_date   = STATS_DATE(object_id, stats_id)
FROM
	sys.stats_columns
WHERE
	object_id = OBJECT_ID(N'{tableName}', N'U');
";

			using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
			using (var cmd = Db.Connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					if ((string)reader["column_name"] == fieldName)
					{
						var date_obj = reader["last_date"];
						if (date_obj == DBNull.Value)
						{
							AssertEquals(message, expected, false);
						}
						else
						{
							AssertEquals(message, expected, now.Subtract((DateTime)date_obj).TotalSeconds <= 1);
						}

						return;
					}
				}
			}

			Fail();
		}

		void AssertStatisticsNoRecompute(string message, bool expected, string dbName, string tableName, string fieldName)
		{
			var sql = $@"
SELECT
	s.no_recompute
FROM
	sys.stats              AS s
	JOIN sys.stats_columns AS sc ON sc.object_id = s.object_id AND sc.stats_id = s.stats_id
	JOIN sys.columns       AS c  ON c.object_id = sc.object_id AND c.column_id = sc.column_id
WHERE 1=1
	AND s.object_id = OBJECT_ID(N'{tableName}', N'U')
	AND c.name = @col_name

";

			using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@col_name", SqlDbType.NVarChar, 128, fieldName);

				var value = cmd.ExecuteScalar();
				AssertEquals("Statistics exists?", true, value != null);
				AssertEquals(message, expected, Convert.ToBoolean(value));
			}
		}

		void AssertStatisticsName(string message, string expected, string dbName, string tableName, string fieldName)
		{
			var sql = $@"
SELECT
	s.name
FROM
	sys.stats              AS s
	JOIN sys.stats_columns AS sc ON sc.object_id = s.object_id AND sc.stats_id = s.stats_id
	JOIN sys.columns       AS c  ON c.object_id = sc.object_id AND c.column_id = sc.column_id
WHERE 1=1
	AND s.object_id = OBJECT_ID(N'{tableName}', N'U')
	AND c.name = @col_name
ORDER BY
	s.name

";

			var actual = new List<string>();

			using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@col_name", SqlDbType.NVarChar, 128, fieldName);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						actual.Add((string)reader["name"]);
					}
				}
			}

			AssertEquals(message, expected, string.Join("; ", actual));
		}

		void AssertStatisticsSample(string message, IndexStatisticsUpdaterForTest updater, string statsName, int expectedRows, int expectedUnfilteredRows, bool expectedFullScan, string dbName, string tableName)
		{
			var sql = $@"
SELECT
	rows            = ISNULL(p.rows, 0),
	rows_sampled    = ISNULL(p.rows_sampled, 0),
	unfiltered_rows = ISNULL(p.unfiltered_rows, 0)
FROM
	sys.stats AS s
	CROSS APPLY sys.dm_db_stats_properties(s.object_id, s.stats_id) AS p
WHERE 1=1
	AND s.object_id = OBJECT_ID(N'{tableName}', N'U')
	AND s.name = @stats_name

";

			using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@stats_name", SqlDbType.NVarChar, 128, statsName);

				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
					{
						CombineAssertions(() =>
						{
							var rows = Convert.ToInt32(reader["rows"]);
							var rows_sampled = Convert.ToInt32(reader["rows_sampled"]);
							var unfiltered_rows = Convert.ToInt32(reader["unfiltered_rows"]);

							if (expectedFullScan)
							{
								AssertEquals(message + "\r\nRows mismatch", expectedRows, rows);
							}
							else
							{
								AssertEquals(message + $"\r\nRows mismatch. Expected: {expectedRows}, Actual: {rows}", true, expectedRows >= rows);
							}

							AssertEquals(message + "\r\nUnfilteredRows mismatch", expectedUnfilteredRows, unfiltered_rows);
							AssertEquals(message + $"\r\nFullScan mismatch. Rows: {rows}, Rows_Sampled: {rows_sampled}", expectedFullScan,
								updater.TryGetUpdateStatisticsCommandForObject(statsName, out var cmdText) ? cmdText.Contains("WITH FULLSCAN") : rows == rows_sampled);
						});
					}
					else
					{
						Fail("Statistics does not exist");
					}
				}
			}
		}

		int GetValue(string sql, string dbName)
		{
			int value = 0;
			using (DbCommand cmd = Db.Connection.Command(string.Format(sql, dbName, "Table1")))
			using (var rd = cmd.ExecuteReader())
			{
				while (rd.Read())
				{
					object obj = rd.GetValue(0);
					value = (obj == null || obj == DBNull.Value ? 0 : Convert.ToInt32(obj));
				}
			}
			return value;
		}

		protected override ICheckerWithRegistryWorkerForTest GetTestObject()
		{
			TestServiceLogger logger = new TestServiceLogger();
			return new IndexStatisticsUpdaterForTest(logger);
		}

		protected override void TearDown()
		{
			DbCommitTracker.Ignore("TestDb");
			DbCommitTracker.Ignore("DbForConsistencyTest");
			DbCommitTracker.Ignore("DbForStatisticsGetDataTest_720DC3C76570417EA3093AC19D6E81CF");
			DbCommitTracker.Ignore("DbForStatisticsUpdateTest_720DC3C76570417EA3093AC19D6E81CF");
			DbCommitTracker.Ignore("DbForStatisticsUpdateTest_CC8D22AB140F47FD91ABA77B8CD94303");
			DbCommitTracker.Ignore("DbForStatisticsUpdateTest_B3F1921D463C425D8356224A2D84768C");
			DbCommitTracker.Ignore("DbForStatisticsUpdateTest_278B9EA87BD1414C8B4DF93DD7313D7D");
			DbCommitTracker.Ignore("DbForStatisticsUpdateTest_87A96AD90E414D0BAEBD0500A6F10F66");
			DbCommitTracker.Ignore("DbForStatisticsUpdateTest_E82AF4A831B243658187E087BF343EDE");
			DbCommitTracker.Ignore("DbForStatisticsUpdateTest_7F1983682EDB4DC9A5ACBBF74139AB58");

			base.TearDown();
		}

		const string Table1 = "_Table1";

		#endregion // Implementation
	}
}
