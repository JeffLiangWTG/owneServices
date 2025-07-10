using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers.AES.Common;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ComplXAESSupportingDocumentWrapper : AESCommonLineNumberDocumentWrapper, IComplXAESSupportingDocument
	{
		public ComplXAESSupportingDocumentWrapper(SupportingDocument doc, ZShort seqNum) : base(doc, seqNum)
		{
		}

		public IAESCommonSupportingDocumentExtraFields CommonSupportingDocumentExtraFields => commonSupportingDocumentExtraFields ?? (commonSupportingDocumentExtraFields = new AESCommonSupportingDocumentExtraFieldsWrapper(document));
		AESCommonSupportingDocumentExtraFieldsWrapper commonSupportingDocumentExtraFields;

		protected override ZString LineNumberCore => AESWrappersHelper.GetLineNumberForSupportingDocument(document);
	}
}
