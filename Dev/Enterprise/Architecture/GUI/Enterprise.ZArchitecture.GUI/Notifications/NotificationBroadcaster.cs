using System.Windows.Forms;
using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IListenForNotifications
	{
		void NotifyAboutStateOfChildControl(Control control, INotificationType state);
		void NotifyAboutVisibilityChangeOfChildControl(Control control);
	}

	public class NotificationBroadcaster
	{
		static readonly NotificationBroadcaster instance = new NotificationBroadcaster();

		public virtual void BroadcastNotificationsChange(INotificationType state, Control control)
		{
			var current = control.Parent;
			while (current != null)
			{
				var listener = current as IListenForNotifications;
				if (listener != null)
				{
					listener.NotifyAboutStateOfChildControl(control, state);
				}
				current = current.Parent;
			}
		}

		public virtual void BroadcastVisibilityChange(Control control)
		{
			var current = control.Parent;
			while (current != null)
			{
				var listener = current as IListenForNotifications;
				if (listener != null)
				{
					listener.NotifyAboutVisibilityChangeOfChildControl(control);
				}
				current = current.Parent;
			}
		}

		public static NotificationBroadcaster Instance
		{
			get { return instance; }
		}
	}
}
