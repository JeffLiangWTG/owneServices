using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbHealth.Shared.Test;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DbHealth.IndexUpdate.Testing
{
	[UseSnapshotProtection]
	sealed class IndexRebuilderTest : TestCase
	{
		public void TestIndexRebuilder()
		{
			var dbName = Db.Connection.CurrentDatabase + "_SD_TestDb-CB27!@#$%!@#$$$$8BE9014C04175A2C";

			#region Prepare Data

			Env.Registry.ISU_Rebuild_ThresholdPercentage = 40;

			DbWorker.DropTestDbIfExists(dbName);
			DbWorker.CreateTestDb(dbName);

			var sql = $@"
CREATE TABLE {dbName.QuoteName()}.dbo.Table3
(
	id int              NOT NULL PRIMARY KEY, 
	x  uniqueidentifier     NULL,
)
;

CREATE NONCLUSTERED INDEX Table3_X ON {dbName.QuoteName()}.dbo.Table3(x);
";
			Db.Connection.ExecuteNonQuery(sql);

			#endregion // Prepare Data

			using (Db.Connection.TemporarySetLockTimeout(1000))
			{
				try
				{
					// Nothing
					var logger = new TestServiceLogger();
					using (var connection = Db.NewAdminConnection())
					{
						new IndexRebuilder(logger).Run(connection, new string[] { dbName });
					}

					AssertEquals($"Information|Checking for fragmented indexes: DB {dbName.QuoteName()}", logger[0]);

					// Rebuild
					PopulateTable3(dbName, 1000, 0);
					logger = new TestServiceLogger();
					using (var connection = Db.NewAdminConnection())
					{
						new IndexRebuilder(logger).Run(connection, new string[] { dbName });
					}

					AssertEquals($"Information|Checking for fragmented indexes: DB {dbName.QuoteName()}", logger[0]);
					AssertLogContains(logger, $"Information|Rebuilding index: {dbName.QuoteName()}.[Table3].[Table3_X];");

					// Nothing
					logger = new TestServiceLogger();
					using (var connection = Db.NewAdminConnection())
					{
						new IndexRebuilder(logger).Run(connection, new string[] { dbName });
					}

					AssertEquals($"Information|Checking for fragmented indexes: DB {dbName.QuoteName()}", logger[0]);
				}
				finally
				{
					DbWorker.DropTestDbIfExists(dbName);
				}
			}
		}

		public void TestCallsBacklogWaiterWithExpectedMaxTimeFromRegistry()
		{
			// Arrange
			var dbName = $"{Db.Connection.CurrentDatabase}_SD_IndexRebuilder-{nameof(TestCallsBacklogWaiterWithExpectedMaxTimeFromRegistry)}";

			using (AdoTestUtils.CreateDbDropExistingDisposable(dbName))
			{
				const string table = "_Table_1";
				const string index = "_Index_1";

				var sql = $@"
if (OBJECT_ID(N'dbo.[{table}]', N'U') is NOT NULL) DROP TABLE dbo.[{table}];
CREATE TABLE dbo.[{table}]
(
	TT_PK     uniqueidentifier,
);

CREATE NONCLUSTERED INDEX [{index}] ON dbo.[{table}] (TT_PK);

;WITH
	L0 AS (SELECT n = NULL FROM (VALUES (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL)) AS t(n)),
	L1 AS (SELECT n = NULL FROM L0 AS A CROSS JOIN L0 AS B),
	L2 AS (SELECT n = NULL FROM L1 AS A CROSS JOIN L1 AS B),
	L3 AS (SELECT n = NULL FROM L2 AS A CROSS JOIN L2 AS B)
INSERT
	dbo.[{table}] WITH (TABLOCKX) (TT_PK)
SELECT TOP (400)
	TT_PK     = NEWID()
FROM
	L3;
";
				using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
				{
					Db.Connection.ExecuteNonQuery(sql);
				}

				var logger = new TestServiceLogger();
				var mockBacklogWaiter = new Mock<ILowPriorityProcessPauser>();
				var updater = new IndexRebuilder(logger, _ => mockBacklogWaiter.Object);
				var maxWaitMinutes = Env.Registry.ISU_MaxBacklogWaitTime_InMinutes;

				using (Db.Connection.TemporarySetLockTimeout(1000))
				using (var connection = Db.NewAdminConnection())
				{
					// Act
					updater.Run(connection, new[] { dbName });
				}

				// Assert
				AssertNoExceptionThrown(() =>
				{
					mockBacklogWaiter.Verify(
						m => m.Wait(It.IsAny<ILogger>(), TimeSpan.FromMinutes(maxWaitMinutes), null),
						Times.Once);
				});
			}
		}

		public void TestRethrowsBacklogWaiterTimeoutException()
		{
			// Arrange
			var dbName = $"{Db.Connection.CurrentDatabase}_SD_IndexRebuilder-{nameof(TestRethrowsBacklogWaiterTimeoutException)}";

			using (AdoTestUtils.CreateDbDropExistingDisposable(dbName))
			{
				const string table = "_Table_1";
				const string index = "_Index_1";

				var sql = $@"
if (OBJECT_ID(N'dbo.[{table}]', N'U') is NOT NULL) DROP TABLE dbo.[{table}];
CREATE TABLE dbo.[{table}]
(
	TT_PK     uniqueidentifier,
);

CREATE NONCLUSTERED INDEX [{index}] ON dbo.[{table}] (TT_PK);

;WITH
	L0 AS (SELECT n = NULL FROM (VALUES (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL)) AS t(n)),
	L1 AS (SELECT n = NULL FROM L0 AS A CROSS JOIN L0 AS B),
	L2 AS (SELECT n = NULL FROM L1 AS A CROSS JOIN L1 AS B),
	L3 AS (SELECT n = NULL FROM L2 AS A CROSS JOIN L2 AS B)
INSERT
	dbo.[{table}] WITH (TABLOCKX) (TT_PK)
SELECT TOP (400)
	TT_PK     = NEWID()
FROM
	L3;

";
				using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
				{
					Db.Connection.ExecuteNonQuery(sql);
				}

				var logger = new TestServiceLogger();
				var mockBacklogWaiter = new Mock<ILowPriorityProcessPauser>();
				mockBacklogWaiter.Setup(m => m.Wait(It.IsAny<ILogger>(), It.IsAny<TimeSpan>(), null))
					.Throws(new BacklogWaiterTimeoutException(Guid.NewGuid().ToString()));
				var updater = new IndexRebuilder(logger, _ => mockBacklogWaiter.Object);

				using (Db.Connection.TemporarySetLockTimeout(1000))
				using (var connection = Db.NewAdminConnection())
				{
					//Act
					// Assert
					AssertExceptionThrown<BacklogWaiterTimeoutException>(() => updater.Run(connection, new[] { dbName }));
					mockBacklogWaiter.Verify(m => m.Wait(It.IsAny<ILogger>(), It.IsAny<TimeSpan>(), null), Times.Once);
				}
			}
		}

		public void TestHandlesInvalidDateTimeExtendedPropertyValue()
		{
			var dayInTheFuture = DateTime.UtcNow.Date.AddYears(1);
			var testDbName = Db.Connection.CurrentDatabase + "_SD_HandlesInvalidDateTimeExtendedPropertyValue";

			try
			{
				DbWorker.CreateTestDb(testDbName);

				var sql = $@"
CREATE TABLE {testDbName.QuoteName()}.dbo.TestTable1(id int NOT NULL PRIMARY KEY, x uniqueidentifier NULL);
CREATE NONCLUSTERED INDEX TestTable1_X ON {testDbName.QuoteName()}.dbo.TestTable1(x);
";
				Db.Connection.ExecuteNonQuery(sql);

				// No extended properties defined (null) - use default values.
				var logger = new TestServiceLogger();
				var updater = new IndexRebuilder(logger);
				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					updater.Run(connection, new string[] { testDbName });
				}

				AssertEquals($"logger.Count\r\nFull log:\r\n{logger.ToString()}", 7, logger.Count);
				AssertEquals($"Information|Checking for fragmented indexes: DB {testDbName.QuoteName()}", logger[0]);

				var schema = Db.SqlDbOwnerSchema;
				var table_1 = "TestTable1";
				var index_1 = "TestTable1_X";

				// Dates in the future - date datatype.
				ExtProperty.Index.Update(Db.Connection, schema, table_1, index_1, propertyName: IndexUpdater.LastRebuildPtyName, value: SqlFormatInfo.ToSqlDateTimeString(dayInTheFuture));
				ExtProperty.Index.Update(Db.Connection, schema, table_1, index_1, propertyName: IndexUpdater.LastReorganisePtyName, value: SqlFormatInfo.ToSqlDateTimeString(dayInTheFuture));
				ExtProperty.Index.Update(Db.Connection, schema, table_1, index_1, propertyName: IndexUpdater.PendingFillFactorReductionPtyName, value: "0");

				logger.ClearLog();
				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					updater.Run(connection, new string[] { testDbName });
				}

				AssertEquals($"logger.Count\r\nFull log:\r\n{logger.ToString()}", 7, logger.Count);
				AssertEquals($"Information|Checking for fragmented indexes: DB [{testDbName}]", logger[0]);

				// Dates in the future - char datatype, proper date format. Should be converted to date time.
				ExtProperty.Index.Update(Db.Connection, schema, table_1, index_1, propertyName: IndexUpdater.LastRebuildPtyName, value: SqlFormatInfo.ToSqlDateTimeString(dayInTheFuture));
				ExtProperty.Index.Update(Db.Connection, schema, table_1, index_1, propertyName: IndexUpdater.LastReorganisePtyName, value: SqlFormatInfo.ToSqlDateTimeString(dayInTheFuture));

				logger.ClearLog();
				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					updater.Run(connection, new string[] { testDbName });
				}

				AssertEquals($"logger.Count\r\nFull log:\r\n{logger.ToString()}", 7, logger.Count);
				AssertEquals($"Information|Checking for fragmented indexes: DB [{testDbName}]", logger[0]);

				// All extended properties misformatted => min date used. Index should be rebuilt.
				ExtProperty.Index.Update(Db.Connection, schema, table_1, index_1, propertyName: IndexUpdater.LastRebuildPtyName, value: "120315000000000");
				ExtProperty.Index.Update(Db.Connection, schema, table_1, index_1, propertyName: IndexUpdater.LastReorganisePtyName, value: "ABC");
				ExtProperty.Index.Update(Db.Connection, schema, table_1, index_1, propertyName: IndexUpdater.PendingFillFactorReductionPtyName, value: "#");

				logger.ClearLog();
				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					updater.Run(connection, new string[] { testDbName });
				}

				AssertEquals($"logger.Count\r\nFull log:\r\n{logger.ToString()}", 7, logger.Count);
				AssertEquals($"Information|Checking for fragmented indexes: DB [{testDbName}]", logger[0]);
			}
			finally
			{
				DbWorker.DropTestDbIfExists(testDbName);
			}
		}

		public void TestRebuildCannotRunOnline()
		{
			var dbName = Db.Connection.CurrentDatabase + "_SD_RebuildCannotRunOnline";

			try
			{
				DbWorker.CreateTestDb(dbName);

				var sql = $@"
CREATE TABLE {dbName.QuoteName()}.dbo.Table3
(
	id int              NOT NULL PRIMARY KEY NONCLUSTERED, 
	x  uniqueidentifier     NULL,
	y  varchar(max)         NULL,
)
;

CREATE NONCLUSTERED INDEX Table3_X ON {dbName.QuoteName()}.dbo.Table3(x) INCLUDE (y);
";
				Db.Connection.ExecuteNonQuery(sql);

				for (int i = 0; i < 10000; i++)
				{
					Db.Connection.ExecuteNonQuery($"INSERT {dbName.QuoteName()}.dbo.Table3 (id, x) VALUES ({i}, NEWID())");
				}

				var logger = new TestServiceLogger();
				using (var connection = Db.NewAdminConnection())
				{
					new IndexRebuilder(logger).Run(connection, new string[] { dbName });
				}

				AssertEquals($"logger.Count\r\nFull log:\r\n{logger.ToString()}", true, logger.Count > 1);
				AssertLogContains(logger, $"Information|Rebuilding index: {dbName.QuoteName()}.[Table3].[Table3_X];");
				Assert("Expected no error message to be logged, actual log was:\r\n" + logger.ToString(), !logger.ToString().Contains("Error"));
			}
			finally
			{
				DbWorker.DropTestDbIfExists(dbName);
			}
		}

		public void TestLockTimeout_Rebuild()
		{
			var dbName = Db.Connection.CurrentDatabase + "_SD_LockTimeout_Rebuild";
			var table_1 = "_Table_1";
			var table_2 = "_Table_2";
			var index_1 = "_Index_1";
			var index_2 = "_Index_2";

			#region Prepare data

			DbWorker.DropTestDbIfExists(dbName);
			DbWorker.CreateTestDb(dbName);

			using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
			{
				var sql = $@"
if (OBJECT_ID(N'dbo.[{table_1}]', N'U') is NOT NULL) DROP TABLE dbo.[{table_1}];
CREATE TABLE dbo.[{table_1}]
(
	TT_PK     uniqueidentifier,
	TT_Parent uniqueidentifier,
);

CREATE NONCLUSTERED INDEX [{index_1}] ON dbo.[{table_1}] (TT_PK);
CREATE NONCLUSTERED INDEX [{index_2}] ON dbo.[{table_1}] (TT_Parent) WHERE (TT_Parent is NOT NULL);

;WITH
	L0 AS (SELECT n = NULL FROM (VALUES (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL)) AS t(n)),
	L1 AS (SELECT n = NULL FROM L0 AS A CROSS JOIN L0 AS B),
	L2 AS (SELECT n = NULL FROM L1 AS A CROSS JOIN L1 AS B),
	L3 AS (SELECT n = NULL FROM L2 AS A CROSS JOIN L2 AS B)
INSERT
	dbo.[{table_1}] WITH (TABLOCKX) (TT_PK, TT_Parent)
SELECT TOP (800)
	TT_PK     = NEWID(),
	TT_Parent = CASE WHEN ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) % 3 = 0 THEN NEWID() ELSE NULL END
FROM
	L3;

if (OBJECT_ID(N'dbo.[{table_2}]', N'U') is NOT NULL) DROP TABLE dbo.[{table_2}];
CREATE TABLE dbo.[{table_2}]
(
	TT_PK     uniqueidentifier,
	TT_Parent uniqueidentifier,
);

CREATE NONCLUSTERED INDEX [{index_1}] ON dbo.[{table_2}] (TT_PK);

;WITH
	L0 AS (SELECT n = NULL FROM (VALUES (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL)) AS t(n)),
	L1 AS (SELECT n = NULL FROM L0 AS A CROSS JOIN L0 AS B),
	L2 AS (SELECT n = NULL FROM L1 AS A CROSS JOIN L1 AS B),
	L3 AS (SELECT n = NULL FROM L2 AS A CROSS JOIN L2 AS B)
INSERT
	dbo.[{table_2}] WITH (TABLOCKX) (TT_PK, TT_Parent)
SELECT TOP (400)
	TT_PK     = NEWID(),
	TT_Parent = CASE WHEN ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) % 2 = 0 THEN NEWID() ELSE NULL END
