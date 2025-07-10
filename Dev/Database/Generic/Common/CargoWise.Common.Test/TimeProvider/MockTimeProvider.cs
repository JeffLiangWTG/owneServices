using System;
using System.Collections.Generic;

namespace CargoWise.Common
{
	public class MockTimeProvider : ITimeProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public MockTimeProvider()
			: this(DateTime.UtcNow, DateTime.Now)
		{ }

		public MockTimeProvider(DateTime startUtcTime, DateTime startLocalTime)
		{
			utcNow = startUtcTime;
			now = startLocalTime;
		}

		public DateTime GetCurrentLocalMachineDateTime() => now;

		public DateTime GetCurrentUtcDateTime() => utcNow;

		protected virtual MockTimerAdaptor CreateAdaptor()
		{
			return new MockTimerAdaptor(this);
		}

		public ITimerAdaptor GetTimer()
		{
			var timer = CreateAdaptor();
			timers.Add(timer);
			return timer;
		}

		public ITimerAdaptor GetTimer(double interval)
		{
			var timer = GetTimer();
			timer.Interval = interval;
			return timer;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public void Sleep(int milliseconds)
		{
			now = now.AddMilliseconds(milliseconds);
			utcNow = utcNow.AddMilliseconds(milliseconds);
			NotifyTimers();
		}

		public void Sleep(TimeSpan timeSpan)
		{
			Sleep((int)timeSpan.TotalMilliseconds);
		}

		DateTime now;
		DateTime utcNow;

		readonly List<MockTimerAdaptor> timers = new List<MockTimerAdaptor>();

		void NotifyTimers()
		{
			foreach (var timer in timers)
			{
				timer.CheckIfTimerElapsed();
			}
		}
	}
}
