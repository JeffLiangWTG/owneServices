namespace Enterprise.Customs.EU.Business
{
	public interface IRequestedDocumentsProvider
	{
		RequestedDocumentCollection RequestedDocuments { get; }
	}
}
