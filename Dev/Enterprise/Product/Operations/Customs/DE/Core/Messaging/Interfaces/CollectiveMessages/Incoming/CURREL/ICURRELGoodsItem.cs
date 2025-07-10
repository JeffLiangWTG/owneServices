using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ICURRELGoodsItem : IInboundProvider
	{
		ZString SequenceNumber { get; }

		ZString AcceptanceFlag { get; }

		ZString DirectiveFlag { get; }

		ZString RejectionFlag { get; }

		ZString IssuingFlag { get; }
	}
}
