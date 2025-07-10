using System;
using System.Collections.Generic;
using CargoWise.Application;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Integration.CW
{
	// singleton utility class
	class NudgingEventsTracker : INudgingEventsTracker
	{
		NudgingEventsTracker() : this(ObjectFactory.Get<INudgingController>())
		{
		}

		protected NudgingEventsTracker(INudgingController nudgingController)
		{
			nudgingController.NudgeFailedEvent += new EventHandler<NudgeFailedEventArgs>((o, t) => AddNudgeEvent(t));
			nudgingController.NudgeTrackingEvent += new EventHandler<NudgeEventArgs>((o, t) => AddNudgeEvent(t));
		}

		public IEnumerable<NudgeEventWithTime> Events
		{
			get
			{
				lock (nudgeEventsLockObj)
				{
					return nudgeEvents.ToArray();
				}
			}
		}

		void AddNudgeEvent(NudgeEventArgs e)
		{
			lock (nudgeEventsLockObj)
			{
				nudgeEvents.Enqueue(new NudgeEventWithTime(e));

				if (nudgeEvents.Count > 1000)
				{
					nudgeEvents.Dequeue();
				}
			}
		}

		readonly Queue<NudgeEventWithTime> nudgeEvents = new Queue<NudgeEventWithTime>();
		readonly object nudgeEventsLockObj = new object();
	}
}