FROM
	L3;

";

				Db.Connection.ExecuteNonQuery(sql);
			}

			#endregion // Prepare data

			Task task = null;

			try
			{
				var logger = new TestServiceLogger();
				var rebuilder = new IndexRebuilder(logger);

				rebuilder.UserActionRebuild_ForTest = () =>
				{
					task = Task.Run(() =>
					{
						using (Db.DisposableActionForDbConnection())
						using (var connection = Db.NewAdminConnection(dbName))
						{
							connection.BeginTransaction();
							connection.ExecuteNonQuery($"ALTER TABLE [dbo].[{table_1}] ADD new_field bit");
							Thread.Sleep(500);
						}
					});

					Thread.Sleep(100);
				};

				using (var connection = Db.NewAdminConnection())
				using (connection.TemporarySetLockTimeout(TimeSpan.FromMilliseconds(1)))
				{
					rebuilder.Run(connection, new string[] { dbName });
				}

				CombineAssertions(() =>
				{
					AssertEquals($"Full log:\r\n{logger.ToString()}", 11, logger.Count);

					AssertStartsWith("Rebuilding index_1_1", $"Information|Rebuilding index: [{dbName}].[{table_1}].[{index_1}];", logger[8]);
					AssertStartsWith("Rebuilding index_2_1", $"Information|Rebuilding index: [{dbName}].[{table_2}].[{index_1}];", logger[9]);
					AssertStartsWith("Rebuilding index_1_2", $"Information|Rebuilding index: [{dbName}].[{table_1}].[{index_2}];", logger[10]);
				});
			}
			finally
			{
				try
				{
					Task.WaitAll(task);
				}
				finally
				{
					DbWorker.DropTestDbIfExists(dbName);
				}
			}
		}

		public void TestAutoStatsNotChangedDuringRebuild()
		{
			var dbName = Db.Connection.CurrentDatabase + "_SD_AutoStatsDuringRebuild";
			var table_1 = "_Table_1";
			var index_1 = "_Index_1";

			#region Prepare data

			DbWorker.DropTestDbIfExists(dbName);
			DbWorker.CreateTestDb(dbName);

			using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
			{
				var sql = $@"
if (OBJECT_ID(N'dbo.[{table_1}]', N'U') is NOT NULL) DROP TABLE dbo.[{table_1}];
CREATE TABLE dbo.[{table_1}]
(
	TT_PK     uniqueidentifier,
);

CREATE NONCLUSTERED INDEX [{index_1}] ON dbo.[{table_1}] (TT_PK);

;WITH
	L0 AS (SELECT n = NULL FROM (VALUES (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL)) AS t(n)),
	L1 AS (SELECT n = NULL FROM L0 AS A CROSS JOIN L0 AS B),
	L2 AS (SELECT n = NULL FROM L1 AS A CROSS JOIN L1 AS B),
	L3 AS (SELECT n = NULL FROM L2 AS A CROSS JOIN L2 AS B)
INSERT
	dbo.[{table_1}] WITH (TABLOCKX) (TT_PK)
SELECT TOP (400)
	TT_PK     = NEWID()
FROM
	L3;

";

				Db.Connection.ExecuteNonQuery(sql);
			}

			#endregion // Prepare data

			StatisticsSwitch.Instance.SetDbStatisticsSettings(dbName, autoCreateON: true, autoUpdateON: true, autoUpdateAsyncON: true);

			AssertEquals("PRECONDITION", "ON-ON-ON", StatisticsSwitch.Instance.GetStatisticsSettings(Db.Connection, dbName).Current.ToString());

			var statsStates = new List<string>();
			var logger = new TestServiceLogger();

			try
			{
				var rebuilder = new IndexRebuilder(logger);

				rebuilder.UserActionRebuild2_ForTest = (currentDbName) =>
				{
					statsStates.Add(currentDbName + ": " + StatisticsSwitch.Instance.GetStatisticsSettings(Db.Connection, currentDbName).Current.ToString());
				};

				using (var connection = Db.NewAdminConnection())
				{
					rebuilder.Run(connection, new string[] { dbName });
				}

				CombineAssertions(() =>
				{
					AssertEquals($"Full log:\r\n{logger.ToString()}", 7, logger.Count);
					AssertStartsWith($"Rebuilding index_1_1:\r\n{logger[1]}", $"Information|Rebuilding index: [{dbName}].[{table_1}].[{index_1}];", logger[6]);

					AssertEquals("Statistics states during rebuild", dbName + ": ON-ON-ON", string.Join(";", statsStates));
					AssertEquals("Statistics states after rebuild", "ON-ON-ON", StatisticsSwitch.Instance.GetStatisticsSettings(Db.Connection, dbName).Current.ToString());
				});
			}
			finally
			{
				DbWorker.DropTestDbIfExists(dbName);
			}
		}

		public void TestSimultaneousRebuildsOnSharedDatabase()
		{
			const string dbName = RefDbTableNameResolver.SharedDbPrefix + RefDbTableNameResolver.RefDbAffix + "_ISU_Rebuild";
			Task task = null;
			var logger_1 = new TestServiceLogger();
			var logger_2 = new TestServiceLogger();
			try
			{
				var rebuilder2HasPassedCriticalSectionEvent = new AutoResetEvent(false);
				var rebuilder1IsInCriticalSectionEvent = new AutoResetEvent(false);
				DbWorker.CreateTestDb(dbName, createTables: false);
				using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
				{
					Db.Connection.ExecuteNonQuery(ExtProperty.TableDefinition);
				}

				var rebuilder_1 = new IndexRebuilder(logger_1)
				{
					UserActionUpdateIndexes_ForTest = () =>
					{
						rebuilder1IsInCriticalSectionEvent.Set();
						rebuilder2HasPassedCriticalSectionEvent.WaitOne(TimeSpan.FromSeconds(5));
					}
				};

				var rebuilder_2 = new IndexRebuilder(logger_2);

				task = Task.Run(() =>
				{
					using (Db.DisposableActionForDbConnection())
					using (var connection_1 = Db.NewAdminConnection())
					{
						rebuilder_1.Run(connection_1, new[] { dbName });
					}
				});

				rebuilder1IsInCriticalSectionEvent.WaitOne(TimeSpan.FromSeconds(5));
				using (var connection_2 = Db.NewAdminConnection())
				{
					rebuilder_2.Run(connection_2, new[] { dbName });
					rebuilder2HasPassedCriticalSectionEvent.Set();
				}
			}
			finally
			{
				try
				{
					Task.WaitAll(task);
				}
				finally
				{
					DbWorker.DropTestDbIfExists(dbName);
				}
			}

			CombineAssertions(() =>
			{
				AssertEquals($"Full log_1:\r\n{logger_1}", 6, logger_1.Count);
				AssertEquals($"Full log_2:\r\n{logger_2}", 1, logger_2.Count);
				AssertStartsWith("Checking DB", $"Information|Checking for fragmented indexes: DB [{dbName}]", logger_2[0]);
			});
		}

		public void TestObserver_KillBlockers()
		{
			var dbName = Db.Connection.CurrentDatabase + "_SD_ObserverKillBlockers";
			var table_1 = "_Table_1";
			var table_2 = "_Table_2";
			var index_1 = "_Index_1";

			Env.Registry.ISU_Rebuild_UseObserver = true;
			Env.Registry.ISU_Rebuild_Online = false;
			Env.Registry.ISU_Rebuild_MaxWaitInMinutes = 0;
			Env.Registry.ISU_Rebuild_AbortAfterWait = OnlineIndexRebuildLowPriorityAbortAfterWaitList.Codes.Blockers;

			#region Prepare data

			DbWorker.DropTestDbIfExists(dbName);
			DbWorker.CreateTestDb(dbName);

			using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
			{
				var sql = $@"
if (OBJECT_ID(N'dbo.[{table_1}]', N'U') is NOT NULL) DROP TABLE dbo.[{table_1}];
CREATE TABLE dbo.[{table_1}]
(
	TT_PK     uniqueidentifier,
	TT_Parent uniqueidentifier,
);

CREATE NONCLUSTERED INDEX [{index_1}] ON dbo.[{table_1}] (TT_PK);

;WITH
	L0 AS (SELECT n = NULL FROM (VALUES (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL)) AS t(n)),
	L1 AS (SELECT n = NULL FROM L0 AS A CROSS JOIN L0 AS B),
	L2 AS (SELECT n = NULL FROM L1 AS A CROSS JOIN L1 AS B),
	L3 AS (SELECT n = NULL FROM L2 AS A CROSS JOIN L2 AS B)
INSERT
	dbo.[{table_1}] WITH (TABLOCKX) (TT_PK, TT_Parent)
SELECT TOP (800)
	TT_PK     = NEWID(),
	TT_Parent = CASE WHEN ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) % 3 = 0 THEN NEWID() ELSE NULL END
FROM
	L3;

if (OBJECT_ID(N'dbo.[{table_2}]', N'U') is NOT NULL) DROP TABLE dbo.[{table_2}];
CREATE TABLE dbo.[{table_2}]
(
	TT_PK     uniqueidentifier,
	TT_Parent uniqueidentifier,
);

CREATE NONCLUSTERED INDEX [{index_1}] ON dbo.[{table_2}] (TT_PK);

;WITH
	L0 AS (SELECT n = NULL FROM (VALUES (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL)) AS t(n)),
	L1 AS (SELECT n = NULL FROM L0 AS A CROSS JOIN L0 AS B),
	L2 AS (SELECT n = NULL FROM L1 AS A CROSS JOIN L1 AS B),
	L3 AS (SELECT n = NULL FROM L2 AS A CROSS JOIN L2 AS B)
INSERT
	dbo.[{table_2}] WITH (TABLOCKX) (TT_PK, TT_Parent)
SELECT TOP (400)
	TT_PK     = NEWID(),
	TT_Parent = CASE WHEN ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) % 2 = 0 THEN NEWID() ELSE NULL END
