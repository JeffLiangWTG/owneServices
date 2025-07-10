using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public static class EInvoicingBatchExtensions
	{
		#region status update

		public static void UpdateBatchAndPivotStatusAndErrorDescription(this AccEInvoicingBatch batch, IEnumerable<ZGuid> transactionPKs, IEnumerable<ZString> errorMessages, ZString pivotStatus, string batchStatus = null, ZDateTime? lastResponseReceivedUtc = null)
		{
			var errorMessage = string.Join(". ", errorMessages);
			UpdateBatchAndPivotStatusAndErrorDescription(batch, transactionPKs, errorMessage, pivotStatus, batchStatus, lastResponseReceivedUtc);
		}

		public static void UpdateBatchAndPivotStatusAndErrorDescription(this AccEInvoicingBatch batch, IEnumerable<ZGuid> transactionPKs, ZString errorMessage, ZString pivotStatus, string batchStatus = null, ZDateTime? lastResponseReceivedUtc = null)
		{
			if (!string.IsNullOrEmpty(batchStatus))
			{
				batch.AIB_Status = batchStatus;
			}
			batch.UpdatePivotsStatus(pivotStatus, transactionPKs, EventProcessorHelper.CheckLengthAndTruncIfNeeded(batch.Company, errorMessage), lastResponseReceivedUtc);
		}

		public static void MarkBatchAndTransactionPivotsAsSent(this AccEInvoicingBatch batch, IEnumerable<ZGuid> transactionPKs, ZString? warningMessage = null)
		{
			batch.AIB_Status = EInvoicingBatchState.Sent;
			batch.UpdatePivotsStatus(EInvoicingPivotState.Sent, transactionPKs, warningMessage.HasValue ? warningMessage.ToString() : string.Empty, null);
			batch.UpdatePivotsLastSentTimeUtc(ZDateTime.UtcNow, transactionPKs);
		}

		#endregion

		#region load related records in batch

		public static ZGuid[] LoadEInvoicingPKsToBeSent(this AccEInvoicingBatch batch, string parentTableCode)
		{
			switch (parentTableCode)
			{
				case AccTransactionHeaderSchema.Constants.Prefix:
					return batch.LoadInvoicingBasesReadyToBeSent().Select(x => x.PK).ToArray();
				case AccComplianceDocumentHeaderSchema.Constants.Prefix:
					return batch.LoadComplianceDocumentsReadyToBeSent().Select(x => x.PK).ToArray();
				default:
					return null;
			}
		}

		public static InvoicingBase[] LoadInvoicingBasesReadyToBeSent(this AccEInvoicingBatch batch)
		{
			if (AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.Value)
			{
				return batch.GetInvoicesWithStatus(EInvoicingPivotState.Batched, EInvoicingPivotState.BatchedWithError);
			}
			else
			{
				return batch.GetInvoicesWithStatus(EInvoicingPivotState.Batched);
			}
		}

		public static AccComplianceDocumentHeader[] LoadComplianceDocumentsReadyToBeSent(this AccEInvoicingBatch batch)
		{
			if (AccountingConfigurationRegistry.Instance.AllowSendingEInvoicingBatchWithError.Value)
			{
				return batch.GetComplianceDocumentsWithStatus(EInvoicingPivotState.Batched, EInvoicingPivotState.BatchedWithError);
			}
			else
			{
				return batch.GetComplianceDocumentsWithStatus(EInvoicingPivotState.Batched);
			}
		}

		#endregion

	}
}
