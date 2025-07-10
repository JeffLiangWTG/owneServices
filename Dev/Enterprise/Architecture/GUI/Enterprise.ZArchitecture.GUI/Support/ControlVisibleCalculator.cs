using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common.Testing;

namespace Enterprise.Core.Forms
{
	// Control has this code. We are interested in the control visible, not the parent.
	//internal virtual bool GetVisibleCore()
	//{
	//      if (!this.GetState(2))
	//      {
	//          return false;
	//      }
	//      if (this.ParentInternal == null)
	//      {
	//          return true;
	//      }
	//      return this.ParentInternal.GetVisibleCore();
	//}

	public static class ControlVisibleCalculator
	{
		// Checks if the control is set to visible, taking into account that tab pages go invisible when they are not selected.
		public static bool IsSetVisible(Control control)
		{
			var visible = true;
			var c = control;
			while (visible && c != null)
			{
				if (!(c is TabPage))
				{
					visible &= GetControlVisible(c);
				}
				c = c.Parent;
			}
			return visible;
		}

		#if WINZOR

		static bool GetControlVisible(Control control) => control.ControlVisible;

		#else

		static bool GetControlVisible(Control control)
		{
			return (bool)GetStateMethodInfo.Invoke(control, new object[] { 2 }); // 2 is from the decompiled code
		}

		static MethodInfo GetStateMethodInfo
		{
			get
			{
				if (getStateInfo == null)
				{
					getStateInfo = typeof(Control).GetMethod("GetState", BindingFlags.Instance | BindingFlags.NonPublic);
				}
				return getStateInfo;
			}
		}
		[SuppressThreadStaticFieldMessage]
		static MethodInfo getStateInfo;

		#endif
	}
}
