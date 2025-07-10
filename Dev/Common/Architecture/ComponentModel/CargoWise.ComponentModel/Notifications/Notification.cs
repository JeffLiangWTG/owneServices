using System;
using System.Diagnostics;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// A Text notification for a business entity.
	/// </summary>
	[Serializable]
	[DebuggerDisplay("Type={Type},Message={Message}")]
	public class Notification : INotification
	{
		protected Notification(INotificationType notificationType)
		{
			Argument.NotNull(notificationType, nameof(notificationType));
			Type = notificationType;
		}

		public Notification(INotificationType notificationType, string message)
			: this(notificationType)
		{
			Argument.NotNull(notificationType, nameof(notificationType));
			Message = message;
		}

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			var rhs = obj as Notification;
			return rhs != null && Type == rhs.Type && Message == rhs.Message;
		}

		public override int GetHashCode()
		{
			return Message.GetHashCode();
		}

		#endregion

		#region INotification Members

		public INotificationType Type { get; private set; }
		public virtual string Message { get; private set; }

		public virtual INotification ReplaceMessage(string message)
		{
			return new Notification(Type, message);
		}

		#endregion
	}
}
