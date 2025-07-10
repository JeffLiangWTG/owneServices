using System.Collections.Generic;
using System.Linq;
#if !WINZOR
using System.Windows.Controls;
#endif
using System.Windows.Forms.Integration;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI.Test
{
	class MoveToOtherSectionActionGuiTest : NetworkGUITestCase
	{
		public void TestMoveToNonScheduledSection_ShouldPlaceShapeInMiddleOfNonScheduledViewport_DefaultScroll()
		{
			AssertMoveToNonScheduledSectionEndingPosition(shouldScrollFirst: false, expectedXPosition: 141);
		}

		public void TestMoveToNonScheduledSection_ShouldPlaceShapeInMiddleOfNonScheduledViewport_ScrollRightFirst()
		{
			AssertMoveToNonScheduledSectionEndingPosition(shouldScrollFirst: true, expectedXPosition: 843);
		}

		void AssertMoveToNonScheduledSectionEndingPosition(bool shouldScrollFirst, double expectedXPosition)
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var shape = CreateShape(diagram, "Squanch");
			var network = CreateNetwork(diagram);
			var entity = shape.AsEntity(network);

			entity.X = 100;
			entity.Y = 100;
			entity.Width = 300;

			var shapeToEnableScrolling = CreateShape(diagram, "ShapeToEnableScrolling");
			shapeToEnableScrolling.IsNonScheduled = true;
			var entityToEnableScrolling = shapeToEnableScrolling.AsEntity(network);
			entityToEnableScrolling.X = 1000;

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				DoEventsThoroughly();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var diagramControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

#if !WINZOR
				if (shouldScrollFirst)
				{
					var scrollControl = diagramControl.NonScheduledDiagramControl.FindChildren<ScrollViewer>().Single(x => x.Name == "ScrollViewer");
					scrollControl.ScrollToHorizontalOffset(1000);
					DoEventsThoroughly();
				}
#endif
				var viewModel = diagramControl.MainDiagramControl.NetworkViewModel;
				var entityToMove = viewModel.ScheduledNodes.Single().Entity;

				AssertContainsExactElementsInAnyOrder("The shape should not be moved to the non-scheduled section yet. SAD!", new[] { "ShapeToEnableScrolling" }, GetNodeNames(diagramControl.NonScheduledDiagramControl));

				using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(viewModel, entityToMove))
				{
					var action = new MoveToOtherSectionAction(viewModel);
					diagramControl.MainDiagramControl.ExecuteContextMenuAction(action);
					DoEventsThoroughly();
				}

				AssertContainsExactElementsInAnyOrder("The entity should have been moved to the other section. SAD!", new[] { "ShapeToEnableScrolling", "Squanch" }, GetNodeNames(diagramControl.NonScheduledDiagramControl));

				var entityThatWasMoved = diagramControl.NonScheduledDiagramControl.NetworkViewModel.Nodes.Single(x => x.Entity.Name == "Squanch").Entity;
				AssertEquals("The moved shape should be placed in the middle of the viewport. SAD!", (int)expectedXPosition, (int)entityThatWasMoved.X);
			}
		}

		public void TestMoveToScheduledSection_ShouldPlaceShapeInMiddleOfScheduledViewport_DefaultScroll()
		{
			AssertMoveToScheduledEndingPosition(shouldScrollFirst: false, expectedXPosition: 100); // even numbers show that it's snapping to the grid
		}

		public void TestMoveToScheduledSection_ShouldPlaceShapeInMiddleOfScheduledViewport_ScrollRightFirst()
		{
			AssertMoveToScheduledEndingPosition(shouldScrollFirst: true, expectedXPosition: 800); // even numbers show that it's snapping to the grid
		}

		void AssertMoveToScheduledEndingPosition(bool shouldScrollFirst, double expectedXPosition)
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var shape = CreateShape(diagram, "Squanch");
			shape.IsNonScheduled = true;
			var network = CreateNetwork(diagram);
			var entity = shape.AsEntity(network);

			entity.X = 100;
			entity.Y = 100;
			entity.Width = 310;

			var shapeToEnableScrolling = CreateShape(diagram, "ShapeToEnableScrolling");
			var entityToEnableScrolling = shapeToEnableScrolling.AsEntity(network);
			entityToEnableScrolling.X = 1000;

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				DoEventsThoroughly();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var diagramControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
#if !WINZOR
				var scrollControl = diagramControl.MainDiagramControl.FindChildren<ScrollViewer>().Single(x => x.Name == "ScrollViewer");

				if (shouldScrollFirst)
				{
					scrollControl.ScrollToHorizontalOffset(1000);
					DoEventsThoroughly();
				}
