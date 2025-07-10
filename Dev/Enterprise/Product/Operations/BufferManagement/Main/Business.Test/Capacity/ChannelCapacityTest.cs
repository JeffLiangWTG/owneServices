using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	public abstract class ChannelCapacityTest : BMSTestCaseWithFactory
	{
		#region Constrained Mode

		[RequiresSTA]
		[TestDate(2013, 4, 16)]
		public void TestCCRHeadings_Estimates()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var section = config.Section;

			foreach (var ccrCode in new[] { "CR2", "CR3" })
			{
				var ccr = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, ccrCode, ccrCode);
				config.Staffs.Add(ccr);
				ccr.DesignateAsCCR(config.Buffer);
				BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, ccr.PK, overrideChannels: true);

				var workflow = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: string.Empty, currentComponent: config.Buffer, releaseDateTime: ZDateTime.Now, staffCode: ccr.GS_Code, lowEstMinutes: 15);
				config.Workflows.Add(workflow);
			}

			Factory.Save();

			foreach (var direction in new FlowDirectionList())
			{
				config.Section.SectionConfiguration.FlowDirection = direction.ToString();
				var viewModel = config.ResetViewModel();

				var secondaryAxis = config.Section.SectionConfiguration.FlowsInSameDirectionAsAxis() ? 1 : 12;
				var ccrHeadingCell = viewModel.ComponentGrid.Cells.FirstOrDefault(c => c.PrimaryAxis == 8 && c.SecondaryAxis == secondaryAxis);
				var ccrHeadingCapacity = new ChannelCapacity(null, ccrHeadingCell, section, viewModel, viewModel.ComponentGrid.CardAllocationMap);

				var message = string.Format("CCR-heading must only show estimates from its related CCR-channels i.e. CR2 & CR3 and its collapsed-cells (direction: {0})", direction);
				CapacityCalculatorTestHelper.AssertCapacity(message, capacity: ccrHeadingCapacity, expectedCaption: "CCRs 0 % to 41.7 %", expectedFullCapacity: null, expectedAvailableCapacity: null, expectedAllocated: "buffer Total: 0.5 hours", calculatedTime: null);
			}
		}

		[RequiresSTA]
		[TestDate(2013, 4, 16)]
		public void TestCCRHeadings_Estimates_WithAdditionalComponent()
		{
			var config = TestConfigsHelper.CreateComplexConstrainedSchematicTestConfig(Factory, shouldUseExistingSystem: true);
			var section = config.Section;

			foreach (var ccrCode in new[] { "CR2", "CR3" })
			{
				var ccr = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, ccrCode, ccrCode);
				config.Staffs.Add(ccr);
				ccr.DesignateAsCCR(config.Buffer);
				BMSTestHelper.CreatePrimaryChannelForSection(section, ChannelTypeList.Codes.Resource, ccr.PK, overrideChannels: true);

				var workflow = BMSTestHelper.CreateWorkflowAndTask(Factory, completionStatement: string.Empty, currentComponent: config.Buffer, releaseDateTime: ZDateTime.Now, staffCode: ccr.GS_Code, lowEstMinutes: 15);
				config.Workflows.Add(workflow);
			}

			var buffer2 = BMSTestHelper.CreateBuffer(section.Board.System, "buffer2");
			BMSTestHelper.CreateAdditionalComponent(section, buffer2);

			Factory.Save();

			foreach (var direction in new FlowDirectionList())
			{
				config.Section.SectionConfiguration.FlowDirection = direction.ToString();
				var viewModel = config.ResetViewModel();

				var secondaryAxis = config.Section.SectionConfiguration.FlowsInSameDirectionAsAxis() ? 1 : 12;
				var ccrHeadingCell = viewModel.ComponentGrid.Cells.FirstOrDefault(c => c.PrimaryAxis == 8 && c.SecondaryAxis == secondaryAxis);
				var ccrHeadingCapacity = new ChannelCapacity(null, ccrHeadingCell, section, viewModel, viewModel.ComponentGrid.CardAllocationMap);

				var message = string.Format("CCR-heading must only show estimates from its related CCR-channels i.e. CR2 & CR3 and its collapsed-cells (direction: {0})", direction.ToString());
				AssertEquals(message, "buffer Total: 0.5 hours\r\n\r\nbuffer2 Total: 0 hours\r\n", ccrHeadingCapacity.Message);
			}
		}

		#endregion

		#region Release Scheduler

		[RequiresSTA]
		[TestDate(2013, 4, 16)]
		public void TestReleaseSchedulerCapacity()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, GlbDepartment.CurrentDepartment.PK);

			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DAN", "Daniel");
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var buffer = BMSTestHelper.CreateBuffer(system);

			BMSTestHelper.LinkComponents(bucket1, buffer, isReleaseGate: true);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = bucket1.PK;
			var task1 = BMSTestHelper.CreateTask(workflow1, staff1.GS_Code, 60, estVariationFactor: 1);

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = buffer.PK;
			var task2 = BMSTestHelper.CreateTask(workflow2, staff1.GS_Code, 120, estVariationFactor: 1);

			Factory.Save();

			var board = system.Boards.AddNew();
			var section = CreateReleaseSchedulerBoardSection(buffer, null, board);

			Factory.Save();
			var viewModel = BMSTestHelper.CreateViewModel(section);
			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task1, task2 });

			var nonConstrainedCapacity = new ChannelCapacity(viewModel.PrimaryChannels.First(), new CellContent(0, 1, CellContentType.ChannelHeading, section.SectionConfiguration), section, viewModel, viewModel.ComponentGrid.CardAllocationMap);
			CapacityCalculatorTestHelper.AssertCapacity(nonConstrainedCapacity, "Non-Constrained", null, null, "Total Assigned: 3 hours", null);

			var releaseSchedulerCellContent = new CellContent(2, 0, CellContentType.ChannelHeading, section.SectionConfiguration);
			releaseSchedulerCellContent.SecondaryChannel = viewModel.SecondaryAxisChannels.First();
			var releaseSchedulerCapacity = new ChannelCapacity(releaseSchedulerCellContent.SecondaryChannel, releaseSchedulerCellContent, section, viewModel, viewModel.ComponentGrid.CardAllocationMap);
			CapacityCalculatorTestHelper.AssertCapacity(releaseSchedulerCapacity, "Released", null, null, "Total Assigned: 1 hours", null);

			var unreleasedSchedulerCellContent = new CellContent(1, 0, CellContentType.ChannelHeading, section.SectionConfiguration);
			unreleasedSchedulerCellContent.SecondaryChannel = viewModel.SecondaryAxisChannels.ElementAt(1);
			var unreleasedSchedulerCapacity = new ChannelCapacity(unreleasedSchedulerCellContent.SecondaryChannel, unreleasedSchedulerCellContent, section, viewModel, viewModel.ComponentGrid.CardAllocationMap);
			CapacityCalculatorTestHelper.AssertCapacity(unreleasedSchedulerCapacity, "Un-Released", null, null, "Total Assigned: 2 hours", null);
		}

		#endregion

		#region Multi-Component Capacity

		[RequiresSTA]
		[TestDate(2013, 4, 16)]
		public void TestMultiBucketCapacity()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, GlbDepartment.CurrentDepartment.PK);

			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DAN", "Daniel");
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = bucket1.PK;
			var task1 = BMSTestHelper.CreateTask(workflow1, staff1.GS_Code, 60, estVariationFactor: 1);

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = bucket2.PK;
			var task2 = BMSTestHelper.CreateTask(workflow2, staff1.GS_Code, 120, estVariationFactor: 1);

			Factory.Save();

			var board = system.Boards.AddNew();
			var section = BMSTestHelper.CreateBoardSection(bucket1, board);
			BMSTestHelper.CreateAdditionalComponent(section, bucket2);
			var viewModel = BMSTestHelper.CreateViewModel(section);

			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task1, task2 });
			var staff1Capacity = new ChannelCapacity(viewModel.CreateChannelForTest(staff1), new CellContent(0, 0, CellContentType.ChannelHeading), section, viewModel, viewModel.ComponentGrid.CardAllocationMap);

			CapacityCalculatorTestHelper.AssertCapacity(staff1Capacity, "Daniel", null, null, "Total Assigned in bucket1: 1 hours\r\n\r\nTotal Assigned in bucket2: 2 hours", null);
		}

		#endregion

		#region Resource Capacity

		protected void TestCapacityCore(string cultureName = "")
		{
			var currentCulture = Thread.CurrentThread.CurrentCulture;
			if (!string.IsNullOrEmpty(cultureName))
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);
			}

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, GlbDepartment.CurrentDepartment.PK);

			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "PER", "Peter Pan");
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DAN", "Daniel");

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.AddRange(staff1, staff2);

			var buffer = BMSTestHelper.CreateBuffer(system);
			var releaseGroup = BMSTestHelper.CreateReleaseGroup(system, group);

			CreateZoneMultiplier(buffer, zone0Multiplier: 20, zone1Multiplier: 10, zone2Multiplier: 5, zone3Multiplier: 2);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			BMSTestHelper.CreateTask(workflow1, staff1.GS_Code, 20); // 30 mins std est
			BMSTestHelper.CreateTask(workflow1, staff2.GS_Code, 3 * 60); // 4.5 hours std est

			var section = CreateBoardSection(buffer);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);

			var staff1Capacity = new ChannelCapacity(viewModel.CreateChannelForTest(staff1), new CellContent(0, 0, CellContentType.ChannelHeading), section, viewModel, viewModel.ComponentGrid.CardAllocationMap);
			var staff2Capacity = new ChannelCapacity(viewModel.CreateChannelForTest(staff2), new CellContent(0, 0, CellContentType.ChannelHeading), section, viewModel, viewModel.ComponentGrid.CardAllocationMap);

			CapacityCalculatorTestHelper.AssertCapacity(staff1Capacity, "Peter Pan", "Total Capacity: 48 hours", "Available Capacity: 47 hours", ExpectedStaff1AllocatedMessageForTestCapacityCore, "Calculated at: 16-Apr-2013 10:00:00");
			CapacityCalculatorTestHelper.AssertCapacity(staff2Capacity, "Daniel", "Total Capacity: 48 hours", "Available Capacity: 39 hours", ExpectedStaff2AllocatedMessageForTestCapacityCore, "Calculated at: 16-Apr-2013 10:00:00");

			if (!string.IsNullOrEmpty(cultureName))
			{
				Thread.CurrentThread.CurrentCulture = currentCulture;
			}
		}

		protected abstract string ExpectedStaff1AllocatedMessageForTestCapacityCore { get; }

		protected abstract string ExpectedStaff2AllocatedMessageForTestCapacityCore { get; }

		#endregion

		#region Zone Breakdown

		[RequiresSTA]
		[TestDate(2013, 4, 18)]
		public void TestZoneChannelCapacityIncludesInnerChannelsRightFlow()
		{
			TestZoneChannelCapacity(FlowDirectionList.Codes.Right, Tuple.Create(BMConstants.AgeHeadingPosition, 0), Tuple.Create(BMConstants.AgeHeadingPosition, 1));
		}

		[RequiresSTA]
		[TestDate(2013, 4, 18)]
		public void TestZoneChannelCapacityIncludesInnerChannelsDownFlow()
		{
			TestZoneChannelCapacity(FlowDirectionList.Codes.Down, Tuple.Create(0, BMConstants.AgeHeadingPosition), Tuple.Create(1, BMConstants.AgeHeadingPosition));
		}

		void TestZoneChannelCapacity(ZString flowDirection, Tuple<int, int> cell1Coordinates, Tuple<int, int> cell2Coordinates)
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, GlbDepartment.CurrentDepartment.PK);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staff1.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staff2.GS_GE_HomeDepartment = GlbDepartment.CurrentDepartment.PK;

			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = buffer.PK;
			workflow1.FH_ReleaseDateTime = ZDateTime.Today.AddDays(5);
			var task1 = job.WorkflowItems.AddNew();
			task1.P9_FH_ProcessHeader = workflow1.PK;
			task1.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			task1.P9_EstDuration = new ZDateTime(2013, 1, 1, 0, 20, 0); // 30 mins std est

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = buffer.PK;
			workflow2.FH_ReleaseDateTime = ZDateTime.Today.AddDays(-1);
			var task2 = job.WorkflowItems.AddNew();
			task2.P9_FH_ProcessHeader = workflow2.PK;
			task2.P9_GS_NKAssignedStaffMember = staff1.GS_Code;
			task2.P9_EstDuration = new ZDateTime(2013, 1, 1, 0, 20, 0); // 30 mins std est

			Factory.Save();

			var board = system.Boards.AddNew();
			var section = board.Sections.AddNew();
			section.MS_FC_Component = buffer.PK;
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.FlowDirection = flowDirection;
			var viewModel = BMSTestHelper.CreateViewModel(section);

			Factory.Save();

			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, new[] { task1, task2 });

			var cell1 = viewModel.ComponentGrid[cell1Coordinates.Item1, cell1Coordinates.Item2];
			var cell2 = viewModel.ComponentGrid[cell2Coordinates.Item1, cell2Coordinates.Item2];

			var channel1Capacity = new ChannelCapacity(null, cell1, section, viewModel, viewModel.ComponentGrid.CardAllocationMap);
			CapacityCalculatorTestHelper.AssertCapacity(channel1Capacity, cell1.Label, "Buffer Total: 0.5 hours", "", "", "");

			var channel2Capacity = new ChannelCapacity(null, cell2, section, viewModel, viewModel.ComponentGrid.CardAllocationMap);
			CapacityCalculatorTestHelper.AssertCapacity(channel2Capacity, cell2.Label, "Buffer Total: 0.5 hours", "", "", "");

			var componentGrid = section.SectionConfiguration.OrientationValue == BMBoardSectionOrientation.Vertical ? viewModel.ComponentGrid[0, BMConstants.ZoneHeadingPosition] : viewModel.ComponentGrid[BMConstants.ZoneHeadingPosition, 0];

			var totalCapacity = new ChannelCapacity(null, componentGrid, section, viewModel, viewModel.ComponentGrid.CardAllocationMap);
			CapacityCalculatorTestHelper.AssertCapacity(totalCapacity, "Zone 3", "Buffer Total: 1 hours", "", "", "");
		}

		#endregion

		#region Non-Resource Capacity

		[RequiresSTA]
		public void TestNonResourceChannelCapacity_ShouldShowOnlyTotalCapacity()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Description = "Magic";

			var buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			var buffer2 = BMSTestHelper.CreateBuffer(system, "Another Buffer");
			CreateZoneMultiplier(buffer2, zone0Multiplier: 20, zone1Multiplier: 10, zone2Multiplier: 5, zone3Multiplier: 2);

			var section = BMSTestHelper.CreateBoardSection(buffer);
			section.SectionConfiguration.AdditionalComponents.AddNew().BSA_FC_Component = buffer2.PK;
			var viewModel = BMSTestHelper.CreateViewModel(section);

			viewModel.ComponentGrid.AllocateTasks_ForTest(section, viewModel, Array.Empty<ProcessTask>());
			var channel = viewModel.CreateChannelForTest(capability);
			channel.SeedCacheWithChannelDescriptorForTest(Factory);

			var capacity = new ChannelCapacity(channel, new CellContent(0, 0, CellContentType.ChannelHeading), section, viewModel, viewModel.ComponentGrid.CardAllocationMap);
			CapacityCalculatorTestHelper.AssertCapacity(capacity, "Magic", "Buffer Total: 0 hours\r\n\r\nAnother Buffer Total: 0 hours", "", "", "");
		}

		#endregion

		#region Additional Components

		[TestDate(2015, 7, 14)]
		public void TestComponentCapacity_InactiveAdditionalComponent_ShouldNotBeIncluded()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartmentPK);

			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "JL", "JohnLock :'(");

			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory, shouldUseExistingSystem: true);
			var additionalBuffer = BMSTestHelper.CreateBuffer(config.System, "Second Breakfast");
			var section = config.BufferSection;

			BMSTestHelper.CreateAdditionalComponent(section, additionalBuffer);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channelCapacity = new ChannelCapacity(viewModel.CreateChannelForTest(resource), new CellContent(0, 0, CellContentType.ChannelHeading), section, viewModel, viewModel.ComponentGrid.CardAllocationMap);

			AssertComponentCapacity_InactiveAdditionalComponent_ShouldNotBeIncluded_WhenComponentActive(channelCapacity);

			additionalBuffer.FC_IsActive = false;
			Factory.Save();

			viewModel = BMSTestHelper.CreateViewModel(section);
			channelCapacity = new ChannelCapacity(viewModel.CreateChannelForTest(resource), new CellContent(0, 0, CellContentType.ChannelHeading), section, viewModel, viewModel.ComponentGrid.CardAllocationMap);

			AssertComponentCapacity_InactiveAdditionalComponent_ShouldNotBeIncluded_WhenComponentInActive(channelCapacity);
		}

		protected abstract void AssertComponentCapacity_InactiveAdditionalComponent_ShouldNotBeIncluded_WhenComponentActive(ChannelCapacity channelCapacity);

		protected abstract void AssertComponentCapacity_InactiveAdditionalComponent_ShouldNotBeIncluded_WhenComponentInActive(ChannelCapacity channelCapacity);

		[TestDate(2013, 4, 16)]
		public void TestMultiBufferCapacity()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, GlbDepartment.CurrentDepartment.PK);

			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DAN", "Daniel");
			var buffer1 = BMSTestHelper.CreateBuffer(system, "buffer1");
			var buffer2 = BMSTestHelper.CreateBuffer(system, "buffer2");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			workflow1.FH_FC_CurrentComponent = buffer1.PK;
			BMSTestHelper.CreateTask(workflow1, staff1.GS_Code, 60, estVariationFactor: 1);

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_FC_CurrentComponent = buffer2.PK;
			BMSTestHelper.CreateTask(workflow2, staff1.GS_Code, 120, estVariationFactor: 1);

			Factory.Save();

			var board = system.Boards.AddNew();
			var section = BMSTestHelper.CreateBoardSection(buffer1, board);
			BMSTestHelper.CreateAdditionalComponent(section, buffer2);
			var viewModel = BMSTestHelper.CreateViewModel(section);
			var channel = viewModel.CreateChannelForTest(staff1);

			viewModel.PopulateRoadRunnerDetails(Factory, new PropertyCache(), viewModel.ComponentGrid.CardAllocationMap, new[] { channel }, viewModel.ReleaseGroupPK);

			var staff1Capacity = new ChannelCapacity(channel, new CellContent(0, 0, CellContentType.ChannelHeading), section, viewModel, viewModel.ComponentGrid.CardAllocationMap);

			const string totalCapacity =
