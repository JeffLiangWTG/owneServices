using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Egypt
{
	class EgyptTransactionBatchEventMessageProcessor : TransactionBatchEventMessageProcessor
	{
		public EgyptTransactionBatchEventMessageProcessor(EventMessageProcessorData eventMessageProcessorData, ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory)
			: base(eventMessageProcessorData, countryEInvoicingObjectFactory)
		{
		}

		bool ReceivingEInvoiceStatusEnabled => AccountingElectronicMessagingRegistry.Instance
			.EnableReceivingEInvoiceStatusNotification.GetValueWithoutFallback(invoiceBatch.Company.PK.ToGuid(), branchPK: default, departmentPK: default);

		protected override ZString GetPivotStatusForIAK(EventAndDatabaseTransaction eventAndDatabaseTransaction)
			=> !ReceivingEInvoiceStatusEnabled && eventAndDatabaseTransaction.EventData.PivotStatus == EInvoicingPivotState.Delivered
				? Constants.EInvoicingPivotState.Succeed
				: eventAndDatabaseTransaction.EventData.PivotStatus;
	}
}
