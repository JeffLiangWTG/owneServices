using System;
using System.Diagnostics;
using ServiceManager.Common.Abstractions;

namespace ServiceManager.Common
{
	public class StopwatchProxy : IStopwatch
	{
		public StopwatchProxy()
		{
			stopwatch = new Stopwatch();
		}

		public void Restart()
		{
			stopwatch.Restart();
		}

		public void Start()
		{
			stopwatch.Start();
		}

		public void Stop()
		{
			stopwatch.Stop();
		}

		public bool IsRunning => stopwatch.IsRunning;
		public long ElapsedMilliseconds => stopwatch.ElapsedMilliseconds;
		public TimeSpan Elapsed => stopwatch.Elapsed;

		readonly Stopwatch stopwatch;
	}
}
