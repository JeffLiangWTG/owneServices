using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class NFFTAXGoodsItemProvider : INFFTAXGoodsItem
	{
		public NFFTAXGoodsItemProvider(GNTAXKGoodsItem goodsItem)
		{
			this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		}

		public ZString SequenceNumber => goodsItem.SequenceNumber;

		readonly GNTAXKGoodsItem goodsItem;
	}
}
