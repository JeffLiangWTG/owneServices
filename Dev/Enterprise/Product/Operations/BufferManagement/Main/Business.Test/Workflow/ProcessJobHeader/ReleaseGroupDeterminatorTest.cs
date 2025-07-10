using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business.Test
{
	class ReleaseGroupDeterminatorTest_ForWorkItems : BMSTestCaseWithFactory
	{
		#region Job-level Workflows

		public void TestDetermineReleaseGroup_WhenRuleOnGloballyRelevantTemplate_ShouldDetermineReleaseGroupBasedOnRuleMacro()
		{
			var paveTemplate = CreateWorkItemTemplate("PAV", releaseGroupRuleFallbackMethod: FallbackTypeList.Codes.EmptyFallback);
			var coreTemplate = CreateWorkItemTemplate("ARC", releaseGroupRuleFallbackMethod: FallbackTypeList.Codes.EmptyFallback);

			var generalTemplate = CreateWorkItemTemplate();

			MapProductAreasToGroups(generalTemplate, Tuple.Create("PAV", paveTeamGroup), Tuple.Create("ARC", coreTeamGroup));

			Factory.Save();

			var paveWorkItem = CreateWorkItem("PAV");
			var coreWorkItem = CreateWorkItem("ARC");

			Factory.Save();

			AssertExpectedReleaseGroups(paveWorkItem, paveTeamGroup, paveTeamGroup, "A fallback method of EFB on the more specific template should have caused the rule on the less-specific template to be used.");
			AssertExpectedReleaseGroups(coreWorkItem, coreTeamGroup, coreTeamGroup, "A fallback method of EFB on the more specific template should have caused the rule on the less-specific template to be used.");
		}

		public void TestDetermineReleaseGroup_WithCategories_WhenRuleOnGloballyRelevantTemplate_ShouldDetermineReleaseGroupBasedOnRuleMacro()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "BBB", "Berocca");

			var paveTemplate = CreateWorkItemTemplate("PAV", releaseGroupRuleFallbackMethod: FallbackTypeList.Codes.EmptyFallback);
			var coreTemplate = CreateWorkItemTemplate("ARC", releaseGroupRuleFallbackMethod: FallbackTypeList.Codes.EmptyFallback);

			GetOrCreateWorkflowAndTaskWithCategory(paveTemplate, WorkflowDescription, "AAA");
			GetOrCreateWorkflowAndTaskWithCategory(paveTemplate, "Zoot suit riot", "BBB");

			GetOrCreateWorkflowAndTaskWithCategory(coreTemplate, WorkflowDescription, "AAA");
			GetOrCreateWorkflowAndTaskWithCategory(coreTemplate, "Zoot suit riot", "BBB");

			var generalTemplate = CreateWorkItemTemplate();

			var rule = MapProductAreasToGroups(generalTemplate, Tuple.Create("PAV", paveTeamGroup), Tuple.Create("ARC", coreTeamGroup));
			MapCategoriesToRule(rule, "AAA");
			var workflow1 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, WorkflowDescription, "AAA");
			var workflow2 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, "Zoot suit riot", "BBB");

			Factory.Save();

			var paveWorkItem = CreateWorkItem("PAV");
			var coreWorkItem = CreateWorkItem("ARC");

			Factory.Save();

			AssertExpectedReleaseGroups("A fallback method of EFB on the more specific template should have caused the rule on the less-specific template to be used.",
				paveWorkItem, null, Tuple.Create(workflow1, paveTeamGroup), Tuple.Create<ProcessHeader, GlbGroup>(workflow2, null));
			AssertExpectedReleaseGroups("A fallback method of EFB on the more specific template should have caused the rule on the less-specific template to be used.",
				coreWorkItem, null, Tuple.Create(workflow1, coreTeamGroup), Tuple.Create<ProcessHeader, GlbGroup>(workflow2, null));
		}

		public void TestDetermineReleaseGroup_WithCategoriesAndJobMapping_WhenRuleOnGloballyRelevantTemplate_ShouldDetermineReleaseGroupBasedOnRuleMacro()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "BBB", "Berocca");

			var paveTemplate = CreateWorkItemTemplate("PAV", releaseGroupRuleFallbackMethod: FallbackTypeList.Codes.EmptyFallback);
			var coreTemplate = CreateWorkItemTemplate("ARC", releaseGroupRuleFallbackMethod: FallbackTypeList.Codes.EmptyFallback);

			GetOrCreateWorkflowAndTaskWithCategory(paveTemplate, WorkflowDescription, "AAA");
			GetOrCreateWorkflowAndTaskWithCategory(paveTemplate, "Zoot suit riot", "BBB");

			GetOrCreateWorkflowAndTaskWithCategory(coreTemplate, WorkflowDescription, "AAA");
			GetOrCreateWorkflowAndTaskWithCategory(coreTemplate, "Zoot suit riot", "BBB");

			var generalTemplate = CreateWorkItemTemplate();

			var rule = MapProductAreasToGroups(generalTemplate, Tuple.Create("PAV", paveTeamGroup), Tuple.Create("ARC", coreTeamGroup));
			MapCategoriesToRule(rule, "AAA", "JOB");
			var workflow1 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, WorkflowDescription, "AAA");
			var workflow2 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, "Zoot suit riot", "BBB");

			Factory.Save();

			var paveWorkItem = CreateWorkItem("PAV");
			var coreWorkItem = CreateWorkItem("ARC");

			Factory.Save();

			AssertExpectedReleaseGroups("A fallback method of EFB on the more specific template should have caused the rule on the less-specific template to be used.",
				paveWorkItem, paveTeamGroup, Tuple.Create(workflow1, paveTeamGroup), Tuple.Create<ProcessHeader, GlbGroup>(workflow2, null));
			AssertExpectedReleaseGroups("A fallback method of EFB on the more specific template should have caused the rule on the less-specific template to be used.",
				coreWorkItem, coreTeamGroup, Tuple.Create(workflow1, coreTeamGroup), Tuple.Create<ProcessHeader, GlbGroup>(workflow2, null));
		}

		public void TestDetermineReleaseGroup_WhenRuleOnMostSpecificTemplate_ShouldDetermineReleaseGroupBasedOnRuleMacro()
		{
			var paveTemplate = CreateWorkItemTemplate("PAV", releaseGroupRuleFallbackMethod: FallbackTypeList.Codes.EmptyFallback);
			var coreTemplate = CreateWorkItemTemplate("ARC", releaseGroupRuleFallbackMethod: FallbackTypeList.Codes.EmptyFallback);

			var generalTemplate = CreateWorkItemTemplate();

			MapProductAreasToGroups(paveTemplate, Tuple.Create("PAV", paveTeamGroup));
			MapProductAreasToGroups(coreTemplate, Tuple.Create("ARC", coreTeamGroup));

			Factory.Save();

			var paveWorkItem = CreateWorkItem("PAV");
			var coreWorkItem = CreateWorkItem("ARC");

			Factory.Save();

			AssertExpectedReleaseGroups(paveWorkItem, paveTeamGroup, paveTeamGroup, "The rule specified on the most specific template should have been used.");
			AssertExpectedReleaseGroups(coreWorkItem, coreTeamGroup, coreTeamGroup, "The rule specified on the most specific template should have been used.");
		}

		public void TestDetermineReleaseGroup_WithCategories_WhenRuleOnMostSpecificTemplate_ShouldDetermineReleaseGroupBasedOnRuleMacro()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "BBB", "Berocca");

			var paveTemplate = CreateWorkItemTemplate("PAV", releaseGroupRuleFallbackMethod: FallbackTypeList.Codes.EmptyFallback);
			var coreTemplate = CreateWorkItemTemplate("ARC", releaseGroupRuleFallbackMethod: FallbackTypeList.Codes.EmptyFallback);

			var rule1 = MapProductAreasToGroups(paveTemplate, Tuple.Create("PAV", paveTeamGroup), Tuple.Create("ARC", coreTeamGroup));
			MapCategoriesToRule(rule1, "AAA");
			GetOrCreateWorkflowAndTaskWithCategory(paveTemplate, WorkflowDescription, "AAA");
			GetOrCreateWorkflowAndTaskWithCategory(paveTemplate, "Zoot suit riot", "BBB");

			var rule2 = MapProductAreasToGroups(coreTemplate, Tuple.Create("PAV", paveTeamGroup), Tuple.Create("ARC", coreTeamGroup));
			MapCategoriesToRule(rule2, "BBB");
			GetOrCreateWorkflowAndTaskWithCategory(coreTemplate, WorkflowDescription, "AAA");
			GetOrCreateWorkflowAndTaskWithCategory(coreTemplate, "Zoot suit riot", "BBB");

			var generalTemplate = CreateWorkItemTemplate();

			var workflow1 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, WorkflowDescription, "AAA");
			var workflow2 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, "Zoot suit riot", "BBB");

			Factory.Save();

			var paveWorkItem = CreateWorkItem("PAV");
			var coreWorkItem = CreateWorkItem("ARC");

			Factory.Save();

			AssertExpectedReleaseGroups("The rule specified on the most specific template should have been used.",
				paveWorkItem, null, Tuple.Create(workflow1, paveTeamGroup), Tuple.Create<ProcessHeader, GlbGroup>(workflow2, null));
			AssertExpectedReleaseGroups("The rule specified on the most specific template should have been used.",
				coreWorkItem, null, Tuple.Create<ProcessHeader, GlbGroup>(workflow1, null), Tuple.Create(workflow2, coreTeamGroup));
		}

		public void TestDetermineReleaseGroup_WithCategoriesAndJobMapping_WhenRuleOnMostSpecificTemplate_ShouldDetermineReleaseGroupBasedOnRuleMacro()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "BBB", "Berocca");

			var paveTemplate = CreateWorkItemTemplate("PAV", releaseGroupRuleFallbackMethod: FallbackTypeList.Codes.EmptyFallback);
			var coreTemplate = CreateWorkItemTemplate("ARC", releaseGroupRuleFallbackMethod: FallbackTypeList.Codes.EmptyFallback);

			var rule1 = MapProductAreasToGroups(paveTemplate, Tuple.Create("PAV", paveTeamGroup), Tuple.Create("ARC", coreTeamGroup));
			MapCategoriesToRule(rule1, "AAA", "JOB");
			GetOrCreateWorkflowAndTaskWithCategory(paveTemplate, WorkflowDescription, "AAA");
			GetOrCreateWorkflowAndTaskWithCategory(paveTemplate, "Zoot suit riot", "BBB");

			var rule2 = MapProductAreasToGroups(coreTemplate, Tuple.Create("PAV", paveTeamGroup), Tuple.Create("ARC", coreTeamGroup));
			MapCategoriesToRule(rule2, "BBB", "JOB");
			GetOrCreateWorkflowAndTaskWithCategory(coreTemplate, WorkflowDescription, "AAA");
			GetOrCreateWorkflowAndTaskWithCategory(coreTemplate, "Zoot suit riot", "BBB");

			var generalTemplate = CreateWorkItemTemplate();

			var workflow1 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, WorkflowDescription, "AAA");
			var workflow2 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, "Zoot suit riot", "BBB");

			Factory.Save();

			var paveWorkItem = CreateWorkItem("PAV");
			var coreWorkItem = CreateWorkItem("ARC");

			Factory.Save();

			AssertExpectedReleaseGroups("The rule specified on the most specific template should have been used.",
				paveWorkItem, paveTeamGroup, Tuple.Create(workflow1, paveTeamGroup), Tuple.Create<ProcessHeader, GlbGroup>(workflow2, null));
			AssertExpectedReleaseGroups("The rule specified on the most specific template should have been used.",
				coreWorkItem, coreTeamGroup, Tuple.Create<ProcessHeader, GlbGroup>(workflow1, null), Tuple.Create(workflow2, coreTeamGroup));
		}

		public void TestDetermineReleaseGroup_WhenRuleOnlySpecifiedOnLessSpecificTemplate_ShouldConsiderReleaseGroupRuleFallbackMethod()
		{
			var paveTemplate = CreateWorkItemTemplate("PAV", releaseGroupRuleFallbackMethod: FallbackTypeList.Codes.EmptyFallback);
			var coreTemplate = CreateWorkItemTemplate("ARC", releaseGroupRuleFallbackMethod: FallbackTypeList.Codes.NeverFallback); // This template blocks the less-specific one being considered.

			var generalTemplate = CreateWorkItemTemplate();

			MapProductAreasToGroups(generalTemplate, Tuple.Create("PAV", dumpingGroundGroup), Tuple.Create("ARC", dumpingGroundGroup));

			Factory.Save();

			var paveWorkItem = CreateWorkItem("PAV");
			var coreWorkItem = CreateWorkItem("ARC");

			Factory.Save();

			AssertExpectedReleaseGroups(paveWorkItem, dumpingGroundGroup, dumpingGroundGroup, "A fallback method of EFB on the more specific template should have caused the rule on the less-specific template to be used.");
			AssertExpectedReleaseGroups(coreWorkItem, null, null, "A fallback method of NFB on the more specific template should have caused the rule on the less-specific template to NOT be used.");
		}

		public void TestDetermineReleaseGroup_WithCategories_WhenRuleOnlySpecifiedOnLessSpecificTemplate_ShouldConsiderReleaseGroupRuleFallbackMethod()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "BBB", "Berocca");

			var paveTemplate = CreateWorkItemTemplate("PAV", releaseGroupRuleFallbackMethod: FallbackTypeList.Codes.EmptyFallback);
			var coreTemplate = CreateWorkItemTemplate("ARC", releaseGroupRuleFallbackMethod: FallbackTypeList.Codes.NeverFallback); // This template blocks the less-specific one being considered.

			GetOrCreateWorkflowAndTaskWithCategory(paveTemplate, WorkflowDescription, "AAA");
			GetOrCreateWorkflowAndTaskWithCategory(paveTemplate, "Zoot suit riot", "BBB");

			GetOrCreateWorkflowAndTaskWithCategory(coreTemplate, WorkflowDescription, "AAA");
			GetOrCreateWorkflowAndTaskWithCategory(coreTemplate, "Zoot suit riot", "BBB");

			var generalTemplate = CreateWorkItemTemplate();

			var rule = MapProductAreasToGroups(generalTemplate, Tuple.Create("PAV", dumpingGroundGroup), Tuple.Create("ARC", dumpingGroundGroup));
			MapCategoriesToRule(rule, "AAA", "BBB");
			var workflow1 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, WorkflowDescription, "AAA");
			var workflow2 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, "Zoot suit riot", "BBB");

			Factory.Save();

			var paveWorkItem = CreateWorkItem("PAV");
			var coreWorkItem = CreateWorkItem("ARC");

			Factory.Save();

			AssertExpectedReleaseGroups("A fallback method of EFB on the more specific template should have caused the rule on the less-specific template to be used.",
				paveWorkItem, null, Tuple.Create(workflow1, dumpingGroundGroup), Tuple.Create(workflow2, dumpingGroundGroup));
			AssertExpectedReleaseGroups("A fallback method of NFB on the more specific template should have caused the rule on the less-specific template to NOT be used.",
				coreWorkItem, null, Tuple.Create<ProcessHeader, GlbGroup>(workflow1, null), Tuple.Create<ProcessHeader, GlbGroup>(workflow2, null));
		}

		public void TestDetermineReleaseGroup_WithCategoriesAndJobMapping_WhenRuleOnlySpecifiedOnLessSpecificTemplate_ShouldConsiderReleaseGroupRuleFallbackMethod()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "BBB", "Berocca");

			var paveTemplate = CreateWorkItemTemplate("PAV", releaseGroupRuleFallbackMethod: FallbackTypeList.Codes.EmptyFallback);
			var coreTemplate = CreateWorkItemTemplate("ARC", releaseGroupRuleFallbackMethod: FallbackTypeList.Codes.NeverFallback); // This template blocks the less-specific one being considered.

			GetOrCreateWorkflowAndTaskWithCategory(paveTemplate, WorkflowDescription, "AAA");
			GetOrCreateWorkflowAndTaskWithCategory(paveTemplate, "Zoot suit riot", "BBB");

			GetOrCreateWorkflowAndTaskWithCategory(coreTemplate, WorkflowDescription, "AAA");
			GetOrCreateWorkflowAndTaskWithCategory(coreTemplate, "Zoot suit riot", "BBB");

			var generalTemplate = CreateWorkItemTemplate();

			var rule = MapProductAreasToGroups(generalTemplate, Tuple.Create("PAV", dumpingGroundGroup), Tuple.Create("ARC", dumpingGroundGroup));
			MapCategoriesToRule(rule, "AAA", "BBB", "JOB");
			var workflow1 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, WorkflowDescription, "AAA");
			var workflow2 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, "Zoot suit riot", "BBB");

			Factory.Save();

			var paveWorkItem = CreateWorkItem("PAV");
			var coreWorkItem = CreateWorkItem("ARC");

			Factory.Save();

			AssertExpectedReleaseGroups("A fallback method of EFB on the more specific template should have caused the rule on the less-specific template to be used.",
				paveWorkItem, dumpingGroundGroup, Tuple.Create(workflow1, dumpingGroundGroup), Tuple.Create(workflow2, dumpingGroundGroup));
			AssertExpectedReleaseGroups("A fallback method of NFB on the more specific template should have caused the rule on the less-specific template to NOT be used.",
				coreWorkItem, null, Tuple.Create<ProcessHeader, GlbGroup>(workflow1, null), Tuple.Create<ProcessHeader, GlbGroup>(workflow2, null));
		}

		public void TestDetermineReleaseGroup_WhenRuleSpecifiedOnMultipleApplicableTemplates_ShouldConsiderReleaseGroupRuleFallbackMethod_AndResultOfRuleMacro()
		{
			var paveTemplate = CreateWorkItemTemplate("PAV", releaseGroupRuleFallbackMethod: FallbackTypeList.Codes.EmptyFallback);
			var coreTemplate = CreateWorkItemTemplate("ARC", releaseGroupRuleFallbackMethod: FallbackTypeList.Codes.NeverFallback); // This template blocks the less-specific one being considered.

			var generalTemplate = CreateWorkItemTemplate();

			MapProductAreasToGroups(paveTemplate, Tuple.Create("BIL", dumpingGroundGroup));
			MapProductAreasToGroups(coreTemplate, Tuple.Create("SCO", dumpingGroundGroup));
			MapProductAreasToGroups(generalTemplate, Tuple.Create("PAV", paveTeamGroup), Tuple.Create("ARC", coreTeamGroup));

			Factory.Save();

			var paveWorkItem = CreateWorkItem("PAV");
			var coreWorkItem = CreateWorkItem("ARC");

			Factory.Save();

			AssertExpectedReleaseGroups(paveWorkItem, paveTeamGroup, paveTeamGroup, "A fallback method of EFB on the more specific template should have caused the rule on the less-specific template to be used.");
			AssertExpectedReleaseGroups(coreWorkItem, null, null, "A fallback method of NFB on the more specific template should have caused the rule on the less-specific template to NOT be used.");
		}

		public void TestDetermineReleaseGroup_WithCategories_WhenRuleSpecifiedOnMultipleApplicableTemplates_ShouldConsiderReleaseGroupRuleFallbackMethod_AndResultOfRuleMacro()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "BBB", "Berocca");

			var paveTemplate = CreateWorkItemTemplate("PAV", releaseGroupRuleFallbackMethod: FallbackTypeList.Codes.EmptyFallback);
			var coreTemplate = CreateWorkItemTemplate("ARC", releaseGroupRuleFallbackMethod: FallbackTypeList.Codes.NeverFallback); // This template blocks the less-specific one being considered.

			GetOrCreateWorkflowAndTaskWithCategory(paveTemplate, WorkflowDescription, "AAA");
			GetOrCreateWorkflowAndTaskWithCategory(paveTemplate, "Zoot suit riot", "BBB");

			GetOrCreateWorkflowAndTaskWithCategory(coreTemplate, WorkflowDescription, "AAA");
			GetOrCreateWorkflowAndTaskWithCategory(coreTemplate, "Zoot suit riot", "BBB");

			var generalTemplate = CreateWorkItemTemplate();

			var workflow1 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, WorkflowDescription, "AAA");
			var workflow2 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, "Zoot suit riot", "BBB");

			var rule1 = MapProductAreasToGroups(paveTemplate, Tuple.Create("BIL", dumpingGroundGroup));
			MapCategoriesToRule(rule1, "AAA");
			var rule2 = MapProductAreasToGroups(coreTemplate, Tuple.Create("SCO", dumpingGroundGroup));
			MapCategoriesToRule(rule2, "BBB");
			var rule3 = MapProductAreasToGroups(generalTemplate, Tuple.Create("PAV", paveTeamGroup), Tuple.Create("ARC", coreTeamGroup));
			MapCategoriesToRule(rule3, "AAA");

			Factory.Save();

			var paveWorkItem = CreateWorkItem("PAV");
			var coreWorkItem = CreateWorkItem("ARC");

			Factory.Save();

			AssertExpectedReleaseGroups("A fallback method of EFB on the more specific template should have caused the rule on the less-specific template to be used.",
				paveWorkItem, null, Tuple.Create(workflow1, paveTeamGroup), Tuple.Create<ProcessHeader, GlbGroup>(workflow2, null));
			AssertExpectedReleaseGroups("A fallback method of NFB on the more specific template should have caused the rule on the less-specific template to NOT be used.",
				coreWorkItem, null, Tuple.Create<ProcessHeader, GlbGroup>(workflow1, null), Tuple.Create<ProcessHeader, GlbGroup>(workflow2, null));
		}

		public void TestDetermineReleaseGroup_WithCategoriesAndJobMapping_WhenRuleSpecifiedOnMultipleApplicableTemplates_ShouldConsiderReleaseGroupRuleFallbackMethod_AndResultOfRuleMacro()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "BBB", "Berocca");

			var paveTemplate = CreateWorkItemTemplate("PAV", releaseGroupRuleFallbackMethod: FallbackTypeList.Codes.EmptyFallback);
			var coreTemplate = CreateWorkItemTemplate("ARC", releaseGroupRuleFallbackMethod: FallbackTypeList.Codes.NeverFallback); // This template blocks the less-specific one being considered.

			GetOrCreateWorkflowAndTaskWithCategory(paveTemplate, WorkflowDescription, "AAA");
			GetOrCreateWorkflowAndTaskWithCategory(paveTemplate, "Zoot suit riot", "BBB");

			GetOrCreateWorkflowAndTaskWithCategory(coreTemplate, WorkflowDescription, "AAA");
			GetOrCreateWorkflowAndTaskWithCategory(coreTemplate, "Zoot suit riot", "BBB");

			var generalTemplate = CreateWorkItemTemplate();

			var workflow1 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, WorkflowDescription, "AAA");
			var workflow2 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, "Zoot suit riot", "BBB");

			var rule1 = MapProductAreasToGroups(paveTemplate, Tuple.Create("BIL", dumpingGroundGroup));
			MapCategoriesToRule(rule1, "AAA", "JOB");
			var rule2 = MapProductAreasToGroups(coreTemplate, Tuple.Create("SCO", dumpingGroundGroup));
			MapCategoriesToRule(rule2, "BBB", "JOB");
			var rule3 = MapProductAreasToGroups(generalTemplate, Tuple.Create("PAV", paveTeamGroup), Tuple.Create("ARC", coreTeamGroup));
			MapCategoriesToRule(rule3, "AAA", "JOB");

			Factory.Save();

			var paveWorkItem = CreateWorkItem("PAV");
			var coreWorkItem = CreateWorkItem("ARC");

			Factory.Save();

			AssertExpectedReleaseGroups("A fallback method of EFB on the more specific template should have caused the rule on the less-specific template to be used.",
				paveWorkItem, paveTeamGroup, Tuple.Create(workflow1, paveTeamGroup), Tuple.Create<ProcessHeader, GlbGroup>(workflow2, null));
			AssertExpectedReleaseGroups("A fallback method of NFB on the more specific template should have caused the rule on the less-specific template to NOT be used.",
				coreWorkItem, null, Tuple.Create<ProcessHeader, GlbGroup>(workflow1, null), Tuple.Create<ProcessHeader, GlbGroup>(workflow2, null));
		}

		public void TestDetermineReleaseGroup_WhenMultipleRulesSpecifiedOnSingleTemplate_ShouldConsiderRulesInSequence()
		{
			var generalTemplate = CreateWorkItemTemplate();

			var rule1 = MapProductAreasToGroups(generalTemplate, Tuple.Create("PAV", paveTeamGroup), Tuple.Create("ARC", coreTeamGroup));
			var rule2 = MapProductAreasToGroups(generalTemplate, Tuple.Create("PAV", coreTeamGroup), Tuple.Create("ARC", paveTeamGroup), Tuple.Create("GLW", glowTeamGroup)); // Mixes around the results from the first rule, but has a useful mapping of the GLW to GLOWRG config.

			AssertEquals((ZByte)1, rule1.PTR_Sequence);
			AssertEquals((ZByte)2, rule2.PTR_Sequence);

			Factory.Save();

			var paveWorkItem = CreateWorkItem("PAV");
			var coreWorkItem = CreateWorkItem("ARC");
			var glowWorkItem = CreateWorkItem("GLW");

			Factory.Save();

			AssertExpectedReleaseGroups(paveWorkItem, paveTeamGroup, paveTeamGroup, "The first rule should be used to map PAV to PAVERG (the second rule would have mapped PAV to CORERG).");
			AssertExpectedReleaseGroups(coreWorkItem, coreTeamGroup, coreTeamGroup, "The first rule should be used to map ARC to CORERG (the second rule would have mapped ARC to PAVERG).");
			AssertExpectedReleaseGroups(glowWorkItem, glowTeamGroup, glowTeamGroup, "The second rule should be used to map GLW to GLOWRG.");

			DeleteAllWorkflowsInAnotherFactory(paveWorkItem);
			DeleteAllWorkflowsInAnotherFactory(coreWorkItem);
			DeleteAllWorkflowsInAnotherFactory(glowWorkItem);

			AssertExpectedReleaseGroups(paveWorkItem, paveTeamGroup, paveTeamGroup, "Reapplying the rules should be the same as before.");
			AssertExpectedReleaseGroups(coreWorkItem, coreTeamGroup, coreTeamGroup, "Reapplying the rules should be the same as before.");
			AssertExpectedReleaseGroups(glowWorkItem, glowTeamGroup, glowTeamGroup, "The second rule should be used to map GLW to GLOWRG.");
		}

		public void TestDetermineReleaseGroup_WhenMultipleRulesSpecifiedOnSingleTemplate_ShouldOnlyConsiderActiveRules()
		{
			var generalTemplate = CreateWorkItemTemplate();

			var rule1 = MapProductAreasToGroups(generalTemplate, Tuple.Create("PAV", paveTeamGroup), Tuple.Create("ARC", coreTeamGroup));
			var rule2 = MapProductAreasToGroups(generalTemplate, Tuple.Create("PAV", glowTeamGroup), Tuple.Create("ARC", paveTeamGroup), Tuple.Create("GLW", coreTeamGroup));
			rule2.PTR_IsActive = false;
			var rule3 = MapProductAreasToGroups(generalTemplate, Tuple.Create("PAV", coreTeamGroup), Tuple.Create("ARC", paveTeamGroup), Tuple.Create("GLW", glowTeamGroup));

			AssertEquals((ZByte)1, rule1.PTR_Sequence);
			AssertEquals((ZByte)2, rule2.PTR_Sequence);
			AssertEquals((ZByte)3, rule3.PTR_Sequence);

			Factory.Save();

			var paveWorkItem = CreateWorkItem("PAV");
			var coreWorkItem = CreateWorkItem("ARC");
			var glowWorkItem = CreateWorkItem("GLW");

			Factory.Save();

			AssertExpectedReleaseGroups(paveWorkItem, paveTeamGroup, paveTeamGroup, "The first rule should be used to map PAV to PAVERG (the third rule would have mapped PAV to CORERG).");
			AssertExpectedReleaseGroups(coreWorkItem, coreTeamGroup, coreTeamGroup, "The first rule should be used to map ARC to CORERG (the third rule would have mapped ARC to PAVERG).");
			AssertExpectedReleaseGroups(glowWorkItem, glowTeamGroup, glowTeamGroup, "The third (NOT second, inactive) rule should be used to map GLW to GLOWRG.");
		}

		public void TestDetermineReleaseGroup_WithCategories_WhenMultipleRulesSpecifiedOnSingleTemplate_ShouldConsiderRulesInSequence()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "BBB", "Berocca");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "CCC", "Come on");

			var generalTemplate = CreateWorkItemTemplate();

			var workflow1 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, WorkflowDescription, "AAA");
			var workflow2 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, "Zoot suit riot", "BBB");
			var workflow3 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, "Throw back a bottle of beer", "CCC");

			var rule1 = MapProductAreasToGroups(generalTemplate, Tuple.Create("PAV", paveTeamGroup), Tuple.Create("ARC", coreTeamGroup));
			MapCategoriesToRule(rule1, "AAA", "BBB");
			var rule2 = MapProductAreasToGroups(generalTemplate, Tuple.Create("PAV", coreTeamGroup), Tuple.Create("ARC", paveTeamGroup), Tuple.Create("GLW", glowTeamGroup)); // Mixes around the results from the first rule, but has a useful mapping of the GLW to GLOWRG config.
			MapCategoriesToRule(rule2, "BBB", "CCC");

			AssertEquals((ZByte)1, rule1.PTR_Sequence);
			AssertEquals((ZByte)2, rule2.PTR_Sequence);

			Factory.Save();

			var paveWorkItem = CreateWorkItem("PAV");
			var coreWorkItem = CreateWorkItem("ARC");
			var glowWorkItem = CreateWorkItem("GLW");

			Factory.Save();

			AssertExpectedReleaseGroups("The first and second rules should be used to allocate release groups to the PAVE work item.",
				paveWorkItem, null, Tuple.Create(workflow1, paveTeamGroup), Tuple.Create(workflow2, paveTeamGroup), Tuple.Create(workflow3, coreTeamGroup));
			AssertExpectedReleaseGroups("The first and second rules should be used to allocate release groups to the Core work item.",
				coreWorkItem, null, Tuple.Create(workflow1, coreTeamGroup), Tuple.Create(workflow2, coreTeamGroup), Tuple.Create(workflow3, paveTeamGroup));
			AssertExpectedReleaseGroups("The second rule only should be used to allocate release groups to the GLOW work item.",
				glowWorkItem, null, Tuple.Create<ProcessHeader, GlbGroup>(workflow1, null), Tuple.Create(workflow2, glowTeamGroup), Tuple.Create(workflow3, glowTeamGroup));

			DeleteAllWorkflowsInAnotherFactory(paveWorkItem);
			DeleteAllWorkflowsInAnotherFactory(coreWorkItem);
			DeleteAllWorkflowsInAnotherFactory(glowWorkItem);

			AssertExpectedReleaseGroups("The first and second rules should be used to allocate release groups to the PAVE work item.",
				paveWorkItem, null, Tuple.Create(workflow1, paveTeamGroup), Tuple.Create(workflow2, paveTeamGroup), Tuple.Create(workflow3, coreTeamGroup));
			AssertExpectedReleaseGroups("The first and second rules should be used to allocate release groups to the Core work item.",
				coreWorkItem, null, Tuple.Create(workflow1, coreTeamGroup), Tuple.Create(workflow2, coreTeamGroup), Tuple.Create(workflow3, paveTeamGroup));
			AssertExpectedReleaseGroups("The second rule only should be used to allocate release groups to the GLOW work item.",
				glowWorkItem, null, Tuple.Create<ProcessHeader, GlbGroup>(workflow1, null), Tuple.Create(workflow2, glowTeamGroup), Tuple.Create(workflow3, glowTeamGroup));
		}

		public void TestDetermineReleaseGroup_WithCategoriesAndJobMapping_WhenMultipleRulesSpecifiedOnSingleTemplate_ShouldConsiderRulesInSequence()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "BBB", "Berocca");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "CCC", "Come on");

			var generalTemplate = CreateWorkItemTemplate();

			var workflow1 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, WorkflowDescription, "AAA");
			var workflow2 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, "Zoot suit riot", "BBB");
			var workflow3 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, "Throw back a bottle of beer", "CCC");

			var rule1 = MapProductAreasToGroups(generalTemplate, Tuple.Create("PAV", paveTeamGroup), Tuple.Create("ARC", coreTeamGroup));
			MapCategoriesToRule(rule1, "AAA", "BBB", "JOB");
			var rule2 = MapProductAreasToGroups(generalTemplate, Tuple.Create("PAV", coreTeamGroup), Tuple.Create("ARC", paveTeamGroup), Tuple.Create("GLW", glowTeamGroup)); // Mixes around the results from the first rule, but has a useful mapping of the GLW to GLOWRG config.
			MapCategoriesToRule(rule2, "BBB", "CCC", "JOB");

			AssertEquals((ZByte)1, rule1.PTR_Sequence);
			AssertEquals((ZByte)2, rule2.PTR_Sequence);

			Factory.Save();

			var paveWorkItem = CreateWorkItem("PAV");
			var coreWorkItem = CreateWorkItem("ARC");
			var glowWorkItem = CreateWorkItem("GLW");

			Factory.Save();

			AssertExpectedReleaseGroups("The first and second rules should be used to allocate release groups to the PAVE work item.",
				paveWorkItem, paveTeamGroup, Tuple.Create(workflow1, paveTeamGroup), Tuple.Create(workflow2, paveTeamGroup), Tuple.Create(workflow3, coreTeamGroup));
			AssertExpectedReleaseGroups("The first and second rules should be used to allocate release groups to the Core work item.",
				coreWorkItem, coreTeamGroup, Tuple.Create(workflow1, coreTeamGroup), Tuple.Create(workflow2, coreTeamGroup), Tuple.Create(workflow3, paveTeamGroup));
			AssertExpectedReleaseGroups("The second rule only should be used to allocate release groups to the GLOW work item.",
				glowWorkItem, glowTeamGroup, Tuple.Create<ProcessHeader, GlbGroup>(workflow1, null), Tuple.Create(workflow2, glowTeamGroup), Tuple.Create(workflow3, glowTeamGroup));

			DeleteAllWorkflowsInAnotherFactory(paveWorkItem);
			DeleteAllWorkflowsInAnotherFactory(coreWorkItem);
			DeleteAllWorkflowsInAnotherFactory(glowWorkItem);

			AssertExpectedReleaseGroups("The first and second rules should be used to allocate release groups to the PAVE work item.",
				paveWorkItem, paveTeamGroup, Tuple.Create(workflow1, paveTeamGroup), Tuple.Create(workflow2, paveTeamGroup), Tuple.Create(workflow3, coreTeamGroup));
			AssertExpectedReleaseGroups("The first and second rules should be used to allocate release groups to the Core work item.",
				coreWorkItem, coreTeamGroup, Tuple.Create(workflow1, coreTeamGroup), Tuple.Create(workflow2, coreTeamGroup), Tuple.Create(workflow3, paveTeamGroup));
			AssertExpectedReleaseGroups("The second rule only should be used to allocate release groups to the GLOW work item.",
				glowWorkItem, glowTeamGroup, Tuple.Create<ProcessHeader, GlbGroup>(workflow1, null), Tuple.Create(workflow2, glowTeamGroup), Tuple.Create(workflow3, glowTeamGroup));
		}

		public void TestDetermineReleaseGroup_WithCategories_WhenMultipleRulesSpecifiedOnSingleTemplateAndDefaultRule_ShouldApplyDefaultRule()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "BBB", "Berocca");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "CCC", "Come on");

			var generalTemplate = CreateWorkItemTemplate();

			var workflow1 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, WorkflowDescription, "AAA");
			var workflow2 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, "Zoot suit riot", "BBB");
			var workflow3 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, "Throw back a bottle of beer", "CCC");

			var rule1 = MapProductAreasToGroups(generalTemplate, Tuple.Create("PAV", paveTeamGroup), Tuple.Create("ARC", coreTeamGroup));
			MapCategoriesToRule(rule1, "AAA", "BBB");
			var rule2 = MapProductAreasToGroups(generalTemplate, Tuple.Create("PAV", coreTeamGroup), Tuple.Create("ARC", paveTeamGroup), Tuple.Create("GLW", glowTeamGroup)); // Mixes around the results from the first rule, but has a useful mapping of the GLW to GLOWRG config.
			MapCategoriesToRule(rule2, "BBB", "CCC");
			var defaultRule = BMSTestHelper.CreateTemplateReleaseGroupRule(generalTemplate);
			BMSTestHelper.CreateTemplateReleaseGroupRuleMapping(defaultRule, string.Empty, defaultGroup);

			AssertEquals((ZByte)1, rule1.PTR_Sequence);
			AssertEquals((ZByte)2, rule2.PTR_Sequence);
			AssertEquals((ZByte)3, defaultRule.PTR_Sequence);

			AssertEquals(false, rule1.PTR_AreAllWorkflowCategoriesApplicable);
			AssertEquals(false, rule2.PTR_AreAllWorkflowCategoriesApplicable);
			AssertEquals(true, defaultRule.PTR_AreAllWorkflowCategoriesApplicable);

			Factory.Save();

			var paveWorkItem = CreateWorkItem("PAV");
			var coreWorkItem = CreateWorkItem("ARC");
			var glowWorkItem = CreateWorkItem("GLW");

			Factory.Save();

			AssertExpectedReleaseGroups("The first and second rules and the default rule should be used to allocate release groups to the PAVE work item.",
				paveWorkItem, defaultGroup, Tuple.Create(workflow1, paveTeamGroup), Tuple.Create(workflow2, paveTeamGroup), Tuple.Create(workflow3, coreTeamGroup));
			AssertExpectedReleaseGroups("The first and second rules and the default rule should be used to allocate release groups to the Core work item.",
				coreWorkItem, defaultGroup, Tuple.Create(workflow1, coreTeamGroup), Tuple.Create(workflow2, coreTeamGroup), Tuple.Create(workflow3, paveTeamGroup));
			AssertExpectedReleaseGroups("The second rule and the default rule should be used to allocate release groups to the GLOW work item.",
				glowWorkItem, defaultGroup, Tuple.Create(workflow1, defaultGroup), Tuple.Create(workflow2, glowTeamGroup), Tuple.Create(workflow3, glowTeamGroup));
		}

		public void TestDetermineReleaseGroup_WithCategoriesAndJobMapping_WhenMultipleRulesSpecifiedOnSingleTemplateAndDefaultRule_ShouldApplyDefaultRule()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "BBB", "Berocca");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "CCC", "Come on");

			var generalTemplate = CreateWorkItemTemplate();

			var workflow1 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, WorkflowDescription, "AAA");
			var workflow2 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, "Zoot suit riot", "BBB");
			var workflow3 = GetOrCreateWorkflowAndTaskWithCategory(generalTemplate, "Throw back a bottle of beer", "CCC");

			var rule1 = MapProductAreasToGroups(generalTemplate, Tuple.Create("PAV", paveTeamGroup), Tuple.Create("ARC", coreTeamGroup));
			MapCategoriesToRule(rule1, "AAA", "BBB", "JOB");
			var rule2 = MapProductAreasToGroups(generalTemplate, Tuple.Create("PAV", coreTeamGroup), Tuple.Create("ARC", paveTeamGroup), Tuple.Create("GLW", glowTeamGroup)); // Mixes around the results from the first rule, but has a useful mapping of the GLW to GLOWRG config.
			MapCategoriesToRule(rule2, "BBB", "CCC", "JOB");
			var defaultRule = BMSTestHelper.CreateTemplateReleaseGroupRule(generalTemplate);
			BMSTestHelper.CreateTemplateReleaseGroupRuleMapping(defaultRule, string.Empty, defaultGroup);

			AssertEquals((ZByte)1, rule1.PTR_Sequence);
			AssertEquals((ZByte)2, rule2.PTR_Sequence);
			AssertEquals((ZByte)3, defaultRule.PTR_Sequence);

			AssertEquals(false, rule1.PTR_AreAllWorkflowCategoriesApplicable);
			AssertEquals(false, rule2.PTR_AreAllWorkflowCategoriesApplicable);
			AssertEquals(true, defaultRule.PTR_AreAllWorkflowCategoriesApplicable);

			Factory.Save();

			var paveWorkItem = CreateWorkItem("PAV");
			var coreWorkItem = CreateWorkItem("ARC");
			var glowWorkItem = CreateWorkItem("GLW");

			Factory.Save();

			AssertExpectedReleaseGroups("The first and second rules and the default rule should be used to allocate release groups to the PAVE work item.",
				paveWorkItem, paveTeamGroup, Tuple.Create(workflow1, paveTeamGroup), Tuple.Create(workflow2, paveTeamGroup), Tuple.Create(workflow3, coreTeamGroup));
			AssertExpectedReleaseGroups("The first and second rules and the default rule should be used to allocate release groups to the Core work item.",
				coreWorkItem, coreTeamGroup, Tuple.Create(workflow1, coreTeamGroup), Tuple.Create(workflow2, coreTeamGroup), Tuple.Create(workflow3, paveTeamGroup));
			AssertExpectedReleaseGroups("The second rule and the default rule should be used to allocate release groups to the GLOW work item.",
				glowWorkItem, glowTeamGroup, Tuple.Create(workflow1, defaultGroup), Tuple.Create(workflow2, glowTeamGroup), Tuple.Create(workflow3, glowTeamGroup));
		}

		public void TestDetermineReleaseGroup_WhenRuleMacroIsEmpty_AllJobsShouldUseThatReleaseGroup()
		{
			var generalTemplate = CreateWorkItemTemplate();

			MapProductAreasToGroups(generalTemplate, Tuple.Create("PAV", paveTeamGroup), Tuple.Create("ARC", coreTeamGroup), Tuple.Create("", emptyProductAreaGroup));

			var emptyRule = BMSTestHelper.CreateTemplateReleaseGroupRule(generalTemplate, valueSelectionMacro: "");
			BMSTestHelper.CreateTemplateReleaseGroupRuleMapping(emptyRule, value: "", mappedGroup: dumpingGroundGroup);

			generalTemplate.RunPreSaveValidation();
			AssertNoErrors("There's nothing wrong with an empty macro. It'll just produce an empty value, which we're using to select a catch-all group for anything that didn't find a mapping.", generalTemplate);

			Factory.Save();

			var paveWorkItem = CreateWorkItem("PAV");
			var coreWorkItem = CreateWorkItem("ARC");
			var glowWorkItem = CreateWorkItem("GLW");
			var ropeWorkItem = CreateWorkItem("MDM");
			var emptyProductAreaWorkItem = CreateWorkItem("");

			Factory.Save();

			AssertExpectedReleaseGroups(paveWorkItem, paveTeamGroup, paveTeamGroup, "PAVE work items are mapped to a specific group.");
			AssertExpectedReleaseGroups(coreWorkItem, coreTeamGroup, coreTeamGroup, "Core work items are mapped to a specific group.");
			AssertExpectedReleaseGroups(glowWorkItem, dumpingGroundGroup, dumpingGroundGroup, "GLOW work items will end up in the dumping ground group.");
			AssertExpectedReleaseGroups(ropeWorkItem, dumpingGroundGroup, dumpingGroundGroup, "ROPE work items will end up in the dumping ground group.");
			AssertExpectedReleaseGroups(emptyProductAreaWorkItem, emptyProductAreaGroup, emptyProductAreaGroup, "Empty product area work items will end up in the empty product area group.");
		}

		#endregion

		#region Regular Workflows

		public void TestDetermineReleaseGroup_ForWorkflow_WhenJobLevelWorkflowHasReleaseGroupSpecified()
		{
			var job = (IWorkflowProvider)CreateWorkItem(null);
			var jobHeader = BMSTestHelper.CreateJobHeader(job, addDefaultProcessHeaderIfNone: false);

			jobHeader.FH_GG_ReleaseGroup = paveTeamGroup.PK;

			var workflow = jobHeader.ProcessHeaders.AddNew();

			AssertEquals("Creating a new workflow should copy the release group of its job-level workflow, if present", paveTeamGroup.GG_Code, workflow.ReleaseGroup?.GG_Code);
		}

		public void TestDetermineReleaseGroup_ForWorkflow_WhenWorkflowHasReleaseGroupSpecified()
		{
			var job = (IWorkflowProvider)CreateWorkItem(null);
			var jobHeader = BMSTestHelper.CreateJobHeader(job, addDefaultProcessHeaderIfNone: false);

			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_GG_ReleaseGroup = paveTeamGroup.PK;

			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			AssertEquals("Creating another workflow should copy the release group of the previous workflow, which had a release group specified.", paveTeamGroup.GG_Code, workflow2.ReleaseGroup?.GG_Code);
		}

		public void TestDetermineReleaseGroup_ForWorkflow_WhenJobLevelWorkflowReleaseGroupIsEmpty()
		{
			var job = (IWorkflowProvider)CreateWorkItem(null);
			var jobHeader = BMSTestHelper.CreateJobHeader(job, addDefaultProcessHeaderIfNone: false);
			var workflow = jobHeader.ProcessHeaders.AddNew();

			AssertNull(jobHeader.ReleaseGroup);
			AssertNull(workflow.ReleaseGroup);
		}

		public void TestDetermineReleaseGroup_ForWorkflowAppliedFromTemplate_WithReleaseGroupSpecifiedOnTemplateWorkflowDirectly_AndRuleSpecifiedOnTemplate()
		{
			var paveTemplate = CreateWorkItemTemplate("PAV");

			MapProductAreasToGroups(paveTemplate, Tuple.Create("PAV", paveTeamGroup));

			paveTemplate.ProcessHeaders.Cast<ProcessHeader>().Single(w => w.FH_CompletionStatement == WorkflowDescription).FH_GG_ReleaseGroup = coreTeamGroup.PK;

			Factory.Save();

			var paveWorkItem = CreateWorkItem("PAV");
			Factory.Save();

			AssertExpectedReleaseGroups(paveWorkItem, paveTeamGroup, coreTeamGroup, "A release group specified directly on the template workflow always overrides what is determined for the job-level workflow by Release Group Rules.");
		}

		public void TestDetermineReleaseGroup_ForWorkflowAppliedFromTemplate_WithReleaseGroupSpecifiedOnTemplateJobLevelWorkflowDirectly_AndRuleSpecifiedOnTemplate()
		{
			var paveTemplate = CreateWorkItemTemplate("PAV");

			MapProductAreasToGroups(paveTemplate, Tuple.Create("PAV", paveTeamGroup));

			paveTemplate.GetJobHeader().FH_GG_ReleaseGroup = coreTeamGroup.PK;

			Factory.Save();

			var paveWorkItem = CreateWorkItem("PAV");
			Factory.Save();

			AssertExpectedReleaseGroups(paveWorkItem, coreTeamGroup, coreTeamGroup, "A release group specified directly on the template job-level workflow always overrides what is determined by Release Group Rules. This should also be copied to workflows on the job.");
		}

		public void TestDetermineReleaseGroup_WithCategories_ForWorkflowAppliedFromTemplate_WithReleaseGroupSpecifiedOnTemplateWorkflowDirectly_AndRuleSpecifiedOnTemplate()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");

			var paveTemplate = CreateWorkItemTemplate("PAV");

			var rule = MapProductAreasToGroups(paveTemplate, Tuple.Create("PAV", paveTeamGroup));
			MapCategoriesToRule(rule, "AAA");

			var workflow = paveTemplate.ProcessHeaders.Cast<ProcessHeader>().Single(w => w.FH_CompletionStatement == WorkflowDescription);
			workflow.FH_GG_ReleaseGroup = coreTeamGroup.PK;
			workflow.FH_Category = "AAA";

			Factory.Save();

			var paveWorkItem = CreateWorkItem("PAV");
			Factory.Save();

			AssertExpectedReleaseGroups(paveWorkItem, null, coreTeamGroup, "A release group specified directly on the template workflow always overrides what is determined for the job-level workflow by Release Group Rules.");
		}

		public void TestDetermineReleaseGroup_WithCategories_ForWorkflowAppliedFromTemplate_WithReleaseGroupSpecifiedOnTemplateJobLevelWorkflowDirectly_AndRuleSpecifiedOnTemplate()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");

			var paveTemplate = CreateWorkItemTemplate("PAV");

			var rule = MapProductAreasToGroups(paveTemplate, Tuple.Create("PAV", paveTeamGroup));
			MapCategoriesToRule(rule, "AAA");

			var jobHeader = paveTemplate.GetJobHeader();
			jobHeader.FH_GG_ReleaseGroup = coreTeamGroup.PK;

			Factory.Save();

			var paveWorkItem = CreateWorkItem("PAV");
			Factory.Save();

			AssertExpectedReleaseGroups(paveWorkItem, coreTeamGroup, coreTeamGroup, "A release group specified directly on the template job-level workflow always overrides what is determined by Release Group Rules. This should also be copied to workflows on the job.");
		}

		#endregion

		#region Updating Release Groups

		public void TestChangeWorkItemSelectionCriteria_WhenReleaseGroupRuleSpecifiedForNewCriteriaOnly_ShouldChangeReleaseGroup()
		{
			var template = CreateWorkItemTemplate();

			MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup));

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			AssertExpectedReleaseGroups(workItem, null, null, "No matching release group rules, so should be left blank.");

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, coreTeamGroup, coreTeamGroup, "The release group rule should have cause the release group to be re-calculated.");
		}

		public void TestChangeWorkItemSelectionCriteria_WithCategories_WhenReleaseGroupRuleSpecifiedForNewCriteriaOnly_ShouldChangeReleaseGroup()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");

			var template = CreateWorkItemTemplate();

			var rule = MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup));
			MapCategoriesToRule(rule, "AAA");

			GetOrCreateWorkflowAndTaskWithCategory(template, WorkflowDescription, "AAA");

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			AssertExpectedReleaseGroups(workItem, null, null, "No matching release group rules, so should be left blank.");

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, null, coreTeamGroup, "The release group rule should have cause the release group to be re-calculated.");
		}

		public void TestChangeWorkItemSelectionCriteria_WithCategoriesAndJobMapping_WhenReleaseGroupRuleSpecifiedForNewCriteriaOnly_ShouldChangeReleaseGroup()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");

			var template = CreateWorkItemTemplate();

			var rule = MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup));
			MapCategoriesToRule(rule, "AAA", "JOB");

			GetOrCreateWorkflowAndTaskWithCategory(template, WorkflowDescription, "AAA");

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			AssertExpectedReleaseGroups(workItem, null, null, "No matching release group rules, so should be left blank.");

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, coreTeamGroup, coreTeamGroup, "The release group rule should have cause the release group to be re-calculated.");
		}

		public void TestChangeWorkItemSelectionCriteria_WhenReleaseGroupRuleSpecifiedForOldAndNewCriteria_ShouldChangeReleaseGroup()
		{
			var template = CreateWorkItemTemplate();

			MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup), Tuple.Create("PAV", paveTeamGroup));

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			AssertExpectedReleaseGroups(workItem, paveTeamGroup, paveTeamGroup, "Initially-matched release group rule should have been applied.");

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, coreTeamGroup, coreTeamGroup, "The release group rule should have cause the release group to be re-calculated.");
		}

		public void TestChangeWorkItemSelectionCriteria_WithCategories_WhenReleaseGroupRuleSpecifiedForOldAndNewCriteria_ShouldChangeReleaseGroup()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "BBB", "Berocca");

			var template = CreateWorkItemTemplate();

			var rule = MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup), Tuple.Create("PAV", paveTeamGroup));
			MapCategoriesToRule(rule, "AAA", "BBB");

			var workflow1 = GetOrCreateWorkflowAndTaskWithCategory(template, WorkflowDescription, "AAA");
			var workflow2 = GetOrCreateWorkflowAndTaskWithCategory(template, "Zoot suit riot", "BBB");

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			AssertExpectedReleaseGroups("Initially-matched release group rule should have been applied.",
				workItem, null, Tuple.Create(workflow1, paveTeamGroup), Tuple.Create(workflow2, paveTeamGroup));

			ChangeProductAreaInAnotherFactory(workItem, "ARC");

			AssertExpectedReleaseGroups("The release group rule should have cause the release group to be re-calculated.",
				workItem, null, Tuple.Create(workflow1, coreTeamGroup), Tuple.Create(workflow2, coreTeamGroup));
		}

		public void TestChangeWorkItemSelectionCriteria_WithCategoriesAndJobMapping_WhenReleaseGroupRuleSpecifiedForOldAndNewCriteria_ShouldChangeReleaseGroup()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "BBB", "Berocca");

			var template = CreateWorkItemTemplate();

			var rule = MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup), Tuple.Create("PAV", paveTeamGroup));
			MapCategoriesToRule(rule, "AAA", "BBB", "JOB");

			var workflow1 = GetOrCreateWorkflowAndTaskWithCategory(template, WorkflowDescription, "AAA");
			var workflow2 = GetOrCreateWorkflowAndTaskWithCategory(template, "Zoot suit riot", "BBB");

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			AssertExpectedReleaseGroups("Initially-matched release group rule should have been applied.",
				workItem, paveTeamGroup, Tuple.Create(workflow1, paveTeamGroup), Tuple.Create(workflow2, paveTeamGroup));

			ChangeProductAreaInAnotherFactory(workItem, "ARC");

			AssertExpectedReleaseGroups("The release group rule should have cause the release group to be re-calculated.",
				workItem, coreTeamGroup, Tuple.Create(workflow1, coreTeamGroup), Tuple.Create(workflow2, coreTeamGroup));
		}

		public void TestChangeWorkItemSelectionCriteria_WhenReleaseGroupRuleSpecifiedForOldCriteriaOnly_ShouldNotChangeReleaseGroup()
		{
			var template = CreateWorkItemTemplate();

			MapProductAreasToGroups(template, Tuple.Create("PAV", paveTeamGroup));

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			AssertExpectedReleaseGroups(workItem, paveTeamGroup, paveTeamGroup, "Initially-matched release group rule should have been applied.");

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, paveTeamGroup, paveTeamGroup, "The release group rule doesn't find a match, so nothing should be changed.");
		}

		public void TestChangeWorkItemSelectionCriteria_WithCategories_WhenReleaseGroupRuleSpecifiedForOldCriteriaOnly_ShouldNotChangeReleaseGroup()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");

			var template = CreateWorkItemTemplate();

			var rule = MapProductAreasToGroups(template, Tuple.Create("PAV", paveTeamGroup));
			MapCategoriesToRule(rule, "AAA");

			GetOrCreateWorkflowAndTaskWithCategory(template, WorkflowDescription, "AAA");

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			AssertExpectedReleaseGroups(workItem, null, paveTeamGroup, "Initially-matched release group rule should have been applied.");

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, null, paveTeamGroup, "The release group rule doesn't find a match, so nothing should be changed.");
		}

		public void TestChangeWorkItemSelectionCriteria_WithCategoriesAndJobMapping_WhenReleaseGroupRuleSpecifiedForOldCriteriaOnly_ShouldNotChangeReleaseGroup()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");

			var template = CreateWorkItemTemplate();

			var rule = MapProductAreasToGroups(template, Tuple.Create("PAV", paveTeamGroup));
			MapCategoriesToRule(rule, "AAA", "JOB");

			GetOrCreateWorkflowAndTaskWithCategory(template, WorkflowDescription, "AAA");

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			AssertExpectedReleaseGroups(workItem, paveTeamGroup, paveTeamGroup, "Initially-matched release group rule should have been applied.");

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, paveTeamGroup, paveTeamGroup, "The release group rule doesn't find a match, so nothing should be changed.");
		}

		public void TestChangeWorkItemSelectionCriteria_WhenReleaseGroupRuleSpecified_AndReleaseGroupSpecifiedOnTemplateDirectly_ShouldNotChangeReleaseGroup()
		{
			var template = CreateWorkItemTemplate();
			template.GetJobHeader().FH_GG_ReleaseGroup = glowTeamGroup.PK;

			MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup), Tuple.Create("PAV", paveTeamGroup));

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			AssertExpectedReleaseGroups(workItem, glowTeamGroup, glowTeamGroup, "Initially-matched release group rule should not have been applied, because the release group was set directly on the job-level workflow.");

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, glowTeamGroup, glowTeamGroup, "Rules don't matter here.");
		}

		public void TestChangeWorkItemSelectionCriteria_WithCategories_WhenReleaseGroupRuleSpecified_AndReleaseGroupSpecifiedOnTemplateDirectly_ShouldNotChangeReleaseGroup()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");

			var template = CreateWorkItemTemplate();
			template.GetJobHeader().FH_GG_ReleaseGroup = glowTeamGroup.PK;

			var rule = MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup), Tuple.Create("PAV", paveTeamGroup));
			MapCategoriesToRule(rule, "AAA");
			GetOrCreateWorkflowAndTaskWithCategory(template, WorkflowDescription, "AAA");

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			AssertExpectedReleaseGroups(workItem, glowTeamGroup, glowTeamGroup, "Initially-matched release group rule should not have been applied, because the release group was set directly on the job-level workflow.");

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, glowTeamGroup, glowTeamGroup, "Rules don't matter here.");
		}

		public void TestChangeWorkItemSelectionCriteria_WithCategoriesAndJobMapping_WhenReleaseGroupRuleSpecified_AndReleaseGroupSpecifiedOnTemplateDirectly_ShouldNotChangeReleaseGroup()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");

			var template = CreateWorkItemTemplate();
			template.GetJobHeader().FH_GG_ReleaseGroup = glowTeamGroup.PK;

			var rule = MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup), Tuple.Create("PAV", paveTeamGroup));
			MapCategoriesToRule(rule, "AAA", "JOB");
			GetOrCreateWorkflowAndTaskWithCategory(template, WorkflowDescription, "AAA");

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			AssertExpectedReleaseGroups(workItem, glowTeamGroup, glowTeamGroup, "Initially-matched release group rule should not have been applied, because the release group was set directly on the job-level workflow.");

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, glowTeamGroup, glowTeamGroup, "Rules don't matter here.");
		}

		public void TestChangeWorkItemSelectionCriteria_WhenReleaseGroupRuleSpecifiedForOldAndNewCriteria_AndTemplateWorkflowHasReleaseGroupOverridden_OutsideGroup_ShouldChangeJLWReleaseGroupButLeaveOverriddenReleaseGroup()
		{
			var template = CreateWorkItemTemplate();
			var extraWorkflow = BMSTestHelper.CreateWorkflow(template, "Extra workflow with overridden release group", releaseGroupPK: glowTeamGroup.PK);
			BMSTestHelper.CreateTask(template, extraWorkflow, description: "Extra task");

			MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup), Tuple.Create("PAV", paveTeamGroup));

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			var extraWorkflowOnJob = ProcessJobHeader.GetForParentWithoutCreation((IWorkflowProvider)workItem, Factory).ProcessHeaders.Single(w => w.FH_CompletionStatement == extraWorkflow.FH_CompletionStatement);

			AssertExpectedReleaseGroups(workItem, paveTeamGroup, paveTeamGroup, "Initially-matched release group rule should have been applied.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow", glowTeamGroup, extraWorkflowOnJob);

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, coreTeamGroup, coreTeamGroup, "The release group rule should have cause the release group to be re-calculated.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow", glowTeamGroup, extraWorkflowOnJob);
		}

		public void TestChangeWorkItemSelectionCriteria_WhenReleaseGroupRuleSpecifiedForOldAndNewCriteria_AndTemplateWorkflowHasReleaseGroupOverridden_InsideGroup_ShouldChangeJLWReleaseGroupButLeaveOverriddenReleaseGroup()
		{
			var template = CreateWorkItemTemplate();
			var extraWorkflow = BMSTestHelper.CreateWorkflow(template, "Extra workflow with overridden release group", releaseGroupPK: paveTeamGroup.PK);
			BMSTestHelper.CreateTask(template, extraWorkflow, description: "Extra task");

			MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup), Tuple.Create("PAV", paveTeamGroup));

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			var extraWorkflowOnJob = ProcessJobHeader.GetForParentWithoutCreation((IWorkflowProvider)workItem, Factory).ProcessHeaders.Single(w => w.FH_CompletionStatement == extraWorkflow.FH_CompletionStatement);

			AssertExpectedReleaseGroups(workItem, paveTeamGroup, paveTeamGroup, "Initially-matched release group rule should have been applied.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow", paveTeamGroup, extraWorkflowOnJob);

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, coreTeamGroup, coreTeamGroup, "The release group rule should have cause the release group to be re-calculated.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow", paveTeamGroup, extraWorkflowOnJob);
		}

		public void TestChangeWorkItemSelectionCriteria_WithCategories_WhenReleaseGroupRuleSpecifiedForOldAndNewCriteria_AndTemplateWorkflowHasReleaseGroupOverridden_OutsideGroup_ShouldLeaveJLWReleaseGroupAndOverriddenReleaseGroup()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");

			var template = CreateWorkItemTemplate();
			GetOrCreateWorkflowAndTaskWithCategory(template, WorkflowDescription, "AAA");
			var extraWorkflow = BMSTestHelper.CreateWorkflow(template, "Extra workflow with overridden release group", releaseGroupPK: glowTeamGroup.PK); // This RG is NOT defined with the release group rules
			extraWorkflow.FH_Category = "AAA";
			BMSTestHelper.CreateTask(template, extraWorkflow, description: "Extra task");

			var rule = MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup), Tuple.Create("PAV", paveTeamGroup));
			MapCategoriesToRule(rule, "AAA");

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			var extraWorkflowOnJob = ProcessJobHeader.GetForParentWithoutCreation((IWorkflowProvider)workItem, Factory).ProcessHeaders.Single(w => w.FH_CompletionStatement == extraWorkflow.FH_CompletionStatement);

			AssertExpectedReleaseGroups(workItem, null, paveTeamGroup, "Initially-matched release group rule should have been applied.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow",
				glowTeamGroup, extraWorkflowOnJob);

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, null, coreTeamGroup, "The release group rule should have cause the release group to be re-calculated.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow",
				glowTeamGroup, extraWorkflowOnJob);
		}

		public void TestChangeWorkItemSelectionCriteria_WithCategories_WhenReleaseGroupRuleSpecifiedForOldAndNewCriteria_AndTemplateWorkflowHasReleaseGroupOverridden_InsideGroup_ShouldLeaveJLWReleaseGroupAndOverriddenReleaseGroup()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");

			var template = CreateWorkItemTemplate();
			GetOrCreateWorkflowAndTaskWithCategory(template, WorkflowDescription, "AAA");
			var extraWorkflow = BMSTestHelper.CreateWorkflow(template, "Extra workflow with overridden release group", releaseGroupPK: paveTeamGroup.PK);  // This RG IS defined with the release group rules
			extraWorkflow.FH_Category = "AAA";
			BMSTestHelper.CreateTask(template, extraWorkflow, description: "Extra task");

			var rule = MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup), Tuple.Create("PAV", paveTeamGroup));
			MapCategoriesToRule(rule, "AAA");

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			var extraWorkflowOnJob = ProcessJobHeader.GetForParentWithoutCreation((IWorkflowProvider)workItem, Factory).ProcessHeaders.Single(w => w.FH_CompletionStatement == extraWorkflow.FH_CompletionStatement);

			AssertExpectedReleaseGroups(workItem, null, paveTeamGroup, "Initially-matched release group rule should have been applied.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow",
				paveTeamGroup, extraWorkflowOnJob);

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, null, coreTeamGroup, "The release group rule should have cause the release group to be re-calculated.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow",
				paveTeamGroup, extraWorkflowOnJob);
		}

		public void TestChangeWorkItemSelectionCriteria_WithCategoriesAndJobMapping_WhenReleaseGroupRuleSpecifiedForOldAndNewCriteria_AndTemplateWorkflowHasReleaseGroupOverridden_OutsideGroup_ShouldLeaveJLWReleaseGroupAndOverriddenReleaseGroup()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");

			var template = CreateWorkItemTemplate();
			GetOrCreateWorkflowAndTaskWithCategory(template, WorkflowDescription, "AAA");
			var extraWorkflow = BMSTestHelper.CreateWorkflow(template, "Extra workflow with overridden release group", releaseGroupPK: glowTeamGroup.PK);  // This RG is NOT defined with the release group rules
			extraWorkflow.FH_Category = "AAA";
			BMSTestHelper.CreateTask(template, extraWorkflow, description: "Extra task");

			var rule = MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup), Tuple.Create("PAV", paveTeamGroup));
			MapCategoriesToRule(rule, "AAA", "JOB");

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			var extraWorkflowOnJob = ProcessJobHeader.GetForParentWithoutCreation((IWorkflowProvider)workItem, Factory).ProcessHeaders.Single(w => w.FH_CompletionStatement == extraWorkflow.FH_CompletionStatement);

			AssertExpectedReleaseGroups(workItem, paveTeamGroup, paveTeamGroup, "Initially-matched release group rule should have been applied.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow",
				glowTeamGroup, extraWorkflowOnJob);

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, coreTeamGroup, coreTeamGroup, "The release group rule should have cause the release group to be re-calculated.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow",
				glowTeamGroup, extraWorkflowOnJob);
		}

		public void TestChangeWorkItemSelectionCriteria_WithCategoriesAndJobMapping_WhenReleaseGroupRuleSpecifiedForOldAndNewCriteria_AndTemplateWorkflowHasReleaseGroupOverridden_InsideGroup_ShouldLeaveJLWReleaseGroupAndOverriddenReleaseGroup()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");

			var template = CreateWorkItemTemplate();
			GetOrCreateWorkflowAndTaskWithCategory(template, WorkflowDescription, "AAA");
			var extraWorkflow = BMSTestHelper.CreateWorkflow(template, "Extra workflow with overridden release group", releaseGroupPK: paveTeamGroup.PK);  // This RG IS defined with the release group rules
			extraWorkflow.FH_Category = "AAA";
			BMSTestHelper.CreateTask(template, extraWorkflow, description: "Extra task");

			var rule = MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup), Tuple.Create("PAV", paveTeamGroup));
			MapCategoriesToRule(rule, "AAA", "JOB");

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			var extraWorkflowOnJob = ProcessJobHeader.GetForParentWithoutCreation((IWorkflowProvider)workItem, Factory).ProcessHeaders.Single(w => w.FH_CompletionStatement == extraWorkflow.FH_CompletionStatement);

			AssertExpectedReleaseGroups(workItem, paveTeamGroup, paveTeamGroup, "Initially-matched release group rule should have been applied.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow",
				paveTeamGroup, extraWorkflowOnJob);

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, coreTeamGroup, coreTeamGroup, "The release group rule should have cause the release group to be re-calculated.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow",
				paveTeamGroup, extraWorkflowOnJob);
		}

		public void TestChangeWorkItemSelectionCriteria_WhenReleaseGroupRuleSpecifiedForOldAndNewCriteria_AndTemplateWorkflowHasReleaseGroupOverriddenToSameAsPreviousGroup_ShouldChangeJLWReleaseGroupButLeaveOverriddenReleaseGroup()
		{
			var template = CreateWorkItemTemplate();
			var extraWorkflow = BMSTestHelper.CreateWorkflow(template, "Extra workflow with overridden release group", releaseGroupPK: paveTeamGroup.PK);
			BMSTestHelper.CreateTask(template, extraWorkflow, description: "Extra task");

			MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup), Tuple.Create("PAV", paveTeamGroup));

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			var extraWorkflowOnJob = ProcessJobHeader.GetForParentWithoutCreation((IWorkflowProvider)workItem, Factory).ProcessHeaders.Single(w => w.FH_CompletionStatement == extraWorkflow.FH_CompletionStatement);

			AssertExpectedReleaseGroups(workItem, paveTeamGroup, paveTeamGroup, "Initially-matched release group rule should have been applied.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow", paveTeamGroup, extraWorkflowOnJob);

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, coreTeamGroup, coreTeamGroup, "The release group rule should have cause the release group to be re-calculated.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow", paveTeamGroup, extraWorkflowOnJob);
		}

		public void TestChangeWorkItemSelectionCriteria_WithCategories_WhenReleaseGroupRuleSpecifiedForOldAndNewCriteria_AndTemplateWorkflowHasReleaseGroupOverriddenToSameAsPreviousGroup_ShouldLeaveJLWReleaseGroupAndLeaveOverriddenReleaseGroup()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");

			var template = CreateWorkItemTemplate();
			GetOrCreateWorkflowAndTaskWithCategory(template, WorkflowDescription, "AAA");
			var extraWorkflow = BMSTestHelper.CreateWorkflow(template, "Extra workflow with overridden release group", releaseGroupPK: paveTeamGroup.PK);
			extraWorkflow.FH_Category = "AAA";
			BMSTestHelper.CreateTask(template, extraWorkflow, description: "Extra task");

			var rule = MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup), Tuple.Create("PAV", paveTeamGroup));
			MapCategoriesToRule(rule, "AAA");

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			var extraWorkflowOnJob = ProcessJobHeader.GetForParentWithoutCreation((IWorkflowProvider)workItem, Factory).ProcessHeaders.Single(w => w.FH_CompletionStatement == extraWorkflow.FH_CompletionStatement);

			AssertExpectedReleaseGroups(workItem, null, paveTeamGroup, "Initially-matched release group rule should have been applied.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow",
				paveTeamGroup, extraWorkflowOnJob);

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, null, coreTeamGroup, "The release group rule should have caused the release group to be re-calculated.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow",
				paveTeamGroup, extraWorkflowOnJob);
		}

		public void TestChangeWorkItemSelectionCriteria_WithCategoriesAndJobMapping_WhenReleaseGroupRuleSpecifiedForOldAndNewCriteria_AndTemplateWorkflowHasReleaseGroupOverriddenToSameAsPreviousGroup_ShouldLeaveOverriddenReleaseGroup()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");

			var template = CreateWorkItemTemplate();
			GetOrCreateWorkflowAndTaskWithCategory(template, WorkflowDescription, "AAA");
			var extraWorkflow = BMSTestHelper.CreateWorkflow(template, "Extra workflow with overridden release group", releaseGroupPK: paveTeamGroup.PK);
			extraWorkflow.FH_Category = "AAA";
			BMSTestHelper.CreateTask(template, extraWorkflow, description: "Extra task");

			var rule = MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup), Tuple.Create("PAV", paveTeamGroup));
			MapCategoriesToRule(rule, "AAA", "JOB");

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			var extraWorkflowOnJob = ProcessJobHeader.GetForParentWithoutCreation((IWorkflowProvider)workItem, Factory).ProcessHeaders.Single(w => w.FH_CompletionStatement == extraWorkflow.FH_CompletionStatement);

			AssertExpectedReleaseGroups(workItem, paveTeamGroup, paveTeamGroup, "Initially-matched release group rule should have been applied.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow",
				paveTeamGroup, extraWorkflowOnJob);

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, coreTeamGroup, coreTeamGroup, "The release group rule should have caused the release group to be re-calculated.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow",
				paveTeamGroup, extraWorkflowOnJob);
		}

		public void TestChangeWorkItemSelectionCriteria_WhenReleaseGroupRuleSpecifiedForOldAndNewCriteria_AndWorkflowOnJobHasReleaseGroupOverriddenWithReleaseGroupOutsideMapping_ShouldChangeJLWReleaseGroupButLeaveOverriddenReleaseGroup()
		{
			var template = CreateWorkItemTemplate();
			var extraWorkflow = BMSTestHelper.CreateWorkflow(template, "Extra workflow");
			BMSTestHelper.CreateTask(template, extraWorkflow, description: "Extra task");

			MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup), Tuple.Create("PAV", paveTeamGroup));

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			var extraWorkflowOnJob = ProcessJobHeader.GetForParentWithoutCreation((IWorkflowProvider)workItem, Factory).ProcessHeaders.Single(w => w.FH_CompletionStatement == extraWorkflow.FH_CompletionStatement);
			extraWorkflowOnJob.FH_GG_ReleaseGroup = glowTeamGroup.PK;
			Factory.Save();

			AssertExpectedReleaseGroups(workItem, paveTeamGroup, paveTeamGroup, "Initially-matched release group rule should have been applied.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow", glowTeamGroup, extraWorkflowOnJob);

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, coreTeamGroup, coreTeamGroup, "The release group rule should have cause the release group to be re-calculated.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow", glowTeamGroup, extraWorkflowOnJob);
		}

		public void TestChangeWorkItemSelectionCriteria_WhenReleaseGroupRuleSpecifiedForOldAndNewCriteria_AndWorkflowOnJobHasReleaseGroupOverriddenWithReleaseGroupInMapping_ShouldChangeJLWReleaseGroupButLeaveOverriddenReleaseGroup()
		{
			var template = CreateWorkItemTemplate();
			var extraWorkflow = BMSTestHelper.CreateWorkflow(template, "Extra workflow");
			BMSTestHelper.CreateTask(template, extraWorkflow, description: "Extra task");

			MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup), Tuple.Create("PAV", paveTeamGroup));

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			var extraWorkflowOnJob = ProcessJobHeader.GetForParentWithoutCreation((IWorkflowProvider)workItem, Factory).ProcessHeaders.Single(w => w.FH_CompletionStatement == extraWorkflow.FH_CompletionStatement);
			extraWorkflowOnJob.FH_GG_ReleaseGroup = paveTeamGroup.PK;
			Factory.Save();

			AssertExpectedReleaseGroups(workItem, paveTeamGroup, paveTeamGroup, "Initially-matched release group rule should have been applied.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow", paveTeamGroup, extraWorkflowOnJob);

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, coreTeamGroup, coreTeamGroup, "The release group rule should have cause the release group to be re-calculated.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow", paveTeamGroup, extraWorkflowOnJob);
		}

		public void TestChangeWorkItemSelectionCriteria_WithCategories_WhenReleaseGroupRuleSpecifiedForOldAndNewCriteria_AndWorkflowOnJobHasReleaseGroupOverriddenWithReleaseGroupOutsideMapping_ShouldLeaveJLWReleaseGroupAndLeaveOverriddenReleaseGroup()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");

			var template = CreateWorkItemTemplate();
			GetOrCreateWorkflowAndTaskWithCategory(template, WorkflowDescription, "AAA");
			var extraWorkflow = GetOrCreateWorkflowAndTaskWithCategory(template, "Extra workflow", "AAA");
			BMSTestHelper.CreateTask(template, extraWorkflow, description: "Extra task");

			var rule = MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup), Tuple.Create("PAV", paveTeamGroup));
			MapCategoriesToRule(rule, "AAA");

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			var extraWorkflowOnJob = ProcessJobHeader.GetForParentWithoutCreation((IWorkflowProvider)workItem, Factory).ProcessHeaders.Single(w => w.FH_CompletionStatement == extraWorkflow.FH_CompletionStatement);
			extraWorkflowOnJob.FH_GG_ReleaseGroup = glowTeamGroup.PK;
			Factory.Save();

			AssertEquals("AAA", extraWorkflowOnJob.FH_Category);
			AssertExpectedReleaseGroups(workItem, null, paveTeamGroup, "Initially-matched release group rule should have been applied.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow",
				glowTeamGroup, extraWorkflowOnJob);

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, null, coreTeamGroup, "The release group rule should have caused the release group to be re-calculated.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow",
				glowTeamGroup, extraWorkflowOnJob);
		}

		public void TestChangeWorkItemSelectionCriteria_WithCategories_WhenReleaseGroupRuleSpecifiedForOldAndNewCriteria_AndWorkflowOnJobHasReleaseGroupOverriddenWithReleaseGroupInMapping_ShouldLeaveJLWReleaseGroupAndLeaveOverriddenReleaseGroup()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");

			var template = CreateWorkItemTemplate();
			GetOrCreateWorkflowAndTaskWithCategory(template, WorkflowDescription, "AAA");
			var extraWorkflow = GetOrCreateWorkflowAndTaskWithCategory(template, "Extra workflow", "AAA");
			BMSTestHelper.CreateTask(template, extraWorkflow, description: "Extra task");

			var rule = MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup), Tuple.Create("PAV", paveTeamGroup));
			MapCategoriesToRule(rule, "AAA");

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			var extraWorkflowOnJob = ProcessJobHeader.GetForParentWithoutCreation((IWorkflowProvider)workItem, Factory).ProcessHeaders.Single(w => w.FH_CompletionStatement == extraWorkflow.FH_CompletionStatement);
			extraWorkflowOnJob.FH_GG_ReleaseGroup = paveTeamGroup.PK;
			Factory.Save();

			AssertEquals("AAA", extraWorkflowOnJob.FH_Category);
			AssertExpectedReleaseGroups(workItem, null, paveTeamGroup, "Initially-matched release group rule should have been applied.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow",
				paveTeamGroup, extraWorkflowOnJob);

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, null, coreTeamGroup, "The release group rule should have caused the release group to be re-calculated.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow",
				paveTeamGroup, extraWorkflowOnJob);
		}

		public void TestChangeWorkItemSelectionCriteria_WithCategoriesAndJobMapping_WhenReleaseGroupRuleSpecifiedForOldAndNewCriteria_AndWorkflowOnJobHasReleaseGroupOverriddenWithReleaseGroupOutsideMapping_ShouldLeaveJLWReleaseGroupAndLeaveOverriddenReleaseGroup()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");

			var template = CreateWorkItemTemplate();
			GetOrCreateWorkflowAndTaskWithCategory(template, WorkflowDescription, "AAA");
			var extraWorkflow = GetOrCreateWorkflowAndTaskWithCategory(template, "Extra workflow", "AAA");
			BMSTestHelper.CreateTask(template, extraWorkflow, description: "Extra task");

			var rule = MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup), Tuple.Create("PAV", paveTeamGroup));
			MapCategoriesToRule(rule, "AAA", "JOB");

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			var extraWorkflowOnJob = ProcessJobHeader.GetForParentWithoutCreation((IWorkflowProvider)workItem, Factory).ProcessHeaders.Single(w => w.FH_CompletionStatement == extraWorkflow.FH_CompletionStatement);
			extraWorkflowOnJob.FH_GG_ReleaseGroup = glowTeamGroup.PK;
			Factory.Save();

			AssertEquals("AAA", extraWorkflowOnJob.FH_Category);
			AssertExpectedReleaseGroups(workItem, paveTeamGroup, paveTeamGroup, "Initially-matched release group rule should have been applied.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow",
				glowTeamGroup, extraWorkflowOnJob);

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, coreTeamGroup, coreTeamGroup, "The release group rule should have caused the release group to be re-calculated.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow",
				glowTeamGroup, extraWorkflowOnJob);
		}

		public void TestChangeWorkItemSelectionCriteria_WithCategoriesAndJobMapping_WhenReleaseGroupRuleSpecifiedForOldAndNewCriteria_AndWorkflowOnJobHasReleaseGroupOverriddenWithReleaseGroupInMapping_ShouldLeaveJLWReleaseGroupAndLeaveOverriddenReleaseGroup()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");

			var template = CreateWorkItemTemplate();
			GetOrCreateWorkflowAndTaskWithCategory(template, WorkflowDescription, "AAA");
			var extraWorkflow = GetOrCreateWorkflowAndTaskWithCategory(template, "Extra workflow", "AAA");
			BMSTestHelper.CreateTask(template, extraWorkflow, description: "Extra task");

			var rule = MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup), Tuple.Create("PAV", paveTeamGroup));
			MapCategoriesToRule(rule, "AAA", "JOB");

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			var extraWorkflowOnJob = ProcessJobHeader.GetForParentWithoutCreation((IWorkflowProvider)workItem, Factory).ProcessHeaders.Single(w => w.FH_CompletionStatement == extraWorkflow.FH_CompletionStatement);
			extraWorkflowOnJob.FH_GG_ReleaseGroup = paveTeamGroup.PK;
			Factory.Save();

			AssertEquals("AAA", extraWorkflowOnJob.FH_Category);
			AssertExpectedReleaseGroups(workItem, paveTeamGroup, paveTeamGroup, "Initially-matched release group rule should have been applied.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow",
				paveTeamGroup, extraWorkflowOnJob);

			ChangeProductAreaInAnotherFactory(workItem, "ARC");
			AssertExpectedReleaseGroups(workItem, coreTeamGroup, coreTeamGroup, "The release group rule should have caused the release group to be re-calculated.");
			BMSTestHelper.AssertReleaseGroup("Release group rules should not take precedence over the release group specified on the template version of this workflow",
				paveTeamGroup, extraWorkflowOnJob);
		}

		public void TestChangeWorkItemSelectionCriteria_ForJobLevelWorkflowNotAppliedFromTemplate_ShouldSetReleaseGroupBasedOnTemplateRules()
		{
			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			var jobHeader = ProcessJobHeader.GetForParent((IWorkflowProvider)workItem, Factory);
			Factory.Save();

			BMSTestHelper.AssertReleaseGroup(null, jobHeader);

			var template = CreateWorkItemTemplate();

			MapProductAreasToGroups(template, Tuple.Create("PAV", paveTeamGroup));
			Factory.Save();

			((IWorkflowProvider)workItem).ApplyWorkflowTemplates();
			BMSTestHelper.AssertReleaseGroup(paveTeamGroup, jobHeader);
		}

		#endregion

		#region Performance

		public void TestManyTemplatesAndRules_DbHits()
		{
			var template1 = CreateWorkItemTemplate("PAV", FallbackTypeList.Codes.EmptyFallback, "ENT", "BUF", "PRD", "ALP");
			var template2 = CreateWorkItemTemplate("PAV", FallbackTypeList.Codes.EmptyFallback, "ENT", "BUF", "PRD");
			var template3 = CreateWorkItemTemplate("PAV", FallbackTypeList.Codes.EmptyFallback, "ENT", "BUF");
			var template4 = CreateWorkItemTemplate("PAV", FallbackTypeList.Codes.EmptyFallback, "ENT");
			var template5 = CreateWorkItemTemplate("PAV", FallbackTypeList.Codes.EmptyFallback);
			var template6 = CreateWorkItemTemplate("", FallbackTypeList.Codes.EmptyFallback);

			MapProductAreasToGroups(template1, Tuple.Create("ZZZ", dumpingGroundGroup), Tuple.Create("XXX", dumpingGroundGroup));
			MapProductAreasToGroups(template2, Tuple.Create("ZZZ", dumpingGroundGroup), Tuple.Create("XXX", dumpingGroundGroup));
			MapProductAreasToGroups(template3, Tuple.Create("ZZZ", dumpingGroundGroup), Tuple.Create("XXX", dumpingGroundGroup));
			MapProductAreasToGroups(template4, Tuple.Create("ZZZ", dumpingGroundGroup), Tuple.Create("XXX", dumpingGroundGroup));
			MapProductAreasToGroups(template5, Tuple.Create("ZZZ", dumpingGroundGroup), Tuple.Create("XXX", dumpingGroundGroup));
			MapProductAreasToGroups(template6, Tuple.Create("PAV", paveTeamGroup));

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var workItem = CreateWorkItem("PAV", "ENT", "BUF", "PRD", "ALP", newFactory);

			newFactory.Save();
			BMSTestHelper.ClearUberFactory();

			AssertExpectedReleaseGroups(workItem, paveTeamGroup, paveTeamGroup, "The least specific template had a matching release group mapping");
			AssertMaxDbHits(new Dictionary<string, int>
			{
				{ BMComponentSchema.Constants.TableName, 1 },
				{ BMComponentLinkSchema.Constants.TableName, 1 },
				{ BMSystemSchema.Constants.TableName, 1 },
				{ BMSystemReleaseGroupSchema.Constants.TableName, 1 },
				{ BMSystemWorkflowDeterminerSchema.Constants.TableName, 1 },
				{ GenPivotSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ GlbGroupSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessHeaderSchema.Constants.TableName, 1 },
				{ ProcessHeaderLinkSchema.Constants.TableName, 2 },
				{ ProcessTasksSchema.Constants.TableName, 4 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ ProcessTemplateReleaseGroupRuleSchema.Constants.TableName, 1 },
				{ ProcessTemplateReleaseGroupRuleMappingSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 3 },
				{ TagLinkSchema.Constants.TableName, 1 },
				{ WorkItemSchema.Constants.TableName, 1 },
				{ WorkItemRequestLinkSchema.Constants.TableName, 1 },
			}, newFactory);
		}

		public void TestManyTemplatesAndRulesAndCategories_DbHits()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "BBB", "Berocca");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "KKK", "Bad boyz");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "SSS", "Tiny snek");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "XXX", "Not for kids");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "ZZZ", "Sleepy time");

			var template1 = CreateWorkItemTemplate("PAV", FallbackTypeList.Codes.EmptyFallback, "ENT", "BUF", "PRD", "ALP");
			var template2 = CreateWorkItemTemplate("PAV", FallbackTypeList.Codes.EmptyFallback, "ENT", "BUF", "PRD");
			var template3 = CreateWorkItemTemplate("PAV", FallbackTypeList.Codes.EmptyFallback, "ENT", "BUF");
			var template4 = CreateWorkItemTemplate("PAV", FallbackTypeList.Codes.EmptyFallback, "ENT");
			var template5 = CreateWorkItemTemplate("PAV", FallbackTypeList.Codes.EmptyFallback);
			var template6 = CreateWorkItemTemplate("", FallbackTypeList.Codes.EmptyFallback);

			GetOrCreateWorkflowAndTaskWithCategory(template1, WorkflowDescription, "AAA");
			GetOrCreateWorkflowAndTaskWithCategory(template2, WorkflowDescription, "BBB");
			GetOrCreateWorkflowAndTaskWithCategory(template3, WorkflowDescription, "KKK");
			GetOrCreateWorkflowAndTaskWithCategory(template4, WorkflowDescription, "SSS");
			GetOrCreateWorkflowAndTaskWithCategory(template5, WorkflowDescription, "XXX");
			GetOrCreateWorkflowAndTaskWithCategory(template6, WorkflowDescription, "AAA");

			var rule1 = MapProductAreasToGroups(template1, Tuple.Create("ZZZ", dumpingGroundGroup), Tuple.Create("XXX", dumpingGroundGroup));
			var rule2 = MapProductAreasToGroups(template2, Tuple.Create("ZZZ", dumpingGroundGroup), Tuple.Create("XXX", dumpingGroundGroup));
			var rule3 = MapProductAreasToGroups(template3, Tuple.Create("ZZZ", dumpingGroundGroup), Tuple.Create("XXX", dumpingGroundGroup));
			var rule4 = MapProductAreasToGroups(template4, Tuple.Create("ZZZ", dumpingGroundGroup), Tuple.Create("XXX", dumpingGroundGroup));
			var rule5 = MapProductAreasToGroups(template5, Tuple.Create("ZZZ", dumpingGroundGroup), Tuple.Create("XXX", dumpingGroundGroup));
			var rule6 = MapProductAreasToGroups(template6, Tuple.Create("PAV", paveTeamGroup));

			MapCategoriesToRule(rule1, "AAA");
			MapCategoriesToRule(rule2, "BBB");
			MapCategoriesToRule(rule3, "KKK");
			MapCategoriesToRule(rule4, "SSS");
			MapCategoriesToRule(rule5, "XXX");
			MapCategoriesToRule(rule6, "AAA", "ZZZ");

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var workItem = CreateWorkItem("PAV", "ENT", "BUF", "PRD", "ALP", newFactory);

			newFactory.Save();
			BMSTestHelper.ClearUberFactory();

			AssertExpectedReleaseGroups(workItem, null, paveTeamGroup, "The least specific template had a matching release group mapping");
			AssertMaxDbHits(new Dictionary<string, int>
			{
				{ BMComponentSchema.Constants.TableName, 1 },
				{ BMComponentLinkSchema.Constants.TableName, 1 },
				{ BMSystemSchema.Constants.TableName, 1 },
				{ BMSystemReleaseGroupSchema.Constants.TableName, 1 },
				{ BMSystemWorkflowDeterminerSchema.Constants.TableName, 1 },
				{ GenPivotSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ GlbGroupSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessHeaderSchema.Constants.TableName, 1 },
				{ ProcessHeaderLinkSchema.Constants.TableName, 2 },
				{ ProcessTasksSchema.Constants.TableName, 4 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ ProcessTemplateReleaseGroupRuleSchema.Constants.TableName, 1 },
				{ ProcessTemplateReleaseGroupRuleCategorySchema.Constants.TableName, 1 },
				{ ProcessTemplateReleaseGroupRuleMappingSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 3 },
				{ TagLinkSchema.Constants.TableName, 1 },
				{ WorkItemSchema.Constants.TableName, 1 },
				{ WorkItemRequestLinkSchema.Constants.TableName, 1 },
			}, newFactory);
		}

		public void TestChangeWorkItemSelectionCriteria_DbHits()
		{
			var template1 = CreateWorkItemTemplate("", FallbackTypeList.Codes.EmptyFallback);
			var template2 = CreateWorkItemTemplate("", FallbackTypeList.Codes.EmptyFallback, "ENT");
			var template3 = CreateWorkItemTemplate("PAV", FallbackTypeList.Codes.EmptyFallback, "ENT");
			var template4 = CreateWorkItemTemplate("PAV", FallbackTypeList.Codes.EmptyFallback, "ENT", "BUF");
			var template5 = CreateWorkItemTemplate("PAV", FallbackTypeList.Codes.EmptyFallback, "ENT", "BUF", "PRD");
			var template6 = CreateWorkItemTemplate("PAV", FallbackTypeList.Codes.EmptyFallback, "ENT", "BUF", "PRD", "ALP");

			MapProductAreasToGroups(template1, Tuple.Create("ARC", coreTeamGroup), Tuple.Create("PAV", paveTeamGroup));

			// Extra workflows to ensure the template version lookup is fetch hinted.
			var extraWorkflow1 = BMSTestHelper.CreateWorkflow(template6, "Tanya Plibersek for PM");
			var extraWorkflow2 = BMSTestHelper.CreateWorkflow(template6, "Scott Morrison for the (metaphorical) dumpster");

			BMSTestHelper.CreateTask(template6, extraWorkflow1);
			BMSTestHelper.CreateTask(template6, extraWorkflow2);

			Factory.Save();

			var workItem = CreateWorkItem("PAV", "ENT", "BUF", "PRD", "ALP");
			Factory.Save();

			AssertExpectedReleaseGroups(workItem, paveTeamGroup, paveTeamGroup, "Initially-matched release group rule should have been applied.");

			var newFactory = Factory.CreateNewFactory();
			var loadedWorkItem = newFactory.Load<IWorkItem>(workItem.PK);

			loadedWorkItem.WKI_WorkItemArea = "ARC";
			newFactory.Save();
			BMSTestHelper.ClearUberFactory();

			AssertExpectedReleaseGroups(loadedWorkItem, coreTeamGroup, coreTeamGroup, "Now that the factory is saved, the release group rule should have been re-calculated.");
			AssertMaxDbHits(new Dictionary<string, int>
			{
				{ BMComponentSchema.Constants.TableName, 1 },
				{ BMSystemSchema.Constants.TableName, 1 },
				{ BMSystemReleaseGroupSchema.Constants.TableName, 1 },
				{ BMSystemWorkflowDeterminerSchema.Constants.TableName, 1 },
				{ GenPivotSchema.Constants.TableName, 1 },
				{ GlbGroupSchema.Constants.TableName, 1 },
				{ GlbGroupLinkSchema.Constants.TableName, 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessHeaderSchema.Constants.TableName, 4 }, // One for the job-level workflow, one for all its workflows, and one for all their template versions. No more!
				{ ProcessHeaderLinkSchema.Constants.TableName, 10 },
				{ ProcessTasksSchema.Constants.TableName, 5 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 }, // One for determining matching templates, none for validating Release Groups (it's no longer necessary to lookup the template's BM System's list of release groups).
				{ ProcessTemplateReleaseGroupRuleSchema.Constants.TableName, 1 },
				{ ProcessTemplateReleaseGroupRuleMappingSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 1 },
				{ TagLinkSchema.Constants.TableName, 1 },
				{ WorkItemSchema.Constants.TableName, 1 },
				{ WorkItemRequestLinkSchema.Constants.TableName, 1 },
			}, newFactory);
		}

		public void TestChangeWorkItemSelectionCriteria_WithCategories_DbHits()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "AAA", "It hurts");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "BBB", "Berocca");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "KKK", "Bad boyz");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "SSS", "Tiny snek");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "XXX", "Not for kids");
			BMSTestHelper.AddWorkflowCategoryToRegistry("WKI", "ZZZ", "Sleepy time");

			var template1 = CreateWorkItemTemplate("", FallbackTypeList.Codes.EmptyFallback);
			var template2 = CreateWorkItemTemplate("", FallbackTypeList.Codes.EmptyFallback, "ENT");
			var template3 = CreateWorkItemTemplate("PAV", FallbackTypeList.Codes.EmptyFallback, "ENT");
			var template4 = CreateWorkItemTemplate("PAV", FallbackTypeList.Codes.EmptyFallback, "ENT", "BUF");
			var template5 = CreateWorkItemTemplate("PAV", FallbackTypeList.Codes.EmptyFallback, "ENT", "BUF", "PRD");
			var template6 = CreateWorkItemTemplate("PAV", FallbackTypeList.Codes.EmptyFallback, "ENT", "BUF", "PRD", "ALP");

			GetOrCreateWorkflowAndTaskWithCategory(template1, WorkflowDescription, "AAA");
			GetOrCreateWorkflowAndTaskWithCategory(template2, WorkflowDescription, "BBB");
			GetOrCreateWorkflowAndTaskWithCategory(template3, WorkflowDescription, "KKK");
			GetOrCreateWorkflowAndTaskWithCategory(template4, WorkflowDescription, "SSS");
			GetOrCreateWorkflowAndTaskWithCategory(template5, WorkflowDescription, "XXX");
			GetOrCreateWorkflowAndTaskWithCategory(template6, WorkflowDescription, "AAA");

			var rule = MapProductAreasToGroups(template1, Tuple.Create("ARC", coreTeamGroup), Tuple.Create("PAV", paveTeamGroup));
			MapCategoriesToRule(rule, "AAA");

			// Extra workflows to ensure the template version lookup is fetch hinted.
			var extraWorkflow1 = BMSTestHelper.CreateWorkflow(template6, "Peter Dutton for Nauru");
			extraWorkflow1.FH_Category = "ZZZ";
			var extraWorkflow2 = BMSTestHelper.CreateWorkflow(template6, "Alan Jones for the (metaphorical) chaff bag");
			extraWorkflow2.FH_Category = "ZZZ";

			BMSTestHelper.CreateTask(template6, extraWorkflow1);
			BMSTestHelper.CreateTask(template6, extraWorkflow2);

			Factory.Save();

			var workItem = CreateWorkItem("PAV", "ENT", "BUF", "PRD", "ALP");
			Factory.Save();

			AssertExpectedReleaseGroups(workItem, null, paveTeamGroup, "Initially-matched release group rule should have been applied.");

			var newFactory = Factory.CreateNewFactory();
			var loadedWorkItem = newFactory.Load<IWorkItem>(workItem.PK);

			loadedWorkItem.WKI_WorkItemArea = "ARC";
			newFactory.Save();
			BMSTestHelper.ClearUberFactory();

			AssertExpectedReleaseGroups(loadedWorkItem, null, coreTeamGroup, "Now that the factory is saved, the release group rule should have been re-calculated.");
			AssertMaxDbHits(new Dictionary<string, int>
			{
				{ BMComponentSchema.Constants.TableName, 1 },
				{ BMSystemSchema.Constants.TableName, 1 },
				{ BMSystemReleaseGroupSchema.Constants.TableName, 1 },
				{ BMSystemWorkflowDeterminerSchema.Constants.TableName, 1 },
				{ GenPivotSchema.Constants.TableName, 1 },
				{ GlbGroupSchema.Constants.TableName, 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessHeaderSchema.Constants.TableName, 4 }, // One for the job-level workflow, one for all its workflows, and one for all their template versions. No more!
				{ ProcessHeaderLinkSchema.Constants.TableName, 4 },
				{ ProcessTasksSchema.Constants.TableName, 5 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 4 },
				{ ProcessTemplateReleaseGroupRuleSchema.Constants.TableName, 1 },
				{ ProcessTemplateReleaseGroupRuleCategorySchema.Constants.TableName, 1 },
				{ ProcessTemplateReleaseGroupRuleMappingSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 1 },
				{ TagLinkSchema.Constants.TableName, 1 },
				{ WorkItemSchema.Constants.TableName, 1 },
				{ WorkItemRequestLinkSchema.Constants.TableName, 1 },
			}, newFactory);
		}

		public void TestChangeWorkItemSelectionCriteria_WhenReleaseGroupSpecifiedOnTemplateJobLevelWorkflow_ShouldNotHitReleaseGroupRules()
		{
			var template = CreateWorkItemTemplate();
			template.GetJobHeader().FH_GG_ReleaseGroup = glowTeamGroup.PK;

			MapProductAreasToGroups(template, Tuple.Create("ARC", coreTeamGroup), Tuple.Create("PAV", paveTeamGroup));

			Factory.Save();

			var workItem = CreateWorkItem("PAV");
			Factory.Save();

			AssertExpectedReleaseGroups(workItem, glowTeamGroup, glowTeamGroup, "Release group rule should not have been applied, since a release group was specified on the template job-level workflow directly.");

			var newFactory = Factory.CreateNewFactory();
			var loadedWorkItem = newFactory.Load<IWorkItem>(workItem.PK);

			loadedWorkItem.WKI_WorkItemArea = "ARC";
			newFactory.Save();
			BMSTestHelper.ClearUberFactory();

			AssertExpectedReleaseGroups(loadedWorkItem, glowTeamGroup, glowTeamGroup, "Changing selection criteria values shouldn't impact the release group, since a release group was specified on the template job-level workflow directly.");
			AssertMaxDbHits(new Dictionary<string, int>
			{
				{ BMSystemSchema.Constants.TableName, 1 },
				{ BMSystemWorkflowDeterminerSchema.Constants.TableName, 1 },
				{ GenPivotSchema.Constants.TableName, 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessHeaderSchema.Constants.TableName, 3 }, // One for the job-level workflow, one for all its workflows, and one for all their template versions. No more!
				{ ProcessHeaderLinkSchema.Constants.TableName, 2 },
				{ ProcessTasksSchema.Constants.TableName, 4 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 1 },
				{ TagLinkSchema.Constants.TableName, 1 },
				{ WorkItemSchema.Constants.TableName, 1 },
				{ WorkItemRequestLinkSchema.Constants.TableName, 1 },
			}, newFactory);
		}

		public void TestDisableFeature_ShouldNotHitDatabaseForReleaseGroupRules()
		{
			var template = CreateWorkItemTemplate();

			MapProductAreasToGroups(template, Tuple.Create("PAV", paveTeamGroup));

			Factory.Save();

			WorkflowDataRegistry.Instance.EnableTemplateReleaseGroupRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var newFactory = Factory.CreateNewFactory();
			var workItem = CreateWorkItem("PAV", factory: newFactory);

			newFactory.Save();
			BMSTestHelper.ClearUberFactory();

			AssertExpectedReleaseGroups(workItem, null, null, "Release group rule should not have been considered since the feature is disabled.");
			AssertMaxDbHits(new Dictionary<string, int>
			{
				{ BMComponentSchema.Constants.TableName, 1 },
				{ BMComponentLinkSchema.Constants.TableName, 1 },
				{ BMSystemSchema.Constants.TableName, 1 },
				{ BMSystemWorkflowDeterminerSchema.Constants.TableName, 1 },
				{ BMSystemReleaseGroupSchema.Constants.TableName, 1 },
				{ GenPivotSchema.Constants.TableName, 1 },
				{ GlbCompanySchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessHeaderSchema.Constants.TableName, 1 },
				{ ProcessHeaderLinkSchema.Constants.TableName, 2 },
				{ ProcessTasksSchema.Constants.TableName, 4 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 3 },
				{ TagLinkSchema.Constants.TableName, 1 },
				{ WorkItemSchema.Constants.TableName, 1 },
				{ WorkItemRequestLinkSchema.Constants.TableName, 1 },
			}, newFactory);

			WorkflowDataRegistry.Instance.EnableTemplateReleaseGroupRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			workItem = CreateWorkItem("PAV", factory: newFactory);
			newFactory.Save();

			AssertExpectedReleaseGroups(workItem, paveTeamGroup, paveTeamGroup, "Release group rule should have been considered now that the feature was enabled.");
		}

		#endregion

		#region Implementation

		ProcessTaskTemplate CreateWorkItemTemplate(string productArea = null, string releaseGroupRuleFallbackMethod = null, string product = null, string module = null, string changeType = null, string priority = null)
		{
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.WorkItemWorkflowDescriptorCode, subType1: product, subType2: productArea, subType3: module, subType4: changeType, subType5: priority);

			if (releaseGroupRuleFallbackMethod != null)
			{
				template.P0_ReleaseGroupFallbackMethod = releaseGroupRuleFallbackMethod;
			}

			var templateWorkflow = BMSTestHelper.CreateWorkflow(template, description: WorkflowDescription);
			BMSTestHelper.CreateTask(template, templateWorkflow);

			return template;
		}

		const string WorkflowDescription = "Albo for PM?";

		ProcessTemplateReleaseGroupRule MapProductAreasToGroups(ProcessTaskTemplate template, params Tuple<string, GlbGroup>[] productAreaToGroupMappings)
		{
			var rule = BMSTestHelper.CreateTemplateReleaseGroupRule(template, "<WKI_WorkItemArea>");
			foreach (var tuple in productAreaToGroupMappings)
			{
				BMSTestHelper.CreateTemplateReleaseGroupRuleMapping(rule, tuple.Item1, tuple.Item2);
			}

			return rule;
		}

		void MapCategoriesToRule(ProcessTemplateReleaseGroupRule rule, params string[] categoryCodes)
		{
			foreach (var code in categoryCodes)
			{
				BMSTestHelper.CreateTemplateReleaseGroupRuleCategory(rule, code);
			}

			rule.PTR_AreAllWorkflowCategoriesApplicable = false;
		}

		IWorkItem CreateWorkItem(string productArea, string product = null, string module = null, string changeType = null, string priority = null, BusinessObjectFactory factory = null)
		{
			var workItem = (IWorkItem)BMSTestHelper.CreateJob<IWorkItem>(factory ?? Factory);

			workItem.WKI_WorkItemType = product;
			workItem.WKI_WorkItemArea = productArea;
			workItem.WKI_ActivityType = module;
			workItem.WKI_ActivitySubtype = changeType;
			workItem.WKI_Priority = priority;

			return workItem;
		}

		ProcessHeader GetOrCreateWorkflowAndTaskWithCategory(ProcessTaskTemplate template, string completionStatement, string category)
		{
			var workflows = template.ProcessHeaders as ProcessHeaderCollection;
			var existingWorkflow = workflows.SingleOrDefault(w => w.FH_CompletionStatement == completionStatement);

			if (existingWorkflow != null)
			{
				existingWorkflow.FH_Category = category;
				return existingWorkflow;
			}

			var templateWorkflow = BMSTestHelper.CreateWorkflow(template, description: completionStatement);
			templateWorkflow.FH_Category = category;
			BMSTestHelper.CreateTask(template, templateWorkflow);

			return templateWorkflow;
		}

		static void ChangeProductAreaInAnotherFactory(IWorkItem workItem, string newProductArea)
		{
			var newFactory = ((BusinessObject)workItem).Factory.CreateNewFactory(); // Using a new factory so we know the change is made, even when nothing PAVE-related is held in the factory cache.
			var loadedWorkItem = newFactory.Load<IWorkItem>(workItem.PK);

			loadedWorkItem.WKI_WorkItemArea = newProductArea;

			newFactory.Save();
		}

		static void DeleteAllWorkflowsInAnotherFactory(IWorkItem workItem)
		{
			var newFactory = ((BusinessObject)workItem).Factory.CreateNewFactory(); // Using a new factory so we know the change is made, even when nothing PAVE-related is held in the factory cache.
			var loadedWorkItem = newFactory.Load<IWorkItem>(workItem.PK);

			var processJobHeader = ProcessJobHeader.GetForParentWithoutCreation((IWorkflowProvider)loadedWorkItem, newFactory);
			if (processJobHeader != null)
			{
				processJobHeader.ProcessHeaders.DeleteAll();
				processJobHeader.Delete();
			}

			newFactory.Save();
		}

		void AssertExpectedReleaseGroups(IWorkItem workItem, GlbGroup expectedJobLevelWorkflowReleaseGroup, GlbGroup expectedWorkflowReleaseGroup, string assertionMessage)
		{
			var jobLevelWorkflow = ProcessJobHeader.GetForParentWithoutCreation((IWorkflowProvider)workItem, Factory);
			var workflow = jobLevelWorkflow?.ProcessHeaders.Single(w => w.FH_CompletionStatement == WorkflowDescription);

			CombineAssertions(assertionMessage, () =>
			{
				AssertNotNull("A job-level workflow should have been created when saving the factory. Very Unfair!", jobLevelWorkflow);

				if (jobLevelWorkflow != null)
				{
					AssertEquals("Job-level workflow release group", expectedJobLevelWorkflowReleaseGroup?.GG_Code, jobLevelWorkflow.ReleaseGroup?.GG_Code);
					AssertEquals("Workflow release group", expectedWorkflowReleaseGroup?.GG_Code, workflow.ReleaseGroup?.GG_Code);
				}
			});
		}

		void AssertExpectedReleaseGroups(string assertionMessage, IWorkItem workItem, GlbGroup expectedJobLevelWorkflowReleaseGroup, params Tuple<ProcessHeader, GlbGroup>[] expectedWorkflowsAndReleaseGroups)
		{
			var jobLevelWorkflow = ProcessJobHeader.GetForParentWithoutCreation((IWorkflowProvider)workItem, Factory);

			CombineAssertions(assertionMessage, () =>
			{
				AssertNotNull("A job-level workflow should have been created when saving the factory. Very Unfair!", jobLevelWorkflow);

				if (jobLevelWorkflow != null)
				{
					AssertEquals("Job-level workflow release group", expectedJobLevelWorkflowReleaseGroup?.GG_Code, jobLevelWorkflow.ReleaseGroup?.GG_Code);

					foreach (var flowGroup in expectedWorkflowsAndReleaseGroups)
					{
						var completionStatement = flowGroup.Item1.FH_CompletionStatement;
						var workflow = jobLevelWorkflow.ProcessHeaders.Single(w => w.FH_CompletionStatement == completionStatement);

						AssertNotNull($"Workflow {completionStatement} should have been created when saving the factory. Scandalous!", workflow);
						AssertEquals($"Workflow release group for {completionStatement}", flowGroup.Item2?.GG_Code, workflow.ReleaseGroup?.GG_Code);
					}
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);

			paveTeamGroup = BMSTestHelper.CreateGroup(Factory, "PAVERG", "PAVE Team 100 Years");
			coreTeamGroup = BMSTestHelper.CreateGroup(Factory, "CORERG", "www core team dot com");
			glowTeamGroup = BMSTestHelper.CreateGroup(Factory, "GLOWRG", "GLOW team you and me Morty");
			dumpingGroundGroup = BMSTestHelper.CreateGroup(Factory, "ALLRG", "The outside world is our enemy");
			defaultGroup = BMSTestHelper.CreateGroup(Factory, "DEFRG", "Default group gonna run around Morty");
			emptyProductAreaGroup = BMSTestHelper.CreateGroup(Factory, "NORG", "Full ming mong, empty gorb dorp");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "WI00637617 - This is needed to have a valid BMSystem, see http://crikey.wtg.zone/TestResults/ee1d3757-f61e-40ab-9941-fa3c4b90abcc")]
		SchematicTestConfig config;
		GlbGroup paveTeamGroup;
		GlbGroup coreTeamGroup;
		GlbGroup glowTeamGroup;
		GlbGroup dumpingGroundGroup;
		GlbGroup emptyProductAreaGroup;
		GlbGroup defaultGroup;

		#endregion
	}

	class ReleaseGroupDeterminatorTest_ForShipments : BMSTestCaseWithFactory
	{
		#region Shipments

		public void TestWorkflowReleaseGroup_Found()
		{
			SetupWorkflowTemplateAndReleaseGroupRules("SHP", "<JS_TransportMode>", Tuple.Create("AIR", group));

			CreateShipmentAndAssertReleaseGroup("AIR", group);
		}

		public void TestWorkflowReleaseGroup_EmptyValue()
		{
			SetupWorkflowTemplateAndReleaseGroupRules("SHP", "<JS_TransportMode>", Tuple.Create("", group));

			CreateShipmentAndAssertReleaseGroup("AIR", null);
			CreateShipmentAndAssertReleaseGroup("", group);
		}

		public void TestWorkflowReleaseGroup_InactiveGroup()
		{
			SetupWorkflowTemplateAndReleaseGroupRules("SHP", "<JS_TransportMode>", Tuple.Create("AIR", group));
			CreateShipmentAndAssertReleaseGroup("AIR", group);

			group.GG_IsActive = false;
			CreateShipmentAndAssertReleaseGroup("AIR", null);
		}

		public void TestWorkflowReleaseGroup_NotFound()
		{
			SetupWorkflowTemplateAndReleaseGroupRules("SHP", "<JS_TransportMode>", Tuple.Create("SEA", group));

			CreateShipmentAndAssertReleaseGroup("AIR", null);
		}

		public void TestWorkflowReleaseGroup_MultipleDeterminers()
		{
			SetupWorkflowTemplateAndReleaseGroupRules("SHP", "<JS_TransportMode>", Tuple.Create("AIR", group), Tuple.Create("SEA", group));

			CreateShipmentAndAssertReleaseGroup("AIR", group);
			CreateShipmentAndAssertReleaseGroup("SEA", group);
			CreateShipmentAndAssertReleaseGroup("RAI", null);
			CreateShipmentAndAssertReleaseGroup("", null);
		}

		#endregion

		#region Implementation

		void SetupWorkflowTemplateAndReleaseGroupRules(string processType, string releaseGroupRuleValueSelectionMacro, params Tuple<string, GlbGroup>[] valueToGroupMappings)
		{
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, processType);
			var rule = BMSTestHelper.CreateTemplateReleaseGroupRule(template, releaseGroupRuleValueSelectionMacro);

			foreach (var tuple in valueToGroupMappings)
			{
				BMSTestHelper.CreateTemplateReleaseGroupRuleMapping(rule, tuple.Item1, tuple.Item2);
			}

			Factory.Save();
		}

		void CreateShipmentAndAssertReleaseGroup(string transportMode, GlbGroup expectedReleaseGroup)
		{
			var job = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			job[JobShipmentSchema.JS_TransportMode] = transportMode;
			job[JobShipmentSchema.JS_E_ARV] = ZDateTime.Today.AddDays(1);

			Factory.Save();

			AssertReleaseGroup(expectedReleaseGroup, (IWorkflowProvider)job);
		}

		static void AssertReleaseGroup(GlbGroup expectedReleaseGroup, IWorkflowProvider job)
		{
			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, ((BusinessObject)job).Factory);
			AssertEquals("Default workflow should be created", 1, jobHeader.ProcessHeaders.Count);
			BMSTestHelper.AssertReleaseGroup(expectedReleaseGroup, jobHeader.ProcessHeaders[0]);

			var secondWorkflow = jobHeader.ProcessHeaders.AddNew();
			BMSTestHelper.AssertReleaseGroup(expectedReleaseGroup, secondWorkflow);
		}

		protected override void SetUp()
		{
			base.SetUp();

			config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode);
			group = Factory.New<GlbGroup>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "WI00637617 - This is needed to have a valid BMSystem, see http://crikey.wtg.zone/TestResults/ee1d3757-f61e-40ab-9941-fa3c4b90abcc")]
		SchematicTestConfig config;
		GlbGroup group;

		#endregion
	}
}
