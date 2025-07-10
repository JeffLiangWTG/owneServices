namespace Enterprise.Customs.IT.Business;

sealed class Ucc6ExportEntryInstructionPreviousDocumentReferenceNumberValidator : PreviousDocumentReferenceNumberValidator
{
	public Ucc6ExportEntryInstructionPreviousDocumentReferenceNumberValidator(IPreviousDocumentReferenceNumberProvider previousDocument, PreviousDocumentFieldsInfo previousDocumentSettings) : base(previousDocument, previousDocumentSettings)
	{
	}

	protected override void CheckReferenceNumberFormat()
	{
		// Intentionally Kept blank
	}
}
