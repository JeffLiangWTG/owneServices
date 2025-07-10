using IEntryInstructionValidationDecider = Enterprise.Customs.EU.Business.Declaration.IEntryInstructionValidationDecider;

namespace Enterprise.Customs.ES.Business.Declaration
{
	sealed class UCC6ImportEntryInstructionValidationDecider : IEntryInstructionValidationDecider
	{
		bool IEntryInstructionValidationDecider.IsRuleC0619ActiveForGoodsLocationDescription => true;
		bool IEntryInstructionValidationDecider.IsRuleC0626ActiveForCEI_OA_Warehouse2 => true;
		bool IEntryInstructionValidationDecider.IsRuleC0628ActiveForGoodsLocationDescription => true;
		bool IEntryInstructionValidationDecider.IsRuleC0829ActiveForCEI_OA_Warehouse2 => true;
		bool IEntryInstructionValidationDecider.IsRuleC0853ActiveForCEI_OA_Warehouse => true;
		bool IEntryInstructionValidationDecider.IsRuleC0382ActiveForGoodsLocationAddressHouseNumber => true;
		bool IEntryInstructionValidationDecider.IsMaximumEntryLinesAllowedRuleActive => true;
	}
}
