using System;
using System.Reflection;
using System.Windows.Forms;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Windows.UI
{
	public static class BindingExtensions
	{
		/// <summary>
		/// Add this Binding to a control and force the binding to commence even if the control is
		/// not visible. This is used for binding to Notifications for example when a tab isn't shown
		/// but the tab's notification icon needs to be updated based on the state of the control.
		/// </summary>
		public static void ForceBinding(this Binding binding, Control control)
		{
			if (!control.IsHandleCreated && control.Parent != null)
			{
				try
				{
					// force Control.Created to be true so binding can start now
					SetControlState(control, 1, true);
					BindingContext.UpdateBinding(control.BindingContext, binding);
					SetControlState(control, 1, false);
				}
				catch (ArgumentException)
				{
					// This exception may be thrown intermittently.
					// The only side-effect will be a notification may be rendered incorrectly, and even this is very unlikely to occur
				}
			}
		}

		static SetControlStateDelegate SetControlState
		{
			get
			{
				if (setControlState == null)
				{
					MethodInfo method = typeof(Control).GetMethod("SetState", BindingFlags.NonPublic | BindingFlags.Instance);
					setControlState = (SetControlStateDelegate)Delegate.CreateDelegate(typeof(SetControlStateDelegate), method);
				}
				return setControlState;
			}
		}
		[ThreadSafe]
		static SetControlStateDelegate setControlState;
		delegate void SetControlStateDelegate(Control control, int flag, bool value);
	}
}
