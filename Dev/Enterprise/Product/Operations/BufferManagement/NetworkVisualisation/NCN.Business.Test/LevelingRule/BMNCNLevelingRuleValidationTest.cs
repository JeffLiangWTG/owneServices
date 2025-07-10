using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class BMNCNLevelingRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateName()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var rule1 = NetworkTestCase.CreateLevelingRule(diagram);
			var rule2 = NetworkTestCase.CreateLevelingRule(diagram);

			rule1.BNR_Name = "";
			rule2.BNR_Name = "";

			AssertMandatoryValidationError(rule1.BNR_NameInfo, isExpectingError: true);
			AssertMandatoryValidationError(rule2.BNR_NameInfo, isExpectingError: true);

			rule1.BNR_Name = "You've got to know your rules";
			rule2.BNR_Name = "You've got to know your rules";

			AssertPropertyIsUniqueInCollectionValidationError(rule1.BNR_NameInfo, isExpectingError: true);
			AssertPropertyIsUniqueInCollectionValidationError(rule2.BNR_NameInfo, isExpectingError: true);

			rule2.BNR_Name = "Learn your rules";

			AssertNoErrors(rule1);
			AssertNoErrors(rule2);
		}

		public void TestValidateRuleType()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var rule = NetworkTestCase.CreateLevelingRule(diagram);

			rule.BNR_Type = "69";
			AssertListValidationInvalidCodeError(rule.BNR_TypeInfo, isExpectingError: true);

			rule.BNR_Type = "";
			AssertMandatoryValidationError(rule.BNR_TypeInfo, isExpectingError: true);

			foreach (ICodeDescription cdp in new LevelingRuleTypeList())
			{
				rule.BNR_Type = cdp.Code;

				AssertNoErrors(rule);
			}
		}

		public void TestValidateRuleValue()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var rule = NetworkTestCase.CreateLevelingRule(diagram);

			rule.BNR_RuleValue = -1;
			AssertHasError(rule.BNR_RuleValueInfo, "Please enter a 'Value' greater than or equal to 1.");

			rule.BNR_RuleValue = 0;
			AssertHasError(rule.BNR_RuleValueInfo, "Please enter a 'Value' greater than or equal to 1.");

			rule.BNR_RuleValue = 1;
			AssertNoErrors(rule.BNR_RuleValueInfo);
		}

		public void TestValidateColorName()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var rule = NetworkTestCase.CreateLevelingRule(diagram);

			rule.ColorName = "";
			AssertHasError(rule.ColorNameInfo, "Please enter a Color.");

			rule.ColorName = "Beijing";
			AssertHasError(rule.ColorNameInfo, "Enter a valid Color.");

			rule.ColorName = "Beige";
			AssertNoErrors(rule.ColorNameInfo);
		}
	}
}
