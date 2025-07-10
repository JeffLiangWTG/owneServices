using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing.EventMessageProcessor;
using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.Accounting.ElectronicMessaging.China
{
	internal class EInvoicingEventMessageCNProcessor : GlobalEInvoicingEventMessageProcessor
	{
		public EInvoicingEventMessageCNProcessor(EventMessageProcessorData eventMessageProcessorData, ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory)
			: base(eventMessageProcessorData, countryEInvoicingObjectFactory)
		{
		}

		protected override void ProcessIAKEventMessage()
		{
			if (EventAndDatabaseTransaction.Transaction.ComplianceDocumentStatus.StartsWith(ChinaComplianceInfo.ComplianceDocumentStatusTypes.CDI.Code)
				&& EventAndDatabaseTransaction.Transaction.AH_TransactionReference != EventAndDatabaseTransaction.EventData.ComplianceNumber)
			{
				EventAndDatabaseTransaction.Transaction.AH_TransactionReference = ZString.Empty;
				EventAndDatabaseTransaction.Transaction.AH_ComplianceDocumentDate = ZDate.Empty;
			}

			base.ProcessIAKEventMessage();

			EventAndDatabaseTransaction.MapVoidedAndCreditedAmountToDatabase();
		}
	}
}
