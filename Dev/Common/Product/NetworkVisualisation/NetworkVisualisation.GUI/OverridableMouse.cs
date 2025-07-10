using System.Windows;
using System.Windows.Input;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class OverridableMouse
	{
		public OverridableMouse()
		{
			MousePositionOverride = null;
		}

		public OverridableMouse(Point position)
		{
			MousePositionOverride = position;
		}

		Point? MousePositionOverride;

		public Point GetPosition(IInputElement element)
		{
			if (MousePositionOverride != null)
			{
				if (element is FrameworkElement frameworkElement)
				{
					var referencePosition = frameworkElement.TranslatePoint(new Point(0.0, 0.0), null); // Gets the position of an element relative to the root element
					var mousePosition = (Point)MousePositionOverride;

					return new Point(mousePosition.X - referencePosition.X, mousePosition.Y - referencePosition.Y);
				}
			}

			return Mouse.GetPosition(element);
		}
	}
}
