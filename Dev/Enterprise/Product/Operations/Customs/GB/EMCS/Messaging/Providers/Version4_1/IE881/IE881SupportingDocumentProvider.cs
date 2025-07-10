using System.Collections.Generic;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie881;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE881SupportingDocumentProvider : IIE881SupportingDocument
	{
		public static IE881SupportingDocumentProvider NewOrNull(SupportingDocumentsType supportingDocument) => supportingDocument != null ? new IE881SupportingDocumentProvider(supportingDocument) : null;

		IE881SupportingDocumentProvider(SupportingDocumentsType supportingDocument)
		{
			this.supportingDocument = supportingDocument;
		}
		readonly SupportingDocumentsType supportingDocument;

		public ITextAndLanguage SupportingDocumentDescription => supportingDocumentDescription ?? (supportingDocumentDescription = IE881SupportingDocumentDescriptionProvider.NewOrNull(supportingDocument.SupportingDocumentDescription));
		ITextAndLanguage supportingDocumentDescription;

		public ITextAndLanguage ReferenceOfSupportingDocument => referenceOfSupportingDocument ?? (referenceOfSupportingDocument = IE881ReferenceOfSupportingDocumentProvider.NewOrNull(supportingDocument.ReferenceOfSupportingDocument));
		ITextAndLanguage referenceOfSupportingDocument;

		public IReadOnlyCollection<byte> ImageOfDocument => supportingDocument.ImageOfDocument;

		public ZString SupportingDocumentType => supportingDocument.SupportingDocumentType;
	}
}
