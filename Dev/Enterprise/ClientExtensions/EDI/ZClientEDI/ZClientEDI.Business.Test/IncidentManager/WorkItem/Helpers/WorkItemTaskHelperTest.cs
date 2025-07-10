using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Schema;
using WorkItemProcessTask = Enterprise.Client.EDI.IncidentManager.Business.WorkItemProcessTask;

namespace Enterprise.Client.EDI.Test 
{
	class WorkItemTaskHelperTest : TestCaseWithFactory
	{
		public void TestGetWorkflowsWithinJobThatContainAndPrecedeTask()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			const string codingTaskType = "COD";
			const string codeReviewTaskType = "CBC";
			MasterFilesTestHelper.AddTaskTypesToRegistry(WorkflowDescriptors.WorkItemWorkflowDescriptorCode, codingTaskType, codeReviewTaskType);
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType(codeReviewTaskType, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);

			var prereqJobHeader = helper.CreateJobHeader<WorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var parentJobHeader = helper.CreateJobHeader<WorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader = helper.CreateJobHeader<WorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			prereqJobHeader.GetOrCreateDependencyLink(jobHeader);
			jobHeader.GetOrCreateLinkToParent(parentJobHeader);

			var workflow1_1 = helper.CreateWorkflow(prereqJobHeader, "Job 1 Workflow 1");
			var workflow1_2 = helper.CreateWorkflow(prereqJobHeader, "Job 1 Workflow 2");
			workflow1_1.GetOrCreateDependencyLink(workflow1_2);
			helper.CreateTask(workflow1_1, taskType: codingTaskType);
			helper.CreateTask(workflow1_2, taskType: codeReviewTaskType);

			var workflow2_1 = helper.CreateWorkflow(jobHeader, "Job 2 Workflow 2");
			var workflow2_2 = helper.CreateWorkflow(jobHeader, "Job 2 Workflow 2");
			var workflow2_3 = helper.CreateWorkflow(prereqJobHeader, "Job 2 Workflow 3");
			workflow2_1.GetOrCreateDependencyLink(workflow2_2);
			workflow2_1.GetOrCreateLinkToParent(workflow2_3);
			workflow2_2.GetOrCreateLinkToParent(workflow2_3);
			var task1 = helper.CreateTask(workflow2_1, taskType: codingTaskType, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = helper.CreateTask(workflow2_2, taskType: codeReviewTaskType);

			var iterationWorkflow1 = helper.CreateQualityIteration(task1, task2, iterationWorkflowDescription: "Iteration 1", shouldCreateWorkflowForIteration: true);
			AssertEquals(2, iterationWorkflow1.Tasks.Count());

			var iteration1Task1 = iterationWorkflow1.Tasks.First();
			var iteration1Task2 = iterationWorkflow1.Tasks.Last();

			helper.CreateQualityIteration(iteration1Task1, iteration1Task2, iterationWorkflowDescription: "Iteration 2", shouldCreateWorkflowForIteration: false);
			AssertEquals(4, iterationWorkflow1.Tasks.Count());

			var iteration2Task1 = iterationWorkflow1.Tasks.Skip(2).First();
			var iteration2Task2 = iterationWorkflow1.Tasks.Skip(2).Last();

			var iterationWorkflow2 = helper.CreateQualityIteration(iteration2Task1, iteration2Task2, iterationWorkflowDescription: "Iteration 2", shouldCreateWorkflowForIteration: true);
			AssertEquals(2, iterationWorkflow2.Tasks.Count());

			var iteration3Task1 = iterationWorkflow2.Tasks.First();
			var iteration3Task2 = iterationWorkflow2.Tasks.Last();

			AssertWorkflowsWithinJobThatContainAndPrecedeTask(iteration3Task1, iterationWorkflow2, iterationWorkflow1, workflow2_2, workflow2_1);
			AssertWorkflowsWithinJobThatContainAndPrecedeTask(iteration3Task2, iterationWorkflow2, iterationWorkflow1, workflow2_2, workflow2_1);

			AssertWorkflowsWithinJobThatContainAndPrecedeTask(iteration2Task2, iterationWorkflow1, workflow2_2, workflow2_1);
			AssertWorkflowsWithinJobThatContainAndPrecedeTask(iteration2Task1, iterationWorkflow1, workflow2_2, workflow2_1);

			AssertWorkflowsWithinJobThatContainAndPrecedeTask(iteration1Task2, iterationWorkflow1, workflow2_2, workflow2_1);
			AssertWorkflowsWithinJobThatContainAndPrecedeTask(iteration1Task1, iterationWorkflow1, workflow2_2, workflow2_1);

			AssertWorkflowsWithinJobThatContainAndPrecedeTask(task2, workflow2_2, workflow2_1);
			AssertWorkflowsWithinJobThatContainAndPrecedeTask(task1, workflow2_1);

			AssertFindWorkflowsUpTheQualityIterationHierarchy(iteration3Task1, iterationWorkflow2, iterationWorkflow1, workflow2_2);
			AssertFindWorkflowsUpTheQualityIterationHierarchy(iteration3Task2, iterationWorkflow2, iterationWorkflow1, workflow2_2);

			AssertFindWorkflowsUpTheQualityIterationHierarchy(iteration2Task2, iterationWorkflow1, workflow2_2);
			AssertFindWorkflowsUpTheQualityIterationHierarchy(iteration2Task1, iterationWorkflow1, workflow2_2);

			AssertFindWorkflowsUpTheQualityIterationHierarchy(iteration1Task2, iterationWorkflow1, workflow2_2);
			AssertFindWorkflowsUpTheQualityIterationHierarchy(iteration1Task1, iterationWorkflow1, workflow2_2);

			AssertFindWorkflowsUpTheQualityIterationHierarchy(task2, workflow2_2);
			AssertFindWorkflowsUpTheQualityIterationHierarchy(task1, workflow2_1);
		}

