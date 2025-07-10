namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsDepartureCargoDescPhase5ValidationDecider : INctsCargoDescValidationDecider
	{
		bool IsRuleB1805_1Active { get; }

		bool IsRuleB1875_1Active { get; }

		bool IsRuleB1922Active { get; }

		bool IsRuleB2101Active { get; }

		bool IsRuleB2400_1Active { get; }

		bool IsRuleC0045Active { get; }

		bool IsRuleC0153_1Active { get; }

		bool IsRuleC0343Active { get; }

		bool IsRuleC0343_1Active { get; }

		bool IsRuleC0343_2Active { get; }

		bool IsRuleC0821_1Active { get; }

		bool IsRuleC0837Active { get; }

		bool IsRuleC0837_1Active { get; }

		bool IsRuleC901Active { get; }

		bool IsRuleC0909Active { get; }

		bool IsRuleC0909_1Active { get; }

		bool IsRuleE1107Active { get; }

		bool IsRuleE1107_1Active { get; }

		bool IsRuleE1109Active { get; }

		bool IsRuleE1301Active { get; }

		bool IsRuleE1407Active { get; }

		bool IsRuleNR0020Active { get; }

		bool IsRuleNR0021Active { get; }

		bool IsRuleNR0024Active { get; }

		bool IsRuleNR0025Active { get; }

		bool IsRuleNR0040Active { get; }

		bool IsRuleNR0041Active { get; }

		bool IsRuleNR0042Active { get; }

		bool IsRuleNR0043Active { get; }

		bool IsRuleNR0044Active { get; }

		bool IsRuleNR0045Active { get; }

		bool IsRuleR0221_1Active { get; }

		bool IsRuleR0221_3Active { get; }

		bool IsRuleR0507Active { get; }

		bool IsRuleR0507_1Active { get; }

		bool IsRuleR0601_1Active { get; }

		bool IsRuleR0909Active { get; }

		bool IsRuleRP11Active { get; }

		bool IsRuleTR0068Active { get; }

		bool IsRuleTR0076Active { get; }

		bool IsRuleTR0096Active { get; }

		bool IsRuleTR0100Active { get; }
	}
}
