using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface IUnderCustomsControlGoodsItemAddress : IInboundProvider
	{
		ZString Line { get; }

		ZString Country { get; }

		ZString Postcode { get; }

		ZString City { get; }

		ZString District { get; }
	}
}
