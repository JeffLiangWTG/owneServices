
using System.Collections.Generic;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.NCTS.Business
{
	class GoodsItemRateCalcData : EU.NCTS.Business.GoodsItemRateCalcData
	{
		public GoodsItemRateCalcData(EU.NCTS.Business.NctsDepartureCargoDesc goodsItem) : base(goodsItem)
		{
		}

		protected new NctsDepartureCargoDesc GoodsItem => (NctsDepartureCargoDesc)base.GoodsItem;

		protected override IDictionary<string, decimal> CountrySpecificValueListCore => new Dictionary<string, decimal>() { { UniversalReferenceConstants.ReservedRateFormulaValue.RetailPriceCode, GoodsItem.PVPValue } };
	}
}
