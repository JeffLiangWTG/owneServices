using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class AdditionalTransitOperationCollection : CusSupportingInfoCollection<AdditionalTransitOperation>
{
	public AdditionalTransitOperationCollection(NctsArrivalMovementHeader parent) : base(parent, Common.CH.CusSupportingInfoTypeList.Codes.AdditionalTransitOperation)
	{
	}
}
