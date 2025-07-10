using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Types;

namespace Enterprise.Customs.CH.NCTS.Business;

public class CommodityCodeDataProvider : ICommodityCode
{
	public static CommodityCodeDataProvider New(ZString harmonisedTariff) => harmonisedTariff == ZString.Empty ? null :  new CommodityCodeDataProvider(harmonisedTariff);

	CommodityCodeDataProvider(ZString harmonisedTariff)
	{
		this.harmonisedTariff = harmonisedTariff;
	}
	readonly ZString harmonisedTariff;

	public string NationalCustomsTariffNumber => harmonisedTariff.SubstringSafe(0, 4) + "." + harmonisedTariff.SubstringSafe(4, 4);

	public string ControlCode => harmonisedTariff.Length > 8 ? (string)harmonisedTariff.SubstringSafe(8, 3) : null;

	public string CombinedNomenclatureCode => null;

	public string CustomsFavourCode => null;

	public decimal? CustomsRate => null;

	public string VatCode => null;
}
