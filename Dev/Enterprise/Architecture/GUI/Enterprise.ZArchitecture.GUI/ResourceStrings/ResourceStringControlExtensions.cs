using System.Windows.Forms;

using CargoWise.Common.Testing;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	public static class ResourceStringControlExtensions
	{
		public static Control GetResourceStringIdentifyingControl(this Control control)
		{
			var result = control.IsDisposed ? null : (Control)control.GetUserData(UserDataKey);
			if (result != null && result.IsDisposed)
			{
				SetResourceStringIdentifyingControl(control, null);
				result = null;
			}
			return result;
		}

		public static void SetResourceStringIdentifyingControl(this Control control, Control identifyingControl)
		{
			control.SetUserData(UserDataKey, identifyingControl);
		}

		[SuppressThreadStaticFieldMessage]
		static readonly int UserDataKey = ControlExtensions.CreateUserDataKey();
	}
}
