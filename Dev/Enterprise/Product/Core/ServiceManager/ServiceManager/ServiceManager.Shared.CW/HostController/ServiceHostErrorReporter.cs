using System;
using CargoWise.Common;
using ServiceManager.Integration.ServiceHostClient.Abstractions;

namespace ServiceManager.Shared.CW
{
	class ServiceHostErrorReporter : IServiceHostErrorReporter
	{
		public void ReportException(string key, string message, Exception ex)
		{
			ErrorReporter.ReportOnce(key, message, ex);
		}
	}
}
