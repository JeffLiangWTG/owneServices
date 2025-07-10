using System;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Scheduler.GraphEngine
{
	public interface IPreKeyBacklogChecker
	{
		bool IsBacklogTooBig { get; }
	}

	public interface IPreKeyBacklogCheckerSupporter
	{
		int PreKeyMaxBacklogSize { get; }
		int CountBacklog(string status, GrEngineEnums.Status includeOrExclude);
	}

	public class PreKeyBacklogChecker : IPreKeyBacklogChecker
	{
		internal PreKeyBacklogChecker(IPreKeyBacklogCheckerSupporter supporter)
		{
			this.supporter = Argument.NotNull(supporter, nameof(supporter));
			backlogCheckFrequency = TimeSpan.FromSeconds(20);
		}

		readonly IPreKeyBacklogCheckerSupporter supporter;
		readonly TimeSpan backlogCheckFrequency;
		ZDateTime lastBacklogCheck;
		int lastBacklog;

		public bool IsBacklogTooBig
		{
			get
			{
				var maxBacklogSize = supporter.PreKeyMaxBacklogSize;
				if (maxBacklogSize > 0)
				{
					var now = ZDateTime.UtcNow;
					if (lastBacklogCheck.IsEmpty || lastBacklogCheck.Add(backlogCheckFrequency) < now)
					{
						lastBacklog = GetBacklog();
						lastBacklogCheck = now;
					}

					return lastBacklog >= maxBacklogSize;
				}
				else
				{
					return false;
				}
			}
		}

		int GetBacklog()
		{
			if (supporter.CountBacklog(QueueStatusCodes.Codes.Queued, GrEngineEnums.Status.Include) == 0) // There is a chance that this could happen
			{
				return 0;
			}
			else
			{
				return supporter.CountBacklog(QueueStatusCodes.Codes.Processed, GrEngineEnums.Status.Exclude);
			}
		}
	}
}
