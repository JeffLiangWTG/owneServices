using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class LoggingInformationForTesting : LoggingInformation
	{
		public LoggingInformationForTesting() : base()
		{
			LogMessages = new ZStringBuilder();
			OnLogInfoAdded += new LogInfoAdded((string log, LogType logType) => LogMessages.AppendLine($"{logType.ToString()}: {log}"));
		}

		public override void ClearLogs()
		{
			base.ClearLogs();
			LogMessages = new ZStringBuilder();
		}

		internal ZStringBuilder LogMessages;
	}
}
