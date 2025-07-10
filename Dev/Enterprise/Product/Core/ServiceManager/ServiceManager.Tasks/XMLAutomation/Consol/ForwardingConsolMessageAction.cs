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
	public class ForwardingConsolMessageAction : ImportMessageAction
	{
		public ForwardingConsolMessageAction(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider)
		{ }

		protected override IDataImporterControllingSave GetDataImporter()
		{
			return new XmlDataImporter(FactoryProvider, new BatchForwardingConsolValueObjectDataAdapter());
		}

		protected override IGlbGroup NotificationGroup
		{
			get
			{
				return notificationGroup ?? (notificationGroup = FactoryProvider.Current.Load<GlbGroup>(NotificationDataRegistry.Instance.ConsolShipmentImportNotificationGroup.Value));
			}
		}
		IGlbGroup notificationGroup;

		protected override ZString NotificationGroupRegistryPath
		{
			get { return Res.GetString("107917AB-EAD1-4BF6-988E-A61ED8F12A1F", "System->Registry->Notification->Consol Shipment Import Notification Group"); }
		}
	}
}
