namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface INctsMovementHeaderValidationDecider
	{
		bool IsInBondEntryTypeListValidationActive { get; }
		bool IsRuleB1858Active { get; }
		bool IsRuleC0191Active { get; }
	}
}
