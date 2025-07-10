namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public interface IPreviousDocumentsProviderWithValidationDecider : IPreviousDocumentsProvider
	{
		IPreviousDocumentValidationDecider ValidationDecider { get; }
	}
}
