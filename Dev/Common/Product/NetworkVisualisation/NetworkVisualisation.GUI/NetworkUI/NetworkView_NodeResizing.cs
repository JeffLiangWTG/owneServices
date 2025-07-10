using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI
{
	public partial class NetworkView
	{
		void NodeItem_ResizeStarted(object source, NodeResizeStartedEventArgs e)
		{
			e.Handled = true;

			foreach (NodeItem node in e.Nodes)
			{
				node.IsContinuouslyChanging = true;
			}

			var eventArgs = new NodeResizeStartedEventArgs(NodeResizeStartedEvent, this, e.Nodes);
			RaiseEvent(eventArgs);
		}

		void NodeItem_Resizing(object sender, NodeResizingEventArgs e)
		{
			e.Handled = true;

			foreach (NodeItem node in e.Nodes)
			{
				var viewModel = node.DataContext as NodeViewModel;

				if (viewModel != null)
				{
					using (viewModel.NetworkViewModel.PerformNodeResize())
					{
						double horizontalChange = AdjustHorizontalChange(e.HorizontalChange, viewModel);
						double verticalChange = AdjustVerticalChange(e.VerticalChange, viewModel);

						switch (viewModel.ReferencePoint)
						{
							case NodeResizeReferencePoint.LeftTop:
								viewModel.X += horizontalChange;
								viewModel.Y += verticalChange;
								viewModel.Width -= horizontalChange;
								viewModel.Height -= verticalChange;
								break;
							case NodeResizeReferencePoint.RightTop:
								viewModel.Y += verticalChange;
								viewModel.Width += horizontalChange;
								viewModel.Height -= verticalChange;
								break;
							case NodeResizeReferencePoint.LeftBottom:
								viewModel.X += horizontalChange;
								viewModel.Width -= horizontalChange;
								viewModel.Height += verticalChange;
								break;
							case NodeResizeReferencePoint.RightBottom:
								viewModel.Width += horizontalChange;
								viewModel.Height += verticalChange;
								break;
							default:
								break;
						}
					}
				}
			}

			var eventArgs = new NodeResizingEventArgs(NodeResizeStartedEvent, this, e.Nodes, e.HorizontalChange, e.VerticalChange);
			RaiseEvent(eventArgs);
		}

		void NodeItem_ResizeCompleted(object source, NodeResizeCompletedEventArgs e)
		{
			e.Handled = true;

			foreach (NodeItem node in e.Nodes)
			{
				node.IsContinuouslyChanging = false;
			}

			var eventArgs = new NodeResizeCompletedEventArgs(NodeResizeCompletedEvent, this, e.Nodes);
			RaiseEvent(eventArgs);
		}

		double AdjustHorizontalChange(double expectedChange, NodeViewModel viewModel)
		{
			double actualChange = expectedChange;

			if (expectedChange < 0)
			{
				actualChange = AdjustHorisontalChangeNeg(expectedChange, viewModel);
			}
			else if (expectedChange > 0)
			{
				actualChange = AdjustHorisontalChangePos(expectedChange, viewModel);
			}

			return actualChange;
		}

		double AdjustHorisontalChangeNeg(double expectedChange, NodeViewModel viewModel)
		{
			double minX = 0, refX = 0;

			switch (viewModel.ReferencePoint)
			{
				case NodeResizeReferencePoint.LeftTop:
				case NodeResizeReferencePoint.LeftBottom:
					minX = GetParentValue(viewModel, entityFunc: e => e.X, marginFunc: m => m.Left, 0d);
					refX = viewModel.X;
					break;
				case NodeResizeReferencePoint.RightTop:
				case NodeResizeReferencePoint.RightBottom:
					minX = GetChildrenValue(viewModel,
											transformFunc: (coll, func) => coll.Max(func),
											childFunc: c => c.X + c.Width,
											marginFunc: m => m.Right,
											defaultValue: viewModel.X + NodeViewModel.MinAllowedNodeWidth);
					refX = viewModel.X + viewModel.Width;
					break;
			}

			return (refX + expectedChange < minX) ? minX - refX : expectedChange;
		}

		double AdjustHorisontalChangePos(double expectedChange, NodeViewModel viewModel)
		{
			double maxX = 0, refX = 0;

			switch (viewModel.ReferencePoint)
			{
				case NodeResizeReferencePoint.LeftTop:
				case NodeResizeReferencePoint.LeftBottom:
					maxX = GetChildrenValue(viewModel,
											transformFunc: (coll, func) => coll.Min(func),
											childFunc: c => c.X,
											marginFunc: m => -m.Left,
											defaultValue: viewModel.X + viewModel.Width - NodeViewModel.MinAllowedNodeWidth);
					refX = viewModel.X;
					break;
				case NodeResizeReferencePoint.RightTop:
				case NodeResizeReferencePoint.RightBottom:
					maxX = GetParentValue(viewModel, entityFunc: e => e.X + e.Width, marginFunc: m => -m.Right, double.MaxValue);
					refX = viewModel.X + viewModel.Width;
					break;
			}

			return (refX + expectedChange > maxX) ? maxX - refX : expectedChange;
		}

		double AdjustVerticalChange(double expectedChange, NodeViewModel viewModel)
		{
			double actualChange = expectedChange;

			if (expectedChange < 0)
			{
				actualChange = AdjustVerticalChangeNeg(expectedChange, viewModel);
			}
			else if (expectedChange > 0)
			{
				actualChange = AdjustVerticalChangePos(expectedChange, viewModel);
			}

			return actualChange;
		}

		double AdjustVerticalChangeNeg(double expectedChange, NodeViewModel viewModel)
		{
			double minY = 0, refY = 0;

			switch (viewModel.ReferencePoint)
			{
				case NodeResizeReferencePoint.LeftTop:
				case NodeResizeReferencePoint.RightTop:
					minY = GetParentValue(viewModel, entityFunc: e => e.Y, marginFunc: m => m.Top, 0d);
					refY = viewModel.Y;
					break;
				case NodeResizeReferencePoint.LeftBottom:
				case NodeResizeReferencePoint.RightBottom:
					minY = GetChildrenValue(viewModel,
											transformFunc: (coll, func) => coll.Max(func),
											childFunc: c => c.Y + c.Height,
											marginFunc: m => m.Bottom,
											defaultValue: viewModel.Y + NodeViewModel.MinAllowedNodeHeight);
					refY = viewModel.Y + viewModel.Height;
					break;
			}

			return (refY + expectedChange < minY) ? minY - refY : expectedChange;
		}

		double AdjustVerticalChangePos(double expectedChange, NodeViewModel viewModel)
		{
			double maxY = 0, refY = 0;

			switch (viewModel.ReferencePoint)
			{
				case NodeResizeReferencePoint.LeftTop:
				case NodeResizeReferencePoint.RightTop:
					maxY = GetChildrenValue(viewModel,
											transformFunc: (coll, func) => coll.Min(func),
											childFunc: c => c.Y,
											marginFunc: m => -m.Top,
											defaultValue: viewModel.Y + viewModel.Height - NodeViewModel.MinAllowedNodeHeight);
					refY = viewModel.Y;
					break;
				case NodeResizeReferencePoint.LeftBottom:
				case NodeResizeReferencePoint.RightBottom:
					maxY = GetParentValue(viewModel, entityFunc: e => e.Y + e.Height, marginFunc: m => -m.Bottom, double.MaxValue);
					refY = viewModel.Y + viewModel.Height;
					break;
			}

			return (refY + expectedChange > maxY) ? maxY - refY : expectedChange;
		}

		double GetParentValue(NodeViewModel viewModel, Func<INetworkEntity, double> entityFunc, Func<Thickness, double> marginFunc, double defaultValue)
		{
			var entity = viewModel.Entity.Parent as INetworkEntity;
			var margin = viewModel.Network.EntityPositionStrategy.GetInternalMargin();
			return entity != null && entity.Parent != null ? entityFunc(entity) + marginFunc(margin) : defaultValue;
		}

		double GetChildrenValue(NodeViewModel viewModel, Func<IEnumerable<NodeViewModel>, Func<NodeViewModel, double>, double> transformFunc,
						   Func<NodeViewModel, double> childFunc, Func<Thickness, double> marginFunc, double defaultValue)
		{
			var margin = viewModel.Network.EntityPositionStrategy.GetInternalMargin();
			return viewModel.Entity.Children.Any() ? transformFunc(viewModel.ChildNodes, childFunc) + marginFunc(margin) : defaultValue;
		}
	}
}
