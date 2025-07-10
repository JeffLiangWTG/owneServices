using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class CURRELGoodsItemProvider : ICURRELGoodsItem
	{
		public CURRELGoodsItemProvider(GCRELGGoodsItem goodsItem)
		{
			this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		}
		readonly GCRELGGoodsItem goodsItem;

		public ZString SequenceNumber => goodsItem.SequenceNumber;

		public ZString AcceptanceFlag => goodsItem.AcceptanceFlag;

		public ZString DirectiveFlag => goodsItem.DirectiveFlag;

		public ZString RejectionFlag => goodsItem.RejectionFlag;

		public ZString IssuingFlag => goodsItem.IssuingFlag;
	}
}
