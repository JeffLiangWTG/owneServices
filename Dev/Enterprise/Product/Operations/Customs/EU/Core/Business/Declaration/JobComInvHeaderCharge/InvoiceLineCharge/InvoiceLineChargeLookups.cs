namespace Enterprise.Customs.EU.Business.Declaration
{
	public class InvoiceLineChargeLookups : Customs.Business.JobComInvHeaderChargeLookups
	{
		public InvoiceLineChargeLookups(InvoiceLineCharge parent) : base(parent)
		{
		}

		public new InvoiceLineCharge Parent => (InvoiceLineCharge)base.Parent;
	}
}