@"Total Capacity:
    buffer1: 48 hours
    buffer2: 48 hours";
			const string availableCapacity =
@"Available Capacity:
    buffer1: 47 hours
    buffer2: 46 hours";

			const string calculatedTime =
@"Calculated at: 16-Apr-2013 10:00:00 (buffer1)
Calculated at: 16-Apr-2013 10:00:00 (buffer2)";

			CapacityCalculatorTestHelper.AssertCapacity(staff1Capacity, "Daniel", totalCapacity, availableCapacity, ExpectedAllocatedMessageForTestMultiBufferCapacity2Core, calculatedTime);
		}

		protected abstract string ExpectedAllocatedMessageForTestMultiBufferCapacity2Core { get; }

		#endregion

		#region Turned off in registry

		public void TestHeader_WhenDisableCapacityCalculationsEnabled_ShouldReturnEmptyCapacity_WithExplanatoryMessage()
		{
			BMSRegistry.Instance.DisableCapacityCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, GlbDepartment.CurrentDepartment.PK);

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "PER", "Peter Pan");
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staff);

			var buffer = BMSTestHelper.CreateBuffer(system);
			BMSTestHelper.CreateReleaseGroup(system, group);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			BMSTestHelper.CreateTask(workflow1, staff.GS_Code, 20);
			var section = CreateBoardSection(buffer);

			Factory.Save();

			var viewModel = BMSTestHelper.CreateViewModel(section);
			var capacity = new ChannelCapacity(viewModel.CreateChannelForTest(staff), new CellContent(0, 0, CellContentType.ChannelHeading), section, viewModel, viewModel.ComponentGrid.CardAllocationMap);

			AssertMultilineASCIIEquals("Capacity unavailable because the [Workflow Manager -> Buffer Management -> Release Gate -> Disable Capacity Calculations] registry item is enabled.", capacity.Message);
			AssertEquals(0m, capacity.UtilisedCapacityPercent);
		}

		#endregion

		#region Implementation

		protected virtual bool IsNewCapacityCalculatorEnabled => false;

		protected BMSystem system;

		protected override void SetUp()
		{
			base.SetUp();
			system = BMSTestHelper.CreateSystem(Factory, "DUM");
		}

		#endregion
	}
}
