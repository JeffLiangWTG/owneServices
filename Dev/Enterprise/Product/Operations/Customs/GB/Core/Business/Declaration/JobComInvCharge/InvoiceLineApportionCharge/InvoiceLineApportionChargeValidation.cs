namespace Enterprise.Customs.GB.Business.Declaration
{
	public class InvoiceLineApportionChargeValidation
		: EU.Business.Declaration.InvoiceLineApportionChargeValidation
	{
		public InvoiceLineApportionChargeValidation(InvoiceLineApportionCharge invoiceLineApportionCharge)
			: base(invoiceLineApportionCharge)
		{
		}

		public new InvoiceLineApportionCharge Parent => (InvoiceLineApportionCharge)base.Parent;
	}
}
