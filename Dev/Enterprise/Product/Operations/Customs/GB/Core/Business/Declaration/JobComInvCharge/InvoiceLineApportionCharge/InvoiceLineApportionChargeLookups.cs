namespace Enterprise.Customs.GB.Business.Declaration
{
	public class InvoiceLineApportionChargeLookups
		: EU.Business.Declaration.InvoiceLineApportionChargeLookups
	{
		public InvoiceLineApportionChargeLookups(InvoiceLineApportionCharge invoiceLineApportionCharge)
			: base(invoiceLineApportionCharge)
		{
		}

		public new InvoiceLineApportionCharge Parent => (InvoiceLineApportionCharge)base.Parent;
	}
}
