using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(UCC6ImportEntryInstructionValidationDecider))]
	sealed class UCC6ImportEntryInstructionValidationDeciderTest : EntryInstructionValidationDeciderTest<UCC6ImportEntryInstructionValidationDecider>
	{
		public void TestIRuleC0614ForCEI_SubStyleDecider_IsActive()
		{
			AssertEquals(true, ((IRuleC0614ForCEI_SubStyleDecider)validationDecider).IsActive);
		}

		protected override bool ExpectedIsRuleC0619ActiveForGoodsLocationDescriptionResult => true;

		protected override bool ExpectedIsRuleC0626ActiveForCEI_OA_Warehouse2Result => true;

		protected override bool ExpectedIsRuleC0628ActiveForGoodsLocationDescriptionResult => true;

		protected override bool ExpectedIsRuleC0829ActiveForCEI_OA_Warehouse2Result => true;

		protected override bool ExpectedIsRuleC0853ActiveForCEI_OA_WarehouseResult => true;

		protected override bool ExpectedIsRuleC0382ActiveForGoodsLocationAddressHouseNumberResult => true;

		protected override bool ExpectedIsMaximumEntryLinesAllowedRuleActive => true;
	}
}
