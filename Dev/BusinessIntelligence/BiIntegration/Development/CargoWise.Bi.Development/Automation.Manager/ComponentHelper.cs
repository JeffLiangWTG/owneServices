using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace CargoWise.Bi.Development.Automation.Manager
{
	public static class ComponentHelper
	{
		public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
			{
				var child = VisualTreeHelper.GetChild(obj, i);
				if (child != null && child is T)
				{
					return (T)child;
				}
				else
				{
					T childOfChild = FindVisualChild<T>(child);
					if (childOfChild != null)
					{
						return childOfChild;
					}
				}
			}
			return null;
		}

		public static DataGridCell FindCellFromDataGrid(DataGrid grid, int rowNumber = 0, int colNumber = 0)
		{
			var row = grid.ItemContainerGenerator.ContainerFromIndex(rowNumber) as DataGridRow;
			if (row != null)
			{
				var presenter = ComponentHelper.FindVisualChild<DataGridCellsPresenter>(row);
				if (presenter != null)
				{
					return presenter.ItemContainerGenerator.ContainerFromIndex(colNumber) as DataGridCell;
				}
			}
			return null;
		}
	}
}
