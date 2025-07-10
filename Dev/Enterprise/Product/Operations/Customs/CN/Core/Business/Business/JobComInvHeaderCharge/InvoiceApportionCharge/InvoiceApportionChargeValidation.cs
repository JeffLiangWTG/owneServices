namespace Enterprise.Customs.CN.Business
{
	public class InvoiceApportionChargeValidation : Customs.Business.BaseApportionedChargeValidation
	{
		public InvoiceApportionChargeValidation(InvoiceApportionCharge invoiceApportionCharge)
			: base(invoiceApportionCharge)
		{
		}

		public InvoiceApportionCharge InvoiceApportionCharge => Parent;

		protected new InvoiceApportionCharge Parent => (InvoiceApportionCharge)base.Parent;
	}
}
