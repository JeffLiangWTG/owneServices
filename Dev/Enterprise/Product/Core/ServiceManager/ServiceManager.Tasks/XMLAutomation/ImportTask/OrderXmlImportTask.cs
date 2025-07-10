using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	class OrderXmlImportTask : XmlImportTask
	{
		public OrderXmlImportTask(INotifications notify)
			: this(SystemDataRegistry.Instance.OrdersXMLDataImportDirectory, notify, NotificationDataRegistry.Instance.OrderImportNotificationGroup)
		{
		}

		public OrderXmlImportTask(StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup)
			: base(registryPath, notify, notificationGroup, BillingInterfaceName.OrderXmlImport)
		{
		}

		public override ZString TaskDescription
		{
			get { return Res.GetString("cd5e6379-0260-48ef-9656-e518e7550035", "Order XML Import"); }
		}

		protected override DataImporter NewImporter()
		{
			return new OrderXmlDataImporter(new BatchStandaloneOrderValueObjectDataAdapter());
		}
	}
}
