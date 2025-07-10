namespace Enterprise.Customs.GB.Business.Declaration
{
	public class InvoiceApportionChargeValidation
		: EU.Business.Declaration.InvoiceApportionChargeValidation
	{
		public InvoiceApportionChargeValidation(InvoiceApportionCharge invoiceApportionCharge)
			: base(invoiceApportionCharge)
		{
		}

		public new InvoiceApportionCharge Parent => (InvoiceApportionCharge)base.Parent;
	}
}
