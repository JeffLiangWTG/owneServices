namespace Enterprise.Customs.EU.Business.Declaration
{
	sealed class UCC6ExportEntryInstructionValidationDecider : IEntryInstructionValidationDecider
		, IRuleG0128ForCEI_ProcedureDecider
		, IRuleR0028EForCEI_SubStyleDecider
	{
		public bool IsRuleC0619ActiveForGoodsLocationDescription => false;
		public bool IsRuleC0626ActiveForCEI_OA_Warehouse2 => false;
		public bool IsRuleC0628ActiveForGoodsLocationDescription => false;
		public bool IsRuleC0829ActiveForCEI_OA_Warehouse2 => false;
		public bool IsRuleC0853ActiveForCEI_OA_Warehouse => false;
		public bool IsRuleC0382ActiveForGoodsLocationAddressHouseNumber => true;
		public bool IsMaximumEntryLinesAllowedRuleActive => true;

		bool IRuleG0128ForCEI_ProcedureDecider.IsActive => false;
		bool IRuleR0028EForCEI_SubStyleDecider.IsActive => false;
	}
}
