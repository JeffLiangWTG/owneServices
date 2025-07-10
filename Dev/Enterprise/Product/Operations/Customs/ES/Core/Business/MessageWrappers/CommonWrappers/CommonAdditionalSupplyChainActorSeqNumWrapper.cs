using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class CommonAdditionalSupplyChainActorSeqNumWrapper : AdditionalSupplyChainActorCommonWrapper, ICommonAdditionalSupplyChainActorSeqNum
	{
		public CommonAdditionalSupplyChainActorSeqNumWrapper(CusSupplyChainActorReference supplyChainActorRef, ZShort seqNum) : base(supplyChainActorRef)
		{
			SequenceNumber = seqNum.ToString();
		}

		public ZString SequenceNumber { get; }
	}
}
