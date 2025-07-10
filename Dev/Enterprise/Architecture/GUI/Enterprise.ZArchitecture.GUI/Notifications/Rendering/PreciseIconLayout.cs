using System.Drawing;

using CargoWise.EntityFramework;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.Notifications
{
	abstract class PreciseIconLayout : IconLayoutBase
	{
		public sealed override Rectangle Compute(Rectangle area, Size clip)
		{
			var align = GetAlignmentDirection();
			var width = GetPreciseWidth();

			var pos = align > 0 ? width : area.Width - clip.Width - width;
			var room = width + clip.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(XOffset);

			var y = area.Height / 2 - clip.Height / 2;
			var x = area.Width >= room ? pos + align : width / 2 - clip.Width / 2;

			var rectangle = ControlDpiScalingHelper.NewScaledRectangle(x, y, clip.Width, clip.Height, false);
#if !WINZOR
			return rectangle;
#else
			if (Target is ZCheckBox)
			{
				return rectangle;
			}
			return Target.RectangleToScreen(rectangle);
#endif
		}

		public sealed override void ResizeControl(bool revert)
		{
			if (!ShouldResize)
			{
				return;
			}

			var requiredSpace = NotificationIconScheme.Instance.GetMiniImage(NotificationType.Error).Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(XOffset);
			var hasRoom = Target.Width > GetPreciseWidth() + requiredSpace;

			if (wasResized && revert)
			{
				Resize(0);
				wasResized = false;
			}
			else if (!hasRoom)
			{
				Resize(requiredSpace);
				wasResized = true;
			}
		}
		bool wasResized;

		void Resize(int width)
		{
			AddWidth(width);
			Target.PerformLayout();
		}

		/// <summary>
		/// Gets or sets a value indicating whether control should be resized when 
		/// control width plus icon width doesn't fit.
		/// </summary>
		/// <value><c>true</c> if should resize; otherwise, <c>false</c>.</value>
		protected virtual bool ShouldResize { get { return false; } }

		/// <summary>
		/// Should return precise width of the control.
		/// </summary>
		protected abstract int GetPreciseWidth();

		/// <summary>
		/// Adds the given width to control's width.
		/// </summary>
		/// <param name="width">The width.</param>
		protected virtual void AddWidth(int width) { }

		/// <summary>
		/// Should return 1 or -1 depending on control alignment.
		/// </summary>
		/// <returns></returns>
		protected abstract int GetAlignmentDirection();
	}
}
