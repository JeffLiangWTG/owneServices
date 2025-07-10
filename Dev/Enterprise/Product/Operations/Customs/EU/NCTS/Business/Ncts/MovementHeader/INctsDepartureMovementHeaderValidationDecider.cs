namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsDepartureMovementHeaderValidationDecider : INctsMovementHeaderValidationDecider
	{
		bool IsRuleC035Active { get; }
		bool IsRuleC0839_1Active { get; }
		bool IsRuleC191Active { get; }
		bool IsRuleC547Active { get; }
		bool IsRuleC589Active { get; }
		bool IsRuleC599Active { get; }
		bool IsRuleR0909Active { get; }
		bool IsRuleR902Active { get; }
		bool IsRuleR903Active { get; }
		bool IsRuleR911Active { get; }
	}

	public interface INctsDepartureMovementHeaderPhase4ValidationDecider : INctsDepartureMovementHeaderValidationDecider
	{
		bool IsRuleC010Active { get; }
		bool IsRuleC011Active { get; }
		bool IsRuleC531Active { get; }
		bool IsRuleTR9090Active { get; }
		bool IsRuleTR9095Active { get; }
	}

	public interface INctsDepartureMovementHeaderPhase5ValidationDecider : INctsDepartureMovementHeaderValidationDecider
	{
		bool IsRuleB1091Active { get; }
		bool IsRuleB1806Active { get; }
		bool IsRuleB1836Active { get; }
		bool IsRuleB1838Active { get; }
		bool IsRuleB1848Active { get; }
		bool IsRuleB1848_1Active { get; }
		bool IsRuleB1850Active { get; }
		bool IsRuleB1858_1Active { get; }
		bool IsRuleB1889Active { get; }
		bool IsRuleB1891_1Active { get; }
		bool IsRuleB1892Active { get; }
		bool IsRuleB1893Active { get; }
		bool IsRuleB1893_1Active { get; }
		bool IsRuleB1893_2Active { get; }
		bool IsRuleB1897Active { get; }
		bool IsRuleB1922Active { get; }
		bool IsRuleB2101Active { get; }
		bool IsRuleBR5410Active { get; }
		bool IsRuleC0030Active { get; }
		bool IsRuleC0101_1Active { get; }
		bool IsRuleC0191_1Active { get; }
		bool IsRuleC0191_2Active { get; }
		bool IsRuleC0337_2Active { get; }
		bool IsRuleC0343Active { get; }
		bool IsRuleC0343_2Active { get; }
		bool IsRuleC0387Active { get; }
		bool IsRuleC0403Active { get; }
		bool IsRuleC0403_1Active { get; }
		bool IsRuleC0531Active { get; }
		bool IsRuleC0586Active { get; }
		bool IsRuleC0599Active { get; }
		bool IsRuleC0599_1Active { get; }
		bool IsRuleC0599_2Active { get; }
		bool IsRuleC0710Active { get; }
		bool IsRuleC0806Active { get; }
		bool IsRuleC0909Active { get; }
		bool IsRuleCN839Active { get; }
		bool IsRuleE1103Active { get; }
		bool IsRuleE1109Active { get; }
		bool IsRuleE1301Active { get; }
		bool IsRuleG0114Active { get; }
		bool IsRuleG0789_1Active { get; }
		bool IsRuleN0002Active { get; }
		bool IsRuleNR0007Active { get; }
		bool IsRuleNR0018Active { get; }
		bool IsRuleNR0031Active { get; }
		bool IsRuleNR0035Active { get; }
		bool IsRuleNR0036Active { get; }
		bool IsRuleNR0037Active { get; }
		bool IsRuleNR0038Active { get; }
		bool IsRuleNR0039Active { get; }
		bool IsRuleNR0056Active { get; }
		bool IsRuleNR0070Active { get; }
		bool IsRuleNR0072Active { get; }
		bool IsRuleNR0073Active { get; }
		bool IsRuleNR0080Active { get; }
		bool IsRuleNR0088Active { get; }
		bool IsRulePLR0601Active { get; }
		bool IsRuleR0020Active { get; }
		bool IsRuleR0020_1Active { get; }
		bool IsRuleR0350Active { get; }
		bool IsRuleR0601_1Active { get; }
		bool IsRuleR0789Active { get; }
		bool IsRuleR0789_1Active { get; }
		bool IsRuleR0473Active { get; }
		bool IsRuleR0473_1Active { get; }
		bool IsRuleR0850Active { get; }
		bool IsRuleR0900_4Active { get; }
		bool IsRuleR0911Active { get; }
		bool IsRuleR0990Active { get; }
		bool IsRuleR0994Active { get; }
		bool IsRuleR0994_1Active { get; }
		bool IsRuleTR0017Active { get; }
		bool IsRuleTR0048Active { get; }
		bool IsRuleTR0049Active { get; }
		bool IsRuleTR0050Active { get; }
		bool IsRuleTR0051Active { get; }
		bool IsRuleTR0053Active { get; }
		bool IsRuleTR0054Active { get; }
		bool IsRuleTR0086Active { get; }
		bool IsRuleTR0092Active { get; }
	}

	public interface IRuleB1858_2Decider
	{
		bool IsActive { get; }
	}

	public interface IRuleC0035_1Decider
	{
		bool IsActive { get; }
	}
}
