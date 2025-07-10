using CargoWise.Customs.EU.MessageContracts.ICS2;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class PostalChargesProvider : IPostalCharge
	{
		public PostalChargesProvider(decimal value, string currency)
		{
			Value = value;
			Currency = currency;
		}

		public static PostalChargesProvider NewOrNull(AsycudaPackedItem packedItem) => packedItem != null ? new PostalChargesProvider(packedItem.API_GoodsValue, packedItem.API_RX_NKGoodsValueCurrency) : null;

		public decimal Value { get; }

		public string Currency { get; }
	}
}
