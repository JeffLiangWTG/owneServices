namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public interface ISupportingDocumentsProvider
	{
		ISupportingDocumentCollection<SupportingDocument> SupportingDocuments { get; }
	}
}
