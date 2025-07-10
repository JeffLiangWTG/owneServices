using System;
using System.Threading;
using CargoWise.Common;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Core
{
	public class TimeProvider : ITimeProvider
	{
		public DateTime GetCurrentLocalMachineDateTime() => DateTime.Now;  // Local time without querying the database server

		public DateTime GetCurrentUtcDateTime()
		{
			IEnvironment env = EnvProxy.Instance;
			return env.Time.CurrentUtcDateTime;
		}

		public ITimerAdaptor GetTimer()
		{
			return new TimerAdaptor();
		}

		public ITimerAdaptor GetTimer(double interval)
		{
			return new TimerAdaptor(interval);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public void Sleep(int milliseconds)
		{
			Thread.Sleep(milliseconds);
		}

		public void Sleep(TimeSpan timeSpan)
		{
			Thread.Sleep(timeSpan);
		}
	}
}
