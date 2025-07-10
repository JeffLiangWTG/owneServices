using System;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Integration;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Service.Test
{
	public abstract class ComponentSectionBuilderTestBase : TestCaseWithFactory
	{
		#region Tag

		[TestDate(2018, 3, 17, 18, 30, 0)]
		public void TestGetData_ShouldReturnCorrectComponentSectionTagDTOs()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");
			system.FS_Name = "System";

			var buffer = BMSTestHelper.CreateBuffer(system, "buffer", 300);
			var board = system.Boards.AddNew();

			var bufferSection = board.Sections.AddNew();
			bufferSection.MS_FC_Component = buffer.PK;
			var sectionPK = bufferSection.PK.ToGuid();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";
			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, staff.PK);

			var workItem = Factory.New<IWorkItem>();
			var header = ProcessJobHeader.GetForParent(workItem as IWorkflowProvider, Factory);
			var workflow1 = BMSTestHelper.CreateProcessHeader(header, buffer, "Workflow 1", TestDateAttribute.Date.AddDays(-10));
			var task = VisualBoardsTestCase.CreateTask(workflow1, staff.GS_Code, 300, taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 1, description: "Task 1");

			var tagMagnitude1 = BMSTestHelper.CreateTagMagnitude(BMSTestHelper.CreateTagDefinition(Factory, "TD1"), "TG1", color: Color.White);
			workflow1.AddTag(tagMagnitude1, false);

			var tagMagnitude2 = BMSTestHelper.CreateTagMagnitude(BMSTestHelper.CreateTagDefinition(Factory, "TD2"), "TG2", color: Color.Red);
			task.AddTag(tagMagnitude2, false);

			Factory.Save();

			var expectedTags = new[] { tagMagnitude1, tagMagnitude2 };

			var sectionDTO = BuildSectionDTO(bufferSection);
			var serviceSectionDTO = sectionDTO as ComponentSectionDTO;

			AssertNotNull(serviceSectionDTO);
			AssertEquals(sectionPK, serviceSectionDTO.PK);
			AssertionHelper.AssertTags(expectedTags, serviceSectionDTO.Tags.ToArray());
		}

		#endregion

		#region Job

		[TestDate(2018, 3, 17, 18, 30, 0)]
		public void TestGetData_ShouldReturnCorrectComponentSectionJobDTOs()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");
			system.FS_Name = "System";

			var buffer = BMSTestHelper.CreateBuffer(system, "buffer", 300);
			var board = system.Boards.AddNew();

			var bufferSection = board.Sections.AddNew();
			bufferSection.MS_FC_Component = buffer.PK;
			var sectionPK = bufferSection.PK.ToGuid();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST1";

			var workItem1 = Factory.New<IWorkItem>();
			var header1 = ProcessJobHeader.GetForParent(workItem1 as IWorkflowProvider, Factory);
			var workflow1 = BMSTestHelper.CreateProcessHeader(header1, buffer, "Workflow 1", TestDateAttribute.Date.AddDays(-10));
			VisualBoardsTestCase.CreateTask(workflow1, staff1.GS_Code, 300, taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 1, description: "Task 1");

			var workItem2 = Factory.New<IWorkItem>();
			var header2 = ProcessJobHeader.GetForParent(workItem2 as IWorkflowProvider, Factory);
			var workflow2 = BMSTestHelper.CreateProcessHeader(header2, buffer, "Workflow 2", TestDateAttribute.Date.AddDays(-10));
			VisualBoardsTestCase.CreateTask(workflow2, staff1.GS_Code, 300, taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 1, description: "Task 2");

			var expectedJobs = new[] { header1.Parent, header2.Parent };

			Factory.Save();

			var sectionDTO = BuildSectionDTO(bufferSection);
			var serviceSectionDTO = sectionDTO as ComponentSectionDTO;

			AssertNotNull(serviceSectionDTO);
			AssertEquals(sectionPK, serviceSectionDTO.PK);
			AssertionHelper.AssertJobs(expectedJobs, serviceSectionDTO.Jobs.ToArray());
		}

		#endregion

		#region Task

		[TestDate(2018, 3, 17, 18, 30, 0)]
		public void TestGetData_ShouldReturnCorrectComponentSectionTaskDTOs()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");
			system.FS_Name = "System";

			var buffer = BMSTestHelper.CreateBuffer(system, "buffer", 300);
			var board = system.Boards.AddNew();

			var bufferSection = board.Sections.AddNew();
			bufferSection.MS_FC_Component = buffer.PK;
			var sectionPK = bufferSection.PK.ToGuid();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";
			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, staff.PK);

			var workItem1 = Factory.New<IWorkItem>();
			var header1 = ProcessJobHeader.GetForParent(workItem1 as IWorkflowProvider, Factory);
			var workflow1 = BMSTestHelper.CreateProcessHeader(header1, buffer, "Workflow 1", TestDateAttribute.Date.AddDays(-11));
			var task1 = VisualBoardsTestCase.CreateTask(workflow1, staff.GS_Code, 300, taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 1, description: "Task 1");

			var workItem2 = Factory.New<IWorkItem>();
			var header2 = ProcessJobHeader.GetForParent(workItem2 as IWorkflowProvider, Factory);
			var workflow2 = BMSTestHelper.CreateProcessHeader(header2, buffer, "Workflow 2", TestDateAttribute.Date.AddDays(-10));
			var task2 = VisualBoardsTestCase.CreateTask(workflow2, staff.GS_Code, 300, taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 1, description: "Task 2");

			var tasks = new[] { task1, task2 };

			Factory.Save();

			var sectionDTO = BuildSectionDTO(bufferSection);
			var serviceSectionDTO = sectionDTO as ComponentSectionDTO;

			AssertNotNull(serviceSectionDTO);
			AssertEquals(sectionPK, serviceSectionDTO.PK);

			AssertionHelper.AssertTasks(tasks, serviceSectionDTO.Tasks.ToArray());
		}

		[TestDate(2018, 3, 17, 18, 30, 0)]
		public void TestGetData_TaskLowEstimate_ShouldNotHaveRoundingErrors()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "System";
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Resource 1");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow", buffer);
			var task = BMSTestHelper.CreateTask(workflow, staffCode: "AAA", lowEstMinutes: 10, estVariationFactor: 3);
			task.P9_EstimatedTimeToComplete = new ZDateTime(2018, 1, 1, 0, 20, 0);

			var board = system.Boards.AddNew();
			var bufferSection = board.Sections.AddNew();
			var sectionPK = bufferSection.PK.ToGuid();

			bufferSection.MS_FC_Component = buffer.PK;
			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, resource1.PK);

			Factory.Save();

			var componentSectionDTO = BuildSectionDTO(bufferSection) as ComponentSectionDTO;
			var taskDTO = componentSectionDTO.Tasks.First();

			CombineAssertions(() =>
			{
				AssertEquals("Low estimate should be exactly 10 minutes", 10m, taskDTO.Properties["lowEstimatedMinutes"]);
				AssertEquals("Standard estimate should be exactly 20 minutes", 20m, taskDTO.Properties["standardEstimatedMinutes"]);
				AssertEquals("High estimate should be exactly 20 minutes", 30m, taskDTO.Properties["highEstimatedMinutes"]);
				AssertEquals("Estimate time to complete should be exactly 20 minutes", 20m, taskDTO.Properties["estimatedTimeToCompleteMinutes"]);
			});
		}

		[TestDate(2018, 3, 17, 18, 30, 0)]
		public void TestGetData_ShouldOnlyShowWorkflowsAndTaskInReleaseGroup_WhenShowWorkInReleaseGroupOnly()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");
			system.FS_Name = "System";

			var buffer = BMSTestHelper.CreateBuffer(system, "buffer", 300);
			var board = system.Boards.AddNew();

			var bufferSection = board.Sections.AddNew();
			bufferSection.MS_FC_Component = buffer.PK;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";
			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, staff.PK);

			var group1 = BMSTestHelper.CreateGroup(Factory, "GP1", "Group1");
			var releaseGroup1 = BMSTestHelper.CreateReleaseGroup(system, group1);
			var group2 = BMSTestHelper.CreateGroup(Factory, "GP2", "Group2");
			var releaseGroup2 = BMSTestHelper.CreateReleaseGroup(system, group2);

			var workItem = Factory.New<IWorkItem>();
			var header = ProcessJobHeader.GetForParent(workItem as IWorkflowProvider, Factory);
			var workflowInReleaseGroup1 = BMSTestHelper.CreateWorkflow(header, "Workflow1", buffer, releaseGroupPK: releaseGroup1.FSG_GG_Group);
			var workflowInReleaseGroup2 = BMSTestHelper.CreateWorkflow(header, "Workflow2", buffer, releaseGroupPK: releaseGroup2.FSG_GG_Group);

			var taskInWorkflowInReleaseGroup1 = VisualBoardsTestCase.CreateTask(workflowInReleaseGroup1, staff.GS_Code, 300, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 1, description: "taskInWorkflowInReleaseGroup1");
			var taskInWorkflowInReleaseGroup2 = VisualBoardsTestCase.CreateTask(workflowInReleaseGroup2, staff.GS_Code, 300, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 1, description: "taskinWorkflowInReleaseGroup2");
			var taskInGroup1InWorkflowInReleaseGroup2 = VisualBoardsTestCase.CreateTask(workflowInReleaseGroup2, staff.GS_Code, 300, taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, sequence: 1, description: "taskinWorkflowInReleaseGroup2");
			taskInGroup1InWorkflowInReleaseGroup2.P9_GG_AssignedGroup = group1.PK;

			Factory.Save();

			AssertEquals(false, bufferSection.SectionConfiguration.ShowWorkInReleaseGroupOnly);

			var serviceSectionDTO = BuildSectionDTO(bufferSection) as ComponentSectionDTO;

			AssertContainsExactElementsInAnyOrder("Should contain all tasks, ShowWorkInReleaseGroupOnly is false", new[] {
				taskInWorkflowInReleaseGroup1.PK.ToGuid(),
				taskInWorkflowInReleaseGroup2.PK.ToGuid() ,
				taskInGroup1InWorkflowInReleaseGroup2.PK.ToGuid() },
				serviceSectionDTO.Tasks.Select(t => t.PK).ToArray());

			bufferSection.SectionConfiguration.ReleaseGroupPK = releaseGroup1.FSG_GG_Group;
			bufferSection.SectionConfiguration.ShowWorkInReleaseGroupOnly = true;

			Factory.Save();

			serviceSectionDTO = BuildSectionDTO(bufferSection) as ComponentSectionDTO;

			AssertContainsExactElementsInAnyOrder("Should contains only taskInWorkflowInReleaseGroup1 and taskInGroup1InWorkflowInReleaseGroup2", new[] {
				taskInWorkflowInReleaseGroup1.PK.ToGuid(),
				taskInGroup1InWorkflowInReleaseGroup2.PK.ToGuid() },
				serviceSectionDTO.Tasks.Select(t => t.PK).ToArray());
		}

		#endregion

		#region Workflow

		[TestDate(2018, 3, 17, 18, 30, 0)]
		public void TestGetData_ShouldReturnCorrectComponentSectionWorkflowDTOs()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");
			system.FS_Name = "System";

			var buffer = BMSTestHelper.CreateBuffer(system, "buffer", 300);
			var board = system.Boards.AddNew();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST1";

			var bufferSection = board.Sections.AddNew();
			bufferSection.MS_FC_Component = buffer.PK;
			var sectionPK = bufferSection.PK.ToGuid();

			var workItem1 = Factory.New<IWorkItem>();
			var header1 = ProcessJobHeader.GetForParent(workItem1 as IWorkflowProvider, Factory);
			var workflow1 = BMSTestHelper.CreateProcessHeader(header1, buffer, "Workflow 1", TestDateAttribute.Date.AddDays(-9));
			VisualBoardsTestCase.CreateTask(workflow1, staff1.GS_Code, 300, taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 1, description: "Task 1");

			var workItem2 = Factory.New<IWorkItem>();
			var header2 = ProcessJobHeader.GetForParent(workItem2 as IWorkflowProvider, Factory);
			var workflow2 = BMSTestHelper.CreateProcessHeader(header2, buffer, "Workflow 2", TestDateAttribute.Date.AddDays(-10));
			VisualBoardsTestCase.CreateTask(workflow2, staff1.GS_Code, 300, taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 1, description: "Task 2");

			var expectedWorkflows = new[] { workflow1, workflow2 };

			Factory.Save();

			var sectionDTO = BuildSectionDTO(bufferSection);
			var serviceSectionDTO = sectionDTO as ComponentSectionDTO;

			AssertNotNull(serviceSectionDTO);
			AssertEquals(sectionPK, serviceSectionDTO.PK);
			AssertionHelper.AssertWorkflows(expectedWorkflows.ToArray(), serviceSectionDTO.Workflows.ToArray());
		}

		#region Parent Workflow

		public void TestParentWorkflow_WhenTopParentIsClosed()
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "ORG");
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "System";
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Resource 1");
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			var header = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var board = system.Boards.AddNew();
			var bufferSection = board.Sections.AddNew();
			var sectionPK = bufferSection.PK.ToGuid();

			bufferSection.MS_FC_Component = buffer.PK;
			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, resource1.PK);

			var parentWorkflow = BMSTestHelper.CreateProcessHeader(header, buffer, "Workflow with child", TestDateAttribute.Date.AddDays(1));
			var parentWorkflowQCBTask = VisualBoardsTestCase.CreateTask(parentWorkflow, "AAA", 30, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 1, description: "mainWorkflowTask");

			var childWorkflow = BMSTestHelper.CreateProcessHeader(header, buffer, "child Workflow1", TestDateAttribute.Date.AddDays(1));
			var childWorkflow1QCBTask = VisualBoardsTestCase.CreateTask(childWorkflow, "AAA", 30, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 2, description: "childWorkflowTask1");
			BMSTestHelper.CreateParentChildLink(parentWorkflow, childWorkflow);
			BMSTestCaseWithFactory.CreateIterationLink(parentWorkflowQCBTask, childWorkflow);

			var childOfChildWorkflow = BMSTestHelper.CreateProcessHeader(header, buffer, "child Workflow2", TestDateAttribute.Date.AddDays(1));
			var childWorkflow2Task = VisualBoardsTestCase.CreateTask(childOfChildWorkflow, "AAA", 30, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 3, description: "childWorkflowTask2");
			BMSTestHelper.CreateParentChildLink(childWorkflow, childOfChildWorkflow);
			BMSTestCaseWithFactory.CreateIterationLink(childWorkflow1QCBTask, childOfChildWorkflow);

			Factory.Save();

			var componentSectionDTO = BuildSectionDTO(bufferSection) as ComponentSectionDTO;

			AssertEquals(2, componentSectionDTO.Workflows.Count());

			var childWorkflowDTO = componentSectionDTO.Workflows.First(workflow => workflow.PK == childWorkflow.PK);
			var childOfChildWorkflowDTO = componentSectionDTO.Workflows.First(workflow => workflow.PK == childOfChildWorkflow.PK);

			AssertNull(childWorkflowDTO.ParentPK);
			AssertEquals(childWorkflowDTO.PK, childOfChildWorkflowDTO.ParentPK);
		}

		public void TestParentWorkflow_WhenOneChildIsClosed()
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "ORG");
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "System";
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Resource 1");
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			var header = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var board = system.Boards.AddNew();
			var bufferSection = board.Sections.AddNew();
			var sectionPK = bufferSection.PK.ToGuid();

			bufferSection.MS_FC_Component = buffer.PK;
			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, resource1.PK);

			var parentWorkflow = BMSTestHelper.CreateProcessHeader(header, buffer, "Workflow with child", TestDateAttribute.Date.AddDays(1));
			var parentWorkflowQCBTask = VisualBoardsTestCase.CreateTask(parentWorkflow, "AAA", 30, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 1, description: "mainWorkflowTask");

			var childWorkflow = BMSTestHelper.CreateProcessHeader(header, buffer, "child Workflow1", TestDateAttribute.Date.AddDays(1));
			var childWorkflow1QCBTask = VisualBoardsTestCase.CreateTask(childWorkflow, "AAA", 30, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 2, description: "childWorkflowTask1");
			BMSTestHelper.CreateParentChildLink(parentWorkflow, childWorkflow);
			BMSTestCaseWithFactory.CreateIterationLink(parentWorkflowQCBTask, childWorkflow);

			var childOfChildWorkflow = BMSTestHelper.CreateProcessHeader(header, buffer, "child Workflow2", TestDateAttribute.Date.AddDays(1));
			var childWorkflow2Task = VisualBoardsTestCase.CreateTask(childOfChildWorkflow, "AAA", 30, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 3, description: "childWorkflowTask2");
			BMSTestHelper.CreateParentChildLink(childWorkflow, childOfChildWorkflow);
			BMSTestCaseWithFactory.CreateIterationLink(childWorkflow1QCBTask, childOfChildWorkflow);

			Factory.Save();

			var componentSectionDTO = BuildSectionDTO(bufferSection) as ComponentSectionDTO;

			AssertEquals(2, componentSectionDTO.Workflows.Count());

			var parentWorkflowDTO = componentSectionDTO.Workflows.First(workflow => workflow.PK == parentWorkflow.PK);
			var childOfChildWorkflowDTO = componentSectionDTO.Workflows.First(workflow => workflow.PK == childOfChildWorkflow.PK);

			AssertNull(parentWorkflowDTO.ParentPK);
			AssertEquals(parentWorkflowDTO.PK, childOfChildWorkflowDTO.ParentPK);
		}

		public void TestParentWorkflow_WhenTwoChildClosed()
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "ORG");
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "System";
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Resource 1");
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			var header = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var board = system.Boards.AddNew();
			var bufferSection = board.Sections.AddNew();
			var sectionPK = bufferSection.PK.ToGuid();

			bufferSection.MS_FC_Component = buffer.PK;
			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, resource1.PK);

			var parentWorkflow = BMSTestHelper.CreateProcessHeader(header, buffer, "Workflow with child", TestDateAttribute.Date.AddDays(1));
			var parentWorkflowQCBTask = VisualBoardsTestCase.CreateTask(parentWorkflow, "AAA", 30, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 1, description: "mainWorkflowTask");

			var firstChildWorkflow = BMSTestHelper.CreateProcessHeader(header, buffer, "child Workflow1", TestDateAttribute.Date.AddDays(1));
			var childWorkflow1QCBTask = VisualBoardsTestCase.CreateTask(firstChildWorkflow, "AAA", 30, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 2, description: "childWorkflowTask1");
			BMSTestCaseWithFactory.CreateIterationLink(parentWorkflowQCBTask, firstChildWorkflow);
			BMSTestHelper.CreateParentChildLink(parentWorkflow, firstChildWorkflow);

			var secondChildWorkflow = BMSTestHelper.CreateProcessHeader(header, buffer, "child Workflow2", TestDateAttribute.Date.AddDays(1));
			var childWorkflow2Task = VisualBoardsTestCase.CreateTask(secondChildWorkflow, "AAA", 30, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 3, description: "childWorkflowTask2");
			BMSTestCaseWithFactory.CreateIterationLink(childWorkflow1QCBTask, secondChildWorkflow);
			BMSTestHelper.CreateParentChildLink(firstChildWorkflow, secondChildWorkflow);

			var thirdChildWorkflow = BMSTestHelper.CreateProcessHeader(header, buffer, "child Workflow3", TestDateAttribute.Date.AddDays(1));
			var childWorkflow3Task = VisualBoardsTestCase.CreateTask(thirdChildWorkflow, "AAA", 30, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 4, description: "childWorkflowTask3");
			BMSTestCaseWithFactory.CreateIterationLink(childWorkflow2Task, thirdChildWorkflow);
			BMSTestHelper.CreateParentChildLink(secondChildWorkflow, thirdChildWorkflow);

			Factory.Save();

			var componentSectionDTO = BuildSectionDTO(bufferSection) as ComponentSectionDTO;

			AssertEquals(2, componentSectionDTO.Workflows.Count());

			var parentWorkflowDTO = componentSectionDTO.Workflows.First(workflow => workflow.PK == parentWorkflow.PK);
			var thirdChildWorkflowDTO = componentSectionDTO.Workflows.First(workflow => workflow.PK == thirdChildWorkflow.PK);

			AssertNull(parentWorkflowDTO.ParentPK);
			AssertEquals(thirdChildWorkflowDTO.ParentPK, parentWorkflowDTO.PK);
		}

		[TestDate(2020, 11, 19, 1, 0, 0)]
		[TestDateIncremental]
		public void TestParentWorkflow_AllWorkflowsOpen()
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "ORG");
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "System";
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Resource 1");
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			var header = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var board = system.Boards.AddNew();
			var bufferSection = board.Sections.AddNew();
			var sectionPK = bufferSection.PK.ToGuid();

			bufferSection.MS_FC_Component = buffer.PK;
			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, resource1.PK);

			var parentWorkflow = BMSTestHelper.CreateProcessHeader(header, buffer, "Workflow with child", TestDateAttribute.Date.AddDays(1));
			var parentWorkflowQCBTask = VisualBoardsTestCase.CreateTask(parentWorkflow, "AAA", 30, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 1, description: "mainWorkflowTask");

			var childWorkflow = BMSTestHelper.CreateProcessHeader(header, buffer, "child Workflow1", TestDateAttribute.Date.AddDays(1));
			var childWorkflow1QCBTask = VisualBoardsTestCase.CreateTask(childWorkflow, "AAA", 30, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 2, description: "childWorkflowTask1");
			BMSTestCaseWithFactory.CreateIterationLink(parentWorkflowQCBTask, childWorkflow);
			BMSTestHelper.CreateParentChildLink(parentWorkflow, childWorkflow);

			var childOfChildWorkflow = BMSTestHelper.CreateProcessHeader(header, buffer, "child Workflow2", TestDateAttribute.Date.AddDays(1));
			var childWorkflow2QCBTask = VisualBoardsTestCase.CreateTask(childOfChildWorkflow, "AAA", 30, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 3, description: "childWorkflowTask2");
			BMSTestCaseWithFactory.CreateIterationLink(childWorkflow1QCBTask, childOfChildWorkflow);
			BMSTestHelper.CreateParentChildLink(childWorkflow, childOfChildWorkflow);

			Factory.Save();

			var componentSectionDTO = BuildSectionDTO(bufferSection) as ComponentSectionDTO;

			AssertEquals(3, componentSectionDTO.Workflows.Count());

			var parentWorkflowDTO = componentSectionDTO.Workflows.First(workflow => workflow.PK == parentWorkflow.PK);
			var childWorkflowDTO = componentSectionDTO.Workflows.First(workflow => workflow.PK == childWorkflow.PK);
			var childOfChildWorkflowDTO = componentSectionDTO.Workflows.First(workflow => workflow.PK == childOfChildWorkflow.PK);

			AssertNull(parentWorkflowDTO.ParentPK);
			AssertEquals(childWorkflowDTO.ParentPK, parentWorkflowDTO.PK);
			AssertEquals(childOfChildWorkflowDTO.ParentPK, childWorkflowDTO.PK);
		}

		public void TestParentWorkflow_ThirdWorkflowClosed()
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "ORG");
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			system.FS_Name = "System";
			var resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Resource 1");
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			var header = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var board = system.Boards.AddNew();
			var bufferSection = board.Sections.AddNew();
			var sectionPK = bufferSection.PK.ToGuid();

			bufferSection.MS_FC_Component = buffer.PK;
			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, resource1.PK);

			var parentWorkflow = BMSTestHelper.CreateProcessHeader(header, buffer, "Workflow with child", TestDateAttribute.Date.AddDays(1));
			var parentWorkflowQCBTask = VisualBoardsTestCase.CreateTask(parentWorkflow, "AAA", 30, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 1, description: "mainWorkflowTask");

			var childWorkflow = BMSTestHelper.CreateProcessHeader(header, buffer, "child Workflow1", TestDateAttribute.Date.AddDays(1));
			var childWorkflow1QCBTask = VisualBoardsTestCase.CreateTask(childWorkflow, "AAA", 30, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 2, description: "childWorkflowTask1");
			BMSTestCaseWithFactory.CreateIterationLink(parentWorkflowQCBTask, childWorkflow);
			BMSTestHelper.CreateParentChildLink(parentWorkflow, childWorkflow);

			var childOfChildWorkflow = BMSTestHelper.CreateProcessHeader(header, buffer, "child Workflow2", TestDateAttribute.Date.AddDays(1));
			var childWorkflow2QCBTask = VisualBoardsTestCase.CreateTask(childOfChildWorkflow, "AAA", 30, taskType: "QCB", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, sequence: 3, description: "childWorkflowTask2");
			BMSTestCaseWithFactory.CreateIterationLink(childWorkflow1QCBTask, childOfChildWorkflow);
			BMSTestHelper.CreateParentChildLink(childWorkflow, childOfChildWorkflow);

			Factory.Save();

			var componentSectionDTO = BuildSectionDTO(bufferSection) as ComponentSectionDTO;

			AssertEquals(2, componentSectionDTO.Workflows.Count());

			var parentWorkflowDTO = componentSectionDTO.Workflows.First(workflow => workflow.PK == parentWorkflow.PK);
			var childWorkflowDTO = componentSectionDTO.Workflows.First(workflow => workflow.PK == childWorkflow.PK);

			AssertNull(parentWorkflowDTO.ParentPK);
			AssertEquals(childWorkflowDTO.ParentPK, parentWorkflowDTO.PK);
		}

		#endregion

		#endregion

		#region Capability

		[TestDate(2018, 3, 17, 18, 30, 0)]
		public void TestGetData_ShouldReturnCorrectComponentSectionCapablityDTOs()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");
			system.FS_Name = "System";

			var buffer = BMSTestHelper.CreateBuffer(system, "buffer", 300);
			var board = system.Boards.AddNew();

			var bufferSection = board.Sections.AddNew();
			bufferSection.MS_FC_Component = buffer.PK;
			var sectionPK = bufferSection.PK.ToGuid();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ST1";
			BMSTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, staff.PK);

			var capability1 = BMSTestHelper.CreateCapability(Factory, description: "Capability1");
			var capability2 = BMSTestHelper.CreateCapability(Factory, description: "Capability2");

			staff.Capabilities.Add(capability2);

			var workItem1 = Factory.New<IWorkItem>();
			var header1 = ProcessJobHeader.GetForParent(workItem1 as IWorkflowProvider, Factory);
			var workflow1 = BMSTestHelper.CreateProcessHeader(header1, buffer, "Workflow 1", TestDateAttribute.Date.AddDays(-10));
			VisualBoardsTestCase.CreateTask(workflow1, staff.GS_Code, 300, taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 1, description: "Task 1", capability: capability1);
			VisualBoardsTestCase.CreateTask(workflow1, staffCode: string.Empty, 300, taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 1, description: "Task 2", capability: capability2);

			var expectedCapabilities = new[] { capability1, capability2 };

			Factory.Save();

			var sectionDTO = BuildSectionDTO(bufferSection);
			var serviceSectionDTO = sectionDTO as ComponentSectionDTO;

			AssertNotNull(serviceSectionDTO);
			AssertEquals(sectionPK, serviceSectionDTO.PK);
			AssertionHelper.AssertCapabilities(expectedCapabilities, serviceSectionDTO.Capabilities.ToArray());
		}

		#endregion

		#region General

		public void TestGetData_ShouldReturnStandardProperties_InsteadOfCustomisationData_WhenWAVEIsDisabledInRegistry()
		{
			BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader = ProcessJobHeaderProvider.GetForParent(org, Factory) as ProcessJobHeader;
			var orgWorkflow = orgHeader.ProcessHeaders[0];
			var staff = BMSTestHelper.CreateStaff(Factory, "STF");
			var orgTask = BMSTestHelper.CreateTask(orgWorkflow);
			orgTask.P9_GS_NKAssignedStaffMember = "STF";

			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			orgWorkflow.MoveToComponent(buffer);

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.CardType = CardTypeList.Codes.Task;
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK);

			var orgCustomisation = BMSTestHelper.CreateControlCustomisation(Factory, CustomisedControlTypeList.Codes.TaskCard);
			var orgCustomisationForSectionLink = section.SectionConfiguration.CustomisedLayoutLinks.AddNew();
			orgCustomisationForSectionLink.FML_FM_ControlCustomisation = orgCustomisation.PK;
			orgCustomisationForSectionLink.FML_JobType = "ORG";

			orgCustomisation.FM_JobType = "ORG";
			orgCustomisation.RenderOnTheWeb = true;
			BMSTestHelper.CreateLine(orgCustomisation, PropertySourceList.Codes.ProcessTask, "P9_EstimateVariationFactor", PropertyTypeList.Codes.Number);

			orgTask.P9_EstimateVariationFactor = 2;

			Factory.Save();

			var componentSectionDTO = BuildSectionDTO(section) as ComponentSectionDTO;
			var orgTaskDTO = componentSectionDTO.Tasks.SingleOrDefault(t => t.PK == orgTask.PK);
			AssertNotNull(orgTaskDTO);

			var standardTaskPropertiesNames = new string[]
			{
				"description",
				"note",
				"status",
				"type",
				"resourceCode",
				"lowEstimatedMinutes",
				"standardEstimatedMinutes",
				"highEstimatedMinutes",
				"estimateVariationFactor",
				"estimatedTimeToCompleteMinutes",
				"isStartable",
			};

			AssertContainsExactElementsInAnyOrder(standardTaskPropertiesNames, orgTaskDTO.Properties.Keys);
		}

		#endregion

		#region Helper functions

		protected abstract ISection BuildSectionDTO(BMBoardSection section);

		#endregion

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();

			Globals.IsWebService = true;
			Globals.IsUserInteractive = false;
			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
