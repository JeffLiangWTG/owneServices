using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public interface IAdditionalSupplyChainActorCommon
	{
		ZString Role { get; }
		ZString Id { get; }
	}

	public interface ICommonAdditionalSupplyChainActorSeqNum : IAdditionalSupplyChainActorCommon
	{
		ZString SequenceNumber { get; }
	}
}
