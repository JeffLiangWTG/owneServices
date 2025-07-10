using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.DialogDefault;

namespace Enterprise.ZArchitecture.Environment
{
	/// <summary>
	/// Interface for user notification classes.
	/// </summary>
	public interface IUserNotification
	{
		bool IsInteractive { get; }
		bool IsShowingError { get; }
		void ShowDeveloperErrorOnce(string key, string message, string caption);
		void ShowDeveloperErrorAlways(string key, string message, string caption);
		void ShowDeveloperErrorAlways(string message, string caption);
		void ShowDeveloperException(string key, string message, Exception e);
		void ShowDeveloperException(string message, Exception e);
		void ShowDeveloperExceptionOnce(string key, string message, Exception e);
		void ShowDeveloperException(Exception e);
		void Show(string message);
		void Show(MultilingualString message);
		void Show(INotification notification);
		void ShowErrorOnce(string message, string caption);
		void ShowError(string message);
		void ShowError(MultilingualString message);
		void ShowError(string message, string caption);
		void ShowError(MultilingualString message, string caption);
		void ShowWarning(string message);
		void ShowWarning(MultilingualString message);
		void ShowWarning(string message, string caption);
		void ShowWarning(MultilingualString message, string caption);
		void ShowInformation(string message);
		void ShowInformation(MultilingualString message);
		void ShowInformation(string message, string caption);
		void ShowInformation(MultilingualString message, string caption);
		ZDialogResult Show(string message, string caption, ZMessageBoxButtons buttons, ZDialogResult defaultResult);
		ZDialogResult Show(string message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon, string button1Text, string button2Text);
		ZDialogResult Show(MultilingualString message, string caption, ZMessageBoxButtons buttons, ZDialogResult defaultResult);
		ZDialogResult Show(string message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon, ZDialogResult defaultResult, IComponent parent = null);
		ZDialogResult Show(MultilingualString message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon, ZDialogResult defaultResult, IComponent parent = null);
		ZDialogResult Show(string message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon);
		ZDialogResult Show(MultilingualString message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon);
		ZDialogResult ShowConfirmation(string message, string caption, string confirmationString, ZMessageBoxIcon icon);
		ZDialogResult ShowConfirmation(MultilingualString message, string caption, string confirmationString, ZMessageBoxIcon icon);
		ZDialogResult ShowConfirmation(string message, string caption, string confirmationString, ZMessageBoxIcon icon, ZMessageBoxButtons buttons);
		ZDialogResult ShowConfirmation(MultilingualString message, string caption, string confirmationString, ZMessageBoxIcon icon, ZMessageBoxButtons buttons);
		ZDialogResult ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString, ZMessageBoxIcon icon);
		ZDialogResult ShowConfirmation(MultilingualString message, string caption, string confirmationPrompt, string confirmationString, ZMessageBoxIcon icon);
		ZDialogResult ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString, ZMessageBoxIcon icon, ConfirmationMessageLayout layout);
		ZDialogResult ShowConfirmation(MultilingualString message, string caption, string confirmationPrompt, string confirmationString, ZMessageBoxIcon icon, ConfirmationMessageLayout layout);
		ZDialogResult ShowConfirmationWithNotifications(ConfirmationDialogDescriptor descriptor);
		ZDialogResult ShowRepeatableConfirmation(string message, string caption, string confirmationString, ZMessageBoxIcon icon, ZMessageBoxButtons buttons, out bool repeatAnswerForEntireSession);
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		ZDialogResult ShowRepeatableConfirmation(MultilingualString message, string caption, string confirmationString, ZMessageBoxIcon icon, ZMessageBoxButtons buttons, out bool repeatAnswerForEntireSession);
		ZDialogResult ShowOrDefault(DialogDefaultContext context, string message);
		string QueryUserResponse(string message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon, ZMessageBoxDefaultButton defaultButton);
		string QueryUserResponse(UserResponseArgument args);
		string QueryDefaultValue(string value, string message, string caption, int minimumResponseLength);
		string QueryDefaultValue(string value, string message, string caption, int minimumResponseLength, int maximumResponseLength);
		string QueryDefaultValue(string value, string message, string caption, int minimumResponseLength, int maximumResponseLength, bool forceCancelButton);
		string QueryDefaultValue(string value, string message, string caption, int minimumResponseLength, bool forceCancelButton, string button1Text);
	}

	//
	//	Layout types on Confirmation Message Box
	//
	//	AllInOneLine:
	//		[Prompt Message] [Confirmation Text] [Text Box To Input Confirmation]
	//		
	//	LineBreakAfterConfirmationPrompt:
	//		[Prompt Message]
	//		[Confirmation Text] [Text Box To Input Confirmation]
	//
	//	LineBreakAfterEachPart:
	//		[Prompt Message]
	//		[Confirmation Text]
	//		[Text Box To Input Confirmation]
	//
	public enum ConfirmationMessageLayout
	{
		AllInOneLine,
		LineBreakAfterConfirmationPrompt,
		LineBreakAfterEachPart
	}
}
