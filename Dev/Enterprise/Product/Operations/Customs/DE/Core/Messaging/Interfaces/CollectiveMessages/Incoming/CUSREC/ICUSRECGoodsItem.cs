using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ICUSRECGoodsItem : IInboundProvider
	{
		ZString SequenceNumber { get; }

		ZString NotificationSeverity { get; }

		ZString NotificationCode { get; }
	}
}
