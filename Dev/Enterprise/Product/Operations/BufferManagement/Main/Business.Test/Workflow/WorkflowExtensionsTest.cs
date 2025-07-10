using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class WorkflowExtensionsTest : BMSTestCaseWithFactory
	{
		#region IsCapacityConstrained

		public void TestIsCapacityConstrained()
		{
			var resource1_nonConstraint = Factory.NewWithValidTestData<GlbStaff>();
			var resource2_constraint = Factory.NewWithValidTestData<GlbStaff>();

			var system = CreateSystem("ORG");

			var bucket1 = CreateBucket(system, "bucket1");
			var bucket2 = CreateBucket(system, "bucket2");
			var bucket3 = CreateBucket(system, "bucket3");

			var buffer = CreateBuffer(system);
			buffer.FC_BufferTimeCapacityConstraintThresholdMultiple = 2;

			var link1 = LinkComponents(bucket1, buffer);
			var link2 = LinkComponents(bucket2, buffer);
			var link3 = LinkComponents(bucket3, buffer);
			link3.FL_IsReleaseGateRuleApplied = false;

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = bucket1.PK;
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = bucket2.PK;
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			workflow3.FH_FC_CurrentComponent = bucket3.PK;

			var task1_1 = CreateTask(workflow1, resource1_nonConstraint.GS_Code, lowEstMinutes: 10);
			var task1_2 = CreateTask(workflow1, resource2_constraint.GS_Code, lowEstMinutes: buffer.FC_BufferTimespanInMinutes);
			var task1_3 = CreateTask(workflow1, resource1_nonConstraint.GS_Code, lowEstMinutes: buffer.FC_BufferTimespanInMinutes * 4); // Really big task, but it's closed, so doesn't count.
			task1_3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var task2_1 = CreateTask(workflow2, resource1_nonConstraint.GS_Code, lowEstMinutes: 10);
			var task2_2 = CreateTask(workflow2, resource2_constraint.GS_Code, lowEstMinutes: buffer.FC_BufferTimespanInMinutes);

			var task3_1 = CreateTask(workflow3, resource1_nonConstraint.GS_Code, lowEstMinutes: buffer.FC_BufferTimespanInMinutes * 2);

			Factory.Save();

			var standardEstimateMinutes = BMSTestHelper.GetStandardEstimateHoursInFeedingComponents(resource1_nonConstraint, buffer);
			AssertEquals(false, buffer.IsOverCapacityConstrainedThreshold(standardEstimateMinutes));
			AssertEquals(30m, standardEstimateMinutes);
			AssertEquals(96 * 60 * 2, buffer.CCRThresholdMinutes);

			standardEstimateMinutes = BMSTestHelper.GetStandardEstimateHoursInFeedingComponents(resource2_constraint, buffer);
			AssertEquals(true, buffer.IsOverCapacityConstrainedThreshold(standardEstimateMinutes));
			AssertEquals(17280m, standardEstimateMinutes);
		}

		#endregion

		#region CCR Status

		public void TestIsResourceMarkedCCR()
		{
			var system = Factory.New<BMSystem>();
			var component1 = system.Components.AddNew();
			var component2 = system.Components.AddNew();

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			AssertEquals(false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource, component1));

			var componentLink = component1.ResourceLinks.AddNew();
			componentLink.FD_GS_NKResource = resource.GS_Code;
			componentLink.FD_IsCapacityConstrained = true;

			AssertEquals(true, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource, component1));
			AssertEquals(false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource, component2));

			componentLink.FD_IsCapacityConstrained = false;
			AssertEquals(false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource, component1));
		}

		public void TestMarkAsCCR_EmptyResourceCode_ShouldThrowException()
		{
			var buffer = CreateBuffer(CreateSystem());

			var resource = Factory.New<GlbStaff>();
			AssertExceptionThrown<InvalidOperationException>(() => resource.DesignateAsCCR(buffer));
		}

		[TestDate(2013, 10, 26)]
		public void TestMarkAsCCR()
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			AssertEquals(false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource, component));

			resource.DesignateAsCCR(component);
			AssertEquals(true, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource, component));

			var link = component.ResourceLinks.First(l => l.FD_GS_NKResource == resource.GS_Code);
			link.FD_CapacityConstraintDetectedUtc = ZDateTime.UtcNow;

			resource.DesignateAsNonCCR(component);
			AssertEquals(false, ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(resource, component));
			AssertEquals("Setting as non-CCR should still preserve candidate CCR status", ZDateTime.UtcNow, link.FD_CapacityConstraintDetectedUtc);
		}

		[TestDate(2013, 12, 4)]
		public void TestIsCapacityConstrainedCandidate_ShouldUseFlag()
		{
			BMSRegistry.Instance.PersistentlyOverloadedBufferFactor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2.0m);

			var system = Factory.New<BMSystem>();
			var buffer = CreateBuffer(system);

			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var link = buffer.ResourceLinks.AddNew();
			link.FD_GS_NKResource = resource.GS_Code;

			AssertEquals(false, resource.IsCapacityConstrainedCandidate(buffer));

			link.FD_CapacityConstraintDetectedUtc = ZDateTime.UtcNow;
			AssertEquals(false, resource.IsCapacityConstrainedCandidate(buffer));

			link.FD_CapacityConstraintDetectedUtc = ZDateTime.UtcNow.AddMinutes(-buffer.FC_BufferTimespanInMinutes);
			AssertEquals(false, resource.IsCapacityConstrainedCandidate(buffer));

			link.FD_CapacityConstraintDetectedUtc = ZDateTime.UtcNow.AddMinutes(-buffer.FC_BufferTimespanInMinutes * 2);
			AssertEquals(false, resource.IsCapacityConstrainedCandidate(buffer));

			link.FD_IsPersistentlyOverloaded = true;
			AssertEquals(true, resource.IsCapacityConstrainedCandidate(buffer));
		}

		#endregion

		#region IsStartable

		[GuiTest]
		public void TestIsStartable_WhenQualityIterationWorkflowIsBlocked()
		{
			WorkflowDataRegistry.Instance.CreateNewWorkflowsForQualityIterationsByDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var staffUnderReview = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var reviewer = CreateStaffInCurrentBranchDept("BIL", "Bilbo Baggins");

			var mainWorkflow = BMSTestHelper.CreateWorkflow(Factory, "Main Workflow");

			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", mainWorkflow.JobHeader.FH_WorkflowType);

			var workingTask = CreateTask(mainWorkflow, staffUnderReview.GS_Code, lowEstMinutes: 1, sequence: 1);
			var qcbTask = CreateTask(mainWorkflow, reviewer.GS_Code, lowEstMinutes: 1, sequence: 2, taskType: "QCB");
			var lastTask = CreateTask(mainWorkflow, staffUnderReview.GS_Code, lowEstMinutes: 1, sequence: 100);

			Factory.Save();

			workingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			using (var viewModel = new ContainmentBarrierViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed))
			{
				viewModel.Response = ContainmentBarrierResponses.IterationRequired;
				viewModel.IterateFromTaskPK = workingTask.PK;

				viewModel.CommitResponse();
				qcbTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();
			}

			var qiWorkflow = mainWorkflow.JobHeader.ProcessHeaders.First(p => p != mainWorkflow);

			AssertEquals("QualityIterationWorkflow should have 2 tasks", 2, qiWorkflow.Tasks.Count());

			var workingTaskInQIWorkflow = qiWorkflow.Tasks.First(t => t.P9_Type == workingTask.P9_Type);
			var qcbTaskInQIWorkflow = qiWorkflow.Tasks.First(t => t.P9_Type == qcbTask.P9_Type);

			CombineAssertions(() =>
			{
				AssertEquals("Completed workingTask should not be startable", false, workingTask.IsStartable());
				AssertEquals("Completed qcbTask should not be startable", false, qcbTask.IsStartable());
				AssertEquals("Blocked lastTask should not be startable", false, lastTask.IsStartable());
				AssertEquals("workingTaskInQIWorkflow has no prereqs tasks or workflows so should be startable", true, workingTaskInQIWorkflow.IsStartable());
				AssertEquals("qcbTaskInQIWorkflow task should not be startable", false, qcbTaskInQIWorkflow.IsStartable());
			});

			var prereqOfQIWorkflow = BMSTestHelper.CreateWorkflow(Factory, "Prereq of Quality Iteration Workflow");
			var taskInprereqOfQIWorkflow = CreateTask(prereqOfQIWorkflow, staffUnderReview.GS_Code, lowEstMinutes: 1, sequence: 1);
			prereqOfQIWorkflow.GetOrCreateDependencyLink(qiWorkflow);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Completed workingTask should not be startable", false, workingTask.IsStartable());
				AssertEquals("Completed qcbTask should not be startable", false, qcbTask.IsStartable());
				AssertEquals("Blocked lastTask should not be startable", false, lastTask.IsStartable());
				AssertEquals("workingTaskInQIWorkflow has prereqs workflow so should not be startable", false, workingTaskInQIWorkflow.IsStartable());
				AssertEquals("qcbTaskInQIWorkflow task should not be startable", false, qcbTaskInQIWorkflow.IsStartable());
				AssertEquals("taskInprereqOfQIWorkflow  has no prereqs tasks or workflows so should be startable", true, taskInprereqOfQIWorkflow.IsStartable());
			});
		}

		[GuiTest]
		public void TestIsStartable_WhenMoreThenOneQualityIterationFromSameContainmentBarrierTaskInSameWorkflow()
		{
			WorkflowDataRegistry.Instance.CreateNewWorkflowsForQualityIterationsByDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var staffUnderReview = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var reviewer = CreateStaffInCurrentBranchDept("BIL", "Bilbo Baggins");

			var mainWorkflow = BMSTestHelper.CreateWorkflow(Factory, "Main Workflow");

			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", mainWorkflow.JobHeader.FH_WorkflowType);

			var workingTask = CreateTask(mainWorkflow, staffUnderReview.GS_Code, lowEstMinutes: 1, sequence: 1);
			var qcbTask = CreateTask(mainWorkflow, reviewer.GS_Code, lowEstMinutes: 1, sequence: 2, taskType: "QCB");
			var lastTask = CreateTask(mainWorkflow, staffUnderReview.GS_Code, lowEstMinutes: 1, sequence: 100);

			workingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			using (var viewModel = new ContainmentBarrierViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed))
			{
				viewModel.Response = ContainmentBarrierResponses.IterationRequired;
				viewModel.IterateFromTaskPK = workingTask.PK;

				viewModel.CommitResponse();
				viewModel.CommitResponse();

				qcbTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

				Factory.Save();
			}

			AssertEquals("Workflow should have 6 tasks", 7, mainWorkflow.Tasks.Count());

			var tasksFromFirstIteration = mainWorkflow.Tasks.Where(t => t.Iteration.Equals("1"));
			var workingTaskFromFirstIteration = tasksFromFirstIteration.First(t => t.P9_Type == workingTask.P9_Type);
			var qcbTaskFromFirstIteration = tasksFromFirstIteration.First(t => t.P9_Type == qcbTask.P9_Type);

			var taskFromSecondIteration = mainWorkflow.Tasks.Where(t => t.Iteration.Equals("2"));
			var workingTaskFromSecondIteration = taskFromSecondIteration.First(t => t.P9_Type == workingTask.P9_Type);
			var qcbTaskFromSecondIteration = taskFromSecondIteration.First(t => t.P9_Type == qcbTask.P9_Type);

			CombineAssertions(() =>
			{
				AssertEquals("Completed workingTask should not be startable", false, workingTask.IsStartable());
				AssertEquals("Completed qcbTask should not be startable", false, qcbTask.IsStartable());
				AssertEquals("Blocked lastTask should not be startable", false, lastTask.IsStartable());
				AssertEquals("Blocked by second Iteration workingTaskFromFirstIteration should not be startable", false, workingTaskFromFirstIteration.IsStartable());
				AssertEquals("Blocked qcbTaskFromFirstIteration should not be startable", false, qcbTaskFromFirstIteration.IsStartable());
				AssertEquals("workingTaskFromSecondIteration should be startable", true, workingTaskFromSecondIteration.IsStartable());
				AssertEquals("Blocked qcbTaskFromSecondIteration should not be startable", false, qcbTaskFromSecondIteration.IsStartable());
			});
		}

		[GuiTest]
		public void TestIsStartable_WhenOpenQualityIterationsExist()
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "ORG");

			var resource1 = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var resource2 = CreateStaffInCurrentBranchDept("BIL", "Bilbo Baggins");

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];
			var qualityIterationWorkflow = jobHeader.ProcessHeaders.AddNew();
			qualityIterationWorkflow.GetOrCreateLinkToParent(workflow);

			var task1 = CreateTask(workflow, resource1.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var qcbTask = CreateTask(workflow, resource2.GS_Code, 60, taskType: "QCB");
			var task3 = CreateTask(workflow, resource1.GS_Code, 60);

			var qiTask = CreateTask(qualityIterationWorkflow, resource1.GS_Code, 60);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Completed task1 should not be current", false, task1.IsStartable());
				AssertEquals("Open qcbTask should be current", true, qcbTask.IsStartable());
				AssertEquals("Blocked task3 should not be current", false, task3.IsStartable());
				AssertEquals("qiTask has no prereq tasks or workflows so should be current", true, qiTask.IsStartable());
			});

			qcbTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Completed task1 should not be current", false, task1.IsStartable());
				AssertEquals("Completed qcbTask should not be current", false, qcbTask.IsStartable());
				AssertEquals("task3 has no prereq tasks or workflows so should be current", true, task3.IsStartable());
				AssertEquals("qiTask has no prereq tasks or workflows so should be current", true, qiTask.IsStartable());
			});

			var pivot = CreateIterationLink(qcbTask, qualityIterationWorkflow, task1, IterationLinkTypeList.Codes.QualityIterationTask);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Completed task1 should not be current", false, task1.IsStartable());
				AssertEquals("Completed qcbTask should not be current", false, qcbTask.IsStartable());
				AssertEquals("task3 has an open prereq quality iteration workflow so should not be current", false, task3.IsStartable());
				AssertEquals("qiTask has no prereq tasks or workflows so should be current", true, qiTask.IsStartable());
			});

			qiTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Completed task1 should not be current", false, task1.IsStartable());
				AssertEquals("Completed qcbTask should not be current", false, qcbTask.IsStartable());
				AssertEquals("task3 has a closed prereq quality iteration workflow so should be current now", true, task3.IsStartable());
				AssertEquals("Completed qiTask should not be current", false, qiTask.IsStartable());
			});
		}

		[GuiTest]
		public void TestGetOpenIterationLinksQuery_ShouldUseTableValuedParameters()
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "ORG");

			var resource1 = CreateStaffInCurrentBranchDept("HAP", "Harry Potter");
			var resource2 = CreateStaffInCurrentBranchDept("DRM", "Draco Malfoy");

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];
			var qualityIterationWorkflow = jobHeader.ProcessHeaders.AddNew();
			qualityIterationWorkflow.GetOrCreateLinkToParent(workflow);

			var task1 = CreateTask(workflow, resource1.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var qcbTask = CreateTask(workflow, resource2.GS_Code, 60, taskType: "QCB");
			var qiTask = CreateTask(qualityIterationWorkflow, resource1.GS_Code, 60);
			var task3 = CreateTask(workflow, resource1.GS_Code, 60);

			using (Db.Connection.TrackExecutedCommands())
			{
				using (var settings = TestEntityFrameworkSettings.Get())
				{
					settings.TVPRule = new TVPRule("0");
					Factory.Save();

					qcbTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					Factory.Save();

					var pivot = CreateIterationLink(qcbTask, qualityIterationWorkflow, task1, IterationLinkTypeList.Codes.QualityIterationTask);
					Factory.Save();

					qiTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					Factory.Save();

					var executedCommands = Db.Connection.ExecutedCommands.Where(c => c.Contains("FROM dbo.ProcessTaskIterationLink\r\n\tWHERE"));
					Assert("At least one matching command should be executed", !executedCommands.IsNullOrEmpty());

					foreach (string query in executedCommands)
					{
						CombineAssertions(() =>
						{
							AssertContains("WorkflowExtensions.GetOpenIterationLinksQuery() should use TVPs", "WHERE (P9I_P9_ContainmentBarrierTask in (SELECT Value FROM", query);
							Assert("WorkflowExtensions.GetOpenIterationLinksQuery() should not use a parameter list)", !Regex.IsMatch(query, @"WHERE \(P9I_P9_ContainmentBarrierTask in \((@(.*?),)*?@(.*?)\)"));
							AssertNotContains("WorkflowExtensions.GetOpenIterationLinksQuery() should not use a parameter comparison", "WHERE P9I_P9_ContainmentBarrierTask = ", query);
						});
					}
				}
			}
		}

		public void TestIsStartable_WhenTaskStartabilityNotYetCached()
		{
			BMSTestHelper.CreateSystem(Factory, "INQ");
			var workflow1 = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "workflow");
			var workflow2 = BMSTestHelper.CreateWorkflowAndParents<SalesEnquiry>(Factory, "workflow");
			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code);
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code);

			Factory.Save();

			var startable = new ConcurrentHashSet<ZGuid>();
			startable.TryAdd(task1.PK);
			var map = new IsStartableMap(startable, ImmutableHashSet<ZGuid>.Empty, new ConcurrentHashSet<ZGuid>());
			var service = new TaskStartabilityService(map);
			Factory.ServiceContainer.AddService(service);

			AssertEquals("The startability of task2 should have been evaluated even though it wasn't included in the original startability map. SAD!", true, task2.IsStartable());
		}

		[GuiTest]
		public void TestIsStartable_FirstIterationInNewWorkflow_SecondIterationInOriginalWorkflow()
		{
			(var system, var buffer) = BMSTestHelper.CreateSystemAndBuffer(Factory, "ORG");
			var tuple = BMSTestHelper.CreateSectionAndViewModel(buffer);
			var section = tuple.Item1;
			var sectionViewModel = tuple.Item2;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow");
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", workflow.JobHeader.FH_WorkflowType);

			var task1 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, sequence: 1);
			var qcbTask = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskType: "QCB", sequence: 2);
			var task2 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, sequence: 3);
			Factory.Save();

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			using (var viewModel = new ContainmentBarrierViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed))
			{
				viewModel.Response = ContainmentBarrierResponses.IterationRequired;
				viewModel.ShouldCreateWorkflowForIteration = true;
				viewModel.IterateFromTaskPK = task1.PK;
				viewModel.CommitResponse();
				qcbTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();
			}

			var newWorkflow = workflow.JobHeader.ProcessHeaders.First(p => p != workflow);
			var newTask1 = newWorkflow.Tasks.First(t => t.P9_Sequence == 3);
			var newQcbTask = newWorkflow.Tasks.First(t => t.P9_Sequence == 4);

			newTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			using (var viewModel = new ContainmentBarrierViewModel(newQcbTask, ProcessTaskStatusCodeList.Codes.Closed))
			{
				viewModel.Response = ContainmentBarrierResponses.IterationRequired;
				viewModel.ShouldCreateWorkflowForIteration = false;
				viewModel.IterateFromTaskPK = newTask1.PK;
				viewModel.CommitResponse();
				newQcbTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();
			}

			BMBoardSectionViewModel.CreateAndPopulatePropertyCache(TaskChannelMap.ForTest(section, sectionViewModel, workflow, newWorkflow), sectionViewModel);

			var newestTask1 = newWorkflow.Tasks.First(t => t.P9_Sequence == 5);
			var newestQcbTask = newWorkflow.Tasks.First(t => t.P9_Sequence == 6);

			CombineAssertions(() =>
			{
				AssertEquals("Should be 2 workflows in total", 2, workflow.JobHeader.ProcessHeaders.Count);
				AssertEquals("task2.P9_SequenceNumber should get up to 7", 7, task2.P9_Sequence);

				AssertEquals("newestTask1 should be startable", true, newestTask1.IsStartable());
				AssertEquals("newestQcbTask1 should NOT be startable", false, newestQcbTask.IsStartable());
				AssertEquals("task2 should NOT be startable", false, task2.IsStartable());

				AssertEquals("newestTask1 should be startable (cache value)", true, sectionViewModel.Cache.GetCachedValue(newestTask1.PK, TaskJobWorkflowCacheHelper.CacheConstants.IsCurrent, newestTask1.IsStartable));
				AssertEquals("newestQcbTask1 should NOT be startable (cache value)", false, sectionViewModel.Cache.GetCachedValue(newestQcbTask.PK, TaskJobWorkflowCacheHelper.CacheConstants.IsCurrent, newestQcbTask.IsStartable));
				AssertEquals("task2 should NOT be startable (cache value)", false, sectionViewModel.Cache.GetCachedValue(task2.PK, TaskJobWorkflowCacheHelper.CacheConstants.IsCurrent, task2.IsStartable));
			});
		}

		#endregion

		#region IsStartable_IgnoringIterations

		[GuiTest]
		public void TestIsStartable_IgnoringIterations_WhenQualityIterationWorkflowIsBlocked()
		{
			WorkflowDataRegistry.Instance.CreateNewWorkflowsForQualityIterationsByDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.IgnoreIterationsWhenCalculatingStartability.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var staffUnderReview = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var reviewer = CreateStaffInCurrentBranchDept("BIL", "Bilbo Baggins");

			var mainWorkflow = BMSTestHelper.CreateWorkflow(Factory, "Main Workflow");

			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", mainWorkflow.JobHeader.FH_WorkflowType);

			var workingTask = CreateTask(mainWorkflow, staffUnderReview.GS_Code, lowEstMinutes: 1, sequence: 1);
			var qcbTask = CreateTask(mainWorkflow, reviewer.GS_Code, lowEstMinutes: 1, sequence: 2, taskType: "QCB");
			var lastTask = CreateTask(mainWorkflow, staffUnderReview.GS_Code, lowEstMinutes: 1, sequence: 100);

			Factory.Save();

			workingTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			using (var viewModel = new ContainmentBarrierViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed))
			{
				viewModel.Response = ContainmentBarrierResponses.IterationRequired;
				viewModel.IterateFromTaskPK = workingTask.PK;

				viewModel.CommitResponse();
				qcbTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();
			}

			var qiWorkflow = mainWorkflow.JobHeader.ProcessHeaders.First(p => p != mainWorkflow);

			AssertEquals("QualityIterationWorkflow should have 2 tasks", 2, qiWorkflow.Tasks.Count());

			var workingTaskInQIWorkflow = qiWorkflow.Tasks.First(t => t.P9_Type == workingTask.P9_Type);
			var qcbTaskInQIWorkflow = qiWorkflow.Tasks.First(t => t.P9_Type == qcbTask.P9_Type);

			CombineAssertions(() =>
			{
				AssertEquals("Completed workingTask should not be startable", false, workingTask.IsStartable());
				AssertEquals("Completed qcbTask should not be startable", false, qcbTask.IsStartable());
				AssertEquals("lastTask should be startable", true, lastTask.IsStartable());
				AssertEquals("workingTaskInQIWorkflow has no prereqs tasks or workflows so should be startable", true, workingTaskInQIWorkflow.IsStartable());
				AssertEquals("qcbTaskInQIWorkflow task should not be startable", false, qcbTaskInQIWorkflow.IsStartable());
			});

			var prereqOfQIWorkflow = BMSTestHelper.CreateWorkflow(Factory, "Prereq of Quality Iteration Workflow");
			var taskInprereqOfQIWorkflow = CreateTask(prereqOfQIWorkflow, staffUnderReview.GS_Code, lowEstMinutes: 1, sequence: 1);
			prereqOfQIWorkflow.GetOrCreateDependencyLink(qiWorkflow);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Completed workingTask should not be startable", false, workingTask.IsStartable());
				AssertEquals("Completed qcbTask should not be startable", false, qcbTask.IsStartable());
				AssertEquals("lastTask should be startable", true, lastTask.IsStartable());
				AssertEquals("workingTaskInQIWorkflow has prereqs workflow so should not be startable", false, workingTaskInQIWorkflow.IsStartable());
				AssertEquals("qcbTaskInQIWorkflow task should not be startable", false, qcbTaskInQIWorkflow.IsStartable());
				AssertEquals("taskInprereqOfQIWorkflow  has no prereqs tasks or workflows so should be startable", true, taskInprereqOfQIWorkflow.IsStartable());
			});
		}

		[GuiTest]
		public void TestIsStartable_IgnoringIterations_WhenOpenQualityIterationsExist()
		{
			BMSRegistry.Instance.IgnoreIterationsWhenCalculatingStartability.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "ORG");

			var resource1 = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var resource2 = CreateStaffInCurrentBranchDept("BIL", "Bilbo Baggins");

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];
			var qualityIterationWorkflow = jobHeader.ProcessHeaders.AddNew();
			qualityIterationWorkflow.GetOrCreateLinkToParent(workflow);

			var task1 = CreateTask(workflow, resource1.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var qcbTask = CreateTask(workflow, resource2.GS_Code, 60, taskType: "QCB");
			var task3 = CreateTask(workflow, resource1.GS_Code, 60);

			var qiTask = CreateTask(qualityIterationWorkflow, resource1.GS_Code, 60);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Completed task1 should not be current", false, task1.IsStartable());
				AssertEquals("Open qcbTask should be current", true, qcbTask.IsStartable());
				AssertEquals("Blocked task3 should not be current", false, task3.IsStartable());
				AssertEquals("qiTask has no prereq tasks or workflows so should be current", true, qiTask.IsStartable());
			});

			qcbTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Completed task1 should not be current", false, task1.IsStartable());
				AssertEquals("Completed qcbTask should not be current", false, qcbTask.IsStartable());
				AssertEquals("task3 has no prereq tasks or workflows so should be current", true, task3.IsStartable());
				AssertEquals("qiTask has no prereq tasks or workflows so should be current", true, qiTask.IsStartable());
			});

			var pivot = CreateIterationLink(qcbTask, qualityIterationWorkflow, task1, IterationLinkTypeList.Codes.QualityIterationTask);

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Completed task1 should not be current", false, task1.IsStartable());
				AssertEquals("Completed qcbTask should not be current", false, qcbTask.IsStartable());
				AssertEquals("task3 should remain current because the iteration does not affect its startability", true, task3.IsStartable());
				AssertEquals("qiTask has no prereq tasks or workflows so should be current", true, qiTask.IsStartable());
			});

			qiTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Completed task1 should not be current", false, task1.IsStartable());
				AssertEquals("Completed qcbTask should not be current", false, qcbTask.IsStartable());
				AssertEquals("task3 has a closed prereq quality iteration workflow so should be current now", true, task3.IsStartable());
				AssertEquals("Completed qiTask should not be current", false, qiTask.IsStartable());
			});
		}

		[GuiTest]
		public void TestIsStartable_IgnoringIterations_FirstIterationInNewWorkflow_SecondIterationInOriginalWorkflow()
		{
			BMSRegistry.Instance.IgnoreIterationsWhenCalculatingStartability.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			(var system, var buffer) = BMSTestHelper.CreateSystemAndBuffer(Factory, "ORG");
			var tuple = BMSTestHelper.CreateSectionAndViewModel(buffer);
			var section = tuple.Item1;
			var sectionViewModel = tuple.Item2;

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "workflow");
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", workflow.JobHeader.FH_WorkflowType);

			var task1 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, sequence: 1);
			var qcbTask = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, taskType: "QCB", sequence: 2);
			var task2 = BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, sequence: 3);
			Factory.Save();

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			using (var viewModel = new ContainmentBarrierViewModel(qcbTask, ProcessTaskStatusCodeList.Codes.Closed))
			{
				viewModel.Response = ContainmentBarrierResponses.IterationRequired;
				viewModel.ShouldCreateWorkflowForIteration = true;
				viewModel.IterateFromTaskPK = task1.PK;
				viewModel.CommitResponse();
				qcbTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();
			}

			var newWorkflow = workflow.JobHeader.ProcessHeaders.First(p => p != workflow);
			var newTask1 = newWorkflow.Tasks.First(t => t.P9_Sequence == 3);
			var newQcbTask = newWorkflow.Tasks.First(t => t.P9_Sequence == 4);

			newTask1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			using (var viewModel = new ContainmentBarrierViewModel(newQcbTask, ProcessTaskStatusCodeList.Codes.Closed))
			{
				viewModel.Response = ContainmentBarrierResponses.IterationRequired;
				viewModel.ShouldCreateWorkflowForIteration = false;
				viewModel.IterateFromTaskPK = newTask1.PK;
				viewModel.CommitResponse();
				newQcbTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Factory.Save();
			}

			BMBoardSectionViewModel.CreateAndPopulatePropertyCache(TaskChannelMap.ForTest(section, sectionViewModel, workflow, newWorkflow), sectionViewModel);

			var newestTask1 = newWorkflow.Tasks.First(t => t.P9_Sequence == 5);
			var newestQcbTask = newWorkflow.Tasks.First(t => t.P9_Sequence == 6);

			CombineAssertions(() =>
			{
				AssertEquals("Should be 2 workflows in total", 2, workflow.JobHeader.ProcessHeaders.Count);
				AssertEquals("task2.P9_SequenceNumber should get up to 7", 7, task2.P9_Sequence);

				AssertEquals("newestTask1 should be startable", true, newestTask1.IsStartable());
				AssertEquals("newestQcbTask1 should NOT be startable", false, newestQcbTask.IsStartable());
				AssertEquals("task2 should be startable", true, task2.IsStartable());

				AssertEquals("newestTask1 should be startable (cache value)", true, sectionViewModel.Cache.GetCachedValue(newestTask1.PK, TaskJobWorkflowCacheHelper.CacheConstants.IsCurrent, newestTask1.IsStartable));
				AssertEquals("newestQcbTask1 should NOT be startable (cache value)", false, sectionViewModel.Cache.GetCachedValue(newestQcbTask.PK, TaskJobWorkflowCacheHelper.CacheConstants.IsCurrent, newestQcbTask.IsStartable));
				AssertEquals("task2 should be startable (cache value)", true, sectionViewModel.Cache.GetCachedValue(task2.PK, TaskJobWorkflowCacheHelper.CacheConstants.IsCurrent, task2.IsStartable));
			});
		}

		#endregion

		#region IsAssignedToResourceOrResourceHasCapability

		public void TestCanTaskBeShownInResourceChannel()
		{
			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			var resource1 = CreateStaffInCurrentBranchDept("BEN", "Benedict Cumberbetch", capability1);
			var resource2 = CreateStaffInCurrentBranchDept("PRL", "Fla sai dah", capability1);

			var workflow = CreateWorkflow(CreateJobHeader<OrgHeader>(), "workflow");
			var task1 = CreateTask(workflow, resource1.GS_Code, 60);
			var task2 = CreateTask(workflow, string.Empty, 60, capability: capability1);
			var task3 = CreateTask(workflow, string.Empty, 60, capability: capability2);

			AssertEquals(true, task1.CanTaskBeShownInResourceChannel(resource1));
			AssertEquals(true, task2.CanTaskBeShownInResourceChannel(resource1));
			AssertEquals(false, task3.CanTaskBeShownInResourceChannel(resource1));

			AssertEquals(false, task1.CanTaskBeShownInResourceChannel(resource2));
			AssertEquals(true, task2.CanTaskBeShownInResourceChannel(resource2));
			AssertEquals(false, task3.CanTaskBeShownInResourceChannel(resource2));

			task2.P9_GS_NKAssignedStaffMember = resource1.GS_Code;

			AssertEquals(true, task1.CanTaskBeShownInResourceChannel(resource1));
			AssertEquals(true, task2.CanTaskBeShownInResourceChannel(resource1));
			AssertEquals(false, task3.CanTaskBeShownInResourceChannel(resource1));

			AssertEquals(false, task1.CanTaskBeShownInResourceChannel(resource2));
			AssertEquals(false, task2.CanTaskBeShownInResourceChannel(resource2));
			AssertEquals(false, task3.CanTaskBeShownInResourceChannel(resource2));
		}

		#endregion

		#region IsInWorkflowWithCCR

		public void TestIsInWorkflowWithCCR()
		{
			var resourceCCR = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CCR", "CCR Staff");
			var resourceNonCCR = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "NCC", "Non CCR Staff");

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			resourceCCR.DesignateAsCCR(buffer);

			var worflow = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: ZDateTime.Today, staffCode: resourceNonCCR.GS_Code, lowEstMinutes: 60, sequence: 400);
			var task = worflow.Parent.WorkflowItems[0];

			var preTask = BMSTestHelper.CreateTask(worflow, resourceCCR.GS_Code, sequence: 400);
			AssertEquals("Pre-constraint Filter: IsInWorkflowWithCCR must be TRUE if there is CCR-task with same sequence-number", true, task.IsInWorkflowWithCCR(ccrConstraintFilter: CCRConstraintFilter.Preconstraint));
			preTask.P9_Sequence = 500;
			AssertEquals("Pre-constraint Filter: IsInWorkflowWithCCR must be TRUE if there is CCR-task with greater-than sequence-number", true, task.IsInWorkflowWithCCR(ccrConstraintFilter: CCRConstraintFilter.Preconstraint));

			var postTask = BMSTestHelper.CreateTask(worflow, resourceCCR.GS_Code, sequence: 300);
			AssertEquals("Post-constraint Filter: IsInWorkflowWithCCR must be TRUE if there is CCR-task with less-than sequence-number", true, task.IsInWorkflowWithCCR(ccrConstraintFilter: CCRConstraintFilter.Postconstraint));

			worflow = BMSTestHelper.CreateWorkflowAndTask(Factory, string.Empty, buffer, releaseDateTime: ZDateTime.Today, staffCode: resourceCCR.GS_Code, sequence: 60);
			task = worflow.Parent.WorkflowItems[0];
			AssertEquals("None-constraint Filter: IsInWorkflowWithCCR must be TRUE if the current task's resource is CCR", true, task.IsInWorkflowWithCCR(ccrConstraintFilter: CCRConstraintFilter.None));
		}

		#endregion

		#region Designated as CCR By

		public void TestDesignatedAsCcrByColumn_RespondsToChangesInCcrs()
		{
			var testConfig = TestConfigsHelper.CreateConstrainedSchematicTestConfig(Factory);
			var componentLink = testConfig.Buffer.GetOrCreateResourceLink(testConfig.CCR.GS_Code);
			AssertEquals(GlbStaff.GetCurrentUser(Factory).GS_Code, componentLink.FD_GS_NKDesignatedAsCapacityConstrainedBy);
			AssertEquals(GlbStaff.GetCurrentUser(Factory).GS_FullName, componentLink.MarkedAsCapacityConstrainedByFullName);

			testConfig.CCR.DesignateAsNonCCR(testConfig.Buffer);
			AssertEquals(string.Empty, componentLink.FD_GS_NKDesignatedAsCapacityConstrainedBy);
			AssertEquals(string.Empty, componentLink.MarkedAsCapacityConstrainedByFullName);

			using (Env.SetTemporaryUserContext(User.WebUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				testConfig.CCR.DesignateAsCCR(testConfig.Buffer);
			}

			var query = new ZQuery(GlbStaffSchema.GS_Code, User.WebUserCode);
			var userFullName = Factory.LoadTop1<GlbStaff>(query) != null ? Factory.LoadTop1<GlbStaff>(query).GS_FullName : ZString.Empty;

			AssertEquals(User.WebUserCode, componentLink.FD_GS_NKDesignatedAsCapacityConstrainedBy);
			AssertEquals(userFullName, componentLink.MarkedAsCapacityConstrainedByFullName);
		}

		#endregion

		#region GetJobHeader

		[TestDate(2019, 7, 29)]
		public void TestGetJobHeader_WhenMultipleJobHeadersExistOnTemplate_ShouldReportErrorWithDetails()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "INQ");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "INQ", "CCR", "WEB", name: "Beautifully cooked");
			var workflow1 = BMSTestHelper.CreateWorkflow(template, "Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(template, "Workflow 2");

			var task1 = template.WorkflowItems.Tasks.AddNew();
			var task2 = template.WorkflowItems.Tasks.AddNew();
			var task3 = template.WorkflowItems.Tasks.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;
			task2.P9_FH_ProcessHeader = workflow1.PK;
			task3.P9_FH_ProcessHeader = workflow2.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedTemplate = newFactory.Load<ProcessTaskTemplate>(template.PK);

			var extraJobLevelWorkflow = newFactory.New<ProcessJobHeader>();
			extraJobLevelWorkflow.FH_CompletionStatement = "Extra JLW";
			extraJobLevelWorkflow.FH_SystemCreateUser = "ADK";
			extraJobLevelWorkflow.FH_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1);

			loadedTemplate.ProcessHeaders.Add(extraJobLevelWorkflow);
			var extraWorkflow = BMSTestHelper.CreateWorkflow(extraJobLevelWorkflow, "Workflow 3");

			try
			{
				loadedTemplate.GetJobHeader();

				AssertEquals("TemplateAppliedWithMultipleJobLevelWorkflows", ErrorReporter.LastKeyReported);

				var version1 = @"1. Workflow 1 (2 tasks)
2. Workflow 2 (1 task)";
				var version2 = @"1. Workflow 2 (1 task)
2. Workflow 1 (2 tasks)";

				var expected = @"Tried to apply a workflow template that has multiple job-level workflows. Each template may only have one. SAD!

Template details:
Name: Beautifully cooked
Workflow Type: INQ
Selection Criteria: CCR, WEB
System: N
Partial: N
Universal: N

Job-Level Workflow: Job is complete.
Created: 29 Jul 2019 00:00
Created by: E
Workflows for this Job-Level Workflow:
{0}

Job-Level Workflow: Extra JLW
Created: 30 Jul 2019 00:00
Created by: ADK
Workflows for this Job-Level Workflow:
1. Workflow 3 (0 tasks)";

				Assert(string.Format(expected, version1) == ErrorReporter.LastMessageReported || string.Format(expected, version2) == ErrorReporter.LastMessageReported);
			}
			finally
			{
				if (ErrorReporter.TotalErrorCount == 1)
				{
					ErrorReporter.Clear();
				}
			}
		}

		[TestDate(2019, 7, 29)]
		public void TestGetJobHeader_WhenMultipleJobHeadersExistOnTemplate_ShouldSelectOneDeterministically_AndNotThrowException()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "INQ");
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "INQ", "CCR", "WEB", name: "Beautifully cooked");
			var workflow1 = BMSTestHelper.CreateWorkflow(template, "Workflow 1");

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedTemplate = newFactory.Load<ProcessTaskTemplate>(template.PK);

			var extraJobHeader1 = newFactory.New<ProcessJobHeader>();
			extraJobHeader1.FH_CompletionStatement = "BBB Extra JLW 1";
			extraJobHeader1.FH_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1);
			loadedTemplate.ProcessHeaders.Add(extraJobHeader1);

			var extraWorkflow1 = BMSTestHelper.CreateWorkflow(extraJobHeader1, "Workflow 2");
			var extraWorkflow2 = BMSTestHelper.CreateWorkflow(extraJobHeader1, "Workflow 3");

			TestDateAttribute.AddDays(1);
			var extraJobHeader2 = newFactory.New<ProcessJobHeader>();
			extraJobHeader2.FH_CompletionStatement = "AAA Extra JLW 2";
			extraJobHeader2.FH_SystemCreateTimeUtc = ZDateTime.Now.AddDays(2);
			loadedTemplate.ProcessHeaders.Add(extraJobHeader2);

			var extraWorkflow3 = BMSTestHelper.CreateWorkflow(extraJobHeader2, "Workflow 4");
			var extraWorkflow4 = BMSTestHelper.CreateWorkflow(extraJobHeader2, "Workflow 5");

			var resultJobHeader = loadedTemplate.GetJobHeader();
			AssertEquals("There should have been an error reported, the details of which are tested in another test.", 1, ErrorReporter.TotalErrorCount);
			AssertEquals("There should have been an error reported, the details of which are tested in another test.", "TemplateAppliedWithMultipleJobLevelWorkflows", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();

			AssertEquals("The Job-level workflow with the most workflows, then the one created earliest, should be the chosen workflow. SAD!", "BBB Extra JLW 1", resultJobHeader.FH_CompletionStatement);
			AssertContainsExactElementsInAnyOrder(new[] { "Workflow 2", "Workflow 3" }, resultJobHeader.ProcessHeaders.Select(x => x.FH_CompletionStatement));
		}

		#endregion
	}
}
