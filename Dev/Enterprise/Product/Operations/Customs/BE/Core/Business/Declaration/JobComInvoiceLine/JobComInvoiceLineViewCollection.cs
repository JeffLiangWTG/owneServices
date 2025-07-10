using Enterprise.Customs.Business;

namespace Enterprise.Customs.BE.Business.Declaration;

public class JobComInvoiceLineViewCollection : EU.Business.Declaration.JobComInvoiceLineViewCollection
{
	public JobComInvoiceLineViewCollection(JobComInvoiceHeader invoice, InvoiceLineCompleteCollection completeCollection)
		: base(invoice, completeCollection)
	{
	}

	public JobComInvoiceLineViewCollection(JobComInvoiceHeader parent, InvoiceLineDependentCollection completeCollection)
		: base(parent, completeCollection)
	{
	}

	public new JobComInvoiceLine AddNew() => (JobComInvoiceLine)base.AddNew();

	public new JobComInvoiceLine this[int index] => (JobComInvoiceLine)Elements[index];
}
