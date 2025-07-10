using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Core.DialogDefault;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.GUI;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.Core.Environment
{
	/// <summary>
	/// User notification for interactive operation.
	/// Singleton.
	/// </summary>
	public sealed class UserNotification : UserNotificationBase, IUserNotification
	{
		internal UserNotification()
		{
		}

		public static UserNotification Instance
		{
			get { return instance ?? (instance = new UserNotification()); }
		}
		[ThreadStatic]
		static UserNotification instance;

		public bool IsInteractive
		{
			get { return true; }
		}

		#region Developer Exception

		public override void ShowDeveloperException(string key, string message, Exception e)
		{
			ExceptionReporter.Instance.ReportDeveloperExceptionOrHandleSilently(key, message, e);
		}

		public void ShowDeveloperException(Exception e)
		{
			ShowDeveloperException(e.Message, e);
		}

		#endregion

		#region Show Error

		public void ShowError(string message)
		{
			try
			{
				IsShowingError = true;
				ShowError(message, Res.GetString("0c557ebf-68a4-48c4-910d-f8875ed99ea3", "Error"));
			}
			finally
			{
				IsShowingError = false;
			}
		}

		public void ShowError(MultilingualString message)
		{
			try
			{
				IsShowingError = true;
				ShowError(message, Res.GetString("0c557ebf-68a4-48c4-910d-f8875ed99ea3", "Error"));
			}
			finally
			{
				IsShowingError = false;
			}
		}

		public override void ShowError(string message, string caption)
		{
			try
			{
				IsShowingError = true;
				Show(message, caption, ZMessageBoxButtons.OK, ZMessageBoxIcon.Error, ZDialogResult.OK);
			}
			finally
			{
				IsShowingError = false;
			}
		}

		public void ShowError(MultilingualString message, string caption)
		{
			try
			{
				IsShowingError = true;
				Show(message, caption, ZMessageBoxButtons.OK, ZMessageBoxIcon.Error, ZDialogResult.OK);
			}
			finally
			{
				IsShowingError = false;
			}
		}

		public bool IsShowingError { get; set; }

		#endregion

		#region Show Warning

		public void ShowWarning(string message)
		{
			ShowWarning(message, Res.GetString("77b0776a-7a2d-4975-9b84-5fabd50b0e64", "Warning"));
		}

		public void ShowWarning(MultilingualString message)
		{
			ShowWarning(message, Res.GetString("77b0776a-7a2d-4975-9b84-5fabd50b0e64", "Warning"));
		}

		public void ShowWarning(string message, string caption)
		{
			Show(message, caption, ZMessageBoxButtons.OK, ZMessageBoxIcon.Warning, ZDialogResult.OK);
		}

		public void ShowWarning(MultilingualString message, string caption)
		{
			Show(message, caption, ZMessageBoxButtons.OK, ZMessageBoxIcon.Warning, ZDialogResult.OK);
		}

		#endregion

		#region Show Information

		public void ShowInformation(string message)
		{
			ShowInformation(message, Res.GetString("d22faf72-969e-4da6-9b69-06883df6ae6c", "Information"));
		}

		public void ShowInformation(MultilingualString message)
		{
			ShowInformation(message, Res.GetString("d22faf72-969e-4da6-9b69-06883df6ae6c", "Information"));
		}

		public void ShowInformation(string message, string caption)
		{
			Show(message, caption, ZMessageBoxButtons.OK, ZMessageBoxIcon.Information, ZDialogResult.OK);
		}

		public void ShowInformation(MultilingualString message, string caption)
		{
			Show(message, caption, ZMessageBoxButtons.OK, ZMessageBoxIcon.Information, ZDialogResult.OK);
		}
		#endregion

		#region Show

		public void Show(string message)
		{
			Show(message, "", ZMessageBoxButtons.OK, ZMessageBoxIcon.Information, ZDialogResult.OK);
		}

		public void Show(MultilingualString message)
		{
			Show(message, "", ZMessageBoxButtons.OK, ZMessageBoxIcon.Information, ZDialogResult.OK);
		}

		public void Show(INotification notification)
		{
			Argument.NotNull(notification, "Notification");
			Show(notification.Message, "", ZMessageBoxButtons.OK, EnumStringConverter.ConvertStringToEnumEntry(notification, ZMessageBoxIcon.Information), ZDialogResult.OK);
		}

		public ZDialogResult Show(string message, string caption, ZMessageBoxButtons buttons, ZDialogResult defaultResult)
		{
			return Show(message, caption, buttons, ZMessageBoxIcon.Information, defaultResult);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Only used as a backup in case of failure.")]
		public ZDialogResult Show(string message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon, string button1Text, string button2Text)
		{
			var result = DialogResult.None;
			try
			{
				using (var msgBox = NewMessageBox(message, caption, (MessageBoxButtons)buttons, (MessageBoxIcon)icon, button1Text, button2Text))
				{
					result = ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				//Try one more time with System.Windows.Forms.MessageBox, in case it's the only way to display the message.
				try
				{
					result = System.Windows.Forms.MessageBox.Show(message, caption, (MessageBoxButtons)buttons, (MessageBoxIcon)icon, MessageBoxDefaultButton.Button1);
				}
				catch (Exception ex2) when (!ex2.IsCriticalException())
				{
					//OK, we did our best.
				}
			}

			return (ZDialogResult)result;
		}

		public ZDialogResult Show(MultilingualString message, string caption, ZMessageBoxButtons buttons, ZDialogResult defaultResult)
		{
			return Show(message, caption, buttons, ZMessageBoxIcon.Information, defaultResult);
		}

		public ZDialogResult Show(string message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon)
		{
			return Show(message, caption, buttons, icon, ZDialogResult.None);
		}

		public ZDialogResult Show(MultilingualString message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon)
		{
			return Show(message, caption, buttons, icon, ZDialogResult.None);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Only used as a backup in case of failure.")]
		public ZDialogResult Show(string message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon, ZDialogResult defaultResult, IComponent parent = null)
		{
			var result = DialogResult.None;
			try
			{
				using (var msgBox = NewMessageBox(message, caption, (MessageBoxButtons)buttons, (MessageBoxIcon)icon, (DialogResult)defaultResult))
				{
					result = ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox, parent as Form);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				//Try one more time with System.Windows.Forms.MessageBox, in case it's the only way to display the message.
				try
				{
					result = System.Windows.Forms.MessageBox.Show(message, caption, (MessageBoxButtons)buttons, (MessageBoxIcon)icon, MessageBoxDefaultButton.Button1);
				}
				catch (Exception ex2) when (!ex2.IsCriticalException())
				{
					//OK, we did our best.
				}
			}

			return (ZDialogResult)result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1076:MessageBoxShow", Justification = "Only used as a backup in case of failure.")]
		public ZDialogResult Show(MultilingualString message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon, ZDialogResult defaultResult, IComponent parent = null)
		{
			var result = DialogResult.None;

			try
			{
				using (var msgBox = NewMessageBox(message, caption, (MessageBoxButtons)buttons, (MessageBoxIcon)icon, (DialogResult)defaultResult))
				{
					result = ZFormModaliser.ShowMessageBoxWithoutDispose(msgBox, parent as Form);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				//Try one more time with System.Windows.Forms.MessageBox, in case it's the only way to display the message.
				try
				{
					result = System.Windows.Forms.MessageBox.Show(message, caption, (MessageBoxButtons)buttons, (MessageBoxIcon)icon, MessageBoxDefaultButton.Button1);
				}
				catch (Exception ex2) when (!ex2.IsCriticalException())
				{
					//OK, we did our best.
				}
			}

			return (ZDialogResult)result;
		}

		#endregion

		#region Show Confirmation

		public ZDialogResult ShowConfirmation(string message, string caption, string confirmationString, ZMessageBoxIcon icon)
		{
			return ShowConfirmation(message, caption, confirmationString, icon, DefaultShowConfirmationButtons);
		}

		public ZDialogResult ShowConfirmation(MultilingualString message, string caption, string confirmationString, ZMessageBoxIcon icon)
		{
			return ShowConfirmation(message, caption, confirmationString, icon, DefaultShowConfirmationButtons);
		}

		public ZDialogResult ShowConfirmation(string message, string caption, string confirmationString, ZMessageBoxIcon icon, ZMessageBoxButtons buttons)
		{
			return ShowConfirmation(message, caption, DefaultConfirmationPrompt, confirmationString, icon, buttons, ConfirmationMessageLayout.AllInOneLine);
		}

		public ZDialogResult ShowConfirmation(MultilingualString message, string caption, string confirmationString, ZMessageBoxIcon icon, ZMessageBoxButtons buttons)
		{
			return ShowConfirmation(message, caption, DefaultConfirmationPrompt, confirmationString, icon, buttons, ConfirmationMessageLayout.AllInOneLine);
		}

		public ZDialogResult ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString, ZMessageBoxIcon icon)
		{
			return ShowConfirmation(message, caption, confirmationPrompt, confirmationString, icon, DefaultShowConfirmationButtons, ConfirmationMessageLayout.AllInOneLine);
		}

		public ZDialogResult ShowConfirmation(MultilingualString message, string caption, string confirmationPrompt, string confirmationString, ZMessageBoxIcon icon)
		{
			return ShowConfirmation(message, caption, confirmationPrompt, confirmationString, icon, DefaultShowConfirmationButtons, ConfirmationMessageLayout.AllInOneLine);
		}

		public ZDialogResult ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString, ZMessageBoxIcon icon, ConfirmationMessageLayout layout)
		{
			return ShowConfirmation(message, caption, confirmationPrompt, confirmationString, icon, DefaultShowConfirmationButtons, layout);
		}

		public ZDialogResult ShowConfirmation(MultilingualString message, string caption, string confirmationPrompt, string confirmationString, ZMessageBoxIcon icon, ConfirmationMessageLayout layout)
		{
			return ShowConfirmation(message, caption, confirmationPrompt, confirmationString, icon, DefaultShowConfirmationButtons, layout);
		}

		ZDialogResult ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString, ZMessageBoxIcon icon, ZMessageBoxButtons buttons, ConfirmationMessageLayout layout)
		{
			using (var dialog = new UserConfirmationDialog(message, caption, (MessageBoxButtons)buttons, (MessageBoxIcon)icon, MessageBoxDefaultButton.Button1, layout))
			{
				dialog.ExpectedString = confirmationString;
				dialog.ConfirmationPromptLabelText = confirmationPrompt;
				var result = ZFormModaliser.ShowDialogWithoutDispose(dialog);
				return (ZDialogResult)result;
			}
		}

		public ZDialogResult ShowRepeatableConfirmation(string message, string caption, string confirmationString, ZMessageBoxIcon icon, ZMessageBoxButtons buttons, out bool repeatAnswerForEntireSession)
		{
			using (var dialog = new UserConfirmationDialog(message, caption, (MessageBoxButtons)buttons, (MessageBoxIcon)icon, MessageBoxDefaultButton.Button1, showRepeatableConfirmationOption: true))
			{
				dialog.ExpectedString = confirmationString;
				dialog.ConfirmationPromptLabelText = DefaultConfirmationPrompt;
				var result = ZFormModaliser.ShowDialogWithoutDispose(dialog);
				repeatAnswerForEntireSession = dialog.UseThisAnswerForAllConfirmationsOfThisTypeInThisSession;
				return (ZDialogResult)result;
			}
		}

		public ZDialogResult ShowRepeatableConfirmation(MultilingualString message, string caption, string confirmationString, ZMessageBoxIcon icon, ZMessageBoxButtons buttons, out bool repeatAnswerForEntireSession)
		{
			using (var dialog = new UserConfirmationDialog(message, caption, (MessageBoxButtons)buttons, (MessageBoxIcon)icon, MessageBoxDefaultButton.Button1, showRepeatableConfirmationOption: true))
			{
				dialog.ExpectedString = confirmationString;
				dialog.ConfirmationPromptLabelText = DefaultConfirmationPrompt;
				var result = ZFormModaliser.ShowDialogWithoutDispose(dialog);
				repeatAnswerForEntireSession = dialog.UseThisAnswerForAllConfirmationsOfThisTypeInThisSession;
				return (ZDialogResult)result;
			}
		}

		#endregion

		#region Defaultable

		[ThreadStatic]
		static DialogDefaultHandler _defaultHandler;
		internal DialogDefaultHandler DefaultHandler
		{
			get { return _defaultHandler ?? (_defaultHandler = new DialogDefaultHandler(new BusinessObjectFactory())); }
		}

		public ZDialogResult ShowOrDefault(DialogDefaultContext context, string message)
		{
			return (ZDialogResult)DefaultHandler.ShowOrDefault(context, message);
		}

		string DefaultConfirmationPrompt
		{
			get { return Res.GetString("18653640-2f23-481a-aa5d-f33e63014a66", "Please type the following to continue:") + " "; }
		}

		const ZMessageBoxButtons DefaultShowConfirmationButtons = ZMessageBoxButtons.OKCancel;

		#endregion

		#region Show Confirmation with Notifications

		public ZDialogResult ShowConfirmationWithNotifications(ConfirmationDialogDescriptor descriptor)
		{
			using (var form = new ConfirmationDialogForm(descriptor))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}

			return descriptor.Result;
		}

		#endregion

		#region Query User Response

		public string QueryUserResponse(UserResponseArgument arguments)
		{
			using (var dialog = new UserResponseDialog(arguments.Message, arguments.Caption, (MessageBoxButtons)arguments.Buttons, (MessageBoxIcon)arguments.Icon, (MessageBoxDefaultButton)arguments.DefaultButton, arguments.AnswerList, arguments.DefaultAnswer))
			{
				dialog.MinimumResponseLength = arguments.MinimumResponseLength;
				dialog.MaximumResponseLength = arguments.MaximumResponseLength;
				dialog.UserResponseTextBoxCharactersCasing = (CharacterCasing)arguments.UserResponseTextBoxCharactersCasing;
				dialog.UserResponseDropEditCharactersCasing = (CharacterCasing)arguments.UserResponseDropEditCharactersCasing;
				dialog.UserResponseDropEditOnlyShowCode = arguments.UserResponseDropEditOnlyShowCode;
				dialog.PasswordChar = arguments.PasswordChar;

				var answer = ZFormModaliser.ShowDialogWithoutDispose(dialog);
				return (answer == DialogResult.OK || answer == DialogResult.Yes) ? (string)dialog.UserResponse : string.Empty;
			}
		}

		public string QueryUserResponse(string message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon, ZMessageBoxDefaultButton defaultButton)
		{
			return QueryUserResponse(new UserResponseArgument { Message = message, Caption = caption, Buttons = buttons, Icon = icon, DefaultButton = defaultButton });
		}

		/// <summary>
		/// Handy for querying a default value prior to using it - eg. confirming a registry email address
		/// </summary>
		/// <param name="value">The data being confirmed.</param>
		public string QueryDefaultValue(string value, string message, string caption, int minimumResponseLength)
		{
			return QueryDefaultValue(value, message, caption, minimumResponseLength, 0, false);
		}

		public string QueryDefaultValue(string value, string message, string caption, int minimumResponseLength, int maximumResponseLength)
		{
			return QueryDefaultValue(value, message, caption, minimumResponseLength, maximumResponseLength, false);
		}

		public string QueryDefaultValue(string value, string message, string caption, int minimumResponseLength, int maximumResponseLength, bool forceCancelButton)
		{
			var buttonsToShow = string.IsNullOrEmpty(value) || forceCancelButton ? MessageBoxButtons.OKCancel : MessageBoxButtons.OK;
			using (var dialog = new UserResponseDialog(message, caption, buttonsToShow, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
			{
				dialog.UserResponse = value;
				dialog.MinimumResponseLength = minimumResponseLength;
				dialog.MaximumResponseLength = maximumResponseLength;
				return (ZFormModaliser.ShowDialogWithoutDispose(dialog) == DialogResult.OK) ? (string)dialog.UserResponse : string.Empty;
			}
		}

		public string QueryDefaultValue(string value, string message, string caption, int minimumResponseLength, bool forceCancelButton, string button1Text)
		{
			var buttonsToShow = string.IsNullOrEmpty(value) || forceCancelButton ? MessageBoxButtons.OKCancel : MessageBoxButtons.OK;
			using (var dialog = new UserResponseDialog(message, caption, buttonsToShow, MessageBoxIcon.Question, button1Text))
			{
				dialog.UserResponse = value;
				dialog.MinimumResponseLength = minimumResponseLength;
				return (ZFormModaliser.ShowDialogWithoutDispose(dialog) == DialogResult.OK) ? (string)dialog.UserResponse : string.Empty;
			}
		}

		#endregion

		#region Interactive Mode

		public static IDisposable InteractiveMode()
		{
			Globals.IsUserInteractive = true;
			Globals.InteractiveNotification = Instance;
			return new DisposableAction(delegate
			{
				Globals.IsUserInteractive = false;
				Globals.InteractiveNotification = null;
			});
		}

		#endregion

		#region Implementation

		ZMessageBox NewMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, DialogResult defaultResult)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var defaultButton = MessageBoxDefaultButton.Button1;

				if (defaultResult != DialogResult.None)
				{
					defaultButton = ButtonsHelper.DefaultButtonFromDialogResult(buttons, defaultResult);
				}

				return new ZMessageBox(message, caption, buttons, icon, defaultButton);
			}
		}

		ZMessageBox NewMessageBox(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, string button1Text, string button2Text)
		{
			using (Db.DisposableActionForDbConnection())
			{
				return new ZMessageBox(message, caption, buttons, icon, button1Text, button2Text);
			}
		}

		ZMessageBox NewMessageBox(MultilingualString message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, DialogResult defaultResult)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var defaultButton = MessageBoxDefaultButton.Button1;

				if (defaultResult != DialogResult.None)
				{
					defaultButton = ButtonsHelper.DefaultButtonFromDialogResult(buttons, defaultResult);
				}

				return new ZMessageBox(message, caption, buttons, icon, defaultButton);
			}
		}

		MessageBoxButtonsHelper ButtonsHelper => buttonsHelper ?? (buttonsHelper = new MessageBoxButtonsHelper());
		MessageBoxButtonsHelper buttonsHelper;

		#endregion
	}

	public class MessageBoxButtonsHelper
	{
		public MessageBoxDefaultButton DefaultButtonFromDialogResult(MessageBoxButtons buttons, DialogResult defaultResult)
		{
			var invalid = false;
			var result = MessageBoxDefaultButton.Button1;

			switch (buttons)
			{
				case MessageBoxButtons.AbortRetryIgnore:
					if (defaultResult == DialogResult.Abort)
					{
						result = MessageBoxDefaultButton.Button1;
					}
					else if (defaultResult == DialogResult.Retry)
					{
						result = MessageBoxDefaultButton.Button1;
					}
					else if (defaultResult == DialogResult.Cancel)
					{
						result = MessageBoxDefaultButton.Button1;
					}
					else
					{
						invalid = true;
					}

					break;

				case MessageBoxButtons.OK:
					if (defaultResult == DialogResult.OK)
					{
						result = MessageBoxDefaultButton.Button1;
					}
					else
					{
						invalid = true;
					}

					break;

				case MessageBoxButtons.OKCancel:
					if (defaultResult == DialogResult.OK)
					{
						result = MessageBoxDefaultButton.Button1;
					}
					else if (defaultResult == DialogResult.Cancel)
					{
						result = MessageBoxDefaultButton.Button2;
					}
					else
					{
						invalid = true;
					}

					break;

				case MessageBoxButtons.RetryCancel:
					if (defaultResult == DialogResult.Retry)
					{
						result = MessageBoxDefaultButton.Button1;
					}
					else if (defaultResult == DialogResult.Cancel)
					{
						result = MessageBoxDefaultButton.Button2;
					}
					else
					{
						invalid = true;
					}

					break;

				case MessageBoxButtons.YesNo:
					if (defaultResult == DialogResult.Yes)
					{
						result = MessageBoxDefaultButton.Button1;
					}
					else if (defaultResult == DialogResult.No)
					{
						result = MessageBoxDefaultButton.Button2;
					}
					else
					{
						invalid = true;
					}

					break;

				case MessageBoxButtons.YesNoCancel:
					if (defaultResult == DialogResult.Yes)
					{
						result = MessageBoxDefaultButton.Button1;
					}
					else if (defaultResult == DialogResult.No)
					{
						result = MessageBoxDefaultButton.Button2;
					}
					else if (defaultResult == DialogResult.Cancel)
					{
						result = MessageBoxDefaultButton.Button3;
					}
					else
					{
						invalid = true;
					}

					break;

				default:
					invalid = true;
					break;
			}

			if (invalid)
			{
				throw new ApplicationException("DefaultResult " + defaultResult + " not found in Buttons " + buttons);
			}

			return result;
		}
	}
}
