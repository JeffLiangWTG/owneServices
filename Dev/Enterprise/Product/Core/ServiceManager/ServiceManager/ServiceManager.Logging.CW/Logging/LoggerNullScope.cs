using System;

namespace ServiceManager.Logging.CW
{
	sealed class LoggerNullScope : IDisposable
	{
		internal static LoggerNullScope Instance { get; } = new LoggerNullScope();

		LoggerNullScope()
		{
		}

		public void Dispose()
		{
		}
	}
}
