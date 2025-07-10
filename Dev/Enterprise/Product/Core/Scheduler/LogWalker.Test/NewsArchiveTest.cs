using System;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LogWalker.Testing
{
	sealed class NewsArchiveTest : TestCaseWithFactory
	{
		public void TestCleanupAncientQueuedLogs()
		{
			var subscriber = new MockSubscriber("MingMing");
			AssertCleanupAncientLogs(new[] { subscriber }, "Bobbus", isDeleted: true);
			AssertCleanupAncientLogs(new[] { subscriber }, "MingMing", isDeleted: false);
			AssertCleanupAncientLogs(Array.Empty<LogSubscriber>(), "Bobbus", isDeleted: false);
		}

		void AssertCleanupAncientLogs(LogSubscriber[] subscribers, string logName, bool isDeleted)
		{
			var queue = Factory.NewWithValidTestData<StmJobQueue>();
			queue.SJ_Status = JobQueueStatus.StatusQueued;
			queue.SJ_FilterName = logName;
			queue.SJ_PostedTimeUtc = ZDateTime.UtcNow.AddDays(-32 * 3);
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();

			new NewsArchive(subscribers).CleanupOldLogs(CancellationToken.None);
			if (isDeleted)
			{
				AssertNull($"There is no {logName} subscriber, so delete the log after 3 months.", newFactory.Load<StmJobQueue>(queue.PK));
			}
			else
			{
				AssertNotNull($"There is a {logName} subscriber, so the log should exist.", newFactory.Load<StmJobQueue>(queue.PK));
			}
		}

		public void TestCleanupOldLogs()
		{
			ZDateTime testStartTime = ZDateTime.UtcNow;
			var subscriber = new MockSubscriber("MingMing");
			Db.Connection.ExecuteNonQuery("TRUNCATE TABLE StmJobQueue;");
			CreateTestLogs(testStartTime, subscriber);

			BusinessObjectFactory preConditionFactory = new BusinessObjectFactory();
			StmJobQueue[] logsBeforePurge = preConditionFactory.Load<StmJobQueue>(new ZQuery());
			AssertEquals("Log count before purge", 7, logsBeforePurge.Length);

			NewsArchive archive = new NewsArchive(new[] { subscriber });
			var testNotifier = new LoggerForTesting() { AllowDebug = true };
			archive.CleanupOldLogs(CancellationToken.None, testNotifier);

			BusinessObjectFactory loadFactory = new BusinessObjectFactory();
			StmJobQueue[] allLogs = loadFactory.Load<StmJobQueue>(new ZQuery());
			AssertEquals("Log count after purge", 5, allLogs.Length);

			StmJobQueue[] queuedLogs = loadFactory.Load<StmJobQueue>(new ZQuery(StmJobQueueSchema.SJ_Status, JobQueueStatus.StatusQueued));
			AssertEquals("QUE Log count", 2, queuedLogs.Length);

			StmJobQueue[] processedLogs = loadFactory.Load<StmJobQueue>(new ZQuery(StmJobQueueSchema.SJ_Status, JobQueueStatus.StatusProcessed));
			AssertEquals("PRS Log count", 1, processedLogs.Length);
			AssertEquals("PRS Log Posted Time not older than 2 days", true, processedLogs[0].SJ_PostedTimeUtc > testStartTime.AddDays(-1));

			StmJobQueue[] failedLogs = loadFactory.Load<StmJobQueue>(new ZQuery(StmJobQueueSchema.SJ_Status, JobQueueStatus.StatusFailed));
			AssertEquals("FAI Log count", 2, failedLogs.Length);
			AssertEquals("FAI Log Posted Time not older than 7 days", true, failedLogs[0].SJ_PostedTimeUtc > testStartTime.AddDays(-7));

			AssertEquals("Log results", "Cleaned:1 PRS log(s), 1 FAL log(s), 0 dead log(s).", testNotifier.NotifiedEventList[0]);
		}

		public void TestCancelNewsArchive()
		{
			ZDateTime testStartTime = ZDateTime.UtcNow;
			var subscriber = new MockSubscriber("MingMing");
			Db.Connection.ExecuteNonQuery("TRUNCATE TABLE StmJobQueue;");
			CreateTestLogs(testStartTime, subscriber);

			BusinessObjectFactory preConditionFactory = new BusinessObjectFactory();
			StmJobQueue[] logsBeforePurge = preConditionFactory.Load<StmJobQueue>(new ZQuery());
			AssertEquals("Log count before purge", 7, logsBeforePurge.Length);

			NewsArchive archive = new NewsArchive(new[] { subscriber });
			var testNotifier = new LoggerForTesting() { AllowDebug = true };

			var tokenSource = new CancellationTokenSource();
			tokenSource.Cancel();
			var token = tokenSource.Token;
			var logger = new LoggerForTest();
			LogWalkerRunner.Master().Process(logger, CancellationToken.None);
			AssertExceptionThrown<OperationCanceledException>(() => archive.CleanupOldLogs(token, testNotifier));

			BusinessObjectFactory loadFactory = new BusinessObjectFactory();
			StmJobQueue[] allLogs = loadFactory.Load<StmJobQueue>(new ZQuery());
			AssertEquals("Log count after purge", 7, allLogs.Length); //I.e. check nothing was actually purged due to cancellation
		}

		public void TestCleanupOldLogs_MultipleSubscribers()
		{
			ZDateTime testStartTime = ZDateTime.UtcNow;
			var subscriber = new MockSubscriber("Sub1");
			var subscriber2 = new MockSubscriber("Sub2");
			Db.Connection.ExecuteNonQuery("TRUNCATE TABLE StmJobQueue;");
			CreateTestLogs(testStartTime, subscriber);
			CreateTestLogs(testStartTime, subscriber2);

			BusinessObjectFactory preConditionFactory = new BusinessObjectFactory();
			StmJobQueue[] logsBeforePurge = preConditionFactory.Load<StmJobQueue>(new ZQuery());
			AssertEquals("Log count before purge", 14, logsBeforePurge.Length);

			NewsArchive archive = new NewsArchive(new[] { subscriber, subscriber2 });
			var testNotifier = new LoggerForTesting() { AllowDebug = true };
			archive.CleanupOldLogs(CancellationToken.None, testNotifier);

			BusinessObjectFactory loadFactory = new BusinessObjectFactory();
			StmJobQueue[] allLogs = loadFactory.Load<StmJobQueue>(new ZQuery());
			AssertEquals("Log count after purge", 10, allLogs.Length);

			StmJobQueue[] queuedLogs = loadFactory.Load<StmJobQueue>(new ZQuery(StmJobQueueSchema.SJ_Status, JobQueueStatus.StatusQueued));
			AssertEquals("QUE Log count", 4, queuedLogs.Length);

			StmJobQueue[] processedLogs = loadFactory.Load<StmJobQueue>(new ZQuery(StmJobQueueSchema.SJ_Status, JobQueueStatus.StatusProcessed));
			AssertEquals("PRS Log count", 2, processedLogs.Length);
			AssertEquals("PRS Log Posted Time not older than 2 days", true, processedLogs[0].SJ_PostedTimeUtc > testStartTime.AddDays(-1));

			StmJobQueue[] failedLogs = loadFactory.Load<StmJobQueue>(new ZQuery(StmJobQueueSchema.SJ_Status, JobQueueStatus.StatusFailed));
			AssertEquals("FAI Log count", 4, failedLogs.Length);
			AssertEquals("FAI Log Posted Time not older than 7 days", true, failedLogs[0].SJ_PostedTimeUtc > testStartTime.AddDays(-7));

			AssertEquals("Log results", "Cleaned:2 PRS log(s), 2 FAL log(s), 0 dead log(s).", testNotifier.NotifiedEventList[0]);
		}

		[UseSnapshotProtection]
		public void TestCleanupOldLogsWithDatabaseLocks()
		{
			var subscriber = new MockSubscriber("MingMing");

			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.ExecuteNonQuery($@"
TRUNCATE TABLE StmJobQueue;
DECLARE @now datetime = GETUTCDATE();
DECLARE @10daysago datetime = DATEADD(d, -10, @now);

INSERT dbo.StmJobQueue (SJ_PK, SJ_PostedTimeUtc, SJ_FilterName, SJ_Status, SJ_ParentID, SJ_ALogReference, SJ_EventTime, SJ_EventTimeUtc, SJ_ParentTableCode) VALUES
	(NEWID(), @now      , '{subscriber.Name}', 'FAI', NEWID(), NEWID(), GETDATE(), GETUTCDATE(), 'JS'),
	(NEWID(), @10daysago, '{subscriber.Name}', 'FAI', NEWID(), NEWID(), GETDATE(), GETUTCDATE(), 'JS'),
	(NEWID(), @10daysago, '{subscriber.Name}', 'PRS', NEWID(), NEWID(), GETDATE(), GETUTCDATE(), 'JS')
;");
			}

			using (var anotherConnection = Db.NewExtraConnectionToMainDb())
			using (anotherConnection.BeginTransactionWithManager())
			{
				anotherConnection.ExecuteNonQuery("SELECT * FROM dbo.StmJobQueue WITH (UPDLOCK) WHERE SJ_STATUS = 'FAI' AND  SJ_PostedTimeUtc < GETUTCDATE();");

				var sqlCount = "SELECT COUNT(*) FROM dbo.StmJobQueue ";
				AssertEquals("Should have all three entries", 3, TestConnection.ExecuteScalar(sqlCount));
				var archive = new NewsArchive(new[] { subscriber });
				archive.CleanupOldLogs(CancellationToken.None);
				AssertEquals("One entry should be deleted", 2, TestConnection.ExecuteScalar(sqlCount));
			}

			var loadFactory = new BusinessObjectFactory();
			var allLogs = loadFactory.Load<StmJobQueue>(new ZQuery());
			AssertEquals("Log count after purge with lock", 2, allLogs.Length);
		}

		public void TestCleanupOldLogs_HasConfigurableBatchSize()
		{
			var subscriber = new MockSubscriber();

			Db.Connection.ExecuteNonQuery("TRUNCATE TABLE dbo.StmJobQueue");
			var log1 = Factory.New<StmJobQueue>();
			log1.SJ_PostedTimeUtc = ZDateTime.UtcNow.AddDays(-10);
			log1.SJ_FilterName = subscriber.Name;
			log1.SJ_Status = JobQueueStatus.StatusProcessed;
			log1.SJ_EventTime = ZDateTime.MinSmallDateTimeValue;
			log1.SJ_EventTimeUtc = ZDateTime.MinSmallDateTimeValue;
			log1.SJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var log2 = Factory.New<StmJobQueue>();
			log2.SJ_PostedTimeUtc = ZDateTime.UtcNow.AddDays(-10);
			log2.SJ_FilterName = subscriber.Name;
			log2.SJ_Status = JobQueueStatus.StatusProcessed;
			log2.SJ_EventTime = ZDateTime.MinSmallDateTimeValue;
			log2.SJ_EventTimeUtc = ZDateTime.MinSmallDateTimeValue;
			log2.SJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var log3 = Factory.New<StmJobQueue>();
			log3.SJ_PostedTimeUtc = ZDateTime.UtcNow.AddDays(-10);
			log3.SJ_FilterName = subscriber.Name;
			log3.SJ_Status = JobQueueStatus.StatusFailed;
			log3.SJ_EventTime = ZDateTime.MinSmallDateTimeValue;
			log3.SJ_EventTimeUtc = ZDateTime.MinSmallDateTimeValue;
			log3.SJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var log4 = Factory.New<StmJobQueue>();
			log4.SJ_PostedTimeUtc = ZDateTime.UtcNow.AddDays(-10);
			log4.SJ_FilterName = subscriber.Name;
			log4.SJ_Status = JobQueueStatus.StatusFailed;
			log4.SJ_EventTime = ZDateTime.MinSmallDateTimeValue;
			log4.SJ_EventTimeUtc = ZDateTime.MinSmallDateTimeValue;
			log4.SJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var log5 = Factory.New<StmJobQueue>();
			log5.SJ_PostedTimeUtc = ZDateTime.UtcNow.AddMonths(-4);
			log5.SJ_FilterName = "Jeff";
			log5.SJ_Status = JobQueueStatus.StatusFailed;
			log5.SJ_EventTime = ZDateTime.MinSmallDateTimeValue;
			log5.SJ_EventTimeUtc = ZDateTime.MinSmallDateTimeValue;
			log5.SJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var log6 = Factory.New<StmJobQueue>();
			log6.SJ_PostedTimeUtc = ZDateTime.UtcNow.AddMonths(-4);
			log6.SJ_FilterName = "Jeff";
			log6.SJ_Status = JobQueueStatus.StatusFailed;
			log6.SJ_EventTime = ZDateTime.MinSmallDateTimeValue;
			log6.SJ_EventTimeUtc = ZDateTime.MinSmallDateTimeValue;
			log6.SJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			Factory.Save();

			using (Db.Connection.TrackExecutedCommands())
			using (SystemDataRegistry.Instance.LogWalkerPurgeBatchSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				var archive = new NewsArchive(new[] { subscriber });
				archive.CleanupOldLogs(CancellationToken.None);
				var executedCommands = Db.Connection.ExecutedCommands;
				AssertEquals(9, executedCommands.Count(c => c.Contains($"DELETE TOP (@BatchSize)")));
			}
		}

		public void TestCleanupOldLogs_LogsCurrentBatchWhenLockErrorOccurs()
		{
			var subscriber1 = new MockSubscriber("subscriber1");
			var subscriber2 = new MockSubscriber("subscriber2");

			Db.Connection.ExecuteNonQuery("TRUNCATE TABLE dbo.StmJobQueue");

			var log1 = Factory.New<StmJobQueue>();
			log1.SJ_PostedTimeUtc = ZDateTime.UtcNow.AddDays(-10);
			log1.SJ_FilterName = subscriber1.Name;
			log1.SJ_Status = JobQueueStatus.StatusProcessed;
			log1.SJ_EventTime = ZDateTime.MinSmallDateTimeValue;
			log1.SJ_EventTimeUtc = ZDateTime.MinSmallDateTimeValue;
			log1.SJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var log2 = Factory.New<StmJobQueue>();
			log2.SJ_PostedTimeUtc = ZDateTime.UtcNow.AddDays(-10);
			log2.SJ_FilterName = subscriber1.Name;
			log2.SJ_Status = JobQueueStatus.StatusProcessed;
			log2.SJ_EventTime = ZDateTime.MinSmallDateTimeValue;
			log2.SJ_EventTimeUtc = ZDateTime.MinSmallDateTimeValue;
			log2.SJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			Factory.Save();

			using (new DisposableAction(() => { Db.Connection.OnExecute += ThrowDeadlockException; }, () => { Db.Connection.OnExecute -= ThrowDeadlockException; }))
			{
				var logger = new LoggerForTesting() { AllowDebug = true };
				var archive = new NewsArchive(new[] { subscriber1, subscriber2 });
				archive.CleanupOldLogs(CancellationToken.None, logger);
				AssertEquals("Log results", "Cleaned:2 PRS log(s), 0 FAL log(s), 0 dead log(s).", logger.NotifiedEventList[1]);
				AssertEquals("Lock error occured on batch no 3 with batch size of 1000", logger.NotifiedEventList[2]);
			}

			void ThrowDeadlockException(DbCommand command)
			{
				if (command.CommandText.Contains($"DELETE TOP (@BatchSize)") && command.GetParameter("@Subscriber").Value.Equals(subscriber2.Name))
				{
					throw SqlExceptionBuilder.CreateSqlException(1205, "Transaction (Process ID 1404) was deadlocked on lock resources with another process and has been chosen as the deadlock victim. Rerun the transaction.");
				}
			}
		}

		void CreateTestLogs(ZDateTime testStartTime, LogSubscriber subscriber)
		{
			// INSERT A MIX OF QUEUED, PROCESSED AND FAILED LOGS in StmJobQueue

			// QUE - 30 days old
			StmJobQueue log1 = Factory.New<StmJobQueue>();
			log1.SJ_FilterName = subscriber.Name;
			log1.SJ_Status = JobQueueStatus.StatusQueued;
			log1.SJ_PostedTimeUtc = testStartTime.AddDays(-30);
			log1.SJ_EventTime = ZDateTime.MinSmallDateTimeValue;
			log1.SJ_EventTimeUtc = ZDateTime.MinSmallDateTimeValue;
			log1.SJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			// QUE - posted time in the future
			StmJobQueue log2 = Factory.New<StmJobQueue>();
			log2.SJ_FilterName = subscriber.Name;
			log2.SJ_Status = JobQueueStatus.StatusQueued;
			log2.SJ_PostedTimeUtc = testStartTime.AddDays(+1);
			log2.SJ_EventTime = ZDateTime.MinSmallDateTimeValue;
			log2.SJ_EventTimeUtc = ZDateTime.MinSmallDateTimeValue;
			log2.SJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			// PRS - 4 days old
			StmJobQueue log3 = Factory.New<StmJobQueue>();
			log3.SJ_FilterName = subscriber.Name;
			log3.SJ_Status = JobQueueStatus.StatusProcessed;
			log3.SJ_PostedTimeUtc = testStartTime.AddDays(-4);
			log3.SJ_EventTime = ZDateTime.MinSmallDateTimeValue;
			log3.SJ_EventTimeUtc = ZDateTime.MinSmallDateTimeValue;
			log3.SJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			// PRS - posted time in the future
			StmJobQueue log4 = Factory.New<StmJobQueue>();
			log4.SJ_FilterName = subscriber.Name;
			log4.SJ_Status = JobQueueStatus.StatusProcessed;
			log4.SJ_PostedTimeUtc = testStartTime.AddDays(+1);
			log4.SJ_EventTime = ZDateTime.MinSmallDateTimeValue;
			log4.SJ_EventTimeUtc = ZDateTime.MinSmallDateTimeValue;
			log4.SJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			// FAI - 10 days old
			StmJobQueue log5 = Factory.New<StmJobQueue>();
			log5.SJ_FilterName = subscriber.Name;
			log5.SJ_Status = JobQueueStatus.StatusFailed;
			log5.SJ_PostedTimeUtc = testStartTime.AddDays(-10);
			log5.SJ_EventTime = ZDateTime.MinSmallDateTimeValue;
			log5.SJ_EventTimeUtc = ZDateTime.MinSmallDateTimeValue;
			log5.SJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			// FAI - 5 days old
			StmJobQueue log6 = Factory.New<StmJobQueue>();
			log6.SJ_FilterName = subscriber.Name;
			log6.SJ_Status = JobQueueStatus.StatusFailed;
			log6.SJ_PostedTimeUtc = testStartTime.AddDays(-5);
			log6.SJ_EventTime = ZDateTime.MinSmallDateTimeValue;
			log6.SJ_EventTimeUtc = ZDateTime.MinSmallDateTimeValue;
			log6.SJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			// FAI - posted time in the future
			StmJobQueue log7 = Factory.New<StmJobQueue>();
			log7.SJ_FilterName = subscriber.Name;
			log7.SJ_Status = JobQueueStatus.StatusFailed;
			log7.SJ_PostedTimeUtc = testStartTime.AddDays(+1);
			log7.SJ_EventTime = ZDateTime.MinSmallDateTimeValue;
			log7.SJ_EventTimeUtc = ZDateTime.MinSmallDateTimeValue;
			log7.SJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			Factory.Save();
		}
	}
}
