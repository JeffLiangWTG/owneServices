using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class APAdjustmentNoteValidationTest : AdjustmentNoteValidationTest
	{
		protected override Type InvoiceType
		{
			get { return typeof(APAdjustmentNote); }
		}

		protected override InvoiceBaseValidation GetValidation(TransactionHeader parent)
		{
			return parent.Validation as APAdjustmentNoteValidation;
		}

		protected override void AssertCreditOnHoldError(ZPropertyInfo aH_OHInfo)
		{
			AssertNoErrors("There should be no Credit On Hold error for AP", aH_OHInfo);
		}
	}
}
