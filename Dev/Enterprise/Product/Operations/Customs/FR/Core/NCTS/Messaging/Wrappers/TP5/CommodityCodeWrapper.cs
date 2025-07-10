using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class CommodityCodeWrapper : ICommodityCode
	{
		CommodityCodeWrapper(ZString harmonisedTariff)
		{
			this.harmonisedTariff = Argument.NotNull(harmonisedTariff, nameof(harmonisedTariff));
		}

		readonly ZString harmonisedTariff;

		public static CommodityCodeWrapper New(ZString harmonisedTariff) => new CommodityCodeWrapper(harmonisedTariff);

		public string HarmonizedSystemSubHeadingCode => harmonizedSystemSubHeadingCode ?? (harmonizedSystemSubHeadingCode = harmonisedTariff.Left(6));
		string harmonizedSystemSubHeadingCode;

		public string CombinedNomenclatureCode => combinedNomenclatureCode ?? (combinedNomenclatureCode = harmonisedTariff.SubstringSafe(6, 2));
		string combinedNomenclatureCode;
	}
}
