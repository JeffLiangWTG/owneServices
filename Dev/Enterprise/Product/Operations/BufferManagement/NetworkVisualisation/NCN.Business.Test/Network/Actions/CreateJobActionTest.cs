using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(CreateJobAction))]
	class CreateJobActionTest : JobNetworkActionTestCase<CreateJobAction>
	{
		public void TestCreateJob_WhenInDifferentFactoryFromNetwork_ShouldConnectDependenciesCorrectly()
		{
			CreateSystem("ORG");
			var jobHeader1 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "Diagram job");
			var jobHeader2 = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, false, "Sub-Diagram job");
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader1, "Shape workflow");

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(diagram);
			var shape = CreateShape(workflow, diagram);

			subDiagram.BNS_JobType = "ORG";

			subDiagram.MakeVisiblePrerequisiteOf(shape, diagram);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedJobHeader2 = newFactory.Load<ProcessJobHeader>(jobHeader2.PK);

			var controller = CreateMockableController(Mocks);
			controller.Setup(m => m.ShowNewFormAsDialogAndGetSaved()).Returns(loadedJobHeader2);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			AssertIsNotPrerequisite(jobHeader2, workflow);

			new CreateJobAction(networkViewModel, linkOnly: true).ExecuteAfterActivatingEntity_ForTest(subDiagram);

			AssertEquals(jobHeader2, subDiagram.ProcessHeader);
			AssertNoExceptionThrown(Factory.Save);
			AssertIsPrerequisite(jobHeader2, workflow);
		}

		public void TestCreateJobActionLinksAreReal()
		{
			CreateSystem("WKI");
			var workflow1 = CreateWorkflow(CreateJobHeader<OrgHeader>(), "Just for the shape");
			var diagram1 = CreateDiagram(Factory);

			var shape = CreateShape(workflow1, diagram1);
			var subDiagram1 = CreateShape(diagram1);
			subDiagram1.BNS_JobType = "WKI";

			var jobHeader2 = ProcessJobHeader.GetForParent((IWorkflowProvider)Factory.New<IWorkItem>(), Factory, false);
			var subDiagram2 = CreateShape(jobHeader2, diagram1);
			subDiagram2.BNS_JobType = "WKI";

			shape.MakeVisiblePrerequisiteOf(subDiagram1, diagram1);
			subDiagram1.MakeVisiblePrerequisiteOf(subDiagram2, diagram1);

			var jobHeader1 = ProcessJobHeader.GetForParent((IWorkflowProvider)Factory.New<IWorkItem>(), Factory, false);

			var controller = CreateMockableController(Mocks);
			controller.Setup(m => m.CreateJob(It.IsAny<string>(), It.IsAny<BusinessObjectFactory>(), It.IsAny<string>()))
				.Returns(jobHeader1);
			controller.Setup(m => m.ShowNewFormAsDialogAndGetSaved()).Returns(jobHeader1);
			controller.Setup(m => m.EditEntity(It.IsAny<IProposedNetworkEntity>()))
				.Callback((IProposedNetworkEntity entity) =>
				{
					entity.AsShape().BNS_JobType = "WKI";
				}).Verifiable();

			var networkViewModel = CreateNetworkViewModel(diagram1, controller: controller.Object);

			AssertNull(subDiagram1.ProcessHeader);
			AssertEquals(jobHeader2, subDiagram2.ProcessHeader);

			var action = new CreateJobAction(networkViewModel, linkOnly: true);
			action.ExecuteAfterActivatingEntity_ForTest(subDiagram1);

			Factory.Save();

			AssertEquals(jobHeader1, subDiagram1.ProcessHeader);

			AssertCollectionContains(subDiagram1.ProcessHeader, subDiagram1.ProcessHeader.PrerequisiteLinks.Select(p => p.HeaderTo));
			AssertCollectionContains(shape.ProcessHeader, subDiagram1.ProcessHeader.PrerequisiteLinks.Select(p => p.HeaderFrom));
			AssertCollectionContains(subDiagram2.ProcessHeader, subDiagram1.ProcessHeader.PostrequisiteLinks.Select(p => p.HeaderTo));
			AssertEquals(1, Factory.GetDatabaseCount(typeof(ProcessHeader), new ZQuery(ProcessHeaderSchema.FH_ParentId, jobHeader1.Parent.PK)));
			AssertEquals(1, Factory.GetDatabaseCount(typeof(ProcessHeader), new ZQuery(ProcessHeaderSchema.FH_ParentId, jobHeader2.Parent.PK)));
		}

		public void TestDontDeleteOldLinks()
		{
			CreateSystem("WKI");
			var diagram = CreateDiagram(Factory);
			var diagram1 = CreateShape(diagram);
			diagram1.BNS_JobType = "WKI";

			var diagram2 = CreateShape(ProcessJobHeader.GetForParent((IWorkflowProvider)Factory.New<IWorkItem>(), Factory, false), diagram);
			diagram2.MakeVisiblePrerequisiteOf(diagram1, diagram);

			var jobHeader1 = ProcessJobHeader.GetForParent((IWorkflowProvider)Factory.New<IWorkItem>(), Factory, false);

			var controller = CreateMockableController(Mocks);
			controller.Setup(m => m.CreateJob(It.IsAny<string>(), It.IsAny<BusinessObjectFactory>(), It.IsAny<string>()))
				.Returns(jobHeader1);
			controller.Setup(m => m.ShowNewFormAsDialogAndGetSaved()).Returns(jobHeader1);
			controller.Setup(m => m.EditEntity(It.IsAny<IProposedNetworkEntity>()))
				.Callback((IProposedNetworkEntity entity) =>
				{
					entity.AsShape().BNS_JobType = "ORG";
				}).Verifiable();

			var networkViewModel = CreateNetworkViewModel(diagram1, controller: controller.Object);

			var action = new CreateJobAction(networkViewModel, linkOnly: true);
			action.ExecuteAfterActivatingEntity_ForTest(diagram1);

			var jobHeader = (ProcessJobHeader)diagram1.ProcessHeader;
			AssertNotNull(jobHeader);
			AssertNotNull(jobHeader.Parent);

			AssertCollectionContains(diagram1, diagram2.Postrequisites(new BMNCNShapeDescendantsStrategy()));
		}

		public void TestAlwaysCreatesJobHeader()
		{
			CreateSystem("WKI");
			var diagram = CreateDiagram(Factory);

			var diagram1 = CreateShape(diagram);
			diagram1.BNS_JobType = "WKI";

			var diagram2 = CreateShape(ProcessJobHeader.GetForParent((IWorkflowProvider)Factory.New<IWorkItem>(), Factory, false), diagram);
			diagram2.MakeVisiblePrerequisiteOf(diagram1, diagram);

			var workItem = Factory.New<IWorkItem>();

			var controller = CreateMockableController(Mocks);

			controller
				.Setup(m => m.CreateJob(It.IsAny<string>(), It.IsAny<BusinessObjectFactory>(), It.IsAny<string>()))
				.Returns(ProcessJobHeader.GetForParent((IWorkflowProvider)workItem, Factory));

			controller.Setup(m => m.ShowNewFormAsDialogAndGetSaved()).Returns(ProcessJobHeader.GetForParent((IWorkflowProvider)workItem, Factory));

			controller.Setup(m => m.EditEntity(It.IsAny<IProposedNetworkEntity>()))
				.Callback((IProposedNetworkEntity entity) =>
				{
					entity.AsShape().BNS_JobType = "WKI";
				}).Verifiable();

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);

			var action = new CreateJobAction(networkViewModel);
			action.ExecuteAfterActivatingEntity_ForTest(diagram);

			var newShape = diagram.ChildShapes.First(s => s.ProcessHeader == ProcessJobHeader.GetForParent((IWorkflowProvider)workItem, Factory));
			var jobHeader2 = ProcessJobHeader.GetForParentWithoutCreation((IWorkflowProvider)workItem, Factory);
			AssertNotNull(jobHeader2);
			AssertNotNull(newShape.ProcessHeader);
			AssertEquals(jobHeader2, newShape.ProcessHeader);
		}

		public void TestShouldBeDisabled_WhenJobTypesAreNotAvailable()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			AssertEquals(0, diagram.Lookups.JobTypes.Count);

			var viewModel = CreateNetworkViewModel(diagram);

			var action = new CreateJobAction(viewModel);

			var isAllowed = action.IsApplicableToEntity(diagram);
			AssertEquals(true, isAllowed.IsAllowed);

			var enabledResult = action.IsEnabledForEntity(diagram);
			AssertEquals(false, enabledResult.IsAllowed);
			AssertContainsExactElementsInAnyOrder(new[] { "At least one job type must be associated with a Buffer Management System." }, enabledResult.DenialReasons.Select(x => x.Explanation));
		}

		protected override void TestExecuteCore()
		{
			CreateSystem("ORG");
			var diagram = Factory.New<BMNCNShape>();

			var controller = CreateMockableController(Mocks);
			controller
				.Setup(m => m.CreateJob(It.IsAny<string>(), It.IsAny<BusinessObjectFactory>(), It.IsAny<string>()))
				.Returns(ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory));
			controller.Setup(m => m.ShowNewFormAsDialogAndGetSaved()).Returns(ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory));

			controller.Setup(m => m.EditEntity(It.IsAny<IProposedNetworkEntity>()))
				.Callback((IProposedNetworkEntity entity) =>
				{
					entity.AsShape().BNS_JobType = "ORG";
				}).Verifiable();

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);

			var refreshes = new List<RefreshType>();
			networkViewModel.GetJobNetwork().Refreshed += (s, e) => refreshes.Add(e.RefreshType);

			Factory.Save();

			var action = new CreateJobAction(networkViewModel);
			action.ExecuteAfterActivatingEntity_ForTest(diagram);

			AssertArrayEqualsByElements(new[] { RefreshType.EntitiesReloaded, RefreshType.EntityEdited, RefreshType.EntitiesReloaded, RefreshType.RedrawDiagram }, refreshes.ToArray());

			AssertEquals(1, diagram.ChildShapes.Count);

			var shape = diagram.ChildShapes.Single();
			var processHeader = shape.ProcessHeader;
			AssertNotNull(processHeader);
			AssertType<ProcessJobHeader>(processHeader);
		}

		[GuiTest]
		public void TestExecute_NoJobTypeSpecified()
		{
			CreateSystem("ORG");
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);

			Factory.Save();
			var refreshCount = 0;
			networkViewModel.GetJobNetwork().Refreshed += (s, e) => refreshCount++;

			var action = new CreateJobAction(networkViewModel);
			action.ExecuteAfterActivatingEntity_ForTest(diagram);

			AssertEquals("Cannot create a job without a Job Type specified.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(4, refreshCount);

			var jobHeader = (ProcessJobHeader)diagram.ProcessHeader;
			AssertNull(jobHeader);
		}

		protected override void TestIsApplicableCore()
		{
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);
			var buffer = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Buffer);
			var annotation = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Annotation);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(diagram));
			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(childShape));

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("buffer", new string[] {
					"The shape should not be a buffer or annotation." }, action.IsApplicableAfterActivatingEntity_ForTest(buffer));

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("annotation", new string[] {
					"The shape should not be a buffer or annotation." }, action.IsApplicableAfterActivatingEntity_ForTest(annotation));

			AssertCannotExecuteInWorkflowRelationshipDesigner();
		}

		protected override void TestIsEnabledCore()
		{
			var system = CreateSystem("ORG");
			var jobHeader = CreateJobHeader<OrgHeader>();
			var linkedDiagram = CreateDiagram(jobHeader);
			var networkViewModel1 = CreateNetworkViewModel(linkedDiagram);

			var linkedWorkflowShape = networkViewModel1.CreateNewWorkflow(linkedDiagram);

			var unLinkedDiagram = Factory.New<BMNCNShape>();
			var networkViewModel2 = CreateNetworkViewModel(unLinkedDiagram);

			var unLinkedWorkflowShape = networkViewModel2.CreateNewShape(unLinkedDiagram);

			AssertEquals(true, GetAction(networkViewModel1).IsEnabledAfterActivatingEntity_ForTest(linkedDiagram).IsAllowed);
			AssertEquals(false, GetAction(networkViewModel1).IsEnabledAfterActivatingEntity_ForTest(linkedWorkflowShape).IsAllowed);

			AssertEquals(true, GetAction(networkViewModel2).IsEnabledAfterActivatingEntity_ForTest(unLinkedDiagram).IsAllowed);
			AssertEquals(true, GetAction(networkViewModel2).IsEnabledAfterActivatingEntity_ForTest(unLinkedWorkflowShape).IsAllowed);
		}

		protected override void TestGetNameCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Create Job", GetAction(CreateNetworkViewModel(diagram)).GetName());
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Creates and opens a new job", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon(string.Empty);
		}

		protected override CreateJobAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new CreateJobAction(networkViewModel);
		}
	}
}
