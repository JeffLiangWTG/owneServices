using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public class EntityNotification : IEntityNotification
	{
		public EntityNotification(EntityNotifcationType type, string message, string parent)
		{
			this.notificationType = type;
			this.message = message;
			this.parent = parent;
		}

		public EntityNotification(EntityNotifcationType type, string message)
		{
			this.notificationType = type;
			this.message = message;
			this.parent = string.Empty;
		}

		public string Message
		{
			get { return message; }
		}
		readonly string message;

		public EntityNotifcationType NotificationType
		{
			get { return notificationType; }
		}
		readonly EntityNotifcationType notificationType;

		public string ParentName
		{
			get { return parent; }
		}
		readonly string parent;
	}
}
