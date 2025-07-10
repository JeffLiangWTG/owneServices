namespace Enterprise.Customs.GB.Business.Declaration
{
	public class InvoiceApportionChargeLookups
		: EU.Business.Declaration.InvoiceApportionChargeLookups
	{
		public InvoiceApportionChargeLookups(InvoiceApportionCharge invoiceApportionCharge)
			: base(invoiceApportionCharge)
		{
		}

		public new InvoiceApportionCharge Parent => (InvoiceApportionCharge)base.Parent;
	}
}
