using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(UCC6ImportEntryInstructionValidationDecider))]
	sealed class UCC6ImportEntryInstructionValidationDeciderTest : EU.Business.Declaration.Testing.EntryInstructionValidationDeciderTest<UCC6ImportEntryInstructionValidationDecider>
	{
		protected override bool ExpectedIsRuleC0619ActiveForGoodsLocationDescriptionResult => true;
		protected override bool ExpectedIsRuleC0626ActiveForCEI_OA_Warehouse2Result => true;
		protected override bool ExpectedIsRuleC0628ActiveForGoodsLocationDescriptionResult => true;
		protected override bool ExpectedIsRuleC0829ActiveForCEI_OA_Warehouse2Result => true;
		protected override bool ExpectedIsRuleC0853ActiveForCEI_OA_WarehouseResult => true;
		protected override bool ExpectedIsRuleC0382ActiveForGoodsLocationAddressHouseNumberResult => true;
		protected override bool ExpectedIsMaximumEntryLinesAllowedRuleActive => true;
	}
}
