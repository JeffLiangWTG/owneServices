using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business.Declaration;

public class InvoiceLineDependentCollection : EU.Business.Declaration.InvoiceLineDependentCollection
{
	public InvoiceLineDependentCollection(JobComInvoiceHeader invoice) : base(invoice)
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
