using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class OrganisationXmlImportTask : XmlImportTask
	{
		public OrganisationXmlImportTask(INotifications notify)
			: this(SystemDataRegistry.Instance.OrganisationDataImportDirectory, notify, NotificationDataRegistry.Instance.OrganisationImportNotificationGroup)
		{
		}

		public OrganisationXmlImportTask(StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup)
			: base(registryPath, notify, notificationGroup, BillingInterfaceName.OrganisationXmlImport)
		{
		}

		public override ZString TaskDescription
		{
			get { return Res.GetString("4deca0a4-bab0-41ca-808c-9ac3861e1004", "Organization XML Import"); }
		}

		protected override DataImporter NewImporter()
		{
			return new OrganisationXmlDataImporter(GetFactoryProvider(), new StandardManualAndBatchImportOrganisationValueObjectDataAdapter());
		}

		protected BusinessObjectFactoryProvider GetFactoryProvider()
		{
			BusinessObjectFactoryProvider provider = new BusinessObjectFactoryProvider();
			provider.Current.RefreshEnabled = false;
			return provider;
		}
	}
}
