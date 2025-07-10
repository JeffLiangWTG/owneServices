using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration;

public class AddInfoJobComInvoiceLineLookups : EU.Business.Declaration.AddInfoJobComInvoiceLineLookups
{
	public AddInfoJobComInvoiceLineLookups(EU.Business.Declaration.AddInfoJobComInvoiceLine parent)
		: base(parent)
	{
	}

	public new AddInfoJobComInvoiceLine Parent => (AddInfoJobComInvoiceLine)base.Parent;

	public SteelTypeList SteelTypeList => Factory.GetCachedValue<SteelTypeList>();

	public CodeDescriptionPairList PortTaxRateList => UniversalReferenceHelper.GetPortTaxRateList(Factory);
}
