using System;
using Enterprise.eHubMessaging.Business;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	public interface IeHubServiceTaskSupport
	{
		string ServiceTaskName { get; }
		string DefaultServerAddress { get; }
		ICompanySettingsManager CompanySettingsManager { get; }
		void ReportKnownException(Exception ex, string message = null);
	}
}
