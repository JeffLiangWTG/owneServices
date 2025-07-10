using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class CommodityCodeProvider : ICommodityCode
	{
		CommodityCodeProvider(ZString tariff)
		{
			HarmonizedSystemSubHeadingCode = tariff.SubstringSafe(0, 6);
			CombinedNomenclatureCode = tariff.SubstringSafe(6, 2);
		}

		public static CommodityCodeProvider NewOrNull(ZString tariff) => !tariff.IsEmpty ? new CommodityCodeProvider(tariff) : null;

		public string HarmonizedSystemSubHeadingCode { get; }

		public string CombinedNomenclatureCode { get; }
	}
}