		public void TestGetWorkflowsWithinJobThatContainAndPrecedeTask_WhenContainmentBarrierTaskInItsOwnIteration()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			const string codingTaskType = "COD";
			const string codeReviewTaskType = "CBC";
			MasterFilesTestHelper.AddTaskTypesToRegistry(WorkflowDescriptors.WorkItemWorkflowDescriptorCode, codingTaskType, codeReviewTaskType);
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType(codeReviewTaskType, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);

			var jobHeader = helper.CreateJobHeader<WorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "Workflow");
			var task1 = (ProcessTask)helper.CreateTask(workflow, sequence: 10, taskType: codingTaskType, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = (ProcessTask)helper.CreateTask(workflow, sequence: 20, taskType: codeReviewTaskType);
			var submissionTask = (ProcessTask)helper.CreateTask(workflow, sequence: 30, taskType: EDITaskTypes.TaskCheckin);

			AssertEquals(3, workflow.Tasks.Count());

			helper.CreateQualityIteration(task1, task2, shouldCreateWorkflowForIteration: false);

			AssertEquals(5, workflow.Tasks.Count());

			var task3 = workflow.Tasks.Cast<ProcessTask>().Single(t => t.P9_Sequence == 21);

			AssertNullOrEmpty(task2.Iteration);
			AssertNullOrEmpty(submissionTask.Iteration);
			AssertNotNullOrEmpty(task3.Iteration);
			AssertWorkflowsWithinJobThatContainAndPrecedeTask(submissionTask, workflow);

			task2.Iteration = task3.Iteration;
			AssertWorkflowsWithinJobThatContainAndPrecedeTask(submissionTask, workflow);

			submissionTask.Iteration = task3.Iteration;
			AssertWorkflowsWithinJobThatContainAndPrecedeTask(submissionTask, workflow);
		}

		public void TestGetWorkflowsWithinJobThatMayContainReviewTask()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			const string codingTaskType = "COD";
			const string codeReviewTaskType = "CBC";
			MasterFilesTestHelper.AddTaskTypesToRegistry(WorkflowDescriptors.WorkItemWorkflowDescriptorCode, codingTaskType, codeReviewTaskType);
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType(codeReviewTaskType, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);

