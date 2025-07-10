using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public sealed class UCC6ImportAdditionalInfoValidationDecider : IAdditionalInfoValidationDecider
	{
		public bool IsBR2038Rule => true;

		public bool IsC0612Rule => true;
	}
}
