using System;

namespace Enterprise.RemotePrinting.Engine
{
	public class ReportErrorEventArgs : EventArgs
	{
		public string Key { get; }
		public string Message { get; }
		public Exception Exception { get; }

		public ReportErrorEventArgs(string key, string message, Exception exception)
		{
			Key = key;
			Message = message;
			Exception = exception;
		}
	}
}
