using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IN.Business;

public class CusContainerLookups : Customs.Business.CusContainerLookups
{
	public CusContainerLookups(CusContainer parent)
		: base(parent)
	{
	}

	public CodeDescriptionPairList SealTypeList => Factory.GetCachedValue<SealTypeList>();
}
