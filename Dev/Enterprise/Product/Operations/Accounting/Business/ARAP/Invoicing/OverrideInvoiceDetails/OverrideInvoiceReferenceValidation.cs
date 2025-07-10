namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class OverrideInvoiceReferenceValidation : InvoiceBaseValidation
	{
		public OverrideInvoiceReferenceValidation(InvoicingBase parent)
			: base(parent)
		{
		}

		protected override void CheckAH_TransactionNum()
		{
			if (Parent.AH_TransactionNumInfo.HasChanges)
			{
				base.CheckAH_TransactionNum();
			}
		}
	}
}
