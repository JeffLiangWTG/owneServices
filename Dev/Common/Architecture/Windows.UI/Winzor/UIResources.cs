using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	public class UIResources
	{
		public static UIResources Instance { get; } = new UIResources();

		public IEnumerable<string> GetAllControls(Control control)
		{
			yield return control.GetType().FullName;

			foreach (var c in control.Controls)
			{
				foreach (var controlTypeName in GetAllControls(c))
				{
					yield return controlTypeName;
				}
			}
		}

		public void MonitorUIResourcesInBackground(EventHandler resourceNearlyOverflowHandler)
		{
		}
	}
}