			var prereqJobHeader = helper.CreateJobHeader<WorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var dependentJobHeader = helper.CreateJobHeader<WorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var parentJobHeader = helper.CreateJobHeader<WorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader = helper.CreateJobHeader<WorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			prereqJobHeader.GetOrCreateDependencyLink(jobHeader);
			jobHeader.GetOrCreateLinkToParent(parentJobHeader);

			var workflow1_1 = helper.CreateWorkflow(prereqJobHeader, "Job 1 Workflow 1");
			var workflow1_2 = helper.CreateWorkflow(prereqJobHeader, "Job 1 Workflow 2");
			workflow1_1.GetOrCreateDependencyLink(workflow1_2);
			helper.CreateTask(workflow1_1, taskType: codingTaskType);
			helper.CreateTask(workflow1_2, taskType: codeReviewTaskType);

			var workflow2_1 = helper.CreateWorkflow(jobHeader, "Job 2 Workflow 2");
			var workflow2_2 = helper.CreateWorkflow(jobHeader, "Job 2 Workflow 2");
			var workflow2_3 = helper.CreateWorkflow(jobHeader, "Job 2 Workflow 3");
			var workflow2_4 = helper.CreateWorkflow(jobHeader, "Job 2 Workflow 4");
			var workflow2_5 = helper.CreateWorkflow(jobHeader, "Job 2 Workflow 5");
			workflow2_1.GetOrCreateDependencyLink(workflow2_2);
			workflow2_1.GetOrCreateLinkToParent(workflow2_3);
			workflow2_2.GetOrCreateLinkToParent(workflow2_3);
			workflow2_1.GetOrCreateDependencyLink(workflow2_4);
			workflow2_4.GetOrCreateDependencyLink(workflow2_5);
			workflow2_5.GetOrCreateDependencyLink(dependentJobHeader);
			var task1 = helper.CreateTask(workflow2_1, taskType: codingTaskType, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = helper.CreateTask(workflow2_2, taskType: codeReviewTaskType);

			var iterationWorkflow1 = helper.CreateQualityIteration(task1, task2, iterationWorkflowDescription: "Iteration 1", shouldCreateWorkflowForIteration: true);
			AssertEquals(2, iterationWorkflow1.Tasks.Count());

			var iteration1Task1 = iterationWorkflow1.Tasks.First();
			var iteration1Task2 = iterationWorkflow1.Tasks.Last();

			helper.CreateQualityIteration(iteration1Task1, iteration1Task2, iterationWorkflowDescription: "Iteration 2", shouldCreateWorkflowForIteration: false);
			AssertEquals(4, iterationWorkflow1.Tasks.Count());

			var iteration2Task1 = iterationWorkflow1.Tasks.Skip(2).First();
			var iteration2Task2 = iterationWorkflow1.Tasks.Skip(2).Last();

			var iterationWorkflow2 = helper.CreateQualityIteration(iteration2Task1, iteration2Task2, iterationWorkflowDescription: "Iteration 2", shouldCreateWorkflowForIteration: true);
			AssertEquals(2, iterationWorkflow2.Tasks.Count());

			var iteration3Task2 = iterationWorkflow2.Tasks.Last();

			AssertWorkflowsWithinJobThatMayContainReviewTask(iteration3Task2, iterationWorkflow2, iterationWorkflow1, workflow2_2, workflow2_1, workflow2_4, workflow2_5);
			AssertWorkflowsWithinJobThatMayContainReviewTask(iteration2Task2, iterationWorkflow2, iterationWorkflow1, workflow2_2, workflow2_1, workflow2_4, workflow2_5);
			AssertWorkflowsWithinJobThatMayContainReviewTask(iteration1Task2, iterationWorkflow2, iterationWorkflow1, workflow2_2, workflow2_1, workflow2_4, workflow2_5);
			AssertWorkflowsWithinJobThatMayContainReviewTask(task2, iterationWorkflow2, iterationWorkflow1, workflow2_2, workflow2_1, workflow2_4, workflow2_5);
		}

