using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	class ScheduleXmlImportTask : XmlImportTask
	{
		public ScheduleXmlImportTask(INotifications notify)
			: this(SystemDataRegistry.Instance.ScheduleDataImportDirectory, notify, NotificationDataRegistry.Instance.SchedulesImportNotificationGroup)
		{
		}

		public ScheduleXmlImportTask(StringRegistryItem registryPath, INotifications notify, GuidRegistryItem notificationGroup)
			: base(registryPath, notify, notificationGroup, BillingInterfaceName.ScheduleXmlImport)
		{
		}

		public override ZString TaskDescription
		{
			get { return Res.GetString("cac96e9c-5f35-4018-8e47-4beb7a8ea40b", "Schedule XML Import"); }
		}

		protected override DataImporter NewImporter()
		{
			return new XmlDataImporter(new BatchScheduleValueObjectDataAdapter());
		}
	}
}
