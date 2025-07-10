using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ICUSSTPGoodsItem : IInboundProvider
	{
		ZString SequenceNumber { get; }

		ZString CustomsGoodsStatus { get; }
	}
}
