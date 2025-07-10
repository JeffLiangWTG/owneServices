using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.India
{
	// Code will be removed in WI00829724 once we are confident all customers support new style XUE messages

	public class EInvoicingEventMessageINProcessor : EInvoicingEventMessageProcessor
	{
		internal new static class ContextTypeCode
		{
			public const string ResponseMessage = "ResponseMessage";
		}

		internal static class MessageSubTypeCode
		{
			public const string InvoiceResponse = "GEN";
		}

		public EInvoicingEventMessageINProcessor(IXmlSessionTracker logger, IEDIMessage message, UniversalEvent universalEvent, AccEInvoicingBatch invoiceBatch)
			: base(logger, message, universalEvent, invoiceBatch)
		{
		}

		List<InvoicingBase> Transactions
		{
			get
			{
				if (transactions == null && transactionPivot != null)
				{
					transactions = new List<InvoicingBase>();
					var transaction = invoiceBatch.Factory.LoadTop1<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.PK, transactionPivot.AIP_ParentID));
					if (transaction != null)
					{
						transactions.Add(transaction);
					}
				}

				return transactions;
			}
		}
		List<InvoicingBase> transactions;
		AccEInvoicingTransactionPivot transactionPivot;

		public override void Process()
		{
			if (invoiceBatch.AIB_Status == Core.Constants.EInvoicingBatchState.Discarded)
			{
				return;
			}

			transactionPivot = invoiceBatch.TransactionPivots.Cast<AccEInvoicingTransactionPivot>().FirstOrDefault();
			if (transactionPivot == null)
			{
				ReportAndLogError("PivotNotFound", Res.GetString("4a3bd231-08af-43e0-86d5-c2794324a5e5", "No update performed due to transaction pivot not found for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			if (transactionPivot.AIP_Status == Core.Constants.EInvoicingPivotState.Succeed)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("bdeff497-a255-4f0a-be38-78dc10f43743", "No update performed due to transaction pivot having 'SUC' status for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			if (universalEvent.EventType.Value == AutoEvents.InterchangeAcknowledgedCode)
			{
				var messageSubType = universalEvent.EventParameters?.MessageSubType ?? ZString.Empty;
				if (messageSubType.IsEmpty)
				{
					ReportAndLogError("EmptyMessageSubType", Res.GetString("3029b75c-5a20-4ba2-ad93-186415549bbf", "No update performed due to universal event not containing message sub type for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
					return;
				}

				ProcessIAKEventMessage(messageSubType);
			}
			else
			{
				ProcessIRJEventMessage();
			}
		}

		#region IRJ Event Message

		void ProcessIRJEventMessage()
		{
			invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(Transactions.Select(x => x.PK), universalEvent.EventParameters?.Reason ?? ZString.Empty, EInvoicingPivotState.Failed, lastResponseReceivedUtc: LastResponseReceivedTime);
			SendErrorNotificationEmail(Res.GetString("669ee1c8-bfbc-4226-bea1-e70411690556", "Message to India invoice authorization service was rejected for invoice batch {0} in {1} with the following reason: {2}", invoiceBatch.AIB_BatchNumber, companyName, universalEvent.EventParameters?.Reason ?? ZString.Empty));
		}

		#endregion

		#region IAK Event Message

		void ProcessIAKEventMessage(ZString messageType)
		{
			switch (messageType)
			{
				case MessageSubTypeCode.InvoiceResponse:
					ProcessInvoiceResponseMessage();
					break;
				default:
					ReportAndLogError("UnknownMessageSubType", Res.GetString("d2aa393a-add5-4a59-9353-3518dd18aab4", "No update performed due to invalid message sub type [{0}] found for invoice batch {1} in {2}.", messageType, invoiceBatch.AIB_BatchNumber, companyName));
					break;
			}
		}

		void ProcessInvoiceResponseMessage()
		{
			transactionPivot.AIP_LastResponseReceivedUtc = LastResponseReceivedTime;

			if (universalEvent.ContextCollection == null)
			{
				ReportAndLogError("MissingContextCollection", Res.GetString("190f3680-6d5b-47eb-9252-2493f8786ad7", "India Invoice Response was not processed due to universal event message not having context collection for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			var responseContext = universalEvent.ContextCollection.FirstOrDefault(x => x.Type == ContextTypeCode.ResponseMessage);
			if (responseContext == null)
			{
				ReportAndLogError("MissingResponseMessageContext", Res.GetString("66e551e0-8da6-4f30-b151-e7c74219e5a4", "India Invoice Response was not processed due to context collection not having Response Message Context for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			if (!responseContext.Value.HasValue || responseContext.Value.Value.IsEmpty)
			{
				ReportAndLogError("ResponseMessageContextHasNoValue", Res.GetString("4ed98ff9-043f-4682-bf7e-802128043c74", "India Invoice Response was not processed due to Response Message Context value was empty for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			ProcessInvoiceResponse(responseContext.Value.Value);
			AttachResponseMessageToeDocs();
		}

		void ProcessInvoiceResponse(ZString response)
		{
			var notifications = new NotificationBuffer();

			var transaction = Transactions.FirstOrDefault();
			if (transaction == null)
			{
				LogError(Res.GetString("01a89109-12e0-4e65-80e3-891b34af91ff", "Response Message was not processed due to related transaction was not found for pivot in invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			var reader = new IndiaEInvoiceResponseReader(response, notifications);
			var jsonResponse = reader.Read();
			if (jsonResponse == null || notifications.HasErrors)
			{
				var errors = notifications.AsString;
				LogError(errors);
				return;
			}

			new IndiaEInvoiceAuthorisationRecordCreator(jsonResponse, transaction.PK).Create(invoiceBatch.Factory);
			invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(Transactions.Select(x => x.PK), universalEvent.EventParameters?.Reason ?? ZString.Empty, EInvoicingPivotState.Succeed, lastResponseReceivedUtc: LastResponseReceivedTime);
		}

		#endregion

		#region Helpers

		protected override bool ShouldSendEmail => !Transactions.IsNullOrEmpty();

		protected override string LogErrorNotFoundKey => "TransactionNotFound"; // Error Message Key for Developers Only

		protected override GEIEmailNotificationCreator GetEmailNotificationCreator(ZString errorMessage, ILogger logCollector)
		{
			return new GEIEmailNotificationCreator(ediMessage, Transactions.First(), new List<ZString>() { errorMessage }, logCollector);
		}

		void AttachResponseMessageToeDocs()
		{
			var responseMessageContext = universalEvent.ContextCollection.FirstOrDefault(x => x.Type == ContextTypeCode.ResponseMessage);
			if (responseMessageContext == null)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("117de129-475a-4a8c-a8a9-6102418d69dd", "India Response Message was not attached to eDocs due to context collection not having Response Message Context for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			if (!responseMessageContext.Value.HasValue || responseMessageContext.Value.Value.IsEmpty)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("27346bd7-21a5-4f7f-bde7-a6fe9305fb55", "India Response Message was not attached to eDocs due to Response Message Context value was empty for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			var transaction = Transactions.FirstOrDefault();
			if (transaction == null)
			{
				ReportAndLogError("TransactionNotFound", Res.GetString("b184845f-0ffb-499e-bfcf-873531e873f2", "India Response Message was not attached to eDocs due to related transaction was not found for pivot in invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			var fileName = universalEvent.EventParameters.MessageSubType.Value + "_" + ZDateTime.UtcNow.ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture) + ".json";
			transaction.DocManagerInfo.AddFileOrDocument(MessageEncoding.UTF8WithoutBOM.GetBytes(responseMessageContext.Value.Value.ToUTF8FromBase64()), fileName, Core.Constants.ReferenceTypes.Accounting);
			transaction.DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(false);
		}

		#endregion
	}
}
