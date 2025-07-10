using CargoWise.EntityFramework;

namespace Enterprise.Customs.NL.Business.Declaration;

public class CusLinkPackageCollection : Customs.Business.BaseCusLinkPackageCollection
{
	public CusLinkPackageCollection(JobComInvoiceLine invoiceLine) : base(invoiceLine)
	{
	}

	public new CusLinkPackage this[int index] => (CusLinkPackage)base[index];

	public new CusLinkPackage AddNew() => (CusLinkPackage)base.AddNew();

	protected override BusinessObject CreateNonPersistentBusinessObject() => new CusLinkPackage((JobComInvoiceLine)Supporter);
}
