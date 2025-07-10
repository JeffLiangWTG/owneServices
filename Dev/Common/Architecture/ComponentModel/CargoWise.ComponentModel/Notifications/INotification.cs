using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	/// <summary>
	/// A Text notification for a business entity.
	/// </summary>
	public interface INotification
	{
		/// <summary>
		/// Get the type of notification.
		/// </summary>
		INotificationType Type { get; }

		/// <summary>
		/// Get the text message of the notification, including a newline at the end.
		/// </summary>
		string Message { get; }

		/// <summary>
		/// Clones this notification with a different message.
		/// </summary>
		INotification ReplaceMessage(string message);
	}

	public static class NotificationEnumerableExtensions
	{
		#region HasNotifications / HasWarnings / HasErrors

		/// <summary>
		/// Are there any notifications in the collection?
		/// </summary>
		public static bool HasNotifications(this IEnumerable<INotification> notifications)
		{
			Argument.NotNull(notifications, nameof(notifications)); // Suggested By ReviewBot 
			foreach (var notification in notifications)
			{
				return true;
			}
			return false;
		}

		/// <summary>
		/// Are there any notifications of the given type in this collection?
		/// </summary>
		public static bool HasNotifications(this IEnumerable<INotification> notifications, INotificationType type)
		{
			Argument.NotNull(notifications, nameof(notifications)); // Suggested By ReviewBot 
			return notifications.Any(n => n?.Type == type);
		}

		#endregion

		#region ToMessageListString / ToUniqueMessageListString

		/// <summary>
		/// Get the notification messages in a single string, separated by newlines.
		/// </summary>
		public static string ToMessageListString(this IEnumerable<INotification> notifications)
		{
			Argument.NotNull(notifications, nameof(notifications)); // Suggested By ReviewBot 
			var result = "";
			foreach (var notification in notifications)
			{
				if (!string.IsNullOrEmpty(result))
				{
					result += "\n";
				}
				if (notification != null)
				{
					result += notification.Message;
				}
			}
			return result;
		}

		/// <summary>
		/// Get the notification messages without any duplicates.
		/// </summary>
		public static string[] GetUniqueMessageList(this IEnumerable<INotification> notifications)
		{
			Argument.NotNull(notifications, nameof(notifications)); // Suggested By ReviewBot 
			var result = new HashSet<string>();
			foreach (var notification in notifications)
			{
				if (notification != null)
				{
					result.Add(notification.Message);
				}
			}
			return result.ToArray();
		}

		/// <summary>
		/// Get the notification messages in a single string, separated by newlines. No lines are
		/// duplicate.
		/// </summary>
		public static string ToUniqueMessageListString(this IEnumerable<INotification> notifications)
		{
			Argument.NotNull(notifications, nameof(notifications));
			return string.Join("\n", GetUniqueMessageList(notifications));
		}

		public static string ToUniqueMessageListString(this IEnumerable<INotification> notifications, string joinCharacters)
		{
			Argument.NotNull(notifications, nameof(notifications));
			return string.Join(joinCharacters, GetUniqueMessageList(notifications));
		}

		/// <summary>
		/// Get an enumerable of INotification objects that are not duplicated.
		/// </summary>
		public static IEnumerable<INotification> GetUniqueNotifications(this IEnumerable<INotification> notifications)
		{
			Argument.NotNull(notifications, nameof(notifications));
			var soFar = new HashSet<INotification>();
			foreach (var notification in notifications)
			{
				if (notification != null && soFar.Add(notification))
				{
					yield return notification;
				}
			}
		}

		#endregion

		#region Count / GetFirst

		public static int Count(this IEnumerable<INotification> notifications)
		{
			Argument.NotNull(notifications, nameof(notifications));
			var list = notifications as IList;
			var result = 0;
			if (list != null)
			{
				result = list.Count;
			}
			else
			{
				foreach (var notification in notifications)
				{
					result++;
				}
			}
			return result;
		}

		public static INotification GetFirst(this IEnumerable<INotification> notifications)
		{
			Argument.NotNull(notifications, nameof(notifications)); // Suggested By ReviewBot 
			foreach (var notification in notifications)
			{
				return notification;
			}
			return null;
		}

		public static string GetFirstMessage(this IEnumerable<INotification> notifications)
		{
			Argument.NotNull(notifications, nameof(notifications));
			var notification = notifications.GetFirst();
			return notification == null ? null : notification.Message;
		}

		#endregion

		#region GetHighestSeverityNotification / GetHighestSeverityNotificationType / IsFatal

		public static INotification GetHighestSeverityNotification(this IEnumerable<INotification> notifications)
		{
			Argument.NotNull(notifications, nameof(notifications));
			INotification result = null;
			var highestSeverityNotificationType = notifications.GetHighestSeverityNotificationType();
			if (highestSeverityNotificationType != null)
			{
				var enumerator = notifications.GetNotifications(highestSeverityNotificationType).GetEnumerator();
				enumerator.MoveNext();
				result = enumerator.Current;
			}
			return result;
		}

		/// <summary>
		/// Get the notification type that is of the highest severity, that is, with it's Severity
		/// property the lowest value.
		/// </summary>
		public static INotificationType GetHighestSeverityNotificationType(this IEnumerable<INotification> notifications)
		{
			Argument.NotNull(notifications, nameof(notifications)); // Suggested By ReviewBot 
			INotificationType result = null;
			foreach (var notification in notifications)
			{
				if (notification != null && (result == null || (notification.Type.Severity < result.Severity)))
				{
					result = notification.Type;
				}
			}
			return result;
		}

		/// <summary>
		/// Do any of the notifications in the collection prevent a save?
		/// </summary>
		public static bool IsFatal(this IEnumerable<INotification> notifications)
		{
			Argument.NotNull(notifications, nameof(notifications)); // Suggested By ReviewBot 
			foreach (var notification in notifications)
			{
				if (notification != null && notification.Type.IsFatal)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region HasSameNotificationsIgnoringOrder

		public static bool HasSameNotificationsIgnoringOrder(this IEnumerable<INotification> lhs, IEnumerable<INotification> rhs)
		{
			Argument.NotNull(lhs, nameof(lhs));
			Argument.NotNull(rhs, nameof(rhs));
			var lhsList = new List<INotification>(lhs);
			var rhsList = new List<INotification>(rhs);

			var result = lhsList.Count == rhsList.Count;
			if (result)
			{
				result = true;
				foreach (var notification in lhsList)
				{
					if (notification != null && !ContainsNotification(rhsList, notification))
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}

		#endregion

		#region GetNotifications

		public static IEnumerable<INotification> GetNotifications(this IEnumerable<INotification> notifications, INotificationType type)
		{
			Argument.NotNull(notifications, nameof(notifications));
			foreach (var notification in notifications)
			{
				if (notification != null && notification.Type == type)
				{
					yield return notification;
				}
			}
		}

		#endregion

		#region Contains / ContainsNotificationContaining

		public static bool Contains(this IEnumerable<INotification> notifications, string message)
		{
			Argument.NotNull(notifications, nameof(notifications)); // Suggested By ReviewBot 
			foreach (var notification in notifications)
			{
				if (notification != null && notification.Message == message)
				{
					return true;
				}
			}
			return false;
		}

		public static bool ContainsNotificationContaining(this IEnumerable<INotification> notifications, string partialMessage)
		{
			Argument.NotNull(notifications, nameof(notifications)); // Suggested By ReviewBot 
			Argument.NotNull(partialMessage, nameof(partialMessage));
			foreach (var notification in notifications)
			{
				if (notification != null && notification.Message.Contains(partialMessage))
				{
					return true;
				}
			}
			return false;
		}

		static bool ContainsNotification(IEnumerable<INotification> list, INotification notification)
		{
			Argument.NotNull(notification, nameof(notification));
			Argument.NotNull(list, nameof(list)); // Suggested By ReviewBot 
			foreach (var current in list)
			{
				if (current != null && current.Type == notification.Type && current.Message == notification.Message)
				{
					return true;
				}
			}
			return false;
		}

		#endregion
	}
}
