using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using NUnit.Framework;

namespace Enterprise.BufferManagement.DataTransfer.Test.Universal
{
	class UniversalTaskSetWriterTest : BMSTestCaseWithFactory
	{
		public void TestPopulateTaskSets_BufferManagementDisabled_WithNoTasks_ShouldNotIncludeWorkflowCollection()
		{
			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			var activity = new Activity(DefaultDataObjectWriterStrategy.TestInstance);
			var writer = new UniversalTaskSetWriter();
			writer.PopulateTaskSets(DefaultDataObjectWriterStrategy.TestInstance, job, activity);

			AssertNull("There aren't any tasks so no colleciton should be included. SAD!", activity.TaskSetCollection);
		}

		public void TestPopulateTaskSets_BufferManagementDisabled_WithTasks_ShouldCreateJobLevelWorkflowAndDefaultWorkflowForTasks()
		{
			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			var task = job.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Baby Legs";
			var activity = new Activity(DefaultDataObjectWriterStrategy.TestInstance);
			var writer = new UniversalTaskSetWriter();
			writer.PopulateTaskSets(DefaultDataObjectWriterStrategy.TestInstance, job, activity);

			AssertNotNull("There is a task, so workflows and tasks should be included. SAD!", activity.TaskSetCollection);
			AssertTaskSetDescriptions(activity.TaskSetCollection, "Job is complete.");

			var jobDeliverable = activity.TaskSetCollection.Single();
			AssertTaskSetDetails(jobDeliverable, "Job is complete.", "JOB", null, null, null, null, null, null, null, null);
			AssertTaskSetDescriptions(jobDeliverable.TaskSetCollection, "Job Workflow");

			var unit = jobDeliverable.TaskSetCollection.Single();
			AssertTaskSetDetails(unit, "Job Workflow", "TKS", null, null, null, null, null, null, null, null, "Baby Legs");
			AssertNull(unit.TaskSetCollection);
		}

		public void TestTypeCodesAndDescriptions_BufferManagementDisabled()
		{
			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			var task = job.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Baby Legs";
			var activity = new Activity(DefaultDataObjectWriterStrategy.TestInstance);
			var writer = new UniversalTaskSetWriter();
			writer.PopulateTaskSets(DefaultDataObjectWriterStrategy.TestInstance, job, activity);

			AssertNotNull("There is a task, so workflows and tasks should be included. SAD!", activity.TaskSetCollection);
			AssertTaskSetDescriptions(activity.TaskSetCollection, "Job is complete.");

			var jobDeliverable = activity.TaskSetCollection.Single();
			AssertEquals("These codes must not ever change because they're exported in Universal XML. Descriptions can change, but codes can't. SAD!", "JOB", jobDeliverable.Type?.Code);
			AssertEquals("Job", jobDeliverable.Type?.Description);

			var unit = jobDeliverable.TaskSetCollection.Single();
			AssertEquals("These codes must not ever change because they're exported in Universal XML. Descriptions can change, but codes can't. SAD!", "TKS", unit.Type?.Code);
			AssertEquals("Workflow", unit.Type?.Description);
		}

		[TestDate(2018, 8, 23, 14, 0, 0)]
		public void TestPopulateTaskSets_BufferManagementEnabled()
		{
			BMSTestHelper.EnableBMSInRegistry();
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "INQ");
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false, description: "I have a job. Perhaps you'd like to complete it.");
			var taskSet1 = BMSTestHelper.CreateWorkflow(jobHeader, "Another quick mystery", config.Buffer, ZDateTime.Now.AddDays(-2), config.ReleaseGroup.PK);
			BMSTestHelper.CreateTask(taskSet1, description: "Baby Legs");
			BMSTestHelper.CreateTask(taskSet1, description: "Regular Legs");

			var otherGroup = Factory.NewWithValidTestData<GlbGroup>();
			otherGroup.GG_Code = "OTH";
			jobHeader.FH_GG_ReleaseGroup = otherGroup.PK;
			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.Now.AddDays(-10);
			jobHeader.FH_AgreedDeliveryDate = ZDateTime.Now.AddDays(6);
			jobHeader.WorkflowNote.ST_NoteDataAsText = "Shmlonathan";

