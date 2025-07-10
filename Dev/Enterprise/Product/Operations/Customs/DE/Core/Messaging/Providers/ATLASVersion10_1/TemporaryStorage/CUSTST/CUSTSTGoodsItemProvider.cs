using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1
{
	public class CUSTSTGoodsItemProvider : ICUSTSTGoodsItem
	{
		public CUSTSTGoodsItemProvider(SCTSTJGoodsItem goodsItem)
		{
			this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		}
		readonly SCTSTJGoodsItem goodsItem;

		public ZString SequenceNumber => goodsItem.SequenceNumber;

		public ZString CustodianReferenceNumber => goodsItem.Custodian?.Identification?.ReferenceNumber;

		public ZString CustodianSubsidiaryNumber => goodsItem.Custodian?.Identification?.SubsidiaryNumber;

		public ZString CustodianName => goodsItem.Custodian?.Name;

		public IUnderCustomsControlGoodsItemAddress CustodianAddress => CachedValueHelper.GetValue(ref custodianAddressCached, () =>
		{
			IUnderCustomsControlGoodsItemAddress result = null;
			var address = goodsItem.Custodian?.Address;
			if (address != null)
			{
				result = new CUSTSTGoodsItemCustodianAddressProvider(address);
			}

			return result;
		});
		CachedValue<IUnderCustomsControlGoodsItemAddress> custodianAddressCached;

		public ZString CustodyPlaceCode => goodsItem.CustodyPlaceCode;

		public ZString CustodyPlaceInformation => goodsItem.CustodyPlace?.Information;

		public IUnderCustomsControlGoodsItemAddress CustodyPlaceAddress => CachedValueHelper.GetValue(ref custodyPlaceAddressCached, () =>
		{
			IUnderCustomsControlGoodsItemAddress result = null;
			var address = goodsItem.CustodyPlace?.Address;
			if (address != null)
			{
				result = new CUSTSTGoodsItemCustodyPlaceAddressProvider(address);
			}

			return result;
		});

		CachedValue<IUnderCustomsControlGoodsItemAddress> custodyPlaceAddressCached;

		public ZString DisposalEntitledTraderReferenceNumber => goodsItem.DisposalEntitledTrader?.Identification?.ReferenceNumber;

		public ZString DisposalEntitledTraderSubsidiaryNumber => goodsItem.DisposalEntitledTrader?.Identification?.SubsidiaryNumber;

		public ZString OwnerReferenceType => goodsItem.ClassificationKeyKind;

		public ZString OwnerReferenceNumber => goodsItem.ClassificationKeyNumber;

		public ZString LocationOfGoods => goodsItem.CustodyPlaceCode;

		public ZString GoodsDescription => goodsItem.GoodsDescription;

		public ZString PackageType => goodsItem.Package.Kind;

		public ZInt PackageQty => ZInt.ParseSafe(goodsItem.Package.Quantity, ZInt.Zero);

		public ZDecimal GrossWeight => goodsItem.GrossMassMeasure;

		public ZDate LimitDate => goodsItem.DeclarationLimitDateSpecified ? new ZDate(goodsItem.DeclarationLimitDate) : ZDate.Empty;

		public ZString CustomsGoodsStatus => goodsItem.CustomsGoodsStatus;
	}
}
