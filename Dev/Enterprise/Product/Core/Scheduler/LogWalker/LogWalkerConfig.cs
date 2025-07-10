using System;
using Enterprise.Registry.Business;

namespace Enterprise.LogWalker
{
	class LogWalkerConfig : IStmJobQueueLoggerConfig
	{
		public static IStmJobQueueLoggerConfig Create()
		{
			var result = (IStmJobQueueLoggerConfig)new LogWalkerConfig();
			return result;
		}

		LogWalkerConfig()
		{
			LogInterval = new TimeSpan(
				0,
				SystemDataRegistry.Instance.LogWalkerStmJobQueueReportLogInterval.Value,
				0);

			LogAllowedLag = new TimeSpan(
				0,
				SystemDataRegistry.Instance.LogWalkerDelayLogTime.Value,
				0);

			MaxItems = SystemDataRegistry.Instance.LogWalkerStmJobQueueReportMaxRows.Value;
		}

		public TimeSpan LogInterval { get; }

		public TimeSpan LogAllowedLag { get; }

		public int MaxItems { get; }
	}
}
