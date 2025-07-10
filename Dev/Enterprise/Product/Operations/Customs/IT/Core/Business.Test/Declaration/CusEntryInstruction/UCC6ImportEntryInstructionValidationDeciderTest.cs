using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(UCC6ImportEntryInstructionValidationDecider))]
sealed class UCC6ImportEntryInstructionValidationDeciderTest : EU.Business.Declaration.Testing.EntryInstructionValidationDeciderTest<UCC6ImportEntryInstructionValidationDecider>
{
	protected override bool ExpectedIsRuleC0619ActiveForGoodsLocationDescriptionResult => false;
	protected override bool ExpectedIsRuleC0626ActiveForCEI_OA_Warehouse2Result => false;
	protected override bool ExpectedIsRuleC0628ActiveForGoodsLocationDescriptionResult => false;
	protected override bool ExpectedIsRuleC0829ActiveForCEI_OA_Warehouse2Result => false;
	protected override bool ExpectedIsRuleC0853ActiveForCEI_OA_WarehouseResult => false;
	protected override bool ExpectedIsRuleC0382ActiveForGoodsLocationAddressHouseNumberResult => true;
	protected override bool ExpectedIsMaximumEntryLinesAllowedRuleActive => true;
}
