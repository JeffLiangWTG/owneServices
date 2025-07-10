using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class UnloadingRemarksCollection : SingleCusCodeDataCollection<UnloadingRemarks>
{
	public UnloadingRemarksCollection(CusSeal parent) : base(parent, CusCodeDataTypeList.Codes.UnloadingRemarks)
	{
	}
}
