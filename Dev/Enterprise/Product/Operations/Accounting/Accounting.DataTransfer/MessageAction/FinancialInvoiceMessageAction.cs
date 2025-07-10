using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.DataTransfer.Integration;
using Enterprise.eHubMessaging.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Accounting.DataTransfer
{
	public class FinancialInvoiceMessageAction : ImportMessageAction
	{
		public FinancialInvoiceMessageAction(BusinessObjectFactoryProvider factoryProvider)
			: base(factoryProvider)
		{
			this.OnlySaveDataWhenNoRecordsHaveErrors = true;
		}

		internal MultipleInvoiceXmlDataTransferDirector GetNewDataTransferDirector()
		{
			return new MultipleInvoiceXmlDataTransferDirector(true);
		}

		protected override IDataImporterControllingSave GetDataImporter()
		{
			return GetNewDataTransferDirector().GetNewXmlDataImporter(FactoryProvider);
		}

		protected override IGlbGroup NotificationGroup
		{
			get { return notificationGroup ?? (notificationGroup = FactoryProvider.Current.Load<GlbGroup>(NotificationDataRegistry.Instance.FinancialTransactionImportNotificationGroup.Value)); }
		}

		IGlbGroup notificationGroup;

		protected override ZString NotificationGroupRegistryPath
		{
			get { return Res.GetString("6C21CF0D-C806-4BD3-9355-5209204797E3", "System->Registry->Notification->Financial Transaction Import Notification Group"); }
		}
	}
}
