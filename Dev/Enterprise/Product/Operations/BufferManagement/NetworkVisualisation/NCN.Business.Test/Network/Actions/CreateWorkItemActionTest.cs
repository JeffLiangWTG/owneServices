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
	[TestedType(typeof(CreateWorkItemAction))]
	class CreateWorkItemActionTest : JobNetworkActionTestCase<CreateWorkItemAction>
	{
		protected override CreateWorkItemAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new CreateWorkItemAction(networkViewModel);
		}

		protected override void TestExecuteCore()
		{
			CreateSystem("WKI");
			var diagram = Factory.New<BMNCNShape>();

			var controller = CreateMockableController(Mocks);
			controller
				.Setup(m => m.CreateJob(It.IsAny<string>(), It.IsAny<BusinessObjectFactory>(), It.IsAny<string>()))
				.Returns(ProcessJobHeader.GetForParent(BMSTestHelper.CreateJob<IWorkItem>(Factory), Factory));
			controller
				.Setup(m => m.ShowNewFormAsDialogAndGetSaved())
				.Returns(ProcessJobHeader.GetForParent(BMSTestHelper.CreateJob<IWorkItem>(Factory), Factory));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);

			var refreshes = new List<RefreshType>();
			networkViewModel.GetJobNetwork().Refreshed += (s, e) => refreshes.Add(e.RefreshType);

			Factory.Save();

			var action = new CreateWorkItemAction(networkViewModel);
			action.ExecuteAfterActivatingEntity_ForTest(diagram);

			AssertArrayEqualsByElements(new[] { RefreshType.EntitiesReloaded, RefreshType.EntityEdited, RefreshType.EntitiesReloaded, RefreshType.RedrawDiagram }, refreshes.ToArray());

			AssertEquals(1, diagram.ChildShapes.Count);

			var childShape = diagram.ChildShapes.Single();
			var jobHeader = (ProcessJobHeader)childShape.ProcessHeader;
			AssertNotNull(jobHeader);
			AssertEquals("WKI", childShape.BNS_JobType);
			AssertType<WorkItem>(jobHeader.Parent);
		}
		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Creates and opens a new work item", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon(string.Empty);
		}

		protected override void TestGetNameCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Create Work Item", GetAction(CreateNetworkViewModel(diagram)).GetName());
		}

		protected override void TestIsApplicableCore()
		{
			var system = CreateSystem(new[] { "WKI" });
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);
			var buffer = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Buffer);
			var annotation = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Annotation);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("buffer", new string[] {
					"The shape should not be a buffer or annotation." }, action.IsApplicableAfterActivatingEntity_ForTest(buffer));

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("annotation", new string[] {
					"The shape should not be a buffer or annotation." }, action.IsApplicableAfterActivatingEntity_ForTest(annotation));
		}

		protected override void TestIsEnabledCore()
		{
			var system = CreateSystem(new string[] { "ORG", "WKI" });
			var jobHeader = CreateJobHeader<OrgHeader>();
			var linkedDiagram = CreateDiagram(jobHeader);

			var shape = CreateShape(linkedDiagram);

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var linkedShape = CreateShape(jobHeader1, linkedDiagram);

			var networkViewModel1 = CreateNetworkViewModel(linkedDiagram);

			var unlinkedDiagram = Factory.New<BMNCNShape>();
			var networkViewModel2 = CreateNetworkViewModel(unlinkedDiagram);

			AssertEquals(true, GetAction(networkViewModel1).IsEnabledAfterActivatingEntity_ForTest(shape).IsAllowed);
			AssertEquals(false, GetAction(networkViewModel1).IsEnabledAfterActivatingEntity_ForTest(linkedShape).IsAllowed);
			AssertEquals(true, GetAction(networkViewModel1).IsEnabledAfterActivatingEntity_ForTest(linkedDiagram).IsAllowed);
			AssertEquals(true, GetAction(networkViewModel2).IsEnabledAfterActivatingEntity_ForTest(unlinkedDiagram).IsAllowed);
		}

		public void TestIsEnabled_NoWorkItemInAllowedWorkflowTypes()
		{
			var system = CreateSystem(new string[] { "ORG" });

			var unLinkedDiagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(unLinkedDiagram);

			AssertEquals(false, GetAction(networkViewModel).IsEnabledAfterActivatingEntity_ForTest(unLinkedDiagram).IsAllowed);
			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("denialReason", new string[] {
					"There is no buffer management system registered for this job type." },
					GetAction(networkViewModel).IsEnabledAfterActivatingEntity_ForTest(unLinkedDiagram));
		}
	}
}