			taskSet1.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(-8);
			taskSet1.FH_AgreedDeliveryDate = ZDateTime.Now.AddDays(5);
			taskSet1.FH_DateAcceptability = DateAcceptabilityList.Codes.ExtendedStartGraduatedFinish;
			taskSet1.WorkflowNote.ST_NoteDataAsText = "Shmlangela";

			var taskSet2 = BMSTestHelper.CreateWorkflow(jobHeader, "Guilty! Sentenced to murder!", config.Bucket);
			taskSet2.FH_IsActive = false;

			Factory.Save();

			var activity = new Activity(DefaultDataObjectWriterStrategy.TestInstance);
			var writer = new UniversalTaskSetWriter();
			writer.PopulateTaskSets(DefaultDataObjectWriterStrategy.TestInstance, (BusinessObject)jobHeader.Parent, activity);

			AssertNotNull("There is a task, so workflows and tasks should be included. SAD!", activity.TaskSetCollection);
			AssertTaskSetDescriptions(activity.TaskSetCollection, "I have a job. Perhaps you'd like to complete it.");

			var jobDeliverableData = activity.TaskSetCollection.Single();
			AssertTaskSetDetails(jobDeliverableData, "I have a job. Perhaps you'd like to complete it.", "JOB", string.Empty, "Shmlonathan", true, ZDateTime.Now.AddDays(-10), ZDateTime.Now.AddDays(6), string.Empty, "OTH", "OPN");
			AssertTaskSetDescriptions(jobDeliverableData.TaskSetCollection, "Another quick mystery", "Guilty! Sentenced to murder!");

			var taskSetData = jobDeliverableData.TaskSetCollection.Single(x => x.Description.HasValue && x.Description.Value == "Another quick mystery");
			AssertTaskSetDetails(taskSetData, "Another quick mystery", "TKS", "buffer - Zone 2", "Shmlangela", true, ZDateTime.UtcNow.AddDays(-8), ZDateTime.Now.AddDays(5), DateAcceptabilityList.Codes.ExtendedStartGraduatedFinish, config.ReleaseGroup.GG_Code, "OPN", "Baby Legs", "Regular Legs");
			AssertNull(taskSetData.TaskSetCollection);

