namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public interface ISupportingDocumentValidationDecider
	{
		bool SupportBR20311Rule { get; }

		bool SupportC0612Rule { get; }

		bool ShouldCheckIssuingAuthorityNameForEucdm { get; }
	}
}
