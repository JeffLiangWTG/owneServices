using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Business;

public class InvoiceLineCompleteCollection : TypeSafeInvoiceLineCompleteCollection
{
	public InvoiceLineCompleteCollection(JobDeclaration jobDeclaration)
		: base(jobDeclaration)
	{
	}

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		base.SetDefaultsForNewChild(child);
		if (child is JobComInvoiceLine jobComInvoiceLine && jobComInvoiceLine.IsExport)
		{
			jobComInvoiceLine.JI_ValuationMarkup = 110.00m;
		}
	}
}
