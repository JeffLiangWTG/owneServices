using System;

namespace ServiceManager.Common.Abstractions;

public interface IErrorReporterProxy
{
	public void ReportOnce(string message, Exception? exception = null);

	public void ReportOnce(string key, string message);

	public void ReportDeveloperExceptionOnce(string message, Exception? exception = null);
}

