using System;
using Enterprise.eHubMessaging.Business;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	class eAdaptorMessagingCompanySettingsManager : eHubMessagingCompanySettingsManager
	{
		protected override ICompanySettings CreateCompanySettings(Guid companyPK)
		{
			return new eAdaptorMessagingCompanySettings(companyPK);
		}
	}
}
