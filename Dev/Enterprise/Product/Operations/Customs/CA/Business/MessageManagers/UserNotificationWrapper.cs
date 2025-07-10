using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.Customs.Business.MessageManagers;

namespace Enterprise.Customs.CA.Business
{
	public class UserNotificationWrapper : IUserNotification
	{
		public UserNotificationWrapper(INotifications notifications)
		{
			this.notifications = Argument.NotNull(notifications, "notifications");
		}
		readonly INotifications notifications;

		#region IUserNotification Members

		bool IUserNotification.ShowConfirmation(string message, string caption, bool warning)
		{
			return true;
		}

		bool IUserNotification.ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString)
		{
			return true;
		}

		void IUserNotification.ShowError(string message, string caption)
		{
			notifications.AddError(message);
		}

		void IUserNotification.ShowInformation(string message, string caption)
		{
		}

		string IUserNotification.ShowQuestion(string message, string caption, int answerLength, ZArchitecture.Core.CodeDescriptionPairList answerList, string defaultAnswer)
		{
			return string.Empty;
		}

		void IUserNotification.ShowWarning(string message, string caption)
		{
			notifications.AddWarning(message);
		}

		#endregion
	}
}
