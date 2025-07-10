#if DEBUG

using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI.Controls;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.ZArchitecture.GUI
{
	public class ContainerControlBasher : IControlBasher
	{
		public virtual void Bash(Control control, INotifications notifications)
		{
			var containerControl = control as ContainerControl;
			if (containerControl != null)
			{
				var form = containerControl as Form;
				var parentForm = form ?? containerControl.ParentForm;
				if (parentForm != null && parentForm.GetType().FullName == "Enterprise.DocumentEngine.RuntimeOptions.RuntimeOptionsForm")
				{
					if (form != null)
					{
						if (form.AutoScaleMode != AutoScaleMode.None)
						{
							notifications.AddError(ControlDescription.GetControlPathAndLocation(form) + " - AutoScaleMode should be set to 'AutoScaleMode.None' on all Forms.");
						}
					}
					else
					{
						if (containerControl.AutoScaleMode != AutoScaleMode.Inherit)
						{
							notifications.AddError(ControlDescription.GetControlPathAndLocation(form) + " - AutoScaleMode should be set to 'AutoScaleMode.Inherit' on all ContainerContol subclasses.");
						}
					}
				}
			}
		}
	}
}
#endif
