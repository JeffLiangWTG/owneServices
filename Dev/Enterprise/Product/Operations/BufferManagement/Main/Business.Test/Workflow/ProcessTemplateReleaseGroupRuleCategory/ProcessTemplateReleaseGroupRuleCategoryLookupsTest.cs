using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProcessTemplateReleaseGroupRuleCategoryLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestWorkflowCategories()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry(WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode, "TS1", "Test category");
			BMSTestHelper.AddWorkflowCategoryToRegistry(WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode, "TS2", "Test category");
			BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, processType: WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var rule = BMSTestHelper.CreateTemplateReleaseGroupRule(template);
			var ruleCategory = BMSTestHelper.CreateTemplateReleaseGroupRuleCategory(rule);

			var categoryCodes = ruleCategory.Lookups.WorkflowCategories.ToArray().Select(c => c.Code);
			var categoryDescriptions = ruleCategory.Lookups.WorkflowCategories.ToArray().Select(c => c.Description);

			AssertContainsExactElementsInAnyOrder(new[] { "TS1", "TS2", "JOB" }, categoryCodes);
			AssertContainsExactElementsInAnyOrder(new[] { "Test category", "Test category", "Job-level workflow" }, categoryDescriptions);
		}

		public void TestWorkflowCategories_ReturnsDefaultCategories_WhenNoCategoriesConfigured()
		{
			BMSTestHelper.CreateSystem(Factory, "INQ");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, processType: WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode);
			var rule = BMSTestHelper.CreateTemplateReleaseGroupRule(template);
			var ruleCategory = BMSTestHelper.CreateTemplateReleaseGroupRuleCategory(rule);

			var categoryCodes = ruleCategory.Lookups.WorkflowCategories.ToArray().Select(c => c.Code);
			var categoryDescriptions = ruleCategory.Lookups.WorkflowCategories.ToArray().Select(c => c.Description);

			AssertContainsExactElementsInAnyOrder(new[] { "UDF", "JOB" }, categoryCodes);
			AssertContainsExactElementsInAnyOrder(new[] { "Undefined - You can modify this in the System Registry, under Workflow Manager/Buffer Management/Workflow Categories", "Job-level workflow" }, categoryDescriptions);
		}
	}
}
