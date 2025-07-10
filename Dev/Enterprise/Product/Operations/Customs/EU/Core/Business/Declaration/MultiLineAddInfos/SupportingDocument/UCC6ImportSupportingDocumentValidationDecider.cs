namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public sealed class UCC6ImportSupportingDocumentValidationDecider : ISupportingDocumentValidationDecider
	{
		public bool SupportBR20311Rule => true;

		public bool SupportC0612Rule => false;

		public bool ShouldCheckIssuingAuthorityNameForEucdm => true;
	}
}
