using System;
using System.Diagnostics;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Environment;

namespace Enterprise.ArchiveManager.Business
{
	public class ArchiveStageStopwatch : IArchiveStageStopwatch
	{
		readonly Stopwatch stopwatch = new();

		public ZDateTime? StartAt { get; private set; }

		public ZDateTime? EndAt { get; private set; }

		public TimeSpan Elapsed
			=> stopwatch.Elapsed;

		public void Stop()
		{
			EndAt = Env.Time.CurrentLocalDateTime;

			stopwatch.Stop();
		}

		public void Restart()
		{
			StartAt = Env.Time.CurrentLocalDateTime;
			EndAt = null;

			stopwatch.Restart();
		}
	}
}
