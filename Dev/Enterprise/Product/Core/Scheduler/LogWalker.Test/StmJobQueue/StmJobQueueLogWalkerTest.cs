using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.LogWalker.Test
{
	sealed class StmJobQueueLogWalkerTest : LogWalkerTestCase
	{
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestEventTimeUtc()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00);

			var newsPublisher = new MockNewsPublisher();
			MockSubscriber subscriber1 = null;
			subscriber1 = new MockSubscriber(logs =>
			{
				AssertEquals(1, logs.Length);
				var faked = MockNewsTransmitter.CreateLogQueueItemForTest(log.Factory, log, subscriber1);
				var queued = logs.Single();

				Assert("queued.SJ_EventTimeUtc is not populated", queued.SJ_EventTimeUtc.IsValid && !queued.SJ_EventTimeUtc.IsEmpty);
				AssertEquals("queued.SJ_EventTimeUtc does not have the correct value", queued.SJ_EventTime.ToDateTime().ToUniversalTime(), queued.SJ_EventTimeUtc.ToDateTime());
				AssertEquals("fakes.SJ_EventTimeUtc does not have the correct value", queued.SJ_EventTimeUtc, faked.SJ_EventTimeUtc);
			}, name: "AllCustomEvents");

			Factory.Save();
			var transmitter = new MockNewsTransmitter(subscriber1, new[] { subscriber1 }, new SubscriberParameters { Logger = Logger });
			AssertEquals(1, newsPublisher.PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriber1 }));
			transmitter.ProcessLogsFromDb(1);
			AssertLoggerHasNoErrors();
			AssertContains("processing 1 logged event(s).", string.Join(System.Environment.NewLine, Logger.LogEntries));
			ErrorReporter.Clear();
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestEventTimeUtcForWTE()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "test trigger";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;

			Factory.Save();

			var log = dummy.Logs.AddNew(Events.CustomisableEvent00, DateTime.Now.AddHours(-3));

			var newsPublisher = new MockNewsPublisher();
			MockSubscriber subscriber1 = null;
			subscriber1 = new MockSubscriber(logs =>
			{
				AssertEquals(1, logs.Length);
				var queued = logs.Single();

				Assert("queued.SJ_EventTimeUtc is not populated", queued.SJ_EventTimeUtc.IsValid && !queued.SJ_EventTimeUtc.IsEmpty);
				AssertEquals("queued.SJ_EventTimeUtc does not have the correct value", queued.SJ_EventTime.ToDateTime().ToUniversalTime(), queued.SJ_EventTimeUtc.ToDateTime());
			}, name: "AllCustomEvents");

			Factory.Save();

			var transmitter = new MockNewsTransmitter(subscriber1, new[] { subscriber1 }, new SubscriberParameters { Logger = Logger });
			AssertEquals(5, newsPublisher.PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriber1 }));
			transmitter.ProcessLogsFromDb(1);
			AssertLoggerHasNoErrors();
			AssertContains("processing 1 logged event(s).", string.Join(System.Environment.NewLine, Logger.LogEntries));

			var jobQueueLogs = Factory.Load<StmJobQueue>(new ZQuery());
			AssertEquals(2, jobQueueLogs.Length);
			var logZ00 = jobQueueLogs.Single(j => j.SJ_SE_NKEvent == Events.CustomisableEvent00.Code);
			var logWTE = jobQueueLogs.Single(j => j.SJ_SE_NKEvent == Events.WorkflowTriggerEvent.Code);
			AssertEquals(logZ00.SJ_EventTime.ToDateTime().ToUniversalTime(), logZ00.SJ_EventTimeUtc.ToDateTime());
			AssertEquals(logWTE.SJ_EventTime.ToDateTime().ToUniversalTime(), logWTE.SJ_EventTimeUtc.ToDateTime());
			AssertEquals(logZ00.SJ_EventTimeUtc, logWTE.SJ_EventTimeUtc);
		}
	}
}
