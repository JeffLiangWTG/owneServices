using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC043CCommodityProvider
	{
		public CC043CCommodityProvider(CommodityType08 commodity)
		{
			this.commodity = Argument.NotNull(commodity, nameof(commodity));
		}

		readonly CommodityType08 commodity;

		public ZString DescriptionOfGoods => commodity.DescriptionOfGoods ?? ZString.Empty;
		public ZString CusCode => commodity.CusCode ?? ZString.Empty;

		public CC043CGoodsMeasureProvider GoodsMeasure => goodsMeasureCached ?? (goodsMeasureCached = commodity.GoodsMeasure == null ? null : new CC043CGoodsMeasureProvider(commodity.GoodsMeasure));
		CC043CGoodsMeasureProvider goodsMeasureCached;

		public CC043CCommodityCodeProvider CommodityCode => commodityCodeCached ?? (commodityCodeCached = commodity.CommodityCode == null ? null : new CC043CCommodityCodeProvider(commodity.CommodityCode));

		CC043CCommodityCodeProvider commodityCodeCached;
	}
}
