using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI
{
	internal static class ControlStatisticsDescription
	{
		public static string GetDescription(Control initialControl)
		{
			return GetControlHierarchy(initialControl) + "; " + GetFormText(initialControl);
		}

		#region Implementation

		static string GetControlHierarchy(Control initialControl)
		{
			string result = initialControl.Name;
			Control parentControl = initialControl.Parent;
			while (parentControl != null)
			{
				result = parentControl.Name + "." + result;
				parentControl = parentControl.Parent;
			}
			return result;
		}

		static string GetFormText(Control initialControl)
		{
			string result = string.Empty;
			Control control = initialControl;
			while (control != null)
			{
				if (control is Form)
				{
					result = ((Form)control).Text;
					break;
				}
				control = control.Parent;
			}
			return result;
		}

		#endregion
	}
}
