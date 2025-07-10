using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicenceFeeCollectionNonDependent : ActiveBusinessObjectCollection<ClientLicenceFee>
	{
		public ClientLicenceFeeCollectionNonDependent(BusinessObjectFactory factory)
			: base(factory, new AdhocCollectionRelationship(typeof(ClientLicenceFee)))
		{
		}
	}
}


