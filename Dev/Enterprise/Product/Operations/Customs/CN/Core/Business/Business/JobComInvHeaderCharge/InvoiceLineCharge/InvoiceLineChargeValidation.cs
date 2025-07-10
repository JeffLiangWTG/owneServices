namespace Enterprise.Customs.CN.Business
{
	public class InvoiceLineChargeValidation : Customs.Business.InvoiceLineChargeValidation
	{
		public InvoiceLineChargeValidation(InvoiceLineCharge invoiceLineCharge)
			: base(invoiceLineCharge)
		{
		}

		public new InvoiceLineCharge InvoiceLineCharge => Parent;

		protected new InvoiceLineCharge Parent => (InvoiceLineCharge)base.Parent;
	}
}
