using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class NctsDepartureMovementHeaderPhase5ValidationDecider : INctsDepartureMovementHeaderPhase5ValidationDecider, IRuleB1858_2Decider, IRuleC0035_1Decider
{
	public bool IsRuleB1091Active => true;

	public bool IsRuleB1806Active => false;

	public bool IsRuleB1836Active => false;

	public bool IsRuleB1838Active => true;

	public bool IsRuleB1848Active => false;

	public bool IsRuleB1848_1Active => false;

	public bool IsRuleB1850Active => true;

	public bool IsRuleB1858Active => false;

	public bool IsRuleB1858_1Active => false;

	bool IRuleB1858_2Decider.IsActive => true;

	public bool IsRuleB1889Active => false;

	public bool IsRuleB1891_1Active => false;

	public bool IsRuleB1892Active => false;

	public bool IsRuleB1893Active => false;

	public bool IsRuleB1893_1Active => false;

	public bool IsRuleB1893_2Active => false;

	public bool IsRuleB1897Active => false;

	public bool IsRuleB1922Active => false;

	public bool IsRuleB2101Active => false;

	public bool IsRuleBR5410Active => false;

	public bool IsRuleC0030Active => false;

	public bool IsRuleC0101_1Active => true;

	public bool IsRuleC0191Active => false;

	public bool IsRuleC0191_1Active => true;

	public bool IsRuleC0191_2Active => false;

	public bool IsRuleC0337_2Active => true;

	public bool IsRuleC0343Active => false;

	public bool IsRuleC0343_2Active => false;

	public bool IsRuleC035Active => false;

	public bool IsRuleC0387Active => false;

	bool IRuleC0035_1Decider.IsActive => true;

	public bool IsRuleC0403Active => false;

	public bool IsRuleC0403_1Active => false;

	public bool IsRuleC0531Active => true;

	public bool IsRuleC0586Active => true;

	public bool IsRuleC0599_1Active => true;

	public bool IsRuleC0599_2Active => true;

	public bool IsRuleC0710Active => false;

	public bool IsRuleC0806Active => false;

	public bool IsRuleC0839_1Active => false;

	public bool IsRuleC191Active => false;

	public bool IsRuleC0599Active => false;

	public bool IsRuleC0909Active => false;

	public bool IsRuleC547Active => false;

	public bool IsRuleC589Active => false;

	public bool IsRuleC599Active => false;

	public bool IsRuleCN839Active => false;

	public bool IsRuleE1103Active => true;

	public bool IsRuleE1109Active => true;

	public bool IsRuleE1301Active => false;

	public bool IsRuleG0114Active => true;

	public bool IsRuleG0789_1Active => false;

	public bool IsRuleN0002Active => false;

	public bool IsRuleNR0007Active => false;

	public bool IsRuleNR0018Active => true;

	public bool IsRuleNR0031Active => true;

	public bool IsRuleNR0035Active => false;

	public bool IsRuleNR0036Active => true;

	public bool IsRuleNR0037Active => true;

	public bool IsRuleNR0038Active => false;

	public bool IsRuleNR0039Active => true;

	public bool IsRuleNR0056Active => false;

	public bool IsRuleNR0070Active => false;

	public bool IsRuleNR0072Active => false;

	public bool IsRuleNR0073Active => false;

	public bool IsRuleNR0080Active => false;

	public bool IsRuleNR0088Active => false;

	public bool IsRuleR0473Active => true;

	public bool IsRuleR902Active => true;

	public bool IsRuleR903Active => true;

	public bool IsRuleR911Active => true;

	public bool IsRulePLR0601Active => true;

	public bool IsRuleR0020Active => false;

	public bool IsRuleR0020_1Active => true;

	public bool IsRuleR0350Active => true;
	
	public bool IsRuleR0473_1Active => false;

	public bool IsRuleR0601_1Active => false;

	public bool IsRuleR0789Active => false;

	public bool IsRuleR0789_1Active => false;

	public bool IsRuleR0850Active => false;

	public bool IsRuleR0900_4Active => false;

	public bool IsRuleR0909Active => true;

	public bool IsRuleR0911Active => false;

	public bool IsRuleR0990Active => false;

	public bool IsRuleR0994Active => false;

	public bool IsRuleR0994_1Active => false;

	public bool IsRuleTR0017Active => true;

	public bool IsRuleTR0048Active => false;

	public bool IsRuleTR0049Active => true;

	public bool IsRuleTR0050Active => true;

	public bool IsRuleTR0051Active => false;

	public bool IsRuleTR0053Active => false;

	public bool IsRuleTR0054Active => true;

	public bool IsRuleTR0086Active => false;

	public bool IsRuleTR0092Active => true;

	public bool IsInBondEntryTypeListValidationActive => true;
}
