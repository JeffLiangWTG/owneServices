using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CargoWise.Main.Navigation.WPF;

namespace CargoWise.NetworkVisualisation.GUI
{
	public class ResizeThumb : Thumb
	{
		public ResizeThumb()
		{
			DragDelta += ResizeThumb_DragDelta;
			DragStarted += ResizeThumb_DragStarted;
			DragCompleted += ResizeThumb_DragCompleted;
		}

		void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
		{
			var item = GetElementToResize();
			var dragResizable = item as IDragResizable;
			if (dragResizable != null)
			{
				dragResizable.NotifyResizing(e);
			}
			else if (item != null)
			{
				double deltaVertical = 0.0, deltaHorizontal = 0.0;
				WpfUtils.UpdateFocusBinding();

				switch (VerticalAlignment)
				{
					case VerticalAlignment.Bottom:
						deltaVertical = Math.Min(-e.VerticalChange, item.ActualHeight - item.MinHeight);
						break;

					case VerticalAlignment.Top:
						deltaVertical = Math.Min(e.VerticalChange, item.ActualHeight - item.MinHeight);
						Canvas.SetTop(item, Canvas.GetTop(item) + deltaVertical);
						break;
				}

				item.Height = item.ActualHeight - deltaVertical;

				switch (HorizontalAlignment)
				{
					case HorizontalAlignment.Left:
						deltaHorizontal = Math.Min(e.HorizontalChange, item.ActualWidth - item.MinWidth);
						Canvas.SetLeft(item, Canvas.GetLeft(item) + deltaHorizontal);
						break;

					case HorizontalAlignment.Right:
						deltaHorizontal = Math.Min(-e.HorizontalChange, item.ActualWidth - item.MinWidth);
						break;
				}

				item.Width = item.ActualWidth - deltaHorizontal;
			}

			e.Handled = true;
		}

		void ResizeThumb_DragStarted(object sender, DragStartedEventArgs e)
		{
			var item = GetElementToResize() as IDragResizable;
			if (item != null)
			{
				item.NotifyResizeStarted(e);
			}
		}

		void ResizeThumb_DragCompleted(object sender, DragCompletedEventArgs e)
		{
			var item = GetElementToResize() as IDragResizable;
			if (item != null)
			{
				item.NotifyResizeCompleted(e);
			}
		}

		Control GetElementToResize()
		{
			var control = TemplatedParent as Control;

			if (control != null)
			{
				control = WpfUtils.FindVisualParentWithType<NodeItem>(control) ?? control;
			}

			return control;
		}
	}
}
