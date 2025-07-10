using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Registry.Business;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	class WarehouseIFSXmlImportTask : XmlImportTask
	{
		public WarehouseIFSXmlImportTask(INotifications notify)
			: base(SystemDataRegistry.Instance.WarehouseIFSDataImportDirectory, notify, NotificationDataRegistry.Instance.WarehouseImportNotificationGroup, BillingInterfaceName.WarehouseIFSXmlImport)
		{
		}

		public override ZString TaskDescription
		{
			get { return Res.GetString("4db58d61-4785-4e61-9e81-1efb631f276c", "Warehouse IFS XML Import"); }
		}

		protected override DataImporter NewImporter()
		{
			return new WarehouseIFSDataImporter();
		}
	}
}
