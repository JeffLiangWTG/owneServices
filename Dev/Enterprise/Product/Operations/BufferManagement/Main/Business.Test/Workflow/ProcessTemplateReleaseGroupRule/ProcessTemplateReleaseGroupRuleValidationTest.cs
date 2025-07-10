using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProcessTemplateReleaseGroupRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSequence_ShouldBeUniquePerTemplate()
		{
			var template1 = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var template2 = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);

			var rule1_1 = BMSTestHelper.CreateTemplateReleaseGroupRule(template1);
			var rule1_2 = BMSTestHelper.CreateTemplateReleaseGroupRule(template1);

			var rule2_1 = BMSTestHelper.CreateTemplateReleaseGroupRule(template2);
			var rule2_2 = BMSTestHelper.CreateTemplateReleaseGroupRule(template2);

			rule1_2.PTR_Sequence = 1;

			rule2_1.Validation.ValidateAll();
			rule2_2.Validation.ValidateAll();

			AssertPropertyIsUniqueInCollectionValidationError(rule1_1.PTR_SequenceInfo, isExpectingError: true);
			AssertPropertyIsUniqueInCollectionValidationError(rule1_2.PTR_SequenceInfo, isExpectingError: true);

			AssertPropertyIsUniqueInCollectionValidationError(rule2_1.PTR_SequenceInfo, isExpectingError: false);
			AssertPropertyIsUniqueInCollectionValidationError(rule2_2.PTR_SequenceInfo, isExpectingError: false);

			rule1_2.PTR_Sequence = 69;

			AssertPropertyIsUniqueInCollectionValidationError(rule1_1.PTR_SequenceInfo, isExpectingError: false);
			AssertPropertyIsUniqueInCollectionValidationError(rule1_2.PTR_SequenceInfo, isExpectingError: false);

			AssertPropertyIsUniqueInCollectionValidationError(rule2_1.PTR_SequenceInfo, isExpectingError: false);
			AssertPropertyIsUniqueInCollectionValidationError(rule2_2.PTR_SequenceInfo, isExpectingError: false);
		}

		public void TestSequence_TwoWithZeroSequence_ShouldNotBeValid()
		{
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var rule1 = BMSTestHelper.CreateTemplateReleaseGroupRule(template);
			var rule2 = BMSTestHelper.CreateTemplateReleaseGroupRule(template);

			rule1.PTR_Sequence = 0;
			rule2.PTR_Sequence = 0;

			AssertPropertyIsUniqueInCollectionValidationError(rule1.PTR_SequenceInfo, isExpectingError: true);
			AssertPropertyIsUniqueInCollectionValidationError(rule2.PTR_SequenceInfo, isExpectingError: true);

			rule2.PTR_Sequence = 1;

			AssertPropertyIsUniqueInCollectionValidationError(rule1.PTR_SequenceInfo, isExpectingError: false);
			AssertPropertyIsUniqueInCollectionValidationError(rule2.PTR_SequenceInfo, isExpectingError: false);
		}

		public void TestAddRuleForInactiveTemplate_ShouldNotHaveValidationError()
		{
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			Factory.Save();

			template.P0_IsActive = false;

			var rule = BMSTestHelper.CreateTemplateReleaseGroupRule(template);
			rule.Validation.ValidateAll();

			AssertNoErrors(rule);
		}

		public void TestAreAllWorkflowCategoriesApplicable()
		{
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var rule = BMSTestHelper.CreateTemplateReleaseGroupRule(template);

			Factory.Save();

			AssertEquals("Pre-condition", 0, rule.Categories.Count);

			rule.PTR_AreAllWorkflowCategoriesApplicable = false;
			AssertEquals(0, rule.Categories.Count);
			AssertHasError(rule.PTR_AreAllWorkflowCategoriesApplicableInfo, "Please enter at least one applicable workflow category.");

			rule.PTR_AreAllWorkflowCategoriesApplicable = true;
			AssertNoErrors(rule.PTR_AreAllWorkflowCategoriesApplicableInfo);

			rule.Categories.AddNew();
			Factory.Save();

			rule.PTR_AreAllWorkflowCategoriesApplicable = false;
			AssertEquals(1, rule.Categories.Count);
			AssertNoErrors(rule.PTR_AreAllWorkflowCategoriesApplicableInfo);

			rule.PTR_AreAllWorkflowCategoriesApplicable = true;
			AssertEquals("Setting the flag to true also clears the rule's categories", 0, rule.Categories.Count);
			AssertNoErrors(rule.PTR_AreAllWorkflowCategoriesApplicableInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
			TestConfigsHelper.CreateSchematicTestConfig(Factory);
		}
	}
}
