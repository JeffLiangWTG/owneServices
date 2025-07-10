using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ProcessTemplateReleaseGroupRuleCategory))]
	class ProcessTemplateReleaseGroupRuleCategoryTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCategoryDescription()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("ORG", "AAA", "It hurts");
			BMSTestHelper.AddWorkflowCategoryToRegistry("ORG", "KKK", "Bad boyz");

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var rule = BMSTestHelper.CreateTemplateReleaseGroupRule(template);
			var ruleCategory = BMSTestHelper.CreateTemplateReleaseGroupRuleCategory(rule);

			AssertEquals(ZString.Empty, ruleCategory.CategoryDescription);

			ruleCategory.PTC_Category = "AAA";
			AssertEquals("It hurts", ruleCategory.CategoryDescription);

			ruleCategory.PTC_Category = "KKK";
			AssertEquals("Bad boyz", ruleCategory.CategoryDescription);

			ruleCategory.PTC_Category = "UDF";
			AssertEquals(ZString.Empty, ruleCategory.CategoryDescription);
		}

		public void TestCategoryDescription_NoWorkflowCategoriesDefined()
		{
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var rule = BMSTestHelper.CreateTemplateReleaseGroupRule(template);
			var ruleCategory = BMSTestHelper.CreateTemplateReleaseGroupRuleCategory(rule);

			AssertEquals(ZString.Empty, ruleCategory.CategoryDescription);

			ruleCategory.PTC_Category = "AAA";
			AssertEquals(ZString.Empty, ruleCategory.CategoryDescription);

			ruleCategory.PTC_Category = "UDF";
			AssertEquals("Undefined - You can modify this in the System Registry, under Workflow Manager/Buffer Management/Workflow Categories", ruleCategory.CategoryDescription);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var rule = Factory.New<ProcessTemplateReleaseGroupRule>();
			var ruleCategory = Factory.New<ProcessTemplateReleaseGroupRuleCategory>();
			ruleCategory.PTC_PTR_Rule = rule.PK;

			return ruleCategory;
		}

		#endregion
	}
}
