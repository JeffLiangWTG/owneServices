using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class GoodsItemDifferencesDetailsCollection : SingleCusCodeDataCollection<GoodsItemDifferencesDetails>
{
	public GoodsItemDifferencesDetailsCollection(NctsArrivalCargoDesc parent) : base(parent, CusCodeDataTypeList.Codes.UnloadingRemarks)
	{
	}
}
