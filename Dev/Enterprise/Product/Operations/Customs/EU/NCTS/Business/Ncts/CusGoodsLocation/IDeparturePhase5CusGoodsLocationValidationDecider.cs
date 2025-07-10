namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface IDeparturePhase5CusGoodsLocationValidationDecider : ICusGoodsLocationValidationDecider
	{
		bool IsRuleC0394Active { get; }

		bool IsRuleNR0013Active { get; }

		bool IsRuleNR0023Active { get; }

		bool IsRuleNR0050Active { get; }

		bool IsRuleNR0063Active { get; }
	}
}
