using CargoWise.Types;

namespace Enterprise.Customs.CN.Business
{
	public interface ICusCommonLine
	{
		ZInt ProductManualNo { get; }
		ZString TariffCode { get; }
		ZDecimal TradeQuantity { get; }
		ZString TradeUnitQty { get; }
		ZDecimal CustomsQuantity { get; }
		ZString CustomsUnitQty { get; }
		ZDecimal CustomsSecondQuantity { get; }
		ZString CustomsSecondUnit { get; }
		ZString GoodsOriginCode { get; }
		ZString GoodsDestCode { get; }
		ZDecimal UnitPrice { get; }
		ZDecimal TotalPrice { get; }
		ZString CurrencyCode { get; }
		ZString DutyModeCode { get; }
		ZString ProductCode { get; }
		ZString ProductVersion { get; }
	}
}
