namespace Enterprise.Customs.FR.Business.Declaration
{
	public class InvoiceLineChargeLookups : EU.Business.Declaration.InvoiceLineChargeLookups
	{
		public InvoiceLineChargeLookups(EU.Business.Declaration.InvoiceLineCharge parent) : base(parent)
		{
		}

		protected new InvoiceLineCharge Parent => (InvoiceLineCharge)base.Parent;
	}
}
