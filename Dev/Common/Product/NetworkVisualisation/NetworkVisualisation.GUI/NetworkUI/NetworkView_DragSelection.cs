using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CargoWise.NetworkVisualisation.Business;

namespace CargoWise.NetworkVisualisation.GUI
{
	/// <summary>
	/// Partial definition of the NetworkView class.
	/// This file only contains private members related to drag selection.
	/// </summary>
	public partial class NetworkView
	{
		Point selectionInitialMousePosition;
		Canvas selectionCanvas;
		Border selectionBorder;
		NodeViewModelCollection selectionNodesCache;
		NodeViewModel[] selectionPreSelectedNodesCache;

		Point GetMousePosition()
		{
#if DEBUG
			if (MouseController.CurrentMousePosition_ForTest != null)
			{
				return MouseController.CurrentMousePosition_ForTest.Value;
			}
#endif
			return System.Windows.Input.Mouse.GetPosition(selectionCanvas);
		}

		public void StartSelection(Canvas canvas, bool retainExisting = false)
		{
			selectionCanvas = canvas;
			selectionInitialMousePosition = GetMousePosition();
			selectionBorder = selectionCanvas.Children[0] as Border;
			var viewModel = DataContext as DiagramAreaUserControlViewModel;

			selectionNodesCache = viewModel?.NetworkViewModel?.GetAppropriateNodeCollection(viewModel.IsNonScheduled);

			if (retainExisting && selectionNodesCache != null)
			{
				selectionPreSelectedNodesCache = selectionNodesCache.Where(n => n.IsSelected).ToArray();
			}
		}

		public void UpdateSelection()
		{
			if (selectionCanvas == null || selectionBorder == null || selectionNodesCache == null)
			{
				return;
			}

			var currentMousePositon = GetMousePosition();

			Canvas.SetLeft(selectionBorder, Math.Min(currentMousePositon.X, selectionInitialMousePosition.X));
			Canvas.SetTop(selectionBorder, Math.Min(currentMousePositon.Y, selectionInitialMousePosition.Y));
			selectionBorder.Width = Math.Abs(currentMousePositon.X - selectionInitialMousePosition.X);
			selectionBorder.Height = Math.Abs(currentMousePositon.Y - selectionInitialMousePosition.Y);
			if (selectionCanvas.Visibility != Visibility.Visible)
			{
				selectionCanvas.Visibility = Visibility.Visible;
			}

			var rect = new Rect(selectionInitialMousePosition, currentMousePositon);

			foreach (var node in selectionNodesCache)
			{
				if (selectionPreSelectedNodesCache == null || !selectionPreSelectedNodesCache.Contains(node))
				{
					node.IsSelected = rect.IntersectsWith(new Rect(new Point(node.X, node.Y), new Size(node.Width, node.Height)));
				}
			}
		}

		public void EndSelection()
		{
			selectionNodesCache = null;
			selectionPreSelectedNodesCache = null;

			if (selectionCanvas != null)
			{
				selectionCanvas.Visibility = Visibility.Collapsed;
			}
		}
	}
}