using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Integration;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class Report_ContainmentBarrierOutcomesTest : BMSTestCaseWithFactory
	{
		[GuiTest, TestDate(2015, 7, 14)]
		public void TestReportContents_ShouldBeRestrictedBySpecifiedFilters()
		{
			MasterFilesTestHelper.AddTaskTypesToRegistry(WorkflowDescriptors.WorkItemWorkflowDescriptorCode, "ONE", "TWO", "THR");
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("THR", WorkflowDescriptors.WorkItemWorkflowDescriptorCode);
			MasterFilesTestHelper.AddIterationReasonToRegistry(WorkflowDescriptors.WorkItemWorkflowDescriptorCode, ":)", "I like it!");

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);
			config.ReleaseGroup.GG_Code = "PaveTeam100Yrs";
			var otherReleaseGroup = BMSTestHelper.CreateGroup(Factory, "Trip Hop");
			BMSTestHelper.CreateReleaseGroup(config.System, otherReleaseGroup);

			var resource1 = BMSTestHelper.CreateStaff(Factory, "AAA", "3D");
			var resource2 = BMSTestHelper.CreateStaff(Factory, "BBB", "Daddy G");
			var resource3 = BMSTestHelper.CreateStaff(Factory, "CCC", "Tricky");

			var workItem = (IWorkItem)BMSTestHelper.CreateJob<IWorkItem>(Factory);
			workItem.WKI_WorkItemNumber = "WI00000100";
			workItem.WKI_Summary = "Massive Attack";

			var jobHeader = BMSTestHelper.CreateJobHeader((IWorkflowProvider)workItem);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Day", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Dreaming", releaseGroupPK: otherReleaseGroup.PK);

			var task1_1 = BMSTestHelper.CreateTask(workflow1, resource1.GS_Code, taskType: "ONE", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 1, description: "Task One");
			var task1_2 = BMSTestHelper.CreateTask(workflow1, resource2.GS_Code, taskType: "TWO", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 2, description: "Task Two");
			var task1_3 = BMSTestHelper.CreateTask(workflow1, resource3.GS_Code, taskType: "THR", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 3, description: "Task Three");

			var task2_1 = BMSTestHelper.CreateTask(workflow2, resource3.GS_Code, taskType: "ONE", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 1, description: "Task One");
			var task2_2 = BMSTestHelper.CreateTask(workflow2, resource2.GS_Code, taskType: "TWO", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 2, description: "Task Two");
			var task2_3 = BMSTestHelper.CreateTask(workflow2, resource1.GS_Code, taskType: "THR", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 3, description: "Task Three");

			Factory.Save();

			// Create two nested quality iterations for the first workflow, passing the final one.

			TestDateAttribute.AddDays(1);

			var iterationWorkflow1_1 = CreateQualityIteration(task1_1, task1_3, resourceUnderReviewStaffCode: resource2.GS_Code, iterationReasonCode: ":)");
			Factory.Save();

			TestDateAttribute.AddDays(1);

			iterationWorkflow1_1.Tasks.ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Closed);
			var iterationWorkflow1_2 = CreateQualityIteration(iterationWorkflow1_1.Tasks.Single(t => t.P9_Type == "TWO"), iterationWorkflow1_1.Tasks.Single(t => t.P9_Type == "THR"), resourceUnderReviewStaffCode: resource2.GS_Code, iterationReasonCode: ":)");
			Factory.Save();

			TestDateAttribute.AddDays(1);

			iterationWorkflow1_2.Tasks.ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Closed);
			CreatePassedContainmentBarrierRecord(iterationWorkflow1_2.Tasks.Last());
			Factory.Save();

			// Create two nested quality iterations for the second workflow, passing the final one.

			TestDateAttribute.AddDays(1);

			var iterationWorkflow2_1 = CreateQualityIteration(task2_1, task2_3, resourceUnderReviewStaffCode: resource3.GS_Code, iterationReasonCode: ":)");
			Factory.Save();

			TestDateAttribute.AddDays(1);

			iterationWorkflow2_1.Tasks.ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Closed);
			var iterationWorkflow2_2 = CreateQualityIteration(iterationWorkflow2_1.Tasks.Single(t => t.P9_Type == "TWO"), iterationWorkflow2_1.Tasks.Single(t => t.P9_Type == "THR"), resourceUnderReviewStaffCode: resource2.GS_Code, iterationReasonCode: ":)");
			Factory.Save();

			TestDateAttribute.AddDays(1);

			iterationWorkflow2_2.Tasks.ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Closed);
			CreatePassedContainmentBarrierRecord(iterationWorkflow2_2.Tasks.Last());
			Factory.Save();

			var options = new ReportOptions
			{
				ReleaseGroupPK = config.ReleaseGroup.PK,
			};

			AssertRows("Filtering by a release group only (no date range)", options,
				new RowDetails
				{
					JobNumber = "WI00000100",
					ReleaseGroupCode = "PaveTeam100Yrs",
					ReviewWorkflowDescription = "Day",
					ReviewTaskID = "T00001002",
					ReviewTaskSequence = 3,
					ReviewTaskType = "THR",
					ReviewTaskDescription = "Task Three",
					ReviewerStaffCode = "CCC",
					ReviewerStaffName = "Tricky",
					ReviewCompletedTimeUTC = new ZDateTime(2015, 7, 15, 0, 0, 0),
					ResourceUnderReviewStaffCode = "BBB",
					ResourceUnderReviewStaffName = "Daddy G",
					ContainmentBarrierOutcome = "ITR",
					IterationReason = ":)",
					IterationWorkflowDescription = "Day (Quality Iteration 1)",
					IterationDepth = 1,
					IteratedToWorkflowDescription = "Day",
					IteratedToTaskID = "T00001000",
					IteratedToTaskSequence = 1,
					IteratedToTaskType = "ONE",
					IteratedToTaskDescription = "Task One",
					IteratedToTaskStaffCode = "AAA",
					IteratedToTaskStaffName = "3D",
				},
				new RowDetails
				{
					JobNumber = "WI00000100",
					ReleaseGroupCode = "PaveTeam100Yrs",
					ReviewWorkflowDescription = "Day (Quality Iteration 1)",
					ReviewTaskID = "T00001008",
					ReviewTaskSequence = 6,
					ReviewTaskType = "THR",
					ReviewTaskDescription = "Task Three",
					ReviewerStaffCode = "CCC",
					ReviewerStaffName = "Tricky",
					ReviewCompletedTimeUTC = new ZDateTime(2015, 7, 16, 0, 0, 0),
					ResourceUnderReviewStaffCode = "BBB",
					ResourceUnderReviewStaffName = "Daddy G",
					ContainmentBarrierOutcome = "ITR",
					IterationReason = ":)",
					IterationWorkflowDescription = "Day (Quality Iteration 2)",
					IterationDepth = 2,
					IteratedToWorkflowDescription = "Day (Quality Iteration 1)",
					IteratedToTaskID = "T00001007",
					IteratedToTaskSequence = 5,
					IteratedToTaskType = "TWO",
					IteratedToTaskDescription = "Task Two",
					IteratedToTaskStaffCode = "BBB",
					IteratedToTaskStaffName = "Daddy G",
				},
				new RowDetails
				{
					JobNumber = "WI00000100",
					ReleaseGroupCode = "PaveTeam100Yrs",
					ReviewWorkflowDescription = "Day (Quality Iteration 2)",
					ReviewTaskID = "T00001010",
					ReviewTaskSequence = 8,
					ReviewTaskType = "THR",
					ReviewTaskDescription = "Task Three",
					ReviewerStaffCode = "CCC",
					ReviewerStaffName = "Tricky",
					ReviewCompletedTimeUTC = new ZDateTime(2015, 7, 17, 0, 0, 0),
					ResourceUnderReviewStaffCode = "BBB",
					ResourceUnderReviewStaffName = "Daddy G",
					ContainmentBarrierOutcome = "PAS",
					IterationReason = "",
					IterationWorkflowDescription = "",
					IterationDepth = 0,
					IteratedToWorkflowDescription = "",
					IteratedToTaskID = "",
					IteratedToTaskSequence = -1,
					IteratedToTaskType = "",
					IteratedToTaskDescription = "",
					IteratedToTaskStaffCode = "",
					IteratedToTaskStaffName = "",
				}
				);

			options.ReleaseGroupPK = ZGuid.Empty;
			options.ReportPeriodStartUtc = new ZDateTime(2015, 7, 17);
			options.ReportPeriodEndUtc = new ZDateTime(2015, 7, 18);

			AssertRows("Filtering by just a time range", options,
				new RowDetails
				{
					JobNumber = "WI00000100",
					ReleaseGroupCode = "PaveTeam100Yrs",
					ReviewWorkflowDescription = "Day (Quality Iteration 2)",
					ReviewTaskID = "T00001010",
					ReviewTaskSequence = 8,
					ReviewTaskType = "THR",
					ReviewTaskDescription = "Task Three",
					ReviewerStaffCode = "CCC",
					ReviewerStaffName = "Tricky",
					ReviewCompletedTimeUTC = new ZDateTime(2015, 7, 17, 0, 0, 0),
					ResourceUnderReviewStaffCode = "BBB",
					ResourceUnderReviewStaffName = "Daddy G",
					ContainmentBarrierOutcome = "PAS",
					IterationReason = "",
					IterationWorkflowDescription = "",
					IterationDepth = 0,
					IteratedToWorkflowDescription = "",
					IteratedToTaskID = "",
					IteratedToTaskSequence = -1,
					IteratedToTaskType = "",
					IteratedToTaskDescription = "",
					IteratedToTaskStaffCode = "",
					IteratedToTaskStaffName = "",
				},
				new RowDetails
				{
					JobNumber = "WI00000100",
					ReleaseGroupCode = "Trip Hop",
					ReviewWorkflowDescription = "Dreaming",
					ReviewTaskID = "T00001005",
					ReviewTaskSequence = 3,
					ReviewTaskType = "THR",
					ReviewTaskDescription = "Task Three",
					ReviewerStaffCode = "AAA",
					ReviewerStaffName = "3D",
					ReviewCompletedTimeUTC = new ZDateTime(2015, 7, 18, 0, 0, 0),
					ResourceUnderReviewStaffCode = "CCC",
					ResourceUnderReviewStaffName = "Tricky",
					ContainmentBarrierOutcome = "ITR",
					IterationReason = ":)",
					IterationWorkflowDescription = "Dreaming (Quality Iteration 1)",
					IterationDepth = 1,
					IteratedToWorkflowDescription = "Dreaming",
					IteratedToTaskID = "T00001003",
					IteratedToTaskSequence = 1,
					IteratedToTaskType = "ONE",
					IteratedToTaskDescription = "Task One",
					IteratedToTaskStaffCode = "CCC",
					IteratedToTaskStaffName = "Tricky",
				}
				);

			options.ReportPeriodStartUtc = new ZDateTime(2015, 7, 14);
			options.ReportPeriodEndUtc = new ZDateTime(2015, 7, 16);
			options.ReviewerStaffCode = "CCC";
			options.RevieweeStaffCode = "BBB";

			AssertRows("Filtering by a reviewer, reviewee and a time range", options,
				new RowDetails
				{
					JobNumber = "WI00000100",
					ReleaseGroupCode = "PaveTeam100Yrs",
					ReviewWorkflowDescription = "Day",
					ReviewTaskID = "T00001002",
					ReviewTaskSequence = 3,
					ReviewTaskType = "THR",
					ReviewTaskDescription = "Task Three",
					ReviewerStaffCode = "CCC",
					ReviewerStaffName = "Tricky",
					ReviewCompletedTimeUTC = new ZDateTime(2015, 7, 15, 0, 0, 0),
					ResourceUnderReviewStaffCode = "BBB",
					ResourceUnderReviewStaffName = "Daddy G",
					ContainmentBarrierOutcome = "ITR",
					IterationReason = ":)",
					IterationWorkflowDescription = "Day (Quality Iteration 1)",
					IterationDepth = 1,
					IteratedToWorkflowDescription = "Day",
					IteratedToTaskID = "T00001000",
					IteratedToTaskSequence = 1,
					IteratedToTaskType = "ONE",
					IteratedToTaskDescription = "Task One",
					IteratedToTaskStaffCode = "AAA",
					IteratedToTaskStaffName = "3D",
				},
				new RowDetails
				{
					JobNumber = "WI00000100",
					ReleaseGroupCode = "PaveTeam100Yrs",
					ReviewWorkflowDescription = "Day (Quality Iteration 1)",
					ReviewTaskID = "T00001008",
					ReviewTaskSequence = 6,
					ReviewTaskType = "THR",
					ReviewTaskDescription = "Task Three",
					ReviewerStaffCode = "CCC",
					ReviewerStaffName = "Tricky",
					ReviewCompletedTimeUTC = new ZDateTime(2015, 7, 16, 0, 0, 0),
					ResourceUnderReviewStaffCode = "BBB",
					ResourceUnderReviewStaffName = "Daddy G",
					ContainmentBarrierOutcome = "ITR",
					IterationReason = ":)",
					IterationWorkflowDescription = "Day (Quality Iteration 2)",
					IterationDepth = 2,
					IteratedToWorkflowDescription = "Day (Quality Iteration 1)",
					IteratedToTaskID = "T00001007",
					IteratedToTaskSequence = 5,
					IteratedToTaskType = "TWO",
					IteratedToTaskDescription = "Task Two",
					IteratedToTaskStaffCode = "BBB",
					IteratedToTaskStaffName = "Daddy G",
				}
				);

			options.ReportPeriodEndUtc = new ZDateTime(2015, 8, 14);
			options.ReviewerStaffCode = "";
			options.RevieweeStaffCode = "";
			options.IteratedToStaffCode = "CCC";

			AssertRows("Filtering by a resource assigned to the task that was iterated back to", options,
				new RowDetails
				{
					JobNumber = "WI00000100",
					ReleaseGroupCode = "Trip Hop",
					ReviewWorkflowDescription = "Dreaming",
					ReviewTaskID = "T00001005",
					ReviewTaskSequence = 3,
					ReviewTaskType = "THR",
					ReviewTaskDescription = "Task Three",
					ReviewerStaffCode = "AAA",
					ReviewerStaffName = "3D",
					ReviewCompletedTimeUTC = new ZDateTime(2015, 7, 18, 0, 0, 0),
					ResourceUnderReviewStaffCode = "CCC",
					ResourceUnderReviewStaffName = "Tricky",
					ContainmentBarrierOutcome = "ITR",
					IterationReason = ":)",
					IterationWorkflowDescription = "Dreaming (Quality Iteration 1)",
					IterationDepth = 1,
					IteratedToWorkflowDescription = "Dreaming",
					IteratedToTaskID = "T00001003",
					IteratedToTaskSequence = 1,
					IteratedToTaskType = "ONE",
					IteratedToTaskDescription = "Task One",
					IteratedToTaskStaffCode = "CCC",
					IteratedToTaskStaffName = "Tricky",
				}
				);
		}

		[GuiTest, TestDate(2015, 7, 14)]
		public void TestReportContents_UnFiltered()
		{
			MasterFilesTestHelper.AddTaskTypesToRegistry(WorkflowDescriptors.WorkItemWorkflowDescriptorCode, "CDU", "CDF", "CBC");
			MasterFilesTestHelper.AddTaskTypesToRegistry(WorkflowDescriptors.ProjectWorkflowDescriptorCode, "CNT", "INV", "CBA");

			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CBC", WorkflowDescriptors.WorkItemWorkflowDescriptorCode);
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("CBA", WorkflowDescriptors.ProjectWorkflowDescriptorCode);

			MasterFilesTestHelper.AddIterationReasonToRegistry(WorkflowDescriptors.WorkItemWorkflowDescriptorCode, ":)", "I like it!");
			MasterFilesTestHelper.AddIterationReasonToRegistry(WorkflowDescriptors.WorkItemWorkflowDescriptorCode, ":(", "I don't like it.");

			MasterFilesTestHelper.AddIterationReasonToRegistry(WorkflowDescriptors.ProjectWorkflowDescriptorCode, ":D", "I love it!");
			MasterFilesTestHelper.AddIterationReasonToRegistry(WorkflowDescriptors.ProjectWorkflowDescriptorCode, ":|", "I am ambivalent towards it.");

			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "MOU", "The mouse is orange");
			var resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "ELE", "The elephant is blue");
			var resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DUC", "The duck is yellow");

			var group1 = BMSTestHelper.CreateGroup(Factory, "Lachgeschichten");
			var group2 = BMSTestHelper.CreateGroup(Factory, "Sachgeschichten");

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory, new[] { WorkflowDescriptors.WorkItemWorkflowDescriptorCode, WorkflowDescriptors.ProjectWorkflowDescriptorCode });
			var workItem = (IWorkItem)BMSTestHelper.CreateJob<IWorkItem>(Factory);
			var project = (IProject)BMSTestHelper.CreateJob<IProject>(Factory);

			workItem.WKI_WorkItemNumber = "WI00000100";
			workItem.WKI_Summary = "Käpt'n Blaubär";
			project.WKP_ProjectNumber = "PRJ00000100";
			project.WKP_Summary = "Shaun das Schaf";

			var jobHeader1 = BMSTestHelper.CreateJobHeader((IWorkflowProvider)workItem);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader1, "Schnappi", releaseGroupPK: group1.PK);
			var task1_1 = BMSTestHelper.CreateTask(workflow1, resource1.GS_Code, taskType: "CDU", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 1, description: "Task One");
			var task1_2 = BMSTestHelper.CreateTask(workflow1, resource1.GS_Code, taskType: "CDF", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 2, description: "Task Two");
			var task1_3 = BMSTestHelper.CreateTask(workflow1, resource2.GS_Code, taskType: "CBC", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 3, description: "Task Three");

			var jobHeader2 = BMSTestHelper.CreateJobHeader((IWorkflowProvider)project);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader2, "Das kleine Krokodil", releaseGroupPK: group2.PK);
			var task2_1 = BMSTestHelper.CreateTask(workflow2, resource1.GS_Code, taskType: "INV", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 10, description: "Task Ten");
			var task2_2 = BMSTestHelper.CreateTask(workflow2, resource1.GS_Code, taskType: "INV", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 20, description: "Task Twenty");
			var task2_3 = BMSTestHelper.CreateTask(workflow2, resource2.GS_Code, taskType: "CNT", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 30, description: "Task Thirty");
			var task2_4 = BMSTestHelper.CreateTask(workflow2, resource3.GS_Code, taskType: "CBA", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 40, description: "Task Forty");

			Factory.Save();

			TestDateAttribute.AddMinutes(1);

			var iterationWorkflow1_1 = CreateQualityIteration(task1_1, task1_3, resourceUnderReviewStaffCode: resource1.GS_Code, iterationReasonCode: ":(");
			Factory.Save();

			TestDateAttribute.AddMinutes(1);

			iterationWorkflow1_1.Tasks.ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Closed);
			var iterationWorkflow1_2 = CreateQualityIteration(iterationWorkflow1_1.Tasks.Single(t => t.P9_Type == "CDF"), iterationWorkflow1_1.Tasks.Single(t => t.P9_Type == "CBC"), resourceUnderReviewStaffCode: resource1.GS_Code, iterationReasonCode: ":)");
			Factory.Save();

			TestDateAttribute.AddMinutes(1);

			iterationWorkflow1_2.Tasks.ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Closed);
			CreatePassedContainmentBarrierRecord(iterationWorkflow1_2.Tasks.Last());
			Factory.Save();

			TestDateAttribute.AddMinutes(1);

			var iterationWorkflow2_1 = CreateQualityIteration(task2_2, task2_4, resourceUnderReviewStaffCode: resource2.GS_Code, iterationReasonCode: ":|");
			Factory.Save();

			TestDateAttribute.AddMinutes(1);

			iterationWorkflow2_1.Tasks.ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Closed);
			var iterationWorkflow2_2 = CreateQualityIteration(iterationWorkflow2_1.Tasks.Single(t => t.P9_Type == "CNT"), iterationWorkflow2_1.Tasks.Single(t => t.P9_Type == "CBA"), resourceUnderReviewStaffCode: resource2.GS_Code, iterationReasonCode: ":D");
			Factory.Save();

			TestDateAttribute.AddMinutes(1);

			iterationWorkflow2_2.Tasks.ForEach(t => t.P9_Status = ProcessTaskStatusCodeList.Codes.Closed);
			CreatePassedContainmentBarrierRecord(iterationWorkflow2_2.Tasks.Last());
			Factory.Save();

			AssertRows("Report filters encompasing all data points should not exclude any rows", new ReportOptions
			{
				ReportPeriodStartUtc = ZDateTime.UtcNow.AddDays(-1),
				ReportPeriodEndUtc = ZDateTime.UtcNow.AddDays(1),
			},
				new RowDetails
				{
					JobNumber = "PRJ00000100",
					ReleaseGroupCode = "Sachgeschichten",
					ReviewWorkflowDescription = "Das kleine Krokodil",
					ReviewTaskID = "T00001006",
					ReviewTaskSequence = 40,
					ReviewTaskType = "CBA",
					ReviewTaskDescription = "Task Forty",
					ReviewerStaffCode = "DUC",
					ReviewerStaffName = "The duck is yellow",
					ReviewCompletedTimeUTC = new ZDateTime(2015, 7, 14, 0, 4, 0),
					ResourceUnderReviewStaffCode = "ELE",
					ResourceUnderReviewStaffName = "The elephant is blue",
					ContainmentBarrierOutcome = "ITR",
					IterationReason = ":|",
					IterationWorkflowDescription = "Das kleine Krokodil (Quality Iteration 1)",
					IterationDepth = 1,
					IteratedToWorkflowDescription = "Das kleine Krokodil",
					IteratedToTaskID = "T00001004",
					IteratedToTaskSequence = 20,
					IteratedToTaskType = "INV",
					IteratedToTaskDescription = "Task Twenty",
					IteratedToTaskStaffCode = "MOU",
					IteratedToTaskStaffName = "The mouse is orange",
				},
				new RowDetails
				{
					JobNumber = "PRJ00000100",
					ReleaseGroupCode = "Sachgeschichten",
					ReviewWorkflowDescription = "Das kleine Krokodil (Quality Iteration 1)",
					ReviewTaskID = "T00001014",
					ReviewTaskSequence = 43,
					ReviewTaskType = "CBA",
					ReviewTaskDescription = "Task Forty",
					ReviewerStaffCode = "DUC",
					ReviewerStaffName = "The duck is yellow",
					ReviewCompletedTimeUTC = new ZDateTime(2015, 7, 14, 0, 5, 0),
					ResourceUnderReviewStaffCode = "ELE",
					ResourceUnderReviewStaffName = "The elephant is blue",
					ContainmentBarrierOutcome = "ITR",
					IterationReason = ":D",
					IterationWorkflowDescription = "Das kleine Krokodil (Quality Iteration 2)",
					IterationDepth = 2,
					IteratedToWorkflowDescription = "Das kleine Krokodil (Quality Iteration 1)",
					IteratedToTaskID = "T00001013",
					IteratedToTaskSequence = 42,
					IteratedToTaskType = "CNT",
					IteratedToTaskDescription = "Task Thirty",
					IteratedToTaskStaffCode = "ELE",
					IteratedToTaskStaffName = "The elephant is blue",
				},
				new RowDetails
				{
					JobNumber = "PRJ00000100",
					ReleaseGroupCode = "Sachgeschichten",
					ReviewWorkflowDescription = "Das kleine Krokodil (Quality Iteration 2)",
					ReviewTaskID = "T00001016",
					ReviewTaskSequence = 45,
					ReviewTaskType = "CBA",
					ReviewTaskDescription = "Task Forty",
					ReviewerStaffCode = "DUC",
					ReviewerStaffName = "The duck is yellow",
					ReviewCompletedTimeUTC = new ZDateTime(2015, 7, 14, 0, 6, 0),
					ResourceUnderReviewStaffCode = "ELE",
					ResourceUnderReviewStaffName = "The elephant is blue",
					ContainmentBarrierOutcome = "PAS",
					IterationReason = "",
					IterationWorkflowDescription = "",
					IterationDepth = 0,
					IteratedToWorkflowDescription = "",
					IteratedToTaskID = "",
					IteratedToTaskSequence = -1,
					IteratedToTaskType = "",
					IteratedToTaskDescription = "",
					IteratedToTaskStaffCode = "",
					IteratedToTaskStaffName = "",
				},
				new RowDetails
				{
					JobNumber = "WI00000100",
					ReleaseGroupCode = "Lachgeschichten",
					ReviewWorkflowDescription = "Schnappi",
					ReviewTaskID = "T00001002",
					ReviewTaskSequence = 3,
					ReviewTaskType = "CBC",
					ReviewTaskDescription = "Task Three",
					ReviewerStaffCode = "ELE",
					ReviewerStaffName = "The elephant is blue",
					ReviewCompletedTimeUTC = new ZDateTime(2015, 7, 14, 0, 1, 0),
					ResourceUnderReviewStaffCode = "MOU",
					ResourceUnderReviewStaffName = "The mouse is orange",
					ContainmentBarrierOutcome = "ITR",
					IterationReason = ":(",
					IterationWorkflowDescription = "Schnappi (Quality Iteration 1)",
					IterationDepth = 1,
					IteratedToWorkflowDescription = "Schnappi",
					IteratedToTaskID = "T00001000",
					IteratedToTaskSequence = 1,
					IteratedToTaskType = "CDU",
					IteratedToTaskDescription = "Task One",
					IteratedToTaskStaffCode = "MOU",
					IteratedToTaskStaffName = "The mouse is orange",
				},
				new RowDetails
				{
					JobNumber = "WI00000100",
					ReleaseGroupCode = "Lachgeschichten",
					ReviewWorkflowDescription = "Schnappi (Quality Iteration 1)",
					ReviewTaskID = "T00001009",
					ReviewTaskSequence = 6,
					ReviewTaskType = "CBC",
					ReviewTaskDescription = "Task Three",
					ReviewerStaffCode = "ELE",
					ReviewerStaffName = "The elephant is blue",
					ReviewCompletedTimeUTC = new ZDateTime(2015, 7, 14, 0, 2, 0),
					ResourceUnderReviewStaffCode = "MOU",
					ResourceUnderReviewStaffName = "The mouse is orange",
					ContainmentBarrierOutcome = "ITR",
					IterationReason = ":)",
					IterationWorkflowDescription = "Schnappi (Quality Iteration 2)",
					IterationDepth = 2,
					IteratedToWorkflowDescription = "Schnappi (Quality Iteration 1)",
					IteratedToTaskID = "T00001008",
					IteratedToTaskSequence = 5,
					IteratedToTaskType = "CDF",
					IteratedToTaskDescription = "Task Two",
					IteratedToTaskStaffCode = "MOU",
					IteratedToTaskStaffName = "The mouse is orange",
				},
				new RowDetails
				{
					JobNumber = "WI00000100",
					ReleaseGroupCode = "Lachgeschichten",
					ReviewWorkflowDescription = "Schnappi (Quality Iteration 2)",
					ReviewTaskID = "T00001011",
					ReviewTaskSequence = 8,
					ReviewTaskType = "CBC",
					ReviewTaskDescription = "Task Three",
					ReviewerStaffCode = "ELE",
					ReviewerStaffName = "The elephant is blue",
					ReviewCompletedTimeUTC = new ZDateTime(2015, 7, 14, 0, 3, 0),
					ResourceUnderReviewStaffCode = "MOU",
					ResourceUnderReviewStaffName = "The mouse is orange",
					ContainmentBarrierOutcome = "PAS",
					IterationReason = "",
					IterationWorkflowDescription = "",
					IterationDepth = 0,
					IteratedToWorkflowDescription = "",
					IteratedToTaskID = "",
					IteratedToTaskSequence = -1,
					IteratedToTaskType = "",
					IteratedToTaskDescription = "",
					IteratedToTaskStaffCode = "",
					IteratedToTaskStaffName = "",
				}
				);
		}

		#region Assertions

		void AssertRows(string assertionMessage, ReportOptions options, params RowDetails[] expectedRows)
		{
			var expectedResults = string.Join(System.Environment.NewLine, expectedRows.Select(r => r.ToString()));
			var results = string.Join(System.Environment.NewLine, GetReportResults(options).Select(r => r.ToString()));
			var message = assertionMessage + System.Environment.NewLine + "Report options: " + options.ToString();

			AssertMultilineASCIIEquals(message, expectedResults.StripTaskIds(), results.StripTaskIds());
		}

		class ReportOptions
		{
			internal ZDateTime ReportPeriodStartUtc { get; set; }
			internal ZDateTime ReportPeriodEndUtc { get; set; }
			internal ZGuid ReleaseGroupPK { get; set; }
			internal string ReviewerStaffCode { get; set; }
			internal string RevieweeStaffCode { get; set; }
			internal string IteratedToStaffCode { get; set; }

			public override string ToString()
			{
				return FormatPropertiesNicely(this);
			}
		}

		class RowDetails
		{
			internal string JobNumber { get; set; }
			internal string ReleaseGroupCode { get; set; }
			internal string ReviewWorkflowDescription { get; set; }
			internal string ReviewTaskID { get; set; }
			internal int ReviewTaskSequence { get; set; }
			internal string ReviewTaskType { get; set; }
			internal string ReviewTaskDescription { get; set; }
			internal string ReviewerStaffCode { get; set; }
			internal string ReviewerStaffName { get; set; }
			internal ZDateTime ReviewCompletedTimeUTC { get; set; }
			internal string ResourceUnderReviewStaffCode { get; set; }
			internal string ResourceUnderReviewStaffName { get; set; }
			internal string ContainmentBarrierOutcome { get; set; }
			internal string IterationReason { get; set; }
			internal string IterationWorkflowDescription { get; set; }
			internal int IterationDepth { get; set; }
			internal string IteratedToWorkflowDescription { get; set; }
			internal string IteratedToTaskID { get; set; }
			internal int IteratedToTaskSequence { get; set; }
			internal string IteratedToTaskType { get; set; }
			internal string IteratedToTaskDescription { get; set; }
			internal string IteratedToTaskStaffCode { get; set; }
			internal string IteratedToTaskStaffName { get; set; }

			public override string ToString()
			{
				return FormatPropertiesNicely(this);
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			MasterFilesTestHelper.ClearWorkflowTables();
		}

		static string FormatPropertiesNicely(object o)
		{
			var values =
				from property in o.GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance)
				let value = property.GetValue(o)
				let name = property.Name
				orderby name
				select string.Format($"{name}: {value}");

			return string.Join(", ", values);
		}

		List<RowDetails> GetReportResults(ReportOptions options)
		{
			var results = new List<RowDetails>();

			var sql = $@"
SELECT * FROM Report_ContainmentBarrierOutcomes(
	{EncloseInQuotesOrNullStringWhenNullOrEmpty(options.ReportPeriodStartUtc, dt => dt.SqlFormat)},
	{EncloseInQuotesOrNullStringWhenNullOrEmpty(options.ReportPeriodEndUtc, dt => dt.SqlFormat)},
	{EncloseInQuotesOrNullStringWhenNullOrEmpty(options.ReleaseGroupPK)},
	{EncloseInQuotesOrNullStringWhenNullOrEmpty(options.ReviewerStaffCode)},
	{EncloseInQuotesOrNullStringWhenNullOrEmpty(options.RevieweeStaffCode)},
	{EncloseInQuotesOrNullStringWhenNullOrEmpty(options.IteratedToStaffCode)}
)
ORDER BY JobNumber, ReviewCompletedTimeUTC";

			using (var reader = TestConnection.Command(sql).ExecuteReader())
			{
				while (reader.Read())
				{
					results.Add(new RowDetails
					{
						JobNumber = reader.GetString(0),
						ReleaseGroupCode = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
						ReviewWorkflowDescription = reader.GetString(2),
						ReviewTaskID = reader.GetString(3),
						ReviewTaskSequence = reader.GetInt32(4),
						ReviewTaskType = reader.GetString(5),
						ReviewTaskDescription = reader.GetString(6),
						ReviewerStaffCode = reader.GetString(7),
						ReviewerStaffName = reader.GetString(8),
						ReviewCompletedTimeUTC = reader.IsDBNull(9) ? ZDateTime.Empty : reader.GetDateTime(9),
						ResourceUnderReviewStaffCode = reader.GetString(10),
						ResourceUnderReviewStaffName = reader.GetString(11),
						ContainmentBarrierOutcome = reader.GetString(12),
						IterationReason = reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
						IterationWorkflowDescription = reader.IsDBNull(14) ? string.Empty : reader.GetString(14),
						IterationDepth = reader.IsDBNull(15) ? 0 : reader.GetInt32(15),
						IteratedToWorkflowDescription = reader.IsDBNull(16) ? string.Empty : reader.GetString(16),
						IteratedToTaskID = reader.IsDBNull(18) ? string.Empty : reader.GetString(17),
						IteratedToTaskSequence = reader.IsDBNull(18) ? -1 : reader.GetInt32(18),
						IteratedToTaskType = reader.IsDBNull(19) ? string.Empty : reader.GetString(19),
						IteratedToTaskDescription = reader.IsDBNull(20) ? string.Empty : reader.GetString(20),
						IteratedToTaskStaffCode = reader.IsDBNull(21) ? string.Empty : reader.GetString(21),
						IteratedToTaskStaffName = reader.IsDBNull(22) ? string.Empty : reader.GetString(22),
					});
				}
			}

			return results;
		}

		static string EncloseInQuotesOrNullStringWhenNullOrEmpty<T>(T @object, Func<T, string> stringFormatter = null)
		{
			var isNull = @object == null
				|| (@object is IZType zType && zType.IsEmpty)
				|| (@object is string s && string.IsNullOrEmpty(s));

			if (isNull)
			{
				if (@object is ZDateTime)
				{
					return "''"; // Because DocEngine passes in empty strings for null datetimes rather than actually passing in null. Thanks DocEngine.
				}

				return "null";
			}

			return $"'{stringFormatter?.Invoke(@object) ?? @object.ToString()}'";
		}

		#endregion
	}
}
