namespace Enterprise.Customs.CH.Business;

public class InvoiceHeaderActiveCollection : Customs.Business.InvoiceHeaderActiveCollection
{
	public InvoiceHeaderActiveCollection(JobDeclaration declaration)
		: base(declaration)
	{
	}

	public InvoiceHeaderActiveCollection(JobComInvoiceGroupHeader groupInvoice, bool isDirectRelationship)
		: base(groupInvoice, isDirectRelationship)
	{
	}

	public InvoiceHeaderActiveCollection(Bill bill)
		: base(bill)
	{
	}

	public new JobComInvoiceHeader AddNew() => (JobComInvoiceHeader)base.AddNew();

	public new JobComInvoiceHeader this[int index] => (JobComInvoiceHeader)(base[index]);
}
