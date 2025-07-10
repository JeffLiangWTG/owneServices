using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	class OrderCsvImportTask : XmlImportTask
	{
		public OrderCsvImportTask(INotifications notify)
			: this(SystemDataRegistry.Instance.OrdersCSVDataImportDirectory, notify, NotificationDataRegistry.Instance.OrderImportNotificationGroup)
		{
		}

		public OrderCsvImportTask(StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup)
			: base(registryPath, notify, notificationGroup, BillingInterfaceName.OrderCsvImport)
		{
		}

		public override ZString TaskDescription
		{
			get { return Res.GetString("83cc8665-1022-44d5-a4bf-ccfcf9bf1e28", "Order CSV Import"); }
		}

		protected override ZString FileExtension
		{
			get { return "*.csv"; }
		}

		protected override DataImporter NewImporter()
		{
			return new CsvOrderDataImporter();
		}
	}
}
