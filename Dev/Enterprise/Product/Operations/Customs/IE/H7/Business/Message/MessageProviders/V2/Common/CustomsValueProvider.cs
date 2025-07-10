using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.H7.Business
{
	public class CustomsValueProvider : IMoney
	{
		public CustomsValueProvider(AsycudaPackedItem packedItem)
		{
			this.packedItem = packedItem;
		}

		readonly AsycudaPackedItem packedItem;

		public decimal Amount => packedItem.API_GoodsValue;

		public string Currency => packedItem.API_RX_NKGoodsValueCurrency;
	}
}
