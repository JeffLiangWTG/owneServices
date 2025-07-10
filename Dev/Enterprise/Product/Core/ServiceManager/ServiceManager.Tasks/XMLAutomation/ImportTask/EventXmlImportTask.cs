using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class EventXmlImportTask : XmlImportTask
	{
		public EventXmlImportTask(INotifications notify)
			: this(SystemDataRegistry.Instance.EventDataImportDirectory, notify, NotificationDataRegistry.Instance.EventImportNotificationGroup)
		{
		}

		public EventXmlImportTask(StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup)
			: base(registryPath, notify, notificationGroup, BillingInterfaceName.EventXmlImport)
		{
		}

		public override ZString TaskDescription
		{
			get { return Res.GetString("49d89e6a-d3bc-41f8-bc41-48d72755a0e1", "Event XML Import"); }
		}

		protected override DataImporter NewImporter()
		{
			return new XmlDataImporter(new StmALogValueObjectDataAdapterForBatchImport());
		}
	}
}
