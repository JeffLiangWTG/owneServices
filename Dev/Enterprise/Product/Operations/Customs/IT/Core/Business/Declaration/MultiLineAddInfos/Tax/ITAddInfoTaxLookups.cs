using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration;

public class ITAddInfoTaxLookups : EUAddInfoTaxLookups
{
	public ITAddInfoTaxLookups(AutoEUAddInfoTax parent) : base(parent)
	{
	}

	public new Tax_OnlyForPivot Parent => (Tax_OnlyForPivot)base.Parent;
	public CodeDescriptionPairList PortTaxRateList => UniversalReferenceHelper.GetPortTaxRateList(Factory);
}
