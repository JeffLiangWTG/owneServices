using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.CW;

namespace Enterprise.ServiceManager.HostsController.Test
{
	public class NudgingEventsTrackerTest : TestCase
	{
		public void TestEventsAreTracked()
		{
			var controller = new Mock<INudgingController>();
			var tracker = new NudgingEventsTrackerForTest(controller.Object);

			controller.Raise(c => c.NudgeTrackingEvent += null, new NudgeFailedEventArgs(new[] { "FAL" }, "Failed description", 0));
			controller.Raise(c => c.NudgeTrackingEvent += null, new NudgeSucceededEventArgs(new[] { "SUC" }));
			controller.Raise(c => c.NudgeTrackingEvent += null, new NudgeIgnoredEventArgs(new[] { "IGN" }, "Ignore description"));
			controller.Raise(c => c.NudgeTrackingEvent += null, new NudgeStartedEventArgs(new[] { "STA" }, null));

			AssertEquals("Incorrect number of events", 4, tracker.Events.Count());
			AssertNudgingEvent(tracker.Events, "succeeded", "SUC", string.Empty);
			AssertNudgingEvent(tracker.Events, "failed", "FAL", "Failed description");
			AssertNudgingEvent(tracker.Events, "ignored", "IGN", "Ignore description");
			AssertNudgingEvent(tracker.Events, "started", "STA", string.Empty);
		}

		void AssertNudgingEvent(IEnumerable<NudgeEventWithTime> events, string expectedResult, string expectedTask, string expectedDescription)
		{
			var eventsMatchingResult = events.Where(e => e.EventType.Equals(expectedResult));
			AssertEquals("Incorrect number of events matching EventType", 1, eventsMatchingResult.Count());
			var nudgeEvent = eventsMatchingResult.First();
			AssertEquals("Incorrect task", expectedTask, nudgeEvent.TaskCodes.First());
			AssertEquals("Incorrect description", expectedDescription, nudgeEvent.Description);
		}

		public void TestOnlyKeep1000Events()
		{
			var controller = new Mock<INudgingController>();
			var tracker = new NudgingEventsTrackerForTest(controller.Object);

			var i = 0;
			for (; i < 1000; i++)
			{
				controller.Raise(c => c.NudgeTrackingEvent += null, new NudgeIgnoredEventArgs(null, i.ToString()));
			}

			AssertEquals("Incorrect number of attempts", 1000, tracker.Events.Count());

			controller.Raise(c => c.NudgeTrackingEvent += null, new NudgeIgnoredEventArgs(null, i.ToString()));
			AssertEquals("Events not capped at 1000", 1000, tracker.Events.Count());
			AssertEquals("Oldest attempt not removed", expected: false, tracker.Events.Any(a => a.Description.Equals("0")));
			AssertEquals("Newest attempt not added", expected: true, tracker.Events.Any(a => a.Description.Equals("1000")));
		}
	}

	class NudgingEventsTrackerForTest : NudgingEventsTracker
	{
		public NudgingEventsTrackerForTest(INudgingController controller) : base(controller)
		{
		}
	}
}
