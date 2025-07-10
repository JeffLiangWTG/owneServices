using System.Threading;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.LogWalker.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.LogWalker.Test
{
	class NewsBroadcasterTestWithoutTransaction : TestCase
	{
		[UseSnapshotProtection]
		public void TestNewsBroadcasterDetectsSubscriberDanglingTransactionAndRecyclesConnection()
		{
			var factory = new BusinessObjectFactory();
			var evt = factory.NewWithValidTestData<StmEvent>();
			MockEventSubscriber subscriber1 = new MockEventSubscriber("SUB1");
			MockEventSubscriber subscriber2 = new TransactionLeakingLogSubscriber("Leaking Subscriber");
			MockEventSubscriber subscriber3 = new MockEventSubscriber("SUB3");

			CreateQueueLogs(factory, subscriber1, evt, 10);
			CreateQueueLogs(factory, subscriber2, evt, 10);
			CreateQueueLogs(factory, subscriber3, evt, 10);
			factory.Save();

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
			AssertContains("[SUB1] processing 10 logged event(s)", testNotifier.ToString());
			AssertContains("[Leaking Subscriber] processing 10 logged event(s)", testNotifier.ToString());
			AssertContains("[SUB3] processing 10 logged event(s)", testNotifier.ToString());

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("LogWalker.SubscriberTransactionLeak", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		void CreateQueueLogs(BusinessObjectFactory factory, MockEventSubscriber subscriber, BusinessObject parent, int logCount)
		{
			for (int i = 0; i < logCount; i++)
			{
				subscriber.QueueNewLog(factory, parent);
			}
		}
	}
}
