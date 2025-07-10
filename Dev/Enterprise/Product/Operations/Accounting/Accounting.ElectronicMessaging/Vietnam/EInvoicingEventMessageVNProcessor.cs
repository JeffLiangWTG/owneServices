using System;
using System.Collections.Generic;
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
using Newtonsoft.Json;
using static Enterprise.Core.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Vietnam
{
	public class EInvoicingEventMessageVNProcessor : EInvoicingEventMessageProcessor
	{
		internal new static class ContextTypeCode
		{
			public const string ResponseMessage = "ResponseMessage";
		}

		public EInvoicingEventMessageVNProcessor(IXmlSessionTracker logger, IEDIMessage message, UniversalEvent universalEvent, AccEInvoicingBatch invoiceBatch) : base(logger, message, universalEvent, invoiceBatch)
		{
		}

		InvoicingBase[] Transactions
		{
			get
			{
				if (transactions == null)
				{
					transactions = invoiceBatch.Factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.PK, invoiceBatch.TransactionPivots.Cast<AccEInvoicingTransactionPivot>().Select(x => x.AIP_ParentID)));
				}

				return transactions;
			}
		}
		InvoicingBase[] transactions;

		AccEInvoicingTransactionPivot TransactionPivot;

		public override void Process()
		{
			if (invoiceBatch.AIB_Status != EInvoicingBatchState.Discarded)
			{
				TransactionPivot = invoiceBatch.TransactionPivots.Cast<AccEInvoicingTransactionPivot>().FirstOrDefault();
				if (TransactionPivot != null)
				{
					if (invoiceBatch.TransactionPivots.Cast<AccEInvoicingTransactionPivot>().Any(x => x.AIP_Status != EInvoicingPivotState.Succeed))
					{
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
					else
					{
						logger.LogBoth(LogType.Warning, Res.GetString("97D9763B-90E7-48AA-B51D-5B93B17579CC", "No update performed due to transaction pivot having 'SUC' status for invoicing batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
					}
				}
				else
				{
					ReportAndLogError("PivotNotFound", Res.GetString("E55D5A41-2A95-47A8-8B4B-9ACD5AF73A73", "No update performed due to transaction pivot not found for invoicing batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				}
			}
		}

		#region ProcessIAKEventMessage

		void ProcessIAKEventMessage(ZString messageType)
		{
			switch (messageType)
			{
				case VietnamEInvoiceAPICommandList.Codes.SendReceivablesInvoice:
					ProcessSRNEventMessage();
					break;
				case VietnamEInvoiceAPICommandList.Codes.CancelReceivablesInvoice:
				case VietnamEInvoiceAPICommandList.Codes.CancelReceivablesCircular78Invoice:
					ProcessCRNEventMessage();
					break;
				case VietnamEInvoiceAPICommandList.Codes.RequestDocumentForInvoice:
					ProcessRDNEventMessage();
					break;
				case VietnamEInvoiceAPICommandList.Codes.AdjustReceivablesInvoice:
					ProcessARNEventMessage();
					break;
				case VietnamEInvoiceAPICommandList.Codes.ApproveReceivablesInvoice:
					ProcessAPRNEventMessage();
					break;
				default:
					ReportAndLogError("UnknownMessageSubType", Res.GetString("d2aa393a-add5-4a59-9353-3518dd18aab4", "No update performed due to invalid message sub type [{0}] found for invoice batch {1} in {2}.", messageType, invoiceBatch.AIB_BatchNumber, companyName));
					break;
			}
		}

		void ProcessSRNEventMessage()
		{
			UpdateBatchAndPivotThenCreatePivotQueue(EInvoicingPivotActionType.DocumentAction);
		}

		void ProcessCRNEventMessage()
		{
			UpdateBatchAndPivotThenCreatePivotQueue(EInvoicingPivotActionType.DocumentAction);
		}

		void ProcessRDNEventMessage()
		{
			UpdateBatchAndPivotStatusAndErrorDescription();
			AttachResponseMessageToeDocs();
		}

		void ProcessARNEventMessage()
		{
			UpdateBatchAndPivotThenCreatePivotQueue(EInvoicingPivotActionType.Approve);
		}

		void ProcessAPRNEventMessage()
		{
			UpdateBatchAndPivotThenCreatePivotQueue(EInvoicingPivotActionType.DocumentAction);
		}

		void UpdateBatchAndPivotThenCreatePivotQueue(string pivotActionType)
		{
			UpdateBatchAndPivotStatusAndErrorDescription();

			var transaction = Transactions.First();
			transaction.CreateNewEInvoicingPivot(transaction.Factory, pivotActionType);
		}

		void UpdateBatchAndPivotStatusAndErrorDescription()
		{
			TransactionPivot.AIP_LastResponseReceivedUtc = LastResponseReceivedTime;
			invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(Transactions.Select(x => x.PK), universalEvent.EventParameters?.Reason ?? ZString.Empty, EInvoicingPivotState.Succeed, lastResponseReceivedUtc: LastResponseReceivedTime);
		}

		void ProcessIRJEventMessage()
		{
			var errorMessage = universalEvent.EventParameters?.Reason ?? ZString.Empty;
			invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(Transactions.Select(x => x.PK), universalEvent.EventParameters?.Reason ?? ZString.Empty, EInvoicingPivotState.Failed, lastResponseReceivedUtc: LastResponseReceivedTime);
			SendErrorNotificationEmail(errorMessage);
		}

		#endregion

		protected override GEIEmailNotificationCreator GetEmailNotificationCreator(ZString errorMessage, ILogger logCollector)
		{
			return new GEIEmailNotificationCreator(ediMessage, Transactions.First(), new List<ZString>() { errorMessage }, logCollector);
		}

		void AttachResponseMessageToeDocs()
		{
			var responseMessageContext = universalEvent.ContextCollection.FirstOrDefault(x => x.Type == ContextTypeCode.ResponseMessage);

			if (responseMessageContext == null)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("80a34e57-cc6f-4080-b0b1-34432d538404", "Vietnam Response Message was not attached to eDocs due to context collection not having Response Message Context for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			if (!responseMessageContext.Value.HasValue || responseMessageContext.Value.Value.IsEmpty)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("77f4a499-0c97-42e4-834b-3abdb598e244", "Vietnam Response Message was not attached to eDocs due to Response Message Context value was empty for invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			var transaction = Transactions.FirstOrDefault();
			if (transaction == null)
			{
				ReportAndLogError("TransactionNotFound", Res.GetString("5F56F8A2-DDC3-4409-96C1-B45BEA18E22A", "Vietnam Response Message was not attached to eDocs due to related transaction was not found for pivot in invoice batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				return;
			}

			var transactionReference = ZString.Empty;
			var complianceDocumentDate = ZDate.Empty;

			if (transaction.IsCancelled)
			{
				transactionReference = transaction.OriginalReferenceTransaction.AH_TransactionReference;
				complianceDocumentDate = transaction.OriginalReferenceTransaction.AH_ComplianceDocumentDate;
			}
			else
			{
				transactionReference = transaction.AH_TransactionReference;
				complianceDocumentDate = transaction.AH_ComplianceDocumentDate;
			}

			var fileName = "EInvoice_" + transactionReference + "_" + complianceDocumentDate.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);

			var codedContext = responseMessageContext.Value.Value.ToUTF8FromBase64().Trim(new char[] { '[', ']' });
			var eInvoiceDocumentResponse = JsonConvert.DeserializeObject<VietnamEInvoiceDocumentResponse>(codedContext);
			byte[] eInvoiceDocumentContext = null;

			if (!eInvoiceDocumentResponse.pdf.IsEmpty)
			{
				eInvoiceDocumentContext = Convert.FromBase64String(eInvoiceDocumentResponse.pdf.Replace("data:application/pdf;base64,", ""));
				fileName += ".pdf";
			}
			else if (!eInvoiceDocumentResponse.xml.IsEmpty)
			{
				eInvoiceDocumentContext = MessageEncoding.UTF8WithoutBOM.GetBytes(eInvoiceDocumentResponse.xml);
				fileName += ".xml";
			}
			else if (eInvoiceDocumentResponse.doc != null)
			{
				eInvoiceDocumentContext = MessageEncoding.UTF8WithoutBOM.GetBytes(eInvoiceDocumentResponse.doc.ToString());
				fileName += ".json";
			}

			transaction.DocManagerInfo.AddFileOrDocument(eInvoiceDocumentContext, fileName, Core.Constants.ReferenceTypes.Accounting);
			transaction.DocManagerInfo.SetupEDocsFactoryToBeSavedWithMainFactory(false);
		}

		protected override bool ShouldSendEmail => !Transactions.IsNullOrEmpty();

		protected override string LogErrorNotFoundKey => "TransactionNotFound";
	}
}
