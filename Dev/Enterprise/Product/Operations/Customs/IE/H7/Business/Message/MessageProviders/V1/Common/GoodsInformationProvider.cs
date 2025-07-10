using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class GoodsInformationProvider : IGoodsInformation
	{
		public GoodsInformationProvider(AsycudaPackedItem packedItem)
		{
			this.packedItem = packedItem;
			pack = packedItem.Pack;
		}
		readonly AsycudaPack pack;
		readonly AsycudaPackedItem packedItem;

		public string DescriptionOfGoods => packedItem.API_GoodsDescription;

		public string CommodityCode => packedItem.API_Tariff.Left(6);

		public ISupplementaryUnits SupplementaryUnits => CachedValueHelper.GetValue(ref supplementaryUnitsCached, () => new SupplementaryUnitsProvider(packedItem));
		CachedValue<ISupplementaryUnits> supplementaryUnitsCached;

		public string PackagingNumberOfPackages => pack?.APA_PackQty.ToString() ?? string.Empty;
	}
}
