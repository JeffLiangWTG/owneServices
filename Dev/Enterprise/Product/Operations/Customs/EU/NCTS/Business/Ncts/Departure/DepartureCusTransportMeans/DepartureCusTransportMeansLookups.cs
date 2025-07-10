using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class DepartureCusTransportMeansLookups : Customs.Business.CusTransportMeansLookups
	{
		public DepartureCusTransportMeansLookups(DepartureCusTransportMeans parent)
			: base(parent)
		{
		}

		protected new DepartureCusTransportMeans Parent => (DepartureCusTransportMeans)base.Parent;

		public CodeDescriptionPairList BorderModeOfTransportList => Factory.GetCachedValue<ModeOfTransportList>();

		public CodeDescriptionPairList TransportAtBorderTypeOfIdList => TransportAtBorderTypeOfIdListCore;

		protected virtual CodeDescriptionPairList TransportAtBorderTypeOfIdListCore => Factory.GetCachedValue("EU.NCTS.TransportAtBorderTypeOfIdList", () =>
		{
			var list = new NctsTransportTypeOfIdList();
			list.RemoveCode(NctsTransportTypeOfIdList.Codes._20);
			list.RemoveCode(NctsTransportTypeOfIdList.Codes._31);
			return list;
		});

		public CodeDescriptionPairList OfficeCodeList => Parent.MovementHeaderParent.Lookups.OfficeCodeList;

		public ZZRefCusCodeListCombinedCollection TransportNationalityList => Factory.GetNCNATCountryList();
	}
}
