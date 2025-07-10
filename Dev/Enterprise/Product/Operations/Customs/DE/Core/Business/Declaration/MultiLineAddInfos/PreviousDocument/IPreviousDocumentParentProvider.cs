namespace Enterprise.Customs.DE.Business.Declaration
{
	public interface IPreviousDocumentParentProvider
	{
		PreviousDocumentCollection PreviousDocuments { get; }

		PreviousDocumentMaster PreviousDocumentMaster { get; }

		JobDeclaration JobDeclaration { get; }
	}
}
