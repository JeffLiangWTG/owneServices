using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public static class ValidationExtensions
	{
		public static void AddNotificationBasedOnChildValidationStatus(
			this ZPropertyInfo masterTargetPtyInfo,
			ZString masterNotificationMessage,
			Action childValidateAction,
			INotificationProvider childNotificationProvider)
		{
			Argument.NotNull(masterTargetPtyInfo, nameof(masterTargetPtyInfo));
			Argument.NotNullOrEmpty(masterNotificationMessage, nameof(masterNotificationMessage));
			Argument.NotNull(childValidateAction, nameof(childValidateAction));
			Argument.NotNull(childNotificationProvider, nameof(childNotificationProvider));

			childValidateAction();

			var mostSevereNotificationType = childNotificationProvider.GetHighestSeverityNotificationType();
			if (mostSevereNotificationType != null)
			{
				masterTargetPtyInfo.AddNotification(mostSevereNotificationType, masterNotificationMessage);
			}
		}
	}
}
