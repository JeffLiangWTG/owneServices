using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class WorkflowTransferOrderSorterTest : BMSTestCaseWithFactory
	{
		#region Tag Nudge

		[TestDate(2013, 10, 24, 9, 0, 0)]
		public void TestSort_SortByReleaseDate()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "workflow1";
			workflow1.FH_ReleaseDateTime = ZDateTime.UtcNow;
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", releaseDateTime: ZDateTime.UtcNow.AddMinutes(60));
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3", releaseDateTime: ZDateTime.UtcNow.AddMinutes(120));

			AssertSortResults(
				Tuple.Create(string.Empty, workflow1),
				Tuple.Create(string.Empty, workflow2),
				Tuple.Create(string.Empty, workflow3)
				);
		}

		[TestDate(2013, 10, 24, 9, 0, 0)]
		public void TestSort_TagNudge_Workflow()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "AWE", "Shock and awe");
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup, "NEI", "NEEEiiiwwww", nudge: 1);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "workflow1";
			workflow1.FH_ReleaseDateTime = ZDateTime.UtcNow;
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", releaseDateTime: ZDateTime.UtcNow.AddMinutes(60));
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3", releaseDateTime: ZDateTime.UtcNow.AddMinutes(120));

			workflow2.AddTag(tag1);

			AssertSortResults(
				Tuple.Create(string.Empty, workflow2),
				Tuple.Create(string.Empty, workflow1),
				Tuple.Create(string.Empty, workflow3)
				);
		}

		[TestDate(2013, 10, 24, 9, 0, 0)]
		public void TestSort_TagNudge_JobHeader()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "AWE", "Shock and awe");
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup, "NEI", "NEEEiiiwwww", nudge: 1);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "workflow1";
			workflow1.FH_ReleaseDateTime = ZDateTime.UtcNow;

			var jobHeader2 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow2 = jobHeader2.ProcessHeaders[0];
			workflow2.FH_CompletionStatement = "workflow2";
			workflow2.FH_ReleaseDateTime = ZDateTime.UtcNow.AddMinutes(60);

			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3", releaseDateTime: ZDateTime.UtcNow.AddMinutes(120));

			jobHeader2.AddTag(tag1);

			AssertSortResults(
				Tuple.Create(string.Empty, workflow2),
				Tuple.Create(string.Empty, workflow1),
				Tuple.Create(string.Empty, workflow3)
				);
		}

		[TestDate(2013, 10, 24, 9, 0, 0)]
		public void TestSort_TagNudge_OnlyDistinctMagnitudesAccumulate()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "DUM");
			var tagGroup = BMSTestHelper.CreateTagDefinition(Factory, "AWE", "Shock and awe");
			var tag_1nudge = BMSTestHelper.CreateTagMagnitude(tagGroup, "NEI", "NEEEiiiwwww", nudge: 1);
			var tag_3nudge = BMSTestHelper.CreateTagMagnitude(tagGroup, "OBB", "Olay Biscuit Barrel", nudge: 3);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_CompletionStatement = "workflow1";
			workflow1.FH_ReleaseDateTime = ZDateTime.UtcNow;
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2", releaseDateTime: ZDateTime.UtcNow.AddMinutes(60));
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3", releaseDateTime: ZDateTime.UtcNow.AddMinutes(120));

			workflow1.AddTag(tag_3nudge);

			for (int i = 0; i < 5; i++)
			{
				workflow3.AddTag(tag_1nudge);
			}

			AssertSortResults(
				Tuple.Create(string.Empty, workflow3),
				Tuple.Create(string.Empty, workflow1),
				Tuple.Create(string.Empty, workflow2));
		}

		#endregion

		#region DeliveryDate

		void SetRegistryToOrderReleaseFirstByAgreedDeliveryDate()
		{
			BMSRegistry.Instance.UseDateOrderingForReleaseGate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		public void TestAgreedDeliveryDateWorkflow()
		{
			SetRegistryToOrderReleaseFirstByAgreedDeliveryDate();

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflowAndTask(jobHeader, "workflow1", ZDateTime.UtcNow, DateAcceptabilityList.Codes.SharpStartSharpFinish, 60, ZDateTime.UtcNow);
			var workflow2 = CreateWorkflowAndTask(jobHeader, "workflow2", ZDateTime.UtcNow.AddHours(2), DateAcceptabilityList.Codes.SharpStartSharpFinish, 60, ZDateTime.UtcNow.AddHours(-1));
			var workflow3 = CreateWorkflowAndTask(jobHeader, "workflow3", ZDateTime.UtcNow.AddHours(4), DateAcceptabilityList.Codes.SharpStartSharpFinish, 60, ZDateTime.UtcNow.AddHours(-2));
			var workflow4 = CreateWorkflowAndTask(jobHeader, "workflow4", ZDateTime.Empty, DateAcceptabilityList.Codes.SharpStartSharpFinish, 60, ZDateTime.UtcNow.AddHours(-3));

			AssertSortResults(
				Tuple.Create(string.Empty, workflow1),
				Tuple.Create(string.Empty, workflow2),
				Tuple.Create(string.Empty, workflow3),
				Tuple.Create(string.Empty, workflow4)
				);
		}

		public void TestAgreedDeliveryDateWorkflowAndJob()
		{
			SetRegistryToOrderReleaseFirstByAgreedDeliveryDate();

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflowAndTask(jobHeader, "workflow1", ZDateTime.UtcNow.AddHours(6), DateAcceptabilityList.Codes.SharpStartSharpFinish, 60, ZDateTime.UtcNow);
			var workflow2 = CreateWorkflowAndTask(jobHeader, "workflow2", ZDateTime.UtcNow.AddHours(2), DateAcceptabilityList.Codes.SharpStartSharpFinish, 60, ZDateTime.UtcNow.AddHours(-1));

			var jobHeader2 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader2.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddHours(-5);
			var workflow21 = CreateWorkflowAndTask(jobHeader2, "workflow21", ZDateTime.Empty, DateAcceptabilityList.Codes.SharpStartSharpFinish, 60, ZDateTime.UtcNow);
			var workflow22 = CreateWorkflowAndTask(jobHeader2, "workflow22", ZDateTime.Empty, DateAcceptabilityList.Codes.SharpStartSharpFinish, 60, ZDateTime.UtcNow.AddHours(-1));

			AssertSortResults(
				Tuple.Create(string.Empty, workflow22),
				Tuple.Create(string.Empty, workflow21),
				Tuple.Create(string.Empty, workflow2),
				Tuple.Create(string.Empty, workflow1)
				);
		}

		public void TestAgreedDeliveryDateJob()
		{
			SetRegistryToOrderReleaseFirstByAgreedDeliveryDate();

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddHours(-5);
			var workflow1 = CreateWorkflowAndTask(jobHeader, "workflow1", ZDateTime.Empty, DateAcceptabilityList.Codes.SharpStartSharpFinish, 60, ZDateTime.UtcNow);
			var workflow2 = CreateWorkflowAndTask(jobHeader, "workflow2", ZDateTime.Empty, DateAcceptabilityList.Codes.SharpStartSharpFinish, 60, ZDateTime.UtcNow.AddHours(-1));

			var jobHeader2 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader2.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddDays(-2);
			var workflow21 = CreateWorkflowAndTask(jobHeader2, "workflow21", ZDateTime.Empty, DateAcceptabilityList.Codes.SharpStartSharpFinish, 60, ZDateTime.UtcNow, 10);
			var workflow22 = CreateWorkflowAndTask(jobHeader2, "workflow22", ZDateTime.Empty, DateAcceptabilityList.Codes.SharpStartSharpFinish, 60, ZDateTime.UtcNow, 20);

			AssertSortResults(
				Tuple.Create(string.Empty, workflow22),
				Tuple.Create(string.Empty, workflow21),
				Tuple.Create(string.Empty, workflow2),
				Tuple.Create(string.Empty, workflow1)
				);
		}

		public void TestAgreedDeliveryDateWorkflowOverrideJob()
		{
			SetRegistryToOrderReleaseFirstByAgreedDeliveryDate();

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddHours(-5);
			var workflow1 = CreateWorkflowAndTask(jobHeader, "workflow1", ZDateTime.Empty, DateAcceptabilityList.Codes.SharpStartSharpFinish, 60, ZDateTime.UtcNow);
			var workflow2 = CreateWorkflowAndTask(jobHeader, "workflow2", ZDateTime.UtcNow.AddDays(-3), DateAcceptabilityList.Codes.SharpStartSharpFinish, 60, ZDateTime.UtcNow.AddHours(-1));

			var jobHeader2 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			jobHeader2.FH_AgreedDeliveryDate = ZDateTime.UtcNow.AddDays(-2);
			var workflow21 = CreateWorkflowAndTask(jobHeader2, "workflow21", ZDateTime.Empty, DateAcceptabilityList.Codes.SharpStartSharpFinish, 60, ZDateTime.UtcNow, 10);
			var workflow22 = CreateWorkflowAndTask(jobHeader2, "workflow22", ZDateTime.Empty, DateAcceptabilityList.Codes.SharpStartSharpFinish, 60, ZDateTime.UtcNow, 20);

			AssertSortResults(
				Tuple.Create(string.Empty, workflow2),
				Tuple.Create(string.Empty, workflow22),
				Tuple.Create(string.Empty, workflow21),
				Tuple.Create(string.Empty, workflow1)
				);
		}

		public void TestAgreedDeliveryDateFallback()
		{
			SetRegistryToOrderReleaseFirstByAgreedDeliveryDate();

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflowAndTask(jobHeader, "workflow1", ZDateTime.Empty, DateAcceptabilityList.Codes.SharpStartSharpFinish, 60, ZDateTime.UtcNow);
			var jobHeader1 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow2 = CreateWorkflowAndTask(jobHeader1, "workflow2", ZDateTime.Empty, DateAcceptabilityList.Codes.SharpStartSharpFinish, 60, ZDateTime.UtcNow, 10);
			var jobHeader2 = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow3 = CreateWorkflowAndTask(jobHeader2, "workflow3", ZDateTime.Empty, DateAcceptabilityList.Codes.SharpStartSharpFinish, 60, ZDateTime.UtcNow);
			jobHeader2.FH_AgreedDeliveryDate = ZDateTime.UtcNow;

			AssertSortResults(
				Tuple.Create(string.Empty, workflow3),
				Tuple.Create(string.Empty, workflow2),
				Tuple.Create(string.Empty, workflow1)
				);
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		ProcessHeader CreateWorkflowAndTask(ProcessJobHeader jobHeader, string completionStatement, ZDateTime agreedDeliveryDate, ZString dateAcceptability, int taskDurationMinutes, ZDateTime releaseDateTime, short voteUpDownAmount = 0)
		{
			var workflow = jobHeader.ProcessHeaders.AddNew();
			workflow.FH_CompletionStatement = completionStatement;
			workflow.FH_AgreedDeliveryDate = agreedDeliveryDate;
			workflow.FH_DateAcceptability = dateAcceptability;
			workflow.FH_ReleaseDateTime = releaseDateTime;
			workflow.FH_VoteUpDownAmount = new ZShort(voteUpDownAmount);

			var task = jobHeader.Parent.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;
			task.P9_EstDuration = new ZInt(taskDurationMinutes).GetDateTimeFromMinutes();

			return workflow;
		}

		public void TestSort()
		{
			var system = CreateSystem("ORG");

			var priorityTags = BMSTestHelper.CreateTagDefinition(Factory, "PRI", "Priority Tags");
			var platinum = BMSTestHelper.CreateTagMagnitude(priorityTags, "PLT", nudge: 100);
			var green = BMSTestHelper.CreateTagMagnitude(priorityTags, "GRN", nudge: 1000);

			var job1 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var workflow1_1 = jobHeader1.ProcessHeaders.AddNew();
			workflow1_1.FH_CompletionStatement = "workflow1_1";
			workflow1_1.AddTag(green);
			var workflow1_2 = jobHeader1.ProcessHeaders.AddNew();
			workflow1_2.FH_CompletionStatement = "workflow1_2";
			workflow1_2.AddTag(platinum);

			// No closed prereqs, job creation date is today
			var job2 = Factory.NewWithValidTestData<OrgHeader>();
			job2.OH_SystemCreateTimeUtc = ZDateTime.Now;
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);
			var workflow2_1 = jobHeader2.ProcessHeaders.AddNew();
			workflow2_1.FH_CompletionStatement = "workflow2_1";
			var workflow2_2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2_2.FH_CompletionStatement = "workflow2_2";
			var task2_1 = job2.WorkflowItems.AddNew();
			task2_1.P9_FH_ProcessHeader = workflow2_1.PK;
			var task2_2 = job2.WorkflowItems.AddNew();
			task2_2.P9_FH_ProcessHeader = workflow2_2.PK;

			// Closed prereq
			var job3 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader3 = ProcessJobHeader.GetForParent(job3, Factory);
			var workflow3_1 = jobHeader3.ProcessHeaders.AddNew();
			workflow3_1.FH_CompletionStatement = "workflow3_1";
			var workflow3_2 = jobHeader3.ProcessHeaders.AddNew();
			workflow3_2.FH_CompletionStatement = "workflow3_2";
			workflow3_1.GetOrCreateDependencyLink(workflow3_2);
			var task3_1 = job2.WorkflowItems.AddNew();
			task3_1.P9_FH_ProcessHeader = workflow3_1.PK;
			task3_1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			var task3_2 = job2.WorkflowItems.AddNew();
			task3_2.P9_FH_ProcessHeader = workflow3_2.PK;

			// Older job creation date
			var job4 = Factory.NewWithValidTestData<OrgHeader>();
			job4.OH_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);
			var jobHeader4 = ProcessJobHeader.GetForParent(job4, Factory);
			var workflow4 = jobHeader4.ProcessHeaders.AddNew();
			workflow4.FH_CompletionStatement = "workflow4";

			//Start date ordering
			workflow3_2.FH_ReleaseDateTime = ZDateTime.Now;
			workflow4.FH_ReleaseDateTime = ZDateTime.Now.AddDays(1);
			workflow2_1.FH_ReleaseDateTime = ZDateTime.Now.AddDays(2);
			workflow2_2.FH_ReleaseDateTime = ZDateTime.Now.AddDays(2);

			workflow2_1.FH_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-1);
			workflow2_2.FH_SystemCreateTimeUtc = ZDateTime.Now;
			workflow3_1.FH_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(-2);

			AssertSortResults(
				Tuple.Create("Green workflow", workflow1_1),
				Tuple.Create("Platinum workflow", workflow1_2),
				Tuple.Create("Has pre-req closed - Oldest job", workflow3_2),
				Tuple.Create("Second oldest job", workflow4),
				Tuple.Create("Equal priority workflow, same sort date, earlier create time", workflow2_1),
				Tuple.Create("Equal priority workflow, same sort date, later create time", workflow2_2),
				Tuple.Create("Equal priority workflow, no sort date", workflow3_1)
				);

			// Test Nudge down
			workflow1_1.FH_VoteUpDownAmount = -950;

			AssertSortResults(
				Tuple.Create("Platinum workflow - moves up since workflow1_1 has been nudged down", workflow1_2),
				Tuple.Create("Green workflow - nudged down", workflow1_1),
				Tuple.Create("Has pre-req closed - Oldest job", workflow3_2),
				Tuple.Create("Second oldest job", workflow4),
				Tuple.Create("Equal priority workflow, same sort date, earlier create time", workflow2_1),
				Tuple.Create("Equal priority workflow, same sort date, later create time", workflow2_2),
				Tuple.Create("Equal priority workflow, no sort date", workflow3_1)
				);

			SetRegistryToOrderReleaseFirstByAgreedDeliveryDate();
			workflow3_1.FH_AgreedDeliveryDate = ZDateTime.Now.AddDays(-2);
			workflow2_2.FH_AgreedDeliveryDate = ZDateTime.Now.AddMinutes(1);

			AssertSortResults(
				Tuple.Create("Earliest AgreedDeliveryDate First", workflow3_1),
				Tuple.Create("Add in the future", workflow2_2),
				Tuple.Create("Platinum workflow - moves up since workflow1_1 has been nudged down", workflow1_2),
				Tuple.Create("Green workflow - nudged down", workflow1_1),
				Tuple.Create("Has pre-req closed - Oldest job", workflow3_2),
				Tuple.Create("Second oldest job", workflow4),
				Tuple.Create("Equal priority workflow, same sort date, earlier create time", workflow2_1)
				);
		}

		public void TestSortByStartDate()
		{
			var system = CreateSystem("ORG");

			//Create workflows
			var job1 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var workflow1 = jobHeader1.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";

			var job2 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";

			var job3 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader3 = ProcessJobHeader.GetForParent(job3, Factory);
			var workflow3 = jobHeader3.ProcessHeaders.AddNew();
			workflow3.FH_CompletionStatement = "workflow3";

			var job4 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader4 = ProcessJobHeader.GetForParent(job4, Factory);
			var workflow4 = jobHeader4.ProcessHeaders.AddNew();
			workflow4.FH_CompletionStatement = "workflow4";

			//Set start dates of workflows
			workflow1.FH_ReleaseDateTime = ZDateTime.Now;
			workflow2.FH_ReleaseDateTime = ZDateTime.Now.AddDays(1);
			workflow3.FH_ReleaseDateTime = ZDateTime.Now.AddDays(2);
			workflow4.FH_ReleaseDateTime = ZDateTime.Now.AddDays(3);

			var workflows = WorkflowTransferOrderSorter.Sort(new[] { workflow4, workflow3, workflow2, workflow1 }).ToArray();

			AssertEquals(workflow1, workflows[0]);
			AssertEquals(workflow2, workflows[1]);
			AssertEquals(workflow3, workflows[2]);
			AssertEquals(workflow4, workflows[3]);

			//DoNotStartBeforeDate should have higher priority than ReleaseDateTime
			workflow1.FH_DoNotStartBeforeDate = ZDateTime.Now.AddDays(10);
			workflow3.FH_ReleaseDateTime = ZDateTime.Empty;

			var workflows2 = WorkflowTransferOrderSorter.Sort(new[] { workflow4, workflow3, workflow2, workflow1 }).ToArray();

			AssertEquals(workflow2, workflows2[0]);
			AssertEquals(workflow4, workflows2[1]);
			AssertEquals(workflow1, workflows2[2]);
			AssertEquals(workflow3, workflows2[3]);
		}

		public void TestSortAndSetReleaseSequence()
		{
			var system = CreateSystem("ORG");
			system.FS_Name = "MyBM";

			var priorityTags = BMSTestHelper.CreateTagDefinition(Factory, "PRI", "Priority Tags");
			var platinum = BMSTestHelper.CreateTagMagnitude(priorityTags, "PLT", nudge: 100);
			var green = BMSTestHelper.CreateTagMagnitude(priorityTags, "GRN", nudge: 1000);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";
			workflow1.AddTag(green);
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";
			workflow2.AddTag(platinum);

			Factory.Save();

			WorkflowTransferOrderSorter.SortAndSetReleaseSequence(Factory, new[] { workflow1, workflow2, jobHeader });

			AssertEquals("MyBM:00001", workflow1.ReleaseSequence);
			AssertEquals("MyBM:00002", workflow2.ReleaseSequence);
			Assert(string.IsNullOrEmpty(jobHeader.ReleaseSequence));
		}

		public void TestShouldNotSortWorkflowsWithNullBM()
		{
			var job1 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader1 = ProcessJobHeader.GetForParent(job1, Factory);
			var workflow1 = jobHeader1.ProcessHeaders.AddNew();
			workflow1.FH_CompletionStatement = "workflow1";

			var job2 = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader2 = ProcessJobHeader.GetForParent(job2, Factory);
			var workflow2 = jobHeader2.ProcessHeaders.AddNew();
			workflow2.FH_CompletionStatement = "workflow2";

			Factory.Save();

			AssertNull(workflow1.BMSystem);
			AssertNoExceptionThrown(() => WorkflowTransferOrderSorter.SortAndSetReleaseSequence(Factory, new[] { workflow1, workflow2 }));
		}

		public void TestSort_NullSystemOnAllWorkflows()
		{
			var workflow1 = CreateJobHeader<SalesEnquiry>().ProcessHeaders[0];
			var workflow2 = CreateJobHeader<OrgHeader>().ProcessHeaders[0];

			AssertNoExceptionThrown(() => WorkflowTransferOrderSorter.Sort(new[] { workflow1, workflow2 }));
		}

		public void TestSort_WithNullSequence_ShouldReturnEmptyEnumerable()
		{
			var result = WorkflowTransferOrderSorter.Sort((ProcessHeader[])null);
			AssertNotNull(result);
			AssertEquals(0, result.Count());
		}

		void AssertSortResults(params Tuple<string, ProcessHeader>[] workflowMessagePairs)
		{
			var results = WorkflowTransferOrderSorter.Sort(workflowMessagePairs.Select(x => x.Item2).Reverse()).ToArray();
			AssertEquals(workflowMessagePairs.Length, results.Length);

			CombineAssertions(() =>
			{
				for (int i = 0; i < results.Length; i++)
				{
					var pair = workflowMessagePairs[i];
					AssertEquals(pair.Item1, pair.Item2, results[i]);
				}
			});

			Factory.Save();

			var resultsFromDb = WorkflowTransferOrderSorter.Sort(workflowMessagePairs.Select(x => new ReleaseGateRequest(x.Item2, config.ComponentLink)).Reverse()).ToArray();
			AssertEquals(workflowMessagePairs.Length, resultsFromDb.Length);

			CombineAssertions(() =>
			{
				for (int i = 0; i < resultsFromDb.Length; i++)
				{
					var pair = workflowMessagePairs[i];
					AssertEquals(pair.Item1, pair.Item2.PK, resultsFromDb[i].WorkflowPK);
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			config = TestConfigsHelper.CreateSchematicTestConfig(Factory, "SHP");
		}

		SchematicTestConfig config;
	}
}
