using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business.Declaration;

public class JobComInvoiceLineViewCollection : EU.Business.Declaration.JobComInvoiceLineViewCollection
{
	public JobComInvoiceLineViewCollection(JobComInvoiceHeader invoice, InvoiceLineCompleteCollection completeCollection) : base(invoice, completeCollection)
	{
	}

	public JobComInvoiceLineViewCollection(JobComInvoiceHeader parent, InvoiceLineDependentCollection completeCollection)
		: base(parent, completeCollection)
	{
	}

	public new JobComInvoiceLine AddNew()
	{
		return (JobComInvoiceLine)base.AddNew();
	}

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		base.SetDefaultsForNewChild(child);
		var invLine = (JobComInvoiceLine)child;
		invLine.SetDefaultValuesForOrigin();
	}

	public new JobComInvoiceLine this[int index] => (JobComInvoiceLine)Elements[index];
}
