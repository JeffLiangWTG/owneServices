namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class UACreditNoteValidation : APCreditNoteValidation
	{
		public UACreditNoteValidation(UACreditNote parent) : base(parent)
		{
		}

		UACreditNote ParentUACreditNote
		{
			get { return fParentUACreditNote ?? (fParentUACreditNote = (UACreditNote)Parent); }
		}
		UACreditNote fParentUACreditNote;

		#region Overrides

		protected override void ValidateAllCore()
		{
			if (!ParentUACreditNote.IsInDatabase ||
				ParentUACreditNote.RelatedClaim == null || !ParentUACreditNote.RelatedClaim.IsIntercompanyClaim)
			{
				base.ValidateAllCore();
			}
		}

		protected override void CheckAH_TransactionNum()
		{
			if (!ParentUACreditNote.IsRelatedToClaim)
			{
				base.CheckAH_TransactionNum();
			}
		}

		protected override bool ShouldValidateBranchDepartmentCombinationForParentInDatabase
		{
			get
			{
				return true;
			}
		}

		#endregion
	}
}
