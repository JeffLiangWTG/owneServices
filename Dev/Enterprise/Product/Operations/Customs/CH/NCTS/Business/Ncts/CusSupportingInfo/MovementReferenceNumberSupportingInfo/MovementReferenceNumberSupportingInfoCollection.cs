using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class MovementReferenceNumberSupportingInfoCollection : CusSupportingInfoCollection<MovementReferenceNumberSupportingInfo>
{
	public MovementReferenceNumberSupportingInfoCollection(NctsArrivalMovementHeader parent) : base(parent, Common.CH.CusSupportingInfoTypeList.Codes.MovementReferenceNumber)
	{
	}
}
