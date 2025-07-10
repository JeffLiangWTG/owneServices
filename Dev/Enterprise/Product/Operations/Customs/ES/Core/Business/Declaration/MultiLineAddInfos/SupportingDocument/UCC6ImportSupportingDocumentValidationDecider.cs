using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class UCC6ImportSupportingDocumentValidationDecider : ISupportingDocumentValidationDecider
	{
		public bool SupportBR20311Rule => true;

		public bool SupportC0612Rule => false;

		public bool ShouldCheckIssuingAuthorityNameForEucdm => false;
	}
}
