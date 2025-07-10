using System.Collections.Generic;
using CargoWise.Common;

namespace CargoWise.ComponentModel
{
	public static class NotificationTypeEnumerableExtensions
	{
		public static INotificationType GetHighestSeverityNotificationType(this IEnumerable<INotificationType> types)
		{
			Argument.NotNull(types, nameof(types)); // Suggested By ReviewBot 
			INotificationType result = null;
			foreach (INotificationType type in types)
			{
				if (result == null || (type != null && type.Severity < result.Severity))
				{
					result = type;
				}
			}
			return result;
		}
	}
}
