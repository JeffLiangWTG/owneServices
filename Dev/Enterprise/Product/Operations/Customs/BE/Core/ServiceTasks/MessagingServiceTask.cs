using Enterprise.BatchProcessor;
using Enterprise.Integration;

namespace Enterprise.Customs.BE.ServiceTasks;

public abstract class MessagingServiceTask : Customs.ServiceTasks.CustomsServiceTask
{
	public const string MessageServiceTaskCategory = "BEC";

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