FROM
	L3;

";

				Db.Connection.ExecuteNonQuery(sql);
			}

			#endregion // Prepare data

			var tasks = new List<Task>();

			try
			{
				var logger = new TestServiceLogger();
				var rebuilder = new IndexRebuilder(logger);

				var isRunning = false;
				rebuilder.UserActionRebuild3_ForTest = () =>
				{
					if (!isRunning)
					{
						tasks.Add(Task.Run(() =>
						{
							using (var connection = Db.NewAdminConnection(dbName))
							using (connection.TemporarySetLockTimeout(DbConnection.LockTimeout.NoWait))
							using (connection.BeginTransactionWithManager())
							{
								try
								{
									isRunning = true;
									connection.ExecuteNonQuery($"ALTER TABLE [dbo].[{table_1}] ADD new_field bit; WAITFOR DELAY '00:00:10';");
								}
								catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.SevereError)
								{
									// connection has been killed
								}
							}
						}));

						while (!isRunning)
						{
							Thread.Sleep(1);
						}
					}
				};

				using (var connection = Db.NewAdminConnection())
				{
					rebuilder.Run(connection, new string[] { dbName });
				}

				var fullLog = $"\r\nFull log:\r\n{logger.ToString()}";
				CombineAssertions(() =>
				{
					AssertEquals($"logger.Count{fullLog}", 10, logger.Count);

					AssertStartsWith($"Rebuilding index_1_1{fullLog}", $"Information|Rebuilding index: [{dbName}].[{table_1}].[{index_1}];", logger[7]);
					AssertStartsWith($"Killing blockers index_1_1{fullLog}", $"Information|Killing blockers for index: [{dbName}].[{table_1}].[{index_1}];", logger[8]);
					AssertStartsWith($"Rebuilding index_2_1{fullLog}", $"Information|Rebuilding index: [{dbName}].[{table_2}].[{index_1}];", logger[9]);
				});
			}
			finally
			{
				try
				{
					Task.WaitAll(tasks.ToArray());
				}
				finally
				{
					DbWorker.DropTestDbIfExists(dbName);
				}
			}
		}

		public void TestObserver_Requeue()
		{
			var dbName = Db.Connection.CurrentDatabase + "_SD_ObserverRequeue";
			var table_1 = "_Table_1";
			var table_2 = "_Table_2";
			var index_1 = "_Index_1";

			Env.Registry.ISU_Rebuild_UseObserver = true;
			Env.Registry.ISU_Rebuild_Online = true;
			Env.Registry.ISU_Rebuild_MaxWaitInMinutes = 0;
			Env.Registry.ISU_Rebuild_AbortAfterWait = OnlineIndexRebuildLowPriorityAbortAfterWaitList.Codes.None;

			#region Prepare data

			DbWorker.DropTestDbIfExists(dbName);
			DbWorker.CreateTestDb(dbName);

			using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
			{
				var sql = $@"
if (OBJECT_ID(N'dbo.[{table_1}]', N'U') is NOT NULL) DROP TABLE dbo.[{table_1}];
CREATE TABLE dbo.[{table_1}]
(
	TT_PK     uniqueidentifier,
	TT_Parent uniqueidentifier,
);

CREATE NONCLUSTERED INDEX [{index_1}] ON dbo.[{table_1}] (TT_PK);

;WITH
	L0 AS (SELECT n = NULL FROM (VALUES (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL)) AS t(n)),
	L1 AS (SELECT n = NULL FROM L0 AS A CROSS JOIN L0 AS B),
	L2 AS (SELECT n = NULL FROM L1 AS A CROSS JOIN L1 AS B),
	L3 AS (SELECT n = NULL FROM L2 AS A CROSS JOIN L2 AS B)
INSERT
	dbo.[{table_1}] WITH (TABLOCKX) (TT_PK, TT_Parent)
SELECT TOP (800)
	TT_PK     = NEWID(),
	TT_Parent = CASE WHEN ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) % 3 = 0 THEN NEWID() ELSE NULL END
FROM
	L3;

if (OBJECT_ID(N'dbo.[{table_2}]', N'U') is NOT NULL) DROP TABLE dbo.[{table_2}];
CREATE TABLE dbo.[{table_2}]
(
	TT_PK     uniqueidentifier,
	TT_Parent uniqueidentifier,
);

CREATE NONCLUSTERED INDEX [{index_1}] ON dbo.[{table_2}] (TT_PK);

;WITH
	L0 AS (SELECT n = NULL FROM (VALUES (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL)) AS t(n)),
	L1 AS (SELECT n = NULL FROM L0 AS A CROSS JOIN L0 AS B),
	L2 AS (SELECT n = NULL FROM L1 AS A CROSS JOIN L1 AS B),
	L3 AS (SELECT n = NULL FROM L2 AS A CROSS JOIN L2 AS B)
INSERT
	dbo.[{table_2}] WITH (TABLOCKX) (TT_PK, TT_Parent)
SELECT TOP (400)
	TT_PK     = NEWID(),
	TT_Parent = CASE WHEN ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) % 2 = 0 THEN NEWID() ELSE NULL END
