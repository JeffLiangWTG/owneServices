using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class CUSRECGoodsItemProvider : ICUSRECGoodsItem
	{
		public CUSRECGoodsItemProvider(GCRECFGoodsItem goodsItem)
		{
			this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		}
		readonly GCRECFGoodsItem goodsItem;

		public ZString SequenceNumber => goodsItem.SequenceNumber;

		public ZString NotificationSeverity => goodsItem.Notification?.Severity;

		public ZString NotificationCode => goodsItem.Notification?.Code;
	}
}
