using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ICUSTSTGoodsItem : IUnderCustomsControlGoodsItem
	{
		ZString CustodianName { get; }

		IUnderCustomsControlGoodsItemAddress CustodianAddress { get; }

		ZString CustodyPlaceCode { get; }

		ZString CustodyPlaceInformation { get; }

		IUnderCustomsControlGoodsItemAddress CustodyPlaceAddress { get; }
	}
}