			taskSetData = jobDeliverableData.TaskSetCollection.Single(x => x.Description.HasValue && x.Description.Value == "Guilty! Sentenced to murder!");
			AssertTaskSetDetails(taskSetData, "Guilty! Sentenced to murder!", "TKS", "bucket", string.Empty, false, ZDateTime.Empty, ZDateTime.Empty, string.Empty, "OTH", "CLS");
			AssertNull(taskSetData.TaskSetCollection);
		}

		public void TestPopulateTaskSets_BufferManagementEnabled_WithNoTasks_ShouldIncludeJobLevelWorkflowAndNothingElse()
		{
			BMSTestHelper.EnableBMSInRegistry();
			TestConfigsHelper.CreateSchematicTestConfig(Factory, "INQ");
			var job = Factory.NewWithValidTestData<SalesEnquiry>();
			Factory.Save();

			var activity = new Activity(DefaultDataObjectWriterStrategy.TestInstance);
			var writer = new UniversalTaskSetWriter();
			writer.PopulateTaskSets(DefaultDataObjectWriterStrategy.TestInstance, job, activity);

			AssertNotNull("There aren't any tasks, but buffer management is enabled for this workflow type, so a job-level workflow should still be included. SAD!", activity.TaskSetCollection);
			AssertTaskSetDescriptions(activity.TaskSetCollection, "Job Inquiry (I00001000) is complete.");

			var jobDeliverableData = activity.TaskSetCollection.Single();
			AssertTaskSetDetails(jobDeliverableData, "Job Inquiry (I00001000) is complete.", "JOB", string.Empty, string.Empty, true, ZDateTime.Empty, ZDateTime.Empty, string.Empty, null, "OPN");
			AssertNull(jobDeliverableData.TaskSetCollection);
		}

		public void TestPopulateTaskSets_WithCircularHierarchy_InSameJob_ShouldNotIncludeTaskSets_AndNotStackOverflow()
		{
			BMSTestHelper.EnableBMSInRegistry();
			TestConfigsHelper.CreateSchematicTestConfig(Factory, "INQ");
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader, "Workflow2");

			BMSTestHelper.MakeChildOf(workflow1, workflow2);
			BMSTestHelper.MakeChildOf(workflow2, workflow1);

			var activity = new Activity(DefaultDataObjectWriterStrategy.TestInstance);
			var writer = new UniversalTaskSetWriter();
			writer.PopulateTaskSets(DefaultDataObjectWriterStrategy.TestInstance, (BusinessObject)workflow1.Parent, activity);

			AssertTaskSetDescriptions(activity.TaskSetCollection, "Job Inquiry is complete.");

			var jobDeliverableData = activity.TaskSetCollection.Single();
			AssertNull("The workflows have parents in the same job, so they should be be excluded because we're using the job header's ChildWorkflows collection (which excludes these). SAD!", jobDeliverableData.TaskSetCollection);
		}

		public void TestPopulateTaskSets_WithCircularHierarchy_InDifferentJob_ShouldOnlyIncludeTaskSetsInSameJob_AndNotStackOverflow()
		{
			BMSTestHelper.EnableBMSInRegistry();
			TestConfigsHelper.CreateSchematicTestConfig(Factory, "INQ");
			var jobHeader1 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var taskSet1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader1, "Workflow1");
			var taskSet2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader2, "Workflow2");

			BMSTestHelper.MakeChildOf(taskSet1, taskSet2);
			BMSTestHelper.MakeChildOf(taskSet2, taskSet1);

			var activity = new Activity(DefaultDataObjectWriterStrategy.TestInstance);
			var writer = new UniversalTaskSetWriter();
			writer.PopulateTaskSets(DefaultDataObjectWriterStrategy.TestInstance, (BusinessObject)taskSet1.Parent, activity);

			AssertTaskSetDescriptions(activity.TaskSetCollection, "Job Inquiry is complete.");

			var jobDeliverableData = activity.TaskSetCollection.Single();
			AssertTaskSetDescriptions(jobDeliverableData.TaskSetCollection, "Workflow1");

			var taskSetData = jobDeliverableData.TaskSetCollection.Single();
			AssertNull(taskSetData.TaskSetCollection);
		}

		public void TestPopulateTaskSets_ShouldNotIncludeChildWorkflowsFromOtherJobs()
		{
			BMSTestHelper.EnableBMSInRegistry();
			TestConfigsHelper.CreateSchematicTestConfig(Factory, "INQ");
			var jobHeader1 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);

			var taskSet1 = BMSTestHelper.CreateWorkflowAndTask(jobHeader1, "Workflow1");
			var taskSet2 = BMSTestHelper.CreateWorkflowAndTask(jobHeader1, "Workflow2");
			var taskSet3 = BMSTestHelper.CreateWorkflowAndTask(jobHeader2, "Workflow3");
			var taskSet4 = BMSTestHelper.CreateWorkflowAndTask(jobHeader2, "Workflow4");

			BMSTestHelper.MakeChildOf(taskSet2, taskSet1);
			BMSTestHelper.MakeChildOf(taskSet3, jobHeader1);
			BMSTestHelper.MakeChildOf(taskSet4, taskSet1);

			Factory.Save();

			var activity = new Activity(DefaultDataObjectWriterStrategy.TestInstance);
			var writer = new UniversalTaskSetWriter();
			writer.PopulateTaskSets(DefaultDataObjectWriterStrategy.TestInstance, (BusinessObject)taskSet1.Parent, activity);

			AssertTaskSetDescriptions(activity.TaskSetCollection, "Job Inquiry is complete.");
			var jobDeliverableData = activity.TaskSetCollection.Single();
			AssertTaskSetDescriptions(jobDeliverableData.TaskSetCollection, "Workflow1");

			var taskSetData = jobDeliverableData.TaskSetCollection.Single();
			AssertTaskSetDescriptions(taskSetData.TaskSetCollection, "Workflow2");

			taskSetData = taskSetData.TaskSetCollection.Single();
			AssertNull(taskSetData.TaskSetCollection);
		}

		public void TestDeliverableStatus()
		{
			BMSTestHelper.EnableBMSInRegistry();
			TestConfigsHelper.CreateSchematicTestConfig(Factory, "INQ");
			var taskSet = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Delivered!");
			Factory.Save();

			var activity = new Activity(DefaultDataObjectWriterStrategy.TestInstance);
			var writer = new UniversalTaskSetWriter();
			writer.PopulateTaskSets(DefaultDataObjectWriterStrategy.TestInstance, (BusinessObject)taskSet.Parent, activity);
			var taskSetData = activity.TaskSetCollection.Single().TaskSetCollection.Single();

			AssertEquals("CLS", taskSetData.TaskSetStatus?.Code);
			AssertEquals("Closed", taskSetData.TaskSetStatus?.Description);
		}

		public void TestDateAcceptability()
		{
			BMSTestHelper.EnableBMSInRegistry();
			TestConfigsHelper.CreateSchematicTestConfig(Factory, "INQ");
			var taskSet = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "Delivered!");
			taskSet.FH_DateAcceptability = DateAcceptabilityList.Codes.ExtendedStartSharpFinish;
			Factory.Save();

			var activity = new Activity(DefaultDataObjectWriterStrategy.TestInstance);
			var writer = new UniversalTaskSetWriter();
			writer.PopulateTaskSets(DefaultDataObjectWriterStrategy.TestInstance, (BusinessObject)taskSet.Parent, activity);
			var taskSetData = activity.TaskSetCollection.Single().TaskSetCollection.Single();

			AssertEquals(DateAcceptabilityList.Codes.ExtendedStartSharpFinish, taskSetData.DateAcceptability?.Code);
			AssertEquals(DateAcceptabilityList.Descriptions.ExtendedStartSharpFinish, taskSetData.DateAcceptability?.Description);
		}

		#region Implementation

		static void AssertTaskSetDescriptions(IEnumerable<TaskSet> taskSets, params string[] expectedDescriptions)
		{
			AssertContainsExactElementsInAnyOrder(expectedDescriptions, taskSets.Where(x => x.Description.HasValue).Select(x => x.Description.Value));
		}

		static void AssertTaskSetDetails(TaskSet unit, string description, string typeCode, string currentStatus, string notes, bool? isActive, ZDateTime? earliestStartDate,
			ZDateTime? agreedDeliveryDate, string dateAcceptabilityCode, string releaseGroupCode, string taskSetStatusCode, params string[] taskDescriptions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Description", description, unit.Description);
				AssertEquals("Workflow Type (job or workflow/task set)", typeCode, unit.Type?.Code);
				AssertEquals("Current status (component - zone)", currentStatus, unit.CurrentStatus);
				AssertEquals("Deliverable notes (Workflow notes)", notes, unit.Notes);
				AssertEquals("Active", isActive, unit.IsActive);

				if (earliestStartDate == null)
				{
					AssertEquals("Ealiest start date", false, unit.EarliestStartDateUTC.HasValue);
				}
				else
				{
					AssertEquals("Ealiest start date", earliestStartDate, unit.EarliestStartDateUTC.Value.ToZDateTime());
					AssertEquals("Earliest start date is UTC so has 00:00 offset", TimeSpan.Zero, unit.EarliestStartDateUTC.Value.Offset);
				}
				if (agreedDeliveryDate == null)
				{
					AssertEquals("Agreed delivery date", false, unit.AgreedDeliveryDateUTC.HasValue);
				}
				else
				{
					AssertEquals("Agreed delivery date", agreedDeliveryDate, unit.AgreedDeliveryDateUTC.Value.ToZDateTime());
					AssertEquals("Agreed delivery date is UTC so has 00:00 offset", TimeSpan.Zero, unit.AgreedDeliveryDateUTC.Value.Offset);
				}
				AssertEquals("Date acceptability", dateAcceptabilityCode, unit.DateAcceptability?.Code);
				AssertEquals("Release group code", releaseGroupCode, unit.ReleaseGroup?.Code);
				AssertEquals("Task set status code (Workflow Status)", taskSetStatusCode, unit.TaskSetStatus?.Code);

				if (taskDescriptions.Any())
				{
					AssertContainsExactElementsInAnyOrder("Task descriptions", taskDescriptions, unit.TaskCollection.Where(x => x.Description.HasValue).Select(x => x.Description.Value));
				}
				else
				{
					AssertNull("Not expecting any tasks, so the whole collection should be null. SAD!", unit.TaskCollection);
				}
			});
		}

		#endregion
	}
}
