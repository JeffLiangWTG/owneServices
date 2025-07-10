using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI.Controls;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class SuspendableLayoutControlBasher : IControlBasher
	{
		public void Bash(Control control, INotifications notifications)
		{
			if (control == null)
			{
				return;
			}

			var isLayoutSuspendedProperty = IsLayoutSuspendedProperty;
			if (isLayoutSuspendedProperty != null)
			{
				var isLayoutSuspended = (bool)isLayoutSuspendedProperty.GetValue(control);
				if (isLayoutSuspended)
				{
					notifications.AddError(ControlDescription.GetControlPath(control) + " - control layout is suspended. Please check that each SuspendLayout() has matching ResumeLayout().");
				}
			}
		}

		static PropertyInfo IsLayoutSuspendedProperty =>
			isLayoutSuspendedProperty ?? (isLayoutSuspendedProperty = typeof(Control).GetProperty("IsLayoutSuspended", BindingFlags.Instance | BindingFlags.NonPublic));

		[ThreadStatic]
		static PropertyInfo isLayoutSuspendedProperty;

		public static bool ShouldBash(Control control)
		{
			return
				control is ScrollableControl || // Includes Form, UserControl, Panel, TabPage, ContainerControl
				control is TabControl ||
				control is GroupBox ||
				control.Controls.Count > 0;
		}
	}
}
