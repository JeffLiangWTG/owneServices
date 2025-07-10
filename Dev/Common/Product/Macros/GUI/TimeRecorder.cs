using System;
using System.Diagnostics;
using System.Reactive.Disposables;

namespace CargoWise.Macros.GUI
{
	sealed class TimeRecorder
	{
		public TimeRecorder()
		{
			this.stopwatch = new Stopwatch();
			stopwatch.Start();
		}

		readonly Stopwatch stopwatch;

		public TimeSpan Elapsed => stopwatch.Elapsed;

		public TimeSpan TotalRecorded => totalRecorded;
		TimeSpan totalRecorded = TimeSpan.Zero;

		public IDisposable Record()
		{
			stopwatch.Restart();
			return Disposable.Create(() =>
			{
				stopwatch.Stop();
				totalRecorded += stopwatch.Elapsed;
			});
		}
	}
}
