using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public class NotificationManager : INotificationManager
	{
		public NotificationManager(INotifications notificationSubscriber)
		{
			fNotificationSubscriber = notificationSubscriber;
		}

		public void ReportNoBizObjsFoundError(string bizObjDescription, string valueForDisplay, int numberOfBizObjs, string errorContext)
		{
			if (numberOfBizObjs == 0)
			{
				AddErrorToNotifications(GetNoMatchesMessage(errorContext, bizObjDescription, valueForDisplay));
			}
		}

		public void ReportNoBizObjsFoundWarning(BusinessObject bizObj, string bizObjDescription, string valueForDisplay, string errorContext)
		{
			if (bizObj == null)
			{
				if (!string.IsNullOrEmpty(valueForDisplay))
				{
					AddWarningToNotifications(GetNoMatchesMessage(errorContext, bizObjDescription, valueForDisplay));
				}
			}
		}

		static string GetNoMatchesMessage(string errorContext, string bizObjDescription, string valueForDisplay)
		{
			return errorContext + Res.GetString("e242cc25-3efa-43e8-8f5e-8964dffbc925", "No matches were found for the following {0}: {1}", bizObjDescription, valueForDisplay);
		}

		public void AddNewlineNotification()
		{
			NotificationSubscriber.Notify(new NewlineNotification());
		}

		public void AddInfoNotification(string message)
		{
			NotificationSubscriber.Notify(new InfoNotification(message));
		}

		public void AddErrorToNotifications(string errorMessage)
		{
			fErrorsHaveBeenReported = true;
			NotificationSubscriber.Notify(new ErrorNotification(ErrorType.Error, errorMessage));
		}

		public void AddDataErrorPreventSaveToNotifications(string errorMessage)
		{
			fErrorsHaveBeenReported = true;
			NotificationSubscriber.Notify(new ErrorNotification(ErrorType.DataErrorPreventSave, errorMessage));
		}

		public void AddWarningToNotifications(string warningMessage)
		{
			NotificationSubscriber.Notify(new WarningNotification(warningMessage));
		}

		protected virtual bool AllowImportOfBusinessObjectIfErrorsAreReported
		{
			get { return false; }
		}

		public bool ErrorsHaveBeenReported
		{
			get { return fErrorsHaveBeenReported; }
			set { fErrorsHaveBeenReported = value; }
		}

		public INotifications NotificationSubscriber
		{
			get { return fNotificationSubscriber; }
		}

		bool fErrorsHaveBeenReported;
		readonly INotifications fNotificationSubscriber;
	}
}
