using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.Customs.CH.Business;

public sealed class NotificationHelper
{
	public static void TurnMessageErrorsIntoErrors(ZPropertyInfo propertyInfo, Func<INotification, bool> predicate = null)
	{
		ConvertMessageErrorToAnotherNotificationType(propertyInfo, NotificationType.Error, predicate);
	}

	public static void TurnMessageErrorsIntoWarning(ZPropertyInfo propertyInfo, Func<INotification, bool> predicate = null)
	{
		ConvertMessageErrorToAnotherNotificationType(propertyInfo, NotificationType.Warning, predicate);
	}

	static void ConvertMessageErrorToAnotherNotificationType(ZPropertyInfo propertyInfo, INotificationType notificationType, Func<INotification, bool> predicate = null)
	{
		foreach (var notification in propertyInfo.GetMessageErrors().ToArray())
		{
			if (predicate == null || predicate(notification))
			{
				propertyInfo.ReplaceNotification(notification,notificationType, notification.Message);
			}
		}
	}
}
