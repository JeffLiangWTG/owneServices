using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonCommodityCodeWrapper : INCTSCommonCommodityCode
	{
		public NCTS5CommonCommodityCodeWrapper(ZString harmonizedCode, ZString combinedCode)
		{
			HarmonizedSystemSubHeadingCode = harmonizedCode;
			CombinedNomenclatureCode = combinedCode;
		}

		public NCTS5CommonCommodityCodeWrapper(NctsCommonCargoDesc item)
		{
			Argument.NotNull(item, nameof(item));

			var harmonizedLength = 6;
			var combinedLength = 2;

			var tariffCode = item.BY_HarmonisedTariff;
			HarmonizedSystemSubHeadingCode = tariffCode.SubstringSafe(0, tariffCode.Length < harmonizedLength ? tariffCode.Length : harmonizedLength);
			CombinedNomenclatureCode = tariffCode.SubstringSafe(harmonizedLength, combinedLength);
		}

		public ZString HarmonizedSystemSubHeadingCode { get; }

		public ZString CombinedNomenclatureCode { get; }
	}
}
