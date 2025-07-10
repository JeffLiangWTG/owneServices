using System;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.DialogDefault;

namespace Enterprise.ZArchitecture.Environment
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Console I/O is required for a console-based UI.")]
	public sealed class CommandLineUserNotification : UserNotificationBase, IUserNotification
	{
		[ThreadStatic]
		static CommandLineUserNotification instance;

		public static CommandLineUserNotification Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new CommandLineUserNotification();
				}
				return instance;
			}
		}

		public bool IsInteractive => false;
		public bool IsShowingError => false;

		public override void ShowDeveloperException(string key, string message, Exception e)
		{
			Console.WriteLine(message); // Console I/O is required for a console-based UI.
			Console.WriteLine(e); // Console I/O is required for a console-based UI.
			ExceptionReporter.Instance.ReportDeveloperExceptionOrHandleSilently(key, message, e);
		}

		public void ShowDeveloperException(Exception e)
		{
			ShowDeveloperException(e.Message, e);
		}

		public void ShowError(string message)
		{
			Show(message);
		}

		public void ShowError(MultilingualString message)
		{
			Show(message);
		}

		public override void ShowError(string message, string caption)
		{
			Show(caption + System.Environment.NewLine + message);
		}

		public void ShowError(MultilingualString message, string caption)
		{
			ShowError(message.ToString(), caption);
		}

		public void ShowWarning(string message)
		{
			Show(message);
		}

		public void ShowWarning(MultilingualString message)
		{
			Show(message);
		}

		public void ShowWarning(string message, string caption)
		{
			Show(caption, message);
		}

		public void ShowWarning(MultilingualString message, string caption)
		{
			ShowWarning(message.ToString(), caption);
		}

		public void ShowInformation(string message)
		{
			Show(message);
		}

		public void ShowInformation(MultilingualString message)
		{
			Show(message);
		}

		public void ShowInformation(string message, string caption)
		{
			Show(caption, message);
		}

		public void ShowInformation(MultilingualString message, string caption)
		{
			ShowInformation(message.ToString(), caption);
		}

		public void Show(string message)
		{
			Console.WriteLine(message); // Console I/O is required for a console-based UI.
		}

		public void Show(MultilingualString message)
		{
			Console.WriteLine(message); // Console I/O is required for a console-based UI.
		}

		public void Show(INotification notification)
		{
			Argument.NotNull(notification, "Notification");
			Console.WriteLine(notification.Message); // Console I/O is required for a console-based UI.
		}

		public void Show(string message, string caption)
		{
			Console.WriteLine(caption); // Console I/O is required for a console-based UI.
			Console.WriteLine(message); // Console I/O is required for a console-based UI.
		}

		public void Show(MultilingualString message, string caption)
		{
			Console.WriteLine(caption); // Console I/O is required for a console-based UI.
			Console.WriteLine(message); // Console I/O is required for a console-based UI.
		}

		public ZDialogResult Show(string message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon, ZDialogResult defaultResult, IComponent parent = null)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the command line");
		}

		public ZDialogResult Show(MultilingualString message, string caption, ZMessageBoxButtons buttons, ZDialogResult defaultResult)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the command line");
		}

		public ZDialogResult Show(string message, string caption, ZMessageBoxButtons buttons, ZDialogResult defaultResult)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the command line");
		}

		public ZDialogResult Show(string message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon, string button1Text, string button2Text)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the command line");
		}

		public ZDialogResult Show(MultilingualString message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon, ZDialogResult defaultResult, IComponent parent = null)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the command line");
		}

		public ZDialogResult Show(string message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the command line");
		}

		public ZDialogResult Show(MultilingualString message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the command line");
		}

		public ZDialogResult ShowConfirmation(string message, string caption, string confirmationString, ZMessageBoxIcon icon)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the command line");
		}

		public ZDialogResult ShowConfirmation(MultilingualString message, string caption, string confirmationString, ZMessageBoxIcon icon)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the command line");
		}

		public ZDialogResult ShowConfirmation(string message, string caption, string confirmationString, ZMessageBoxIcon icon, ZMessageBoxButtons buttons)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the command line");
		}

		public ZDialogResult ShowConfirmation(MultilingualString message, string caption, string confirmationString, ZMessageBoxIcon icon, ZMessageBoxButtons buttons)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the command line");
		}

		public ZDialogResult ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString, ZMessageBoxIcon icon)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the command line");
		}

		public ZDialogResult ShowConfirmation(MultilingualString message, string caption, string confirmationPrompt, string confirmationString, ZMessageBoxIcon icon)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the command line");
		}

		public ZDialogResult ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString, ZMessageBoxIcon icon, ConfirmationMessageLayout layout)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the command line");
		}

		public ZDialogResult ShowConfirmation(MultilingualString message, string caption, string confirmationPrompt, string confirmationString, ZMessageBoxIcon icon, ConfirmationMessageLayout layout)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the command line");
		}

		public ZDialogResult ShowConfirmationWithNotifications(ConfirmationDialogDescriptor descriptor)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the command line");
		}

		public ZDialogResult ShowRepeatableConfirmation(string message, string caption, string confirmationString, ZMessageBoxIcon icon, ZMessageBoxButtons buttons, out bool repeatAnswerForEntireSession)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the command line");
		}

		public ZDialogResult ShowRepeatableConfirmation(MultilingualString message, string caption, string confirmationString, ZMessageBoxIcon icon, ZMessageBoxButtons buttons, out bool repeatAnswerForEntireSession)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the command line");
		}

		public ZDialogResult ShowOrDefault(DialogDefaultContext context, string message)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the command line");
		}

		public string QueryUserResponse(string message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon, ZMessageBoxDefaultButton defaultButton)
		{
			return "";
		}

		public string QueryUserResponse(UserResponseArgument args)
		{
			return "";
		}

		public string QueryDefaultValue(string value, string message, string caption, int minimumResponseLength)
		{
			return "";
		}

		public string QueryDefaultValue(string value, string message, string caption, int minimumResponseLength, int maximumResponseLength)
		{
			return "";
		}

		public string QueryDefaultValue(string value, string message, string caption, int minimumResponseLength, int maximumResponseLength, bool forceCancelButton)
		{
			return "";
		}

		public string QueryDefaultValue(string value, string message, string caption, int minimumResponseLength, bool forceCancelButton, string button1Text)
		{
			return "";
		}
	}
}
