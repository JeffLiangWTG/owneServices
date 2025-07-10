namespace Enterprise.Customs.GB.Business.Declaration
{
	public class InvoiceLineChargeLookups
		: EU.Business.Declaration.InvoiceLineChargeLookups
	{
		public InvoiceLineChargeLookups(InvoiceLineCharge parent)
			: base(parent)
		{
		}

		public new InvoiceLineCharge Parent => (InvoiceLineCharge)base.Parent;
	}
}
