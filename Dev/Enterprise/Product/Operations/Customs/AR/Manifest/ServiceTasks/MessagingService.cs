using Enterprise.BatchProcessor;
using Enterprise.Integration;

namespace Enterprise.Customs.AR.Manifest.ServiceTasks
{
	public abstract class MessagingService : Customs.ServiceTasks.CustomsServiceTask
	{
		protected LoggingInformation GetNewLogger()
		{
			var result = new LoggingInformation();
			result.OnLogInfoAdded += new LoggingInformation.LogInfoAdded(Logger_OnLogInfoAdded);
			return result;
		}

		void Logger_OnLogInfoAdded(string log, LogType logType)
		{
			ServiceLogger.Log(logType, log.Trim());
		}
	}
}
