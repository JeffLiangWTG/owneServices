using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.NL.Business.Declaration;

public sealed class AddInfoJobComInvoiceHeader : EU.Business.Declaration.AddInfoJobComInvoiceHeader
{
	public AddInfoJobComInvoiceHeader(ZPropertyInfo addInfoPropertyInfo)
		: base(addInfoPropertyInfo)
	{
	}

	public new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

	public new AddInfoJobComInvoiceHeaderLookups Lookups => (AddInfoJobComInvoiceHeaderLookups)base.Lookups;

	protected override EUAddInfoLookups GetNewLookups() => new AddInfoJobComInvoiceHeaderLookups(this);
}
