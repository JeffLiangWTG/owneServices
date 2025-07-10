using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business.Declaration;

public class AddInfoJobComInvoiceHeader : EU.Business.Declaration.AddInfoJobComInvoiceHeader
{
	public AddInfoJobComInvoiceHeader(ZPropertyInfo addInfoPropertyInfo)
		: base(addInfoPropertyInfo)
	{
	}

	public new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

	public new AddInfoJobComInvoiceHeaderValidation Validation => (AddInfoJobComInvoiceHeaderValidation)base.Validation;

	protected override EUAddInfoValidation GetNewValidation() => new AddInfoJobComInvoiceHeaderValidation(this);

	public new AddInfoJobComInvoiceHeaderLookups Lookups => (AddInfoJobComInvoiceHeaderLookups)base.Lookups;

	protected override EUAddInfoLookups GetNewLookups() => new AddInfoJobComInvoiceHeaderLookups(this);
}
