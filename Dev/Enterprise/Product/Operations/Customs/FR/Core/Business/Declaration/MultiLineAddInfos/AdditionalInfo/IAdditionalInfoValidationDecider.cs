namespace Enterprise.Customs.FR.Business.Declaration
{
	public interface IAdditionalInfoValidationDecider : EU.Business.Declaration.MultiLineAddInfos.IAdditionalInfoValidationDecider
	{
		public bool IsRuleC0834_N01Active { get; }

		public bool IsRuleNAT_088BisActive { get; }

		public bool IsRuleNAT_041Active { get; }

		public bool IsRuleNAT_228Active { get; }
	}
}
