using System;
using CargoWise.Types;

namespace Enterprise.ArchiveManager.Integration
{
	public interface IArchiveStageStopwatch
	{
		ZDateTime? StartAt { get; }

		ZDateTime? EndAt { get; }

		TimeSpan Elapsed { get; }

		void Stop();

		void Restart();
	}
}
