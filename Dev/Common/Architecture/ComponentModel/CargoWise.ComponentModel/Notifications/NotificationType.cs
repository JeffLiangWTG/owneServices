using System;
using System.Collections.Generic;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// Describes a notification type.
	/// </summary>
	[Serializable]
	[WTG.StaticAnalysis.Annotation.Immutable]
	public class NotificationType : INotificationType
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error reporting")]
		public NotificationType(int severity, bool isFatal)
			: this(null, severity, isFatal, "Information")
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error reporting")]
		public NotificationType(string name, int severity, bool isFatal)
			: this(name, severity, isFatal, "Information")
		{
		}

		public NotificationType(string name, int severity, bool isFatal, string enumValueName)
		{
			this.name = name;
			this.severity = severity;
			this.isFatal = isFatal;
			this.enumValueName = enumValueName;
		}

		public string Name { get { return name; } }
		public bool IsFatal { get { return isFatal; } }
		public int Severity { get { return severity; } }

		readonly string name;
		readonly bool isFatal;
		readonly int severity;

		/// <summary>
		/// In general comes from System.Forms.MessageBoxIcon enum. Put just a name here.
		/// </summary>
		public string EnumValueName { get { return enumValueName; } }

		readonly string enumValueName;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1058:DoNotUseDebuggerIsAttached", Justification = "Baseline")]
		public override string ToString()
		{
#if DEBUG
			if (!System.Diagnostics.Debugger.IsAttached)
			{
				ErrorReporter.ReportOnce("NotificationTypes.ToString()", "NotificationType.ToString() should not be used, use ZNotificationsExtensions.NotificationTypeName(INotificationType notification, Constants.PluralState pluralState) instead", new InvalidOperationException());
			}
#endif
			return Name ?? base.ToString();
		}

		/// <summary>
		/// An information notification that doesn't prevent a save.
		/// </summary>
		public static INotificationType Information
		{
			get { return information; }
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error reporting")]
		readonly static INotificationType information = new NotificationType("Information", 300, false, "Information");

		/// <summary>
		/// A warning notification that doesn't prevent a save.
		/// </summary>
		public static INotificationType Warning
		{
			get { return warning; }
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error reporting")]
		readonly static INotificationType warning = new NotificationType("Warning", 200, false, "Warning");

		/// <summary>
		/// An error notification that prevents a save.
		/// </summary>
		public static INotificationType Error
		{
			get { return error; }
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error reporting")]
		readonly static INotificationType error = new NotificationType("Error", 100, true, "Error");
	}

	public static class NotificationProviderConvenienceMethodExtensions
	{
		public static IEnumerable<INotification> GetFatalNotifications(this IEnumerable<INotification> notifications)
		{
			foreach (INotification notification in notifications)
			{
				if (notification.Type.IsFatal)
				{
					yield return notification;
				}
			}
		}

		#region Error

		/// <summary>
		/// Are there any notifications of type NotificationType.Error?
		/// </summary>
		public static bool HasErrors(this IEnumerable<INotification> notifications)
		{
			Argument.NotNull(notifications, nameof(notifications));
			return notifications.HasNotifications(NotificationType.Error);
		}

		/// <summary>
		/// Are there any notifications of type NotificationType.Error?
		/// </summary>
		public static bool HasErrors(this INotificationProvider provider)
		{
			Argument.NotNull(provider, nameof(provider)); // Suggested By ReviewBot 
			return provider.HasNotifications(NotificationType.Error);
		}

		/// <summary>
		/// Get notifications of type NotificationType.Error.
		/// </summary>
		public static IEnumerable<INotification> GetErrors(this IEnumerable<INotification> notifications)
		{
			Argument.NotNull(notifications, nameof(notifications));
			return notifications.GetNotifications(NotificationType.Error);
		}

		/// <summary>
		/// Get notifications of type NotificationType.Error.
		/// </summary>
		public static IEnumerable<INotification> GetErrors(this INotificationProvider provider)
		{
			Argument.NotNull(provider, nameof(provider)); // Suggested By ReviewBot 
			return provider.Notifications.GetErrors();
		}

		#endregion

		#region Warning

		/// <summary>
		/// Are there any notifications of type NotificationType.Warning?
		/// </summary>
		public static bool HasWarnings(this IEnumerable<INotification> notifications)
		{
			Argument.NotNull(notifications, nameof(notifications));
			return notifications.HasNotifications(NotificationType.Warning);
		}

		/// <summary>
		/// Are there any notifications of type NotificationType.Warning?
		/// </summary>
		public static bool HasWarnings(this INotificationProvider provider)
		{
			Argument.NotNull(provider, nameof(provider)); // Suggested By ReviewBot 
			return provider.HasNotifications(NotificationType.Warning);
		}

		/// <summary>
		/// Get notifications of type NotificationType.Warning.
		/// </summary>
		public static IEnumerable<INotification> GetWarnings(this IEnumerable<INotification> notifications)
		{
			Argument.NotNull(notifications, nameof(notifications));
			return notifications.GetNotifications(NotificationType.Warning);
		}

		/// <summary>
		/// Get notifications of type NotificationType.Warning.
		/// </summary>
		public static IEnumerable<INotification> GetWarnings(this INotificationProvider provider)
		{
			Argument.NotNull(provider, nameof(provider)); // Suggested By ReviewBot 
			return provider.Notifications.GetWarnings();
		}

		#endregion
	}
}
