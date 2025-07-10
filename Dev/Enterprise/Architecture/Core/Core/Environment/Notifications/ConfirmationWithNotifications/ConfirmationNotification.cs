using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.Core
{
	public sealed class ConfirmationNotification : NonPersistentBusinessObject
	{
		public ConfirmationNotification(NotificationTypes notificationType, string message)
		{
			Message = message;
			NotificationType = notificationType;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1507:Use nameof to express symbol names", Justification = "Unable to locate the member IsIgnored_ReadOnly")]
		[ReadOnlyMember("IsIgnored_ReadOnly")]
		[ResourceStringData("ConfirmationNotification.IsIgnored", Caption = "Ignore", FullDescription = "Ignore this notification")]
		public ZBool IsIgnored
		{
			get { return isIgnored; }
			set { SetNonPersistentPropertyValue(IsIgnoredInfo, ref isIgnored, value); }
		}

		ZBool isIgnored;

		public ZPropertyInfo IsIgnoredInfo
		{
			get { return GetZPropertyInfo(nameof(IsIgnored)); }
		}

		[ResourceStringData("ConfirmationNotification.Message", Caption = "Notification Message")]
		public ZString Message
		{
			get;
			private set;
		}

		public NotificationTypes NotificationType
		{
			get;
			private set;
		}
	}
}
