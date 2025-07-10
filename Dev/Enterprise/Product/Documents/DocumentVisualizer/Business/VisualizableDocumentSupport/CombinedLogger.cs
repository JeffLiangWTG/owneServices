using System;
using System.Linq;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Business
{
	sealed class CombinedLogger : ILogger
	{
		public CombinedLogger(params ILogger[] loggers)
		{
			this.loggers = loggers?.Where(l => l != null).ToArray()
				?? Array.Empty<ILogger>();
		}

		readonly ILogger[] loggers;

		public void Log(LogMessageType messageType, params object[] parameters)
		{
			foreach (var logger in loggers)
			{
				logger.Log(messageType, parameters);
			}
		}
	}
}
