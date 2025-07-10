using System.Diagnostics;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProcessTemplateReleaseGroupRuleMappingValidationTest : BusinessObjectValidationTestCase
	{
		[DeveloperOnlyTest]
		public void TestValueValidationShouldRunQuickly()
		{
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var rule = BMSTestHelper.CreateTemplateReleaseGroupRule(template);
			var value = 1;

			using (rule.GetValidationSuspender())
			{
				for (; value <= 2000; value++)
				{
					BMSTestHelper.CreateTemplateReleaseGroupRuleMapping(rule, value.ToString(), config.ReleaseGroup);
				}
			}

			var newMapping = BMSTestHelper.CreateTemplateReleaseGroupRuleMapping(rule);
			newMapping.PTM_GG_Group = config.ReleaseGroup.PK;
			newMapping.PTM_Value = 25.ToString();

			var sw = Stopwatch.StartNew();
			newMapping.Validation.ValidatePTM_Value();
			var timeElapsedFor2000 = sw.Elapsed;

			AssertPropertyIsUniqueInCollectionValidationError(newMapping.PTM_ValueInfo, isExpectingError: true);

			using (rule.GetValidationSuspender())
			{
				for (; value <= 4000; value++)
				{
					BMSTestHelper.CreateTemplateReleaseGroupRuleMapping(rule, value.ToString(), config.ReleaseGroup);
				}
			}

			newMapping.PTM_Value = 125.ToString();

			sw.Restart();
			newMapping.Validation.ValidatePTM_Value();
			var timeElapsedFor4000 = sw.Elapsed;

			AssertLessThanOrEqualTo("Time to validate when rule has 4000 mappings should be less than double of time to validate with 2000 mappings",
				timeElapsedFor4000.Milliseconds, timeElapsedFor2000.Milliseconds * 2);

			AssertPropertyIsUniqueInCollectionValidationError(newMapping.PTM_ValueInfo, isExpectingError: true);
		}

		[DeveloperOnlyTest]
		public void TestValueRunPreSaveValidationShouldRunQuickly()
		{
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var rule = BMSTestHelper.CreateTemplateReleaseGroupRule(template);
			var value = 1;

			using (rule.GetValidationSuspender())
			{
				for (; value <= 2000; value++)
				{
					BMSTestHelper.CreateTemplateReleaseGroupRuleMapping(rule, value.ToString(), config.ReleaseGroup);
				}
			}

			var newMapping1 = BMSTestHelper.CreateTemplateReleaseGroupRuleMapping(rule);
			newMapping1.PTM_GG_Group = config.ReleaseGroup.PK;
			newMapping1.PTM_Value = 25.ToString();
			var newMapping2 = BMSTestHelper.CreateTemplateReleaseGroupRuleMapping(rule);
			newMapping2.PTM_GG_Group = config.ReleaseGroup.PK;
			newMapping2.PTM_Value = 125.ToString();

			var sw = Stopwatch.StartNew();
			rule.RunPreSaveValidation();
			var timeElapsedFor2000 = sw.Elapsed;

			using (rule.GetValidationSuspender())
			{
				for (; value <= 4000; value++)
				{
					BMSTestHelper.CreateTemplateReleaseGroupRuleMapping(rule, value.ToString(), config.ReleaseGroup);
				}
			}

			sw.Restart();
			rule.RunPreSaveValidation();
			var timeElapsedFor4000 = sw.Elapsed;

			AssertLessThanOrEqualTo("Time to RunPreSaveValidation for 4000 mappings should be less than double of time to 2000 mappings",
			timeElapsedFor4000.Milliseconds, timeElapsedFor2000.Milliseconds * 3);

			AssertPropertyIsUniqueInCollectionValidationError(newMapping1.PTM_ValueInfo, isExpectingError: true);
			AssertPropertyIsUniqueInCollectionValidationError(newMapping2.PTM_ValueInfo, isExpectingError: true);
		}

		public void TestValue_ShouldBeUniquePerRule()
		{
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);

			var rule1 = BMSTestHelper.CreateTemplateReleaseGroupRule(template);
			var rule2 = BMSTestHelper.CreateTemplateReleaseGroupRule(template);

			var mapping1_1 = BMSTestHelper.CreateTemplateReleaseGroupRuleMapping(rule1);
			var mapping1_2 = BMSTestHelper.CreateTemplateReleaseGroupRuleMapping(rule1);

			var mapping2_1 = BMSTestHelper.CreateTemplateReleaseGroupRuleMapping(rule2);
			var mapping2_2 = BMSTestHelper.CreateTemplateReleaseGroupRuleMapping(rule2);

			mapping1_1.PTM_Value = "Who are we?";
			mapping1_2.PTM_Value = "Who are we going to beat?";

			mapping2_1.PTM_Value = "The Wildcats!";
			mapping2_2.PTM_Value = "The Wildcats!";

			AssertNoErrors(mapping1_1);
			AssertNoErrors(mapping1_2);

			AssertPropertyIsUniqueInCollectionValidationError(mapping2_1.PTM_ValueInfo, isExpectingError: true);
			AssertPropertyIsUniqueInCollectionValidationError(mapping2_2.PTM_ValueInfo, isExpectingError: true);

			mapping2_2.PTM_Value = "Now it's time for the easiest part of any coach's job. The cuts. Although I wasn't able to cut everyone I wanted to, I have cut a lot of you. Wendell is cut. Rudy is cut. Janey, you're gone. Steven, I like your hussle. That's why it was so hard to cut you. Congratulations, the rest of you made the team! Except you, you and you.";

			AssertPropertyIsUniqueInCollectionValidationError(mapping2_1.PTM_ValueInfo, isExpectingError: false);
			AssertPropertyIsUniqueInCollectionValidationError(mapping2_2.PTM_ValueInfo, isExpectingError: false);
		}

		#region Implementation

		SchematicTestConfig config;

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
			config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
		}

		#endregion
	}
}
