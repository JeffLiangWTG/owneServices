using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public class InvoiceLineViewCollection : Customs.Business.InvoiceLineViewCollection<JobComInvoiceLine>
{
	public InvoiceLineViewCollection(JobDeclaration jobDeclaration)
		: base(jobDeclaration)
	{
	}

	protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected override void OnAdded(BusinessObject bizOAdded)
	{
		base.OnAdded(bizOAdded);

		var entryInstruction = Declaration.AddDefaultEntryInstructionIfRequired();
		if ((entryInstruction?.IsPersistent ?? false) && bizOAdded is JobComInvoiceLine invoiceLine && invoiceLine.JI_CEI.IsEmpty)
		{
			invoiceLine.JI_CEI = entryInstruction.PK;
		}
	}
}
