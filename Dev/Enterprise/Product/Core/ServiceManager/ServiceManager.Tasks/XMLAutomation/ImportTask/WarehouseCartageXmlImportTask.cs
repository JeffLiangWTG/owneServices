using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Registry.Business;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	class WarehouseCartageXmlImportTask : XmlImportTask
	{
		public WarehouseCartageXmlImportTask(INotifications notify)
			: base(SystemDataRegistry.Instance.WarehouseCartageDataImportDirectory, notify, NotificationDataRegistry.Instance.WarehouseImportNotificationGroup, BillingInterfaceName.WarehouseCartageXmlImport)
		{
		}

		public override ZString TaskDescription
		{
			get { return Res.GetString("892b0272-a2c5-4d5d-99c0-a9b41c255d0b", "Warehouse Port Transport XML Import"); }
		}

		protected override DataImporter NewImporter()
		{
			return new WarehouseCartageDataImporter();
		}
	}
}
