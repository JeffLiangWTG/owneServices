using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.eHubMessaging.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class OrderMessageAction : ImportMessageAction
	{
		public OrderMessageAction(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider)
		{ }

		protected override IDataImporterControllingSave GetDataImporter()
		{
			return new XmlDataImporter(FactoryProvider, new BatchStandaloneOrderValueObjectDataAdapter());
		}

		protected override IGlbGroup NotificationGroup
		{
			get { return notificationGroup ?? (notificationGroup = FactoryProvider.Current.Load<GlbGroup>(NotificationDataRegistry.Instance.OrderImportNotificationGroup.Value)); }
		}
		IGlbGroup notificationGroup;

		protected override ZString NotificationGroupRegistryPath
		{
			get { return Res.GetString("CACBA716-F3DB-4EDD-85EC-824D90AD40D6", "System->Registry->Notification->Order Import Notification Group"); }
		}
	}
}
