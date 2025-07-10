using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business;

public sealed class NctsArrivalMovementHeaderPhase5ValidationDecider : INctsArrivalMovementHeaderPhase5ValidationDecider
{
	public bool IsRuleB1858Active => true;

	public bool IsRuleC0191Active => true;

	public bool IsInBondEntryTypeListValidationActive => true;

	public bool IsRuleNR0009Active => false;

	public bool IsRuleNR0026Active => false;

	public bool IsRuleNR0028Active => false;

	public bool IsRuleNR0076Active => false;

	public bool IsRuleTR0022Active => true;

	public bool IsRuleTR0034Active => true;

	public bool IsRuleTR0042Active => true;

	public bool IsRuleTR0063Active => false;

	public bool IsRuleTR0071Active => false;

	public bool IsRuleTR0072Active => true;

	public bool IsRuleTR0091Active => true;

	public bool IsRuleTR0098Active => false;
}
