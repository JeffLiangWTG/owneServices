using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	class LogFileArchiveCleanerTask : IServiceManagerTask
	{
		public LogFileArchiveCleanerTask(ILogFileArchiveCleaner logFileArchiveCleaner, IHostLogger logger)
		{
			this.logFileArchiveCleaner = logFileArchiveCleaner ?? throw new ArgumentNullException(nameof(logFileArchiveCleaner));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public string Name { get; } = typeof(LogFileArchiveCleanerTask).FullName;
		public TimeSpan RunDelay { get; } = TimeSpan.FromHours(6);
		public TimeSpan ErrorDelay { get; } = TimeSpan.FromHours(1);

		public void Initialise(CancellationToken cancellationToken)
		{
		}

		public void Run(CancellationToken cancellationToken)
		{
			logger.Log(LogLevel.Debug, "Cleaning up space from old log files.");
			logFileArchiveCleaner.PerformCleaning();
			logger.Log(LogLevel.Debug, "Cleaning complete.");
		}

		readonly ILogFileArchiveCleaner logFileArchiveCleaner;
		readonly IHostLogger logger;
	}
}
