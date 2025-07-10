using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(WorkQueue))]
	class WorkQueueTest : EnterpriseBusinessObjectTestCase
	{
		#region TagRule

		public void TestTagRule()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var band = Factory.NewWithValidTestData<BMComponentAcceptabilityBand>();

			Factory.Save();

			AssertEquals(false, queue.HasChanges);
			var rule = queue.TagRule;
			var templateTagLink = rule.TagTemplate;

			AssertEquals(false, queue.HasChanges);

			queue.Validation.ValidateAll();
			rule.Validation.ValidateAll();
			templateTagLink.Validation.ValidateAll();

			AssertNoErrors(queue);
			AssertNoErrors(rule);
			AssertNoErrors(templateTagLink);

			AssertEquals(TagRuleActionTypeList.Codes.MaintainMagnitude, rule.TGR_ActionType);
			AssertEquals(true, rule.TGR_IsSystem);
			AssertEquals(1m, templateTagLink.TGL_Magnitude);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedQueue = newFactory.Load<WorkQueue>(queue.PK);
			var loadedRule = loadedQueue.TagRule;

			AssertNotNull(loadedRule);
			AssertEquals(rule.PK, loadedRule.PK);

			queue.Delete();

			AssertEquals(true, rule.IsDeleted);
			AssertEquals(false, loadedRule.IsDeleted);

			Factory.Save();
			AssertEquals(true, loadedRule.IsDeleted);
		}

		public void TestTagRule_ShouldUseQueueName()
		{
			var queue = Factory.New<WorkQueue>();
			queue.TGM_Description = "I can't eat chups";

			var rule = queue.TagRule;
			AssertEquals("I can't eat chups", rule.TGR_Name);

			queue.TGM_Description = "This chup?";
			AssertEquals("This chup?", rule.TGR_Name);
		}

		#endregion

		#region AddMember

		public void TestRename_WithNoAddRemoveResequenceSecurity()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "QUA", "origional name");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var link = queue.AddMember(workflow).Link;

			using (Env.SetTemporarySecurityInstanceForTest(BMSecurityTestHelper.GetSecurityInstance(Factory)))
			{
				Env.Security.FindCheckPoint("triggerSecurityLoading");
				BMSecurityTestHelper.DenyPermissionsForCheckpoint(Env.Security.WorkQueuesAddToQueue);
				BMSecurityTestHelper.DenyPermissionsForCheckpoint(Env.Security.WorkQueuesRemoveFromQueue);
				BMSecurityTestHelper.DenyPermissionsForCheckpoint(Env.Security.WorkQueuesResequenceQueue);

				queue.TGM_Description = "new name when I didnt have security";
				link.Validation.ValidateAll();

				AssertNoErrors(workflow.TagLinks_ForBinding.First().TGL_TGM_MagnitudeInfo);
			}
		}

		public void TestAddMember_MultipleQueueMembership()
		{
			var queue1 = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var queue2 = BMSTestHelper.CreateWorkQueue(Factory, "BBB", "bbb");

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			AssertAddedToQueue(queue1, jobHeader1);
			AssertEquals(true, queue1.ContainsMember(jobHeader1));

			AssertNotAddedToQueue(queue1, jobHeader1, "Cannot add an item to same queue more than once.");
			AssertNotAddedToQueue(queue2, jobHeader1, "Cannot add an item to multiple queues.");

			AssertAddedToQueue(queue2, jobHeader2);
			AssertEquals(true, queue2.ContainsMember(jobHeader2));

			AssertNotAddedToQueue(queue1, jobHeader2, "Cannot add an item to multiple queues.");
			AssertNotAddedToQueue(queue2, jobHeader2, "Cannot add an item to same queue more than once.");
		}

		public void TestAddMember_AddingWorkflowsOfJobsAlreadyInQueue()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			AssertAddedToQueue(queue, jobHeader);
			AssertEquals(true, queue.ContainsMember(jobHeader));

			AssertNotAddedToQueue(queue, workflow1, "Cannot add a workflow of a job already in the queue.");
			AssertNotAddedToQueue(queue, workflow2, "Cannot add a workflow of a job already in the queue.");

			AssertEquals(false, queue.ContainsMember(workflow1));
			AssertEquals(false, queue.ContainsMember(workflow2));
		}

		public void TestAddMember_AddingJobsOfWorkflowsAlreadyInQueue()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			AssertAddedToQueue(queue, workflow1);
			AssertAddedToQueue(queue, workflow2);
			AssertNotAddedToQueue(queue, jobHeader,
@"Cannot add the job of a workflow already in the queue. The following workflows are already present in this queue:
	workflow1
	workflow2
");
		}

		public void TestAddMember_AddingWorkflowsOfJobsAlreadyInAnotherQueue()
		{
			var queue1 = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var queue2 = BMSTestHelper.CreateWorkQueue(Factory, "BBB", "bbb");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			AssertAddedToQueue(queue1, jobHeader);
			AssertEquals(true, queue1.ContainsMember(jobHeader));

			AssertNotAddedToQueue(queue2, workflow1, "Cannot add a workflow of a job already in another queue (AAA - aaa).");
			AssertNotAddedToQueue(queue2, workflow2, "Cannot add a workflow of a job already in another queue (AAA - aaa).");

			AssertEquals(false, queue2.ContainsMember(workflow1));
			AssertEquals(false, queue2.ContainsMember(workflow2));
		}

		public void TestAddMember_AddingJobsOfWorkflowsAlreadyInAnotherQueue()
		{
			var queue1 = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var queue2 = BMSTestHelper.CreateWorkQueue(Factory, "BBB", "bbb");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			AssertAddedToQueue(queue1, workflow1);
			AssertAddedToQueue(queue1, workflow2);
			AssertNotAddedToQueue(queue2, jobHeader,
@"Cannot add the job of a workflow already in a queue. The following workflows are already present in another queue:
	workflow1 (AAA - aaa)
	workflow2 (AAA - aaa)
");
		}

		public void TestAddMember_ShouldIncrementSequence()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "jobHeader1");
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "jobHeader2");
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "jobHeader3");
			var jobHeader4 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "jobHeader4");

			jobHeader1.GetOrCreateDependencyLink(jobHeader2);
			jobHeader2.GetOrCreateDependencyLink(jobHeader3);
			jobHeader3.GetOrCreateDependencyLink(jobHeader4);

			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			AssertAddedToQueue(queue, jobHeader1, 1);
			AssertAddedToQueue(queue, jobHeader2, 2);
			AssertAddedToQueue(queue, jobHeader3, 3);
			AssertAddedToQueue(queue, jobHeader4, 4);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedQueue = newFactory.Load<WorkQueue>(queue.PK);

			AssertEquals(4, loadedQueue.Members.Count);
			AssertEquals(false, loadedQueue.HasChanges);

			BMSTestCaseWithFactory.AssertSamePK(jobHeader1, loadedQueue.MembersInSequence.ElementAt(0));
			BMSTestCaseWithFactory.AssertSamePK(jobHeader2, loadedQueue.MembersInSequence.ElementAt(1));
			BMSTestCaseWithFactory.AssertSamePK(jobHeader3, loadedQueue.MembersInSequence.ElementAt(2));
			BMSTestCaseWithFactory.AssertSamePK(jobHeader4, loadedQueue.MembersInSequence.ElementAt(3));
		}

		public void TestAddMember_ResequenceWhenBeyondShortInt()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			var result = queue.AddMember(workflow1);
			queue.Members[0].TGL_Sequence = short.MaxValue;

			Assert(result.WasSuccessful);
			AssertEquals(short.MaxValue, queue.Members[0].TGL_Sequence);

			result = queue.AddMember(workflow2);

			Assert(result.WasSuccessful);
			AssertEquals((ZShort)1, queue.Members.OrderBy(m => m.TGL_Sequence).ElementAt(0).TGL_Sequence);
			AssertEquals((ZShort)2, queue.Members.OrderBy(m => m.TGL_Sequence).ElementAt(1).TGL_Sequence);
		}

		public void TestAddMember_ResequenceWhenBeyondShortInt_MoreMembers()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow4");
			var workflow5 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow5");

			queue.AddMember(workflow1);
			queue.AddMember(workflow2);
			queue.AddMember(workflow3);
			queue.AddMember(workflow4);

			var orderedQueue = queue.Members.OrderBy(m => m.TGL_Sequence).ToArray();

			orderedQueue[0].TGL_Sequence = short.MaxValue - 3;
			orderedQueue[1].TGL_Sequence = short.MaxValue - 2;
			orderedQueue[2].TGL_Sequence = short.MaxValue - 1;
			orderedQueue[3].TGL_Sequence = short.MaxValue;

			var result = queue.AddMember(workflow5);
			Assert(result.WasSuccessful);

			orderedQueue = queue.Members.OrderBy(m => m.TGL_Sequence).ToArray();

			AssertEquals((ZShort)1, orderedQueue[0].TGL_Sequence);
			AssertEquals((ZShort)2, orderedQueue[1].TGL_Sequence);
			AssertEquals((ZShort)3, orderedQueue[2].TGL_Sequence);
			AssertEquals((ZShort)4, orderedQueue[3].TGL_Sequence);
			AssertEquals((ZShort)5, orderedQueue[4].TGL_Sequence);
		}

		public void TestAddMember_AttemptResequenceAccordingToSequencePattern()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow4");
			var workflow5 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow5");

			var result = queue.AddMember(workflow1);

			Assert(result.WasSuccessful);
			AssertEquals((ZShort)1, queue.Members[0].TGL_Sequence);

			queue.Members[0].TGL_Sequence = 100;
			result = queue.AddMember(workflow2);

			Assert(result.WasSuccessful);
			AssertEquals((ZShort)100, queue.Members.OrderBy(m => m.TGL_Sequence).ElementAt(0).TGL_Sequence);
			AssertEquals((ZShort)101, queue.Members.OrderBy(m => m.TGL_Sequence).ElementAt(1).TGL_Sequence);

			queue.Members.OrderBy(m => m.TGL_Sequence).ElementAt(1).TGL_Sequence = 200;
			result = queue.AddMember(workflow3);

			Assert(result.WasSuccessful);
			AssertEquals((ZShort)100, queue.Members.OrderBy(m => m.TGL_Sequence).ElementAt(0).TGL_Sequence);
			AssertEquals((ZShort)200, queue.Members.OrderBy(m => m.TGL_Sequence).ElementAt(1).TGL_Sequence);
			AssertEquals((ZShort)201, queue.Members.OrderBy(m => m.TGL_Sequence).ElementAt(2).TGL_Sequence);

			queue.Members.OrderBy(m => m.TGL_Sequence).ElementAt(2).TGL_Sequence = short.MaxValue;
			result = queue.AddMember(workflow4);

			Assert(result.WasSuccessful);
			AssertEquals((ZShort)100, queue.Members.OrderBy(m => m.TGL_Sequence).ElementAt(0).TGL_Sequence);
			AssertEquals((ZShort)200, queue.Members.OrderBy(m => m.TGL_Sequence).ElementAt(1).TGL_Sequence);
			AssertEquals((ZShort)300, queue.Members.OrderBy(m => m.TGL_Sequence).ElementAt(2).TGL_Sequence);
			AssertEquals((ZShort)301, queue.Members.OrderBy(m => m.TGL_Sequence).ElementAt(3).TGL_Sequence);

			queue.Members.OrderBy(m => m.TGL_Sequence).ElementAt(3).TGL_Sequence = short.MaxValue;
			result = queue.AddMember(workflow5);

			Assert(result.WasSuccessful);
			AssertEquals((ZShort)100, queue.Members.OrderBy(m => m.TGL_Sequence).ElementAt(0).TGL_Sequence);
			AssertEquals((ZShort)200, queue.Members.OrderBy(m => m.TGL_Sequence).ElementAt(1).TGL_Sequence);
			AssertEquals((ZShort)300, queue.Members.OrderBy(m => m.TGL_Sequence).ElementAt(2).TGL_Sequence);
			AssertEquals((ZShort)400, queue.Members.OrderBy(m => m.TGL_Sequence).ElementAt(3).TGL_Sequence);
			AssertEquals((ZShort)401, queue.Members.OrderBy(m => m.TGL_Sequence).ElementAt(4).TGL_Sequence);
		}

		#endregion

		#region Queue Running Totals

		public void TestFirstItemRunningTotal_IsPlannedDuration()
		{
			var queue = Factory.New<WorkQueue>();

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader1.FH_PlannedDurationInMinutes = 62;
			var jobHeader1Link = queue.AddMember(jobHeader1).Link;

			AssertEquals("One item in a work queue, RunningTotal should be equal to that items planned duration", 62, jobHeader1Link.RunningTotalInMinutes);
		}

		public void TestSecondItemRunningTotal_IsAddedCorrectly()
		{
			var queue = Factory.New<WorkQueue>();

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader1.FH_PlannedDurationInMinutes = 43;
			var link1 = queue.AddMember(jobHeader1).Link;

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader2.FH_PlannedDurationInMinutes = 81;
			var link2 = queue.AddMember(jobHeader2).Link;

			AssertEquals("Two Items in work queue, RunningTotal should be their planned durations added together",
				(link1.Parent.FH_PlannedDurationInMinutes + link2.Parent.FH_PlannedDurationInMinutes), queue.Members.Max(m => m.RunningTotalInMinutes));
		}

		public void TestNthItemRunningTotal_IsAddedCorrectly()
		{
			var queue = Factory.New<WorkQueue>();

			ZInt plannedDurationSum = 0;

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader1.FH_PlannedDurationInMinutes = 342;
			queue.AddMember(jobHeader1);
			plannedDurationSum += jobHeader1.FH_PlannedDurationInMinutes;

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader2.FH_PlannedDurationInMinutes = 654;
			queue.AddMember(jobHeader2);
			plannedDurationSum += jobHeader2.FH_PlannedDurationInMinutes;

			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader3.FH_PlannedDurationInMinutes = 1234;
			queue.AddMember(jobHeader3);
			plannedDurationSum += jobHeader3.FH_PlannedDurationInMinutes;

			var jobHeader4 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader4.FH_PlannedDurationInMinutes = 6534;
			queue.AddMember(jobHeader4);
			plannedDurationSum += jobHeader4.FH_PlannedDurationInMinutes;

			var jobHeader5 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader5.FH_PlannedDurationInMinutes = 123;
			queue.AddMember(jobHeader5);
			plannedDurationSum += jobHeader5.FH_PlannedDurationInMinutes;

			var jobHeader6 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader6.FH_PlannedDurationInMinutes = 523;
			queue.AddMember(jobHeader6);
			plannedDurationSum += jobHeader6.FH_PlannedDurationInMinutes;

			AssertEquals("Largest Running total in the work queue should be equal to the sum of allz planned durations in the queue.",
				plannedDurationSum.GetDateTimeFromMinutes(), queue.Members.Max(m => m.RunningTotal));
		}

		public void TestRunningTotal_ItemsWith0Duration()
		{
			var queue = Factory.New<WorkQueue>();

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader1.FH_PlannedDurationInMinutes = 0;
			queue.AddMember(jobHeader1);

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader2.FH_PlannedDurationInMinutes = 0;
			queue.AddMember(jobHeader2);

			AssertEquals("Two items in queue, both with 0 duration, runningTotal should be 0.", 0, queue.Members.Max(m => m.RunningTotalInMinutes));
		}

		public void TestRunningTotal_Cache()
		{
			var queue = Factory.New<WorkQueue>();
			queue.TGM_Code = "JIM";

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader1Link = queue.AddMember(jobHeader1).Link;
			jobHeader1Link.TGL_Sequence = 1;

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2Link = queue.AddMember(jobHeader2).Link;
			jobHeader2Link.TGL_Sequence = 2;

			AssertNotNull(jobHeader1Link.RunningTotalInMinutes);
			AssertNotNull(jobHeader2Link.RunningTotalInMinutes);

			Factory.Save();
			jobHeader1.FH_PlannedDurationInMinutes = 10;
			jobHeader2.FH_PlannedDurationInMinutes = 1;

			AssertEquals(11, jobHeader2Link.RunningTotalInMinutes);
			jobHeader2Link.TGL_Sequence = 1;
			jobHeader1Link.TGL_Sequence = 2;
			AssertEquals(11, jobHeader2Link.RunningTotalInMinutes);

			Factory.Save();
			jobHeader1.FH_PlannedDurationInMinutes = 10;
			jobHeader2.FH_PlannedDurationInMinutes = 1;

			AssertEquals("After Factory Save, cache should be invalidated and now return the right value.", 1, jobHeader2Link.RunningTotalInMinutes);
		}

		public void TestRunningTotal_DoesntHitDB()
		{
			var queue = Factory.New<WorkQueue>();
			queue.TGM_Code = "JME";

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader1.FH_PlannedDurationInMinutes = 43;

			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader2.FH_PlannedDurationInMinutes = 81;

			queue.AddMember(jobHeader1);
			var link2 = queue.AddMember(jobHeader2).Link;

			Factory.Save();
			var newFactory = Factory.CreateNewFactory();
			newFactory.Load<WorkQueue>(queue.PK);
			var initialDBHits = newFactory.GetTableHitCount(TagMagnitudeSchema.Constants.TableName);

			AssertNotNull(newFactory.Load<WorkQueueMembershipLink>(link2.PK).RunningTotalInMinutes);
			AssertEquals("Factory hit counts shouldn't change from calculating TotalRunTime", newFactory.GetTableHitCount(TagMagnitudeSchema.Constants.TableName), initialDBHits);
		}

		public void TestRunningTotal_IntToDateTime()
		{
			var queue = Factory.New<WorkQueue>();

			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader1.FH_PlannedDurationInMinutes = 6432;

			var jobHeader1Link = queue.AddMember(jobHeader1).Link;
			AssertEquals("[PreCondition] RunningTotalInMinutes must return the correct value", 6432, jobHeader1Link.RunningTotalInMinutes);
			AssertEquals("RunningTotal Should return RunningTotalInMinutes in DateTime Format", jobHeader1.PlannedDuration, jobHeader1Link.RunningTotal);
		}

		#endregion

		#region Add Tag

		public void TestAddTag_ShouldUseQueueMembershipValidation()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var jobHeader = ProcessJobHeader.GetForParent(org, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workfloow");

			var result = workflow.AddTag(queue);
			AssertEquals(true, result.WasSuccessful);
			BMSTestCaseWithFactory.AssertTagApplied(workflow, queue);

			result = jobHeader.AddTag(queue);
			AssertEquals(false, result.WasSuccessful);
			AssertMultilineASCIIEquals("",
@"Cannot add the job of a workflow already in the queue. The following workflows are already present in this queue:
	workfloow", result.Message);
			BMSTestCaseWithFactory.AssertTagNotApplied(jobHeader, queue);
		}

		#endregion

		#region Delete

		public void TestDelete()
		{
			var queuesGroup = TagProvider.GetWorkQueuesTagGroup(Factory);
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "Bbb");

			Factory.Save();

			AssertNoExceptionThrown(() => queue.Delete());
			AssertExceptionThrown<CannotDeleteException>(() => queuesGroup.Delete());

			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestDelete_ShouldAlsoDeleteMemberLinks()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var link1 = queue.AddMember(jobHeader1).Link;
			var link2 = queue.AddMember(jobHeader2).Link;

			queue.Delete();

			AssertEquals(true, link1.IsDeleted);
			AssertEquals(true, link2.IsDeleted);

			AssertEquals(false, jobHeader1.IsDeleted);
			AssertEquals(false, jobHeader2.IsDeleted);
		}

		public void TestDeleteWorkflow_ShouldKnitQueue()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow4");

			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var link1 = queue.AddMember(workflow1).Link;
			var link2 = queue.AddMember(workflow2).Link;
			var link3 = queue.AddMember(workflow3).Link;
			var link4 = queue.AddMember(workflow4).Link;

			AssertEquals(4, queue.Members.Count);

			workflow2.Delete();
			AssertEquals(true, link2.IsDeleted);

			Factory.Save();

			var loadedQueue = Factory.CreateNewFactory().Load<WorkQueue>(queue.PK);

			AssertEquals(3, loadedQueue.Members.Count);
			AssertEquals(false, loadedQueue.HasChanges);

			BMSTestCaseWithFactory.AssertSamePK(workflow1, loadedQueue.MembersInSequence.ElementAt(0));
			BMSTestCaseWithFactory.AssertSamePK(workflow3, loadedQueue.MembersInSequence.ElementAt(1));
			BMSTestCaseWithFactory.AssertSamePK(workflow4, loadedQueue.MembersInSequence.ElementAt(2));
		}

		#endregion

		#region Properties

		public void TestCreateWorkQueue_ShouldAssociateWithTagGroup()
		{
			var queuesGroup = TagProvider.GetWorkQueuesTagGroup(Factory);
			var queue = Factory.New<WorkQueue>();

			AssertEquals(queuesGroup.PK, queue.TGM_TGD_Tag);
		}

		public void TestCode_ShouldNotHaveErrors()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			queue.Validation.ValidateAll();

			AssertNoErrors(queue);
		}

		public void TestFieldReadonliness()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			AssertEquals(false, queue.TGM_CodeInfo.ReadOnly);
			AssertEquals(false, queue.TGM_DescriptionInfo.ReadOnly);
		}

		#endregion

		#region Sequence

		public void TestSequence()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);

			var link = queue.AddMember(jobHeader).Link;
			AssertEquals((short)1, link.TGL_Sequence);
			AssertEquals(false, link.TGL_SequenceInfo.ReadOnly);

			Factory.Save();

			AssertEquals(false, queue.HasChanges);
			AssertEquals(false, link.HasChanges);

			link.TGL_Sequence = 2;

			AssertEquals(true, queue.HasChanges);
			AssertEquals(true, link.HasChanges);
		}

		public void TestChangeSequenceAndSave_ShouldLeaveSequenceNumbers()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");

			var link1 = queue.AddMember(workflow1).Link;
			var link2 = queue.AddMember(workflow2).Link;
			var link3 = queue.AddMember(workflow3).Link;

			link3.TGL_Sequence = 30;
			link2.TGL_Sequence = 20;
			link1.TGL_Sequence = 10;

			AssertEquals((short)10, link1.TGL_Sequence);
			AssertEquals((short)20, link2.TGL_Sequence);
			AssertEquals((short)30, link3.TGL_Sequence);

			Factory.Save();

			AssertEquals((short)10, link1.TGL_Sequence);
			AssertEquals((short)20, link2.TGL_Sequence);
			AssertEquals((short)30, link3.TGL_Sequence);
		}

		public void TestSequenceNumbers_ShouldIncrementFromHighestNumber()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			var link1 = queue.AddMember(workflow1).Link;
			link1.TGL_Sequence = 10;

			var link2 = queue.AddMember(workflow2).Link;

			AssertEquals((short)10, link1.TGL_Sequence);
			AssertEquals((short)11, link2.TGL_Sequence);
		}

		public void TestSequenceNumbers_ShouldIncrement()
		{
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "jobHeader1");
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "jobHeader2");
			var jobHeader3 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "jobHeader3");
			var jobHeader4 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "jobHeader4");
			var jobHeader5 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "jobHeader5");

			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");
			Factory.Save();

			var link1 = queue.AddMember(jobHeader1).Link;
			var link2 = queue.AddMember(jobHeader2).Link;
			var link3 = queue.AddMember(jobHeader3).Link;
			var link4 = queue.AddMember(jobHeader4).Link;

			link4.TGL_Sequence = 1;
			link3.TGL_Sequence = 2;
			link2.TGL_Sequence = 3;
			link1.TGL_Sequence = 4;

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedQueue = newFactory.Load<WorkQueue>(queue.PK);

			AssertEquals(4, loadedQueue.Members.Count);

			var loadedLink1 = loadedQueue.Members.Single(l => l.TGL_ParentId == jobHeader1.PK);
			var loadedLink2 = loadedQueue.Members.Single(l => l.TGL_ParentId == jobHeader2.PK);
			var loadedLink3 = loadedQueue.Members.Single(l => l.TGL_ParentId == jobHeader3.PK);
			var loadedLink4 = loadedQueue.Members.Single(l => l.TGL_ParentId == jobHeader4.PK);

			AssertEquals((short)4, loadedLink1.TGL_Sequence);
			AssertEquals((short)3, loadedLink2.TGL_Sequence);
			AssertEquals((short)2, loadedLink3.TGL_Sequence);
			AssertEquals((short)1, loadedLink4.TGL_Sequence);

			BMSTestCaseWithFactory.AssertSamePK(jobHeader4, loadedQueue.MembersInSequence.ElementAt(0));
			BMSTestCaseWithFactory.AssertSamePK(jobHeader3, loadedQueue.MembersInSequence.ElementAt(1));
			BMSTestCaseWithFactory.AssertSamePK(jobHeader2, loadedQueue.MembersInSequence.ElementAt(2));
			BMSTestCaseWithFactory.AssertSamePK(jobHeader1, loadedQueue.MembersInSequence.ElementAt(3));

			var link5 = loadedQueue.AddMember(jobHeader5).Link;
			AssertEquals((short)5, link5.TGL_Sequence);

			newFactory.Save();

			BMSTestCaseWithFactory.AssertSamePK(jobHeader4, loadedQueue.MembersInSequence.ElementAt(0));
			BMSTestCaseWithFactory.AssertSamePK(jobHeader3, loadedQueue.MembersInSequence.ElementAt(1));
			BMSTestCaseWithFactory.AssertSamePK(jobHeader2, loadedQueue.MembersInSequence.ElementAt(2));
			BMSTestCaseWithFactory.AssertSamePK(jobHeader1, loadedQueue.MembersInSequence.ElementAt(3));
			BMSTestCaseWithFactory.AssertSamePK(jobHeader5, loadedQueue.MembersInSequence.ElementAt(4));

			link5.ShouldValidateSequenceUniqueness = true;
			link5.TGL_Sequence = 2;
			AssertHasError(link5.TGL_SequenceInfo, "The Sequence has been duplicated and must be unique for each item in a Work Queue.");

			link5.TGL_Sequence = -1;
			AssertHasError(link5.TGL_SequenceInfo, "Please enter a 'Sequence' greater than or equal to 0.");

			link5.TGL_Sequence = 10;
			AssertNoErrors(link5.TGL_SequenceInfo);
		}

		#endregion

		#region TypeDecider

		public void TestTypeDecider()
		{
			var config = TestConfigsHelper.CreateTagsTestConfig(Factory);
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();

			var loadedQueue = newFactory.Load<TagMagnitude>(queue.PK);
			var loadedTag = newFactory.Load<TagMagnitude>(config.PlatinumTag.PK);

			AssertType<WorkQueue>(loadedQueue);
			AssertType<TagMagnitude>(loadedTag);
		}

		#endregion

		#region Logs

		public void TestNoStmALogs()
		{
			var queue = BMSTestHelper.CreateWorkQueue(Factory, "AAA", "aaa");

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, queue.PK);
			AssertEquals("Should not create Add event", 0, Factory.Load<StmALog>(query).Length);

			queue.TGM_Description = "New description";
			Factory.Save();
			AssertEquals("Should not create Edit event", 0, Factory.Load<StmALog>(query).Length);

			queue.Delete();
			Factory.Save();
			AssertEquals("Should not create Delete event", 0, Factory.Load<StmALog>(query).Length);
		}

		#endregion

		#region Implementation

		static void AssertAddedToQueue(WorkQueue queue, ProcessHeader processHeader, int? sequence = null)
		{
			var result = queue.AddMember(processHeader);

			CombineAssertions(() =>
			{
				AssertEquals("Should have been successfully added to queue", true, result.WasSuccessful);
				AssertNotNull("Should have a link returned", result.Link);
				AssertNull("Should have no failure message", result.Message);

				if (sequence != null)
				{
					AssertEquals("Link Sequence", sequence.Value, result.Link.TGL_Sequence);
				}
			});
		}

		static void AssertNotAddedToQueue(WorkQueue queue, ProcessHeader processHeader, string expectedMessage)
		{
			var result = queue.AddMember(processHeader);

			CombineAssertions(() =>
			{
				AssertEquals("Should not have been added to queue", false, result.WasSuccessful);
				AssertNull("Should be no link returned", result.Link);
				AssertMultilineASCIIEquals("Failure message", expectedMessage, result.Message);
			});
		}

		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return "ApplyColorToBackground";
				yield return "ApplyColorToBorder";
				yield return "BorderStyle";
				yield return "Color";
				yield return "VisualStylePriority";
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(factory);
			var queue = BMSTestHelper.CreateWorkQueue(factory, "AAA", "aaa");

			queue.AddMember(jobHeader);

			return queue;
		}

		#endregion
	}
}
