using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class BatchLogger : ILogger
	{
		public BatchLogger(ILogger logger, string batchName)
		{
			this.logger = logger;
			this.batchName = batchName;
		}

		readonly ILogger logger;
		readonly string batchName;

		public virtual IDisposable BatchLoadStarting()
		{
			var startingTime = ZDateTime.UtcNow;

			return new DisposableAction(() =>
			{
				var finishedTime = ZDateTime.UtcNow;
				totalMillis += (long)(finishedTime - startingTime).TotalMilliseconds;
				batchesProcessed++;
			});
		}

		static TimeSpan GetWarningThreshold() => TimeSpan.FromMilliseconds(BMSRegistry.Instance.PerformanceLogsWarningThreshold.Value);

		public void Flush()
		{
			var timespan = TimeSpan.FromMilliseconds(totalMillis);
			var logType = timespan > GetWarningThreshold() ? LogType.Warning : LogType.Debug;
			var message = string.Format(CultureInfo.InvariantCulture,
(NoResString)"Performance: {0}, Loading time: [{1}] Batches processed: [{2}]", // Service task logging
				/*0*/ batchName,
				/*1*/ timespan,
				/*2*/ batchesProcessed);

			logger?.Log(logType, message);

			batchesProcessed = 0;
			totalMillis = 0;
		}

		int batchesProcessed;
		long totalMillis;

		#region ILogger Members

		public void Log(LogType type, string message)
		{
			logger?.Log(type, message);
		}

		public void Log(LogType type, string message, Exception ex)
		{
			logger?.Log(type, message, ex);
		}

		#endregion
	}
}
