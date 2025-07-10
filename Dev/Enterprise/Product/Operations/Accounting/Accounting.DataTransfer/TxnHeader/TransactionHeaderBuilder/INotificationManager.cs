using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public interface INotificationManager
	{
		void AddErrorToNotifications(string errorMessage);

		void AddWarningToNotifications(string errorMessage);

		void AddInfoNotification(string errorMessage);

		INotifications NotificationSubscriber { get; }

		bool ErrorsHaveBeenReported { get; }

		void ReportNoBizObjsFoundError(string bizObjDescription, string valueForDisplay, int numberOfBizObjs, string errorContext);

		void ReportNoBizObjsFoundWarning(BusinessObject bizObj, string bizObjDescription, string valueForDisplay, string errorContext);
	}
}
