using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CargoWise.Common;

namespace CargoWise.Main.Navigation.DragDrop;
public static class UIHelper
{
	internal static UIElement GetUIElement(ItemsControl container, Point position)
	{
		Argument.NotNull(container, nameof(container)); // Suggested By ReviewBot 
		var elementAtPosition = container.InputHitTest(position) as DependencyObject;
		if (elementAtPosition != null)
		{
			while (elementAtPosition != null)
			{
				var testUIElement = container.ItemContainerGenerator.ItemFromContainer(elementAtPosition);
				if (testUIElement != DependencyProperty.UnsetValue)
				{
					return elementAtPosition as UIElement;
				}
				else
				{
					elementAtPosition = VisualTreeHelper.GetParent(elementAtPosition);
				}
			}
		}

		return null;
	}

	internal static bool IsPositionAboveElement(UIElement i, Point relativePosition)
	{
		Argument.NotNull(i, nameof(i));
		return relativePosition.Y < ((FrameworkElement)i).ActualHeight / 2;
	}
}
