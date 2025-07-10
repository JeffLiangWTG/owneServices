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
	public class ForwardingShipmentMessageAction : ImportMessageAction
	{
		public ForwardingShipmentMessageAction(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider)
		{ }

		protected override IDataImporterControllingSave GetDataImporter()
		{
			return new XmlDataImporter(FactoryProvider, new BatchForwardingShipmentValueObjectDataAdapter());
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
			get { return Res.GetString("644A77A6-163B-49B1-99E7-042D0DF9D3E1", "System->Registry->Notification->Consol Shipment Import Notification Group"); }
		}
	}
}
