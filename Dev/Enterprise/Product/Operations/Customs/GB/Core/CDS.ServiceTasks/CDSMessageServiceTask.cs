using Enterprise.BatchProcessor;
using Enterprise.Integration;

namespace Enterprise.Customs.GB.CDS.ServiceTasks
{
	public abstract class CDSMessageServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		public const string MessageServiceTaskCategory = CDSServiceTaskConstants.CDSMessageServiceTaskCategory;

		protected LoggingInformation GetNewLogger()
		{
			var result = new LoggingInformation();
			result.OnLogInfoAdded += Logger_OnLogInfoAdded;
			return result;
		}

		void Logger_OnLogInfoAdded(string log, LogType logType)
		{
			ServiceLogger.Log(logType, log.Trim());
		}
	}
}
