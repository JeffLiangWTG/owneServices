using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public sealed class UCC6ImportSupportingDocumentValidationDecider : ISupportingDocumentValidationDecider
	{
		public bool SupportBR20311Rule => true;

		public bool SupportC0612Rule => true;

		public bool ShouldCheckIssuingAuthorityNameForEucdm => true;
	}
}
