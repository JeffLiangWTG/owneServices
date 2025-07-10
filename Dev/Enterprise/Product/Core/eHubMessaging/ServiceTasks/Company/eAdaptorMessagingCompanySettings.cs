using System;
using Enterprise.eHubMessaging.ServiceTasks.Company;
using Enterprise.Integration;
using Enterprise.Registry.Business.eServices;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	class eAdaptorMessagingCompanySettings : MessagingCompanySettings
	{
		public eAdaptorMessagingCompanySettings(Guid companyPK)
			: base(companyPK)
		{
		}

		protected override IRegistryItemInternals PasswordRegistryItem
		{
			get { return eAdaptorRegistry.Instance.eAdaptorOutboundPassword; }
		}

		protected override string GetPasswordCore()
		{
			return Password;
		}
	}
}
