using IEntryInstructionValidationDecider = Enterprise.Customs.EU.Business.Declaration.IEntryInstructionValidationDecider;

namespace Enterprise.Customs.IT.Business.Declaration;

public sealed class UCC6ImportEntryInstructionValidationDecider : IEntryInstructionValidationDecider
{
	bool IEntryInstructionValidationDecider.IsRuleC0382ActiveForGoodsLocationAddressHouseNumber => true;
	bool IEntryInstructionValidationDecider.IsRuleC0619ActiveForGoodsLocationDescription => false;
	bool IEntryInstructionValidationDecider.IsRuleC0626ActiveForCEI_OA_Warehouse2 => false;
	bool IEntryInstructionValidationDecider.IsRuleC0628ActiveForGoodsLocationDescription => false;
	bool IEntryInstructionValidationDecider.IsRuleC0829ActiveForCEI_OA_Warehouse2 => false;
	bool IEntryInstructionValidationDecider.IsRuleC0853ActiveForCEI_OA_Warehouse => false;
	bool IEntryInstructionValidationDecider.IsMaximumEntryLinesAllowedRuleActive => true;
}
