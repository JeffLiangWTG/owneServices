using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class ECWINFGoodsItemProvider : IECWINFGoodsItem
	{
		public ECWINFGoodsItemProvider(LECWIFCustomsWarehouseGoodsItem goodsItem)
		{
			this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		}
		readonly LECWIFCustomsWarehouseGoodsItem goodsItem;

		public ZString ReferencedRegistrationNumber => goodsItem.ReferencedRegistrationNumber;

		public string MRN => goodsItem.MRN;
	}
}
