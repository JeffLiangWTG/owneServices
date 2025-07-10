using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicenceFeeFlattenedCollection : NonPersistentBusinessObjectCollection<ClientLicenceFeeFlattened>
	{
		public ClientLicenceFeeFlattenedCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ClientLicenceFeeFlattened();
		}
	}
}


