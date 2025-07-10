using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	public static class ControlTestHelper
	{
		public static void AssertControlSize(int standardDpiWidth, int standardDpiHeight, Control control, int allowedVariation = 1)
		{
			AssertControlSize(null, standardDpiWidth, standardDpiHeight, control, allowedVariation);
		}

		public static void AssertControlSize(string message, int standardDpiWidth, int standardDpiHeight, Control control, int allowedVariation = 1)
		{
			Assertion.CombineAssertions(message, () =>
			{
				AssertControlWidth("Width", standardDpiWidth, control, allowedVariation);
				AssertControlHeight("Height", standardDpiHeight, control, allowedVariation);
			});
		}

		public static void AssertControlWidth(string message, int expected, Control control, int allowedVariation = 1)
		{
			Assertion.AssertCloseEnough(message, ControlDpiScalingHelper.ScaleToCurrentDpiX(expected), control.Width, ControlDpiScalingHelper.ScaleToCurrentDpiX(allowedVariation));
		}

		public static void AssertControlWidth(int expected, Control control, int allowedVariation = 1)
		{
			AssertControlWidth(null, expected, control, allowedVariation);
		}

		public static void AssertControlHeight(string message, int expected, Control control, int allowedVariation = 1)
		{
			Assertion.AssertCloseEnough(message, ControlDpiScalingHelper.ScaleToCurrentDpiY(expected), control.Height, ControlDpiScalingHelper.ScaleToCurrentDpiY(allowedVariation));
		}

		public static void AssertControlHeight(int expected, Control control, int allowedVariation = 1)
		{
			AssertControlHeight(null, expected, control, allowedVariation);
		}

		public static void AssertControlDimensions(string message, int expectedWidth, int expectedHeight, Control control, int allowedVariation = 1)
		{
			AssertionWithHtml.CombineAssertions(() =>
			{
				AssertControlWidth(message, expectedWidth, control, allowedVariation);
				AssertControlHeight(message, expectedHeight, control, allowedVariation);
			});
		}

		public static void AssertControlDimensions(int expectedWidth, int expectedHeight, Control control, int allowedVariation = 1)
		{
			AssertControlDimensions(null, expectedWidth, expectedHeight, control, allowedVariation);
		}

		public static void AssertControlXLocation(string message, int expected, Control control, int allowedVariation = 1)
		{
			Assertion.AssertCloseEnough(message, ControlDpiScalingHelper.ScaleToCurrentDpiX(expected), control.Location.X, ControlDpiScalingHelper.ScaleToCurrentDpiX(allowedVariation));
		}

		public static void AssertControlXLocation(int expected, Control control, int allowedVariation = 1)
		{
			AssertControlXLocation(null, expected, control, allowedVariation);
		}

		public static void AssertControlYLocation(string message, int expected, Control control, int allowedVariation = 1)
		{
			Assertion.AssertCloseEnough(message, ControlDpiScalingHelper.ScaleToCurrentDpiY(expected), control.Location.Y, ControlDpiScalingHelper.ScaleToCurrentDpiY(allowedVariation));
		}

		public static void AssertControlYLocation(int expected, Control control, int allowedVariation = 1)
		{
			AssertControlYLocation(null, expected, control, allowedVariation);
		}

		public static void AssertControlLocation(string message, int expectedX, int expectedY, Control control, int allowedVariation = 1)
		{
			AssertionWithHtml.CombineAssertions(() =>
			{
				AssertControlXLocation(message, expectedX, control, allowedVariation);
				AssertControlYLocation(message, expectedY, control, allowedVariation);
			});
		}

		public static void AssertControlLocation(int expectedX, int expectedY, Control control, int allowedVariation = 1)
		{
			AssertControlLocation(null, expectedX, expectedY, control, allowedVariation);
		}

		public static int GetControlAbsoluteLeft(Control control, Control masterParent)
		{
			var left = control.Left;

			if (control.Parent != masterParent)
			{
				left += GetControlAbsoluteLeft(control.Parent, masterParent);
			}

			return left;
		}

		public static int GetControlAbsoluteRight(Control control, Control masterParent)
		{
			var right = control.Right;

			if (control.Parent != masterParent)
			{
				right += GetControlAbsoluteLeft(control.Parent, masterParent);
			}

			return right;
		}

		public static int GetControlAbsoluteTop(Control control, Control masterParent)
		{
			var top = control.Top;

			if (control.Parent != masterParent)
			{
				top += GetControlAbsoluteTop(control.Parent, masterParent);
			}

			return top;
		}

		public static int GetControlAbsoluteBottom(Control control, Control masterParent)
		{
			var bottom = control.Bottom;

			if (control.Parent != masterParent)
			{
				bottom += GetControlAbsoluteTop(control.Parent, masterParent);
			}

			return bottom;
		}

		public static IEnumerable<T> FindControls<T>(Control parent, bool searchAllChildren = true) where T : Control
		{
			var result = new List<T>();

			foreach (Control control in parent.Controls)
			{
				var controlAsT = control as T;

				if (controlAsT != null)
				{
					result.Add(controlAsT);
				}

				if (searchAllChildren)
				{
					result.AddRange(FindControls<T>(control));
				}
			}

			return result.Distinct();
		}
	}
}
