using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class CreditNoteLineValidation : InvoicingLineBaseValidation
	{
		public CreditNoteLineValidation(CreditNoteLine parent)
			: base(parent)
		{
		}

		CreditNoteLine CreditNoteLine => (CreditNoteLine)Parent;

		#region Overrides for Check*

		protected override void CheckAL_Desc()
		{
			base.CheckAL_Desc();
			MandatoryValidation.CheckEntered(Parent.AL_DescInfo);
		}

		protected override void CheckAL_AT()
		{
			base.CheckAL_AT();

			if (!Parent.AL_ATInfo.HasErrors() && !Parent.AL_AT.IsEmpty && !CheckIndiaGSTReversalAllowedPeriod())
			{
				Parent.AL_ATInfo.AddError(IndiaGSTReversalHelper.ARCreditingIndiaGSTEightMonthsAfterFinancialYearEndMessage_CreditNoteGST);
			}
		}

		bool CheckIndiaGSTReversalAllowedPeriod()
		{
			if (CreditNoteLine.InvoiceBase != null && CreditNoteLine.InvoiceBase is CreditNote creditNote && creditNote.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				var originalInvoicePostDate = creditNote.OriginalTransactionIsSet
					? creditNote.OriginalReferenceTransaction.AH_PostDate
					: creditNote.AH_OriginalInvoiceDate;
				return
					 (!creditNote.OriginalTransactionIsSet && originalInvoicePostDate.IsEmpty)
					|| Env.Security.AllowCreditingIndiaGSTEightMonthsAfterFinancialYearEnd.IsAllowed
					|| !IndiaGSTReversalHelper.CheckIsConstraintTaxID(CreditNoteLine.TaxRate)
					|| !IndiaGSTReversalHelper.CheckIsExceedIndiaFinancialYearEndDatePeriod(originalInvoicePostDate, creditNote.AH_PostDate);
			}

			return true;
		}

		#endregion
	}
}
