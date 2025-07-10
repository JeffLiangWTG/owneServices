using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI
{
	public static class ExtensionMethods
	{
		public static T FindParent<T>(this DependencyObject obj)
			where T : class
		{
			var parent = VisualTreeHelper.GetParent(obj);
			if (parent != null)
			{
				var result = parent as T;
				return result ?? FindParent<T>(parent);
			}
			else
			{
				return null;
			}
		}

		public static IEnumerable<T> FindChildren<T>(this DependencyObject depObj) where T : class
		{
			// Thanks to http://stackoverflow.com/questions/974598/find-all-controls-in-wpf-window-by-type
			if (depObj != null)
			{
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
				{
					DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
					if (child != null && child is T)
					{
						yield return (T)(object)child;
					}

					foreach (T childOfChild in FindChildren<T>(child))
					{
						yield return childOfChild;
					}
				}
			}
		}

		public static Thickness GetInternalMargin(this IEntityPositionStrategy strategy)
		{
			return new Thickness(strategy.LeftMargin, strategy.TopMargin, strategy.RightMargin, strategy.BottomMargin);
		}
	}
}
