using System;
using Enterprise.Integration;

namespace Enterprise.Client.EDI.ServiceTasks.Testing
{
	public class ServiceLoggerForExceptionTest : ILogger
	{
		public void Log(LogType type, string message)
		{
			if (type == LogType.Information)
			{
				throw new InvalidOperationException();
			}
		}

		public void Log(LogType type, string message, Exception ex)
		{
		}
	}
}
