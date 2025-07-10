using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonDocumentWithInfoWrapper : CommonDocumentSequenceNumberWrapper, INCTSCommonDocumentWithInfo
	{
		public NCTS5CommonDocumentWithInfoWrapper(CusSupportingInfo doc, ZInt seqNum) : base(doc.CSI_Code, doc.CSI_ReferenceNumber, seqNum)
		{
			ComplementaryInformation = doc.CSI_ReferenceNumber2;
		}

		public NCTS5CommonDocumentWithInfoWrapper(ZString code, ZString referenceNumber, ZString referenceNumber2, ZInt seqNum) : base(code, referenceNumber, seqNum)
		{
			ComplementaryInformation = referenceNumber2;
		}

		public ZString ComplementaryInformation { get; }
	}
}