FROM
	L3;

";

				Db.Connection.ExecuteNonQuery(sql);
			}

			#endregion // Prepare data

			var tasks = new List<Task>();

			try
			{
				var logger = new TestServiceLogger();
				var rebuilder = new IndexRebuilder(logger);

				var isRunning = false;
				rebuilder.UserActionRebuild3_ForTest = () =>
				{
					if (!isRunning)
					{
						tasks.Add(Task.Run(() =>
						{
							using (var connection = Db.NewAdminConnection(dbName))
							using (connection.TemporarySetLockTimeout(DbConnection.LockTimeout.NoWait))
							using (connection.BeginTransactionWithManager())
							{
								try
								{
									isRunning = true;
									connection.ExecuteNonQuery($"ALTER TABLE [dbo].[{table_1}] ADD new_field bit; WAITFOR DELAY '00:00:10';");
								}
								catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.SevereError)
								{
									// connection has been killed
								}
							}
						}));

						while (!isRunning)
						{
							Thread.Sleep(1);
						}
					}
				};

				using (var connection = Db.NewAdminConnection())
				{
					rebuilder.Run(connection, new string[] { dbName });
				}

				var fullLog = $"\r\nFull log:\r\n{logger.ToString()}";
				CombineAssertions(() =>
				{
					AssertEquals($"logger.Count{fullLog}", 12, logger.Count);

					AssertStartsWith($"Rebuilding index_1_1{fullLog}", $"Information|Rebuilding index: [{dbName}].[{table_1}].[{index_1}];", logger[7]);
					AssertStartsWith($"Re-queued index_1_1{fullLog}", $"Information|Re-queued due to blocking process: [{dbName}].[{table_1}].[{index_1}];", logger[8]);
					AssertStartsWith($"Rebuilding index_2_1{fullLog}", $"Information|Rebuilding index: [{dbName}].[{table_2}].[{index_1}];", logger[9]);
					AssertStartsWith($"Rebuilding index_1_1{fullLog}", $"Information|Rebuilding index: [{dbName}].[{table_1}].[{index_1}];", logger[10]);
					AssertStartsWith($"Skipped index_1_1{fullLog}", $"Information|Skipped due to blocking process: [{dbName}].[{table_1}].[{index_1}];", logger[11]);
				});
			}
			finally
			{
				try
				{
					Task.WaitAll(tasks.ToArray());
				}
				finally
				{
					DbWorker.DropTestDbIfExists(dbName);
				}
			}
		}

		[SnailTest]
		public void TestLongRunningSqlIsNotKilledByIsuOnSharedDb_NoObserver()
		{
			Env.Registry.ISU_Rebuild_UseObserver = false;
			Env.Registry.ISU_Rebuild_MaxWaitInMinutes = 1;
			var dbName = "CW-RefDb-ISU_Rebuild";
			TestLongRunningSqlKilledByISUCore(dbName, "'00:02:40'", (wasKilled, fullLog, table, index, logger, blockerLogger) =>
			{
				AssertEquals($"logger.Count{fullLog}", 8, logger.Count);
				AssertEquals($"blockerLogger.Count\r\n{blockerLogger}", 0, blockerLogger.Count);

				AssertStartsWith(fullLog, $"Information|Checking for fragmented indexes: DB [{dbName}]", logger[0]);
				AssertStartsWith(fullLog, "Information|Starting ExecuteReader", logger[1]);
				AssertStartsWith(fullLog, "Information|Reader executed to retrieve index details", logger[2]);
				AssertStartsWith(fullLog, $"Information|Retrieved data for dbo.{table} ({index})", logger[3]);
				AssertStartsWith(fullLog, $"Information|Rebuilding index: [{dbName}].[{table}].[{index}];", logger[4]);
				AssertStartsWith(fullLog, $"Information|Re-queued due to blocking process: [{dbName}].[{table}].[{index}];", logger[5]);
				AssertStartsWith(fullLog, $"Information|Rebuilding index: [{dbName}].[{table}].[{index}];", logger[6]);
				AssertStartsWith(fullLog, $"Information|Canceled due to blocking process: [{dbName}].[{table}].[{index}]", logger[7]);
				AssertEquals("Blocking process was killed.", false, wasKilled);
			});
		}

		[SnailTest]
		public void TestLongRunningSqlIsKilledByIsuOnNonSharedDb_NoObserver()
		{
			Env.Registry.ISU_Rebuild_UseObserver = false;
			Env.Registry.ISU_Rebuild_MaxWaitInMinutes = 0;
			var dbName = Db.Connection.CurrentDatabase + "_SD_ISU_Rebuild";
			TestLongRunningSqlKilledByISUCore(dbName, "'00:02:40'", (wasKilled, fullLog, table, index, logger, blockerLogger) =>
			{
				AssertEquals($"logger.Count{fullLog}", 5, logger.Count);

				AssertStartsWith(fullLog, $"Information|Checking for fragmented indexes: DB [{dbName}]", logger[0]);
				AssertStartsWith(fullLog, "Information|Starting ExecuteReader", logger[1]);
				AssertStartsWith(fullLog, "Information|Reader executed to retrieve index details", logger[2]);
				AssertStartsWith(fullLog, $"Information|Retrieved data for dbo.{table} ({index})", logger[3]);
				AssertStartsWith(fullLog, $"Information|Rebuilding index: [{dbName}].[{table}].[{index}];", logger[4]);

				AssertEquals($"Blocking process was not killed.\r\n{blockerLogger}", true, wasKilled);
				AssertStartsWith($"Session killed\r\n{blockerLogger}", "Error|Your session has been disconnected because of a high priority DDL operation.", blockerLogger[0]);
			});
		}

		[SnailTest]
		public void TestLongRunningSqlIsNotKilledByIsuOnSharedDb_WithObserver()
		{
			Env.Registry.ISU_Rebuild_UseObserver = true;
			Env.Registry.ISU_Rebuild_MaxWaitInMinutes = 0;
			var dbName = "CW-RefDb-ISU_Rebuild";
			TestLongRunningSqlKilledByISUCore(dbName, "'00:00:30'", (wasKilled, fullLog, table, index, logger, blockerLogger) =>
			{
				AssertEquals($"logger.Count{fullLog}", 8, logger.Count);
				AssertEquals($"blockerLogger.Count\r\n{blockerLogger}", 0, blockerLogger.Count);

				AssertStartsWith(fullLog, $"Information|Checking for fragmented indexes: DB [{dbName}]", logger[0]);
				AssertStartsWith(fullLog, "Information|Starting ExecuteReader", logger[1]);
				AssertStartsWith(fullLog, "Information|Reader executed to retrieve index details", logger[2]);
				AssertStartsWith(fullLog, $"Information|Retrieved data for dbo.{table} ({index})", logger[3]);
				AssertStartsWith(fullLog, $"Information|Rebuilding index: [{dbName}].[{table}].[{index}];", logger[4]);
				AssertStartsWith(fullLog, $"Information|Re-queued due to blocking process: [{dbName}].[{table}].[{index}];", logger[5]);
				AssertStartsWith(fullLog, $"Information|Rebuilding index: [{dbName}].[{table}].[{index}];", logger[6]);
				AssertStartsWith(fullLog, $"Information|Skipped due to blocking process: [{dbName}].[{table}].[{index}]", logger[7]);
				AssertEquals("Blocking process was killed.", false, wasKilled);
			});
		}

		[SnailTest]
		public void TestLongRunningSqlIsKilledByIsuOnNonSharedDb_WithObserver()
		{
			Env.Registry.ISU_Rebuild_UseObserver = true;
			Env.Registry.ISU_Rebuild_MaxWaitInMinutes = 0;
			var dbName = Db.Connection.CurrentDatabase + "_SD_ISU_Rebuild";
			TestLongRunningSqlKilledByISUCore(dbName, "'00:02:40'", (wasKilled, fullLog, table, index, logger, blockerLogger) =>
			{
				AssertEquals($"logger.Count{fullLog}", 6, logger.Count);
				AssertEquals($"blockerLogger.Count\r\n{blockerLogger}", 1, blockerLogger.Count);

				AssertStartsWith(fullLog, $"Information|Checking for fragmented indexes: DB [{dbName}]", logger[0]);
				AssertStartsWith(fullLog, "Information|Starting ExecuteReader", logger[1]);
				AssertStartsWith(fullLog, "Information|Reader executed to retrieve index details", logger[2]);
				AssertStartsWith(fullLog, $"Information|Retrieved data for dbo.{table} ({index})", logger[3]);
				AssertStartsWith(fullLog, $"Information|Rebuilding index: [{dbName}].[{table}].[{index}];", logger[4]);
				AssertStartsWith(fullLog, $"Information|Killing blockers for index: [{dbName}].[{table}].[{index}];", logger[5]);

				AssertStartsWith($"Session killed\r\n{blockerLogger}", "Error|Cannot continue the execution because the session is in the kill state.", blockerLogger[0]);

				AssertEquals("Blocking process was not killed.", true, wasKilled);
			});
		}

		public void TestSpatialIndexNotRebuiltOnline()
		{
			var dbName = Db.Connection.CurrentDatabase + "_SD_SpatialIndex_TestDb";
			var table = "_Table";
			var indexPK = "_indexPK";
			var indexSpatial = "_indexSpatial";

			#region Prepare data

			Env.Registry.ISU_Rebuild_Online = true;

			DbWorker.DropTestDbIfExists(dbName);
			DbWorker.CreateTestDb(dbName, createTables: false);

			using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
			{
				var sql = $@"
if (OBJECT_ID(N'dbo.[{table}]', N'U') is NOT NULL) DROP TABLE dbo.[{table}];
CREATE TABLE dbo.[{table}]
(
	TT_PK        uniqueidentifier NOT NULL CONSTRAINT [{indexPK}] PRIMARY KEY CLUSTERED,
	TT_Geography geography        NOT NULL DEFAULT CONVERT(geography, 'POINT EMPTY'),
);

CREATE SPATIAL INDEX [{indexSpatial}] ON dbo.[{table}] (TT_Geography) USING GEOGRAPHY_AUTO_GRID;

;WITH
	L0 AS (SELECT n = NULL FROM (VALUES (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL)) AS t(n)),
	L1 AS (SELECT n = NULL FROM L0 AS A CROSS JOIN L0 AS B),
	L2 AS (SELECT n = NULL FROM L1 AS A CROSS JOIN L1 AS B),
	L3 AS (SELECT n = NULL FROM L2 AS A CROSS JOIN L2 AS B)
INSERT
	dbo.[{table}] WITH (TABLOCKX) (TT_PK)
SELECT TOP (200)
	TT_PK = NEWID()
FROM
	L3;

";

				Db.Connection.ExecuteNonQuery(sql);
			}

			#endregion // Prepare data

			try
			{
				var logger = new TestServiceLogger();
				var updater = new IndexRebuilder(logger);

				using (var connection = Db.NewAdminConnection())
				{
					updater.Run(connection, new string[] { dbName });
				}

				AssertNotContains("Full log should not contain spatial index", indexSpatial, logger.ToString());
			}
			finally
			{
				DbWorker.DropTestDbIfExists(dbName);
			}
		}

		public void TestSkipQuerying()
		{
			var dbName = Db.Connection.CurrentDatabase + "_SD_TestDb-CB27!@#$%!@#$$$$8BE9014C04175A2C";
			var logger = new TestServiceLogger();

			Env.Registry.ISU_Rebuild_ThresholdPercentage = 100;

			using (var connection = Db.NewAdminConnection())
			using (AdoTestUtils.CreateDbDropExistingDisposable(connection, dbName))
			{
				new IndexRebuilder(logger).Run(connection, new string[] { dbName });
			}

			AssertEquals($"logger.Count\r\nFull log:\r\n{logger.ToString()}", 1, logger.Count);
			AssertEquals($"Information|Rebuilding indexes on database {dbName.QuoteName()} skipped due to configuration (Rebuild Threshold = 100)", logger[0]);
		}

		#region Implementation

		static void AssertLogContains(TestServiceLogger logger, string expected)
		{
			Assert("Expected: " + expected + "\r\nActual:\r\n" + logger.ToString(), logger.ToString().Contains(expected));
		}

		static void PopulateTable3(string testDbName, int numToCreate, int startId)
		{
			for (int i = 0; i < numToCreate; i++)
			{
				using (var cmd = Db.Connection.Command($"INSERT {testDbName.QuoteName()}.dbo.Table3 (id, x) VALUES (@id, NEWID());"))
				{
					cmd.AddParameter("@id", System.Data.SqlDbType.Int, startId + i);
					cmd.ExecuteNonQuery();
				}
			}
		}

		static void SetRegistryForRebuild()
		{
			Env.Registry.ISU_MinimumIndexPageCount = 1;

			Env.Registry.ISU_Rebuild_ThresholdPercentage = 1;
			Env.Registry.ISU_Reorganize_Threshold = 100;
			Env.Registry.ISU_Reorganize_PeriodInDays = 100;

			Env.Registry.ISU_Rebuild_MaxDopPercentage = 1;

			Env.Registry.ISU_Rebuild_Online = false;
			Env.Registry.ISU_Rebuild_MaxWaitInMinutes = 0;
			Env.Registry.ISU_Rebuild_AbortAfterWait = OnlineIndexRebuildLowPriorityAbortAfterWaitList.Codes.Blockers;

			Env.Registry.ISU_Rebuild_UseObserver = false;
		}

		void TestLongRunningSqlKilledByISUCore(string dbName, string waitforDelay, Action<bool, string, string, string, TestServiceLogger, TestServiceLogger> assertions)
		{
			var table = "_Table_1";
			var index_1 = "_Index_1";

			Env.Registry.ISU_Rebuild_Online = true;
			Env.Registry.ISU_Rebuild_AbortAfterWait = OnlineIndexRebuildLowPriorityAbortAfterWaitList.Codes.Blockers;

			using (AdoTestUtils.CreateDbDropExistingDisposable(dbName, Db.DatabaseName))
			{
				using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
				{
					var sql = $@"
if (OBJECT_ID(N'dbo.[{table}]', N'U') is NOT NULL) DROP TABLE dbo.[{table}];
CREATE TABLE dbo.[{table}]
(
	TT_PK     uniqueidentifier,
	TT_Parent uniqueidentifier,
);

CREATE NONCLUSTERED INDEX [{index_1}] ON dbo.[{table}] (TT_PK);

;WITH
	L0 AS (SELECT n = NULL FROM (VALUES (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL)) AS t(n)),
	L1 AS (SELECT n = NULL FROM L0 AS A CROSS JOIN L0 AS B),
	L2 AS (SELECT n = NULL FROM L1 AS A CROSS JOIN L1 AS B),
	L3 AS (SELECT n = NULL FROM L2 AS A CROSS JOIN L2 AS B)
INSERT
	dbo.[{table}] WITH (TABLOCKX) (TT_PK, TT_Parent)
SELECT TOP (800)
	TT_PK     = NEWID(),
	TT_Parent = CASE WHEN ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) % 3 = 0 THEN NEWID() ELSE NULL END
FROM
	L3;

CREATE TABLE dbo.[StmExtendedProperty]
(
   [SEP_Class] VARCHAR(60) NOT NULL DEFAULT '',
   [SEP_DatabaseNameSuffix] VARCHAR(128) NOT NULL DEFAULT '',
   [SEP_SchemaName] NVARCHAR(128) NOT NULL DEFAULT '',
   [SEP_MajorObjectName] NVARCHAR(128) NOT NULL DEFAULT '',
   [SEP_MinorObjectName] NVARCHAR(128) NOT NULL DEFAULT '',
   [SEP_Name] VARCHAR(200) NOT NULL DEFAULT '',
   [SEP_Value] VARCHAR(200) NOT NULL DEFAULT '',
)
";

					Db.Connection.ExecuteNonQuery(sql);
				}

				var wasKilled = false;
				var wasRun = false;

				var logger = new TestServiceLogger();
				var blockerLogger = new TestServiceLogger();
				var rebuilder = new IndexRebuilder(logger);
				Task task = null;

				rebuilder.UserActionRebuild3_ForTest = () =>
				{
					if (task == null)
					{
						using (var resetEvent = new AutoResetEvent(initialState: false))
						{
							task = Task.Run(() =>
							{
								using (var connection = Db.NewAdminConnection(dbName))
								using (connection.BeginTransactionWithManager())
								{
									try
									{
										connection.ExecuteNonQuery($"ALTER TABLE [dbo].[{table}] ADD new_field bit;");
										resetEvent.Set();
										wasRun = true;
										connection.ExecuteNonQuery($"WAITFOR DELAY {waitforDelay};");
									}
									catch (SqlException ex)
									{
										wasKilled = true;
										blockerLogger.Log(LogType.Error, ex.Message);
									}
								}
							});

							resetEvent.WaitOne(TimeSpan.FromSeconds(30));
						}
					}
				};

				using (var connection = Db.NewAdminConnection())
				{
					rebuilder.Run(connection, new string[] { dbName });
				}

				task.Wait(); // first task.

				var fullLog = $"\r\nFull log:\r\n{logger}";
				CombineAssertions(() =>
				{
					AssertEquals("Blocking process was not run.", true, wasRun);
					assertions(wasKilled, fullLog, table, index_1, logger, blockerLogger);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			SetRegistryForRebuild();
		}

		#endregion // Implementation
	}
}
