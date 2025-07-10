using System.Web.UI;
using System.Web.UI.WebControls;

using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	#region SuppressResourceStringsCheckRegion

	public class ZControlNotifications : CompositeControl
	{
		#region Constructors

		public ZControlNotifications()
			: base()
		{
			InitializeWrapper();
		}

		public ZControlNotifications(ZPage page)
			: this()
		{
			this.Page = page;
		}

		#endregion

		#region Overrides

		public void RenderBeginTag(HtmlTextWriter writer, INotificationProvider control)
		{
			if (ShowNotifications(control))
			{
				base.RenderBeginTag(writer);
			}
		}

		public void RenderEndTag(HtmlTextWriter writer, INotificationProvider control)
		{
			if (ShowNotifications(control))
			{
				ZNotificationIcon notificationIcon = null;
				if (control.HasErrors())
				{
					notificationIcon = new ZErrorNotificationIcon(control.Notifications);
				}
				else if (control.HasMessageErrors())
				{
					notificationIcon = new ZMessageErrorNotificationIcon(control.Notifications);
				}
				else if (control.HasWarnings())
				{
					notificationIcon = new ZWarningNotificationIcon(control.Notifications);
				}
				if (notificationIcon != null)
				{
					IWebNotificationProvider controlWithWebNotifications = control as IWebNotificationProvider;
					if (controlWithWebNotifications != null)
					{
						if (!string.IsNullOrEmpty(controlWithWebNotifications.NotificationID))
						{
							notificationIcon.ID = controlWithWebNotifications.NotificationID;
						}
					}
					RenderNotificationIcon(writer, notificationIcon);
				}
				base.RenderEndTag(writer);
			}
		}

		#endregion

		#region Implementation

		public new ZPage Page
		{
			get
			{
				return (ZPage)base.Page;
			}
			set
			{
				base.Page = value;
			}
		}

		void RenderNotificationIcon(HtmlTextWriter writer, ZNotificationIcon notificationIcon)
		{
			notificationIcon.Page = this.Page;
			notificationIcon.RenderControl(writer);
		}

		bool ShowNotifications(INotificationProvider control)
		{
			bool displayNotifications = true;
			if (control is IUINotiicationsProvider)
			{
				displayNotifications = ((IUINotiicationsProvider)control).DisplayNotifications;
			}
			return displayNotifications && (control.HasErrors() || control.HasMessageErrors() || control.HasWarnings());
		}

		void InitializeWrapper()
		{
			SetStyle();
		}

		protected virtual void SetStyle()
		{
			Style.Add("display", "inline");
			Style.Add("white-space", "nowrap");
		}

		#endregion
	}

	#endregion
}
