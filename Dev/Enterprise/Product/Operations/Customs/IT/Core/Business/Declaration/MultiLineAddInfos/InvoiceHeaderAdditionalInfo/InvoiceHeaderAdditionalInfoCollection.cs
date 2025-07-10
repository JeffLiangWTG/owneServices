namespace Enterprise.Customs.IT.Business.Declaration;

public class InvoiceHeaderAdditionalInfoCollection : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoCollection
{
	public InvoiceHeaderAdditionalInfoCollection(JobComInvoiceHeader parent) : base(parent)
	{
	}

	public new InvoiceHeaderAdditionalInfo this[int i] => (InvoiceHeaderAdditionalInfo)base[i];

	public new InvoiceHeaderAdditionalInfo AddNew() => (InvoiceHeaderAdditionalInfo)base.AddNew();
}
