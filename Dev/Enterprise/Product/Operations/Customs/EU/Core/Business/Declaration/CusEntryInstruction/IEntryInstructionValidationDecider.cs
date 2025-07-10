namespace Enterprise.Customs.EU.Business.Declaration
{
	public interface IEntryInstructionValidationDecider
	{
		bool IsRuleC0619ActiveForGoodsLocationDescription { get; }
		bool IsRuleC0626ActiveForCEI_OA_Warehouse2 { get; }
		bool IsRuleC0628ActiveForGoodsLocationDescription { get; }
		bool IsRuleC0829ActiveForCEI_OA_Warehouse2 { get; }
		bool IsRuleC0853ActiveForCEI_OA_Warehouse { get; }
		bool IsRuleC0382ActiveForGoodsLocationAddressHouseNumber { get; }
		bool IsMaximumEntryLinesAllowedRuleActive { get; }
	}

	public interface IRuleC0614ForCEI_SubStyleDecider
	{
		bool IsActive { get; }
	}

	public interface IRuleG0128ForCEI_ProcedureDecider
	{
		bool IsActive { get; }
	}

	public interface IRuleR0028EForCEI_SubStyleDecider
	{
		bool IsActive { get; }
	}
}
