namespace Enterprise.Customs.IT.Business;

public class PreviousDocumentFieldsInfo
{
	public bool IsReferenceNumberEditable { get; protected set; }
	public bool IsReferenceNumber2Editable { get; }
	public bool IsDateOfIssueEditable { get; }
	public bool IsLineNoEditable { get; protected set; }
	public bool IsStatusEditable { get; }
	public bool IsCustomsOfficeEditable { get; }

	public bool IsNetMassEditable { get; } = true;
	public bool IsGrossMassEditable { get; } = true;
	public bool IsPackageQuantityEditable { get; } = true;

	public PreviousDocumentFieldsInfo(PreviousDocumentCombinationTemplate? combinationTemplate = null)
	{
		switch (combinationTemplate)
		{
			case PreviousDocumentCombinationTemplate._1:
				IsReferenceNumberEditable = true;
				IsDateOfIssueEditable = true;
				break;

			case PreviousDocumentCombinationTemplate._2:
				IsReferenceNumber2Editable = true;
				IsLineNoEditable = true;
				break;

			case PreviousDocumentCombinationTemplate._3:
				IsReferenceNumberEditable = true;
				IsDateOfIssueEditable = true;
				IsCustomsOfficeEditable = true;
				IsLineNoEditable = true;
				break;

			case PreviousDocumentCombinationTemplate._4:
				IsReferenceNumberEditable = true;
				IsDateOfIssueEditable = true;
				IsCustomsOfficeEditable = true;
				IsStatusEditable = true;
				break;

			case PreviousDocumentCombinationTemplate._5:
				IsReferenceNumberEditable = true;
				IsDateOfIssueEditable = true;
				IsCustomsOfficeEditable = true;
				break;

			case PreviousDocumentCombinationTemplate.ImportNUM:
				IsReferenceNumberEditable = true;
				IsNetMassEditable = false;
				IsGrossMassEditable = false;
				IsPackageQuantityEditable = false;
				break;

			case PreviousDocumentCombinationTemplate.ImportNN:
				IsReferenceNumberEditable = true;
				IsDateOfIssueEditable = true;
				IsCustomsOfficeEditable = true;
				break;
		}
	}
}
