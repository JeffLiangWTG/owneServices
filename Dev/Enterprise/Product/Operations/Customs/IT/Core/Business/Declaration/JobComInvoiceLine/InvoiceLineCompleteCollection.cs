using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business.Declaration;

public class InvoiceLineCompleteCollection : EU.Business.Declaration.InvoiceLineCompleteCollection
{
	public InvoiceLineCompleteCollection(JobDeclaration jobDeclaration) : base(jobDeclaration)
	{
	}

	public new JobComInvoiceLine this[int index] => (JobComInvoiceLine)Elements[index];

	public new JobComInvoiceLine AddNew() => (JobComInvoiceLine)base.AddNew();

	protected override void OnAdded(BusinessObject bizOAdded)
	{
		base.OnAdded(bizOAdded);
		if (bizOAdded is JobComInvoiceLine invoiceLine)
		{
			invoiceLine.DefaultSupportingDocument01DI();
		}
	}

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		base.SetDefaultsForNewChild(child);
		var invLine = (JobComInvoiceLine)child;
		invLine.SetDefaultValuesForOrigin();
	}
}
