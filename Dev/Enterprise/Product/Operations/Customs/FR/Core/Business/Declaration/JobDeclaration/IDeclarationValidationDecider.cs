namespace Enterprise.Customs.FR.Business.Declaration
{
	public interface IDeclarationValidationDecider : EU.Business.Declaration.IDeclarationValidationDecider
	{
		bool IsRuleC0810_N01Active { get; }

		bool IsRuleNAT_020Active { get; }

		bool IsRuleNAT_021Active { get; }

		bool IsRuleNAT_130BisActive { get; }

		bool IsRuleNat_145BisActive { get; }

		bool IsRuleNAT_041QuinquiesActive { get; }
	}
}
