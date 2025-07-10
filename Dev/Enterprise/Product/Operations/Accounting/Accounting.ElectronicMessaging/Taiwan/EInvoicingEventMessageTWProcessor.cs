using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.Taiwan
{
	public class EInvoicingEventMessageTWProcessor : EInvoicingEventMessageProcessor
	{
		public EInvoicingEventMessageTWProcessor(IXmlSessionTracker logger, IEDIMessage message, UniversalEvent universalEvent, AccEInvoicingBatch invoiceBatch)
			: base(logger, message, universalEvent, invoiceBatch)
		{
		}

		AccComplianceDocumentHeader[] ComplianceDocuments
		{
			get
			{
				if (complianceDocuments == null)
				{
					var query = new ZQuery(AccComplianceDocumentHeaderSchema.PK, invoiceBatch.TransactionPivots.Cast<AccEInvoicingTransactionPivot>().Select(x => x.AIP_ParentID));
					complianceDocuments = invoiceBatch.Factory.Load<AccComplianceDocumentHeader>(query);
				}

				return complianceDocuments;
			}
		}
		AccComplianceDocumentHeader[] complianceDocuments;

		public override void Process()
		{
			if (invoiceBatch.AIB_Status != EInvoicingBatchState.Discarded)
			{
				if (ComplianceDocuments != null && ComplianceDocuments.Length > 0)
				{
					if (invoiceBatch.TransactionPivots.Cast<AccEInvoicingTransactionPivot>().Any(x => x.AIP_Status != EInvoicingPivotState.Succeed))
					{
						switch (universalEvent.EventType.Value)
						{
							case AutoEvents.InterchangeAcknowledgedCode:
								ProcessIAKEventMessage();
								break;
							case AutoEvents.InterchangeRejectedCode:
								ProcessIRJEventMessage();
								break;
							case AutoEvents.InterchangeSentCode:
								ProcessISNEventMessage();
								break;
						}
					}
					else
					{
						logger.LogBoth(LogType.Warning, Res.GetString("e61da529-dec8-4020-bbd0-7e6170435fe4", "No update performed due to compliance document pivot having 'SUC' status for invoicing batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
					}
				}
				else
				{
					ReportAndLogError("PivotNotFound", Res.GetString("937c5173-d80d-4c7b-bd35-a09334add04f", "No update performed due to compliance document pivot not found for invoicing batch {0} in {1}.", invoiceBatch.AIB_BatchNumber, companyName));
				}
			}
		}

		void ProcessIAKEventMessage()
		{
			var fileResultContext = universalEvent.ContextCollection.FirstOrDefault(x => x.Type == ContextTypeCode.ResponseFileResult);
			if (fileResultContext != null && fileResultContext.Value.HasValue && !fileResultContext.Value.Value.IsEmpty)
			{
				var decryptedData = Convert.FromBase64String(fileResultContext.Value.Value);
				var decryptedText = Encoding.UTF8.GetString(decryptedData);

				var lines = decryptedText.Split('\n');
				foreach (var line in lines)
				{
					var fields = line.Split('|');
					var complianceDocumentReferenceNumber = fields[0];
					var internalReference = (complianceDocumentReferenceNumber?.Length ?? 0) > 8 ? complianceDocumentReferenceNumber.Substring(complianceDocumentReferenceNumber.Length - 8, 8) : string.Empty;
					var pivotState = EInvoicingPivotState.Succeed;
					var errorMessage = ZString.Empty;

					if (fields.Length >= 6)
					{
						pivotState = fields[5] == "Y" ? EInvoicingPivotState.Succeed : EInvoicingPivotState.Failed;
						errorMessage = fields[6];
					}

					invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(ComplianceDocuments.Where(x => x.ADH_InternalReference == internalReference).Select(x => x.PK), errorMessage, pivotState, lastResponseReceivedUtc: LastResponseReceivedTime);
				}
			}
			else
			{
				invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(ComplianceDocuments.Select(x => x.PK), ZString.Empty, EInvoicingPivotState.Succeed, lastResponseReceivedUtc: LastResponseReceivedTime);
			}
		}

		void ProcessIRJEventMessage()
		{
			var errorMessage = universalEvent.EventParameters?.Reason ?? ZString.Empty;
			invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(ComplianceDocuments.Select(x => x.PK), errorMessage, EInvoicingPivotState.Failed, lastResponseReceivedUtc: LastResponseReceivedTime);
			SendErrorNotificationEmail(errorMessage);
		}

		void ProcessISNEventMessage()
		{
			var errorMessage = universalEvent.EventParameters?.Reason ?? ZString.Empty;
			var transactionPivots = invoiceBatch.TransactionPivots.Where(x => x.AIP_Status == EInvoicingPivotState.Sent).Select(x => x.AIP_ParentID);
			var transactionPKs = ComplianceDocuments.Where(x => transactionPivots.Any(pivot => pivot == x.PK)).Select(x => x.PK);
			if (transactionPKs.IsNullOrEmpty())
			{
				return;
			}

			invoiceBatch.UpdateBatchAndPivotStatusAndErrorDescription(transactionPKs, errorMessage, EInvoicingPivotState.Delivered, lastResponseReceivedUtc: LastResponseReceivedTime);
		}

		#region Helpers

		protected override bool ShouldSendEmail => !ComplianceDocuments.IsNullOrEmpty();

		protected override string LogErrorNotFoundKey => "ComplianceDocument"; // Error Message Key for Developers Only

		protected override GEIEmailNotificationCreator GetEmailNotificationCreator(ZString errorMessage, ILogger logCollector)
		{
			return new GEIEmailNotificationCreator(ediMessage, ComplianceDocuments.First(), new List<ZString>() { errorMessage }, logCollector);
		}

		#endregion
	}
}
