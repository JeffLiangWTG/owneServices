using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(SupernumeraryGoodsCollection))]
sealed class SupernumeraryGoodsCollectionTest : CusSupportingInfoCollectionTest<SupernumeraryGoods>
{
	protected override CusSupportingInfoCollection<SupernumeraryGoods> GetCusSupportingInfoCollection()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader.ArrivalMovementHeader.SupernumeraryGoods;
	}
}
