using System;
using System.Reflection;
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
	class IndexReorganiserTest : TestCase
	{
		public void TestLockTimeout_Reorganize()
		{
			// Arrange
			var dbName = Db.Connection.CurrentDatabase + "_SD_LockTimeout_Reorganize_TestDb-AA9E106A9A874ED89B488C7667C14623";
			var tableName_1 = "_TestTable_1";
			var indexName_1_1 = "_TestIndex_1_1";
			var indexName_1_2 = "_TestIndex_1_2";
			var tableName_2 = "_TestTable_2";
			var indexName_2_1 = "_TestIndex_2_1";

			#region Prepare data

			Env.Registry.ISU_MinimumIndexPageCount = 1;
			Env.Registry.ISU_Rebuild_ThresholdPercentage = 100;
			Env.Registry.ISU_Reorganize_Threshold = 1;
			Env.Registry.ISU_Reorganize_PeriodInDays = 0;
			Env.Registry.ISU_Rebuild_MaxDopPercentage = 100;
			Env.Registry.ISU_Reorganize_MaxConcurrentProcessesPercentage = 1;

			DbWorker.DropTestDbIfExists(dbName);
			DbWorker.CreateTestDb(dbName);

			using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
			{
				var sql = $@"
if (OBJECT_ID(N'dbo.[{tableName_1}]', N'U') is NOT NULL) DROP TABLE dbo.[{tableName_1}];
CREATE TABLE dbo.[{tableName_1}]
(
	TT_PK     uniqueidentifier,
	TT_Parent uniqueidentifier,
);

CREATE NONCLUSTERED INDEX [{indexName_1_1}] ON dbo.[{tableName_1}] (TT_PK);
CREATE NONCLUSTERED INDEX [{indexName_1_2}] ON dbo.[{tableName_1}] (TT_Parent) WHERE (TT_Parent is NOT NULL);

;WITH
	L0 AS (SELECT n = NULL FROM (VALUES (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL)) AS t(n)),
	L1 AS (SELECT n = NULL FROM L0 AS A CROSS JOIN L0 AS B),
	L2 AS (SELECT n = NULL FROM L1 AS A CROSS JOIN L1 AS B),
	L3 AS (SELECT n = NULL FROM L2 AS A CROSS JOIN L2 AS B)
INSERT
	dbo.[{tableName_1}] WITH (TABLOCKX) (TT_PK, TT_Parent)
SELECT TOP (800)
	TT_PK     = NEWID(),
	TT_Parent = CASE WHEN ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) % 3 = 0 THEN NEWID() ELSE NULL END
FROM
	L3;

if (OBJECT_ID(N'dbo.[{tableName_2}]', N'U') is NOT NULL) DROP TABLE dbo.[{tableName_2}];
CREATE TABLE dbo.[{tableName_2}]
(
	TT_PK     uniqueidentifier,
	TT_Parent uniqueidentifier,
);

CREATE NONCLUSTERED INDEX [{indexName_2_1}] ON dbo.[{tableName_2}] (TT_PK);

;WITH
	L0 AS (SELECT n = NULL FROM (VALUES (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL)) AS t(n)),
	L1 AS (SELECT n = NULL FROM L0 AS A CROSS JOIN L0 AS B),
	L2 AS (SELECT n = NULL FROM L1 AS A CROSS JOIN L1 AS B),
	L3 AS (SELECT n = NULL FROM L2 AS A CROSS JOIN L2 AS B)
INSERT
	dbo.[{tableName_2}] WITH (TABLOCKX) (TT_PK, TT_Parent)
SELECT TOP (400)
	TT_PK     = NEWID(),
	TT_Parent = CASE WHEN ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) % 2 = 0 THEN NEWID() ELSE NULL END
FROM
	L3;
WAITFOR DELAY '00:00:02';
";

				Db.Connection.ExecuteNonQuery(sql);
			}

			#endregion // Prepare data

			var lockTimeout = Env.Registry.LockTimeout;

			try
			{
				Env.Registry.LockTimeout = 1;
				Db.Connection.CloseConnection();
				Db.Connection.EnsureIsOpen();

				var logger = new TestServiceLogger();
				var updater = new IndexReorganiser(logger);

				Task taskAcquiringExclusionLock = null;
				using (var exclusionLockAcquiredByTaskEvent = new ManualResetEventSlim(false))
				using (var releaseExclusionLockEvent = new ManualResetEventSlim(false))
				using (new DisposableAction(() =>
				{
					releaseExclusionLockEvent.Set();
					taskAcquiringExclusionLock.GetAwaiter().GetResult();
				}))
				{
					updater.UserActionReorganize_ForTest = () =>
					{
						taskAcquiringExclusionLock = Task.Run(() =>
						{
							using (Db.DisposableActionForDbConnection())
							using (var connection = Db.NewAdminConnection(dbName))
							{
								connection.BeginTransaction();
								connection.ExecuteNonQuery($"ALTER TABLE [dbo].[{tableName_1}] ADD new_field bit");
								exclusionLockAcquiredByTaskEvent.Set();
								releaseExclusionLockEvent.Wait();
							}
						});
						exclusionLockAcquiredByTaskEvent.Wait();
					};

					using (var connection = Db.NewAdminConnection())
					{
						// Act
						updater.Run(connection, new string[] { dbName });
					}
				}

				// Assert
				CombineAssertions(() =>
				{
					AssertEquals("Full log:\r\n" + logger.ToString(), 17, logger.Count);

					AssertEquals("Reorganizing index_1_1", true, logger[8].StartsWith($"Information|Reorganizing fragmented index: [{dbName}].[{tableName_1}].[{indexName_1_1}];"));
					AssertEquals("Re-queue due to LockTimeout: index_1_1", true, logger[9].StartsWith($"Information|Re-queued due to LockTimeout: [{dbName}].[{tableName_1}].[{indexName_1_1}];"));
					AssertEquals("Reorganizing index_2_1", true, logger[10].StartsWith($"Information|Reorganizing fragmented index: [{dbName}].[{tableName_2}].[{indexName_2_1}];"));
					AssertEquals("Reorganizing index_1_2", true, logger[11].StartsWith($"Information|Reorganizing fragmented index: [{dbName}].[{tableName_1}].[{indexName_1_2}];"));
					AssertEquals("Re-queue due to LockTimeout: index_1_2", true, logger[12].StartsWith($"Information|Re-queued due to LockTimeout: [{dbName}].[{tableName_1}].[{indexName_1_2}];"));
					AssertEquals("Reorganizing index_1_1", true, logger[13].StartsWith($"Information|Reorganizing fragmented index: [{dbName}].[{tableName_1}].[{indexName_1_1}];"));
					AssertEquals("Skipped due to LockTimeout: index_1_1", true, logger[14].StartsWith($"Information|Skipped due to LockTimeout: [{dbName}].[{tableName_1}].[{indexName_1_1}];"));
					AssertEquals("Reorganizing index_1_2", true, logger[15].StartsWith($"Information|Reorganizing fragmented index: [{dbName}].[{tableName_1}].[{indexName_1_2}];"));
					AssertEquals("Skipped due to LockTimeout: index_1_2", true, logger[16].StartsWith($"Information|Skipped due to LockTimeout: [{dbName}].[{tableName_1}].[{indexName_1_2}];"));
				});
			}
			finally
			{
				Env.Registry.LockTimeout = lockTimeout;
				Db.Connection.CloseConnection();
				Db.Connection.EnsureIsOpen();

				DbWorker.DropTestDbIfExists(dbName);
			}
		}

		public void TestCallsBacklogWaiterWithExpectedMaxTimeFromRegistry()
		{
			// Arrange
			var dbName = $"{Db.Connection.CurrentDatabase}_SD_IndexReorganiser-{nameof(TestCallsBacklogWaiterWithExpectedMaxTimeFromRegistry)}";

			using (AdoTestUtils.CreateDbDropExistingDisposable(dbName))
			{
				Env.Registry.ISU_MinimumIndexPageCount = 1;
				Env.Registry.ISU_Rebuild_ThresholdPercentage = 100;
				Env.Registry.ISU_Reorganize_Threshold = 1;
				Env.Registry.ISU_Reorganize_PeriodInDays = 0;
				Env.Registry.ISU_Rebuild_MaxDopPercentage = 100;
				Env.Registry.ISU_Reorganize_MaxConcurrentProcessesPercentage = 100;

				var tableName = "_TestTable";
				var indexName = "_TestIndex";

				var sql = $@"
if (OBJECT_ID(N'dbo.[{tableName}]', N'U') is NOT NULL) DROP TABLE dbo.[{tableName}];
CREATE TABLE dbo.[{tableName}]
(
	TT_PK     uniqueidentifier,
	TT_Parent uniqueidentifier,
);

CREATE NONCLUSTERED INDEX [{indexName}] ON dbo.[{tableName}] (TT_PK);

;WITH
	L0 AS (SELECT n = NULL FROM (VALUES (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL)) AS t(n)),
	L1 AS (SELECT n = NULL FROM L0 AS A CROSS JOIN L0 AS B),
	L2 AS (SELECT n = NULL FROM L1 AS A CROSS JOIN L1 AS B),
	L3 AS (SELECT n = NULL FROM L2 AS A CROSS JOIN L2 AS B)
INSERT
	dbo.[{tableName}] WITH (TABLOCKX) (TT_PK, TT_Parent)
SELECT TOP (400)
	TT_PK     = NEWID(),
	TT_Parent = CASE WHEN ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) % 2 = 0 THEN NEWID() ELSE NULL END
FROM
	L3;
";
				using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
				{
					Db.Connection.ExecuteNonQuery(sql);
				}

				var mockBacklogWaiter = new Mock<ILowPriorityProcessPauser>();
				var logger = new TestServiceLogger();
				var updater = new IndexReorganiser(logger, _ => mockBacklogWaiter.Object)
				{
					reorganizeDelay_ms_ForTest = 100
				};
				var maxWaitMinutes = Env.Registry.ISU_MaxBacklogWaitTime_InMinutes;

				using (var connection = Db.NewAdminConnection())
				{
					updater.Run(connection, new[] { dbName });
				}

				// Assert
				AssertNoExceptionThrown(() =>
					mockBacklogWaiter.Verify(
						m => m.Wait(It.IsAny<ILogger>(), TimeSpan.FromMinutes(maxWaitMinutes), null),
						Times.Once));
			}
		}

		public void TestRethrowsBacklogWaiterTimeoutException()
		{
			// Arrange
			var dbName = $"{Db.Connection.CurrentDatabase}_SD_IndexReorganiser-{nameof(TestRethrowsBacklogWaiterTimeoutException)}";

			using (AdoTestUtils.CreateDbDropExistingDisposable(dbName))
			{
				Env.Registry.ISU_MinimumIndexPageCount = 1;
				Env.Registry.ISU_Rebuild_ThresholdPercentage = 100;
				Env.Registry.ISU_Reorganize_Threshold = 1;
				Env.Registry.ISU_Reorganize_PeriodInDays = 0;
				Env.Registry.ISU_Rebuild_MaxDopPercentage = 100;
				Env.Registry.ISU_Reorganize_MaxConcurrentProcessesPercentage = 100;

				var tableName = "_TestTable";
				var indexName = "_TestIndex";

				var sql = $@"
if (OBJECT_ID(N'dbo.[{tableName}]', N'U') is NOT NULL) DROP TABLE dbo.[{tableName}];
CREATE TABLE dbo.[{tableName}]
(
	TT_PK     uniqueidentifier,
	TT_Parent uniqueidentifier,
);

CREATE NONCLUSTERED INDEX [{indexName}] ON dbo.[{tableName}] (TT_PK);

;WITH
	L0 AS (SELECT n = NULL FROM (VALUES (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL)) AS t(n)),
	L1 AS (SELECT n = NULL FROM L0 AS A CROSS JOIN L0 AS B),
	L2 AS (SELECT n = NULL FROM L1 AS A CROSS JOIN L1 AS B),
	L3 AS (SELECT n = NULL FROM L2 AS A CROSS JOIN L2 AS B)
INSERT
	dbo.[{tableName}] WITH (TABLOCKX) (TT_PK, TT_Parent)
SELECT TOP (400)
	TT_PK     = NEWID(),
	TT_Parent = CASE WHEN ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) % 2 = 0 THEN NEWID() ELSE NULL END
FROM
	L3;
";
				using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
				{
					Db.Connection.ExecuteNonQuery(sql);
				}

				var mockBacklogWaiter = new Mock<ILowPriorityProcessPauser>();
				mockBacklogWaiter.Setup(m => m.Wait(It.IsAny<ILogger>(), It.IsAny<TimeSpan>(), null))
					.Throws(new BacklogWaiterTimeoutException(Guid.NewGuid().ToString()));
				var logger = new TestServiceLogger();
				var updater = new IndexReorganiser(logger, _ => mockBacklogWaiter.Object)
				{
					reorganizeDelay_ms_ForTest = 100
				};

				using (var connection = Db.NewAdminConnection())
				{
					//Act
					// Assert
					AssertExceptionThrown<BacklogWaiterTimeoutException>(() => updater.Run(connection, new string[] { dbName }));
					mockBacklogWaiter.Verify(m => m.Wait(It.IsAny<ILogger>(), It.IsAny<TimeSpan>(), null), Times.Once);
				}
			}
		}

		[SnailTest]
		public void TestCurrentReorganizes()
		{
			var dbName = Db.Connection.CurrentDatabase + "_SD_TestDb-C2847F4006D049E581281459319F6AFB";
			var tableName = "_TestTable";
			var indexName_1 = "_TestIndex_1";
			var indexName_2 = "_TestIndex_2";

			Env.Registry.ISU_MinimumIndexPageCount = 1;
			Env.Registry.ISU_Rebuild_ThresholdPercentage = 100;
			Env.Registry.ISU_Reorganize_Threshold = 1;
			Env.Registry.ISU_Reorganize_PeriodInDays = 0;
			Env.Registry.ISU_Rebuild_MaxDopPercentage = 100;
			Env.Registry.ISU_Reorganize_MaxConcurrentProcessesPercentage = 100;

			try
			{
				DbWorker.DropTestDbIfExists(dbName);
				DbWorker.CreateTestDb(dbName);

				using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
				{
					var sql = $@"
if (OBJECT_ID(N'dbo.[{tableName}]', N'U') is NOT NULL) DROP TABLE dbo.[{tableName}];
CREATE TABLE dbo.[{tableName}]
(
	TT_PK     uniqueidentifier,
	TT_Parent uniqueidentifier,
);

CREATE NONCLUSTERED INDEX [{indexName_1}] ON dbo.[{tableName}] (TT_PK);
CREATE NONCLUSTERED INDEX [{indexName_2}] ON dbo.[{tableName}] (TT_Parent) WHERE (TT_Parent is NOT NULL);

;WITH
	L0 AS (SELECT n = NULL FROM (VALUES (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL)) AS t(n)),
	L1 AS (SELECT n = NULL FROM L0 AS A CROSS JOIN L0 AS B),
	L2 AS (SELECT n = NULL FROM L1 AS A CROSS JOIN L1 AS B),
	L3 AS (SELECT n = NULL FROM L2 AS A CROSS JOIN L2 AS B)
INSERT
	dbo.[{tableName}] WITH (TABLOCKX) (TT_PK, TT_Parent)
SELECT TOP (400)
	TT_PK     = NEWID(),
	TT_Parent = CASE WHEN ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) % 2 = 0 THEN NEWID() ELSE NULL END
FROM
	L3;
";

					Db.Connection.ExecuteNonQuery(sql);
				}

				var logger = new TestServiceLogger();
				using (var connection = Db.NewAdminConnection())
				{
					var updater = new IndexReorganiser(logger);
					updater.reorganizeDelay_ms_ForTest = 100;
					updater.Run(connection, new string[] { dbName });
				}

				CombineAssertions(() =>
				{
					AssertEquals("Current wrong log:\r\n" + logger.ToString(), 9, logger.Count);

					AssertEquals("Reorganizing index 1", true, logger[7].StartsWith($"Information|Reorganizing fragmented index: [{dbName}].[{tableName}].[{indexName_1}];"));
					AssertEquals("Current reorganizes 1 header + details", true, logger[7].Contains("Currently running reorganizes:\r\nNo current reorganizes detected"));

					AssertEquals("Reorganizing index 2", true, logger[8].StartsWith($"Information|Reorganizing fragmented index: [{dbName}].[{tableName}].[{indexName_2}];"));
					AssertEquals("Current reorganizes 2 header", true, logger[8].Contains("Currently running reorganizes:"));
					AssertEquals("Current reorganizes 2 details", true, logger[8].Contains($"Index: [{dbName}].[{tableName}].[{indexName_1}];"));
				});
			}
			finally
			{
				DbWorker.DropTestDbIfExists(dbName);
			}
		}

		[SnailTest]
		public void TestReorganizeTimeout()
		{
			Env.Registry.ISU_MinimumIndexPageCount = 1;
			Env.Registry.ISU_Rebuild_ThresholdPercentage = 100;
			Env.Registry.ISU_Reorganize_Threshold = 0;
			Env.Registry.ISU_Reorganize_PeriodInDays = 0;
			Env.Registry.ISU_Rebuild_MaxDopPercentage = 100;
			Env.Registry.ISU_Reorganize_MaxConcurrentProcessesPercentage = 100;

			var dbName = Db.Connection.CurrentDatabase + "_SD_TestDb-C2847F4006D049E581281459319F6AFB";
			var tableName = "_TestTable";
			var indexName = "_TestIndex";

			try
			{
				DbWorker.DropTestDbIfExists(dbName);
				DbWorker.CreateTestDb(dbName);

				using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
				{
					var sql = $@"
if (OBJECT_ID(N'dbo.[{tableName}]', N'U') is NOT NULL) DROP TABLE dbo.[{tableName}];
CREATE TABLE dbo.[{tableName}]
(
	TT_PK  uniqueidentifier,
	TT_Val varchar(8000),
);

INSERT dbo.[{tableName}] WITH (TABLOCKX) (TT_PK) VALUES
	(NEWID()),
	(NEWID())

CREATE NONCLUSTERED INDEX [{indexName}] ON dbo.[{tableName}] (TT_PK) INCLUDE (TT_Val);

UPDATE dbo.[{tableName}] WITH (TABLOCKX) SET
	TT_Val = REPLICATE('x', 5000)
";

					Db.Connection.ExecuteNonQuery(sql);
				}

				var logger = new TestServiceLogger();
				var updater = new IndexReorganiser(logger)
				{
					TimeoutForReorganize_ForTest = 1 // 1 second
				};

				AssertEquals("PRECONDITION", null, ExtProperty.Index.Select(Db.Connection, dbName, "dbo", tableName, indexName, IndexUpdater.LastReorganisePtyName));

				using (var blocker = Db.NewAdminConnection(dbName))
				using (blocker.BeginTransactionWithManager())
				{
					blocker.ExecuteNonQuery($"UPDATE dbo.[{tableName}] SET TT_Val = 'x';");

					using (var connection = Db.NewAdminConnection())
					{
						updater.Run(connection, new string[] { dbName });
					}
				}

				CombineAssertions(() =>
				{
					AssertEquals("Index reorganize has been timed out", null, ExtProperty.Index.Select(Db.Connection, dbName, "dbo", tableName, indexName, IndexUpdater.LastReorganisePtyName));
					AssertEquals($"Current log:\r\n{logger.ToString()}", 7, logger.Count);
					AssertEquals($"Reorganizing index\r\n{logger[6]}", true, logger[6].StartsWith($"Information|Reorganizing fragmented index: [{dbName}].[{tableName}].[{indexName}];"));
				});
			}
			finally
			{
				DbWorker.DropTestDbIfExists(dbName);
			}
		}

		public void TestHandlesInvalidDateTimeExtendedPropertyValue()
		{
			var dayInTheFuture = DateTime.UtcNow.Date.AddYears(1);
			var testDbName = Db.Connection.CurrentDatabase + "_SD_TestDb-A0F20D6304C94294A69F01FC2BAFC7E3";

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
				var updater = new IndexReorganiser(logger);
				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					updater.Run(connection, new string[] { testDbName });
				}

				AssertEquals($"logger.Count\r\nFull log:\r\n{logger.ToString()}", 7, logger.Count);
				AssertEquals($"Information|Checking for indexes to reorganise: DB {testDbName.QuoteName()}", logger[0]);

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
				AssertEquals($"Information|Checking for indexes to reorganise: DB [{testDbName}]", logger[0]);

				// Dates in the future - char datatype, proper date format. Should be converted to date time.
				ExtProperty.Index.Update(Db.Connection, schema, table_1, index_1, propertyName: IndexUpdater.LastRebuildPtyName, value: SqlFormatInfo.ToSqlDateTimeString(dayInTheFuture));
				ExtProperty.Index.Update(Db.Connection, schema, table_1, index_1, propertyName: IndexUpdater.LastReorganisePtyName, value: SqlFormatInfo.ToSqlDateTimeString(dayInTheFuture));

				logger.ClearLog();
				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					updater.Run(connection, new string[] { testDbName });
				}

				AssertEquals($"logger.Count\r\nFull log:\r\n{logger.ToString()}", 7, logger.Count);
				AssertEquals($"Information|Checking for indexes to reorganise: DB [{testDbName}]", logger[0]);

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
				AssertEquals($"Information|Checking for indexes to reorganise: DB [{testDbName}]", logger[0]);
			}
			finally
			{
				DbWorker.DropTestDbIfExists(testDbName);
			}
		}

		[ExpectNoExceptions]
		public void TestUsingDbConnectionWhenReorganizeInSeparateThread()
		{
			var testDbName = Db.Connection.CurrentDatabase + "_SD_TestDb_CB273A3271D74AB28BE9014C04175A2C";

			try
			{
				DbWorker.CreateTestDb(testDbName);

				var sql = $@"
CREATE TABLE {testDbName.QuoteName()}.dbo.Table3
(
	id int              NOT NULL PRIMARY KEY, 
	x  uniqueidentifier     NULL,
)
;

CREATE NONCLUSTERED INDEX Table3_X ON {testDbName.QuoteName()}.dbo.Table3(x);
";
				Db.Connection.ExecuteNonQuery(sql);

				PopulateTable3(testDbName, 100, 0);

				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					var registrySettings = new IndexUpdateSettings();
					typeof(IndexUpdateSettings).GetField("Reorganize_ThresholdPercentage", BindingFlags.Instance | BindingFlags.Public).SetValue(registrySettings, -10);
					typeof(IndexUpdateSettings).GetField("MinimumIndexPageCount", BindingFlags.Instance | BindingFlags.Public).SetValue(registrySettings, 0);
					var logger = new MyTestLogger();
					var updater = new IndexReorganiser(logger);
					typeof(IndexReorganiser).GetField("registrySettings", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(updater, registrySettings);
					updater.Run(connection, new string[] { testDbName });
				}
			}
			finally
			{
				DbWorker.DropTestDbIfExists(testDbName);
			}
		}

		public void TestPageLocksForReorganizing()
		{
			var dbName = Db.Connection.CurrentDatabase + "_SD_TestDb-C2847F4006D049E581281459319F6AFB";
			var tableName = "_TestTable";
			var indexName_1 = "_TestIndex_1";
			var indexName_2 = "_TestIndex_2";

			Env.Registry.ISU_MinimumIndexPageCount = 1;
			Env.Registry.ISU_Rebuild_ThresholdPercentage = 100;
			Env.Registry.ISU_Reorganize_Threshold = 1;
			Env.Registry.ISU_Reorganize_PeriodInDays = 0;
			Env.Registry.ISU_Rebuild_MaxDopPercentage = 100;
			Env.Registry.ISU_Reorganize_MaxConcurrentProcessesPercentage = 100;

			try
			{
				DbWorker.DropTestDbIfExists(dbName);
				DbWorker.CreateTestDb(dbName);

				using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
				{
					var sql = $@"
if (OBJECT_ID(N'dbo.{tableName.QuoteName()}', N'U') is NOT NULL) DROP TABLE dbo.{tableName.QuoteName()};
CREATE TABLE dbo.{tableName.QuoteName()}
(
	TT_PK     uniqueidentifier,
	TT_Parent uniqueidentifier,
);

CREATE NONCLUSTERED INDEX {indexName_1.QuoteName()} ON dbo.{tableName.QuoteName()} (TT_PK) WITH (ALLOW_PAGE_LOCKS = OFF);
CREATE NONCLUSTERED INDEX {indexName_2.QuoteName()} ON dbo.{tableName.QuoteName()} (TT_Parent) WHERE (TT_Parent is NOT NULL);

;WITH
	L0 AS (SELECT n = NULL FROM (VALUES (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL), (NULL)) AS t(n)),
	L1 AS (SELECT n = NULL FROM L0 AS A CROSS JOIN L0 AS B),
	L2 AS (SELECT n = NULL FROM L1 AS A CROSS JOIN L1 AS B),
	L3 AS (SELECT n = NULL FROM L2 AS A CROSS JOIN L2 AS B)
INSERT
	dbo.{tableName.QuoteName()} WITH (TABLOCKX) (TT_PK, TT_Parent)
SELECT TOP (400)
	TT_PK     = NEWID(),
	TT_Parent = CASE WHEN ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) % 2 = 0 THEN NEWID() ELSE NULL END
FROM
	L3;
";
					Db.Connection.ExecuteNonQuery(sql);
				}

				var logger = new TestServiceLogger();
				using (var connection = Db.NewAdminConnection())
				{
					new IndexReorganiser(logger).Run(connection, new string[] { dbName });
				}

				CombineAssertions(() =>
				{
					AssertEquals($"Current wrong log:\r\nFull log:\r\n{logger.ToString()}", 8, logger.Count);

					// index_1 skipped because reorganizing requires PAGE_LOCK
					AssertEquals("Reorganizing index 2", true, logger[7].StartsWith($"Information|Reorganizing fragmented index: {dbName.QuoteName()}.{tableName.QuoteName()}.{indexName_2.QuoteName()};"));
					AssertEquals("Current reorganizes 2 header + details", true, logger[7].Contains("Currently running reorganizes:\r\nNo current reorganizes detected"));
				});
			}
			finally
			{
				DbWorker.DropTestDbIfExists(dbName);
			}
		}

		public void TestSkipQuerying()
		{
			var dbName = Db.Connection.CurrentDatabase + "_SD_Reorganize_TestDb-AA9E106A9A874ED89B488C7667C14623";
			var logger = new TestServiceLogger();

			Env.Registry.ISU_Reorganize_Threshold = 100;

			using (var connection = Db.NewAdminConnection())
			using (AdoTestUtils.CreateDbDropExistingDisposable(connection, dbName))
			{
				new IndexReorganiser(logger).Run(connection, new string[] { dbName });
			}

			AssertEquals($"logger.Count\r\nFull log:\r\n{logger.ToString()}", 1, logger.Count);
			AssertEquals($"Information|Reorganizing indexes on database {dbName.QuoteName()} skipped due to configuration (Reorganize Threshold = 100)", logger[0]);
		}

		public void TestClientTableDroppedInAnOnlinePostUpgradeTransform()
		{
			// Arrange
			var dbName = Db.Connection.CurrentDatabase + "_SD_TestDb-C2847F4006D049E581281459319F6AFB";

			using (AdoTestUtils.CreateDbDropExistingDisposable(dbName))
			{
				using (((ICurrentDbControl)Db.Connection).UseDatabase(dbName))
				{
					for (var i = 0; i < 10_000; i++)
					{
						CreateTableWithIndex($"ATestTable{i}");
					}

					CreateTableWithIndex("ClientTestTable");
				}

				var dropCallbackCalled = false;
				var logger = new Mock<ILogger>();
				logger.Setup(l => l.Log(It.IsAny<LogType>(), "Retrieved data for dbo.ATestTable0 (IX_ATestTable0_PK)")) // log for the first table indicates datatable is open
					.Callback(() =>
					{
						// With some bad luck, an Online Post Upgrade Transformation is dropping one of the tables that are subject of IndexUpdater's query
						using var otherConnection = Db.NewAdminConnection(dbName);
						otherConnection.ExecuteNonQuery("DROP TABLE dbo.[ClientTestTable]");
						dropCallbackCalled = true;
					});

				var updater = new IndexReorganiser(logger.Object);
				using var connection = Db.NewAdminConnection();

				// Act
				AssertNoExceptionThrown(() => updater.Run(connection, new [] { dbName }));

				// Assert
				Assert("Table should have been dropped as part of Test Setup", dropCallbackCalled);

				void CreateTableWithIndex(string tableName)
				{
					Db.Connection.ExecuteNonQuery($@"
CREATE TABLE dbo.[{tableName}]
(
	CT_PK     uniqueidentifier,
);

CREATE NONCLUSTERED INDEX [IX_{tableName}_PK] ON dbo.[{tableName}] (CT_PK);");
				}
			}
		}

		#region Implementation

		static void PopulateTable3(string testDbName, int numToCreate, int startId)
		{
			for (int i = 0; i < numToCreate; i++)
			{
				Db.Connection.ExecuteNonQuery($"INSERT {testDbName.QuoteName()}.dbo.Table3 (id, x) VALUES ({startId + i}, NEWID())");
			}
		}

		#endregion // Implementation
	}
}
