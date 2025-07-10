using System;
using CargoWise.Common;
using Microsoft.Extensions.Logging;

namespace Enterprise.Licensing.Billing.Business
{
	public class ErrorReporterLogger<T> : ILogger<T>
	{
		public ErrorReporterLogger(string context)
		{
			this.context = context;
		}

		readonly string context;

		public IDisposable BeginScope<TState>(TState state)
		{
			throw new NotImplementedException();
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			throw new NotImplementedException();
		}

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			var message = formatter(state, exception);
			ErrorReporter.ReportOnce(context, message, exception);
		}
	}
}
