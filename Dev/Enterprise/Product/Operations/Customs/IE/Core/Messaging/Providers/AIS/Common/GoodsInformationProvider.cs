using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public sealed class GoodsInformationProvider
	{
		public GoodsInformationProvider(ICustomsValueType customsValueType)
		{
			this.customsValueType = customsValueType;
		}
		readonly ICustomsValueType customsValueType;

		public ZString Currency => customsValueType.Currency;

		public ZDecimal Amount => (ZDecimal)customsValueType.Amount;
	}
}
