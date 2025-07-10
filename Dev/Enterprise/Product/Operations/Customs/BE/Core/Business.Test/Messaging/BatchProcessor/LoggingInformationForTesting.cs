using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.BE.Business.Testing;

class LoggingInformationForTesting : LoggingInformation
{
	public LoggingInformationForTesting()
	{
		LogMessages = new ZStringBuilder();
		OnLogInfoAdded += (log, logType) => LogMessages.AppendLine($"{logType.ToString()}: {log}");
	}

	public override void ClearLogs()
	{
		base.ClearLogs();
		LogMessages = new ZStringBuilder();
	}

	internal ZStringBuilder LogMessages;
}
