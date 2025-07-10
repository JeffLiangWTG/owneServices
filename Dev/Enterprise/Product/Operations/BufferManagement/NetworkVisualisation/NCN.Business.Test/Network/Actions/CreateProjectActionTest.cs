using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(CreateProjectAction))]
	class CreateProjectActionTest : JobNetworkActionTestCase<CreateProjectAction>
	{
		protected override CreateProjectAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new CreateProjectAction(networkViewModel);
		}

		protected override void TestExecuteCore()
		{
			CreateSystem("WKP");
			var diagram = Factory.New<BMNCNShape>();

			var controller = CreateMockableController(Mocks);
			controller
				.Setup(m => m.CreateJob(It.IsAny<string>(), It.IsAny<BusinessObjectFactory>(), It.IsAny<string>()))
				.Returns(ProcessJobHeader.GetForParent(BMSTestHelper.CreateJob<IProject>(Factory), Factory));

			controller
				.Setup(m => m.ShowNewFormAsDialogAndGetSaved())
				.Returns(ProcessJobHeader.GetForParent(BMSTestHelper.CreateJob<IProject>(Factory), Factory));

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);

			var refreshes = new List<RefreshType>();
			networkViewModel.GetJobNetwork().Refreshed += (s, e) => refreshes.Add(e.RefreshType);

			Factory.Save();

			var action = new CreateProjectAction(networkViewModel);
			action.ExecuteAfterActivatingEntity_ForTest(diagram);

			AssertArrayEqualsByElements(new[] { RefreshType.EntitiesReloaded, RefreshType.EntityEdited, RefreshType.EntitiesReloaded, RefreshType.RedrawDiagram }, refreshes.ToArray());

			var newShape = diagram.ChildShapes.Single();
			var jobHeader = (ProcessJobHeader)newShape.ProcessHeader;
			AssertNotNull(jobHeader);
			AssertEquals("WKP", newShape.BNS_JobType);
			AssertType<Project>(jobHeader.Parent);
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Creates and opens a new project", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon(string.Empty);
		}

		protected override void TestGetNameCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Create Project", GetAction(CreateNetworkViewModel(diagram)).GetName());
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
		}

		protected override void TestIsEnabledCore()
		{
			CreateSystem(new string[] { "ORG", "WKP" });

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow = CreateWorkflow(jobHeader, "Completed or something");

			var linkedDiagram = CreateDiagram(jobHeader);
			var linkedWorkflowShape = CreateShape(workflow, linkedDiagram);

			var networkViewModel1 = CreateNetworkViewModel(linkedDiagram);

			var unLinkedDiagram = CreateDiagram(Factory);
			var unLinkedWorkflowShape = CreateShape(unLinkedDiagram);

			var networkViewModel2 = CreateNetworkViewModel(unLinkedDiagram);

			AssertEquals(true, GetAction(networkViewModel1).IsEnabledAfterActivatingEntity_ForTest(linkedDiagram).IsAllowed);
			AssertEquals(true, GetAction(networkViewModel2).IsEnabledAfterActivatingEntity_ForTest(unLinkedDiagram).IsAllowed);
			AssertEquals(true, GetAction(networkViewModel2).IsEnabledAfterActivatingEntity_ForTest(unLinkedWorkflowShape).IsAllowed);
		}

		public void TestIsEnabled_NoWorkItemInAllowedWorkflowTypes()
		{
			var unLinkedDiagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(unLinkedDiagram);

			var unLinkedWorkflowShape = networkViewModel.CreateNewShape(unLinkedDiagram);

			AssertEquals(false, GetAction(networkViewModel).IsEnabledAfterActivatingEntity_ForTest(unLinkedDiagram).IsAllowed);
			AssertEquals(false, GetAction(networkViewModel).IsEnabledAfterActivatingEntity_ForTest(unLinkedWorkflowShape).IsAllowed);
		}
	}
}
