using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	class ConsolXmlImportTask : XmlImportTask
	{
		public ConsolXmlImportTask(INotifications notify)
			: this(SystemDataRegistry.Instance.ConsolsDataImportDirectory, notify, NotificationDataRegistry.Instance.ConsolShipmentImportNotificationGroup)
		{
		}

		public ConsolXmlImportTask(StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup)
			: base(registryPath, notify, notificationGroup, BillingInterfaceName.ConsolXmlImport)
		{
		}

		public override ZString TaskDescription
		{
			get { return Res.GetString("44c85222-d384-43e5-ac74-a38872a93b8b", "Consol XML Import"); }
		}

		protected override DataImporter NewImporter()
		{
			return new XmlDataImporter(new BatchForwardingConsolValueObjectDataAdapter());
		}
	}
}
