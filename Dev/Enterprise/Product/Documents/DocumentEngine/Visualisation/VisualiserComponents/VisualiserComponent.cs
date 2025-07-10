using System;
using System.Drawing;
using static Enterprise.DocumentEngine.ControlDpiScalingHelper;

namespace Enterprise.DocumentEngine.Visualisation
{
	public abstract class VisualiserComponent
	{
		protected VisualiserComponent(Point location, Size size)
		{
			this.location = location;
			this.Size = size;
		}

		Point location;
		public Point Location
		{
			get { return location; }
		}

		public Size Size;

		internal void AddToLocationY(int valueScaled)
		{
			ControlDpiScalingHelper.SetY(ref location, location.Y + valueScaled, false);
		}

#if DEBUG
		public virtual string GetControlDescriptionForTesting()
		{
			var unscaledLocation = NewScaledPoint(UnscaleFromCurrentDpiX(Location.X), UnscaleFromCurrentDpiY(Location.Y), false);
			var unscaledSize = NewScaledSize(UnscaleFromCurrentDpiX(Size.Width), UnscaleFromCurrentDpiY(Size.Height), false);

			return "At: " + unscaledLocation.ToString() + "\r\n"
				+ "Size: " + unscaledSize.ToString() + "\r\n";
		}

		public virtual object GetRenderedControlValueForTesting()
		{
			return null;
		}

		public virtual bool IsModifiableForTesting
		{
			get { return false; }
		}

		public virtual void SimulateUserSettingValueForTesting(object value)
		{
			throw new InvalidOperationException("Cannot set the value on a non modifiable object.");
		}
#endif

	}
}
