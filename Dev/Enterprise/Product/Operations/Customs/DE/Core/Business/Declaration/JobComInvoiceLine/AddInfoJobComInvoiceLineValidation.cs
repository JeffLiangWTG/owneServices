namespace Enterprise.Customs.DE.Business.Declaration
{
	public class AddInfoJobComInvoiceLineValidation : EU.Business.Declaration.AddInfoJobComInvoiceLineValidation
	{
		public AddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent)
			: base(parent)
		{
		}

		protected JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)Parent.Parent;
	}
}
