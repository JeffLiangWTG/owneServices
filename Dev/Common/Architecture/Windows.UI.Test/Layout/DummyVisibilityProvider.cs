using System;

namespace CargoWise.Windows.UI.Layout.Testing
{
	sealed class DummyVisibilityProvider : IVisibilityProvider
	{
		public bool Visible
		{
			get { return visible; }
			set
			{
				if (visible != value)
				{
					visible = value;

					if (VisibleChanged != null)
					{
						VisibleChanged(this, EventArgs.Empty);
					}
				}
			}
		}
		bool visible;

		public event EventHandler VisibleChanged;
	}
}
