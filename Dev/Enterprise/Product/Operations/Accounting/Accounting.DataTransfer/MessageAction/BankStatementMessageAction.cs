using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.BankStatement;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.eHubMessaging.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Accounting.DataTransfer
{
	public class BankStatementMessageAction : ImportMessageAction
	{
		public BankStatementMessageAction(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider)
		{
			this.OnlySaveDataWhenNoRecordsHaveErrors = true;
		}

		protected override IDataImporterControllingSave GetDataImporter()
		{
			return new XmlDataImporter(FactoryProvider, new BankStatementDataAdapter());
		}

		protected override IGlbGroup NotificationGroup
		{
			get { return notificationGroup ?? (notificationGroup = FactoryProvider.Current.Load<GlbGroup>(NotificationDataRegistry.Instance.BankStatementImportNotificationGroup.Value)); }
		}
		IGlbGroup notificationGroup;

		protected override ZString NotificationGroupRegistryPath
		{
			get { return Res.GetString("499c4b37-53cb-44f9-99d2-0fbb6a5d3682", "System->Registry->Notification->Bank Statement Import Notification Group"); }
		}
	}
}