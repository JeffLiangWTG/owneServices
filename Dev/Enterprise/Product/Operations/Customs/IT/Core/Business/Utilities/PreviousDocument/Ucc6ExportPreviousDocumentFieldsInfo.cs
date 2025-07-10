namespace Enterprise.Customs.IT.Business;

sealed class Ucc6ExportPreviousDocumentFieldsInfo : PreviousDocumentFieldsInfo
{
	public Ucc6ExportPreviousDocumentFieldsInfo(PreviousDocumentCombinationTemplate? combinationTemplate = null) : base(combinationTemplate)
	{
		IsReferenceNumberEditable = true;
	}
}
