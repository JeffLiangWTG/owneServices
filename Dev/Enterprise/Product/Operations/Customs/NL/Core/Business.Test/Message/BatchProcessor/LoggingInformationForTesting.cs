using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;

namespace Enterprise.Customs.NL.Business.Testing;

internal class LoggingInformationForTesting : LoggingInformation
{
	public LoggingInformationForTesting() : base()
	{
		LogMessages = new ZStringBuilder();
		this.OnLogInfoAdded += new LogInfoAdded((string log, LogType logType) => LogMessages.AppendLine($"{logType.ToString()}: {log}"));
	}

	public override void ClearLogs()
	{
		base.ClearLogs();
		LogMessages = new ZStringBuilder();
	}

	internal ZStringBuilder LogMessages;
}
