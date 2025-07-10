namespace Enterprise.Customs.DK.Business.Declaration
{
	public class AddInfoJobComInvoiceLineLookups : EU.Business.Declaration.AddInfoJobComInvoiceLineLookups
	{
		public AddInfoJobComInvoiceLineLookups(AddInfoJobComInvoiceLine parent) : base(parent)
		{
		}

		protected new AddInfoJobComInvoiceLine Parent => (AddInfoJobComInvoiceLine)base.Parent;
	}
}
