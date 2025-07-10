using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ICUSNOAGoodsItem : IInboundProvider
	{
		ZString SequenceNumber { get; }

		ZString DocumentType { get; }

		ZString DocumentReference { get; }

		ZString CancellationWriteOffFlag { get; }
	}
}
