using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicenceBillingFlattenedCollection : NonPersistentBusinessObjectCollection<ClientLicenceBillingFlattened>, IImportWizardProvider
	{
		public ClientLicenceBillingFlattenedCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ClientLicenceBillingFlattened();
		}

		public ImportWizard GetImportWizard(IImportCollectionInfo collectionInfo, ISettingsStorage settingsStorage, IFileMapper fileMapper)
		{
			return new ClientLicenceBillingImportWizard(collectionInfo, settingsStorage, fileMapper);
		}
	}
}


