using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicenceBillingExcludeOrgUpdater : NonPersistentBusinessObject, IObsoleteValidation
	{
		public ClientLicenceBillingExcludeOrgUpdater(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ClientLicenceBillingExcludeOrgCollection ExcludeOrgs
		{
			get
			{
				if (excludeOrgs == null)
				{
					excludeOrgs = new ClientLicenceBillingExcludeOrgCollection(Factory);
					RegisterEditableChildObject(excludeOrgs);
				}
				return excludeOrgs;
			}
		}
		ClientLicenceBillingExcludeOrgCollection excludeOrgs;
	}
}

