using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicenceBillingExcludeOrgCollection : ActiveBusinessObjectCollection<ClientLicenceBillingExcludeOrg>
	{
		public ClientLicenceBillingExcludeOrgCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}

