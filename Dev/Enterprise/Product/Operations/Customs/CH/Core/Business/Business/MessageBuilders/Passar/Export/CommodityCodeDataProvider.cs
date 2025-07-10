using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.Business;

public class CommodityCodeDataProvider : ICommodityCode
{
	public static CommodityCodeDataProvider New(CusEntryLine entryLine) => entryLine == null ? null : new CommodityCodeDataProvider(entryLine);

	CommodityCodeDataProvider(CusEntryLine entryLine)
	{
		this.entryLine = entryLine;
	}
	readonly CusEntryLine entryLine;

	public string NationalCustomsTariffNumber => entryLine.CL_AdValoremTariff.SubstringSafe(0, 4) + "." + entryLine.CL_AdValoremTariff.SubstringSafe(4, 4);

	public string ControlCode => entryLine.CL_AdValoremTariff.SubstringSafe(8, 3).PadRight(3, '0');

	public string CombinedNomenclatureCode => null;

	public string CustomsFavourCode => null;

	public decimal? CustomsRate => null;

	public string VatCode => null;
}
