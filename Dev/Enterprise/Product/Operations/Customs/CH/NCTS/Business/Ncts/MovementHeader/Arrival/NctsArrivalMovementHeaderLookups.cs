using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsArrivalMovementHeaderLookups : EU.NCTS.Business.NctsArrivalMovementHeaderLookups
{
	public NctsArrivalMovementHeaderLookups(NctsArrivalMovementHeader parent) : base(parent)
	{
	}

	public CodeDescriptionPairList TransportAtArrivalTypeList => NctsLookupsHelper.TransportTypeOfIdList(Factory);

	public ZZRefCusCodeListCombinedCollection NationalityList => NctsLookupsHelper.NationalityList(Factory);

	protected override CodeDescriptionPairList NctsTransitStatusListCore => Factory.GetCachedValue<NCTS5ArrivalCustomsStatusList>();
}
