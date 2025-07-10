using System;
using System.Diagnostics;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LogWalker.Test
{
	sealed class NewsBroadcasterTest : LogWalkerTestCase
	{
		public void TestNewsBroadcasterExitsWhenNoMessagesAreFoundThatCanBeProcessedWhenAnotherServiceTaskHasTheMessagesLocked()
		{
			using (var extraConnection = Db.NewExtraConnectionToMainDb())
			{
				var logBroadcaster = new NewsBroadcasterWithMockCallForTesting();
				logBroadcaster.ThrowErrorOnSecondProcess = true;

				LoggerForTesting testNotifier = new LoggerForTesting();

				StmJobQueue log1 = MockEventSubscriber.QueueNewLogForTestSubscriber(Factory);
				Factory.Save();

				var key = "LogSubscriber:" + log1.SJ_FilterName + "," + (log1.SJ_ProcessTaskParentID.IsValid ? log1.SJ_ProcessTaskParentID : log1.SJ_ParentID).ToString().ToLowerInvariant();
				SqlApplicationLock mutex;
				Assert("Precondition: A lock was acquired", extraConnection.TryGetLock(key, out mutex));

				try
				{
					var sw = Stopwatch.StartNew();
					logBroadcaster.ProcessLogsAndReturnIsComplete_ForMockSubscriber(testNotifier);
					AssertEquals("Subscriber should only be called once as item is locked.", 1, logBroadcaster.CalledSubscribers.Count);
					Assert(sw.ElapsedMilliseconds < 10000);
				}
				finally
				{
					mutex.Dispose();
				}
			}
		}

		public void TestFutureEventsAreNotProcessed()
		{
			var logBroadcaster = new NewsBroadcasterWithMockCallForTesting();
			LoggerForTesting testNotifier = new LoggerForTesting();

			StmJobQueue log1 = MockEventSubscriber.QueueNewLogForTestSubscriber(Factory);
			log1.SJ_IsDelayFired = true;
			log1.SJ_EventTimeUtc = ZDateTime.UtcNow.AddHours(1);
			Factory.Save();

			logBroadcaster.CalledSubscribers.Clear();
			logBroadcaster.ProcessLogsAndReturnIsComplete_ForMockSubscriber(testNotifier);
			AssertEquals("QUE", log1.SJ_Status);

			log1.SJ_EventTimeUtc = ZDateTime.UtcNow.AddHours(-1);
			Factory.Save();

			logBroadcaster.ProcessLogsAndReturnIsComplete_ForMockSubscriber(testNotifier);
			log1.Reload();
			AssertEquals("PRS", log1.SJ_Status);
		}

		public void TestDoesNotFailIfSubscriberCalledToProcessTwice()
		{
			MockEventSubscriber subscriber = new MockEventSubscriber();
			LogSubscriber[] testSubscribers = new LogSubscriber[] { subscriber };
			NewsBroadcaster logBroadcaster = new NewsBroadcaster();
			LoggerForTesting testNotifier = new LoggerForTesting();
			testNotifier.AllowDebug = true;

			StmJobQueue log = MockEventSubscriber.QueueNewLogForTestSubscriber(Factory);
			Factory.Save();

			logBroadcaster.ProcessLogs(new SubscriberParameters() { Logger = testNotifier }, testSubscribers, CancellationToken.None);

			AssertEquals("Notification Count", 2, testNotifier.NotifiedEventList.Count);
			AssertEquals("Notification Text [0]", "[MockEventSubscriber] processing 1 logged event(s).", testNotifier.NotifiedEventList[0]);
			AssertEquals("Notification Text [1]", "[MockEventSubscriber] finished processing logs.", testNotifier.NotifiedEventList[1]);
			AssertEquals("Last Processed Queued Log PK", log.PK, subscriber.LastProcessedLog_Pk);

			log = MockEventSubscriber.QueueNewLogForTestSubscriber(Factory);
			Factory.Save();

			logBroadcaster.ProcessLogs(new SubscriberParameters() { Logger = testNotifier }, testSubscribers, CancellationToken.None);

			// MockSubscriber is called to process logs again and its running status updated in the subscribersRunning Dictionary
			AssertEquals("Notification Count", 4, testNotifier.NotifiedEventList.Count);
			AssertEquals("Notification Text [2]", "[MockEventSubscriber] processing 1 logged event(s).", testNotifier.NotifiedEventList[2]);
			AssertEquals("Notification Text [3]", "[MockEventSubscriber] finished processing logs.", testNotifier.NotifiedEventList[3]);
			AssertEquals("Last Processed Queued Log PK", log.PK, subscriber.LastProcessedLog_Pk);
		}

		public void TestAllDefaultLoggersAreSet()
		{
			MockEventSubscriber subscriber1 = new MockEventSubscriber();
			MockEventSubscriber subscriber2 = new MockEventSubscriber();
			MockEventSubscriber subscriber3 = new MockEventSubscriber();

			LogSubscriber[] subscribers = new LogSubscriber[] { subscriber1, subscriber2, subscriber3 };
			NewsBroadcaster logBroadcaster = new NewsBroadcaster();
			LoggerForTesting testNotifier = new LoggerForTesting();

			var evt = Factory.NewWithValidTestData<StmEvent>();

			// Subscriber1 = 11 logs
			CreateQueueLogs(subscriber1, evt, 11);
			// Subscriber2 = 9 log
			CreateQueueLogs(subscriber2, evt, 9);
			// Subscriber3 = 20 logs
			CreateQueueLogs(subscriber3, evt, 20);

			Factory.Save();

			subscriber1.ProcessLogQueueItemsCore = (logs) =>
			{
				foreach (var subscriber in subscribers)
				{
					AssertNotEquals(typeof(NullLogger), subscriber.DefaultLogger.GetType());
				}
			};

			logBroadcaster.ProcessLogs(new SubscriberParameters() { Logger = testNotifier }, subscribers, CancellationToken.None);
		}

		public void TestProcessOutstandingQueuedLogs_WithSubscriberMultiplex()
		{
			var evt = Factory.NewWithValidTestData<StmEvent>();
			MockEventSubscriber subscriber1 = new MockEventSubscriber("SUB1");
			MockEventSubscriber subscriber2 = new MockEventSubscriber("SUB2");
			MockEventSubscriber subscriber3 = new MockEventSubscriber("SUB3");

			// Subscriber1 = 11 logs
			CreateQueueLogs(subscriber1, evt, 11);
			// Subscriber2 = 9 log
			CreateQueueLogs(subscriber2, evt, 9);
			// Subscriber3 = 20 logs
			CreateQueueLogs(subscriber3, evt, 20);
			Factory.Save();

			LogSubscriber[] testSubscribers = new LogSubscriber[]
			{
				subscriber1,
				subscriber2,
				subscriber3,
			};

			LoggerForTesting testNotifier = new LoggerForTesting();
			testNotifier.AllowDebug = true;
			testNotifier.NotifiedEventList.Clear();
			var logBroadcaster = new NewsBroadcasterForMultiplexTesting();

			logBroadcaster.ProcessLogs(new SubscriberParameters() { Logger = testNotifier }, testSubscribers, CancellationToken.None);

			AssertEquals("Notified event count", 10, testNotifier.NotifiedEventList.Count);

			// Assert Notifications in multiplex sequence
			AssertEquals("Notified - SUB1 1st batch start ", "[SUB1] processing 10 logged event(s).", testNotifier.NotifiedEventList[0]);
			AssertEquals("Notified - SUB1 1st batch finish", "[SUB1] finished processing logs.", testNotifier.NotifiedEventList[1]);
			AssertEquals("Notified - SUB2 1st batch start ", "[SUB2] processing 9 logged event(s).", testNotifier.NotifiedEventList[2]);
			AssertEquals("Notified - SUB2 1st batch finish", "[SUB2] finished processing logs.", testNotifier.NotifiedEventList[3]);
			AssertEquals("Notified - SUB3 1st batch start ", "[SUB3] processing 10 logged event(s).", testNotifier.NotifiedEventList[4]);
			AssertEquals("Notified - SUB3 1st batch finish", "[SUB3] finished processing logs.", testNotifier.NotifiedEventList[5]);

			AssertEquals("Notified - SUB1 2nd batch start ", "[SUB1] processing 1 logged event(s).", testNotifier.NotifiedEventList[6]);
			AssertEquals("Notified - SUB1 2nd batch finish", "[SUB1] finished processing logs.", testNotifier.NotifiedEventList[7]);
			AssertEquals("Notified - SUB3 2nd batch start ", "[SUB3] processing 10 logged event(s).", testNotifier.NotifiedEventList[8]);
			AssertEquals("Notified - SUB3 2nd batch finish", "[SUB3] finished processing logs.", testNotifier.NotifiedEventList[9]);
		}

		void CreateQueueLogs(MockEventSubscriber subscriber, BusinessObject parent, int logCount)
		{
			for (int i = 0; i < logCount; i++)
			{
				subscriber.QueueNewLog(Factory, parent);
			}
		}

		public void TestSubscriberYield()
		{
			var d1 = Factory.New<IDummyWithWorkflow>();
			var d2 = Factory.New<IDummyWithWorkflow>();
			var d3 = Factory.New<IDummyWithWorkflow>();
			((BusinessObject)d1).GetLogs().AddNew(Events.CustomisableEvent00);
			((BusinessObject)d2).GetLogs().AddNew(Events.CustomisableEvent00);
			((BusinessObject)d3).GetLogs().AddNew(Events.CustomisableEvent00);

			Factory.Save();

			SystemDataRegistry.Instance.SecondsUntilLogWalkerStopsAllocatingLogBatchesToASubscriber.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			SystemDataRegistry.Instance.LogWalkerBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			var sleepySubscriber = new MockSubscriber(logs =>
			{
				Thread.Sleep(1001); // I know I know! Sleeping is bad. In this case I am testing that a timer works correctly.
				AssertEquals("Preassert: Explode if logs batch is wrong.", 1, logs.Length);
				logs.ForEach(l => l.Factory.Load<IDummyWithWorkflow>(l.SJ_ParentID).Z0_Description = "PONG");
			},
			new[] { Events.CustomisableEvent00Code },
			new[] { DummyBizoSchema.Constants.TableName },
			"SleepySubscriber");
			ObjectFactory.Substitute("SystemLogSubscribers", new ILogSubscriber[] { sleepySubscriber });
			LogWalkerRunner.Master().Process(Logger, CancellationToken.None);
			LogWalkerRunner.Default().Process(Logger, CancellationToken.None);
			LogWalkerRunner.Purge().Process(Logger, CancellationToken.None);

			var dummies = Factory.Load<IDummyWithWorkflow>(new ZQuery(DummyBizoSchema.Z0_Description, "PONG") { ReLoadExistingRows = true });
			AssertEquals(3, dummies.Length);
			AssertContains("Subscriber SleepySubscriber yields after running for more than 1 seconds", string.Join("\r\n", Logger.LogEntries));
		}

		public void TestCancellationToken()
		{
			var dummy = (BusinessObject)Factory.New<IDummyWithWorkflow>();
			dummy.GetLogs().AddNew(Events.CustomisableEvent00);
			dummy.GetLogs().AddNew(Events.CustomisableEvent01);

			Factory.Save();

			var tokenSource = new CancellationTokenSource();
			var token = tokenSource.Token;

			ProcessLogs processLogs = logs => tokenSource.Cancel();
			ProcessLogs processLogs1 = logs => Logger.Log(Integration.LogType.Information, "Token cancelled so this should not be processed");

			var bestSubscriber = new MockSubscriber(processLogs, new[] { Events.CustomisableEvent00Code }, new[] { DummyBizoSchema.Constants.TableName }, "BestSubscriber");
			var idiotSubscriber = new MockSubscriber(processLogs1, new[] { Events.CustomisableEvent01Code }, new[] { DummyBizoSchema.Constants.TableName }, "IdiotSubscriber");

			ObjectFactory.Substitute("SystemLogSubscribers", new ILogSubscriber[] { bestSubscriber, idiotSubscriber });

			LogWalkerRunner.Master().Process(Logger, CancellationToken.None);
			LogWalkerRunner.Default().Process(Logger, token);
			LogWalkerRunner.Purge().Process(Logger, CancellationToken.None);

			AssertNotContains("Token cancelled so this should not be processed", string.Join("\r\n", Logger.LogEntries));
		}

		public void TestCancelQueueLogs()
		{
			var dummy = (BusinessObject)Factory.New<IDummyWithWorkflow>();
			dummy.GetLogs().AddNew(Events.CustomisableEvent00);
			dummy.GetLogs().AddNew(Events.CustomisableEvent01);

			Factory.Save();

			var tokenSource = new CancellationTokenSource();
			tokenSource.Cancel();
			var token = tokenSource.Token;
			var queueLogsTest = new QueueLogs();

			ProcessLogs processLogs = logs => tokenSource.Cancel(); //Cancellation actually occurs above
			ProcessLogs processLogs1 = logs => Logger.Log(Integration.LogType.Information, "Token cancelled so this should not be processed");

			var bestSubscriber = new MockSubscriber(processLogs, new[] { Events.CustomisableEvent00Code }, new[] { DummyBizoSchema.Constants.TableName }, "BestSubscriber");
			var idiotSubscriber = new MockSubscriber(processLogs1, new[] { Events.CustomisableEvent01Code }, new[] { DummyBizoSchema.Constants.TableName }, "IdiotSubscriber");

			AssertExceptionThrown<OperationCanceledException>(() => queueLogsTest.Execute(new SubscriberParameters { Logger = Logger }, new LogSubscriber[] { bestSubscriber, idiotSubscriber }, token));
		}

		public void TestCancelNewsTransmitter()
		{
			var dummy = (BusinessObject)Factory.New<IDummyWithWorkflow>(); //Populates stmalog queue as if triggers had actually fired
			dummy.GetLogs().AddNew(Events.CustomisableEvent00); //Dummy is what logs hang off of e.g. a work item or shipment
			dummy.GetLogs().AddNew(Events.CustomisableEvent01);

			Factory.Save();
			var tokenSource = new CancellationTokenSource();
			tokenSource.Cancel();
			var token = tokenSource.Token;

			ProcessLogs processLogs = logs => tokenSource.Cancel(); //Cancellation actually occurs above
			ProcessLogs processLogs1 = logs => Logger.Log(Integration.LogType.Information, "Token cancelled so this should not be processed");

			var bestSubscriber = new MockSubscriber(processLogs, new[] { Events.CustomisableEvent00Code }, new[] { DummyBizoSchema.Constants.TableName }, "BestSubscriber");
			var idiotSubscriber = new MockSubscriber(processLogs1, new[] { Events.CustomisableEvent01Code }, new[] { DummyBizoSchema.Constants.TableName }, "IdiotSubscriber");

			var transmitter = new MockNewsTransmitter(bestSubscriber, new LogSubscriber[] { bestSubscriber, idiotSubscriber }, new SubscriberParameters { Logger = Logger });
			LogWalkerRunner.Master().Process(Logger, CancellationToken.None); //Moves logs to default queue for when transmitter is called
			AssertExceptionThrown<OperationCanceledException>(() => transmitter.ProcessLogsFromDb(1, token));
		}

		public void TestWTELogReferenceIsNotTruncated()
		{
			var dummy = (DummyWithWorkflow)Factory.New<IDummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			var wteLog = trigger.Logs.AddNew(Events.WorkflowTriggerEvent);
			using (wteLog.LockForUpdatingKeyFieldsForTesting())
			{
				wteLog.SL_Reference = new String('A', StmALogSchema.SL_Reference.MaxLength);
			}

			Factory.Save();
			LogWalkerRunner.Master().Process(Logger, CancellationToken.None);

			var stmJobQueueReference = Db.Connection.ExecuteScalar<string>("SELECT TOP 1 SJ_Reference FROM dbo.StmJobQueue WHERE SJ_SE_NKEvent = 'WTE'");
			AssertEquals(wteLog.SL_Reference, stmJobQueueReference);
		}
	}
}
