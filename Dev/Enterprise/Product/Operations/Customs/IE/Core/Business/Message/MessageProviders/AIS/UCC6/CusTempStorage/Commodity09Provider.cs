using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage
{
	public class Commodity09Provider : ICommodity09
	{
		public Commodity09Provider(AsycudaPackedItem packedItem)
		{
			this.packedItem = Argument.NotNull(packedItem, nameof(packedItem));
		}
		readonly AsycudaPackedItem packedItem;

		public static Commodity09Provider New(AsycudaPackedItem packedItem) => packedItem == null ? null : new Commodity09Provider(packedItem);

		public string DescriptionOfGoods => packedItem.API_GoodsDescription;

		public string CusCode => packedItem.API_ChemicalSubstanceCode;

		public ICommodityCode02 CommodityCode => CachedValueHelper.GetValue(ref commodityCodeCached, () => CommodityCode02Provider.New(packedItem.API_Tariff.Left(6), packedItem.API_Tariff.SubstringSafe(6,2)));
		CachedValue<ICommodityCode02> commodityCodeCached;
	}
}
