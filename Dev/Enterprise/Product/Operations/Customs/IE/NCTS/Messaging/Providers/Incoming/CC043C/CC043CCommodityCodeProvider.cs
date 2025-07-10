using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC043CCommodityCodeProvider
	{
		readonly CommodityCodeType05 commodityCode;

		public CC043CCommodityCodeProvider(CommodityCodeType05 commodityCode)
		{
			this.commodityCode = Argument.NotNull(commodityCode, nameof(commodityCode));
		}

		public ZString HarmonizedSystemSubHeadingCode => commodityCode.HarmonizedSystemSubHeadingCode ?? ZString.Empty;
		public ZString CombinedNomenclatureCode => commodityCode.CombinedNomenclatureCode ?? ZString.Empty;
	}
}
