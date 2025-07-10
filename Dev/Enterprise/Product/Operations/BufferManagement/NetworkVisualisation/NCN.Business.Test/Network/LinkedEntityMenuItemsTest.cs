using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	public class LinkedEntityMenuItemsTest : NetworkTestCase
	{
		#region Menu Structure

		public void TestMenuStructure_ForNonLinkedShape()
		{
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram);
			var networkViewModel = CreateNetworkViewModel(diagram);

			AssertLinkedEntityMenuStructure(networkViewModel, diagram, SelectFromHyperlinkInTheClipboard, SelectWorkflow, SelectDiagramForDiagram);
			AssertLinkedEntityMenuStructure(networkViewModel, shape, SelectFromHyperlinkInTheClipboard, SelectWorkflow, SelectDiagram);
		}

		public void TestMenuStructure_ForShapeLinkedToWorkflow()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Try moving the slider");
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Try adjusting the phase");

			var diagram = CreateDiagram(jobHeader);
			var shape = CreateShape(workflow, diagram);
			var networkViewModel = CreateNetworkViewModel(diagram);

			AssertLinkedEntityMenuStructure(networkViewModel, diagram, SelectFromHyperlinkInTheClipboard, SelectWorkflow, SelectDiagramForDiagram, Separator, OpenWorkflow, UnLinkWorkflow);
			AssertLinkedEntityMenuStructure(networkViewModel, shape, SelectFromHyperlinkInTheClipboard, SelectWorkflow, SelectDiagram, Separator, OpenWorkflow, UnLinkWorkflow);
		}

		public void TestMenuStructure_ForShapeLinkedToShape()
		{
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram);
			var networkViewModel = CreateNetworkViewModel(diagram);

			var otherDiagram1 = CreateDiagram(Factory, name: "Try moving the slider");
			var otherDiagram2 = CreateDiagram(Factory, name: "Try adjusting the phase");

			NetworkTestCase.LinkToRelatedDiagram(diagram, otherDiagram1);
			NetworkTestCase.LinkToRelatedDiagram(shape, otherDiagram2);

			AssertLinkedEntityMenuStructure(networkViewModel, diagram, SelectFromHyperlinkInTheClipboard, SelectWorkflow, SelectDiagramForDiagram, Separator, OpenDiagram, UnLinkDiagram);
			AssertLinkedEntityMenuStructure(networkViewModel, shape, SelectFromHyperlinkInTheClipboard, SelectWorkflow, SelectDiagram, Separator, OpenDiagram, UnLinkDiagram);
		}

		static void AssertLinkedEntityMenuStructure(NetworkViewModel networkViewModel, BMNCNShape shape, params MenuItemConfig[] expectedLinkedEntityMenuItems)
		{
			var entity = networkViewModel.GetJobNetwork().Entities.GetInstance(shape);
			var menuItems = networkViewModel.GetSecondLevelNetworkActionsMenuItems_ForTesting(entity, "Linked Entity");
			var actualItemsConfig = menuItems.Select(i => i == null ? null : new MenuItemConfig(i.Name, i.Tooltip, i.Enabled)).ToArray();

			CombineAssertions("Menu items for shape " + shape.Name, () =>
			{
				for (var i = 0; i < expectedLinkedEntityMenuItems.Length; i++)
				{
					var expectedMenuItem = expectedLinkedEntityMenuItems[i];
					var actualMenuItem = i < actualItemsConfig.Length ? actualItemsConfig[i] : null;

					if (expectedMenuItem == null)
					{
						AssertNull("Should be a separator at position " + i, actualMenuItem);
					}
					else
					{
						if (actualMenuItem == null)
						{
							AssertEquals("There was no menu item at position " + i, expectedMenuItem.Item1, null);
						}
						else
						{
							AssertEquals("Text for menu item at position " + i, expectedMenuItem.Item1, actualMenuItem.Item1);
							AssertEquals("Tooltip for menu item at position " + i, expectedMenuItem.Item2, actualMenuItem.Item2);
							AssertEquals("Enabled for menu item at position " + i, expectedMenuItem.Item3, actualMenuItem.Item3);
						}
					}
				}
			});
		}

		static MenuItemConfig SelectFromHyperlinkInTheClipboard => new MenuItemConfig("Link to Entity from Clipboard", @"Link to the hyperlink currently in the clipboard. (Paste will also link from the clipboard)

This action cannot be executed for the given shape(s) due to the following reasons:
New Diagram: There is no hyperlink in the clipboard. If a hyperlink has been copied to the clipboard then this shape can be linked to the job.", false);
		static MenuItemConfig SelectWorkflow => new MenuItemConfig("Select Workflow...", "Choose a record from the Job Workflows module to which this shape will be linked.", true);
		static MenuItemConfig SelectDiagram => new MenuItemConfig("Select Diagram...", "Choose a record from the Network Diagrams module to which this shape will be linked.", true);

		static MenuItemConfig SelectDiagramForDiagram => new MenuItemConfig("Select Diagram...", @"Choose a record from the Network Diagrams module to which this shape will be linked.

This action cannot be executed for the given shape(s) due to the following reasons:
New Diagram: The action can be applied to shapes only.", false);
		static MenuItemConfig Separator => null;
		static MenuItemConfig OpenWorkflow => new MenuItemConfig("Open Linked Workflow", "Open the linked workflow in its job's form.", true);
		static MenuItemConfig OpenDiagram => new MenuItemConfig("Open Linked Diagram", "Open the linked diagram in its own form.", true);
		static MenuItemConfig UnLinkWorkflow => new MenuItemConfig("Un-link this Workflow", "Un-links the shape from its currently-linked workflow.", true);
		static MenuItemConfig UnLinkDiagram => new MenuItemConfig("Un-link this Diagram", "Un-links the shape from its currently-linked diagram.", true);

		class MenuItemConfig : Tuple<string, string, bool>
		{
			internal MenuItemConfig(string text, string tooltip, bool isEnabled)
				: base(text, tooltip, isEnabled)
			{
			}
		}

		#endregion

		#region Linking Menu Items

		public void TestLinkWorkflowMenuItem_ShouldLinkShapeToSelectedWorkflow()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "The phase seems well adjusted");
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Try adjusting the phase");

			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram);

			controller
				.SetupSequence(m => m.PickEntity(ModuleIDs.ProcessHeader, false))
				.Returns(jobHeader)
				.Returns(workflow);

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			AssertNull(diagram.ProcessHeader);
			AssertNull(shape.ProcessHeader);

			ClickLinkToWorkflowAction(diagram, networkViewModel);
			ClickLinkToWorkflowAction(shape, networkViewModel);

			AssertEquals(jobHeader, diagram.ProcessHeader);
			AssertEquals(workflow, shape.ProcessHeader);
		}

		public void TestLinkDiagramMenuItem_ShouldLinkShapeToSelectedDiagram()
		{
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram);

			var otherDiagram1 = CreateDiagram(Factory, name: "Try the amplitude");
			var otherDiagram2 = CreateDiagram(Factory, name: "That's the leftmost dial");

			controller.Setup(m => m.PickEntity(It.IsAny<ModuleIdentifier>(), It.IsAny<bool>()))
				.Returns(otherDiagram1);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			AssertNull(diagram.RelatedShape);
			AssertNull(shape.RelatedShape);

			ClickLinkToDiagramAction(diagram, networkViewModel);
			ClickLinkToDiagramAction(shape, networkViewModel);

			AssertEquals(null, diagram.RelatedShape);
			AssertEquals(otherDiagram1, shape.RelatedShape);
			controller.Verify(m => m.PickEntity(It.IsAny<ModuleIdentifier>(), It.IsAny<bool>()), Times.Once());
		}

		public void TestLinkDiagramMenuItem_WhenShapeSelected_ShouldLinkShapeToSelectedShape()
		{
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram);

			var otherDiagram1 = CreateDiagram(Factory);
			var otherDiagram2 = CreateDiagram(Factory);
			var otherShape1 = CreateShape(otherDiagram1, name: "Reposition the phase");
			var otherShape2 = CreateShape(otherDiagram2, name: "Try the dial on the right");

			controller.Setup(m => m.PickEntity(It.IsAny<ModuleIdentifier>(), It.IsAny<bool>()))
				.Returns(otherDiagram2);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			AssertNull(diagram.RelatedShape);
			AssertNull(shape.RelatedShape);

			ClickLinkToDiagramAction(diagram, networkViewModel);
			ClickLinkToDiagramAction(shape, networkViewModel);

			AssertEquals(null, diagram.RelatedShape);
			AssertEquals(otherDiagram2, shape.RelatedShape);
			controller.Verify(m => m.PickEntity(It.IsAny<ModuleIdentifier>(), It.IsAny<bool>()), Times.Once());
		}

		public void TestLinkDiagramMenuItem_WhenShapePartOfSameDiagramSelected_ShouldShowError()
		{
			var diagram1 = CreateDiagram(Factory);
			var shape1_1 = CreateShape(diagram1);
			var shape1_2 = CreateShape(diagram1);

			var diagram2 = CreateDiagram(Factory);

			AttemptLinkToDiagram(shape1_1, diagram1, shape1_2, "Only a diagram can be linked to a shape on another diagram.");
			AttemptLinkToDiagram(shape1_1, diagram1, diagram1, "Cannot link a shape to this diagram.");
		}

		public void TestLinkDiagramMenuItem_WhenProhibitedShapeTypeSelected_ShouldShowError()
		{
			var diagram1 = CreateDiagram(Factory);
			var shape1 = CreateShape(diagram1);

			var diagram2 = CreateDiagram(Factory);
			var annotation = CreateShape(diagram2, shapeType: ShapeTypeList.Codes.Annotation);
			var buffer = CreateShape(diagram2, shapeType: ShapeTypeList.Codes.Buffer);
			var shape2 = CreateShape(diagram2);

			AttemptLinkToDiagram(shape1, diagram1, annotation, "Cannot link a shape to an annotation.");
			AttemptLinkToDiagram(shape1, diagram1, buffer, "Only a diagram can be linked to a shape on another diagram.");
			AttemptLinkToDiagram(shape1, diagram1, shape2, "Only a diagram can be linked to a shape on another diagram.");
			AttemptLinkToDiagram(shape1, diagram1, diagram2, null);
		}

		void AttemptLinkToDiagram(BMNCNShape shape, BMNCNShape rootDiagram, BMNCNShape linkToShape, string expectedError)
		{
			controller.Setup(m => m.PickEntity(It.IsAny<ModuleIdentifier>(), It.IsAny<bool>()))
				.Returns(linkToShape);
			var networkViewModel = CreateNetworkViewModel(rootDiagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			if (shape.RelatedShape != null)
			{
				network.UnlinkEntity(shape);
			}

			AssertNull(shape.RelatedShape);

			ClickLinkToDiagramAction(shape, networkViewModel);

			AssertEquals(expectedError == null ? linkToShape : null, shape.RelatedShape);

			if (expectedError != null)
			{
				AssertEquals(expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			controller.Verify(m => m.PickEntity(It.IsAny<ModuleIdentifier>(), It.IsAny<bool>()), Times.Once());
			UnitTestUserNotification.Instance.ClearMessages();
			CreateController();
		}

		public static void ClickLinkToWorkflowAction(BMNCNShape shape, NetworkViewModel networkViewModel)
		{
			ClickActionByName(shape, networkViewModel, SelectWorkflow.Item1);
		}

		public static void ClickLinkToDiagramAction(BMNCNShape shape, NetworkViewModel networkViewModel)
		{
			ClickActionByName(shape, networkViewModel, SelectDiagram.Item1);
		}

		#endregion

		#region Linking From Hyperlink

		public void TestLinkFromHyperlinkMenuItem_ShouldLinkShapeToSelectedWorkflow()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "The phase seems well adjusted");
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Try adjusting the phase");

			var diagram = CreateDiagram(Factory);
			CreateController(stubGetJobsFromClipboard: false);

			controller
				.Setup(m => m.GetJobsFromClipboard(It.IsAny<BusinessObjectFactory>()))
				.Returns(new[] { jobHeader });
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);

			AssertNull(diagram.ProcessHeader);

			ClickActionByName(diagram, networkViewModel, "Link to The phase seems well adjusted (from clipboard)");

			AssertEquals(jobHeader, diagram.ProcessHeader);
		}

		#endregion

		#region Open Linked Entity Menu Item

		[ExpectNoExceptions]
		public void TestOpenLinkedEntity_LinkedToWorkflow()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "Atrus pls");
			var diagram = CreateDiagram(jobHeader);

			controller.Setup(m => m.OpenLinkedEntity(jobHeader, ControllerIDs.ProcessHeader));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			ClickOpenLinkedWorkflowAction(diagram, networkViewModel);
			controller.Verify(m => m.OpenLinkedEntity(jobHeader, ControllerIDs.ProcessHeader), Times.Once());
		}

		[ExpectNoExceptions]
		public void TestOpenLinkedEntity_LinkedToDiagram()
		{
			var diagram = CreateDiagram(Factory);
			var otherDiagram = CreateDiagram(Factory, name: "You adjust the damn phase");

			NetworkTestCase.LinkToRelatedDiagram(diagram, otherDiagram);

			controller.Setup(m => m.OpenLinkedEntity(otherDiagram, ControllerIDs.NetworkDiagram));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			ClickOpenLinkedDiagramAction(diagram, networkViewModel);
			controller.Verify(m => m.OpenLinkedEntity(otherDiagram, ControllerIDs.NetworkDiagram), Times.Once());
		}

		public static void ClickOpenLinkedWorkflowAction(BMNCNShape shape, NetworkViewModel networkViewModel)
		{
			ClickActionByName(shape, networkViewModel, OpenWorkflow.Item1);
		}

		public static void ClickOpenLinkedDiagramAction(BMNCNShape shape, NetworkViewModel networkViewModel)
		{
			ClickActionByName(shape, networkViewModel, OpenDiagram.Item1);
		}

		#endregion

		#region Un-link Entity Menu Item

		public void TestUnLinkShape_ForLinkedWorkflow()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false, description: "The phase seems well adjusted");
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Try adjusting the phase");

			var diagram = CreateDiagram(jobHeader);
			var shape = CreateShape(workflow, diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			AssertEquals(jobHeader, diagram.ProcessHeader);
			AssertEquals(workflow, shape.ProcessHeader);

			ClickUnLinkWorkflowAction(diagram, networkViewModel);
			ClickUnLinkWorkflowAction(shape, networkViewModel);

			AssertNull(diagram.ProcessHeader);
			AssertNull(shape.ProcessHeader);
		}

		public void TestUnLinkShape_ForLinkedDiagram()
		{
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram);

			var otherDiagram1 = CreateDiagram(Factory, name: "Try the amplitude");
			var otherDiagram2 = CreateDiagram(Factory, name: "That's the leftmost dial");

			LinkToRelatedDiagram(diagram, otherDiagram1);
			LinkToRelatedDiagram(shape, otherDiagram2);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			AssertEquals(otherDiagram1, diagram.RelatedShape);
			AssertEquals(otherDiagram2, shape.RelatedShape);

			ClickUnLinkDiagramAction(diagram, networkViewModel);
			ClickUnLinkDiagramAction(shape, networkViewModel);

			AssertNull(diagram.RelatedShape);
			AssertNull(shape.RelatedShape);
		}

		static void ClickUnLinkWorkflowAction(BMNCNShape shape, NetworkViewModel networkViewModel)
		{
			ClickActionByName(shape, networkViewModel, UnLinkWorkflow.Item1);
		}

		static void ClickUnLinkDiagramAction(BMNCNShape shape, NetworkViewModel networkViewModel)
		{
			ClickActionByName(shape, networkViewModel, UnLinkDiagram.Item1);
		}

		#endregion

		#region Availability By Shape Type

		public void TestLinkedEntityMenuItem_ShouldBePresentOnlyOnCertainShapeTypes()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape = NetworkTestCase.CreateShape(diagram);

			CombineAssertions("Linked Entity menu item availability for the various shape types:", () =>
			{
				foreach (ICodeDescription shapeType in new ShapeTypeList())
				{
					shape.BNS_ShapeType = shapeType.Code;

					var networkViewModel = CreateNetworkViewModel(diagram);
					var network = networkViewModel.GetJobNetwork();
					var entity = network.Entities.GetInstance(shape);

					var linkedEntityMenuItems = networkViewModel.GetSecondLevelNetworkActionsMenuItems_ForTesting(shape, "Linked Entity");

					switch (shapeType.Code)
					{
						case ShapeTypeList.Codes.Diagram:
						case ShapeTypeList.Codes.Shape:
							AssertNotNull("Shape Type: " + shapeType.Code, linkedEntityMenuItems);
							break;

						default:
							AssertNull("Shape Type: " + shapeType.Code, linkedEntityMenuItems);
							break;
					}
				}
			});
		}

		#endregion

		#region Implementation

		static void ClickActionByName(BMNCNShape shape, NetworkViewModel networkViewModel, string actionName)
		{
			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, shape))
			{
				networkViewModel.GetSecondLevelNetworkActionsMenuItem_ForTesting(shape, "Linked Entity", actionName).Action.Execute();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			CreateController();
		}

		Mock<IBMNetworkEntityController> controller;

		void CreateController(bool stubGetJobsFromClipboard = true)
		{
			controller = NetworkTestCase.CreateMockableController(Mocks);

			if (stubGetJobsFromClipboard)
			{
				controller.Setup(m => m.GetJobsFromClipboard(It.IsAny<BusinessObjectFactory>())).Returns(Array.Empty<BusinessObject>());
			}
		}

		#endregion
	}
}
