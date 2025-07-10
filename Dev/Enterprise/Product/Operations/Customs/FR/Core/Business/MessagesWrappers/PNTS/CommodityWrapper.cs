using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS
{
	public class CommodityWrapper : ICommodity
	{
		CommodityWrapper(TemporaryStoragePackedItem item)
		{
			this.item = Argument.NotNull(item, nameof(item));
		}

		readonly AsycudaPackedItem item;

		public ICommodityCode CommodityCode => commodityCode ?? (commodityCode = CommodityCodeWrapper.New(item));
		ICommodityCode commodityCode;

		public string CusCode => cusCode ?? (cusCode = item.API_ChemicalSubstanceCode);
		string cusCode;

		public string DescriptionOfGoods => descriptionOfGoods ?? (descriptionOfGoods = item.API_GoodsDescription);
		string descriptionOfGoods;

		public static CommodityWrapper New(TemporaryStoragePackedItem item) => item == null ? null : new CommodityWrapper(item);
	}
}
