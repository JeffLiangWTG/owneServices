using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public static class PaymentApprovalItemOSAmountProvider
	{
		public static void SetPaymentAmounts(AccPaymentApprovalItem approvalItem, ZDecimal localAmount, ZDecimal osAmount)
		{
			approvalItem.A2_OSPaymentThisRun = TransactionHeaderOSOutstandingAmountProvider.IsFeatureEnabled(approvalItem.TransactionHeader)
				? osAmount
				: new ZDecimal(0m);

			approvalItem.A2_PaymentThisRun = localAmount;
		}

		public static bool IsPaymentApprovalValidForMatch(PaymentApprovalItem approvalItem)
		{
			return approvalItem.IsInDatabase &&
				approvalItem.Approval != null &&
				approvalItem.Approval.IsInDatabase &&
				!approvalItem.Approval.IsPosted &&
				!approvalItem.Approval.IsRejected &&
				!approvalItem.Approval.IsCancelled;
		}
	}
}
