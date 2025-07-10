namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos
{
	public class UCC6ImportAdditionalInfoValidationDecider : IAdditionalInfoValidationDecider
	{
		public bool IsBR2038Rule => true;

		public bool IsC0612Rule => false;
	}
}
