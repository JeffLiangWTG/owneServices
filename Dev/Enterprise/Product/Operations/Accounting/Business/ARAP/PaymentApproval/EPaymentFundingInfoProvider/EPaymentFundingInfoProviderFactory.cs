namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public static class EPaymentFundingInfoProviderFactory
	{
		public static IEPaymentFundingInfoProvider CreateProvider(PaymentApprovalBase paymentApproval)
		{
			if (paymentApproval?.AV_AB_FundingBankAccount.IsEmpty == false || paymentApproval?.ParentPaymentBatch == null)
			{
				return new PaymentApprovalFundingInfoProvider(paymentApproval);
			}

			return new PaymentBatchFundingInfoProvider(paymentApproval.ParentPaymentBatch);
		}

		public static IEPaymentFundingInfoProvider CreateProvider(AccPaymentBatch paymentBatch)
		{
			return new PaymentBatchFundingInfoProvider(paymentBatch);
		}
	}
}
