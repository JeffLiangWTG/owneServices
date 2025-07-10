using System;

namespace ServiceManager.Runner.Abstractions
{
	public interface IScheduledRunCommandInfo : IRunCommandInfo
	{
		DateTime ExpectedNextRunTime { get; }
		DateTime NextRunTime { get; }
	}
}
