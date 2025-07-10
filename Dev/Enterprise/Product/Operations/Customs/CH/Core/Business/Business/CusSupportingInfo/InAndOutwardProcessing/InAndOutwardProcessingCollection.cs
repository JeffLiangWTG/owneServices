using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public class InAndOutwardProcessingCollection : SingleCusSupportingInfoCollection<InAndOutwardProcessing>
{
	public InAndOutwardProcessingCollection(BusinessObject parent) : base(parent, Common.CH.CusSupportingInfoTypeList.Codes.InAndOutwardProcessing)
	{
	}
}
