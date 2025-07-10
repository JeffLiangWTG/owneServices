using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using Color = System.Drawing.Color;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[GuiTest]
	[TestDate(2015, 6, 16)]
	class JobNetworkNodeViewModelTest : NetworkTestCase
	{
		#region Estimates

		public void TestEstimates_WhenShapeIsLinkedToNonScaledDiagram()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Main Diagram", isScaled: true);
			var linkedShape = NetworkTestCase.CreateShape(diagram, name: "Linked Shape");

			var relatedDiagram = NetworkTestCase.CreateDiagram(Factory, name: "Related Diagram", isScaled: false);
			AssertEquals(false, relatedDiagram.IsScaled);

			BMNCNShapeTest.LinkShapeToDiagram(linkedShape, parentDiagram: diagram, diagramToLink: relatedDiagram);
			AssertEquals("Precondition", relatedDiagram, linkedShape.RelatedShape);

			var shape1 = NetworkTestCase.CreateShape(relatedDiagram, name: "Assigned Shape", explicitDurationMinutes: 8 * 60);
			shape1.BNS_Status = ShapeStatusList.Codes.Assigned;
			AssertEquals(8m, shape1.ExplicitDurationHours);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var estimates = GetEstimateDetails(networkViewModel, linkedShape);
			AssertNull(estimates);
		}

		public void TestEstimates_WhenShapeIsNotLinked()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Main Diagram", isScaled: true);
			var nonLinkedShape = NetworkTestCase.CreateShape(diagram, name: "Non Linked Shape");

			var networkViewModel = CreateNetworkViewModel(diagram);
			var estimates = GetEstimateDetails(networkViewModel, nonLinkedShape);
			AssertNull(estimates);
		}

		public void TestEstimates_WhenRelatedDiagramIsEmpty()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Main Diagram", isScaled: true);
			var linkedShape = NetworkTestCase.CreateShape(diagram, name: "Linked Shape");

			var relatedDiagram = NetworkTestCase.CreateDiagram(Factory, name: "Related Diagram", isScaled: true);

			BMNCNShapeTest.LinkShapeToDiagram(linkedShape, parentDiagram: diagram, diagramToLink: relatedDiagram);
			AssertEquals("Precondition", relatedDiagram, linkedShape.RelatedShape);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var estimates = GetEstimateDetails(networkViewModel, linkedShape);
			AssertNull(estimates);
		}

		public void TestEstimates_WhenRelatedDiagramHasNonLinkedShapesOfDifferentStatuses()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Main Diagram", isScaled: true);
			var linkedShape = NetworkTestCase.CreateShape(diagram, name: "Linked Shape");

			var relatedDiagram = NetworkTestCase.CreateDiagram(Factory, name: "Related Diagram", isScaled: true);

			BMNCNShapeTest.LinkShapeToDiagram(linkedShape, parentDiagram: diagram, diagramToLink: relatedDiagram);
			AssertEquals("Precondition", relatedDiagram, linkedShape.RelatedShape);

			var shapeASN = NetworkTestCase.CreateShape(relatedDiagram, name: "Assigned Shape", explicitDurationMinutes: 8 * 60);
			shapeASN.BNS_Status = ShapeStatusList.Codes.Assigned;
			AssertEquals(8m, shapeASN.ExplicitDurationHours);

			var shapeCLS = NetworkTestCase.CreateShape(relatedDiagram, name: "Closed Shape", explicitDurationMinutes: 2 * 8 * 60);
			shapeCLS.BNS_Status = ShapeStatusList.Codes.Closed;
			AssertEquals(16m, shapeCLS.ExplicitDurationHours);

			var shapeCAN = NetworkTestCase.CreateShape(relatedDiagram, name: "Cancelled Shape", explicitDurationMinutes: 3 * 8 * 60);
			shapeCAN.BNS_Status = ShapeStatusList.Codes.Cancelled;
			AssertEquals(24m, shapeCAN.ExplicitDurationHours);

			AssertEquals("Precondition: RemainingEstimateHoursIncludingChildren should be taken from all non-closed and non-cancelled shapes", 8m, linkedShape.RemainingEstimateHoursIncludingChildren);
			AssertEquals("Precondition: TotalNonCancelledEstimatedHoursIncludingChildren should be taken from all non-cancelled shapes", 3 * 8m, linkedShape.TotalNonCancelledEstimatedHoursIncludingChildren);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var estimates = GetEstimateDetails(networkViewModel, linkedShape);
			AssertNotNull(estimates);
			AssertEquals("TotalEstimateHours: should take from TotalNonCancelledEstimatedHoursIncludingChildren", 24m, estimates.TotalEstimateHours);
			AssertEquals("CompletedEstimateHours: should be the difference between TotalNonCancelledEstimatedHoursIncludingChildren and RemainingEstimateHoursIncludingChildren", 16m, estimates.CompletedEstimateHours);
		}

		public void TestEstimates_WhenRelatedDiagramHasShapeLinkedToProcessHeader()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Main Diagram", isScaled: true);
			var linkedShape = NetworkTestCase.CreateShape(diagram, name: "Linked Shape");

			var relatedDiagram = NetworkTestCase.CreateDiagram(Factory, name: "Related Diagram", isScaled: true);

			BMNCNShapeTest.LinkShapeToDiagram(linkedShape, parentDiagram: diagram, diagramToLink: relatedDiagram);
			AssertEquals("Precondition", relatedDiagram, linkedShape.RelatedShape);

			var shapeASN = NetworkTestCase.CreateShape(relatedDiagram, name: "Assigned Shape", explicitDurationMinutes: 8 * 60);
			shapeASN.BNS_Status = ShapeStatusList.Codes.Assigned;
			AssertEquals(8m, shapeASN.ExplicitDurationHours);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 60, description: "Assigned Task", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, estVariationFactor: 1);
			BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 2 * 60, description: "Closed Task", taskStatus: ProcessTaskStatusCodeList.Codes.Closed, estVariationFactor: 1);
			BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 3 * 60, description: "Cancelled Task", taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled, estVariationFactor: 1);

			var shapeLinkedToWorkflow = NetworkTestCase.CreateShape(workflow, relatedDiagram, name: "Shape Linked to Workflow");

			AssertEquals("Precondition: RemainingEstimateHoursIncludingChildren on workflow should be taken from all non-closed and non-cancelled tasks", 1m, shapeLinkedToWorkflow.RemainingEstimateHoursIncludingChildren);
			AssertEquals("Precondition: TotalNonCancelledEstimatedHoursIncludingChildren on workflow should be taken from all non-cancelled tasks", 3m, shapeLinkedToWorkflow.TotalNonCancelledEstimatedHoursIncludingChildren);

			AssertEquals("Precondition: RemainingEstimateHoursIncludingChildren on shape linked to diagram", 8m + 1m, linkedShape.RemainingEstimateHoursIncludingChildren);
			AssertEquals("Precondition: TotalNonCancelledEstimatedHoursIncludingChildren on shape linked to diagram", 8m + 3m, linkedShape.TotalNonCancelledEstimatedHoursIncludingChildren);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var estimates = GetEstimateDetails(networkViewModel, linkedShape);
			AssertNotNull(estimates);
			AssertEquals("TotalEstimateHours: should take from TotalNonCancelledEstimatedHoursIncludingChildren", 11m, estimates.TotalEstimateHours);
			AssertEquals("CompletedEstimateHours: should be the difference between TotalNonCancelledEstimatedHoursIncludingChildren and RemainingEstimateHoursIncludingChildren", 2m, estimates.CompletedEstimateHours);
		}

		JobNetworkNodeEstimateDetails GetEstimateDetails(NetworkViewModel networkViewModel, BMNCNShape shape)
		{
			var network = networkViewModel.GetJobNetwork();
			return network.Entities.GetInstance(shape).GetEstimateDetails();
		}

		#endregion

		#region Tooltip

		public void TestTooltip_WorkflowsInSameJob()
		{
			TestTooltip(isWorkflowsInSameJob: true);
		}

		public void TestTooltip_WorkflowsInDifferentJobs()
		{
			TestTooltip(isWorkflowsInSameJob: false);
		}

		void TestTooltip(bool isWorkflowsInSameJob)
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);

			var parentJob = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Parent Job 1");
			var parentWorkflow = BMSTestHelper.CreateWorkflow(parentJob, "Parent Workflow 1", releaseGroupPK: config.ReleaseGroup.PK);
			var parentTask = BMSTestHelper.CreateTask(parentWorkflow, GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 60, description: "Parent Task 1", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, estVariationFactor: 1);

			var job = isWorkflowsInSameJob
				? parentJob
				: BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Child Job 1");

			var childWorkflow = BMSTestHelper.CreateWorkflow(job, "Child Workflow 1", releaseGroupPK: config.ReleaseGroup.PK);
			var childTask = BMSTestHelper.CreateTask(childWorkflow, GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 60, description: "Child Task 1", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, estVariationFactor: 1);

			job = isWorkflowsInSameJob
				? parentJob
				: BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Grandchild Job 1");

			var grandchildWorkflow = BMSTestHelper.CreateWorkflow(job, "Grandchild Workflow 1", releaseGroupPK: config.ReleaseGroup.PK);
			var grandchildTask = BMSTestHelper.CreateTask(grandchildWorkflow, GlbStaff.CurrentUser.GS_Code, lowEstMinutes: 60, description: "Grandchild Task 1", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned, estVariationFactor: 1);

			BufferManagement.Business.Test.BMSTestHelper.MakeChildOf(childWorkflow, parentWorkflow);
			BufferManagement.Business.Test.BMSTestHelper.MakeChildOf(grandchildWorkflow, childWorkflow);

			Factory.Save();

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var parentShape = NetworkTestCase.CreateShape(parentWorkflow, diagram);
			var childShape = NetworkTestCase.CreateShape(childWorkflow, diagram);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			CombineAssertions("GIVEN parent, child, granchild workflows WHEN calling LowerBarTooltip THEN should show positive value", () =>
			{
				var parentViewModel = CreateViewModel(network.Entities.GetInstance(parentShape), networkViewModel);

				AssertTooltip(
					diagram,
					parentWorkflow,
					parentViewModel,
					parentTaskStatus: ProcessTaskStatusCodeList.Codes.Assigned,
					childTaskStatus: ProcessTaskStatusCodeList.Codes.Assigned,
					grandchildTaskStatus: ProcessTaskStatusCodeList.Codes.Assigned,
					expectedTooltip: "0% complete (0 hours of 3 hours complete)");

				AssertTooltip(
					diagram,
					parentWorkflow,
					parentViewModel,
					parentTaskStatus: ProcessTaskStatusCodeList.Codes.Assigned,
					childTaskStatus: ProcessTaskStatusCodeList.Codes.Assigned,
					grandchildTaskStatus: ProcessTaskStatusCodeList.Codes.Closed,
					expectedTooltip: "33% complete (1 hour of 3 hours complete)");

				AssertTooltip(
					diagram,
					parentWorkflow,
					parentViewModel,
					parentTaskStatus: ProcessTaskStatusCodeList.Codes.Assigned,
					childTaskStatus: ProcessTaskStatusCodeList.Codes.Closed,
					grandchildTaskStatus: ProcessTaskStatusCodeList.Codes.Assigned,
					expectedTooltip: "33% complete (1 hour of 3 hours complete)");

				AssertTooltip(
					diagram,
					parentWorkflow,
					parentViewModel,
					parentTaskStatus: ProcessTaskStatusCodeList.Codes.Assigned,
					childTaskStatus: ProcessTaskStatusCodeList.Codes.Closed,
					grandchildTaskStatus: ProcessTaskStatusCodeList.Codes.Closed,
					expectedTooltip: "67% complete (2 hours of 3 hours complete)");

				AssertTooltip(
					diagram,
					parentWorkflow,
					parentViewModel,
					parentTaskStatus: ProcessTaskStatusCodeList.Codes.Closed,
					childTaskStatus: ProcessTaskStatusCodeList.Codes.Assigned,
					grandchildTaskStatus: ProcessTaskStatusCodeList.Codes.Assigned,
					expectedTooltip: "33% complete (1 hour of 3 hours complete)");

				AssertTooltip(
					diagram,
					parentWorkflow,
					parentViewModel,
					parentTaskStatus: ProcessTaskStatusCodeList.Codes.Closed,
					childTaskStatus: ProcessTaskStatusCodeList.Codes.Assigned,
					grandchildTaskStatus: ProcessTaskStatusCodeList.Codes.Closed,
					expectedTooltip: "67% complete (2 hours of 3 hours complete)");

				AssertTooltip(
					diagram,
					parentWorkflow,
					parentViewModel,
					parentTaskStatus: ProcessTaskStatusCodeList.Codes.Closed,
					childTaskStatus: ProcessTaskStatusCodeList.Codes.Closed,
					grandchildTaskStatus: ProcessTaskStatusCodeList.Codes.Assigned,
					expectedTooltip: "67% complete (2 hours of 3 hours complete)");

				AssertTooltip(
					diagram,
					parentWorkflow,
					parentViewModel,
					parentTaskStatus: ProcessTaskStatusCodeList.Codes.Closed,
					childTaskStatus: ProcessTaskStatusCodeList.Codes.Closed,
					grandchildTaskStatus: ProcessTaskStatusCodeList.Codes.Closed,
					expectedTooltip: string.Empty);
			});
		}

		void AssertTooltip(BMNCNShape diagram, ProcessHeader parentWorkflow, NodeViewModel parentViewModel, string parentTaskStatus, string childTaskStatus, string grandchildTaskStatus, string expectedTooltip)
		{
			var childWorkflow = parentWorkflow.ChildHeaders.First();
			var grandchildWorkflow = childWorkflow.ChildHeaders.First();

			SetTaskStatusAndUpateWorkflowStatus(parentWorkflow, parentTaskStatus);

			SetTaskStatusAndUpateWorkflowStatus(childWorkflow, childTaskStatus);

			SetTaskStatusAndUpateWorkflowStatus(grandchildWorkflow, grandchildTaskStatus);

			Factory.Save();

			diagram.Refresh();

			AssertEquals(
				$"Parent={parentTaskStatus}; Child={childTaskStatus}; Grandchild={grandchildTaskStatus}",
				expectedTooltip,
				parentViewModel.LowerBarTooltip ?? string.Empty);
		}

		void SetTaskStatusAndUpateWorkflowStatus(ProcessHeader workflow, string status)
		{
			var task = workflow.Tasks.First();
			if (task.P9_Status != status)
			{
				task.P9_Status = status;

				workflow.FH_AgreedDeliveryDate = workflow.FH_AgreedDeliveryDate.IsValid // Update process-header STATUS that occurs when we close task in workflow form
					? workflow.FH_AgreedDeliveryDate.AddDays(1)
					: ZDateTime.Today.AddDays(1);
			}
		}

		#endregion

		#region Risk States

		public void TestLowerBar_LinkedDiagramIsNotComplete_ButAllChildrenAreComplete()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "New Horizons");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Jupiter", releaseGroupPK: config.ReleaseGroup.PK);
			var task1 = BMSTestHelper.CreateTask(workflow1, taskStatus: "CLS", lowEstMinutes: 200);

			Factory.Save();

			var diagramShape = CreateDiagram(Factory);
			var shape1 = CreateShape(diagramShape);
			var network1 = CreateNetworkViewModel(diagramShape).GetJobNetwork();

			var diagramShape2 = CreateDiagram(Factory);
			var shape2 = CreateShape(diagramShape2);
			var network2  = CreateNetworkViewModel(diagramShape2).GetJobNetwork();
			network2.LinkEntity(shape2, workflow1);
			network1.LinkEntity(shape1, diagramShape2);
			
			var entity = network1.Entities.GetInstance(shape1);
			var details = entity.GetEstimateDetails();
			AssertEquals(1m, details.PercentComplete);
		}

		public void TestLowerBarRiskStates_LateStart()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "New Horizons");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Jupiter", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Pluto", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Kuiper Belt", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "Outer Void", releaseGroupPK: config.ReleaseGroup.PK);
			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 10, description: "Swing By", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 10, description: "Photograph", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);
			var task3 = BMSTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 10, description: "Discover", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);
			var task4 = BMSTestHelper.CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, 10, description: "Crash in a big ball of flames", taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled);

			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow3);
			workflow3.GetOrCreateDependencyLink(workflow4);

			var diagramShape = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var subDiagram = CreateShape(jobHeader, diagram);
			var shape1 = CreateShape(workflow1, subDiagram);
			var shape2 = CreateShape(workflow2, subDiagram);
			var shape3 = CreateShape(workflow3, subDiagram);
			var shape4 = CreateShape(workflow4, subDiagram);

			diagram.ScheduledStartTimeLocal = ZDateTime.Now.AddDays(1);
			UnitTestUserNotification.Instance.AddOKAnswer();

			network.SwitchToScaled();
			networkViewModel.ToggleApproval();
			network.EditEntity(diagram);

			var subDiagramViewModel = CreateViewModel(subDiagram, networkViewModel);
			var shape1ViewModel = CreateViewModel(shape1, networkViewModel);
			var shape2ViewModel = CreateViewModel(shape2, networkViewModel);
			var shape3ViewModel = CreateViewModel(shape3, networkViewModel);
			var shape4ViewModel = CreateViewModel(shape4, networkViewModel);

			AssertEquals(0.3333, subDiagramViewModel.LowerBar.percent, 0.001);

			AssertEquals(0.0, shape2ViewModel.LowerBar.percent);

			AssertEquals(0.0, shape3ViewModel.LowerBar.percent);

			AssertNull("Workflow1 is complete - no progress bar", shape1ViewModel.LowerBar);

			AssertNull(shape1ViewModel.LowerBarTooltip);
			AssertEquals("0% complete (0 hours of 0.25 hours complete)", shape2ViewModel.LowerBarTooltip);
			AssertEquals("0% complete (0 hours of 0.25 hours complete)", shape3ViewModel.LowerBarTooltip);
			AssertNull(shape4ViewModel.LowerBarTooltip);

			AssertEquals("No risk state exists, so fade background is transparent", Color.LightGray, subDiagramViewModel.LowerBar.background);
			AssertEquals("No risk state exists, so fade background is transparent", Color.LightGray, shape2ViewModel.LowerBar.background);
			AssertEquals("No risk state exists, so fade background is transparent", Color.LightGray, shape3ViewModel.LowerBar.background);

			diagram.ScheduledStartTimeLocal = ZDateTime.Now.AddDays(-20);
			network.EditEntity(diagram);

			AssertEquals("Workflow2 within this job is not yet released and is scheduled to have already started", JobNetworkNodeViewModel.RiskColor, subDiagramViewModel.LowerBar.background);
			AssertEquals("Workflow2 is not yet released and is scheduled to have already started", JobNetworkNodeViewModel.RiskColor, shape2ViewModel.LowerBar.background);
			AssertEquals("Workflow3 is not yet released and is scheduled to have already started, but is not startable", Color.LightGray, shape3ViewModel.LowerBar.background);

			AssertEquals("0% complete (0 hours of 0.25 hours complete)\r\nWARNING: This entity is scheduled to have started and is startable but has not been released to a buffer", shape2ViewModel.LowerBarTooltip);
			AssertEquals("0% complete (0 hours of 0.25 hours complete)", shape3ViewModel.LowerBarTooltip);

			workflow2.FH_FC_CurrentComponent = config.Buffer.PK;
			network.EditEntity(diagram);

			AssertEquals("All startable workflows within this job are complete or released", Color.LightGray, subDiagramViewModel.LowerBar.background);
			AssertEquals("Workflow2 has been released", Color.LightGray, shape2ViewModel.LowerBar.background);
			AssertEquals("Workflow3 is not yet released and is scheduled to have already started, but is not startable", Color.LightGray, shape3ViewModel.LowerBar.background);

			AssertEquals("0% complete (0 hours of 0.25 hours complete)", shape2ViewModel.LowerBarTooltip);
			AssertEquals("0% complete (0 hours of 0.25 hours complete)", shape3ViewModel.LowerBarTooltip);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			AssertEquals("Workflow3 within this job is not yet released and is scheduled to have already started", Color.LightGray, subDiagramViewModel.LowerBar.background);
			AssertNull("Workflow2 is complete - no progress bar", shape1ViewModel.LowerBar);
			AssertEquals("Workflow3 is not yet released and is scheduled to have already started", JobNetworkNodeViewModel.RiskColor, shape3ViewModel.LowerBar.background);

			AssertNull(shape2ViewModel.LowerBarTooltip);
			AssertEquals("0% complete (0 hours of 0.25 hours complete)\r\nWARNING: This entity is scheduled to have started and is startable but has not been released to a buffer", shape3ViewModel.LowerBarTooltip);

			workflow3.FH_FC_CurrentComponent = config.Buffer.PK;
			network.EditEntity(diagram);

			AssertEquals("All startable workflows within this job are complete or released", Color.LightGray, subDiagramViewModel.LowerBar.background);
			AssertNull("Workflow2 is complete - no progress bar", shape1ViewModel.LowerBar);
			AssertEquals("Workflow3 is released", Color.LightGray, shape3ViewModel.LowerBar.background);

			AssertEquals("0% complete (0 hours of 0.25 hours complete)", shape3ViewModel.LowerBarTooltip);
		}

		public void TestLowerBarRiskStates_LargerActualDurationThanEstimate()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "New Horizons");
			var workflow0 = BMSTestHelper.CreateWorkflow(jobHeader, "Sun", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Jupiter", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Pluto", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Kuiper Belt", releaseGroupPK: config.ReleaseGroup.PK);
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "Outer Void", releaseGroupPK: config.ReleaseGroup.PK);
			BMSTestHelper.MakeChildOf(workflow1, workflow0);
			BMSTestHelper.MakeChildOf(workflow2, workflow0);
			BMSTestHelper.MakeChildOf(workflow3, workflow0);
			BMSTestHelper.MakeChildOf(workflow4, workflow0);
			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 10, description: "Swing By", taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 10, description: "Photograph", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);
			var task3 = BMSTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 10, description: "Discover", taskStatus: ProcessTaskStatusCodeList.Codes.Assigned);
			var task4 = BMSTestHelper.CreateTask(workflow4, GlbStaff.CurrentUser.GS_Code, 10, description: "Fly safely through the air", taskStatus: ProcessTaskStatusCodeList.Codes.Cancelled);

			workflow1.GetOrCreateDependencyLink(workflow2);
			workflow2.GetOrCreateDependencyLink(workflow3);
			workflow3.GetOrCreateDependencyLink(workflow4);

			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var subDiagram = NetworkTestCase.CreateShape(workflow0, diagram);

			diagramShape.EarliestStartTimeUtc = ZDateTime.UtcNow.AddDays(1);

			network.SwitchToScaled();
			networkViewModel.ToggleApproval();

			subDiagram.SetWidthForDuration(3 * 8 * 60); // 3 days
			network.EditEntity(diagram);

			var subDiagramViewModel = CreateViewModel(subDiagram, networkViewModel);

			AssertEquals("No risk state", Color.LightGray, subDiagramViewModel.LowerBar.background);
			AssertEquals("33% complete (0.25 hours of 0.75 hours complete)", subDiagramViewModel.LowerBarTooltip);

			task1.P9_ActualDuration = new ZInt(4 * 8 * 60).GetDateTimeFromMinutes();
			Factory.Save();
			AssertEquals("Actual duration is larger than the planned duration, and the shape is not complete yet", JobNetworkNodeViewModel.RiskColor, subDiagramViewModel.LowerBar.background);
			AssertEquals("33% complete (0.25 hours of 0.75 hours complete)\r\nWARNING: This entity has already taken longer to complete than its planned duration", subDiagramViewModel.LowerBarTooltip);

			task2.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task3.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			AssertNull("Shape is now complete, so no risk state", subDiagramViewModel.LowerBar);
			AssertNull(subDiagramViewModel.LowerBarTooltip);
		}

		public void TestLowerBarRiskStates_ShouldNotApplyToNonScaledDiagram()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "You can't just ask somebody why they're white");
			var task1 = BMSTestHelper.CreateTask(workflow, lowEstMinutes: 60, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			var task2 = BMSTestHelper.CreateTask(workflow, lowEstMinutes: 60);
			var task3 = BMSTestHelper.CreateTask(workflow, lowEstMinutes: 60);
			task1.P9_ActualDuration = new ZInt(60).GetDateTimeFromMinutes();

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape = NetworkTestCase.CreateShape(workflow, diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var viewModel = CreateViewModel(network.Entities.GetInstance(shape), networkViewModel);

			AssertEquals("No risk state", Color.LightGray, viewModel.LowerBar.background);
			AssertEquals("33% complete (1.5 hours of 4.5 hours complete)", viewModel.LowerBarTooltip);
		}

		#endregion

		#region Progress Bar

		public void TestLowerBar_WhenFlagOffForRootDiagram()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			BMSTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);

			var diagramShape = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;

			var linkedSubDiagramShape = CreateShape(jobHeader, diagram);
			var linkedShape = CreateShape(workflow, linkedSubDiagramShape);
			var linkedShapeViewModel = CreateViewModel(linkedShape, networkViewModel);
			var linkedSubDiagramViewModel = CreateViewModel(linkedSubDiagramShape, networkViewModel);

			AssertEquals(5, linkedShapeViewModel.LowerBarHeight);
			AssertEquals(5, linkedSubDiagramViewModel.LowerBarHeight);
			AssertNotNull(linkedShapeViewModel.LowerBar);
			AssertNotNull(linkedShapeViewModel.LowerBar);

			diagram.Shape.DisplayCompletenessIndicator = false;

			AssertEquals(0, linkedShapeViewModel.LowerBarHeight);
			AssertEquals(0, linkedSubDiagramViewModel.LowerBarHeight);
			AssertNull(linkedShapeViewModel.LowerBar);
			AssertNull(linkedShapeViewModel.LowerBar);
		}

		public void TestLowerBar_UpdateBarWhenQualityIterationExists()
		{
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType("QCB", "ORG");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");

			var task1_workflow1 = BMSTestHelper.CreateTask(workflow1, lowEstMinutes: 60, taskType: "QCB");
			var task2_workflow1 = BMSTestHelper.CreateTask(workflow1, lowEstMinutes: 45, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);

			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			var task1_workflow2 = BMSTestHelper.CreateTask(workflow2, lowEstMinutes: 30);
			var task2_workflow2 = BMSTestHelper.CreateTask(workflow2, lowEstMinutes: 40);
			workflow2.GetOrCreateLinkToParent(workflow1);

			var pivot = CreateIterationLink(task1_workflow1, workflow2, task2_workflow1, IterationLinkTypeList.Codes.QualityIterationTask);
			Factory.Save();

			var diagramShape = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;

			var linkedShape = CreateShape(workflow1, diagram);

			diagram.Shape.DisplayCompletenessIndicator = true;

			var linkedShapeViewModel = CreateViewModel(linkedShape, networkViewModel);
			AssertNotNull(linkedShapeViewModel.LowerBar);

			AssertEquals(0.2571, linkedShapeViewModel.LowerBar.percent, 0.001);
		}

		#endregion

		#region Default Diagrams

		public void TestDefaultDiagramShapes_ShouldNotShowProgressBar()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "New Horizons");
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Jupiter");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Pluto");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Kuiper Belt");
			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 10, description: "Swing By");
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 10, description: "Photograph");
			var task3 = BMSTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 10, description: "Discover");

			var defaultDiagram = jobHeader.GetDefaultDiagram();
			var defaultShape1 = workflow1.GetDefaultShape(defaultDiagram);
			var defaultShape2 = workflow2.GetDefaultShape(defaultDiagram);
			var defaultShape3 = workflow3.GetDefaultShape(defaultDiagram);

			var nonDefaultDiagram = CreateDiagram(jobHeader);
			var nonDefaultShape1 = CreateShape(workflow1, nonDefaultDiagram);
			var nonDefaultShape2 = CreateShape(workflow2, nonDefaultDiagram);
			var nonDefaultShape3 = CreateShape(workflow3, nonDefaultDiagram);

			var defaultNetworkViewModel = CreateNetworkViewModel(defaultDiagram);
			var defaultNetwork = defaultNetworkViewModel.GetJobNetwork();

			var nonDefaultNetworkViewModel = CreateNetworkViewModel(nonDefaultDiagram);
			var nonDefaultNetwork = nonDefaultNetworkViewModel.GetJobNetwork();

			var defaultShape1ViewModel = CreateViewModel(defaultShape1.AsEntity(defaultNetwork), defaultNetworkViewModel);
			var defaultShape2ViewModel = CreateViewModel(defaultShape2.AsEntity(defaultNetwork), defaultNetworkViewModel);
			var defaultShape3ViewModel = CreateViewModel(defaultShape3.AsEntity(defaultNetwork), defaultNetworkViewModel);

			var nonDefaultShape1ViewModel = CreateViewModel(nonDefaultShape1.AsEntity(nonDefaultNetwork), nonDefaultNetworkViewModel);
			var nonDefaultShape2ViewModel = CreateViewModel(nonDefaultShape2.AsEntity(nonDefaultNetwork), nonDefaultNetworkViewModel);
			var nonDefaultShape3ViewModel = CreateViewModel(nonDefaultShape3.AsEntity(nonDefaultNetwork), nonDefaultNetworkViewModel);

			AssertNull(defaultShape1ViewModel.LowerBar);
			AssertNull(defaultShape2ViewModel.LowerBar);
			AssertNull(defaultShape3ViewModel.LowerBar);

			AssertNotNull(nonDefaultShape1ViewModel.LowerBar);
			AssertNotNull(nonDefaultShape2ViewModel.LowerBar);
			AssertNotNull(nonDefaultShape3ViewModel.LowerBar);
		}

		#endregion

		#region Implementation

		static NodeViewModel CreateViewModel(INetworkEntity entity, NetworkViewModel networkViewModel)
		{
			var viewModelProvider = new JobNetworkNodeViewModelProvider();
			return viewModelProvider.Create(entity, networkViewModel);
		}

		#endregion
	}

	class JobNetworkNodeViewModelTest_WithSampleData : NetworkTestCase
	{
		#region Progress Bar

		public void TestLowerBarHeight()
		{
			AssertEquals(0, nonLinkedShapeViewModel.LowerBarHeight);
			AssertEquals(5, linkedShapeViewModel.LowerBarHeight);
			AssertEquals(0, linkedShapeWithZeroEstimateViewModel.LowerBarHeight);
			AssertEquals(5, linkedShapeWithZeroCompletedViewModel.LowerBarHeight);
		}

		public void TestLowerBarBrush()
		{
			AssertNull(nonLinkedShapeViewModel.LowerBar);
			AssertNotNull(linkedSubDiagramShapeViewModel.LowerBar);
			AssertNotNull(linkedShapeViewModel.LowerBar);
			AssertNull(linkedShapeWithZeroEstimateViewModel.LowerBar);
			AssertNotNull(linkedShapeWithZeroCompletedViewModel.LowerBar);

			AssertEquals(1.0 / 4.0, linkedSubDiagramShapeViewModel.LowerBar.percent, 0.001);

			AssertEquals(1.0 / 3.0, linkedShapeViewModel.LowerBar.percent, 0.001);

			AssertEquals(Color.LightGray, linkedShapeWithZeroCompletedViewModel.LowerBar.background);
		}

		public void TestLowerBarBrush_ShouldConsiderEstimateToComplete()
		{
			AssertEquals(1.0 / 3.0, linkedShapeViewModel.LowerBar.percent, 0.001);

			var workflow1 = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "workflow1");
			var task2 = workflow1.Tasks.Single(t => t.P9_Sequence == 2);
			task2.P9_EstimatedTimeToComplete = new ZInt(30).GetDateTimeFromMinutes();

			AssertEquals(1.0 / 4.0, linkedShapeViewModel.LowerBar.percent, 0.001);

			AssertEquals(Color.LightGray, linkedShapeWithZeroCompletedViewModel.LowerBar.background);
		}

		public void TestLowerBarTooltip()
		{
			AssertNull(nonLinkedShapeViewModel.LowerBarTooltip);
			AssertNull(linkedShapeWithZeroEstimateViewModel.LowerBarTooltip);

			AssertEquals("0% complete (0 hours of 0.25 hours complete)", linkedShapeWithZeroCompletedViewModel.LowerBarTooltip);
			AssertEquals("25% complete (0.25 hours of 1 hour complete)", linkedSubDiagramShapeViewModel.LowerBarTooltip);
			AssertEquals("33% complete (0.25 hours of 0.75 hours complete)", linkedShapeViewModel.LowerBarTooltip);
		}

		#endregion

		#region Implementation

		NodeViewModel CreateViewModel(INetworkEntity entity)
		{
			return viewModelProvider.Create(entity, networkViewModel);
		}

		protected override void SetUp()
		{
			base.SetUp();

			network = new DummyNetwork();
			viewModelProvider = new JobNetworkNodeViewModelProvider();
			networkViewModel = new NetworkViewModel(network, nodeViewModelProvider: viewModelProvider);

			jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");

			BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 10, taskStatus: ProcessTaskStatusCodeList.Codes.Closed);
			BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 10);
			BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 10);

			BMSTestHelper.CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 10);

			var diagramShape = CreateDiagram(Factory);
			var jobNetwork = CreateNetwork(diagramShape);
			var diagram = jobNetwork.DiagramEntity;
			var linkedSubDiagramShape = CreateShape(jobHeader, diagram);

			var nonLinkedShape = CreateShape(diagram);
			var linkedShape = CreateShape(workflow1, linkedSubDiagramShape);
			var linkedShapeWithZeroEstimate = CreateShape(workflow2, linkedSubDiagramShape);
			var linkedShapeWithZeroCompleted = CreateShape(workflow3, linkedSubDiagramShape);

			nonLinkedShapeViewModel = CreateViewModel(nonLinkedShape);
			linkedShapeViewModel = CreateViewModel(linkedShape);
			linkedShapeWithZeroEstimateViewModel = CreateViewModel(linkedShapeWithZeroEstimate);
			linkedShapeWithZeroCompletedViewModel = CreateViewModel(linkedShapeWithZeroCompleted);
			linkedSubDiagramShapeViewModel = CreateViewModel(linkedSubDiagramShape);
		}

		ProcessJobHeader jobHeader;
		NodeViewModel nonLinkedShapeViewModel, linkedShapeViewModel, linkedShapeWithZeroEstimateViewModel, linkedShapeWithZeroCompletedViewModel, linkedSubDiagramShapeViewModel;

		INetwork network;
		NodeViewModelProvider viewModelProvider;
		NetworkViewModel networkViewModel;

		#endregion
	}
}
