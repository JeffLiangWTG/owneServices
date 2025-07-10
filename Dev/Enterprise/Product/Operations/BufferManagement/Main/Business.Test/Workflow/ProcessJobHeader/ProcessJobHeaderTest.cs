using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ProcessJobHeader))]
	public class ProcessJobHeaderTest : EnterpriseBusinessObjectTestCase
	{
		#region Performance

		public void TestGetForParentShouldNotApplyTemplatesOnEveryCheck()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");
			var workItem = Factory.New<IWorkItem>();
			((BusinessObject)workItem).FillWithValidTestData();
			Factory.Save();
			var count = 0;
			void OnMatch()
			{
				count++;
			}
			ProcessTask.Loader.OnLoadTemplateMatches_ForTest.Value = OnMatch;

			workItem.WKI_Summary = "Hoping for the best";
			for (int i = 0; i < 10; i++)
			{
				AssertNotNull(ProcessJobHeader.GetForParent((IWorkflowProvider)workItem, Factory));
			}
			AssertEquals("Why should this evaluate the template over and over?", 1, count);
		}

		public void TestApplyTemplate_ProcessHeaderLinkQueryShouldHave2QueriesForHeaderFromAndHeaderTo()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");
			var templateWorkflow = template.ProcessHeaders.AddNew();
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_FH_ProcessHeader = templateWorkflow.PK;

			Factory.Save();

			using (Db.Connection.TrackExecutedCommands(includeStackTrace: true))
			{
				using (var settings = TestEntityFrameworkSettings.Get())
				{
					settings.TVPRule = new TVPRule("0");
					var newFactory = new BusinessObjectFactory() { NameForDebugging = "TestApplyTemplate_ProcessHeaderLinkQueryShouldHave2QueriesForHeaderFromAndHeaderTo New Factory" };

					var job = newFactory.New<IWorkItem>();
					job.WKI_Summary = "Summary X";
					newFactory.Save();

					var jobHeader = ProcessJobHeader.GetForParent((IWorkflowProvider)job, newFactory);
					AssertNotNull("jobHeader should be created", jobHeader);
					AssertEquals("should have 1 workflow from template", 1, jobHeader.ProcessHeaders.Count);

					var relevantQueries = Db.Connection.ExecutedCommands.Where(x => x.Contains($"FROM {ProcessHeaderLinkSchema.Constants.SqlSchemaName}.{ProcessHeaderLinkSchema.Constants.TableName}"));

					CombineAssertions("WHEN apply template, THEN should have 2 queries for FP_FH_HeaderTo and FP_FH_HeaderFrom instead of 'OR'", () =>
					{
						Assert("should have ProcessHeaderLink query", relevantQueries.Any());

						foreach (var query in relevantQueries)
						{
							AssertNotContains("should not contain 'or (FP_FH_HeaderTo'", " or (FP_FH_HeaderTo", query, ignoreCase: true);
							AssertNotContains("should not contain 'or (FP_FH_HeaderFrom'", " or (FP_FH_HeaderFrom", query, ignoreCase: true);
							AssertContains("should contain 'Table valued parameters'", "SELECT Value FROM @CWO", query, ignoreCase: true);
						}
					});
				}
			}
		}

		#endregion

		#region Default Values

		public void TestDefaultCategoryValues()
		{
			var jobHeader = Factory.New<ProcessJobHeader>();
			AssertEquals(BMConstants.JobLevelWorkflowCategoryCode, jobHeader.FH_Category);

			var workflow = Factory.New<ProcessHeader>();
			AssertEquals("UDF", workflow.FH_Category);
		}

		#endregion

		#region Demote

		public void TestDemote_MovesTasks()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1_1 = jobHeader1.ProcessHeaders.AddNew();

			var workflow2_1 = jobHeader2.ProcessHeaders.AddNew();
			var workflow2_2 = jobHeader2.ProcessHeaders.AddNew();

			var task1 = BMSTestHelper.CreateTask(workflow1_1, string.Empty, 30);
			var task2 = BMSTestHelper.CreateTask(workflow2_1, string.Empty, 30);
			var task3 = BMSTestHelper.CreateTask(workflow2_2, string.Empty, 30);

			var name = jobHeader1.ProviderJobNumber + ": " + jobHeader1.FH_CompletionStatement;

			var newWorkflow = jobHeader1.Demote(jobHeader2.Parent, Factory);

			AssertEquals(false, jobHeader1.ProcessHeaders.Any());

			Factory.Save();

			AssertEquals("Make sure the demoted job header hasn't recreated it's workflows.", false, jobHeader1.ProcessHeaders.Any());

			jobHeader1.Validation.ValidateAll();

			AssertEquals(name, newWorkflow.FH_CompletionStatement);
			AssertEquals(true, workflow1_1.IsDeleted);
			AssertNoErrors(jobHeader1);
			AssertNoErrors(jobHeader2);
			AssertNoErrors(workflow2_1);
			AssertNoErrors(workflow2_2);
			AssertEquals(jobHeader2.Parent.PK, task1.P9_ParentID);
			AssertEquals(jobHeader2.Parent.PK, task2.P9_ParentID);
			AssertEquals(jobHeader2.Parent.PK, task3.P9_ParentID);
		}

		public void TestDemote_CollapsesWorkflowsIntoOne()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1_1 = jobHeader1.ProcessHeaders.AddNew();
			var workflow1_2 = jobHeader1.ProcessHeaders.AddNew();
			var workflow1_3 = jobHeader1.ProcessHeaders.AddNew();

			var workflow2_1 = jobHeader2.ProcessHeaders.AddNew();
			var workflow2_2 = jobHeader2.ProcessHeaders.AddNew();

			var link1 = workflow1_1.GetOrCreateDependencyLink(workflow1_2);
			var link2 = workflow1_2.GetOrCreateDependencyLink(workflow1_3);
			var link3 = workflow2_1.GetOrCreateDependencyLink(workflow2_2);

			var task1_1 = BMSTestHelper.CreateTask(workflow1_1, string.Empty, 30);
			var task1_3 = BMSTestHelper.CreateTask(workflow1_3, string.Empty, 30);
			var task2_1 = BMSTestHelper.CreateTask(workflow2_1, string.Empty, 30);
			var task2_2 = BMSTestHelper.CreateTask(workflow2_2, string.Empty, 30);

			AssertNoErrors(jobHeader1);
			AssertNoErrors(jobHeader2);

			var newWorkflow = jobHeader1.Demote(jobHeader2.Parent, Factory);

			Factory.Save();

			AssertNoErrors(jobHeader2);
			AssertNoErrors(jobHeader1);

			AssertEquals(true, workflow1_1.IsDeleted);
			AssertEquals(true, workflow1_2.IsDeleted);
			AssertEquals(true, workflow1_3.IsDeleted);
			AssertEquals(true, link1.IsDeleted);
			AssertEquals(true, link2.IsDeleted);
			AssertEquals(newWorkflow, task1_1.ProcessHeader);
			AssertEquals(newWorkflow, task1_3.ProcessHeader);
			AssertEquals(false, newWorkflow.LinksFromOthersToMe.Any());
			AssertEquals(false, newWorkflow.LinksFromMeToOthers.Any());
		}

		public void TestDemote_ExternalLinkOnNewWorkflow()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1_1 = jobHeader1.ProcessHeaders.AddNew();
			var workflow1_2 = jobHeader1.ProcessHeaders.AddNew();
			var workflow2_1 = jobHeader2.ProcessHeaders.AddNew();
			var workflow2_2 = jobHeader2.ProcessHeaders.AddNew();

			var task1 = BMSTestHelper.CreateTask(workflow1_1, string.Empty, 30);
			var task2 = BMSTestHelper.CreateTask(workflow2_1, string.Empty, 30);
			var task3 = BMSTestHelper.CreateTask(workflow2_2, string.Empty, 30);

			var link1 = jobHeader1.GetOrCreateDependencyLink(workflow2_1);
			var link2 = workflow2_2.GetOrCreateDependencyLink(jobHeader1);
			var link3 = workflow2_2.GetOrCreateDependencyLink(workflow1_1);

			var newWorkflow = jobHeader1.Demote(jobHeader2.Parent, Factory);

			AssertEquals(true, link1.IsDeleted);
			AssertEquals(true, link2.IsDeleted);
			AssertEquals(true, link3.IsDeleted);

			AssertNoErrors(newWorkflow);

			AssertCollectionContains(newWorkflow.Links.Cast<ProcessHeaderLink>(), l => l.HeaderTo == workflow2_1);
			AssertCollectionContains(newWorkflow.Links.Cast<ProcessHeaderLink>(), l => l.HeaderFrom == workflow2_2);
		}

		public void TestDemote_DontCreateInvalidRelationships()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var workflow1_1 = jobHeader1.ProcessHeaders.AddNew();
			var workflow2_1 = jobHeader2.ProcessHeaders.AddNew();

			var task1 = BMSTestHelper.CreateTask(workflow1_1, string.Empty, 30);
			var task2 = BMSTestHelper.CreateTask(workflow2_1, string.Empty, 30);

			var link1 = jobHeader1.GetOrCreateDependencyLink(jobHeader2);

			var newWorkflow = jobHeader1.Demote(jobHeader2.Parent, Factory);

			AssertEquals(true, link1.IsDeleted);

			AssertNoErrors(newWorkflow);
			AssertNoErrors(jobHeader2);
			AssertEquals("The invalid link between jobHeader2 and workflow2_1 shouldn't exist.", false, jobHeader2.Links.Any());
		}

		#endregion

		#region Job-level Workflow Release Groups

		public void TestJobHeaderReleaseGroup_NotReadOnly()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");

			AssertEquals(false, jobHeader.FH_GG_ReleaseGroupInfo.ReadOnly);
			AssertEquals(false, workflow.FH_GG_ReleaseGroupInfo.ReadOnly);
		}

		public void TestJobLevelWorkflowReleaseGroup_DeleteTasksAndWorkflowsFromJob_ReapplyTemplate_EnsureReleaseGroupReCopied()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");
			var templateGroupOld = Factory.NewWithValidTestData<GlbGroup>();

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");
			var templateJobHeader = template.GetJobHeader();
			templateJobHeader.FH_GG_ReleaseGroup = templateGroupOld.PK;
			var templateWorkflow = (ProcessHeader)template.ProcessHeaders.AddNew();
			BMSTestHelper.CreateTask(template, templateWorkflow);

			Factory.Save();

			var job = Factory.New<IWorkItem>();
			job.WKI_Summary = "Do the thing.";

			Factory.Save();

			var jobHeader = ProcessJobHeader.GetForParent((IWorkflowProvider)job, Factory);
			AssertNotNull(jobHeader);
			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals(templateGroupOld.PK, jobHeader.FH_GG_ReleaseGroup);

			jobHeader.ProcessHeaders.First().Tasks.DeleteAll();
			jobHeader.ProcessHeaders.DeleteAll();
			jobHeader.Delete();

			var templateGroupNew = Factory.NewWithValidTestData<GlbGroup>();
			templateJobHeader.FH_GG_ReleaseGroup = templateGroupNew.PK;

			Factory.Save();

			jobHeader = ProcessJobHeader.GetForParent((IWorkflowProvider)job, Factory);
			AssertNotNull(jobHeader);
			jobHeader.ApplyTemplate(template);
			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals(templateGroupNew.PK, jobHeader.FH_GG_ReleaseGroup);
		}

		public void TestJobLevelWorkflowReleaseGroup_EnsureFirstSpecifiedJobLevelWorkflowIsApplied_MultipleTemplatesFallback()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");
			var template1Group = Factory.NewWithValidTestData<GlbGroup>();
			var template2Group = Factory.NewWithValidTestData<GlbGroup>();

			var releaseGroupForTemplate1 = system.ReleaseGroups.AddNew();
			releaseGroupForTemplate1.FSG_GG_Group = template1Group.PK;
			var releaseGroupForTemplate2 = system.ReleaseGroups.AddNew();
			releaseGroupForTemplate2.FSG_GG_Group = template2Group.PK;

			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");
			var template1JobHeader = template1.GetJobHeader();
			template1JobHeader.FH_GG_ReleaseGroup = template1Group.PK;
			var template1Workflow = template1.ProcessHeaders.AddNew();

			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");
			var template2JobHeader = template2.GetJobHeader();
			template2JobHeader.FH_GG_ReleaseGroup = template2Group.PK;
			var template2Workflow = template2.ProcessHeaders.AddNew();

			template1.P0_SubType1 = "ENT";

			template2.P0_SubType1 = "ENT";
			template2.P0_SubType3 = "FIX";

			template1.P0_TaskFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			template2.P0_TaskFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

			Factory.Save();

			var job = Factory.New<IWorkItem>();
			job.WKI_Summary = "Do the thing.";
			job.WKI_WorkItemType = "ENT";
			job.WKI_ActivityType = "FIX";

			Factory.Save();

			var jobHeader = ProcessJobHeader.GetForParent((IWorkflowProvider)job, Factory);
			AssertNotNull(jobHeader);
			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals("The JLW release group should only be set to the one specified on the highest priority template", template2Group.PK, jobHeader.FH_GG_ReleaseGroup);
		}

		public void TestJobLevelWorkflowReleaseGroup_ShouldNotOverrideManuallySetReleaseGroup_MultipleTemplatesFallback()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");
			var template1Group = Factory.NewWithValidTestData<GlbGroup>();
			var template2Group = Factory.NewWithValidTestData<GlbGroup>();

			var releaseGroupForTemplate1 = system.ReleaseGroups.AddNew();
			releaseGroupForTemplate1.FSG_GG_Group = template1Group.PK;
			var releaseGroupForTemplate2 = system.ReleaseGroups.AddNew();
			releaseGroupForTemplate2.FSG_GG_Group = template2Group.PK;

			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");
			var template1JobHeader = template1.GetJobHeader();
			template1JobHeader.FH_GG_ReleaseGroup = template1Group.PK;
			var template1Workflow = template1.ProcessHeaders.AddNew();

			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");
			var template2JobHeader = template2.GetJobHeader();
			template2JobHeader.FH_GG_ReleaseGroup = template2Group.PK;
			var template2Workflow = template2.ProcessHeaders.AddNew();

			template1.P0_SubType1 = "ENT";

			template2.P0_SubType1 = "ENT";
			template2.P0_SubType3 = "FIX";

			template1.P0_TaskFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			template2.P0_TaskFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

			Factory.Save();

			var job = Factory.New<IWorkItem>();
			job.WKI_Summary = "Do the thing.";
			job.WKI_WorkItemType = "ENT";
			job.WKI_ActivityType = "FIX";
			var jobHeader = ProcessJobHeader.GetForParent((IWorkflowProvider)job, Factory);
			AssertNotNull(jobHeader);
			jobHeader.FH_GG_ReleaseGroup = template1Group.PK;

			Factory.Save();

			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals("JLW's manually-set release group should remain unchanged even after template application", template1Group.PK, jobHeader.FH_GG_ReleaseGroup);
		}

		public void TestJobLevelWorkflowReleaseGroup_EnsureReleaseGroupRulesWorkOnTemplateReapplication()
		{
			BMSTestHelper.CreateSystem(Factory, "WKI");
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			var group2 = Factory.NewWithValidTestData<GlbGroup>();

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", "ENT");
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);
			BMSTestHelper.CreateTask(template, templateWorkflow);

			var rule = (IProcessTemplateReleaseGroupRule)template.ReleaseGroupRules.AddNew();
			rule.PTR_ValueSelectionMacro = "<WKI_ActivityType>";
			var map1 = (IProcessTemplateReleaseGroupRuleMapping)rule.GroupMappings.AddNew();
			map1.PTM_Value = "BUF";
			map1.PTM_GG_Group = group1.PK;
			var map2 = (IProcessTemplateReleaseGroupRuleMapping)rule.GroupMappings.AddNew();
			map2.PTM_Value = "FIX";
			map2.PTM_GG_Group = group2.PK;

			Factory.Save();

			var job = Factory.New<IWorkItem>();
			job.WKI_Summary = "Do the thing.";
			job.WKI_WorkItemType = "ENT";
			job.WKI_ActivityType = "BUF";
			var jobHeader = ProcessJobHeader.GetForParent((IWorkflowProvider)job, Factory, false);
			AssertNotNull(jobHeader);

			Factory.Save();

			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals("Template should set JLW release group to Group 1", group1.PK, jobHeader.FH_GG_ReleaseGroup);
			AssertEquals("Template should set WF release group to Group 1", group1.PK, jobHeader.ProcessHeaders[0].FH_GG_ReleaseGroup);

			job.WKI_ActivityType = "FIX";
			jobHeader.ProcessHeaders[0].Delete();

			Factory.Save();

			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals("Template should set JLW release group to Group 2", group2.PK, jobHeader.FH_GG_ReleaseGroup);
			AssertEquals("Template should set WF release group to Group 2", group2.PK, jobHeader.ProcessHeaders[0].FH_GG_ReleaseGroup);
		}

		#endregion

		#region Apply Template

		public void TestApplySingleTemplate_EnsureSystemSpecifiedInJobLevelWorkflowIsAutomaticallyApplied()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var system = config.System;
			var systemForTemplate = BMSTestHelper.CreateSystem(Factory);

			var templateGroup = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroupForTemplate = system.ReleaseGroups.AddNew();
			releaseGroupForTemplate.FSG_GG_Group = templateGroup.PK;

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG", isPartial: false);
			template.P0_FS_BufferManagementSystem = systemForTemplate.PK;

			var templateJobHeader = template.GetJobHeader();
			templateJobHeader.FH_GG_ReleaseGroup = templateGroup.PK;

			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template, "From");
			var templateTask1 = BMSTestHelper.CreateTask(template, templateWorkflow1, description: "from");

			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template, "To");
			var templateTask2 = BMSTestHelper.CreateTask(template, templateWorkflow2, description: "to");

			var templateLink = BMSTestHelper.CreateDependencyLink(template, templateWorkflow1, templateWorkflow2);

			templateJobHeader.AddTag(config.DerpyHoovesTag);
			templateWorkflow1.AddTag(config.PrincessCelestiaTag);

			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, false);

			CombineAssertions("Before saving", () =>
			{
				AssertEquals("Should apply the system specified in the template to the job header before saving", systemForTemplate.PK, jobHeader.BMSystem.PK);

				AssertEquals(0, jobHeader.ProcessHeaders.Count);
				AssertEquals(0, job.WorkflowItems.Count);
				AssertEquals("Release group copied over", templateGroup.PK, jobHeader.FH_GG_ReleaseGroup);
			});

			Factory.Save();

			CombineAssertions("After saving", () =>
			{
				AssertEquals("Should retain the system in the job header after saving", systemForTemplate.PK, jobHeader.BMSystem.PK);
				AssertEquals("Release group copied over", templateGroup.PK, jobHeader.FH_GG_ReleaseGroup);

				AssertEquals("Workflows copied over", 2, jobHeader.ProcessHeaders.Count);
				AssertEquals("Tasks copied over", 2, job.WorkflowItems.Count);
				AssertEquals("From link copied over", 1, jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "From").Links.Count());
				AssertEquals("To link copied over", 1, jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "To").Links.Count());

				BMSTestCaseWithFactory.AssertTagApplied(jobHeader, config.DerpyHoovesTag);
				BMSTestCaseWithFactory.AssertTagApplied(jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "From"), config.PrincessCelestiaTag);
			});
		}

		public void TestApplyFallbackTemplate_EnsureRelevantSystemSpecifiedInJobLevelWorkflowIsAutomaticallyApplied()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory, "WKI");

			var system = config.System;
			var systemForParentTemplate = BMSTestHelper.CreateSystem(Factory);
			var systemForChildTemplate = BMSTestHelper.CreateSystem(Factory);

			var templateGroupParent = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroupForParentTemplate = system.ReleaseGroups.AddNew();
			releaseGroupForParentTemplate.FSG_GG_Group = templateGroupParent.PK;

			var templateGroupChild = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroupForChildTemplate = system.ReleaseGroups.AddNew();
			releaseGroupForChildTemplate.FSG_GG_Group = templateGroupChild.PK;

			var parentTemplate = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", isPartial: false);
			parentTemplate.P0_FS_BufferManagementSystem = systemForParentTemplate.PK;

			var parentTemplateJobHeader = parentTemplate.GetJobHeader();
			parentTemplateJobHeader.FH_GG_ReleaseGroup = templateGroupParent.PK;

			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(parentTemplate, "From");
			var templateTask1 = BMSTestHelper.CreateTask(parentTemplate, templateWorkflow1, description: "f");

			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(parentTemplate, "To");
			var templateTask2 = BMSTestHelper.CreateTask(parentTemplate, templateWorkflow2, description: "t");

			var templateLink = BMSTestHelper.CreateDependencyLink(parentTemplate, templateWorkflow1, templateWorkflow2);

			parentTemplateJobHeader.AddTag(config.DerpyHoovesTag);
			templateWorkflow1.AddTag(config.PrincessCelestiaTag);

			var childTemplate = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", isPartial: false);
			childTemplate.P0_FS_BufferManagementSystem = systemForChildTemplate.PK;

			var childTemplateJobHeader = childTemplate.GetJobHeader();
			childTemplateJobHeader.FH_GG_ReleaseGroup = templateGroupChild.PK;

			var templateWorkflow3 = BMSTestHelper.CreateWorkflow(childTemplate, "Another");
			var templateTask3 = BMSTestHelper.CreateTask(childTemplate, templateWorkflow3, description: "a");

			parentTemplate.P0_SubType1 = "ENT";
			childTemplate.P0_SubType1 = "ENT";
			childTemplate.P0_SubType3 = "BUF";

			childTemplate.P0_TaskFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

			Factory.Save();

			var job = Factory.New<IWorkItem>();
			job.WKI_Summary = "Do the thing.";
			job.WKI_WorkItemType = "ENT";
			job.WKI_ActivityType = "BUF";

			var jobHeader = ProcessJobHeader.GetForParent((IWorkflowProvider)job, Factory, false);

			CombineAssertions("Before saving", () =>
			{
				AssertEquals("Should apply the most specific system specified in the template to the job header before saving", systemForChildTemplate.PK, jobHeader.BMSystem.PK);

				AssertEquals(0, jobHeader.ProcessHeaders.Count);
				AssertEquals("Release group copied over", templateGroupChild.PK, jobHeader.FH_GG_ReleaseGroup);
			});

			Factory.Save();

			CombineAssertions("After saving", () =>
			{
				AssertEquals("Should retain the system in the job header after saving", systemForChildTemplate.PK, jobHeader.BMSystem.PK);
				AssertEquals("Release group copied over and NOT overwritten", templateGroupChild.PK, jobHeader.FH_GG_ReleaseGroup);

				AssertEquals("Workflows copied over", 3, jobHeader.ProcessHeaders.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "From", "To", "Another" }, jobHeader.ProcessHeaders.Select(w => w.FH_CompletionStatement));
				AssertEquals("From link copied over", 1, jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "From").Links.Count());
				AssertEquals("To link copied over", 1, jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "To").Links.Count());

				BMSTestCaseWithFactory.AssertTagApplied(jobHeader, config.DerpyHoovesTag);
				BMSTestCaseWithFactory.AssertTagApplied(jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "From"), config.PrincessCelestiaTag);
			});
		}

		public void TestApplySingleTemplate_ShouldApplyReleaseGroupToJobHeader_IfJobHeaderNotAlreadySet()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var templateGroup = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroupForTemplate = system.ReleaseGroups.AddNew();
			releaseGroupForTemplate.FSG_GG_Group = templateGroup.PK;

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG", isPartial: false);
			var templateJobHeader = template.GetJobHeader();
			templateJobHeader.FH_GG_ReleaseGroup = templateGroup.PK;
			var templateWorkflow = template.ProcessHeaders.AddNew();

			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, false);

			Factory.Save();

			AssertEquals("Should change after template application", jobHeader.FH_GG_ReleaseGroup, templateGroup.PK);
		}

		public void TestApplySingleTemplate_ShouldNotApplyReleaseGroupToJobHeader_IfJobHeaderSet()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var templateGroup = Factory.NewWithValidTestData<GlbGroup>();
			var jobGroup = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroupForJob = system.ReleaseGroups.AddNew();
			releaseGroupForJob.FSG_GG_Group = jobGroup.PK;
			var releaseGroupForTemplate = system.ReleaseGroups.AddNew();
			releaseGroupForTemplate.FSG_GG_Group = templateGroup.PK;

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG", isPartial: false);
			var templateJobHeader = template.GetJobHeader();
			templateJobHeader.FH_GG_ReleaseGroup = templateGroup.PK;
			var templateWorkflow = template.ProcessHeaders.AddNew();

			Factory.Save();

			var job = Factory.New<OrgHeader>();
			job.OH_Code = "SUM";
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, false);
			jobHeader.FH_GG_ReleaseGroup = jobGroup.PK;

			Factory.Save();

			AssertEquals("Should not change after template application", jobHeader.FH_GG_ReleaseGroup, jobGroup.PK);
		}

		public void TestApplyTemplate_ShouldApplyReleaseGroupToChildWorkflowsIfSpecifiedOnTemplateJobWorkflow()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var templateGroupJLW = Factory.NewWithValidTestData<GlbGroup>();
			var templateGroupChild = Factory.NewWithValidTestData<GlbGroup>();
			var jobGroup = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroupForJob = system.ReleaseGroups.AddNew();
			releaseGroupForJob.FSG_GG_Group = jobGroup.PK;
			var releaseGroupForTemplate = system.ReleaseGroups.AddNew();
			releaseGroupForTemplate.FSG_GG_Group = templateGroupJLW.PK;

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG", isPartial: false);
			var templateJobHeader = template.GetJobHeader();
			templateJobHeader.FH_GG_ReleaseGroup = templateGroupJLW.PK;
			var templateWorkflow = template.ProcessHeaders.AddNew();

			templateWorkflow.FH_GG_ReleaseGroup = templateGroupChild.PK;

			templateWorkflow.FH_CompletionStatement = "temp";
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_FH_ProcessHeader = templateWorkflow.PK;

			Factory.Save();

			AssertEquals(template.PK, templateWorkflow.FH_P0_Template);

			var job = Factory.New<OrgHeader>();
			job.OH_Code = "SUM";
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, false);
			AssertEquals(0, jobHeader.ProcessHeaders.Count);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_GG_ReleaseGroup = jobGroup.PK;

			Factory.Save();

			AssertEquals(2, jobHeader.ProcessHeaders.Count);

			AssertEquals("Applied to JLW 1", templateGroupJLW.PK, jobHeader.FH_GG_ReleaseGroup);
			AssertEquals("Not applied to previously existing child with previously specified release group", jobGroup.PK, workflow.FH_GG_ReleaseGroup);
			AssertEquals("Applied to new workflow -- release group specified on workflow", templateGroupChild.PK, jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "temp").FH_GG_ReleaseGroup);
		}

		public void TestApplyTemplate_ShouldNotApplyReleaseGroupToChildWorkflowsIfNotSpecifiedOnTemplateJobWorkflow()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var templateGroupJLW = Factory.NewWithValidTestData<GlbGroup>();
			var templateGroupChild = Factory.NewWithValidTestData<GlbGroup>();
			var jobGroup = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroupForJob = system.ReleaseGroups.AddNew();
			releaseGroupForJob.FSG_GG_Group = jobGroup.PK;
			var releaseGroupForTemplate = system.ReleaseGroups.AddNew();
			releaseGroupForTemplate.FSG_GG_Group = templateGroupJLW.PK;

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG", isPartial: false);
			var templateJobHeader = template.GetJobHeader();
			templateJobHeader.FH_GG_ReleaseGroup = templateGroupJLW.PK;
			var templateWorkflow = template.ProcessHeaders.AddNew();
			templateWorkflow.FH_CompletionStatement = "temp2";
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_FH_ProcessHeader = templateWorkflow.PK;

			Factory.Save();

			var job = Factory.New<OrgHeader>();
			job.OH_Code = "PIG";
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, false);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_GG_ReleaseGroup = jobGroup.PK;

			Factory.Save();

			AssertEquals(2, jobHeader.ProcessHeaders.Count);

			AssertEquals("Applied to JLW", templateGroupJLW.PK, jobHeader.FH_GG_ReleaseGroup);
			AssertEquals("Not applied to previously existing child with previously specified release group", jobGroup.PK, workflow.FH_GG_ReleaseGroup);
			AssertEquals("Workflows should inherit the release group of their job-level workflow unless otherwise specified on their template equivalent", templateGroupJLW.PK, jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "temp2").FH_GG_ReleaseGroup);
		}

		public void TestApplyTemplate_WhenReleaseGroupRegistryItemEnabled_ShouldTryToApplyReleaseGroupFromOtherWorkflows()
		{
			AssertEquals("The registry item should be enabled by default, so that customers don't see any change without changing it. SAD!", true, BMSRegistry.Instance.ReleaseGroupsCanBeAssignedFromOtherWorkflowsWhenApplyingTemplates.Value);

			BMSTestHelper.CreateSystem(Factory, "INQ");
			var group = BMSTestHelper.CreateGroup(Factory);

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "INQ");
			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template, "Workflow 1", group.PK);
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template, "Workflow 2", ZGuid.Empty);
			AssertEquals("Precondition: Workflow 2's group must be empty in order for this test to have meaning. SAD!", ZGuid.Empty, templateWorkflow2.FH_GG_ReleaseGroup);

			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_FH_ProcessHeader = templateWorkflow1.PK;
			templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_FH_ProcessHeader = templateWorkflow2.PK;

			Factory.Save();

			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			Factory.Save();

			var jobHeader = BMSTestHelper.GetJobHeaderForParent(job, Factory);
			var workflows = jobHeader.ProcessHeaders.Cast<ProcessHeader>().ToArray();
			var workflow1 = workflows.Single(x => x.FH_CompletionStatement == "Workflow 1");
			var workflow2 = workflows.Single(x => x.FH_CompletionStatement == "Workflow 2");

			AssertEquals("Workflow 1's release group was specified, so it should have been copied to the job workflow. SAD!", group.PK, workflow1.FH_GG_ReleaseGroup);
			AssertEquals("Workflow 2's release group wasn't specified, but Workflow 1's group should have been copied to Workflow 2 because the registry setting allows it. SAD!", group.PK, workflow2.FH_GG_ReleaseGroup);
		}

		public void TestApplyTemplate_WhenReleaseGroupRegistryItemDisabled_ShouldNotTryToApplyReleaseGroupFromOtherWorkflows()
		{
			BMSRegistry.Instance.ReleaseGroupsCanBeAssignedFromOtherWorkflowsWhenApplyingTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			BMSTestHelper.CreateSystem(Factory, "INQ");
			var group = BMSTestHelper.CreateGroup(Factory);

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "INQ");
			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template, "Workflow 1", group.PK);
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template, "Workflow 2", ZGuid.Empty);
			AssertEquals("Precondition: Workflow 2's group must be empty in order for this test to have meaning. SAD!", ZGuid.Empty, templateWorkflow2.FH_GG_ReleaseGroup);

			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_FH_ProcessHeader = templateWorkflow1.PK;
			templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_FH_ProcessHeader = templateWorkflow2.PK;

			Factory.Save();

			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			Factory.Save();

			var jobHeader = BMSTestHelper.GetJobHeaderForParent(job, Factory);
			var workflows = jobHeader.ProcessHeaders.Cast<ProcessHeader>().ToArray();
			var workflow1 = workflows.Single(x => x.FH_CompletionStatement == "Workflow 1");
			var workflow2 = workflows.Single(x => x.FH_CompletionStatement == "Workflow 2");

			AssertEquals("Workflow 1's release group was specified, so it should have been copied to the job workflow. SAD!", group.PK, workflow1.FH_GG_ReleaseGroup);
			AssertEquals("Workflow 2's release group wasn't specified, and the registry setting is disabled, so it should not have copied Workflow 1's release group. SAD!", ZGuid.Empty, workflow2.FH_GG_ReleaseGroup);
		}

		public void TestApplyTemplate_LinksBetweenTemplates_ShouldNotBeCopied_WhenOnlyOneTemplateIsApplied()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "WKI");
			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", "ENT");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", "GLW");

			var workflowFrom = BMSTestHelper.CreateWorkflow(template1, description: "Workflow 1");
			var workflowTo = BMSTestHelper.CreateWorkflow(template2, description: "Workflow 2");
			var someOtherWorkflow1 = BMSTestHelper.CreateWorkflow(template1, description: "Some other workflow 1");
			var someOtherWorkflow2 = BMSTestHelper.CreateWorkflow(template2, description: "Some other workflow 2");

			BMSTestHelper.CreateTask(template1, workflowFrom, string.Empty, description: "Cody Love");
			BMSTestHelper.CreateTask(template2, workflowTo, string.Empty, description: "Alex Beagles");
			BMSTestHelper.CreateTask(template1, someOtherWorkflow1, string.Empty, description: "Do this");
			BMSTestHelper.CreateTask(template2, someOtherWorkflow2, string.Empty, description: "Do that");

			var link = (ProcessHeaderLink)template1.ProcessHeaderLinks.AddNew();

			link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;
			link.FP_FH_HeaderFrom = workflowFrom.PK;
			link.FP_FH_HeaderTo = workflowTo.PK;

			Factory.Save();

			var job = Factory.New<IWorkItem>();
			job.WKI_Summary = "Do the thing.";
			job.WKI_WorkItemType = "ENT";

			var workflowProvider = (IWorkflowProvider)job;

			Factory.Save();

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(workflowProvider, Factory);

			AssertContainsExactElementsInAnyOrder("Just the first template should be applied", new[] { "Workflow 1", "Some other workflow 1" }, jobHeader.ProcessHeaders.Select(w => w.FH_CompletionStatement));
			AssertEquals("No link between worklows should be created as the second workflow is absent", 0, jobHeader.ProcessHeaders[0].Links.Count());
		}

		public void TestApplyTemplate_LinksBetweenWorkflowsWithTheSameName_ShouldNotBeCreated()
		{
			TestConfigsHelper.CreateSchematicTestConfig(Factory, "WKI");
			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", "ENT");
			template2.P0_TaskFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

			const string theSameName = "He Who Must Not Be Named";
			var workflowFrom = BMSTestHelper.CreateWorkflow(template1, description: theSameName);
			var workflowTo = BMSTestHelper.CreateWorkflow(template2, description: theSameName);
			var someOtherWorkflow1 = BMSTestHelper.CreateWorkflow(template1, description: "Some other workflow 1");
			var someOtherWorkflow2 = BMSTestHelper.CreateWorkflow(template2, description: "Some other workflow 2");

			BMSTestHelper.CreateTask(template1, workflowFrom, string.Empty, description: "Do this");
			BMSTestHelper.CreateTask(template2, workflowTo, string.Empty, description: "Do that");
			BMSTestHelper.CreateTask(template1, someOtherWorkflow1, string.Empty, description: "Do this");
			BMSTestHelper.CreateTask(template2, someOtherWorkflow2, string.Empty, description: "Do that");

			var link = (ProcessHeaderLink)template1.ProcessHeaderLinks.AddNew();

			link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;
			link.FP_FH_HeaderFrom = workflowFrom.PK;
			link.FP_FH_HeaderTo = workflowTo.PK;

			Factory.Save();

			var job = Factory.New<IWorkItem>();
			job.WKI_Summary = "Do the thing.";
			job.WKI_WorkItemType = "ENT";

			var workflowProvider = (IWorkflowProvider)job;

			Factory.Save();

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(workflowProvider, Factory);

			AssertContainsExactElementsInAnyOrder("Both templates should be applied", new[] { "He Who Must Not Be Named", "Some other workflow 1", "Some other workflow 2" }, jobHeader.ProcessHeaders.Select(w => w.FH_CompletionStatement));
			AssertEquals("No link between worklows with the same name should be created", 0, jobHeader.ProcessHeaders[0].Links.Count());
		}

		public void TestShouldApplyLinksBetweenWorklowsWithinTheSameTemplate()
		{
			BMSTestHelper.CreateSystem(Factory, "WKI");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", name: "Template");

			var templateWorkflowFrom = BMSTestHelper.CreateWorkflow(template, "Workflow From");
			var templateTaskFrom = BMSTestHelper.CreateTask(template, templateWorkflowFrom);

			var templateWorkflowTo = BMSTestHelper.CreateWorkflow(template, "Workflow To");
			var templateTaskTo = BMSTestHelper.CreateTask(template, templateWorkflowTo);

			var templateLink = BMSTestHelper.CreateDependencyLink(template, templateWorkflowFrom, templateWorkflowTo);

			Factory.Save();

			var job = Factory.New<IWorkItem>();
			job.WKI_Summary = "Work Item";

			Factory.Save();

			var jobHeader = ProcessJobHeader.GetForParent((IWorkflowProvider)job, Factory);
			var headerFrom = jobHeader.ProcessHeaders.SingleOrDefault(w => w.FH_CompletionStatement == "Workflow From");
			var headerTo = jobHeader.ProcessHeaders.SingleOrDefault(w => w.FH_CompletionStatement == "Workflow To");
			AssertNotNull(headerFrom);
			AssertNotNull(headerTo);

			var link = headerFrom.Links.SingleOrDefault();
			AssertNotNull(link);
			AssertEquals(headerFrom, link.HeaderFrom);
			AssertEquals(headerTo, link.HeaderTo);
		}

		public void TestTemplateWorkflowAppliesTasksToJobWithSameSelectionCriteria()
		{
			BMSTestHelper.CreateSystem(Factory, "WKI");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", subType1: "AAA", name: "Template 2");

			var templateWorkflow = BMSTestHelper.CreateWorkflow(template, "TemplateWorkflow");

			var templateTask = BMSTestHelper.CreateTask(template, templateWorkflow, description: "TemplateTask");

			var templateConditionalTaskValid = BMSTestHelper.CreateTask(template, templateWorkflow, description: "TemplateConditionalTaskValid");
			templateConditionalTaskValid.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateConditionalTaskValid.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";

			var templateConditionalTaskInvalid = BMSTestHelper.CreateTask(template, templateWorkflow, description: "TemplateConditionalTaskInvalid");
			templateConditionalTaskInvalid.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateConditionalTaskInvalid.TemplateConditions.TemplateCondition2Value = "\"1\"!=\"1\"";

			Factory.Save();

			var job = Factory.New<IWorkItem>();
			job.WKI_Summary = "Work Item";
			job.WKI_WorkItemType = "AAA"; // selection criteria

			Factory.Save();

			var jobHeader = ProcessJobHeader.GetForParent((IWorkflowProvider)job, Factory);

			var normalTask = jobHeader.Tasks.FirstOrDefault();
			AssertNotNull(normalTask);
			AssertEquals("Condition: Normal template tasks applied to Work Item with same selection criteria", "TemplateTask", normalTask.P9_Description);

			var validConditionalTask = jobHeader.Tasks.FirstOrDefault(t => t.P9_Description == "TemplateConditionalTaskValid");
			AssertNotNull(validConditionalTask);
			AssertEquals("Condition: Conditional template tasks applied to Work Item with same selection criteria", "TemplateConditionalTaskValid", validConditionalTask.P9_Description);

			var invalidConditionalTask = jobHeader.Tasks.FirstOrDefault(t => t.P9_Description == "TemplateConditionalTaskInvalid");
			AssertNull(invalidConditionalTask);
		}

		public void TestShouldApplyLinksBetweenWorkflowInDifferentTemplates()
		{
			BMSTestHelper.CreateSystem(Factory, "WKI");
			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", name: "Template 1");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI", subType1: "AAA", name: "Template 2");
			template2.P0_TaskFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

			var templateWorkflowFrom = BMSTestHelper.CreateWorkflow(template1, "Workflow From");
			var templateTaskFrom = BMSTestHelper.CreateTask(template1, templateWorkflowFrom);

			var templateWorkflowTo = BMSTestHelper.CreateWorkflow(template2, "Workflow To");
			var templateTaskTo = BMSTestHelper.CreateTask(template2, templateWorkflowTo);

			var templateLink = BMSTestHelper.CreateDependencyLink(template1, templateWorkflowFrom, templateWorkflowTo);

			Factory.Save();

			var job = Factory.New<IWorkItem>();
			job.WKI_Summary = "Work Item";
			job.WKI_WorkItemType = "AAA"; // to apply both templates

			Factory.Save();

			var jobHeader = ProcessJobHeader.GetForParent((IWorkflowProvider)job, Factory);
			var headerFrom = jobHeader.ProcessHeaders.SingleOrDefault(w => w.FH_CompletionStatement == "Workflow From");
			var headerTo = jobHeader.ProcessHeaders.SingleOrDefault(w => w.FH_CompletionStatement == "Workflow To");
			AssertNotNull(headerFrom);
			AssertNotNull(headerTo);

			var link = headerFrom.Links.SingleOrDefault();
			AssertNotNull(link);
			AssertEquals(headerFrom, link.HeaderFrom);
			AssertEquals(headerTo, link.HeaderTo);
		}

		public void TestApplyTemplate_ShouldCopyTimeDelayFields()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");

			var templateWorkflow1 = (ProcessHeader)template.ProcessHeaders.AddNew();
			templateWorkflow1.FH_CompletionStatement = "workflow1";
			var templateWorkflow2 = (ProcessHeader)template.ProcessHeaders.AddNew();
			templateWorkflow2.FH_CompletionStatement = "workflow2";

			var templateLink = BMSTestHelper.CreateDependencyLink(template, templateWorkflow1, templateWorkflow2);
			templateLink.FP_TimeDelayFactor = 2.1m;
			templateLink.FP_TimeDelayMinutes = 100;

			var job = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, false);
			jobHeader.ApplyTemplate(template);

			AssertEquals(2, jobHeader.ProcessHeaders.Count);
			var link = jobHeader.ProcessHeaders[0].Links.First();
			AssertEquals(2.1m, link.FP_TimeDelayFactor);
			AssertEquals(100, link.FP_TimeDelayMinutes);
		}

		public void TestApplyTemplate_ShouldCopyRelevantFields()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");

			var templateWorkflow = template.ProcessHeaders.AddNew();
			templateWorkflow.FH_CompletionStatement = "Workflow 1";
			templateWorkflow.FH_AllowTaskAutoAssignment = true;
			templateWorkflow.FH_IsCriticalHandover = true;
			templateWorkflow.FH_IsStandby = true;
			templateWorkflow.FH_DateAcceptability = DateAcceptabilityList.Codes.ExtendedStartGraduatedFinish;
			templateWorkflow.FH_DeadlineType = DeadlineTypeList.Codes.Soft;
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_FH_ProcessHeader = templateWorkflow.PK;

			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory);
			jobHeader.ApplyTemplate(template);

			var workflow = jobHeader.ProcessHeaders.First(w => w.FH_CompletionStatement == "Workflow 1");
			Assert(workflow.FH_AllowTaskAutoAssignment);
			Assert(workflow.FH_IsCriticalHandover);
			Assert(workflow.FH_IsStandby);
			AssertEquals(DateAcceptabilityList.Codes.ExtendedStartGraduatedFinish, workflow.FH_DateAcceptability);
			AssertEquals(DeadlineTypeList.Codes.Soft, workflow.FH_DeadlineType);
			AssertEquals("UDF", workflow.FH_Category);
			AssertEquals(templateWorkflow.CategoryDescription, workflow.CategoryDescription);
		}

		public void TestApplyTemplate_ShouldCopyCategory()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry(WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode, "TS1", "Test category");
			BMSTestHelper.AddWorkflowCategoryToRegistry(WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode, "TS2", "Test2 category");

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");

			var templateWorkflow = template.ProcessHeaders.AddNew();
			templateWorkflow.FH_CompletionStatement = "Workflow 1";
			templateWorkflow.FH_Category = "TS2";
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_FH_ProcessHeader = templateWorkflow.PK;

			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory);
			jobHeader.ApplyTemplate(template);

			var workflow = jobHeader.ProcessHeaders.First(w => w.FH_CompletionStatement == "Workflow 1");
			AssertEquals("TS2", workflow.FH_Category);
			AssertEquals("Test2 category", workflow.CategoryDescription);
		}

		public void TestApplyTemplate_ShouldCopyTags()
		{
			var tagDef = Factory.NewWithValidTestData<TagDefinition>();
			tagDef.TGD_Code = "PRI";
			tagDef.TGD_Description = "Priority Tags";
			var tagMag1 = tagDef.Magnitudes.AddNew();
			tagMag1.TGM_Code = "PLT";
			tagMag1.TGM_Description = "Platinum";
			var tagMag2 = tagDef.Magnitudes.AddNew();
			tagMag2.TGM_Code = "GLD";
			tagMag2.TGM_Description = "Gold";

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");

			var templateWorkflow1 = (ProcessHeader)template.ProcessHeaders.AddNew();
			templateWorkflow1.FH_CompletionStatement = "Workflow 1";
			var templateWorkflow2 = (ProcessHeader)template.ProcessHeaders.AddNew();
			templateWorkflow2.FH_CompletionStatement = "Workflow 2";

			var templateLink = (ProcessHeaderLink)template.ProcessHeaderLinks.AddNew();
			templateLink.FP_FH_HeaderFrom = templateWorkflow1.PK;
			templateLink.FP_FH_HeaderTo = templateWorkflow2.PK;

			templateWorkflow1.AddTag(tagMag1);
			templateWorkflow2.AddTag(tagMag2);

			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory);
			jobHeader.ApplyTemplate(template);

			var workflow1 = jobHeader.ProcessHeaders.First(w => w.FH_CompletionStatement == "Workflow 1");
			AssertCollectionContains(workflow1.TagLinks, l => l.TagMagnitude.TGM_Code == tagMag1.TGM_Code);
			var workflow2 = jobHeader.ProcessHeaders.First(w => w.FH_CompletionStatement == "Workflow 2");
			AssertCollectionContains(workflow2.TagLinks, l => l.TagMagnitude.TGM_Code == tagMag2.TGM_Code);
		}

		public void TestApplyTemplate_ShouldCopyMilestoneCompletionPivots()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "DUM");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM");
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template, "Workflow");
			var templateJobLevelWorkflow = templateWorkflow.JobHeader;
			var templateTask = BMSTestHelper.CreateTaskForTemplate(template, templateWorkflow);

			var templateMilestone1 = template.WorkflowItems.Milestones.AddNew();
			var templateMilestone2 = template.WorkflowItems.Milestones.AddNew();
			var templateMilestone3 = template.WorkflowItems.Milestones.AddNew();
			templateMilestone1.P9_Sequence = 1;
			templateMilestone2.P9_Sequence = 2;
			templateMilestone3.P9_Sequence = 3;
			templateMilestone1.P9_Description = "Milestone 1";
			templateMilestone2.P9_Description = "Milestone 2";
			templateMilestone3.P9_Description = "Milestone 3";
			templateMilestone1.TriggerConditions.TriggerEventCode = "Z00";
			templateMilestone2.TriggerConditions.TriggerEventCode = "Z00";
			templateMilestone3.TriggerConditions.TriggerEventCode = "Z00";

			MilestoneCompletionHelper.SetCompletionMilestone(templateJobLevelWorkflow, templateMilestone1);
			MilestoneCompletionHelper.SetCompletionMilestone(templateWorkflow, templateMilestone2);
			MilestoneCompletionHelper.SetCompletionMilestone(templateTask, templateMilestone3);

			Factory.Save();

			var job = Factory.New<DummyWithWorkflow>();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(job, TemplateApplicationParameters.ApplyIgnoreHasChanges());

			var milestones = job.WorkflowItems.Milestones.Cast<ProcessTask>().ToArray();
			AssertEquals("Milestones should have been loaded from the template", 3, milestones.Length);

			var jobLevelWorkflow = (ProcessJobHeader)BMSTestHelper.GetJobHeaderForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = jobLevelWorkflow.ProcessHeaders.Cast<ProcessHeader>().Single();

			var expectedCompletionMilestoneForJobLevelWorkflow = milestones.Single(x => x.P9_Sequence == 1);
			AssertEquals(expectedCompletionMilestoneForJobLevelWorkflow.P9_MilestoneCompletionPivotKey, jobLevelWorkflow.FH_MilestoneCompletionPivotKey);

			var expectedCompletionMilestoneForWorkflow = milestones.Single(x => x.P9_Sequence == 2);
			AssertEquals(expectedCompletionMilestoneForWorkflow.P9_MilestoneCompletionPivotKey, workflow.FH_MilestoneCompletionPivotKey);

			var expectedCompletionMilestoneForTask = milestones.Single(x => x.P9_Sequence == 3);
			var task = workflow.Tasks.Single();
			AssertEquals(expectedCompletionMilestoneForTask.P9_MilestoneCompletionPivotKey, task.P9_MilestoneCompletionPivotKey);
		}

		public void TestApplyTemplate_WhenWorkflowAndTaskCompletionMilestonesSpecified_DbHits()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "DUM");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM");
			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template, "Workflow 1");
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template, "Workflow 2");
			var templateWorkflow3 = BMSTestHelper.CreateWorkflow(template, "Workflow 3");
			var templateTask1 = BMSTestHelper.CreateTaskForTemplate(template, templateWorkflow1);
			var templateTask2 = BMSTestHelper.CreateTaskForTemplate(template, templateWorkflow2);
			var templateTask3 = BMSTestHelper.CreateTaskForTemplate(template, templateWorkflow3);

			var templateMilestone1 = template.WorkflowItems.Milestones.AddNew();
			var templateMilestone2 = template.WorkflowItems.Milestones.AddNew();
			var templateMilestone3 = template.WorkflowItems.Milestones.AddNew();
			templateMilestone1.P9_Sequence = 1;
			templateMilestone2.P9_Sequence = 2;
			templateMilestone3.P9_Sequence = 3;
			templateMilestone1.P9_Description = "Milestone 1";
			templateMilestone2.P9_Description = "Milestone 2";
			templateMilestone3.P9_Description = "Milestone 3";

			MilestoneCompletionHelper.SetCompletionMilestone(templateWorkflow1, templateMilestone1);
			MilestoneCompletionHelper.SetCompletionMilestone(templateWorkflow2, templateMilestone2);
			MilestoneCompletionHelper.SetCompletionMilestone(templateWorkflow3, templateMilestone3);

			MilestoneCompletionHelper.SetCompletionMilestone(templateTask1, templateMilestone1);
			MilestoneCompletionHelper.SetCompletionMilestone(templateTask2, templateMilestone2);
			MilestoneCompletionHelper.SetCompletionMilestone(templateTask3, templateMilestone3);

			Factory.Save();

			var job = Factory.New<DummyWithWorkflow>();

			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(job, TemplateApplicationParameters.ApplyIgnoreHasChanges());

			var milestones = job.WorkflowItems.Milestones.Cast<ProcessTask>().ToArray();
			AssertEquals("Milestones should have been loaded from the template", 3, milestones.Length);

			var expectedCompletionMilestoneForWorkflow = milestones.Single(x => x.P9_Sequence == 2);
			var jobLevelWorkflow = BMSTestHelper.GetJobHeaderForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			foreach (var workflow in jobLevelWorkflow.ProcessHeaders.Cast<ProcessHeader>())
			{
				AssertNotEquals(ZString.Empty, workflow.FH_MilestoneCompletionPivotKey);

				var task = workflow.Tasks.Single();
				AssertNotEquals(ZString.Empty, task.P9_MilestoneCompletionPivotKey);
			}
		}

		public void TestApplyTemplate_ShouldAssignWorkflowTemplateAndSourceTemplateID()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateJobHeader = template.GetJobHeader();
			var templateWorkflow = template.ProcessHeaders.AddNew();
			templateWorkflow.FH_CompletionStatement = "Workflow 1";
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			templateTask.P9_FH_ProcessHeader = templateWorkflow.PK;

			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();

			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(job);

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);

			AssertNotNull(jobHeader);

			var appliedWorkflow = jobHeader.ProcessHeaders.First(w => w.FH_CompletionStatement == "Workflow 1");
			AssertEquals("ParentTemplateId should be applied", templateJobHeader.PK, jobHeader.FH_ParentTemplateId);
			AssertEquals("ParentTemplateId should be applied", templateWorkflow.PK, appliedWorkflow.FH_ParentTemplateId);
			AssertEquals("Should able to get SourceTemplatePK", template.PK, appliedWorkflow.SourceTemplatePK);
		}

		public void TestApplyTemplate_ShouldCopyJobLevelTags()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateJobHeader = template.GetJobHeader();
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template, "Sparkle");
			BMSTestHelper.CreateTask(template, templateWorkflow, GlbStaff.CurrentUser.GS_Code);

			templateJobHeader.AddTag(config.DerpyHoovesTag);
			templateWorkflow.AddTag(config.PrincessCelestiaTag);

			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();

			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(job);
			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);

			AssertNotNull(jobHeader);
			AssertEquals(1, jobHeader.ProcessHeaders.Count);

			var workflow = jobHeader.ProcessHeaders[0];
			AssertEquals("Sparkle", workflow.FH_CompletionStatement);

			BMSTestCaseWithFactory.AssertTagApplied(jobHeader, config.DerpyHoovesTag);
			BMSTestCaseWithFactory.AssertTagApplied(workflow, config.PrincessCelestiaTag);
		}

		public void TestApplyTemplateMultipleTimes_WhenTagsAppliedToJob()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateJobHeader = template.GetJobHeader();
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);
			var templateTask = BMSTestHelper.CreateTask(template, templateWorkflow, GlbStaff.CurrentUser.GS_Code);

			templateJobHeader.AddTag(config.DerpyHoovesTag);
			templateWorkflow.AddTag(config.PrincessCelestiaTag);

			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);

			AssertNotNull(jobHeader);
			BMSTestCaseWithFactory.AssertTagApplied(jobHeader, config.DerpyHoovesTag);
			BMSTestCaseWithFactory.AssertTagApplied(jobHeader.ProcessHeaders.Single(), config.PrincessCelestiaTag);

			jobHeader.ProcessHeaders[0].Delete();
			Factory.Save();

			AssertEquals("Should have created a new workflow when re-applying template", 1, jobHeader.ProcessHeaders.Count);
			BMSTestCaseWithFactory.AssertTagApplied("Job-level tags should only apply once.", jobHeader, config.DerpyHoovesTag);
			BMSTestCaseWithFactory.AssertTagApplied(jobHeader.ProcessHeaders.Single(), config.PrincessCelestiaTag);
		}

		public void TestApplyTemplateMultipleTimes_DueToWorkflowInheritance_WhenTagsAppliedToJob_NonExclusiveTags()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory, "WKI");
			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");

			template1.P0_SubType1 = "ENT";
			template1.P0_SubType3 = "BUF";
			template2.P0_SubType1 = "ENT";

			template1.P0_TaskFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

			var templateJobHeader1 = template1.GetJobHeader();
			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template1, "SIIICK");
			var templateTask1 = BMSTestHelper.CreateTask(template1, templateWorkflow1, GlbStaff.CurrentUser.GS_Code);

			var templateJobHeader2 = template2.GetJobHeader();
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template2, "Adam Durkee is sick today.");
			var templateTask2 = BMSTestHelper.CreateTask(template2, templateWorkflow2, GlbStaff.CurrentUser.GS_Code);

			templateJobHeader1.AddTag(config.DerpyHoovesTag);
			templateWorkflow1.AddTag(config.PrincessCelestiaTag);

			templateJobHeader2.AddTag(config.DerpyHoovesTag);
			templateWorkflow2.AddTag(config.PrincessCelestiaTag);

			Factory.Save();

			var job = Factory.New<IWorkItem>();
			job.WKI_Summary = "Do the thing.";
			job.WKI_WorkItemType = "ENT";
			job.WKI_ActivityType = "BUF";

			Factory.Save();

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation((IWorkflowProvider)job, Factory);

			AssertNotNull(jobHeader);
			BMSTestCaseWithFactory.AssertTagApplied(jobHeader, config.DerpyHoovesTag);
			BMSTestCaseWithFactory.AssertTagApplied(jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "SIIICK"), config.PrincessCelestiaTag);
			BMSTestCaseWithFactory.AssertTagApplied(jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Adam Durkee is sick today."), config.PrincessCelestiaTag);

			jobHeader.ProcessHeaders.DeleteAll();
			Factory.Save();

			AssertEquals("Should have created new workflows when re-applying template", 2, jobHeader.ProcessHeaders.Count);
			BMSTestCaseWithFactory.AssertTagApplied("Job-level tags should only apply once.", jobHeader, config.DerpyHoovesTag);
			BMSTestCaseWithFactory.AssertTagApplied(jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "SIIICK"), config.PrincessCelestiaTag);
			BMSTestCaseWithFactory.AssertTagApplied(jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Adam Durkee is sick today."), config.PrincessCelestiaTag);
		}

		public void TestApplyTemplateMultipleTimes_DueToWorkflowInheritance_WhenTagsAppliedToJob_ExclusiveTags()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory, "WKI");
			var template1 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");
			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "WKI");

			template1.P0_SubType1 = "ENT";
			template1.P0_SubType3 = "BUF";
			template2.P0_SubType1 = "ENT";

			template1.P0_TaskFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

			var templateJobHeader1 = template1.GetJobHeader();
			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template1, "SIIICK");
			var templateTask1 = BMSTestHelper.CreateTask(template1, templateWorkflow1, GlbStaff.CurrentUser.GS_Code);

			var templateJobHeader2 = template2.GetJobHeader();
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template2, "Adam Durkee is sick today.");
			var templateTask2 = BMSTestHelper.CreateTask(template2, templateWorkflow2, GlbStaff.CurrentUser.GS_Code);

			templateJobHeader1.AddTag(config.PlatinumTag);
			templateWorkflow1.AddTag(config.RedTag);

			templateJobHeader2.AddTag(config.RedTag);
			templateWorkflow2.AddTag(config.PlatinumTag);

			Factory.Save();

			var job = Factory.New<IWorkItem>();
			job.WKI_Summary = "Do the thing.";
			job.WKI_WorkItemType = "ENT";
			job.WKI_ActivityType = "BUF";

			Factory.Save();

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation((IWorkflowProvider)job, Factory);

			AssertNotNull(jobHeader);
			BMSTestCaseWithFactory.AssertTagApplied("Should use exclusive tag of most specific workflow template.", jobHeader, config.PlatinumTag);
			BMSTestCaseWithFactory.AssertTagNotApplied(jobHeader, config.RedTag);
			BMSTestCaseWithFactory.AssertTagApplied(jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "SIIICK"), config.RedTag);
			BMSTestCaseWithFactory.AssertTagApplied(jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Adam Durkee is sick today."), config.PlatinumTag);

			jobHeader.ProcessHeaders.DeleteAll();
			Factory.Save();

			AssertEquals("Should have created new workflows when re-applying template", 2, jobHeader.ProcessHeaders.Count);
			BMSTestCaseWithFactory.AssertTagApplied("Job-level tags should only apply once. Should use exclusive tag of most specific workflow template.", jobHeader, config.PlatinumTag);
			BMSTestCaseWithFactory.AssertTagNotApplied(jobHeader, config.RedTag);
			BMSTestCaseWithFactory.AssertTagApplied(jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "SIIICK"), config.RedTag);
			BMSTestCaseWithFactory.AssertTagApplied(jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Adam Durkee is sick today."), config.PlatinumTag);
		}

		public void TestApplyTemplate_ThenCreateDependencyLinks_ShouldNotReportValidationErrors()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template, "Zoot! Review.", releaseGroupPK: config.ReleaseGroup.PK);
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template, "As a guilty mum...", releaseGroupPK: config.ReleaseGroup.PK);
			var templateWorkflow3 = BMSTestHelper.CreateWorkflow(template, "Blah blah dettol blah.", releaseGroupPK: config.ReleaseGroup.PK);

			BMSTestHelper.CreateDependencyLink(template, templateWorkflow1, templateWorkflow2);
			BMSTestHelper.CreateTask(template, templateWorkflow1, string.Empty);
			BMSTestHelper.CreateTask(template, templateWorkflow2, string.Empty);
			BMSTestHelper.CreateTask(template, templateWorkflow3, string.Empty);

			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();

			job.ApplyWorkflowTemplates();

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);
			AssertNotNull(jobHeader);
			AssertEquals(3, jobHeader.ProcessHeaders.Count);

			var workflow2 = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "As a guilty mum...");
			var workflow3 = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Blah blah dettol blah.");

			workflow2.MakePrerequisiteOf(workflow3);

			AssertNoErrors(jobHeader);
		}

		public void TestApplyTemplates_ForJobWithPropertyWhoseValueChangesAtWill_AndTaskWithTemplateConditionBasedOnThatProperty_ShouldApplyTaskWithTemplateWorkflow()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, DummyWorkflowDescriptor.Instance.Code);

			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(BadDummyWithPropertyWithSideEffects);

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);

			var workflow1 = BMSTestHelper.CreateWorkflow(template, "I too have a Nuclear Button");
			var workflow2 = BMSTestHelper.CreateWorkflow(template, "A very stable genius");

			var task1 = BMSTestHelper.CreateTask(template, workflow1, description: "It is a much bigger & more powerful one than his");
			var task2 = BMSTestHelper.CreateTask(template, workflow2, description: "I have GREAT confidence in MY intelligence people");

			task2.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			task2.TemplateConditions.TemplateCondition2Value = "\"<BadProperty>\"==\"I don't see why it wouldn't be Russia\"";

			Factory.Save();

			var job = Factory.New<BadDummyWithPropertyWithSideEffects>();

			job.ApplyWorkflowTemplates();

			AssertTasks("Applying templates for the first time will only match the first task since the second task doesn't match the template condition yet.", new[]
			{
				Tuple.Create("I too have a Nuclear Button", "It is a much bigger & more powerful one than his"),
			});

			job.ApplyWorkflowTemplates(TemplateApplicationParameters.ApplySpecificEntityTypes(TemplateEntityType.All, jobAttributesMayHaveChangedSinceLastTemplateApplication: true)); // Ensures macro cache is cleared.

			AssertTasks("Applying templates a second time will apply the second task because side effects have caused its template condition to match. We should get its workflow applied too.", new[]
			{
				Tuple.Create("I too have a Nuclear Button", "It is a much bigger & more powerful one than his"),
				Tuple.Create("A very stable genius", "I have GREAT confidence in MY intelligence people"),
			});

			void AssertTasks(string message, Tuple<string, string>[] expectedTaskAndWorkflowDescriptions)
			{
				AssertContainsExactElementsInAnyOrder(message, expectedTaskAndWorkflowDescriptions, job.WorkflowItems.Tasks.Cast<ProcessTask>().Select(t => Tuple.Create(t.ProcessHeader?.FH_CompletionStatement.ToString(), t.P9_Description.ToString())));
			}
		}

		public void TestApplyTemplate_MostSpecificSystemTemplateChosen_WhenNoUserTemplatesExist()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "DUM");
			var generalSysTemplate = CreateSystemWorkflowTemplate("DUM");
			var specificSysTemplate = CreateSystemWorkflowTemplate("DUM", subType1: "ZZZ", subType2: "UUU");

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.SubType1 = "ZZZ";
			job.Z0_Code = "UUU";
			job.ApplyWorkflowTemplates();

			var templates = ProcessTask.Loader.LoadTemplateMatches(job, Factory);
			AssertContainsExactElementsInAnyOrder(new[] { generalSysTemplate, specificSysTemplate }.Select(t => t.P0_Name), templates.Select(t => t.P0_Name));

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);
			var templateHeader = Factory.Load<ProcessHeader>(jobHeader.FH_ParentTemplateId);
			AssertEquals(specificSysTemplate.P0_Name, templateHeader.Template.P0_Name);
		}

		public void TestApplyTemplate_MostSpecificSystemTemplateChosen_WhenLessSpecificUserTemplateExists()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "DUM");
			var generalSysTemplate = CreateSystemWorkflowTemplate("DUM");
			var specificSysTemplate = CreateSystemWorkflowTemplate("DUM", subType1: "ZZZ", subType2: "UUU");
			var userTemplate = CreateUserWorkflowTemplate("DUM", subType2: "UUU");

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.SubType1 = "ZZZ";
			job.Z0_Code = "UUU";
			job.ApplyWorkflowTemplates();

			var templates = ProcessTask.Loader.LoadTemplateMatches(job, Factory);
			AssertContainsExactElementsInAnyOrder(new[] { generalSysTemplate, specificSysTemplate, userTemplate }.Select(t => t.P0_Name), templates.Select(t => t.P0_Name));

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);
			var templateHeader = Factory.Load<ProcessHeader>(jobHeader.FH_ParentTemplateId);
			AssertEquals(specificSysTemplate.P0_Name, templateHeader.Template.P0_Name);
		}

		public void TestApplyTemplate_MostSpecificSystemTemplateChosen_WhenLessSpecificUserTemplateExists_ButHasNoTasks()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "DUM");
			var generalSysTemplate = CreateSystemWorkflowTemplate("DUM");
			var specificSysTemplate = CreateSystemWorkflowTemplate("DUM", subType1: "ZZZ", subType2: "UUU");
			var userTemplate = CreateUserWorkflowTemplate("DUM", subType2: "UUU", createWorkflowAndTask: false);

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.SubType1 = "ZZZ";
			job.Z0_Code = "UUU";
			job.ApplyWorkflowTemplates();

			var templates = ProcessTask.Loader.LoadTemplateMatches(job, Factory);
			AssertContainsExactElementsInAnyOrder(new[] { generalSysTemplate, specificSysTemplate, userTemplate }.Select(t => t.P0_Name), templates.Select(t => t.P0_Name));

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);
			var templateHeader = Factory.Load<ProcessHeader>(jobHeader.FH_ParentTemplateId);
			AssertEquals(specificSysTemplate.P0_Name, templateHeader.Template.P0_Name);
		}

		public void TestApplyTemplate_UserTemplateChosen_WhenEquallySpecificSystemTemplateExists()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "DUM");

			var generalSysTemplate = CreateSystemWorkflowTemplate("DUM");
			var specificSysTemplate = CreateSystemWorkflowTemplate("DUM", subType1: "ZZZ", subType2: "UUU");
			var userTemplate = CreateUserWorkflowTemplate("DUM", subType1: "ZZZ", subType2: "UUU");

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.SubType1 = "ZZZ";
			job.Z0_Code = "UUU";
			job.ApplyWorkflowTemplates();

			var templates = ProcessTask.Loader.LoadTemplateMatches(job, Factory);
			AssertContainsExactElementsInAnyOrder(new[] { generalSysTemplate, specificSysTemplate, userTemplate }.Select(t => t.P0_Name), templates.Select(t => t.P0_Name));

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);
			var templateHeader = Factory.Load<ProcessHeader>(jobHeader.FH_ParentTemplateId);
			AssertEquals(userTemplate.P0_Name, templateHeader.Template.P0_Name);
		}

		public void TestApplyTemplate_MostSpecificUserTemplateChosen()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "DUM");

			var generalSysTemplate = CreateSystemWorkflowTemplate("DUM");
			var userTemplate1 = CreateUserWorkflowTemplate("DUM", subType1: "ZZZ");
			var userTemplate2 = CreateUserWorkflowTemplate("DUM", subType2: "UUU");
			var specificUserTemplate = CreateUserWorkflowTemplate("DUM", subType1: "ZZZ", subType2: "UUU");

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.SubType1 = "ZZZ";
			job.Z0_Code = "UUU";
			job.ApplyWorkflowTemplates();

			var templates = ProcessTask.Loader.LoadTemplateMatches(job, Factory);
			AssertContainsExactElementsInAnyOrder(new[] { generalSysTemplate, userTemplate1, userTemplate2, specificUserTemplate }.Select(t => t.P0_Name), templates.Select(t => t.P0_Name));

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);
			var templateHeader = Factory.Load<ProcessHeader>(jobHeader.FH_ParentTemplateId);
			AssertEquals(specificUserTemplate.P0_Name, templateHeader.Template.P0_Name);
		}

		public void TestApplyTemplate_ShouldCopyWorkflowLevelApprovedFlag()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateJobHeader = template.GetJobHeader();
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template, "ApprovedTest");
			BMSTestHelper.CreateTask(template, templateWorkflow);

			templateJobHeader.FH_IsApproved = true;

			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory);
			jobHeader.ApplyTemplate(template);

			var workflow = jobHeader.ProcessHeaders.First(w => w.FH_CompletionStatement == "ApprovedTest");
			AssertEquals("Approved flag is applied to workflows", true, workflow.FH_IsApproved);
		}

		public void TestApplyTemplate_ShouldCopyBranchAndDepartment()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateJobHeader = template.GetJobHeader();
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template, "BranchAndDepartmentTest");
			BMSTestHelper.CreateTask(template, templateWorkflow);

			var branchBNE = Factory.NewWithValidTestData<GlbBranch>();
			branchBNE.GB_BranchName = "Brisbane of my existence";
			branchBNE.GB_Code = "AUB";
			branchBNE.GB_RL_NKHomePort = "AUBNE";

			var deptJony = Factory.NewWithValidTestData<GlbDepartment>();
			deptJony.GE_Desc = "Jony";
			deptJony.GE_Code = "DEP";

			templateWorkflow.FH_GB_Branch = branchBNE.PK;
			templateWorkflow.FH_GE_Department = deptJony.PK;

			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory);
			jobHeader.ApplyTemplate(template);

			var workflow = jobHeader.ProcessHeaders.First(w => w.FH_CompletionStatement == "BranchAndDepartmentTest");
			AssertEquals("Branch is applied to workflows", branchBNE.PK, workflow.FH_GB_Branch);
			AssertEquals("Department is applied to workflows", deptJony.PK, workflow.FH_GE_Department);
		}

		public void TestApplyTemplate_ShouldCopyBufferTimespan()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateJobHeader = template.GetJobHeader();
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template, "BufferTimespanTest");
			BMSTestHelper.CreateTask(template, templateWorkflow);

			var timespan = Factory.New<BMBufferTimespan>();
			timespan.BMT_Name = "Test Span";
			timespan.BMT_BufferTimespanInMinutes = 20;

			templateWorkflow.FH_BMT_BufferTimespan = timespan.PK;

			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory);
			jobHeader.ApplyTemplate(template);

			var workflow = jobHeader.ProcessHeaders.First(w => w.FH_CompletionStatement == "BufferTimespanTest");
			AssertEquals("Buffer timespan is applied to workflows", timespan.PK, workflow.FH_BMT_BufferTimespan);
		}

		ProcessTaskTemplate CreateSystemWorkflowTemplate(string processType, string subType1 = null, string subType2 = null)
		{
			var name = GenerateTemplateName(processType, subType1, subType2, "global, system");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, processType, subType1, subType2, name: name);

			template.P0_IsSystem = true;
			template.GlobalTemplate = true;

			return template;
		}

		ProcessTaskTemplate CreateUserWorkflowTemplate(string processType, string subType1 = null, string subType2 = null, bool createWorkflowAndTask = true)
		{
			var name = GenerateTemplateName(processType, subType1, subType2, "user template");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, processType, subType1, subType2, name: name);

			if (createWorkflowAndTask)
			{
				var templateWorkflow = BMSTestHelper.CreateWorkflow(template);
				BMSTestHelper.CreateTask(template, templateWorkflow);
			}

			return template;
		}

		static string GenerateTemplateName(string processType, string subType1, string subType2, string description)
		{
			var nameBuilder = new ZStringBuilder(processType);

			nameBuilder.Append(", ");

			foreach (var subType in new[] { subType1, subType2 })
			{
				if (!string.IsNullOrEmpty(subType))
				{
					nameBuilder.Append(subType);
					nameBuilder.Append(", ");
				}
			}

			nameBuilder.Append(description);

			return nameBuilder.ToString();
		}

		#endregion

		#region Template Application via Conditions

		[TestDate(2015, 7, 14)]
		public void TestApplyTemplateConditionalTask_UserDefinedCondition_ProcesssJobHeaderCondition()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "ORG");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template, description: "Template Workflow 1");
			var templateTask = BMSTestHelper.CreateTask(template, templateWorkflow, string.Empty, description: "Template Task 1");

			templateTask.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTask.TemplateConditions.TemplateCondition2Value = "\"<FH_CompletionStatement>\" == \"test1\"";

			AssertEquals("GIVEN TriggerCondition2 = User Defined (UDF)", ProcessTasksLookups.UserDefinedCondition, templateTask.TemplateConditions.TemplateCondition2);

			Factory.Save();

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			jobHeader1.Parent.ApplyWorkflowTemplates();
			CombineAssertions("WHEN apply template, THEN none should be applied", () =>
			{
				AssertNotEquals("GIVEN ProcessJobHeader condition is not satisfied", templateWorkflow.FH_CompletionStatement, jobHeader1.FH_CompletionStatement);
				AssertNull("template task", jobHeader1.ProcessHeaders.FirstOrDefault(p => p.FH_CompletionStatement == "Template Workflow 1")?.Tasks.FirstOrDefault(t => t.P9_Description == "Template Task 1"));
			});

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "test1");
			jobHeader2.Parent.ApplyWorkflowTemplates();
			AssertEquals("GIVEN ProcessJobHeader condition is satisfied", "test1", jobHeader2.FH_CompletionStatement);
			CombineAssertions("WHEN apply template, THEN should be applied", () =>
			{
				AssertNotNull("template task", jobHeader2.ProcessHeaders.FirstOrDefault(p => p.FH_CompletionStatement == "Template Workflow 1")?.Tasks.FirstOrDefault(t => t.P9_Description == "Template Task 1"));
			});
		}

		[TestDate(2015, 7, 14)]
		public void TestApplyTemplateConditionalTask_WithProcessJobHeaderConditionAsFLDAction()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "ORG");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Job Header 1");
			AssertEquals("GIVEN jobHeader.FH_CompletionStatement = Job Header 1", "Job Header 1", jobHeader.FH_CompletionStatement);

			var orgHeader = jobHeader.Parent as OrgHeader;
			AssertEquals("GIVEN orgHeader.OH_FullName = empty", "", orgHeader.OH_FullName);

			var trigger = orgHeader.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = "FLD";
			action.PQ_FieldName = "<OH_FullName>";
			action.PQ_FieldValue = "<FH_CompletionStatement>";
			AssertEquals("GIVEN action = FLD", "FLD", action.PQ_TriggerType);
			AssertEquals("GIVEN Field Value", "<FH_CompletionStatement>", action.PQ_FieldValue);

			orgHeader.Logs.AddNew(Events.Authorised);

			Factory.Save();

			AssertNotEquals("Precondition Field Name should not be equal to FH_CompletionStatement", "Job Header 1", orgHeader.OH_FullName);

			BMSTestHelper.RunLogWalker();

			orgHeader.Reload();
			AssertEquals("WHEN trigger fire THEN Field Name should be set to Field Value", "Job Header 1", orgHeader.OH_FullName);
		}

		[TestDate(2015, 7, 14)]
		public void TestApplyTemplateConditionalTask_WhenTemplateWorkflowCloned_ShouldNotThrowException()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "DUM");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM");

			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);
			var templateTask1 = BMSTestHelper.CreateTask(template, templateWorkflow, string.Empty, description: "Ponko");
			var templateTask2 = BMSTestHelper.CreateTask(template, templateWorkflow, string.Empty, description: "With Conditions");

			templateTask2.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTask2.TemplateConditions.TemplateCondition2Value = "\"<Z0_Code>\"==\"ZUM\"";

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.ApplyWorkflowTemplates();

			AssertEquals(1, job.WorkflowItems.Tasks.Count);
			AssertEquals("Ponko", job.WorkflowItems.Tasks[0].P9_Description);

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders.Single();

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			var workflow2 = workflow1.CloneWorkflow();

			AssertEquals(2, job.WorkflowItems.Tasks.Count);

			job.Z0_Code = "ZUM";
			job.ApplyWorkflowTemplates();

			AssertEquals(3, job.WorkflowItems.Tasks.Count);
			AssertEquals("With Conditions", job.WorkflowItems.Tasks[2].P9_Description);
			AssertEquals(workflow2, job.WorkflowItems.Tasks[2].ProcessHeader);
		}

		[TestDate(2015, 7, 14)]
		public void TestApplyTemplateConditionalTask_WhenTemplateWorkflowClonedAndExistingWorkflowRenamed_ShouldNotThrowException()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "DUM");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM");

			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);
			var templateTask1 = BMSTestHelper.CreateTask(template, templateWorkflow, string.Empty, description: "Lemming");
			var templateTask2 = BMSTestHelper.CreateTask(template, templateWorkflow, string.Empty, description: "With Conditions");

			templateTask2.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTask2.TemplateConditions.TemplateCondition2Value = "\"<Z0_Code>\"==\"ZUM\"";

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.ApplyWorkflowTemplates();

			AssertEquals(1, job.WorkflowItems.Tasks.Count);
			AssertEquals("Lemming", job.WorkflowItems.Tasks[0].P9_Description);

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders.Single();

			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			var workflow2 = workflow1.CloneWorkflow();

			workflow1.FH_CompletionStatement += " NOT!";
			workflow2.FH_CompletionStatement += " NOT!";

			AssertEquals(2, job.WorkflowItems.Tasks.Count);

			job.Z0_Code = "ZUM";
			job.ApplyWorkflowTemplates();

			AssertEquals(2, jobHeader.ProcessHeaders.Count);
			AssertEquals(3, job.WorkflowItems.Tasks.Count);
			AssertEquals("With Conditions", job.WorkflowItems.Tasks[2].P9_Description);
			AssertEquals(workflow2, job.WorkflowItems.Tasks[2].ProcessHeader);
		}

		public void TestTemplateConditionsOnTasks_ShouldImportWorkflowAndLinks_InsertBetweenOtherWorkflows()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");

			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template, "Workflow Won", releaseGroupPK: config.ReleaseGroup.PK);
			var templateWorkflow3 = BMSTestHelper.CreateWorkflow(template, "Workflow Three", releaseGroupPK: config.ReleaseGroup.PK);
			var templateTask1 = BMSTestHelper.CreateTask(template, templateWorkflow1, string.Empty, description: "You're a wizard, Harry.");
			var templateTask3 = BMSTestHelper.CreateTask(template, templateWorkflow3, string.Empty, description: "Are you?");

			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();

			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks(job);

			Factory.Save();

			AssertEquals(2, job.WorkflowItems.Tasks.Count);

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);

			AssertNotNull(jobHeader);
			AssertEquals(2, jobHeader.ProcessHeaders.Count);
			AssertEquals(0, jobHeader.ProcessHeaders[0].Links.Count());

			AssertEquals(WorkflowStatusList.Codes.Open, jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Workflow Won").FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Workflow Three").FH_Status);

			// Create a new workflow and task on the template. Since the task has a template application condition, it'll be considered to be added to the job on next save.

			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template, "Workflow Too");
			var templateTask2 = BMSTestHelper.CreateTask(template, templateWorkflow2, string.Empty, description: "I'm not a wizard, I'm just harry.");

			templateTask2.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTask2.TemplateConditions.TemplateCondition2Value = "\"<OH_Code>\" == \"SHERSON\"";

			templateWorkflow2.AddTag(config.PrincessCelestiaTag);
			BMSTestHelper.CreateDependencyLink(template, templateWorkflow1, templateWorkflow2);
			BMSTestHelper.CreateDependencyLink(template, templateWorkflow2, templateWorkflow3);

			Factory.Save();

			job.OH_Code = "SHERSON";
			AssertEquals(2, job.WorkflowItems.Tasks.Count);

			Factory.Save();

			AssertEquals(3, job.WorkflowItems.Tasks.Count);

			var task2 = job.WorkflowItems.Tasks.Cast<ProcessTask>().Single(t => t.P9_Description == "I'm not a wizard, I'm just harry.");

			AssertNotNull(task2.ProcessHeader);
			AssertEquals("Workflow Too", task2.ProcessHeader.FH_CompletionStatement);
			BMSTestCaseWithFactory.AssertTagApplied(task2.GetProcessHeader(), config.PrincessCelestiaTag);

			AssertEquals(WorkflowStatusList.Codes.Open, jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Workflow Won").FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, task2.ProcessHeader.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Workflow Three").FH_Status);

			AssertNoErrors(jobHeader);
		}

		public void TestTemplateConditionsOnTasks_ShouldImportWorkflowAndLinks_AppendAfterOtherWorkflows()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");

			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template, "Workflow Won", releaseGroupPK: config.ReleaseGroup.PK);
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template, "Workflow Too", releaseGroupPK: config.ReleaseGroup.PK);
			var templateTask1 = BMSTestHelper.CreateTask(template, templateWorkflow1, string.Empty, description: "You're a wizard, Harry.");
			var templateTask2 = BMSTestHelper.CreateTask(template, templateWorkflow2, string.Empty, description: "I'm not a wizard, I'm just harry.");

			BMSTestHelper.CreateDependencyLink(template, templateWorkflow1, templateWorkflow2);

			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();

			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks(job);

			Factory.Save();

			AssertEquals(2, job.WorkflowItems.Tasks.Count);

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);

			AssertNotNull(jobHeader);
			AssertEquals(2, jobHeader.ProcessHeaders.Count);

			var workflow1 = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Workflow Won");
			var workflow2 = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Workflow Too");

			AssertEquals(WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, workflow2.FH_Status);

			// Create a new workflow and task on the template. Since the task has a template application condition, it'll be considered to be added to the job on next save.

			var templateWorkflow3 = BMSTestHelper.CreateWorkflow(template, "Workflow Three");
			var templateTask3 = BMSTestHelper.CreateTask(template, templateWorkflow3, string.Empty, description: "Are you?");

			templateTask3.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTask3.TemplateConditions.TemplateCondition2Value = "\"<OH_Code>\" == \"SHERSON\"";

			templateWorkflow3.AddTag(config.PrincessCelestiaTag);
			BMSTestHelper.CreateDependencyLink(template, templateWorkflow2, templateWorkflow3);

			Factory.Save();

			job.OH_Code = "SHERSON";
			AssertEquals(2, job.WorkflowItems.Tasks.Count);

			Factory.Save();

			AssertEquals(3, job.WorkflowItems.Tasks.Count);

			var task3 = job.WorkflowItems.Tasks.Cast<ProcessTask>().Single(t => t.P9_Description == "Are you?");

			AssertNotNull(task3.ProcessHeader);
			AssertEquals("Workflow Three", task3.ProcessHeader.FH_CompletionStatement);
			BMSTestCaseWithFactory.AssertTagApplied(task3.GetProcessHeader(), config.PrincessCelestiaTag);
			AssertEquals(WorkflowStatusList.Codes.Blocked, task3.ProcessHeader.FH_Status);

			AssertNoErrors(jobHeader);
		}

		public void TestTemplateConditionsOnTasks_ShouldImportWorkflowAndLinks_WhenOriginalWorkflowIsRenamed_ShouldNotExplode()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");

			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template, "Workflow Won", releaseGroupPK: config.ReleaseGroup.PK);
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template, "Workflow Too", releaseGroupPK: config.ReleaseGroup.PK);
			var templateTask1 = BMSTestHelper.CreateTask(template, templateWorkflow1, string.Empty, description: "You're a wizard, Harry.");
			var templateTask2 = BMSTestHelper.CreateTask(template, templateWorkflow2, string.Empty, description: "I'm not a wizard, I'm just harry.");

			BMSTestHelper.CreateDependencyLink(template, templateWorkflow1, templateWorkflow2);

			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();

			BMSTestHelper.FillWithValidTestDataSoFormSaveWorks(job);

			Factory.Save();

			AssertEquals(2, job.WorkflowItems.Tasks.Count);

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);

			AssertNotNull(jobHeader);
			AssertEquals(2, jobHeader.ProcessHeaders.Count);

			var workflow1 = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Workflow Won");
			var workflow2 = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Workflow Too");

			AssertEquals(WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, workflow2.FH_Status);

			// Create a new workflow and task on the template. Since the task has a template application condition, it'll be considered to be added to the job on next save.

			var templateWorkflow3 = BMSTestHelper.CreateWorkflow(template, "Workflow Three");
			var templateTask3 = BMSTestHelper.CreateTask(template, templateWorkflow3, string.Empty, description: "Are you?");

			templateTask3.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTask3.TemplateConditions.TemplateCondition2Value = "\"<OH_Code>\" == \"SHERSON\"";

			templateWorkflow3.AddTag(config.PrincessCelestiaTag);
			BMSTestHelper.CreateDependencyLink(template, templateWorkflow2, templateWorkflow3);

			Factory.Save();

			job.OH_Code = "SHERSON";
			var task1 = job.WorkflowItems.Tasks.Cast<ProcessTask>().Single(t => t.P9_Description == "You're a wizard, Harry.");
			task1.P9_Status = "CLS";
			workflow2.FH_CompletionStatement += ". NYA AH AHHHH";
			AssertEquals(2, job.WorkflowItems.Tasks.Count);

			Factory.Save();

			AssertEquals(3, job.WorkflowItems.Tasks.Count);

			var task3 = job.WorkflowItems.Tasks.Cast<ProcessTask>().Single(t => t.P9_Description == "Are you?");
			AssertNotNull(task3.ProcessHeader);
			AssertEquals("Workflow Three", task3.ProcessHeader.FH_CompletionStatement);
			BMSTestCaseWithFactory.AssertTagApplied(task3.GetProcessHeader(), config.PrincessCelestiaTag);
			AssertEquals("Even though the workflow was renamed we look at the original workflow from the template to find dependancies. This is used for functionality that allows users to reapply templates casuing 'Workflow' and 'Workflow (2)' etc. If the user does want the depandancies/tasks from the template to be applied to their workflow they should create a new workflow",
				WorkflowStatusList.Codes.Blocked, task3.ProcessHeader.FH_Status);

			AssertNoErrors(jobHeader);
		}

		#endregion

		#region On Factory Saved

		public void TestWorkflowShouldTransferSynchronouslyOnSave_WhenRegistryItemIsEnabled()
		{
			TestSynchronousResponsiveTransfer(true);
		}

		public void TestWorkflowShouldNotTransferSynchronouslyOnSave_WhenRegistryItemIsDisabled()
		{
			TestSynchronousResponsiveTransfer(false);
		}

		void TestSynchronousResponsiveTransfer(bool enableRegistryItem)
		{
			BMSRegistry.Instance.EnableSynchronousPAVEDataProcessing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enableRegistryItem);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "b2");
			BMSTestHelper.LinkComponents(bucket1, bucket2);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			workflow.FH_FC_CurrentComponent = bucket1.PK;
			AssertEquals("Workflow should be in bucket1", bucket1.PK, workflow.FH_FC_CurrentComponent);
			Factory.Save();

			if (enableRegistryItem)
			{
				AssertEquals("Workflow should transfer to bucket2", bucket2.PK, workflow.FH_FC_CurrentComponent);
			}
			else
			{
				AssertEquals("Workflow should remain in bucket1", bucket1.PK, workflow.FH_FC_CurrentComponent);
			}
		}

		public void TestWorkflowShouldNotTransferSynchronouslyOnSave_IfParentIsNull()
		{
			BMSRegistry.Instance.EnableSynchronousPAVEDataProcessing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "b2");
			BMSTestHelper.LinkComponents(bucket1, bucket2);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			jobHeader.FH_ParentId = ZGuid.Empty;
			workflow.FH_FC_CurrentComponent = bucket1.PK;
			AssertEquals("Workflow should be in bucket1", bucket1.PK, workflow.FH_FC_CurrentComponent);
			Factory.Save();

			AssertEquals("Workflow should remain in bucket1", bucket1.PK, workflow.FH_FC_CurrentComponent);
		}

		public void TestWorkflowShouldTransferSynchronouslyOnSave_IfJobHasChanges()
		{
			BMSRegistry.Instance.EnableSynchronousPAVEDataProcessing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "b2");
			BMSTestHelper.LinkComponents(bucket1, bucket2);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			workflow.FH_FC_CurrentComponent = bucket1.PK;
			AssertEquals("Workflow should be in bucket1", bucket1.PK, workflow.FH_FC_CurrentComponent);
			Factory.Save();

			BMSRegistry.Instance.EnableSynchronousPAVEDataProcessing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Factory.Save();
			AssertEquals("Workflow should remain in bucket1 since job has not changed", bucket1.PK, workflow.FH_FC_CurrentComponent);

			workflow.FH_CompletionStatement = "test";
			Factory.Save();
			AssertEquals("Workflow should transfer to bucket2", bucket2.PK, workflow.FH_FC_CurrentComponent);
		}

		#endregion

		#region Status

		public void TestStatus_WhenWorkflowIsDeleted_ShouldReCalculateStatus()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "workflow2");

			Factory.Save();

			CombineAssertions("Pre-condition statii", () =>
			{
				AssertEquals("jobHeader", WorkflowStatusList.Codes.Open, jobHeader.FH_Status);
				AssertEquals("workflow1", WorkflowStatusList.Codes.Open, workflow1.FH_Status);
				AssertEquals("workflow2", WorkflowStatusList.Codes.Open, workflow2.FH_Status);
			});

			workflow1.Tasks.Single().P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			workflow2.Delete();
			Factory.Save();

			CombineAssertions("After deleting the last incomplete workflow, the job-level workflow status should be re-calculated", () =>
			{
				AssertEquals("jobHeader", WorkflowStatusList.Codes.Closed, jobHeader.FH_Status);
				AssertEquals("workflow1", WorkflowStatusList.Codes.Closed, workflow1.FH_Status);
			});
		}

		#endregion

		#region SourceTemplatePK

		public void TestSourceTemplatePK()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			AssertEquals("SourceTemplatePK is empty if not applied from template", ZGuid.Empty, workflow.SourceTemplatePK);

			workflow.FH_ParentTemplateId = ZGuid.Invalid;
			AssertEquals("SourceTemplatePK is also invalid when ParentTemplateId is invalid", ZGuid.Invalid, workflow.SourceTemplatePK);

			workflow.FH_ParentTemplateId = ZGuid.NewZGuid();
			AssertEquals("SourceTemplatePK is also invalid when unable to load ProcessTaskTemplate", ZGuid.Invalid, workflow.SourceTemplatePK);
		}

		#endregion

		#region ProcessTask Tests

		public void TestAddTask_WhenNoWorkflowPresent_ShouldAddDefaultWorkflow()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			AssertEquals(0, jobHeader.ProcessHeaders.Count);

			var task = job.WorkflowItems.Tasks.AddNew();

			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals(jobHeader.ProcessHeaders[0], task.ProcessHeader);
		}

		public void TestNonDefaultEmptyWorkflow_ShouldNotGetDeleted()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM");

			var templateHeader1 = template.ProcessHeaders.AddNew();
			templateHeader1.FH_CompletionStatement = "Header 1";

			var templateHeader2 = template.ProcessHeaders.AddNew();
			templateHeader2.FH_CompletionStatement = "Header 2";

			var templateTask1 = template.WorkflowItems.Tasks.AddNew();
			templateTask1.P9_FH_ProcessHeader = templateHeader1.PK;

			var templateTask2 = template.WorkflowItems.Tasks.AddNew();

			var templateTask3 = template.WorkflowItems.Tasks.AddNew();
			templateTask3.P9_FH_ProcessHeader = templateHeader2.PK;

			var templateTask4 = template.WorkflowItems.Tasks.AddNew();
			templateTask4.P9_FH_ProcessHeader = templateHeader2.PK;

			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(dummy, Factory, addDefaultProcessHeaderIfNone: false);
			AssertEquals(0, jobHeader.ProcessHeaders.Count);
			AssertEquals(0, dummy.WorkflowItems.Tasks.Count);
			dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();

			AssertEquals(2, jobHeader.ProcessHeaders.Count);

			var processHeader = jobHeader.ProcessHeaders.AddNew();
			processHeader.FH_CompletionStatement = "Something";

			dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();

			AssertEquals(3, jobHeader.ProcessHeaders.Count);
		}

		public void TestCreateTaskFromTemplate_WithNoHeader_ShouldSetWorkflowFK()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			template.WorkflowItems.AddNew();
			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.CreateProcessTaskFromTemplate(Factory);
			AssertEquals(1, job.WorkflowItems.Count);
			Factory.Save();

			var workflow = ProcessJobHeaderProvider.GetForParent(job, Factory).ProcessHeaders[0];
			AssertEquals(1, Factory.CreateNewFactory().Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_FH_ProcessHeader, workflow.PK)).Length);
		}

		public void TestCreateTask_ShouldSetWorkflowFK()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(org, Factory);
			var header = jobHeader.ProcessHeaders[0];
			var task = org.WorkflowItems.AddNew();

			Factory.Save();

			AssertNotNull(Factory.CreateNewFactory().LoadTop1<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_FH_ProcessHeader, header.PK)));
		}

		public void TestCreateTask_WhenTwoWorkflowsExistIncludingDefaultWorkflow_ShouldNotSetWorkflowFK()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory);
			var defaultWorkflow = jobHeader.ProcessHeaders[0];
			var anotherWorkflow = jobHeader.ProcessHeaders.AddNew();
			anotherWorkflow.FH_CompletionStatement = "Another workflow";

			var task1 = job.WorkflowItems.AddNew();
			AssertNull("Should not find a ProcessHeader since there are multiple workflows", task1.ProcessHeader);
		}

		public void TestProcessHeaders_Job()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(dummy, Factory);
			jobHeader.ProcessHeaders.DeleteAll();
			var header1 = jobHeader.ProcessHeaders.AddNew();
			var header2 = jobHeader.ProcessHeaders.AddNew();

			var dummy2 = Factory.New<DummyWithWorkflow>();
			var jobHeader2 = ProcessJobHeaderProvider.GetForParent(dummy2, Factory);
			jobHeader2.ProcessHeaders.DeleteAll();
			var header3 = jobHeader2.ProcessHeaders.AddNew();

			var task = dummy.WorkflowItems.Tasks.AddNew();

			AssertEquals(2, task.Lookups.ProcessHeaders.Count);
			AssertCollectionContains(header1, task.Lookups.ProcessHeaders);
			AssertCollectionContains(header2, task.Lookups.ProcessHeaders);
			AssertCollectionNotContains(header3, task.Lookups.ProcessHeaders);
		}

		public void TestProcessHeaders_StandAlone()
		{
			var task = Factory.New<ProcessTask>();
			AssertEquals(0, task.Lookups.ProcessHeaders.Count);
		}

		public void TestProcessHeaders_Template()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var header1 = template.ProcessHeaders.AddNew();
			var header2 = template.ProcessHeaders.AddNew();

			var template2 = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var header3 = template2.ProcessHeaders.AddNew();

			var task = template.WorkflowItems.Tasks.AddNew();
			AssertEquals(2, task.Lookups.ProcessHeaders.Count);
			AssertCollectionContains(header1, task.Lookups.ProcessHeaders);
			AssertCollectionContains(header2, task.Lookups.ProcessHeaders);
			AssertCollectionNotContains(header3, task.Lookups.ProcessHeaders);
		}

		#endregion

		#region CreateIteration

		public void TestCreateIteration_ShouldAllowEvenWhenNoSecurityToChangeComponent()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var bucket2 = BMSTestHelper.CreateBucket(config.System, "bucket2");
			BMSTestHelper.LinkComponents(config.Buffer, bucket2);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", currentComponent: config.Buffer);

			Factory.Save();

			IProcessHeader qualityIteration;

			using (Env.SetTemporaryUserContext(resource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed = false;

				qualityIteration = ((IProcessJobHeader)jobHeader).CreateQualityIterationWorkflow(workflow);
				AssertEquals(config.Buffer, qualityIteration.CurrentComponent);
				AssertNoErrors("Should be allowed to place the quality iteration workflow into the same component as the parent workflow", qualityIteration.FH_FC_CurrentComponentInfo);

				qualityIteration.FH_FC_CurrentComponent = bucket2.PK;
				AssertHasError("Should not be able to change component to something other than what the parent workflow is set to", qualityIteration.FH_FC_CurrentComponentInfo, "You do not have security rights to change Current Component.");

				qualityIteration.FH_FC_CurrentComponent = config.Buffer.PK;
				AssertNoErrors("Should be allowed to move the quality iteration workflow back into the correct component", qualityIteration.FH_FC_CurrentComponentInfo);

				Factory.Save();
			}

			Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed = true;

			qualityIteration.FH_FC_CurrentComponent = bucket2.PK;
			AssertNoErrors("Someone with permission should be able to move the workflow wherever they want", qualityIteration.FH_FC_CurrentComponentInfo);

			Factory.Save();

			using (Env.SetTemporaryUserContext(resource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed = false;

				qualityIteration.FH_FC_CurrentComponent = config.Buffer.PK;
				AssertHasError("Once the QI workflow has been saved, we don't allow manipulation by unauthorised parties", qualityIteration.FH_FC_CurrentComponentInfo, "You do not have security rights to change Current Component.");
			}
		}

		[TestDate(2017, 1, 3)]
		public void TestCreateIteration_ShouldCopyAllProcessHeaderFields()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("INQ", "TS1", "Test category");
			BMSTestHelper.AddWorkflowCategoryToRegistry("INQ", "TS2", "Test category2");
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "INQ");
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var originalWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow", config.Bucket, releaseDateTime: ZDateTime.Now.AddDays(-4), releaseGroupPK: config.ReleaseGroup.PK, autoAssignTasks: true);
			originalWorkflow.FH_AgreedDeliveryDate = ZDateTime.Now.AddDays(1);
			originalWorkflow.FH_DateAcceptability = DateAcceptabilityList.Codes.ExtendedStartExtendedFinish;
			originalWorkflow.FH_DoNotStartBeforeDate = ZDateTime.Now.AddDays(-5);
			originalWorkflow.FH_PlannedDurationInMinutes = 50;
			originalWorkflow.FH_StaggeredReleaseDelayExpiry = ZDateTime.Now.AddDays(2);
			originalWorkflow.FH_TimeDelayFactor = 2.1;
			originalWorkflow.FH_TimeDelayMinutes = 13;
			originalWorkflow.FH_IsActive = false;
			originalWorkflow.FH_IsCriticalHandover = true;
			originalWorkflow.FH_IsReleasableUnitParent = true;
			originalWorkflow.FH_IsStandby = true;
			originalWorkflow.FH_VoteUpDownAmount = 3;
			originalWorkflow.FH_P0_Template = ZGuid.BrettsGuid;
			originalWorkflow.FH_ParentTemplateId = ZGuid.BrettsGuid;
			originalWorkflow.FH_Category = "TS2";

			var iteration = (ProcessHeader)((IProcessJobHeader)jobHeader).CreateQualityIterationWorkflow(originalWorkflow);

			AssertEquals("Workflow (Quality Iteration 1)", iteration.FH_CompletionStatement);
			AssertEquals(config.Bucket.PK, iteration.FH_FC_CurrentComponent);
			AssertEquals(true, iteration.FH_AllowTaskAutoAssignment);
			AssertEquals(ZDateTime.Now.AddDays(-4), iteration.FH_ReleaseDateTime);
			AssertEquals(config.ReleaseGroup.PK, iteration.FH_GG_ReleaseGroup);
			AssertEquals(ZDateTime.Now.AddDays(1), iteration.FH_AgreedDeliveryDate);
			AssertEquals(DateAcceptabilityList.Codes.ExtendedStartExtendedFinish, iteration.FH_DateAcceptability);
			AssertEquals(ZDateTime.Now.AddDays(-5), iteration.FH_DoNotStartBeforeDate);
			AssertEquals(50, iteration.FH_PlannedDurationInMinutes);
			AssertEquals(ZDateTime.Now.AddDays(2), iteration.FH_StaggeredReleaseDelayExpiry);
			AssertEquals(2.1m, iteration.FH_TimeDelayFactor);
			AssertEquals(13, iteration.FH_TimeDelayMinutes);
			AssertEquals(false, iteration.FH_IsActive);
			AssertEquals(true, iteration.FH_IsCriticalHandover);
			AssertEquals(true, iteration.FH_IsReleasableUnitParent);
			AssertEquals(true, iteration.FH_IsStandby);
			AssertEquals((short)3, iteration.FH_VoteUpDownAmount);
			AssertEquals("TS2", iteration.FH_Category);
			AssertEquals("Test category2", iteration.CategoryDescription);
			AssertEquals("Template fields should be blank because the iteration didn't come from a template, and yet...", ZGuid.Empty, iteration.FH_P0_Template);
			AssertEquals("Template fields should be blank because the iteration didn't come from a template, and yet...", ZGuid.Empty, iteration.FH_ParentTemplateId);
		}

		public void TestCreateIteration_ShouldNotCopyTasksOrTagLinks()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory, "INQ");
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var originalWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var task = BMSTestHelper.CreateTask(originalWorkflow);
			BMSTestHelper.CreateTagLink(originalWorkflow, config.PrincessCelestiaTag);

			var iteration = (ProcessHeader)((IProcessJobHeader)jobHeader).CreateQualityIterationWorkflow(originalWorkflow);
			AssertEquals("Iterations should not be created with child objects including tasks, and yet...", 0, iteration.Tasks.Count());
			AssertEquals("Iterations should not be created with child objects including TagLinks, and yet...", 0, iteration.TagLinks.Count);
		}

		public void TestCreateIteration_WithRegexSpecialCharactersInIterationType_ShouldNotThrowExceptions()
		{
			TestConfigsHelper.CreateTagsTestConfig(Factory, "INQ");
			const string regexCharacters = @"\^$.|?*+()[{";
			AssertEquals("It's important that we check all the regex special characters. Please don't remove any.", 12, regexCharacters.Length);

			foreach (var character in regexCharacters)
			{
				var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
				var originalWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
				var iterationType = $"{character} Quality {character} Iteration {character}{character}{character}";
				ProcessHeader iteration = null;

				AssertNoExceptionThrown(() =>
				{
					iteration = (ProcessHeader)((IProcessJobHeader)jobHeader).CreateQualityIterationWorkflow(originalWorkflow, iterationType);
				});

				AssertEquals($"Workflow ({character} Quality {character} Iteration {character}{character}{character} 1)", iteration.FH_CompletionStatement);
			}
		}

		#endregion

		#region Category

		public void TestFH_Category_ReadOnly_IsTrue()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);

			Assert(jobHeader.FH_CategoryInfo.ReadOnly);
		}

		#endregion

		#region Planned Duration

		public void TestPlannedDuration_ShouldNotDoubleAddWorkflows()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow1_1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1_1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow2_1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2_1");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");
			var workflow3_1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3_1");

			workflow1_1.GetOrCreateLinkToParent(workflow1);
			workflow2_1.GetOrCreateLinkToParent(workflow2);
			workflow3_1.GetOrCreateLinkToParent(workflow3);

			var task1 = BMSTestHelper.CreateTask(workflow1, lowEstMinutes: 60, estVariationFactor: 1);
			var task1_1 = BMSTestHelper.CreateTask(workflow1_1, lowEstMinutes: 60, estVariationFactor: 1);
			var task2 = BMSTestHelper.CreateTask(workflow2, lowEstMinutes: 60, estVariationFactor: 1);
			var task2_1 = BMSTestHelper.CreateTask(workflow2_1, lowEstMinutes: 60, estVariationFactor: 1);
			var task3 = BMSTestHelper.CreateTask(workflow3, lowEstMinutes: 60, estVariationFactor: 1);
			var task3_1 = BMSTestHelper.CreateTask(workflow3_1, lowEstMinutes: 60, estVariationFactor: 1);

			Factory.Save();

			CombineAssertions("Planned durations", () =>
			{
				AssertEquals("jobHeader", 6 * 60, jobHeader.FH_PlannedDurationInMinutes);
				AssertEquals("workflow1", 2 * 60, workflow1.FH_PlannedDurationInMinutes);
				AssertEquals("workflow1_1", 1 * 60, workflow1_1.FH_PlannedDurationInMinutes);
				AssertEquals("workflow2", 2 * 60, workflow2.FH_PlannedDurationInMinutes);
				AssertEquals("workflow2_1", 1 * 60, workflow2_1.FH_PlannedDurationInMinutes);
				AssertEquals("workflow3", 2 * 60, workflow3.FH_PlannedDurationInMinutes);
				AssertEquals("workflow3_1", 1 * 60, workflow3_1.FH_PlannedDurationInMinutes);
			});
		}

		#endregion

		#region IsReleased

		public void TestIsReleased()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Severus");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Snape");

			AssertEquals(false, jobHeader.IsReleased);
			AssertEquals(false, workflow1.IsReleased);
			AssertEquals(false, workflow2.IsReleased);

			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;

			AssertEquals(true, jobHeader.IsReleased);
			AssertEquals(true, workflow1.IsReleased);
			AssertEquals(false, workflow2.IsReleased);

			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;

			AssertEquals(true, jobHeader.IsReleased);
			AssertEquals(true, workflow1.IsReleased);
			AssertEquals(true, workflow2.IsReleased);
		}

		public void TestIsReleased_WhenNoWorkflows()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			AssertEquals(false, jobHeader.IsReleased);
		}

		#endregion

		#region IsApproved

		public void TestIsApproved()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			AssertEquals(false, jobHeader.FH_IsApproved);
			AssertEquals(false, workflow1.FH_IsApproved);
			AssertEquals(false, workflow2.FH_IsApproved);

			jobHeader.FH_IsApproved = true;

			AssertEquals(true, jobHeader.FH_IsApproved);
			AssertEquals(true, workflow1.FH_IsApproved);
			AssertEquals(true, workflow2.FH_IsApproved);
		}

		public void TestIsApproved_ShouldBeReadOnlyForProcessHeaders()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();

			AssertEquals(false, jobHeader.FH_IsApprovedInfo.ReadOnly);
			AssertEquals(true, workflow.FH_IsApprovedInfo.ReadOnly);
		}

		public void TestIsApproved_GivesWarning_IfTooManyDescendants()
		{
			BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var parentHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);

			var childHeaders = Enumerable.Range(0, 16).Select(_ =>
			{
				var childHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
				BMSTestHelper.CreateParentChildLink(parentHeader, childHeader);
				return childHeader;
			})
			.ToList();

			parentHeader.FH_IsApproved = true;

			AssertContains("This will set (or reset) the Approved flag for 17 job-level workflows.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContains("Confirm Cascading Approved Flag", UnitTestUserNotification.Instance.LastMessage.Caption);

			AssertEquals(true, parentHeader.FH_IsApproved);
			foreach (var childHeader in childHeaders)
			{
				AssertEquals(true, childHeader.FH_IsApproved);
			}
		}

		public void TestIsApproved_CascadesFromParentToChild()
		{
			var jobHeader1 = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			BMSTestHelper.CreateParentChildLink(jobHeader1, jobHeader2);

			AssertEquals(false, jobHeader1.FH_IsApproved);
			AssertEquals(false, jobHeader2.FH_IsApproved);

			jobHeader1.FH_IsApproved = true;

			AssertEquals(true, jobHeader1.FH_IsApproved);
			AssertEquals(true, jobHeader2.FH_IsApproved);

			jobHeader2.FH_IsApproved = false;

			AssertEquals(true, jobHeader1.FH_IsApproved);
			AssertEquals(false, jobHeader2.FH_IsApproved);
		}

		public void TestIsApproved_CascadesToDescendants()
		{
			var jobHeader1 = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var jobHeader3 = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			BMSTestHelper.CreateParentChildLink(jobHeader1, jobHeader2);
			BMSTestHelper.CreateParentChildLink(jobHeader2, jobHeader3);

			AssertEquals(false, jobHeader1.FH_IsApproved);
			AssertEquals(false, jobHeader2.FH_IsApproved);
			AssertEquals(false, jobHeader3.FH_IsApproved);

			jobHeader1.FH_IsApproved = true;

			AssertEquals(true, jobHeader1.FH_IsApproved);
			AssertEquals(true, jobHeader2.FH_IsApproved);
			AssertEquals(true, jobHeader3.FH_IsApproved);
		}

		public void TestIsApproved_Cascades_WhenNewParentChildLinkCreated()
		{
			BMSTestHelper.CreateSystem(Factory, "DUM");

			var parentJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var childJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			parentJobHeader.FH_IsApproved = true;
			AssertEquals(false, childJobHeader.FH_IsApproved);

			childJobHeader.GetOrCreateLinkToParent(parentJobHeader);

			AssertEquals(true, childJobHeader.FH_IsApproved);
		}

		public void TestIsApproved_IgnoresNonJobLevelLinks()
		{
			var jobHeader1 = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader1.ProcessHeaders.AddNew();
			var jobHeader2 = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			BMSTestHelper.CreateParentChildLink(jobHeader1, workflow2);
			BMSTestHelper.CreateParentChildLink(workflow1, jobHeader2);

			jobHeader1.FH_IsApproved = true;

			AssertEquals(true, jobHeader1.FH_IsApproved);
			AssertEquals(true, workflow1.FH_IsApproved);
			AssertEquals(false, jobHeader2.FH_IsApproved);
			AssertEquals(false, workflow2.FH_IsApproved);
		}

		public void TestIsApproved_HandlesCyclicLoops()
		{
			var jobHeader1 = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var jobHeader2 = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			BMSTestHelper.CreateParentChildLink(jobHeader1, jobHeader2);
			BMSTestHelper.CreateParentChildLink(jobHeader2, jobHeader1);

			jobHeader1.FH_IsApproved = true;

			AssertEquals(true, jobHeader1.FH_IsApproved);
			AssertEquals(true, jobHeader2.FH_IsApproved);
		}

		public void TestIsApproved_ShouldCreateAPR_Event_WhenApproved()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			Factory.Save();

			jobHeader.FH_IsApproved = true;
			Factory.Save();

			var firstEvent = jobHeader.Logs.GetAllLogs()[0].Event;
			AssertEquals(Events.Approved, firstEvent);
		}

		public void TestIsApproved_ShouldCreateUNA_Event_WhenUnapproved()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			jobHeader.FH_IsApproved = true;
			Factory.Save();

			jobHeader.FH_IsApproved = false;
			Factory.Save();

			var firstEvent = jobHeader.Logs.GetAllLogs()[0].Event;
			AssertEquals(Events.Unapproved, firstEvent);
		}

		#endregion

		#region HasChanges

		[TestDate(2014, 7, 10)]
		[TestUtcOffset(10, 0, 0)]
		[ExpectNoExceptions]
		public void TestHasChangesOnJobs_ShouldNotGetActivatedWhenTasksSavedInAnotherTimeZone()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<JobThatDoesntLikeBeingSavedWhenItReallyHasNoChanges>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			task.TaskProperties.ActualDate = ZDateTimeOffset.Now;

			Factory.Save();

			TestUtcOffsetAttribute.Time = new TimeSpan(9, 0, 0);

			Factory.Save();
		}

		class JobThatDoesntLikeBeingSavedWhenItReallyHasNoChanges : OrgHeader
		{
			public JobThatDoesntLikeBeingSavedWhenItReallyHasNoChanges(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void OnFactorySaving()
			{
				base.OnFactorySaving();

				if (IsInDatabase && HasChanges)
				{
					throw new InvalidOperationException("Nup - no one has made changes to me, go away.");
				}
			}
		}

		#endregion

		#region Delete

		[ExpectNoExceptions]
		public void TestDelete_ShouldDeleteProcessHeadersNotLinkedToSameJob()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			var nonLinkedWorkflow = Factory.NewWithValidTestData<ProcessHeader>();
			nonLinkedWorkflow.FH_FH_ParentHeader = jobHeader.PK;

			Factory.Save();

			jobHeader.Delete();
			Factory.Save();
		}

		public void TestDeletedProcessHeaderLinks_ShouldStayDeleted()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM");

			var templateHeader1 = (ProcessHeader)template.ProcessHeaders.AddNew();
			templateHeader1.FH_CompletionStatement = "Header 1";

			var templateHeader2 = (ProcessHeader)template.ProcessHeaders.AddNew();
			templateHeader2.FH_CompletionStatement = "Header 2";

			var templateLink = BMSTestHelper.CreateDependencyLink(template, templateHeader1, templateHeader2);

			var task1 = template.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = templateHeader1.PK;

			var task2 = template.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = templateHeader2.PK;

			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(dummy, Factory);

			AssertEquals(2, jobHeader.ProcessHeaders.Count);
			AssertEquals(2, dummy.WorkflowItems.Tasks.Count);

			var header1 = jobHeader.ProcessHeaders.First(h => h.FH_CompletionStatement == "Header 1");
			AssertEquals(1, header1.LinksFromMeToOthers.Count());

			templateLink = header1.LinksFromMeToOthers.Single();
			templateLink.Delete();
			Factory.Save();

			dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals(0, header1.LinksFromMeToOthers.Count());
		}

		#endregion

		#region Properties

		public void TestWorkflowSpecificProperties_ShouldBeReadOnly()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();

			AssertEquals(true, jobHeader.FH_FC_CurrentComponentInfo.ReadOnly);
			AssertEquals(false, jobHeader.FH_GG_ReleaseGroupInfo.ReadOnly);

			AssertEquals(false, workflow.FH_FC_CurrentComponentInfo.ReadOnly);
			AssertEquals(false, workflow.FH_GG_ReleaseGroupInfo.ReadOnly);
		}

		public void TestIsOpen()
		{
			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			var task = job.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;

			Assert(jobHeader.IsOpen);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			Assert(!jobHeader.IsOpen);
		}

		public void TestGetIsOpen_CircularDependency()
		{
			var jobHeaders = BMSTestHelper.CreateCircularDependency_JobHeader(Factory, TestConnection);
			var jobHeader1 = jobHeaders.First();
			var jobHeader2 = jobHeader1;

			AssertNullOrEmpty(ErrorReporter.LastMessageReported);

			// make modification to trigger GetIsOpen call
			jobHeader1.FH_CompletionStatement = "jobHeader1";
			jobHeader2.FH_CompletionStatement = "jobHeader2";

			AssertEquals("GIVEN circular-dependency between jobHeader1 and jobHeader2, WHEN calling JobHeader.IsOpen THEN should return false",
				false,
				jobHeader1.JobHeader.IsOpen);
		}

		public void TestNudge_ShouldNudgeAllWorkflows()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			BMSTestHelper.CreateTask(workflow1);
			BMSTestHelper.CreateTask(workflow2);
			workflow1.FH_FC_DedicatedBuffer = buffer.PK;
			workflow2.FH_FC_DedicatedBuffer = buffer.PK;

			AssertEquals((short)0, workflow1.FH_VoteUpDownAmount);
			AssertEquals((short)0, workflow2.FH_VoteUpDownAmount);

			jobHeader.NudgeUp();

			AssertEquals((short)1, jobHeader.FH_VoteUpDownAmount);
			AssertEquals(1m, workflow1.EffectiveNudge);
			AssertEquals(1m, workflow2.EffectiveNudge);

			workflow1.UpdateEffectiveNudge();
			workflow2.UpdateEffectiveNudge();
			AssertEquals(1m, workflow1.FH_EffectiveNudge);
			AssertEquals(1m, workflow2.FH_EffectiveNudge);

			jobHeader.NudgeDown();

			AssertEquals((short)0, jobHeader.FH_VoteUpDownAmount);
			AssertEquals(0m, workflow1.EffectiveNudge);
			AssertEquals(0m, workflow2.EffectiveNudge);

			workflow1.UpdateEffectiveNudge();
			workflow2.UpdateEffectiveNudge();
			AssertEquals(0m, workflow1.FH_EffectiveNudge);
			AssertEquals(0m, workflow2.FH_EffectiveNudge);
		}

		public void TestProcessHeaderType()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<DummyWithWorkflow>(), Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			AssertEquals("Job", jobHeader.ProcessHeaderType);
			AssertEquals("Workflow", workflow.ProcessHeaderType);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();

			var loadedJobHeader = newFactory.Load<ProcessHeader>(jobHeader.PK);
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);
			AssertEquals("Job", loadedJobHeader.ProcessHeaderType);
			AssertEquals("Workflow", loadedWorkflow.ProcessHeaderType);
		}

		public void TestProcessHeaderType_TemplateRecords()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");

			var jobHeader = template.GetJobHeader();
			var workflow = template.ProcessHeaders.AddNew();

			AssertEquals("Template Job", jobHeader.ProcessHeaderType);
			AssertEquals("Template Workflow", workflow.ProcessHeaderType);
		}

		public void TestJobDescription()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "MAIORGSYD";
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			AssertEquals("Organization (MAIORGSYD)", jobHeader.ParentJobDescription);
		}

		public void TestTimeDelayMinutes()
		{
			var jobHeader = Factory.NewWithValidTestData<ProcessJobHeader>();
			jobHeader.FH_TimeDelayMinutes = 10;
			AssertEquals((ZDateTime)TimeSpan.FromMinutes(10), jobHeader.StaggeredReleaseTimeDelay);

			jobHeader.StaggeredReleaseTimeDelay = TimeSpan.FromMinutes(130);
			AssertEquals(130, jobHeader.FH_TimeDelayMinutes);
		}

		#endregion

		#region Template Workflows

		public void TestHeadersGetCopiedFromTemplate()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM");
			var templateJobHeader = template.GetJobHeader();

			templateJobHeader.FH_TimeDelayFactor = 2.5;
			templateJobHeader.FH_TimeDelayMinutes = 50;

			var group = Factory.NewWithValidTestData<GlbGroup>();

			var templateHeader1 = (ProcessHeader)template.ProcessHeaders.AddNew();
			templateHeader1.FH_CompletionStatement = "Header 1";
			templateHeader1.FH_GG_ReleaseGroup = group.PK;

			var templateHeader2 = (ProcessHeader)template.ProcessHeaders.AddNew();
			templateHeader2.FH_CompletionStatement = "Header 2";

			var templateHeader3 = (ProcessHeader)template.ProcessHeaders.AddNew();
			templateHeader3.FH_CompletionStatement = "Header 3";

			var templateHeader4 = (ProcessHeader)template.ProcessHeaders.AddNew();
			templateHeader4.FH_CompletionStatement = "Header 4";

			var link1 = BMSTestHelper.CreateParentChildLink(template, templateHeader3, templateHeader2);
			var link2 = BMSTestHelper.CreateDependencyLink(template, templateHeader1, templateHeader3);

			var templateTask1 = template.WorkflowItems.Tasks.AddNew();
			templateTask1.P9_FH_ProcessHeader = templateHeader1.PK;

			var templateTask2 = template.WorkflowItems.Tasks.AddNew();

			var templateTask3 = template.WorkflowItems.Tasks.AddNew();
			templateTask3.P9_FH_ProcessHeader = templateHeader2.PK;

			var templateTask4 = template.WorkflowItems.Tasks.AddNew();
			templateTask4.P9_FH_ProcessHeader = templateHeader2.PK;

			var templateTask5 = template.WorkflowItems.Tasks.AddNew();
			templateTask5.P9_FH_ProcessHeader = templateHeader3.PK;

			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			AssertEquals(0, dummy.WorkflowItems.Tasks.Count);
			dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(dummy, Factory);

			AssertEquals(2.5m, jobHeader.FH_TimeDelayFactor);
			AssertEquals(50, jobHeader.FH_TimeDelayMinutes);

			AssertEquals(3, jobHeader.ProcessHeaders.Count);
			AssertEquals(true, jobHeader.ProcessHeaders.Any(x => x.FH_CompletionStatement == "Header 1"));
			AssertEquals(true, jobHeader.ProcessHeaders.Any(x => x.FH_CompletionStatement == "Header 2"));
			AssertEquals(true, jobHeader.ProcessHeaders.Any(x => x.FH_CompletionStatement == "Header 3"));
			AssertEquals("Should not have added 'Header 4' since it contains no tasks or links", false, jobHeader.ProcessHeaders.Any(x => x.FH_CompletionStatement == "Header 4"));
			AssertEquals("New records - not same PK", false, jobHeader.ProcessHeaders.Any(x => x.PK == templateHeader1.PK));
			AssertEquals("New records - not same PK", false, jobHeader.ProcessHeaders.Any(x => x.PK == templateHeader2.PK));
			AssertEquals("New records - not same PK", false, jobHeader.ProcessHeaders.Any(x => x.PK == templateHeader3.PK));

			AssertEquals(5, dummy.WorkflowItems.Tasks.Count);
			AssertEquals(1, dummy.WorkflowItems.Tasks.Cast<ProcessTask>().Count(x => x.ProcessHeader != null && x.ProcessHeader.FH_CompletionStatement == "Header 1"));
			AssertEquals(2, dummy.WorkflowItems.Tasks.Cast<ProcessTask>().Count(x => x.ProcessHeader != null && x.ProcessHeader.FH_CompletionStatement == "Header 2"));
			AssertEquals(1, dummy.WorkflowItems.Tasks.Cast<ProcessTask>().Count(x => x.ProcessHeader != null && x.ProcessHeader.FH_CompletionStatement == "Header 3"));
			AssertEquals(1, dummy.WorkflowItems.Tasks.Cast<ProcessTask>().Count(x => x.ProcessHeader == null));

			var header1 = jobHeader.ProcessHeaders.First(x => x.FH_CompletionStatement == "Header 1");
			var header2 = jobHeader.ProcessHeaders.First(x => x.FH_CompletionStatement == "Header 2");
			var header3 = jobHeader.ProcessHeaders.First(x => x.FH_CompletionStatement == "Header 3");

			AssertEquals(group.PK, header1.FH_GG_ReleaseGroup);
			AssertEquals(1, header1.LinksFromMeToOthers.Count());
			AssertEquals(ProcessHeaderLinkTypeList.Codes.Dependency, header1.LinksFromMeToOthers.Single().FP_LinkType);
			AssertEquals(header3.PK, header1.LinksFromMeToOthers.Single().FP_FH_HeaderTo);

			AssertEquals(1, header3.LinksFromMeToOthers.Count());
			AssertEquals(ProcessHeaderLinkTypeList.Codes.ParentChild, header3.LinksFromMeToOthers.Single().FP_LinkType);
			AssertEquals(header2.PK, header3.LinksFromMeToOthers.Single().FP_FH_HeaderTo);

			dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals(1, header1.LinksFromMeToOthers.Count());

			jobHeader.FH_TimeDelayFactor = 1.5;
			jobHeader.FH_TimeDelayMinutes = 100;

			Factory.Save();

			dummy.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals(1.5m, jobHeader.FH_TimeDelayFactor);
			AssertEquals(100, jobHeader.FH_TimeDelayMinutes);
		}

		#endregion

		#region CompletionStatementTaskCollection

		public void TestSequenceNumber()
		{
			var job = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedWorkflowTaskType = categorisedTaskTypes.AddNew();
			categorisedWorkflowTaskType.Code = "ORG";
			var taskType = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskType.Code = "COM";
			taskType.IsCompletionStatementTaskType = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var collection = jobHeader.CompletionStatementTasksIncludingChildWorkflowTasks;

			var completionStatementTask1 = collection.AddNew();
			AssertEquals("COM", completionStatementTask1.P9_Type);
			AssertEquals(1001, completionStatementTask1.P9_Sequence);
			var completionStatementTask2 = collection.AddNew();
			AssertEquals("COM", completionStatementTask2.P9_Type);
			AssertEquals(1002, completionStatementTask2.P9_Sequence);

			completionStatementTask2.P9_Sequence = 1100;

			var completionStatementTask3 = collection.AddNew();
			AssertEquals("COM", completionStatementTask3.P9_Type);
			AssertEquals(1101, completionStatementTask3.P9_Sequence);
		}

		public void TestLoadCollection_NoWorkflowType()
		{
			var job = Factory.New<OrgHeader>();
			var task1 = job.WorkflowItems.AddNew();
			var task2 = job.WorkflowItems.AddNew();
			AssertEquals(ZString.Empty, task1.P9_Type);
			AssertEquals(ZString.Empty, task2.P9_Type);

			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var completionStatementTaskType = WorkflowDataRegistry.Instance.TaskTypes.Value.GetTaskTypesFromWorkflowCode(jobHeader.Parent.WorkflowType)
				.Cast<WorkflowTaskType>()
				.FirstOrDefault(t => t.IsCompletionStatementTaskType);

			AssertNull(completionStatementTaskType);

			var collection = jobHeader.CompletionStatementTasksIncludingChildWorkflowTasks;
			AssertEquals(0, collection.Count);
		}

		public void TestLoadCollection_WithWorkflowType()
		{
			var job = Factory.New<OrgHeader>();
			var task1 = job.WorkflowItems.AddNew();
			task1.P9_Type = "COM";
			task1.P9_Description = "Stuff is coded";
			var task2 = job.WorkflowItems.AddNew();
			task2.P9_Type = "COD";
			task2.P9_Description = "Code stuff";

			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedWorkflowTaskType = categorisedTaskTypes.AddNew();
			categorisedWorkflowTaskType.Code = "ORG";
			var taskType = categorisedWorkflowTaskType.TaskTypes.AddNew();
			taskType.Code = task1.P9_Type;
			taskType.IsCompletionStatementTaskType = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var completionStatementTaskType = WorkflowDataRegistry.Instance.TaskTypes.Value.GetTaskTypesFromWorkflowCode(jobHeader.Parent.WorkflowType)
				.Cast<WorkflowTaskType>()
				.FirstOrDefault(t => t.IsCompletionStatementTaskType);

			AssertNotNull(completionStatementTaskType);

			var collection = jobHeader.CompletionStatementTasksIncludingChildWorkflowTasks;
			AssertEquals(1, collection.Count);
			AssertEquals("Should load only COM task types", task1, collection[0]);
		}

		#endregion

		#region TypeDecider

		public void TestTypeDecider()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");

			AssertEquals(true, template.ProcessHeaders.AllowNew);

			var templateJobHeader = template.ProcessHeaders[0];
			var templateWorkflow = template.ProcessHeaders.AddNew();

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();

			AssertType<ProcessJobHeader>(newFactory.Load<ProcessHeader>(jobHeader.PK));
			AssertType<ProcessHeader>(newFactory.Load<ProcessHeader>(workflow.PK));

			AssertType<ProcessJobHeader>(newFactory.Load<ProcessHeader>(templateJobHeader.PK));
			AssertType<ProcessHeader>(newFactory.Load<ProcessHeader>(templateWorkflow.PK));
		}

		#endregion

		#region Parent

		public void TestParent_InvalidParentTableCode()
		{
			var header = Factory.NewWithValidTestData<ProcessJobHeader>();
			header.FH_ParentId = ZGuid.NewZGuid();
			header.FH_ParentTableCode = "XXX";
			AssertEquals(null, header.Parent);

			AssertEquals("Cannot determine the Busines object for TablePrefix 'XXX'", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestParent_ValidParentTableCode()
		{
			var header = Factory.NewWithValidTestData<ProcessJobHeader>();
			var dummyWithWorkflow = Factory.New<DummyWithWorkflow>();
			header = ProcessJobHeader.GetForParent(dummyWithWorkflow, Factory);
			AssertEquals(dummyWithWorkflow, header.Parent);
		}

		public void TestPropertiesWhichRelyOnParent_ShouldNotThrowExceptionsOnAccess()
		{
			var jobHeader = Factory.NewWithValidTestData<ProcessJobHeader>();
			AssertNull(jobHeader.Parent);

			CombineAssertions("The following properties threw exceptions on access when there is no Parent", () =>
			{
				foreach (var property in typeof(ProcessJobHeader).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
				{
					AssertNoExceptionThrown(property.Name, () => property.GetValue(jobHeader, null));
				}
			});
		}

		public void TestGetForParentOnDeletedBusinessObject_ShouldThrowDeveloperNotificationException()
		{
			var dummyWithWorkflow = Factory.New<DummyWithWorkflow>();
			dummyWithWorkflow.OverridenHumanReadableName = "Are we human? Or are we dancer?";
			dummyWithWorkflow.Delete();

			ProcessJobHeader.GetForParent(dummyWithWorkflow, Factory, checkTemplates: false);
			AssertMultilineASCIIEquals($@"Tried to add/initialise a job header on a job that has been deleted. That's madness.

Job type: DummyWithWorkflow
Job name/code: Are we human? Or are we dancer?
PK: {dummyWithWorkflow.PK}
Is in database: False
", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion

		#region System

		public void TestBMSystem()
		{
			var system1 = Factory.New<BMSystem>();
			var workflowType = system1.RelatedWorkflowTypes.AddNew();
			workflowType.FSW_WorkflowType = "XYZ";

			var system2 = Factory.New<BMSystem>();

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeader.GetForParent(dummy, Factory);

			AssertNull(jobHeader.BMSystem);

			var workflowType2 = system2.RelatedWorkflowTypes.AddNew();
			workflowType2.FSW_WorkflowType = "DUM";
			AssertEquals(system2, jobHeader.BMSystem);
		}

		#endregion

		#region Estimates / Actuals

		public void TestTotalEstimatedHoursSummaryAndOverallEstimateFactor()
		{
			var dummyWithWorkflow = Factory.New<DummyWithWorkflow>();

			var flowHeader = ProcessJobHeader.GetForParent(dummyWithWorkflow, Factory);
			AssertEquals("0.0 hrs to 0.0 hrs.", flowHeader.TotalEstimatedHoursSummary);
			AssertEquals(0m, flowHeader.OverallEstimateFactor);

			var task1 = dummyWithWorkflow.WorkflowItems.Tasks.AddNew();
			task1.P9_EstDuration = new ZDateTime(2012, 1, 1, 1, 30, 0);
			task1.P9_EstimateVariationFactor = 2;

			var task2 = dummyWithWorkflow.WorkflowItems.Tasks.AddNew();
			task2.P9_EstDuration = new ZDateTime(2012, 1, 1, 3, 0, 0);
			task2.P9_EstimateVariationFactor = 3;

			var task3 = dummyWithWorkflow.WorkflowItems.Tasks.AddNew();
			task3.P9_EstDuration = new ZDateTime(2012, 1, 1, 5, 30, 0);
			task3.P9_EstimateVariationFactor = 1;

			AssertEquals("10.0 hrs to 17.5 hrs.", flowHeader.TotalEstimatedHoursSummary);
			AssertEquals(1.75m, flowHeader.OverallEstimateFactor);

			var task4 = dummyWithWorkflow.WorkflowItems.Tasks.AddNew();
			AssertEquals("10.0 hrs to 17.5 hrs.", flowHeader.TotalEstimatedHoursSummary);
			AssertEquals(1.75m, flowHeader.OverallEstimateFactor);
		}

		public void TestImplicitDurationMinutes_ShouldEqualTotalEstimate()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			BMSTestHelper.CreateTask(workflow1, string.Empty, 60);
			BMSTestHelper.CreateTask(workflow2, string.Empty, 60);

			Factory.Save();

			AssertEquals(1.5m, workflow1.TotalRelevantEstimatedHours);
			AssertEquals("1:30", workflow1.TotalRelevantEstimatedHoursLabel);
			AssertEquals("1.5 hrs.", workflow1.TotalRelevantEstimatedHoursSummary);
			AssertEquals(1.5m, workflow2.TotalRelevantEstimatedHours);
			AssertEquals("1:30", workflow2.TotalRelevantEstimatedHoursLabel);
			AssertEquals("1.5 hrs.", workflow2.TotalRelevantEstimatedHoursSummary);

			var expectedImplicitDurationHours = workflow1.TotalRelevantEstimatedHours + workflow2.TotalRelevantEstimatedHours;
			AssertEquals(expectedImplicitDurationHours, jobHeader.ImplicitDurationHours);
		}

		public void TestTotalActualHours()
		{
			var dummyWithWorkflow = Factory.New<DummyWithWorkflow>();
			var flowHeader = ProcessJobHeader.GetForParent(dummyWithWorkflow, Factory);
			AssertEquals(0m, flowHeader.TotalActualHours);
			AssertEquals("0:00", flowHeader.TotalActualHoursLabel);
			AssertEquals("0.0 hrs.", flowHeader.TotalActualHoursSummary);

			var task1 = dummyWithWorkflow.WorkflowItems.Tasks.AddNew();
			task1.P9_ActualDuration = new ZDateTime(2012, 1, 1, 1, 30, 0);
			AssertEquals(1.5m, flowHeader.TotalActualHours);
			AssertEquals("1:30", flowHeader.TotalActualHoursLabel);
			AssertEquals("1.5 hrs.", flowHeader.TotalActualHoursSummary);

			var task2 = dummyWithWorkflow.WorkflowItems.Tasks.AddNew();
			task2.P9_ActualDuration = new ZDateTime(2012, 1, 1, 3, 45, 0);
			AssertEquals(5.25m, flowHeader.TotalActualHours);
			AssertEquals("5:15", flowHeader.TotalActualHoursLabel);
			AssertEquals("5.2 hrs.", flowHeader.TotalActualHoursSummary);

			var task3 = dummyWithWorkflow.WorkflowItems.Tasks.AddNew();
			AssertEquals(5.25m, flowHeader.TotalActualHours);
			AssertEquals("5:15", flowHeader.TotalActualHoursLabel);
			AssertEquals("5.2 hrs.", flowHeader.TotalActualHoursSummary);
		}

		#endregion

		#region Dependencies

		public void TestDependencyGraph()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			AssertEquals("1", jobHeader.DependencyGraph.GetSequence(workflow1));

			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var link = Factory.New<ProcessHeaderLink>();
			link.FP_FH_HeaderFrom = workflow2.PK;
			link.FP_FH_HeaderTo = jobHeader.ProcessHeaders[0].PK;
			link.FP_LinkType = "DEP";

			AssertEquals("1", jobHeader.DependencyGraph.GetSequence(workflow1));
			AssertEquals(string.Empty, jobHeader.DependencyGraph.GetSequence(workflow2));

			Factory.Save();
			AssertEquals("2", jobHeader.DependencyGraph.GetSequence(workflow1));
			AssertEquals("1", jobHeader.DependencyGraph.GetSequence(workflow2));
		}

		#endregion

		#region GetForParent

		public void TestGetForParent()
		{
			var dummyWithWorkflow = Factory.New<DummyWithWorkflow>();
			dummyWithWorkflow.HasChanges = false;

			var flowHeader = ProcessJobHeader.GetForParent(dummyWithWorkflow, Factory);

			AssertEquals(false, dummyWithWorkflow.HasChanges);
			AssertEquals(false, flowHeader.IsInDatabase);
			AssertEquals(false, flowHeader.HasChanges);
			AssertEquals(dummyWithWorkflow.PK, flowHeader.FH_ParentId);
			AssertEquals(dummyWithWorkflow.TablePrefix, flowHeader.FH_ParentTableCode);
			AssertEquals(true, dummyWithWorkflow.IsRegisteredEditableChildObject(flowHeader));

			Factory.Save();

			var flowHeaderNow = ProcessJobHeader.GetForParent(dummyWithWorkflow, Factory);
			AssertEquals(true, flowHeaderNow.IsInDatabase);
			AssertEquals(flowHeader, flowHeaderNow);
			AssertEquals(true, dummyWithWorkflow.IsRegisteredEditableChildObject(flowHeaderNow));
		}

		public void TestGetForParent_ShouldGetJobProcessHeaderNotChild()
		{
			var job1 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job1, Factory);
			AssertEquals(1, jobHeader.ProcessHeaders.Count);

			var workflow = jobHeader.ProcessHeaders[0];
			AssertEquals(job1.PK, workflow.FH_ParentId);
			AssertEquals(jobHeader.PK, workflow.FH_FH_ParentHeader);

			var job2 = Factory.NewWithValidTestData<OrgHeader>();
			jobHeader.FH_ParentId = job2.PK;

			Factory.Save();

			var loadedJobHeader = ProcessJobHeader.GetForParent(job1, Factory);
			AssertNotEquals("Should create a new ProcessJobHeader since the original ProcessHeader record is pointing at a new parent", jobHeader.PK, loadedJobHeader.PK);
			AssertNotEquals("Should NOT find the child workflow as it is not a top-level 'job' header.", workflow.PK, loadedJobHeader.PK);
		}

		public void TestGetForParent_WhenBMSDeactivated_ShouldNotAddDefaultWorkflow()
		{
			var job = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			AssertEquals("When BMS is enabled in the registry we should get a default workflow created", 1, jobHeader.ProcessHeaders.Count);

			BMSTestHelper.DisableBMSInRegistry();

			job = Factory.New<OrgHeader>();
			jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			AssertEquals("When BMS is disabled in the registry we should NOT get a default workflow created", 0, jobHeader.ProcessHeaders.Count);
		}

		#endregion

		#region Unique Index Handler

		public void TestSavingDuplicatedJobLevelWorkflows_ForWorkflowTemplate_ShouldReportStacktrace()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var workflow1 = BMSTestHelper.CreateWorkflow(template);
			var jobHeader = workflow1.JobHeader;

			AssertNotNull(jobHeader);

			Factory.Save();

			var anotherJobLevelWorkflow = CreateDuplicateJobLevelWorkflowForTemplate(template);

			var ex = AssertExceptionThrown<ZSaveException>(() => ZExceptionReporting.ProcessWithSaveExceptionHandling(Factory.Save, () => { }));
			AssertContains("Cannot insert duplicate key row in object 'dbo.ProcessHeader' with unique index 'NR_UX__FH_P0_Template'", ex.Message);

			AssertContains("There are duplicate job-level workflows. This form will need to be closed and re-opened to correct this problem.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContains("Cannot Save", UnitTestUserNotification.Instance.LastMessage.Caption);

			anotherJobLevelWorkflow.Delete();

			AssertNoExceptionThrown(() => ZExceptionReporting.ProcessWithSaveExceptionHandling(Factory.Save, () => { }));

			ErrorReporter.Clear();
		}

		public void TestSavingDuplicatedJobLevelWorkflows_ForOperationalJobs_ShouldReportStackTrace()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var job = (OrgHeader)BMSTestHelper.CreateJob<OrgHeader>(Factory);
			var jobHeader = CreateJobHeaderForOperationalJobInAnotherMethod(job);

			Factory.Save();

			var anotherJobLevelWorkflow = CreateDuplicateJobLevelWorkflowForJob(job, Factory);

			AssertNoExceptionThrown(() => ZExceptionReporting.ProcessWithSaveExceptionHandling(Factory.Save, () => { }));

			CombineAssertions("Since we are deleting one of the duplicate job-level workflows, there is no need to notify the user.", () =>
			{
				AssertNotContains("There are duplicate job-level workflows. This form will need to be closed and re-opened to correct this problem.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains("Cannot Save", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("The non-duplicate job header should not be deleted", false, jobHeader.IsDeleted);
			});

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertNotNull("The JLW should DEFINITELY still exist, otherwise we've done something seriously wrong", newFactory.Load<ProcessJobHeader>(jobHeader.PK));

			ErrorReporter.Clear();
		}

		public void TestSavingDuplicatedJobLevelWorkflows_ForOperationalJobs_ShouldReportStackTrace_WhenDuplicatedJobHeaderIsAlreadySavedButAnotherFactory()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var job = (OrgHeader)BMSTestHelper.CreateJob<OrgHeader>(Factory);
			var jobHeader = CreateJobHeaderForOperationalJobInAnotherMethod(job);

			Factory.Save();

			var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var anotherJobLevelWorkflow = CreateDuplicateJobLevelWorkflowForJob(job, anotherFactory);

			AssertNoExceptionThrown(() => ZExceptionReporting.ProcessWithSaveExceptionHandling(anotherFactory.Save, () => { }));

			CombineAssertions("Since we are deleting one of the duplicate job-level workflows, there is no need to notify the user.", () =>
			{
				AssertNotContains("There are duplicate job-level workflows. This form will need to be closed and re-opened to correct this problem.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotContains("Cannot Save", UnitTestUserNotification.Instance.LastMessage.Caption);
			});

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			AssertNotNull("The JLW should DEFINITELY still exist, otherwise we've done something seriously wrong", newFactory.Load<ProcessJobHeader>(jobHeader.PK));

			ErrorReporter.Clear();
		}

		ProcessJobHeader CreateJobHeaderForOperationalJobInAnotherMethod(OrgHeader job)
		{
			return BMSTestHelper.CreateJobHeader(job);
		}

		public static ProcessJobHeader CreateDuplicateJobLevelWorkflowForTemplate(ProcessTaskTemplate template)
		{
			var jobLevelWorkflow = template.Factory.New<ProcessJobHeader>();
			jobLevelWorkflow.FH_P0_Template = template.PK;
			jobLevelWorkflow.FH_CompletionStatement = @"
TONY ABBOTT IS NOT THE PRIME MINISTER AND HE'LL NEVER BE THE PRIME MINISTER AGAIN.
IF KEVIN RUDD WANTED TO BE PM AGAIN HE COULD PROBABLY GO OUT AND DO IT BECAUSE HE IS WHAT? SICKENING.
TONY ABBOTT WILL NEVER BE THE PM BECAUSE HE IS. NOT. THAT. KIND. OF. GIRL.
BABY EVERY TIME K RUDD WAS PM IT WAS BECAUSE HE WORKED FOR IT AND GOT IT HIMSELF.
HE BUILT HIMSELF FROM THE GROUND UP YOU F****** B****
*THROWS DRINK*
";

			return jobLevelWorkflow;
		}

		public static ProcessJobHeader CreateDuplicateJobLevelWorkflowForJob<T>(T job, BusinessObjectFactory factory)
			where T : BusinessObject, IWorkflowProvider
		{
			var jobLevelWorkflow = factory.New<ProcessJobHeader>();
			jobLevelWorkflow.FH_ParentId = job.PK;
			jobLevelWorkflow.FH_ParentTableCode = job.TablePrefix;
			jobLevelWorkflow.FH_CompletionStatement = "...Backrolls?";

			return jobLevelWorkflow;
		}

		#endregion

		#region Resource Strings

		public void TestResourceString_StaggeredReleaseTimeDelayInfo()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeader.GetForParent(dummy, Factory);

			var fullDescription = DataBoundResourceStrings.GetDataForProperty(jobHeader.StaggeredReleaseTimeDelayInfo).FullDescription;

			AssertEquals("The number of hours/minutes from release of previous workflows that need to be elapsed when determining a workflow’s Staggered Release Delay Expiry.", fullDescription);
		}

		#endregion

		#region Getting ProcessJobHeader for Parent

		public void TestGetForParentWithoutCreation_ShouldFetchFromLocalCacheOnly_WhenParentIsNotSaved()
		{
			AssertEquals(false, WorkflowDataRegistry.Instance.AlwaysCheckForExistingJobLevelWorkflowsInTheDatabaseWhenCreatingJobLevelWorkflows.Value);

			TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var job = (OrgHeader)BMSTestHelper.CreateJob<OrgHeader>(Factory);
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);

			Assert(!job.IsInDatabase);

			var query = ProcessJobHeader.GetJobHeaderQuery(job);
			AssertEquals(true, query.FetchOnlyFromLocalCache);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var result = newFactory.LoadTop1<ProcessJobHeader>(query);

			AssertNull("The new factory doesn't have the job-level workflow cached yet, and shouldn't be able to find it.", result);

			var expectedHits = new List<KeyValuePair<string, int>> { new KeyValuePair<string, int>(ProcessHeaderSchema.Constants.TableName, 0) };
			AssertDbHits(expectedHits, newFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true, ignoreHitsFromTablesCachedInUberFactory: true);
		}

		public void TestGetForParentWithoutCreation_ShouldFetchFromLocalCacheOnly_WhenParentIsNotSaved_ButFetchingFromDatabaseIsEnforcedInRegistry()
		{
			WorkflowDataRegistry.Instance.AlwaysCheckForExistingJobLevelWorkflowsInTheDatabaseWhenCreatingJobLevelWorkflows.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var job = (OrgHeader)BMSTestHelper.CreateJob<OrgHeader>(Factory);
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);

			Assert(!job.IsInDatabase);

			var query = ProcessJobHeader.GetJobHeaderQuery(job);
			AssertEquals("Even though checking the database is enabled in the registry, it makes no sense to do this for an unsaved workflow provider.", true, query.FetchOnlyFromLocalCache);
		}

		public void TestGetForParentWithoutCreation_ShouldFetchFromDatabase_WhenParentIsSaved()
		{
			AssertEquals(false, WorkflowDataRegistry.Instance.AlwaysCheckForExistingJobLevelWorkflowsInTheDatabaseWhenCreatingJobLevelWorkflows.Value);

			TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var job = (OrgHeader)BMSTestHelper.CreateJob<OrgHeader>(Factory);
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);

			Factory.Save();

			Assert(job.IsInDatabase);

			var query = ProcessJobHeader.GetJobHeaderQuery(job);
			AssertEquals(false, query.FetchOnlyFromLocalCache);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var result = newFactory.LoadTop1<ProcessJobHeader>(query);

			AssertNotNull("Should be found in the database", result);

			var expectedHits = new List<KeyValuePair<string, int>> { new KeyValuePair<string, int>(ProcessHeaderSchema.Constants.TableName, 1) };
			AssertDbHits(expectedHits, newFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true, ignoreHitsFromTablesCachedInUberFactory: true);

			var rowsUpdated = TestConnection.ExecuteScalar<int>($"UPDATE dbo.ProcessHeader SET FH_CompletionStatement = 'Changed' WHERE FH_PK = '{jobHeader.PK}'; SELECT @@ROWCOUNT");
			AssertEquals("The direct query should have updated the job-level workflow in the database.", 1, rowsUpdated);

			result = newFactory.LoadTop1<ProcessJobHeader>(query);
			AssertNotNull("Should be found again in but in the cache this time, default Factory behaviour.", result);
			AssertDbHits(expectedHits, newFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true, ignoreHitsFromTablesCachedInUberFactory: true);
			AssertNotEquals("The object should not be reloaded from the db because we didn't specify that was needed.", "Changed", result.FH_CompletionStatement);
		}

		public void TestGetForTemplate_ShouldReloadObjectsEvenWhenAlreadyCached()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var job = Factory.New<DummyWithWorkflow>();
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM");
			Factory.Save();

			var workflows = Factory.Load<ProcessHeader>(new ZQuery(ProcessHeaderSchema.FH_ParentId, job.PK));
			AssertContainsExactElementsInAnyOrder("There shouldn't be any workflows in the database.", Array.Empty<string>(), workflows.Select(x => x.FH_CompletionStatement));

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var query = ProcessJobHeader.GetJobHeaderQuery(job);
			var result = newFactory.LoadTop1<ProcessJobHeader>(query); // will cache the query and result (null) in the factory

			AssertNull("No job-level workflow should be found in the database.", result);

			var jobLevelWorkflowPk = ZGuid.NewZGuid();
			TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.ProcessHeader (FH_PK, FH_CompletionStatement, FH_ParentId, FH_ParentTableCode, FH_FH_ParentHeader, FH_WorkflowType, FH_SystemCreateTimeUtc, FH_SystemLastEditTimeUtc, FH_SystemCreateUser, FH_SystemLastEditUser)
