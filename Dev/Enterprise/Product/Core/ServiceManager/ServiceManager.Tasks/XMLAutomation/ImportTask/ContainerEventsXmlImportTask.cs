using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	class ContainerEventsXmlImportTask : XmlImportTask
	{
		public ContainerEventsXmlImportTask(INotifications notify)
			: this(SystemDataRegistry.Instance.ContainerEventsDataImportDirectory, notify, NotificationDataRegistry.Instance.ContainerEventsImportNotificationGroup)
		{
		}

		public ContainerEventsXmlImportTask(StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup)
			: base(registryPath, notify, notificationGroup, BillingInterfaceName.ContainerEventsXmlImport)
		{
		}

		public override ZString TaskDescription
		{
			get { return Res.GetString("657a7cd7-804d-41f0-a1a1-8960a6dd6727", "Container Events XML Import"); }
		}

		protected override DataImporter NewImporter()
		{
			return new ContainerEventsDataImporter();
		}
	}
}
