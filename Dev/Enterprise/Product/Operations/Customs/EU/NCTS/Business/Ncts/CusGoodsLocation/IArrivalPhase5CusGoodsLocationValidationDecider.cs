namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface IArrivalPhase5CusGoodsLocationValidationDecider : ICusGoodsLocationValidationDecider
	{
		bool IsRuleNR0011Active { get; }
		
		bool IsRuleNR0012Active { get; }

		bool IsRuleNR0013Active { get; }

		bool IsRuleNR0075Active { get; }

		bool IsRuleTR0069Active { get; }

		bool IsRuleC0394Active { get; }
	}
}
