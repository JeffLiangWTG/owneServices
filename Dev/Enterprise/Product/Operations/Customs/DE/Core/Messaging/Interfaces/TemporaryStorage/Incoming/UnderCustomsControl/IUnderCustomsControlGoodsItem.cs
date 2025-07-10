using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface IUnderCustomsControlGoodsItem : IInboundProvider
	{
		ZString SequenceNumber { get; }
		ZString CustodianReferenceNumber { get; }
		ZString CustodianSubsidiaryNumber { get; }
		ZString DisposalEntitledTraderReferenceNumber { get; }
		ZString DisposalEntitledTraderSubsidiaryNumber { get; }
		ZString OwnerReferenceType { get; }
		ZString OwnerReferenceNumber { get; }
		ZString LocationOfGoods { get; }
		ZString GoodsDescription { get; }
		ZString PackageType { get; }
		ZInt PackageQty { get; }
		ZDecimal GrossWeight { get; }
		ZDate LimitDate { get; }
		ZString CustomsGoodsStatus { get; }
	}
}
