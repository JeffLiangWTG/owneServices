using System;
using Enterprise.Environment;
using Enterprise.Integration;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DbHealth.IndexUpdate.Testing
{
	[Serializable]
	sealed class MyTestLogger : TestServiceLogger, ILogger
	{
		void ILogger.Log(LogType type, string message)
		{
			var dateTime = Env.Time.CurrentUtcDateTime;
		}

		void ILogger.Log(LogType type, string message, Exception ex)
		{ }
	}
}
