using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;

namespace Enterprise.Messaging.Business
{
	public class LogDiagnosticTicks : IDisposable
	{
		public LogDiagnosticTicks(string messageText, LoggingInformation logger)
		{
			this.messageText = messageText;
			this.logger = logger;
			startTime = ZDateTime.UtcNow;
			if (Env.Registry.EnableCustomsDiagnostics && logger != null)
			{
				logger.Log("Starting to " + messageText);
			}
		}
		readonly string messageText;
		readonly LoggingInformation logger;
		readonly ZDateTime startTime;

		public void Dispose()
		{
			if (Env.Registry.EnableCustomsDiagnostics && logger != null)
			{
				logger.Log(string.Format(messageText + " took {0} seconds to process.", (ZDateTime.UtcNow - startTime).TotalSeconds));
			}
		}
	}
}
