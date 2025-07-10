using System.Collections.Generic;
using System.Collections.Specialized;
using Enterprise.Integration;
using Enterprise.Integration.BatchProcessor;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.BatchProcessor
{
	public abstract class LoggingInformationBase : ILoggingInformation, ISimpleLogger
	{
		public StringCollection UserLogStrings = new StringCollection();
		public StringCollection DebugLogStrings = new StringCollection();

		public abstract IEnumerable<ISimpleLog> Logs { get; }

		public abstract void ClearLogs();
		public abstract void Log(string logMsg);
		public abstract void LogError(string logMsg);
		public abstract void AddBlankLine();
		public abstract void Log(LogType type, string message);
	}
}
