using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls
{
	public abstract class ZNotificationIcon : Image
	{
		#region Contructors

		public ZNotificationIcon(IEnumerable<INotification> notifications)
			: base()
		{
			this.Notifications = notifications;
		}

		#endregion

		#region Overrides

		public override string ImageUrl => GetImageURL();

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			base.AddAttributesToRender(writer);

			var tooltip = NotificationMessagesText;
			if (!string.IsNullOrEmpty(tooltip))
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Title, tooltip);
			}
		}

		#endregion

		#region Implementation

		protected string NotificationMessagesText
		{
			get
			{
				string result = "";
				foreach (INotification notification in GetNotificationMessages().GetUniqueNotifications())
				{
					result += notification.Message;
				}
				return result;
			}
		}

		protected abstract IEnumerable<INotification> GetNotificationMessages();

		protected virtual string GetImageURL()
		{
			ZWebResource resource = new ZWebResource(typeof(ZControlNotifications), GetResourceName(), (ZPage)base.Page);
			resource.Extract();
			return resource.FileName;
		}

		protected abstract string GetResourceName();

		protected IEnumerable<INotification> Notifications;

		#endregion
	}
}
