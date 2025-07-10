using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class ImportPreviousDocumentReferenceNumberValidator : PreviousDocumentReferenceNumberValidator
{
	public ImportPreviousDocumentReferenceNumberValidator(PreviousDocument previousDocument, PreviousDocumentFieldsInfo previousDocumentSettings) : base(previousDocument, previousDocumentSettings)
	{
		this.previousDocument = Argument.NotNull(previousDocument, nameof(previousDocument));
	}

	readonly PreviousDocument previousDocument;

	protected override void CheckReferenceNumberFormat()
	{
		var previousDocumentWrapper = (IPreviousDocument)new PreviousDocumentWrapper(previousDocument);
		if (previousDocumentWrapper.ReferenceNumber.Length > Ucc6XmlConstants.PreviousDocument.ReferenceNumberMaxLength)
		{
			ReferenceNumberInfo.AddMessageError(ValidationCaptions.PreviousDocument.NumberExceedsTheMaximumLengthInTheMessage);
		}
	}
}
