using CargoWise.ComponentModel;
using CargoWise.ResourceStrings.Grammar;

namespace CargoWise.EntityFramework
{
	public static class ZNotificationsExtensions
	{
		public static void AddMessageError(this INotifications notifications, string message)
		{
			notifications.Add(NotificationType.MessageError, message);
		}

		public static string NotificationTypeName(this INotificationType notification, PluralState pluralState, Capitalisation capitalisation = Capitalisation.Capitalised)
		{
			if (notification.Equals(CargoWise.ComponentModel.NotificationType.Error))
			{
				switch (capitalisation)
				{
					case Capitalisation.UpperCase:
						return pluralState == PluralState.Plural ? Res.GetString("NotificationType|Error|Plural|UpperCase", "ERRORS") : Res.GetString("NotificationType|Error|NonPlural|UpperCase", "ERROR");
					case Capitalisation.LowerCase:
						return pluralState == PluralState.Plural ? Res.GetString("NotificationType|Error|Plural|LowerCase", "errors") : Res.GetString("NotificationType|Error|NonPlural|LowerCase", "error");
					default:
						return pluralState == PluralState.Plural ? Res.GetString("NotificationType|Error|Plural", "Errors") : Res.GetString("NotificationType|Error|NonPlural", "Error");
				}
			}
			else if (notification.Equals(CargoWise.ComponentModel.NotificationType.Warning))
			{
				switch (capitalisation)
				{
					case Capitalisation.UpperCase:
						return pluralState == PluralState.Plural ? Res.GetString("NotificationType|Warning|Plural|UpperCase", "WARNINGS") : Res.GetString("NotificationType|Warning|NonPlural|UpperCase", "WARNING");
					case Capitalisation.LowerCase:
						return pluralState == PluralState.Plural ? Res.GetString("NotificationType|Warning|Plural|LowerCase", "warnings") : Res.GetString("NotificationType|Warning|NonPlural|LowerCase", "warning");
					default:
						return pluralState == PluralState.Plural ? Res.GetString("NotificationType|Warning|Plural", "Warnings") : Res.GetString("NotificationType|Warning|NonPlural", "Warning");
				}
			}
			else
			{
				switch (capitalisation)
				{
					case Capitalisation.UpperCase:
						return pluralState == PluralState.Plural ? Res.GetString("NotificationType|MessageError|Plural|UpperCase", "MESSAGE ERRORS") : Res.GetString("NotificationType|MessageError|NonPlural|UpperCase", "MESSAGE ERROR");
					case Capitalisation.LowerCase:
						return pluralState == PluralState.Plural ? Res.GetString("NotificationType|MessageError|Plural|LowerCase", "message errors") : Res.GetString("NotificationType|MessageError|NonPlural|LowerCase", "message error");
					default:
						return pluralState == PluralState.Plural ? Res.GetString("NotificationType|MessageError|Plural", "Message Errors") : Res.GetString("NotificationType|MessageError|NonPlural", "Message Error");
				}
			}
		}
	}
}
