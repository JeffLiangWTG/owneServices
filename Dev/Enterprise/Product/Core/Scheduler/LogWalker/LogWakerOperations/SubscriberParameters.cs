using System;
using System.Threading;
using CargoWise.Async.AsyncTaskContext.Public;
using Enterprise.Integration;

namespace Enterprise.LogWalker
{
	public sealed class SubscriberParameters
	{
		public ILogger Logger { get; set; }
		public Func<ITaskRunner> BackgroundWorkerFactory { get; set; } = () => throw new InvalidOperationException();
		public IStmJobQueueLogger StmJobQueueLogger { get; set; } = new NullStmJobQueueLogger();
		public IStmJobQueueLoggerConfig StmJobQueueLoggerConfig { get; set; } = new DefaultStmJobQueueLoggerConfig();
	}

	sealed class NullStmJobQueueLogger : IStmJobQueueLogger
	{
		public void GoForthAndLog()
		{
		}
	}

	sealed class DefaultStmJobQueueLoggerConfig : IStmJobQueueLoggerConfig
	{
		public TimeSpan LogInterval => Timeout.InfiniteTimeSpan;
		public TimeSpan LogAllowedLag => Timeout.InfiniteTimeSpan;
		public int MaxItems => 0;
	}
}