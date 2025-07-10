using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class AESCommonLineNumberDocumentWrapper : CommonDocumentSequenceNumberWrapper, IAESCommonLineNumberDocument
	{
		public AESCommonLineNumberDocumentWrapper(CusSupportingInfo doc, ZShort seqNum) : base(doc.CSI_Code, doc.CSI_ReferenceNumber, seqNum)
		{
			document = doc;
		}
		protected readonly CusSupportingInfo document;

		public AESCommonLineNumberDocumentWrapper(ZString code, ZString referenceNumber, ZShort seqNum) : base(code, referenceNumber, seqNum)
		{
		}

		public ZString LineNumber => LineNumberCore;

		protected virtual ZString LineNumberCore => (document == null || document.CSI_LineNo.IsEmpty) ? string.Empty : document.CSI_LineNo.ToString();
	}
}
