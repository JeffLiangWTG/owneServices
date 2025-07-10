using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business.Declaration;

public class InvoiceLineViewCollection : EU.Business.Declaration.InvoiceLineViewCollection<JobComInvoiceLine>
{
	public InvoiceLineViewCollection(JobDeclaration jobDeclaration) : base(jobDeclaration)
	{
	}

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		base.SetDefaultsForNewChild(child);
		var invLine = (JobComInvoiceLine)child;
		invLine.SetDefaultValuesForOrigin();
	}
}
