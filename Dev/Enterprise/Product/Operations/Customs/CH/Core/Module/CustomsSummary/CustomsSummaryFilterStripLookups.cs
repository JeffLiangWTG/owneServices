using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Module;
public sealed class CustomsSummaryFilterStripLookups : CommonFilterLookups
{
	public CustomsSummaryFilterStripLookups(CustomsSummaryFilterStripBusinessObject filterBizObj) : base(filterBizObj)
	{
	}

	public CodeDescriptionPairList BordereauReceivedStatusList => Factory.GetCachedValue<BordereauReceivedStatusList>();

	public CodeDescriptionPairList BordereauChargeTypeList => Factory.GetCachedValue<BordereauChargeTypeList>();
}
