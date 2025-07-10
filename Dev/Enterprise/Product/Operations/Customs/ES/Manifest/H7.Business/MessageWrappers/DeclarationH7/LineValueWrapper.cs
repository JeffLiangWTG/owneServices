using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Manifest.H7.Business;

public class LineValueWrapper(AsycudaPackedItem item) : IH7Value
{
	public ZDecimal Amount => item.API_GoodsValue.Round(2);
	public ZString CurrencyCode => item.API_RX_NKGoodsValueCurrency;
}
