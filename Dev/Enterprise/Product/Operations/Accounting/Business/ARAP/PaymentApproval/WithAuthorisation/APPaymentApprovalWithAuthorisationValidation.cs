namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public class APPaymentApprovalWithAuthorisationValidation : PaymentApprovalValidation
	{
		public APPaymentApprovalWithAuthorisationValidation(APPaymentApprovalWithAuthorisation parent) : base(parent)
		{
			Parent = parent;
		}

		protected override void CheckAV_AB_FundingBankAccount()
		{
			base.CheckAV_AB_FundingBankAccount();

			if (Parent.ParentPaymentBatch != null &&
				Parent.ParentPaymentBatch.APB_AB_FundingBankAccount != Parent.FundingBankAccountPK)
			{
				var errorMsg = ResString.GetMultilingualString("cdaf8cc6-888d-4054-9347-646ce73dea2a", @"This payment belongs to Payment Batch {0}.
Funding Bank Account needs to be changed for the batch in the Manage > Payables > Payment Batches module.", Parent.PaymentBatch.APB_BatchNumber);
				Parent.AV_AB_FundingBankAccountInfo.AddError(errorMsg);
			}
		}

		readonly new APPaymentApprovalWithAuthorisation Parent;
	}
}
