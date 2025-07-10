
namespace Enterprise.Customs.ES.NCTS.Business
{
	class GoodsItemDutyCalculator : EU.NCTS.Business.GoodsItemDutyCalculator
	{
		public GoodsItemDutyCalculator(EU.NCTS.Business.NctsDepartureCargoDesc goodsItem) : base(goodsItem)
		{
		}

		protected new NctsDepartureCargoDesc GoodsItem => (NctsDepartureCargoDesc)base.GoodsItem;

		protected override EU.NCTS.Business.GoodsItemRateCalcData GetGoodsItemRateCalcData() => new GoodsItemRateCalcData(GoodsItem);
	}
}
