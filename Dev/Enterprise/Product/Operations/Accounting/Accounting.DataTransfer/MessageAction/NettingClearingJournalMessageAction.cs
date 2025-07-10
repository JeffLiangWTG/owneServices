using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.eHubMessaging.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Accounting.DataTransfer
{
	public class NettingClearingJournalMessageAction : ImportMessageAction
	{
		public NettingClearingJournalMessageAction(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider)
		{
		}

		protected override IDataImporterControllingSave GetDataImporter()
		{
			var adapter = new RemittanceFileImportAdapter();
			var importer = new XmlDataImporter(FactoryProvider, adapter);
			adapter.ProcessWithSaveExceptionHandling = importer.ProcessWithSaveExceptionHandling;

			return importer;
		}

		protected override IGlbGroup NotificationGroup
		{
			get { return notificationGroup ?? (notificationGroup = FactoryProvider.Current.Load<GlbGroup>(NotificationDataRegistry.Instance.NettingClearingJournalImportNotificationGroup.Value)); }
		}
		IGlbGroup notificationGroup;

		protected override ZString NotificationGroupRegistryPath
		{
			get { return Res.GetString("b69547f8-fbda-4a9a-8c71-3761dca2e0a9", "System->Registry->Notification->Netting Clearing Journal Import Notification Group"); }
		}
	}
}
