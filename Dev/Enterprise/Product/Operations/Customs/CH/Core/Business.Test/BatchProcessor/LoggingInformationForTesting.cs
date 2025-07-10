using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.CH.Business.Testing;

public class LoggingInformationForTesting : LoggingInformation
{
	public LoggingInformationForTesting() : base()
	{
		AccumulatedLogMessages = new ZStringBuilder();
		OnLogInfoAdded += (log, logType) => AccumulatedLogMessages.AppendLine($"{logType}: {log.Trim()}");
	}

	public ZStringBuilder AccumulatedLogMessages { get; private set; }

	public bool ContainsLogEntry(ZString partialLogEntry)
	{
		return AccumulatedLogMessages.ToString().Trim().Contains(partialLogEntry);
	}

	public override void ClearLogs()
	{
		base.ClearLogs();
		AccumulatedLogMessages.Clear();
	}
}
