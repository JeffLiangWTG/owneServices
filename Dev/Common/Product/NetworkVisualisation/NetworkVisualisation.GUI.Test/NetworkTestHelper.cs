using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	public static class NetworkTestHelper
	{
		#region Getting Controls

		public static IEnumerable<Line> GetChannelLines(DependencyObject control)
		{
			return control.FindChildren<Line>().Where(x => x.Tag?.ToString() == "ChannelLine" && x.StrokeThickness < 2d);
		}

		public static IEnumerable<TextBlock> GetChannelHeaders(DependencyObject control)
		{
			return control.FindChildren<TextBlock>().Where(x => x.Tag?.ToString() == "ChannelHeaderTag");
		}

		public static DynamicGrid GetDynamicGrid(DependencyObject control)
		{
			return control.FindChildren<DynamicGrid>().SingleOrDefault();
		}

#pragma warning disable CS0618
		public static IEnumerable<AdornedControl> GetShapeControls(DiagramAreaUserControl diagramControl)
		{
			return diagramControl.FindChildren<AdornedControl>().Where(x => x.DataContext is NodeViewModel);
		}
#pragma warning restore CS0618

#pragma warning disable CS0618
		public static AdornedControl GetShapeControl(DiagramAreaUserControl diagramControl, string shapeName)
		{
			return GetShapeControls(diagramControl).SingleOrDefault(x => ((NodeViewModel)x.DataContext).Name == shapeName);
		}
#pragma warning restore CS0618

		#endregion

		#region Control Positioning

		public static Point GetRelativePosition(UIElement elementToGetPositionOf, UIElement reletiveTo)
		{
			return elementToGetPositionOf.TranslatePoint(new Point(0, 0), reletiveTo);
		}

		#endregion

		#region Creating Nodes

#pragma warning disable CS0618
		public static void CreateNewEntityUsingContextMenu(NetworkUserControl control, string entityName)
		{
			var entity = new Entity()
			{
				Name = entityName
			};
			NetworkVisualisationTestHelper.AddEntityToNetwork(control.NetworkViewModel.Network, entity);
			var createEntityAction = new StaticNetworkAction(() => entity);
			control.MainDiagramControl.ExecuteContextMenuAction(createEntityAction);
		}
#pragma warning restore CS0618

		#endregion

		#region Moving Nodes

		public static void SimulateNodeDragCompletedEvent(NetworkView view, NodeViewModel node)
		{
			view.RaiseEvent(new NodeDragCompletedEventArgs(NetworkView.NodeDragCompletedEvent, node, new[] { node }));
		}

		public static void SimulateNodeDragCompletedEventOnNodeItem(NetworkView view, NodeViewModel node)
		{
			view.RaiseEvent(new NodeDragCompletedEventArgs(NodeItem.NodeDragCompletedEvent, node, new[] { node }));
		}

		public static void SimulateDraggingNodeToNewPosition(NodeViewModel node, double x, double y, NetworkView view)
		{
			node.X = x;
			node.Y = y;
			SimulateNodeDragCompletedEvent(view, node);
		}

		#endregion
	}
}
