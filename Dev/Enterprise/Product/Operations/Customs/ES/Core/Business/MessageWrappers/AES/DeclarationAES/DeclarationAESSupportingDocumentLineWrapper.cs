using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers.AES.Common;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DeclarationAESSupportingDocumentLineWrapper : AESCommonDocumentWrapper, IDeclarationAESSupportingDocumentLine
	{
		public DeclarationAESSupportingDocumentLineWrapper(SupportingDocument doc, ZShort seqNum) : base(doc, seqNum)
		{
		}

		public DeclarationAESSupportingDocumentLineWrapper(ZString code, ZString referenceNumber, ZShort seqNum) : base(code, referenceNumber, seqNum)
		{
		}

		public IAESCommonSupportingDocumentExtraFields CommonSupportingDocumentExtraFields => commonSupportingDocumentExtraFields ?? (commonSupportingDocumentExtraFields = document == null ? null : new AESCommonSupportingDocumentExtraFieldsWrapper(document));
		AESCommonSupportingDocumentExtraFieldsWrapper commonSupportingDocumentExtraFields;

		protected override ZString LineNumberCore => AESWrappersHelper.GetLineNumberForSupportingDocument(document);
	}
}
