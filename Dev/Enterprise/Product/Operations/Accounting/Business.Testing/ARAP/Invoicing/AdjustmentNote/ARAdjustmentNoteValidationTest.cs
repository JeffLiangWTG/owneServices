using System;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class ARAdjustmentNoteValidationTest : AdjustmentNoteValidationTest
	{
		protected override InvoiceBaseValidation GetValidation(TransactionHeader parent)
		{
			return new AdjustmentNoteValidation((AdjustmentNote)parent);
		}

		protected override Type InvoiceType
		{
			get
			{
				return typeof(ARAdjustmentNote);
			}
		}
	}
}
