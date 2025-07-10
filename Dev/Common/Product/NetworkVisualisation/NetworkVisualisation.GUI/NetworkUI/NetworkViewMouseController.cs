using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CargoWise.Common;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class NetworkViewMouseController
	{
		public NetworkViewMouseController(NetworkView networkView)
		{
			this.networkView = networkView;
		}

		readonly NetworkView networkView;

		#region Public Properties And Methods

		public bool IsDragging { get; private set; }

		public bool IsLeftMouseDown => leftClickedElement != null;

		public bool IsLeftButtonPressed(MouseEventArgs e)
		{
			return e.LeftButton == MouseButtonState.Pressed;
		}

		public bool IsRightButtonPressed(MouseEventArgs e)
		{
			return e.RightButton == MouseButtonState.Pressed;
		}

		public bool IsCtrlPressed
		{
			get
			{
#if DEBUG
				if (ControlPressed_ForTest)
				{
					return true;
				}
#endif
				return (Keyboard.Modifiers & ModifierKeys.Control) != 0;
			}
		}

		public void ProcessLeftMouseDownOnElement(MouseButtonEventArgs e, UIElement element)
		{
			leftClickedElement = element;
			RememberLastMousePointForDragging(e);

			CaptureMouse(leftClickedElement);
		}

		public void ProcessLeftMouseUp()
		{
			StopDraggingLeftClickedElement();
		}

		public void ProcessLostMouseCapture()
		{
			StopDraggingLeftClickedElement();
		}

		public void StartDraggingLeftClickedElement()
		{
			if (!IsLeftMouseDown)
			{
				ErrorReporter.ReportOnce("Left clicked element should be defined by this moment.");
			}

			IsDragging = true;
		}

		public void StopDraggingLeftClickedElement()
		{
			if (IsLeftMouseDown) // ReleaseMouseCapture may generate LostMouseCapture event which causes this method to be called twice so we need to check if we already processed stopping
			{
				IsDragging = false;

				ReleaseMouseCapture(leftClickedElement);
			}
		}

		public void ProcessDoubleClick(MouseButtonEventArgs e)
		{
			RememberLastMousePointForDragging(e);
			ReleaseMouseCapture(leftClickedElement);
		}

		public void ProcessMouseMoveWhileDragging(MouseEventArgs e, Action<Vector> callbackIfMoved)
		{
			var curMousePoint = GetCurrentPosition(e);
			var offset = curMousePoint - lastMousePointForDragging;

			if (offset.X != 0.0 || offset.Y != 0.0)
			{
				RememberLastMousePointForDragging(e);
				callbackIfMoved(offset);
			}
		}

		public bool IsFarEnoughToStartDragging(MouseEventArgs e, double dragThreshold)
		{
			var dragDelta = GetCurrentPosition(e) - lastMousePointForDragging;
			return Math.Abs(dragDelta.Length) > dragThreshold;
		}

		#endregion

		#region Implementation

		UIElement leftClickedElement;

		Point lastMousePointForDragging;

		Point GetCurrentPosition(MouseEventArgs e)
		{
#if DEBUG
			if (CurrentMousePosition_ForTest != null)
			{
				return CurrentMousePosition_ForTest.Value;
			}
#endif
			return e.GetPosition(networkView);
		}
		void RememberLastMousePointForDragging(MouseEventArgs e)
		{
			lastMousePointForDragging = GetCurrentPosition(e);
		}

		void CaptureMouse(UIElement element)
		{
			element.CaptureMouse();
		}

		void ReleaseMouseCapture(UIElement element)
		{
			element?.ReleaseMouseCapture();
			leftClickedElement = null;
		}

		#endregion

		#region For Testing
#if DEBUG

		public UIElement ElementThatCapturedMouse => leftClickedElement; // We cannot use Mouse.Captured or Element.IsMouseCaptured for tests as they will fail on DAT, so we need to remember the element which captured mouse in another way

		public Point? CurrentMousePosition_ForTest { get; private set; }

		public void SetCurrentMousePosition_ForTest(int x, int y)
		{
			CurrentMousePosition_ForTest = new Point(x, y);
		}

		public bool ControlPressed_ForTest { get; set; }

		#region Test Helper Methods

		public MouseButtonEventArgs CreateLeftMouseDownEvent(int x, int y)
		{
			SetCurrentMousePosition_ForTest(x, y);
			return new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = UIElement.MouseDownEvent };
		}

		public MouseButtonEventArgs CreateLeftMouseUpEvent(int x, int y)
		{
			SetCurrentMousePosition_ForTest(x, y);
			return new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = UIElement.MouseUpEvent };
		}

		public MouseButtonEventArgs CreateRightMouseDownEvent(int x, int y)
		{
			SetCurrentMousePosition_ForTest(x, y);
			return new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right) { RoutedEvent = UIElement.MouseDownEvent };
		}

		public MouseButtonEventArgs CreateRightMouseUpEvent(int x, int y)
		{
			SetCurrentMousePosition_ForTest(x, y);
			return new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right) { RoutedEvent = UIElement.MouseUpEvent };
		}

		public MouseEventArgs CreateMouseMoveEvent(int x, int y)
		{
			SetCurrentMousePosition_ForTest(x, y);
			return new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = UIElement.MouseMoveEvent };
		}

		public MouseEventArgs CreateLostMouseCaptureEvent()
		{
			return new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = UIElement.LostMouseCaptureEvent };
		}

		public MouseButtonEventArgs CreateMouseDoubleClickEvent(int x, int y)
		{
			SetCurrentMousePosition_ForTest(x, y);
			return new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Control.MouseDoubleClickEvent };
		}

		#endregion

#endif
		#endregion
	}
}
