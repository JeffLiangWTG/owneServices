namespace Enterprise.Customs.FR.Business.Declaration
{
	public sealed class UCC6ImportAdditionalInfoValidationDecider : IAdditionalInfoValidationDecider
	{
		public bool IsRuleC0834_N01Active => true;

		public bool IsBR2038Rule => true;

		public bool IsC0612Rule => false;

		public bool IsRuleNAT_088BisActive => true;

		public bool IsRuleNAT_041Active => true;

		public bool IsRuleNAT_228Active => true;
	}
}
