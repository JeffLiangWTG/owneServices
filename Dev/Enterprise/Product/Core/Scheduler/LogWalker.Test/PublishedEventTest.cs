using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using static Enterprise.LogWalker.WorkflowEventsPublisher;

namespace Enterprise.LogWalker.Test
{
	[TestedType(typeof(PublishedEvent))]
	sealed class PublishedEventTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return PublishedEvent.Create(new QueuedLogForTesting(Factory));
		}

		public void TestPublishedEvent_PK()
		{
			var log = new QueuedLogForTesting(Factory);
			var publishedEvent = PublishedEvent.Create(log);
			AssertEquals(log.PK, publishedEvent.SL_PK);
			AssertEquals(log.PK, ((IIdentified)publishedEvent).Identifier);
		}

		public void TestPublishedEventHasEventTimeUtc()
		{
			var log = new QueuedLogForTesting(Factory)
			{
				SJ_EventTime = ZDateTime.Now,
				SJ_EventTimeUtc = ZDateTime.UtcNow
			};
			var publishedEvent = PublishedEvent.Create(log);
			AssertEquals(log.SJ_EventTimeUtc, publishedEvent.SL_EventTimeUtc);
		}
	}
}
