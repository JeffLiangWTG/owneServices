using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI.Controls;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZToolStripBasher : IControlBasher
	{
		public void Bash(Control control, INotifications notifications)
		{
			var toolStrip = control as ZToolStrip;

			if (toolStrip != null)
			{
				if (toolStrip.Items.Count > 0)
				{
					var totalWidth = 0;

					foreach (ToolStripItem item in toolStrip.Items)
					{
						if (item.Available)
						{
							if (toolStrip.Orientation == Orientation.Horizontal)
							{
								totalWidth += item.Width;
							}
							else
							{
								totalWidth = item.Width > totalWidth ? item.Width : totalWidth;
							}
						}
					}

					if (totalWidth > toolStrip.Width)
					{
						notifications.AddError("Control buttons do not fit the panel. Control : " + ControlDescription.GetControlPathAndLocation(control) +
							", Control Width : " + toolStrip.Width + ", Control Items Width : " + totalWidth);
					}
				}
			}
		}
	}
}
