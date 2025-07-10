using System;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DummyLogger : ILogger
	{
		public DummyLogger(Action<LogMessageType, string> logHandler)
		{
			this.logHandler = logHandler;
		}

		readonly Action<LogMessageType, string> logHandler;

		public void Log(LogMessageType messageType, params object[] parameters)
		{
			if (logHandler == null)
			{
				return;
			}

			var paramsAsStrings = string.Join(", ", parameters);
			logHandler(messageType, paramsAsStrings);
		}
	}
}
