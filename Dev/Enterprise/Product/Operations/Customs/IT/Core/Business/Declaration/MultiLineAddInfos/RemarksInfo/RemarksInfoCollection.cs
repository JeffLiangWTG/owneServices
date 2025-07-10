using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class RemarksInfoCollection : CusSupportingInfoCollection<CusSupportingInfo>
{
	public RemarksInfoCollection(JobComInvoiceLine parent) : base(parent, ITCusSupportingInfoTypeList.Codes.Remarks)
	{
	}
}
