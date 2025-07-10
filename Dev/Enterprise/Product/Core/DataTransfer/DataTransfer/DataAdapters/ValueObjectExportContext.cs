using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.DataTransfer.DataAdapters
{
	public class ValueObjectExportContext : IValueObjectExportContext
	{
		public ValueObjectExportContext(INotifications notifications)
		{
			this.Notifications = notifications;
		}

		public bool SimplifiedXML
		{
			get { return simplifiedXML != null ? simplifiedXML.Value : SystemDataRegistry.Instance.SimpleXMLExportFormat.Value; }
			set { simplifiedXML = value; }
		}

		bool? simplifiedXML;

		#region INotifications Members

		readonly INotifications Notifications;

		void INotifications.Add(INotification @event)
		{
			Notifications.Notify(@event);
		}

		public void QueryUser(IQueryUserEventArgs e)
		{
			Notifications.QueryUser(e);
		}

		public ZString LastNotificationMessage
		{
			get
			{
				ZString result = ZString.Empty;
				NotificationBuffer buffer = Notifications as NotificationBuffer;
				if (buffer != null && buffer.Events.Length > 0)
				{
					result = buffer.Events[buffer.Events.Length - 1].Message;
				}
				return result;
			}
		}

		#endregion

		public string ExportPurpose { get; set; }
	}
}
