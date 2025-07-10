namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	class UnapprovedCreditNoteCandidateValidation : APCreditNoteValidation
	{
		public UnapprovedCreditNoteCandidateValidation(APCreditNote parent)
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
