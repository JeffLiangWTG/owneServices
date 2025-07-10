using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IN.Business;

public class SWConstituentLookups : CusSupportingInfoLookups
{
	public SWConstituentLookups(AutoCusSupportingInfo parent) : base(parent)
	{
	}

	public override CodeDescriptionPairList StatusList => Factory.GetCachedValue<Universal.CodeDescriptionPairLists.YesNoList>();
}
