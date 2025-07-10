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

namespace Enterprise.Accounting.ElectronicMessaging.Germany
{
	public class EInvoicingEventMessageDEProcessor : EInvoicingEventMessageProcessor
	{
		internal new static class ContextTypeCode
		{
			public const string InvoiceDocument = "InvoiceDocument";
			public const string TransactionID = "TransactionID";
		}

		internal static class MessageSubTypeCode
		{
			public const string InvoiceResponse = "GEN";
		}

		public EInvoicingEventMessageDEProcessor(IXmlSessionTracker logger, IEDIMessage message, UniversalEvent universalEvent, AccEInvoicingBatch invoiceBatch)
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
				ReportAndLogError("PivotNotFound", Res.GetString("4a3bd331-08af-43e0-86d5-c2794324a5e5", "No update performed due to transaction pivot not found for invoice batch '{0}' in '{1}'.", invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			if (transactionPivot.AIP_Status == Core.Constants.EInvoicingPivotState.Succeed)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("bdeff497-b255-4f0a-be38-78dc10f43743", "No update performed due to transaction pivot having 'SUC' status for invoice batch '{0}' in '{1}'.", invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			AttachResponseMessageToeDocs();

			if (universalEvent.EventType.Value == AutoEvents.InterchangeAcknowledgedCode)
			{
				var messageSubType = universalEvent.EventParameters?.MessageSubType ?? ZString.Empty;
				if (messageSubType.IsEmpty)
				{
					ReportAndLogError("EmptyMessageSubType", Res.GetString("3039b75c-5a20-4ba2-ad93-186415549bbf", "No update performed due to universal event not containing message sub type for invoice batch '{0}' in '{1}'.", invoiceBatch.AIB_BatchNumber, companyName));
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
			SendErrorNotificationEmail(Res.GetString("2EE21E01-86D4-441B-AF53-60BB5C3FAECD", "Message to Germany invoice authorization service was rejected for invoice batch {0} in {1} with the following reason: {2}", invoiceBatch.AIB_BatchNumber, companyName, universalEvent.EventParameters?.Reason ?? ZString.Empty));
		}

		#endregion

		#region IAK Event Message

		void ProcessIAKEventMessage(ZString messageType)
		{
			switch (messageType)
			{
				case MessageSubTypeCode.InvoiceResponse:
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

			invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(
				Transactions.Select(x => x.PK),
				universalEvent.EventParameters?.Reason ?? ZString.Empty,
				EInvoicingPivotState.Succeed,
				lastResponseReceivedUtc: LastResponseReceivedTime);
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
			var invoiceDocumentContext = universalEvent.ContextCollection.FirstOrDefault(x => x.Type == ContextTypeCode.InvoiceDocument);
			if (invoiceDocumentContext == null)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("619530D3-7EB9-4522-AF84-D685330EFD34", "Germany Response Message was not attached to eDocs due to context collection not having Response Message Context for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			if (!invoiceDocumentContext.Value.HasValue || invoiceDocumentContext.Value.Value.IsEmpty)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("0C4CEFB9-8F87-42B2-91FD-FA4E3D4F7C45", "Germany Response Message was not attached to eDocs due to Response Message Context value was empty for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			var transaction = Transactions.FirstOrDefault();
			if (transaction == null)
			{
				ReportAndLogError("TransactionNotFound", Res.GetString("E2313B75-CD61-4089-A142-17001FEB6B56", "Germany Response Message was not attached to eDocs due to related transaction was not found for pivot in invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			var fileName = MessageSubTypeCode.InvoiceResponse + "_" + ZDateTime.UtcNow.ToString("yyyyMMddhhmmss", CultureInfo.InvariantCulture) + ".xml";
			transaction.DocManagerInfo.AddFileOrDocument(MessageEncoding.UTF8WithoutBOM.GetBytes(invoiceDocumentContext.Value.Value.ToUTF8FromBase64()), fileName, Core.Constants.ReferenceTypes.Accounting);
			transaction.DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(false);
		}

		#endregion
	}
}
