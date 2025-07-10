using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(ApproveDiagramAction))]
	class ApproveDiagramActionTest : JobNetworkActionTestCase<ApproveDiagramAction>
	{
		#region Execute

		protected override void TestExecuteCore()
		{
			var resource1 = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var resource2 = CreateStaffInCurrentBranchDept("SAM", "Samwise the Brave");

			Factory.Save();

			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);
			var arrow = shape1.MakeVisiblePrerequisiteOf(shape2);

			network.SwitchToScaled();
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;

			AssertEquals(false, diagram.IsApproved);
			AssertEquals(false, shape1.IsApproved);
			AssertEquals(false, shape2.IsApproved);
			AssertEquals(false, arrow.IsApproved);

			AssertEquals(0, diagram.Logs.GetAllLogs().Count);

			var action = GetAction(networkViewModel);

			using (Env.SetTemporaryUserContext(resource1.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				action.ExecuteAfterActivatingEntity_ForTest(diagram);
				Factory.Save();
			}

			AssertEquals(true, diagram.IsApproved);
			AssertEquals(true, shape1.IsApproved);
			AssertEquals(true, shape2.IsApproved);
			AssertEquals(true, arrow.IsApproved);

			AssertEquals(resource1.GS_Code, diagram.BNS_GS_NKApprovedBy);
			AssertEquals(resource1.GS_Code, shape1.Shape.BNS_GS_NKApprovedBy);
			AssertEquals(resource1.GS_Code, shape2.Shape.BNS_GS_NKApprovedBy);
			AssertEquals(resource1.GS_Code, arrow.BNA_GS_NKApprovedBy);

			var log = diagram.Logs.GetAllLogs().Cast<StmALog>().Single(l => l.SL_Reference == "Diagram approved by [Frodo Baggins]");
			AssertNotNull(log);
		}

		public void TestExecute_ShouldUnApproveProcessHeaderAlso()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = CreateWorkflow(jobHeader, "workflow");
			var diagram = CreateDiagram(jobHeader);
			var shape = CreateShape(workflow, diagram);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			AssertNull(jobHeader.ApprovedShape);
			AssertNull(workflow.ApprovedShape);

			networkViewModel.ToggleApproval();
			AssertNotNull(jobHeader.ApprovedShape);
			AssertNotNull(workflow.ApprovedShape);

			networkViewModel.ToggleApproval();
			AssertNull(jobHeader.ApprovedShape);
			AssertNull(workflow.ApprovedShape);
		}

		public void TestExecute_WhenCircularDependencyExists_BetweenShapesWithinDiagram_ShouldDisplayError()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Nose", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Non-nose", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Nine", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);

			var diagram = NetworkTestCase.CreateDiagram(Factory, scheduledStartTimeUTC: ZDateTime.UtcNow);
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram, name: "Shápe 1");
			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram, name: "Shapé 2");
			var shape3 = NetworkTestCase.CreateShape(workflow3, diagram, name: "Shápé 3");

			var arrow1 = shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			var arrow2 = shape2.MakeVisiblePrerequisiteOf(shape3, diagram);

			var circularArrow = shape3.MakeVisiblePrerequisiteOf(shape1, diagram);

			var viewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = viewModel.GetJobNetwork();
			network.SwitchToScaled();

			NetworkTestCase.CreateBufferOnArrow(arrow1, viewModel, "Bifur");
			NetworkTestCase.CreateBufferOnArrow(arrow2, viewModel, "Bofur");

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedDiagram = newFactory.Load<BMNCNShape>(diagram.PK);
			var loadedArrow = newFactory.Load<BMNCNAttachment>(circularArrow.PK);

			viewModel = NetworkTestCase.CreateNetworkViewModel(loadedDiagram);
			network = viewModel.GetJobNetwork();

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			new ApproveDiagramAction(viewModel).ExecuteAfterActivatingEntity_ForTest(loadedDiagram);

			AssertConfirmationNotificationShown(NotificationTypes.Error,
@"Shapes on this diagram have errors. See error icons on the relevant shapes.
Record: This shape has a circular dependency");
			AssertEquals(false, loadedDiagram.IsApproved);

			UnitTestUserNotification.Instance.ClearMessages();

			loadedArrow.ProcessHeaderLink.Delete();
			loadedArrow.Delete();

			UnitTestUserNotification.Instance.AddOKAnswer(); // Bypass warnings shown.
			new ApproveDiagramAction(viewModel).ExecuteAfterActivatingEntity_ForTest(loadedDiagram);

			AssertConfirmationNotificationShown("Warning should have been shown, but approval can proceed nonetheless", NotificationTypes.Warning,
				"There are shapes which have no connection to a buffer. This will cause the entities linked to those shapes to not use the CCPM schedule when considering them for release to a buffer. Consider connecting these shapes to a downstream buffer. Shape: [Shápé 3]");
			AssertEquals("Now that there is no circular dependency, there should be nothing stopping the diagram being approved", true, loadedDiagram.IsApproved);
		}

		public void TestExecute_WhenCircularDependencyExists_BetweenWorkflowsOutsideDiagram_ShouldDisplayError()
		{
			BMSRegistry.Instance.AutomaticallyValidateWorkflowLoopsOnSave.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Non-nose", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Nine", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Nose", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);

			var diagram = NetworkTestCase.CreateDiagram(Factory, scheduledStartTimeUTC: ZDateTime.UtcNow);
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram);
			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram);
			var shape3 = NetworkTestCase.CreateShape(workflow3, diagram);

			shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			shape2.MakeVisiblePrerequisiteOf(shape3, diagram);

			var circularDependency = workflow3.GetOrCreateDependencyLink(workflow1);

			var viewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = viewModel.GetJobNetwork();
			network.SwitchToScaled();
			viewModel.SuggestAndAcceptAllBuffers();

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedDiagram = newFactory.Load<BMNCNShape>(diagram.PK);
			var loadedDependency = newFactory.Load<ProcessHeaderLink>(circularDependency.PK);

			viewModel = NetworkTestCase.CreateNetworkViewModel(loadedDiagram);
			network = viewModel.GetJobNetwork();

			new ApproveDiagramAction(viewModel).ExecuteAfterActivatingEntity_ForTest(loadedDiagram);

			AssertConfirmationNotificationShown(NotificationTypes.Error,
@"The following workflows are involved in a loop:
Organization (XVBQP68SIYXQ) - Nine
Organization (XVBQP68SIYXQ) - Non-nose
Organization (XVBQP68SIYXQ) - Nose"); // Partial assertion, because message can have any order.
			AssertEquals(false, loadedDiagram.IsApproved);

			UnitTestUserNotification.Instance.ClearMessages();

			loadedDependency.Delete();

			new ApproveDiagramAction(viewModel).ExecuteAfterActivatingEntity_ForTest(loadedDiagram);

			AssertNoConfirmationNotificationShown();
			AssertEquals("Now that there is no circular dependency, there should be nothing stopping the diagram being approved", true, loadedDiagram.IsApproved);
		}

		#endregion

		#region Entity Approval Restrictions

		public void TestExecute_AlreadyHasApprovedShapes()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);
			var shape3 = networkViewModel.CreateNewShape(diagram);

			network.SwitchToScaled();

			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;

			shape1.Name = "shape1";
			shape2.Name = "shape2";
			shape3.Name = "shape3";
			shape2.Approve(GlbStaff.CurrentUser.GS_Code);
			shape3.Approve(GlbStaff.CurrentUser.GS_Code);

			GetAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);

			AssertEquals(false, diagram.IsApproved);
			AssertMultilineASCIIEquals("", @"This diagram contains approved shapes and cannot be approved. The following shapes are approved on this diagram:
	shape2
	shape3", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestExecute_AlreadyHasApprovedShapesOnOtherDiagrams()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var diagram1 = CreateDiagram(jobHeader, name: "diagram1");
			var workflowShape1_1 = CreateShape(workflow1, diagram1, "workflowShape1_1");
			var workflowShape1_2 = CreateShape(workflow2, diagram1, "workflowShape1_2");
			var workflowShape1_3 = CreateShape(workflow3, diagram1, "workflowShape1_3");

			var networkViewModel1 = CreateNetworkViewModel(diagram1);
			var network1 = networkViewModel1.GetJobNetwork();

			var diagram2 = CreateDiagram(jobHeader, name: "diagram2");
			var workflowShape2_1 = CreateShape(workflow1, diagram2, "workflowShape2_1");
			var workflowShape2_2 = CreateShape(workflow2, diagram2, "workflowShape2_2");
			var workflowShape2_3 = CreateShape(workflow3, diagram2, "workflowShape2_3");

			var networkViewModel2 = CreateNetworkViewModel(diagram2);

			((IApprovable)workflowShape2_2).Approve(GlbStaff.CurrentUser.GS_Code);
			((IApprovable)workflowShape2_3).Approve(GlbStaff.CurrentUser.GS_Code);

			network1.SwitchToScaled();
			diagram1.ScheduledStartTimeUtc = ZDateTime.UtcNow;

			GetAction(networkViewModel1).ExecuteAfterActivatingEntity_ForTest(diagram1);

			AssertEquals(false, diagram1.IsApproved);
			AssertMultilineASCIIEquals("", @"This diagram contains shapes that are approved on other diagrams, and cannot be approved. The following shapes are approved on other diagrams:
	workflowShape1_2
	workflowShape1_3", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestExecute_HiddenChildProcessHeadersOfShapesOnTheDiagram()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var parentJobHeader1 = CreateJobHeader<OrgHeader>();
			var parentJobHeader2 = CreateJobHeader<OrgHeader>();

			var childJobHeader1 = CreateJobHeader<OrgHeader>();
			childJobHeader1.FH_CompletionStatement = "Dat Job 1";
			var childJobHeader2 = CreateJobHeader<OrgHeader>();
			childJobHeader2.FH_CompletionStatement = "Dat Job 2";
			var grandchildJobHeader = CreateJobHeader<OrgHeader>();
			grandchildJobHeader.FH_CompletionStatement = "Dat Other Job";

			VisualBoardsTestHelper.MakeChildOf(childJobHeader1, parentJobHeader1);
			VisualBoardsTestHelper.MakeChildOf(childJobHeader2, parentJobHeader2);
			VisualBoardsTestHelper.MakeChildOf(grandchildJobHeader, childJobHeader1);

			var diagram = CreateDiagram(Factory, name: "diagram");
			var subDiagram1 = CreateShape(parentJobHeader1, diagram, "subDiagram1");
			var subDiagram2 = CreateShape(parentJobHeader2, diagram, "subDiagram2");

			var diagramForChildJob1 = CreateDiagram(childJobHeader1, name: "diagramForChildJob1");
			((IApprovable)diagramForChildJob1).Approve(GlbStaff.CurrentUser.GS_Code);
			var nonApprovedChild1 = CreateShape(diagramForChildJob1);

			var diagramForChildJob2 = CreateDiagram(childJobHeader2, name: "diagramForChildJob2");
			((IApprovable)diagramForChildJob2).Approve(GlbStaff.CurrentUser.GS_Code);
			var nonApprovedChild2 = CreateShape(diagramForChildJob2);

			var diagramForGrandchildJob = CreateDiagram(grandchildJobHeader, name: "diagramForGrandchildJob");
			((IApprovable)diagramForGrandchildJob).Approve(GlbStaff.CurrentUser.GS_Code);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;

			var action = GetAction(networkViewModel);
			action.ExecuteAfterActivatingEntity_ForTest(diagram);

			AssertEquals(false, diagram.IsApproved);
			AssertMultilineASCIIEquals("", @"This diagram contains shapes with non-visible child entities that are approved on other diagrams, and so cannot be approved. The following shapes have non-visible entities approved on other diagrams:
	subDiagram1
		Approved shape: [diagramForChildJob1] for entity: [Dat Job 1]
		Approved shape: [diagramForGrandchildJob] for entity: [Dat Other Job]

	subDiagram2
		Approved shape: [diagramForChildJob2] for entity: [Dat Job 2]", UnitTestUserNotification.Instance.LastMessage.Text);

			diagramForChildJob1.AsEntity(network).Approve(ZString.Empty);
			diagramForChildJob2.AsEntity(network).Approve(ZString.Empty);
			diagramForGrandchildJob.AsEntity(network).Approve(ZString.Empty);

			action.ExecuteAfterActivatingEntity_ForTest(diagram);
			AssertEquals(true, diagram.IsApproved);
		}

		public void TestExecute_HiddenChildProcessHeadersOfDiagramShape()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			jobHeader2.FH_CompletionStatement = "Dis Job";

			VisualBoardsTestHelper.MakeChildOf(jobHeader2, jobHeader1);

			var diagram1 = CreateDiagram(jobHeader1, name: "diagram1");
			var diagram2 = CreateDiagram(jobHeader1, name: "diagram2");
			((IApprovable)diagram2).Approve(GlbStaff.CurrentUser.GS_Code);

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram1);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			diagram1.ScheduledStartTimeUtc = ZDateTime.UtcNow;

			var action = GetAction(networkViewModel);
			action.ExecuteAfterActivatingEntity_ForTest(diagram1);

			AssertEquals(false, diagram1.IsApproved);
			AssertMultilineASCIIEquals("", @"This diagram contains shapes with non-visible child entities that are approved on other diagrams, and so cannot be approved. The following shapes have non-visible entities approved on other diagrams:
	diagram1
		Approved shape: [diagram2] for entity: [Job Organization (XVBQP68SIYXQ) is complete.]", UnitTestUserNotification.Instance.LastMessage.Text);

			diagram2.AsEntity(network).Approve(ZString.Empty);

			action.ExecuteAfterActivatingEntity_ForTest(diagram1);
			AssertEquals(true, diagram1.IsApproved);
		}

		#endregion

		#region Related Buffers

		[TestDate(2019, 11, 25)] //Monday
		public void TestWorkflowCurrentStatus_WhenInAprovedDiagram_ShouldUseDiagramDepartmentWorkingHoursToShowZone()
		{
			var operationalBufferBranch = Factory.NewWithValidTestData<GlbBranch>();
			operationalBufferBranch.GB_RL_NKHomePort = "AUMEL";
			var operationalBufferDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			operationalBufferDepartment.GE_Code = "OBD";
			var resource = BMSTestHelper.CreateStaff(Factory, "RES", "Resource", operationalBufferBranch, operationalBufferDepartment);

			WorkingDaysTestHelper.CreateWorkTime(Factory, operationalBufferDepartment.PK, GlbDepartmentSchema.Constants.Prefix,
					new Dictionary<DayOfWeek, string> {
						{ DayOfWeek.Sunday, string.Empty },
						{ DayOfWeek.Monday, "*" },
						{ DayOfWeek.Tuesday, string.Empty },
						{ DayOfWeek.Wednesday,string.Empty },
						{ DayOfWeek.Thursday, string.Empty },
						{ DayOfWeek.Friday, "*" },
						{ DayOfWeek.Saturday, string.Empty }
					}); // 1 hour per week

			var diagramBranch = Factory.NewWithValidTestData<GlbBranch>();
			diagramBranch.GB_RL_NKHomePort = "AUMEL";
			var diagramDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			WorkingDaysTestHelper.CreateWorkTime(Factory, diagramDepartment.PK, GlbDepartmentSchema.Constants.Prefix,
				new Dictionary<DayOfWeek, string> {
					{ DayOfWeek.Sunday, string.Empty },
					{ DayOfWeek.Monday, "********************" },
					{ DayOfWeek.Tuesday, "********************" },
					{ DayOfWeek.Wednesday, "********************" },
					{ DayOfWeek.Thursday, "********************" },
					{ DayOfWeek.Friday, "********************" },
					{ DayOfWeek.Saturday, string.Empty },
				}); // 50 hour per week

			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var operationalBuffer = BMSTestHelper.CreateBuffer(system, timespanMinutes: 600);

			operationalBuffer.FC_Name = "Operational Component Buffer";
			operationalBuffer.FC_GB_AgingBranch = operationalBufferBranch.PK;
			operationalBuffer.FC_GE_AgingDepartment = operationalBufferDepartment.PK;

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "workflow");
			BMSTestHelper.CreateTask(workflow, resource.GS_Code, 600, sequence: 10);
			workflow.FH_FC_CurrentComponent = operationalBuffer.PK;
			workflow.FH_ReleaseDateTime = ZDateTime.UtcNow.AddDays(-28);

			Factory.Save();

			AssertMultilineASCIIEquals("Workflow should be in zone 2 of Operational Component Buffer", "Operational Component Buffer - Zone 2", workflow.CurrentStatus);

			var diagramShape = CreateDiagram(jobHeader);
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var shape = CreateShape(workflow, diagram);

			network.SwitchToScaled();
			diagramShape.ScheduleBizo.BNC_GB_Branch = diagramBranch.PK;
			diagramShape.ScheduleBizo.BNC_GE_Department = diagramDepartment.PK;
			diagramShape.ScheduledStartTimeUtc = workflow.FH_ReleaseDateTime;
			diagram.Name = "diagram";
			shape.Name = "shape";

			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			Factory.Save();

			AssertNoConfirmationNotificationShown();
			AssertEquals(true, diagram.IsApproved);
			AssertMultilineASCIIEquals("Workflow should be in zone 0 of Project Buffer", "Operational Component Buffer - Zone 0 (Project Buffer)", workflow.CurrentStatus);
		}

		public void TestExecute_ShouldRecordRelatedBuffers()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();
			var workflow4 = jobHeader.ProcessHeaders.AddNew();

			var diagramShape = CreateDiagram(jobHeader);
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var shape1 = CreateShape(workflow1, diagram);
			var shape2 = CreateShape(workflow2, diagram);
			var shape3 = CreateShape(workflow3, diagram);
			var shape4 = CreateShape(workflow4, diagram);

			diagram.Name = "diagram";
			shape1.Name = "shape1";
			shape2.Name = "shape2";
			shape3.Name = "shape3";
			shape4.Name = "shape4";

			var arrow1_2 = shape1.MakeVisiblePrerequisiteOf(shape2);
			var arrow2_3 = shape2.MakeVisiblePrerequisiteOf(shape3);
			var arrow4_3 = shape4.MakeVisiblePrerequisiteOf(shape3);

			Factory.Save();

			network.SwitchToScaled();
			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow;
			network.Refresh(RefreshType.RedrawDiagram);

			AssertEquals(true, shape1.IsOnCriticalPath);
			AssertEquals(true, shape2.IsOnCriticalPath);
			AssertEquals(true, shape3.IsOnCriticalPath);
			AssertEquals(false, shape4.IsOnCriticalPath);

			AssertEquals(4, network.Entities.Count);

			networkViewModel.SuggestAndAcceptAllBuffers();

			AssertEquals(6, network.Entities.Count);
			var projectBuffer = network.Shapes.OfType<BMNCNBufferShape>().Single(s => s.Name.EndsWith("Project Buffer"));
			var feedingBuffer = network.Shapes.OfType<BMNCNBufferShape>().Single(s => s.Name.EndsWith("Feeding Buffer"));

			CombineAssertions(() =>
			{
				AssertRelatedBuffers<IBuffer>(shape1);
				AssertRelatedBuffers<IBuffer>(shape2);
				AssertRelatedBuffers<IBuffer>(shape3);
				AssertRelatedBuffers<IBuffer>(shape4);
				AssertRelatedBuffers<IBuffer>(projectBuffer);
				AssertRelatedBuffers<IBuffer>(feedingBuffer);
			});

			networkViewModel.ToggleApproval();

			CombineAssertions(() =>
			{
				AssertRelatedBuffers<IBuffer>(shape1, projectBuffer);
				AssertRelatedBuffers<IBuffer>(shape2, projectBuffer);
				AssertRelatedBuffers<IBuffer>(shape3, projectBuffer);
				AssertRelatedBuffers<IBuffer>(shape4, projectBuffer, feedingBuffer);
				AssertRelatedBuffers<IBuffer>(projectBuffer);
				AssertRelatedBuffers<IBuffer>(feedingBuffer);
			});

			networkViewModel.ToggleApproval();

			CombineAssertions(() =>
			{
				AssertRelatedBuffers<IBuffer>(shape1);
				AssertRelatedBuffers<IBuffer>(shape2);
				AssertRelatedBuffers<IBuffer>(shape3);
				AssertRelatedBuffers<IBuffer>(shape4);
				AssertRelatedBuffers<IBuffer>(projectBuffer);
				AssertRelatedBuffers<IBuffer>(feedingBuffer);
			});
		}

		#endregion

		#region Date Validation

		public void TestExecute_ShouldNotBePossibleWithoutADiagramDate_ADD()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagram = CreateDiagram(jobHeader);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			var action = GetAction(networkViewModel);

			action.ExecuteAfterActivatingEntity_ForTest(diagram);
			AssertEquals(false, diagram.IsApproved);
			AssertConfirmationNotificationShown(NotificationTypes.Error, "The diagram requires either a Scheduled Start Time or a Scheduled Finish Time. These can be set on the Properties form accessed from the designer surface.");

			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;

			action.ExecuteAfterActivatingEntity_ForTest(diagram);
			AssertEquals(true, diagram.IsApproved);
		}

		public void TestExecute_ShouldNotBePossibleWithoutADiagramDate_DNSB()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagram = CreateDiagram(jobHeader);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			var action = GetAction(networkViewModel);

			action.ExecuteAfterActivatingEntity_ForTest(diagram);
			AssertEquals(false, diagram.IsApproved);
			AssertConfirmationNotificationShown(NotificationTypes.Error, "The diagram requires either a Scheduled Start Time or a Scheduled Finish Time. These can be set on the Properties form accessed from the designer surface.");

			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;

			action.ExecuteAfterActivatingEntity_ForTest(diagram);
			AssertEquals(true, diagram.IsApproved);
		}

		public void TestExecute_ApprovedDiagramWithoutADiagramDate()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagram = CreateDiagram(jobHeader);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;

			var action = GetAction(networkViewModel);

			action.ExecuteAfterActivatingEntity_ForTest(diagram);
			AssertEquals(true, diagram.IsApproved);

			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.Empty;

			new UnapproveDiagramAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);
			AssertEquals(false, diagram.IsApproved);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestExecute_ShouldNotBePossibleWithoutBranchSet()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "BR1";
			branch.GB_RL_NKHomePort = "AUSYD";

			Factory.Save();

			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagram = CreateDiagram(jobHeader);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;
			diagram.ScheduleBizo.BNC_GB_Branch = new ZGuid(null);
			Assert(diagram.ScheduleBizo.BNC_GB_Branch.IsEmpty);

			var action = GetAction(networkViewModel);

			action.ExecuteAfterActivatingEntity_ForTest(diagram);
			AssertEquals(false, diagram.IsApproved);
			AssertConfirmationNotificationShown(NotificationTypes.Error, "The diagram requires a valid branch. This can be set on the Properties form accessed from the designer surface under 'Branch' .");
			UnitTestUserNotification.Instance.ClearMessages();

			diagram.ScheduleBizo.BNC_GB_Branch = branch.PK;

			action.ExecuteAfterActivatingEntity_ForTest(diagram);
			AssertEquals(true, diagram.IsApproved);
			AssertNoConfirmationNotificationShown();
		}

		public void TestExecute_ShouldNotBePossibleWithoutDepartmentSet()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagram = CreateDiagram(jobHeader);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;
			diagram.ScheduleBizo.BNC_GE_Department = new ZGuid(null);
			Assert(diagram.ScheduleBizo.BNC_GE_Department.IsEmpty);

			var action = GetAction(networkViewModel);

			action.ExecuteAfterActivatingEntity_ForTest(diagram);
			AssertEquals(false, diagram.IsApproved);
			AssertConfirmationNotificationShown(NotificationTypes.Error, "The diagram requires a valid department. This can be set on the Properties form accessed from the designer surface under 'Department'.");
			UnitTestUserNotification.Instance.ClearMessages();

			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = "DP1";
			diagram.ScheduleBizo.BNC_GE_Department = department1.PK;
			Factory.Save();

			action.ExecuteAfterActivatingEntity_ForTest(diagram);
			AssertEquals(true, diagram.IsApproved);
			AssertNoConfirmationNotificationShown();
		}

		#endregion

		#region Shape Validation

		public void TestExecute_WhenShapeHasValidationWarning_ShouldDisplayNotificationConfirmation()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var shape1 = NetworkTestCase.CreateShape(diagram);
			var shape2 = NetworkTestCase.CreateShape(diagram);
			var arrow = shape1.MakeVisiblePrerequisiteOf(shape2);

			network.FullRefresh();

			shape2.X -= 100; // Force a back-in-time validation warning.

			arrow.ClearAllNotifications();

			networkViewModel.ToggleApproval(ensureIsNowApproved: false);

			AssertHasRowWarning("Attempting to approve should perform expensive validation", network.Entities.GetInstance(arrow), "The post-requisite is scheduled to begin before its pre-requisite is scheduled to complete.");
			AssertEquals(false, diagram.IsApproved);

			AssertConfirmationNotificationShown("It should still be possible to approve a diagram in this state (hence only a warning)",
				NotificationTypes.Warning, @"Shapes on this diagram have warnings. See warning icons on the relevant shapes.
Shape Connection: The post-requisite is scheduled to begin before its pre-requisite is scheduled to complete.");
		}

		public void TestExecute_WhenShapeHasValidationError_ShouldDisplayNotificationConfirmation()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var shape1 = NetworkTestCase.CreateShape(diagram);
			var shape2 = NetworkTestCase.CreateShape(diagram);
			var arrow = shape1.MakeVisiblePrerequisiteOf(shape2);

			network.FullRefresh();

			new SuggestBufferAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);

			networkViewModel.ToggleApproval(ensureIsNowApproved: false);

			AssertConfirmationNotificationShown("Error should prevent approving the diagram",
				NotificationTypes.Error, @"Shapes on this diagram have errors. See error icons on the relevant shapes.
[Project Buffer] Active: This buffer has not been accepted yet. Please accept the buffer or hide it from the diagram.");
		}

		public void TestExecute_WhenShapeHasNoArrowToBuffer_ShouldDisplayNotificationConfirmation()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var shape1 = NetworkTestCase.CreateShape(diagram, "shape1");
			var shape2 = NetworkTestCase.CreateShape(diagram, "shape2");
			var shape3 = NetworkTestCase.CreateShape(diagram, "shape3");
			var arrow = shape1.MakeVisiblePrerequisiteOf(shape2);

			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval(ensureIsNowApproved: false);

			AssertConfirmationNotificationShown("It should still be possible to approve a diagram in this state (hence only a warning)",
				NotificationTypes.Warning, "There are shapes which have no connection to a buffer. This will cause the entities linked to those shapes to not use the CCPM schedule when considering them for release to a buffer. Consider connecting these shapes to a downstream buffer. Shape: [shape3]");
			AssertEquals(false, diagram.IsApproved);
		}

		public void TestExecute_WhenMultipleShapesHaveNoArrowToBuffer_ShouldDisplayNotificationConfirmation()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var shape1 = NetworkTestCase.CreateShape(diagram, "shape1");
			var shape2 = NetworkTestCase.CreateShape(diagram, "shape2");
			var shape3 = NetworkTestCase.CreateShape(diagram, "shape3");
			var shape4 = NetworkTestCase.CreateShape(diagram, "shape4");
			var arrow = shape1.MakeVisiblePrerequisiteOf(shape2);

			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval(ensureIsNowApproved: false);

			AssertConfirmationNotificationShown("It should still be possible to approve a diagram in this state (hence only a warning)",
				NotificationTypes.Warning, "There are shapes which have no connection to a buffer. This will cause the entities linked to those shapes to not use the CCPM schedule when considering them for release to a buffer. Consider connecting these shapes to a downstream buffer. Shapes: [shape3], [shape4]");
			AssertEquals(false, diagram.IsApproved);
		}

		public void TestExecute_WhenAllShapesHaveArrowToBuffer_ShouldNotDisplayNotificationConfirmation()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var shape1 = NetworkTestCase.CreateShape(diagram, "shape1");
			var shape2 = NetworkTestCase.CreateShape(diagram, "shape2");
			var shape3 = NetworkTestCase.CreateShape(diagram, "shape3");
			shape1.MakeVisiblePrerequisiteOf(shape2);
			shape2.MakeVisiblePrerequisiteOf(shape3);

			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			AssertNoConfirmationNotificationShown();
			AssertEquals(true, diagram.IsApproved);
		}

		public void TestExecute_WhenShapeHasIndirectArrowToBufferViaParentShape_ShouldDisplayNotificationConfirmation()
		{
			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var subDiagram = NetworkTestCase.CreateShape(diagram, "subDiagram");
			var subSubDiagram = NetworkTestCase.CreateShape(subDiagram, "subSubDiagram");
			var shape1 = NetworkTestCase.CreateShape(subSubDiagram, "shape1");
			var shape2 = NetworkTestCase.CreateShape(diagram, "shape2");
			var arrow = subDiagram.MakeVisiblePrerequisiteOf(shape2);

			network.SwitchToScaled();
			networkViewModel.PushAsLateAsPossible();
			networkViewModel.SuggestAndAcceptAllBuffers();

			network.Shapes.OfType<BMNCNBufferShape>().Single(b => b.Type == BufferType.Feeding).Delete(); // There isn't really a CC, this is just silly behaviour by the Suggest Buffers function.
			networkViewModel.ToggleApproval();

			AssertNoConfirmationNotificationShown("There are shapes with no direct post-requisites, however their parents are connected to the project buffer via their parents.");
			AssertEquals(true, diagram.IsApproved);
		}

		#endregion

		#region Annotations

		public void TestApprove_WhenAnnotationsExist_ShouldBeAbleToDeleteAnnotations()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var annotation = networkViewModel.CreateNewAnnotation(diagram);

			network.SwitchToScaled();
			networkViewModel.ToggleApproval();

			AssertEquals(false, annotation.IsApproved);

			annotation.Delete();

			AssertEquals(true, annotation.IsDeleted);
		}

		#endregion

		#region Progress Notification

		public void TestExecute_ShouldNotifyProgressAtEachEntity()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape1 = NetworkTestCase.CreateShape(diagram);
			var shape2 = NetworkTestCase.CreateShape(diagram);

			var reporter = new DummyProgressReporter();
			var reporterProvider = new Mock<IProgressReporterProvider>(MockBehavior.Strict);

			reporterProvider.Setup(m => m.CreateProgressReporter("Validating diagram for approval", 4, It.IsAny<bool>())).Returns(reporter);
			var networkViewModel = CreateNetworkViewModel(diagram, progressReporterProvider: reporterProvider.Object);
			var network = networkViewModel.GetJobNetwork();

			network.SwitchToScaled();
			networkViewModel.ToggleApproval();

			AssertEquals(4, reporter.ItemsProcessed);
			AssertEquals(true, reporter.IsDisposed);
			AssertEquals(true, diagram.IsApproved);
		}

		public void TestExecute_WhenProgressIsCancelled_ShouldNotApprove()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape1 = NetworkTestCase.CreateShape(diagram);
			var shape2 = NetworkTestCase.CreateShape(diagram);

			var reporter = new DummyProgressReporter { CancelAfterItemNumber = 1 };
			var reporterProvider = new Mock<IProgressReporterProvider>(MockBehavior.Strict);

			reporterProvider.Setup(m => m.CreateProgressReporter("Validating diagram for approval", 4, It.IsAny<bool>())).Returns(reporter);
			var networkViewModel = CreateNetworkViewModel(diagram, progressReporterProvider: reporterProvider.Object);
			var network = networkViewModel.GetJobNetwork();

			network.SwitchToScaled();
			networkViewModel.ToggleApproval();

			AssertEquals(1, reporter.ItemsProcessed);
			AssertEquals(true, reporter.IsDisposed);
			AssertEquals(false, diagram.IsApproved);
		}

		#endregion

		#region NonScheduled Items

		public void TestApproveDiagram_ShouldNotApproveNonScheduledShapesAndArrows()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var scheduledShape1 = CreateShape(diagram);
			var scheduledShape2 = CreateShape(diagram);
			var scheduledArrow = scheduledShape1.MakeVisiblePrerequisiteOf(scheduledShape2, diagram);

			var nonScheduledShape1 = CreateShape(diagram);
			nonScheduledShape1.IsNonScheduled = true;
			var nonScheduledShape2 = CreateShape(diagram);
			nonScheduledShape2.IsNonScheduled = true;
			var nonScheduledArrow = nonScheduledShape1.MakeVisiblePrerequisiteOf(nonScheduledShape2, diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);

			AssertEquals(false, scheduledShape1.IsApproved);
			AssertEquals(false, scheduledShape2.IsApproved);
			AssertEquals(false, scheduledArrow.IsApproved);
			AssertEquals(false, nonScheduledShape1.IsApproved);
			AssertEquals(false, nonScheduledShape2.IsApproved);
			AssertEquals(false, nonScheduledArrow.IsApproved);

			networkViewModel.ToggleApproval();

			CombineAssertions("Nonscheduled items should not be approved with the Approve Diagram action. SAD!", () =>
			{
				AssertEquals(true, scheduledShape1.IsApproved);
				AssertEquals(true, scheduledShape2.IsApproved);
				AssertEquals(true, scheduledArrow.IsApproved);
				AssertEquals("nonScheduledShape1", false, nonScheduledShape1.IsApproved);
				AssertEquals("nonScheduledShape2", false, nonScheduledShape2.IsApproved);
				AssertEquals("nonScheduledArrow", false, nonScheduledArrow.IsApproved);
			});
		}

		#endregion

		#region JobNetworkActionTestCase Overrides

		[TestDate(2019, 06, 01)]
		public void TestApproveButton_GetsDisabled_WhenDiagramIsApproved_BeforeSaving()
		{
			var diagram = Factory.New<BMNCNShape>();
			diagram.SwitchToScaled();
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var shape = networkViewModel.CreateNewShape(diagram);
			Factory.Save();

			var action = GetAction(networkViewModel);

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, diagram))
			{
				NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabled());
				action.Execute();
				Assert(shape.IsApproved);
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The shape should not be approved.", action.IsEnabled());
			}
		}

		protected override void TestIsApplicableCore()
		{
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new string[] {
				"This action is accessible to the root diagram only.",
				"Cannot execute when the diagram is not in scaled mode." }, action.IsApplicableAfterActivatingEntity_ForTest(childShape));

			AssertEquals("Precondition: not scaled", false, diagram.IsScaled);
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Cannot execute when the diagram is not in scaled mode.", action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			diagram.SwitchToScaled();

			AssertEquals("Precondition: not approved", false, diagram.IsApproved);
			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			networkViewModel.ToggleApproval();
			AssertEquals("Precondition: approved", true, diagram.IsApproved);
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The shape should not be approved.", action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			AssertCannotExecuteInWorkflowRelationshipDesigner();
		}

		protected override void TestIsEnabledCore()
		{
			var diagram = CreateDiagram(Factory);
			diagram.SwitchToScaled();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			AssertEquals("Precondition: not approved", false, diagram.IsApproved);
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledAfterActivatingEntity_ForTest(diagram));
		}

		protected override void TestGetNameCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Approve Diagram", GetAction(CreateNetworkViewModel(diagram)).GetName());
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Marks this diagram as approved, causing its shapes to be released according to CCPM or NCN scheduling rules", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("Approve");
		}

		protected override ApproveDiagramAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new ApproveDiagramAction(networkViewModel);
		}

		#endregion
	}
}
