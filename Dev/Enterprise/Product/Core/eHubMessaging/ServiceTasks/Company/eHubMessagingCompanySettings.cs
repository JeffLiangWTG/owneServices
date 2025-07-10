using System;
using CargoWise.Application;
using Enterprise.eHubMessaging.Business;
using Enterprise.Integration.Licensing;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	class eHubMessagingCompanySettings : ICompanySettings
	{
		public eHubMessagingCompanySettings(Guid companyPK)
		{
			rego = ObjectFactory.Get<IProductRegistration>();
		}

		readonly IProductRegistration rego;

		public string GetPassword()
		{
			return rego.Key.Password;
		}

		public bool PasswordExists
		{
			get { return true; }
		}
	}
}