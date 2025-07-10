using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.MasterFiles
{
	public class RegionOrgImpAddInfoLookups : EUOrgImpAddInfoLookups
	{
		public RegionOrgImpAddInfoLookups(AutoEUOrgImpAddInfo parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList DefermentMethodList => Factory.GetCachedValue<MethodOfPaymentList>();
	}
}
