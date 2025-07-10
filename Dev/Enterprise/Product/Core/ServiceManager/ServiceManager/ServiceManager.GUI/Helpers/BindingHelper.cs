using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;

namespace Enterprise.ServiceManager.GUI
{
	class BindingHelper
	{
		public static void UpdateControlBindTos(Control control, string bindTo)
		{
			foreach (Control childControl in control.Controls)
			{
				if (!string.IsNullOrEmpty(childControl.GetBindingMember()))
				{
					childControl.SetBindingMember(BindToWithDot(bindTo) + childControl.GetBindingMember());
				}

				if (childControl is IBindToList bindToList && !string.IsNullOrEmpty(bindToList.BindToList))
				{
					bindToList.BindToList = BindToWithDot(bindTo) + bindToList.BindToList;
				}

				UpdateControlBindTos(childControl, bindTo);
			}
		}

		static string BindToWithDot(string bindTo)
		{
			var result = bindTo;
			if (result != null && !result.EndsWith("."))
			{
				result += ".";
			}
			return result;
		}
	}
}
