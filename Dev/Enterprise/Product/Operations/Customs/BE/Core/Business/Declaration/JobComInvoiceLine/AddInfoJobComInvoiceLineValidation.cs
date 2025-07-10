namespace Enterprise.Customs.BE.Business.Declaration;

public class AddInfoJobComInvoiceLineValidation : EU.Business.Declaration.AddInfoJobComInvoiceLineValidation
{
	public AddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent) : base(parent)
	{
	}

	public new AddInfoJobComInvoiceLine Parent => (AddInfoJobComInvoiceLine)base.Parent;
}
