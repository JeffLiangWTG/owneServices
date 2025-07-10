namespace Enterprise.Customs.DK.Business.Declaration
{
	public class AddInfoJobComInvoiceLineValidation : EU.Business.Declaration.AddInfoJobComInvoiceLineValidation
	{
		public AddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent) : base(parent)
		{
		}

		protected new AddInfoJobComInvoiceLine Parent => (AddInfoJobComInvoiceLine)base.Parent;
	}
}
