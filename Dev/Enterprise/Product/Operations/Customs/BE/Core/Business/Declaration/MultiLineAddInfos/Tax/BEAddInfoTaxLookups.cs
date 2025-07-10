using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.BE.Business.Declaration;

public class BEAddInfoTaxLookups : EUAddInfoTaxLookups
{
	public BEAddInfoTaxLookups(Tax_OnlyForPivot parent) : base(parent)
	{
	}

	public new Tax_OnlyForPivot Parent => (Tax_OnlyForPivot)base.Parent;

	protected override TaxLookupsCommon GetNewCommonLookupsHelper() => new BETaxLookupsCommon(Parent);
}
