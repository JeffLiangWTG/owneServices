using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.UPE.ServiceTask
{
	public static class UPESaveConcurrencyExceptionResolver
	{
		public static void HandleException(Action saveAction, INotifications notify)
		{
			try
			{
				saveAction();
			}
			catch (ZSaveConcurrencyException ex)
			{
				if (notify != null)
				{
					var notificationBuilder = new ZStringBuilder();
					notificationBuilder.Append("CONCURRENCY: List of Concurrency objects:");
					foreach (var obj in ex.BusinessObjects)
					{
						notificationBuilder.Append(ZString.Format("BizObjectName: {0} BizObjectTableName: {1} PK: {2}", obj.HumanReadableName, obj.TableName, obj.PK.ToString()));
					}
					notify.Notify(new WarningNotification(notificationBuilder.ToStringWithNewLineBetweenAppends()));
				}

				ZExceptionReporting.HandleZSaveConcurrencyException(ex, NotificationHandler.Instance, false);
				saveAction();
			}
		}
	}
}
