namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class UnapprovedInvoiceCandidateValidation : APInvoiceValidation
	{
		public UnapprovedInvoiceCandidateValidation(APInvoice parent)
			: base(parent)
		{
		}

		protected override void ValidateAllCore()
		{
			new UnapprovedTransactionValidationHelper(true).CheckSecurityLevelsAndPromptForAuthorisation(Parent);
			base.ValidateAllCore();
		}
	}
}
