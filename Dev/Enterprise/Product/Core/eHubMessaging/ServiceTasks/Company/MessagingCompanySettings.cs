using System;
using Enterprise.eHubMessaging.Business;
using Enterprise.Integration;

namespace Enterprise.eHubMessaging.ServiceTasks.Company
{
	internal abstract class MessagingCompanySettings : ICompanySettings
	{
		protected MessagingCompanySettings(Guid companyPK)
		{
			this.companyPK = companyPK;
		}

		public bool PasswordExists
		{
			get { return Password != null; }
		}

		public string GetPassword()
		{
			return GetPasswordCore();
		}

		internal string Password
		{
			get { return PasswordRegistryItem.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty) as string; }
		}

		protected abstract IRegistryItemInternals PasswordRegistryItem { get; }

		protected abstract string GetPasswordCore();

		readonly Guid companyPK;
	}
}