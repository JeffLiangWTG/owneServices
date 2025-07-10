using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class OverrideInvoiceDetailValidation : TransactionHeaderValidation
	{
		public OverrideInvoiceDetailValidation(InvoicingBase parent)
			: base(parent)
		{
		}

		protected override void CheckAH_PostDate()
		{
		}
	}
}
