using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Module
{
	public class LPCOFilterLookups : CommonFilterLookups
	{
		public LPCOFilterLookups(LPCOFilterStripBusinessObject filterBizObj) : base(filterBizObj)
		{
		}

		protected override CodeDescriptionPairList GetMainMessageStatusList() => Factory.GetCachedValue<BRMessageStatusList>();
	}
}
