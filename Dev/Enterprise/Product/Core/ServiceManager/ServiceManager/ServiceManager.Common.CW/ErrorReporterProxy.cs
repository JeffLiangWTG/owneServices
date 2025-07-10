using System;
using CargoWise.Common;
using ServiceManager.Common.Abstractions;

namespace ServiceManager.Common.CW;

public class ErrorReporterProxy : IErrorReporterProxy
{
	public void ReportOnce(string message, Exception? exception = null)
	{
		ErrorReporter.ReportOnce(message, exception);
	}

	public void ReportOnce(string key, string message)
	{
		ErrorReporter.ReportOnce(key, message);
	}

	public void ReportDeveloperExceptionOnce(string message, Exception? exception = null)
	{
		ErrorReporter.ReportDeveloperExceptionOnce(message, exception);
	}
}
