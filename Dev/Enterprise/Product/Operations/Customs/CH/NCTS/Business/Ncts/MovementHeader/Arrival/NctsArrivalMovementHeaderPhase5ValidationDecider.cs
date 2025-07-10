using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class NctsArrivalMovementHeaderPhase5ValidationDecider : INctsArrivalMovementHeaderPhase5ValidationDecider
{
	public bool IsRuleB1858Active => false;

	public bool IsRuleC0191Active => false;

	public bool IsRuleNR0009Active => false;

	public bool IsRuleNR0026Active => true;

	public bool IsRuleNR0028Active => true;

	public bool IsRuleNR0076Active => false;

	public bool IsRuleTR0022Active => true;

	public bool IsRuleTR0034Active => false;

	public bool IsRuleTR0042Active => false;

	public bool IsRuleTR0063Active => true;

	public bool IsRuleTR0071Active => false;

	public bool IsRuleTR0072Active => false;

	public bool IsRuleTR0091Active => true;

	public bool IsRuleTR0098Active => true;

	public bool IsInBondEntryTypeListValidationActive => false;
}
