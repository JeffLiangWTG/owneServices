using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class SupernumeraryGoodsCollection : CusSupportingInfoCollection<SupernumeraryGoods>
{
	public SupernumeraryGoodsCollection(NctsArrivalMovementHeader parent) : base(parent, Common.CH.CusSupportingInfoTypeList.Codes.SupernumeraryGoods)
	{
	}
}
