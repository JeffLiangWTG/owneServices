using System;
using System.Reflection;
using System.Windows.Forms;

namespace Enterprise.Registry.GUI.Testing
{
	static class CheckBoxOnClick
	{
		public static void OnClick(this CheckBox checkBox, EventArgs e)
		{
			typeof(Control).InvokeMember("OnClick", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, checkBox, new object[] { e });
		}
	}
}
