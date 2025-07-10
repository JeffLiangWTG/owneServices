using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	class ShipmentXmlImportTask : XmlImportTask
	{
		public ShipmentXmlImportTask(INotifications notify)
			: this(SystemDataRegistry.Instance.ShipmentDataImportDirectory, notify, NotificationDataRegistry.Instance.ConsolShipmentImportNotificationGroup)
		{
		}

		public ShipmentXmlImportTask(StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup)
			: base(registryPath, notify, notificationGroup, BillingInterfaceName.ShipmentXmlImport)
		{
		}

		public override ZString TaskDescription
		{
			get { return Res.GetString("1b91ff4c-2079-4d9d-80d6-f35c5acb1853", "Shipment XML Import"); }
		}

		protected override DataImporter NewImporter()
		{
			return new ShipmentDataImporter();
		}
	}
}
