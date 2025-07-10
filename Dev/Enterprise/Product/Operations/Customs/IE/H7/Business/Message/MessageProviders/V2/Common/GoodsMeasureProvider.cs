using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.H7.Business
{
	public class GoodsMeasureProvider : IGoodsMeasure
	{
		GoodsMeasureProvider(AsycudaPackedItem packedItem)
		{
			this.packedItem = packedItem;
		}
		readonly AsycudaPackedItem packedItem;

		public static GoodsMeasureProvider NewOrNull(AsycudaPackedItem packItem) => packItem is null ? null : new GoodsMeasureProvider(packItem);

		public decimal GrossMass => packedItem.API_GrossWeight;

		public decimal NetMass => 0m;

		public decimal SupplementaryUnits => packedItem.API_CustomsQty2;
	}
}
