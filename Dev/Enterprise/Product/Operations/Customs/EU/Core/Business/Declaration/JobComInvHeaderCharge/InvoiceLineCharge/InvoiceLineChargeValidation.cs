namespace Enterprise.Customs.EU.Business.Declaration
{
	public class InvoiceLineChargeValidation : Customs.Business.InvoiceLineChargeValidation
	{
		public InvoiceLineChargeValidation(InvoiceLineCharge invoiceLineCharge) : base(invoiceLineCharge)
		{
		}

		public new InvoiceLineCharge Parent => (InvoiceLineCharge)base.Parent;
	}
}
