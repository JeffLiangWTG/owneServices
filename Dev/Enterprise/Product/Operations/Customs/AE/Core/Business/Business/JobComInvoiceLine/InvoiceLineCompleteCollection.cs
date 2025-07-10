namespace Enterprise.Customs.AE.Business;

public class InvoiceLineCompleteCollection : Customs.Business.InvoiceLineCompleteCollection
{
	public InvoiceLineCompleteCollection(JobDeclaration declaration)
		: base(declaration)
	{
	}

	public new JobComInvoiceLine this[int index]
	{
		get { return (JobComInvoiceLine)base[index]; }
	}

	public new JobComInvoiceLine AddNew()
	{
		return (JobComInvoiceLine)base.AddNew();
	}
}
