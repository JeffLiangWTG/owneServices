using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.China
{
	public class EInvoicingEventMessageCNProcessorForInvoice : EInvoicingEventMessageProcessor
	{
		public EInvoicingEventMessageCNProcessorForInvoice(IXmlSessionTracker logger, IEDIMessage message, UniversalEvent universalEvent, InvoicingBase invoice) : base(logger, message, universalEvent, invoice)
		{
		}

		protected override GEIEmailNotificationCreator GetEmailNotificationCreator(ZString errorMessage, ILogger logCollector)
		{
			return new GEIEmailNotificationCreator(ediMessage, invoice, new List<ZString>() { errorMessage }, logCollector);
		}

		protected override string LogErrorNotFoundKey => "TransactionNotFound";

		public override void Process()
		{
			if (universalEvent.EventType.Value == AutoEvents.InterchangeAcknowledgedCode)
			{
				ProcessIAKEventMessage();
			}
			else
			{
				ProcessIRJEventMessage();
			}
		}

		void ProcessIAKEventMessage()
		{
			var eventAndDatabaseTransaction = new EventAndDatabaseTransaction()
			{
				Transaction = invoice,
				UniversalEvent = universalEvent,
				CompanyName = companyName,
				EventData = UniversalEventTransactionDataObject.FromUniversalEvent(universalEvent)
			};

			if (eventAndDatabaseTransaction.Transaction.ComplianceDocumentStatus.StartsWith(ChinaComplianceInfo.ComplianceDocumentStatusTypes.CDI.Code)
				&& eventAndDatabaseTransaction.Transaction.AH_TransactionReference != eventAndDatabaseTransaction.EventData.ComplianceNumber)
			{
				eventAndDatabaseTransaction.Transaction.AH_TransactionReference = ZString.Empty;
				eventAndDatabaseTransaction.Transaction.AH_ComplianceDocumentDate = ZDate.Empty;
			}

			eventAndDatabaseTransaction.MapComplianceNumberToDatabase(logger);
			eventAndDatabaseTransaction.MapComplianceDateToDatabase(logger);
			eventAndDatabaseTransaction.MapComplianceSubTypeToDatabase(logger);
			eventAndDatabaseTransaction.MapComplianceDocumentStatusToDatabase();
			eventAndDatabaseTransaction.MapVoidedAndCreditedAmountToDatabase();
			ChinaEInvoiceHelper.UpdateDDIReferenceAndFileName(invoice, universalEvent.AttachedDocumentCollection);
		}

		void ProcessIRJEventMessage()
		{
			var errorMessage = universalEvent.EventParameters?.Reason ?? ZString.Empty;
			SendErrorNotificationEmail(errorMessage);
		}
	}
}