#endif
				var viewModel = diagramControl.NonScheduledDiagramControl.NetworkViewModel;
				var entityToMove = viewModel.NonScheduledNodes.Single().Entity;

				AssertContainsExactElementsInAnyOrder("The shape should not be moved to the scheduled section yet. SAD!", new[] { "ShapeToEnableScrolling" }, GetNodeNames(diagramControl.MainDiagramControl));

				using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(viewModel, entityToMove))
				{
					var action = new MoveToOtherSectionAction(viewModel);
					diagramControl.NonScheduledDiagramControl.ExecuteContextMenuAction(action);
					DoEventsThoroughly();
				}

				AssertContainsExactElementsInAnyOrder("The entity should have been moved to the other section. SAD!", new[] { "ShapeToEnableScrolling", "Squanch" }, GetNodeNames(diagramControl.MainDiagramControl));

				var entityThatWasMoved = diagramControl.MainDiagramControl.NetworkViewModel.Nodes.Single(x => x.Entity.Name == "Squanch").Entity;
				AssertEquals("The moved shape should be placed in the middle of the viewport. SAD!", (int)expectedXPosition, (int)entityThatWasMoved.X);
				AssertEquals("The moved shape should be resized to fit the scale grid. SAD!", 300d, entityThatWasMoved.Width);
			}
		}

		public void TestMoveShape_WithChildShapes_ShouldMoveShapeWithChildShapes()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;
			var network = CreateNetwork(diagram);

			var parentShape = CreateShape(diagram, "Parent");
			var childShape = CreateShape(parentShape, "Child ");
			var grandchildShape = CreateShape(childShape, "Grandchild ");

			var parentEntity = parentShape.AsEntity(network);
			var childEntity = childShape.AsEntity(parentEntity);
			var grandchildEntity = grandchildShape.AsEntity(childEntity);

			parentEntity.X = 100;
			parentEntity.Y = 100;
			parentEntity.Width = 600;
			parentEntity.Height = 600;
			childEntity.X = 200;
			childEntity.Y = 150;
			childEntity.Width = 400;
			childEntity.Height = 400;
			grandchildEntity.X = 300;
			grandchildEntity.Y = 200;
			grandchildEntity.Width = 200;
			grandchildEntity.Height = 200;

			Factory.Save();

			using (var form = new NetworkDiagramForm(diagram) { Size = ControlDpiScalingHelper.NewScaledSize(1600, 1000) })
			{
				form.Show();
				DoEventsThoroughly();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var diagramControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete

				diagramControl.SetNonScheduledSectionWidth(800d);
				DoEventsThoroughly();

				var viewModel = diagramControl.MainDiagramControl.NetworkViewModel;

				AssertContainsExactElementsInAnyOrder("The shape should not be moved to the non-scheduled section yet. SAD!", System.Array.Empty<string>(), GetNodeNames(diagramControl.NonScheduledDiagramControl));

				using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(viewModel, parentEntity))
				{
					var action = new MoveToOtherSectionAction(viewModel);
					diagramControl.MainDiagramControl.ExecuteContextMenuAction(action);
					DoEventsThoroughly();
				}

				AssertContainsExactElementsInAnyOrder("The entity should have been moved to the other section along with all its descendants. SAD!",
					new[] { "Parent", "Child", "Grandchild" }, GetNodeNames(diagramControl.NonScheduledDiagramControl));

				var nodes = diagramControl.NonScheduledDiagramControl.NetworkViewModel.Nodes;
				var parentNode = nodes.Single(x => x.Entity.Name == "Parent");
				var childNode = nodes.Single(x => x.Entity.Name == "Child");
				var grandchildNode = nodes.Single(x => x.Entity.Name == "Grandchild");

				AssertEquals("The moved shape should be in the expected position. SAD!", 91, (int)parentNode.X);
				AssertEquals("The child shape should have moved and kept its original offset with its parent. SAD!", 191, (int)childNode.X);
				AssertEquals("The grandchild shape should have moved and kept its original offset with its parent. SAD!", 291, (int)grandchildNode.X);

				AssertEquals("The Y coordinates for all shapes should not have changed. SAD!", 100d, parentNode.Y);
				AssertEquals("The Y coordinates for all shapes should not have changed. SAD!", 150d, childNode.Y);
				AssertEquals("The Y coordinates for all shapes should not have changed. SAD!", 200d, grandchildNode.Y);
			}
		}

		public void TestMoveShapeFromNonScheduledToScheduledSection_ForApprovedDiagram_ShouldKeepShapeAsNonApproved()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;
			var viewModel = CreateNetworkViewModel(diagram);

			var scheduledShape = CreateShape(diagram, "Scheduled");
			var nonScheduledShape = CreateShape(diagram, "Non-Scheduled");
			nonScheduledShape.IsNonScheduled = true;

			var approveAction = new ApproveDiagramAction(viewModel);
			approveAction.ExecuteForEntityWithoutAccessCheck(diagram);

			AssertEquals(true, scheduledShape.IsApproved);
			AssertEquals(false, nonScheduledShape.IsApproved);

			using (var form = new NetworkDiagramForm(diagram))
			{
				form.Show();
				DoEventsThoroughly();

				var elementHost = form.FindAll<ElementHost>().Single();
#pragma warning disable CS0618 // Type or member is obsolete
				var diagramControl = (NetworkUserControl)elementHost.Child;
#pragma warning restore CS0618 // Type or member is obsolete
				var nonScheduledViewModel = diagramControl.NonScheduledDiagramControl.NetworkViewModel;
				var entityToMove = nonScheduledViewModel.NonScheduledNodes.Single().Entity;

				using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(nonScheduledViewModel, entityToMove))
				{
					var action = new MoveToOtherSectionAction(nonScheduledViewModel);
					diagramControl.NonScheduledDiagramControl.ExecuteContextMenuAction(action);
					DoEventsThoroughly();
				}

				var entities = diagramControl.MainDiagramControl.NetworkViewModel.Nodes.Select(x => x.Entity).ToArray();
				var scheduledEntity = entities.Single(x => x.Name == "Scheduled");
				var movedEntity = entities.Single(x => x.Name == "Non-Scheduled");

				AssertEquals(false, scheduledEntity.IsNonScheduled);
				AssertEquals(false, movedEntity.IsNonScheduled);

				AssertEquals("Moving a non-scheduled shape into the scheduled section should not have unapproved other shapes. SAD!", true, scheduledEntity.AsShape().IsApproved);
				AssertEquals("Moving a non-scheduled shape into the scheduled section should not approve that shape just because the diagram is already approved. SAD!", false, movedEntity.AsShape().IsApproved);
			}
		}

#pragma warning disable CS0618 // Type or member is obsolete
		static IEnumerable<string> GetNodeNames(DiagramAreaUserControl control)
#pragma warning restore CS0618 // Type or member is obsolete
		{
			var viewModel = control.NetworkViewModel;
			var nodes = control.IsNonScheduled ? viewModel.NonScheduledNodes : viewModel.ScheduledNodes;

			return nodes.Select(x => x.Name);
		}
	}
}
