using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ICUSFINGoodsItem : IInboundProvider
	{
		ZString ReferencedRegistrationNumber { get; }
		string MRN {  get; }
		ZString ReferencedSequenceNumber { get; }
		ZInt Quantity { get; }
		ZString CancellationFlag { get; }
	}
}
