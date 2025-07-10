using System.Globalization;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public static class ElectronicInvoicingHelper
	{
		public static bool TryConvertUTCDateTimeToBranchLocalDateTime(this GlbBranch branch, ZDateTime utcDateTime, out ZDateTime branchLocalDateTime)
		{
			short offsetMinutesFromUtc = 0;
			var result = TryGetBranchOffsetInMinutes(out offsetMinutesFromUtc, branch?.GB_RL_NKHomePort, utcDateTime);
			branchLocalDateTime = result ? utcDateTime.AddMinutes(offsetMinutesFromUtc) : ZDateTime.Empty;
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		static bool TryGetBranchOffsetInMinutes(out short offsetMinutesFromUtc, string unLOCO, ZDateTime utcDateTime)
		{
			offsetMinutesFromUtc = 0;
			var result = false;

			if (!string.IsNullOrEmpty(unLOCO))
			{
				var dbCommand = Db.Connection.Command(string.Format(CultureInfo.InvariantCulture, "SELECT Offset FROM dbo.GetTimeZoneOffsetInMinutes('{0}', '{1}')", unLOCO, utcDateTime.SqlFormat));
				var sqlResult = dbCommand.ExecuteScalar();
				if (sqlResult != null)
				{
					offsetMinutesFromUtc = (short)sqlResult;
					result = true;
				}
			}
			return result;
		}

		public static ZString TaxCorePreComplianceOriginalTransactionReferenceNumber => "XXXXXXXX-XXXXXXXX-1";

		public static bool HasPendingApprovalRequest(this InvoicingBase header) => header.HasPendingRequest(GenApprovalRequestApprovalStatus.ApprovalRequested);

		public static bool HasPendingRejectionRequest(this InvoicingBase header) => header.HasPendingRequest(GenApprovalRequestApprovalStatus.RejectionRequested);

		static bool HasPendingRequest(this InvoicingBase header, string expectedStatus) =>
			header != null
			&& header.AH_Ledger == LedgerTypes.TransactionsPendingAllocation
			&& header.TransactionRelatedApprovalRequest != null
			&& header.TransactionRelatedApprovalRequest.XP_ApprovalStatus == expectedStatus
			&& header.TransactionRelatedApprovalRequest.XP_ApprovalStatusInfo.HasChanges;

		public static bool IsCancellationRequest(this AccTransactionHeader header) =>
			header.IsReversal() && !header.IsAPCreditNote() && !header.IsARInvoice();

		public static bool IsReversal(this AccTransactionHeader header) => header.IsInDatabase
					? header.AH_IsCancelled && header.AH_TransactionBelongsToGroup.IsValid
					: header is TransactionHeader th && th.IsReverseTransaction;

		public static ZQuery GetAIPStatusQuery(ZString aipStatus)
		{
			var query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			var subQuery = new ZDBOnlySubQuery(typeof(AccEInvoicingTransactionPivot), AccEInvoicingTransactionPivotSchema.AIP_ParentID);
			subQuery.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, aipStatus);
			subQuery.AddToFilter(
				new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ActionType, Constants.EInvoicingPivotActionType.Submit)
				.AddToFilter(JoinCondition.Or, AccEInvoicingTransactionPivotSchema.AIP_ActionType, Constants.EInvoicingPivotActionType.Cancel));
			query.AddSubQuery(subQuery, JoinCondition.And);
			return query;
		}

		static bool IsARInvoice(this AccTransactionHeader header) => header.AH_Ledger == LedgerTypes.AccountsReceivable && header.AH_TransactionType == TransactionTypes.Invoice;

		static bool IsAPCreditNote(this AccTransactionHeader header) => header.AH_Ledger == LedgerTypes.AccountsPayable && header.AH_TransactionType == TransactionTypes.CreditNote;

		public static bool IsSubmitSuccess(this AccTransactionHeader header) => header.Factory.Exists(typeof(AccEInvoicingTransactionPivot),
				new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, header.PK)
						.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.Submit)
						.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, EInvoicingPivotState.Succeed));
	}
}
