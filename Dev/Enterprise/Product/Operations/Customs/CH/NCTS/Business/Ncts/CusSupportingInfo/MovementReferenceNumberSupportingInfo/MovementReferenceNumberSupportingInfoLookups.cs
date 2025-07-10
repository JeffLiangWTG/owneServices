using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using YesNoList = Enterprise.Customs.Universal.CodeDescriptionPairLists.YesNoList;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class MovementReferenceNumberSupportingInfoLookups : CusSupportingInfoLookups
{
	public MovementReferenceNumberSupportingInfoLookups(MovementReferenceNumberSupportingInfo parent) : base(parent)
	{
	}

	public CodeDescriptionPairList YesNoList => Factory.GetCachedValue<YesNoList>();
}
