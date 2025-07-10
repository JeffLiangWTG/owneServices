using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class SimpleQueryBoardLoaderTest : BMSTestCaseWithFactory
	{
		public void TestLoadTasks_WhenTaskHasCapabilityAndStaff_ShouldLoadOnlyOnce()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var capability = BMSTestHelper.CreateCapability(Factory);

			staff.Capabilities.Add(capability);

			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, name: "Primary Buffer");
			var section = BMSTestHelper.CreateBoardSection(buffer);
			EnableSimpleQuery(section.Board.PK);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff.PK, displaySequence: 1);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Capability, capability.PK, displaySequence: 2);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflowInComponent = BMSTestHelper.CreateWorkflow(jobHeader, "WF", buffer);

			var task = BMSTestHelper.CreateTask(workflowInComponent, staffCode: staff.GS_Code, capability: capability);

			Factory.Save();

			var tasks = SimpleQueryBoardLoader.LoadTasks(section, section.SectionConfiguration.Channels.ToArray(), section.WorkflowSectionFilter, section.TaskSectionFilter);

			AssertEquals(1, tasks.Length);
			AssertEquals(tasks[0].PK, task.PK);
		}

		public void TestLoadTasks_ShouldLoadStaffAndCapabilityTasks()
		{
			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var capability1 = BMSTestHelper.CreateCapability(Factory);
			var capability2 = BMSTestHelper.CreateCapability(Factory);

			staff1.Capabilities.Add(capability2);

			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, name: "Primary Buffer");
			var section = BMSTestHelper.CreateBoardSection(buffer);
			var additionalBuffer = BMSTestHelper.CreateBuffer(section.Board.System, "additional Buffer");
			EnableSimpleQuery(section.Board.PK);

			BMSTestHelper.CreateAdditionalComponent(section, additionalBuffer);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff1.PK, displaySequence: 1);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, staff2.PK, displaySequence: 2);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Capability, capability1.PK, displaySequence: 3);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflowInComponent1 = BMSTestHelper.CreateWorkflow(jobHeader, "WF1", buffer);
			var workflowInComponent2 = BMSTestHelper.CreateWorkflow(jobHeader, "WF2", additionalBuffer);

			var staffTask1 = BMSTestHelper.CreateTask(workflowInComponent1, staff1.GS_Code);
			var staffTask2 = BMSTestHelper.CreateTask(workflowInComponent2, staff2.GS_Code);
			var capabilityTask1 = BMSTestHelper.CreateTask(workflowInComponent1, capability: capability1);
			var capabilityTask2 = BMSTestHelper.CreateTask(workflowInComponent2, capability: capability2);
			var taskNotOnBoard1 = BMSTestHelper.CreateTask(workflowInComponent1);
			var taskNotOnBoard2 = BMSTestHelper.CreateTask(workflowInComponent2);

			Factory.Save();

			var tasks = SimpleQueryBoardLoader.LoadTasks(section, section.SectionConfiguration.Channels.ToArray(), section.WorkflowSectionFilter, section.TaskSectionFilter);

			AssertNotNull(tasks);
			AssertContainsExactElementsInAnyOrder(new[] { staffTask1.PK, staffTask2.PK, capabilityTask1.PK, capabilityTask2.PK }, tasks.Select(t => t.PK).ToArray());
		}

		public void TestLoadTasks_ShouldConsiderWorkflowAndTaskFilters()
		{
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var capability = BMSTestHelper.CreateCapability(Factory);
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, name: "Primary Buffer");
			var section = BMSTestHelper.CreateBoardSection(buffer);
			EnableSimpleQuery(section.Board.PK);

			AssertEquals(false, section.AreWorkflowFiltersSpecified);

			FilterStripsTestHelper.AddStartsWithFilter(section.WorkflowFilter, "Completion Statement", "Im a workflow");
			FilterStripsTestHelper.AddStartsWithFilter(section.TaskFilter, "Description", "Im a task");

			Assert(section.AreWorkflowFiltersSpecified);

			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, resource.PK, displaySequence: 1);
			BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Capability, capability.PK, displaySequence: 2);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);

			var workflowOnBoard = BMSTestHelper.CreateWorkflow(jobHeader, "Im a workflow", buffer);
			var taskOnBoard1 = BMSTestHelper.CreateTask(workflowOnBoard, resource.GS_Code, description: "Im a task");
			var taskOnBoard2 = BMSTestHelper.CreateTask(workflowOnBoard, capability: capability, description: "Im a task");
			var taskNotOnBoard1 = BMSTestHelper.CreateTask(workflowOnBoard, resource.GS_Code, description: "Im a not on board");

			var workflowNotOnBoard = BMSTestHelper.CreateWorkflow(jobHeader, "Im not in board", buffer);
			var taskNotOnBoard2 = BMSTestHelper.CreateTask(workflowNotOnBoard, capability: capability, description: "Im a task, but Im not on board");

			Factory.Save();

			var tasks = SimpleQueryBoardLoader.LoadTasks(section, section.SectionConfiguration.Channels.ToArray(), section.WorkflowSectionFilter, section.TaskSectionFilter);

			AssertNotNull(tasks);
			AssertContainsExactElementsInAnyOrder(new[] { taskOnBoard1.PK, taskOnBoard2.PK }, tasks.Select(t => t.PK).ToArray());
		}

		void EnableSimpleQuery(ZGuid boardPK)
		{
			BMSRegistry.Instance.EnablePaveExperimentalFeatures.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var provider = new ExperimentalSettingsProvider(boardPK, Factory);
			provider.ExperimentalSettings.Add(new ExperimentalSetting { Key = ExperimentalSettingsProvider.SimpleBoardQueryExperimentalSettingsKey, Value = true.ToString() });
			provider.SaveSettings();
		}
	}
}
