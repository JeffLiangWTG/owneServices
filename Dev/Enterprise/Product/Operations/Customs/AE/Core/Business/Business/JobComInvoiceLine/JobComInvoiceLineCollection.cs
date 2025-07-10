namespace Enterprise.Customs.AE.Business;

public class JobComInvoiceLineViewCollection : Customs.Business.BaseJobComInvoiceLineViewCollection
{
	public JobComInvoiceLineViewCollection(JobComInvoiceHeader invoice, InvoiceLineCompleteCollection allInvoiceLines)
		: base(invoice, allInvoiceLines)
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
