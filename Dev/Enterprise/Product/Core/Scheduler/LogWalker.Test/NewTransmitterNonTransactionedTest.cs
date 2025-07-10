using System;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.LogWalker.Testing
{
	[UseSnapshotProtection]
	sealed class NewTransmitterNonTransactionedTest : TestCase
	{
		public void TestAppLockLost()
		{
			var factory = new BusinessObjectFactory();
			var jobQueues = Enumerable.Range(0, 20).Select(i => MockSubscriber.QueueNewLogForTestSubscriber(factory)).ToList();
			var company1 = factory.NewWithValidTestData<GlbCompany>();
			var branch1 = factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;

			factory.Save();

			var logger = new LoggerForTest();
			var subscriber = new MockSubscriber();
			subscriber.ProcessLogQueueItemsCore = logs =>
			{
				logs[0].Factory.Load<GlbStaff>(Env.CurrentUserPK);
			};
			var newsTransmitter = new NewsTransmitterThatMimicsAppLockLost(subscriber, new[] { subscriber }, new SubscriberParameters() { Logger = logger }, branch1);

			AssertNoExceptionThrown("Oh no, we lost the connection, but we will still be fine", () => newsTransmitter.ProcessLogQueueBatch(10));
			AssertEquals("No exception reported", ErrorReporter.TotalErrorCount, 0);
		}

		public void TestConcurrentAppLock()
		{
			var subscriber = new MockSubscriber();
			var batchSize = 10;
			var groupSize = 10;
			var queueSize = 15;

			Db.Connection.ExecuteNonQuery($@"
TRUNCATE TABLE StmJobQueue

INSERT dbo.StmJobQueue WITH (TABLOCKX) (SJ_PK, SJ_Status, SJ_FilterName, SJ_PostedTimeUtc, SJ_EventTime, SJ_EventTimeUtc, SJ_ParentID, SJ_ALogReference, SJ_ParentTableCode)
SELECT
	SJ_PK              = NEWID()
	, SJ_Status        = 'QUE'
	, SJ_FilterName    = {subscriber.Name.QuoteName('\'')}
	, SJ_PostedTimeUtc = DATEADD(ms, 10 * (SNS_Number / 5), DATEADD(YEAR, -2, GETUTCDATE()))
	, SJ_EventTime     = DATEADD(ms, 10 * SNS_Number, DATEADD(YEAR, -2, GETDATE()))
	, SJ_EventTimeUtc  = DATEADD(ms, 10 * SNS_Number, DATEADD(YEAR, -2, GETUTCDATE()))
	, SJ_ParentID      = CONVERT(uniqueidentifier, CONVERT(binary(16), SNS_Number / {groupSize}))
	, SJ_ALogReference = 'AD831797-4403-40BD-9514-9F9A1195D849'
	, SJ_ParentTableCode = 'JS'
FROM
	dbo.StmNumberSequence
WHERE 1=1
	AND SNS_Number < {queueSize}

ALTER INDEX ALL ON StmJobQueue REBUILD

");

			int GetLogsQueued()
			{
				return Db.Connection.ExecuteScalar<int>($"SELECT COUNT(*) FROM dbo.StmJobQueue WHERE SJ_FilterName = {subscriber.Name.QuoteName('\'')} AND SJ_Status = 'QUE'");
			}

			AssertEquals("PRECONDITION: Logs queued", 15, GetLogsQueued());

			using (var blocker = Db.NewExtraConnectionToMainDb())
			{
				// simulates a runner having taken an applock that covers more rows than it will process
				AppLockExtensions.UserAction_ForTest.Value = () =>
				{
					blocker.ExecuteNonQuery($"EXEC sp_getapplock @Resource = 'LOGSUBSCRIBER:{subscriber.Name.ToUpperInvariant()},00000000-0000-0000-0000-000000000000', @LockMode = N'Exclusive', @LockOwner = N'Session';");
				};

				var logger = new LoggerForTest();
				new NewsTransmitter(subscriber, new[] { subscriber }, new SubscriberParameters() { Logger = logger })
					.ProcessLogQueueBatch(batchSize);
			}

			AssertEquals("Logs queued (10 - skipped due to AppLock, 5 - processed)", 10, GetLogsQueued());
		}

		#region Helper classes

		[Serializable]
		class NewsTransmitterThatMimicsAppLockLost : NewsTransmitter
		{
			[NonSerialized]
			readonly GlbBranch branch;

			public NewsTransmitterThatMimicsAppLockLost(LogSubscriber subscriber, LogSubscriber[] subscribers, SubscriberParameters subscriberParameters, GlbBranch branch) : base(subscriber, subscribers, subscriberParameters)
			{
				this.branch = branch;
			}

			internal override BatchResult ProcessAndSave(ProcessableLogGroupStack logsProcessingStack, CancellationToken token)
			{
				logsProcessingStack.Peek().Factory.NewWithValidTestData<ProcessTaskForNewsTransmitterThatMimicsAppLockLost>().BranchPK = branch.PK.ToGuid();
				return base.ProcessAndSave(logsProcessingStack, token);
			}
		}

		class ProcessTaskForNewsTransmitterThatMimicsAppLockLost : ProcessTask
		{
			public ProcessTaskForNewsTransmitterThatMimicsAppLockLost(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public Guid BranchPK { get; set; }

			protected override void OnFactorySaving()
			{
				var context = new UserContext(Env.CurrentUserPK, BranchPK, Env.CurrentDepartmentPK);
				AdoTestUtils.KillConnection(Db.Connection);
				Db.Connection.RollbackTransaction();
				using (Env.SetTemporaryUserContext(context))
				{
					base.OnFactorySaving();
				}
			}
		}

		#endregion // Helper classes
	}
}
