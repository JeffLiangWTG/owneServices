using System.Reflection;
using System.Windows.Forms;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[CodeAlive("Helper class to find a control from UI")]
	public static class GUITestHelper
	{
		#region FindControl

		public static T FindControl<T>(Control.ControlCollection controls, string name = null) where T : Control
		{
			foreach (Control control in controls)
			{
				var controlAsT = control as T;
				if (controlAsT != null && (string.IsNullOrEmpty(name) || controlAsT.Name == name))
				{
					return controlAsT;
				}

				var childControl = FindControl<T>(control.Controls, name);
				if (childControl != null)
				{
					return childControl;
				}
			}

			return null;
		}

		#endregion

		public static T GetProtectedField<T, U>(this U control, string controlName) where U : ZForm
		{
			var fieldInfo = typeof(ZForm).GetField(controlName, BindingFlags.Instance | BindingFlags.NonPublic);
			return (T)fieldInfo.GetValue(control);
		}
	}
}
