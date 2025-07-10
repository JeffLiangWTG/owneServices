using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonCustomsOfficeWrapper : INCTSCommonCustomsOffice
	{
		public NCTS5CommonCustomsOfficeWrapper(ZString referenceNumber, ZShort seqNum)
		{
			SequenceNumber = seqNum.ToString();
			ReferenceNumber = referenceNumber;
		}

		public ZString SequenceNumber { get; }

		public ZString ReferenceNumber { get; }
	}
}
