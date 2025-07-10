using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class CUSSTAGoodsItemProvider : ICUSSTAGoodsItem
	{
		public CUSSTAGoodsItemProvider(SCSTABSummaryDeclarationGoodsItem goodsItem)
		{
			this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		}

		public ZString ReferencedRegistrationNumber => goodsItem.IdentificationByRegistration?.ReferencedRegistrationNumber;

		public ZString ReferencedSequenceNumber => goodsItem.IdentificationByRegistration?.ReferencedSequenceNumber;

		public string MRN => goodsItem.IdentificationByRegistration?.MRN;

		public ZInt Quantity => ZInt.ParseSafe(goodsItem.Quantity, 0);

		public ZBool CancellationFlag => goodsItem.CancellationFlag == "J";

		readonly SCSTABSummaryDeclarationGoodsItem goodsItem;
	}
}
