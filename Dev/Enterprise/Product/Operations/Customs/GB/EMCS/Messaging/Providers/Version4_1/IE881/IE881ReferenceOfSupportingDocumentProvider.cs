using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie881;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE881ReferenceOfSupportingDocumentProvider : ITextAndLanguage
	{
		public static IE881ReferenceOfSupportingDocumentProvider NewOrNull(LsdReferenceOfSupportingDocumentType referenceOfSupportingDocument)
			=> referenceOfSupportingDocument != null ? new IE881ReferenceOfSupportingDocumentProvider(referenceOfSupportingDocument) : null;

		IE881ReferenceOfSupportingDocumentProvider(LsdReferenceOfSupportingDocumentType referenceOfSupportingDocument)
		{
			this.referenceOfSupportingDocument = referenceOfSupportingDocument;
		}
		readonly LsdReferenceOfSupportingDocumentType referenceOfSupportingDocument;

		public string Text => referenceOfSupportingDocument.Value;

		public string Language => referenceOfSupportingDocument.Language;
	}
}
