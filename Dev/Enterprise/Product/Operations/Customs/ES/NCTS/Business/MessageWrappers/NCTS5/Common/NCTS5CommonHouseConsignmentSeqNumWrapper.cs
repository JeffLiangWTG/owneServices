using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonHouseConsignmentSeqNumWrapper : INCTSCommonHouseConsignmentSeqNum
	{
		public NCTS5CommonHouseConsignmentSeqNumWrapper(ZShort seqNum)
		{
			SequenceNumber = seqNum.ToString();
		}

		public ZString SequenceNumber { get; }
	}
}
