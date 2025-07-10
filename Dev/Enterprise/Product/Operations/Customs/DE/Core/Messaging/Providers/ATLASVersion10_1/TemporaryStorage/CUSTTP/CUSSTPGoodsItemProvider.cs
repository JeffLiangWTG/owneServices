using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class CUSSTPGoodsItemProvider : ICUSSTPGoodsItem
	{
		public CUSSTPGoodsItemProvider(SCSTPCGoodsItem goodsItem)
		{
			this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		}
		readonly SCSTPCGoodsItem goodsItem;

		public ZString SequenceNumber => goodsItem.CustomsIntervention.SequenceNumber;

		public ZString CustomsGoodsStatus => goodsItem.CustomsIntervention.Code;
	}
}
