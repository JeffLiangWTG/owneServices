using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestDate(2014, 9, 3)] // Wednesday
	class CapacityCalculatorNetworkTest : NetworkTestCase
	{
		[GuiTest]
		public void TestGetUtilisedCapacity_ShouldUseZoneMultipliers_ConsideringCCPMBufferZone()
		{
			BMSRegistry.Instance.CacheCalculatedCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(-6);
			var workflow = CreateWorkflow(jobHeader, "workflow", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var task = CreateTask(workflow, resource.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 5);

			var diagram = CreateDiagram(jobHeader);
			var shape = CreateShape(workflow, diagram, "shape");

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();

			Factory.Save();

			AssertEquals(FullCapacity, CapacityCalculator.GetFullCapacity_ForTest(resource, config.Buffer));
			AssertEquals("Should be no multiplier on capacity as it isn't part of an approved diagram and is in zone 3 of the operational buffer", FullCapacity - task.StandardEstimateHours, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);
			AssertEquals(3, workflow.BufferZone);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-12);
			Factory.Save();

			AssertEquals("Should be a zone 1 multiplier from the operational buffer", FullCapacity - (task.StandardEstimateHours * 2), CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);
			AssertEquals(1, workflow.BufferZone);

			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow;
			Factory.Save();

			AssertEquals("Should be no multiplier on capacity as it isn't part of an approved diagram and is in zone 3 of the operational buffer", FullCapacity - task.StandardEstimateHours, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);
			AssertEquals(3, workflow.BufferZone);

			networkViewModel.ToggleApproval();
			Factory.Save();

			RunCCPMAndNCNTagRules(Factory);
			RunBufferPenetrationUpdater();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedWorkflow = newFactory.Load<ProcessHeader>(workflow.PK);
			var loadedDiagramShape = newFactory.Load<BMNCNShape>(diagram.PK);

			AssertEquals("Should be a zone 1 multiplier from the project buffer", FullCapacity - (task.StandardEstimateHours * 2), CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);
			AssertEquals(1, loadedWorkflow.BufferZone);

			var newNetwork = CreateNetwork(loadedDiagramShape);
			loadedDiagramShape.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddDays(-12);
			newNetwork.EditEntity(newNetwork.DiagramEntity); // To cause schedules to update
			newFactory.Save();

			RunBufferPenetrationUpdater();

			AssertEquals("Should be a zone 0 multiplier from the project buffer", FullCapacity - (task.StandardEstimateHours * 3), CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);
			AssertEquals(0, loadedWorkflow.BufferZone);
		}

		public void TestGetUtilisedCapacity_ShouldUseZoneMultipliers_ConsideringCCPMBufferZone_ForJobApprovedShape()
		{
			BMSRegistry.Instance.CacheCalculatedCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var jobHeader1 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			jobHeader1.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(-6);

			var jobHeader2 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader2, "workflow", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var task = CreateTask(workflow, resource.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 5);

			Factory.Save();

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram, "subDiagram");

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();

			Factory.Save();

			AssertEquals(FullCapacity, CapacityCalculator.GetFullCapacity_ForTest(resource, config.Buffer));
			AssertEquals("Should be no multiplier on capacity as it isn't part of an approved diagram and is in zone 3 of the operational buffer", FullCapacity - task.StandardEstimateHours, CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);
			AssertEquals(3, workflow.BufferZone);

			networkViewModel.ToggleApproval();
			Factory.Save();

			RunCCPMAndNCNTagRules(Factory);
			RunBufferPenetrationUpdater();
			Factory.Save();

			var loadedWorkflow = new BusinessObjectFactory().Load<ProcessHeader>(workflow.PK);
			var loadedTask = loadedWorkflow.Factory.Load<ProcessTask>(task.PK);

			AssertEquals("Should be a zone 1 multiplier from the project buffer", FullCapacity - (loadedTask.StandardEstimateHours * 2), CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer).AvailableCapacity);
			AssertEquals(1, loadedWorkflow.BufferZone);
		}

		public void TestGetAvailableCapacity_ShouldUseDetailsOnJobHeaderApprovedShapeForZone_ButReserveOnlyCapacityInBuffer()
		{
			BMSRegistry.Instance.CacheCalculatedCapacity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var otherResource = CreateStaffInCurrentBranchDept("PLG", "Darth Plagueis the Wise");
			var otherBuffer = CreateBuffer(config.System, "Other Buffer");
			LinkComponents(config.Bucket, otherBuffer);

			var section = CreateBoardSection(config.Buffer);
			section.SectionConfiguration.AdditionalComponents.AddNew().BSA_FC_Component = otherBuffer.PK;
			section.SectionConfiguration.CellsPerSubsection = 13;
			section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			var viewModel = VisualBoardsTestHelper.CreateViewModel(section);

			var jobHeader1 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			jobHeader1.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(-1);

			var jobHeader2 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader2, "workflow1", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = CreateWorkflow(jobHeader2, "workflow2", otherBuffer, releaseGroupPK: config.ReleaseGroup.PK);

			var task1_1 = CreateTask(workflow1, resource.GS_Code, (int)(60 * BMConstants.WorkingHoursPerDay * 0.2), estVariationFactor: 1, description: "T1");
			var task1_2 = CreateTask(workflow1, resource.GS_Code, (int)(60 * BMConstants.WorkingHoursPerDay * 0.2), estVariationFactor: 1, description: "T2");
			var task1_3 = CreateTask(workflow1, otherResource.GS_Code, (int)(60 * BMConstants.WorkingHoursPerDay * 0.6), estVariationFactor: 1, description: "T3");

			var task2_1 = CreateTask(workflow2, resource.GS_Code, 60 * BMConstants.WorkingHoursPerDay, estVariationFactor: 1, description: "T4");
			var task2_2 = CreateTask(workflow2, resource.GS_Code, 60 * BMConstants.WorkingHoursPerDay, estVariationFactor: 1, description: "T5");
			var task2_3 = CreateTask(workflow2, otherResource.GS_Code, 60 * BMConstants.WorkingHoursPerDay, estVariationFactor: 1, description: "T6");

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram, "subDiagram");

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			subDiagram.AsEntity(network).Width = 400;
			network.Refresh(RefreshType.RedrawDiagram);

			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			var buffer = network.Shapes.OfType<BMNCNBufferShape>().Single();
			AssertEquals(200.0, buffer.AsEntity(network).Width);

			Factory.Save();

			RunCCPMAndNCNTagRules(Factory);
			Factory.Save();

			var allTasks = new[]
			{
				task1_1, task1_2, task1_3, task2_1, task2_2, task2_3
			};

			const decimal stdEstHoursResource1Buffer1 = 3.2m;
			const decimal stdEstHoursResource2Buffer1 = 4.8m;
			const decimal stdEstHoursResource1Buffer2 = 16m;
			const decimal stdEstHoursResource2Buffer2 = 8m;

			// Preconditions of a zone 2 approved project plan item
			RunBufferPenetrationUpdaterAndAssertTasksInCell(section, viewModel, allTasks, Tuple.Create(6, 2, allTasks));

			var capacityBreakdown1 = CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer);
			var capacityBreakdown2 = CapacityCalculator.GetUtilisedCapacityBreakdown(otherResource, config.Buffer);
			var capacityBreakdown3 = CapacityCalculator.GetUtilisedCapacityBreakdown(resource, otherBuffer);
			var capacityBreakdown4 = CapacityCalculator.GetUtilisedCapacityBreakdown(otherResource, otherBuffer);

			AssertEquals(FullCapacity, capacityBreakdown1.FullCapacity);
			AssertEquals(stdEstHoursResource1Buffer1, capacityBreakdown1.UtilisedCapacity);
			AssertEquals("Project item is in zone 2, so all non-visualised workflows should be in zone 2", stdEstHoursResource1Buffer1, capacityBreakdown1.GetZoneAllocatedCapacity(2));
			AssertEquals("Zone 2 has no multiplier", stdEstHoursResource1Buffer1, capacityBreakdown1.GetZoneReservedCapacity(2));

			AssertEquals(FullCapacity, capacityBreakdown2.FullCapacity);
			AssertEquals(stdEstHoursResource2Buffer1, capacityBreakdown2.UtilisedCapacity);
			AssertEquals("Project item is in zone 2, so all non-visualised workflows should be in zone 2", stdEstHoursResource2Buffer1, capacityBreakdown2.GetZoneAllocatedCapacity(2));
			AssertEquals("Zone 2 has no multiplier", stdEstHoursResource2Buffer1, capacityBreakdown2.GetZoneReservedCapacity(2));

			AssertEquals(FullCapacity, capacityBreakdown3.FullCapacity);
			AssertEquals(stdEstHoursResource1Buffer2, capacityBreakdown3.UtilisedCapacity);
			AssertEquals("Project item is in zone 2, so all non-visualised workflows should be in zone 2", stdEstHoursResource1Buffer2, capacityBreakdown3.GetZoneAllocatedCapacity(2));
			AssertEquals("Zone 2 has no multiplier", stdEstHoursResource1Buffer2, capacityBreakdown3.GetZoneReservedCapacity(2));

			AssertEquals(FullCapacity, capacityBreakdown4.FullCapacity);
			AssertEquals(stdEstHoursResource2Buffer2, capacityBreakdown4.UtilisedCapacity);
			AssertEquals("Project item is in zone 2, so all non-visualised workflows should be in zone 2", stdEstHoursResource2Buffer2, capacityBreakdown4.GetZoneAllocatedCapacity(2));
			AssertEquals("Zone 2 has no multiplier", stdEstHoursResource2Buffer2, capacityBreakdown4.GetZoneReservedCapacity(2));

			// Advance the date to increase the gap between now and StartableTime
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(12);
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			section = newFactory.Load<BMBoardSection>(section.PK);
			viewModel = VisualBoardsTestHelper.CreateViewModel(section);

			RunBufferPenetrationUpdaterAndAssertTasksInCell(section, viewModel, allTasks, Tuple.Create(1, 2, allTasks));

			capacityBreakdown1 = CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer);
			capacityBreakdown2 = CapacityCalculator.GetUtilisedCapacityBreakdown(otherResource, config.Buffer);
			capacityBreakdown3 = CapacityCalculator.GetUtilisedCapacityBreakdown(resource, otherBuffer);
			capacityBreakdown4 = CapacityCalculator.GetUtilisedCapacityBreakdown(otherResource, otherBuffer);

			AssertEquals("Project is now in zone 1, which has a capacity multiplier in this buffer", stdEstHoursResource1Buffer1 * 2, capacityBreakdown1.UtilisedCapacity);
			AssertEquals(0m, capacityBreakdown1.GetZoneAllocatedCapacity(2));
			AssertEquals(0m, capacityBreakdown1.GetZoneReservedCapacity(2));
			AssertEquals("Project is now in zone 1, which has a capacity multiplier in this buffer", stdEstHoursResource1Buffer1, capacityBreakdown1.GetZoneAllocatedCapacity(1));
			AssertEquals("Project is now in zone 1, which has a capacity multiplier in this buffer", stdEstHoursResource1Buffer1 * 2, capacityBreakdown1.GetZoneReservedCapacity(1));

			AssertEquals("Project is now in zone 1, which has a capacity multiplier in this buffer", stdEstHoursResource2Buffer1 * 2, capacityBreakdown2.UtilisedCapacity);
			AssertEquals(0m, capacityBreakdown2.GetZoneAllocatedCapacity(2));
			AssertEquals(0m, capacityBreakdown2.GetZoneReservedCapacity(2));
			AssertEquals("Project is now in zone 1, which has a capacity multiplier in this buffer", stdEstHoursResource2Buffer1, capacityBreakdown2.GetZoneAllocatedCapacity(1));
			AssertEquals("Project is now in zone 1, which has a capacity multiplier in this buffer", stdEstHoursResource2Buffer1 * 2, capacityBreakdown2.GetZoneReservedCapacity(1));

			AssertEquals("Project is now in zone 1, which doesn't have a capacity multiplier in this buffer", stdEstHoursResource1Buffer2, capacityBreakdown3.UtilisedCapacity);
			AssertEquals(0m, capacityBreakdown3.GetZoneAllocatedCapacity(2));
			AssertEquals(0m, capacityBreakdown3.GetZoneReservedCapacity(2));
			AssertEquals("Project is now in zone 1, which doesn't have a capacity multiplier in this buffer", stdEstHoursResource1Buffer2, capacityBreakdown3.GetZoneAllocatedCapacity(1));
			AssertEquals("Project is now in zone 1, which doesn't have a capacity multiplier in this buffer", stdEstHoursResource1Buffer2, capacityBreakdown3.GetZoneReservedCapacity(1));

			AssertEquals("Project is now in zone 1, which doesn't have a capacity multiplier in this buffer", stdEstHoursResource2Buffer2, capacityBreakdown4.UtilisedCapacity);
			AssertEquals(0m, capacityBreakdown4.GetZoneAllocatedCapacity(2));
			AssertEquals(0m, capacityBreakdown4.GetZoneReservedCapacity(2));
			AssertEquals("Project is now in zone 1, which doesn't have a capacity multiplier in this buffer", stdEstHoursResource2Buffer2, capacityBreakdown4.GetZoneAllocatedCapacity(1));
			AssertEquals("Project is now in zone 1, which doesn't have a capacity multiplier in this buffer", stdEstHoursResource2Buffer2, capacityBreakdown4.GetZoneReservedCapacity(1));

			// Close a task to reduce RemainingEstimateHours
			task2_1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			var newStdEstHoursResource1Buffer2 = stdEstHoursResource1Buffer2 - task2_1.StandardEstimateHours;

			newFactory = Factory.CreateNewFactory();
			section = newFactory.Load<BMBoardSection>(section.PK);
			viewModel = VisualBoardsTestHelper.CreateViewModel(section);
			allTasks = allTasks.Where(t => t.PK != task2_1.PK).ToArray(); // Filter out the closed task, because we don't expect it to be assigned a cell.

			RunBufferPenetrationUpdaterAndAssertTasksInCell(section, viewModel, allTasks, Tuple.Create(7, 2, allTasks));

			capacityBreakdown1 = CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer);
			capacityBreakdown2 = CapacityCalculator.GetUtilisedCapacityBreakdown(otherResource, config.Buffer);
			capacityBreakdown3 = CapacityCalculator.GetUtilisedCapacityBreakdown(resource, otherBuffer);
			capacityBreakdown4 = CapacityCalculator.GetUtilisedCapacityBreakdown(otherResource, otherBuffer);

			AssertEquals(stdEstHoursResource1Buffer1, capacityBreakdown1.UtilisedCapacity);
			AssertEquals("Project item is back in zone 2, so all non-visualised workflows should be in zone 2", stdEstHoursResource1Buffer1, capacityBreakdown1.GetZoneAllocatedCapacity(2));
			AssertEquals("Zone 2 has no multiplier", stdEstHoursResource1Buffer1, capacityBreakdown1.GetZoneReservedCapacity(2));

			AssertEquals(stdEstHoursResource2Buffer1, capacityBreakdown2.UtilisedCapacity);
			AssertEquals("Project item is back in zone 2, so all non-visualised workflows should be in zone 2", stdEstHoursResource2Buffer1, capacityBreakdown2.GetZoneAllocatedCapacity(2));
			AssertEquals("Zone 2 has no multiplier", stdEstHoursResource2Buffer1, capacityBreakdown2.GetZoneReservedCapacity(2));

			AssertEquals(newStdEstHoursResource1Buffer2, capacityBreakdown3.UtilisedCapacity);
			AssertEquals("Project item is back in zone 2, so all non-visualised workflows should be in zone 2", newStdEstHoursResource1Buffer2, capacityBreakdown3.GetZoneAllocatedCapacity(2));
			AssertEquals("Zone 2 has no multiplier", newStdEstHoursResource1Buffer2, capacityBreakdown3.GetZoneReservedCapacity(2));

			AssertEquals(stdEstHoursResource2Buffer2, capacityBreakdown4.UtilisedCapacity);
			AssertEquals("Project item is back in zone 2, so all non-visualised workflows should be in zone 2", stdEstHoursResource2Buffer2, capacityBreakdown4.GetZoneAllocatedCapacity(2));
			AssertEquals("Zone 2 has no multiplier", stdEstHoursResource2Buffer2, capacityBreakdown4.GetZoneReservedCapacity(2));

			// Un-approve diagram to return to using operational buffers for penetration
			subDiagram.UnApprove();
			Factory.Save();

			newFactory = Factory.CreateNewFactory();
			section = newFactory.Load<BMBoardSection>(section.PK);
			viewModel = VisualBoardsTestHelper.CreateViewModel(section);

			RunBufferPenetrationUpdaterAndAssertTasksInCell(section, viewModel, allTasks, Tuple.Create(12, 2, allTasks));

			capacityBreakdown1 = CapacityCalculator.GetUtilisedCapacityBreakdown(resource, config.Buffer);
			capacityBreakdown2 = CapacityCalculator.GetUtilisedCapacityBreakdown(otherResource, config.Buffer);
			capacityBreakdown3 = CapacityCalculator.GetUtilisedCapacityBreakdown(resource, otherBuffer);
			capacityBreakdown4 = CapacityCalculator.GetUtilisedCapacityBreakdown(otherResource, otherBuffer);

			AssertEquals(stdEstHoursResource1Buffer1, capacityBreakdown1.UtilisedCapacity);
			AssertEquals("No longer an approved project item, so should use operational buffer for penetration", stdEstHoursResource1Buffer1, capacityBreakdown1.GetZoneAllocatedCapacity(3));
			AssertEquals("Zone 3 has no multiplier", stdEstHoursResource1Buffer1, capacityBreakdown1.GetZoneReservedCapacity(3));

			AssertEquals(stdEstHoursResource2Buffer1, capacityBreakdown2.UtilisedCapacity);
			AssertEquals("No longer an approved project item, so should use operational buffer for penetration", stdEstHoursResource2Buffer1, capacityBreakdown2.GetZoneAllocatedCapacity(3));
			AssertEquals("Zone 3 has no multiplier", stdEstHoursResource2Buffer1, capacityBreakdown2.GetZoneReservedCapacity(3));

			AssertEquals(newStdEstHoursResource1Buffer2, capacityBreakdown3.UtilisedCapacity);
			AssertEquals("No longer an approved project item, so should use operational buffer for penetration", newStdEstHoursResource1Buffer2, capacityBreakdown3.GetZoneAllocatedCapacity(3));
			AssertEquals("Zone 3 has no multiplier", newStdEstHoursResource1Buffer2, capacityBreakdown3.GetZoneReservedCapacity(3));

			AssertEquals(stdEstHoursResource2Buffer2, capacityBreakdown4.UtilisedCapacity);
			AssertEquals("No longer an approved project item, so should use operational buffer for penetration", stdEstHoursResource2Buffer2, capacityBreakdown4.GetZoneAllocatedCapacity(3));
			AssertEquals("Zone 3 has no multiplier", stdEstHoursResource2Buffer2, capacityBreakdown4.GetZoneReservedCapacity(3));
		}

		public void TestWorkflowCurrentStatus_ShouldUseJobHeaderApprovedShapeForZoneInOperationalBuffer()
		{
			var nonCCRStaff = CreateStaffInCurrentBranchDept("PLG", "Darth Plagueis the Wise");
			var buffer1 = CreateBuffer(config.System, "Buffer 1");
			LinkComponents(config.Bucket, buffer1);

			var ccrStaff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "CR1", "CCR Resource 1");
			ccrStaff.DesignateAsCCR(buffer1);

			var preBuffer = CreateSubBuffer(buffer1, "Pre Buffer 1", timespanMinutes: 64 * 60, offsetMinutes: 0);
			var constraint = BMSTestHelper.CreateConstraint(buffer1, "Constraint 1", offsetMinutes: 64 * 60);
			var postBuffer = CreateSubBuffer(buffer1, "Post Buffer 1", timespanMinutes: 32 * 60, offsetMinutes: 64 * 60);

			var jobHeader1 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			jobHeader1.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(-1);

			var jobHeader2 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader2, "workflow1", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = CreateWorkflow(jobHeader2, "workflow2", buffer1, ZDateTime.UtcNow.AddDays(-100), config.ReleaseGroup.PK);

			var task1_1 = CreateTask(workflow1, nonCCRStaff.GS_Code, (int)(60 * BMConstants.WorkingHoursPerDay * 0.2), estVariationFactor: 1);
			var task1_2 = CreateTask(workflow1, nonCCRStaff.GS_Code, (int)(60 * BMConstants.WorkingHoursPerDay * 0.2), estVariationFactor: 1);
			var task1_3 = CreateTask(workflow1, nonCCRStaff.GS_Code, (int)(60 * BMConstants.WorkingHoursPerDay * 0.6), estVariationFactor: 1);

			var task2_1 = CreateTask(workflow2, nonCCRStaff.GS_Code, 60 * BMConstants.WorkingHoursPerDay, estVariationFactor: 1);
			var task2_2 = CreateTask(workflow2, nonCCRStaff.GS_Code, 60 * BMConstants.WorkingHoursPerDay, estVariationFactor: 1);
			var task2_3 = CreateTask(workflow2, nonCCRStaff.GS_Code, 60 * BMConstants.WorkingHoursPerDay, estVariationFactor: 1);
			var task2_4 = CreateTask(workflow2, ccrStaff.GS_Code, 1, estVariationFactor: 1, sequence: 1000);

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram, "subDiagram");

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			subDiagram.AsEntity(network).Width = 400;
			network.Refresh(RefreshType.RedrawDiagram);

			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			var buffer2 = network.Entities.Single(e => e.ShapeType == ShapeTypeList.Codes.Buffer);
			AssertEquals(200.0, buffer2.Width);

			Factory.Save();

			RunCCPMAndNCNTagRules(Factory);
			Factory.Save();

			AssertCcpmRtrTagApplied(jobHeader2);
			AssertCcpmRtrTagNotApplied(workflow1);
			AssertCcpmRtrTagNotApplied(workflow2);

			// Preconditions of a zone 2 approved project plan item
			AssertMultilineASCIIEquals("Project should be in zone 2", "buffer - Zone 2 (Project Buffer)", workflow1.CurrentStatus);
			AssertMultilineASCIIEquals("Project should be in zone 2",
				@"Buffer 1 - Zone 2 (Project Buffer)
	Pre Buffer 1 - Zone 1", workflow2.CurrentStatus);

			// Advance the date to increase the gap between now and StartableTime
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(12);
			Factory.Save();

			AssertMultilineASCIIEquals("Project is now in zone 1", "buffer - Zone 1 (Project Buffer)", workflow1.CurrentStatus);
			AssertMultilineASCIIEquals("Project is now in zone 1",
				@"Buffer 1 - Zone 1 (Project Buffer)
	Pre Buffer 1 - Zone 0", workflow2.CurrentStatus);

			// Close a task to reduce RemainingEstimateHours
			task2_1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertMultilineASCIIEquals("Project returns to zone 2", "buffer - Zone 2 (Project Buffer)", workflow1.CurrentStatus);
			AssertMultilineASCIIEquals("Project returns to zone 2, but slightly within Zone 2 bounds of Sub 1 buffer",
				@"Buffer 1 - Zone 2 (Project Buffer)
	Pre Buffer 1 - Zone 2", workflow2.CurrentStatus);

			// Un-approve diagram to return to using operational buffers for penetration
			subDiagram.UnApprove();
			Factory.Save();

			AssertMultilineASCIIEquals("Item is in zone 3 of operational buffer", "buffer - Zone 3", workflow1.CurrentStatus);
			AssertMultilineASCIIEquals("Item is in zone 0 of operational buffer",
				@"Buffer 1 - Zone 0
	Pre Buffer 1 - Zone 0", workflow2.CurrentStatus);
		}

		public void TestWorkflowCurrentStatus_WhenWorkflowPartOfApprovedNCN_ShouldUseWorkflowScheduleNotNCNSchedule()
		{
			config.Buffer.FC_Name = "Buffanwe";

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Octomum", config.Buffer, ZDateTime.UtcNow.AddDays(-7), config.ReleaseGroup.PK);
			BMSTestHelper.CreateTask(workflow);

			Factory.Save();

			AssertEquals("Buffanwe - Zone 2", workflow.CurrentStatus);

			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true, scheduledStartTimeUTC: ZDateTime.UtcNow.AddYears(-1));
			var shape = NetworkTestCase.CreateShape(workflow.JobHeader, diagram);

			var viewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = viewModel.GetJobNetwork();

			viewModel.ToggleApproval();

			Factory.Save();

			NetworkTestCase.RunCCPMAndNCNTagRules(Factory);
			NetworkTestCase.RunBufferPenetrationUpdater();

			workflow = Factory.CreateNewFactory().Load<ProcessHeader>(workflow.PK);

			AssertEquals("Buffanwe - Zone 2", workflow.CurrentStatus);

			viewModel.ToggleApproval();
			viewModel.SuggestAndAcceptAllBuffers();
			viewModel.ToggleApproval();
			Factory.Save();

			NetworkTestCase.RunCCPMAndNCNTagRules(Factory);
			NetworkTestCase.RunBufferPenetrationUpdater();

			workflow = Factory.CreateNewFactory().Load<ProcessHeader>(workflow.PK);

			AssertEquals("Buffanwe - Zone 0 (Project Buffer)", workflow.CurrentStatus);
		}

		public void TestGetBufferPenetration_WhenWorkflowsAreShownOnDiagram_ShouldUseWorkflowShapePlannedDurationAndRemainingEstimate()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			jobHeader1.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddDays(-10);

			var jobHeader2 = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader2, "workflow1", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = CreateWorkflow(jobHeader2, "workflow2", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);

			var task1_1 = CreateTask(workflow1, resource.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 2);
			var task1_2 = CreateTask(workflow1, resource.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 2);
			var task2_1 = CreateTask(workflow2, resource.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 1);
			var task2_2 = CreateTask(workflow2, resource.GS_Code, 60 * BMConstants.WorkingHoursPerDay * 1);

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram, "subDiagram");
			var shape1 = CreateShape(workflow1, subDiagram);
			var shape2 = CreateShape(workflow2, subDiagram);

			shape1.MakeVisiblePrerequisiteOf(shape2, subDiagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			subDiagram.AsEntity(network).Width = 900;
			shape1.AsEntity(network).Width = 600;
			shape2.AsEntity(network).Width = 300;
			network.RefreshSchedules();
			networkViewModel.PushAsLateAsPossible();

			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			Factory.Save();

			RunCCPMAndNCNTagRules(Factory);
			RunBufferPenetrationUpdater();
			Factory.Save();

			AssertCcpmRtrTagApplied(workflow1);
			AssertCcpmRtrTagApplied(workflow2);

			var componentContents = CapacityCalculator.GetComponentContents_ForTest(config.Buffer, new[]
			{
				resource
			});
			var workflow1DTO = componentContents.Single(d => d.PK == workflow1.PK);
			var workflow2DTO = componentContents.Single(d => d.PK == workflow2.PK);

			var context = WorkingTimeContext.Create(config.Buffer);

			var workflow1Penetration = BufferPenetrationCalculator.CalculatePenetrationPercentage(workflow1DTO, context, Factory).Penetration;
			var workflow2Penetration = BufferPenetrationCalculator.CalculatePenetrationPercentage(workflow2DTO, context, Factory).Penetration;

			CombineAssertions(() =>
			{
				AssertEquals("workflow1Penetration: Penetration for workflow1 should use its own planned duration/remaining estimate", 1.425m, workflow1Penetration);
				AssertEquals("workflow2Penetration: Penetration for workflow2 should use its own planned duration/remaining estimate", 0.225m, workflow2Penetration);

				AssertEquals("workflow1Penetration: Penetration should be the same whether we're using DTOs or the real thing", workflow1Penetration, BufferPenetrationCalculator.CalculatePenetrationPercentage(workflow1, context, Factory).Penetration);
				AssertEquals("workflow2Penetration: Penetration should be the same whether we're using DTOs or the real thing", workflow2Penetration, BufferPenetrationCalculator.CalculatePenetrationPercentage(workflow2, context, Factory).Penetration);
			});
		}

		public void TestGetUtilisedCapacity_WhenShapePenetratesMultipleBuffers()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Snoke is not Plagueis", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, resource.GS_Code, 60, estVariationFactor: 1);

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape_CC1 = NetworkTestCase.CreateShape(diagram);
			var shape_CC2 = NetworkTestCase.CreateShape(diagram);
			var shape_CC3 = NetworkTestCase.CreateShape(diagram);
			var shape_nonCC = NetworkTestCase.CreateShape(workflow, diagram);

			shape_CC1.MakeVisiblePrerequisiteOf(shape_CC2, diagram);
			shape_CC2.MakeVisiblePrerequisiteOf(shape_CC3, diagram);
			shape_nonCC.MakeVisiblePrerequisiteOf(shape_CC3, diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			var projectBuffer = network.Entities.Single(b => b.ShapeType == ShapeTypeList.Codes.Buffer && b.Name.Contains("Project Buffer")).AsShape();
			var feedingBuffer = network.Entities.Single(b => b.ShapeType == ShapeTypeList.Codes.Buffer && b.Name.Contains("Feeding Buffer")).AsShape();

			AssertContainsExactElementsInAnyOrder(new[] { projectBuffer, feedingBuffer }, shape_nonCC.GetRelatedBuffers());

			Factory.Save();

			RunCCPMAndNCNTagRules(Factory);
			Factory.Save();

			AssertEquals(1.0m, CapacityCalculator.GetUtilisedCapacity_ForTest(resource, config.Buffer));
		}

		public void TestCapacityReCalculation_ForApprovedCCPMWorkReleasedIntoZone0_ShouldPersistChanges_AndConsiderZoneMultipliers()
		{
			BMSTestHelper.CreateZoneMultiplier(config.Buffer, zone0Multiplier: 10, releaseGroupPK: config.ReleaseGroup.PK); // Work released into zone 0 gets a multiplier. This should capture work released into a zone that isn't zone 3, which is only possible for CCPM work.

			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "Welcome to your nightmare", releaseGroupPK: config.ReleaseGroup.PK);
			var task = BMSTestHelper.CreateTask(workflow, resource.GS_Code, 60, estVariationFactor: 1);

			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true, scheduledStartTimeUTC: ZDateTime.UtcNow.AddDays(-100)); // Scheduled far enough into the past so will be in zone 0.
			var shape = NetworkTestCase.CreateShape(workflow, diagram);

			var viewModel = NetworkTestCase.CreateNetworkViewModel(diagram, createNodeViewModels: false);
			var network = viewModel.GetJobNetwork();

			viewModel.SuggestAndAcceptAllBuffers();
			viewModel.ToggleApproval();

			Factory.Save();

			RunCCPMAndNCNTagRules(Factory);
			RunBufferPenetrationUpdater();

			AssertEquals("Pre-condition: shape is in zone 0, with more than 100% buffer penetration", 72.125m, shape.BufferPenetration);
			AssertAvailableCapacity("Pre-condition: resource should have their full capacity available", resource, config.Buffer, FullCapacity);

			RunReleaseGateAndAssertReleaseOutcome(workflow, config.Buffer, shouldBeReleasedToBuffer: true);

			AssertCapacity(shouldConsiderZoneMultiplier: false);

			BufferCapacityCache.Clear();
			AssertCapacity(shouldConsiderZoneMultiplier: false);

			BufferCapacityCacheTest.PurgeCachedCapacity();
			AssertCapacity(shouldConsiderZoneMultiplier: true);

			void AssertCapacity(bool shouldConsiderZoneMultiplier)
			{
				// TODO: in WI00222320, make all calls to this assertion have the same result.
				var expectedAvailableCapacity = shouldConsiderZoneMultiplier ? FullCapacity - 10m : FullCapacity - 1m;

				AssertAvailableCapacity("Resource capacity should be reduced by the size of the task, multiplied by the zone 0 throttle.", resource, config.Buffer, expectedAvailableCapacity);
			}
		}

		#region Implementation

		GlbStaff resource;
		SchematicTestConfig config;

		const decimal FullCapacity = 48.0m;

		protected override void SetUp()
		{
			base.SetUp();

			BMSRegistry.Instance.EnableSimpleCapacityCalculationForAllSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			resource = CreateStaffInCurrentBranchDept("SAU", "Lord Sauron");
			config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			config.ReleaseGroup.Staff.Add(Factory.NewWithValidTestData<GlbStaff>()); // to avoid release group validation errors on shape's workflow

			CreateZoneMultiplier(config.Buffer);

			BMSTestHelper.CreateServiceTask(Factory, TransferRuleRunnerServiceTask.Code, TransferRuleRunnerServiceTask.Description, taskPeriodCount: 1, taskPeriod: 'H', branch: Env.CurrentBranchPK);
			Factory.Save();
		}

		protected override bool ShouldDisableAsyncBehaviour => true;

		#endregion
	}
}
