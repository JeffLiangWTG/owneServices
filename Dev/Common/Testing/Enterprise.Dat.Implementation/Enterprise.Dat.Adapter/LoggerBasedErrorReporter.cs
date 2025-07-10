using System;
using CargoWise.Common;
using Dat.Integration;

namespace Enterprise.Dat.Implementation
{
	sealed class LoggerBasedErrorReporter : IErrorReporter
	{
		readonly ITaskLogger logger;

		public LoggerBasedErrorReporter(ITaskLogger logger)
		{
			this.logger = logger;
		}

		public void Clear()
		{
		}

		public void Report(string key, string message, Exception exception)
		{
			ReportCore(key, message, exception);
		}

		public void ReportDeveloperExceptionOrHandleSilently(string key, string message, Exception exception)
		{
			ReportCore($"{nameof(ReportDeveloperExceptionOrHandleSilently)}+{key}", message, exception);
		}

		void ReportCore(string key, string message, Exception exception)
		{
			logger.RecordInfo($@"A new error reported:
Key = {key},
Message = {message},
Exception = {exception}");
		}
	}
}
