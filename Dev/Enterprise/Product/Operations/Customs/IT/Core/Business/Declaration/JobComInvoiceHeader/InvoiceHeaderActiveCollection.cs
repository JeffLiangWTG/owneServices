using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class InvoiceHeaderActiveCollection : EU.Business.Declaration.InvoiceHeaderActiveCollection
{
	public InvoiceHeaderActiveCollection(JobDeclaration declaration) : base(declaration)
	{
	}

	public InvoiceHeaderActiveCollection(JobComInvoiceGroupHeader groupInvoice, bool isDirectRelationship)
		: base(groupInvoice, isDirectRelationship)
	{
	}

	public new JobComInvoiceHeader AddNew()
	{
		return (JobComInvoiceHeader)base.AddNew();
	}

	public new JobComInvoiceHeader this[int index] => (JobComInvoiceHeader)(base[index]);

	protected override void SetDefaultsForNewElementCore(BaseJobComInvoiceHeader newElement)
	{
		base.SetDefaultsForNewElementCore(newElement);

		((JobComInvoiceHeader)newElement).AeoCertificateManager.AddAeoCertificatesIfNeeded();
	}

	protected override Customs.Business.DefaultSetterForInvoiceHeader GetDefaultSetterForInvoiceHeader(BaseJobComInvoiceHeader newElement, BaseJobDeclaration declaration)
	{
		return new DefaultSetterForInvoiceHeader((JobComInvoiceHeader)newElement, (JobDeclaration)declaration);
	}
}
