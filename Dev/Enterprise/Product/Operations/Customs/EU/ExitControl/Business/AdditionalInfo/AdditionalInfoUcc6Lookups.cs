using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.ExitControl.Business;

public class AdditionalInfoUcc6Lookups : AdditionalInfoLookups
{
	public AdditionalInfoUcc6Lookups(AdditionalInfo parent)
		: base(parent)
	{
	}

	public override CodeDescriptionPairList StatusList => Factory.GetCachedValue<DiscrepanciesStatusCodeList>();
}
