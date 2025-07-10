using System;
using System.Diagnostics;
using CargoWise.Common;

namespace CargoWise.Integration
{
	public interface ILogSource
	{
		event Action<LogEventArgs> Log;
	}

	public class LogEventArgs : EventArgs
	{
		public LogEventArgs(TraceEventType eventType, string message)
		{
			Argument.NotNullOrEmpty(message, nameof(message));
			EventType = eventType;
			Message = message;
		}

		public TraceEventType EventType { get; private set; }
		public string Message { get; private set; }
	}
}
