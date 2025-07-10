namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public interface ISupportingDocumentsProvider : Integration.Customs.ICusSupportingInfoTypeSupporter
	{
		SupportingDocumentCollection SupportingDocuments { get; }
	}
}