		public void TestGetWorkflowsWithinJobThatMayContainReviewTask_ShouldIncludeChildIterations()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			const string codingTaskType = "COD";
			const string codeReviewTaskType = "CBC";
			MasterFilesTestHelper.AddTaskTypesToRegistry(WorkflowDescriptors.WorkItemWorkflowDescriptorCode, codingTaskType, codeReviewTaskType);
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType(codeReviewTaskType, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);

			var jobHeader = helper.CreateJobHeader<WorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "Workflow 1");
			var task1 = helper.CreateTask(workflow, taskType: codingTaskType, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 10);
			var task2 = helper.CreateTask(workflow, taskType: codeReviewTaskType, sequence: 20);
			var task3 = helper.CreateTask(workflow, taskType: EDITaskTypes.TaskCheckin, sequence: 30);

			var iterationWorkflow1 = helper.CreateQualityIteration(task1, task2, iterationWorkflowDescription: "Iteration 1", shouldCreateWorkflowForIteration: true);
			AssertEquals(2, iterationWorkflow1.Tasks.Count());

			var iterationWorkflow2 = helper.CreateQualityIteration(iterationWorkflow1.Tasks.First(), iterationWorkflow1.Tasks.Last(), iterationWorkflowDescription: "Iteration 2", shouldCreateWorkflowForIteration: true);
			AssertEquals(2, iterationWorkflow2.Tasks.Count());

			var iterationWorkflow3 = helper.CreateQualityIteration(iterationWorkflow2.Tasks.First(), iterationWorkflow2.Tasks.Last(), iterationWorkflowDescription: "Iteration 2", shouldCreateWorkflowForIteration: true);
			AssertEquals(2, iterationWorkflow3.Tasks.Count());

			AssertWorkflowsWithinJobThatMayContainReviewTask(task3, workflow, iterationWorkflow3, iterationWorkflow2, iterationWorkflow1);
		}

		public void TestGetWorkflowsWithinJobThatMayContainReviewTask_DbHits()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			const string codingTaskType = "COD";
			const string codeReviewTaskType = "CBC";
			MasterFilesTestHelper.AddTaskTypesToRegistry(WorkflowDescriptors.WorkItemWorkflowDescriptorCode, codingTaskType, codeReviewTaskType);
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType(codeReviewTaskType, WorkflowDescriptors.WorkItemWorkflowDescriptorCode);

			var prereqJobHeader = helper.CreateJobHeader<WorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var dependentJobHeader = helper.CreateJobHeader<WorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var parentJobHeader = helper.CreateJobHeader<WorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var jobHeader = helper.CreateJobHeader<WorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			prereqJobHeader.GetOrCreateDependencyLink(jobHeader);
			jobHeader.GetOrCreateLinkToParent(parentJobHeader);

			var workflow1_1 = helper.CreateWorkflow(prereqJobHeader, "Job 1 Workflow 1");
			var workflow1_2 = helper.CreateWorkflow(prereqJobHeader, "Job 1 Workflow 2");
			workflow1_1.GetOrCreateDependencyLink(workflow1_2);
			helper.CreateTask(workflow1_1, taskType: codingTaskType);
			helper.CreateTask(workflow1_2, taskType: codeReviewTaskType);

			var workflow2_1 = helper.CreateWorkflow(jobHeader, "Job 2 Workflow 2");
			var workflow2_2 = helper.CreateWorkflow(jobHeader, "Job 2 Workflow 2");
			var workflow2_3 = helper.CreateWorkflow(jobHeader, "Job 2 Workflow 3");
			var workflow2_4 = helper.CreateWorkflow(jobHeader, "Job 2 Workflow 4");
			var workflow2_5 = helper.CreateWorkflow(jobHeader, "Job 2 Workflow 5");
			workflow2_1.GetOrCreateDependencyLink(workflow2_2);
			workflow2_1.GetOrCreateLinkToParent(workflow2_3);
			workflow2_2.GetOrCreateLinkToParent(workflow2_3);
			workflow2_1.GetOrCreateDependencyLink(workflow2_4);
			workflow2_4.GetOrCreateDependencyLink(workflow2_5);
			workflow2_5.GetOrCreateDependencyLink(dependentJobHeader);
			var task1 = helper.CreateTask(workflow2_1, taskType: codingTaskType, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = helper.CreateTask(workflow2_2, taskType: codeReviewTaskType);

			var iterationWorkflow1 = helper.CreateQualityIteration(task1, task2, iterationWorkflowDescription: "Iteration 1", shouldCreateWorkflowForIteration: true);
			AssertEquals(2, iterationWorkflow1.Tasks.Count());

			var iteration1Task1 = iterationWorkflow1.Tasks.First();
			var iteration1Task2 = iterationWorkflow1.Tasks.Last();

			helper.CreateQualityIteration(iteration1Task1, iteration1Task2, iterationWorkflowDescription: "Iteration 2", shouldCreateWorkflowForIteration: false);
			AssertEquals(4, iterationWorkflow1.Tasks.Count());

			var iteration2Task1 = iterationWorkflow1.Tasks.Skip(2).First();
			var iteration2Task2 = iterationWorkflow1.Tasks.Skip(2).Last();

			var iterationWorkflow2 = helper.CreateQualityIteration(iteration2Task1, iteration2Task2, iterationWorkflowDescription: "Iteration 2", shouldCreateWorkflowForIteration: true);
			AssertEquals(2, iterationWorkflow2.Tasks.Count());

			var iteration3Task1 = iterationWorkflow2.Tasks.First();
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedTask = newFactory.Load<ProcessTask>(iteration3Task1.PK);

			var hits = new Dictionary<string, int>
			{
				{ ProcessTasksSchema.Constants.TableName, 4 },
				{ ProcessHeaderSchema.Constants.TableName, 9 },
				{ ProcessHeaderLinkSchema.Constants.TableName, 14 },
				{ ProcessTaskIterationLinkSchema.Constants.TableName, 3 },
				{ ProcessTaskIterationLinkPivotSchema.Constants.TableName, 4 },
			};

			var workflows = WorkItemTaskHelper.GetWorkflowsWithinJobThatMayContainReviewTask(loadedTask).ToArray();
			AssertEquals(6, workflows.Length);
			AssertDbHits(hits, newFactory);
		}

		static void AssertWorkflowsWithinJobThatContainAndPrecedeTask(IProcessTask targetTask, params IProcessHeader[] expectedWorkflows)
		{
			AssertSequencesEqual(expectedWorkflows, WorkItemTaskHelper.GetWorkflowsWithinJobThatContainAndPrecedeTask((ProcessTask)targetTask));
		}

		static void AssertWorkflowsWithinJobThatMayContainReviewTask(IProcessTask targetTask, params IProcessHeader[] expectedWorkflows)
		{
			AssertContainsExactElementsInAnyOrder(expectedWorkflows, WorkItemTaskHelper.GetWorkflowsWithinJobThatMayContainReviewTask((ProcessTask)targetTask));
		}

		static void AssertFindWorkflowsUpTheQualityIterationHierarchy(IProcessTask targetTask, params IProcessHeader[] expectedWorkflows)
		{
			AssertSequencesEqual(expectedWorkflows, WorkItemTaskHelper.FindWorkflowsUpTheQualityIterationHierarchy((ProcessTask)targetTask));
		}

		public void TestGetCompletedReviewTasks_ShouldOrderBySequenceAndCompletedTime()
		{
			const string matchingTaskType = WorkItemProcessTask.CodeReviewTaskType;
			const string nonMatchingTaskType = "CBF";

			const string matchingStatus = ProcessTaskStatusCodeList.Codes.Closed;
			const string nonMatchingStatus = ProcessTaskStatusCodeList.Codes.Cancelled;

			const int latestSequence = 20;
			const int nonLatestSequence = 10;

			var latestDateTime = ZDateTime.UtcNow.AddDays(1);
			var nonLatestDateTime = ZDateTime.UtcNow.AddDays(-1);

			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			var jobHeader = helper.CreateJobHeader<NewWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "Workflow");
			var prerequisiteWorkflow = helper.CreateWorkflow(jobHeader, "Prereq Workflow");
			prerequisiteWorkflow.GetOrCreateDependencyLink(workflow);

			var task1 = CreateTask(matchingTaskType, matchingStatus, nonLatestSequence, nonLatestDateTime, "Task1");
			var task2 = CreateTask(matchingTaskType, matchingStatus, latestSequence, nonLatestDateTime, "Task2");
			var task3 = CreateTask(matchingTaskType, matchingStatus, nonLatestSequence, latestDateTime, "Task3");
			var task4 = CreateTask(matchingTaskType, matchingStatus, latestSequence, latestDateTime, "Task4");
			CreateTask(matchingTaskType, nonMatchingStatus, latestSequence, latestDateTime, "Task5");
			CreateTask(nonMatchingTaskType, matchingStatus, latestSequence, latestDateTime, "Task6");
			CreateTask(nonMatchingTaskType, nonMatchingStatus, latestSequence, latestDateTime, "Task7");

			var prereqTask1 = CreateTask(matchingTaskType, matchingStatus, nonLatestSequence, nonLatestDateTime, "Prereq Task1", prerequisiteWorkflow);
			var prereqTask2 = CreateTask(matchingTaskType, matchingStatus, latestSequence, nonLatestDateTime, "Prereq Task2", prerequisiteWorkflow);
			var prereqTask3 = CreateTask(matchingTaskType, matchingStatus, nonLatestSequence, latestDateTime, "Prereq Task3", prerequisiteWorkflow);
			var prereqTask4 = CreateTask(matchingTaskType, matchingStatus, latestSequence, latestDateTime, "Prereq Task4", prerequisiteWorkflow);

			var submissionTask = (ProcessTask)helper.CreateTask(workflow, sequence: latestSequence + 1);

			var orderedReviewTasks = WorkItemTaskHelper.GetCompletedReviewTasks(submissionTask).Select(t => t.P9_Description.ToString());
			var expectedDescriptions = new[] { "Task4", "Task2", "Task3", "Task1", "Prereq Task4", "Prereq Task2", "Prereq Task3", "Prereq Task1" };

			AssertSequencesEqual(expectedDescriptions, orderedReviewTasks);

			ProcessTask CreateTask(string type, string status, int sequence, ZDateTime completedDateTime, string description, IProcessHeader workflowForTask = null)
			{
				var task = helper.CreateTask(workflowForTask ?? workflow, staffCode: "DE", taskType: type, taskStatus: status, sequence: sequence, description: description);
				task.P9_CompletedTime = completedDateTime.UtcToDateTimeOffset();
				return (ProcessTask)task;
			}
		}

		public void TestGetCompletedReviewTasks_WhenPrerequisiteWorkflowTasksHigherInSequenceThanTargetTask()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();

			var jobHeader = helper.CreateJobHeader<NewWorkItem>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "Workflow");
			var prerequisiteWorkflow = helper.CreateWorkflow(jobHeader, "Prereq Workflow");
			prerequisiteWorkflow.GetOrCreateDependencyLink(workflow);

			helper.CreateTask(prerequisiteWorkflow, staffCode: "AM", taskType: WorkItemProcessTask.CodeReviewTaskType, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 100, description: "Review in prereq workflow");
			helper.CreateTask(workflow, staffCode: "AM", taskType: WorkItemProcessTask.CodeReviewTaskType, taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 100, description: "Review in submission workflow");
			var submissionTask = helper.CreateTask(workflow, staffCode: "DE", taskType: WorkItemProcessTask.CodeReviewTaskType, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 10, description: "Submission task");

			var reviewTaskDescriptions = WorkItemTaskHelper.GetCompletedReviewTasks((ProcessTask)submissionTask).Select(t => t.P9_Description.ToString());

			AssertSequencesEqual(new[] { "Review in prereq workflow" }, reviewTaskDescriptions);
		}
	}
}
