using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management.Management.SessionLogging
{
	public class TaskLogger : ISimpleLogger
	{
		public TaskLogger(ISimpleLogger taskLogger) {
			this.taskLogger = Argument.NotNull(taskLogger, "ILoggingInformation taskLogger");
		}

		readonly ISimpleLogger taskLogger;

		public IEnumerable<ISimpleLog> Logs => taskLogger.Logs;

		public virtual bool ShouldLog
		{
			get {
				bool shouldLog = false;
				#if DEBUG
					shouldLog = true;
				#endif
				return shouldLog;
			}
		}

		public void Log(LogType type, string message)
		{
			if (ShouldLog)
			{
				taskLogger.Log(type, message);
			}
		}
	}
}
