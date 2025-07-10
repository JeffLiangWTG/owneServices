using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class SealCommonWrapper : ISealCommon
	{
		public SealCommonWrapper(ZShort seqNum, ZString seal)
		{
			SequenceNumber = seqNum.ToString();
			SealNumber = seal;
		}

		public ZString SequenceNumber { get; }

		public ZString SealNumber { get; }
	}
}
