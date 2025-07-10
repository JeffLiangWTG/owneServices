using System;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class FixedDurationStopwatchTest : IStopwatch
	{
		public FixedDurationStopwatchTest(TimeSpan duration)
		{
			this.duration = duration;
		}

		public bool IsRunning { get; private set; }

		public long ElapsedMilliseconds => (long)duration.TotalMilliseconds;

		public TimeSpan Elapsed => duration;

		public void Restart()
		{
			IsRunning = true;
		}

		public void Start()
		{
			IsRunning = true;
		}

		public void Stop()
		{
			IsRunning = false;
		}

		readonly TimeSpan duration;
	}
}
