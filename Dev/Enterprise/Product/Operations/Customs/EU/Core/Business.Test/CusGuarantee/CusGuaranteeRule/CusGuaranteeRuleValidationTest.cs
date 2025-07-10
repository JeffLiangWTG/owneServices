using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Testing
{
	class CusGuaranteeRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCPR_ValueFrom_MandatoryValidation()
		{
			cusGuaranteeRule.CPR_ValueFrom = "";
			AssertHasErrorContaining("When ValueFrom is empty", cusGuaranteeRule.CPR_ValueFromInfo, MandatoryValidation.MustBeEntered);

			cusGuaranteeRule.CPR_ValueFrom = "ABC";
			AssertNoErrorContaining("When ValueFrom is not empty", cusGuaranteeRule.CPR_ValueFromInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestCheckCPR_ValueFrom_ListValidation_EnterValidLiabilityApplicablePercentage()
		{
			const string enterValidLiabilityApplicablePercentage = "Enter a valid Liability Applicable Percentage";

			cusGuaranteeRule.CPR_RuleCode = PermitRuleCodeList.Codes.LAP;
			cusGuaranteeRule.CPR_ValueFrom = "";
			AssertNoErrorContaining("When RuleCode is LAP and ValueFrom is empty", cusGuaranteeRule.CPR_ValueFromInfo, enterValidLiabilityApplicablePercentage);

			cusGuaranteeRule.CPR_ValueFrom = LiabilityApplicablePercentageCodeList.Codes.ZER;
			AssertNoErrorContaining("When RuleCode is LAP and ValueFrom is valid", cusGuaranteeRule.CPR_ValueFromInfo, enterValidLiabilityApplicablePercentage);

			cusGuaranteeRule.CPR_ValueFrom = "XYZ";
			AssertHasErrorContaining("When RuleCode is LAP and ValueFrom is invalid", cusGuaranteeRule.CPR_ValueFromInfo, enterValidLiabilityApplicablePercentage);

			cusGuaranteeRule.CPR_RuleCode = PermitRuleCodeList.Codes.ADD;
			cusGuaranteeRule.CPR_ValueFrom = "XYZ";
			AssertNoErrorContaining("When RuleCode is not LAP and ValueFrom is invalid", cusGuaranteeRule.CPR_ValueFromInfo, enterValidLiabilityApplicablePercentage);
		}

		public void TestCheckValidateGuaranteeRules()
		{
			var composedLiabilityRuleCodes = new string[]
			{
				PermitRuleCodeList.Codes.PCP,
				PermitRuleCodeList.Codes.PCV,
				PermitRuleCodeList.Codes.PCD
			};

			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();

			foreach (var ruleCode in composedLiabilityRuleCodes)
			{
				var rule = guaranteeHeader.CusGuaranteeRules.AddNew();
				rule.CPR_RuleCode = ruleCode;
				rule.Validation.ValidateAll();
				AssertNoRowError($"No RowError is expected when only {ruleCode} is present.", rule, $"{ruleCode} rule cannot exist with LAP.");
			}

			var rule1 = guaranteeHeader.CusGuaranteeRules.AddNew();
			rule1.CPR_RuleCode = PermitRuleCodeList.Codes.LAP;
			rule1.Validation.ValidateAll();
			AssertHasRowError("RowError is expected when LAP is added after PCP, PCD, or PCV.", rule1, "LAP rule cannot exist with PCP, PCV, or PCD.");

			var guaranteeHeader1 = Factory.New<CusGuaranteeHeader>();

			var rule2 = guaranteeHeader1.CusGuaranteeRules.AddNew();
			rule2.CPR_RuleCode = PermitRuleCodeList.Codes.LAP;
			rule2.Validation.ValidateAll();
			AssertNoRowError("No RowError is expected when only LAP is present.", rule2, "LAP rule cannot exist with PCP, PCV, or PCD.");

			foreach (var ruleCode in composedLiabilityRuleCodes)
			{
				var rule3 = guaranteeHeader1.CusGuaranteeRules.AddNew();
				rule3.CPR_RuleCode = ruleCode;
				rule3.Validation.ValidateAll();
				AssertHasRowError($"RowError is expected when LAP exists first, then {ruleCode} is added.", rule3, $"{ruleCode} rule cannot exist with LAP.");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusGuaranteeRule = Factory.New<CusGuaranteeRule>();
		}

		CusGuaranteeRule cusGuaranteeRule;
	}
}
