namespace Enterprise.Customs.CH.Business;

public class InvoiceLineCompleteCollection : Customs.Business.InvoiceLineCompleteCollection
{
	public InvoiceLineCompleteCollection(JobDeclaration jobDeclaration)
		: base(jobDeclaration)
	{
	}

	public new JobComInvoiceLine this[int index] => (JobComInvoiceLine)Elements[index];

	public new JobComInvoiceLine AddNew() => (JobComInvoiceLine)base.AddNew();

	protected new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;
}
