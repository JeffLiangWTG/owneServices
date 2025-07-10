using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Integration;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentVisualizer.GUI
{
	sealed class UserNotificationService : IUserNotificationService
	{
		public void ShowMessage(string message, string caption)
		{
			Globals.Message.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		public bool ShowConfirmation(string message, string caption)
		{
			return Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
		}

		public bool ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString)
		{
			return Globals.Message.ShowConfirmation(message, caption, confirmationPrompt, confirmationString, MessageBoxIcon.Warning, ConfirmationMessageLayout.LineBreakAfterEachPart) == DialogResult.OK;
		}

		public string QueryUserResponse(string message, string caption, int minimumResponseLength, int maximumResponseLength)
		{
			return Globals.Message.QueryUserResponse(new UserResponseArgument
			{
				Message = message,
				Caption = caption,
				Buttons = ZMessageBoxButtons.OKCancel,
				Icon = ZMessageBoxIcon.Warning,
				DefaultButton = ZMessageBoxDefaultButton.Button1,
				MinimumResponseLength = minimumResponseLength,
				MaximumResponseLength = maximumResponseLength
			});
		}

		public ICodeDescription QueryUserResponse(string message, string caption, ICodeDescriptionPairList optionsList)
		{
			using (var form = new UserDropDownConfirmationDialog(message, caption, optionsList))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
				{
					return form.Option;
				}

				return null;
			}
		}

		public void ShowNotifications(IEnumerable<Core.INotification> notifications)
		{
			if (notifications != null)
			{
				var bindableNotifications = new BindableNotifications(notifications);

				var view = new NotificationsView(bindableNotifications);
				ZFormModaliser.ShowDialogAndDispose(view);
			}
		}
	}
}
