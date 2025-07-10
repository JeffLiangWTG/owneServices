using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(CreateShapeFromClipboardAction))]
	class CreateShapeFromClipboardActionTest : JobNetworkActionTestCase<CreateShapeFromClipboardAction>
	{
		#region Job Hyperlinks

		public void TestJobInClipboadIsAlreadyLinked()
		{
			const string jobName = "YicketyYamDowzerDowz";

			var jobHeader = CreateJobHeader<OrgHeader>();
			((OrgHeader)jobHeader.Parent).OH_FullName = jobName;

			Factory.Save();

			var message = string.Empty;
			var controller = CreateMockableControllerWithMockableInteractionImplementor(Mocks);
			controller.Setup(m => m.GetJobsFromClipboard(Factory))
				.Returns(new[] { (BusinessObject)jobHeader.Parent });

			controller.Setup(m => m.UserInteractionImplementor.HasUserConfirmed(It.IsAny<string>(),
					It.IsAny<string>(), It.IsAny<ConfirmationNotification[]>()))
				.Returns(new Func<string, string, ConfirmationNotification[], bool>((m, c, n) =>
				{
					message = m;
					return true;
				}));

			var diagram = CreateDiagram(jobHeader);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			var action = new CreateShapeFromClipboardAction(networkViewModel);
			AssertEquals(true, action.IsEnabled().IsAllowed);
			AssertEquals("Shape for [YicketyYamDowzerDowz] (from clipboard)", action.GetName());

			action.ExecuteForEntityWithoutAccessCheck(diagram);

			AssertEquals(0, network.Entities.Count);
			AssertEquals(@"Items in the clipboard could not be added to the diagram:
YicketyYamDowzerDowz
	Cannot link this Business Entity to the shape because another shape with this Business Entity already exists on the diagram.
", message);
		}

		public void TestCreateNewShapeFromClipboardBetweenTwoCloseShapes()
		{
			const string jobName = "ClipboardShape";

			var controller = CreateMockableController(Mocks);
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			((OrgHeader)jobHeader1.Parent).OH_FullName = jobName;

			controller.Setup(m => m.GetJobsFromClipboard(Factory)).Returns(new[] { (BusinessObject)jobHeader1.Parent });

			var jobHeader2 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader3 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var jobHeader4 = VisualBoardsTestHelper.CreateJobHeader<OrgHeader>(Factory);

			Factory.Save();
			var diagramShape = NetworkTestCase.CreateDiagram(jobHeader2);

			var network = NetworkTestCase.CreateNetwork(diagramShape);
			var diagram = network.DiagramEntity;

			var shape1 = CreateShapeAtLocation(jobHeader3, diagram, 0, 1, 5, 5);
			var shape2 = CreateShapeAtLocation(jobHeader4, diagram, 0, 5, 20, 20);
			var networkViewModel = CreateNetworkViewModel(diagramShape, controller: controller.Object);

			var action = networkViewModel.GetCreateEntityActions().Single(a => a.GetName() == "Shape for [ClipboardShape] (from clipboard)");
			var shape = action.Execute();
			var entity = shape as INetworkEntity;

			networkViewModel.SetDefaultsForNewNode(entity, new Location(0, 0), false);

			var newShape = network.Shapes.Single(s => s.Name == "ClipboardShape");

			CombineAssertions("The newly created shape should overlap with the existing shapes", () =>
			{
				AssertEquals(0.0m, newShape.Top);
				AssertEquals(0.0m, newShape.Left);
			});
		}

		public void TestJobInClipboadHasNoWorkflow()
		{
			const string jobName = "ShazzamaJazzamaWhoopShoes";

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_FullName = jobName;
			Factory.Save();

			var controller = CreateMockableController(Mocks);
			controller.Setup(m => m.GetJobsFromClipboard(Factory)).Returns(new[] { job });
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			var action = new CreateShapeFromClipboardAction(networkViewModel);
			AssertEquals(false, action.IsEnabled().IsAllowed);
			AssertEquals("The link in the clipboard could not be identified.", action.GetName());

			action.ExecuteForEntityWithoutAccessCheck(diagram);

			AssertEquals(0, network.Entities.Count);
		}

		public void TestEmptyClipboard()
		{
			var controller = CreateMockableController(Mocks);
			controller.Setup(m => m.GetJobsFromClipboard(Factory)).Returns(Enumerable.Empty<BusinessObject>());
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			var action = new CreateShapeFromClipboardAction(networkViewModel);
			AssertEquals(false, action.IsEnabled().IsAllowed);
			AssertEquals("Shapes for hyperlinks in clipboard (clipboard is empty)", action.GetName());

			action.ExecuteForEntityWithoutAccessCheck(diagram);

			AssertEquals(0, network.Entities.Count);
		}

		#endregion

		#region Diagram Hyperlinks

		public void TestCreateShapeFromClipboard_WhenShapePartOfSameDiagramSelected_ShouldShowError()
		{
			var diagram1 = CreateDiagram(Factory, name: "Donnager");
			var shape1_1 = CreateShape(diagram1, name: "Rocinante");
			var shape1_2 = CreateShape(diagram1, name: "Canterbury");

			var diagram2 = CreateDiagram(Factory);
			var shape2 = CreateShape(diagram2, name: "Tycho");

			Factory.Save();

			AttemptToCreateShapeFromHyperlink(shape1_1, diagram1, shape1_2,
@"Items in the clipboard could not be added to the diagram:
Canterbury
	Only a diagram can be linked to a shape on another diagram.");

			AttemptToCreateShapeFromHyperlink(shape1_1, diagram1, diagram1,
@"Items in the clipboard could not be added to the diagram:
Donnager
	Cannot link a shape to this diagram.");

			AttemptToCreateShapeFromHyperlink(diagram1, diagram1, diagram1, @"Items in the clipboard could not be added to the diagram:
Donnager
	Cannot link a shape to itself.");

			AttemptToCreateShapeFromHyperlink(shape1_1, diagram1, shape2, @"Items in the clipboard could not be added to the diagram:
Tycho
	Only a diagram can be linked to a shape on another diagram.");
		}

		void AttemptToCreateShapeFromHyperlink(BMNCNShape shape, BMNCNShape rootDiagram, BMNCNShape shapeToCopyHyperlink, string expectedError)
		{
			var controller = CreateMockableControllerWithMockableInteractionImplementor(Mocks);
			string actualError = null;

			controller.Setup(m => m.GetJobsFromClipboard(Factory)).Returns(new[] { shapeToCopyHyperlink });

			controller
				.Setup(m => m.UserInteractionImplementor.HasUserConfirmed(It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<ConfirmationNotification[]>()))
				.Returns(new Func<string, string, ConfirmationNotification[], bool>((m, c, rn) =>
				{
					actualError = m;
					return true;
				}));

			var networkViewModel = CreateNetworkViewModel(rootDiagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			var action = new CreateShapeFromClipboardAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(shape);

			if (expectedError == null)
			{
				var newShape = network.Shapes.Single(s => !s.IsInDatabase);

				AssertEquals(ShapeTypeList.Codes.Shape, newShape.BNS_ShapeType);
				AssertEquals(shapeToCopyHyperlink, newShape.RelatedShape);
			}

			AssertMultilineASCIIEquals(expectedError, actualError);
		}

		#endregion

		#region Multiple Hyperlinks

		public void TestMultiCreate()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();
			var jobHeader4 = CreateJobHeader<OrgHeader>();

			var allHeaders = new[] { jobHeader1, jobHeader2, jobHeader3, jobHeader4 };

			var controller = CreateMockableController(Mocks);
			controller.Setup(m => m.GetJobsFromClipboard(Factory)).Returns(allHeaders.Select(a => (BusinessObject)a.Parent));
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			var action = new CreateShapeFromClipboardAction(networkViewModel);
			AssertEquals(true, action.IsEnabled().IsAllowed);
			AssertEquals("Shapes for 4 copied items", action.GetName());

			var result = action.ExecuteForEntityWithoutAccessCheck(diagram);

			AssertEquals(4, network.Shapes.Count);
			AssertEquals(result.GetType(), typeof(EntityCollection));
			var linkedHeaders = network.Shapes.Select(s => s.ProcessHeader);
			AssertContainsExactElementsInAnyOrder(allHeaders, linkedHeaders);
		}

		public void TestMultiCreate_PartiallyBroken_DuplicatesAndDeleted()
		{
			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();
			jobHeader3.Name = "Boink";
			var jobHeader4 = CreateJobHeader<OrgHeader>();

			var allHeaders = new[] { jobHeader1, jobHeader2, jobHeader3, jobHeader3, jobHeader4 };

			jobHeader4.Delete();

			var controller = CreateMockableControllerWithMockableInteractionImplementor(Mocks);
			var message = string.Empty;
			controller.Setup(m => m.GetJobsFromClipboard(Factory)).Returns(allHeaders);
			controller.Setup(m => m.UserInteractionImplementor.HasUserConfirmed(It.IsAny<string>(),
					It.IsAny<string>(), It.IsAny<ConfirmationNotification[]>()))
				.Returns(new Func<string, string, ConfirmationNotification[], bool>((m, c, n) =>
				{
					message = m;
					return true;
				}));

			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			var action = new CreateShapeFromClipboardAction(networkViewModel);
			AssertEquals(true, action.IsEnabled().IsAllowed);
			AssertEquals("Shapes for 4 copied items", action.GetName());

			action.ExecuteForEntityWithoutAccessCheck(diagram);

			AssertEquals(3, network.Shapes.Count);

			var linkedHeaders = network.Shapes.Select(s => s.ProcessHeader);
			AssertContainsExactElementsInAnyOrder(new[] { jobHeader1, jobHeader2, jobHeader3 }, linkedHeaders);
			AssertEquals(
				"Items in the clipboard could not be added to the diagram:\r\nBoink\r\n\tCannot link this Business Entity to the shape because another shape with this Business Entity already exists on the diagram.\r\n",
				message);
		}

		#endregion

		#region Implementation

		protected override void TestExecuteCore()
		{
			const string jobName = "BoinkyBoinkerBoink";

			var jobHeader = CreateJobHeader<OrgHeader>();
			((OrgHeader)jobHeader.Parent).OH_FullName = jobName;

			Factory.Save();

			var controller = CreateMockableController(Mocks);
			controller.Setup(m => m.GetJobsFromClipboard(Factory)).Returns(new[] { (BusinessObject)jobHeader.Parent });
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);

			var newShape = network.Shapes[0];
			AssertEquals(ShapeTypeList.Codes.Shape, newShape.BNS_ShapeType);
			AssertEquals(jobHeader, newShape.ProcessHeader);
			AssertEquals(jobName, newShape.Name);
		}
		protected override void TestIsApplicableCore()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var root = CreateDiagram(jobHeader);
			AssertEquals("Precondition", ShapeTypeList.Codes.Diagram, root.BNS_ShapeType);
			AssertNotNull("Precondition", root.ProcessHeader);

			var shape = CreateShape(root);
			var buffer = CreateShape(root, shapeType: ShapeTypeList.Codes.Buffer);
			var annotation = CreateShape(root, shapeType: ShapeTypeList.Codes.Annotation);

			var networkViewModel = CreateNetworkViewModel(root);
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(root));

			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(shape));

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The shape should not be a buffer or annotation.", action.IsApplicableAfterActivatingEntity_ForTest(buffer));

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The shape should not be a buffer or annotation.", action.IsApplicableAfterActivatingEntity_ForTest(annotation));

			AssertCannotExecuteInWorkflowRelationshipDesigner();
		}

		protected override void TestIsEnabledCore()
		{
			const string jobName = "RhamaLambaBingyBong";

			var jobHeader = CreateJobHeader<OrgHeader>();
			((OrgHeader)jobHeader.Parent).OH_FullName = jobName;

			Factory.Save();

			var controller = CreateMockableController(Mocks);
			controller.Setup(m => m.GetJobsFromClipboard(Factory)).Returns(new[] { (BusinessObject)jobHeader.Parent });
			var diagram = CreateDiagram(jobHeader);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			var action = GetAction(networkViewModel);
			AssertEquals(true, action.IsEnabledAfterActivatingEntity_ForTest(diagram).IsAllowed);
		}

		protected override void TestGetNameCore()
		{
			const string jobName = "ShooBobShooWaddaWadda";

			var jobHeader = CreateJobHeader<OrgHeader>();
			((OrgHeader)jobHeader.Parent).OH_FullName = jobName;

			Factory.Save();

			var controller = CreateMockableController(Mocks);
			controller.Setup(m => m.GetJobsFromClipboard(Factory)).Returns(new[] { (BusinessObject)jobHeader.Parent });
			var diagram = CreateDiagram(jobHeader);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			var action = GetAction(networkViewModel);
			AssertEquals("Shape for [ShooBobShooWaddaWadda] (from clipboard)", action.GetNameAfterActivatingEntity_ForTest(diagram));
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Create a new shape for the hyperlink currently in the clipboard", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("Shape");
		}

		protected override CreateShapeFromClipboardAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new CreateShapeFromClipboardAction(networkViewModel);
		}

		#endregion
	}
}
