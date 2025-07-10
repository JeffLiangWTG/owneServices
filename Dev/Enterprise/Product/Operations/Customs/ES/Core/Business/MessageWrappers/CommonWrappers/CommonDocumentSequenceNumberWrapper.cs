using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class CommonDocumentSequenceNumberWrapper : DocumentCommonWrapper, ICommonDocumentSequenceNumber
	{
		public CommonDocumentSequenceNumberWrapper(ZString code, ZString referenceNumber, ZInt seqNum) : base(code, referenceNumber)
		{
			SequenceNumber = seqNum.ToString();
		}

		public ZString SequenceNumber { get; }
	}
}
