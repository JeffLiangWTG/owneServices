using System.Windows.Forms;

using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.GUI
{
	public class Statusbar
	{
		public virtual void Initialize(Control control)
		{
			this.control = control;
		}

		internal void Initialize(IUpdateStatusBar statusBar)
		{
			this.statusBar = statusBar;
		}

		Control control;

		public virtual bool CanUpdate()
		{
			return StatusbarCore != null;
		}

		public virtual void Update(string notification, INotificationType state)
		{
			StatusbarCore.UpdateStatusBar(notification, state);
		}

		IUpdateStatusBar StatusbarCore
		{
			get { return statusBar ?? (statusBar = GetUpdateStatusBar(control)); }
		}
		IUpdateStatusBar statusBar;

		static IUpdateStatusBar GetUpdateStatusBar(Control control)
		{
			while (control != null && !(control is IUpdateStatusBar))
			{
				control = control.Parent;
			}
			return control as IUpdateStatusBar;
		}
	}
}
