using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface INFFTAXGoodsItem : IInboundProvider
	{
		ZString SequenceNumber { get; }
	}
}
