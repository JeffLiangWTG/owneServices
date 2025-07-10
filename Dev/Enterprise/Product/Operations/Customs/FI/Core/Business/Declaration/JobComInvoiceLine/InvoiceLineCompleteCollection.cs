namespace Enterprise.Customs.FI.Business;

public class InvoiceLineCompleteCollection : EU.Business.Declaration.InvoiceLineCompleteCollection
{
	public InvoiceLineCompleteCollection(JobDeclaration jobDeclaration)
		: base(jobDeclaration)
	{
	}

	public new JobComInvoiceLine this[int index] => (JobComInvoiceLine)Elements[index];

	public new JobComInvoiceLine AddNew() => (JobComInvoiceLine)base.AddNew();
}
