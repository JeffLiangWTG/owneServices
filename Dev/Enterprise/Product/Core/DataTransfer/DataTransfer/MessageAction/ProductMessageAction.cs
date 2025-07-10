using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.eHubMessaging.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.DataTransfer
{
	public class ProductMessageAction : ImportMessageAction
	{
		public ProductMessageAction(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider)
		{ }

		protected override IDataImporterControllingSave GetDataImporter()
		{
			return new XmlDataImporter(FactoryProvider, ProductValueObjectDataAdapter.New());
		}

		protected override IGlbGroup NotificationGroup
		{
			get
			{
				return notificationGroup ?? (notificationGroup = FactoryProvider.Current.Load<GlbGroup>(NotificationDataRegistry.Instance.ProductImportNotificationGroup.Value));
			}
		}
		IGlbGroup notificationGroup;

		protected override ZString NotificationGroupRegistryPath
		{
			get { return Res.GetString("CB5F74AA-0A32-4C98-B7C4-2EC2EDF0DD84", "System->Registry->Notification->Product Import Notification Group"); }
		}
	}
}
