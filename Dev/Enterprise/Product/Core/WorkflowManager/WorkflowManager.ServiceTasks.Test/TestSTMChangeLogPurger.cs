using System;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	class TestStmChangeLogPurger : TestCaseWithFactory
	{
		public void TestPurge()
		{
			var testStartTime = ZDateTime.UtcNow;
			Db.Connection.ExecuteNonQuery("TRUNCATE TABLE StmChangeLog");
			CreateTestLogs(testStartTime);

			var prePurgeFactory = new BusinessObjectFactory();
			var logsBeforePurge = prePurgeFactory.Load<StmChangeLog>(new ZQuery());

			AssertEquals(6, logsBeforePurge.Length);

			var notification = new NotificationLoggerWomboComboForTest();
			var purger = new StmChangeLogPurger(Db.Connection);
			purger.Purge(CancellationToken.None);

			var loadFactory = new BusinessObjectFactory();
			var allLogsAfterPurge = loadFactory.Load<StmChangeLog>(new ZQuery());

			AssertEquals("Log count after purge", 4, allLogsAfterPurge.Length);
			//AssertEquals("Log processed time not older than 7 days", true, allLogsAfterPurge[0].SY_PostedTimeUtc > testStartTime.AddDays(-7));
		}

		public void TestCancelPurge()
		{
			var testStartTime = ZDateTime.UtcNow;
			Db.Connection.ExecuteNonQuery("TRUNCATE TABLE StmChangeLog");
			CreateTestLogs(testStartTime);

			var prePurgeFactory = new BusinessObjectFactory();
			var logsBeforePurge = prePurgeFactory.Load<StmChangeLog>(new ZQuery());

			AssertEquals(6, logsBeforePurge.Length);

			var purger = new StmChangeLogPurger(Db.Connection);

			var tokenSource = new CancellationTokenSource();
			tokenSource.Cancel();
			var token = tokenSource.Token;

			AssertExceptionThrown<OperationCanceledException>(() => purger.Purge(token));

			var loadFactory = new BusinessObjectFactory();
			var allLogs = loadFactory.Load<StmChangeLog>(new ZQuery());

			AssertEquals("Log count after purge has been canceled", 6, allLogs.Length);
		}

		[UseSnapshotProtection]
		public void TestPurgeWithDatabaseLocks()
		{
			var testStartTime = ZDateTime.UtcNow;
			SystemDataRegistry.Instance.WorkflowFieldChangeTriggerHWM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testStartTime.ToDateTime()); //Setting the registry watermark date and time to the test start time to ensure that the processing date and the now date are the same
			var now = DateTime.UtcNow;

			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.ExecuteNonQuery($@"
TRUNCATE TABLE StmChangeLog
DECLARE @9DaysAgo datetime = DATEADD(d, -9, @now);
DECLARE @10DaysAgo datetime = DATEADD(d, -10, @now);

INSERT dbo.StmChangeLog (SY_PK, SY_PostedTimeUtc, SY_Status, SY_RetryCount, SY_Changes, SY_ParentID, SY_ParentTableCode) VALUES
	(NEWID(), @9DaysAgo, 'QUE', 0, 'JK_MasterBillNum||MB123457', NEWID(), 'JK'),
	(NEWID(), @now, 'QUE', 0, 'JK_MasterBillNum||MB123456', NEWID(), 'JK'),
	(NEWID(), @10DaysAgo, 'QUE', 0, 'JS_HouseBill||MB123458', NEWID(), 'JS')
", command => command.AddParameter("now", System.Data.SqlDbType.DateTime, now));
			}

			using (var anotherConnection = Db.NewExtraConnectionToMainDb())
			using (anotherConnection.BeginTransactionWithManager())
			{
				anotherConnection.ExecuteNonQuery(@"select * from dbo.StmChangeLog with (UPDLOCK) WHERE SY_PostedTimeUtc=@date", c => c.AddParameter("date", System.Data.SqlDbType.DateTime, now.AddDays(-9)));
				var sqlRowCont = "SELECT COUNT(*) FROM dbo.StmChangeLog";

				AssertEquals("The table should have three entries", 3, Db.Connection.ExecuteScalar(sqlRowCont));

				var purger = new StmChangeLogPurger(Db.Connection);
				purger.Purge(CancellationToken.None);

				AssertEquals("One entry should be deleted", 2, Db.Connection.ExecuteScalar(sqlRowCont));
			}

			var loadFactory = new BusinessObjectFactory();
			var allLogs = loadFactory.Load<StmChangeLog>(new ZQuery());
			AssertEquals("One entry should be deleted after the transaction", 2, allLogs.Length);
		}

		public void TestPurgerWithInvalidWorkflowFieldChangeTriggerHWM()
		{
			using (SystemDataRegistry.Instance.WorkflowFieldChangeTriggerHWM.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DateTime.MinValue))
			{
				var purger = new StmChangeLogPurger(Db.Connection);
				purger.Purge(CancellationToken.None);
				AssertNoExceptionThrown(() => purger.Purge(CancellationToken.None));
			}
		}

		void CreateTestLogs(ZDateTime testStartTime)
		{
			SystemDataRegistry.Instance.WorkflowFieldChangeTriggerHWM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testStartTime.ToDateTime()); //Setting the watermark so log dates can be set before the watermark, below 7 days after the watermark and 7 days and after

			// 7 Days before the watermark
			var log1 = Factory.New<StmChangeLog>();
			var log2 = Factory.New<StmChangeLog>();
			var log3 = Factory.New<StmChangeLog>();
			var log4 = Factory.New<StmChangeLog>();
			var log5 = Factory.New<StmChangeLog>();
			var log6 = Factory.New<StmChangeLog>();
			Factory.Save();

			log1.SY_PostedTimeUtc = testStartTime.AddDays(+7);

			// 2 Days before the watermark
			log2.SY_PostedTimeUtc = testStartTime.AddDays(+2);

			// On watermark date
			log3.SY_PostedTimeUtc = testStartTime;

			// 2 day after watermark
			log4.SY_PostedTimeUtc = testStartTime.AddDays(-2);

			// 7 days after watermark
			log5.SY_PostedTimeUtc = testStartTime.AddDays(-7);

			// 30 days after the watermark
			log6.SY_PostedTimeUtc = testStartTime.AddDays(-30);

			Factory.Save();
		}
	}
}
