namespace Enterprise.Customs.ES.Business.Declaration
{
	public class InvoiceChargeLookups : EU.Business.Declaration.InvoiceChargeLookups
	{
		public InvoiceChargeLookups(InvoiceCharge invoiceCharge)
			: base(invoiceCharge)
		{
		}

		public new InvoiceCharge Parent => (InvoiceCharge)base.Parent;
	}
}
