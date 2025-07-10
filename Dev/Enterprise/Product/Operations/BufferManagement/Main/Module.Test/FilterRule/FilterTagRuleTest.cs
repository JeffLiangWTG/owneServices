using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.BufferManagement.Module.Test
{
	public class FilterTagRuleTest : BMSTestCaseWithFactory
	{
		public void TestRemoveTag_UpdatesCompletedTime()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "MIP", "Military Police");
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup, "MOP", "Clean the floor");

			var jobHeader1 = CreateJobHeader<OrgHeader>(false, "I am not tagged.");
			var workflow1_1 = CreateWorkflow(jobHeader1, "I get untagged.");
			var workflow1_2 = CreateWorkflow(jobHeader1, "I am definitely not tagged");

			jobHeader1.AddTag(tag1);
			var w1tag1 = (TagLink)workflow1_1.AddTag(tag1).Link;
			var rule = BMSTestHelper.CreateTagRule(tag1, "The Rule Rule", TagRuleActionTypeList.Codes.RemoveTag);
			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.JobOrWorkflow,
				FilterStripValueSetter = f => ((JobOrWorkflowFilter)f).Property1 = true, // Workflows only
			});

			Factory.Save();

			var workflow1_lastEditTime = workflow1_1.FH_SystemLastEditTimeUtc;
			var workflow2_lastEditTime = workflow1_2.FH_SystemLastEditTimeUtc;

			var logger = new TestServiceLogger();
			var dummyRunner = new DummyTagRuleRunner(logger, rule);

			dummyRunner.Process();
			AssertContains("Information|Processed rule [The Rule Rule], rows modified [1]", logger.ToString());

			Factory.ReloadAll<ProcessHeader>();
			Factory.ReloadAll<TagLink>();

			AssertNull(Factory.CreateNewFactory().Load<TagLink>(w1tag1.PK));

			AssertNotEquals("Should change during update.", workflow1_lastEditTime, workflow1_1.FH_SystemLastEditTimeUtc);
			AssertEquals("Should not change. Already correct.", workflow2_lastEditTime, workflow1_2.FH_SystemLastEditTimeUtc);
		}

		public void TestMagnitudeRule_JobHeader_UpdatesLastEditOfAllChildren()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "POL", "Policy academy");
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup, "HOP", "Blackmail not snailmail.");

			var jobHeader1 = CreateJobHeader<OrgHeader>(false, "Joob");
			var workflow1_1 = CreateWorkflow(jobHeader1, "Terp");

			var jobHeader2 = CreateJobHeader<OrgHeader>(false, "Mermle");
			var workflow2_1 = CreateWorkflow(jobHeader2, "Gerp");

			var j1tag1 = jobHeader1.AddTag(tag1).Link;
			var j1tag2 = jobHeader2.AddTag(tag1).Link;
			j1tag1.TGL_Magnitude = 7m; // Already set to the expected magnitude.

			var rule = BMSTestHelper.CreateTagRule(tag1, "The Carbine rule", TagRuleActionTypeList.Codes.MaintainMagnitude);
			rule.TagTemplate.TGL_Magnitude = 7m;
			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.JobOrWorkflow,
				FilterStripValueSetter = f => ((JobOrWorkflowFilter)f).Property0 = true, // JoHeaders only
			});

			Factory.Save();

			var workflow1_lastEditTime = workflow1_1.FH_SystemLastEditTimeUtc;
			var workflow2_lastEditTime = workflow2_1.FH_SystemLastEditTimeUtc;

			var logger = new TestServiceLogger();
			var dummyRunner = new DummyTagRuleRunner(logger, rule);

			dummyRunner.Process();
			AssertContains("Information|Processed rule [The Carbine rule], rows modified [2]", logger.ToString());

			Factory.ReloadAll<ProcessHeader>();
			Factory.ReloadAll<TagLink>();

			AssertTagApplied(jobHeader1, tag1);
			AssertTagNotApplied(workflow1_1, tag1);
			AssertTagApplied(jobHeader2, tag1);
			AssertTagNotApplied(workflow2_1, tag1);

			AssertEquals("Should not have updated because parent wasn't updated.", workflow1_lastEditTime, workflow1_1.FH_SystemLastEditTimeUtc);
			AssertNotEquals("Should change during update.", workflow2_lastEditTime, workflow2_1.FH_SystemLastEditTimeUtc);
		}

		public void TestMagnitudeRule_JobHeader_CustomSql()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "POL", "Policy academy");
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup, "HOP", "Blackmail not snailmail.");

			var jobHeader1 = CreateJobHeader<OrgHeader>(false, "Joob");
			var workflow1_1 = CreateWorkflow(jobHeader1, "Terp");

			var jobHeader2 = CreateJobHeader<OrgHeader>(false, "Mermle");
			var workflow2_1 = CreateWorkflow(jobHeader2, "Gerp");

			var j1tag1 = jobHeader1.AddTag(tag1).Link;
			var j1tag2 = jobHeader2.AddTag(tag1).Link;
			j1tag1.TGL_Magnitude = 7m; // Already set to the expected magnitude.

			var rule = BMSTestHelper.CreateTagRule(tag1, "The Carbine rule", TagRuleActionTypeList.Codes.MaintainMagnitude);
			rule.TagTemplate.TGL_Magnitude = 7m;
			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "FH_CompletionStatement in ('Joob', 'Mermle')", // JoHeaders only
			});

			Factory.Save();

			var workflow1_lastEditTime = workflow1_1.FH_SystemLastEditTimeUtc;
			var workflow2_lastEditTime = workflow2_1.FH_SystemLastEditTimeUtc;

			var logger = new TestServiceLogger();
			var dummyRunner = new DummyTagRuleRunner(logger, rule);

			dummyRunner.Process();
			AssertContains("Information|Processed rule [The Carbine rule], rows modified [2]", logger.ToString());

			Factory.ReloadAll<ProcessHeader>();
			Factory.ReloadAll<TagLink>();

			AssertTagApplied(jobHeader1, tag1);
			AssertTagNotApplied(workflow1_1, tag1);
			AssertTagApplied(jobHeader2, tag1);
			AssertTagNotApplied(workflow2_1, tag1);

			AssertEquals("Should not have updated because parent wasn't updated.", workflow1_lastEditTime, workflow1_1.FH_SystemLastEditTimeUtc);
			AssertNotEquals("Should change during update.", workflow2_lastEditTime, workflow2_1.FH_SystemLastEditTimeUtc);
		}

		public void TestMagnitudeRule()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "POL", "Policy academy");
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup, "HOP", "Blackmail not snailmail.");
			var tag2 = BMSTestHelper.CreateTagMagnitude(tagGroup, "NEI", "Don't lie to your mother!");

			var jobHeader1 = CreateJobHeader<OrgHeader>(false, "My name is Daniel Clarke");
			var workflow1_1 = CreateWorkflow(jobHeader1, "<British>");
			var workflow1_2 = CreateWorkflow(jobHeader1, "<Customs>");
			var workflow1_3 = CreateWorkflow(jobHeader1, "<Barbershop>");

			var jobHeader2 = CreateJobHeader<OrgHeader>(false, "Lets go bother Snape");
			var workflow2_1 = CreateWorkflow(jobHeader2, "Crewel Joke");
			var task = CreateTask(workflow2_1, string.Empty, 60);

			var j1tag1 = jobHeader1.AddTag(tag1).Link;
			var w1tag1 = workflow1_1.AddTag(tag1).Link;
			var w2tag2 = workflow2_1.AddTag(tag2).Link;
			var w3tag1 = workflow1_3.AddTag(tag1).Link;
			var t1tag1 = task.AddTag(tag1).Link;

			w3tag1.TGL_Magnitude = 7m; // Already set to the expected magnitude.

			var rule = BMSTestHelper.CreateTagRule(tag1, "The Moopoo rule", TagRuleActionTypeList.Codes.MaintainMagnitude);
			rule.TagTemplate.TGL_Magnitude = 7m;
			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.JobOrWorkflow,
				FilterStripValueSetter = f => ((JobOrWorkflowFilter)f).Property1 = true, // Workflows only
			});

			Factory.Save();

			var workflow1_lastEditTime = workflow1_1.FH_SystemLastEditTimeUtc;
			var workflow2_lastEditTime = workflow2_1.FH_SystemLastEditTimeUtc;
			var workflow3_lastEditTime = workflow1_3.FH_SystemLastEditTimeUtc;

			var logger = new TestServiceLogger();
			var dummyRunner = new DummyTagRuleRunner(logger, rule);

			dummyRunner.Process();
			AssertContains("Information|Processed rule [The Moopoo rule], rows modified [1]", logger.ToString());

			Factory.ReloadAll<ProcessHeader>();
			Factory.ReloadAll<TagLink>();

			AssertTagApplied(jobHeader1, tag1);
			AssertTagApplied(workflow1_1, tag1);
			AssertTagNotApplied(workflow1_2, tag1);
			AssertTagNotApplied(jobHeader2, tag1);
			AssertTagNotApplied(workflow2_1, tag1);
			AssertTagApplied(workflow2_1, tag2);
			AssertTagApplied(task, tag1);

			AssertEquals("Ignored because of filter.", 1m, j1tag1.TGL_Magnitude);
			AssertEquals("Ignored because is task.", 1m, t1tag1.TGL_Magnitude);
			AssertEquals("Use the acceptability band magnitude.", 7m, w1tag1.TGL_Magnitude);
			AssertEquals("Use the acceptability band magnitude.", 7m, w3tag1.TGL_Magnitude);
			AssertEquals("Ignored because different tag.", 1m, w2tag2.TGL_Magnitude);

			AssertEquals("Make sure we aren't double tagging", 1, workflow1_1.TagLinks.Count);

			AssertEquals("Should not have updated.", workflow2_lastEditTime, workflow2_1.FH_SystemLastEditTimeUtc);
			AssertNotEquals("Should change during update.", workflow1_lastEditTime, workflow1_1.FH_SystemLastEditTimeUtc);
			AssertEquals("Doesn't change because magnitude already exists!", workflow3_lastEditTime, workflow1_3.FH_SystemLastEditTimeUtc);
		}

		public void TestTagger_1kWorkflows()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "RID", "Ride that lion", isExclusive: true);
			var magnitude0 = BMSTestHelper.CreateTagMagnitude(definition, "MAK", "But don't fall off", ruleRunSequence: 0);

			var rule = BMSTestHelper.CreateTagRule(magnitude0, "Tag Frono", TagRuleActionTypeList.Codes.AddTag);
			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Look out!",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflows = new List<ProcessHeader>();
			for (int i = 0; i < 1000; i++)
			{
				workflows.Add(BMSTestHelper.CreateWorkflow(jobHeader, "Look out!" + i));
			}

			Factory.Save();
			var logger = new TestServiceLogger();
			new DummyTagRuleRunner(logger, rule).Process();

			AssertEquals(2, logger.Count);
			var loggerOutput = logger.ToString();
		}

		public void TestTagger_1kWorkflowsAndJobs()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "RID", "Ride that lion", isExclusive: true);
			var magnitude0 = BMSTestHelper.CreateTagMagnitude(definition, "MAK", "But don't fall off", ruleRunSequence: 0);

			var rule = BMSTestHelper.CreateTagRule(magnitude0, "Tag Frono", TagRuleActionTypeList.Codes.AddTag);
			rule.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Look out!",
			});

			var workflows = new List<Tuple<ProcessJobHeader, ProcessHeader>>();
			for (int i = 0; i < 1000; i++)
			{
				var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
				workflows.Add(Tuple.Create(jobHeader, BMSTestHelper.CreateWorkflow(jobHeader, "Look out!" + i)));
			}

			Factory.Save();
			var logger = new TestServiceLogger();
			new DummyTagRuleRunner(logger, rule).Process();

			AssertEquals(2, logger.Count);
			var loggerOutput = logger.ToString();
		}

		public void TestTagger_HandleUnknownSQLFailure()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "RID", "Ride that lion", isExclusive: true);
			var magnitude0 = BMSTestHelper.CreateTagMagnitude(definition, "MAK", "But don't fall off", ruleRunSequence: 0);

			var rule = BMSTestHelper.CreateTagRule(magnitude0, "Tag Frono", TagRuleActionTypeList.Codes.AddTag);

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = "Custom SQL Filter",
				FilterStripValueSetter = f => ((ModuleSQLFilter)f).Property1 = "This SQL is fairly invalid",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow0 = BMSTestHelper.CreateWorkflow(jobHeader, "Look out!");

			Factory.Save();
			var logger = new TestServiceLogger();
			new ValidationlessDummyTagRuleRunner(logger, rule).Process();

			AssertEquals(1, logger.Count);
			var loggerOutput = logger.ToString();
			AssertContains("Incorrect syntax near 'SQL'", loggerOutput);
			ErrorReporter.Clear();
		}

		#region Validationless Dummy Tag Rule Runner

		class ValidationlessDummyTagRuleRunner : DummyTagRuleRunner
		{
			public ValidationlessDummyTagRuleRunner(ILogger logger, params TagRule[] rules)
				: base(logger, rules)
			{
			}

			protected override IEnumerable<TagRule> GetTagRulesToRun(TagRule[] tagRules)
			{
				return tagRules;
			}
		}

		#endregion

		public void TestExclusiveTagging_RulesAreOrderedBySequence()
		{
			TestCaseHelper.ClearTable(TagRuleSchema.Constants.TableName);

			var definition = BMSTestHelper.CreateTagDefinition(Factory, "SIX", "Six big poodles", isExclusive: true);
			var magnitude0 = BMSTestHelper.CreateTagMagnitude(definition, "EAT", "They are scrumptious", ruleRunSequence: 0);
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "HUG", "They are cuddly", ruleRunSequence: 1);

			var rule1 = BMSTestHelper.CreateTagRule(magnitude1, "Tag Frodo", TagRuleActionTypeList.Codes.AddTag);
			var rule2 = BMSTestHelper.CreateTagRule(magnitude0, "Tag Frono", TagRuleActionTypeList.Codes.AddTag);

			rule1.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			rule2.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			FilterStripsTestHelper.AddFilterStrips(rule1.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Look out!",
			});
			FilterStripsTestHelper.AddFilterStrips(rule2.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Look out!",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow0 = BMSTestHelper.CreateWorkflow(jobHeader, "Look out!");

			Factory.Save();
			var logger = new TestServiceLogger();
			new TagRuleRunner(logger).Process();

			AssertEquals(1, workflow0.TagLinks.Count);
			AssertCollectionContains(magnitude0, TagProvider.GetApplicableTags(workflow0));
			AssertContains("Frono happens before Frodo",
			string.Format(@"Information|Processed rule [Tag Frono], rows modified [1], branch [{0}], department [{1}]
Information|Processed rule [Tag Frodo], rows modified [0], branch [{0}], department [{1}]", Env.CurrentBranch.Code, Env.CurrentDepartment.Code), logger.ToString());
		}

		public void TestExclusiveTagging_LowestSequenceWins()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "TEN", "Ten trillion grillion billion ways to ride a hippopotamus", isExclusive: true);
			var magnitude0 = BMSTestHelper.CreateTagMagnitude(definition, "JUS", "Just sit on it", ruleRunSequence: 0);
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "SAD", "Maybe have a saddle", ruleRunSequence: 1);
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition, "ANG", "Poke it until it's angry", ruleRunSequence: 2);
			var magnitude3 = BMSTestHelper.CreateTagMagnitude(definition, "POE", "Write a love poem", ruleRunSequence: 1);

			var rule = BMSTestHelper.CreateTagRule(magnitude1, "Tag Frodo", TagRuleActionTypeList.Codes.AddTag);

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Look out!",
			});

			var workflow0 = CreateLookOutWorkflow(magnitude0);
			var workflow1 = CreateLookOutWorkflow(magnitude1);
			var workflow2 = CreateLookOutWorkflow(magnitude2);
			var workflow3 = CreateLookOutWorkflow(magnitude3);

			var workflow1TagLink = workflow1.TagLinks.Single();

			Factory.Save();
			new TagRuleRunner(new DummyLogger()).Process();

			var newFactory = new BusinessObjectFactory();
			var newWorkflow0 = newFactory.Load<ProcessHeader>(workflow0.PK);
			var newWorkflow1 = newFactory.Load<ProcessHeader>(workflow1.PK);
			var newWorkflow2 = newFactory.Load<ProcessHeader>(workflow2.PK);
			var newWorkflow3 = newFactory.Load<ProcessHeader>(workflow3.PK);

			var newMagnitude0 = newFactory.Load<TagMagnitude>(magnitude0.PK);
			var newMagnitude1 = newFactory.Load<TagMagnitude>(magnitude1.PK);
			var newMagnitude2 = newFactory.Load<TagMagnitude>(magnitude2.PK);
			var newMagnitude3 = newFactory.Load<TagMagnitude>(magnitude3.PK);

			AssertEquals(1, newWorkflow0.TagLinks.Count);
			AssertEquals(1, newWorkflow1.TagLinks.Count);
			AssertEquals(1, newWorkflow2.TagLinks.Count);
			AssertEquals(1, newWorkflow3.TagLinks.Count);
			AssertCollectionContains("It's sequence is lower than the rule templates so no change", newMagnitude0, TagProvider.GetApplicableTags(newWorkflow0));
			AssertCollectionContains(newMagnitude1, TagProvider.GetApplicableTags(newWorkflow1));
			AssertEquals("Workflow 1 hasn't been retagged", workflow1TagLink.PK, newWorkflow1.TagLinks.Single().PK);
			AssertCollectionContains("The rule sequence was higher so it was re-tagged", newMagnitude1, TagProvider.GetApplicableTags(newWorkflow2));
			AssertCollectionContains("The rule sequence was the same as existing tag, so no change", newMagnitude3, TagProvider.GetApplicableTags(newWorkflow3));
		}

		ProcessHeader CreateLookOutWorkflow(TagMagnitude magnitude)
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_CompletionStatement = "Look out!";
			workflow.AddTag(magnitude);
			return workflow;
		}

		public void TestTagRule_ExclusiveScope_KillsExtras()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "CDL", "Cody Love");
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "BIG", "Big");

			var rule = BMSTestHelper.CreateTagRule(magnitude1, "Tag Frodo", TagRuleActionTypeList.Codes.AddTag);

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Look out!",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Look out!";
			workflow1.AddTag(magnitude1);
			workflow1.AddTag(magnitude1);

			AssertEquals(2, workflow1.TagLinks.Count);
			Factory.Save();

			definition.TGD_IsExclusive = true;

			Factory.Save();
			new TagRuleRunner(new DummyLogger()).Process();

			var loadedWorkflow1 = new BusinessObjectFactory().Load<ProcessHeader>(workflow1.PK);

			AssertEquals(1, loadedWorkflow1.TagLinks.Count);
		}

		public void TestTagRule_InvalidRulesDontRun_UsageScope()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "SIZ", "Size Categories");
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "BIG", "Big");

			var rule = BMSTestHelper.CreateTagRule(magnitude1, "Tag Frodo", TagRuleActionTypeList.Codes.AddTag);

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Look out!",
			});

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Look out!";

			Factory.Save();

			definition.TGD_UsageScope = TagUsageScopeList.Codes.User;
			Factory.Save();
			new TagRuleRunner(new DummyLogger()).Process();

			var newFactory = Factory.CreateNewFactory();
			var newWorkflow1 = newFactory.Load<ProcessHeader>(workflow1.PK);

			AssertTagNotApplied("No tagging occured because Tag Rule is Invalid.", newWorkflow1, magnitude1);
			AssertEquals(false, rule.TGR_IsActive);

			definition.TGD_UsageScope = TagUsageScopeList.Codes.All;
			rule.TGR_IsActive = true;
			Factory.Save();
			new TagRuleRunner(new DummyLogger()).Process();

			var newFactory2 = Factory.CreateNewFactory();
			var newWorkflow1_2 = newFactory2.Load<ProcessHeader>(workflow1.PK);

			AssertTagApplied("Tag applied, because the rule is valid again.", newWorkflow1_2, magnitude1);
			ErrorReporter.Clear();
		}

		public void TestTagRule_OverrideExclusiveTags()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "SIZ", "Size Categories", isExclusive: true);
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "BIG", "Big", ruleRunSequence: 0);
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition, "SMA", "Small", ruleRunSequence: 1);

			var rule = Factory.New<TagRule>();
			rule.TGR_IsActive = true;
			rule.TGR_Name = "Tag Frodo";
			rule.TGR_ActionType = TagRuleActionTypeList.Codes.AddTag;

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Look out!",
			});

			var template = rule.TagTemplate;
			template.TGL_TGM_Magnitude = magnitude1.PK;
			template.TGL_Magnitude = 19;
			template.TGL_Description = "I'm A Description";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Look out!";

			var tag = (TagLink)workflow1.AddTag(magnitude2).Link;

			Factory.Save();

			new TagRuleRunner(new DummyLogger()).Process();

			var newFactory = Factory.CreateNewFactory();
			AssertNull(newFactory.Load<TagLink>(tag.PK));
			AssertTagApplied("First workflow has been tagged with Magnitude1", newFactory.Load<ProcessHeader>(workflow1.PK), magnitude1);
		}

		public void TestTagRule_Add()
		{
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "LTR");
			definition.TGD_IsExclusive = false;
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "FRO", "Bilbo");
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition, "GIM", "Gimli");

			var rule = Factory.New<TagRule>();
			rule.TGR_IsActive = true;
			rule.TGR_Name = "Tag Frodo";
			rule.TGR_ActionType = TagRuleActionTypeList.Codes.AddTag;

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Look out!",
			});

			var template = rule.TagTemplate;
			template.TGL_TGM_Magnitude = magnitude1.PK;
			template.TGL_Magnitude = 19;
			template.TGL_Description = "I'm A Description";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Look out!";
			workflow1.AddTag(magnitude2);

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Look out!!";

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "Surely this workflow will be ignored!";

			Factory.Save();

			new TagRuleRunner(new DummyLogger()).Process();

			var newFactory = new BusinessObjectFactory();
			var loadedWorkflow1 = newFactory.Load<ProcessHeader>(workflow1.PK);
			var loadedWorkflow2 = newFactory.Load<ProcessHeader>(workflow2.PK);
			var loadedWorkflow3 = newFactory.Load<ProcessHeader>(workflow3.PK);

			AssertTagApplied("First workflow has been tagged with FRO", loadedWorkflow1, magnitude1);
			AssertTagApplied("First workflow has been tagged with GIM", loadedWorkflow1, magnitude2);
			AssertTagApplied("Second workflow has been tagged with FRO", loadedWorkflow2, magnitude1);
			AssertTagNotApplied("Third workflow has not been tagged", loadedWorkflow3, magnitude1);

			new TagRuleRunner(new DummyLogger()).Process();

			var newFactory2 = new BusinessObjectFactory();
			var loadedWorkflow1_2 = newFactory2.Load<ProcessHeader>(workflow1.PK);
			var loadedWorkflow2_2 = newFactory2.Load<ProcessHeader>(workflow2.PK);
			var loadedWorkflow3_2 = newFactory2.Load<ProcessHeader>(workflow3.PK);

			AssertTagApplied("First workflow has been tagged with FRO", loadedWorkflow1_2, magnitude1);
			AssertTagApplied("Second workflow has been tagged with FRO", loadedWorkflow2_2, magnitude1);
			AssertTagNotApplied("Third workflow has not been tagged", loadedWorkflow3_2, magnitude1);
		}

		public void TestTagRule_Remove()
		{
			var definition = Factory.NewWithValidTestData<TagDefinition>();
			var magnitude1 = BMSTestHelper.CreateTagMagnitude(definition, "FRO");
			var magnitude2 = BMSTestHelper.CreateTagMagnitude(definition, "WAG");

			var rule = Factory.New<TagRule>();
			rule.TGR_IsActive = true;
			rule.TGR_Name = "Tag Frodo";
			rule.TGR_ActionType = TagRuleActionTypeList.Codes.RemoveTag;

			FilterStripsTestHelper.AddFilterStrips(rule.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = "Look out!",
			});

			var template = rule.TagTemplate;
			template.TGL_TGM_Magnitude = magnitude1.PK;
			template.TGL_Magnitude = 19;
			template.TGL_Description = "I'm A Description";

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "Look out!";
			workflow1.AddTag(magnitude1);

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Look out!!";
			workflow2.AddTag(magnitude1);
			workflow2.AddTag(magnitude2);

			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "Surely this workflow will be ignored!";
			workflow3.AddTag(magnitude2);

			Factory.Save();

			AssertTagApplied("First workflow has been tagged with FRO", workflow1, magnitude1);
			AssertTagApplied("Second workflow has been tagged with FRO", workflow2, magnitude1);
			AssertTagApplied("Second workflow has been tagged with WAG", workflow2, magnitude2);
			AssertTagApplied("Third workflow has been tagged with WAG", workflow3, magnitude2);

			new TagRuleRunner(new DummyLogger()).Process();

			var newFactory = new BusinessObjectFactory();

			var loadedWorkflow1 = newFactory.Load<ProcessHeader>(workflow1.PK);
			var loadedWorkflow2 = newFactory.Load<ProcessHeader>(workflow2.PK);
			var loadedWorkflow3 = newFactory.Load<ProcessHeader>(workflow3.PK);

			AssertTagNotApplied("First workflow has no FRO tag", loadedWorkflow1, magnitude1);
			AssertTagNotApplied("Second workflow has no FRO tag", loadedWorkflow2, magnitude1);
			AssertTagApplied("Second workflow has been tagged with WAG", loadedWorkflow2, magnitude2);
			AssertTagApplied("Third workflow has been tagged with WAG", loadedWorkflow3, magnitude2);
		}

		[TestDate(2014, 6, 2)]
		[TestDateIncremental(seconds: 1)]
		public void TestTagRuleLogging()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "PRI", "Priority");
			var platinum = BMSTestHelper.CreateTagMagnitude(tagGroup, "PLT", "Platinum");
			var gold = BMSTestHelper.CreateTagMagnitude(tagGroup, "GLD", "Gold");
			var red = BMSTestHelper.CreateTagMagnitude(tagGroup, "RED", "Red");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MAIORGSYD";
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			jobHeader.FH_CompletionStatement = "jobHeader";
			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";
			workflow3.FH_CompletionStatement = "workflow3";

			workflow1.AddTag(platinum);
			workflow2.AddTag(platinum);
			workflow3.AddTag(gold).Link.TGL_Magnitude = 0m;

			var rule1 = BMSTestHelper.CreateTagRule(platinum, "Remove Platinum", TagRuleActionTypeList.Codes.RemoveTag);
			var rule2 = BMSTestHelper.CreateTagRule(gold, "Add Gold", TagRuleActionTypeList.Codes.AddTag);
			var rule3 = BMSTestHelper.CreateTagRule(red, "Add Red", TagRuleActionTypeList.Codes.AddTag);

			rule1.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			rule2.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow.AddDays(-2);
			rule3.TGR_LastPerformanceVerificationDateTimeUtc = ZDateTime.UtcNow.AddDays(-2);

			FilterStripsTestHelper.AddFilterStrips(rule1.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.JobOrWorkflow,
				FilterStripValueSetter = f => ((JobOrWorkflowFilter)f).Property1 = true, // Workflow only
			});
			FilterStripsTestHelper.AddFilterStrips(rule2.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.JobOrWorkflow,
				FilterStripValueSetter = f => ((JobOrWorkflowFilter)f).Property1 = true, // Workflow only
			});

			FilterStripsTestHelper.AddFilterStrips(rule3.Filter,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.JobOrWorkflow,
				FilterStripValueSetter = f => ((JobOrWorkflowFilter)f).Property0 = true, // Job only
			});

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			var logger = new BufferManagementLogger();
			var runner = new TagRuleRunner(logger);
			runner.Process();

			var newFactory = new BusinessObjectFactory();

			var loadedJobHeader = newFactory.Load<ProcessJobHeader>(jobHeader.PK);
			var loadedWorkflow1 = newFactory.Load<ProcessHeader>(workflow1.PK);
			var loadedWorkflow2 = newFactory.Load<ProcessHeader>(workflow2.PK);
			var loadedWorkflow3 = newFactory.Load<ProcessHeader>(workflow3.PK);

			AssertTagApplied(loadedJobHeader, red);

			AssertTagNotApplied(loadedWorkflow1, platinum);
			AssertTagNotApplied(loadedWorkflow2, platinum);

			AssertTagApplied(loadedWorkflow1, gold);
			AssertTagApplied(loadedWorkflow2, gold);
			AssertTagApplied(loadedWorkflow3, gold);

			var logOutput = logger.ToString();

			AssertContains(string.Format(@"Processed rule [Add Gold], rows modified [2], branch [{0}], department [{1}]", Env.CurrentBranch.Code, Env.CurrentDepartment.Code), logOutput);
			AssertContains(string.Format(@"Processed rule [Add Red], rows modified [1], branch [{0}], department [{1}]", Env.CurrentBranch.Code, Env.CurrentDepartment.Code), logOutput);
			AssertContains(string.Format(@"Processed rule [Remove Platinum], rows modified [2], branch [{0}], department [{1}]", Env.CurrentBranch.Code, Env.CurrentDepartment.Code), logOutput);

			var logs = loadedJobHeader.Logs.GetAllLogs().Cast<StmALog>().Select(l => l.SL_Reference);

			AssertTagEventRaised(loadedJobHeader, TagActionType.AddTag, red, rule3);
			AssertTagEventRaised(workflow1, TagActionType.RemoveTag, platinum, rule1);
			AssertTagEventRaised(workflow2, TagActionType.RemoveTag, platinum, rule1);
			AssertTagEventRaised(workflow1, TagActionType.AddTag, gold, rule2);
			AssertTagEventRaised(workflow2, TagActionType.AddTag, gold, rule2);
		}
	}
}
