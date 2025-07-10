namespace Enterprise.Customs.CA.Business;

public class RequiredDocumentTypeCategory
{
	public RequiredDocumentTypeCategory(DocumentTypeRequieredType requiredType = DocumentTypeRequieredType.Mandatory)
	{
		RequieredType = requiredType;
	}

	public DocumentTypeRequieredType RequieredType { get; }
}
