using Enterprise.Environment;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class APAdjustmentNoteValidation : AdjustmentNoteValidation
	{
		public APAdjustmentNoteValidation(APAdjustmentNote parent)
			: base(parent)
		{
		}

		protected override void CheckValidateExpectedInvoiceTotal()
		{
			if (!Env.Security.AllowAPAdjustNoteChangeDefaultExpectedTotalValue.IsAllowed)
			{
				CheckValidateExpectedInvoiceTotalCore();
			}
		}
	}
}
