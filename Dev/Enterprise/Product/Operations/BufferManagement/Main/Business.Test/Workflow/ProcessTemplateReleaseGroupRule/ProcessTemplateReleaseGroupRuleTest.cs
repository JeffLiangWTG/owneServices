using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ProcessTemplateReleaseGroupRule))]
	class ProcessTemplateReleaseGroupRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestRootTypeProvider_ShouldProvideTypeFromWorkflowDescriptor()
		{
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var rule = BMSTestHelper.CreateTemplateReleaseGroupRule(template);

			var rootTypeProvider = (IRootTypeProvider)rule;

			AssertSequencesEqual("Should return the bizo type from the WorkflowDescriptor so we can define macros that apply to the relevant job type.", new[] { typeof(OrgHeader) }, rootTypeProvider.RootTypes);
			AssertArrayEqualsByElements(Array.Empty<BusinessObject>(), rootTypeProvider.Roots);

			template.P0_ProcessType = WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode;
			AssertSequencesEqual("Should return the bizo type from the WorkflowDescriptor so we can define macros that apply to the relevant job type.", new[] { typeof(SalesEnquiry) }, rootTypeProvider.RootTypes);
		}

		public void TestOnLoad_ShouldApplyFetchHints()
		{
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var rule1 = BMSTestHelper.CreateTemplateReleaseGroupRule(template);
			var rule2 = BMSTestHelper.CreateTemplateReleaseGroupRule(template);
			var rule3 = BMSTestHelper.CreateTemplateReleaseGroupRule(template);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedTemplate = newFactory.Load<ProcessTaskTemplate>(template.PK);

			foreach (ProcessTemplateReleaseGroupRule rule in loadedTemplate.ReleaseGroupRules)
			{
				rule.GroupMappings.ToArray();
				rule.Categories.ToArray();
			}

			AssertDbHits(new Dictionary<string, int>
			{
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ ProcessTemplateReleaseGroupRuleSchema.Constants.TableName, 1 },
				{ ProcessTemplateReleaseGroupRuleCategorySchema.Constants.TableName, 1 },
				{ ProcessTemplateReleaseGroupRuleMappingSchema.Constants.TableName, 1 },
			}, newFactory);
		}

		public void TestShouldClearCategories_WhenAllWorkflowCategoriesApplicable()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("ORG", "AAA", "It hurts");
			BMSTestHelper.AddWorkflowCategoryToRegistry("ORG", "KKK", "Bad boyz");

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var rule = BMSTestHelper.CreateTemplateReleaseGroupRule(template);

			BMSTestHelper.CreateTemplateReleaseGroupRuleCategory(rule, "AAA");
			BMSTestHelper.CreateTemplateReleaseGroupRuleCategory(rule, "KKK");

			AssertEquals(2, rule.Categories.Count);

			rule.PTR_AreAllWorkflowCategoriesApplicable = true;

			AssertEquals(0, rule.Categories.Count);
		}
	}
}
