using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using CusEntryInstruction = Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class EntryInstructionValidationDeciderTestContextTest : TestCaseWithFactory
	{
		public void TestIsUCC6() => CombineAssertions(() =>
		{
			using (var testContext = new EntryInstructionValidationDeciderTestContext(declaration, isUCC6: true))
			{
				AssertEquals("UCC6 true", true, declaration.IsUCC6);
			}

			using (var testContext = new EntryInstructionValidationDeciderTestContext(declaration, isUCC6: false))
			{
				AssertEquals("UCC6 false", false, declaration.IsUCC6);
			}
		});

		public void TestBaseRulesEnableAndDisable() => CombineAssertions(() =>
		{
			using var testContext = new EntryInstructionValidationDeciderTestContext(declaration, isUCC6: true);

			testContext.EnableRule(x => x.IsRuleC0619ActiveForGoodsLocationDescription);
			AssertEquals("IsRuleC0619ActiveForGoodsLocationDescription enabled", true, ValidationDecider.IsRuleC0619ActiveForGoodsLocationDescription);

			testContext.DisableRule(x => x.IsRuleC0619ActiveForGoodsLocationDescription);
			AssertEquals("IsRuleC0619ActiveForGoodsLocationDescription disabled", false, ValidationDecider.IsRuleC0619ActiveForGoodsLocationDescription);
		});

		public void TestAdditionalRulesEnableAndDisable() => CombineAssertions(() =>
		{
			using var testContext = new EntryInstructionValidationDeciderTestContext(declaration, isUCC6: true, typeof(IRuleC0614ForCEI_SubStyleDecider));

			testContext.EnableRuleDecider<IRuleC0614ForCEI_SubStyleDecider>(x => x.IsActive);
			AssertEquals("IRuleC0614ForCEI_SubStyleDecider.IsActive true", true, ((IRuleC0614ForCEI_SubStyleDecider)ValidationDecider).IsActive);

			testContext.DisableRuleDecider<IRuleC0614ForCEI_SubStyleDecider>(x => x.IsActive);
			AssertEquals("IRuleC0614ForCEI_SubStyleDecider.IsActive false", false, ValidationDecider.IsRuleC0619ActiveForGoodsLocationDescription);
		});

		public void TestMixedRulesEnableAndDisable() => CombineAssertions(() =>
		{
			using var testContext = new EntryInstructionValidationDeciderTestContext(declaration, isUCC6: true, typeof(IRuleC0614ForCEI_SubStyleDecider));

			testContext.EnableRule(x => x.IsRuleC0619ActiveForGoodsLocationDescription);
			testContext.EnableRuleDecider<IRuleC0614ForCEI_SubStyleDecider>(x => x.IsActive);
			AssertEquals("IsRuleC0619ActiveForGoodsLocationDescription enabled", true, ValidationDecider.IsRuleC0619ActiveForGoodsLocationDescription);
			AssertEquals("IRuleC0614ForCEI_SubStyleDecider.IsActive true", true, ((IRuleC0614ForCEI_SubStyleDecider)ValidationDecider).IsActive);

			testContext.DisableRule(x => x.IsRuleC0619ActiveForGoodsLocationDescription);
			testContext.DisableRuleDecider<IRuleC0614ForCEI_SubStyleDecider>(x => x.IsActive);
			AssertEquals("IRuleC0614ForCEI_SubStyleDecider.IsActive false", false, ValidationDecider.IsRuleC0619ActiveForGoodsLocationDescription);
			AssertEquals("IsRuleC0619ActiveForGoodsLocationDescription disabled", false, ValidationDecider.IsRuleC0619ActiveForGoodsLocationDescription);
		});

		IEntryInstructionValidationDecider ValidationDecider => instruction.Validation.ValidationDecider;

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			instruction = declaration.CustomsEntryInstructions.AddNew();
		}

		JobDeclaration declaration;
		CusEntryInstruction instruction;
	}
}
