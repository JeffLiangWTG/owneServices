using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class CUSCANGoodsItemProvider : ICUSCANGoodsItem
	{
		public CUSCANGoodsItemProvider(SCCANEGoodsItem goodsItem)
		{
			this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		}
		readonly SCCANEGoodsItem goodsItem;

		public ZString SequenceNumber => goodsItem.SequenceNumber;

		public ZString CustomsGoodsStatus => goodsItem.CustomsGoodsStatus;
	}
}
