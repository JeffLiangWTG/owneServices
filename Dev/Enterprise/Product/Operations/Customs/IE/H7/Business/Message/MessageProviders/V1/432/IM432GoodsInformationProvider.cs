using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class IM432GoodsInformationProvider : IIM432GoodsInformation
	{
		readonly AsycudaPackedItem packedItem;

		public IM432GoodsInformationProvider(AsycudaPackedItem packedItem)
		{
			this.packedItem = packedItem;
		}

		public decimal GrossMassValue => packedItem.Pack?.APA_Weight ?? 0m;

		public int? Packaging => packedItem.Pack?.APA_PackQty;
	}
}
