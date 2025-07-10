using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business;

public class NctsDepartureCargoDescRemarksCollection : CusSupportingInfoCollection<CusSupportingInfo>
{
	public NctsDepartureCargoDescRemarksCollection(NctsDepartureCargoDesc parent) : base(parent, ITCusSupportingInfoTypeList.Codes.Remarks)
	{
	}
}