VALUES ('{jobLevelWorkflowPk}', 'Sneaky JLW', '{job.PK}', 'Z0', null, 'DUM', GETDATE(), GETDATE(), '~BP', '~BP'),
(NEWID(), 'Workflow needed to avoid renaming JLW on load', '{job.PK}', 'Z0', '{jobLevelWorkflowPk}', 'DUM', GETDATE(), GETDATE(), '~BP', '~BP')");

			var loadedTemplate = newFactory.Load<ProcessTaskTemplate>(template.PK);
			result = ProcessJobHeader.GetForTemplate(loadedTemplate, job);

			AssertEquals("GetForTemplate should check the database if a result isn't found in the cache, even if its query is already cached with no result because a row may have been added by another user, and failure to do this check can result in multiple job-level workflows being added.",
				"Sneaky JLW", result.FH_CompletionStatement);

			var rowsUpdated = TestConnection.ExecuteScalar<int>($"UPDATE dbo.ProcessHeader SET FH_CompletionStatement = 'Changed' WHERE FH_PK = '{result.PK}'; SELECT @@ROWCOUNT");
			AssertEquals("The direct query should have updated the job-level workflow in the database.", 1, rowsUpdated);

			result = ProcessJobHeader.GetForTemplate(loadedTemplate, job);
			AssertEquals("We should only go back to the database if the query isn't cached yet or if the cached result is null. As soon as we have a job-level workflow in the factory cache we can stop doing db hits.", "Sneaky JLW", result.FH_CompletionStatement);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return ProcessJobHeader.GetForParent(factory.NewWithValidTestData<DummyWithWorkflow>(), factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
		}

		class BadDummyWithPropertyWithSideEffects : DummyWithWorkflow
		{
			public BadDummyWithPropertyWithSideEffects(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString BadProperty
			{
				get
				{
					if (!wasPropertyAlreadyAccessed)
					{
						wasPropertyAlreadyAccessed = true;

						return "I don't see why it would be Russia";
					}
					else
					{
						return "I don't see why it wouldn't be Russia";
					}
				}
			}

			bool wasPropertyAlreadyAccessed;
		}

		#endregion
	}
}
