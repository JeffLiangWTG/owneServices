using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class CUSFINGoodsItemProvider : ICUSFINGoodsItem
	{
		public CUSFINGoodsItemProvider(SCFINGSummaryDeclarationGoodsItem goodsItem)
		{
			this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		}
		readonly SCFINGSummaryDeclarationGoodsItem goodsItem;

		public ZString ReferencedRegistrationNumber => goodsItem.IdentificationByRegistration.ReferencedRegistrationNumber;

		public string MRN => goodsItem.IdentificationByRegistration.MRN;

		public ZString ReferencedSequenceNumber => goodsItem.IdentificationByRegistration.ReferencedSequenceNumber;

		public ZInt Quantity => ZInt.ParseSafe(goodsItem.Quantity, ZInt.Zero);

		public ZString CancellationFlag => goodsItem.CancellationFlag;
	}
}
