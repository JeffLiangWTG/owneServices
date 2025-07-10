using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.NCTS.Business;

public sealed class DepartureCusTransportMeansLookups : EU.NCTS.Business.DepartureCusTransportMeansLookups
{
	public DepartureCusTransportMeansLookups(DepartureCusTransportMeans parent)
		: base(parent)
	{
	}

	new DepartureCusTransportMeans Parent => (DepartureCusTransportMeans)base.Parent;

	protected override CodeDescriptionPairList TransportAtBorderTypeOfIdListCore =>
		TransportAtBorderTypeOfIdLookupsHelper.GetCachedTransportAtBorderTypeOfIdList(Parent.MovementHeaderParent);
}
