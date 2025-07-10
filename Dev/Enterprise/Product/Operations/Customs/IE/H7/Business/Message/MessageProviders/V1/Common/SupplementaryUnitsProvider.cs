using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class SupplementaryUnitsProvider : ISupplementaryUnits
	{
		public SupplementaryUnitsProvider(AsycudaPackedItem packedItem)
		{
			this.packedItem = packedItem;
		}
		readonly AsycudaPackedItem packedItem;

		public decimal? GrossMass => packedItem.API_GrossWeight;

		public decimal? SupplementaryUnits => packedItem.API_CustomsQty2;
	}
}
