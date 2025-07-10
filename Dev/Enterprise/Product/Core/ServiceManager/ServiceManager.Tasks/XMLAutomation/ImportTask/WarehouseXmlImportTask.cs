using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	class WarehouseXmlImportTask : XmlImportTask
	{
		public WarehouseXmlImportTask(INotifications notify)
			: this(SystemDataRegistry.Instance.WarehouseDataImportDirectory, notify, NotificationDataRegistry.Instance.WarehouseImportNotificationGroup)
		{
		}

		public WarehouseXmlImportTask(StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup)
			: base(registryPath, notify, notificationGroup, BillingInterfaceName.WarehouseXmlImport)
		{
		}

		public override ZString TaskDescription
		{
			get { return Res.GetString("024b1195-7015-4bcb-a096-f429f1ac9334", "Warehouse XML Import"); }
		}

		protected override DataImporter NewImporter()
		{
			return new WarehouseDocketDataImporter();
		}
	}
}
