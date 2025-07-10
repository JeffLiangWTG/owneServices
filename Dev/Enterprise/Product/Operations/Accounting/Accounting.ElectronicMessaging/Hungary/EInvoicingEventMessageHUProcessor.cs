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
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Hungary
{
	public class EInvoicingEventMessageHUProcessor : EInvoicingEventMessageProcessor
	{
		internal new static class ContextTypeCode
		{
			public const string ResponseMessage = "ResponseMessage";
			public const string TransactionID = "TransactionID";
		}

		static string CountryName => Res.GetString("aace78e6-f400-4b5b-b72a-9409c5678c92", "Hungary");

		public EInvoicingEventMessageHUProcessor(IXmlSessionTracker logger, IEDIMessage message, UniversalEvent universalEvent, AccEInvoicingBatch invoiceBatch)
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
			var transactionIDContext = universalEvent.ContextCollection?.FirstOrDefault(x => x.Type == ContextTypeCode.TransactionID);
			if (transactionIDContext != null)
			{
				// TransactionID is optional for IRJ event; no errors are logged if it is missing or blank.
				invoiceBatch.AIB_GovernmentAllocatedNumber = transactionIDContext.Value ?? ZString.Empty;
			}

			invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(Transactions.Select(x => x.PK), universalEvent.EventParameters?.Reason ?? ZString.Empty, EInvoicingPivotState.Failed, lastResponseReceivedUtc: LastResponseReceivedTime);
			SendErrorNotificationEmail(Res.GetString("e5805f0a-795e-45ea-82b4-a68403c1782d", "Message to {0} invoice authorization service was rejected for invoice batch {1} in {2} with the following reason: {3}", CountryName, invoiceBatch.AIB_BatchNumber, companyName, universalEvent.EventParameters?.Reason ?? ZString.Empty));

			AttachResponseMessageToeDocs();
		}

		#endregion

		#region IAK Event Message

		void ProcessIAKEventMessage(ZString messageType)
		{
			switch (messageType)
			{
				case HungaryEInvoiceAPICommandList.Codes.GenerateInvoiceRequest:
					ProcessGenerateInvoiceResponseMessage();
					break;
				default:
					ReportAndLogError("UnknownMessageSubType", Res.GetString("d2aa393a-add5-4a59-9353-3518dd18aab4", "No update performed due to invalid message sub type [{0}] found for invoice batch {1} in {2}.", messageType, invoiceBatch.AIB_BatchNumber, companyName));
					break;
			}
		}

		void ProcessGenerateInvoiceResponseMessage()
		{
			transactionPivot.AIP_LastResponseReceivedUtc = LastResponseReceivedTime;

			if (universalEvent.ContextCollection == null)
			{
				ReportAndLogError("MissingContextCollection", Res.GetString("3fa19d86-431d-4119-82ec-398e4af4baf8", "{0} Generate Invoice Response was not processed due to universal event message not having context collection for invoice batch {1} in {2}.", CountryName, invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			var transactionIDContext = universalEvent.ContextCollection.FirstOrDefault(x => x.Type == ContextTypeCode.TransactionID);
			if (transactionIDContext == null)
			{
				ReportAndLogError("MissingTransactionIDContext", Res.GetString("94640ee9-5107-466c-b0e0-62153eca73ea", "{0} Generate Invoice Response was not processed due to context collection not having Transaction ID Context for invoice batch {1} in {2}.", CountryName, invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			if (!transactionIDContext.Value.HasValue)
			{
				ReportAndLogError("TransactionIDContextHasNoValue", Res.GetString("7d52e431-a40d-4c2b-a442-e2f77a031660", "{0} Generate Invoice Response was not processed due to Transaction ID Context value was empty for invoice batch {1} in {2}.", CountryName, invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			invoiceBatch.AIB_GovernmentAllocatedNumber = transactionIDContext.Value.Value;
			invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(Transactions.Select(x => x.PK), universalEvent.EventParameters?.Reason ?? ZString.Empty, EInvoicingPivotState.Succeed, lastResponseReceivedUtc: LastResponseReceivedTime);

			AttachResponseMessageToeDocs();
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
				logger.LogBoth(LogType.Warning, Res.GetString("fddf1646-e869-4860-bb8b-2c2f9470f988", "{0} Response Message was not attached to eDocs due to context collection not having Response Message Context for invoice batch {1} in {2}.", CountryName, invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			if (!responseMessageContext.Value.HasValue || responseMessageContext.Value.Value.IsEmpty)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("e009f8ce-ff00-4a51-849c-87fc6f42b377", "{0} Response Message was not attached to eDocs due to Response Message Context value was empty for invoice batch {1} in {2}.", CountryName, invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			var transaction = Transactions.FirstOrDefault();
			if (transaction == null)
			{
				ReportAndLogError("TransactionNotFound", Res.GetString("1db0aa0e-85fb-4599-81d8-32aab1ca150d", "{0} Response Message was not attached to eDocs due to related transaction was not found for pivot in invoice batch {1} in {2}.", CountryName, invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			var fileName = universalEvent.EventParameters.MessageSubType.Value + "_" + ZDateTime.UtcNow.ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture) + ".xml";
			transaction.DocManagerInfo.AddFileOrDocument(MessageEncoding.UTF8WithoutBOM.GetBytes(responseMessageContext.Value.Value.ToUTF8FromBase64()), fileName, Core.Constants.ReferenceTypes.Accounting);
			transaction.DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(false);
		}

		#endregion
	}
}
