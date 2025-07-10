using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ICUSSTAGoodsItem : IInboundProvider
	{
		ZString ReferencedRegistrationNumber { get; }
		ZString ReferencedSequenceNumber { get; }
		string MRN { get; }
		ZInt Quantity { get; }
		ZBool CancellationFlag { get; }
	}
}
