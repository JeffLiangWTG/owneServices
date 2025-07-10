using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers.AES.Common;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DeclarationAESSupportingDocumentHeaderWrapper : AESCommonLineNumberDocumentWrapper, IDeclarationAESSupportingDocumentHeader
	{
		public DeclarationAESSupportingDocumentHeaderWrapper(SupportingDocument doc, ZShort seqNum) : base(doc, seqNum)
		{
		}

		public IAESCommonSupportingDocumentExtraFields CommonSupportingDocumentExtraFields => commonSupportingDocumentExtraFields ?? (commonSupportingDocumentExtraFields = document == null ? null : new AESCommonSupportingDocumentExtraFieldsWrapper(document));
		AESCommonSupportingDocumentExtraFieldsWrapper commonSupportingDocumentExtraFields;

		protected override ZString LineNumberCore => AESWrappersHelper.GetLineNumberForSupportingDocument(document);
	}
}
