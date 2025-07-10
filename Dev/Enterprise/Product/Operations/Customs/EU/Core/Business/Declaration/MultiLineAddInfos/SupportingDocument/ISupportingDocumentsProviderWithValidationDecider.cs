namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public interface ISupportingDocumentsProviderWithValidationDecider : ISupportingDocumentsProvider
	{
		ISupportingDocumentValidationDecider ValidationDecider { get; }
	}
}
