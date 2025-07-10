using CargoWise.Common;
using Enterprise.Customs.ManifestBase;
using static Enterprise.Integration.Customs.ManifestBase;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class CommodityCodeWrapper : CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces.ICommodityCode
	{
		CommodityCodeWrapper(AsycudaPackedItem item)
		{
			this.item = Argument.NotNull(item, nameof(item));
		}

		readonly IAsycudaPackedItem item;

		public string CombinedNomenclatureCode => combinedNomenclatureCode ?? (combinedNomenclatureCode = item.API_Tariff.SubstringSafe(6, 2));
		string combinedNomenclatureCode;

		public string HarmonizedSystemSubHeadingCode => harmonizedSystemSubHeadingCode ?? (harmonizedSystemSubHeadingCode = item.API_Tariff.Left(6));
		string harmonizedSystemSubHeadingCode;

		public static CommodityCodeWrapper New(AsycudaPackedItem item) => item == null ? null : new CommodityCodeWrapper(item);
	}
}
