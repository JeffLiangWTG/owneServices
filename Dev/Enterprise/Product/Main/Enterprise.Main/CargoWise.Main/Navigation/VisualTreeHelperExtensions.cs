#if !WINZOR
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CargoWise.Main.Navigation
{
	public static class VisualTreeHelperExtensions
	{
		public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject parent) where T : DependencyObject
		{
			if (parent == null)
			{
				yield break;
			}

			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				if (child is T tChild)
				{
					yield return tChild;
				}

				foreach (var grandChild in FindVisualChildren<T>(child))
				{
					yield return grandChild;
				}
			}
		}

		public static T FindLogicalChild<T>(this DependencyObject parent) where T : DependencyObject
		{
			if (parent == null)
			{
				return null;
			}

			foreach (var child in LogicalTreeHelper.GetChildren(parent))
			{
				if (child is T tChild)
				{
					return tChild;
				}

				if (child is DependencyObject dependencyChild)
				{
					var result = FindLogicalChild<T>(dependencyChild);
					if (result != null)
					{
						return result;
					}
				}
			}
			return null;
		}

		public static T FindChild<T>(this DependencyObject parent, string childName) where T : DependencyObject
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				if (child is T target && child.GetValue(FrameworkElement.NameProperty) as string == childName)
				{
					return target;
				}

				var result = FindChild<T>(child, childName);
				if (result != null)
				{
					return result;
				}
			}
			return null;
		}
	}
}
#endif
