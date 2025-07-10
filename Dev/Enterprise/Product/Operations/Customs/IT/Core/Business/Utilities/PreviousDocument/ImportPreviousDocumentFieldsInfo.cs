namespace Enterprise.Customs.IT.Business;

sealed class ImportPreviousDocumentFieldsInfo : PreviousDocumentFieldsInfo
{
	public ImportPreviousDocumentFieldsInfo(PreviousDocumentCombinationTemplate? combinationTemplate = null) : base(combinationTemplate)
	{
		IsLineNoEditable = true;
	}
}
