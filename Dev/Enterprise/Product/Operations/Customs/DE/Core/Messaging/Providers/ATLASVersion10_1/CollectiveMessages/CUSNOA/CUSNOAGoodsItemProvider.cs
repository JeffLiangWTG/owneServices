using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class CUSNOAGoodsItemProvider : ICUSNOAGoodsItem
	{
		public CUSNOAGoodsItemProvider(GCNOADGoodsItem goodsItem)
		{
			this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		}
		readonly GCNOADGoodsItem goodsItem;

		public ZString SequenceNumber => goodsItem.SequenceNumber;

		public ZString DocumentType => goodsItem.Document?.Type ?? ZString.Empty;

		public ZString DocumentReference => goodsItem.Document?.ReferenceNumber ?? ZString.Empty;

		public ZString CancellationWriteOffFlag => goodsItem.Document?.CancellationWriteOffFlag ?? ZString.Empty;
	}
}
