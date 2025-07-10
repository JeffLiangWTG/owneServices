using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.BE.Business.Declaration;

public sealed class UCC6ImportSupportingDocumentValidationDecider : ISupportingDocumentValidationDecider
{
	public bool SupportBR20311Rule => false;

	public bool SupportC0612Rule => false;

	public bool ShouldCheckIssuingAuthorityNameForEucdm => false;
}
