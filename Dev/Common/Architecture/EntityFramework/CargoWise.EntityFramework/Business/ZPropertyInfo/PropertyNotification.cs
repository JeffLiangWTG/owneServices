using System;
using CargoWise.ComponentModel;

namespace CargoWise.EntityFramework
{
	[Serializable]
	public class PropertyNotification : Notification
	{
		public PropertyNotification(string propertyName, INotificationType notificationType, string message)
			: base(notificationType, message)
		{
			this.PropertyName = propertyName;
		}

		public string PropertyName { get; private set; }

		public override INotification ReplaceMessage(string message)
		{
			return new PropertyNotification(PropertyName, Type, message);
		}
	}
}
