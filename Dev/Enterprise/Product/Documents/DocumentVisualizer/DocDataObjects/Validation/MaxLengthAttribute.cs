using System;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentVisualizer.DocDataObjects
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class MaxLengthAttribute : System.Attribute
	{
		public MaxLengthAttribute(NotificationTypes notificationType, int maxLength)
		{
			NotificationType = notificationType;
			MaxLength = maxLength;
		}

		public NotificationTypes NotificationType { get; }
		public int MaxLength { get; }
	}
}