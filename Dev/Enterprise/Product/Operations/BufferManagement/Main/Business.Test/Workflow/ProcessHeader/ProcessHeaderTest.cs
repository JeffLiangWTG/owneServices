using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalCopy.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ProcessHeader))]
	public class ProcessHeaderTest : EnterpriseBusinessObjectTestCase
	{
		#region Circular Dependency

		public void TestIsAncestorOf_CircularDependency()
		{
			var processHeaders = BMSTestHelper.CreateCircularDependency(Factory, TestConnection);
			var processHeader1 = processHeaders.First();

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflowX = BMSTestHelper.CreateWorkflow(jobHeader, "workflowX");

			AssertNullOrEmpty(ErrorReporter.LastMessageReported);

			AssertEquals(
				"GIVEN circular-dependency between processheader1 and processheader2, WHEN calling processHeader1.IsAncestorOf(processHeaderX) THEN should return false",
				false,
				processHeader1.IsAncestorOf(workflowX)
			);
		}

		public void TestGetWorkflowParents_CircularDependency()
		{
			var processHeaders = BMSTestHelper.CreateCircularDependency(Factory, TestConnection);
			var processHeader1 = processHeaders.First();
			var processHeader2 = processHeaders.Skip(1).First();

			AssertNullOrEmpty(ErrorReporter.LastMessageReported);

			AssertContainsExactElementsInAnyOrder(
				"GIVEN circular-dependency between processheader1 and processheader2, WHEN calling processHeader1.GetWorkflowParents THEN should just return processHeader2",
				new[] { processHeader2.PK },
				processHeader1.GetWorkflowParents().Select(workflow => workflow.PK)
			);
		}

		public void TestApprovedShape_CircularDependency()
		{
			var processHeaders = BMSTestHelper.CreateCircularDependency(Factory, TestConnection);
			var processHeader1 = processHeaders.First();

			AssertNullOrEmpty(ErrorReporter.LastMessageReported);

			AssertNull(
				"GIVEN circular-dependency between processheader1 and processheader2, WHEN calling processHeader1.ApprovedShape THEN should return NULL",
				processHeader1.ApprovedShape);
		}

		public void TestGetParentsUpTheHierarchy_CircularDependency()
		{
			var processHeaders = BMSTestHelper.CreateCircularDependency(Factory, TestConnection);
			var processHeader1 = processHeaders.First();
			var processHeader2 = processHeaders.Skip(1).First();

			AssertNullOrEmpty(ErrorReporter.LastMessageReported);

			AssertContainsExactElementsInAnyOrder(
				"GIVEN circular-dependency between processheader1 and processheader2, WHEN calling processHeader1.GetParentsUpTheHierarchy THEN should just return processHeader2",
				new[] { processHeader2.PK },
				processHeader1.GetParentsUpTheHierarchy().Select(p => p.ProcessHeader.PK)
			);
		}

		public void TestGetIsOpen_CircularDependency()
		{
			var processHeaders = BMSTestHelper.CreateCircularDependency(Factory, TestConnection);
			var processHeader1 = processHeaders.First();
			var processHeader2 = processHeader1;
			var jobHeader = processHeader1.JobHeader;

			AssertNullOrEmpty(ErrorReporter.LastMessageReported);

			// make modification to trigger GetIsOpen call
			jobHeader.FH_CompletionStatement = "test";
			jobHeader.ProcessHeaders[0].FH_CompletionStatement = "workflow1a";
			jobHeader.ProcessHeaders[1].FH_CompletionStatement = "workflow2a";

			AssertEquals("GIVEN circular-dependency between processheader1 and processheader2, WHEN calling processHeader1.JobHeader.IsOpen THEN should return false",
				false,
				processHeader1.JobHeader.IsOpen);
		}

		#endregion

		#region Release Sequencing

		public void TestNudge()
		{
			var header = Factory.NewWithValidTestData<ProcessHeader>();
			header.NudgeUp();
			AssertEquals("NUDGE UP TO 1", header.Logs.GetAllLogs()[0].SL_Reference);
			AssertEquals((ZShort)1, header.FH_VoteUpDownAmount);
			header.NudgeDown();
			AssertEquals("NUDGE DOWN TO 0", header.Logs.GetAllLogs()[1].SL_Reference);
			AssertEquals((ZShort)0, header.FH_VoteUpDownAmount);
		}

		public void TestEffectiveNudge()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "DAN");
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup, "DAN", "This test was written by Daniel", nudge: 20);
			var tag2 = BMSTestHelper.CreateTagMagnitude(tagGroup, "DAV", "This test was written by Dave", nudge: 10);

			var workflow = BMSTestHelper.CreateWorkflowAndTask(Factory, "Workflow");
			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			var jobHeader = workflow.JobHeader;

			jobHeader.FH_VoteUpDownAmount = 2;
			workflow.FH_VoteUpDownAmount = 1;

			jobHeader.AddTag(tag1);
			workflow.AddTag(tag2);

			AssertEquals(22m, jobHeader.EffectiveNudge);
			AssertEquals(33m, workflow.EffectiveNudge);

			AssertEquals(0m, jobHeader.FH_EffectiveNudge);
			AssertEquals(0m, workflow.FH_EffectiveNudge);
			jobHeader.UpdateEffectiveNudge();
			AssertEquals(33m, workflow.FH_EffectiveNudge);
			AssertEquals("We don't need effective nudge on JLWs for release gate, therefore persistent effective nudge on JLW should always be zero", 0m, jobHeader.FH_EffectiveNudge);
		}

		public void TestEffectiveNudge_ForClosedWorkflows()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			var workflow = BMSTestHelper.CreateWorkflowAndTask(Factory, "Workflow");
			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			workflow.CancelAllTasksAndCompletionStatements("Because we need a closed workflow");
			workflow.FH_VoteUpDownAmount = 10;

			Factory.Save();

			workflow.Reload();
			AssertEquals("Precondition", WorkflowStatusList.Codes.Closed, workflow.FH_Status);

			AssertEquals(10m, workflow.EffectiveNudge);

			AssertEquals(0m, workflow.FH_EffectiveNudge);
			workflow.UpdateEffectiveNudge();
			AssertEquals("Persistent effective nudge should be zero on closed workflows", 0m, workflow.FH_EffectiveNudge);

			BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			workflow.Reload();
			AssertEquals("Precondition", WorkflowStatusList.Codes.Open, workflow.FH_Status);

			workflow.UpdateEffectiveNudge();
			AssertEquals(10m, workflow.FH_EffectiveNudge);
		}

		public void TestEffectiveNudge_ForClosedWorkflowsWithOpenPrerequisites()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			var workflow = BMSTestHelper.CreateWorkflowAndTask(Factory, "Workflow");
			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			workflow.CancelAllTasksAndCompletionStatements("Because we need a closed workflow");
			workflow.FH_VoteUpDownAmount = 10;

			var prereqWorkflow = BMSTestHelper.CreateWorkflowAndTask(Factory, "Prereq");
			prereqWorkflow.MakePrerequisiteOf(workflow);

			Factory.Save();

			workflow.Reload();
			AssertEquals("Precondition", WorkflowStatusList.Codes.ClosedWithOpenPrerequisites, workflow.FH_Status);

			AssertEquals(10m, workflow.EffectiveNudge);

			AssertEquals(0m, workflow.FH_EffectiveNudge);
			workflow.UpdateEffectiveNudge();
			AssertEquals("Persistent effective nudge should be zero on closed workflows (with open prerequisites)", 0m, workflow.FH_EffectiveNudge);

			BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			workflow.Reload();
			AssertEquals("Precondition", WorkflowStatusList.Codes.Blocked, workflow.FH_Status);

			workflow.UpdateEffectiveNudge();
			AssertEquals(10m, workflow.FH_EffectiveNudge);
		}

		public void TestEffectiveNudge_ForWorkflowsWithNoDedicatedBufferSet()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			var workflow = BMSTestHelper.CreateWorkflowAndTask(Factory, "Workflow");
			workflow.FH_VoteUpDownAmount = 10;

			AssertEquals(10m, workflow.EffectiveNudge);

			AssertEquals(0m, workflow.FH_EffectiveNudge);
			workflow.UpdateEffectiveNudge();
			AssertEquals("Persistent effective nudge should be zero on workflows with no dedicated buffer set", 0m, workflow.FH_EffectiveNudge);

			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			workflow.UpdateEffectiveNudge();
			AssertEquals(10m, workflow.FH_EffectiveNudge);
		}

		public void TestEffectiveNudge_ShouldUseLinkMagnitude()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "DAV");
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup, "DAN", "This test was written by Daniel", nudge: 20);
			var tag2 = BMSTestHelper.CreateTagMagnitude(tagGroup, "DAV", "This test was written by Dave", nudge: 30);
			var tag3 = BMSTestHelper.CreateTagMagnitude(tagGroup, "DAV", "This test was written by Alex", nudge: 40);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Ah HA! Of all the nudges, by far the biggest.");

			var link1 = workflow.AddTag(tag1).Link;
			var link2 = workflow.AddTag(tag2).Link;
			var link3 = jobHeader.AddTag(tag3).Link;

			AssertEquals("Should inherit tag nudge of job header", 90m, workflow.EffectiveNudge);
			AssertEquals(40m, jobHeader.EffectiveNudge);

			link3.TGL_Magnitude = 0.0m;

			AssertEquals(50m, workflow.EffectiveNudge);
			AssertEquals(0m, jobHeader.EffectiveNudge);

			link3.TGL_Magnitude = 1.5m;

			AssertEquals("A magnitude of 1.5 applies to tag inherited from job", 110m, workflow.EffectiveNudge);
			AssertEquals("A magnitude of 1.5 applies", 60m, jobHeader.EffectiveNudge);

			link1.TGL_Magnitude = 2.0m;

			AssertEquals("A magnitude of 1.5 applies to tag inherited from job, and 2.0 from tag1", 130m, workflow.EffectiveNudge);
			AssertEquals("A magnitude of 1.5 applies", 60m, jobHeader.EffectiveNudge);
		}

		public void TestEffectiveNudge_ShouldInheritNudgeFromJobLevelWorkflow()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(Factory, "Workflow1");
			workflow1.FH_FC_DedicatedBuffer = buffer.PK;
			var jobHeader1 = workflow1.JobHeader;

			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(Factory, "Workflow2");
			workflow2.FH_FC_DedicatedBuffer = buffer.PK;
			var jobHeader2 = workflow2.JobHeader;

			workflow1.FH_VoteUpDownAmount = 1;

			AssertEquals(1m, workflow1.EffectiveNudge);

			AssertEquals(0m, workflow1.FH_EffectiveNudge);
			workflow1.UpdateEffectiveNudge();
			AssertEquals(1m, workflow1.FH_EffectiveNudge);

			AssertArrayEqualsByElements(new[] { workflow1, workflow2 }, WorkflowTransferOrderSorter.Sort(new[] { workflow1, workflow2 }).ToArray());

			jobHeader2.FH_VoteUpDownAmount = 2;

			AssertEquals(2m, workflow2.EffectiveNudge);

			AssertEquals(0m, workflow2.FH_EffectiveNudge);
			workflow2.UpdateEffectiveNudge();
			AssertEquals(2m, workflow2.FH_EffectiveNudge);

			AssertArrayEqualsByElements(new[] { workflow2, workflow1 }, WorkflowTransferOrderSorter.Sort(new[] { workflow1, workflow2 }).ToArray());
		}

		public void TestEffectiveNudge_ShouldPersistInDatabase()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			var workflow = BMSTestHelper.CreateWorkflowAndTask(Factory, "Workflow");
			workflow.FH_FC_DedicatedBuffer = buffer.PK;

			AssertEquals("Precondition", 0m, workflow.FH_EffectiveNudge);

			workflow.FH_VoteUpDownAmount = 1;
			workflow.UpdateEffectiveNudge();

			Factory.Save();

			var sql = $"SELECT FH_EffectiveNudge FROM dbo.ProcessHeader WHERE FH_PK = '{workflow.PK}'";
			var nudge = TestConnection.ExecuteScalar<decimal>(sql);
			AssertEquals(1m, nudge);
		}

		public void TestEffectiveNudge_WhenWorkflowAndJobLevelWorkflowCombinedExceedShortMaxValue_ShouldNotThrowException()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			var workflow = BMSTestHelper.CreateWorkflowAndTask(Factory, "Workflow");
			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			workflow.JobHeader.FH_VoteUpDownAmount = 30_000;
			workflow.FH_VoteUpDownAmount = 30_000;

			var effectiveNudge = 0m;

			AssertNoExceptionThrown("Accessing effective nudge when both workflow and job-level workflow would overflow a short should not throw an exception. SAD!", () => effectiveNudge = workflow.EffectiveNudge);
			AssertEquals(60_000m, effectiveNudge);

			AssertEquals(0m, workflow.FH_EffectiveNudge);
			workflow.UpdateEffectiveNudge();
			AssertEquals(60_000m, workflow.FH_EffectiveNudge);
		}

		public void TestEffectiveNudge_ForNewTagLinkRecord()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var tagLink = BMSTestHelper.CreateTagLink(jobHeader);

			AssertEquals(0m, jobHeader.EffectiveNudge);
		}

		public void TestEffectiveNudge_OnWorkflow_ForNewTagLinkRecordOnJobHeader()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			var tagLink = BMSTestHelper.CreateTagLink(jobHeader);

			AssertEquals(0m, jobHeader.EffectiveNudge);
			AssertEquals(0m, workflow.EffectiveNudge);
		}

		public void TestEffectiveNudge_OnWorkflow_ExclusiveTagOnTheJobHeader()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			var definition = BMSTestHelper.CreateTagDefinition(Factory, "MMM", isExclusive: true);
			var magnitude = BMSTestHelper.CreateTagMagnitude(definition, "MMM", nudge: 10);
			var tagLink = BMSTestHelper.CreateTagLink(jobHeader, magnitude);

			AssertEquals(10m, jobHeader.EffectiveNudge);
			AssertEquals(10m, workflow.EffectiveNudge);
		}

		public void TestEffectiveNudge_WhenWorkflowLinkedToReleaseSequence_WithTagsApplied_JobLinkedToSequence()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "DAN");
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup, "DAN", "This test was written by Daniel", nudge: 20);
			var tag2 = BMSTestHelper.CreateTagMagnitude(tagGroup, "DAV", "This test was written by Dave", nudge: 10);

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "group");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			jobHeader.FH_VoteUpDownAmount = 2;
			workflow.FH_VoteUpDownAmount = 1;

			jobHeader.AddTag(tag1);
			workflow.AddTag(tag2);

			var sequence = BMSTestHelper.CreateReleaseSequence(Factory, releaseGroup.PK, nudge: 2000);
			BMSTestHelper.CreateReleaseSequenceItem(sequence, jobHeader, position: 2);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedJobHeader = newFactory.Load<ProcessJobHeader>(jobHeader.PK);
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);

			AssertEquals("Precondition", false, BMSRegistry.Instance.ReleaseSequencesModuleEnabled.Value);

			CombineAssertions("Since the release sequence module is not enabled, the effective nudge values should be unchanged", () =>
			{
				AssertEquals(22m, loadedJobHeader.EffectiveNudge);
				AssertEquals(33m, loadedWorkflow.EffectiveNudge);
			});

			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.ReleaseSequenceDefaultNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000);

			CombineAssertions("Effective nudge with release sequencing enabled", () =>
			{
				AssertEquals(1999.9999m, loadedJobHeader.EffectiveNudge);
				AssertEquals(1999.9999m, loadedWorkflow.EffectiveNudge);
			});

			loadedJobHeader.FH_VoteUpDownAmount = 2000;
			loadedWorkflow.FH_VoteUpDownAmount = 1000;

			CombineAssertions("Should be taking the default non-sequenced nudge when default nudge exceeds sequenced nudge", () =>
			{
				AssertEquals(2020m, loadedJobHeader.EffectiveNudge);
				AssertEquals(3030m, loadedWorkflow.EffectiveNudge);
			});
		}

		public void TestEffectiveNudge_WhenWorkflowLinkedToReleaseSequence_NudgeDefaultsToRegistry_WhenNudgeNotSpecifiedInSequence()
		{
			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.ReleaseSequenceDefaultNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000);

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "group");

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "DAN");
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup, "DAN", "This test was written by Daniel", nudge: 20);
			var tag2 = BMSTestHelper.CreateTagMagnitude(tagGroup, "DAV", "This test was written by Dave", nudge: 10);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			jobHeader.FH_VoteUpDownAmount = 2;
			workflow.FH_VoteUpDownAmount = 1;

			jobHeader.AddTag(tag1);
			workflow.AddTag(tag2);

			var sequence = BMSTestHelper.CreateReleaseSequence(Factory, releaseGroup.PK);
			var item = BMSTestHelper.CreateReleaseSequenceItem(sequence, workflow, position: 2);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedJobHeader = newFactory.Load<ProcessJobHeader>(jobHeader.PK);
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);

			CombineAssertions(() =>
			{
				AssertEquals("jobheader", 22m, loadedJobHeader.EffectiveNudge);
				AssertEquals("workflow", 999.9999m, loadedWorkflow.EffectiveNudge);
			});
		}

		public void TestEffectiveNudge_WhenWorkflowLinkedToReleaseSequence_TagsApplied_WorkflowLinkedToSequence()
		{
			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.ReleaseSequenceDefaultNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000);

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "group");

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "DAN");
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup, "DAN", "This test was written by Daniel", nudge: 20);
			var tag2 = BMSTestHelper.CreateTagMagnitude(tagGroup, "DAV", "This test was written by Dave", nudge: 10);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			jobHeader.FH_VoteUpDownAmount = 2;
			workflow.FH_VoteUpDownAmount = 1;

			jobHeader.AddTag(tag1);
			workflow.AddTag(tag2);

			var sequence = BMSTestHelper.CreateReleaseSequence(Factory, releaseGroup.PK, nudge: 2000);
			var item = BMSTestHelper.CreateReleaseSequenceItem(sequence, workflow, position: 2);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedJobHeader = newFactory.Load<ProcessJobHeader>(jobHeader.PK);
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);

			CombineAssertions(() =>
			{
				AssertEquals("jobheader", 22m, loadedJobHeader.EffectiveNudge);
				AssertEquals("workflow", 1999.9999m, loadedWorkflow.EffectiveNudge);
			});
		}

		public void TestEffectiveNudge_WhenWorkflowLinkedToReleaseSequence_NoTagsApplied_JobLinkedToSequence()
		{
			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.ReleaseSequenceDefaultNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000);

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "group");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			jobHeader.FH_VoteUpDownAmount = 2;
			workflow.FH_VoteUpDownAmount = 1;

			var sequence = BMSTestHelper.CreateReleaseSequence(Factory, releaseGroup.PK, nudge: 2000);
			var item = BMSTestHelper.CreateReleaseSequenceItem(sequence, jobHeader, position: 2);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedJobHeader = newFactory.Load<ProcessJobHeader>(jobHeader.PK);
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);

			CombineAssertions(() =>
			{
				AssertEquals("jobheader", 1999.9999m, loadedJobHeader.EffectiveNudge);
				AssertEquals("workflow", 1999.9999m, loadedWorkflow.EffectiveNudge);
			});
		}

		public void TestEffectiveNudge_WhenWorkflowLinkedToReleaseSequence_NoTagsApplied_WorkflowLinkedToSequence()
		{
			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.ReleaseSequenceDefaultNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000);

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "group");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			jobHeader.FH_VoteUpDownAmount = 2;
			workflow.FH_VoteUpDownAmount = 1;

			var sequence = BMSTestHelper.CreateReleaseSequence(Factory, releaseGroup.PK, nudge: 2000);
			var item = BMSTestHelper.CreateReleaseSequenceItem(sequence, workflow, position: 2);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedJobHeader = newFactory.Load<ProcessJobHeader>(jobHeader.PK);
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);

			CombineAssertions(() =>
			{
				AssertEquals("jobheader", 2m, loadedJobHeader.EffectiveNudge);
				AssertEquals("workflow", 1999.9999m, loadedWorkflow.EffectiveNudge);
			});
		}

		public void TestEffectiveNudgeWithOneLevelLinks()
		{
			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			int total = 7;

			var jobHeaders = new List<ProcessJobHeader>();
			var workflows = new List<ProcessHeader>();

			for (int i = 1; i <= total; i++)
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

				var jobHeader = ProcessJobHeader.GetForParent(orgHeader, Factory);
				jobHeader.Name = "JH" + i;
				var workflow1 = jobHeader.ProcessHeaders.AddNew();
				workflow1.Name = "JH" + i + "_W1";
				var workflow2 = jobHeader.ProcessHeaders.AddNew();
				workflow2.Name = "JH" + i + "_W2";

				var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();

				var releaseSequence = Factory.NewWithValidTestData<BMReleaseSequence>();
				releaseSequence.BMR_SequenceNudge = (total - i + 1) * 1000;
				releaseSequence.BMR_GG_ReleaseGroup = releaseGroup.PK;

				var releaseSequenceItem = Factory.NewWithValidTestData<BMReleaseSequenceItem>();
				releaseSequenceItem.BMI_Position = 2;
				releaseSequenceItem.BMI_FH_ProcessHeader = jobHeader.PK;
				releaseSequenceItem.BMI_BMR_Sequence = releaseSequence.PK;

				jobHeaders.Add(jobHeader);
				workflows.Add(workflow1);
				workflows.Add(workflow2);
			}

			for (int i = 1; i <= total - 1; i++)
			{
				var link = Factory.NewWithValidTestData<ProcessHeaderLink>();
				link.FP_FH_HeaderTo = workflows.First(h => h.Name == "JH" + i + "_W1").PK;
				link.FP_FH_HeaderFrom = workflows.First(h => h.Name == "JH" + (i + 1) + "_W2").PK;
				link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;
			}

			CombineAssertions(() =>
			{
				foreach (var jobHeader in jobHeaders)
				{
					int index = int.Parse(jobHeader.Name.Substring(2, 1));
					AssertEquals(jobHeader.Name + ": Each Job Header should follow the release sequence nudge", (decimal)((total - index + 1) * 1000 - 0.0001), jobHeader.EffectiveNudge);
				}

				foreach (var workflow in workflows)
				{
					int index = int.Parse(workflow.Name.Substring(2, 1));

					if (workflow.Name.EndsWith("W1"))
					{
						AssertEquals(workflow.Name + ": Each 1st Workflow should follow the release sequence nudge", (decimal)((total - index + 1) * 1000 - 0.0001), workflow.EffectiveNudge);
					}
					else
					{
						AssertEquals(workflow.Name + ": Each 2nd Workflow should inherit the higher parent sequence nudge (except for the first workflow)", (decimal)((total - index + (index == 1 ? 1 : 2)) * 1000 - 0.0001), workflow.EffectiveNudge);
					}
				}
			});
		}

		public void TestEffectiveNudgeWithMultiLevelLinks()
		{
			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			int total = 7;

			var jobHeaders = new List<ProcessJobHeader>();
			var workflows = new List<ProcessHeader>();

			for (int i = 1; i <= total; i++)
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

				var jobHeader = ProcessJobHeader.GetForParent(orgHeader, Factory);
				jobHeader.Name = "JH" + i;
				var workflow = jobHeader.ProcessHeaders.AddNew();
				workflow.Name = "JH" + i + "_W";

				var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();

				var releaseSequence = Factory.NewWithValidTestData<BMReleaseSequence>();
				releaseSequence.BMR_SequenceNudge = (total - i + 1) * 1000;
				releaseSequence.BMR_GG_ReleaseGroup = releaseGroup.PK;

				var releaseSequenceItem = Factory.NewWithValidTestData<BMReleaseSequenceItem>();
				releaseSequenceItem.BMI_Position = 2;
				releaseSequenceItem.BMI_FH_ProcessHeader = jobHeader.PK;
				releaseSequenceItem.BMI_BMR_Sequence = releaseSequence.PK;

				jobHeaders.Add(jobHeader);
				workflows.Add(workflow);
			}

			for (int i = 1; i <= total - 1; i++)
			{
				var link = Factory.NewWithValidTestData<ProcessHeaderLink>();
				link.FP_FH_HeaderTo = jobHeaders.First(h => h.Name == "JH" + i).PK;
				link.FP_FH_HeaderFrom = jobHeaders.First(h => h.Name == "JH" + (i + 1)).PK;
				link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.ParentChild;
			}

			CombineAssertions(() =>
			{
				foreach (var jobHeader in jobHeaders)
				{
					int index = int.Parse(jobHeader.Name.Substring(2, 1));
					AssertEquals(jobHeader.Name + ": Each Job Header should inherit the highest release sequence nudge (but no further than 5 levels up)", jobHeader.Name == ("JH" + total) ? 5999.9999m : 6999.9999m, jobHeader.EffectiveNudge);
				}

				foreach (var workflow in workflows)
				{
					int index = int.Parse(workflow.Name.Substring(2, 1));
					AssertEquals(workflow.Name + ": Each Workflow should follow the jobHeader sequence nudge", workflow.Name.StartsWith("JH" + total) ? 5999.9999m : 6999.9999m, workflow.EffectiveNudge);
				}
			});
		}

		public void TestEffectiveNudge_ShouldNotReceiveSequenceNudge_WhenReleaseSequenceIsInActive()
		{
			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var processJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = processJobHeader.ProcessHeaders[0];
			workflow.FH_VoteUpDownAmount = 2;

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "group");

			var sequence = BMSTestHelper.CreateReleaseSequence(Factory, releaseGroup.PK, nudge: 3600);
			var sequenceItem = BMSTestHelper.CreateReleaseSequenceItem(sequence, workflow);
			sequence.BMR_IsActive = ZBool.False;

			Factory.Save();

			AssertEquals("Sequence nudge should not be applied to workflow as it is inactive", 2m, workflow.EffectiveNudge);

			sequence.BMR_IsActive = ZBool.True;

			Factory.Save();

			AssertEquals("Sequence nudge should not be applied to workflow as it is inactive", 3600m, workflow.EffectiveNudge);
		}

		[TestDate(2014, 7, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestReleaseSequenceSortDate()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			AssertEquals(ZDateTime.MaxSmallDateTime, workflow.ReleaseSequenceSortDate);

			workflow.FH_ReleaseDateTime = new ZDateTime(2014, 7, 1);
			AssertEquals(new ZDateTime(2014, 7, 1), workflow.ReleaseSequenceSortDate);

			jobHeader.FH_DoNotStartBeforeDate = new ZDateTime(2014, 7, 2);
			AssertEquals(new ZDateTime(2014, 7, 2), workflow.ReleaseSequenceSortDate);

			workflow.FH_DoNotStartBeforeDate = new ZDateTime(2014, 7, 3);
			AssertEquals(new ZDateTime(2014, 7, 3), workflow.ReleaseSequenceSortDate);
		}

		[TestDate(2014, 7, 1)]
		[TestUtcOffset(10, 0, 0)]
		public void TestReleaseSequenceSortDateUtc()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			var workflow = BMSTestHelper.CreateWorkflowAndTask(Factory, "Workflow");
			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			Assert("Precondition", workflow.CanCalculateSparsePropertiesForNewReleaseGate);

			CombineAssertions("FH_ReleaseSequenceSortDateUtc is calculated correctly", () =>
			{
				AssertEquals(new ZDateTime(2014, 7, 1), workflow.FH_ReleaseSequenceSortDateUtc);

				workflow.FH_ReleaseDateTime = new ZDateTime(2014, 7, 2);
				AssertEquals(new ZDateTime(2014, 7, 2), workflow.FH_ReleaseSequenceSortDateUtc);

				workflow.JobHeader.FH_DoNotStartBeforeDate = new ZDateTime(2014, 7, 3);
				AssertEquals(new ZDateTime(2014, 7, 3), workflow.FH_ReleaseSequenceSortDateUtc);

				workflow.FH_DoNotStartBeforeDate = new ZDateTime(2014, 7, 4);
				AssertEquals(new ZDateTime(2014, 7, 4), workflow.FH_ReleaseSequenceSortDateUtc);
			});

			CombineAssertions("Reacts to FH_IsActive", () =>
			{
				workflow.FH_IsActive = false;
				AssertEquals(ZDateTime.Empty, workflow.FH_ReleaseSequenceSortDateUtc);

				workflow.FH_IsActive = true;
				AssertEquals(new ZDateTime(2014, 7, 4), workflow.FH_ReleaseSequenceSortDateUtc);
			});

			CombineAssertions("Reacts to FH_Status", () =>
			{
				workflow.TaskCollection[0].P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				workflow.FH_Status = WorkflowStatusList.Codes.Closed;
				AssertEquals(ZDateTime.Empty, workflow.FH_ReleaseSequenceSortDateUtc);

				workflow.TaskCollection[0].P9_Status = ProcessTaskStatusCodeList.Codes.Open;
				workflow.FH_Status = WorkflowStatusList.Codes.Open;
				AssertEquals(new ZDateTime(2014, 7, 4), workflow.FH_ReleaseSequenceSortDateUtc);
			});

			CombineAssertions("Reacts to FH_FC_DedicatedBuffer", () =>
			{
				workflow.FH_FC_DedicatedBuffer = Guid.Empty;
				AssertEquals(ZDateTime.Empty, workflow.FH_ReleaseSequenceSortDateUtc);

				workflow.FH_FC_DedicatedBuffer = buffer.PK;
				AssertEquals(new ZDateTime(2014, 7, 4), workflow.FH_ReleaseSequenceSortDateUtc);
			});
		}

		public void TestReleaseSequenceName_OnJobLevelWorkflow()
		{
			BMSRegistry.Instance.ReleaseSequenceDefaultNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000);

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "group");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			var sequence = BMSTestHelper.CreateReleaseSequence(Factory, releaseGroup.PK, nudge: 2000);
			var item = BMSTestHelper.CreateReleaseSequenceItem(sequence, jobHeader);

			AssertEquals("Precondition", false, BMSRegistry.Instance.ReleaseSequencesModuleEnabled.Value);

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, jobHeader.ReleaseSequenceName);
				AssertEquals(ZString.Empty, workflow.ReleaseSequenceName);
			});

			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			CombineAssertions(() =>
			{
				AssertEquals("workflow", "Skywalker Saga", workflow.ReleaseSequenceName);
				AssertEquals("jobheader", "Skywalker Saga", jobHeader.ReleaseSequenceName);
			});
		}

		public void TestReleaseSequenceName_OnWorkflow()
		{
			BMSRegistry.Instance.ReleaseSequenceDefaultNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000);

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "group");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			var sequence = BMSTestHelper.CreateReleaseSequence(Factory, releaseGroup.PK, nudge: 2000);
			var item = BMSTestHelper.CreateReleaseSequenceItem(sequence, workflow);

			AssertEquals("Precondition", false, BMSRegistry.Instance.ReleaseSequencesModuleEnabled.Value);

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, jobHeader.ReleaseSequenceName);
				AssertEquals(ZString.Empty, workflow.ReleaseSequenceName);
			});

			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			CombineAssertions(() =>
			{
				AssertEquals("workflow", "Skywalker Saga", workflow.ReleaseSequenceName);
				AssertEquals("jobheader", ZString.Empty, jobHeader.ReleaseSequenceName);
			});
		}

		public void TestReleaseSequencePosition_OnJobLevelWorkflow()
		{
			BMSRegistry.Instance.ReleaseSequenceDefaultNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000);

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "group");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			var sequence = BMSTestHelper.CreateReleaseSequence(Factory, releaseGroup.PK, nudge: 2000);
			var item = BMSTestHelper.CreateReleaseSequenceItem(sequence, jobHeader);

			AssertEquals("Precondition", false, BMSRegistry.Instance.ReleaseSequencesModuleEnabled.Value);

			CombineAssertions(() =>
			{
				AssertEquals(0, jobHeader.ReleaseSequencePosition);
				AssertEquals(0, workflow.ReleaseSequencePosition);
			});

			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			CombineAssertions(() =>
			{
				AssertEquals(1, jobHeader.ReleaseSequencePosition);
				AssertEquals(1, workflow.ReleaseSequencePosition);
			});
		}

		public void TestReleaseSequencePosition_OnWorkflow()
		{
			BMSRegistry.Instance.ReleaseSequenceDefaultNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000);

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "group");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			var sequence = BMSTestHelper.CreateReleaseSequence(Factory, releaseGroup.PK, nudge: 2000);
			var item = BMSTestHelper.CreateReleaseSequenceItem(sequence, workflow);

			AssertEquals("Precondition", false, BMSRegistry.Instance.ReleaseSequencesModuleEnabled.Value);

			CombineAssertions(() =>
			{
				AssertEquals(0, jobHeader.ReleaseSequencePosition);
				AssertEquals(0, workflow.ReleaseSequencePosition);
			});

			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			CombineAssertions(() =>
			{
				AssertEquals(0, jobHeader.ReleaseSequencePosition);
				AssertEquals(1, workflow.ReleaseSequencePosition);
			});
		}

		public void TestReleaseWorkflow_OnJobLevelWorkflow()
		{
			BMSRegistry.Instance.ReleaseSequenceDefaultNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000);

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "group");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			var sequence = BMSTestHelper.CreateReleaseSequence(Factory, releaseGroup.PK, nudge: 2000);
			var item = BMSTestHelper.CreateReleaseSequenceItem(sequence, jobHeader);

			AssertEquals("Precondition", false, BMSRegistry.Instance.ReleaseSequencesModuleEnabled.Value);

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, jobHeader.ReleaseSequenceWorkflow);
				AssertEquals(ZString.Empty, workflow.ReleaseSequenceWorkflow);
			});

			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			CombineAssertions(() =>
			{
				AssertEquals("workflow", "Job Organization (XVBQP68SIYXQ) is complete.", workflow.ReleaseSequenceWorkflow);
				AssertEquals("jobheader", "Job Organization (XVBQP68SIYXQ) is complete.", jobHeader.ReleaseSequenceWorkflow);
			});
		}

		public void TestReleaseSequenceWorkflow_OnWorkflow()
		{
			BMSRegistry.Instance.ReleaseSequenceDefaultNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000);

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "group");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			var sequence = BMSTestHelper.CreateReleaseSequence(Factory, releaseGroup.PK, nudge: 2000);
			var item = BMSTestHelper.CreateReleaseSequenceItem(sequence, workflow);

			AssertEquals("Precondition", false, BMSRegistry.Instance.ReleaseSequencesModuleEnabled.Value);

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, jobHeader.ReleaseSequenceWorkflow);
				AssertEquals(ZString.Empty, workflow.ReleaseSequenceWorkflow);
			});

			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			CombineAssertions(() =>
			{
				AssertEquals("workflow", "Job Workflow", workflow.ReleaseSequenceWorkflow);
				AssertEquals("jobheader", ZString.Empty, jobHeader.ReleaseSequenceWorkflow);
			});
		}

		public void TestReleaseSequenceParentJob_OnJobLevelWorkflow()
		{
			BMSRegistry.Instance.ReleaseSequenceDefaultNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000);

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "group");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			var sequence = BMSTestHelper.CreateReleaseSequence(Factory, releaseGroup.PK, nudge: 2000);
			var item = BMSTestHelper.CreateReleaseSequenceItem(sequence, jobHeader);

			AssertEquals("Precondition", false, BMSRegistry.Instance.ReleaseSequencesModuleEnabled.Value);

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, jobHeader.ReleaseSequenceParentJob);
				AssertEquals(ZString.Empty, workflow.ReleaseSequenceParentJob);
			});

			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			CombineAssertions(() =>
			{
				AssertEquals("workflow", "Organization (XVBQP68SIYXQ) - Job Organization (XVBQP68SIYXQ) is complete.", workflow.ReleaseSequenceParentJob);
				AssertEquals("jobheader", "Organization (XVBQP68SIYXQ) - Job Organization (XVBQP68SIYXQ) is complete.", jobHeader.ReleaseSequenceParentJob);
			});
		}

		public void TestReleaseSequenceParentJob_OnWorkflow()
		{
			BMSRegistry.Instance.ReleaseSequenceDefaultNudge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1000);

			var releaseGroup = BMSTestHelper.CreateGroup(Factory, "group");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			var sequence = BMSTestHelper.CreateReleaseSequence(Factory, releaseGroup.PK, nudge: 2000);
			var item = BMSTestHelper.CreateReleaseSequenceItem(sequence, workflow);

			AssertEquals("Precondition", false, BMSRegistry.Instance.ReleaseSequencesModuleEnabled.Value);

			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, jobHeader.ReleaseSequenceParentJob);
				AssertEquals(ZString.Empty, workflow.ReleaseSequenceParentJob);
			});

			BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			CombineAssertions(() =>
			{
				AssertEquals("workflow", "Organization (XVBQP68SIYXQ) - Job Organization (XVBQP68SIYXQ) is complete.", workflow.ReleaseSequenceParentJob);
				AssertEquals("jobheader", ZString.Empty, jobHeader.ReleaseSequenceParentJob);
			});
		}

		public void TestEffectiveNudge_WhenFH_FH_ParentHeaderNotSet_ShouldReturnZero_AndNotThrowException()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			var workflow = Factory.New<ProcessHeader>();
			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			var result = 0m;

			AssertEquals("Precondition: this exception was being thrown when FH_FH_ParentHeader is empty and EffectiveNudge is called.", ZGuid.Empty, workflow.FH_FH_ParentHeader);
			AssertNoExceptionThrown("Accessing DirectNudge when FH_FH_ParentHeader is not set due to Universal Copy or other means should not throw exceptions.", () => result = workflow.EffectiveNudge);
			AssertEquals(0m, result);
			AssertEquals(0m, workflow.FH_EffectiveNudge);

			workflow.FH_VoteUpDownAmount = 1;

			AssertNoExceptionThrown("Accessing DirectNudge when FH_FH_ParentHeader is not set due to Universal Copy or other means should not throw exceptions.", () => result = workflow.EffectiveNudge);
			AssertEquals("Just checking EffectiveNudge still works.", 1m, result);

			AssertEquals(0m, workflow.FH_EffectiveNudge);
			workflow.UpdateEffectiveNudge();
			AssertEquals("Just checking EffectiveNudge still works.", 1m, workflow.FH_EffectiveNudge);
		}

		#endregion

		#region ReleaseDelayExpiry

		public void TestReleaseDelay_FieldReadonliness()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();

			AssertEquals(false, jobHeader.FH_TimeDelayFactorInfo.ReadOnly);
			AssertEquals(false, jobHeader.FH_TimeDelayMinutesInfo.ReadOnly);

			AssertEquals(true, workflow.FH_TimeDelayFactorInfo.ReadOnly);
			AssertEquals(true, workflow.FH_TimeDelayMinutesInfo.ReadOnly);
		}

		[TestDate(2014, 5, 28, 9, 0, 0)]
		public void TestCalculateReleaseDelayExpiry()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);

			BMSTestHelper.LinkComponents(bucket, buffer);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", bucket);

			var task1 = BMSTestHelper.CreateTask(workflow1);
			var task2 = BMSTestHelper.CreateTask(workflow2);

			var link = workflow1.GetOrCreateDependencyLink(workflow2);
			link.FP_TimeDelayMinutes = 120;

			workflow2.CalculateReleaseDelayExpiry();
			AssertEquals(ZDateTime.UtcNow.AddMinutes(120), workflow2.FH_StaggeredReleaseDelayExpiry);

			Factory.Save();

			AssertEquals(false, workflow2.HasChanges);

			workflow2.CalculateReleaseDelayExpiry();
			AssertEquals(ZDateTime.UtcNow.AddMinutes(120), workflow2.FH_StaggeredReleaseDelayExpiry);
			AssertEquals(false, workflow2.HasChanges);
		}

		[TestDate(2014, 5, 28, 9, 0, 0)]
		public void TestCalculateReleaseDelayExpiry_ClosedPrereq()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);

			BMSTestHelper.LinkComponents(bucket, buffer);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", bucket);

			var link = workflow1.GetOrCreateDependencyLink(workflow2);
			link.FP_TimeDelayMinutes = 120;

			workflow2.CalculateReleaseDelayExpiry();
			AssertEquals(ZDateTime.Empty, workflow2.FH_StaggeredReleaseDelayExpiry);
		}

		public void TestStaggeredReleaseDelayExpiry_IgnoreConcurrencyCheck()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			newFactory.RefreshEnabled = false;
			var reloadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);

			var utcNow = ZDateTime.UtcNow;
			var utcYesterday = utcNow.AddDays(-1);

			workflow.FH_StaggeredReleaseDelayExpiry = utcNow;
			reloadedWorkflow.FH_StaggeredReleaseDelayExpiry = utcYesterday;

			newFactory.Save();
			Factory.Save();

			AssertEquals(utcNow, workflow.FH_StaggeredReleaseDelayExpiry);
			AssertEquals(utcYesterday, reloadedWorkflow.FH_StaggeredReleaseDelayExpiry);
		}

		#endregion

		#region PlannedDuration

		public void TestPlannedDuration_ShouldBeReadonly()
		{
			var workflow = Factory.NewWithValidTestData<ProcessHeader>();
			AssertEquals(true, workflow.FH_PlannedDurationInMinutesInfo.ReadOnly);
		}

		public void TestPlannedDuration_ShouldBeMaintainedByTaskUpdates()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task1 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			AssertEquals(0, workflow.FH_PlannedDurationInMinutes);

			Factory.Save();
			AssertEquals(90, workflow.FH_PlannedDurationInMinutes);

			task1.P9_EstimateVariationFactor = 3;
			Factory.Save();
			AssertEquals(120, workflow.FH_PlannedDurationInMinutes);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertEquals(120, workflow.FH_PlannedDurationInMinutes);

			var task2 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 10);
			Factory.Save();
			AssertEquals(135, workflow.FH_PlannedDurationInMinutes);

			task1.Delete();
			Factory.Save();
			AssertEquals(15, workflow.FH_PlannedDurationInMinutes);

			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			task2.P9_EstDuration = new ZInt(1000).GetDateTimeFromMinutes();
			Factory.Save();
			AssertEquals("Planned duration should not change when workflow is in a buffer", 15, workflow.FH_PlannedDurationInMinutes);

			workflow.FH_FC_CurrentComponent = config.Bucket.PK;
			Factory.Save();
			AssertEquals(1500, workflow.FH_PlannedDurationInMinutes);
		}

		public void TestPlannedDuration_ShouldRoundCorrectly()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 40);

			Factory.Save();

			AssertEquals(60, workflow.FH_PlannedDurationInMinutes);
			AssertEquals(60m, Utilities.Round(task.StandardEstimateHours * 60m, 0));
		}

		public void TestPlannedDuration_ShouldUpdateJobHeader()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			AssertEquals(90, jobHeader.FH_PlannedDurationInMinutes);
			AssertEquals(90, workflow1.FH_PlannedDurationInMinutes);
			AssertEquals(0, workflow2.FH_PlannedDurationInMinutes);

			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			Factory.Save();

			AssertEquals(180, jobHeader.FH_PlannedDurationInMinutes);
			AssertEquals(90, workflow1.FH_PlannedDurationInMinutes);
			AssertEquals(90, workflow2.FH_PlannedDurationInMinutes);

			task2.P9_EstDuration = new ZInt(10).GetDateTimeFromMinutes();
			Factory.Save();

			AssertEquals(105, jobHeader.FH_PlannedDurationInMinutes);
			AssertEquals(90, workflow1.FH_PlannedDurationInMinutes);
			AssertEquals(15, workflow2.FH_PlannedDurationInMinutes);
		}

		public void TestPlannedDuration_WhenSettingComponentToBeBufferBeforeSaving()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task1 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			AssertEquals("Planned Duration should not be set yet", 0, workflow.FH_PlannedDurationInMinutes);

			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			AssertEquals("Planned Duration should be set since we've moved the workflow into a buffer", 90, jobHeader.FH_PlannedDurationInMinutes);
			AssertEquals("Planned Duration should be set since we've moved the workflow into a buffer", 90, workflow.FH_PlannedDurationInMinutes);

			var task2 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			AssertEquals("Planned Duration should not be changed once it's entered a buffer", 90, jobHeader.FH_PlannedDurationInMinutes);
			AssertEquals("Planned Duration should not be changed once it's entered a buffer", 90, workflow.FH_PlannedDurationInMinutes);

			Factory.Save();

			AssertEquals("Planned Duration should not be changed once it's entered a buffer", 90, jobHeader.FH_PlannedDurationInMinutes);
			AssertEquals("Planned Duration should not be changed once it's entered a buffer", 90, workflow.FH_PlannedDurationInMinutes);

			workflow.FH_FC_CurrentComponent = config.Bucket.PK;
			Factory.Save();

			AssertEquals("Planned Duration should be updated once it's entered a bucket", 180, jobHeader.FH_PlannedDurationInMinutes);
			AssertEquals("Planned Duration should be updated once it's entered a bucket", 180, workflow.FH_PlannedDurationInMinutes);

			var task3 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			AssertEquals("Planned Duration should be updated since a new task was added just as it entered a buffer", 270, jobHeader.FH_PlannedDurationInMinutes);
			AssertEquals("Planned Duration should be updated since a new task was added just as it entered a buffer", 270, workflow.FH_PlannedDurationInMinutes);
		}

		public void TestPlannedDuration_ShouldIgnoreConcurrencyError()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");

			Factory.RefreshEnabled = false;
			Factory.Save();

			var factory2 = Factory.CreateNewFactory();
			factory2.RefreshEnabled = false;
			var workflow2 = factory2.Load<ProcessHeader>(workflow1.PK);

			workflow1.FH_PlannedDurationInMinutes = 60;
			Factory.Save();

			workflow2.FH_PlannedDurationInMinutes = 420;
			factory2.Save();

			AssertEquals(60, workflow1.FH_PlannedDurationInMinutes);
			AssertEquals(420, workflow2.FH_PlannedDurationInMinutes);
		}

		#endregion

		#region Child Workflows

		public void TestChildWorkflowExists_ShouldNotAffectStartabilityOfParent()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2");
			var workflow2Child = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2Child");
			var workflow2Grandchild = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2Grandchild");

			workflow2Child.GetOrCreateLinkToParent(workflow2);
			workflow2Grandchild.GetOrCreateLinkToParent(workflow2Child);
			jobHeader1.GetOrCreateDependencyLink(jobHeader2);

			var task1 = BMSTestHelper.CreateTask(workflow1);
			var task2 = BMSTestHelper.CreateTask(workflow2);
			var task3 = BMSTestHelper.CreateTask(workflow2Child);
			var task4 = BMSTestHelper.CreateTask(workflow2Grandchild);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("jobHeader1", WorkflowStatusList.Codes.Open, jobHeader1.FH_Status);
				AssertEquals("workflow1", WorkflowStatusList.Codes.Open, workflow1.FH_Status);

				AssertEquals("jobHeader2", WorkflowStatusList.Codes.Blocked, jobHeader2.FH_Status);
				AssertEquals("workflow2", WorkflowStatusList.Codes.Blocked, workflow2.FH_Status);
				AssertEquals("workflow2Child", WorkflowStatusList.Codes.Blocked, workflow2Child.FH_Status);
				AssertEquals("workflow2Grandchild", WorkflowStatusList.Codes.Blocked, workflow2Grandchild.FH_Status);
			});
		}

		public void TestParentWorkflow_ShouldNotBeCompleteUntilChildIsComplete()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var grandparentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "grandparentWorkflow");
			var parentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "parentWorkflow");
			var childWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "childWorkflow");

			var grandparentTask = BMSTestHelper.CreateTask(grandparentWorkflow, GlbStaff.CurrentUser.GS_Code, 60);
			var parentTask = BMSTestHelper.CreateTask(parentWorkflow, GlbStaff.CurrentUser.GS_Code, 60);
			var childTask = BMSTestHelper.CreateTask(childWorkflow, GlbStaff.CurrentUser.GS_Code, 60);

			BMSTestHelper.MakeChildOf(parentWorkflow, grandparentWorkflow);
			BMSTestHelper.MakeChildOf(childWorkflow, parentWorkflow);

			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, grandparentWorkflow.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, parentWorkflow.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, childWorkflow.FH_Status);

			grandparentTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, grandparentWorkflow.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, parentWorkflow.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, childWorkflow.FH_Status);

			childTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, grandparentWorkflow.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, parentWorkflow.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Closed, childWorkflow.FH_Status);

			parentTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Closed, grandparentWorkflow.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Closed, parentWorkflow.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Closed, childWorkflow.FH_Status);
		}

		public void TestChildWorkflow_ShouldNotBeStartableUntilParentIsStartable()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var prereqWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "prereqWorkflow");
			var grandparentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "grandparentWorkflow");
			var parentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "parentWorkflow");
			var childWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "childWorkflow");

			var prereqTask = BMSTestHelper.CreateTask(prereqWorkflow, GlbStaff.CurrentUser.GS_Code, 60);
			var grandparentTask = BMSTestHelper.CreateTask(grandparentWorkflow, GlbStaff.CurrentUser.GS_Code, 60);
			var parentTask = BMSTestHelper.CreateTask(parentWorkflow, GlbStaff.CurrentUser.GS_Code, 60);
			var childTask = BMSTestHelper.CreateTask(childWorkflow, GlbStaff.CurrentUser.GS_Code, 60);

			prereqWorkflow.GetOrCreateDependencyLink(grandparentWorkflow);
			BMSTestHelper.MakeChildOf(parentWorkflow, grandparentWorkflow);
			BMSTestHelper.MakeChildOf(childWorkflow, parentWorkflow);

			Factory.Save();

			AssertEquals(WorkflowPrerequisiteStatusList.Codes.ClearedToStart, prereqWorkflow.PrerequisiteStatus);
			AssertEquals(WorkflowPrerequisiteStatusList.Codes.Blocked, grandparentWorkflow.PrerequisiteStatus);
			AssertEquals(WorkflowPrerequisiteStatusList.Codes.Blocked, parentWorkflow.PrerequisiteStatus);
			AssertEquals(WorkflowPrerequisiteStatusList.Codes.Blocked, childWorkflow.PrerequisiteStatus);

			prereqTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(WorkflowPrerequisiteStatusList.Codes.ClearedToStart, prereqWorkflow.PrerequisiteStatus);
			AssertEquals(WorkflowPrerequisiteStatusList.Codes.ClearedToStart, grandparentWorkflow.PrerequisiteStatus);
			AssertEquals(WorkflowPrerequisiteStatusList.Codes.ClearedToStart, parentWorkflow.PrerequisiteStatus);
			AssertEquals(WorkflowPrerequisiteStatusList.Codes.ClearedToStart, childWorkflow.PrerequisiteStatus);
		}

		public void TestSynchroniseBufferPenetration()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			var childWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "childWorkflow");

			AssertEquals(true, jobHeader.SynchroniseBufferPenetrationInfo.ReadOnly);
			AssertEquals(true, workflow.SynchroniseBufferPenetrationInfo.ReadOnly);
			AssertEquals(true, childWorkflow.SynchroniseBufferPenetrationInfo.ReadOnly);

			var link = childWorkflow.GetOrCreateLinkToParent(workflow);
			AssertEquals(false, link.FP_SynchroniseBufferPenetration);

			AssertEquals(true, jobHeader.SynchroniseBufferPenetrationInfo.ReadOnly);
			AssertEquals(true, workflow.SynchroniseBufferPenetrationInfo.ReadOnly);
			AssertEquals(false, childWorkflow.SynchroniseBufferPenetrationInfo.ReadOnly);

			childWorkflow.SynchroniseBufferPenetration = true;
			AssertEquals(true, link.FP_SynchroniseBufferPenetration);

			childWorkflow.SynchroniseBufferPenetration = false;
			AssertEquals(false, link.FP_SynchroniseBufferPenetration);
		}

		#endregion

		#region Release Notes

		[TestDate(2015, 1, 2)]
		public void TestManuallyRelease_ShouldLogSuccessfulReleaseNotes()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.LogTransferIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Description = "Being a Dragon";

			var loginResource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DE", "Davey Boy");
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BEN", "Bendy", capability);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "JON", "Jawwwn", capability);
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", config.Bucket);
			var task1 = BMSTestHelper.CreateTask(workflow, string.Empty, 60, capability: capability);
			var task2 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, 60, capability: capability);

			Factory.Save();

			task1.P9_TaskID = "T00002000";
			task2.P9_TaskID = "T00002001";
			Factory.Save();

			AssertEquals(string.Empty, workflow.GetSuccessfulReleaseNotes());

			using (Env.SetTemporaryUserContext(loginResource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				workflow.FH_FC_CurrentComponent = config.Buffer.PK;
				AssertEquals(string.Empty, workflow.GetSuccessfulReleaseNotes());

				Factory.Save();
			}

			AssertMultilineASCIIEquals("Should log release details once workflow is saved",
@"It was manually released by [Davey Boy].", workflow.GetSuccessfulReleaseNotes());
		}

		[TestDate(2019, 1, 1)]
		public void TestManuallyRelease_WhenCapacityForSomeButNotAllResourcesIsCached_ShouldLogSuccessfulReleaseNotes_AndNotReportExceptions()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.LogTransferIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BAR", "Barnaboyce");
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "MAL", "Malcoberts");

			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "This workflow causes capacity to be cached for resource1");
			BMSTestHelper.CreateTask(workflow1, resource1.GS_Code, lowEstMinutes: 10);

			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);

			Factory.Save();

			ReleaseGateKeeperTest.RunReleaseGate(config.System);

			AssertNotNull("Capacity is now cached for resource1 since they had a task in the release gate", BufferCapacityCache.Get(config.Buffer.PK).GetCapacity(resource1.GS_Code, Factory));
			AssertNull("Pre-condition: capacity is not cached for resource2", BufferCapacityCache.Get(config.Buffer.PK).GetCapacity(resource2.GS_Code, Factory));

			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Kicked out, unceremoniously");
			BMSTestHelper.CreateTask(workflow2, resource1.GS_Code, lowEstMinutes: 10);
			BMSTestHelper.CreateTask(workflow2, resource2.GS_Code, lowEstMinutes: 10);

			Factory.Save();

			using (Env.SetTemporaryUserContext(resource2.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
				Factory.Save();
			}

			AssertNullOrEmpty("Even though one resource had their capacity cached and the other did not, that's fine because the release gate hasn't had a chance to run yet. The exception isn't relevant for user sessions.", ErrorReporter.LastMessageReported);

			AssertMultilineASCIIEquals("Should log release details once workflow is saved",
@"It was manually released by [Malcoberts].".StripTaskIds(), workflow2.GetSuccessfulReleaseNotes().StripTaskIds());
		}

		[TestDate(2015, 1, 2)]
		public void TestManuallyRelease_WhenNoCapacity_ShouldLogReleaseNotesAnyway()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.LogTransferIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Description = "Being a Dragon";

			var loginResource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DE", "Davey Boy");
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BEN", "Bendy", capability);
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "JON", "Jawwwn", capability);
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", config.Bucket);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", config.Buffer);
			var task1_1 = BMSTestHelper.CreateTask(workflow1, string.Empty, 60, capability: capability);
			var task1_2 = BMSTestHelper.CreateTask(workflow1, resource1.GS_Code, 60, capability: capability);
			var task2_1 = BMSTestHelper.CreateTask(workflow2, resource1.GS_Code, 3000);

			Factory.Save();

			task1_1.P9_TaskID = "T00002000";
			task1_2.P9_TaskID = "T00002001";
			task2_1.P9_TaskID = "T00002002";
			Factory.Save();

			AssertEquals(string.Empty, workflow1.GetSuccessfulReleaseNotes());

			using (Env.SetTemporaryUserContext(loginResource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
				AssertEquals(string.Empty, workflow1.GetSuccessfulReleaseNotes());

				Factory.Save();
			}

			AssertMultilineASCIIEquals("Should log release details once workflow is saved",
@"It was manually released by [Davey Boy].", workflow1.GetSuccessfulReleaseNotes());
		}

		[TestDate(2015, 1, 9)]
		public void TestManuallyRelease_WhenNoCapacityAndNotAssigned_ShouldLogReleaseNotes()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.LogTransferIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var loginResource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DV", "Darth Vader");
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", config.Bucket);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", config.Buffer);
			var task1_1 = BMSTestHelper.CreateTask(workflow1, string.Empty, 60);
			var task1_2 = BMSTestHelper.CreateTask(workflow1, string.Empty, 60);
			var task2_1 = BMSTestHelper.CreateTask(workflow2, string.Empty, 3000);

			Factory.Save();

			task1_1.P9_TaskID = "T00002000";
			task1_2.P9_TaskID = "T00002001";
			task2_1.P9_TaskID = "T00002002";
			Factory.Save();

			AssertEquals(string.Empty, workflow1.GetSuccessfulReleaseNotes());

			using (Env.SetTemporaryUserContext(loginResource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
				AssertEquals(string.Empty, workflow1.GetSuccessfulReleaseNotes());

				Factory.Save();
			}

			AssertMultilineASCIIEquals("Should log release details once workflow is saved",
				"It was manually released by [Darth Vader].",
				workflow1.GetSuccessfulReleaseNotes());
		}

		[TestDate(2015, 1, 2)]
		public void TestManuallyRelease_WhenReleaseLoggingDisabled_ShouldNotLogSuccessfulReleaseNotes()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			AssertEquals("Precondition: release logging should be disabled in the registry", false, BMSRegistry.Instance.LogTransferIntoBufferDetails.Value);

			var loginResource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DE", "Davey Boy");
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BEN", "Bendy");
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", config.Bucket);
			var task1 = BMSTestHelper.CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			task1.P9_TaskID = "T00002000";
			Factory.Save();

			AssertEquals(string.Empty, workflow.GetSuccessfulReleaseNotes());

			using (Env.SetTemporaryUserContext(loginResource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				workflow.FH_FC_CurrentComponent = config.Buffer.PK;
				AssertEquals(string.Empty, workflow.GetSuccessfulReleaseNotes());

				Factory.Save();
			}

			AssertMultilineASCIIEquals("", "", workflow.GetSuccessfulReleaseNotes());
		}

		public void TestManuallyRelease_WhenCapacityCalculationDisabled_ShouldLogRelease_ButReplaceCapacityDetailsWithCapacityCalculationDisabledMessage()
		{
			BMSRegistry.Instance.LogTransferIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.DisableCapacityCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "BEN", "Bendy");
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", config.Bucket);
			BMSTestHelper.CreateTask(workflow, resource.GS_Code, 60);
			Factory.Save();

			AssertEquals(string.Empty, workflow.GetSuccessfulReleaseNotes());

			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			var releaseNotes = workflow.GetSuccessfulReleaseNotes();
			AssertNotContains("Details of relevant resource capacity are listed below.", releaseNotes);
			AssertContains("It was manually released by [CargoWise Support].", releaseNotes);
			AssertNotEquals("Capacity was calculated even though the DisableCapacityCalculations registry item was enabled.", ErrorReporter.LastMessageReported);
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestManualRelease_ShouldNotUseReleaseFailureService()
		{
			BMSRegistry.Instance.LogTransferIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Workflow", config.Bucket);

			workflow.FH_FC_CurrentComponent = config.Buffer.PK; // Manual release
			Factory.Save();

			AssertNull(Factory.ServiceContainer.GetService<ReleaseGateFailureLogService>());
		}

		[TestDate(2014, 11, 27)]
		public void TestStatusChangeToClosed_ShouldDeleteLastSuccessfulReleaseNote()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			BMSRegistry.Instance.LogReleaseIntoBufferDetails.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "FRO", "Frodo Baggins");
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", config.Bucket);
			var task = BMSTestHelper.CreateTask(workflow, resource.GS_Code, 120);

			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);

			Factory.Save();

			ReleaseGateKeeperTest.RunReleaseGate(config.System);

			var newFactory = Factory.CreateNewFactory();
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);

			BMSTestCaseWithFactory.AssertSamePK(config.Buffer, loadedWorkflow.CurrentComponent);
			AssertNotNull(loadedWorkflow.GetSuccessfulReleaseNote(newFactory));

			loadedWorkflow.FH_FC_CurrentComponent = config.Bucket.PK;
			newFactory.Save();

			AssertNotNull("Moving workflow to another component should not delete the note", loadedWorkflow.GetSuccessfulReleaseNote(newFactory));

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			loadedWorkflow.Tasks.Single().P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			newFactory.Save();

			AssertNull("Closing workflow should delete the note", loadedWorkflow.GetSuccessfulReleaseNote(newFactory));
		}

		public void TestGetReleaseFailureReasonForBuffer_ShouldNotUseStmNote()
		{
			var service = new ReleaseGateFailureLogService_ForTest();
			BMSTestCaseWithFactory.SetFactoryReleaseLogFailureService(Factory, service);
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Workflow", config.Bucket);

			Factory.Save();

			BMSTestHelper.SetLastReleaseFailureReason(workflow, "Needs more dog.", config.Buffer, service);
			string reason;

			using (AssertDbHitsForAllFactories(new Dictionary<string, int> { { StmNoteSchema.Constants.TableName, 0 } }))
			{
				reason = workflow.GetReleaseFailureReasonForBuffer(config.Buffer);
			}

			AssertEquals("Needs more dog.", reason);
		}

		public void TestGetReleaseFailureReasonForBuffer_WhenNoReasonExists_ShouldReturnAppropriateMessage()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Workflow", config.Bucket);

			Factory.Save();

			var reason = workflow.GetReleaseFailureReasonForBuffer(config.Buffer);
			AssertEquals("Details are not available until the next time the service task runs.", reason);
		}

		public void TestGetReleaseFailureReasonForBuffer_WhenNoReasonExists_ShouldReturnAppropriateMessage_WhenWorkflowInactive()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Workflow", config.Bucket);
			workflow.FH_IsActive = false;

			Factory.Save();

			var reason = workflow.GetReleaseFailureReasonForBuffer(config.Buffer);
			AssertEquals(@"Details are not available until the next time the service task runs.
The workflow is currently deactivated and ignored by Release Gate.", reason);
		}

		#endregion

		#region Task Properties

		public void TestGetProcessHeaderForTask_ShouldBeNullWhenJobIsNotRelatedToABufferManagementSystem()
		{
			var processHeaderCount = Factory.GetDatabaseCount(typeof(ProcessHeader));

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesEnquiry = Factory.NewWithValidTestData<SalesEnquiry>();

			var orgTask = org.WorkflowItems.AddNew();
			var salesTask = salesEnquiry.WorkflowItems.AddNew();

			AssertNotNull(orgTask.ProcessHeader);
			AssertNotNull(orgTask.ProcessHeader.JobHeader);
			AssertEquals(1, orgTask.Lookups.ProcessHeaders.Count);

			AssertNull(salesTask.ProcessHeader);
			AssertEquals(0, salesTask.Lookups.ProcessHeaders.Count);

			Factory.Save();

			AssertEquals(processHeaderCount + 2, Factory.GetDatabaseCount(typeof(ProcessHeader)));
		}

		public void TestSupportsFormFlowTypeTasks()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			AssertEquals(false, workflow.SupportsFormFlowTypeTasks);

			workflow.FH_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			AssertEquals(true, workflow.SupportsFormFlowTypeTasks);

			workflow.FH_ParentTableCode = WorkItemSchema.Constants.Prefix;
			AssertEquals(false, workflow.SupportsFormFlowTypeTasks);

			workflow.FH_ParentTableCode = WhsPickSchema.Constants.Prefix;
			AssertEquals(true, workflow.SupportsFormFlowTypeTasks);
			workflow.FH_ParentTableCode = string.Empty; //clean up

			workflow.FH_ParentTableCode = WhsLoadSchema.Constants.Prefix;
			AssertEquals(true, workflow.SupportsFormFlowTypeTasks);
			workflow.FH_ParentTableCode = string.Empty; //clean up

			workflow.FH_ParentTableCode = WhsCycleCountWaveSchema.Constants.Prefix;
			AssertEquals(true, workflow.SupportsFormFlowTypeTasks);
		}

		#endregion

		#region Parent

		public void TestPropertiesWhichRelyOnParent_ShouldNotThrowExceptionsOnAccess()
		{
			var jobHeader = Factory.NewWithValidTestData<ProcessJobHeader>();
			var workflow = jobHeader.ProcessHeaders.AddNew();
			AssertNull(workflow.Parent);

			CombineAssertions("The following properties threw exceptions on access when there is no Parent", () =>
			{
				foreach (var property in typeof(ProcessHeader).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Concat(typeof(IProposedNetworkEntity).GetProperties()))
				{
					AssertNoExceptionThrown(property.Name, () => property.GetValue(workflow, null));
				}
			});
		}

		public void TestIsInSameJob()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1_1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1_1");
			var workflow1_2 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1_2");
			var workflow1_3 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1_3");
			var workflow1_4 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1_4");
			workflow1_3.FH_ParentTableCode = "ZZZ";
			workflow1_4.FH_ParentTableCode = "ZZZ";

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow2_1 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2_1");
			var workflow2_2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2_2");
			var workflow2_3 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2_3");
			var workflow2_4 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2_4");
			workflow2_3.FH_ParentTableCode = "ZZZ";
			workflow2_4.FH_ParentTableCode = "ZZZ";

			var processHeadersInFirstJob = new[] { jobHeader1, workflow1_1, workflow1_2 };
			var processHeadersInFirstJobWithDifferentParentTableCode = new[] { workflow1_3, workflow1_4 };

			var processHeadersInSecondJob = new[] { jobHeader2, workflow2_1, workflow2_2 };
			var processHeadersInSecondJobWithDifferentParentTableCode = new[] { workflow2_3, workflow2_4 };

			AssertIsInSameJob(jobHeader1, processHeadersInFirstJob, processHeadersInSecondJob.Concat(processHeadersInFirstJobWithDifferentParentTableCode));
			AssertIsInSameJob(workflow1_1, processHeadersInFirstJob, processHeadersInSecondJob.Concat(processHeadersInFirstJobWithDifferentParentTableCode));
			AssertIsInSameJob(workflow1_2, processHeadersInFirstJob, processHeadersInSecondJob.Concat(processHeadersInFirstJobWithDifferentParentTableCode));

			AssertIsInSameJob(workflow1_3, processHeadersInFirstJobWithDifferentParentTableCode, processHeadersInSecondJob.Concat(processHeadersInFirstJob));
			AssertIsInSameJob(workflow1_4, processHeadersInFirstJobWithDifferentParentTableCode, processHeadersInSecondJob.Concat(processHeadersInFirstJob));

			AssertIsInSameJob(jobHeader2, processHeadersInSecondJob, processHeadersInFirstJob.Concat(processHeadersInSecondJobWithDifferentParentTableCode));
			AssertIsInSameJob(workflow2_1, processHeadersInSecondJob, processHeadersInFirstJob.Concat(processHeadersInSecondJobWithDifferentParentTableCode));
			AssertIsInSameJob(workflow2_2, processHeadersInSecondJob, processHeadersInFirstJob.Concat(processHeadersInSecondJobWithDifferentParentTableCode));

			AssertIsInSameJob(workflow2_3, processHeadersInSecondJobWithDifferentParentTableCode, processHeadersInSecondJob.Concat(processHeadersInFirstJob));
			AssertIsInSameJob(workflow2_4, processHeadersInSecondJobWithDifferentParentTableCode, processHeadersInSecondJob.Concat(processHeadersInFirstJob));

			void AssertIsInSameJob(ProcessHeader target, IEnumerable<ProcessHeader> processHeadersInSameJob, IEnumerable<ProcessHeader> processHeadersInDifferentJob)
			{
				AssertEquals(true, target.IsInSameJob(target));

				foreach (var processHeader in processHeadersInSameJob)
				{
					AssertEquals(true, target.IsInSameJob(processHeader));
				}

				foreach (var processHeader in processHeadersInDifferentJob)
				{
					AssertEquals(false, target.IsInSameJob(processHeader));
				}
			}
		}

		public void TestJobProperties()
		{
			BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WorkflowManagementModes.Codes.IncludesBufferManagement);

			var system = BMSTestHelper.CreateSystem(Factory, "WKI");

			var job = Factory.New<IWorkItem>();
			job.WKI_WorkItemNumber = "W00123456";
			job.WKI_Summary = "Work Item Summary";

			var provider = job as IWorkflowProvider;
			var jobHeader = (ProcessJobHeader)BMSTestHelper.GetJobHeaderForParent(provider, Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");

			CombineAssertions(() =>
			{
				AssertEquals(string.Empty, jobHeader.FH_JobCode);
				AssertEquals(string.Empty, jobHeader.FH_JobDescription);
				AssertEquals(string.Empty, workflow.FH_JobCode);
				AssertEquals(string.Empty, workflow.FH_JobDescription);
			});

			Factory.Save();

			jobHeader.Reload();
			workflow.Reload();

			CombineAssertions(() =>
			{
				AssertEquals("W00123456", jobHeader.FH_JobCode);
				AssertEquals("Work Item Summary", jobHeader.FH_JobDescription);
				AssertEquals("W00123456", workflow.FH_JobCode);
				AssertEquals("Work Item Summary", workflow.FH_JobDescription);
			});

			job.WKI_WorkItemNumber = "W00123457";
			job.WKI_Summary = "Work Item Summary New";
			Factory.Save();

			jobHeader.Reload();
			workflow.Reload();

			CombineAssertions(() =>
			{
				AssertEquals("W00123457", jobHeader.FH_JobCode);
				AssertEquals("Work Item Summary New", jobHeader.FH_JobDescription);
				AssertEquals("W00123457", workflow.FH_JobCode);
				AssertEquals("Work Item Summary New", workflow.FH_JobDescription);
			});
		}

		#endregion

		#region Network Visualisation

		#region Promote

		public void TestPromote()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			workflow1.FH_CompletionStatement = "The Yolo Workflow";
			workflow2.FH_CompletionStatement = "The Awesome Workflow";
			workflow1.GetOrCreateDependencyLink(workflow2);

			AssertCollectionContains(workflow2, workflow1.Links.Select(l => l.HeaderTo));
			var link = workflow1.Links.First(l => l.HeaderTo == workflow2);
			link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			Factory.Save();

			var promotedHeader = Factory.New<OrgHeader>();

			workflow1.Promote(promotedHeader);
			workflow1.Delete();

			AssertEquals(true, workflow1.IsDeleted);
			AssertEquals(true, link.IsDeleted);
			AssertEquals(false, workflow2.IsDeleted);

			AssertEquals("The Yolo Workflow", promotedHeader.OH_FullName);
			AssertNoErrors(promotedHeader.OH_FullNameInfo);

			var newJobheader = ProcessJobHeader.GetForParentWithoutCreation(promotedHeader, Factory);
			AssertNotNull(newJobheader);

			AssertCollectionContains(newJobheader.LinksFromMeToOthers.Cast<ProcessHeaderLink>(), l => l.FP_FH_HeaderTo == workflow2.PK);

			promotedHeader.FillWithValidTestData();
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestPromote_OrgOnDifferentFactory()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			workflow1.FH_CompletionStatement = "The Yolo Workflow";
			workflow2.FH_CompletionStatement = "The Awesome Workflow";
			workflow1.GetOrCreateDependencyLink(workflow2);

			AssertCollectionContains(workflow2, workflow1.Links.Select(l => l.HeaderTo));
			var link = workflow1.Links.First(l => l.HeaderTo == workflow2);
			link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			Factory.Save();

			var factory2 = Factory.CreateNewFactory();
			var promotedHeader = factory2.New<OrgHeader>();

			workflow1.Promote(promotedHeader);
			AssertNoExceptionThrown(() => workflow1.Delete());

			AssertEquals(true, workflow1.IsDeleted);
			AssertEquals(true, link.IsDeleted);
			AssertEquals(false, workflow2.IsDeleted);

			AssertEquals("The Yolo Workflow", promotedHeader.OH_FullName);
			AssertNoErrors(promotedHeader.OH_FullNameInfo);

			var newJobheader = ProcessJobHeader.GetForParentWithoutCreation(promotedHeader, factory2);
			AssertNotNull(newJobheader);
			AssertCollectionContains(newJobheader.LinksFromMeToOthers.Cast<ProcessHeaderLink>(), l => l.FP_FH_HeaderTo == workflow2.PK);

			promotedHeader.FillWithValidTestData();
			promotedHeader.OH_Code = "NYEH";

			AssertNoExceptionThrown(() => Factory.Save());
			AssertNoExceptionThrown(() => factory2.Save());
		}

		public void TestPromoteJobHeader_Explode()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			AssertExceptionThrown<InvalidOperationException>(() => jobHeader.Promote(Factory.New<OrgHeader>()));
		}

		public void TestPromoteJobHeader_DeleteOldTasks()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "This IS it!");
			var task1 = BMSTestHelper.CreateTask(workflow);
			var task2 = BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var promotedHeader = newFactory.NewWithValidTestData<OrgHeader>();
			promotedHeader.OH_Code = "ORG";

			workflow.Promote(promotedHeader);

			Factory.Save();
			newFactory.Save();

			AssertEquals("This IS it!", promotedHeader.OH_FullName);

			var newJobheader = ProcessJobHeader.GetForParentWithoutCreation(promotedHeader, newFactory);
			AssertNotNull(newJobheader);

			AssertEquals(2, newJobheader.ProcessHeaders[0].TaskCollection.Count);
			Assert(task1.IsDeleted);
			Assert(task2.IsDeleted);
		}

		public void TestPromoteJobHeader_DontDeleteOldTasks()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "This IS it!");
			var task1 = BMSTestHelper.CreateTask(workflow);
			var task2 = BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var promotedJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			workflow.Promote(promotedJobHeader, deleteOldTasks: false);

			Factory.Save();
			newFactory.Save();

			AssertEquals(2, promotedJobHeader.ProcessHeaders[0].TaskCollection.Count);
			Assert(!task1.IsDeleted);
			Assert(!task2.IsDeleted);
			Assert(!promotedJobHeader.ProcessHeaders[0].TaskCollection.Contains(task1));
			Assert(!promotedJobHeader.ProcessHeaders[0].TaskCollection.Contains(task2));
		}

		#endregion

		#region Approval

		public void TestApprovedShape_NotInPlanningManagementMode()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);

			var processHeader = Factory.NewWithValidTestData<ProcessHeader>();
			var shape = Factory.New<IBMNCNShape>();

			AssertNull(processHeader.ApprovedShape);
		}

		public void TestApprovedShape_RelatedShapeIsApproved()
		{
			var processHeader = Factory.NewWithValidTestData<ProcessHeader>();
			var shape = Factory.New<IBMNCNShape>();
			shape.BNS_RelatedEntityID = processHeader.PK;
			shape.Approve(GlbStaff.CurrentUser.GS_Code);

			AssertEquals(shape, processHeader.ApprovedShape);
		}

		public void TestApprovedShape_RelatedShapeIsNotApproved()
		{
			var processHeader = Factory.NewWithValidTestData<ProcessHeader>();
			var shape = Factory.New<IBMNCNShape>();
			shape.BNS_RelatedEntityID = processHeader.PK;

			AssertNull(processHeader.ApprovedShape);
		}

		public void TestApprovedShape_ShouldFindJobShape()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			var shape = Factory.New<IBMNCNShape>();
			shape.Approve(GlbStaff.CurrentUser.GS_Code);
			shape.BNS_RelatedEntityID = jobHeader.PK;

			AssertEquals(shape, jobHeader.ApprovedShape);
			AssertEquals(shape, workflow.ApprovedShape);
		}

		public void TestApprovedShape_DifferentShapeOnWorkflow()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			var jobShape = Factory.New<IBMNCNShape>();
			jobShape.Approve(GlbStaff.CurrentUser.GS_Code);
			jobShape.BNS_RelatedEntityID = jobHeader.PK;

			var workflowShape = Factory.New<IBMNCNShape>();
			workflowShape.Approve(GlbStaff.CurrentUser.GS_Code);
			workflowShape.BNS_RelatedEntityID = workflow.PK;

			AssertEquals(jobShape, jobHeader.ApprovedShape);
			AssertEquals(workflowShape, workflow.ApprovedShape);
		}

		public void TestApprovedShape_ShouldFindShapeOnParentUpTheHierarchy()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader4 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			BMSTestHelper.MakeChildOf(jobHeader2, jobHeader1);
			BMSTestHelper.MakeChildOf(jobHeader3, jobHeader2);
			BMSTestHelper.MakeChildOf(jobHeader4, jobHeader3);

			var shape = Factory.New<IBMNCNShape>();
			shape.Approve(GlbStaff.CurrentUser.GS_Code);
			shape.BNS_RelatedEntityID = jobHeader2.PK;

			AssertNull(jobHeader1.ApprovedShape);
			AssertEquals(shape, jobHeader2.ApprovedShape);
			AssertEquals(shape, jobHeader3.ApprovedShape);
			AssertEquals(shape, jobHeader4.ApprovedShape);
		}

		#endregion

		#endregion

		#region Completion Statement

		public void TestAddCompletionStatementsToChildCollection_AlsoAddsToParent()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedTaskType = categorisedTaskTypes.AddNew();
			categorisedTaskType.Code = "ORG";
			var taskType = categorisedTaskType.TaskTypes.AddNew();
			taskType.Code = "COM";
			taskType.IsCompletionStatementTaskType = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var job = jobHeader.Parent;
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";
			workflow2.FH_CompletionStatement = "workflow2";

			AssertEquals(0, job.WorkflowItems.Count);
			AssertEquals(0, jobHeader.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(0, workflow1.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(0, workflow2.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(0, workflow1.TaskCollection.Count);
			AssertEquals(0, workflow2.TaskCollection.Count);

			var completionStatement1 = workflow1.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();
			AssertEquals("COM", completionStatement1.P9_Type);
			AssertEquals(1001, completionStatement1.P9_Sequence);
			AssertEquals(1, completionStatement1.DisplayOrder);

			AssertEquals(1, workflow1.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(1, job.WorkflowItems.Count);
			AssertEquals(1, jobHeader.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(0, workflow2.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(0, workflow1.TaskCollection.Count);
			AssertEquals(0, workflow2.TaskCollection.Count);

			var completionStatement2 = jobHeader.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();
			AssertEquals("COM", completionStatement2.P9_Type);
			AssertEquals(1002, completionStatement2.P9_Sequence);
			AssertEquals(2, completionStatement2.DisplayOrder);

			AssertEquals(2, job.WorkflowItems.Count);
			AssertEquals(2, jobHeader.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(1, workflow1.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(0, workflow2.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(0, workflow1.TaskCollection.Count);
			AssertEquals(0, workflow2.TaskCollection.Count);

			completionStatement2.P9_FH_ProcessHeader = workflow2.PK;

			AssertEquals(2, job.WorkflowItems.Count);
			AssertEquals(2, jobHeader.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(1, workflow1.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(1, workflow2.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(0, workflow1.TaskCollection.Count);
			AssertEquals(0, workflow2.TaskCollection.Count);

			var task = workflow1.TaskCollection.AddNew();
			AssertNotEquals("COM", task.P9_Type);

			AssertEquals(3, job.WorkflowItems.Count);
			AssertEquals(2, jobHeader.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(1, workflow1.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(1, workflow2.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals(1, workflow1.TaskCollection.Count);
			AssertEquals(0, workflow2.TaskCollection.Count);
		}

		public void TestCompletionStatements()
		{
			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var taskTypeCategory = categorisedTaskTypes.AddNew();
			taskTypeCategory.Code = "ORG";
			var taskType = taskTypeCategory.TaskTypes.AddNew();
			taskType.Code = "COM";
			taskType.Description = (NoResString)"Completion Statement";
			taskType.IsCompletionStatementTaskType = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var job = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var completionStatement1 = jobHeader.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();
			completionStatement1.P9_NotesAsString = "Thing 1 is done";
			completionStatement1.P9_FH_ProcessHeader = workflow1.PK;
			completionStatement1.P9_Sequence = 1002;
			AssertEquals(taskType.Code, completionStatement1.P9_Type);

			var completionStatement2 = jobHeader.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();
			completionStatement2.P9_NotesAsString = "Thing 2 is done";
			completionStatement2.P9_FH_ProcessHeader = workflow1.PK;
			completionStatement2.P9_Sequence = 1001;
			AssertEquals(taskType.Code, completionStatement2.P9_Type);

			var completionStatement3 = jobHeader.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();
			completionStatement3.P9_NotesAsString = "Thing 3 is done";
			completionStatement3.P9_FH_ProcessHeader = workflow2.PK;
			AssertEquals(taskType.Code, completionStatement3.P9_Type);

			var task = job.WorkflowItems.AddNew();
			task.P9_Description = "Some task";
			task.P9_FH_ProcessHeader = workflow2.PK;

			AssertEquals("Thing 2 is done\r\nThing 1 is done", workflow1.CompletionCriteria);
			AssertEquals("Thing 3 is done", workflow2.CompletionCriteria);
		}

		public void TestCompletionStatements_ForWorkflow_ShouldHaveWorkflowFKSet()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var completionStatement1 = jobHeader.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();
			AssertEquals(ZGuid.Empty, completionStatement1.P9_FH_ProcessHeader);

			var completionStatement2 = workflow2.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();
			AssertEquals(workflow2.PK, completionStatement2.P9_FH_ProcessHeader);
		}

		#endregion

		#region Schedule

		public void TestCanReleaseAccordingToApprovedSchedule()
		{
			var tagDef = TagProvider.GetCCPMReleaseTagGroup(Factory);
			var rtrTag = TagProvider.GetCCPMReadyToReleaseTag(tagDef);
			var rblTag = TagProvider.GetCCPMReleaseBlockedTag(tagDef);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			Factory.Save();

			AssertEquals(false, CanReleaseAccordingToApprovedSchedule(workflow));

			var tagLink = BMSTestHelper.CreateRuleRunnerTagLink(rtrTag, jobHeader);
			Factory.Save();
			AssertEquals(true, CanReleaseAccordingToApprovedSchedule(workflow));

			tagLink.Delete();
			tagLink.Factory.Save();
			AssertEquals(false, CanReleaseAccordingToApprovedSchedule(workflow));

			tagLink = BMSTestHelper.CreateRuleRunnerTagLink(rtrTag, workflow);
			Factory.Save();
			AssertEquals(true, CanReleaseAccordingToApprovedSchedule(workflow));

			tagLink.Delete();
			tagLink.Factory.Save();
			AssertEquals(false, CanReleaseAccordingToApprovedSchedule(workflow));

			BMSTestHelper.CreateRuleRunnerTagLink(rblTag, workflow);
			Factory.Save();
			AssertEquals(false, CanReleaseAccordingToApprovedSchedule(workflow));

			BMSTestHelper.CreateRuleRunnerTagLink(rblTag, jobHeader);
			Factory.Save();
			AssertEquals(false, CanReleaseAccordingToApprovedSchedule(workflow));
		}

		bool CanReleaseAccordingToApprovedSchedule(ProcessHeader workflow)
		{
			var cache = new ApprovedCcpmReleaseCache(workflow.Factory, new[] { workflow.PK });

			return cache.IsCCPMReadyToRelease(workflow.PK);
		}

		#endregion

		#region IsChild/IsParent

		public void TestIsChildOrParentOf()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			jobHeader2.GetOrCreateLinkToParent(jobHeader1);

			AssertEquals(true, jobHeader1.IsParentOf(jobHeader2));
			AssertEquals(true, jobHeader2.IsChildOf(jobHeader1));

			AssertEquals(false, jobHeader2.IsParentOf(jobHeader1));
			AssertEquals(false, jobHeader1.IsChildOf(jobHeader2));
		}

		public void TestIsChildWorkflowWithinJobOf()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1");
			var childWorkflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "childWorkflow1");
			var grandchildWorkflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "grandchildWorkflow1");

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2");
			var childWorkflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "childWorkflow2");
			var grandchildWorkflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "grandchildWorkflow2");

			grandchildWorkflow1.GetOrCreateLinkToParent(childWorkflow1);
			grandchildWorkflow1.GetOrCreateLinkToParent(childWorkflow2);
			childWorkflow1.GetOrCreateLinkToParent(workflow1);
			childWorkflow1.GetOrCreateLinkToParent(workflow2);

			grandchildWorkflow2.GetOrCreateLinkToParent(childWorkflow1);
			grandchildWorkflow2.GetOrCreateLinkToParent(childWorkflow2);
			childWorkflow2.GetOrCreateLinkToParent(workflow1);
			childWorkflow2.GetOrCreateLinkToParent(workflow2);

			AssertEquals(workflow1, childWorkflow1.WorkflowParent);
			AssertEquals(childWorkflow1, grandchildWorkflow1.WorkflowParent);

			AssertEquals(false, grandchildWorkflow1.IsChildWorkflowWithinJobOf(grandchildWorkflow1));

			AssertEquals(true, grandchildWorkflow1.IsChildWorkflowWithinJobOf(childWorkflow1));
			AssertEquals(false, grandchildWorkflow1.IsChildWorkflowWithinJobOf(childWorkflow2));
			AssertEquals(false, grandchildWorkflow1.IsChildWorkflowWithinJobOf(workflow1));
			AssertEquals(false, grandchildWorkflow1.IsChildWorkflowWithinJobOf(workflow2));
			AssertEquals(false, grandchildWorkflow1.IsChildWorkflowWithinJobOf(jobHeader1));
			AssertEquals(false, grandchildWorkflow1.IsChildWorkflowWithinJobOf(jobHeader2));

			AssertEquals(true, childWorkflow1.IsChildWorkflowWithinJobOf(workflow1));
			AssertEquals(false, childWorkflow1.IsChildWorkflowWithinJobOf(workflow2));
			AssertEquals(false, childWorkflow1.IsChildWorkflowWithinJobOf(jobHeader1));
			AssertEquals(false, childWorkflow1.IsChildWorkflowWithinJobOf(jobHeader2));

			AssertEquals(false, workflow1.IsChildWorkflowWithinJobOf(jobHeader1));
			AssertEquals(false, workflow1.IsChildWorkflowWithinJobOf(jobHeader2));

			AssertEquals(false, grandchildWorkflow2.IsChildWorkflowWithinJobOf(childWorkflow1));
			AssertEquals(true, grandchildWorkflow2.IsChildWorkflowWithinJobOf(childWorkflow2));
			AssertEquals(false, grandchildWorkflow2.IsChildWorkflowWithinJobOf(workflow1));
			AssertEquals(false, grandchildWorkflow2.IsChildWorkflowWithinJobOf(workflow2));
			AssertEquals(false, grandchildWorkflow2.IsChildWorkflowWithinJobOf(jobHeader1));
			AssertEquals(false, grandchildWorkflow2.IsChildWorkflowWithinJobOf(jobHeader2));

			AssertEquals(false, childWorkflow2.IsChildWorkflowWithinJobOf(workflow1));
			AssertEquals(true, childWorkflow2.IsChildWorkflowWithinJobOf(workflow2));
			AssertEquals(false, childWorkflow2.IsChildWorkflowWithinJobOf(jobHeader1));
			AssertEquals(false, childWorkflow2.IsChildWorkflowWithinJobOf(jobHeader2));

			AssertEquals(false, workflow2.IsChildWorkflowWithinJobOf(jobHeader1));
			AssertEquals(false, workflow2.IsChildWorkflowWithinJobOf(jobHeader2));
		}

		#endregion

		#region Tasks Collection

		public void TestTaskCollectionIncludingChildWorkflowTasks()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var grandparentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "grandparentWorkflow");
			var parentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "parentWorkflow");
			var childWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "childWorkflow");

			childWorkflow.GetOrCreateLinkToParent(parentWorkflow);
			parentWorkflow.GetOrCreateLinkToParent(grandparentWorkflow);

			var task1 = BMSTestHelper.CreateTask(grandparentWorkflow, description: "grandparentTask");
			var task2 = BMSTestHelper.CreateTask(parentWorkflow, description: "parentTask");
			var task3 = BMSTestHelper.CreateTask(childWorkflow, description: "childTask");

			AssertContainsExactElementsInAnyOrder(new[] { task3 }, childWorkflow.TaskCollectionIncludingChildWorkflowTasks);
			AssertContainsExactElementsInAnyOrder(new[] { task3, task2 }, parentWorkflow.TaskCollectionIncludingChildWorkflowTasks);
			AssertContainsExactElementsInAnyOrder(new[] { task3, task2, task1 }, grandparentWorkflow.TaskCollectionIncludingChildWorkflowTasks);

			AssertContainsExactElementsInAnyOrder("Checking again to ensure loading parent collection does not set foreign key of child tasks", new[] { task3, task2 }, parentWorkflow.TaskCollectionIncludingChildWorkflowTasks);
			AssertContainsExactElementsInAnyOrder("Checking again to ensure loading parent collection does not set foreign key of child tasks", new[] { task3 }, childWorkflow.TaskCollectionIncludingChildWorkflowTasks);
		}

		public void TestCompletionStatementTasksIncludingChildWorkflowTasks()
		{
			BMSTestHelper.MakeCompletionStatementTaskType("ORG", "COM");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var grandparentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "grandparentWorkflow");
			var parentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "parentWorkflow");
			var childWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "childWorkflow");

			childWorkflow.GetOrCreateLinkToParent(parentWorkflow);
			parentWorkflow.GetOrCreateLinkToParent(grandparentWorkflow);

			var task1 = BMSTestHelper.CreateTask(grandparentWorkflow, description: "grandparentTask");
			var task2 = BMSTestHelper.CreateTask(parentWorkflow, description: "parentTask");
			var task3 = BMSTestHelper.CreateTask(childWorkflow, description: "childTask");

			var completionStatement1 = BMSTestHelper.CreateTask(grandparentWorkflow, taskType: "COM", description: "grandparentCom");
			var completionStatement2 = BMSTestHelper.CreateTask(parentWorkflow, taskType: "COM", description: "parentCom");
			var completionStatement3 = BMSTestHelper.CreateTask(childWorkflow, taskType: "COM", description: "childCom");

			AssertContainsExactElementsInAnyOrder(new[] { completionStatement3 }, childWorkflow.CompletionStatementTasksIncludingChildWorkflowTasks);
			AssertContainsExactElementsInAnyOrder(new[] { completionStatement3, completionStatement2 }, parentWorkflow.CompletionStatementTasksIncludingChildWorkflowTasks);
			AssertContainsExactElementsInAnyOrder(new[] { completionStatement3, completionStatement2, completionStatement1 }, grandparentWorkflow.CompletionStatementTasksIncludingChildWorkflowTasks);

			AssertContainsExactElementsInAnyOrder("Checking again to ensure loading parent collection does not set foreign key of child tasks", new[] { completionStatement3, completionStatement2 }, parentWorkflow.CompletionStatementTasksIncludingChildWorkflowTasks);
			AssertContainsExactElementsInAnyOrder("Checking again to ensure loading parent collection does not set foreign key of child tasks", new[] { completionStatement3 }, childWorkflow.CompletionStatementTasksIncludingChildWorkflowTasks);
		}

		public void TestTasks()
		{
			var system = Factory.New<BMSystem>();

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeader.GetForParent(dummy, Factory);
			var header1 = jobHeader.ProcessHeaders.AddNew();
			var header2 = jobHeader.ProcessHeaders.AddNew();

			var task1 = dummy.WorkflowItems.Tasks.AddNew();
			task1.P9_FH_ProcessHeader = header1.PK;
			var task2 = dummy.WorkflowItems.Tasks.AddNew();
			task2.P9_FH_ProcessHeader = header1.PK;
			var task3 = dummy.WorkflowItems.Tasks.AddNew();
			task3.P9_FH_ProcessHeader = header2.PK;
			var task4 = dummy.WorkflowItems.Tasks.AddNew();

			AssertEquals(2, header1.Tasks.Count());
			AssertEquals(1, header2.Tasks.Count());

			var jobHeaderOrphan = Factory.NewWithValidTestData<ProcessJobHeader>();
			var header3 = jobHeaderOrphan.ProcessHeaders.AddNew();
			AssertEquals(0, header3.Tasks.Count());
		}

		public void TestTasksCollection_WhileEnumeratingParentTasks()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var task1 = job.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;
			var task2 = job.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = workflow2.PK;

			foreach (ProcessTask task in job.WorkflowItems)
			{
				AssertEquals("can use TaskCollection while enumerating job.WorkflowItems", true, ((ProcessHeader)task.ProcessHeader).TaskCollection.Contains(task));
			}
		}

		public void TestAddTaskToChildCollection_AlsoAddsToParent()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var component1 = system.Components.AddNew();
			component1.FC_Name = "Super Buff";

			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<DummyWithWorkflow>(), Factory);
			var job = jobHeader.Parent;
			var workflow = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "Be the swag king.";

			AssertEquals(0, workflow.TaskCollection.Count);
			AssertEquals(0, job.WorkflowItems.Count);
			AssertEquals(0, jobHeader.TaskCollection.Count);
			AssertEquals(0, workflow2.TaskCollection.Count);

			workflow.TaskCollection.AddNew();

			AssertEquals(1, workflow.TaskCollection.Count);
			AssertEquals(1, job.WorkflowItems.Count);
			AssertEquals(1, jobHeader.TaskCollection.Count);
			AssertEquals(0, workflow2.TaskCollection.Count);

			var task = jobHeader.TaskCollection.AddNew();

			AssertEquals(2, job.WorkflowItems.Count);
			AssertEquals(2, jobHeader.TaskCollection.Count);
			AssertEquals(1, workflow.TaskCollection.Count);
			AssertEquals(0, workflow2.TaskCollection.Count);

			task.P9_FH_ProcessHeader = workflow2.PK;

			AssertEquals(2, job.WorkflowItems.Count);
			AssertEquals(2, jobHeader.TaskCollection.Count);
			AssertEquals(1, workflow.TaskCollection.Count);
			AssertEquals(1, workflow2.TaskCollection.Count);
		}

		public void TestTasksCollection_ShouldIncludeTasksInWorkflowOnly()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var task1 = job.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;
			var task2 = job.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = workflow2.PK;

			AssertEquals(1, workflow1.TaskCollection.Count);
			AssertEquals(task1, workflow1.TaskCollection[0]);

			AssertEquals(1, workflow2.TaskCollection.Count);
			AssertEquals(task2, workflow2.TaskCollection[0]);
		}

		public void TestTasksCollection_ForWorkflowWithNoParent()
		{
			var processHeader = Factory.NewWithValidTestData<ProcessHeader>();
			AssertNoExceptionThrown(() => { var v = processHeader.TaskCollection; });
		}

		public void TestTasksCollectionNotAccessingWorkflowParent_ShouldExcludeNonTasks()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var job = Factory.New<DummyWithWorkflow>();
			var workflow = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders[0];

			var task = job.WorkflowItems.Tasks.AddNew();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			var trigger = job.WorkflowItems.Triggers.AddNew();
			var exception = job.WorkflowItems.Exceptions.AddNew();

			AssertEquals(task, workflow.GetTasksWithoutAccessingWorkflowParent().Single());
		}

		#endregion

		#region Clone

		public void TestClone()
		{
			var group = Factory.New<GlbGroup>();
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "Coding 1";
			workflow1.FH_GG_ReleaseGroup = group.PK;

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow1.GetOrCreateDependencyLink(workflow2);

			var clonedWorkflow = ((IProcessHeader)workflow1).Clone();
			AssertEquals("Coding 1", clonedWorkflow.FH_CompletionStatement);
			AssertEquals(group.PK, clonedWorkflow.FH_GG_ReleaseGroup);
		}

		public void TestCloneWorkflow_ShouldCloneTasksAndIncrementWorkflowSequence()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<DummyWithWorkflow>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "Coding (1)";

			var task1 = jobHeader.Parent.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;
			task1.P9_Description = "task1";
			task1.P9_Sequence = 110;
			var task2 = jobHeader.Parent.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = workflow1.PK;
			task2.P9_Description = "task2";
			task2.P9_Sequence = 120;

			var workflow2 = workflow1.CloneWorkflow();
			AssertEquals("Coding (2)", workflow2.FH_CompletionStatement);

			AssertEquals(2, workflow2.TaskCollection.Count);

			AssertEquals("task1", workflow2.TaskCollection[0].P9_Description);
			AssertEquals(210, workflow2.TaskCollection[0].P9_Sequence);

			AssertEquals("task2", workflow2.TaskCollection[1].P9_Description);
			AssertEquals(220, workflow2.TaskCollection[1].P9_Sequence);
		}

		public void TestCloneWorkflow_ShouldAddWorkflowSequenceNumber()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<DummyWithWorkflow>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "Coding";

			var task1 = jobHeader.Parent.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;
			task1.P9_Description = "task1";
			task1.P9_Sequence = 110;
			var task2 = jobHeader.Parent.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = workflow1.PK;
			task2.P9_Description = "task2";
			task2.P9_Sequence = 120;

			var workflow2 = workflow1.CloneWorkflow();
			AssertEquals("Coding (1)", workflow2.FH_CompletionStatement);

			AssertEquals(2, workflow2.TaskCollection.Count);

			AssertEquals("task1", workflow2.TaskCollection[0].P9_Description);
			AssertEquals(210, workflow2.TaskCollection[0].P9_Sequence);

			AssertEquals("task2", workflow2.TaskCollection[1].P9_Description);
			AssertEquals(220, workflow2.TaskCollection[1].P9_Sequence);
		}

		public void TestCloneWorkflow_WithCompletionStatementContainingInteger_ShouldAddSequenceNumberCorrectly()
		{
			void AssertClonedCompletionStatement(string initialCompletionStatement, string expectedAfterFirstClone = null, string expectedAfterSecondClone = null)
			{
				var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, initialCompletionStatement);
				var clone = workflow.CloneWorkflow();
				AssertEquals(expectedAfterFirstClone ?? initialCompletionStatement + " (1)", clone.FH_CompletionStatement);
				clone = clone.CloneWorkflow();
				AssertEquals(expectedAfterSecondClone ?? initialCompletionStatement + " (2)", clone.FH_CompletionStatement);
			}

			AssertClonedCompletionStatement("2015");
			AssertClonedCompletionStatement("VS2015");
			AssertClonedCompletionStatement("The VS2015");
			AssertClonedCompletionStatement("There were 5 of us");
			AssertClonedCompletionStatement("5 of us were there");
			AssertClonedCompletionStatement("(5) of us were there");
			AssertClonedCompletionStatement("There were (5) of us");
			AssertClonedCompletionStatement("(1)", "(2)", "(3)");
		}

		public void TestCloneWorkflow_ShouldAddToFirstBMComponent()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var component1 = system.Components.AddNew();
			var component2 = system.Components.AddNew();

			var link = component1.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = component2.PK;

			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_FC_CurrentComponent = component2.PK;

			var clone = workflow.CloneWorkflow();
			AssertEquals(component1.PK, clone.FH_FC_CurrentComponent);
		}

		public void TestCloneWorkflow_ShouldAlsoCloneTags()
		{
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "PPL", "A group of hoopy froods");
			var danTag = BMSTestHelper.CreateTagMagnitude(tagGroup, "DAN");
			var daveTag = BMSTestHelper.CreateTagMagnitude(tagGroup, "DAV");
			var alexTag = BMSTestHelper.CreateTagMagnitude(tagGroup, "ALX");

			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			workflow.AddTag(danTag);
			workflow.AddTag(daveTag);

			var clone = workflow.CloneWorkflow();

			BMSTestCaseWithFactory.AssertTagApplied(clone, danTag);
			BMSTestCaseWithFactory.AssertTagApplied(clone, daveTag);
			BMSTestCaseWithFactory.AssertTagNotApplied(clone, alexTag);
		}

		public void TestCloneWorkflow_ShouldCloneTagsButNotWorkQueueMembership()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();

			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(newFactory).ProcessHeaders[0];
			workflow.AddTag(config.RedTag);
			workflow.AddTag(config.PlatinumTag);

			queue.AddMember(workflow);

			newFactory.Save();
			Factory.Save();

			var clone = workflow.CloneWorkflow();

			BMSTestCaseWithFactory.AssertTagApplied(clone, config.RedTag);
			BMSTestCaseWithFactory.AssertTagApplied(clone, config.PlatinumTag);
			BMSTestCaseWithFactory.AssertTagNotApplied(clone, queue);
		}

		public void TestCloneWorkflow_ShouldNotCloneTagsInRULUsageScope()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			workflow.AddTag(config.RedTag);
			workflow.AddTag(config.RuleTag);

			var clone = workflow.CloneWorkflow();

			BMSTestCaseWithFactory.AssertTagNotApplied(clone, config.RuleTag);
			BMSTestCaseWithFactory.AssertTagApplied(clone, config.RedTag);
		}

		public void TestCloneWorkflow_ShouldAlsoCloneCategories()
		{
			BMSTestHelper.AddWorkflowCategoryToRegistry("INQ", "TS1", "Test category");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "anything");
			workflow.FH_Category = "TS1";

			var clone = workflow.CloneWorkflow();

			AssertEquals("TS1", clone.FH_Category);
		}

		public void TestCloneWorkflow_ShouldAlsoCloneCompletionStatementTasks()
		{
			VisualBoardsTestCase.MakeCompletionStatementTaskType("DUM", "COM");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow");
			var regularTask = BMSTestHelper.CreateTask(workflow, description: "Regular Task");
			var completionStatementTask = BMSTestHelper.CreateTask(workflow, description: "Completion Statement", taskType: "COM");

			var clonedWorkflow = workflow.CloneWorkflow();
			AssertContainsExactElementsInAnyOrder("The cloned tasks should include the completion statement. SAD!", new[] { "Regular Task", "Completion Statement" }, clonedWorkflow.Tasks.Select(x => x.P9_Description));
		}

		public void TestCloneWorkflow_ShouldNotCloneTaskIterationPivots()
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CB", "DUM");
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			var task1 = BMSTestHelper.CreateTask(workflow);
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "CB");

			AssertEquals(ZString.Empty, task1.Iteration);
			AssertEquals(ZString.Empty, task2.Iteration);

			BMSTestCaseWithFactory.CreateQualityIteration(task1, task2, "Quality Iteration", shouldCreateWorkflowForIteration: false);
			var iterationTask = workflow.Tasks.Single(x => x.P9_Sequence == 3);
			var containmentBarrierIterationTask = workflow.Tasks.Single(x => x.P9_Sequence == 4);

			AssertEquals(ZString.Empty, task1.Iteration);
			AssertEquals(ZString.Empty, task2.Iteration);
			AssertEquals("1", iterationTask.Iteration);
			AssertEquals("1", containmentBarrierIterationTask.Iteration);

			var clone = workflow.CloneWorkflow();
			AssertContainsExactElementsInAnyOrder("The cloned tasks should not be associated with iterations, because there's no iteration associated with the cloned workflow.",
				new[] { ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty }, clone.Tasks.Select(x => x.Iteration));

			var allPivots = Factory.Load<IProcessTaskIterationLinkPivot>(new ZQuery());
			AssertEquals("No new iteration pivots should have been created for the clone.", 2, allPivots.Length);
		}

		#endregion

		#region Workflow Status

		public void TestIsOpen()
		{
			var job = Factory.New<DummyWithWorkflow>();
			var workflow = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders.AddNew();
			var task = job.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;

			Assert(workflow.IsOpen);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Assert(!workflow.IsOpen);
		}

		public void TestIsOpen_WhenTaskStatusIsCancelledButLowercase_ShouldReturnFalse()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow");
			var task = BMSTestHelper.CreateTask(workflow);
			((INeedRow)task).Row[ProcessTasksSchema.Constants.P9_Status] = "can";
			Factory.Save();

			AssertEquals("Precondition: we must have found a way to set the status to lowercase, otherwise this test is meaningless.", "can", task.P9_Status);
			AssertEquals("The workflow should be closed even though the task's status has lowercase letters. SAD!", false, workflow.IsOpen);
		}

		public void TestIsClosed()
		{
			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];

			foreach (CodeDescriptionPair status in new WorkflowStatusList())
			{
				workflow.FH_Status = status.Code;
				AssertNotEquals(ProcessHeader.GetOpenStatuses().Contains(status.Code), workflow.IsClosed);
			}
		}

		[TestDate(2015, 05, 06, 00, 30, 0)]
		public void TestClosingWorkflowCalculatesBufferPenetrationSmallPenetration()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			Factory.Save();

			var schematic = TestConfigsHelper.CreateSchematicTestConfig(Factory, "DUM");

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", currentComponent: schematic.Buffer);
			var task1_1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 1);
			Factory.Save();

			workflow1.FH_ReleaseDateTime = ZDateTime.UtcNow.AddMinutes(-10);

			jobHeader.TaskCollectionIncludingChildWorkflowTasks.Cast<ProcessTask>().ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Closed);

			Factory.Save();

			AssertEquals("0.0014m because it has aged 10 minutes but had a estimate of 2 minutes. Thus the aging amount is 8 minutes therefore 8/5600", 0.001m, workflow1.FH_BufferPenetrationPercentWhenCompleted);
		}

		public void TestFH_BufferPenetrationPercentWhenCompleted_ShouldIgnoreConcurrencyCheck()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");

			Factory.RefreshEnabled = false;
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			newFactory.RefreshEnabled = false;
			var reloadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);

			workflow.FH_BufferPenetrationPercentWhenCompleted = 0.2550m;
			Factory.Save();

			reloadedWorkflow.FH_BufferPenetrationPercentWhenCompleted = 1m;
			newFactory.Save();

			AssertEquals(0.2550m, workflow.FH_BufferPenetrationPercentWhenCompleted);
			AssertEquals(1m, reloadedWorkflow.FH_BufferPenetrationPercentWhenCompleted);
		}

		[TestDate(2015, 05, 06)]
		public void TestClosingWorkflowCalculatesBufferPenetration()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			Factory.Save();

			var schematic = TestConfigsHelper.CreateSchematicTestConfig(Factory, "DUM");

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", currentComponent: schematic.Buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", currentComponent: schematic.Bucket);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");
			workflow3.FH_FC_CurrentComponent = ZGuid.Empty;

			var task1_1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 1);
			var task2_1 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 1);
			var task3_1 = BMSTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 1);

			Factory.Save();

			AssertEquals(0m, workflow1.FH_BufferPenetrationPercentWhenCompleted);
			AssertEquals(0m, workflow2.FH_BufferPenetrationPercentWhenCompleted);
			AssertEquals(0m, workflow3.FH_BufferPenetrationPercentWhenCompleted);

			workflow1.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-2);
			workflow2.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-2);
			workflow3.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-2);

			jobHeader.TaskCollectionIncludingChildWorkflowTasks.Cast<ProcessTask>().ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Closed);

			Factory.Save();

			AssertEquals(0.166m, workflow1.FH_BufferPenetrationPercentWhenCompleted); // I am in a unit test
			AssertEquals(-1m, workflow2.FH_BufferPenetrationPercentWhenCompleted);
			AssertEquals(0m, workflow3.FH_BufferPenetrationPercentWhenCompleted);

			jobHeader.TaskCollectionIncludingChildWorkflowTasks.Cast<ProcessTask>().ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned);

			Factory.Save();

			workflow1.FH_ReleaseDateTime = ZDateTime.UtcNow.AddHours(-95);
			workflow2.FH_ReleaseDateTime = ZDateTime.UtcNow.AddHours(-95);
			workflow3.FH_ReleaseDateTime = ZDateTime.UtcNow.AddHours(-95);

			jobHeader.TaskCollectionIncludingChildWorkflowTasks.Cast<ProcessTask>().ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Closed);

			Factory.Save();
			AssertEquals(0.177m, workflow1.FH_BufferPenetrationPercentWhenCompleted); // I am in a unit test
			AssertEquals(-1m, workflow2.FH_BufferPenetrationPercentWhenCompleted);
			AssertEquals(0m, workflow3.FH_BufferPenetrationPercentWhenCompleted);

			TestDateAttribute.Date = ZDateTime.Now.AddMinutes(1).ToDateTime();
			jobHeader.TaskCollectionIncludingChildWorkflowTasks.Cast<ProcessTask>().ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned);

			Factory.Save();

			workflow1.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-15);
			workflow2.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-15);
			workflow3.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-15);

			jobHeader.TaskCollectionIncludingChildWorkflowTasks.Cast<ProcessTask>().ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Closed);

			Factory.Save();
			AssertEquals(0.916m, workflow1.FH_BufferPenetrationPercentWhenCompleted); // I am in a unit test
			AssertEquals(-1m, workflow2.FH_BufferPenetrationPercentWhenCompleted);
			AssertEquals(0m, workflow3.FH_BufferPenetrationPercentWhenCompleted);

			TestDateAttribute.Date = ZDateTime.Now.AddMinutes(1).ToDateTime();
			jobHeader.TaskCollectionIncludingChildWorkflowTasks.Cast<ProcessTask>().ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned);

			Factory.Save();

			workflow1.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-50);
			workflow2.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-50);
			workflow3.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-50);

			TestDateAttribute.Date = ZDateTime.Now.AddMinutes(1).ToDateTime();
			jobHeader.TaskCollectionIncludingChildWorkflowTasks.Cast<ProcessTask>().ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Closed);

			Factory.Save();

			AssertEquals(3m, workflow1.FH_BufferPenetrationPercentWhenCompleted); // I am in a unit test
			AssertEquals(-1m, workflow2.FH_BufferPenetrationPercentWhenCompleted);
			AssertEquals(0m, workflow3.FH_BufferPenetrationPercentWhenCompleted);
		}

		public void TestClosingWorkflowInBucket_ShouldSetBufferPenetrationToNegativeOne()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			Factory.Save();

			var schematic = TestConfigsHelper.CreateSchematicTestConfig(Factory, "DUM");

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", currentComponent: schematic.Buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", currentComponent: schematic.Bucket);

			workflow1.FH_Status = WorkflowStatusList.Codes.Closed;
			Factory.Save();
			AssertEquals(0m, workflow1.FH_BufferPenetrationPercentWhenCompleted);

			workflow2.FH_Status = WorkflowStatusList.Codes.Closed;
			Factory.Save();
			AssertEquals(-1m, workflow2.FH_BufferPenetrationPercentWhenCompleted);
		}

		public void TestStatus_ShouldConsiderChildItems()
		{
			var parentJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var parentWorkflow = parentJobHeader.ProcessHeaders[0];
			parentJobHeader.FH_CompletionStatement = "parentJobHeader";
			parentWorkflow.FH_CompletionStatement = "parentWorkflow";
			var parentTask = BMSTestHelper.CreateTask(parentWorkflow, GlbStaff.CurrentUser.GS_Code, 60);

			var childJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var childWorkflow = childJobHeader.ProcessHeaders[0];
			childJobHeader.FH_CompletionStatement = "childJobHeader";
			childWorkflow.FH_CompletionStatement = "childWorkflow";
			var childTask = BMSTestHelper.CreateTask(childWorkflow, GlbStaff.CurrentUser.GS_Code, 60);

			childJobHeader.GetOrCreateLinkToParent(parentJobHeader);

			AssertEquals(true, childJobHeader.IsChildOf(parentJobHeader));

			var postReq = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			postReq.FH_CompletionStatement = "postReq";
			var task = BMSTestHelper.CreateTask(postReq, GlbStaff.CurrentUser.GS_Code, 60);
			parentJobHeader.MakePrerequisiteOf(postReq);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Post-req should still have open pre-req", true, postReq.HasOpenPrerequisites);
				AssertEquals("Parent job header has open child jobs", true, parentJobHeader.IsOpen);
				AssertEquals("Child job header has open tasks", true, childJobHeader.IsOpen);

				AssertEquals("Parent workflow has open task", true, parentWorkflow.IsOpen);
				AssertEquals("Child workflow has open task", true, childWorkflow.IsOpen);
			});

			parentTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Post-req should still have open pre-req", true, postReq.HasOpenPrerequisites);
				AssertEquals("Parent job header has open child jobs", true, parentJobHeader.IsOpen);
				AssertEquals("Child job header has open tasks", true, childJobHeader.IsOpen);

				AssertEquals("Parent workflow is closed", false, parentWorkflow.IsOpen);
				AssertEquals("Child workflow has open task", true, childWorkflow.IsOpen);
			});

			childTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Pre-reqs have now cleared", false, postReq.HasOpenPrerequisites);
				AssertEquals("Child jobs are now all closed", false, parentJobHeader.IsOpen);
				AssertEquals("Child job header tasks are all closed", false, childJobHeader.IsOpen);

				AssertEquals("Parent workflow is closed", false, parentWorkflow.IsOpen);
				AssertEquals("Child workflow is closed", false, childWorkflow.IsOpen);
			});
		}

		public void TestStatus_ShouldNotClose_WhenAnyTaskNotClosed()
		{
			var schematic = TestConfigsHelper.CreateSchematicTestConfig(Factory, "DUM");
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow", currentComponent: schematic.Buffer);

			BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 1, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 1, taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled);
			BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 1, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);

			workflow.FH_Status = WorkflowStatusList.Codes.Closed;

			AssertEquals(WorkflowStatusList.Codes.Open, workflow.FH_Status);
		}

		public void TestStatus_ShouldConsiderChildItemsAtNLevels()
		{
			var grandparentJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var grandparentWorkflow = grandparentJobHeader.ProcessHeaders[0];
			grandparentJobHeader.FH_CompletionStatement = "grandparentJobHeader";
			grandparentWorkflow.FH_CompletionStatement = "grandparentWorkflow";
			var grandparentTask = BMSTestHelper.CreateTask(grandparentWorkflow, GlbStaff.CurrentUser.GS_Code, 60);

			var parentJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var parentWorkflow = parentJobHeader.ProcessHeaders[0];
			parentJobHeader.FH_CompletionStatement = "parentJobHeader";
			parentWorkflow.FH_CompletionStatement = "parentWorkflow";
			var parentTask = BMSTestHelper.CreateTask(parentWorkflow, GlbStaff.CurrentUser.GS_Code, 60);

			var childJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var childWorkflow = childJobHeader.ProcessHeaders[0];
			childJobHeader.FH_CompletionStatement = "childJobHeader";
			childWorkflow.FH_CompletionStatement = "childWorkflow";
			var childTask = BMSTestHelper.CreateTask(childWorkflow, GlbStaff.CurrentUser.GS_Code, 60);

			childJobHeader.GetOrCreateLinkToParent(parentJobHeader);
			parentJobHeader.GetOrCreateLinkToParent(grandparentJobHeader);

			AssertEquals(true, childJobHeader.IsChildOf(parentJobHeader));
			AssertEquals(true, parentJobHeader.IsChildOf(grandparentJobHeader));

			var postReq = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			postReq.FH_CompletionStatement = "postReq";
			var task = BMSTestHelper.CreateTask(postReq, GlbStaff.CurrentUser.GS_Code, 60);
			grandparentJobHeader.MakePrerequisiteOf(postReq);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Post-req should still have open pre-req", true, postReq.HasOpenPrerequisites);
				AssertEquals("Grandparent job header has open child jobs", true, grandparentJobHeader.IsOpen);
				AssertEquals("Parent job header has open child jobs", true, parentJobHeader.IsOpen);
				AssertEquals("Child job header has open tasks", true, childJobHeader.IsOpen);

				AssertEquals("Grandparent workflow has open task", true, grandparentWorkflow.IsOpen);
				AssertEquals("Parent workflow has open task", true, parentWorkflow.IsOpen);
				AssertEquals("Child workflow has open task", true, childWorkflow.IsOpen);
			});

			grandparentTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Post-req should still have open pre-req", true, postReq.HasOpenPrerequisites);
				AssertEquals("Grandparent job header has open child jobs", true, grandparentJobHeader.IsOpen);
				AssertEquals("Parent job header has open child jobs", true, parentJobHeader.IsOpen);
				AssertEquals("Child job header has open tasks", true, childJobHeader.IsOpen);

				AssertEquals("Grandparent workflow is closed", false, grandparentWorkflow.IsOpen);
				AssertEquals("Parent workflow has open task", true, parentWorkflow.IsOpen);
				AssertEquals("Child workflow has open task", true, childWorkflow.IsOpen);
			});

			parentTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Post-req should still have open pre-req", true, postReq.HasOpenPrerequisites);
				AssertEquals("Grandparent job header has open child jobs", true, grandparentJobHeader.IsOpen);
				AssertEquals("Parent job header has open child jobs", true, parentJobHeader.IsOpen);
				AssertEquals("Child job header has open tasks", true, childJobHeader.IsOpen);

				AssertEquals("Grandparent workflow is closed", false, grandparentWorkflow.IsOpen);
				AssertEquals("Parent workflow is closed", false, parentWorkflow.IsOpen);
				AssertEquals("Child workflow has open task", true, childWorkflow.IsOpen);
			});

			childTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Pre-reqs have now cleared", false, postReq.HasOpenPrerequisites);
				AssertEquals("grandparentJobHeader's child jobs are now all closed", false, grandparentJobHeader.IsOpen);
				AssertEquals("parentJobHeader's child jobs are now all closed", false, parentJobHeader.IsOpen);
				AssertEquals("Child job header tasks are all closed", false, childJobHeader.IsOpen);

				AssertEquals("Grandparent workflow is closed", false, grandparentWorkflow.IsOpen);
				AssertEquals("Parent workflow is closed", false, parentWorkflow.IsOpen);
				AssertEquals("Child workflow is closed", false, childWorkflow.IsOpen);
			});
		}

		public void TestCurrentTasksStatus()
		{
			var resource1 = Factory.NewWithValidTestData<GlbStaff>();
			resource1.GS_FullName = "Frodo Baggins";
			var resource2 = Factory.NewWithValidTestData<GlbStaff>();
			resource2.GS_FullName = "Bilbo Baggins";

			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];

			var currentTask1 = BMSTestHelper.CreateTask(workflow, resource1.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Working, sequence: 1);
			var currentTask2 = BMSTestHelper.CreateTask(workflow, resource2.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Suspended, sequence: 1);
			var closedTask = BMSTestHelper.CreateTask(workflow, resource2.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 1);
			var nonCurrentTask = BMSTestHelper.CreateTask(workflow, resource2.GS_Code, 60, sequence: 2);

			AssertEquals("Working (Frodo Baggins), Suspended (Bilbo Baggins)", workflow.CurrentTasksStatus);
		}

		[TestDate(2014, 2, 28)]
		[TestDateIncremental(seconds: 1)]
		public void TestPrerequisiteStatus()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var task1 = BMSTestHelper.CreateTask(workflow1);

			AssertEquals(WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals(WorkflowPrerequisiteStatusList.Codes.ClearedToStart, workflow1.PrerequisiteStatus);
			AssertEquals("No open prerequisites", workflow1.PrerequisiteStatusShortDescription);

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Neep");
			var task2 = BMSTestHelper.CreateTask(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow1);

			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Blocked, workflow1.FH_Status);
			AssertEquals(WorkflowPrerequisiteStatusList.Codes.Blocked, workflow1.PrerequisiteStatus);
			AssertEquals("Blocked", workflow1.PrerequisiteStatusShortDescription);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.ClosedWithOpenPrerequisites, workflow1.FH_Status);
			AssertEquals(WorkflowPrerequisiteStatusList.Codes.ClosedWithOpenPrerequisites, workflow1.PrerequisiteStatus);
			AssertEquals("Workflow is closed with open prerequisites", workflow1.PrerequisiteStatusShortDescription);

			workflow1.FH_StaggeredReleaseDelayExpiry = ZDateTime.UtcNow.AddHours(-1);

			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.ClosedWithOpenPrerequisites, workflow1.FH_Status);
			AssertEquals(WorkflowPrerequisiteStatusList.Codes.ClearedToRelease, workflow1.PrerequisiteStatus);
			AssertEquals("Workflow is complete, but is cleared for a staggered release", workflow1.PrerequisiteStatusShortDescription);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Blocked, workflow1.FH_Status);
			AssertEquals(WorkflowPrerequisiteStatusList.Codes.ClearedToRelease, workflow1.PrerequisiteStatus);
			AssertEquals("Cleared for a staggered release", workflow1.PrerequisiteStatusShortDescription);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			workflow1.FH_StaggeredReleaseDelayExpiry = ZDateTime.Empty;
			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Closed, workflow1.FH_Status);
			AssertEquals(WorkflowPrerequisiteStatusList.Codes.ClearedToStart, workflow1.PrerequisiteStatus);
			AssertEquals("Workflow is complete", workflow1.PrerequisiteStatusShortDescription);
		}

		[TestDate(2017, 10, 03)]
		public void TestClosedWithOpenPrerequisiteStatusFHStatus_ReducedProcessHeaderDbHits()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var prereqWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Prereq");
			var mainWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Main");
			var postreqWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "Postreq");

			var prereqTask = BMSTestHelper.CreateTask(prereqWorkflow);
			var mainTask = BMSTestHelper.CreateTask(mainWorkflow);
			var postreqTask = BMSTestHelper.CreateTask(postreqWorkflow);

			prereqTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			mainTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			postreqTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var dbHits = new Dictionary<string, int>()
				{
					{ ProcessTasksSchema.Constants.TableName, 6 },
					{ ProcessHeaderLinkSchema.Constants.TableName, 2 }
				};

			using (AssertDbHitsForAllFactories(dbHits, ignoreUnspecified: true))
			{
				prereqWorkflow.GetOrCreateDependencyLink(mainWorkflow);
				mainWorkflow.GetOrCreateDependencyLink(postreqWorkflow);

				Factory.Save();

				AssertWorkflowIsOpen("PrereqWorkflow should be open", prereqWorkflow);
				AssertWorkflowIsClosedWithOpenPrerequisites("MainWorkflow should be closed with open prerequisite", mainWorkflow);
				AssertWorkflowIsBlocked("PostreqWorkflow should be blocked", postreqWorkflow);

				var factory2 = new BusinessObjectFactory();
				var reloadTask = factory2.Load<ProcessTask>(postreqTask.PK);

				reloadTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				factory2.Save();
			}
		}

		public void TestClosedWithOpenPrerequisitesStatus_SinglePrereqLevel()
		{
			var workflow1 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow2");

			var task1 = BMSTestHelper.CreateTask(workflow1);
			var task2 = BMSTestHelper.CreateTask(workflow2);

			Factory.Save();

			workflow1.FH_Status = WorkflowStatusList.Codes.Open;
			workflow2.FH_Status = WorkflowStatusList.Codes.Open;

			// assertion for precondition fh_status
			AssertWorkflowIsOpen("Precondition: Workflow1 is open", workflow1);
			AssertWorkflowIsOpen("Precondition: Workflow2 is open", workflow2);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			AssertWorkflowIsOpen("Precondition: Workflow1 is open", workflow1);
			AssertWorkflowIsClosed("Precondition: Workflow2 is now closed", workflow2);

			// create prereq relationship workflow1->workflow2
			workflow1.GetOrCreateDependencyLink(workflow2);

			Factory.Save();

			AssertWorkflowIsOpen("Postcondition: Workflow1 is still open", workflow1);
			AssertWorkflowIsClosedWithOpenPrerequisites("Postcondition: Workflow2 is now closed with open prerequisites", workflow2);

			var workflow3 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow3");
			var workflow4 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow4");

			var task3 = BMSTestHelper.CreateTask(workflow3);
			var task4 = BMSTestHelper.CreateTask(workflow4);

			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			workflow1.GetOrCreateDependencyLink(workflow3);
			workflow1.GetOrCreateDependencyLink(workflow4);

			Factory.Save();

			AssertWorkflowIsClosedWithOpenPrerequisites("Postcondition: Workflow3 is now closed with open prerequisites", workflow3);
			AssertWorkflowIsClosedWithOpenPrerequisites("Postcondition: Workflow4 is now closed with open prerequisites", workflow4);
		}

		public void TestClosedWithOpenPrerequisitesStatus_MultiplePrereqLevel()
		{
			var workflow1 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow3");
			var workflow4 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow4");

			var task1 = BMSTestHelper.CreateTask(workflow1);
			var task2 = BMSTestHelper.CreateTask(workflow2);
			var task3 = BMSTestHelper.CreateTask(workflow3);
			var task4 = BMSTestHelper.CreateTask(workflow4);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			AssertWorkflowIsOpen("Precondition: Workflow1 is open", workflow1);
			AssertWorkflowIsClosed("Precondition: Workflow2 is closed", workflow2);
			AssertWorkflowIsClosed("Precondition: Workflow3 is closed", workflow3);
			AssertWorkflowIsClosed("Precondition: Workflow4 is closed", workflow4);

			workflow3.GetOrCreateDependencyLink(workflow4);
			workflow2.GetOrCreateDependencyLink(workflow3);
			workflow1.GetOrCreateDependencyLink(workflow2);

			Factory.Save();

			AssertWorkflowIsOpen("Postcondition: Workflow1 is still open", workflow1);
			AssertWorkflowIsClosedWithOpenPrerequisites("Postcondition: Workflow2 is now closed with open prerequisites", workflow2);
			AssertWorkflowIsClosedWithOpenPrerequisites("Postcondition: Workflow3 is now closed with open prerequisites", workflow3);
			AssertWorkflowIsClosedWithOpenPrerequisites("Postcondition: Workflow4 is now closed with open prerequisites", workflow4);
		}

		public void TestClosedWithOpenPrerequisiteStatus_ReopenTask()
		{
			var workflow1 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow3");
			var workflow4 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow4");

			var task1 = BMSTestHelper.CreateTask(workflow1);
			var task2 = BMSTestHelper.CreateTask(workflow2);
			var task3 = BMSTestHelper.CreateTask(workflow3);
			var task4 = BMSTestHelper.CreateTask(workflow4);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			AssertWorkflowIsClosed("Precondition: Workflow1 is closed", workflow1);
			AssertWorkflowIsClosed("Precondition: Workflow2 is closed", workflow2);
			AssertWorkflowIsClosed("Precondition: Workflow3 is closed", workflow3);
			AssertWorkflowIsClosed("Precondition: Workflow4 is closed", workflow4);

			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow3);
			workflow3.GetOrCreateDependencyLink(workflow4);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			Factory.Save();

			AssertWorkflowIsOpen("Postcondition: Workflow1 is open", workflow1);
			AssertWorkflowIsClosedWithOpenPrerequisites("Postcondition: Workflow2 is now closed with open prerequisites", workflow2);
			AssertWorkflowIsClosedWithOpenPrerequisites("Postcondition: Workflow3 is now closed with open prerequisites", workflow3);
			AssertWorkflowIsClosedWithOpenPrerequisites("Postcondition: Workflow4 is now closed with open prerequisites", workflow4);
		}
		// create tests for parent child relationship

		[TestDate(2014, 2, 28)]
		public void TestWorkflowJobStatusEvents()
		{
			BMSTestHelper.MakeCompletionStatementTaskType("ORG", "COM");
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");

			var templateWorkflow1 = (ProcessHeader)template.ProcessHeaders.AddNew();
			templateWorkflow1.FH_CompletionStatement = "workflow1";
			var templateWorkflow2 = (ProcessHeader)template.ProcessHeaders.AddNew();
			templateWorkflow2.FH_CompletionStatement = "workflow2";
			var templateTask1_1 = template.WorkflowItems.Tasks.AddNew();
			templateTask1_1.P9_FH_ProcessHeader = templateWorkflow1.PK;
			templateTask1_1.P9_Description = "Task 1_1";
			var templateTask1_2 = template.WorkflowItems.Tasks.AddNew();
			templateTask1_2.P9_FH_ProcessHeader = templateWorkflow1.PK;
			templateTask1_2.P9_Description = "Task 1_2";
			var templateTask2_1 = template.WorkflowItems.Tasks.AddNew();
			templateTask2_1.P9_FH_ProcessHeader = templateWorkflow2.PK;
			templateTask2_1.P9_Description = "Task 2_1";
			var templateTask2_2 = template.WorkflowItems.Tasks.AddNew();
			templateTask2_2.P9_FH_ProcessHeader = templateWorkflow2.PK;
			templateTask2_2.P9_Description = "Task 2_2";

			var templateLink = BMSTestHelper.CreateDependencyLink(template, templateWorkflow1, templateWorkflow2);

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(org);

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(org, Factory);
			var workflow1 = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "workflow1");
			var workflow2 = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "workflow2");
			AssertEquals(2, workflow1.TaskCollection.Count);
			AssertEquals(2, workflow2.TaskCollection.Count);

			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, jobHeader.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, workflow2.FH_Status);

			var startingLogsCount = workflow1.Logs.DatabaseCount;
			workflow1.TaskCollection[0].P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();
			AssertEquals("There is still an open task, so should not add any events yet", startingLogsCount, workflow1.Logs.DatabaseCount);

			AssertEquals(WorkflowStatusList.Codes.Open, jobHeader.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, workflow2.FH_Status);

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			workflow1.TaskCollection[1].P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, jobHeader.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Closed, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, workflow2.FH_Status);

			AssertEquals(startingLogsCount + 1, workflow1.Logs.DatabaseCount);
			var lastLog = GetLastLog(workflow1);
			AssertEquals("Closing the last open task should add the Closed event", Events.JobCloseCode, lastLog.SL_SE_NKEvent);

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			var newCompletionStatement = workflow1.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();
			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, jobHeader.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, workflow2.FH_Status);

			AssertEquals(startingLogsCount + 2, workflow1.Logs.DatabaseCount);
			lastLog = GetLastLog(workflow1);
			AssertEquals("Creating a new task should add the Opened event", Events.JobOpenCode, lastLog.SL_SE_NKEvent);

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			newCompletionStatement.Delete();
			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, jobHeader.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Closed, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, workflow2.FH_Status);

			AssertEquals(startingLogsCount + 3, workflow1.Logs.DatabaseCount);
			lastLog = GetLastLog(workflow1);
			AssertEquals("Deleting the last open task should add the Closed event", Events.JobCloseCode, lastLog.SL_SE_NKEvent);

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			workflow1.TaskCollection[1].P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, jobHeader.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, workflow2.FH_Status);

			AssertEquals(startingLogsCount + 4, workflow1.Logs.DatabaseCount);
			lastLog = GetLastLog(workflow1);
			AssertEquals("Re-opening a task should add the Opened event", Events.JobOpenCode, lastLog.SL_SE_NKEvent);

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			workflow1.TaskCollection[1].P9_FH_ProcessHeader = workflow2.PK;
			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, jobHeader.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Closed, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, workflow2.FH_Status);

			AssertEquals(startingLogsCount + 5, workflow1.Logs.DatabaseCount);
			lastLog = GetLastLog(workflow1);
			AssertEquals("Moving last open task to another workflow should add the Closed event", Events.JobCloseCode, lastLog.SL_SE_NKEvent);

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			foreach (var task in workflow2.Tasks)
			{
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			}

			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Closed, jobHeader.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Closed, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Closed, workflow2.FH_Status);
		}

		public void TestWorkflowStatus_SeveralStepsInDependencyChain()
		{
			var workflow1 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow3");
			var workflow4 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow4");

			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			var task3 = BMSTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 60);
			var task4 = BMSTestHelper.CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, workflow2.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, workflow3.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, workflow4.FH_Status);

			workflow1.MakePrerequisiteOf(workflow2);
			workflow2.MakePrerequisiteOf(workflow3);
			workflow3.MakePrerequisiteOf(workflow4);

			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, workflow2.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, workflow3.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, workflow4.FH_Status);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.ClosedWithOpenPrerequisites, workflow2.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.ClosedWithOpenPrerequisites, workflow3.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, workflow4.FH_Status);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Closed, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Closed, workflow2.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Closed, workflow3.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, workflow4.FH_Status);
		}

		[TestDate(2014, 9, 4)]
		public void TestRemoveDependency_ShouldUpdateStatus()
		{
			var workflow1 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow2");

			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, workflow2.FH_Status);

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			var link = workflow1.GetOrCreateDependencyLink(workflow2);
			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, workflow2.FH_Status);

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			link.Delete();
			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, workflow2.FH_Status);
		}

		[TestDate(2014, 9, 4)]
		public void TestChangeDependencyWorkflow_ShouldUpdateStatus()
		{
			var workflow1 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow3");

			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			var task3 = BMSTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, workflow2.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, workflow3.FH_Status);

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			var link = workflow1.GetOrCreateDependencyLink(workflow2);
			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, workflow2.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, workflow3.FH_Status);

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			link.FP_FH_HeaderTo = workflow3.PK;
			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, workflow2.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, workflow3.FH_Status);
		}

		public void TestDeletePrerequisiteWorkflow_ShouldUpdateStatus()
		{
			var workflow1 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow2");

			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Open, workflow2.FH_Status);

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			var link = workflow1.GetOrCreateDependencyLink(workflow2);
			Factory.Save();

			AssertEquals(WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals(WorkflowStatusList.Codes.Blocked, workflow2.FH_Status);

			task1.Delete();
			workflow1.Delete();
			Factory.Save();

			AssertEquals(true, workflow1.IsDeleted);
			AssertEquals(WorkflowStatusList.Codes.Open, workflow2.FH_Status);
		}

		[TestDate(2014, 2, 28)]
		public void TestWorkflowStatusEvents_OnJobHeader()
		{
			BMSTestHelper.MakeCompletionStatementTaskType("ORG", "COM");
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow1 = (ProcessHeader)template.ProcessHeaders.AddNew();
			templateWorkflow1.FH_CompletionStatement = "workflow1";
			var templateWorkflow2 = (ProcessHeader)template.ProcessHeaders.AddNew();
			templateWorkflow2.FH_CompletionStatement = "workflow2";
			var templateTask1_1 = template.WorkflowItems.Tasks.AddNew();
			templateTask1_1.P9_FH_ProcessHeader = templateWorkflow1.PK;
			var templateTask1_2 = template.WorkflowItems.Tasks.AddNew();
			templateTask1_2.P9_FH_ProcessHeader = templateWorkflow1.PK;
			var templateTask2_1 = template.WorkflowItems.Tasks.AddNew();
			templateTask2_1.P9_FH_ProcessHeader = templateWorkflow2.PK;
			var templateTask2_2 = template.WorkflowItems.Tasks.AddNew();
			templateTask2_2.P9_FH_ProcessHeader = templateWorkflow2.PK;

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(org);

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(org, Factory);
			var workflow1 = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "workflow1");
			var workflow2 = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "workflow2");
			AssertEquals(2, workflow1.TaskCollection.Count);
			AssertEquals(2, workflow2.TaskCollection.Count);
			Factory.Save();

			var startingLogsCount = jobHeader.Logs.DatabaseCount;
			workflow1.TaskCollection[0].P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			workflow1.TaskCollection[1].P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();
			AssertEquals("There is still an open workflow, so should not add any events yet", startingLogsCount, jobHeader.Logs.DatabaseCount);

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			workflow2.TaskCollection[0].P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			workflow2.TaskCollection[1].P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertEquals(startingLogsCount + 1, jobHeader.Logs.DatabaseCount);
			var lastLog = GetLastLog(jobHeader);
			AssertEquals("Closing the last open workflow should add the Closed event to the job", Events.JobCloseCode, lastLog.SL_SE_NKEvent);

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			var newCompletionStatement = workflow1.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();
			Factory.Save();

			AssertEquals(startingLogsCount + 2, jobHeader.Logs.DatabaseCount);
			lastLog = GetLastLog(jobHeader);
			AssertEquals("Creating a new task should add the Opened event", Events.JobOpenCode, lastLog.SL_SE_NKEvent);

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			newCompletionStatement.Delete();
			Factory.Save();

			AssertEquals(startingLogsCount + 3, jobHeader.Logs.DatabaseCount);
			lastLog = GetLastLog(jobHeader);
			AssertEquals("Deleting the last open task should add the Closed event", Events.JobCloseCode, lastLog.SL_SE_NKEvent);

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			workflow1.TaskCollection[1].P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();

			AssertEquals(startingLogsCount + 4, jobHeader.Logs.DatabaseCount);
			lastLog = GetLastLog(jobHeader);
			AssertEquals("Re-opening a task should add the Opened event", Events.JobOpenCode, lastLog.SL_SE_NKEvent);

			TestDateAttribute.Date = TestDateAttribute.Date.AddSeconds(1);
			var taskToDelete = workflow1.TaskCollection[1];
			taskToDelete.P9_FH_ProcessHeader = ZGuid.Empty;
			taskToDelete.Delete();
			Factory.Save();

			AssertEquals(startingLogsCount + 5, jobHeader.Logs.DatabaseCount);
			lastLog = GetLastLog(jobHeader);
			AssertEquals("Moving last open task to an invalid workflow, then deleting it should add the Closed event", Events.JobCloseCode, lastLog.SL_SE_NKEvent);
		}

		[TestDate(2014, 2, 28)]
		public void TestWorkflowStatusEvents_OnTemplateHeader()
		{
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow = (ProcessHeader)template.ProcessHeaders.AddNew();
			var templateTask = templateWorkflow.TaskCollection.AddNew();

			Factory.Save();

			AssertEquals(0, templateWorkflow.Logs.GetAllLogs().Count);
		}

		[TestDate(2014, 4, 23)]
		public void TestWorkflowStatusEvents_CreatingJobWithOneTask_ShouldNotLogMultipleEvents()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			Factory.Save();

			var jobStatusEvents = GetStatusChangeLogs(jobHeader);
			var workflowStatusEvents = GetStatusChangeLogs(workflow);

			AssertEquals(1, jobStatusEvents.Length);
			AssertEquals(1, workflowStatusEvents.Length);

			AssertEquals(Events.JobOpenCode, jobStatusEvents[0].SL_SE_NKEvent);
			AssertEquals(Events.JobOpenCode, workflowStatusEvents[0].SL_SE_NKEvent);
		}

		public void TestTaskLowestOpenSequenceNumber_UserContextAgnostic()
		{
			DummyWorkflowDescriptor.Instance.SetAreTasksCompanySpecific(true);
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Header");

			var task1 = BMSTestHelper.CreateTask(workflow);
			task1.P9_ShareTasksForAllCompanies = false;

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			Factory.Save();
			AssertNotEquals(workflow.FH_TaskLowestOpenSequenceNumber, -1);

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var factory = new BusinessObjectFactory();
				var dummy = factory.Load<DummyWithWorkflow>(jobHeader.FH_ParentId);
				AssertEquals("Can't see the task in another company", 0, dummy.WorkflowItems.Tasks.Count);
				workflow = factory.Load<ProcessHeader>(workflow.PK);
				workflow.FH_CompletionStatement = "Foo";
				factory.Save();

				AssertNotEquals("Even though we can't see the tasks, the sequence number shouldn't change.", workflow.FH_TaskLowestOpenSequenceNumber, -1);
			}
		}

		public void TestTaskLowestOpenSequenceNumber()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Header");

			var task1 = BMSTestHelper.CreateTask(workflow, description: "Task 1");
			task1.P9_Sequence = 1;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var task2 = BMSTestHelper.CreateTask(workflow, description: "Task 2");
			task2.P9_Sequence = 2;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			var task3 = BMSTestHelper.CreateTask(workflow, description: "Task 3");
			task3.P9_Sequence = 3;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			var task4 = BMSTestHelper.CreateTask(workflow, description: "Task 4");
			task4.P9_Sequence = 5;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task5 = BMSTestHelper.CreateTask(workflow, description: "Task 5");
			task5.P9_Sequence = 8;
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			AssertEquals(workflow.FH_TaskLowestOpenSequenceNumber, -1);

			Factory.Save();
			workflow.Reload();
			AssertEquals(workflow.FH_TaskLowestOpenSequenceNumber, 3);

			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			workflow.Reload();
			AssertEquals(workflow.FH_TaskLowestOpenSequenceNumber, 5);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();
			workflow.Reload();
			AssertEquals(workflow.FH_TaskLowestOpenSequenceNumber, 2);

			task2.P9_Sequence = 13;
			Factory.Save();
			workflow.Reload();
			AssertEquals(workflow.FH_TaskLowestOpenSequenceNumber, 5);

			task4.P9_Type = "MIL";
			Factory.Save();
			workflow.Reload();
			AssertEquals(workflow.FH_TaskLowestOpenSequenceNumber, 8);

			task5.P9_ParentTableCode = "P0";
			Factory.Save();
			workflow.Reload();
			AssertEquals(workflow.FH_TaskLowestOpenSequenceNumber, 13);
		}

		[TestDate(2022, 2, 24, 4, 0, 0)]
		public void TestEstimatedMinutesToComplete()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Header");

			var task1 = BMSTestHelper.CreateTask(workflow);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var task2 = BMSTestHelper.CreateTask(workflow);
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			var task3 = BMSTestHelper.CreateTask(workflow);
			task3.P9_EstimatedTimeToComplete = new ZDateTime(2022, 1, 1, 0, 20, 0);
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			var task4 = BMSTestHelper.CreateTask(workflow);
			task4.P9_EstDuration = new ZDateTime(2022, 1, 1, 2, 40, 0);
			task4.P9_EstimateVariationFactor = 1.5;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			var task5 = BMSTestHelper.CreateTask(workflow);
			task5.P9_EstDuration = new ZDateTime(2022, 1, 1, 0, 50, 0);
			task5.P9_EstimateVariationFactor = 1;
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			Factory.Save();
			AssertEquals(270, workflow.FH_RemainingMinutesToComplete);

			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task4.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			workflow.Reload();
			AssertEquals(50, workflow.FH_RemainingMinutesToComplete);
		}

		[TestDate(2017, 10, 03)]
		public void TestIsOpen_DbHits()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "INQ");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Workflow");
			var task1 = BMSTestHelper.CreateTask(workflow, description: "Task 1");
			var task2 = BMSTestHelper.CreateTask(workflow, description: "Task 2");

			Factory.Save();

			using (AssertDbHitsForAllFactories("Closing task1", new Dictionary<string, int> { { ProcessTasksSchema.Constants.TableName, 3 } }, ignoreUnspecified: true))
			{
				task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();
			}

			using (AssertDbHitsForAllFactories("Closing task2", new Dictionary<string, int> { { ProcessTasksSchema.Constants.TableName, 3 } }, ignoreUnspecified: true))
			{
				task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();
			}
		}

		StmALog GetLastLog(ProcessHeader workflow)
		{
			return workflow.Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(l => l.SL_EventTime).FirstOrDefault();
		}

		public static StmALog[] GetStatusChangeLogs(ProcessHeader processHeader)
		{
			return processHeader.Logs.GetAllLogs().Cast<StmALog>().Where(l => l.SL_SE_NKEvent == Events.JobOpenCode || l.SL_SE_NKEvent == Events.JobCloseCode).ToArray();
		}

		#endregion

		#region BusinessObject Overrides

		public void TestHumanReadableName()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			jobHeader.FH_CompletionStatement = "Dat Job is complete";
			workflow.FH_CompletionStatement = "Dis Workflow is complete";

			AssertEquals("Job: Dat Job is complete", jobHeader.HumanReadableName);
			AssertEquals("Workflow: Dis Workflow is complete", workflow.HumanReadableName);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestDataIsLoggedToAuditDB()
		{
			using var adminConnection = Db.NewAdminConnection();
			var factory = new BusinessObjectFactory(adminConnection);
			var auditLogsHelperForTesting = new AuditLogsHelperForTesting(factory, ProcessHeaderSchema.Instance);

			var header = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(factory, "Jimminy Jillikers");
			factory.Save();
			header.NudgeUp();
			factory.Save();
			AssertEquals(2, auditLogsHelperForTesting.GetAuditLogCollection(header).Count);
		}

		#endregion

		#region Prerequisites

		public void TestHasOpenPrerequisites()
		{
			var job = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow1.GetOrCreateDependencyLink(workflow2);

			var task1 = job.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			var task2 = job.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = workflow2.PK;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			AssertEquals(false, workflow1.HasOpenPrerequisites);
			AssertEquals(true, workflow2.HasOpenPrerequisites);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertEquals(true, workflow2.HasOpenPrerequisites);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals(true, workflow2.HasOpenPrerequisites);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			AssertEquals(true, workflow2.HasOpenPrerequisites);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(false, workflow2.HasOpenPrerequisites);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(false, workflow2.HasOpenPrerequisites);
		}

		public void TestHasOpenPrerequisites_CompletionStatement()
		{
			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var categorisedTaskType = categorisedTaskTypes.AddNew();
			categorisedTaskType.Code = "ORG";
			var taskType = categorisedTaskType.TaskTypes.AddNew();
			taskType.Code = "COM";
			taskType.IsCompletionStatementTaskType = true;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			workflow1.MakePrerequisiteOf(workflow2);
			var completionStatement = workflow1.Parent.WorkflowItems.AddNew();
			completionStatement.P9_Type = taskType.Code;
			completionStatement.P9_FH_ProcessHeader = workflow1.PK;
			AssertEquals(true, completionStatement.IsCompletionStatement);

			AssertEquals(true, workflow2.HasOpenPrerequisites);

			completionStatement.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(false, workflow2.HasOpenPrerequisites);
		}

		public void TestGetPrerequisitesUpTheTree()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var job = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);

			var task1 = job.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			var task2 = job.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = workflow2.PK;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var task3 = job.WorkflowItems.AddNew();
			task3.P9_FH_ProcessHeader = workflow3.PK;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			AssertEquals(false, workflow1.HasOpenPrerequisites);
			AssertEquals(true, workflow2.HasOpenPrerequisites);
			AssertEquals(true, workflow3.HasOpenPrerequisites);

			var prereqs = workflow3.GetPrerequisitesUpTheTree().ToArray();
			AssertEquals(2, prereqs.Length);
			AssertEquals(workflow2, prereqs[0]);
			AssertEquals(workflow1, prereqs[1]);
		}

		public void TestGetPrerequisitesUpTheTree_NullJobHeader()
		{
			var workflow = Factory.NewWithValidTestData<ProcessHeader>();
			bool hasPrereqs = false;

			AssertNoExceptionThrown(() => hasPrereqs = workflow.HasOpenPrerequisites);
			AssertEquals(false, hasPrereqs);
		}

		public void TestGetPrerequisitesUpTheTree_NullHeaderFrom()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<DummyWithWorkflow>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var link1_2 = workflow2.LinksFromOthersToMe_ForBinding.AddNew();
			link1_2.FP_FH_HeaderFrom = workflow1.PK;
			link1_2.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			var link1_nothing = workflow1.LinksFromOthersToMe_ForBinding.AddNew();
			link1_nothing.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			AssertNull(link1_nothing.HeaderFrom);
			AssertNoExceptionThrown(() => link1_2.Validation.ValidateAll());
			AssertNoExceptionThrown(() => link1_nothing.Validation.ValidateAll());
		}

		public void TestGetPrerequisitesUpTheTree_ForChildWorkflows()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader1.FH_CompletionStatement = "jobHeader1";
			jobHeader2.FH_CompletionStatement = "jobHeader2";
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2");
			var workflow2Child = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2Child");
			var workflow2Grandchild = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2Grandchild");

			workflow2Child.GetOrCreateLinkToParent(workflow2);
			workflow2Grandchild.GetOrCreateLinkToParent(workflow2Child);
			jobHeader1.GetOrCreateDependencyLink(jobHeader2);

			var task1 = BMSTestHelper.CreateTask(workflow1);
			var task2 = BMSTestHelper.CreateTask(workflow2);
			var task3 = BMSTestHelper.CreateTask(workflow2Child);
			var task4 = BMSTestHelper.CreateTask(workflow2Grandchild);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("jobHeader1", Array.Empty<ProcessHeader>(), jobHeader1.GetPrerequisitesUpTheTree(getApplicableDependenciesOnly: true));
				AssertContainsExactElementsInAnyOrder("workflow1", Array.Empty<ProcessHeader>(), workflow1.GetPrerequisitesUpTheTree(getApplicableDependenciesOnly: true));
				AssertContainsExactElementsInAnyOrder("jobHeader2", new[] { jobHeader1 }, jobHeader2.GetPrerequisitesUpTheTree(getApplicableDependenciesOnly: true));
				AssertContainsExactElementsInAnyOrder("workflow2", new[] { jobHeader1 }, workflow2.GetPrerequisitesUpTheTree(getApplicableDependenciesOnly: true));
				AssertContainsExactElementsInAnyOrder("workflow2Child", new[] { jobHeader1 }, workflow2Child.GetPrerequisitesUpTheTree(getApplicableDependenciesOnly: true));
				AssertContainsExactElementsInAnyOrder("workflow2Grandchild", new[] { jobHeader1 }, workflow2Grandchild.GetPrerequisitesUpTheTree(getApplicableDependenciesOnly: true));
			});
		}

		#endregion

		#region Postrequisites

		public void TestPostRequesiteWorkflowsWithNullLinks()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var link = jobHeader1.PostrequisiteLinks_ForBinding.AddNew();

			AssertEquals(0, jobHeader1.PostrequisiteWorkflows.Count());
		}

		public void TestPostrequisiteWorkflows()
		{
			var job1 = Factory.New<DummyWithWorkflow>();
			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var workflow1_1 = jobHeader1.ProcessHeaders[0];
			workflow1_1.FH_CompletionStatement = "workflow1_1";
			var workflow1_2 = jobHeader1.ProcessHeaders.AddNew();
			workflow1_2.FH_CompletionStatement = "workflow1_2";
			var workflow1_3 = jobHeader1.ProcessHeaders.AddNew();
			workflow1_3.FH_CompletionStatement = "workflow1_3";

			var task1_1 = job1.WorkflowItems.AddNew();
			task1_1.P9_FH_ProcessHeader = workflow1_1.PK;
			var task1_2 = job1.WorkflowItems.AddNew();
			task1_2.P9_FH_ProcessHeader = workflow1_2.PK;
			var task1_3 = job1.WorkflowItems.AddNew();
			task1_3.P9_FH_ProcessHeader = workflow1_3.PK;

			workflow1_1.GetOrCreateDependencyLink(workflow1_2);
			workflow1_2.GetOrCreateDependencyLink(workflow1_3);

			var job2 = Factory.New<DummyWithWorkflow>();
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);
			var workflow2_1 = jobHeader2.ProcessHeaders[0];
			workflow2_1.FH_CompletionStatement = "workflow2_1";
			var workflow2_2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2_2.FH_CompletionStatement = "workflow2_2";
			var workflow2_3 = jobHeader2.ProcessHeaders.AddNew();
			workflow2_3.FH_CompletionStatement = "workflow2_3";

			var task2_1 = job2.WorkflowItems.AddNew();
			task2_1.P9_FH_ProcessHeader = workflow2_1.PK;
			var task2_2 = job2.WorkflowItems.AddNew();
			task2_2.P9_FH_ProcessHeader = workflow2_2.PK;
			var task2_3 = job2.WorkflowItems.AddNew();
			task2_3.P9_FH_ProcessHeader = workflow2_3.PK;

			workflow2_1.GetOrCreateDependencyLink(workflow2_2);
			workflow2_2.GetOrCreateDependencyLink(workflow2_3);
			workflow1_1.GetOrCreateDependencyLink(jobHeader2); // Link from workflow to job2

			var postRequisites = workflow1_1.PostrequisiteWorkflows.ToArray();

			AssertCollectionContains("Should contain direct post-requisite", workflow1_2, postRequisites);
			AssertCollectionContains("Should contain the first workflow in post-requisite job", workflow2_1, postRequisites);
			AssertEquals(2, postRequisites.Length);

			postRequisites = workflow1_2.PostrequisiteWorkflows.ToArray();
			AssertCollectionContains("Should contain direct post-requisite only", workflow1_3, postRequisites);
			AssertEquals(1, postRequisites.Length);

			postRequisites = workflow1_3.PostrequisiteWorkflows.ToArray();
			AssertEquals(0, postRequisites.Length);

			// Close the task of the first workflow in second job
			task2_1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			postRequisites = workflow1_1.PostrequisiteWorkflows.ToArray();

			AssertCollectionContains("Should contain direct post-requisite", workflow1_2, postRequisites);
			AssertCollectionContains("Should contain the second workflow in post-requisite job, now that the first workflow is closed", workflow2_2, postRequisites);
			AssertEquals(2, postRequisites.Length);
		}

		#endregion

		#region Depenencies

		public void TestWorkflowRelationships()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header1 = jobHeader.ProcessHeaders[0];
			var header2 = jobHeader.ProcessHeaders.AddNew();
			var header3 = jobHeader.ProcessHeaders.AddNew();
			var header4 = jobHeader.ProcessHeaders.AddNew();
			var header5 = jobHeader.ProcessHeaders.AddNew();
			var header6 = jobHeader.ProcessHeaders.AddNew();

			var link1To2 = header1.GetOrCreateDependencyLink(header2);
			var link1To3 = header1.GetOrCreateDependencyLink(header3);
			var link2To4 = header2.GetOrCreateDependencyLink(header4);
			var link3To4 = header3.GetOrCreateDependencyLink(header4);

			var link5ChildOf4 = header5.GetOrCreateLinkToParent(header4);

			AssertEquals("header1 should have no prerequisites", 0, header1.PrerequisiteLinks_ForBinding.Count);
			AssertEquals("header1 should have 2 postrequisites", 2, header1.PostrequisiteLinks_ForBinding.Count);
			Assert("header2 should be a postrequisite of header1", header1.PostrequisiteLinks_ForBinding.Contains(link1To2));
			Assert("header3 should be a postrequisite of header1", header1.PostrequisiteLinks_ForBinding.Contains(link1To3));
			AssertEquals("header1 should have no parents", 0, header1.ParentLinks_ForBinding.Count);
			AssertEquals("header1 should have no children", 0, header1.ChildLinks_ForBinding.Count);

			AssertEquals("header2 should have 1 prerequisite", 1, header2.PrerequisiteLinks_ForBinding.Count);
			Assert("header1 should be a prerequisite of header2", header2.PrerequisiteLinks_ForBinding.Contains(link1To2));
			AssertEquals("header2 should have 1 postrequisite", 1, header2.PostrequisiteLinks_ForBinding.Count);
			Assert("header4 should be a postrequisite of header2", header2.PostrequisiteLinks_ForBinding.Contains(link2To4));
			AssertEquals("header2 should have no parents", 0, header2.ParentLinks_ForBinding.Count);
			AssertEquals("header2 should have no children", 0, header2.ChildLinks_ForBinding.Count);

			AssertEquals("header3 should have 1 prerequisite", 1, header3.PrerequisiteLinks_ForBinding.Count);
			Assert("header1 should be a prerequisite of header3", header3.PrerequisiteLinks_ForBinding.Contains(link1To3));
			AssertEquals("header3 should have 1 postrequisite", 1, header3.PostrequisiteLinks_ForBinding.Count);
			Assert("header4 should be a postrequisite of header3", header3.PostrequisiteLinks_ForBinding.Contains(link3To4));
			AssertEquals("header3 should have no parents", 0, header3.ParentLinks_ForBinding.Count);
			AssertEquals("header3 should have no children", 0, header3.ChildLinks_ForBinding.Count);

			AssertEquals("header4 should have 2 prerequisites", 2, header4.PrerequisiteLinks_ForBinding.Count);
			Assert("header2 should be a prerequisite of header4", header4.PrerequisiteLinks_ForBinding.Contains(link2To4));
			Assert("header3 should be a prerequisite of header4", header4.PrerequisiteLinks_ForBinding.Contains(link3To4));
			AssertEquals("header4 should have no postrequisite", 0, header4.PostrequisiteLinks_ForBinding.Count);
			AssertEquals("header4 should have no parents", 0, header4.ParentLinks_ForBinding.Count);
			AssertEquals("header4 should have 1 child", 1, header4.ChildLinks_ForBinding.Count);
			Assert("header5 should be a child of header4", header4.ChildLinks_ForBinding.Contains(link5ChildOf4));

			AssertEquals("header5 should have no prerequisites", 0, header5.PrerequisiteLinks_ForBinding.Count);
			AssertEquals("header5 should have no postrequisites", 0, header5.PostrequisiteLinks_ForBinding.Count);
			AssertEquals("header5 should have 1 parent", 1, header5.ParentLinks_ForBinding.Count);
			Assert("header4 should be parent of header5", header5.ParentLinks_ForBinding.Contains(link5ChildOf4));
			AssertEquals("header5 should have no children", 0, header5.ChildLinks_ForBinding.Count);

			AssertEquals("header6 should have no prerequisites", 0, header6.PrerequisiteLinks_ForBinding.Count);
			AssertEquals("header6 should have no postrequisites", 0, header6.PostrequisiteLinks_ForBinding.Count);
			AssertEquals("header6 should have no parents", 0, header6.ParentLinks_ForBinding.Count);
			AssertEquals("header6 should have no children", 0, header6.ChildLinks_ForBinding.Count);
		}

		public void TestMakeDependency_WorkflowPrerequisitesShouldIncludeJobPrerequisites()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1");
			var task1 = BMSTestHelper.CreateTask(workflow1);

			var jobHeader2 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2");
			var task2 = BMSTestHelper.CreateTask(workflow2);

			var link = jobHeader1.GetOrCreateDependencyLink(jobHeader2);
			link.Validation.ValidateAll();

			Factory.Save();

			Assert(jobHeader2.GetPrerequisitesUpTheTree(true).Any(jh => jh == jobHeader1));
			Assert(workflow2.GetPrerequisitesUpTheTree(true).Any(jh => jh == jobHeader1));

			AssertNoErrors(link.FP_FH_HeaderFromInfo);

			Assert(workflow2.HasOpenPrerequisites);
			Assert(jobHeader2.HasOpenPrerequisites);
		}

		public void TestMakeDependency()
		{
			var dummyWithWorkflow = Factory.New<DummyWithWorkflow>();
			var masterHeader = ProcessJobHeader.GetForParent(dummyWithWorkflow, Factory);

			var header_1 = masterHeader.ProcessHeaders.AddNew();
			header_1.FH_CompletionStatement = "Workflow 1";
			var header_2 = masterHeader.ProcessHeaders.AddNew();
			header_2.FH_CompletionStatement = "Workflow 2";
			var header_3 = masterHeader.ProcessHeaders.AddNew();
			header_3.FH_CompletionStatement = "Workflow 3";

			var links_1 = header_1.Links.Select(l => string.Format("{0}>>{1}>>{2}", l.HeaderFrom.FH_CompletionStatement, l.FP_LinkType, l.HeaderTo.FH_CompletionStatement)).ToList();
			var links_3 = header_3.Links.Select(l => string.Format("{0}>>{1}>>{2}", l.HeaderFrom.FH_CompletionStatement, l.FP_LinkType, l.HeaderTo.FH_CompletionStatement)).ToList();
			AssertEquals("Precondition: ", 0, links_1.Count);
			AssertEquals("Precondition: ", 0, links_3.Count);

			header_1.MakePrerequisiteOf(header_3);
			links_1 = header_1.Links.Select(l => string.Format("{0}>>{1}>>{2}", l.HeaderFrom.FH_CompletionStatement, l.FP_LinkType, l.HeaderTo.FH_CompletionStatement)).ToList();
			links_3 = header_3.Links.Select(l => string.Format("{0}>>{1}>>{2}", l.HeaderFrom.FH_CompletionStatement, l.FP_LinkType, l.HeaderTo.FH_CompletionStatement)).ToList();
			var expected = new string[] { "Workflow 1>>DEP>>Workflow 3" };
			AssertContainsExactElementsInAnyOrder(expected, links_1);
			AssertContainsExactElementsInAnyOrder(expected, links_3);

			header_3.MakePrerequisiteOf(header_1);
			links_1 = header_1.Links.Select(l => string.Format("{0}>>{1}>>{2}", l.HeaderFrom.FH_CompletionStatement, l.FP_LinkType, l.HeaderTo.FH_CompletionStatement)).ToList();
			links_3 = header_3.Links.Select(l => string.Format("{0}>>{1}>>{2}", l.HeaderFrom.FH_CompletionStatement, l.FP_LinkType, l.HeaderTo.FH_CompletionStatement)).ToList();
			expected = new string[] { "Workflow 3>>DEP>>Workflow 1" };
			AssertContainsExactElementsInAnyOrder(expected, links_1);
			AssertContainsExactElementsInAnyOrder(expected, links_3);

			header_3.MakePrerequisiteOf(header_1);
			links_1 = header_1.Links.Select(l => string.Format("{0}>>{1}>>{2}", l.HeaderFrom.FH_CompletionStatement, l.FP_LinkType, l.HeaderTo.FH_CompletionStatement)).ToList();
			links_3 = header_3.Links.Select(l => string.Format("{0}>>{1}>>{2}", l.HeaderFrom.FH_CompletionStatement, l.FP_LinkType, l.HeaderTo.FH_CompletionStatement)).ToList();
			expected = new string[] { "Workflow 3>>DEP>>Workflow 1" };
			AssertContainsExactElementsInAnyOrder(expected, links_1);
			AssertContainsExactElementsInAnyOrder(expected, links_3);
		}

		public void TestJobWorkflowPrerequisitesInherited()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "prerequisite job");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow for prereq job");
			var task1 = BMSTestHelper.CreateTask(workflow1, description: "test for prereq");

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "parent job workflow");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2 for parent job");
			var task2 = BMSTestHelper.CreateTask(workflow2, description: "test 2 for parent");

			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "child job workflow");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader3, "workflow3 for child job");
			var task3 = BMSTestHelper.CreateTask(workflow3, description: "test 3 for child");
			jobHeader3.GetOrCreateLinkToParent(jobHeader2);

			Factory.Save();

			AssertEquals("jobHeader2 is Open if no factory save done. We will update workflow status on Factory.Save()", WorkflowStatusList.Codes.Open, jobHeader2.FH_Status);
			AssertEquals("jobHeader3 is Open if no factory save done. We will update workflow status on Factory.Save()", WorkflowStatusList.Codes.Open, jobHeader3.FH_Status);

			jobHeader1.GetOrCreateDependencyLink(jobHeader2);

			Factory.Save();

			AssertEquals("jobHeader1 should not have prerequisites", 0, jobHeader1.PrerequisiteLinks_ForBinding.Count);
			AssertEquals("jobHeader1 is Open", WorkflowStatusList.Codes.Open, jobHeader1.FH_Status);

			AssertEquals("jobHeader1 should have 1 postrequisite (jobHeader2)", 1, jobHeader1.PostrequisiteLinks_ForBinding.Count);
			AssertEquals("jobHeader1 should not have parent", 0, jobHeader1.ParentLinks_ForBinding.Count);
			AssertEquals("No children", 0, jobHeader1.ChildLinks_ForBinding.Count);

			BMSTestCaseWithFactory.AssertIsPrerequisite(jobHeader1, jobHeader2);
			AssertEquals("jobHeader2 is Blocked because has open prerequisites", WorkflowStatusList.Codes.Blocked, jobHeader2.FH_Status);

			BMSTestCaseWithFactory.AssertIsNotParent(jobHeader2, jobHeader1);
			BMSTestCaseWithFactory.AssertIsParent(jobHeader3, jobHeader2);

			AssertEquals("jobHeader3 should have prerequisites, because jobHeader3 is a child of jobHeader2. Children should inherit prerequisites from parent job", 1, jobHeader3.NumberOfOpenPrerequisitesUpTheTree);
			AssertContainsExactElementsInAnyOrder("jobHeader3 should inherite prerequisites from jobHeader1 via his parent jobHeader2",
				new ProcessHeader[] { jobHeader1 }, jobHeader3.GetPrerequisitesUpTheTree(getApplicableDependenciesOnly: true));

			AssertEquals("jobHeader3 is Blocked because has his parent has open prerequisites", WorkflowStatusList.Codes.Blocked, jobHeader3.FH_Status);
		}

		public void TestCreateRelationshipInAnotherFactory_ShouldUpdateCachedCollections()
		{
			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Laxative in the food");
			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Spray him with the hose");

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();

			var loadedWorkflow1 = newFactory.Load<ProcessHeader>(workflow1.PK);
			var loadedWorkflow2 = newFactory.Load<ProcessHeader>(workflow2.PK);

			loadedWorkflow2.GetOrCreateLinkToParent(loadedWorkflow1);

			AssertEquals(false, workflow1.IsParentOf(workflow2));

			newFactory.Save();

			AssertEquals("DataRefreshBus should propagate changes to first factory, and also clear the cached collections", true, workflow1.IsParentOf(workflow2));
			AssertEquals(1, workflow1.ChildLinks.Count());
			AssertEquals(1, workflow2.ParentLinks.Count());
		}

		#endregion

		#region Delete

		public void TestPropertyValueForDeletedWorkflow_ShouldBeReportedInErrorReport_WhenParentTableCodeIsAccessed()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "I hope our prices aren't too low");
			Factory.Save();

			workflow.Delete();
			AssertEquals("OH", workflow.FH_ParentTableCode);

			AssertContains("Should not be accessing a property on a deleted business object", ErrorReporter.LastMessageReported);
			AssertContains("Property name: FH_ParentTableCode", ErrorReporter.LastMessageReported);
			AssertContains("Property value: OH", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestPropertyValueForDeletedWorkflow_ShouldNotBeReportedInErrorReport_WhenAnotherPropertyIsAccessed()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "I hope our prices aren't too low");
			Factory.Save();

			workflow.Delete();
			AssertEquals("I hope our prices aren't too low", workflow.FH_CompletionStatement);

			AssertContains("Should not be accessing a property on a deleted business object", ErrorReporter.LastMessageReported);
			AssertContains("Property name: FH_CompletionStatement", ErrorReporter.LastMessageReported);
			AssertNotContains("Property value", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestDelete_WhenReferencingInvalidJobHeader_ShouldNotThrowNullReferenceException()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "I hope our prices aren't too low");

			workflow.FH_FH_ParentHeader = ZGuid.NewZGuid(); // I guess the job-level workflow could be deleted without the ProcessHeaderCollection being populated, such as inside SuspendListChangedEvents?

			jobHeader.Delete();
			AssertNoExceptionThrown(workflow.Delete);
		}

		[TestDate(2000, 1, 1)]
		public void TestDelete_AlsoDeletesTagLinks()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var link = (TagLink)jobHeader.AddTag(config.RedTag).Link;

			Factory.Save();
			AssertEquals("Precondition - There should be 1 taglink for this job header.", 1, Factory.GetDatabaseCount(typeof(TagLink), new ZQuery(TagLinkSchema.TGL_ParentId, jobHeader.PK)));

			var editTimeBeforeDelete = jobHeader.FH_SystemLastEditTimeUtc;
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			jobHeader.Delete();
			Factory.Save();

			CombineAssertions("Given deleting the job header", () =>
			{
				AssertEquals("Tag link should be marked as deleted", true, link.IsDeleted);
				AssertEquals("Tag link should be deleted from the DB.", 0, Factory.GetDatabaseCount(typeof(TagLink), new ZQuery(TagLinkSchema.TGL_ParentId, jobHeader.PK)));
			});
		}

		[ExpectNoExceptions]
		public void TestDoubleDelete()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();

			jobHeader.Delete();
			jobHeader.Delete();

			AssertEquals(true, workflow.IsDeleted);
			workflow.Delete();
		}

		[TestDate(2000, 1, 1)]
		public void TestDeleteProcessHeader_WithTasks_ShouldNotUpdateProcessHeaderEditTime()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var workflow = jobHeader.ProcessHeaders.AddNew();

			var task1 = BMSTestHelper.CreateTask(workflow, "", 30);
			var task2 = BMSTestHelper.CreateTask(workflow, "", 30);

			CombineAssertions("Precondition - Tasks in workflow should not be deleted.", () =>
			{
				AssertEquals("task1", false, task1.IsDeleted);
				AssertEquals("task2", false, task2.IsDeleted);

				Factory.Save();
				AssertEquals("Total tasks", 2, Factory.GetDatabaseCount(typeof(ProcessTask), new ZQuery(ProcessTasksSchema.P9_FH_ProcessHeader, workflow.PK)));
			});

			var editTimeBeforeDelete = workflow.FH_SystemLastEditTimeUtc;
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			workflow.Delete();
			AssertNotEquals("Precondition: deleted time and editTimeBeforeDelete is different", editTimeBeforeDelete, TestDateAttribute.Date);

			CombineAssertions("GIVEN workflow with two tasks, WHEN we deleted the workflow", () =>
			{
				AssertEquals("task1 should be deleted", true, task1.IsDeleted);
				AssertEquals("task2 should be deleted", true, task2.IsDeleted);

				var editTimeAfterDelete = workflow.SystemLastEditTimeAfterDelete_ForTest;
				AssertEquals("workflows's edit time should not change.", editTimeBeforeDelete, editTimeAfterDelete);

				Factory.Save();
				AssertEquals("There should be no process tasks for this workflow in the DB.", 0, Factory.GetDatabaseCount(typeof(ProcessTask), new ZQuery(ProcessTasksSchema.P9_FH_ProcessHeader, workflow.PK)));
			});
		}

		[TestDate(2000, 1, 1)]
		public void TestDeleteProcessHeader_WithLink_ShouldNotUpdateProcessHeaderEditTime()
		{
			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			Factory.Save();
			AssertEquals("Precondition - There are supposed to be 3 process headers in the database", 3, Factory.GetDatabaseCount(typeof(ProcessHeader), new ZQuery(ProcessHeaderSchema.FH_FH_ParentHeader, jobHeader.PK)));

			var editTimeBeforeDelete = workflow1.FH_SystemLastEditTimeUtc;
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			workflow1.Delete();
			AssertNotEquals("Precondition: deleted time and editTimeBeforeDelete is different", editTimeBeforeDelete, TestDateAttribute.Date);

			CombineAssertions("GIVEN 2 linked workflows, WHEN deleting a workflow, ", () =>
			{
				AssertEquals("Workflow should be marked as deleted.", true, workflow1.IsDeleted);
				AssertEquals("Link should be deleted.", true, link.IsDeleted);
				AssertEquals("Other workflow should not be deleted.", false, workflow2.IsDeleted);

				var editTimeAfterDelete = workflow1.SystemLastEditTimeAfterDelete_ForTest;
				AssertEquals("Deleted workflow edit time should not change.", editTimeBeforeDelete, editTimeAfterDelete);

				Factory.Save();
				AssertEquals("Process headers count should decrease from 3 to 2.", 2, Factory.GetDatabaseCount(typeof(ProcessHeader), new ZQuery(ProcessHeaderSchema.FH_FH_ParentHeader, jobHeader.PK)));
			});
		}

		public void TestDeleteJob_ShouldAlsoDeleteAllBMSEntities()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedJob = newFactory.Load<SalesEnquiry>(job.PK);

			loadedJob.Delete();
			newFactory.Save();

			AssertNull(newFactory.Load<ProcessHeader>(jobHeader.PK));
			AssertNull(newFactory.Load<ProcessHeader>(workflow1.PK));
			AssertNull(newFactory.Load<ProcessHeader>(workflow2.PK));
			AssertNull(newFactory.Load<ProcessHeaderLink>(link.PK));
		}

		public void TestDeleteJob_ShouldDeleteProcessJobHeader()
		{
			var job = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, job.Factory, true);

			AssertEquals("Precondition for jobHeader", false, jobHeader.IsDeleted);

			job.Delete();
			AssertEquals(true, jobHeader.IsDeleted);
		}

		public void TestCreateJobHeaderForDeletedJob_NoJobHeader()
		{
			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			job.Delete();

			var jobHeader = BMSTestHelper.CreateJobHeader(job, addDefaultProcessHeaderIfNone: false);
			AssertEquals(true, ErrorReporter.HasBeenReported("ProcessJobHeader.Initialise:Deleted"));
			ErrorReporter.Clear();
			AssertNull("Why would we create the jobHeader?", jobHeader);
		}

		public void TestDeleteJobDeletesJobHeader()
		{
			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			var jobHeader = BMSTestHelper.CreateJobHeader(job, addDefaultProcessHeaderIfNone: false);
			job.Delete();
			AssertEquals(true, jobHeader.IsDeleted);
		}

		public void TestShouldSaveDeletionCallstack_WhenRegistryItemIsEnabled()
		{
			var workflow1 = Factory.New<ProcessHeader>();
			workflow1.Delete();
			AssertNull(workflow1.DeletionCallstack_ExposedForTest);

			BMSRegistry.Instance.ReportWorkflowDeletionCallstackWhenLoadingWorkflowsToTransfer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var workflow2 = Factory.New<ProcessHeader>();
			workflow2.Delete();
			AssertNotNull(workflow2.DeletionCallstack_ExposedForTest);
			AssertContains("at Enterprise.BufferManagement.Business.Test.ProcessHeaderTest.TestShouldSaveDeletionCallstack_WhenRegistryItemIsEnabled() in", workflow2.DeletionCallstack_ExposedForTest);
		}

		#endregion

		#region Buffer

		public void TestSettingComponentToBufferSetsReleaseDateTime()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var header = Factory.NewWithValidTestData<ProcessHeader>();
			AssertEquals(ZDateTime.Empty, header.FH_ReleaseDateTime);

			header.FH_FC_CurrentComponent = config.Buffer.PK;
			AssertZDatesWithin5Minutes(string.Format("{0} (should be set to now)", ProcessHeaderSchema.FH_ReleaseDateTime.Name), ZDateTime.UtcNow, header.FH_ReleaseDateTime);

			header.FH_ReleaseDateTime = new ZDateTime(1981, 5, 28);
			header.FH_FC_CurrentComponent = ZGuid.Empty;
			header.FH_FC_CurrentComponent = config.Buffer.PK;
			AssertZDatesWithin5Minutes(string.Format("{0} (should update to new value)", ProcessHeaderSchema.FH_ReleaseDateTime.Name), ZDateTime.UtcNow, header.FH_ReleaseDateTime);
		}

		[TestDate(2013, 2, 22)]
		public void TestBufferZone()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			Factory.Save();

			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var buffer = BMSTestHelper.CreateBuffer(system);
			buffer.FC_Name = "Buffer Component";

			var preBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Pre", timespanMinutes: 64 * 60);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "Constraint", offsetMinutes: 64 * 60);
			var postBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Post", timespanMinutes: 32 * 60, offsetMinutes: 64 * 60);

			var bucket = BMSTestHelper.CreateBucket(system, "Bucket Component");

			var ccrResource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CCR", "CCR");
			ccrResource.DesignateAsCCR(buffer);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			var nonCCRtask = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 10);
			var cCRtask = BMSTestHelper.CreateTask(workflow, ccrResource.GS_Code, 60, sequence: 20);

			Factory.Save();

			workflow.FH_FC_CurrentComponent = ZGuid.Empty;
			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-2);
			AssertEquals(ZString.Empty, workflow.CurrentStatus);

			workflow.FH_FC_CurrentComponent = bucket.PK;
			AssertEquals("Bucket Component", workflow.CurrentStatus);

			workflow.FH_FC_CurrentComponent = buffer.PK;
			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-2);
			AssertMultilineASCIIEquals("",
	@"Buffer Component - Zone 3
	Pre - Zone 3", workflow.CurrentStatus);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddHours(-95);
			ClearCachedCurrentStatus(workflow.PK);

			AssertMultilineASCIIEquals("",
	@"Buffer Component - Zone 3
	Pre - Zone 2", workflow.CurrentStatus);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-4);
			ClearCachedCurrentStatus(workflow.PK);

			AssertMultilineASCIIEquals("Moving beyond parent component zone3, but child offset ensures we're still in zone 3 of the child component",
	@"Buffer Component - Zone 2
	Pre - Zone 2", workflow.CurrentStatus);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-9);
			ClearCachedCurrentStatus(workflow.PK);

			AssertMultilineASCIIEquals("",
	@"Buffer Component - Zone 2
	Pre - Zone 1", workflow.CurrentStatus);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-11);
			ClearCachedCurrentStatus(workflow.PK);

			AssertMultilineASCIIEquals("",
	@"Buffer Component - Zone 1
	Pre - Zone 0", workflow.CurrentStatus);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-16);
			ClearCachedCurrentStatus(workflow.PK);

			AssertMultilineASCIIEquals("",
	@"Buffer Component - Zone 0
	Pre - Zone 0", workflow.CurrentStatus);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-30);
			ClearCachedCurrentStatus(workflow.PK);

			AssertMultilineASCIIEquals("",
	@"Buffer Component - Zone 0
	Pre - Zone 0", workflow.CurrentStatus);

			workflow.FH_ReleaseDateTime = ZDateTime.Empty;
			ClearCachedCurrentStatus(workflow.PK);

			AssertEquals(@"Buffer Component - Zone 3
	Pre - Zone 3", workflow.CurrentStatus);
		}

		public void TestBufferZoneValue()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var bufferComponent = system.Components.AddNew();
			bufferComponent.FC_Type = BMComponentTypeList.Codes.Buffer;
			bufferComponent.BufferTimespan = new ZDateTime(2012, 1, 5, 0, 0, 0);
			bufferComponent.FC_Name = "Buffer Component";

			var bucketComponent = system.Components.AddNew();
			bucketComponent.FC_Type = BMComponentTypeList.Codes.Bucket;
			bucketComponent.FC_Name = "Bucket Component";

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeader.GetForParent(dummy, Factory);
			var header = jobHeader.ProcessHeaders.AddNew();

			header.FH_FC_CurrentComponent = bucketComponent.PK;
			AssertEquals("Bucket Component", header.CurrentStatus);
			AssertNull(header.BufferZone);

			header.FH_FC_CurrentComponent = bufferComponent.PK;

			AssertEquals(3, header.BufferZone);
		}

		[TestDate(2013, 4, 3, 14, 10, 2)]
		[ExpectNoExceptions]
		public void TestBufferZoneValue_WithEmptyDate()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = buffer.PK;
			workflow.FH_ReleaseDateTime = ZDateTime.Empty;

			var task = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			AssertEquals(3, workflow.BufferZone);
		}

		public void TestBufferComponentSet()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var componentBucket = system.Components.AddNew();
			componentBucket.FC_Type = BMComponentTypeList.Codes.Bucket;
			var componentBuffer = system.Components.AddNew();
			componentBuffer.FC_Type = BMComponentTypeList.Codes.Buffer;

			var link1 = componentBucket.FromMeToOthersLinks.AddNew();
			link1.FL_FC_ComponentTo = componentBuffer.PK;

			var dummy = Factory.New<DummyWithWorkflow>();
			var header = ProcessJobHeader.GetForParent(dummy, Factory).ProcessHeaders.AddNew();
			AssertEquals(componentBucket.PK, header.FH_FC_CurrentComponent);
		}

		public void TestBufferAgreedDeliveryDateLocal()
		{
			var header = Factory.NewWithValidTestData<ProcessHeader>();
			header.FH_AgreedDeliveryDate = ZDateTime.UtcNow;

			AssertNotEquals("UTC and local time should not be the same", ZDateTime.UtcNow, ZDateTime.Now);
			AssertDateTimeWithinOneSecond(string.Format("{0} should be in UTC", ProcessHeaderSchema.FH_AgreedDeliveryDate.Name), ZDateTime.UtcNow.ToDateTime(), header.FH_AgreedDeliveryDate.ToDateTime());
			AssertDateTimeWithinOneSecond("AgreedDeliveryDateLocal should be in local time", ZDateTime.Now.ToDateTime(), header.AgreedDeliveryDateLocal.ToDateTime());
		}

		public void TestBufferDoNotStartBeforeDateLocal()
		{
			var header = Factory.NewWithValidTestData<ProcessHeader>();
			header.FH_DoNotStartBeforeDate = ZDateTime.UtcNow;

			AssertNotEquals("UTC and local time should not be the same", ZDateTime.UtcNow, ZDateTime.Now);
			AssertDateTimeWithinOneSecond(string.Format("{0} should be in UTC", ProcessHeaderSchema.FH_DoNotStartBeforeDate.Name), ZDateTime.UtcNow.ToDateTime(), header.FH_DoNotStartBeforeDate.ToDateTime());
			AssertDateTimeWithinOneSecond("DoNotStartBeforeDateLocal should be in local time", ZDateTime.Now.ToDateTime(), header.DoNotStartBeforeDateLocal.ToDateTime());
		}

		public void TestBufferReleaseDateLocal()
		{
			var system = Factory.New<BMSystem>();
			var buffer = system.Components.AddNew();
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;

			var header = Factory.NewWithValidTestData<ProcessHeader>();
			header.FH_FC_CurrentComponent = buffer.PK;

			AssertNotEquals("UTC and local time should not be the same", ZDateTime.UtcNow, ZDateTime.Now);
			AssertDateTimeWithinOneSecond(string.Format("{0} should be in UTC", ProcessHeaderSchema.FH_ReleaseDateTime.Name), ZDateTime.UtcNow.ToDateTime(), header.FH_ReleaseDateTime.ToDateTime());
			AssertDateTimeWithinOneSecond("LastTransferDateLocal should be in local time", ZDateTime.Now.ToDateTime(), header.LastTransferDateLocal.ToDateTime());
		}

		#endregion

		#region Current Component

		public void TestSettingCurrentComponent_WithNoSecurity_ShouldNotAddLog()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var bucket1 = system.Components.AddNew();
			bucket1.FC_Name = "Bucket1";
			var bucket2 = system.Components.AddNew();
			bucket2.FC_Name = "Bucket2";

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "MAIORGSYD";
			var header = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders[0];
			header.FH_CompletionStatement = "Workflow 1";

			var startingLogsCount = header.Logs.LogsNotInDB.Length;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Frodo Baggins";
			staff.GS_Code = "FRO";
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed = false;

				header.FH_FC_CurrentComponent = bucket1.PK;
				header.FH_FC_CurrentComponent = bucket2.PK;
			}
			AssertEquals(startingLogsCount, header.Logs.LogsNotInDB.Length);
		}

		public void TestCannotChangeComponentOnUnsavedRecord()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = system.Components.AddNew();
			bucket.FC_Name = "Bucket";

			var buffer = system.Components.AddNew();
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			buffer.FC_Name = "Buffer";
			buffer.FC_BufferTimespanInMinutes = 1;

			var link = bucket.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = buffer.PK;

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed = false;

				var job = Factory.New<OrgHeader>();
				var header = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders.AddNew();

				AssertNoError(header.FH_FC_CurrentComponentInfo, "You do not have security rights to change Current Component.");
				AssertEquals(bucket, header.CurrentComponent);

				header.FH_FC_CurrentComponent = buffer.PK;
				AssertHasError(header.FH_FC_CurrentComponentInfo, "You do not have security rights to change Current Component.");

				header.FH_FC_CurrentComponent = bucket.PK;
				AssertNoError(header.FH_FC_CurrentComponentInfo, "You do not have security rights to change Current Component.");
			}
		}

		public void TestSetCurrentComponent_WhenNoSecurity_ShouldNotSetReleaseDate()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var system = Factory.NewWithValidTestData<BMSystem>();
			var buffer = system.Components.AddNew();
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			buffer.FC_Name = "Buffer";
			buffer.FC_BufferTimespanInMinutes = 1;

			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeader.GetForParent(dummy, Factory);
			var header = jobHeader.ProcessHeaders.AddNew();
			header.FH_ReleaseDateTime = ZDateTime.BrettsBirthday;

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed = false;
				header.FH_FC_CurrentComponent = buffer.PK;
				AssertEquals(ZDateTime.BrettsBirthday, header.FH_ReleaseDateTime);

				header.FH_FC_CurrentComponent = ZGuid.Empty;

				Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed = true;
				header.FH_FC_CurrentComponent = buffer.PK;
				AssertNotEquals(ZDateTime.BrettsBirthday, header.FH_ReleaseDateTime);
			}
		}

		public void TestAddingNewRecordWithoutBMComponent_ShouldNotHaveErrors()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed = false;

				var job = Factory.New<OrgHeader>();
				var header = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders.AddNew();
				header.Validation.ValidateAll();

				AssertNoError(header.FH_FC_CurrentComponentInfo, "You do not have security rights to change Current Component.");
			}

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = system.Components.AddNew();
			bucket.FC_Name = "Bucket";

			var buffer = system.Components.AddNew();
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			buffer.FC_Name = "Buffer";
			buffer.FC_BufferTimespanInMinutes = 1;

			var link = bucket.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = buffer.PK;

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed = false;

				var job = Factory.New<OrgHeader>();
				var header = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders.AddNew();

				header.FH_FC_CurrentComponent = buffer.PK;
				AssertHasError(header.FH_FC_CurrentComponentInfo, "You do not have security rights to change Current Component.");
			}
		}

		public void TestCurrentComponentSystemPK()
		{
			var system1 = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = system1.Components.AddNew();
			bucket1.FC_Name = "bucket1";
			bucket1.FC_DisplaySequence = 1;
			var bucket11 = system1.Components.AddNew();
			bucket11.FC_Name = "bucket11";
			bucket11.FC_DisplaySequence = 2;
			var link1 = bucket1.FromMeToOthersLinks.AddNew();
			link1.FL_FC_ComponentTo = bucket11.PK;

			var system2 = BMSTestHelper.CreateSystem(Factory, "INQ");
			var bucket2 = system2.Components.AddNew();
			bucket2.FC_Name = "bucket2";
			bucket2.FC_DisplaySequence = 1;
			var bucket22 = system2.Components.AddNew();
			bucket22.FC_DisplaySequence = 2;
			bucket22.FC_Name = "bucket22";
			var link2 = bucket2.FromMeToOthersLinks.AddNew();
			link2.FL_FC_ComponentTo = bucket22.PK;

			var staff = Factory.NewWithValidTestData<GlbStaff>();

			Factory.Save();

			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders[0];

			AssertEquals(system1, workflow.BMSystem);
			AssertEquals(system1.PK, workflow.CurrentComponentSystemPK);
			AssertEquals(bucket1.PK, workflow.FH_FC_CurrentComponent);

			workflow.CurrentComponentSystemPK = system2.PK;
			AssertEquals(system2, workflow.BMSystem);
			AssertEquals("Should change", system2.PK, workflow.CurrentComponentSystemPK);
			AssertEquals("Should change to entry component of system2", bucket2.PK, workflow.FH_FC_CurrentComponent);

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed = false;

				workflow.CurrentComponentSystemPK = system1.PK;
				AssertEquals("Should not change due to lack of permission", system2.PK, workflow.CurrentComponentSystemPK);
				AssertEquals("Should not change due to lack of permission", bucket2.PK, workflow.FH_FC_CurrentComponent);
			}

			bool valueChanged = false;
			workflow.FH_FC_CurrentComponentInfo.ValueChanged += (s, e) => valueChanged = true;
			workflow.CurrentComponentSystemPK = system2.PK;
			Assert("Refresh the lookups when this changes.", valueChanged);
		}

		#region GetFirstBMComponent

		public void TestGetFirstBMComponent_ShouldReturnActiveEntryComponent()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1", sequence: 1);
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2", sequence: 2);
			var bucket3 = BMSTestHelper.CreateBucket(system, "Bucket 3", sequence: 3);
			bucket1.FC_IsActive = false;

			//Creating link Bucket 3 → Bucket 2, to prove that sequence numbers should not affect the calculation of which component is the entry point.
			var link3_2 = BMSTestHelper.LinkComponents(bucket3, bucket2, sequence: 1);
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");

			Factory.Save();

			AssertEquals("Given Bucket 1 is inactive, and Bucket 3 is the entry point to the BMS, when creating a new workflow, then it should have been put into bucket 3 (entry to BMS).",
				bucket3.FC_Name, workflow.CurrentComponent.FC_Name);
		}

		public void TestDeferWorkflow_DeferReasons()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1", sequence: 1);
			var buffer1 = BMSTestHelper.CreateBuffer(system, "Buffer", sequence: 2);
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2", sequence: 3);

			var link1 = BMSTestHelper.LinkComponents(bucket1, buffer1, sequence: 1);
			var link2 = BMSTestHelper.LinkComponents(buffer1, bucket2, sequence: 2);
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1", currentComponent: buffer1);
			AssertEquals("Precondition: workflow should be on the Buffer", buffer1.FC_Name, workflow.CurrentComponent.FC_Name);

			Factory.Save();

			BMSTestHelper.Defer(workflow, WorkflowDeferralReasonsList.Codes.PrioritiesChanged);

			AssertEquals(bucket1.FC_Name, workflow.CurrentComponent.FC_Name);
			BMSTestCaseWithFactory.AssertComponentChangedEventRaised(workflow, ComponentChangeMode.Defer, buffer1.PK, bucket1.PK, bufferPenetration: 0, bufferZone: 3, deferReason: WorkflowDeferralReasonsList.Codes.PrioritiesChanged, status: "CLS");
		}

		public void TestDeferWorkflow_ShouldSelectActiveEntryComponent()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1", sequence: 1);
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2", sequence: 2);
			var bucket3 = BMSTestHelper.CreateBucket(system, "Bucket 3", sequence: 3);
			var buffer1 = BMSTestHelper.CreateBuffer(system, "Buffer Main", sequence: 4);
			bucket1.FC_IsActive = false;

			//Creating link Bucket 3 → Bucket 2, to prove that sequence numbers should not affect the calculation of which component is the entry point.
			var link3_2 = BMSTestHelper.LinkComponents(bucket3, bucket2, sequence: 1);
			var link2_Buffer = BMSTestHelper.LinkComponents(bucket2, buffer1, sequence: 2);
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1", currentComponent: buffer1);
			AssertEquals("Precondition: workflow should be on the Buffer", buffer1.FC_Name, workflow.CurrentComponent.FC_Name);

			Factory.Save();

			BMSTestHelper.Defer(workflow);
			AssertEquals("Given Bucket 1 is inactive, and Bucket 3 is the entry point to the BMS, when deferring a workflow, then it should have been put into bucket 3 (entry to BMS).",
				bucket3.FC_Name, workflow.CurrentComponent.FC_Name);
		}

		public void TestCreateWorkflowFromTemplate_ShouldSelectActiveEntryComponent()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1", sequence: 1);
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2", sequence: 2);
			var bucket3 = BMSTestHelper.CreateBucket(system, "Bucket 3", sequence: 3);
			bucket1.FC_IsActive = false;

			//Creating link Bucket 3 → Bucket 2, to prove that sequence numbers should not affect the calculation of which component is the entry point.
			var link3_2 = BMSTestHelper.LinkComponents(bucket3, bucket2, sequence: 1);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);

			var templateTask = template.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.ApplyWorkflowTemplates();
			AssertEquals("Precondition: workflow was applied to the job", 1, job.WorkflowItems.Count);

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);
			var workflow = jobHeader.ProcessHeaders.Single();

			AssertEquals("Given Bucket 1 is inactive, and Bucket 3 is the entry point to the BMS, when creating a workflow from a template, then then new workflow in the job should have been put into bucket 3 (entry to BMS).",
				bucket3.FC_Name, workflow.CurrentComponent.FC_Name);
		}

		public void TestCreateWorkflowFromTemplate_ChangeTemplatesSystem_ShouldSelectCorrectActiveEntryComponent()
		{
			var system1 = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system1, "Bucket 1", sequence: 1);
			var bucket2 = BMSTestHelper.CreateBucket(system1, "Bucket 2", sequence: 2);
			var bucket3 = BMSTestHelper.CreateBucket(system1, "Bucket 3", sequence: 3);
			bucket1.FC_IsActive = false;

			var system2 = BMSTestHelper.CreateSystem(Factory);
			var bucket4 = BMSTestHelper.CreateBucket(system2, "Bucket 4", sequence: 1);
			var bucket5 = BMSTestHelper.CreateBucket(system2, "Bucket 5", sequence: 2);

			//Creating link Bucket 3 → Bucket 2, to prove that sequence numbers should not affect the calculation of which component is the entry point.
			var link3_2 = BMSTestHelper.LinkComponents(bucket3, bucket2, sequence: 1);

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);
			var templateTask = template.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.ApplyWorkflowTemplates();
			AssertEquals("Precondition: workflow was applied to the job", 1, job.WorkflowItems.Count);

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);
			var workflow = jobHeader.ProcessHeaders.Single();

			AssertEquals("Should choose system 1",
				bucket3.FC_Name, workflow.CurrentComponent.FC_Name);

			template.P0_FS_BufferManagementSystem = system1.PK;
			Factory.Save();

			AssertEquals(system1.PK, workflow.BMSystem.PK);
			AssertEquals("Should stay on system 1",
				bucket3.FC_Name, workflow.CurrentComponent.FC_Name);

			template.P0_FS_BufferManagementSystem = system2.PK;
			Factory.Save();

			AssertEquals(false, workflow.HasChanges);

			AssertEquals("jobheader should switch to system 2", system2.PK, jobHeader.BMSystem.PK);

			AssertEquals(system1.PK, workflow.BMSystem.PK);
			AssertEquals("workflow should stay on system 1",
				bucket3.FC_Name, workflow.CurrentComponent.FC_Name);

			AssertEquals("workflow should not have changes when system has switched", false, workflow.HasChanges);
			AssertEquals("jobheader should not have changes when system has switched", false, jobHeader.HasChanges);
		}

		public void TestCreateWorkflowFromTemplate_TwoJobs_ChangeTemplatesSystem_ShouldSelectCorrectActiveEntryComponent()
		{
			var system1 = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = BMSTestHelper.CreateBucket(system1, "Bucket 1", sequence: 1);

			var system2 = BMSTestHelper.CreateSystem(Factory);
			var bucket2 = BMSTestHelper.CreateBucket(system2, "Bucket 2", sequence: 1);

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);
			var templateTask = template.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			var job1 = Factory.NewWithValidTestData<OrgHeader>();
			job1.ApplyWorkflowTemplates();
			AssertEquals("Precondition: workflow was applied to the job", 1, job1.WorkflowItems.Count);

			var jobHeader1 = ProcessJobHeader.GetForParentWithoutCreation(job1, Factory);
			var workflow1 = jobHeader1.ProcessHeaders.Single();

			AssertEquals("Should choose system 1",
				bucket1.FC_Name, workflow1.CurrentComponent.FC_Name);

			template.P0_FS_BufferManagementSystem = system2.PK;
			Factory.Save();

			var job2 = Factory.NewWithValidTestData<OrgHeader>();
			job2.ApplyWorkflowTemplates();
			AssertEquals("Precondition: workflow was applied to the job", 1, job2.WorkflowItems.Count);

			var jobHeader2 = ProcessJobHeader.GetForParentWithoutCreation(job2, Factory);
			var workflow2 = jobHeader2.ProcessHeaders.Single();

			AssertEquals(system2.PK, workflow2.BMSystem.PK);
			AssertEquals("Should change to system 2",
				bucket2.FC_Name, workflow2.CurrentComponent.FC_Name);
		}

		public void TestCreateWorkflowFromTemplate_AddNewWorkflow_TemplateSystemShouldMatch()
		{
			var system1 = BMSTestHelper.CreateSystem(Factory, "ORG");
			system1.FS_Name = "Fast";
			var bucket1 = BMSTestHelper.CreateBucket(system1, "Bucket 1", sequence: 1);
			var bucket2 = BMSTestHelper.CreateBucket(system1, "Bucket 2", sequence: 2);
			var bucket3 = BMSTestHelper.CreateBucket(system1, "Bucket 3", sequence: 3);
			var buffer = BMSTestHelper.CreateBuffer(system1, "Buffer", sequence: 4);
			var bucket4 = BMSTestHelper.CreateBucket(system1, "Bucket 4", sequence: 5);

			var system2 = BMSTestHelper.CreateSystem(Factory);
			system2.FS_Name = "Slow";
			var bucket5 = BMSTestHelper.CreateBucket(system2, "Bucket 5", sequence: 1);
			var bucket6 = BMSTestHelper.CreateBucket(system2, "Bucket 6", sequence: 2);

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);
			var templateTask = template.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.ApplyWorkflowTemplates();
			AssertEquals("Workflow was applied to the job", 1, job.WorkflowItems.Count);

			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = jobHeader.ProcessHeaders.Single();

			AssertEquals("Job Header should be in system 1",
				system1.FS_Name, jobHeader.BMSystem.FS_Name);

			AssertEquals("Workflow should be in system 1",
				system1.FS_Name, workflow.BMSystem.FS_Name);

			AssertEquals("Workflow should be in system 1's entry point",
				bucket1.FC_Name, workflow.CurrentComponent.FC_Name);

			Factory.Save();

			template.P0_FS_BufferManagementSystem = system2.PK;
			Factory.Save();

			AssertEquals("Job Header should be in system 2",
				system2.FS_Name, jobHeader.BMSystem.FS_Name);

			AssertEquals("Workflow should still be in system 1",
				system1.FS_Name, workflow.BMSystem.FS_Name);

			AssertEquals("Workflow should still be in system 1's entry point",
				bucket1.FC_Name, workflow.CurrentComponent.FC_Name);

			var newFactory = new BusinessObjectFactory();
			var loadedJobHeader = newFactory.Load<ProcessJobHeader>(jobHeader.PK);

			var newWorkflow = loadedJobHeader.ProcessHeaders.AddNew();

			AssertEquals("New workflow should be in system 2",
				system2.FS_Name, newWorkflow.BMSystem.FS_Name);

			AssertEquals("New workflow should be in system 2's entry point",
				bucket5.FC_Name, newWorkflow.CurrentComponent.FC_Name);
		}

		public void TestTemplateWorkflows_NoReleaseGroupsAssigned_ShouldLinkToAppropriateBMSystem()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var releaseGroup = BMSTestHelper.CreateReleaseGroup(system, group);

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);
			var templateTask = template.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			AssertEquals(true, template.P0_FS_BufferManagementSystem.IsEmpty);
			AssertEquals(null, templateWorkflow.ReleaseGroup);
			AssertEquals(true, templateWorkflow.FH_GG_ReleaseGroup.IsEmpty);
			AssertEquals("Lookups should find all available release groups, regardless of system", 6, templateWorkflow.Lookups.AllReleaseGroups.Count);

			AssertEquals("Template workflows should be linked to system1", system.PK, template.ProcessHeaders[0].BMSystem.PK);
			AssertEquals("Template workflows should be linked to system1", system.PK, template.ProcessHeaders[1].BMSystem.PK);

			AssertEquals(true, group.GG_IsActive);
			AssertNull(template.ProcessHeaders[0].Parent);

			Factory.Save();

			var newWorkflow = template.ProcessHeaders.AddNew();
			AssertEquals("New template workflow should be linked to system1", system.PK, newWorkflow.BMSystem.PK);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.ApplyWorkflowTemplates();
			AssertEquals("Workflow was applied to the job", 1, job.WorkflowItems.Count);

			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = jobHeader.ProcessHeaders.Single();

			AssertEquals("Job header created from template should be linked to the system", system.PK, jobHeader.BMSystem.PK);
			AssertEquals("Workflow created from template should be linked to the system", system.PK, workflow.BMSystem.PK);
		}

		#endregion

		#endregion

		#region Component Transfer Event

		[TestDate(2015, 7, 14)]
		[TestDateIncremental(seconds: 1)]
		public void TestChangeComponent_ShouldLogXFREventOnWorkflow()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "werkflow");

			AssertEquals(config.Bucket, workflow.CurrentComponent);

			workflow.FH_FC_CurrentComponent = config.Buffer.PK;

			AssertEquals(config.Buffer, workflow.CurrentComponent);
			var log = BMSTestCaseWithFactory.AssertComponentChangedEventRaised(workflow, ComponentChangeMode.ManualTransfer, config.Bucket.PK, config.Buffer.PK);

			AssertEquals("Moved from component [bucket] to [buffer]. It was manually moved.", log.DisplayEventReference);
		}

		[TestDate(2015, 7, 14)]
		[TestDateIncremental(seconds: 1)]
		public void TestChangeComponent_ShouldLogXFREventOnWorkflow_WithStatus()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "werkflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "werkflow2");
			var task1 = BMSTestHelper.CreateTask(workflow1);
			var task2 = BMSTestHelper.CreateTask(workflow2);

			workflow1.GetOrCreateDependencyLink(workflow2);
			Factory.Save();

			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			BMSTestCaseWithFactory.AssertComponentChangedEventRaised(workflow2, ComponentChangeMode.ManualTransfer, config.Bucket.PK, config.Buffer.PK, status: WorkflowStatusList.Codes.Blocked);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			workflow2.FH_FC_CurrentComponent = config.Bucket.PK;
			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			BMSTestCaseWithFactory.AssertComponentChangedEventRaised(workflow2, ComponentChangeMode.ManualTransfer, config.Bucket.PK, config.Buffer.PK, status: WorkflowStatusList.Codes.Open);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			workflow2.FH_FC_CurrentComponent = config.Bucket.PK;
			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			BMSTestCaseWithFactory.AssertComponentChangedEventRaised(workflow2, ComponentChangeMode.ManualTransfer, config.Bucket.PK, config.Buffer.PK, status: WorkflowStatusList.Codes.Closed);
		}

		[TestDate(2015, 7, 14)]
		[TestDateIncremental(seconds: 1)]
		public void TestChangeComponent_ShouldLogXFREventOnWorkflow_WithBufferPenetration()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "werkflow1");
			var task = BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			BMSTestCaseWithFactory.AssertComponentChangedEventRaised(workflow, ComponentChangeMode.ManualTransfer, config.Bucket.PK, config.Buffer.PK);

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(6);
			Factory.Save();

			workflow.FH_FC_CurrentComponent = config.Bucket.PK;
			var log = BMSTestCaseWithFactory.AssertComponentChangedEventRaised(workflow, ComponentChangeMode.ManualTransfer, config.Buffer.PK, config.Bucket.PK, constraintStatus: ConstraintStatus.NonConstrained, bufferPenetration: 0.5m, bufferZone: 2);
			AssertEquals("Moved from component [buffer] (zone 2) to [bucket]. It was manually moved.", log.DisplayEventReference);

			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(2000);

			workflow.FH_FC_CurrentComponent = config.Bucket.PK;
			BMSTestCaseWithFactory.AssertComponentChangedEventRaised(workflow, ComponentChangeMode.ManualTransfer, config.Buffer.PK, config.Bucket.PK, constraintStatus: ConstraintStatus.NonConstrained, bufferPenetration: 99.99m, bufferZone: 0);
		}

		[TestDate(2015, 7, 14)]
		[TestDateIncremental(seconds: 1)]
		public void TestChangeComponent_WhenInBuffer_ShouldLogXFREventOnWorkflow_WithCcrState()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "werkflow");
			var task1 = BMSTestHelper.CreateTask(workflow, config.NonCCR1.GS_Code, 10);
			var task2 = BMSTestHelper.CreateTask(workflow, config.CCR.GS_Code, 10);
			var task3 = BMSTestHelper.CreateTask(workflow, config.NonCCR2.GS_Code, 10);

			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			BMSTestCaseWithFactory.AssertComponentChangedEventRaised(workflow, ComponentChangeMode.ManualTransfer, config.Bucket.PK, config.Buffer.PK);

			workflow.FH_FC_CurrentComponent = config.Bucket.PK;
			BMSTestCaseWithFactory.AssertComponentChangedEventRaised(workflow, ComponentChangeMode.ManualTransfer, config.Buffer.PK, config.Bucket.PK, constraintStatus: ConstraintStatus.PreConstraint, bufferPenetration: 0.01m, bufferZone: 3);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			BMSTestCaseWithFactory.AssertComponentChangedEventRaised(workflow, ComponentChangeMode.ManualTransfer, config.Bucket.PK, config.Buffer.PK);

			workflow.FH_FC_CurrentComponent = config.Bucket.PK;
			BMSTestCaseWithFactory.AssertComponentChangedEventRaised(workflow, ComponentChangeMode.ManualTransfer, config.Buffer.PK, config.Bucket.PK, constraintStatus: ConstraintStatus.ReadyForConstraint, bufferPenetration: 0.01m, bufferZone: 3);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			BMSTestCaseWithFactory.AssertComponentChangedEventRaised(workflow, ComponentChangeMode.ManualTransfer, config.Bucket.PK, config.Buffer.PK);

			workflow.FH_FC_CurrentComponent = config.Bucket.PK;
			BMSTestCaseWithFactory.AssertComponentChangedEventRaised(workflow, ComponentChangeMode.ManualTransfer, config.Buffer.PK, config.Bucket.PK, constraintStatus: ConstraintStatus.PostConstraint, bufferPenetration: 0.01m, bufferZone: 3);
		}

		[TestDate(2015, 7, 14)]
		[TestDateIncremental(seconds: 1)]
		public void TestChangeComponent_WhenInBuffer_ForNonConstrainedResources_ShouldLogXFREventOnWorkflow_WithCcrState()
		{
			var config = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "werkflow");
			var task = BMSTestHelper.CreateTask(workflow, config.NonCCR1.GS_Code, 10);

			workflow.FH_FC_CurrentComponent = config.Buffer.PK;
			BMSTestCaseWithFactory.AssertComponentChangedEventRaised(workflow, ComponentChangeMode.ManualTransfer, config.Bucket.PK, config.Buffer.PK);

			workflow.FH_FC_CurrentComponent = config.Bucket.PK;
			BMSTestCaseWithFactory.AssertComponentChangedEventRaised(workflow, ComponentChangeMode.ManualTransfer, config.Buffer.PK, config.Bucket.PK, constraintStatus: ConstraintStatus.NonConstrained, bufferPenetration: 0.00m, bufferZone: 3);
		}

		#endregion

		#region Description Properties

		public void TestProviderJobNumber_Description()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "ORG1";
			job.OH_FullName = "Organisation 1";

			Assert(job is IWorkflowProvider);
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);

			AssertEquals("ORG1", jobHeader.ProviderJobNumber);
			AssertEquals("Organisation 1", jobHeader.ProviderJobDescription);
		}

		public void TestParentJobDescription()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeader.GetForParent(dummy, Factory);
			var header1 = jobHeader.ProcessHeaders.AddNew();
			AssertEquals("Dummy Business Object Default", header1.ParentJobDescription);

			var header2 = Factory.NewWithValidTestData<ProcessHeader>();
			AssertEquals(ZString.Empty, header2.ParentJobDescription);

			var jobHeaderOrphan = Factory.NewWithValidTestData<ProcessJobHeader>();
			var header3 = jobHeaderOrphan.ProcessHeaders.AddNew();
			AssertEquals(ZString.Empty, header2.ParentJobDescription);
		}

		public void TestDescription()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeader.GetForParent(dummy, Factory);
			var header1 = jobHeader.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "Completion Bye";
			AssertEquals("Dummy Business Object Default - Completion Bye", header1.Description);

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM");
			var header2 = template.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = "everyone rocks";
			AssertEquals("everyone rocks", header2.Description);
		}

		public void TestCode()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "MAIORGSYD";
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var header1 = jobHeader.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "Completion Bye";
			AssertEquals("Completion Bye", header1.Code);
		}

		public void TestCode_NoParent()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			var header1 = template.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "Completion Bye";
			AssertEquals("Completion Bye", header1.Code);
		}

		#endregion

		#region Estimates

		[TestDate(2020, 3, 1)]
		public void TestEstimates()
		{
			var dummyWithWorkflow = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeader.GetForParent(dummyWithWorkflow, Factory, addDefaultProcessHeaderIfNone: false);
			var header1 = jobHeader.ProcessHeaders.AddNew();
			var header1_1 = jobHeader.ProcessHeaders.AddNew();
			var header2 = jobHeader.ProcessHeaders.AddNew();
			BMSTestHelper.MakeChildOf(header1_1, header1);

			BMSTestHelper.CreateTask(header1, lowEstMinutes: 60, estVariationFactor: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Open).P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(10);
			BMSTestHelper.CreateTask(header1, lowEstMinutes: 70, estVariationFactor: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned).P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(20);
			BMSTestHelper.CreateTask(header1, lowEstMinutes: 80, estVariationFactor: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Working).P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(30);
			BMSTestHelper.CreateTask(header1, lowEstMinutes: 90, estVariationFactor: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Suspended).P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(40);
			BMSTestHelper.CreateTask(header1, lowEstMinutes: 100, estVariationFactor: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Closed).P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(50);
			BMSTestHelper.CreateTask(header1, lowEstMinutes: 110, estVariationFactor: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled).P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(60);

			BMSTestHelper.CreateTask(header1_1, lowEstMinutes: 120, estVariationFactor: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Open).P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(70);
			BMSTestHelper.CreateTask(header1_1, lowEstMinutes: 130, estVariationFactor: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned).P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(80);
			BMSTestHelper.CreateTask(header1_1, lowEstMinutes: 140, estVariationFactor: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Working).P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(90);
			BMSTestHelper.CreateTask(header1_1, lowEstMinutes: 150, estVariationFactor: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Suspended).P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(100);
			BMSTestHelper.CreateTask(header1_1, lowEstMinutes: 160, estVariationFactor: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Closed).P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(110);
			BMSTestHelper.CreateTask(header1_1, lowEstMinutes: 170, estVariationFactor: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled).P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(120);

			BMSTestHelper.CreateTask(header2, lowEstMinutes: 180, estVariationFactor: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Open).P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(130);
			BMSTestHelper.CreateTask(header2, lowEstMinutes: 190, estVariationFactor: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned).P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(140);
			BMSTestHelper.CreateTask(header2, lowEstMinutes: 200, estVariationFactor: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Working).P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(150);
			BMSTestHelper.CreateTask(header2, lowEstMinutes: 210, estVariationFactor: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Suspended).P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(160);
			BMSTestHelper.CreateTask(header2, lowEstMinutes: 220, estVariationFactor: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Closed).P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(170);
			BMSTestHelper.CreateTask(header2, lowEstMinutes: 230, estVariationFactor: 1, taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled).P9_ActualDuration = new ZDateTime(2019, 1, 1).AddMinutes(180);

			CombineAssertions("header1_1", () =>
			{
				AssertEquals("TotalRelevantEstimatedHoursSummary", "14.5 hrs.", header1_1.TotalRelevantEstimatedHoursSummary);
				AssertEquals("TotalRelevantEstimatedHoursLabel", "14:30", header1_1.TotalRelevantEstimatedHoursLabel);
				AssertEquals("TotalRelevantEstimatedHours", 14.5m, header1_1.TotalRelevantEstimatedHours);
				AssertEquals("TotalRelevantEstimatedHoursIncludingChildrenSummary", "14.5 hrs.", header1_1.TotalRelevantEstimatedHoursIncludingChildrenSummary);
				AssertEquals("TotalRelevantEstimatedHoursIncludingChildrenLabel", "14:30", header1_1.TotalRelevantEstimatedHoursIncludingChildrenLabel);
				AssertEquals("TotalRelevantEstimatedHoursIncludingChildren", 14.5m, header1_1.TotalRelevantEstimatedHoursIncludingChildren);

				AssertEquals("TotalNonCancelledEstimatedHoursIncludingChildren", 11.66666666666667m, header1_1.TotalNonCancelledEstimatedHoursIncludingChildren);

				AssertEquals("TotalEstimatedHoursSummary", "14.5 hrs to 14.5 hrs (14.5 standard estimate).", header1_1.TotalEstimatedHoursSummary);

				AssertEquals("TotalActualHoursSummary", "9.5 hrs.", header1_1.TotalActualHoursSummary);
				AssertEquals("TotalActualHoursLabel", "9:30", header1_1.TotalActualHoursLabel);
				AssertEquals("TotalActualHours", 9.5m, header1_1.TotalActualHours);
				AssertEquals("TotalActualHoursIncludingChildrenDateTime", (ZDateTime)new TimeSpan(9, 30, 0), header1_1.TotalActualHoursIncludingChildrenDateTime);
				AssertEquals("TotalActualHoursIncludingChildrenSummary", "9.5 hrs.", header1_1.TotalActualHoursIncludingChildrenSummary);
				AssertEquals("TotalActualHoursIncludingChildrenLabel", "9:30", header1_1.TotalActualHoursIncludingChildrenLabel);
				AssertEquals("TotalActualHoursIncludingChildren", 9.5m, header1_1.TotalActualHoursIncludingChildren);

				AssertEquals("RemainingEstimateHoursSummary", "9.0 hrs.", header1_1.RemainingEstimateHoursSummary);
				AssertEquals("RemainingEstimateHoursLabel", "9:00", header1_1.RemainingEstimateHoursLabel);
				AssertEquals("RemainingEstimateHours", 9m, header1_1.RemainingEstimateHours);
				AssertEquals("RemainingEstimateHoursIncludingChildrenSummary", "9.0 hrs.", header1_1.RemainingEstimateHoursIncludingChildrenSummary);
				AssertEquals("RemainingEstimateHoursIncludingChildrenLabel", "9:00", header1_1.RemainingEstimateHoursIncludingChildrenLabel);
				AssertEquals("RemainingEstimateHoursIncludingChildren", 9m, header1_1.RemainingEstimateHoursIncludingChildren);
			});

			CombineAssertions("header1", () =>
			{
				AssertEquals("TotalRelevantEstimatedHoursSummary", "8.5 hrs.", header1.TotalRelevantEstimatedHoursSummary);
				AssertEquals("TotalRelevantEstimatedHoursLabel", "8:30", header1.TotalRelevantEstimatedHoursLabel);
				AssertEquals("TotalRelevantEstimatedHours", 8.5m, header1.TotalRelevantEstimatedHours);
				AssertEquals("TotalRelevantEstimatedHoursIncludingChildrenSummary", "23.0 hrs.", header1.TotalRelevantEstimatedHoursIncludingChildrenSummary);
				AssertEquals("TotalRelevantEstimatedHoursIncludingChildrenLabel", "23:00", header1.TotalRelevantEstimatedHoursIncludingChildrenLabel);
				AssertEquals("TotalRelevantEstimatedHoursIncludingChildren", 23m, header1.TotalRelevantEstimatedHoursIncludingChildren);

				AssertEquals("TotalNonCancelledEstimatedHoursIncludingChildren", 18.33333333333334m, header1.TotalNonCancelledEstimatedHoursIncludingChildren);

				AssertEquals("TotalEstimatedHoursSummary", "23.0 hrs to 23.0 hrs (23.0 standard estimate).", header1.TotalEstimatedHoursSummary);

				AssertEquals("TotalActualHoursSummary", "3.5 hrs.", header1.TotalActualHoursSummary);
				AssertEquals("TotalActualHoursLabel", "3:30", header1.TotalActualHoursLabel);
				AssertEquals("TotalActualHours", 3.5m, header1.TotalActualHours);
				AssertEquals("TotalActualHoursIncludingChildrenDateTime", (ZDateTime)TimeSpan.FromHours(13), header1.TotalActualHoursIncludingChildrenDateTime);
				AssertEquals("TotalActualHoursIncludingChildrenSummary", "13.0 hrs.", header1.TotalActualHoursIncludingChildrenSummary);
				AssertEquals("TotalActualHoursIncludingChildrenLabel", "13:00", header1.TotalActualHoursIncludingChildrenLabel);
				AssertEquals("TotalActualHoursIncludingChildren", 13m, header1.TotalActualHoursIncludingChildren);

				AssertEquals("RemainingEstimateHoursSummary", "5.0 hrs.", header1.RemainingEstimateHoursSummary);
				AssertEquals("RemainingEstimateHoursLabel", "5:00", header1.RemainingEstimateHoursLabel);
				AssertEquals("RemainingEstimateHours", 5m, header1.RemainingEstimateHours);
				AssertEquals("RemainingEstimateHoursIncludingChildrenSummary", "14.0 hrs.", header1.RemainingEstimateHoursIncludingChildrenSummary);
				AssertEquals("RemainingEstimateHoursIncludingChildrenLabel", "14:00", header1.RemainingEstimateHoursIncludingChildrenLabel);
				AssertEquals("RemainingEstimateHoursIncludingChildren", 14m, header1.RemainingEstimateHoursIncludingChildren);
			});

			CombineAssertions("header2", () =>
			{
				AssertEquals("TotalRelevantEstimatedHoursSummary", "20.5 hrs.", header2.TotalRelevantEstimatedHoursSummary);
				AssertEquals("TotalRelevantEstimatedHoursLabel", "20:30", header2.TotalRelevantEstimatedHoursLabel);
				AssertEquals("TotalRelevantEstimatedHours", 20.5m, header2.TotalRelevantEstimatedHours);
				AssertEquals("TotalRelevantEstimatedHoursIncludingChildrenSummary", "20.5 hrs.", header2.TotalRelevantEstimatedHoursIncludingChildrenSummary);
				AssertEquals("TotalRelevantEstimatedHoursIncludingChildrenLabel", "20:30", header2.TotalRelevantEstimatedHoursIncludingChildrenLabel);
				AssertEquals("TotalRelevantEstimatedHoursIncludingChildren", 20.5m, header2.TotalRelevantEstimatedHoursIncludingChildren);

				AssertEquals("TotalNonCancelledEstimatedHoursIncludingChildren", 16.66666666666667m, header2.TotalNonCancelledEstimatedHoursIncludingChildren);

				AssertEquals("TotalEstimatedHoursSummary", "20.5 hrs to 20.5 hrs (20.5 standard estimate).", header2.TotalEstimatedHoursSummary);

				AssertEquals("TotalActualHoursSummary", "15.5 hrs.", header2.TotalActualHoursSummary);
				AssertEquals("TotalActualHoursLabel", "15:30", header2.TotalActualHoursLabel);
				AssertEquals("TotalActualHours", 15.5m, header2.TotalActualHours);
				AssertEquals("TotalActualHoursIncludingChildrenDateTime", (ZDateTime)new TimeSpan(15, 30, 0), header2.TotalActualHoursIncludingChildrenDateTime);
				AssertEquals("TotalActualHoursIncludingChildrenSummary", "15.5 hrs.", header2.TotalActualHoursIncludingChildrenSummary);
				AssertEquals("TotalActualHoursIncludingChildrenLabel", "15:30", header2.TotalActualHoursIncludingChildrenLabel);
				AssertEquals("TotalActualHoursIncludingChildren", 15.5m, header2.TotalActualHoursIncludingChildren);

				AssertEquals("RemainingEstimateHoursSummary", "13.0 hrs.", header2.RemainingEstimateHoursSummary);
				AssertEquals("RemainingEstimateHoursLabel", "13:00", header2.RemainingEstimateHoursLabel);
				AssertEquals("RemainingEstimateHours", 13m, header2.RemainingEstimateHours);
				AssertEquals("RemainingEstimateHoursIncludingChildrenSummary", "13.0 hrs.", header2.RemainingEstimateHoursIncludingChildrenSummary);
				AssertEquals("RemainingEstimateHoursIncludingChildrenLabel", "13:00", header2.RemainingEstimateHoursIncludingChildrenLabel);
				AssertEquals("RemainingEstimateHoursIncludingChildren", 13m, header2.RemainingEstimateHoursIncludingChildren);
			});

			CombineAssertions("jobHeader", () =>
			{
				AssertEquals("TotalRelevantEstimatedHoursSummary", "43.5 hrs.", jobHeader.TotalRelevantEstimatedHoursSummary);
				AssertEquals("TotalRelevantEstimatedHoursLabel", "43:30", jobHeader.TotalRelevantEstimatedHoursLabel);
				AssertEquals("TotalRelevantEstimatedHours", 43.5m, jobHeader.TotalRelevantEstimatedHours);
				AssertEquals("TotalRelevantEstimatedHoursIncludingChildrenSummary", "43.5 hrs.", jobHeader.TotalRelevantEstimatedHoursIncludingChildrenSummary);
				AssertEquals("TotalRelevantEstimatedHoursIncludingChildrenLabel", "43:30", jobHeader.TotalRelevantEstimatedHoursIncludingChildrenLabel);
				AssertEquals("TotalRelevantEstimatedHoursIncludingChildren", 43.5m, jobHeader.TotalRelevantEstimatedHoursIncludingChildren);

				AssertEquals("TotalNonCancelledEstimatedHoursIncludingChildren", 35.00000000000001m, jobHeader.TotalNonCancelledEstimatedHoursIncludingChildren);

				AssertEquals("TotalEstimatedHoursSummary", "43.5 hrs to 43.5 hrs.", jobHeader.TotalEstimatedHoursSummary);

				AssertEquals("TotalActualHoursSummary", "28.5 hrs.", jobHeader.TotalActualHoursSummary);
				AssertEquals("TotalActualHoursLabel", "28:30", jobHeader.TotalActualHoursLabel);
				AssertEquals("TotalActualHours", 28.5m, jobHeader.TotalActualHours);
				AssertEquals("TotalActualHoursIncludingChildrenDateTime", (ZDateTime)new TimeSpan(1, 4, 30, 0), jobHeader.TotalActualHoursIncludingChildrenDateTime);
				AssertEquals("TotalActualHoursIncludingChildrenSummary", "28.5 hrs.", jobHeader.TotalActualHoursIncludingChildrenSummary);
				AssertEquals("TotalActualHoursIncludingChildrenLabel", "28:30", jobHeader.TotalActualHoursIncludingChildrenLabel);
				AssertEquals("TotalActualHoursIncludingChildren", 28.5m, jobHeader.TotalActualHoursIncludingChildren);

				AssertEquals("RemainingEstimateHoursSummary", "27.0 hrs.", jobHeader.RemainingEstimateHoursSummary);
				AssertEquals("RemainingEstimateHoursLabel", "27:00", jobHeader.RemainingEstimateHoursLabel);
				AssertEquals("RemainingEstimateHours", 27m, jobHeader.RemainingEstimateHours);
				AssertEquals("RemainingEstimateHoursIncludingChildrenSummary", "27.0 hrs.", jobHeader.RemainingEstimateHoursIncludingChildrenSummary);
				AssertEquals("RemainingEstimateHoursIncludingChildrenLabel", "27:00", jobHeader.RemainingEstimateHoursIncludingChildrenLabel);
				AssertEquals("RemainingEstimateHoursIncludingChildren", 27m, jobHeader.RemainingEstimateHoursIncludingChildren);
			});
		}

		[TestDate(2012, 1, 1)]
		public void TestTotalActualHoursIncludingChildren_UsesFactoryCache()
		{
			var dummyWithWorkflow = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeader.GetForParent(dummyWithWorkflow, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Going");
			var task = BMSTestHelper.CreateTask(workflow1, lowEstMinutes: 180, estVariationFactor: 2);
			AssertEquals(0, (int)workflow1.TotalActualHoursIncludingChildren);
			task.P9_ActualDuration = new ZDateTime(2012, 1, 1, 1, 0, 0);
			AssertEquals(0, (int)workflow1.TotalActualHoursIncludingChildren);
			Factory.Save();
			AssertEquals(1, (int)workflow1.TotalActualHoursIncludingChildren);
		}

		public void TestTotalEstimatedHoursSummaryAndOverallEstimateFactor()
		{
			var dummyWithWorkflow = Factory.New<DummyWithWorkflow>();

			var flowHeader = ProcessJobHeader.GetForParent(dummyWithWorkflow, Factory);

			var processHeader = flowHeader.ProcessHeaders[0];
			var processHeader2 = flowHeader.ProcessHeaders.AddNew();
			AssertEquals("0.0 hrs to 0.0 hrs (0.0 standard estimate).", processHeader.TotalEstimatedHoursSummary);
			AssertEquals(0m, processHeader.OverallEstimateFactor);

			var task1 = dummyWithWorkflow.WorkflowItems.AddNew();
			task1.P9_EstDuration = new ZDateTime(2012, 1, 1, 1, 30, 0);
			task1.P9_EstimateVariationFactor = 2;
			task1.P9_FH_ProcessHeader = processHeader2.PK;

			var task2 = dummyWithWorkflow.WorkflowItems.AddNew();
			task2.P9_EstDuration = new ZDateTime(2012, 1, 1, 3, 0, 0);
			task2.P9_EstimateVariationFactor = 3;
			task2.P9_FH_ProcessHeader = processHeader2.PK;

			var task3 = dummyWithWorkflow.WorkflowItems.AddNew();
			task3.P9_EstDuration = new ZDateTime(2012, 1, 1, 5, 30, 0);
			task3.P9_EstimateVariationFactor = 1;
			task3.P9_FH_ProcessHeader = processHeader2.PK;

			AssertEquals("0.0 hrs to 0.0 hrs (0.0 standard estimate).", processHeader.TotalEstimatedHoursSummary);
			task1.P9_FH_ProcessHeader = processHeader.PK;
			task2.P9_FH_ProcessHeader = processHeader.PK;
			task3.P9_FH_ProcessHeader = processHeader.PK;
			AssertEquals("10.0 hrs to 17.5 hrs (13.7 standard estimate).", processHeader.TotalEstimatedHoursSummary);
			AssertEquals(1.75m, processHeader.OverallEstimateFactor);

			var task4 = dummyWithWorkflow.WorkflowItems.AddNew();
			task4.P9_FH_ProcessHeader = processHeader.PK;
			AssertEquals("10.0 hrs to 17.5 hrs (13.7 standard estimate).", processHeader.TotalEstimatedHoursSummary);
			AssertEquals(1.75m, processHeader.OverallEstimateFactor);
		}

		public void TestTotalEstimatedHoursSummaryIncludesOpenChildAndGrandchildWorkflows()
		{
			var dummyWithWorkflow = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			var jobHeader = ProcessJobHeader.GetForParent(dummyWithWorkflow, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			BMSTestHelper.CreateTask(workflow1, lowEstMinutes: 180, estVariationFactor: 2);
			BMSTestHelper.CreateTask(workflow2, lowEstMinutes: 120, estVariationFactor: 2);
			BMSTestHelper.CreateTask(workflow3, lowEstMinutes: 60, estVariationFactor: 2);

			foreach (var workflow in new[] { workflow1, workflow2, workflow3 })
			{
				Assert("Expecting workflow to be open", workflow.IsOpen);
			}

			AssertEquals("6.0 hrs to 12.0 hrs.", jobHeader.TotalEstimatedHoursSummary);
			AssertEquals("3.0 hrs to 6.0 hrs (4.5 standard estimate).", workflow1.TotalEstimatedHoursSummary);
			AssertEquals("2.0 hrs to 4.0 hrs (3.0 standard estimate).", workflow2.TotalEstimatedHoursSummary);
			AssertEquals("1.0 hrs to 2.0 hrs (1.5 standard estimate).", workflow3.TotalEstimatedHoursSummary);

			BMSTestHelper.MakeChildOf(workflow2, workflow1);
			BMSTestHelper.MakeChildOf(workflow3, workflow2);

			AssertEquals("6.0 hrs to 12.0 hrs.", jobHeader.TotalEstimatedHoursSummary);
			AssertEquals("6.0 hrs to 12.0 hrs (9.0 standard estimate).", workflow1.TotalEstimatedHoursSummary);
			AssertEquals("3.0 hrs to 6.0 hrs (4.5 standard estimate).", workflow2.TotalEstimatedHoursSummary);
			AssertEquals("1.0 hrs to 2.0 hrs (1.5 standard estimate).", workflow3.TotalEstimatedHoursSummary);
		}

		public void TestTotalEstimatedHoursSummaryIncludesClosedChildAndGrandchildWorkflows()
		{
			var dummyWithWorkflow = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			var jobHeader = ProcessJobHeader.GetForParent(dummyWithWorkflow, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			var task1 = BMSTestHelper.CreateTask(workflow1, lowEstMinutes: 180, estVariationFactor: 2);
			var task2 = BMSTestHelper.CreateTask(workflow2, lowEstMinutes: 120, estVariationFactor: 2);
			var task3 = BMSTestHelper.CreateTask(workflow3, lowEstMinutes: 60, estVariationFactor: 2);

			foreach (var task in new[] { task1, task2, task3 })
			{
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			}

			foreach (var workflow in new[] { workflow1, workflow2, workflow3 })
			{
				Assert("Expecting workflow to be closed", !workflow.IsOpen);
			}

			AssertEquals("6.0 hrs to 12.0 hrs.", jobHeader.TotalEstimatedHoursSummary);
			AssertEquals("3.0 hrs to 6.0 hrs (4.5 standard estimate).", workflow1.TotalEstimatedHoursSummary);
			AssertEquals("2.0 hrs to 4.0 hrs (3.0 standard estimate).", workflow2.TotalEstimatedHoursSummary);
			AssertEquals("1.0 hrs to 2.0 hrs (1.5 standard estimate).", workflow3.TotalEstimatedHoursSummary);

			BMSTestHelper.MakeChildOf(workflow2, workflow1);
			BMSTestHelper.MakeChildOf(workflow3, workflow2);

			AssertEquals("6.0 hrs to 12.0 hrs.", jobHeader.TotalEstimatedHoursSummary);
			AssertEquals("6.0 hrs to 12.0 hrs (9.0 standard estimate).", workflow1.TotalEstimatedHoursSummary);
			AssertEquals("3.0 hrs to 6.0 hrs (4.5 standard estimate).", workflow2.TotalEstimatedHoursSummary);
			AssertEquals("1.0 hrs to 2.0 hrs (1.5 standard estimate).", workflow3.TotalEstimatedHoursSummary);
		}

		public void TestTotalActualHours()
		{
			var dummyWithWorkflow = Factory.New<DummyWithWorkflow>();
			var flowHeader = ProcessJobHeader.GetForParent(dummyWithWorkflow, Factory);
			var processHeader = flowHeader.ProcessHeaders[0];
			var processHeader2 = flowHeader.ProcessHeaders.AddNew();
			AssertEquals(0m, processHeader.TotalActualHours);
			AssertEquals("0:00", processHeader.TotalActualHoursLabel);

			var task1 = dummyWithWorkflow.WorkflowItems.AddNew();
			task1.P9_ActualDuration = new ZDateTime(2012, 1, 1, 1, 30, 0);
			task1.P9_FH_ProcessHeader = processHeader.PK;
			AssertEquals(1.5m, processHeader.TotalActualHours);
			AssertEquals("1:30", processHeader.TotalActualHoursLabel);

			var task2 = dummyWithWorkflow.WorkflowItems.AddNew();
			task2.P9_ActualDuration = new ZDateTime(2012, 1, 1, 3, 45, 0);
			task2.P9_FH_ProcessHeader = processHeader2.PK;
			AssertEquals(1.5m, processHeader.TotalActualHours);
			AssertEquals("1:30", processHeader.TotalActualHoursLabel);

			task2.P9_FH_ProcessHeader = processHeader.PK;
			AssertEquals(5.25m, processHeader.TotalActualHours);
			AssertEquals("5:15", processHeader.TotalActualHoursLabel);

			var task3 = dummyWithWorkflow.WorkflowItems.AddNew();
			task3.P9_FH_ProcessHeader = processHeader.PK;
			AssertEquals(5.25m, processHeader.TotalActualHours);
			AssertEquals("5:15", processHeader.TotalActualHoursLabel);
		}

		public void TestRemainingEstimateHours_IsRemainingEstimationCorrect()
		{
			var dummyWorkflowProvider = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			var jobHeader = ProcessJobHeader.GetForParent(dummyWorkflowProvider, Factory);
			var processHeader = jobHeader.ProcessHeaders[0];

			AssertEquals(0m, processHeader.RemainingEstimateHours);

			var task1 = BMSTestHelper.CreateTask(processHeader, lowEstMinutes: 90, estVariationFactor: 1);
			var task2 = BMSTestHelper.CreateTask(processHeader, lowEstMinutes: 90, estVariationFactor: 1);

			Factory.Save();

			AssertEquals(3m, processHeader.RemainingEstimateHours);
		}

		public void TestRemainingEstimateHoursIncludingChildren_IsRemainingEstimationCorrect()
		{
			var dummyWithWorkflow = BMSTestHelper.CreateDummyWorkflowProvider(Factory);
			var jobHeader = ProcessJobHeader.GetForParent(dummyWithWorkflow, Factory);
			var processHeader1 = jobHeader.ProcessHeaders[0];

			var processHeader2 = jobHeader.ProcessHeaders.AddNew();
			BMSTestHelper.MakeChildOf(processHeader2, processHeader1);

			AssertEquals(0m, processHeader1.RemainingEstimateHoursIncludingChildren);

			var task1 = BMSTestHelper.CreateTask(processHeader1, lowEstMinutes: 90, estVariationFactor: 1);
			var task2 = BMSTestHelper.CreateTask(processHeader2, lowEstMinutes: 90, estVariationFactor: 1);

			Factory.Save();

			AssertEquals(1.5m, processHeader1.RemainingEstimateHours);
			AssertEquals(3m, processHeader1.RemainingEstimateHoursIncludingChildren);
		}

		#endregion

		#region Effective Buffer Duration
		public void TestEffectiveBufferDuration_SetFromWorkflow()
		{
			var bufferTimespan = Factory.NewWithValidTestData<BMBufferTimespan>();
			bufferTimespan.BMT_BufferTimespanInMinutes = 60;
			Factory.Save();

			var workflow = Factory.NewWithValidTestData<ProcessHeader>();
			workflow.FH_BMT_BufferTimespan = bufferTimespan.PK;
			Factory.Save();

			var effectiveBufferDuration = workflow.EffectiveBufferDurationMinutes;

			AssertEquals(60, (int)effectiveBufferDuration);
		}

		public void TestEffectiveBufferDuration_SetFromJLW()
		{
			var jlwBufferTimespan = Factory.NewWithValidTestData<BMBufferTimespan>();
			jlwBufferTimespan.BMT_BufferTimespanInMinutes = 120;
			Factory.Save();

			var jobLevelWorkflow = Factory.NewWithValidTestData<ProcessJobHeader>();
			jobLevelWorkflow.FH_BMT_BufferTimespan = jlwBufferTimespan.PK;
			Factory.Save();

			var workflow = Factory.NewWithValidTestData<ProcessHeader>();
			workflow.FH_FH_ParentHeader = jobLevelWorkflow.PK;
			Factory.Save();

			var effectiveBufferDuration = workflow.EffectiveBufferDuration;

			AssertEquals(TimeSpan.FromHours(2), effectiveBufferDuration);
		}

		public void TestEffectiveBufferDuration_SetFromDedicatedBuffer()
		{
			var dedicatedBuffer = Factory.NewWithValidTestData<BMComponent>();
			dedicatedBuffer.FC_Type = "BUF";
			dedicatedBuffer.FC_BufferTimespanInMinutes = 180;
			Factory.Save();

			var workflow = Factory.NewWithValidTestData<ProcessHeader>();
			workflow.FH_FC_DedicatedBuffer = dedicatedBuffer.PK;
			Factory.Save();

			var effectiveBufferDuration = workflow.EffectiveBufferDuration;

			AssertEquals(TimeSpan.FromHours(3), effectiveBufferDuration);
		}

		public void TestEffectiveBufferDuration_Default()
		{
			var workflow = Factory.NewWithValidTestData<ProcessHeader>();
			Factory.Save();

			var effectiveBufferDuration = workflow.EffectiveBufferDuration;

			AssertEquals(TimeSpan.Zero, effectiveBufferDuration);
		}

		public void TestEffectiveBufferDuration_Precedence()
		{
			var workflowBufferTimespan = Factory.NewWithValidTestData<BMBufferTimespan>();
			workflowBufferTimespan.BMT_BufferTimespanInMinutes = 60;

			var jlwBufferTimespan = Factory.NewWithValidTestData<BMBufferTimespan>();
			jlwBufferTimespan.BMT_BufferTimespanInMinutes = 120;

			Factory.Save();

			var dedicatedBuffer = Factory.NewWithValidTestData<BMComponent>();
			dedicatedBuffer.FC_Type = "BUF";
			dedicatedBuffer.FC_BufferTimespanInMinutes = 180;
			Factory.Save();

			var jobLevelWorkflow = Factory.NewWithValidTestData<ProcessJobHeader>();
			jobLevelWorkflow.FH_BMT_BufferTimespan = jlwBufferTimespan.PK;
			Factory.Save();

			var workflow = Factory.NewWithValidTestData<ProcessHeader>();
			workflow.FH_BMT_BufferTimespan = workflowBufferTimespan.PK;
			workflow.FH_FH_ParentHeader = jobLevelWorkflow.PK;
			workflow.FH_FC_DedicatedBuffer = dedicatedBuffer.PK;
			Factory.Save();

			var effectiveBufferDuration = workflow.EffectiveBufferDuration;

			AssertEquals(TimeSpan.FromHours(1), effectiveBufferDuration);
		}
		#endregion

		#region Penetrated Component

		[TestDate(2012, 12, 3, 0, 0, 0)]
		public void TestPenetratedComponent_WithValidReleaseDate()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system, "Parent Component");

			var ccrResource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CCR", "CCR");
			ccrResource.DesignateAsCCR(buffer);

			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			var nonCCRtask = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 10);
			var cCRtask = BMSTestHelper.CreateTask(workflow, ccrResource.GS_Code, 60, sequence: 20);

			var preBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Pre", timespanMinutes: 64 * 60);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "Constraint", offsetMinutes: 64 * 60);
			var postBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Post", timespanMinutes: 32 * 60, offsetMinutes: 64 * 60);

			Factory.Save();

			workflow.FH_ReleaseDateTime = new ZDateTime(2012, 12, 3, 0, 0, 0);
			AssertEquals(@"Parent Component - Zone 3
	Pre - Zone 3", workflow.CurrentStatus);

			TestDateAttribute.Date = new DateTime(2012, 12, 7, 0, 0, 0);
			ClearCachedCurrentStatus(workflow.PK);

			AssertMultilineASCIIEquals("",
	@"Parent Component - Zone 2
	Pre - Zone 2", workflow.CurrentStatus);

			TestDateAttribute.Date = new DateTime(2012, 12, 11, 0, 0, 0);
			ClearCachedCurrentStatus(workflow.PK);

			AssertMultilineASCIIEquals("",
	@"Parent Component - Zone 2
	Pre - Zone 1", workflow.CurrentStatus);

			TestDateAttribute.Date = new DateTime(2012, 12, 28, 0, 0, 0);
			ClearCachedCurrentStatus(workflow.PK);

			AssertMultilineASCIIEquals("",
	@"Parent Component - Zone 0
	Pre - Zone 0", workflow.CurrentStatus);

			AssertEquals("Current Component: Parent Component - Zone 0, Penetrated Components: Pre - Zone 0", workflow.CurrentStatusOnSingleLine);
		}

		[TestDate(2014, 1, 29)]
		public void TestPenetratedComponent_NonCCR_Preconstraint()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty);
			var section = sectionAndView.Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;
			var buffer = section.Component;
			var preConstraint = BMSTestHelper.CreateSubBuffer(buffer, "Pre-Constraint", timespanMinutes: 64 * 60);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "Constraint", offsetMinutes: 64 * 60);
			var postConstraint = BMSTestHelper.CreateSubBuffer(buffer, "Post-Constraint", timespanMinutes: 32 * 60, offsetMinutes: 64 * 60);

			var resourceNonCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NC1", "Non CCR Resource 1");
			var resourceCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR1", "CCR Resource 1");
			resourceCCR1.DesignateAsCCR(buffer);

			var workflow = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: new ZDateTime(2014, 1, 22, 8, 36, 0).ToUniversalBranchTime(), staffCode: resourceNonCCR1.GS_Code, lowEstMinutes: 60, sequence: 10);
			var task = BMSTestHelper.CreateTask(workflow, resourceCCR1.GS_Code, 60, sequence: 20);

			Factory.Save();

			var penetratedComponents = workflow.PenetratedComponents.ToArray();

			AssertEquals(1, penetratedComponents.Length);
			AssertEquals(preConstraint, penetratedComponents[0]);

			AssertMultilineASCIIEquals("", @"buffer - Zone 2
	Pre-Constraint - Zone 2", workflow.CurrentStatus);
		}

		[TestDate(2014, 1, 29)]
		public void TestPenetratedComponent_NonCCR_Postconstraint()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty);
			var section = sectionAndView.Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;
			var buffer = section.Component;
			var preBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Pre-Constraint", timespanMinutes: 64 * 60);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "Constraint", offsetMinutes: 64 * 60);
			var postBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Post-Constraint", timespanMinutes: 32 * 60, offsetMinutes: 64 * 60);

			var resourceNonCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NC1", "Non CCR Resource 1");
			var resourceCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR1", "CCR Resource 1");
			resourceCCR1.DesignateAsCCR(buffer);

			var workflow = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: new ZDateTime(2014, 1, 21, 0, 0, 0).ToUniversalBranchTime(), staffCode: resourceNonCCR1.GS_Code, lowEstMinutes: 60, sequence: 20);
			var task = BMSTestHelper.CreateTask(workflow, resourceCCR1.GS_Code, 60, sequence: 10, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

			Factory.Save();

			var penetratedComponents = workflow.PenetratedComponents.ToArray();

			AssertEquals(1, penetratedComponents.Length);
			AssertEquals(postBuffer, penetratedComponents[0]);

			AssertMultilineASCIIEquals("", @"buffer - Zone 2
	Post-Constraint - Zone 3", workflow.CurrentStatus);
		}

		[TestDate(2014, 1, 29)]
		public void TestPenetratedPostConstraint_With2Constraints()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var sectionAndView = BMSTestHelper.CreateSectionAndViewModel(config.Buffer, 1, 13, FlowDirectionList.Codes.Up, LastCellList.Codes.Right, string.Empty);
			var section = sectionAndView.Item1;
			section.SectionConfiguration.OverrideChannels = true;
			section.SectionConfiguration.FadeBackgroundAtPercentage = 80;
			var buffer = section.Component;
			var preBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Pre-Constraint", timespanMinutes: 64 * 60);
			var constraint1 = BMSTestHelper.CreateConstraint(buffer, "Constraint 1", offsetMinutes: 64 * 60);
			var postBuffer1 = BMSTestHelper.CreateSubBuffer(buffer, "Post-Constraint 1", timespanMinutes: 32 * 60, offsetMinutes: 64 * 60);
			var constraint2 = BMSTestHelper.CreateConstraint(buffer, "Constraint 2", offsetMinutes: 96 * 60);
			var postBuffer2 = BMSTestHelper.CreateSubBuffer(buffer, "Post-Constraint 2", timespanMinutes: 32 * 60, offsetMinutes: 96 * 60);

			var resourceNonCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NC1", "Non CCR Resource 1");
			var resourceCCR1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR1", "CCR Resource 1");
			resourceCCR1.DesignateAsCCR(buffer);

			var workflow = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: new ZDateTime(2014, 1, 21, 0, 0, 0).ToUniversalBranchTime(), staffCode: resourceNonCCR1.GS_Code, lowEstMinutes: 60, sequence: 20);
			var task = BMSTestHelper.CreateTask(workflow, resourceCCR1.GS_Code, 60, sequence: 10, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

			Factory.Save();

			var penetratedComponents = workflow.PenetratedComponents.ToArray();

			AssertEquals(2, penetratedComponents.Length);
			AssertEquals(postBuffer1, penetratedComponents[0]);
			AssertEquals(postBuffer2, penetratedComponents[1]);

			AssertMultilineASCIIEquals("", @"buffer - Zone 2
	Post-Constraint 1 - Zone 3
	Post-Constraint 2 - Zone 3", workflow.CurrentStatus);
		}

		[TestDate(2012, 12, 28, 0, 0, 0)]
		public void TestPenetratedComponent_WithInvalidReleaseDate()
		{
			var workflow = (ProcessHeader)GetNewBusinessObject();
			var system = Factory.New<BMSystem>();
			var parentComponent = BMSTestHelper.CreateBuffer(system, "Parent Component");
			workflow.FH_FC_CurrentComponent = parentComponent.PK;

			var child = BMSTestHelper.CreateSubBuffer(parentComponent, "Child Component", timespanMinutes: 5, offsetMinutes: 10);

			workflow.FH_ReleaseDateTime = ZDateTime.Invalid;
			AssertEquals("Parent Component - Zone 3", workflow.CurrentStatus);

			TestDateAttribute.Date = new DateTime(2012, 12, 28, 0, 10, 0);
			AssertEquals("Parent Component - Zone 3", workflow.CurrentStatus);
		}

		[TestDate(2012, 12, 28, 0, 0, 0)]
		public void TestPenetratedComponent_WithNonBufferComponent()
		{
			var header = (ProcessHeader)GetNewBusinessObject();
			var system = Factory.New<BMSystem>();
			var parentComponent = system.Components.AddNew();
			parentComponent.FC_Name = "Parent Component";
			parentComponent.FC_Type = BMComponentTypeList.Codes.Bucket;
			header.FH_FC_CurrentComponent = parentComponent.PK;

			var child = parentComponent.ChildComponents.AddNew();
			child.FC_OffsetInMinutes = 10;
			child.FC_BufferTimespanInMinutes = 5;
			child.FC_Name = "Child Component";

			header.FH_ReleaseDateTime = new ZDateTime(2012, 12, 28, 0, 0, 0);
			AssertEquals("Parent Component", header.CurrentStatus);

			TestDateAttribute.Date = new DateTime(2012, 12, 28, 0, 10, 0);
			AssertEquals("Parent Component", header.CurrentStatus);
		}

		[TestDate(2014, 1, 29)]
		public void TestPenetratedComponents_ShouldUseWorkingHours()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			var preConstraintBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Pre-Constraint", timespanMinutes: 64 * 60);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "Constraint", offsetMinutes: 64 * 60);
			var postConstraintBuffer = BMSTestHelper.CreateSubBuffer(buffer, "Post-Constraint", timespanMinutes: 32 * 60, offsetMinutes: 64 * 60);

			var ccrResource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CCR", "CCR");
			ccrResource.DesignateAsCCR(buffer);

			var workflow = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory).ProcessHeaders[0];
			workflow.FH_ReleaseDateTime = new ZDateTime(2014, 1, 22, 8, 36, 0).ToUniversalBranchTime();
			var nonCCRTask = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, sequence: 10);
			var cCRtask = BMSTestHelper.CreateTask(workflow, ccrResource.GS_Code, 60, sequence: 20);

			Factory.Save();

			var penetratedComponents = workflow.PenetratedComponents.ToArray();

			AssertEquals(1, penetratedComponents.Length);
			AssertMultilineASCIIEquals("", @"Buffer - Zone 2
	Pre-Constraint - Zone 2", workflow.CurrentStatus);

			workflow.FH_ReleaseDateTime = workflow.FH_ReleaseDateTime.AddDays(-5);
			ClearCachedCurrentStatus(workflow.PK);

			penetratedComponents = workflow.PenetratedComponents.ToArray();

			AssertEquals(1, penetratedComponents.Length);
			AssertCollectionContains(penetratedComponents, c => c == preConstraintBuffer);
			AssertMultilineASCIIEquals("", @"Buffer - Zone 1
	Pre-Constraint - Zone 0", workflow.CurrentStatus);

			workflow.FH_ReleaseDateTime = workflow.FH_ReleaseDateTime.AddDays(-2);
			ClearCachedCurrentStatus(workflow.PK);

			AssertMultilineASCIIEquals("", @"Buffer - Zone 1
	Pre-Constraint - Zone 0", workflow.CurrentStatus);

			workflow.FH_ReleaseDateTime = workflow.FH_ReleaseDateTime.AddDays(-2);
			ClearCachedCurrentStatus(workflow.PK);

			AssertMultilineASCIIEquals("", @"Buffer - Zone 0
	Pre-Constraint - Zone 0", workflow.CurrentStatus);
		}

		void ClearCachedCurrentStatus(ZGuid pk)
		{
			Factory.ClearCachedValue<string>("ProcessHeader.CurrentStatus" + pk);
			Factory.ClearCachedValue<BufferPenetrationCalculator.BufferPenetrationCache>();
		}

		#endregion

		#region System

		public void TestGetSystem_WhenNoJobHeaderPresent()
		{
			var header = Factory.NewWithValidTestData<ProcessHeader>();
			AssertNull(header.BMSystem);
		}

		public void TestSystem_ShouldLoadWhenNoCurrentComponent()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var org = Factory.New<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var processHeader = jobHeader.ProcessHeaders[0];

			AssertNull(processHeader.CurrentComponent);
			AssertEquals(system, processHeader.BMSystem);

			var otherSystem = Factory.New<BMSystem>();
			var component = otherSystem.Components.AddNew();

			processHeader.FH_FC_CurrentComponent = component.PK;
			AssertEquals(otherSystem, processHeader.BMSystem);
		}

		#endregion

		#region Sequence

		public void TestSequence_ShouldUseDependencyOrder()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			AssertEquals(string.Empty, jobHeader.Sequence);
			AssertEquals("1", jobHeader.ProcessHeaders[0].Sequence);

			var link = Factory.New<ProcessHeaderLink>();
			link.FP_FH_HeaderFrom = jobHeader.ProcessHeaders.AddNew().PK;
			link.FP_FH_HeaderTo = jobHeader.ProcessHeaders[0].PK;
			link.FP_LinkType = "DEP";

			AssertEquals("1", jobHeader.ProcessHeaders[0].Sequence);
			AssertEquals(string.Empty, jobHeader.ProcessHeaders[1].Sequence);

			Factory.Save();
			AssertEquals("2", jobHeader.ProcessHeaders[0].Sequence);
			AssertEquals("1", jobHeader.ProcessHeaders[1].Sequence);
		}

		public void TestSequence_ShouldOrderByCreateTime()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "XXX");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "YYY");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "ZZZ");

			Factory.Save(); // Force population of system create time.

			CombineAssertions("Default sequence is by description when dependencies aren't present", () =>
			{
				AssertEquals("jobHeader", string.Empty, jobHeader.Sequence);
				AssertEquals("workflow1", "1", workflow1.Sequence);
				AssertEquals("workflow2", "2", workflow2.Sequence);
				AssertEquals("workflow3", "3", workflow3.Sequence);
			});

			workflow1.FH_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			workflow2.FH_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-3);
			workflow3.FH_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-5);

			Factory.Save();

			CombineAssertions("SystemCreateTime difference forces workflow sequence to reverse", () =>
			{
				AssertEquals("jobHeader", string.Empty, jobHeader.Sequence);
				AssertEquals("workflow1", "3", workflow1.Sequence);
				AssertEquals("workflow2", "2", workflow2.Sequence);
				AssertEquals("workflow3", "1", workflow3.Sequence);
			});
		}

		public void TestSequence_ChildWorkflowWithinJob()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");

			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow3);

			var childWorkflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "childWorkflow1");
			var childWorkflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "childWorkflow2");
			var childWorkflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "childWorkflow3");

			var grandchildWorkflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "grandchildWorkflow1");
			var grandchildWorkflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "grandchildWorkflow2");
			var grandchildWorkflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "grandchildWorkflow3");

			childWorkflow1.GetOrCreateLinkToParent(workflow1);
			childWorkflow2.GetOrCreateLinkToParent(workflow1);
			childWorkflow3.GetOrCreateLinkToParent(workflow3);

			grandchildWorkflow1.GetOrCreateLinkToParent(childWorkflow1);
			grandchildWorkflow2.GetOrCreateLinkToParent(childWorkflow1);
			grandchildWorkflow3.GetOrCreateLinkToParent(childWorkflow2);

			Factory.Save();

			AssertEquals(string.Empty, jobHeader.Sequence);
			AssertEquals("1", workflow1.Sequence);
			AssertEquals("2", workflow2.Sequence);
			AssertEquals("3", workflow3.Sequence);

			AssertEquals("1.1", childWorkflow1.Sequence);
			AssertEquals("1.2", childWorkflow2.Sequence);
			AssertEquals("3.1", childWorkflow3.Sequence);

			AssertEquals("1.1.1", grandchildWorkflow1.Sequence);
			AssertEquals("1.1.2", grandchildWorkflow2.Sequence);
			AssertEquals("1.2.1", grandchildWorkflow3.Sequence);
		}

		public void TestSequence_ChildWorkflowInAnotherJob()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2");

			var childWorkflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "childWorkflow1");
			var childWorkflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "childWorkflow2");

			childWorkflow1.GetOrCreateLinkToParent(workflow1);
			childWorkflow2.GetOrCreateLinkToParent(workflow1);

			workflow2.GetOrCreateDependencyLink(childWorkflow2);

			Factory.Save();

			AssertEquals(string.Empty, jobHeader1.Sequence);
			AssertEquals(string.Empty, jobHeader2.Sequence);
			AssertEquals("1", workflow1.Sequence);
			AssertEquals("1", workflow2.Sequence);

			AssertEquals("1.1", childWorkflow1.Sequence);
			AssertEquals("2", childWorkflow2.Sequence);
		}

		public void TestSequence_WhenJobHasChildWorkflowsInOtherJobs_ShouldIgnoreExternalRelationships()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var wf1_jobHeader1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1 jobHeader1");
			var wf2_jobHeader1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow2 jobHeader1");

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var wf1_jobHeader2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow1 jobHeader2");
			var wf2_jobHeader2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2 jobHeader2");
			var wf3_jobHeader2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow3 jobHeader2");
			var wf4_jobHeader2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow4 jobHeader2");

			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var wf1_jobHeader3 = BMSTestHelper.CreateWorkflow(jobHeader3, "workflow1 jobHeader3");
			var wf2_jobHeader3 = BMSTestHelper.CreateWorkflow(jobHeader3, "workflow2 jobHeader3");
			var wf3_jobHeader3 = BMSTestHelper.CreateWorkflow(jobHeader3, "workflow3 jobHeader3");
			var wf4_jobHeader3 = BMSTestHelper.CreateWorkflow(jobHeader3, "workflow4 jobHeader3");
			var wf5_jobHeader3 = BMSTestHelper.CreateWorkflow(jobHeader3, "workflow5 jobHeader3");

			var jobHeader4 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var wf1_jobHeader4 = BMSTestHelper.CreateWorkflow(jobHeader4, "workflow1 jobHeader4");
			var wf2_jobHeader4 = BMSTestHelper.CreateWorkflow(jobHeader4, "workflow2 jobHeader4");
			var wf3_jobHeader4 = BMSTestHelper.CreateWorkflow(jobHeader4, "workflow3 jobHeader4");
			var wf4_jobHeader4 = BMSTestHelper.CreateWorkflow(jobHeader4, "workflow4 jobHeader4");
			var wf5_jobHeader4 = BMSTestHelper.CreateWorkflow(jobHeader4, "workflow5 jobHeader4");
			var wf6_jobHeader4 = BMSTestHelper.CreateWorkflow(jobHeader4, "workflow6 jobHeader4");

			jobHeader3.GetOrCreateLinkToParent(jobHeader4);

			wf1_jobHeader2.GetOrCreateLinkToParent(jobHeader4);
			wf3_jobHeader2.GetOrCreateLinkToParent(jobHeader4);
			wf4_jobHeader2.GetOrCreateLinkToParent(jobHeader4);

			wf1_jobHeader1.GetOrCreateLinkToParent(wf1_jobHeader2);
			wf1_jobHeader2.GetOrCreateLinkToParent(wf2_jobHeader3);
			wf2_jobHeader3.GetOrCreateLinkToParent(wf1_jobHeader4);
			wf1_jobHeader4.GetOrCreateLinkToParent(wf3_jobHeader4);

			Factory.Save();

			CombineAssertions(@"jobHeader4: workflow Sequence should be consistent within one job,
even if this job has external relationships.
For example right sequence: 1, 2, 3, 4 etc,
but wrong sequence: 1, 7, 8, 9, 11, 16 etc", () =>
			{
				AssertEquals(string.Empty, jobHeader4.Sequence);
				AssertEquals("1", wf2_jobHeader4.Sequence);
				AssertEquals("2", wf3_jobHeader4.Sequence);
				AssertEquals("Because wf1_jobHeader4 is a child of wf3_jobHeader4 whithing the same job", "2.1", wf1_jobHeader4.Sequence);
				AssertEquals("3", wf4_jobHeader4.Sequence);
				AssertEquals("4", wf5_jobHeader4.Sequence);
				AssertEquals("5", wf6_jobHeader4.Sequence);
			});

			CombineAssertions(@"jobHeader3: workflow Sequence should be consistent within one job,
even if this job or his workflows have external relationships.
For example right sequence: 1, 2, 3, 4 etc,
but wrong sequence: 1, 7, 8, 9, 11, 16 etc", () =>
			{
				AssertEquals(string.Empty, jobHeader3.Sequence);
				AssertEquals("1", wf1_jobHeader3.Sequence);
				AssertEquals("2", wf2_jobHeader3.Sequence);
				AssertEquals("3", wf3_jobHeader3.Sequence);
				AssertEquals("4", wf4_jobHeader3.Sequence);
				AssertEquals("5", wf5_jobHeader3.Sequence);
			});
		}

		#endregion

		#region CurrentTaskResource

		public void TestCurrentTaskResource()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header1 = jobHeader.ProcessHeaders[0];
			var header2 = jobHeader.ProcessHeaders.AddNew();

			var task1_1 = job.WorkflowItems.AddNew();
			task1_1.P9_FH_ProcessHeader = header1.PK;
			task1_1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task1_1.P9_GS_NKAssignedStaffMember = ZString.Empty;
			task1_1.P9_Sequence = 10;

			var task1_2 = job.WorkflowItems.AddNew();
			task1_2.P9_FH_ProcessHeader = header1.PK;
			task1_2.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task1_2.P9_Sequence = 11;

			var task2_1 = job.WorkflowItems.AddNew();
			task2_1.P9_FH_ProcessHeader = header2.PK;
			task2_1.P9_GS_NKAssignedStaffMember = ZString.Empty;
			task2_1.P9_Sequence = 1;

			AssertEquals("task1_2 is the current task of workflow 1", staff.GS_Code, header1.CurrentTaskResourceCode);
			AssertEquals("task1_2 is the current task of workflow 1", staff.GS_FullName, header1.CurrentTaskResourceName);
			AssertEquals("task2_1 is the current task of workflow 2", ZString.Empty, header2.CurrentTaskResourceCode);
			AssertEquals("task2_1 is the current task of workflow 2", ZString.Empty, header2.CurrentTaskResourceName);

			task1_2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2_1.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			AssertEquals("No current task for workflow 1", ZString.Empty, header1.CurrentTaskResourceCode);
			AssertEquals("No current task for workflow 1", ZString.Empty, header1.CurrentTaskResourceName);
			AssertEquals("task2_1 is the current task of workflow 1", staff.GS_Code, header2.CurrentTaskResourceCode);
			AssertEquals("task2_1 is the current task of workflow 1", staff.GS_FullName, header2.CurrentTaskResourceName);
		}

		#endregion

		#region LastEditTime

		[TestDate(2013, 10, 28, 9, 0, 0)]
		public void TestChangingTaskStatus_ShouldUpdateWorkflowLastEditTime()
		{
			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<DummyWithWorkflow>(), Factory).ProcessHeaders[0];
			var task1 = BMSTestHelper.CreateTask(workflow);
			var task2 = BMSTestHelper.CreateTask(workflow);

			task1.P9_GS_NKAssignedStaffMember = task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();
			AssertEquals(new ZDateTime(2013, 10, 28, 9, 0, 0), workflow.FH_SystemLastEditTimeUtc);

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			Factory.Save();
			AssertEquals(new ZDateTime(2013, 10, 28, 10, 0, 0), workflow.FH_SystemLastEditTimeUtc);
		}

		[TestDate(2013, 10, 28, 9, 0, 0)]
		public void TestDeleteTask_ShouldUpdateWorkflowLastEditTime()
		{
			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<DummyWithWorkflow>(), Factory).ProcessHeaders[0];
			var task1 = BMSTestHelper.CreateTask(workflow);
			var task2 = BMSTestHelper.CreateTask(workflow);

			task1.P9_GS_NKAssignedStaffMember = task2.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
			task1.P9_Status = task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			Factory.Save();
			AssertEquals(new ZDateTime(2013, 10, 28, 9, 0, 0), workflow.FH_SystemLastEditTimeUtc);

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			task1.Delete();
			Factory.Save();
			AssertEquals(new ZDateTime(2013, 10, 28, 10, 0, 0), workflow.FH_SystemLastEditTimeUtc);
		}

		[TestDate(2013, 10, 28, 9, 0, 0)]
		public void TestAddNewTask_ShouldUpdateWorkflowLastEditTime()
		{
			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<DummyWithWorkflow>(), Factory).ProcessHeaders[0];
			var task1 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0);

			Factory.Save();
			AssertEquals(new ZDateTime(2013, 10, 28, 9, 0, 0), workflow.FH_SystemLastEditTimeUtc);

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);

			var task3 = BMSTestHelper.CreateTask(workflow);
			Factory.Save();
			AssertEquals(new ZDateTime(2013, 10, 28, 10, 0, 0), workflow.FH_SystemLastEditTimeUtc);
		}

		[TestDate(2013, 10, 28, 9, 0, 0)]
		public void TestUpdateSystemLastEditTime_ShouldUpdateParentWorkflows()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			var qualityIterationWorkflow1 = (ProcessHeader)((IProcessJobHeader)jobHeader).CreateQualityIterationWorkflow(workflow);
			var qualityIterationWorkflow2 = (ProcessHeader)((IProcessJobHeader)jobHeader).CreateQualityIterationWorkflow(qualityIterationWorkflow1);

			var task1 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0);
			var task2 = BMSTestHelper.CreateTask(qualityIterationWorkflow1, GlbStaff.CurrentUser.GS_Code, 0);
			var task3 = BMSTestHelper.CreateTask(qualityIterationWorkflow2, GlbStaff.CurrentUser.GS_Code, 0);

			Factory.Save();
			AssertEquals(new ZDateTime(2013, 10, 28, 9, 0, 0), workflow.FH_SystemLastEditTimeUtc);
			AssertEquals(new ZDateTime(2013, 10, 28, 9, 0, 0), qualityIterationWorkflow1.FH_SystemLastEditTimeUtc);
			AssertEquals(new ZDateTime(2013, 10, 28, 9, 0, 0), qualityIterationWorkflow2.FH_SystemLastEditTimeUtc);

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			qualityIterationWorkflow1.FH_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			AssertEquals(new ZDateTime(2013, 10, 28, 10, 0, 0), workflow.FH_SystemLastEditTimeUtc);
			AssertEquals(new ZDateTime(2013, 10, 28, 10, 0, 0), qualityIterationWorkflow1.FH_SystemLastEditTimeUtc);
			AssertEquals(new ZDateTime(2013, 10, 28, 9, 0, 0), qualityIterationWorkflow2.FH_SystemLastEditTimeUtc);

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			qualityIterationWorkflow2.FH_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			AssertEquals(new ZDateTime(2013, 10, 28, 11, 0, 0), workflow.FH_SystemLastEditTimeUtc);
			AssertEquals(new ZDateTime(2013, 10, 28, 11, 0, 0), qualityIterationWorkflow1.FH_SystemLastEditTimeUtc);
			AssertEquals(new ZDateTime(2013, 10, 28, 11, 0, 0), qualityIterationWorkflow2.FH_SystemLastEditTimeUtc);
		}

		[TestDate(2013, 10, 28, 9, 0, 0)]
		public void TestUpdateSystemLastEditTime_ShouldNotCauseStackOverflow()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var current = BMSTestHelper.CreateWorkflow(jobHeader, "current");
			var parent = BMSTestHelper.CreateWorkflow(jobHeader, "parent");

			current.ParentLinks_ForBinding.AddNew().FP_FH_HeaderTo = parent.PK;

			var linkChild = current.ChildLinks_ForBinding.AddNew();
			linkChild.FP_FH_HeaderFrom = parent.PK;

			current.FH_SystemLastEditTimeUtc = ZDateTime.UtcNow;
			AssertEquals("Parent's system-last-edit-time-utc must not be set if it is in looped relationship because error will be thrown", new ZDateTime(2013, 10, 28, 9, 0, 0), parent.FH_SystemLastEditTimeUtc);
		}

		#endregion

		#region Last Transfer Type

		public void TestFH_LastTransferTypeIsReadOnly()
		{
			var workflow = Factory.New<ProcessHeader>();
			AssertEquals(true, workflow.FH_LastTransferTypeInfo.ReadOnly);
		}

		public void TestLastTransferTypeDescription()
		{
			var workflow = Factory.New<ProcessHeader>();

			workflow.FH_LastTransferType = TransferTypeList.Codes.Defer;
			AssertEquals(TransferTypeList.Descriptions.Defer, workflow.LastTransferTypeDescription);

			workflow.FH_LastTransferType = "NA";
			AssertEquals(string.Empty, workflow.LastTransferTypeDescription);
		}

		public void TestFH_LastTransferType_TriggerActionsShouldNotBeAbleToChangeValue()
		{
			var job = Factory.New<IWorkItem>();
			var provider = job as IWorkflowProvider;
			var jobHeader = (ProcessJobHeader)BMSTestHelper.GetJobHeaderForParent(provider, Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			BMSTestHelper.CreateTask(workflow);

			var trigger = provider.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.WorkflowTransferredBetweenSystemComponents.Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_FieldName = "<CurrentTask.ProcessHeader.FH_LastTransferType>";
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;

			action.Validation.ValidateAll();

			AssertHasError(action.PQ_FieldNameInfo, "Property Enterprise.BufferManagement.Business.ProcessHeader.FH_LastTransferType of type ZString is read-only and cannot be used in 'Set Field' trigger action.");
		}

		#endregion

		#region Category

		public void TestFH_Category_GetsUDF_WhenNoCategoryIsSet()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, processType: WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var workflow = BMSTestHelper.CreateWorkflow(template);

			AssertEquals("UDF", workflow.FH_Category);
			Assert(!string.IsNullOrEmpty(workflow.CategoryDescription));
		}

		public void TestCategoryDescription_ReturnsEmpty_WhenWorkflowTypeIsEmpty()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			workflow.FH_WorkflowType = ZString.Empty;

			AssertEquals(ZString.Empty, workflow.CategoryDescription);
		}

		public void TestCategoryDescription_ReturnsEmpty_WhenTemplateProcessTypeIsEmpty()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, processType: WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var workflow = BMSTestHelper.CreateWorkflow(template);
			template.P0_ProcessType = ZString.Empty;

			AssertEquals(ZString.Empty, workflow.CategoryDescription);
		}

		public void TestFH_CategoryDescription_GetsDescription_WhenCategoryConfigured()
		{
			var (code, description) = ("TST", "Test category");
			var system = BMSTestHelper.CreateSystem(Factory, "INQ");
			var workflow = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Workflow");
			BMSTestHelper.AddWorkflowCategoryToRegistry(workflow.Parent.WorkflowType, code, description);
			workflow.FH_Category = code;

			AssertEquals(description, workflow.CategoryDescription);
		}

		public void TestFH_CategoryDescription_GetsEmpty_WhenCategoryNotConfigured()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, processType: WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var workflow = BMSTestHelper.CreateWorkflow(template);
			workflow.FH_Category = "TST";

			AssertEquals(ZString.Empty, workflow.CategoryDescription);
		}

		public void TestFH_Category_ReadOnly_IsFalse()
		{
			BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, processType: WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var workflow = BMSTestHelper.CreateWorkflow(template);

			Assert(!workflow.FH_CategoryInfo.ReadOnly);
		}

		#endregion

		#region Reset Task Penetration

		public void TestFH_TaskPenetrationResetDateTimeUtcIsReadOnly()
		{
			var workflow = Factory.New<ProcessHeader>();
			AssertEquals(true, workflow.FH_TaskPenetrationResetDateTimeUtcInfo.ReadOnly);
		}

		[TestDate(2020, 8, 7)]
		public void TestCalculatePenetrationPercentageForTask_SpecialPenetration()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var factory = new BusinessObjectFactory();
			var now = ZDateTime.UtcNow;

			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var config = TestConfigsHelper.CreateSchematicTestConfig(factory, "DUM");
			var branch = factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUSYD";
			var department = factory.NewWithValidTestData<GlbDepartment>();

			var buffer = config.Buffer;
			buffer.FC_GB_AgingBranch = branch.PK;
			buffer.FC_GE_AgingDepartment = department.PK;

			var group = factory.NewWithValidTestData<GlbGroup>();
			var link = buffer.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = group.PK;
			link.FO_ResetTaskPenetrationThrottleFactor = 2;

			var staff = group.Staff.AddNew();
			staff.GS_Code = "AAA";

			var job = factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, factory, addDefaultProcessHeaderIfNone: false);

			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "aaa", releaseGroupPK: group.PK.ToGuid());
			workflow.FH_TaskPenetrationResetDateTimeUtc = now.AddDays(-1);
			workflow.FH_FC_CurrentComponent = buffer.PK;
			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-5);

			var task = BMSTestHelper.CreateTask(workflow, staff.GS_Code, taskType: "UDF");
			task.P9_IsResetBeingAppliedToThisTask = true;

			var context = WorkingTimeContext.Create(buffer, staff, factory);

			factory.Save();

			AssertEquals("Workflow (non-special) penetration", 0.349m, Math.Round(workflow.CalculatePenetrationPercentage(context), 3));

			AssertEquals("Should calculate special penetration and apply a throttle factor of 2", 0.167m, Math.Round(workflow.CalculatePenetrationPercentage(task, context), 3));

			link.FO_ResetTaskPenetrationThrottleFactor = 1;
			Factory.Save();

			AssertEquals("Should calculate special penetration without a throttle factor", 0.083m, Math.Round(workflow.CalculatePenetrationPercentage(task, context), 3));

			link.FO_ResetTaskPenetrationThrottleFactor = 2;

			workflow.FH_TaskPenetrationResetDateTimeUtc = now.AddDays(-3);
			factory.Save();

			AssertEquals("Should not apply special penetration since it would exceed actual penetration", 0.349m, Math.Round(workflow.CalculatePenetrationPercentage(task, context), 3));

			var parentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "bbb", releaseGroupPK: group.PK.ToGuid());
			parentWorkflow.FH_FC_CurrentComponent = buffer.PK;
			parentWorkflow.FH_TaskPenetrationResetDateTimeUtc = now.AddDays(-4);
			BMSTestHelper.CreateTask(workflow, staff.GS_Code, taskType: "UDF");

			var topMostWorkflow = BMSTestHelper.CreateWorkflow(jobHeader, "ccc", releaseGroupPK: group.PK.ToGuid());
			topMostWorkflow.FH_TaskPenetrationResetDateTimeUtc = now.AddDays(-2);
			topMostWorkflow.FH_FC_CurrentComponent = buffer.PK;
			BMSTestHelper.CreateTask(workflow, staff.GS_Code, taskType: "UDF");

			workflow.GetOrCreateLinkToParent(parentWorkflow);
			parentWorkflow.GetOrCreateLinkToParent(topMostWorkflow);

			factory.Save();

			AssertEquals("Should calculate special penetration by considering the topmost workflow's penetration reset date", 0.333m, Math.Round(workflow.CalculatePenetrationPercentage(task, context), 3));
		}

		[TestDate(2020, 8, 7)]
		public void TestCalculatePenetrationPercentageForTask_NoSpecialPenetration()
		{
			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var context = WorkingTimeContext.Create(buffer);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var link = buffer.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = group.PK;
			link.FO_ResetTaskPenetrationThrottleFactor = 2;

			var staff = group.Staff.AddNew();
			staff.GS_Code = "AAA";

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "aaa", releaseGroupPK: group.PK.ToGuid());
			workflow.FH_FC_CurrentComponent = buffer.PK;
			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-3);

			var task = BMSTestHelper.CreateTask(workflow, staff.GS_Code, taskType: "UDF");
			task.P9_IsResetBeingAppliedToThisTask = true;

			Factory.Save();

			AssertEquals("Should not calculate special penetration since the reset task penetration is disabled in the registry",
				0.75m, workflow.CalculatePenetrationPercentage(task, context));

			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			task.P9_IsResetBeingAppliedToThisTask = false;

			Factory.Save();

			AssertEquals("Should not calculate special penetration since task reset is not being applied",
				0.75m, workflow.CalculatePenetrationPercentage(task, context));

			task.P9_IsResetBeingAppliedToThisTask = true;

			Factory.Save();

			AssertEquals("Should not calculate special penetration since the penetration reset date is not specified on the task's workflow",
				0.75m, workflow.CalculatePenetrationPercentage(task, context));

			task.P9_FH_ProcessHeader = ZGuid.Empty;

			Factory.Save();

			AssertEquals("Should not calculate special penetration since the task is not linked to a workflow",
				0.75m, workflow.CalculatePenetrationPercentage(task, context));
		}

		[TestDate(2020, 8, 7)]
		public void TestCalculatePenetrationPercentageForTask_NoSpecialPenetration_WhenWorkflowReleaseGroupNotInSystem()
		{
			WorkflowDataRegistry.Instance.ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var context = WorkingTimeContext.Create(buffer);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var link = buffer.ReleaseGroupLinks.AddNew();
			link.FO_GG_ReleaseGroup = group.PK;
			link.FO_ResetTaskPenetrationThrottleFactor = 2;

			var anotherGroup = Factory.NewWithValidTestData<GlbGroup>();

			var staff = group.Staff.AddNew();
			staff.GS_Code = "AAA";

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "aaa", releaseGroupPK: anotherGroup.PK.ToGuid());
			workflow.FH_FC_CurrentComponent = buffer.PK;
			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-3);

			var task = BMSTestHelper.CreateTask(workflow, staff.GS_Code, taskType: "UDF");
			task.P9_IsResetBeingAppliedToThisTask = true;

			Factory.Save();

			AssertEquals("Should not calculate special penetration since the workflow's release group is not in the set of release group links",
				0.75m, workflow.CalculatePenetrationPercentage(task, context));
		}

		#endregion

		#region Cancel

		public void TestCancelProcessHeader()
		{
			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders[0];
			var task1 = BMSTestHelper.CreateTask(workflow);
			task1.P9_Notes = ZBlob.FromUTF8(ORtfTextUtil.TextToRtf("this is kinda cool"));

			var task2 = BMSTestHelper.CreateTask(workflow, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			task2.P9_Notes = ZBlob.FromUTF8(ORtfTextUtil.TextToRtf("i am closed. This isnt funny"));

			var task3 = BMSTestHelper.CreateTask(workflow);

			BMSTestHelper.MakeCompletionStatementTaskType("ORG", "COM");
			var task4 = BMSTestHelper.CreateTask(workflow);
			task4.P9_Type = ProcessTask.GetCompletionStatementTaskType("ORG");
			task4.P9_Notes = ZBlob.FromUTF8(ORtfTextUtil.TextToRtf("notes should not be overwritten or appended for completion statements"));

			var task5 = BMSTestHelper.CreateTask(workflow);
			task5.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task5.P9_Notes = ZBlob.FromUTF8(ORtfTextUtil.TextToRtf("i am cancelled so nothing should happen"));

			workflow.CancelAllTasksAndCompletionStatements("This has been cancelled.");

			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task1.P9_Status);
			AssertEquals(ORtfTextUtil.AppendRtfStrings(ORtfTextUtil.TextToRtf("this is kinda cool"), ORtfTextUtil.TextToRtf("\r\n\r\nThis has been cancelled.")), task1.P9_Notes.ToUTF8());

			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, task2.P9_Status);
			AssertEquals(ORtfTextUtil.TextToRtf("i am closed. This isnt funny"), task2.P9_Notes.ToUTF8());

			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task3.P9_Status);
			AssertEquals(ORtfTextUtil.TextToRtf("This has been cancelled."), task3.P9_Notes.ToUTF8());

			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task4.P9_Status);
			AssertEquals(ORtfTextUtil.TextToRtf("notes should not be overwritten or appended for completion statements"), task4.P9_Notes.ToUTF8());

			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task5.P9_Status);
			AssertEquals(ORtfTextUtil.TextToRtf("i am cancelled so nothing should happen"), task5.P9_Notes.ToUTF8());
		}

		public void TestCancelProcessHeader_EnsureTaskCancellationValidationNotTriggered()
		{
			var staff = BMSTestHelper.CreateStaff(Factory, "AAA");

			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders[0];
			var task1 = BMSTestHelper.CreateTask(workflow);
			task1.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task1.P9_Notes = ZBlob.FromUTF8(ORtfTextUtil.TextToRtf("this is kinda cool"));

			var categorisedTaskTypes = new CategorisedWorkflowTaskTypesCollection();
			var taskTypes = categorisedTaskTypes.AddNew();
			taskTypes.Code = "ORG";
			var taskType = taskTypes.TaskTypes.AddNew();
			taskType.Code = "UDF";
			taskType.CanCancelTask = false;

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, categorisedTaskTypes);
			Env.Security.WorkflowTasksCanCancelAllTasks.IsAllowed = false;

			Factory.Save();

			workflow.CancelAllTasksAndCompletionStatements("This has been cancelled.");

			AssertEquals(ProcessTaskStatusCodeList.Codes.Cancelled, task1.P9_Status);
			AssertEquals(ORtfTextUtil.AppendRtfStrings(ORtfTextUtil.TextToRtf("this is kinda cool"), ORtfTextUtil.TextToRtf("\r\n\r\nThis has been cancelled.")), task1.P9_Notes.ToUTF8());
			AssertEquals("AAA", task1.P9_GS_NKAssignedStaffMember);
			AssertNoErrors(task1.P9_StatusInfo);
		}

		#endregion

		#region Quality Iteration Reason

		public void TestQualityIterationReason()
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "DUM");

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			var task1 = BMSTestHelper.CreateTask(workflow2, taskType: "QCB");
			var task2 = BMSTestHelper.CreateTask(workflow2);

			var pivot1 = (IProcessTaskIterationLink)task1.IterationLinks.AddNew();
			pivot1.P9I_P9_IterationTask = task2.PK;
			pivot1.P9I_FH_IterationWorkflow = workflow1.PK;
			pivot1.P9I_IterationReason = "RS1";

			Factory.Save();

			AssertEquals(false, workflow1.IterationReasonInfo.ReadOnly);
			AssertEquals("RS1", workflow1.IterationReason);
			workflow1.IterationReason = "ABC";
			AssertEquals("ABC", workflow1.IterationReason);

			AssertEquals(true, workflow2.IterationReasonInfo.ReadOnly);
			AssertEquals("", workflow2.IterationReason);
			AssertExceptionThrown<NotSupportedException>(() => workflow2.IterationReason = "ABC");
		}

		#endregion

		#region Workflow Type

		public void TestWorkflowType()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Fëanor");

			AssertEquals("ORG", jobHeader.FH_WorkflowType);
			AssertEquals("ORG", workflow.FH_WorkflowType);

			AssertEquals(true, jobHeader.FH_WorkflowTypeInfo.ReadOnly);
			AssertEquals(true, workflow.FH_WorkflowTypeInfo.ReadOnly);
		}

		public void TestWorkflowType_ForTemplateWorkflow()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateJobHeader = template.GetJobHeader();
			var templateWorkflow = (ProcessHeader)template.ProcessHeaders.AddNew();
			var templateTask = BMSTestHelper.CreateTask(template, templateWorkflow, string.Empty);

			AssertEquals("P0", templateJobHeader.FH_WorkflowType);
			AssertEquals("P0", templateWorkflow.FH_WorkflowType);

			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			AssertEquals(1, job.WorkflowItems.Count);

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);
			var workflow = jobHeader.ProcessHeaders.Single();

			AssertEquals(templateJobHeader.PK, jobHeader.FH_ParentTemplateId);
			AssertEquals(templateWorkflow.PK, workflow.FH_ParentTemplateId);
			AssertEquals("ORG", jobHeader.FH_WorkflowType);
			AssertEquals("ORG", workflow.FH_WorkflowType);
		}

		public void TestWorkflowType_ForWorkflowsAppliedFromTemplates()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var templateJobHeader = template.GetJobHeader();
			var templateWorkflow = (ProcessHeader)template.ProcessHeaders.AddNew();
			var templateTask = BMSTestHelper.CreateTask(template, templateWorkflow, string.Empty);

			AssertEquals("P0", templateJobHeader.FH_WorkflowType);
			AssertEquals("P0", templateWorkflow.FH_WorkflowType);

			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgHeader>();

			// Simulates what happens when you go to the Workflow Management tab before saving the form.
			// This could equally happen when trying to create a new task in some other way, or when accessing scheduling details from the job-level workflow like in the Projects module.
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			AssertEquals(0, job.WorkflowItems.Count);
			AssertEquals(0, jobHeader.ProcessHeaders.Count);

			Factory.Save();

			AssertEquals(1, job.WorkflowItems.Count);

			var workflow = jobHeader.ProcessHeaders.Single();

			AssertEquals(templateJobHeader.PK, jobHeader.FH_ParentTemplateId);
			AssertEquals(templateWorkflow.PK, workflow.FH_ParentTemplateId);
			AssertEquals("ORG", jobHeader.FH_WorkflowType);
			AssertEquals("ORG", workflow.FH_WorkflowType);
		}

		#endregion

		#region Workflow Descriptor

		public void TestWorkflowDescriptor_Template()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, processType: WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode);
			var workflow = BMSTestHelper.CreateWorkflow(template);

			AssertEquals("should return correct workflow-descriptor", typeof(OrgHeaderWorkflowDescriptor), workflow.WorkflowDescriptor.GetType());
		}

		public void TestWorkflowDescriptor_Job()
		{
			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Workflow 1");
			AssertEquals("should return correct workflow-descriptor", typeof(OrgHeaderWorkflowDescriptor), workflow.WorkflowDescriptor.GetType());
		}

		#endregion

		#region Parent

		public void TestTemplateWorkflow_ShouldHaveNullParent()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var workflow = template.ProcessHeaders.AddNew();

			Factory.Save();

			AssertNull("The Parent property for a template workflow must be null, because there is logic everywhere that relies on this in order to tell if it's a workflow template or not. And yet...", workflow.Parent);
		}

		#endregion

		#region Release Gate Related Properties

		#region Dedicated Buffer

		public void TestDedicatedBuffer_ShouldBeSetToNull_WhenDeactivatingWorkflow()
		{
			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var currentComponent = BMSTestHelper.CreateBucket(system, "Current component");
			var dedicatedBuffer = BMSTestHelper.CreateBuffer(system, "Dedicated Buffer");
			BMSTestHelper.LinkComponents(currentComponent, dedicatedBuffer, isReleaseGate: true);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = currentComponent.PK;
			workflow.FH_FC_DedicatedBuffer = dedicatedBuffer.PK;

			Factory.Save();

			workflow.FH_IsActive = false;

			AssertEquals("Should not change dedicated buffer before save", dedicatedBuffer.PK, workflow.FH_FC_DedicatedBuffer);

			Factory.Save();
			workflow.Reload();

			AssertEquals("Should save empty dedicated buffer in the database", ZGuid.Empty, workflow.FH_FC_DedicatedBuffer);
		}

		public void TestSettingDedicatedBuffer_ShouldUpdateEffectiveNudge_IfDedicatedBufferPreviouslyWasReset()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer1 = BMSTestHelper.CreateBuffer(system, "Buffer 1");
			var buffer2 = BMSTestHelper.CreateBuffer(system, "Buffer 2");

			var workflow = BMSTestHelper.CreateWorkflowAndTask(Factory, "Workflow");
			workflow.FH_VoteUpDownAmount = 100;
			workflow.FH_EffectiveNudge = 5; // some previous random value

			Factory.Save();

			workflow.Reload();
			AssertEquals("Precondition", 5m, workflow.FH_EffectiveNudge);
			Assert("Precondition", workflow.FH_FC_DedicatedBuffer.IsEmpty);

			workflow.FH_FC_DedicatedBuffer = buffer1.PK;

			Factory.Save();

			workflow.Reload();
			AssertEquals("Should recalculate effective nudge on setting dedicated buffer when dedicated buffer was previously reset", 100m, workflow.FH_EffectiveNudge);

			workflow.FH_VoteUpDownAmount = 200;

			Factory.Save();

			workflow.FH_FC_DedicatedBuffer = buffer2.PK;

			Factory.Save();

			workflow.Reload();
			AssertEquals("Should not recalculate effective nudge on setting dedicated buffer when dedicated buffer was not previously reset", 100m, workflow.FH_EffectiveNudge);
		}

		public void TestResettingDedicatedBuffer_ShouldResetEffectiveNudge()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			var workflow = BMSTestHelper.CreateWorkflowAndTask(Factory, "Workflow");
			workflow.FH_VoteUpDownAmount = 100;
			workflow.FH_FC_DedicatedBuffer = buffer.PK;

			Factory.Save();

			workflow.Reload();
			AssertEquals("Precondition: should recalculate when setting dedicated buffer", 100m, workflow.FH_EffectiveNudge);

			workflow.FH_FC_DedicatedBuffer = ZGuid.Empty;

			Factory.Save();

			workflow.Reload();
			AssertEquals("Should reset effective nudge on resetting dedicated buffer", 0m, workflow.FH_EffectiveNudge);
		}

		#endregion

		#region Effective Branch And Department

		public void TestEffectiveBranch()
		{
			var workflowBranch = Factory.NewWithValidTestData<GlbBranch>();
			var jobHeaderBranch = Factory.NewWithValidTestData<GlbBranch>();
			var bucketAgingBranch = Factory.NewWithValidTestData<GlbBranch>();
			var bufferAgingBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			var bufferAgingBranch2 = Factory.NewWithValidTestData<GlbBranch>();

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer1 = BMSTestHelper.CreateBuffer(system);
			var buffer2 = BMSTestHelper.CreateBuffer(system);
			bucket.FC_GB_AgingBranch = bucketAgingBranch.PK;
			buffer1.FC_GB_AgingBranch = bufferAgingBranch1.PK;
			buffer2.FC_GB_AgingBranch = bufferAgingBranch2.PK;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_GB_Branch = workflowBranch.PK;
			workflow.FH_FC_CurrentComponent = bucket.PK; // not released
			AssertEquals("Precondition", jobHeader.FH_GB_Branch, ZGuid.Empty);
			AssertEquals("Precondition", workflowBranch.PK, workflow.FH_GB_EffectiveBranch);

			workflow.FH_GB_Branch = ZGuid.Empty;
			AssertEquals("Should recalculate on workflow's branch change - should be empty (should ignore current bucket's aging branch)", ZGuid.Empty, workflow.FH_GB_EffectiveBranch);
			AssertNull(workflow.EffectiveBranch);

			workflow.FH_FC_DedicatedBuffer = buffer1.PK;
			AssertEquals("Should recalculate on dedicated buffer change - should equal to the dedicated buffer's aging branch", bufferAgingBranch1.PK, workflow.FH_GB_EffectiveBranch);
			AssertEquals(bufferAgingBranch1, workflow.EffectiveBranch);

			workflow.FH_FC_CurrentComponent = ZGuid.Empty;
			AssertEquals("Should recalculate on current component change - should equal to the dedicated buffer's aging branch when current component is empty", bufferAgingBranch1.PK, workflow.FH_GB_EffectiveBranch);
			AssertEquals(bufferAgingBranch1, workflow.EffectiveBranch);

			workflow.FH_FC_CurrentComponent = buffer2.PK;
			AssertEquals("Should recalculate on current component change - should equal to the current buffer's aging branch (current component should have a priority over dedicated buffer)", bufferAgingBranch2.PK, workflow.FH_GB_EffectiveBranch);
			AssertEquals(bufferAgingBranch2, workflow.EffectiveBranch);

			jobHeader.FH_GB_Branch = jobHeaderBranch.PK;
			AssertEquals("Should recalculate on JLW's branch change - should equal to the JLW's branch (JLW should have a priority over current component and dedicated buffer)", jobHeaderBranch.PK, workflow.FH_GB_EffectiveBranch);
			AssertEquals(jobHeaderBranch, workflow.EffectiveBranch);
			AssertEquals("Effective branch on JLW should be always empty", ZGuid.Empty, jobHeader.FH_GB_EffectiveBranch);

			workflow.FH_GB_Branch = workflowBranch.PK;
			AssertEquals(workflowBranch, workflow.EffectiveBranch);
			AssertEquals("Should recalculate on workflow's branch change - should equal to the workflow's branch (data set on workflow should have a priority over everything else)", workflowBranch.PK, workflow.FH_GB_EffectiveBranch);
		}

		public void TestEffectiveDepartment()
		{
			var workflowDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			var jobHeaderDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			var bucketAgingDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			var bufferAgingDepartment1 = Factory.NewWithValidTestData<GlbDepartment>();
			var bufferAgingDepartment2 = Factory.NewWithValidTestData<GlbDepartment>();

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer1 = BMSTestHelper.CreateBuffer(system);
			var buffer2 = BMSTestHelper.CreateBuffer(system);
			bucket.FC_GE_AgingDepartment = bucketAgingDepartment.PK;
			buffer1.FC_GE_AgingDepartment = bufferAgingDepartment1.PK;
			buffer2.FC_GE_AgingDepartment = bufferAgingDepartment2.PK;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_GE_Department = workflowDepartment.PK;
			workflow.FH_FC_CurrentComponent = bucket.PK; // not released
			AssertEquals("Precondition", jobHeader.FH_GE_Department, ZGuid.Empty);
			AssertEquals("Precondition", workflowDepartment.PK, workflow.FH_GE_EffectiveDepartment);

			workflow.FH_GE_Department = ZGuid.Empty;
			AssertEquals("Should recalculate on workflow's department change - should be empty (should ignore current bucket's aging department)", ZGuid.Empty, workflow.FH_GE_EffectiveDepartment);
			AssertNull(workflow.EffectiveDepartment);

			workflow.FH_FC_DedicatedBuffer = buffer1.PK;
			AssertEquals("Should recalculate on dedicated buffer change - should equal to the dedicated buffer's aging department", bufferAgingDepartment1.PK, workflow.FH_GE_EffectiveDepartment);
			AssertEquals(bufferAgingDepartment1, workflow.EffectiveDepartment);

			workflow.FH_FC_CurrentComponent = ZGuid.Empty;
			AssertEquals("Should recalculate on current component change - should equal to the dedicated buffer's aging department when current component is empty", bufferAgingDepartment1.PK, workflow.FH_GE_EffectiveDepartment);
			AssertEquals(bufferAgingDepartment1, workflow.EffectiveDepartment);

			workflow.FH_FC_CurrentComponent = buffer2.PK;
			AssertEquals("Should recalculate on current component change - should equal to the current buffer's aging department (current component should have a priority over dedicated buffer)", bufferAgingDepartment2.PK, workflow.FH_GE_EffectiveDepartment);
			AssertEquals(bufferAgingDepartment2, workflow.EffectiveDepartment);

			jobHeader.FH_GE_Department = jobHeaderDepartment.PK;
			AssertEquals("Should recalculate on JLW's department change - should equal to the JLW's department (JLW should have a priority over current component and dedicated buffer)", jobHeaderDepartment.PK, workflow.FH_GE_EffectiveDepartment);
			AssertEquals(jobHeaderDepartment, workflow.EffectiveDepartment);
			AssertEquals("Effective department on JLW should be always empty", ZGuid.Empty, jobHeader.FH_GE_EffectiveDepartment);

			workflow.FH_GE_Department = workflowDepartment.PK;
			AssertEquals(workflowDepartment, workflow.EffectiveDepartment);
			AssertEquals("Should recalculate on workflow's department change - should equal to the workflow's department (data set on workflow should have a priority over everything else)", workflowDepartment.PK, workflow.FH_GE_EffectiveDepartment);
		}

		public void TestShouldUpdateBranchDepartment_IgnoringUberCache_WhenCurrentComponentChanges()
		{
			var branch0 = Factory.NewWithValidTestData<GlbBranch>();
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var department0 = Factory.NewWithValidTestData<GlbDepartment>();
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer1 = BMSTestHelper.CreateBuffer(system, "Buffer 1");
			var buffer2 = BMSTestHelper.CreateBuffer(system, "Buffer 2");
			buffer1.FC_GB_AgingBranch = branch0.PK;
			buffer1.FC_GE_AgingDepartment = department0.PK;
			buffer2.FC_GB_AgingBranch = branch0.PK;
			buffer2.FC_GE_AgingDepartment = department0.PK;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = buffer1.PK;

			var task = BMSTestHelper.CreateTask(workflow);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedBuffer1 = newFactory.Load<BMComponent>(buffer1.PK);
			var loadedBuffer2 = newFactory.Load<BMComponent>(buffer2.PK);

			UpdateAgingBranchDepartmentInDB(buffer1, branch1, department1);
			UpdateAgingBranchDepartmentInDB(buffer2, branch2, department2);

			CombineAssertions("Precondition: should keep outdated data in memory", () =>
			{
				AssertEquals(branch0.PK, loadedBuffer1.FC_GB_AgingBranch);
				AssertEquals(department0.PK, loadedBuffer1.FC_GE_AgingDepartment);
				AssertEquals(branch0.PK, loadedBuffer2.FC_GB_AgingBranch);
				AssertEquals(department0.PK, loadedBuffer2.FC_GE_AgingDepartment);
			});

			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);
			AssertEquals(branch0.PK, loadedWorkflow.FH_GB_EffectiveBranch);
			AssertEquals(department0.PK, loadedWorkflow.FH_GE_EffectiveDepartment);

			loadedWorkflow.FH_CompletionStatement = "New name";

			newFactory.Save();

			AssertEquals("Should not update effective branch on save yet as neither current component nor dedicated buffer has changed", branch0.PK, loadedWorkflow.FH_GB_EffectiveBranch);
			AssertEquals("Should not update effective department on save yet as neither current component nor dedicated buffer has changed", department0.PK, loadedWorkflow.FH_GE_EffectiveDepartment);

			loadedWorkflow.FH_FC_CurrentComponent = buffer2.PK;

			newFactory.Save();

			AssertEquals("Should update effective branch on save ignoring the Uber Factory cache as current component has changed", branch2.PK, loadedWorkflow.FH_GB_EffectiveBranch);
			AssertEquals("Should update effective department on save ignoring the Uber Factory cache as current component has changed", department2.PK, loadedWorkflow.FH_GE_EffectiveDepartment);
		}

		public void TestShouldUpdateBranchDepartment_IgnoringUberCache_WhenDedicatedBufferChanges()
		{
			var branch0 = Factory.NewWithValidTestData<GlbBranch>();
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var department0 = Factory.NewWithValidTestData<GlbDepartment>();
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer1 = BMSTestHelper.CreateBuffer(system, "Buffer 1");
			var buffer2 = BMSTestHelper.CreateBuffer(system, "Buffer 2");
			buffer1.FC_GB_AgingBranch = branch0.PK;
			buffer1.FC_GE_AgingDepartment = department0.PK;
			buffer2.FC_GB_AgingBranch = branch0.PK;
			buffer2.FC_GE_AgingDepartment = department0.PK;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = bucket.PK;
			workflow.FH_FC_DedicatedBuffer = buffer1.PK;

			var task = BMSTestHelper.CreateTask(workflow);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedBuffer1 = newFactory.Load<BMComponent>(buffer1.PK);
			var loadedBuffer2 = newFactory.Load<BMComponent>(buffer2.PK);

			UpdateAgingBranchDepartmentInDB(buffer1, branch1, department1);
			UpdateAgingBranchDepartmentInDB(buffer2, branch2, department2);

			CombineAssertions("Precondition: should keep outdated data in memory", () =>
			{
				AssertEquals(branch0.PK, loadedBuffer1.FC_GB_AgingBranch);
				AssertEquals(department0.PK, loadedBuffer1.FC_GE_AgingDepartment);
				AssertEquals(branch0.PK, loadedBuffer2.FC_GB_AgingBranch);
				AssertEquals(department0.PK, loadedBuffer2.FC_GE_AgingDepartment);
			});

			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);
			AssertEquals(branch0.PK, loadedWorkflow.FH_GB_EffectiveBranch);
			AssertEquals(department0.PK, loadedWorkflow.FH_GE_EffectiveDepartment);

			loadedWorkflow.FH_CompletionStatement = "New name";

			newFactory.Save();

			AssertEquals("Should not update effective branch on save yet as neither current component nor dedicated buffer has changed", branch0.PK, loadedWorkflow.FH_GB_EffectiveBranch);
			AssertEquals("Should not update effective department on save yet as neither current component nor dedicated buffer has changed", department0.PK, loadedWorkflow.FH_GE_EffectiveDepartment);

			loadedWorkflow.FH_FC_DedicatedBuffer = buffer2.PK;

			newFactory.Save();

			AssertEquals("Should update effective branch on save ignoring the Uber Factory cache as dedicated buffer has changed", branch2.PK, loadedWorkflow.FH_GB_EffectiveBranch);
			AssertEquals("Should update effective department on save ignoring the Uber Factory cache as dedicated buffer has changed", department2.PK, loadedWorkflow.FH_GE_EffectiveDepartment);
		}

		void UpdateAgingBranchDepartmentInDB(BMComponent component, GlbBranch branch, GlbDepartment department)
		{
			using (var command = Db.Connection.Command($@"
UPDATE dbo.BMComponent
SET FC_GB_AgingBranch = '{branch.PK}',
	FC_GE_AgingDepartment = '{department.PK}',
	FC_SystemLastEditTimeUtc = getdate(),
	FC_SystemLastEditUser = '~BP'
WHERE FC_PK = '{component.PK}'"))
			{
				command.ExecuteNonQuery();
			}
		}

		public void TestEffectiveBranchAndDepartment_ResetWhenClosedOrDeactivated()
		{
			var workflowBranch = Factory.NewWithValidTestData<GlbBranch>();
			var workflowDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			var bufferAgingBranch = Factory.NewWithValidTestData<GlbBranch>();
			var bufferAgingDepartment = Factory.NewWithValidTestData<GlbDepartment>();

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			buffer.FC_GB_AgingBranch = bufferAgingBranch.PK;
			buffer.FC_GE_AgingDepartment = bufferAgingDepartment.PK;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_GB_Branch = workflowBranch.PK;
			workflow.FH_GE_Department = workflowDepartment.PK;
			workflow.FH_FC_CurrentComponent = buffer.PK;
			var task1 = BMSTestHelper.CreateTask(workflow);
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			CombineAssertions("Precondition - effective values calculated", () =>
			{
				AssertEquals("Should use workflow branch", workflowBranch.PK, workflow.FH_GB_EffectiveBranch);
				AssertEquals("Should use workflow department", workflowDepartment.PK, workflow.FH_GE_EffectiveDepartment);
				Assert("Workflow should be active", workflow.FH_IsActive);
				Assert("Workflow should be open", workflow.IsOpen);
			});

			CombineAssertions("Reset when workflow is deactivated", () =>
			{
				workflow.FH_IsActive = false;
				workflow.UpdateEffectiveBranchAndDepartment();

				AssertEquals("Effective branch should be reset when deactivated", ZGuid.Empty, workflow.FH_GB_EffectiveBranch);
				AssertEquals("Effective department should be reset when deactivated", ZGuid.Empty, workflow.FH_GE_EffectiveDepartment);
				AssertNull("EffectiveBranch property should be null", workflow.EffectiveBranch);
				AssertNull("EffectiveDepartment property should be null", workflow.EffectiveDepartment);
			});

			CombineAssertions("Recalculate when workflow is reactivated", () =>
			{
				workflow.FH_IsActive = true;
				workflow.UpdateEffectiveBranchAndDepartment();

				AssertEquals("Should recalculate effective branch when reactivated", workflowBranch.PK, workflow.FH_GB_EffectiveBranch);
				AssertEquals("Should recalculate effective department when reactivated", workflowDepartment.PK, workflow.FH_GE_EffectiveDepartment);
			});

			CombineAssertions("Reset when workflow is closed", () =>
			{
				workflow.TaskCollection[0].P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				workflow.FH_Status = WorkflowStatusList.Codes.Closed;
				workflow.UpdateEffectiveBranchAndDepartment();

				AssertEquals("Effective branch should be reset when closed", ZGuid.Empty, workflow.FH_GB_EffectiveBranch);
				AssertEquals("Effective department should be reset when closed", ZGuid.Empty, workflow.FH_GE_EffectiveDepartment);
			});

			CombineAssertions("Recalculate when workflow is reopened", () =>
			{
				workflow.TaskCollection[0].P9_Status = ProcessTaskStatusCodeList.Codes.Open;
				workflow.FH_Status = WorkflowStatusList.Codes.Open;
				workflow.UpdateEffectiveBranchAndDepartment();

				AssertEquals("Should recalculate effective branch when reopened", workflowBranch.PK, workflow.FH_GB_EffectiveBranch);
				AssertEquals("Should recalculate effective department when reopened", workflowDepartment.PK, workflow.FH_GE_EffectiveDepartment);
			});

			CombineAssertions("Reset when both closed and deactivated", () =>
			{
				workflow.FH_IsActive = false;
				workflow.TaskCollection[0].P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				workflow.FH_Status = WorkflowStatusList.Codes.Closed;
				workflow.UpdateEffectiveBranchAndDepartment();

				AssertEquals("Should remain reset when both closed and deactivated", ZGuid.Empty, workflow.FH_GB_EffectiveBranch);
				AssertEquals("Should remain reset when both closed and deactivated", ZGuid.Empty, workflow.FH_GE_EffectiveDepartment);
			});

			CombineAssertions("Test automatic update on save when status changes", () =>
			{
				workflow.FH_IsActive = true;
				workflow.TaskCollection[0].P9_Status = ProcessTaskStatusCodeList.Codes.Open;
				workflow.FH_Status = WorkflowStatusList.Codes.Open;
				Factory.Save(); 

				AssertEquals("Should auto-update on save", workflowBranch.PK, workflow.FH_GB_EffectiveBranch);
				AssertEquals("Should auto-update on save", workflowDepartment.PK, workflow.FH_GE_EffectiveDepartment);

				workflow.TaskCollection[0].P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				workflow.FH_Status = WorkflowStatusList.Codes.Closed;
				Factory.Save();

				AssertEquals("Should auto-reset on save when closed", ZGuid.Empty, workflow.FH_GB_EffectiveBranch);
				AssertEquals("Should auto-reset on save when closed", ZGuid.Empty, workflow.FH_GE_EffectiveDepartment);
			});
		}

		#endregion

		#region Effective Agreed Delivery Date

		public void TestEffectiveAgreedDeliveryDate()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			var workflow = BMSTestHelper.CreateWorkflowAndTask(Factory, "Workflow");
			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			Assert("Precondition", workflow.CanCalculateSparsePropertiesForNewReleaseGate);

			CombineAssertions("FH_ReleaseSequenceSortDateUtc is calculated correctly", () =>
			{
				AssertEquals(ZDateTime.Empty, workflow.FH_EffectiveAgreedDeliveryDateUtc);

				workflow.JobHeader.FH_AgreedDeliveryDate = new ZDateTime(2014, 7, 1);
				AssertEquals(new ZDateTime(2014, 7, 1), workflow.FH_EffectiveAgreedDeliveryDateUtc);

				workflow.FH_AgreedDeliveryDate = new ZDateTime(2014, 7, 2);
				AssertEquals(new ZDateTime(2014, 7, 2), workflow.FH_EffectiveAgreedDeliveryDateUtc);
			});
		}

		public void TestEffectiveAgreedDeliveryDate_NotResetOnDedicatedBufferChange_AndFrozenInBuffer()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system, "Bucket");
			var buffer1 = BMSTestHelper.CreateBuffer(system, "Buffer1");
			var buffer2 = BMSTestHelper.CreateBuffer(system, "Buffer2");

			var workflow = BMSTestHelper.CreateWorkflowAndTask(Factory, "Workflow");
			workflow.FH_FC_CurrentComponent = bucket.PK; 
			workflow.FH_AgreedDeliveryDate = new ZDateTime(2024, 7, 1);

			CombineAssertions("Effective ADD calculated without dedicated buffer", () =>
			{
				AssertEquals(new ZDateTime(2024, 7, 1), workflow.FH_EffectiveAgreedDeliveryDateUtc);
			});

			CombineAssertions("Effective ADD not reset when dedicated buffer changes", () =>
			{
				workflow.FH_FC_DedicatedBuffer = buffer1.PK;
				AssertEquals("Should not reset when dedicated buffer is set", new ZDateTime(2024, 7, 1), workflow.FH_EffectiveAgreedDeliveryDateUtc);

				workflow.FH_FC_DedicatedBuffer = buffer2.PK;
				AssertEquals("Should not reset when dedicated buffer changes", new ZDateTime(2024, 7, 1), workflow.FH_EffectiveAgreedDeliveryDateUtc);

				workflow.FH_AgreedDeliveryDate = new ZDateTime(2024, 7, 5);
				AssertEquals("Should still update when source date changes", new ZDateTime(2024, 7, 5), workflow.FH_EffectiveAgreedDeliveryDateUtc);
			});

			CombineAssertions("Effective ADD frozen when entering buffer", () =>
			{
				workflow.FH_FC_CurrentComponent = buffer1.PK;
				workflow.FH_ReleaseDateTime = ZDateTime.UtcNow;

				var frozenDate = workflow.FH_EffectiveAgreedDeliveryDateUtc;
				AssertEquals("Should maintain value when entering buffer", new ZDateTime(2024, 7, 5), frozenDate);

				workflow.FH_AgreedDeliveryDate = new ZDateTime(2024, 7, 10);
				workflow.JobHeader.FH_AgreedDeliveryDate = new ZDateTime(2024, 7, 15);

				AssertEquals("Should remain frozen and not recalculate when in buffer", frozenDate, workflow.FH_EffectiveAgreedDeliveryDateUtc);
			});

			CombineAssertions("Still reacts to workflow status changes", () =>
			{
				workflow.FH_IsActive = false;
				AssertEquals("Should reset when deactivated", ZDateTime.Empty, workflow.FH_EffectiveAgreedDeliveryDateUtc);

				workflow.FH_IsActive = true;
				AssertEquals("Should recalculate when reactivated", new ZDateTime(2024, 7, 10), workflow.FH_EffectiveAgreedDeliveryDateUtc);

				workflow.TaskCollection[0].P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				workflow.FH_Status = WorkflowStatusList.Codes.Closed;
				AssertEquals("Should reset when closed", ZDateTime.Empty, workflow.FH_EffectiveAgreedDeliveryDateUtc);
			});
		}

		#endregion

		#region Save Concurrency

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestDedicatedBuffer_ShouldCauseZSaveConcurrencyException_WhenExactPropertyHasChangedValueOnly()
		{
			BMSTestHelper.EnableBMSInRegistry();
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var component1 = BMSTestHelper.CreateComponent(Factory, BMComponentTypeList.Codes.Bucket, name: "Bucket 1", system: system);
			var component2 = BMSTestHelper.CreateComponent(Factory, BMComponentTypeList.Codes.Bucket, name: "Bucket 2", system: system);
			var component3 = BMSTestHelper.CreateComponent(Factory, BMComponentTypeList.Codes.Bucket, name: "Bucket 3", system: system);

			Factory.Save();

			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(factory1);
			var workflow = jobHeader.ProcessHeaders[0];

			factory1.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedWorkflow = factory2.Load<ProcessHeader>(workflow.PK);
			AssertNotNull(loadedWorkflow);
			loadedWorkflow.FH_FC_DedicatedBuffer = component1.PK;

			factory2.Save();

			workflow.FH_SystemLastEditTimeUtc = workflow.FH_SystemLastEditTimeUtc.AddSeconds(-10);
			AssertNoExceptionThrown("Should not throw because conflict is set to ignore", () => factory1.Save());

			loadedWorkflow.FH_FC_DedicatedBuffer = component2.PK;
			factory2.Save();

			workflow.FH_FC_DedicatedBuffer = component3.PK;
			var ex = AssertExceptionThrown<ZSaveConcurrencyException>(() => factory1.Save());
			AssertContains("FH_FC_DedicatedBuffer", ex.Message);
			AssertContains("FH_SystemLastEditTimeUtc", ex.Message);
			ZExceptionReporting.HandleSaveException(ex);
			AssertNoExceptionThrown(() => factory1.Save());
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestReleaseGateRelatedDates_ShouldCauseZSaveConcurrencyException_WhenExactPropertyHasChangedValueOnly()
		{
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(factory1);
			var workflow = jobHeader.ProcessHeaders[0];

			factory1.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedWorkflow = factory2.Load<ProcessHeader>(workflow.PK);
			AssertNotNull(loadedWorkflow);
			loadedWorkflow.FH_EffectiveAgreedDeliveryDateUtc = ZDateTime.Now;
			loadedWorkflow.FH_LatestAcceptableReleaseDateUtc = ZDateTime.Now;
			loadedWorkflow.FH_ReleaseSequenceSortDateUtc = ZDateTime.Now;

			factory2.Save();

			workflow.FH_SystemLastEditTimeUtc = workflow.FH_SystemLastEditTimeUtc.AddSeconds(-10);
			AssertNoExceptionThrown("Should not throw because conflict is set to ignore", () => factory1.Save());

			loadedWorkflow.FH_EffectiveAgreedDeliveryDateUtc = ZDateTime.Now.AddDays(1);
			factory2.Save();

			workflow.FH_EffectiveAgreedDeliveryDateUtc = ZDateTime.Now.AddDays(2);
			var ex = AssertExceptionThrown<ZSaveConcurrencyException>(() => factory1.Save());
			AssertContains("FH_EffectiveAgreedDeliveryDateUtc", ex.Message);
			AssertContains("FH_SystemLastEditTimeUtc", ex.Message);
			ZExceptionReporting.HandleSaveException(ex);
			AssertNoExceptionThrown(() => factory1.Save());

			loadedWorkflow.Reload();
			loadedWorkflow.FH_LatestAcceptableReleaseDateUtc = ZDateTime.Now.AddDays(1);
			factory2.Save();

			workflow.FH_LatestAcceptableReleaseDateUtc = ZDateTime.Now.AddDays(2);
			ex = AssertExceptionThrown<ZSaveConcurrencyException>(() => factory1.Save());
			AssertContains("FH_LatestAcceptableReleaseDateUtc", ex.Message);
			AssertContains("FH_SystemLastEditTimeUtc", ex.Message);
			ZExceptionReporting.HandleSaveException(ex);
			AssertNoExceptionThrown(() => factory1.Save());

			loadedWorkflow.Reload();
			loadedWorkflow.FH_ReleaseSequenceSortDateUtc = ZDateTime.Now.AddDays(1);
			factory2.Save();

			workflow.FH_ReleaseSequenceSortDateUtc = ZDateTime.Now.AddDays(2);
			ex = AssertExceptionThrown<ZSaveConcurrencyException>(() => factory1.Save());
			AssertContains("FH_ReleaseSequenceSortDateUtc", ex.Message);
			AssertContains("FH_SystemLastEditTimeUtc", ex.Message);
			ZExceptionReporting.HandleSaveException(ex);
			AssertNoExceptionThrown(() => factory1.Save());
		}

		[UseSnapshotProtection(skipTransaction: true)]
		public void TestEffectiveBranchAndDepartment_ShouldCauseZSaveConcurrencyException_WhenExactPropertyHasChangedValueOnly()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();

			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();
			var department3 = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(factory1);
			var workflow = jobHeader.ProcessHeaders[0];

			factory1.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedWorkflow = factory2.Load<ProcessHeader>(workflow.PK);
			AssertNotNull(loadedWorkflow);
			loadedWorkflow.FH_GB_EffectiveBranch = branch1.PK;
			loadedWorkflow.FH_GE_EffectiveDepartment = department1.PK;

			factory2.Save();

			workflow.FH_SystemLastEditTimeUtc = workflow.FH_SystemLastEditTimeUtc.AddSeconds(-10);
			AssertNoExceptionThrown("Should not throw because conflict is set to ignore", () => factory1.Save());

			loadedWorkflow.FH_GB_EffectiveBranch = branch2.PK;
			factory2.Save();

			workflow.FH_GB_EffectiveBranch = branch3.PK;
			var ex = AssertExceptionThrown<ZSaveConcurrencyException>(() => factory1.Save());
			AssertContains("FH_GB_EffectiveBranch", ex.Message);
			AssertContains("FH_SystemLastEditTimeUtc", ex.Message);
			ZExceptionReporting.HandleSaveException(ex);
			AssertNoExceptionThrown(() => factory1.Save());

			loadedWorkflow.Reload();
			loadedWorkflow.FH_GE_EffectiveDepartment = department2.PK;
			factory2.Save();

			workflow.FH_GE_EffectiveDepartment = department3.PK;
			ex = AssertExceptionThrown<ZSaveConcurrencyException>(() => factory1.Save());
			AssertContains("FH_GE_EffectiveDepartment", ex.Message);
			AssertContains("FH_SystemLastEditTimeUtc", ex.Message);
			ZExceptionReporting.HandleSaveException(ex);
			AssertNoExceptionThrown(() => factory1.Save());
		}

		#endregion

		#region Latest Acceptable Release Date

		[TestDate(2023, 5, 15)]
		public void TestLARD_CalculatesCorrectlyWithSimpleWorkflow()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var buffer = BMSTestHelper.CreateBuffer(system);
			buffer.FC_BufferTimespanInMinutes = 960;
			buffer.FC_GB_AgingBranch = Env.CurrentBranch.PK;
			buffer.FC_GE_AgingDepartment = Env.CurrentDepartment.PK;
			var bucket = BMSTestHelper.CreateBucket(system, "Bucket");
			BMSTestHelper.LinkComponents(bucket, buffer);

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", bucket);
			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			workflow.FH_GB_Branch = Env.CurrentBranch.PK;
			workflow.FH_GE_Department = Env.CurrentDepartment.PK;

			var nowUtc = ZDateTime.UtcNow;
			var deliveryDate = nowUtc.AddDays(10);
			workflow.FH_EffectiveAgreedDeliveryDateUtc = deliveryDate;
			workflow.FH_AgreedDeliveryDate = deliveryDate;

			var task1 = BMSTestHelper.CreateTask(workflow);
			task1.P9_EstimatedTimeToComplete = new ZInt(240).GetDateTimeFromMinutes();
			var task2 = BMSTestHelper.CreateTask(workflow);
			task2.P9_EstimatedTimeToComplete = new ZInt(480).GetDateTimeFromMinutes();
			Factory.Save();

			var lard = workflow.FH_LatestAcceptableReleaseDateUtc;

			AssertEquals(lard, new ZDateTime(2023, 5, 19, 13, 0, 0));
		}

		[TestDate(2023, 5, 15)]
		public void TestLARD_HandlesWorkflowWithNoTasksOrDeliveryDate()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var buffer = BMSTestHelper.CreateBuffer(system);
			buffer.FC_BufferTimespanInMinutes = 960;
			buffer.FC_GB_AgingBranch = Env.CurrentBranch.PK;
			buffer.FC_GE_AgingDepartment = Env.CurrentDepartment.PK;
			var bucket = BMSTestHelper.CreateBucket(system, "Bucket");

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", bucket);
			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			workflow.FH_GB_Branch = Env.CurrentBranch.PK;
			workflow.FH_GE_Department = Env.CurrentDepartment.PK;

			Factory.Save();

			AssertEquals(ZDateTime.Empty, workflow.FH_LatestAcceptableReleaseDateUtc);
		}

		[TestDate(2023, 5, 15)]
		public void TestLARD_UpdatesWhenTasksOrDeliveryDateChanged()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var buffer = BMSTestHelper.CreateBuffer(system);
			buffer.FC_BufferTimespanInMinutes = 960;
			buffer.FC_GB_AgingBranch = Env.CurrentBranch.PK;
			buffer.FC_GE_AgingDepartment = Env.CurrentDepartment.PK;
			var bucket = BMSTestHelper.CreateBucket(system, "Bucket");
			BMSTestHelper.LinkComponents(bucket, buffer);

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", bucket);
			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			workflow.FH_GB_Branch = Env.CurrentBranch.PK;
			workflow.FH_GE_Department = Env.CurrentDepartment.PK;
			workflow.FH_EffectiveAgreedDeliveryDateUtc = ZDateTime.UtcNow.AddDays(10);
			workflow.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddDays(10);
			var task1 = BMSTestHelper.CreateTask(workflow);
			task1.P9_EstimatedTimeToComplete = new ZInt(240).GetDateTimeFromMinutes();
			Factory.Save();

			var initialLARD = workflow.FH_LatestAcceptableReleaseDateUtc;
			Assert("Initial LARD should be calculated", !initialLARD.IsEmpty);

			workflow.FH_EffectiveAgreedDeliveryDateUtc = ZDateTime.UtcNow.AddDays(15);
			workflow.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddDays(15);
			Factory.Save();

			var updatedLARD1 = workflow.FH_LatestAcceptableReleaseDateUtc;
			Assert("LARD should be later after extending the delivery date", updatedLARD1 > initialLARD);

			var task2 = BMSTestHelper.CreateTask(workflow);
			task2.P9_EstimatedTimeToComplete = new ZInt(960).GetDateTimeFromMinutes(); // 16 hours
			Factory.Save();

			var updatedLARD2 = workflow.FH_LatestAcceptableReleaseDateUtc;
			Assert("LARD should be earlier after adding more work", updatedLARD2 < updatedLARD1);

			workflow.FH_FC_CurrentComponent = buffer.PK;
			Factory.Save();

			var bufferLARD = workflow.FH_LatestAcceptableReleaseDateUtc;
			AssertEquals("LARD should remain the same when moving to buffer", updatedLARD2, bufferLARD);

			workflow.FH_EffectiveAgreedDeliveryDateUtc = ZDateTime.UtcNow.AddDays(20);
			workflow.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddDays(20);
			Factory.Save();

			var finalLARD = workflow.FH_LatestAcceptableReleaseDateUtc;
			AssertEquals("LARD should not change after delivery date change while in buffer", bufferLARD, finalLARD);
		}

		[TestDate(2023, 5, 15)]
		public void TestLARD_FreezesValueWhenEnteringBuffer()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var buffer = BMSTestHelper.CreateBuffer(system);
			buffer.FC_BufferTimespanInMinutes = 960;
			buffer.FC_GB_AgingBranch = Env.CurrentBranch.PK;
			buffer.FC_GE_AgingDepartment = Env.CurrentDepartment.PK;

			var bucket = BMSTestHelper.CreateBucket(system, "Bucket");

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", bucket);
			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			workflow.FH_GB_Branch = Env.CurrentBranch.PK;
			workflow.FH_GE_Department = Env.CurrentDepartment.PK;
			workflow.FH_EffectiveAgreedDeliveryDateUtc = ZDateTime.UtcNow.AddDays(10);
			workflow.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddDays(10);

			var task = BMSTestHelper.CreateTask(workflow);
			task.P9_EstimatedTimeToComplete = new ZInt(480).GetDateTimeFromMinutes(); // 8 hours

			Factory.Save();

			var initialLARD = workflow.FH_LatestAcceptableReleaseDateUtc;

			workflow.FH_FC_CurrentComponent = buffer.PK;
			Factory.Save();

			workflow.FH_EffectiveAgreedDeliveryDateUtc = ZDateTime.UtcNow.AddDays(15);
			workflow.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddDays(15);

			var task2 = BMSTestHelper.CreateTask(workflow);
			task2.P9_EstimatedTimeToComplete = new ZInt(960).GetDateTimeFromMinutes(); // 16 hours

			Factory.Save();

			AssertEquals("LARD should remain unchanged after entering buffer, even if delivery date or tasks change", initialLARD, workflow.FH_LatestAcceptableReleaseDateUtc);
		}

		[TestDate(2023, 5, 15)]
		public void TestLARD_UsesEffectiveBranchAndDepartmentInsteadOfBufferBranchAndDepartment()
		{
			// Setup
			var originalDepartment = Env.CurrentDepartment;
			var overrideDepartment = Factory.New<GlbDepartment>();
			overrideDepartment.GE_Code = "OVD";
			overrideDepartment.GE_Desc = "Override Department";
			Factory.Save();

			var overrideBranch = Factory.New<GlbBranch>();
			overrideBranch.GB_Code = "OVB";
			overrideBranch.GB_BranchName = "Override Branch";
			overrideBranch.GB_GC = Env.CurrentCompany.PK;
			Factory.Save();

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, originalDepartment.PK);

			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, overrideDepartment.PK, DayOfWeek.Monday, "");
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, overrideDepartment.PK, DayOfWeek.Tuesday, "");
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, overrideDepartment.PK, DayOfWeek.Wednesday, "");
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, overrideDepartment.PK, DayOfWeek.Thursday, "");
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, overrideDepartment.PK, DayOfWeek.Friday, "");

			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, overrideDepartment.PK, DayOfWeek.Monday, WorkingDaysTestHelper.GetWorkingHoursString(9, 22));
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, overrideDepartment.PK, DayOfWeek.Tuesday, WorkingDaysTestHelper.GetWorkingHoursString(9, 22));
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, overrideDepartment.PK, DayOfWeek.Wednesday, WorkingDaysTestHelper.GetWorkingHoursString(9, 22));
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, overrideDepartment.PK, DayOfWeek.Thursday, WorkingDaysTestHelper.GetWorkingHoursString(9, 22));
			WorkingDaysTestHelper.UpdateDepartmentDay(Factory, overrideDepartment.PK, DayOfWeek.Friday, WorkingDaysTestHelper.GetWorkingHoursString(9, 22));

			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var buffer = BMSTestHelper.CreateBuffer(system);
			buffer.FC_BufferTimespanInMinutes = 960;

			buffer.FC_GB_AgingBranch = Env.CurrentBranch.PK;
			buffer.FC_GE_AgingDepartment = originalDepartment.PK;

			var bucket = BMSTestHelper.CreateBucket(system, "Bucket");
			BMSTestHelper.LinkComponents(bucket, buffer);

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", bucket);
			workflow.FH_FC_DedicatedBuffer = buffer.PK;

			workflow.FH_GB_Branch = ZGuid.Empty;
			workflow.FH_GE_Department = ZGuid.Empty;

			AssertEquals(buffer.FC_GB_AgingBranch, workflow.FH_GB_EffectiveBranch);
			AssertEquals(buffer.FC_GE_AgingDepartment, workflow.FH_GE_EffectiveDepartment);

			var deliveryDate = new ZDateTime(2023, 6, 15, 17, 0, 0); // A month later
			workflow.FH_EffectiveAgreedDeliveryDateUtc = deliveryDate;
			workflow.FH_AgreedDeliveryDate = deliveryDate;

			var task1 = BMSTestHelper.CreateTask(workflow);
			task1.P9_EstimatedTimeToComplete = new ZInt(4800).GetDateTimeFromMinutes(); // 80 hours
			Factory.Save();

			workflow.FH_FC_CurrentComponent = buffer.PK;
			Factory.Save();
			var lardWithOriginal = workflow.FH_LatestAcceptableReleaseDateUtc;

			workflow.FH_FC_CurrentComponent = bucket.PK;
			workflow.FH_FC_DedicatedBuffer = ZGuid.Empty;

			workflow.FH_GB_Branch = overrideBranch.PK;
			workflow.FH_GE_Department = overrideDepartment.PK;
			Factory.Save();

			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			workflow.FH_FC_CurrentComponent = buffer.PK;
			Factory.Save();

			var lardWithOverride = workflow.FH_LatestAcceptableReleaseDateUtc;

			Assert("LARD should be different when using different branches/departments for aging",
				   lardWithOverride != lardWithOriginal);
		}

		[TestDate(2023, 5, 15)]
		public void TestLARD_UsesBMBufferTimespanInsteadOfBufferDuration()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var buffer = BMSTestHelper.CreateBuffer(system);
			buffer.FC_BufferTimespanInMinutes = 960; // 16 hours
			buffer.FC_GB_AgingBranch = Env.CurrentBranch.PK;
			buffer.FC_GE_AgingDepartment = Env.CurrentDepartment.PK;

			var bufferTimespan = Factory.New<BMBufferTimespan>();
			bufferTimespan.BMT_Name = "Custom Buffer Timespan";
			bufferTimespan.BMT_BufferTimespanInMinutes = 480; // 8 hours
			Factory.Save();

			var bucket = BMSTestHelper.CreateBucket(system, "Bucket");
			BMSTestHelper.LinkComponents(bucket, buffer);

			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			workflow1.FH_FC_CurrentComponent = bucket.PK;
			workflow1.FH_FC_DedicatedBuffer = buffer.PK;
			workflow1.FH_BMT_BufferTimespan = bufferTimespan.PK;
			workflow1.FH_EffectiveAgreedDeliveryDateUtc = ZDateTime.UtcNow.AddDays(10);
			workflow1.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddDays(10);
			workflow1.FH_GB_Branch = Env.CurrentBranch.PK;
			workflow1.FH_GE_Department = Env.CurrentDepartment.PK;

			var task1 = BMSTestHelper.CreateTask(workflow1);
			task1.P9_EstimatedTimeToComplete = new ZInt(120).GetDateTimeFromMinutes();
			Factory.Save();

			var lardWithCustomTimespan = workflow1.FH_LatestAcceptableReleaseDateUtc;
			Assert("LARD should be calculated with custom buffer timespan", !lardWithCustomTimespan.IsEmpty);

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			workflow2.FH_FC_CurrentComponent = bucket.PK;
			workflow2.FH_FC_DedicatedBuffer = buffer.PK;
			workflow2.FH_EffectiveAgreedDeliveryDateUtc = ZDateTime.UtcNow.AddDays(10);
			workflow2.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddDays(10);
			workflow2.FH_GB_Branch = Env.CurrentBranch.PK;
			workflow2.FH_GE_Department = Env.CurrentDepartment.PK;

			var task2 = BMSTestHelper.CreateTask(workflow2);
			task2.P9_EstimatedTimeToComplete = new ZInt(120).GetDateTimeFromMinutes();
			Factory.Save();

			var lardWithDefaultTimespan = workflow2.FH_LatestAcceptableReleaseDateUtc;
			Assert("LARD should be calculated with default buffer timespan", !lardWithDefaultTimespan.IsEmpty);

			Assert("LARD with custom timespan should be earlier than LARD with default timespan", lardWithCustomTimespan > lardWithDefaultTimespan);
		}
		#endregion
		#endregion

		#region Saving

		public void TestSavingMultipleJobWorkflows_ShouldNotSave_ErrorMessageDisplayedToUser()
		{
			var saveInitiator = new Mock<ISaveInitiator>();

			var job = Factory.NewWithValidTestData<OrgHeader>();

			var jobHeader1 = ProcessJobHeader.GetForParent(job, Factory);
			var header1 = jobHeader1.ProcessHeaders[0];
			header1.FH_CompletionStatement = "1";
			var header2 = jobHeader1.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = "2";

			var jobHeader2 = Factory.New<ProcessJobHeader>();
			jobHeader2.FH_ParentTableCode = jobHeader1.FH_ParentTableCode;
			jobHeader2.FH_WorkflowType = jobHeader1.FH_WorkflowType;
			jobHeader2.FH_ParentId = job.PK;
			jobHeader2.FH_CompletionStatement = "statement";

			var exception = AssertExceptionThrown<ZSaveException>(() => Factory.Save());
			ZExceptionReporting.HandleSaveException(exception, saveInitiator.Object);

			AssertEquals($"There are duplicate job-level workflows. This form will need to be closed and re-opened to correct this problem.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertContains("Duplicate job-level workflow created.", ErrorReporter.LastMessageReported);
			AssertContains(@"Description: statement
Parent Table Code: OH
Is in database: False", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		#region Unneeded Workflows from Templates

		public void TestApplyingUnneededWorkflowsFromTemplateAndSaving_ShouldNotBePersisted()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);
			var templateWorkflow1 = BMSTestHelper.CreateWorkflow(template, "Workflow 1 - has tag only");
			var templateWorkflow2 = BMSTestHelper.CreateWorkflow(template, "Workflow 2 - has outgoing dependency link");
			var templateWorkflow3 = BMSTestHelper.CreateWorkflow(template, "Workflow 3 - has incoming dependency link");
			var templateWorkflow4 = BMSTestHelper.CreateWorkflow(template, "Workflow 4 - has outgoing parent/child link");
			var templateWorkflow5 = BMSTestHelper.CreateWorkflow(template, "Workflow 5 - has incoming parent/child link");
			var templateWorkflow6 = BMSTestHelper.CreateWorkflow(template, "Workflow 6 - has nothin for no one");

			foreach (var templateWorkflow in new[] { templateWorkflow1, templateWorkflow2, templateWorkflow3, templateWorkflow4, templateWorkflow5, templateWorkflow6 })
			{
				var templateTask = BMSTestHelper.CreateTask(template, templateWorkflow, description: "Ants in my eyes Johnson");

				templateTask.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
				templateTask.TemplateConditions.TemplateCondition2Value = @"""<WKI_Summary>""==""Real fake doors!""";
			}

			templateWorkflow1.AddTag(config.PrincessCelestiaTag);
			BMSTestHelper.CreateDependencyLink(template, templateWorkflow2, templateWorkflow3);
			BMSTestHelper.CreateParentChildLink(template, templateWorkflow4, templateWorkflow5);

			Factory.Save();

			var job = BMSTestHelper.CreateJob<IWorkItem>(Factory);
			((IWorkItem)job).WKI_Summary = "I can't see a thing!";

			job.ApplyWorkflowTemplates();

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				"Workflow 2 - has outgoing dependency link",
				"Workflow 3 - has incoming dependency link",
				"Workflow 4 - has outgoing parent/child link",
				"Workflow 5 - has incoming parent/child link",
			}, jobHeader.ProcessHeaders.Select(w => w.FH_CompletionStatement));

			foreach (var workflow in jobHeader.ProcessHeaders)
			{
				AssertEquals(0, workflow.Tasks.Count());
			}

			Factory.Save();
			AssertEquals(0, job.WorkflowItems.Tasks.Count);

			((IWorkItem)job).WKI_Summary = "Real fake doors!";
			Factory.Save();

			AssertEquals("Now that the job matches the Template Condition of all the template tasks, they should all get applied to the job.", 6, job.WorkflowItems.Tasks.Count);
			AssertSequencesEqual(new[]
			{
				"Workflow 1 - has tag only",
				"Workflow 2 - has outgoing dependency link",
				"Workflow 3 - has incoming dependency link",
				"Workflow 4 - has outgoing parent/child link",
				"Workflow 5 - has incoming parent/child link",
				"Workflow 6 - has nothin for no one",
			}, job.WorkflowItems.Tasks.Cast<ProcessTask>().Select(t => (string)t.ProcessHeader?.FH_CompletionStatement));

			BMSTestCaseWithFactory.AssertTagApplied("Should have properly found and applied the tag from the template workflow", job.WorkflowItems.Tasks[0].GetProcessHeader(), config.PrincessCelestiaTag);

			foreach (var workflow in jobHeader.ProcessHeaders)
			{
				AssertEquals(1, workflow.Tasks.Count());
			}

			AssertContainsExactElementsInAnyOrder("There shouldn't be any extra workflows, like Job Workflow.", new[]
			{
				"Workflow 1 - has tag only",
				"Workflow 2 - has outgoing dependency link",
				"Workflow 3 - has incoming dependency link",
				"Workflow 4 - has outgoing parent/child link",
				"Workflow 5 - has incoming parent/child link",
				"Workflow 6 - has nothin for no one",
			}, jobHeader.ProcessHeaders.Select(w => w.FH_CompletionStatement));
		}

		#endregion

		#region Concurrency

		public void TestLastTransferType_ShouldIgnoreConcurrencyCheck()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			newFactory.RefreshEnabled = false;
			var reloadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);

			workflow.FH_LastTransferType = TransferTypeList.Codes.ManualTransfer;
			reloadedWorkflow.FH_LastTransferType = TransferTypeList.Codes.SchematicTransfer;

			AssertNoExceptionThrown("There should not be a concurrency exception for last transfer type... SAD!", () =>
			{
				newFactory.Save();
				Factory.Save();
			});

			AssertEquals(TransferTypeList.Codes.ManualTransfer, workflow.FH_LastTransferType);
			AssertEquals(TransferTypeList.Codes.SchematicTransfer, reloadedWorkflow.FH_LastTransferType);
		}

		[SuspendCriticalValidation]
		public void TestSavingWorkflowsConcurrency_ShouldNotUnBlock()
		{
			var factory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };

			var config = TestConfigsHelper.CreateSchematicTestConfig(factory1, "INQ");
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(factory1, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1", config.Buffer);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 2", config.Buffer);
			var group = factory1.NewWithValidTestData<GlbGroup>();

			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code);
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code);
			factory1.Save();

			var f2workflow1 = factory2.Load<ProcessHeader>(workflow1.PK);
			var f2workflow2 = factory2.Load<ProcessHeader>(workflow2.PK);
			f2workflow1.GetOrCreateDependencyLink(f2workflow2);
			factory2.Save();

			workflow2.FH_GG_ReleaseGroup = group.PK;

			factory1.Save();

			AssertEquals("Should be blocked in factory1", WorkflowStatusList.Codes.Blocked, workflow2.FH_Status);
			AssertEquals("Should be blocked in factory2", WorkflowStatusList.Codes.Blocked, f2workflow2.FH_Status);
		}

		public void TestLinksReloadedOn_FHStatus_ConcurrencyError()
		{
			var properties = new List<IPropertyRecord> {
				{  new DummyPropertyRecord("FH_Status") }
			};

			Factory.RefreshEnabled = false;

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow2");

			var task1 = BMSTestHelper.CreateTask(workflow1);
			var task2 = BMSTestHelper.CreateTask(workflow2);

			Factory.Save();

			var linkPK = Guid.NewGuid();

			var linkCount = Factory.GetDatabaseCount(typeof(ProcessHeaderLink));
			AssertEquals(0, linkCount);

			string sql = $@"INSERT INTO {ProcessHeaderLinkSchema.Constants.SqlSchemaName}.{ProcessHeaderLinkSchema.Constants.TableName} ({ProcessHeaderLink.Schema.PK}, {ProcessHeaderLink.Schema.FP_LinkType}, {ProcessHeaderLink.Schema.FP_FH_HeaderFrom}, {ProcessHeaderLink.Schema.FP_FH_HeaderTo}, {ProcessHeaderLink.Schema.FP_SystemCreateTimeUtc}, {ProcessHeaderLink.Schema.FP_SystemCreateUser}, {ProcessHeaderLink.Schema.FP_SystemLastEditTimeUtc}, {ProcessHeaderLink.Schema.FP_SystemLastEditUser})
							VALUES ('{linkPK}', '{ProcessHeaderLinkTypeList.Codes.Dependency}', '{workflow1.PK}', '{workflow2.PK}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			Db.Connection.ExecuteNonQuery(sql);

			linkCount = Factory.GetDatabaseCount(typeof(ProcessHeaderLink));
			AssertEquals(1, linkCount);

			AssertEquals("No links exist", false, workflow2.PrerequisiteLinks.Any(link => link.FP_FH_HeaderFrom == workflow1.PK));

			workflow2.OnConcurrencyException(properties);

			AssertEquals("Links should have been reloaded", true, workflow2.PrerequisiteLinks.Any(link => link.FP_FH_HeaderFrom == workflow1.PK));
		}

		[ExpectNoExceptions]
		public void TestLinksReloadedOn_FHStatus_ConcurrencyError_LinkNotInDatabase()
		{
			var properties = new List<IPropertyRecord> {
				{  new DummyPropertyRecord("FH_Status") }
			};

			Factory.RefreshEnabled = false;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			var task1 = BMSTestHelper.CreateTask(workflow1);
			var task2 = BMSTestHelper.CreateTask(workflow2);

			Factory.Save();

			workflow1.GetOrCreateDependencyLink(workflow2);

			var linkCount = Factory.GetDatabaseCount(typeof(ProcessHeaderLink));
			AssertEquals("Link should not be in database", 0, linkCount);

			workflow2.OnConcurrencyException(properties);

			AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestLinksNotReloadedOn_NonFHStatus_ConcurrencyError()
		{
			var properties = new List<IPropertyRecord> {
				{  new DummyPropertyRecord("FH_Not_Status") }
			};

			Factory.RefreshEnabled = false;

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow2");

			var task1 = BMSTestHelper.CreateTask(workflow1);
			var task2 = BMSTestHelper.CreateTask(workflow2);

			Factory.Save();

			var linkPK = Guid.NewGuid();

			var linkCount = Factory.GetDatabaseCount(typeof(ProcessHeaderLink));
			AssertEquals(0, linkCount);

			string sql = $@"INSERT INTO {ProcessHeaderLinkSchema.Constants.SqlSchemaName}.{ProcessHeaderLinkSchema.Constants.TableName} ({ProcessHeaderLink.Schema.PK}, {ProcessHeaderLink.Schema.FP_LinkType}, {ProcessHeaderLink.Schema.FP_FH_HeaderFrom}, {ProcessHeaderLink.Schema.FP_FH_HeaderTo}, {ProcessHeaderLink.Schema.FP_SystemCreateTimeUtc}, {ProcessHeaderLink.Schema.FP_SystemCreateUser}, {ProcessHeaderLink.Schema.FP_SystemLastEditTimeUtc}, {ProcessHeaderLink.Schema.FP_SystemLastEditUser})
							VALUES ('{linkPK}', '{ProcessHeaderLinkTypeList.Codes.Dependency}', '{workflow1.PK}', '{workflow2.PK}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			Db.Connection.ExecuteNonQuery(sql);

			linkCount = Factory.GetDatabaseCount(typeof(ProcessHeaderLink));
			AssertEquals(1, linkCount);

			AssertEquals("No links exist", false, workflow2.PrerequisiteLinks.Any(link => link.FP_FH_HeaderFrom == workflow1.PK));

			workflow2.OnConcurrencyException(properties);

			AssertEquals("Link was not reloaded since FH_Status was not changed", false, workflow2.PrerequisiteLinks.Any(link => link.FP_FH_HeaderFrom == workflow1.PK));
		}

		class DummyPropertyRecord : IPropertyRecord
		{
			public DummyPropertyRecord(string columnName)
			{
				ColumnName = columnName;
			}

			public ZPropertyInfo PropertyInfo => throw new NotImplementedException();
			public string ColumnName { get; }
			public string DisplayName => throw new NotImplementedException();
			public object OriginalValue => throw new NotImplementedException();
			public object CurrentValue => throw new NotImplementedException();
			public object DatabaseValue => throw new NotImplementedException();
			public string LastModified => throw new NotImplementedException();
			public bool HasChangedInDatabase => throw new NotImplementedException();
			public bool HasChangedInSession => throw new NotImplementedException();
			public bool IsConsistentWithDatabase => throw new NotImplementedException();
			public bool IsMergeAllowed => throw new NotImplementedException();
			public void Merge()
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		#region Status Update

		//The following tests use a sample test network as described in the eDocs of WI00181442
		#region Walking Descendants

		public void TestParentChildStatusChangeCascades_MultipleParentChildLevels()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var greatGrandparentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader1, "greatGrandparentWorkflow");
			var grandparentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader1, "grandparentWorkflow");
			var parentWorkflow = BMSTestHelper.CreateWorkflow(jobHeader1, "parentWorkflow");
			var childWorkflow = BMSTestHelper.CreateWorkflow(jobHeader1, "childWorkflow");

			var greatGrandparentTask = BMSTestHelper.CreateTask(greatGrandparentWorkflow);
			var grandparentTask = BMSTestHelper.CreateTask(grandparentWorkflow);
			var parentTask = BMSTestHelper.CreateTask(parentWorkflow);
			var childTask = BMSTestHelper.CreateTask(childWorkflow);

			greatGrandparentTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			grandparentTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			parentTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			childTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			BMSTestHelper.MakeChildOf(grandparentWorkflow, greatGrandparentWorkflow);
			BMSTestHelper.MakeChildOf(parentWorkflow, grandparentWorkflow);
			BMSTestHelper.MakeChildOf(childWorkflow, parentWorkflow);

			Factory.Save();

			AssertWorkflowIsClosed("Precondition: Greatgrandparent workflow should be closed", greatGrandparentWorkflow);
			AssertWorkflowIsClosed("Precondition: Grandparent workflow should be closed", grandparentWorkflow);
			AssertWorkflowIsClosed("Precondition: parent workflow should be closed", parentWorkflow);
			AssertWorkflowIsClosed("Precondition: child workflow should be closed", childWorkflow);

			var prereqWorkflow = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "prereqWorkflow");
			var prereqTask = BMSTestHelper.CreateTask(prereqWorkflow);

			prereqWorkflow.GetOrCreateDependencyLink(greatGrandparentWorkflow);
			prereqTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			Factory.Save();

			AssertWorkflowIsOpen("Postcondition: Prereq workflow should be open", prereqWorkflow);
			AssertWorkflowIsClosedWithOpenPrerequisites("Postcondition: Greatgrandparent workflow should be closed with open prerequisites", greatGrandparentWorkflow);
			AssertWorkflowIsClosedWithOpenPrerequisites("Postcondition: Grandparent workflow should be closed with open prerequisites", grandparentWorkflow);
			AssertWorkflowIsClosedWithOpenPrerequisites("Postcondition: parent workflow should be closed with open prerequisites", parentWorkflow);
			AssertWorkflowIsClosedWithOpenPrerequisites("Postcondition: child workflow should be closed with open prerequisites", childWorkflow);
		}

		public void TestPrereqParentStatusChangeShouldChangeChildStatus()
		{
			var prereqMainWorkflow = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "prereqMainWorkflow");
			var mainWorkflow = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "mainWorkflow");
			var child1OfMainWorkflow = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "child1OfMain");
			var child2OfMainWorkflow = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "child2OfMain");

			var prereqTask = BMSTestHelper.CreateTask(prereqMainWorkflow);
			var mainTask = BMSTestHelper.CreateTask(mainWorkflow);
			var child1Task = BMSTestHelper.CreateTask(child1OfMainWorkflow);
			var child2Task = BMSTestHelper.CreateTask(child2OfMainWorkflow);

			prereqTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			mainTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			child1Task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			child2Task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			BMSTestHelper.MakeChildOf(child1OfMainWorkflow, mainWorkflow);
			BMSTestHelper.MakeChildOf(child2OfMainWorkflow, mainWorkflow);

			Factory.Save();

			AssertWorkflowIsClosed("PrereqMainWorkflow should be Closed", prereqMainWorkflow);
			AssertWorkflowIsClosed("MainWorkflow should be Closed", mainWorkflow);
			AssertWorkflowIsClosed("Child1Workflow should be Closed", child1OfMainWorkflow);
			AssertWorkflowIsClosed("Child2Workflow should be Closed", child2OfMainWorkflow);

			prereqMainWorkflow.GetOrCreateDependencyLink(mainWorkflow);
			prereqTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			Factory.Save();

			AssertWorkflowIsOpen("PrereqMainWorkflow should be Open", prereqMainWorkflow);
			AssertWorkflowIsClosedWithOpenPrerequisites("MainWorkflow should now be closed with open prerequisites", mainWorkflow);
			AssertWorkflowIsClosedWithOpenPrerequisites("Child1Workflow should now be closed with open prerequisites", child1OfMainWorkflow);
			AssertWorkflowIsClosedWithOpenPrerequisites("Child2Workflow should now be closed with open prerequisites", child2OfMainWorkflow);
		}

		public void TestPrerequisiteStatus_CorrespondWithClosedWithOpenPrerequisiteFHStatus()
		{
			var workflow1 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow3");
			var workflow4 = BMSTestHelper.CreateWorkflow(BMSTestHelper.CreateJobHeader<OrgHeader>(Factory), "workflow4");

			var workflow1Task = BMSTestHelper.CreateTask(workflow1);
			var workflow2Task = BMSTestHelper.CreateTask(workflow2);
			var workflow3Task = BMSTestHelper.CreateTask(workflow3);
			var workflow4Task = BMSTestHelper.CreateTask(workflow4);

			Factory.Save();

			AssertWorkflowIsOpen("Precondition: workflow1 should be open", workflow1);
			AssertWorkflowIsOpen("Precondition: workflow2 should be open", workflow2);
			AssertWorkflowIsOpen("Precondition: workflow3 should be open", workflow3);
			AssertWorkflowIsOpen("Precondition: workflow4 should be open", workflow4);

			// test precondition prerequisite status
			AssertEquals("Precondition: workflow1 should have no prerequisite status", WorkflowPrerequisiteStatusList.Codes.ClearedToStart, workflow1.PrerequisiteStatus);
			AssertEquals("Precondition: workflow2 should have no prerequisite status", WorkflowPrerequisiteStatusList.Codes.ClearedToStart, workflow2.PrerequisiteStatus);
			AssertEquals("Precondition: workflow3 should have no prerequisite status", WorkflowPrerequisiteStatusList.Codes.ClearedToStart, workflow3.PrerequisiteStatus);
			AssertEquals("Precondition: workflow4 should have no prerequisite status", WorkflowPrerequisiteStatusList.Codes.ClearedToStart, workflow4.PrerequisiteStatus);

			// create dependency for COP workflow status
			workflow2Task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			workflow3Task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			workflow4Task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow3);
			workflow3.GetOrCreateDependencyLink(workflow4);

			Factory.Save();

			// test postcondition prerequisite status for COP
			AssertWorkflowIsOpen("Postcondition: workflow1 should be open", workflow1);
			AssertWorkflowIsClosedWithOpenPrerequisites("Postcondition: workflow2 should be closed with open prerequisites", workflow2);
			AssertWorkflowIsClosedWithOpenPrerequisites("Postcondition: workflow3 should be closed with open prerequisites", workflow3);
			AssertWorkflowIsClosedWithOpenPrerequisites("Postcondition: workflow4 should be closed with open prerequisites", workflow4);

			AssertEquals("Postcondition: workflow1 prereq status should have no prerequisite status", WorkflowPrerequisiteStatusList.Codes.ClearedToStart, workflow1.PrerequisiteStatus);
			AssertEquals("Postcondition: workflow2 prereq status should be closed with open prerequisites", WorkflowPrerequisiteStatusList.Codes.ClosedWithOpenPrerequisites, workflow2.PrerequisiteStatus);
			AssertEquals("Postcondition: workflow3 prereq status should be closed with open prerequisites", WorkflowPrerequisiteStatusList.Codes.ClosedWithOpenPrerequisites, workflow3.PrerequisiteStatus);
			AssertEquals("Postcondition: workflow4 prereq status should be closed with open prerequisites", WorkflowPrerequisiteStatusList.Codes.ClosedWithOpenPrerequisites, workflow4.PrerequisiteStatus);

			workflow2Task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;

			Factory.Save();
			AssertEquals("Postcondition: workflow1 prereq status should have no prerequisite status", WorkflowPrerequisiteStatusList.Codes.ClearedToStart, workflow1.PrerequisiteStatus);
			AssertEquals("Postcondition: workflow2 prereq status should be blocked", WorkflowPrerequisiteStatusList.Codes.Blocked, workflow2.PrerequisiteStatus);
			AssertEquals("Postcondition: workflow3 prereq status should be closed with open prerequisites", WorkflowPrerequisiteStatusList.Codes.ClosedWithOpenPrerequisites, workflow3.PrerequisiteStatus);
			AssertEquals("Postcondition: workflow4 prereq status should be closed with open prerequisites", WorkflowPrerequisiteStatusList.Codes.ClosedWithOpenPrerequisites, workflow4.PrerequisiteStatus);
		}

		#endregion

		#region Walking Dependants (Post-Requisits)

		public void TestPrereqJobStatusChangesFromOpenToClosed_ShouldVisitPostreqJob()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "jobHeader1", addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "jobHeader2", addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2");
			var workflow2Child = BMSTestHelper.CreateWorkflow(jobHeader2, "workflow2Child");

			workflow2Child.GetOrCreateLinkToParent(workflow2);
			jobHeader1.GetOrCreateDependencyLink(jobHeader2);

			var workflow1Task = BMSTestHelper.CreateTask(workflow1);
			var workflow2Task = BMSTestHelper.CreateTask(workflow2);
			var workflow2childTask = BMSTestHelper.CreateTask(workflow2Child);

			Factory.Save();

			AssertWorkflowIsOpen("workflow1 should initially be open", workflow1);
			AssertWorkflowIsBlocked("workflow2 should be blocked", workflow2);
			AssertWorkflowIsBlocked("workflow2Child should also be blocked", workflow2Child);
			AssertWorkflowIsOpen("jobHeader1 should be open", jobHeader1);
			AssertWorkflowIsBlocked("jobHeader2 should be blocked", jobHeader2);

			workflow1Task.P9_Status = WorkflowStatusList.Codes.Closed;
			Factory.Save();

			AssertWorkflowIsClosed("workflow1 should now be closed", workflow1);
			AssertWorkflowIsOpen("workflow2 should now be open", workflow2);
			AssertWorkflowIsOpen("workflow2Child should also now be open", workflow2Child);
			AssertWorkflowIsClosed("jobHeader1 should now be blocked", jobHeader1);
			AssertWorkflowIsOpen("jobHeader2 should now be open", jobHeader2);
		}

		public void TestPrereqStatusChangeWithCircularDependency_ShouldNotCreateInfiniteCycle()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader1, "workflow3");

			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow3);

			var workflow1Task = BMSTestHelper.CreateTask(workflow1);
			var workflow2Task = BMSTestHelper.CreateTask(workflow2);
			var workflow3Task = BMSTestHelper.CreateTask(workflow3);

			Factory.Save();

			AssertWorkflowIsOpen("Precondition: workflow1 should be open", workflow1);
			AssertWorkflowIsBlocked("Precondition: workflow2 should be blocked", workflow2);
			AssertWorkflowIsBlocked("Precondition: workflow3 should be blocked", workflow3);

			var loop = workflow3.GetOrCreateDependencyLink(workflow1);
			Factory.Save();

			AssertWorkflowIsBlocked("Precondition: workflow1 should be blocked", workflow1);
			AssertWorkflowIsBlocked("Precondition: workflow2 should be blocked", workflow2);
			AssertWorkflowIsBlocked("Precondition: workflow3 should be blocked", workflow3);

			workflow1Task.P9_Status = WorkflowStatusList.Codes.Closed;
			Factory.Save();

			AssertWorkflowIsClosedWithOpenPrerequisites("workflow1 should be closed with open prerequisites", workflow1);
			AssertWorkflowIsBlocked("workflow2 should be blocked", workflow2);
			AssertWorkflowIsBlocked("workflow3 should be blocked", workflow3);

			workflow2Task.P9_Status = WorkflowStatusList.Codes.Closed;
			Factory.Save();

			AssertWorkflowIsClosedWithOpenPrerequisites("workflow1 should be closed with open prerequisites", workflow1);
			AssertWorkflowIsClosedWithOpenPrerequisites("workflow2 should be closed with open prerequisites", workflow2);
			AssertWorkflowIsBlocked("workflow3 should still be blocked", workflow3);

			workflow3Task.P9_Status = WorkflowStatusList.Codes.Closed; //This may cause an infinite update cascade if not handled appropriately
			Factory.Save();

			AssertWorkflowIsClosedWithOpenPrerequisites("workflow1 should now be closed", workflow1);
			AssertWorkflowIsClosedWithOpenPrerequisites("workflow2 should now be closed", workflow2);
			AssertWorkflowIsClosedWithOpenPrerequisites("workflow3 should now be closed", workflow3);

			loop.Delete(); // Remove dependency between w1 and w3 and all workflows should be blocked
			Factory.Save();

			AssertWorkflowIsClosed("workflow1 should now be closed", workflow1);
			AssertWorkflowIsClosed("workflow2 should now be closed", workflow2);
			AssertWorkflowIsClosed("workflow3 should now be closed", workflow3);
		}

		public void TestWorkflowStatusUpdate_BetweenParentChildAndPrePostreqRelationships()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var parent1 = BMSTestHelper.CreateWorkflow(jobHeader1, "parent1");
			var parent2 = BMSTestHelper.CreateWorkflow(jobHeader1, "parent2");
			var child2 = BMSTestHelper.CreateWorkflow(jobHeader1, "child2");
			var postreqOfchild2 = BMSTestHelper.CreateWorkflow(jobHeader1, "postreqOfchild2");

			var parent1Task = BMSTestHelper.CreateTask(parent1);
			var parent2Task = BMSTestHelper.CreateTask(parent2);
			var child2Task = BMSTestHelper.CreateTask(child2);
			var postreqOfchild2Task = BMSTestHelper.CreateTask(postreqOfchild2);

			parent1Task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			parent2Task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			child2Task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			postreqOfchild2Task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			parent1.GetOrCreateDependencyLink(parent2);
			BMSTestHelper.MakeChildOf(child2, parent2);
			child2.GetOrCreateDependencyLink(postreqOfchild2);

			Factory.Save();

			AssertWorkflowIsClosed("Precondition: parent1 should be closed", parent1);
			AssertWorkflowIsClosed("Precondition: parent2 should be closed", parent2);
			AssertWorkflowIsClosed("Precondition: child2 should be closed", child2);
			AssertWorkflowIsClosed("Precondition: child3 should be closed", postreqOfchild2);

			parent1Task.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			Factory.Save();

			AssertWorkflowIsOpen("Postcondition: parent1 should be open", parent1);
			AssertWorkflowIsClosedWithOpenPrerequisites("Postcondition: parent2 should be closed with open prerequisites", parent2);
			AssertWorkflowIsClosedWithOpenPrerequisites("Postcondition: child2 should be closed with open prerequisites", child2); // child2 is child of parent2 while parent2.FH_Status should be COP
			AssertWorkflowIsClosedWithOpenPrerequisites("Postcondition: child3 should be closed with open prerequisites", postreqOfchild2);
		}

		#endregion

		#region Status Update In Multi-User Environment

		public void TestWorkflowStatusUpdates_WhenOneUserClosesOneTask_AndAnotherUserClosesTheOtherTask_InTheSameWorkflow()
		{
			BMSTestHelper.CreateSystem(Factory, "INQ");
			var jobHeader1 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2Postreq = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Workflow 1");
			var workflow2Postreq = BMSTestHelper.CreateWorkflow(jobHeader2Postreq, "Workflow 2 Postreq");
			workflow1.MakePrerequisiteOf(workflow2Postreq);

			var task1_1 = BMSTestHelper.CreateTask(workflow1, sequence: 1);
			var task1_2 = BMSTestHelper.CreateTask(workflow1, sequence: 2);
			var task2_1Postreq = BMSTestHelper.CreateTask(workflow2Postreq);

			Factory.Save();

			AssertEquals("Precondition: Factory 1: first workflow", WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals("Precondition: Factory 1: first JLW", WorkflowStatusList.Codes.Open, jobHeader1.FH_Status);
			AssertEquals(1, jobHeader1.FH_TaskLowestOpenSequenceNumber);
			AssertEquals("Precondition: Factory 1: postreq workflow", WorkflowStatusList.Codes.Blocked, workflow2Postreq.FH_Status);

			task1_2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed; // let's change the status before saving in another factory
			AssertEquals("Precondition: After making changes by the 1st user: Factory 1: first workflow", WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals("Precondition: After making changes by the 1st user: Factory 1: first JLW", WorkflowStatusList.Codes.Open, jobHeader1.FH_Status);
			AssertEquals(1, jobHeader1.FH_TaskLowestOpenSequenceNumber);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var workflow1InFactory2 = factory2.Load<ProcessHeader>(workflow1.PK);
			var jobHeader1InFactory2 = factory2.Load<ProcessHeader>(jobHeader1.PK);
			var workflow2PostreqInFactory2 = factory2.Load<ProcessHeader>(workflow2Postreq.PK);

			workflow1InFactory2.Tasks.Single(x => x.PK == task1_1.PK).P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(1, jobHeader1InFactory2.FH_TaskLowestOpenSequenceNumber);

			factory2.Save();

			AssertEquals("Precondition: After saving by the 2nd user: Factory 1: first workflow", WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 1: first JLW", WorkflowStatusList.Codes.Open, jobHeader1.FH_Status);
			AssertEquals(1, jobHeader1.FH_TaskLowestOpenSequenceNumber);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 1: postreq workflow", WorkflowStatusList.Codes.Blocked, workflow2Postreq.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 2: first workflow", WorkflowStatusList.Codes.Open, workflow1InFactory2.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 2: first JLW", WorkflowStatusList.Codes.Open, jobHeader1InFactory2.FH_Status);
			AssertEquals(2, jobHeader1InFactory2.FH_TaskLowestOpenSequenceNumber);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 2: postreq workflow", WorkflowStatusList.Codes.Blocked, workflow2PostreqInFactory2.FH_Status);

			Factory.Save();

			AssertEquals("After saving by the 1st user: Factory 1: first workflow", WorkflowStatusList.Codes.Closed, workflow1.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 1: first JLW", WorkflowStatusList.Codes.Closed, jobHeader1.FH_Status);
			AssertEquals(-1, jobHeader1.FH_TaskLowestOpenSequenceNumber);
			AssertEquals("After saving by the 1st user: Factory 1: postreq workflow", WorkflowStatusList.Codes.Open, workflow2Postreq.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 2: first workflow", WorkflowStatusList.Codes.Open, workflow1InFactory2.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 2: first JLW", WorkflowStatusList.Codes.Open, jobHeader1InFactory2.FH_Status);
			AssertEquals(2, jobHeader1InFactory2.FH_TaskLowestOpenSequenceNumber);
			AssertEquals("After saving by the 1st user: Factory 2: postreq workflow", WorkflowStatusList.Codes.Blocked, workflow2PostreqInFactory2.FH_Status);
		}

		public void TestWorkflowStatusUpdates_WhenOneUserClosesTask_AndAnotherUserAddsOpenTask_ToTheSameWorkflow()
		{
			BMSTestHelper.CreateSystem(Factory, "INQ");
			var jobHeader1 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2Postreq = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Workflow 1");
			var workflow2Postreq = BMSTestHelper.CreateWorkflow(jobHeader2Postreq, "Workflow 2 Postreq");
			workflow1.MakePrerequisiteOf(workflow2Postreq);

			var task1_1 = BMSTestHelper.CreateTask(workflow1, sequence: 1);
			var task2_1Postreq = BMSTestHelper.CreateTask(workflow2Postreq);

			Factory.Save();

			AssertEquals("Precondition: Factory 1: first workflow", WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals("Precondition: Factory 1: first JLW", WorkflowStatusList.Codes.Open, jobHeader1.FH_Status);
			AssertEquals(1, jobHeader1.FH_TaskLowestOpenSequenceNumber);
			AssertEquals("Precondition: Factory 1: postreq workflow", WorkflowStatusList.Codes.Blocked, workflow2Postreq.FH_Status);

			task1_1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed; // let's change the status before saving in another factory
			AssertEquals("Precondition: After making changes by the 1st user: Factory 1: first workflow", WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals("Precondition: After making changes by the 1st user: Factory 1: first JLW", WorkflowStatusList.Codes.Open, jobHeader1.FH_Status);
			AssertEquals(1, jobHeader1.FH_TaskLowestOpenSequenceNumber);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var workflow1InFactory2 = factory2.Load<ProcessHeader>(workflow1.PK);
			var jobHeader1InFactory2 = factory2.Load<ProcessHeader>(jobHeader1.PK);
			var workflow2PostreqInFactory2 = factory2.Load<ProcessHeader>(workflow2Postreq.PK);

			BMSTestHelper.CreateTask(workflow1InFactory2, sequence: 2);
			AssertEquals(1, jobHeader1InFactory2.FH_TaskLowestOpenSequenceNumber);

			factory2.Save();

			AssertEquals("Precondition: After saving by the 2nd user: Factory 1: first workflow", WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 1: first JLW", WorkflowStatusList.Codes.Open, jobHeader1.FH_Status);
			AssertEquals(1, jobHeader1.FH_TaskLowestOpenSequenceNumber);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 1: postreq workflow", WorkflowStatusList.Codes.Blocked, workflow2Postreq.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 2: first workflow", WorkflowStatusList.Codes.Open, workflow1InFactory2.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 2: first JLW", WorkflowStatusList.Codes.Open, jobHeader1InFactory2.FH_Status);
			AssertEquals(1, jobHeader1InFactory2.FH_TaskLowestOpenSequenceNumber);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 2: postreq workflow", WorkflowStatusList.Codes.Blocked, workflow2PostreqInFactory2.FH_Status);

			Factory.Save();

			AssertEquals("After saving by the 1st user: Factory 1: first workflow", WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 1: first JLW", WorkflowStatusList.Codes.Open, jobHeader1.FH_Status);
			AssertEquals(2, jobHeader1.FH_TaskLowestOpenSequenceNumber);
			AssertEquals("After saving by the 1st user: Factory 1: postreq workflow", WorkflowStatusList.Codes.Blocked, workflow2Postreq.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 2: first workflow", WorkflowStatusList.Codes.Open, workflow1InFactory2.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 2: first JLW", WorkflowStatusList.Codes.Open, jobHeader1InFactory2.FH_Status);
			AssertEquals(1, jobHeader1InFactory2.FH_TaskLowestOpenSequenceNumber);
			AssertEquals("After saving by the 1st user: Factory 2: postreq workflow", WorkflowStatusList.Codes.Blocked, workflow2PostreqInFactory2.FH_Status);
		}

		public void TestWorkflowStatusUpdates_WhenOneUserModifiesEmptyWorkflow_AndAnotherUserAddsOpenTask_ToTheSameWorkflow()
		{
			BMSTestHelper.CreateSystem(Factory, "INQ");
			var jobHeader1 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Workflow 1");

			Factory.Save();

			AssertEquals("Precondition: Factory 1: first workflow", WorkflowStatusList.Codes.Closed, workflow1.FH_Status);
			AssertEquals("Precondition: Factory 1: first JLW", WorkflowStatusList.Codes.Closed, jobHeader1.FH_Status);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var workflow1InFactory2 = factory2.Load<ProcessHeader>(workflow1.PK);
			var jobHeader1InFactory2 = factory2.Load<ProcessHeader>(jobHeader1.PK);

			BMSTestHelper.CreateTask(workflow1InFactory2);
			factory2.Save();

			AssertEquals("Precondition: After saving by the 2nd user: Factory 1: first workflow", WorkflowStatusList.Codes.Closed, workflow1.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 1: first JLW", WorkflowStatusList.Codes.Closed, jobHeader1.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 2: first workflow", WorkflowStatusList.Codes.Open, workflow1InFactory2.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 2: first JLW", WorkflowStatusList.Codes.Open, jobHeader1InFactory2.FH_Status);

			workflow1.FH_CompletionStatement = "New name";
			Factory.Save();

			AssertEquals("After saving by the 1st user: Factory 1: first workflow", WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 1: first JLW", WorkflowStatusList.Codes.Open, jobHeader1.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 2: first workflow", WorkflowStatusList.Codes.Open, workflow1InFactory2.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 2: first JLW", WorkflowStatusList.Codes.Open, jobHeader1InFactory2.FH_Status);
		}

		public void TestWorkflowStatusUpdates_WhenOneUserClosesOneWorkflow_AndAnotherUserClosesTheOtherWorkflow_InTheSameJob()
		{
			BMSTestHelper.CreateSystem(Factory, "INQ");
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 2");

			var task1 = BMSTestHelper.CreateTask(workflow1, sequence: 1, description: "Task 1");
			var task2 = BMSTestHelper.CreateTask(workflow2, sequence: 2, description: "Task 2");

			Factory.Save();

			AssertEquals("Precondition: Factory 1: first workflow", WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals("Precondition: Factory 1: second workflow", WorkflowStatusList.Codes.Open, workflow2.FH_Status);
			AssertEquals("Precondition: Factory 1: JLW", WorkflowStatusList.Codes.Open, jobHeader.FH_Status);
			AssertEquals(1, jobHeader.FH_TaskLowestOpenSequenceNumber);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed; // let's change the status before saving in another factory
			AssertEquals("Precondition: After making changes by the 1st user: Factory 1: first workflow", WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals("Precondition: After making changes by the 1st user: Factory 1: second workflow", WorkflowStatusList.Codes.Open, workflow2.FH_Status);
			AssertEquals("Precondition: After making changes by the 1st user: Factory 1: first JLW", WorkflowStatusList.Codes.Open, jobHeader.FH_Status);
			AssertEquals(1, jobHeader.FH_TaskLowestOpenSequenceNumber);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var workflow1InFactory2 = factory2.Load<ProcessHeader>(workflow1.PK);
			var workflow2InFactory2 = factory2.Load<ProcessHeader>(workflow2.PK);
			var jobHeaderInFactory2 = factory2.Load<ProcessHeader>(jobHeader.PK);

			workflow2InFactory2.Tasks.Single().P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(1, jobHeaderInFactory2.FH_TaskLowestOpenSequenceNumber);

			factory2.Save();

			AssertEquals("Precondition: After saving by the 2nd user: Factory 1: first workflow", WorkflowStatusList.Codes.Open, workflow1.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 1: second workflow", WorkflowStatusList.Codes.Open, workflow2.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 1: JLW", WorkflowStatusList.Codes.Open, jobHeader.FH_Status);
			AssertEquals(1, jobHeader.FH_TaskLowestOpenSequenceNumber);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 2: first workflow", WorkflowStatusList.Codes.Open, workflow1InFactory2.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 2: second workflow", WorkflowStatusList.Codes.Closed, workflow2InFactory2.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 2: JLW", WorkflowStatusList.Codes.Open, jobHeaderInFactory2.FH_Status);
			AssertEquals(1, jobHeaderInFactory2.FH_TaskLowestOpenSequenceNumber);

			Factory.Save();

			AssertEquals("After saving by the 1st user: Factory 1: first workflow", WorkflowStatusList.Codes.Closed, workflow1.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 1: second workflow", WorkflowStatusList.Codes.Closed, workflow2.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 1: JLW", WorkflowStatusList.Codes.Closed, jobHeader.FH_Status);
			AssertEquals(-1, jobHeader.FH_TaskLowestOpenSequenceNumber);
			AssertEquals("After saving by the 1st user: Factory 2: first workflow", WorkflowStatusList.Codes.Open, workflow1InFactory2.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 2: second workflow", WorkflowStatusList.Codes.Closed, workflow2InFactory2.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 2: JLW", WorkflowStatusList.Codes.Open, jobHeaderInFactory2.FH_Status);
			AssertEquals(1, jobHeaderInFactory2.FH_TaskLowestOpenSequenceNumber);
		}

		public void TestWorkflowStatusUpdates_WhenOneUserClosesSingleTaskInParentWorkflow_AndAnotherUserClosesTaskInChildWorkflow_InAnotherJob()
		{
			BMSTestHelper.CreateSystem(Factory, "INQ");
			var jobHeader1Parent = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader 1 Parent");
			var jobHeader2Child = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader 2 Child");
			var jobHeader3Postreq = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader 2 Postreq");
			var workflow1Parent = BMSTestHelper.CreateWorkflow(jobHeader1Parent, "Workflow 1 Parent");
			var workflow2Child = BMSTestHelper.CreateWorkflow(jobHeader2Child, "Workflow 2 Child");
			var workflow3Postreq = BMSTestHelper.CreateWorkflow(jobHeader3Postreq, "Workflow 3 Postreq");
			workflow1Parent.MakePrerequisiteOf(workflow3Postreq);
			BMSTestHelper.MakeChildOfAndGetLink(workflow2Child, workflow1Parent);

			var task1Parent = BMSTestHelper.CreateTask(workflow1Parent, sequence: 1, description: "Task 1 Parent");
			var task2Child = BMSTestHelper.CreateTask(workflow2Child, sequence: 2, description: "Task 2 Child");
			var task3Postreq = BMSTestHelper.CreateTask(workflow3Postreq, description: "Task postreq");

			Factory.Save();

			AssertEquals("Precondition: Factory 1: parent workflow", WorkflowStatusList.Codes.Open, workflow1Parent.FH_Status);
			AssertEquals("Precondition: Factory 1: child workflow", WorkflowStatusList.Codes.Open, workflow2Child.FH_Status);
			AssertEquals("Precondition: Factory 1: parent JLW", WorkflowStatusList.Codes.Open, jobHeader1Parent.FH_Status);
			AssertEquals("Precondition: Factory 1: postreq workflow", WorkflowStatusList.Codes.Blocked, workflow3Postreq.FH_Status);

			task1Parent.P9_Status = ProcessTaskStatusCodeList.Codes.Closed; // let's change the status before saving in another factory
			AssertEquals("Precondition: After making changes by the 1st user: Factory 1: parent workflow", WorkflowStatusList.Codes.Open, workflow1Parent.FH_Status);
			AssertEquals("Precondition: After making changes by the 1st user: Factory 1: parent JLW", WorkflowStatusList.Codes.Open, jobHeader1Parent.FH_Status);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var workflow1ParentInFactory2 = factory2.Load<ProcessHeader>(workflow1Parent.PK);
			var jobHeader1ParentInFactory2 = factory2.Load<ProcessHeader>(jobHeader1Parent.PK);
			var workflow2ChildInFactory2 = factory2.Load<ProcessHeader>(workflow2Child.PK);
			var workflow3PostreqInFactory2 = factory2.Load<ProcessHeader>(workflow3Postreq.PK);

			workflow2ChildInFactory2.Tasks.Single().P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			factory2.Save();

			AssertEquals("Precondition: After saving by the 2nd user: Factory 1: parent workflow", WorkflowStatusList.Codes.Open, workflow1Parent.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 1: child workflow", WorkflowStatusList.Codes.Open, workflow2Child.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 1: parent JLW", WorkflowStatusList.Codes.Open, jobHeader1Parent.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 1: postreq workflow", WorkflowStatusList.Codes.Blocked, workflow3Postreq.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 2: parent workflow", WorkflowStatusList.Codes.Open, workflow1ParentInFactory2.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 2: child workflow", WorkflowStatusList.Codes.Closed, workflow2ChildInFactory2.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 2: parent JLW", WorkflowStatusList.Codes.Open, jobHeader1ParentInFactory2.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 2: postreq workflow", WorkflowStatusList.Codes.Blocked, workflow3PostreqInFactory2.FH_Status);

			Factory.Save();

			AssertEquals("After saving by the 1st user: Factory 1: parent workflow", WorkflowStatusList.Codes.Closed, workflow1Parent.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 1: child workflow", WorkflowStatusList.Codes.Closed, workflow2Child.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 1: parent JLW", WorkflowStatusList.Codes.Closed, jobHeader1Parent.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 1: postreq workflow", WorkflowStatusList.Codes.Open, workflow3Postreq.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 2: parent workflow", WorkflowStatusList.Codes.Open, workflow1ParentInFactory2.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 2: child workflow", WorkflowStatusList.Codes.Closed, workflow2ChildInFactory2.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 2: parent JLW", WorkflowStatusList.Codes.Open, jobHeader1ParentInFactory2.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 2: postreq workflow", WorkflowStatusList.Codes.Blocked, workflow3PostreqInFactory2.FH_Status);
		}

		public void TestWorkflowStatusUpdates_WhenOneUserClosesSingleTaskInChildWorkflow_AndAnotherUserClosesTaskInParentWorkflow_InAnotherJob()
		{
			BMSTestHelper.CreateSystem(Factory, "INQ");
			var jobHeader1Parent = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader 1 Parent");
			var jobHeader2Child = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader 2 Child");
			var jobHeader3Postreq = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader 2 Postreq");
			var workflow1Parent = BMSTestHelper.CreateWorkflow(jobHeader1Parent, "Workflow 1 Parent");
			var workflow2Child = BMSTestHelper.CreateWorkflow(jobHeader2Child, "Workflow 2 Child");
			var workflow3Postreq = BMSTestHelper.CreateWorkflow(jobHeader3Postreq, "Workflow 3 Postreq");
			workflow1Parent.MakePrerequisiteOf(workflow3Postreq);
			BMSTestHelper.MakeChildOfAndGetLink(workflow2Child, workflow1Parent);

			var task1Parent = BMSTestHelper.CreateTask(workflow1Parent, sequence: 1, description: "Task 1 Parent");
			var task2Child = BMSTestHelper.CreateTask(workflow2Child, sequence: 2, description: "Task 2 Child");
			var task3Postreq = BMSTestHelper.CreateTask(workflow3Postreq, description: "Task postreq");

			Factory.Save();

			AssertEquals("Precondition: Factory 1: parent workflow", WorkflowStatusList.Codes.Open, workflow1Parent.FH_Status);
			AssertEquals("Precondition: Factory 1: child workflow", WorkflowStatusList.Codes.Open, workflow2Child.FH_Status);
			AssertEquals("Precondition: Factory 1: parent JLW", WorkflowStatusList.Codes.Open, jobHeader1Parent.FH_Status);
			AssertEquals("Precondition: Factory 1: postreq workflow", WorkflowStatusList.Codes.Blocked, workflow3Postreq.FH_Status);

			task2Child.P9_Status = ProcessTaskStatusCodeList.Codes.Closed; // let's change the status before saving in another factory
			AssertEquals("Precondition: After making changes by the 1st user: Factory 1: child workflow", WorkflowStatusList.Codes.Open, workflow2Child.FH_Status);
			AssertEquals("Precondition: After making changes by the 1st user: Factory 1: parent workflow", WorkflowStatusList.Codes.Open, workflow1Parent.FH_Status);
			AssertEquals("Precondition: After making changes by the 1st user: Factory 1: parent JLW", WorkflowStatusList.Codes.Open, jobHeader1Parent.FH_Status);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var workflow1ParentInFactory2 = factory2.Load<ProcessHeader>(workflow1Parent.PK);
			var jobHeader1ParentInFactory2 = factory2.Load<ProcessHeader>(jobHeader1Parent.PK);
			var workflow2ChildInFactory2 = factory2.Load<ProcessHeader>(workflow2Child.PK);
			var workflow3PostreqInFactory2 = factory2.Load<ProcessHeader>(workflow3Postreq.PK);

			workflow1ParentInFactory2.Tasks.Single().P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			factory2.Save();

			AssertEquals("Precondition: After saving by the 2nd user: Factory 1: parent workflow", WorkflowStatusList.Codes.Open, workflow1Parent.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 1: child workflow", WorkflowStatusList.Codes.Open, workflow2Child.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 1: parent JLW", WorkflowStatusList.Codes.Open, jobHeader1Parent.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 1: postreq workflow", WorkflowStatusList.Codes.Blocked, workflow3Postreq.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 2: parent workflow", WorkflowStatusList.Codes.Open, workflow1ParentInFactory2.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 2: child workflow", WorkflowStatusList.Codes.Open, workflow2ChildInFactory2.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 2: parent JLW", WorkflowStatusList.Codes.Open, jobHeader1ParentInFactory2.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 2: postreq workflow", WorkflowStatusList.Codes.Blocked, workflow3PostreqInFactory2.FH_Status);

			Factory.Save();

			AssertEquals("After saving by the 1st user: Factory 1: parent workflow", WorkflowStatusList.Codes.Closed, workflow1Parent.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 1: child workflow", WorkflowStatusList.Codes.Closed, workflow2Child.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 1: parent JLW", WorkflowStatusList.Codes.Closed, jobHeader1Parent.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 1: postreq workflow", WorkflowStatusList.Codes.Open, workflow3Postreq.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 2: parent workflow", WorkflowStatusList.Codes.Open, workflow1ParentInFactory2.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 2: child workflow", WorkflowStatusList.Codes.Open, workflow2ChildInFactory2.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 2: parent JLW", WorkflowStatusList.Codes.Open, jobHeader1ParentInFactory2.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 2: postreq workflow", WorkflowStatusList.Codes.Blocked, workflow3PostreqInFactory2.FH_Status);
		}

		public void TestWorkflowStatusUpdates_WhenOneUserClosesSingleTaskInWorkflow_AndAnotherUserAddsOpenChildWorkflow()
		{
			BMSTestHelper.CreateSystem(Factory, "INQ");
			var jobHeader1Parent = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader 1 Parent");
			var jobHeader2Child = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader 2 Child");
			var jobHeader3Postreq = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: "JobHeader 2 Postreq");
			var workflow1Parent = BMSTestHelper.CreateWorkflow(jobHeader1Parent, "Workflow 1 Parent");
			var workflow2Child = BMSTestHelper.CreateWorkflow(jobHeader2Child, "Workflow 2 Child");
			var workflow3Postreq = BMSTestHelper.CreateWorkflow(jobHeader3Postreq, "Workflow 3 Postreq");
			workflow1Parent.MakePrerequisiteOf(workflow3Postreq);

			var task1Parent = BMSTestHelper.CreateTask(workflow1Parent, sequence: 1, description: "Task 1 Parent");
			var task2Child = BMSTestHelper.CreateTask(workflow2Child, sequence: 2, description: "Task 2 Child");
			var task3Postreq = BMSTestHelper.CreateTask(workflow3Postreq, description: "Task postreq");

			Factory.Save();

			workflow1Parent.LinksFromMeToOthers.ToArray();
			workflow1Parent.LinksFromOthersToMe.ToArray();
			workflow2Child.LinksFromMeToOthers.ToArray();
			workflow2Child.LinksFromOthersToMe.ToArray();

			AssertEquals("Precondition: Factory 1: parent workflow", WorkflowStatusList.Codes.Open, workflow1Parent.FH_Status);
			AssertEquals("Precondition: Factory 1: child workflow", WorkflowStatusList.Codes.Open, workflow2Child.FH_Status);
			AssertEquals("Precondition: Factory 1: parent JLW", WorkflowStatusList.Codes.Open, jobHeader1Parent.FH_Status);
			AssertEquals("Precondition: Factory 1: postreq workflow", WorkflowStatusList.Codes.Blocked, workflow3Postreq.FH_Status);

			task1Parent.P9_Status = ProcessTaskStatusCodeList.Codes.Closed; // let's change the status before saving in another factory
			AssertEquals("Precondition: After making changes by the 1st user: Factory 1: parent workflow", WorkflowStatusList.Codes.Open, workflow1Parent.FH_Status);
			AssertEquals("Precondition: After making changes by the 1st user: Factory 1: parent JLW", WorkflowStatusList.Codes.Open, jobHeader1Parent.FH_Status);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var workflow1ParentInFactory2 = factory2.Load<ProcessHeader>(workflow1Parent.PK);
			var jobHeader1ParentInFactory2 = factory2.Load<ProcessHeader>(jobHeader1Parent.PK);
			var workflow2ChildInFactory2 = factory2.Load<ProcessHeader>(workflow2Child.PK);
			var workflow3PostreqInFactory2 = factory2.Load<ProcessHeader>(workflow3Postreq.PK);

			BMSTestHelper.MakeChildOfAndGetLink(workflow2ChildInFactory2, workflow1ParentInFactory2);

			factory2.Save();

			AssertEquals("Precondition: After saving by the 2nd user: Factory 1: parent workflow", WorkflowStatusList.Codes.Open, workflow1Parent.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 1: child workflow", WorkflowStatusList.Codes.Open, workflow2Child.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 1: parent JLW", WorkflowStatusList.Codes.Open, jobHeader1Parent.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 1: postreq workflow", WorkflowStatusList.Codes.Blocked, workflow3Postreq.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 2: parent workflow", WorkflowStatusList.Codes.Open, workflow1ParentInFactory2.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 2: child workflow", WorkflowStatusList.Codes.Open, workflow2ChildInFactory2.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 2: parent JLW", WorkflowStatusList.Codes.Open, jobHeader1ParentInFactory2.FH_Status);
			AssertEquals("Precondition: After saving by the 2nd user: Factory 2: postreq workflow", WorkflowStatusList.Codes.Blocked, workflow3PostreqInFactory2.FH_Status);

			Factory.Save();

			AssertEquals("After saving by the 1st user: Factory 1: parent workflow", WorkflowStatusList.Codes.Open, workflow1Parent.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 1: child workflow", WorkflowStatusList.Codes.Open, workflow2Child.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 1: parent JLW", WorkflowStatusList.Codes.Open, jobHeader1Parent.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 1: postreq workflow", WorkflowStatusList.Codes.Blocked, workflow3Postreq.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 2: parent workflow", WorkflowStatusList.Codes.Open, workflow1ParentInFactory2.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 2: child workflow", WorkflowStatusList.Codes.Open, workflow2ChildInFactory2.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 2: parent JLW", WorkflowStatusList.Codes.Open, jobHeader1ParentInFactory2.FH_Status);
			AssertEquals("After saving by the 1st user: Factory 2: postreq workflow", WorkflowStatusList.Codes.Blocked, workflow3PostreqInFactory2.FH_Status);
		}

		#endregion

		#region Some Assertions

		void AssertWorkflowIsOpen(string message, ProcessHeader workflow)
		{
			AssertEquals(message, WorkflowStatusList.Codes.Open, workflow.FH_Status);
		}

		void AssertWorkflowIsClosed(string message, ProcessHeader workflow)
		{
			AssertEquals(message, WorkflowStatusList.Codes.Closed, workflow.FH_Status);
		}

		void AssertWorkflowIsClosedWithOpenPrerequisites(string message, ProcessHeader workflow)
		{
			AssertEquals(message, WorkflowStatusList.Codes.ClosedWithOpenPrerequisites, workflow.FH_Status);
		}

		void AssertWorkflowIsBlocked(string message, ProcessHeader workflow)
		{
			AssertEquals(message, WorkflowStatusList.Codes.Blocked, workflow.FH_Status);
		}

		#endregion

		#endregion

		#endregion

		#region Performance

		public void TestApprovedShape_ShouldNotAccessActiveBusinessObjectCollections()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Shivermetimbers");

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);

			using (ActiveBusinessObjectCollection.TrackIndexedCollectionCounts_ForTest())
			{
				var shape = loadedWorkflow.ApprovedShape;

				AssertContainsExactElementsInAnyOrder(Array.Empty<KeyValuePair<Type, int>>(), ActiveBusinessObjectCollection.IndexedCollections_ForTest);
			}
		}

		public void TestTagableParent_ShouldNotAccessActiveBusinessObjectCollections()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Shivermetimbers");

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);

			using (ActiveBusinessObjectCollection.TrackIndexedCollectionCounts_ForTest())
			{
				var parent = ((ITagable)loadedWorkflow).Parent;

				AssertContainsExactElementsInAnyOrder(Array.Empty<KeyValuePair<Type, int>>(), ActiveBusinessObjectCollection.IndexedCollections_ForTest);
			}
		}

		public void TestAddDeepFetchHintForParentType_WhenDeleted_ShouldNotThrowExceptionsOrReportErrors()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow");
			workflow.Delete();

			AssertNoExceptionThrown(() => workflow.AddDeepFetchHintForParentType());
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestAddDeepFetchHintForParentType_WithNullParent_ShouldNotThrowExceptionsOrReportErrors()
		{
			var workflow = Factory.New<ProcessHeader>();
			workflow.FH_ParentId = Guid.NewGuid();
			workflow.FH_ParentTableCode = "VB";
			workflow.FH_CompletionStatement = "My workflow";

			AssertNoExceptionThrown("Adding fetch hints for this parent-less workflow should not throw uncaught excaptions. SAD!", () => workflow.AddDeepFetchHintForParentType(parentType: typeof(NonPersistentBusinessObjectWorkflowProvider_ForTest)));
			AssertEquals("No errors should be reported, because we are happy to simply not add fetch hints for these rare workflows. SAD!", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestAddDeepFetchHintForParentType_ForParentThatDoesNotHaveWorkflowDescriptor_ShouldReportError()
		{
			var parent = new NonPersistentBusinessObjectWorkflowProvider_ForTest();
			var workflowDescriptor = new WorkflowDescriptor.Loader().GetWorkflowDescriptor(parent.WorkflowType);
			AssertNull("Precondition: Somehow we have a workflow provider without a matching workflow descriptor. Simulating this in a test needs to return null for the descriptor.", workflowDescriptor);

			var workflow = Factory.New<ProcessHeaderWithSettableParent>();
			workflow.SetParent(parent);
			workflow.FH_CompletionStatement = "My workflow";

			AssertNoExceptionThrown("Adding fetch hints for this workflow descriptor-less workflow provider should not throw unhandled exceptions. SAD!", () => workflow.AddDeepFetchHintForParentType(parentType: typeof(NonPersistentBusinessObjectWorkflowProvider_ForTest)));
			AssertEquals("NoWorkflowDescriptorForWorkflowProvider", ErrorReporter.LastKeyReported);
			AssertEquals(@"Attempted to add fetch hints to a workflow for a NonPersistentBusinessObject but could not get a matching workflow provider. Details:
Parent type: NonPersistentBusinessObjectWorkflowProvider_ForTest
Workflow type: ZZZ
FH_ParentTableCode: 
Workflow description: My workflow", ErrorReporter.LastMessageReported);
			AssertEquals(1, ErrorReporter.TotalErrorCount);

			ErrorReporter.Clear();
		}

		class NonPersistentBusinessObjectWorkflowProvider_ForTest : NonPersistentBusinessObject, IWorkflowProvider
		{
			public ZString WorkflowType => "ZZZ";

			#region Irrelevant Implementation Details

			public IColumnValueRanker GetTemplateSelectionCriteria()
			{
				throw new NotImplementedException();
			}

			public IWorkflowInformationProvider GetWorkflowInformationProvider()
			{
				throw new NotImplementedException();
			}

			public IProcessHeaderCollection Workflows { get; }
			public ProcessTaskCollection WorkflowItems { get; }
			public Logs Logs { get; }
			public BusinessObjectFactory LogsFactory { get; }

			#endregion
		}

		class ProcessHeaderWithSettableParent : ProcessHeader
		{
			public ProcessHeaderWithSettableParent(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public void SetParent(IWorkflowProvider parent)
			{
				Parent = parent;
				FH_ParentId = parent.PK;
			}
		}

		#endregion

		#region Resource Strings

		public void TestResourceString_FH_TimeDelayFactor()
		{
			var processHeader = BMSTestHelper.CreateWorkflow(Factory, "workflow");
			var fullDescription = DataBoundResourceStrings.GetDataForProperty(processHeader.FH_TimeDelayFactorInfo).FullDescription;

			AssertEquals("The number of times to multiply (factor) the sum of the estimates of previous workflows that need to be elapsed when determining a workflow's Staggered Release Delay Expiry.", fullDescription);
		}

		public void TestResourceString_FH_TimeDelayMinutes()
		{
			var processHeader = BMSTestHelper.CreateWorkflow(Factory, "workflow");
			var fullDescription = DataBoundResourceStrings.GetDataForProperty(processHeader.FH_TimeDelayMinutesInfo).FullDescription;

			AssertEquals("The number of hours/minutes from release of previous workflows that need to be elapsed when determining a workflow’s Staggered Release Delay Expiry.", fullDescription);
		}

		#endregion

		#region Default Values

		public void TestDefaultValues()
		{
			var workflow = Factory.New<ProcessHeader>();

			AssertEquals("NA", workflow.FH_LastTransferType);
		}

		#endregion

		#region Responsive Actions

		public void TestResponsiveMarkForDedicatedBufferUpdate_AXBAction()
		{
			AssertNotResponsivelyMarkedForDedicatedBufferUpdate(ProcessHeaderResponsiveActionConstants.UpdateAllExceptDedicatedBuffer);
		}

		public void TestResponsiveMarkForDedicatedBufferUpdate_BUFAction()
		{
			AssertResponsivelyMarkedForDedicatedBufferUpdate(ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer);
		}

		public void TestResponsiveMarkForDedicatedBufferUpdate_EBDAction()
		{
			AssertNotResponsivelyMarkedForDedicatedBufferUpdate(ProcessHeaderResponsiveActionConstants.UpdateEffectiveBranchAndDepartment);
		}

		public void TestResponsiveMarkForDedicatedBufferUpdate_BBDAction()
		{
			AssertResponsivelyMarkedForDedicatedBufferUpdate(ProcessHeaderResponsiveActionConstants.UpdateDedicatedBufferEffectiveBranchAndDepartment);
		}

		void AssertResponsivelyMarkedForDedicatedBufferUpdate(string actionCode)
		{
			var processHeader = BMSTestHelper.CreateWorkflow(Factory, "workflow");

			Factory.Save();

			var oldLastEditTime = processHeader.FH_SystemLastEditTimeUtc;
			processHeader.PerformResponsiveAction(actionCode);
			AssertNotEquals("Should mark the workflow for dedicated buffer update by changing its last edit time", oldLastEditTime, processHeader.FH_SystemLastEditTimeUtc);
		}

		void AssertNotResponsivelyMarkedForDedicatedBufferUpdate(string actionCode)
		{
			var processHeader = BMSTestHelper.CreateWorkflow(Factory, "workflow");

			Factory.Save();

			var oldLastEditTime = processHeader.FH_SystemLastEditTimeUtc;
			processHeader.PerformResponsiveAction(actionCode);
			AssertEquals("Should not mark the workflow for dedicated buffer update by changing its last edit time", oldLastEditTime, processHeader.FH_SystemLastEditTimeUtc);
		}

		public void TestResponsiveEffectiveBranchAndDepartmentUpdate_AXBAction()
		{
			AssertResponsivelyUpdatedEffectiveBranchAndDepartment(ProcessHeaderResponsiveActionConstants.UpdateAllExceptDedicatedBuffer);
		}

		public void TestResponsiveEffectiveBranchAndDepartmentUpdate_BUFAction()
		{
			AssertNotResponsivelyUpdatedEffectiveBranchAndDepartment(ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer);
		}

		public void TestResponsiveEffectiveBranchAndDepartmentUpdate_EBDAction()
		{
			AssertResponsivelyUpdatedEffectiveBranchAndDepartment(ProcessHeaderResponsiveActionConstants.UpdateEffectiveBranchAndDepartment);
		}

		public void TestResponsiveEffectiveBranchAndDepartmentUpdate_BBDAction()
		{
			AssertResponsivelyUpdatedEffectiveBranchAndDepartment(ProcessHeaderResponsiveActionConstants.UpdateDedicatedBufferEffectiveBranchAndDepartment);
		}

		void AssertResponsivelyUpdatedEffectiveBranchAndDepartment(string actionCode)
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);

			buffer.FC_GB_AgingBranch = branch1.PK;
			buffer.FC_GE_AgingDepartment = department1.PK;

			BMSTestHelper.LinkComponents(bucket, buffer, isReleaseGate: true);

			var processHeader = BMSTestHelper.CreateWorkflow(Factory, "workflow", currentComponent: bucket);
			processHeader.FH_FC_DedicatedBuffer = buffer.PK;
			AssertEquals(branch1.PK, processHeader.FH_GB_EffectiveBranch);
			AssertEquals(department1.PK, processHeader.FH_GE_EffectiveDepartment);

			buffer.FC_GB_AgingBranch = branch2.PK;
			buffer.FC_GE_AgingDepartment = department2.PK;

			AssertEquals("Effective branch should not change yet", branch1.PK, processHeader.FH_GB_EffectiveBranch);
			AssertEquals("Effective department should not change yet", department1.PK, processHeader.FH_GE_EffectiveDepartment);

			processHeader.PerformResponsiveAction(actionCode);

			AssertEquals("Effective branch should update", branch2.PK, processHeader.FH_GB_EffectiveBranch);
			AssertEquals("Effective department should update", department2.PK, processHeader.FH_GE_EffectiveDepartment);
		}

		void AssertNotResponsivelyUpdatedEffectiveBranchAndDepartment(string actionCode)
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);

			buffer.FC_GB_AgingBranch = branch1.PK;
			buffer.FC_GE_AgingDepartment = department1.PK;

			BMSTestHelper.LinkComponents(bucket, buffer, isReleaseGate: true);

			var processHeader = BMSTestHelper.CreateWorkflow(Factory, "workflow", currentComponent: bucket);
			processHeader.FH_FC_DedicatedBuffer = buffer.PK;
			AssertEquals(branch1.PK, processHeader.FH_GB_EffectiveBranch);
			AssertEquals(department1.PK, processHeader.FH_GE_EffectiveDepartment);

			buffer.FC_GB_AgingBranch = branch2.PK;
			buffer.FC_GE_AgingDepartment = department2.PK;

			AssertEquals("Effective branch should not change yet", branch1.PK, processHeader.FH_GB_EffectiveBranch);
			AssertEquals("Effective department should not change yet", department1.PK, processHeader.FH_GE_EffectiveDepartment);

			processHeader.PerformResponsiveAction(actionCode);

			AssertEquals("Effective branch should not update still", branch1.PK, processHeader.FH_GB_EffectiveBranch);
			AssertEquals("Effective department should not update still", department1.PK, processHeader.FH_GE_EffectiveDepartment);
		}

		public void TestResponsiveEffectiveNudgeUpdate_AXBAction()
		{
			AssertResponsivelyUpdatedEffectiveNudge(ProcessHeaderResponsiveActionConstants.UpdateAllExceptDedicatedBuffer);
		}

		public void TestResponsiveEffectiveNudgeUpdate_BUFAction()
		{
			AssertNotResponsivelyUpdatedEffectiveNudge(ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer);
		}

		public void TestResponsiveEffectiveNudgeUpdate_EBDAction()
		{
			AssertNotResponsivelyUpdatedEffectiveNudge(ProcessHeaderResponsiveActionConstants.UpdateEffectiveBranchAndDepartment);
		}

		public void TestResponsiveEffectiveNudgeUpdate_BBDAction()
		{
			AssertNotResponsivelyUpdatedEffectiveNudge(ProcessHeaderResponsiveActionConstants.UpdateDedicatedBufferEffectiveBranchAndDepartment);
		}

		void AssertResponsivelyUpdatedEffectiveNudge(string actionCode)
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "TGR");
			var tag = BMSTestHelper.CreateTagMagnitude(tagGroup, "TAG");

			var processHeader = BMSTestHelper.CreateWorkflowAndTask(Factory, "workflow", currentComponent: bucket);
			processHeader.FH_FC_DedicatedBuffer = buffer.PK; // we don't calculate effective nudge for workflows with no dedicated buffer set
			BMSTestHelper.CreateTagLink(processHeader, tag);
			tag.TGM_NudgeAmount = 1000;

			AssertEquals(0m, processHeader.FH_EffectiveNudge);

			processHeader.PerformResponsiveAction(actionCode);

			AssertEquals("Effective nudge should update", 1000m, processHeader.FH_EffectiveNudge);
		}

		void AssertNotResponsivelyUpdatedEffectiveNudge(string actionCode)
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);

			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "TGR");
			var tag = BMSTestHelper.CreateTagMagnitude(tagGroup, "TAG");

			var processHeader = BMSTestHelper.CreateWorkflowAndTask(Factory, "workflow", currentComponent: bucket);
			processHeader.FH_FC_DedicatedBuffer = buffer.PK; // we don't calculate effective nudge for workflows with no dedicated buffer set
			BMSTestHelper.CreateTagLink(processHeader, tag);
			tag.TGM_NudgeAmount = 1000;

			AssertEquals(0m, processHeader.FH_EffectiveNudge);

			processHeader.PerformResponsiveAction(actionCode);

			AssertEquals("Effective nudge should not update", 0m, processHeader.FH_EffectiveNudge);
		}

		public void TestResponsiveEffectiveAgreedDeliveryDateUpdate_AXBAction()
		{
			AssertResponsivelyUpdatedEffectiveAgreedDeliveryDate(ProcessHeaderResponsiveActionConstants.UpdateAllExceptDedicatedBuffer);
		}

		public void TestResponsiveEffectiveAgreedDeliveryDateUpdate_BUFAction()
		{
			AssertNotResponsivelyUpdatedEffectiveAgreedDeliveryDate(ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer);
		}

		public void TestResponsiveEffectiveAgreedDeliveryDateUpdate_EBDAction()
		{
			AssertNotResponsivelyUpdatedEffectiveAgreedDeliveryDate(ProcessHeaderResponsiveActionConstants.UpdateEffectiveBranchAndDepartment);
		}

		public void TestResponsiveEffectiveAgreedDeliveryDateUpdate_BBDAction()
		{
			AssertNotResponsivelyUpdatedEffectiveAgreedDeliveryDate(ProcessHeaderResponsiveActionConstants.UpdateDedicatedBufferEffectiveBranchAndDepartment);
		}

		void AssertResponsivelyUpdatedEffectiveAgreedDeliveryDate(string actionCode)
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			var workflow = BMSTestHelper.CreateWorkflowAndTask(Factory, "Workflow");
			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			Assert("Precondition", workflow.CanCalculateSparsePropertiesForNewReleaseGate);

			workflow.FH_AgreedDeliveryDate = new ZDateTime(2025, 1, 1);
			AssertEquals("Precondition", new ZDateTime(2025, 1, 1), workflow.FH_EffectiveAgreedDeliveryDateUtc);

			workflow.FH_EffectiveAgreedDeliveryDateUtc = new ZDateTime(2025, 2, 2);

			workflow.PerformResponsiveAction(actionCode);

			AssertEquals("Should update effective agreed delivery date", new ZDateTime(2025, 1, 1), workflow.FH_EffectiveAgreedDeliveryDateUtc);
		}

		void AssertNotResponsivelyUpdatedEffectiveAgreedDeliveryDate(string actionCode)
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			var workflow = BMSTestHelper.CreateWorkflowAndTask(Factory, "Workflow");
			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			Assert("Precondition", workflow.CanCalculateSparsePropertiesForNewReleaseGate);

			workflow.FH_AgreedDeliveryDate = new ZDateTime(2025, 1, 1);
			AssertEquals("Precondition", new ZDateTime(2025, 1, 1), workflow.FH_EffectiveAgreedDeliveryDateUtc);

			workflow.FH_EffectiveAgreedDeliveryDateUtc = new ZDateTime(2025, 2, 2);

			workflow.PerformResponsiveAction(actionCode);

			AssertEquals("Should not update effective agreed delivery date", new ZDateTime(2025, 2, 2), workflow.FH_EffectiveAgreedDeliveryDateUtc);
		}

		public void TestResponsiveReleaseSequenceSortDateUpdate_AXBAction()
		{
			AssertResponsivelyUpdatedReleaseSequenceSortDate(ProcessHeaderResponsiveActionConstants.UpdateAllExceptDedicatedBuffer);
		}

		public void TestResponsiveReleaseSequenceSortDateUpdate_BUFAction()
		{
			AssertNotResponsivelyUpdatedReleaseSequenceSortDate(ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer);
		}

		public void TestResponsiveReleaseSequenceSortDateUpdate_EBDAction()
		{
			AssertNotResponsivelyUpdatedReleaseSequenceSortDate(ProcessHeaderResponsiveActionConstants.UpdateEffectiveBranchAndDepartment);
		}

		public void TestResponsiveReleaseSequenceSortDateUpdate_BBDAction()
		{
			AssertNotResponsivelyUpdatedReleaseSequenceSortDate(ProcessHeaderResponsiveActionConstants.UpdateDedicatedBufferEffectiveBranchAndDepartment);
		}

		void AssertResponsivelyUpdatedReleaseSequenceSortDate(string actionCode)
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			Assert("Precondition", workflow.CanCalculateSparsePropertiesForNewReleaseGate);

			workflow.FH_ReleaseDateTime = new ZDateTime(2025, 1, 1);
			AssertEquals("Precondition", new ZDateTime(2025, 1, 1), workflow.FH_ReleaseSequenceSortDateUtc);

			workflow.FH_ReleaseSequenceSortDateUtc = new ZDateTime(2025, 2, 2);

			workflow.PerformResponsiveAction(actionCode);

			AssertEquals("Should update release sequence sort date", new ZDateTime(2025, 1, 1), workflow.FH_ReleaseSequenceSortDateUtc);
		}

		void AssertNotResponsivelyUpdatedReleaseSequenceSortDate(string actionCode)
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			Assert("Precondition", workflow.CanCalculateSparsePropertiesForNewReleaseGate);

			workflow.FH_ReleaseDateTime = new ZDateTime(2025, 1, 1);
			AssertEquals("Precondition", new ZDateTime(2025, 1, 1), workflow.FH_ReleaseSequenceSortDateUtc);

			workflow.FH_ReleaseSequenceSortDateUtc = new ZDateTime(2025, 2, 2);

			workflow.PerformResponsiveAction(actionCode);

			AssertEquals("Should not update release sequence sort date", new ZDateTime(2025, 2, 2), workflow.FH_ReleaseSequenceSortDateUtc);
		}

		[TestDate(2023, 5, 15)]
		public void TestResponsiveLatestAcceptableReleaseDateUpdate_AXBAction()
		{
			AssertResponsivelyUpdatedLatestAcceptableReleaseDate(ProcessHeaderResponsiveActionConstants.UpdateAllExceptDedicatedBuffer);
		}

		[TestDate(2023, 5, 15)]
		public void TestResponsiveLatestAcceptableReleaseDateUpdate_BUFAction()
		{
			AssertNotResponsivelyUpdatedLatestAcceptableReleaseDate(ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer);
		}

		[TestDate(2023, 5, 15)]
		public void TestResponsiveLatestAcceptableReleaseDateUpdate_EBDAction()
		{
			AssertNotResponsivelyUpdatedLatestAcceptableReleaseDate(ProcessHeaderResponsiveActionConstants.UpdateEffectiveBranchAndDepartment);
		}

		[TestDate(2023, 5, 15)]
		public void TestResponsiveLatestAcceptableReleaseDateUpdate_BBDAction()
		{
			AssertNotResponsivelyUpdatedLatestAcceptableReleaseDate(ProcessHeaderResponsiveActionConstants.UpdateDedicatedBufferEffectiveBranchAndDepartment);
		}

		void AssertResponsivelyUpdatedLatestAcceptableReleaseDate(string actionCode)
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var buffer = BMSTestHelper.CreateBuffer(system);
			buffer.FC_BufferTimespanInMinutes = 960;
			buffer.FC_GB_AgingBranch = Env.CurrentBranch.PK;
			buffer.FC_GE_AgingDepartment = Env.CurrentDepartment.PK;
			var bucket = BMSTestHelper.CreateBucket(system, "Bucket");
			BMSTestHelper.LinkComponents(bucket, buffer);
			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", bucket);

			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			workflow.FH_GB_Branch = Env.CurrentBranch.PK;
			workflow.FH_GE_Department = Env.CurrentDepartment.PK;

			var nowUtc = ZDateTime.UtcNow;
			var deliveryDate = nowUtc.AddDays(10);
			workflow.FH_EffectiveAgreedDeliveryDateUtc = deliveryDate;
			workflow.FH_AgreedDeliveryDate = deliveryDate;

			var task1 = BMSTestHelper.CreateTask(workflow);
			task1.P9_EstimatedTimeToComplete = new ZInt(240).GetDateTimeFromMinutes();
			var task2 = BMSTestHelper.CreateTask(workflow);
			task2.P9_EstimatedTimeToComplete = new ZInt(480).GetDateTimeFromMinutes();
			Factory.Save();

			AssertEquals("Precondition", new ZDateTime(2023, 5, 19, 13, 0, 0), workflow.FH_LatestAcceptableReleaseDateUtc);

			workflow.FH_LatestAcceptableReleaseDateUtc = new ZDateTime(2025, 6, 3);

			workflow.PerformResponsiveAction(actionCode);

			AssertEquals("Should update latest acceptable release date", new ZDateTime(2023, 5, 19, 13, 0, 0), workflow.FH_LatestAcceptableReleaseDateUtc);
		}

		void AssertNotResponsivelyUpdatedLatestAcceptableReleaseDate(string actionCode)
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var buffer = BMSTestHelper.CreateBuffer(system);
			buffer.FC_BufferTimespanInMinutes = 960;
			buffer.FC_GB_AgingBranch = Env.CurrentBranch.PK;
			buffer.FC_GE_AgingDepartment = Env.CurrentDepartment.PK;
			var bucket = BMSTestHelper.CreateBucket(system, "Bucket");
			BMSTestHelper.LinkComponents(bucket, buffer);
			var job = Factory.New<DummyWithWorkflow>();
			var jobHeader = ProcessJobHeaderProvider.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1", bucket);

			workflow.FH_FC_DedicatedBuffer = buffer.PK;
			workflow.FH_GB_Branch = Env.CurrentBranch.PK;
			workflow.FH_GE_Department = Env.CurrentDepartment.PK;

			var nowUtc = ZDateTime.UtcNow;
			var deliveryDate = nowUtc.AddDays(10);
			workflow.FH_EffectiveAgreedDeliveryDateUtc = deliveryDate;
			workflow.FH_AgreedDeliveryDate = deliveryDate;

			var task1 = BMSTestHelper.CreateTask(workflow);
			task1.P9_EstimatedTimeToComplete = new ZInt(240).GetDateTimeFromMinutes();
			var task2 = BMSTestHelper.CreateTask(workflow);
			task2.P9_EstimatedTimeToComplete = new ZInt(480).GetDateTimeFromMinutes();
			Factory.Save();

			AssertEquals("Precondition", new ZDateTime(2023, 5, 19, 13, 0, 0), workflow.FH_LatestAcceptableReleaseDateUtc);

			workflow.FH_LatestAcceptableReleaseDateUtc = new ZDateTime(2025, 6, 3);

			workflow.PerformResponsiveAction(actionCode);

			AssertEquals("Should not update latest acceptable release date", new ZDateTime(2025, 6, 3), workflow.FH_LatestAcceptableReleaseDateUtc);
		}

		#endregion

		#region Fetch Strategy

		[ExpectNoExceptions]
		public void TestProcessHeaderFetchStrategy()
		{
			var header = Factory.NewWithValidTestData<ProcessHeader>();
			header.FH_FC_CurrentComponent = ZGuid.Empty;
			var strategy = new ProcessHeader.ProcessHeaderFetchStrategy(header);
			strategy.FetchForView(new TableColumn[]
			{
				new TableColumn(ProcessHeaderSchema.Constants.TableName, "EffectiveNudge"),
				new TableColumn(ProcessHeaderSchema.Constants.TableName, "CompletionMilestonePK"),
				new TableColumn(ProcessHeaderSchema.Constants.TableName, "CurrentComponentSystemPK"),
				new TableColumn(ProcessHeaderSchema.Constants.TableName, "BlueGreyShmack"),
			});
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var dummyWithWorkflow = Factory.New<DummyWithWorkflow>();
			var flowHeader = ProcessJobHeader.GetForParent(dummyWithWorkflow, Factory);
			return flowHeader.ProcessHeaders[0];
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var dummyWithWorkflow = Factory.New<DummyWithWorkflow>();
			var flowHeader = ProcessJobHeader.GetForParent(dummyWithWorkflow, Factory);
			return flowHeader.ProcessHeaders[0];
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var dummyWithWorkflow = factory.NewWithValidTestData<DummyWithWorkflow>();
			var flowHeader = ProcessJobHeader.GetForParent(dummyWithWorkflow, factory);
			return flowHeader.ProcessHeaders[0];
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();

			ObjectFactory.Substitute<IResponsiveManagementServiceTaskChecker>(new TestAuditETLServiceTaskChecker());
			auditEnabled = false;
		}

		#endregion

		#region Test Classes

		static bool auditEnabled;
		class TestAuditETLServiceTaskChecker : IResponsiveManagementServiceTaskChecker
		{
			public bool AreDependentServiceTasksActive() => auditEnabled;
		}

		#endregion
	}

	#region IProposedNetworkEntity

	class ProcessHeaderINetworkEntityTest : BMSTestCaseWithFactory
	{
		public void TestImplicitDurationMinutes()
		{
			BMSTestHelper.MakeCompletionStatementTaskType("ORG", "COM");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task1 = BMSTestHelper.CreateTask(workflow, string.Empty, 60);
			var task2 = BMSTestHelper.CreateTask(workflow, string.Empty, 60);

			var completionStatement = workflow.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();
			completionStatement.P9_EstDuration = new ZInt(10).GetDateTimeFromMinutes();

			var expectedImplicitDurationHours = task1.StandardEstimateHours + task2.StandardEstimateHours + completionStatement.StandardEstimateHours;
			AssertEquals(expectedImplicitDurationHours, workflow.ImplicitDurationHours);
		}

		public void TestCompletionStatements()
		{
			BMSTestHelper.MakeCompletionStatementTaskType("ORG", "COM");

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var cs1 = workflow.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();
			var cs2 = workflow.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();
			var cs3 = workflow.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();
			var cs4 = workflow.CompletionStatementTasksIncludingChildWorkflowTasks.AddNew();

			cs1.P9_NotesAsString = "Do Thing 1";
			cs2.P9_NotesAsString = "Do Thing 2";
			cs3.P9_NotesAsString = "Do Thing 3";
			cs4.P9_NotesAsString = "Do Thing 4";

			AssertMultilineASCIIEquals("",
	@"Do Thing 1
Do Thing 2
Do Thing 3
Do Thing 4", ((IProposedNetworkEntity)workflow).CompletionCriteria);

			((IProposedNetworkEntity)workflow).CompletionCriteria =
	@"Do Thing 1
Do Thing 2
Do Thing 4";

			AssertEquals("Do Thing 1", cs1.P9_NotesAsString);
			AssertEquals("Do Thing 2", cs2.P9_NotesAsString);
			AssertEquals("Do Thing 4", cs3.P9_NotesAsString);
			AssertEquals(true, cs4.IsDeleted);

			((IProposedNetworkEntity)workflow).CompletionCriteria =
	@"Do Thing 1
Do Thing 2
Do Thing 4
Do One More Thing";

			AssertEquals("Do Thing 1", cs1.P9_NotesAsString);
			AssertEquals("Do Thing 2", cs2.P9_NotesAsString);
			AssertEquals("Do Thing 4", cs3.P9_NotesAsString);

			AssertEquals(4, workflow.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals("Do One More Thing", workflow.CompletionStatementTasksIncludingChildWorkflowTasks[3].P9_NotesAsString);
		}

		public void TestJobName()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "MAIORGSYD";

			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var entity = (IProposedNetworkEntity)jobHeader;

			AssertEquals("MAIORGSYD", entity.JobName);

			entity.JobName = "URORGSYD";
			AssertEquals("URORGSYD", org.OH_FullName);
		}

		public void TestJobName_WhenEnteredTextIsTooLong_ShouldTruncate()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "MAIORGSYD";

			var jobHeader = ProcessJobHeader.GetForParent(org, Factory);
			var entity = (IProposedNetworkEntity)jobHeader;

			AssertEquals("MAIORGSYD", entity.JobName);

			var tooLongText = new string('W', OrgHeaderSchema.OH_FullName.MaxLength + 1);
			var justLongEnoughText = new string('W', OrgHeaderSchema.OH_FullName.MaxLength);

			entity.JobName = tooLongText;
			AssertEquals(justLongEnoughText, org.OH_FullName);
		}

		public void TestJobName_NoParent()
		{
			var workflow = Factory.NewWithValidTestData<ProcessHeader>();
			var entity = (IProposedNetworkEntity)workflow;
			entity.JobName = "Blah blah";
			AssertEquals(string.Empty, entity.JobName);
		}

		public void TestStatus_ShouldIncludeCompletionStatements()
		{
			MakeCompletionStatementTaskType("ORG", "COM");

			var workflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			var completionStatement = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 0, taskType: "COM");

			var entity = (IProposedNetworkEntity)workflow;

			AssertEquals(WorkStatus.Startable, entity.Status);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(WorkStatus.Startable, entity.Status);

			completionStatement.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(WorkStatus.Complete, entity.Status);

			completionStatement.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(WorkStatus.Startable, entity.Status);

			completionStatement.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(WorkStatus.Cancelled, entity.Status);

			completionStatement.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals(WorkStatus.Working, entity.Status);

			completionStatement.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertEquals(WorkStatus.Suspended, entity.Status);

			var prereqWorkflow = CreateJobHeader<OrgHeader>().ProcessHeaders[0];
			prereqWorkflow.MakePrerequisiteOf(workflow);
			var prereqTask = CreateTask(prereqWorkflow, GlbStaff.CurrentUser.GS_Code, 60);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			completionStatement.P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			AssertEquals(WorkStatus.Blocked, entity.Status);

			prereqTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(WorkStatus.Startable, entity.Status);
		}

		public void TestStatus_ForJobHeader()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");
			workflow1.GetOrCreateDependencyLink(workflow2);

			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			AssertEquals(WorkStatus.Startable, ((IProposedNetworkEntity)jobHeader).Status);
			AssertEquals(WorkStatus.Startable, ((IProposedNetworkEntity)workflow1).Status);
			AssertEquals(WorkStatus.Blocked, ((IProposedNetworkEntity)workflow2).Status);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			AssertEquals(WorkStatus.Startable, ((IProposedNetworkEntity)jobHeader).Status);
			AssertEquals(WorkStatus.Complete, ((IProposedNetworkEntity)workflow1).Status);
			AssertEquals(WorkStatus.Startable, ((IProposedNetworkEntity)workflow2).Status);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			AssertEquals(WorkStatus.Complete, ((IProposedNetworkEntity)jobHeader).Status);
			AssertEquals(WorkStatus.Complete, ((IProposedNetworkEntity)workflow1).Status);
			AssertEquals(WorkStatus.Complete, ((IProposedNetworkEntity)workflow2).Status);
		}

		public void TestStatus_ForJobHeader_ExcludesNonTasks()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "workflow1");
			var task = CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			var milestone = jobHeader.Parent.WorkflowItems.Milestones.AddNew();
			var trigger = jobHeader.Parent.WorkflowItems.Triggers.AddNew();
			var exception = jobHeader.Parent.WorkflowItems.Exceptions.AddNew();

			AssertEquals(WorkStatus.Startable, ((IProposedNetworkEntity)jobHeader).Status);
			AssertEquals(WorkStatus.Startable, ((IProposedNetworkEntity)workflow).Status);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			AssertEquals(WorkStatus.Cancelled, ((IProposedNetworkEntity)jobHeader).Status);
			AssertEquals(WorkStatus.Cancelled, ((IProposedNetworkEntity)workflow).Status);
		}

		public void TestEstimateSummary()
		{
			var workflow = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory).ProcessHeaders[0];
			var task1 = CreateTask(workflow, string.Empty, 60);
			var task2 = CreateTask(workflow, string.Empty, 30);

			var entity = (IProposedNetworkEntity)workflow;
			AssertEquals("1.5 hrs to 3.0 hrs (2.2 standard estimate).", entity.EstimateSummary);
		}

		public void TestName()
		{
			var workflow = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory).ProcessHeaders.AddNew();
			workflow.FH_CompletionStatement = "Workflow 1";

			var entity = (IProposedNetworkEntity)workflow;
			AssertEquals("Workflow 1", entity.Name);

			entity.Name = "Workflow 2";
			AssertEquals("Workflow 2", workflow.FH_CompletionStatement);
			AssertEquals("We need this property declared as public so that WPF can bind to it, and the designer doesn't die", "Workflow 2", workflow.Name);
		}

		public void TestPreReqDepth()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			workflow1.MakePrerequisiteOf(workflow2);
			workflow2.MakePrerequisiteOf(workflow3);

			var entity1 = (IProposedNetworkEntity)workflow1;
			var entity2 = (IProposedNetworkEntity)workflow2;
			var entity3 = (IProposedNetworkEntity)workflow3;

			AssertEquals(0, entity1.GetPreRequisiteDepth());
			AssertEquals(1, entity2.GetPreRequisiteDepth());
			AssertEquals(2, entity3.GetPreRequisiteDepth());
		}

		public void TestStatus_Startable()
		{
			var workflow = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory).ProcessHeaders[0];
			var task = workflow.TaskCollection.AddNew();

			var entity = (IProposedNetworkEntity)workflow;
			AssertEquals(WorkStatus.Startable, entity.Status);
			AssertEquals("This Workflow is ready to start", entity.StatusDescription);
		}

		public void TestStatus_Blocked()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow1.MakePrerequisiteOf(workflow2);

			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 0);
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 0);

			var entity = (IProposedNetworkEntity)workflow2;
			AssertEquals(WorkStatus.Blocked, entity.Status);
			AssertEquals("Has incomplete pre-requisites", entity.StatusName);
			AssertEquals(string.Empty, entity.StatusDescription);
		}

		public void TestStatus_Working()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task1 = workflow.TaskCollection.AddNew();
			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			var task2 = workflow.TaskCollection.AddNew();
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			var entity = (IProposedNetworkEntity)workflow;
			AssertEquals(WorkStatus.Working, entity.Status);
			AssertEquals("Working", entity.StatusName);
			AssertEquals("This Workflow has one or more working tasks", entity.StatusDescription);
		}

		public void TestStatus_Suspended()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task = workflow.TaskCollection.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;

			var entity = (IProposedNetworkEntity)workflow;
			AssertEquals(WorkStatus.Suspended, entity.Status);
			AssertEquals("Suspended", entity.StatusName);
			AssertEquals("This Workflow has one or more suspended tasks", entity.StatusDescription);
		}

		public void TestStatus_Complete_ForLinkedItemWithNoTasks()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory);
			var workflow = jobHeader.ProcessHeaders[0];

			var entity = (IProposedNetworkEntity)workflow;
			AssertEquals(WorkStatus.Complete, entity.Status);

			var task1 = workflow.TaskCollection.AddNew();
			var task2 = workflow.TaskCollection.AddNew();
			AssertEquals(WorkStatus.Startable, entity.Status);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(WorkStatus.Complete, entity.Status);
			AssertEquals("Complete", entity.StatusName);
			AssertEquals("This Workflow is complete", entity.StatusDescription);
		}

		public void TestStatus_None()
		{
			var entity = (IProposedNetworkEntity)Factory.NewWithValidTestData<ProcessHeader>();
			AssertEquals(WorkStatus.None, entity.Status);
		}

		public void TestStatus_Cancelled()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];
			var task = workflow.TaskCollection.AddNew();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;

			var entity = (IProposedNetworkEntity)workflow;
			AssertEquals(WorkStatus.Cancelled, entity.Status);
			AssertEquals("Cancelled", entity.StatusName);
			AssertEquals("This Workflow has been canceled", entity.StatusDescription);
		}

		public void TestStatus_ShouldNotAccessTasksWhenAvoidable()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var task1_1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60);
			var task2_1 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);
			var task2_2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60);

			workflow1.GetOrCreateDependencyLink(workflow2);

			Factory.Save();

			ReloadAndAssertProcessTasksHits(workflow2.PK, WorkStatus.Blocked, 0);
			ReloadAndAssertProcessTasksHits(workflow2.PK, false, 0);

			task1_1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			ReloadAndAssertProcessTasksHits(workflow2.PK, WorkStatus.Startable, 1);
			ReloadAndAssertProcessTasksHits(workflow2.PK, false, 0);

			task2_1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task2_2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			ReloadAndAssertProcessTasksHits(workflow2.PK, WorkStatus.Complete, 1);
			ReloadAndAssertProcessTasksHits(workflow2.PK, true, 0);

			task2_1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			task2_2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			Factory.Save();

			ReloadAndAssertProcessTasksHits(workflow2.PK, WorkStatus.Cancelled, 1);
			ReloadAndAssertProcessTasksHits(workflow2.PK, true, 0);

			task2_1.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			Factory.Save();

			ReloadAndAssertProcessTasksHits(workflow2.PK, WorkStatus.Working, 1);
			ReloadAndAssertProcessTasksHits(workflow2.PK, false, 0);

			task2_1.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			Factory.Save();

			ReloadAndAssertProcessTasksHits(workflow2.PK, WorkStatus.Suspended, 1);
			ReloadAndAssertProcessTasksHits(workflow2.PK, false, 0);
		}

		void ReloadAndAssertProcessTasksHits(ZGuid workflowPK, WorkStatus expectedStatus, int expectedProcessTasksHits)
		{
			var factory = new BusinessObjectFactory();
			var workflow = factory.Load<ProcessHeader>(workflowPK);

			AssertEquals(0, factory.GetTableHitCount(ProcessTasksSchema.Constants.TableName));

			AssertEquals(expectedStatus, ((IProposedNetworkEntity)workflow).Status);
			AssertEquals("Expected db hits when WorkStatus is " + expectedStatus, expectedProcessTasksHits, factory.GetTableHitCount(ProcessTasksSchema.Constants.TableName));
		}

		void ReloadAndAssertProcessTasksHits(ZGuid workflowPK, bool expectedIsClosed, int expectedProcessTasksHits)
		{
			var factory = new BusinessObjectFactory();
			var workflow = factory.Load<ProcessHeader>(workflowPK);

			AssertEquals(0, factory.GetTableHitCount(ProcessTasksSchema.Constants.TableName));

			AssertEquals(expectedIsClosed, workflow.IsClosed);
			AssertEquals("Expected db hits when IsClosed is " + expectedIsClosed, expectedProcessTasksHits, factory.GetTableHitCount(ProcessTasksSchema.Constants.TableName));
		}

		public void TestPrePostRequisiteLinks()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			workflow1.MakePrerequisiteOf(workflow2);
			workflow1.MakePrerequisiteOf(workflow3);
			workflow2.MakePrerequisiteOf(workflow3);

			var entity1 = (IProposedNetworkEntity)workflow1;
			var entity2 = (IProposedNetworkEntity)workflow2;
			var entity3 = (IProposedNetworkEntity)workflow3;

			AssertEquals(0, entity1.PreRequisiteLinks.Count());
			AssertEquals(2, entity1.PostRequisiteLinks.Count());
			AssertEquals(workflow1, entity1.PostRequisiteLinks.ElementAt(0).From);
			AssertEquals(workflow2, entity1.PostRequisiteLinks.ElementAt(0).To);
			AssertEquals(workflow1, entity1.PostRequisiteLinks.ElementAt(1).From);
			AssertEquals(workflow3, entity1.PostRequisiteLinks.ElementAt(1).To);

			AssertEquals(1, entity2.PreRequisiteLinks.Count());
			AssertEquals(1, entity2.PostRequisiteLinks.Count());
			AssertEquals(workflow1, entity2.PreRequisiteLinks.ElementAt(0).From);
			AssertEquals(workflow2, entity2.PreRequisiteLinks.ElementAt(0).To);
			AssertEquals(workflow2, entity2.PostRequisiteLinks.ElementAt(0).From);
			AssertEquals(workflow3, entity2.PostRequisiteLinks.ElementAt(0).To);

			AssertEquals(2, entity3.PreRequisiteLinks.Count());
			AssertEquals(0, entity3.PostRequisiteLinks.Count());
			AssertEquals(workflow1, entity3.PreRequisiteLinks.ElementAt(0).From);
			AssertEquals(workflow3, entity3.PreRequisiteLinks.ElementAt(0).To);
			AssertEquals(workflow2, entity3.PreRequisiteLinks.ElementAt(1).From);
			AssertEquals(workflow3, entity3.PreRequisiteLinks.ElementAt(1).To);
		}

		public void TestIsStartable()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.New<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow1.MakePrerequisiteOf(workflow2);

			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 0);
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 0);

			var entity = (IProposedNetworkEntity)workflow2;
			AssertEquals(false, entity.IsStartable);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(true, entity.IsStartable);

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(true, entity.IsStartable);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(false, entity.IsStartable);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			AssertEquals(false, entity.IsStartable);
		}

		#region IsOnCriticalPath

		public void TestIsOnCriticalPath_CircularDependency()
		{
			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "workflow1";
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";

			workflow1.MakePrerequisiteOfAllowingReverseRelationship_ForTest(workflow2);
			workflow2.MakePrerequisiteOfAllowingReverseRelationship_ForTest(workflow1); // Circular dependency

			CombineAssertions("Preconditions", () =>
			{
				Assert("workflow1", workflow1.IsPostrequisiteOf(workflow2));
				Assert("workflow2", workflow2.IsPostrequisiteOf(workflow1));
				AssertEquals("The graph should have the cycle", false, jobHeader.DependencyGraph.IsDirectedAcyclicGraph());
			});

			var task1 = workflow1.Parent.WorkflowItems.AddNew();
			task1.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();
			task1.P9_FH_ProcessHeader = workflow1.PK;

			var task2 = workflow2.Parent.WorkflowItems.AddNew();
			task2.P9_EstDuration = new ZInt(60).GetDateTimeFromMinutes();
			task2.P9_FH_ProcessHeader = workflow2.PK;

			Factory.Save();

			AssertEquals(false, ((IProposedNetworkEntity)workflow1).IsOnCriticalPath);
			AssertEquals(false, ((IProposedNetworkEntity)workflow2).IsOnCriticalPath);
		}

		#endregion
	}

	#endregion

	#region UniversalCopy

	class ProcessHeaderUniversalCopyTest : BMSTestCaseWithFactory
	{
		[TestDate(2013, 09, 11, 10, 00, 00)]
		public void TestDefaultTemplate_ShouldCopyTags()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Octomum", releaseGroupPK: config.ReleaseGroup.PK);
			workflow.AddTag(config.DerpyHoovesTag);

			var clone = CopyUniversallyWithDefaultTemplate(workflow);
			AssertTagApplied(clone, config.DerpyHoovesTag);
		}

		[TestDate(2017, 09, 10, 12, 00, 00)]
		public void TestDefaultTemplate_ShouldNotCopyRULTags()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Octomum", releaseGroupPK: config.ReleaseGroup.PK);
			var ruleTag = workflow.AddTag(config.RuleTag).Link;
			workflow.AddTag(config.RedTag);

			((TagLink)ruleTag).AreOnSavingChecksDisabledForTesting = true;

			var clone = CopyUniversallyWithDefaultTemplate(workflow);
			AssertTagApplied(clone, config.RedTag);
			AssertTagNotApplied(clone, config.RuleTag);
		}

		[TestDate(2017, 09, 10, 12, 00, 00)]
		public void TestDefaultTemplate_TagsAreNotBeingCached()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Octomum", releaseGroupPK: config.ReleaseGroup.PK);
			var ruleTag = workflow.AddTag(config.RuleTag).Link;
			workflow.AddTag(config.RedTag);

			((TagLink)ruleTag).AreOnSavingChecksDisabledForTesting = true;

			var clone = CopyUniversallyWithDefaultTemplate(workflow);

			clone.AddTag(config.PlatinumTag);
			clone.AddTag(config.PrincessLunaTag);

			clone = CopyUniversallyWithDefaultTemplate(clone);

			AssertTagApplied(clone, config.RedTag);
			AssertTagNotApplied(clone, config.RuleTag);
			AssertTagApplied(clone, config.PlatinumTag);
			AssertTagApplied(clone, config.PrincessLunaTag);
		}

		[TestDate(2013, 09, 11, 10, 00, 00)]
		public void TestDefaultTemplate_ForItemInWorkQueue_ShouldAddToEndOfQueue()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			config.System.FS_Name = "FIXIT";
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Octomum", releaseGroupPK: config.ReleaseGroup.PK);
			var link = (TagLink)workflow.AddTag(queue).Link;

			link.TGL_Sequence = 10;

			var clone = CopyUniversallyWithDefaultTemplate(workflow);
			AssertTagApplied(clone, queue, sequence: 11);
		}

		[TestDate(2013, 09, 11, 10, 00, 00)]
		public void TestHasGlowInterfaceReference()
		{
			var glowInterface = GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(ProcessHeader), true);
			AssertNotNull(glowInterface);
			var processTaskInterface = GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(ProcessTask), true);
			AssertNotNull(processTaskInterface);
		}

		[TestDate(2013, 09, 11, 10, 00, 00)]
		public void TestWorkflowRecurrenceCopyTemplate()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			var job = Factory.NewWithValidTestData<OrgHeader>();

			var workflow = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders[0];
			workflow.FH_GG_ReleaseGroup = releaseGroup.PK;
			workflow.FH_DateAcceptability = DateAcceptabilityList.Codes.GraduatedStartSharpFinish;
			workflow.FH_CompletionStatement = "Test Workflow";

			var assignedUser = Factory.NewWithValidTestData<GlbStaff>();
			var assignedGroup = Factory.NewWithValidTestData<GlbGroup>();
			var requiredCapability = Factory.NewWithValidTestData<GlbCapability>();

			var task1 = job.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow.PK;
			task1.P9_Description = "Test Task 1";
			task1.P9_Type = "XXX";
			task1.P9_GS_NKAssignedStaffMember = assignedUser.GS_Code;
			task1.P9_GG_AssignedGroup = assignedGroup.PK;
			task1.P9_G4_RequiredCapability = requiredCapability.PK;
			task1.P9_EstDuration = new ZDateTime(2013, 1, 1, 4, 30, 0);
			task1.P9_EstimateVariationFactor = 4;
			task1.P9_Sequence = 5;

			var task2 = job.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = workflow.PK;
			task2.P9_Description = "Test Task 2";
			task2.P9_Type = "YYY";
			task2.P9_Sequence = 10;
			task2.P9_GS_NKAssignedStaffMember = ZString.Empty;
			task2.P9_GG_AssignedGroup = ZGuid.Empty;

			Factory.Save();

			var workflowCopy = CopyUniversallyWithDefaultTemplate(workflow);

			CombineAssertions("GIVEN test-workflow with 2 tasks, WHEN executing StmUniversalCopyScheduleTask.Run(test-workflow) SHOULD create new-workflow with same tasks", () =>
			{
				AssertEquals(releaseGroup.PK, workflowCopy.FH_GG_ReleaseGroup);
				AssertEquals(DateAcceptabilityList.Codes.GraduatedStartSharpFinish, workflowCopy.FH_DateAcceptability);
				AssertEquals("Test Workflow (11-Sep-13 10:00)", workflowCopy.FH_CompletionStatement);
				AssertEquals(4, job.WorkflowItems.Count);
				AssertEquals(2, workflowCopy.Tasks.Count());

				AssertContainsExactElementsInAnyOrder(
					"New-workflow should contains the exact tasks in test-workflow regardless of the P9_Sequence",
					new ZString[] { "Test Task 2", "Test Task 1" },
					workflowCopy.Tasks.Select(t => t.P9_Description));

				var testTask1 = workflowCopy.Tasks.First(item => item.P9_Description == "Test Task 1");
				AssertEquals("XXX", testTask1.P9_Type);
				AssertEquals("ASN", testTask1.P9_Status);
				AssertEquals(assignedUser.GS_Code, testTask1.P9_GS_NKAssignedStaffMember);
				AssertEquals(assignedGroup.PK, testTask1.P9_GG_AssignedGroup);
				AssertEquals(requiredCapability.PK, testTask1.P9_G4_RequiredCapability);
				AssertEquals((ZDateTime)new TimeSpan(4, 30, 0), testTask1.P9_EstDuration);
				AssertEquals((ZDecimal)4, testTask1.P9_EstimateVariationFactor);
				AssertEquals(11, testTask1.P9_Sequence);

				var testTask2 = workflowCopy.Tasks.First(item => item.P9_Description == "Test Task 2");
				AssertEquals("OPN", testTask2.P9_Status);
				AssertEquals(12, testTask2.P9_Sequence);
			});
		}

		[TestDate(2013, 09, 11, 10, 00, 00)]
		public void TestAfterCopy_ShouldSetCurrentComponent()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var bucket1 = system.Components.AddNew();
			var bucket2 = system.Components.AddNew();

			var link = bucket1.FromMeToOthersLinks.AddNew();
			link.FL_FC_ComponentTo = bucket2.PK;

			var jobHeader = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			workflow.FH_FC_CurrentComponent = ZGuid.Empty;

			AssertAfterCopyMethodCalledCondition(workflow, () => AssertEquals(bucket1.PK, workflow.FH_FC_CurrentComponent));
		}

		[TestDate(2013, 09, 11, 10, 00, 00)]
		public void TestIUniversalCopySelectivelySupportableMembers()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Donary Clintump");

			AssertEquals(false, ((IUniversalCopySelectivelySupportable)jobHeader).SupportsUniversalCopy);
			AssertEquals("There can only be one job-level workflow per job.", ((IUniversalCopySelectivelySupportable)jobHeader).ReasonForNotSupportingUniversalCopy);

			AssertNull(workflow as IUniversalCopySelectivelySupportable);
		}

		public void TestPerformUniversalCopy_WhenCopyTemplateDoesNotIncludeParentHeader_ShouldCopyParentHeaderAnyway()
		{
			var copyTemplate = CreateCopyTemplateForColumnNames(nameof(ProcessHeader.FH_CompletionStatement));
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Hey hey! Ho ho! I want my ge-la-tissi-mo!");

			AssertEquals("ORG", workflow.FH_WorkflowType);

			var workflowCopy = CopyUniversally(workflow, copyTemplate);

			AssertEquals("Hey hey! Ho ho! I want my ge-la-tissi-mo! [1]", workflowCopy.FH_CompletionStatement);
			AssertEquals(workflow.JobHeader, workflowCopy.JobHeader);
			AssertEquals("ORG", workflowCopy.FH_WorkflowType);
			AssertEquals(workflow.Parent, workflowCopy.Parent);

			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestPerformUniversalCopy_WhenCopyTemplateDoesNotIncludeParentHeader_ShouldNotThrowException()
		{
			var copyTemplateTree = CreateCopyTemplateForColumnNames(nameof(ProcessHeader.FH_CompletionStatement));

			var workflow = BMSTestHelper.CreateWorkflow(Factory, "Hey hey! Ho ho! Tired old chants have got to go!");

			var templateFilter = Factory.New<UniversalCopyTemplate>();
			templateFilter.S9_ModuleID = "ProcessHeader_UC";
			templateFilter.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			templateFilter.CopyTemplateTree = new CopyTemplateTreeBizo(copyTemplateTree, templateFilter);
			templateFilter.S9_IsPublished = false;

			var copyScheduleTask = Factory.New<StmUniversalCopyScheduleTask>();
			copyScheduleTask.Parent.CopyObject = workflow;
			copyScheduleTask.Parent.SUC_S9_CopyTemplate = templateFilter.PK;

			Factory.Save();

			copyScheduleTask.RunPreSaveValidation();

			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestPerformUniversalCopy_FromOldJobToNewJob_EnsureCopiedWorkflowsAndTasksReferenceNewJobAndJobLevelWorkflow()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var copyTemplate = GetTestUniversalCopyTemplate();

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Hey hey! Ho ho! Digging tools have got to go!", buffer);
			var task = BMSTestHelper.CreateTask(workflow, description: "Flower power");

			Factory.Save();

			var copiedJob = new BusinessObjectCopyManager().Copy(jobHeader.Parent, copyTemplate).Object as OrgHeader;
			AssertNotEquals(jobHeader.Parent.PK, copiedJob.PK);

			var workflowQuery = new ZQuery(ProcessHeaderSchema.FH_ParentId, copiedJob.PK);
			workflowQuery.AddToFilter(ProcessHeaderSchema.FH_ParentTableCode, OrgHeaderSchema.Constants.Prefix);
			var copiedWorkflows = Factory.Load<ProcessHeader>(workflowQuery);
			AssertEquals(2, copiedWorkflows.Length);

			var copiedJobHeader = copiedWorkflows.SingleOrDefault(w => w.FH_Category == "JOB");
			var copiedWorkflow = copiedWorkflows.SingleOrDefault(w => w.FH_CompletionStatement.StartsWith(workflow.FH_CompletionStatement));
			var copiedTask = copiedWorkflow.TaskCollection.SingleOrDefault() as ProcessTask;

			CombineAssertions(() =>
			{
				AssertEquals(false, object.ReferenceEquals(job, copiedJob));
				AssertEquals(false, object.ReferenceEquals(jobHeader, copiedJobHeader));
				AssertEquals(false, object.ReferenceEquals(workflow, copiedWorkflow));
				AssertEquals(false, object.ReferenceEquals(task, copiedTask));

				AssertEquals("Original task should be linked to original job", task.P9_ParentID, job.PK);
				AssertEquals("Original task should be linked to original workflow", task.P9_FH_ProcessHeader, workflow.PK);
				AssertEquals("Original workflow should be linked to original job", workflow.FH_ParentId, job.PK);
				AssertEquals("Original workflow should be linked to original job-level workflow", workflow.FH_FH_ParentHeader, jobHeader.PK);
				AssertEquals("Original job-level workflow should be linked to original job", jobHeader.FH_ParentId, job.PK);
				AssertEquals("Original job-level workflow should not be linked to another workflow", jobHeader.FH_FH_ParentHeader, ZGuid.Empty);
				AssertEquals("Job Organization (XVBQP68SIYXQ) is complete.", jobHeader.FH_CompletionStatement);

				AssertEquals("Copied task should be linked to copied job", copiedTask.P9_ParentID, copiedJob.PK);
				AssertEquals("Copied task should be linked to copied workflow", copiedTask.P9_FH_ProcessHeader, copiedWorkflow.PK);
				AssertEquals("Copied workflow should be linked to copied job", copiedWorkflow.FH_ParentId, copiedJob.PK);
				AssertEquals("Copied workflow should be linked to copied job-level workflow", copiedWorkflow.FH_FH_ParentHeader, copiedJobHeader.PK);
				AssertEquals("Copied job-level workflow should be linked to copied job", copiedJobHeader.FH_ParentId, copiedJob.PK);
				AssertEquals("Copied job-level workflow should not be linked to another workflow", copiedJobHeader.FH_FH_ParentHeader, ZGuid.Empty);
				AssertEquals("Job Organization (XVBQP68SIYXQ) is complete.", copiedJobHeader.FH_CompletionStatement);
			});

			copiedJob.OH_Code = "MAIFRSORG";
			AssertNoExceptionThrown(Factory.Save);
		}

		CopyTemplateTree GetTestUniversalCopyTemplate()
		{
			#region XML
			var xmlString = @"﻿<?xml version='1.0' encoding='utf-8'?>
<CopyTemplateTree xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' N='OrgHeader' ConfigurationSource='SLT' NominatedRecordPk='00000000-0000-0000-0000-000000000000' Active='true'>
<E N='OrgHeader'>
<C N='ProcessHeaders' ItemPropertyName='FH_ParentId' ItemParentTablePropertyName='FH_ParentTableCode' ItemTableName='ProcessHeader' Do='All' IsSplitCollection='false' Order='0'>
<E N='ProcessHeader`1'>
<P N='FH_GG_ReleaseGroup' Do='Copy' />
<P N='FH_CompletionStatement' Do='Copy' />
<P N='FH_FC_CurrentComponent' Do='Copy' />
<P N='FH_AgreedDeliveryDate' Do='Copy' />
<P N='FH_AgreedDeliveryDateDefaultHoursOffset' Do='Copy' />
<P N='FH_AgreedDeliveryDateDefaultsFrom' Do='Copy' />
<P N='FH_AllowTaskAutoAssignment' Do='Copy' />
<P N='FH_BufferPenetrationPercentWhenCompleted' Do='Copy' />
<P N='FH_Category' Do='Copy' />
<P N='FH_DateAcceptability' Do='Copy' />
<P N='FH_DoNotStartBeforeDate' Do='Copy' />
<P N='FH_EarliestStartDateDefaultsFrom' Do='Copy' />
<P N='FH_EarliestStartDefaultHoursOffset' Do='Copy' />
<P N='FH_IsCriticalHandover' Do='Copy' />
<P N='FH_IsReleasableUnitParent' Do='Copy' />
<P N='FH_IsReleaseGroupSetByTemplate' Do='Copy' />
<P N='FH_IsStandby' Do='Copy' />
<P N='FH_LastTransferType' Do='Copy' />
<P N='FH_ParentTemplateId' Do='Copy' />
<P N='FH_PlannedDurationInMinutes' Do='Copy' />
<P N='FH_ReleaseDateTime' Do='Copy' />
<P N='FH_StaggeredReleaseDelayExpiry' Do='Copy' />
<P N='FH_Status' Do='Copy' />
<P N='FH_TaskPenetrationResetDateTimeUtc' Do='Copy' />
<P N='FH_TimeDelayFactor' Do='Copy' />
<P N='FH_TimeDelayMinutes' Do='Copy' />
<P N='FH_VoteUpDownAmount' Do='Copy' />
</E>
</C>
<C N='ProcessTasks' ItemPropertyName='P9_ParentID' ItemParentTablePropertyName='P9_ParentTableCode' ItemTableName='ProcessTasks' Do='All' IsSplitCollection='false' Order='0'>
<E N='ProcessTask`1'><P N='P9_GS_NKAssignedStaffMember' Do='Copy' />
<P N='P9_G4_RequiredCapability' Do='Copy' />
<P N='P9_GG_AssignedGroup' Do='Copy' />
<P N='P9_Description' Do='Copy' />
<P N='P9_Sequence' Do='Copy' />
<P N='P9_Type' Do='Copy' />
<R N='ProcessHeader' RelatedPropertyName='P9_FH_ProcessHeader' RelatedEntityTableName='ProcessHeader' Do='LinkCopied'><E N='ProcessHeader'></E></R>
</E>
</C>
</E>
</CopyTemplateTree>";
			#endregion

			using (var serializedCopyTemplateStream = new MemoryStream())
			using (var sw = new StreamWriter(serializedCopyTemplateStream))
			{
				sw.Write(xmlString, new UnicodeEncoding());
				sw.Flush();
				serializedCopyTemplateStream.Seek(0, SeekOrigin.Begin);
				return CopyTemplateTree.Deserialize(serializedCopyTemplateStream);
			}
		}

		static List<Tuple<string[], string, string>> CollidingCompletionStatementsTestData
		{
			get
			{
				return new List<Tuple<string[], string, string>>()
				{
					Tuple.Create(new [] { "A", "B", "C" }, "A", "A [1]")
					,Tuple.Create(new [] { "A", "B", "C" }, "B", "B [1]")
					,Tuple.Create(new [] { "A", "B", "C" }, "C", "C [1]")
					,Tuple.Create(new [] { "A [1]", "A [2]", "B", "C" }, "A [1]", "A [3]")
					,Tuple.Create(new [] { "A", "A [2]", "B", "C" }, "A", "A [1]")
					,Tuple.Create(new [] { "A", "A [2]", "B", "C" }, "A [2]", "A [3]")
				};
			}
		}

		public void TestUniversalCopy_DoesNotCauseCompletionStatementDuplication()
		{
			CombineAssertions(() =>
			{
				foreach (var test in CollidingCompletionStatementsTestData)
				{
					var strings = test.Item1;
					var collidingString = test.Item2;
					var expected = test.Item3;

					var workflowToCopy = CreateTestJobWithWorkflows(strings).ProcessHeaders.First(header => header.FH_CompletionStatement == collidingString);

					var template = CreateCopyTemplateForWorkflowCompletionStatement();
					var workflowCopy = CopyUniversally(workflowToCopy, template);

					AssertEquals("Universal copy should not duplicate Completion Statements", expected, workflowCopy.FH_CompletionStatement);
				}
			});
		}

		public void TestUniversalCopy_DoesNotCauseCompletionStatementDuplication_WithNestedWorkflows()
		{
			CombineAssertions(() =>
			{
				foreach (var test in CollidingCompletionStatementsTestData)
				{
					var strings = test.Item1;
					var collidingString = test.Item2;
					var expected = test.Item3;

					var job = CreateTestJobWithWorkflows(strings);
					var parentWorkflow = BMSTestHelper.CreateWorkflow(job, "parent");
					var workflowToCopy = job.ProcessHeaders.First(header => header.FH_CompletionStatement == collidingString);
					BMSTestHelper.MakeChildOf(workflowToCopy, parentWorkflow);

					CopyTemplateTree template = CreateCopyTemplateForWorkflowCompletionStatement();

					var workflowCopy = CopyUniversally(workflowToCopy, template);

					AssertEquals("Universal copy should not duplicate Completion Statements", expected, workflowCopy.FH_CompletionStatement);
				}
			});
		}

		static CopyTemplateTree CreateCopyTemplateForWorkflowCompletionStatement()
		{
			return CreateCopyTemplateForColumnNames(nameof(ProcessHeader.FH_FH_ParentHeader), nameof(ProcessHeader.FH_CompletionStatement));
		}

		static CopyTemplateTree CreateCopyTemplateForColumnNames(params string[] columnNames)
		{
			var template = new CopyTemplateTree();
			var node = new EntityCopyTemplateNode();

			foreach (var column in columnNames)
			{
				node.Nodes.Add(new PropertyCopyTemplateNode() { Name = column, CopyMethod = CopyMethod.Copy });
			}

			template.InnerNode = node;

			return template;
		}

		ProcessJobHeader CreateTestJobWithWorkflows(string[] completionStatements)
		{
			var releaseGroup = Factory.NewWithValidTestData<GlbGroup>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var job = ProcessJobHeader.GetForParent(orgHeader, Factory);

			foreach (string completionStatement in completionStatements)
			{
				BMSTestHelper.CreateWorkflow(job, completionStatement);
			}
			return job;
		}

		#region Implementation

		static void AssertAfterCopyMethodCalledCondition(ProcessHeader processHeader, Action assertAction)
		{
			var attribute = typeof(ProcessHeader).GetCustomAttributes(true).OfType<UniversalCopyWithExtendedEntitiesAttribute>().Single();
			var method = typeof(ProcessHeader).GetMethod(attribute.FinishCopyMethod, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			method.Invoke(processHeader, null);

			assertAction();
		}

		static ProcessHeader CopyUniversally(ProcessHeader workflow, CopyTemplateTree copyTemplate)
		{
			return (ProcessHeader)new BusinessObjectCopyManager().Copy(workflow, copyTemplate).Object;
		}

		static ProcessHeader CopyUniversallyWithDefaultTemplate(ProcessHeader processHeader)
		{
			var existingWorkflowPKs = processHeader.JobHeader.ProcessHeaders.Select(w => w.PK).ToArray();

			var systemFilter = new StmModuleFilterCollection(processHeader.Factory, "ProcessHeader_UC", new FilterStripLayoutsHelper()).FirstOrDefault(filter => filter.S9_IsSystem);
			AssertNotNull("There should be 1 system defined copy template for ProcessHeader", systemFilter);
			AssertEquals("Workflow/T&asks Copy Template", systemFilter.S9_FilterName);

			var copyScheduleTask = processHeader.Factory.New<StmUniversalCopyScheduleTask>();
			copyScheduleTask.Parent.CopyObject = processHeader;
			copyScheduleTask.Parent.SUC_S9_CopyTemplate = systemFilter.PK;

			var notifications = new NotificationBuffer();
			copyScheduleTask.Run(notifications);
			Assert(notifications.AsString, !notifications.HasErrors);

			var job = processHeader.JobHeader.Parent;
			job.WorkflowItems.Load();

			return processHeader.JobHeader.ProcessHeaders.Single(w => !existingWorkflowPKs.Contains(w.PK));
		}

		#endregion
	}

	#endregion

	#region CustomisedControlsPropertiesTest

	class ProcessHeaderCustomisedControlsPropertiesTest : BMSTestCaseWithFactory
	{
		public void TestList_Workflow()
		{
			var list = new CustomisationLinePropertyList(PropertySourceList.Codes.Workflow);
			Assert("Should have a bunch of properties", list.Count > 10);

			AssertCollectionContains(list.Cast<CodeDescriptionPair>(), c => c.Code == ProcessHeaderSchema.FH_CompletionStatement.Name);
			AssertCollectionContains(list.Cast<CodeDescriptionPair>(), c => c.Code == ProcessHeaderSchema.FH_AgreedDeliveryDate.Name);

			var interfaceProperties = typeof(IProcessHeader).GetProperties();

			CombineAssertions(() =>
			{
				foreach (CodeDescriptionPair item in list)
				{
					var message = string.Format("{0} should contain a definition for property [{1}] so that customised task cards can use it. Either add a definition to the interface, or apply the CustomisedControlExcludeAttribute.", typeof(IProcessHeader).FullName, item.Code);
					AssertCollectionContains(message, interfaceProperties, p => p.Name == item.Code);
				}
			});
		}

		public void TestPropertyInfoExistOnIProcessHeaderInterface()
		{
			var iZTypeSettableProperties = typeof(IProcessHeader).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).Where(p => p.PropertyType.GetInterfaces().Contains(typeof(IZType)) && p.CanWrite);
			var interfaceZPropertyInfos = typeof(IProcessHeader).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy).Where(p => p.PropertyType == typeof(ZPropertyInfo));

			CombineAssertions(() =>
			{
				var stringBuilder = new ZStringBuilder();

				foreach (var property in iZTypeSettableProperties)
				{
					var message = string.Format("{0} should a {1} for every {2} property. Without it proper ZPropertyInfo Binding is not supported. {3} is missing", typeof(IProcessHeader).FullName, typeof(ZPropertyInfo).FullName, typeof(IZType).FullName, property.Name);

					AssertCollectionContains(message, interfaceZPropertyInfos, p => p.Name == property.Name + "Info");

					stringBuilder.Append(string.Format("{0} {1}Info {{ get; }}", nameof(ZPropertyInfo), property.Name));
				}

				AssertEquals("Should have the following properties on the interface: \r\n " + stringBuilder.ToStringWithNewLineBetweenAppends(), iZTypeSettableProperties.Count(), interfaceZPropertyInfos.Count());
			});
		}
	}

	#endregion

	#region Workflow Events

	class ProcessHeader_WorkflowEventsTest : BMSTestCaseWithFactory
	{
		[TestDate(2015, 7, 14)]
		public void TestWorkflowEvents_FiredDuringTemplateApplication_ShouldFireForTriggersOnParentJob()
		{
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM");
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);

			var templateTrigger1 = template.WorkflowItems.Triggers.AddNew();
			templateTrigger1.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			templateTrigger1.P9_Sequence = 1;
			var templateTrigger2 = template.WorkflowItems.Triggers.AddNew();
			templateTrigger2.TriggerConditions.TriggerEventCode = Events.JobOpenCode;
			templateTrigger2.P9_Sequence = 2;

			var templateTask = template.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();

			job.ApplyWorkflowTemplates();

			AssertEquals(1, job.WorkflowItems.Tasks.Count);
			AssertEquals(2, job.WorkflowItems.Triggers.Count);

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);
			var workflow = jobHeader.ProcessHeaders.Single();

			Factory.Save();

			AssertEventRaised("A JOP event should have been raised when the workflow was created", workflow, Events.JobOpenCode);
			AssertEquals("No ATH event has been raised", ZDateTime.Empty, job.WorkflowItems.Triggers[0].P9_ActualDate.ToZDateTime());
			AssertEquals("Trigger should have fired because JOP event was raised on a workflow for this job", new ZDateTime(2015, 7, 14), job.WorkflowItems.Triggers[1].P9_ActualDate.ToZDateTime());
		}

		[TestDate(2015, 7, 14)]
		public void TestWorkflowEvents_EventAddedAfterTemplateApplication_ShouldFireForTriggersOnParentJob()
		{
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM");
			var templateWorkflow = BMSTestHelper.CreateWorkflow(template);

			var templateTrigger1 = template.WorkflowItems.Triggers.AddNew();
			templateTrigger1.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			templateTrigger1.P9_Sequence = 1;
			var templateTrigger2 = template.WorkflowItems.Triggers.AddNew();
			templateTrigger2.TriggerConditions.TriggerEventCode = Events.JobOpenCode;
			templateTrigger2.P9_Sequence = 2;

			var templateTask = template.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();

			job.ApplyWorkflowTemplates();

			AssertEquals(1, job.WorkflowItems.Tasks.Count);
			AssertEquals(2, job.WorkflowItems.Triggers.Count);

			var jobHeader = ProcessJobHeader.GetForParentWithoutCreation(job, Factory);
			var workflow = jobHeader.ProcessHeaders.Single();

			workflow.Logs.AddNew(Events.Authorised);

			AssertEventRaised("An ATH event was raised on the workflow manually", workflow, Events.AuthorisedCode);
			AssertEquals("Trigger should have fired because ATH event was raised on a workflow for this job", new ZDateTime(2015, 7, 14), job.WorkflowItems.Triggers[0].P9_ActualDate.ToZDateTime());

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			jobHeader.Logs.AddNew(Events.Authorised);

			AssertEquals("Trigger should have fired again because ATH event was raised on the job-level workflow for this job", new ZDateTime(2015, 7, 14, 0, 1, 0), job.WorkflowItems.Triggers[0].P9_ActualDate.ToZDateTime());
		}

		public void TestEventAddedViaDefer_ShouldExposeTriggeringEventToNotificationMacro()
		{
			AssertNotNull(DummyWorkflowDescriptor.Instance);
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "SAM", "Samwise the Brave");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM");

			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.WorkflowTransferredBetweenSystemComponentsCode;
			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			trigger.TriggerConditions.TriggerConditionValue = "MOD=DFR";
			trigger.P9_Description = "Workflow deferred";

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			triggerAction.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			triggerAction.PQ_EmailAddr = "this@daveeast.com";
			triggerAction.PQ_EmailText =
	@"A workflow was deferred in (*Z0_VarCharMax*):<br />
<br />
The workflow deferred was:<br />
(*TriggeringEvent.SL_TableFriendlyName*)<br />
<br />
It was deferred by:<br />
(*TriggeringEvent.User.GS_FullName*)<br />
";

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.Z0_VarCharMax = "Did Nothing Wrong";

			job.ApplyWorkflowTemplates();

			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "werkflow", config.Buffer);
			BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			using (Env.SetTemporaryUserContext(resource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				BMSTestHelper.Defer(workflow);
			}

			Factory.Save();

			BMSTestCaseWithFactory.AssertComponentChangedEventRaised(workflow, ComponentChangeMode.Defer, config.Buffer.PK, config.Bucket.PK, constraintStatus: ConstraintStatus.NonConstrained, bufferPenetration: 0m, bufferZone: 3);

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			BMSTestHelper.RunLogWalker(); // Creates a workflow trigger event for the delay-fired event.
			BMSTestHelper.RunLogWalker(); // Executes the trigger action to send an email.

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEmailContent(Env.OutgoingMailManager.EmailsCreated[0], "Dummy Task Provider - Workflow deferred", @"
A workflow was deferred in Did Nothing Wrong:
The workflow deferred was:
This Workflow: werkflow
It was deferred by:
Samwise the Brave
");
		}

		public void TestManuallyAddedEvent_ShouldExposeTriggeringEventToNotificationMacro()
		{
			AssertNotNull(DummyWorkflowDescriptor.Instance);
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "SAM", "Samwise the Brave");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "DUM");

			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			trigger.P9_Description = "Arrrival";

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			triggerAction.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			triggerAction.PQ_EmailAddr = "this@daveeast.com";
			triggerAction.PQ_EmailText =
	@"Something arrived in (*Z0_VarCharMax*):<br />
<br />
The workflow which arrived was:<br />
(*TriggeringEvent.SL_TableFriendlyName*)<br />
<br />
It arrived under the watch of:<br />
(*TriggeringEvent.User.GS_FullName*)<br />
";

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.Z0_VarCharMax = "Did Nothing Wrong";

			job.ApplyWorkflowTemplates();

			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "werkflow", config.Buffer);
			BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			using (Env.SetTemporaryUserContext(resource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				workflow.Logs.AddNew(Events.Arrival);
			}

			Factory.Save();

			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);

			BMSTestHelper.RunLogWalker();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEmailContent(Env.OutgoingMailManager.EmailsCreated[0], "Dummy Task Provider - Arrrival", @"
Something arrived in Did Nothing Wrong:
The workflow which arrived was:
This Workflow: werkflow
It arrived under the watch of:
Samwise the Brave
");
		}

		public void TestUniversalTrigger_ShouldRespectTemplateConditionsOnParentJob()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode);

			var normalTemplate = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode);
			var templateWorkflow = BMSTestHelper.CreateWorkflow(normalTemplate);
			var templateTask = BMSTestHelper.CreateTask(normalTemplate, templateWorkflow, GlbStaff.CurrentUser.GS_Code);

			var universalTemplate = BMSTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode, isUniversal: true);
			var universalTrigger = BMSTestHelper.CreateUniversalTrigger(universalTemplate, Events.WorkflowTransferredBetweenSystemComponents);

			universalTrigger.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			universalTrigger.TriggerConditionValue = "MOD=DFR";
			universalTrigger.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			universalTrigger.TemplateCondition2Value = "\"<O1_EnquiryType>\"==\"CCR\"";

			var triggerAction = BMSTestHelper.CreateEmailTriggerAction(universalTrigger,
	@"A workflow was deferred in (*O1_LeadUniqueReference*)<br />
<br />
The workflow deferred was:<br />
(*TriggeringEvent.SL_TableFriendlyName*)<br />
<br />
It was deferred by:<br />
(*TriggeringEvent.User.GS_FullName*)<br />
");

			Factory.Save();

			var job1 = (SalesEnquiry)BMSTestHelper.CreateJob<SalesEnquiry>(Factory);
			var job2 = (SalesEnquiry)BMSTestHelper.CreateJob<SalesEnquiry>(Factory);

			job1.O1_EnquiryType = "CCR";
			job2.O1_EnquiryType = "INQ";

			Factory.Save();

			var workflow1 = ProcessJobHeader.GetForParentWithoutCreation(job1, Factory).ProcessHeaders.Single();
			var workflow2 = ProcessJobHeader.GetForParentWithoutCreation(job2, Factory).ProcessHeaders.Single();

			workflow1.FH_FC_CurrentComponent = config.Buffer.PK;
			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;

			Factory.Save();

			BMSTestHelper.Defer(workflow1);
			BMSTestHelper.Defer(workflow2);

			Factory.Save();

			BMSTestHelper.RunLogWalker();
			BMSTestHelper.RunLogWalker();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEmailContent(Env.OutgoingMailManager.EmailsCreated[0], "Sales Inquiry - Workflow transferred between system components", @"
A workflow was deferred in I00001000
The workflow deferred was:
This Workflow: Zoot! Review.
It was deferred by:
CargoWise Support");
		}

		#region Implementation

		SchematicTestConfig config;

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();

			config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);

			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithWorkflow);
		}

		protected override void TearDown()
		{
			base.TearDown();

			DummyBusinessObject.TypeDecider.TypeForLoadOverride = null;
		}

		#endregion
	}

	#endregion

	#region NonTransactionedTests

	class ProcessHeaderTestNonTransactionedTest : NonTransactionedTestCase
	{
		public void TestFetchForLoad_ShoulBeThreadSafe()
		{
			ErrorReporter.Clear();

			var wf = BMSTestHelper.CreateWorkflow(Factory, "WF");
			Factory.Save();

			var thread = new Thread(() => wf.FetchStrategy.FetchForLoad());

			thread.Start();
			thread.Join();

			(wf.Factory.ThreadSentry as ThreadSentry).ExecuteAll_ForTest();

			AssertNull(ErrorReporter.LastExceptionReported);
		}
	}

	#endregion
}
