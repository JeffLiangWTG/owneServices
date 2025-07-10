using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class CUSFSTGoodsItemProvider : IUnderCustomsControlGoodsItem
	{
		public CUSFSTGoodsItemProvider(SCFSTFGoodsItem goodsItem)
		{
			this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		}
		readonly SCFSTFGoodsItem goodsItem;

		public ZString SequenceNumber => goodsItem.SequenceNumber;

		public ZString CustodianReferenceNumber => goodsItem.Storer?.Identification?.ReferenceNumber;

		public ZString CustodianSubsidiaryNumber => goodsItem.Storer?.Identification?.SubsidiaryNumber;

		public ZString DisposalEntitledTraderReferenceNumber => goodsItem.DisposalEntitledTrader?.Identification?.ReferenceNumber;

		public ZString DisposalEntitledTraderSubsidiaryNumber => goodsItem.DisposalEntitledTrader?.Identification?.SubsidiaryNumber;

		public ZString OwnerReferenceType => goodsItem.ClassificationKeyKind;

		public ZString OwnerReferenceNumber => goodsItem.ClassificationKeyNumber;

		public ZString LocationOfGoods => goodsItem.StoragePlaceCode;

		public ZString GoodsDescription => goodsItem.GoodsDescription;

		public ZString PackageType => goodsItem.Package.Kind;

		public ZInt PackageQty => ZInt.ParseSafe(goodsItem.Package.Quantity, ZInt.Zero);

		public ZDecimal GrossWeight => goodsItem.GrossMassMeasure;

		public ZString CustomsGoodsStatus => goodsItem.CustomsGoodsStatus;

		public ZDate LimitDate => ZDate.Empty;
	}
}
