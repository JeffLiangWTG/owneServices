using Enterprise.Environment;
using NLog;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.ServiceHostClient.DataContracts;
using ServiceManager.Logging.Abstractions;

namespace ServiceManager.Logging.CW
{
	class QueueMonitorLogger : IQueueMonitorLogger
	{
		public QueueMonitorLogger(IHostRegistrySettings hostRegistry)
		{
			Initialize(hostRegistry);

			logger = LogManager.GetLogger(NLogQueueMonitorTargetFactory.TargetName);
		}

		static void Initialize(IHostRegistrySettings hostRegistry)
		{
			LoggerHelper.ConfigureTargetAndRule(new NLogQueueMonitorTargetFactory(hostRegistry), NLogQueueMonitorTargetFactory.TargetName, LogLevel.Info);

			LogManager.ReconfigExistingLoggers();
		}

		public void Log(QueueDTO queue)
		{
			const string queuePropertyPrefix = $"wisecloud.servicetask.queue";

			var logEventInfo = new LogEventInfo
			{
				TimeStamp = Env.Time.CurrentUtcDateTime,
				Level = LogLevel.Info,
				Message = queue.IsActive
					? $"{queue.ServiceTaskCode}|{queue.QueueName}|Running {queue.RunningCount}/{queue.ItemCount} (oldest:{queue.MaximumItemAgeInSeconds}) with {queue.ErrorCountLast24Hours} errors"
					: $"{queue.ServiceTaskCode}|{queue.QueueName}|Inactive",
				Properties =
				{
					[$"{queuePropertyPrefix}.QueueName"] = queue.QueueName,
					[$"{queuePropertyPrefix}.RunningCount"] = queue.RunningCount,
					[$"{queuePropertyPrefix}.ErrorCountLast24Hours"] = queue.ErrorCountLast24Hours,
					[$"{queuePropertyPrefix}.ServiceTaskCode"] = queue.ServiceTaskCode,
					[$"{queuePropertyPrefix}.IsActive"] = queue.IsActive,
					[$"{queuePropertyPrefix}.ItemCount"] = queue.ItemCount,
					[$"{queuePropertyPrefix}.MaximumItemAgeInSeconds"] = queue.MaximumItemAgeInSeconds,
				},
			};

			logger.Log(logEventInfo);
		}

		readonly NLog.ILogger logger;
	}
}
