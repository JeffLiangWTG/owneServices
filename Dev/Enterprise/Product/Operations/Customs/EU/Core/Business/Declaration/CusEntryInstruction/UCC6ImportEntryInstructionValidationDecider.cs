namespace Enterprise.Customs.EU.Business.Declaration
{
	sealed class UCC6ImportEntryInstructionValidationDecider : IEntryInstructionValidationDecider, IRuleC0614ForCEI_SubStyleDecider
	{
		bool IRuleC0614ForCEI_SubStyleDecider.IsActive => true;

		public bool IsRuleC0619ActiveForGoodsLocationDescription => true;

		public bool IsRuleC0626ActiveForCEI_OA_Warehouse2 => true;

		public bool IsRuleC0628ActiveForGoodsLocationDescription => true;

		public bool IsRuleC0829ActiveForCEI_OA_Warehouse2 => true;

		public bool IsRuleC0853ActiveForCEI_OA_Warehouse => true;

		public bool IsRuleC0382ActiveForGoodsLocationAddressHouseNumber => true;

		public bool IsMaximumEntryLinesAllowedRuleActive => true;
	}
}
