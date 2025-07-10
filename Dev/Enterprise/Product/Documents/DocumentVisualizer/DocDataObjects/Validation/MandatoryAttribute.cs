using System;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class MandatoryAttribute : System.Attribute
	{
		public MandatoryAttribute(NotificationTypes notificationType)
		{
			NotificationType = notificationType;
		}

		public NotificationTypes NotificationType { get; }
	}
}