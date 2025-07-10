using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public class ImportLoggerNotificationManager : INotificationManager
	{
		public ImportLoggerNotificationManager(IXmlImportLogger logger)
		{
			Logger = logger;
		}

		readonly IXmlImportLogger Logger;

		public void AddErrorToNotifications(string errorMessage)
		{
			Logger.LogBoth(Enterprise.Integration.LogType.Error, errorMessage);
		}

		public void AddWarningToNotifications(string warningMessage)
		{
			Logger.LogBoth(Enterprise.Integration.LogType.Warning, warningMessage);
		}

		public void AddInfoNotification(string message)
		{
			Logger.LogBoth(Enterprise.Integration.LogType.Information, message);
		}

		public INotifications NotificationSubscriber
		{
			get
			{
				if (fNotificationSubscriber == null)
				{
					fNotificationSubscriber = new NotificationBuffer();
				}

				return fNotificationSubscriber;
			}
		}

		NotificationBuffer fNotificationSubscriber;

		public bool ErrorsHaveBeenReported => Logger.HasErrors();

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
	}
}
