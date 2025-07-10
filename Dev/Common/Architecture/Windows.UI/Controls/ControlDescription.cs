using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Windows.UI.Controls
{
	public static class ControlDescription
	{
		public static string GetControlPath(Control control)
		{
			var names = new List<string>();
			var current = control;

			while (current != null)
			{
				names.Insert(0, current.Name);
				current = current.Parent;
			}

			return string.Join(" : ", names) + " (" + control.GetType().Name + ")";
		}

		public static string GetControlPathAndLocation(Control control)
		{
			var result = GetControlPath(control) + (NoResString)" at ";
			if (control.Parent != null)
			{
				result += control.Parent.PointToScreen(control.Location).ToString();
			}
			else
			{
				result += control.Location.ToString();
			}
			return result;
		}
	}
}
