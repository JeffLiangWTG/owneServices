using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicenceBillingCollectionNonDependent : ActiveBusinessObjectCollection<ClientLicenceBilling>
	{
		public ClientLicenceBillingCollectionNonDependent(BusinessObjectFactory factory)
			: base(factory, new AdhocCollectionRelationship(typeof(ClientLicenceBilling)))
		{
		}
	}
}


