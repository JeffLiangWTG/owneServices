using CargoWise.EntityFramework.Testing;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProcessTemplateReleaseGroupRuleCategoryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateCategory_ValueEntered()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("ORG", "AAA", "It hurts");

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var rule = BMSTestHelper.CreateTemplateReleaseGroupRule(template);
			var ruleCategory = BMSTestHelper.CreateTemplateReleaseGroupRuleCategory(rule);

			ruleCategory.PTC_Category = string.Empty;
			AssertHasError(ruleCategory.PTC_CategoryInfo, "Please enter a Category.");

			ruleCategory.PTC_Category = "XXX";
			AssertHasError(ruleCategory.PTC_CategoryInfo, "Enter a valid Category.");

			ruleCategory.PTC_Category = "AAA";
			AssertNoErrors(ruleCategory.PTC_CategoryInfo);
		}

		public void TestValidateCategory_NoDuplicateValues()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("ORG", "AAA", "It hurts");
			BMSTestHelper.AddWorkflowCategoryToRegistry("ORG", "KKK", "Bad boyz");

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var rule = BMSTestHelper.CreateTemplateReleaseGroupRule(template);

			var ruleCategory1 = BMSTestHelper.CreateTemplateReleaseGroupRuleCategory(rule, "AAA");
			var ruleCategory2 = BMSTestHelper.CreateTemplateReleaseGroupRuleCategory(rule, "AAA");

			ruleCategory1.Validation.ValidateAll();
			ruleCategory2.Validation.ValidateAll();

			AssertHasError(ruleCategory1.PTC_CategoryInfo, "The Category has been duplicated and must be unique.");
			AssertHasError(ruleCategory2.PTC_CategoryInfo, "The Category has been duplicated and must be unique.");

			ruleCategory2.PTC_Category = "KKK";

			ruleCategory1.Validation.ValidateAll();
			ruleCategory2.Validation.ValidateAll();

			AssertNoErrors(ruleCategory1.PTC_CategoryInfo);
			AssertNoErrors(ruleCategory2.PTC_CategoryInfo);
		}

		public void TestValidateCategory_NoDuplicateValues_MultipleRules()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("ORG", "AAA", "It hurts");
			BMSTestHelper.AddWorkflowCategoryToRegistry("ORG", "KKK", "Bad boyz");
			BMSTestHelper.AddWorkflowCategoryToRegistry("ORG", "SSS", "Tiny snek");

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var rule1 = BMSTestHelper.CreateTemplateReleaseGroupRule(template);

			var ruleCategory1 = BMSTestHelper.CreateTemplateReleaseGroupRuleCategory(rule1, "AAA");
			var ruleCategory2 = BMSTestHelper.CreateTemplateReleaseGroupRuleCategory(rule1, "KKK");

			var rule2 = BMSTestHelper.CreateTemplateReleaseGroupRule(template);

			var ruleCategory3 = BMSTestHelper.CreateTemplateReleaseGroupRuleCategory(rule2, "AAA");
			var ruleCategory4 = BMSTestHelper.CreateTemplateReleaseGroupRuleCategory(rule2, "KKK");
			var ruleCategory5 = BMSTestHelper.CreateTemplateReleaseGroupRuleCategory(rule2, "KKK");

			ruleCategory1.Validation.ValidateAll();
			ruleCategory2.Validation.ValidateAll();
			ruleCategory3.Validation.ValidateAll();
			ruleCategory4.Validation.ValidateAll();
			ruleCategory5.Validation.ValidateAll();

			AssertNoErrors(ruleCategory1.PTC_CategoryInfo);
			AssertNoErrors(ruleCategory2.PTC_CategoryInfo);
			AssertNoErrors(ruleCategory3.PTC_CategoryInfo);
			AssertHasError(ruleCategory4.PTC_CategoryInfo, "The Category has been duplicated and must be unique.");
			AssertHasError(ruleCategory5.PTC_CategoryInfo, "The Category has been duplicated and must be unique.");

			ruleCategory5.PTC_Category = "SSS";

			ruleCategory1.Validation.ValidateAll();
			ruleCategory2.Validation.ValidateAll();
			ruleCategory3.Validation.ValidateAll();
			ruleCategory4.Validation.ValidateAll();
			ruleCategory5.Validation.ValidateAll();

			AssertNoErrors(ruleCategory1.PTC_CategoryInfo);
			AssertNoErrors(ruleCategory2.PTC_CategoryInfo);
			AssertNoErrors(ruleCategory3.PTC_CategoryInfo);
			AssertNoErrors(ruleCategory4.PTC_CategoryInfo);
			AssertNoErrors(ruleCategory5.PTC_CategoryInfo);
		}
	}
}
