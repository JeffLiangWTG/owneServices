using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Core.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.DialogDefault;

namespace Enterprise.ZArchitecture.Environment
{
	public static class UserNotificationExtensions
	{
		public static DialogResult Show(this IUserNotification userNotification, string message, string caption, MessageBoxButtons buttons, DialogResult defaultResult)
		{
			return (DialogResult)userNotification.Show(message, caption, (ZMessageBoxButtons)buttons, (ZDialogResult)defaultResult);
		}

		public static DialogResult Show(this IUserNotification userNotification, MultilingualString message, string caption, MessageBoxButtons buttons, DialogResult defaultResult)
		{
			return (DialogResult)userNotification.Show(message, caption, (ZMessageBoxButtons)buttons, (ZDialogResult)defaultResult);
		}

		public static DialogResult Show(this IUserNotification userNotification, string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, DialogResult defaultResult, IComponent parent = null)
		{
			return (DialogResult)userNotification.Show(message, caption, (ZMessageBoxButtons)buttons, (ZMessageBoxIcon)icon, (ZDialogResult)defaultResult, parent);
		}

		public static DialogResult Show(this IUserNotification userNotification, MultilingualString message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, DialogResult defaultResult, IComponent parent = null)
		{
			return (DialogResult)userNotification.Show(message, caption, (ZMessageBoxButtons)buttons, (ZMessageBoxIcon)icon, (ZDialogResult)defaultResult, parent);
		}

		public static DialogResult Show(this IUserNotification userNotification, string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			return (DialogResult)userNotification.Show(message, caption, (ZMessageBoxButtons)buttons, (ZMessageBoxIcon)icon);
		}

		public static DialogResult Show(this IUserNotification userNotification, MultilingualString message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			return (DialogResult)userNotification.Show(message, caption, (ZMessageBoxButtons)buttons, (ZMessageBoxIcon)icon);
		}

		public static DialogResult ShowConfirmation(this IUserNotification userNotification, string message, string caption, string confirmationString, MessageBoxIcon icon)
		{
			return (DialogResult)userNotification.ShowConfirmation(message, caption, confirmationString, (ZMessageBoxIcon)icon);
		}

		public static DialogResult ShowConfirmation(this IUserNotification userNotification, MultilingualString message, string caption, string confirmationString, MessageBoxIcon icon)
		{
			return (DialogResult)userNotification.ShowConfirmation(message, caption, confirmationString, (ZMessageBoxIcon)icon);
		}

		public static DialogResult ShowConfirmation(this IUserNotification userNotification, string message, string caption, string confirmationString, MessageBoxIcon icon, MessageBoxButtons buttons)
		{
			return (DialogResult)userNotification.ShowConfirmation(message, caption, confirmationString, (ZMessageBoxIcon)icon, (ZMessageBoxButtons)buttons);
		}

		public static DialogResult ShowConfirmation(this IUserNotification userNotification, MultilingualString message, string caption, string confirmationString, MessageBoxIcon icon, MessageBoxButtons buttons)
		{
			return (DialogResult)userNotification.ShowConfirmation(message, caption, confirmationString, (ZMessageBoxIcon)icon, (ZMessageBoxButtons)buttons);
		}

		public static DialogResult ShowConfirmation(this IUserNotification userNotification, string message, string caption, string confirmationPrompt, string confirmationString, MessageBoxIcon icon)
		{
			return (DialogResult)userNotification.ShowConfirmation(message, caption, confirmationPrompt, confirmationString, (ZMessageBoxIcon)icon);
		}

		public static DialogResult ShowConfirmation(this IUserNotification userNotification, MultilingualString message, string caption, string confirmationPrompt, string confirmationString, MessageBoxIcon icon)
		{
			return (DialogResult)userNotification.ShowConfirmation(message, caption, confirmationPrompt, confirmationString, (ZMessageBoxIcon)icon);
		}

		public static DialogResult ShowConfirmation(this IUserNotification userNotification, string message, string caption, string confirmationPrompt, string confirmationString, MessageBoxIcon icon, ConfirmationMessageLayout layout)
		{
			return (DialogResult)userNotification.ShowConfirmation(message, caption, confirmationPrompt, confirmationString, (ZMessageBoxIcon)icon, layout);
		}

		public static DialogResult ShowConfirmation(this IUserNotification userNotification, MultilingualString message, string caption, string confirmationPrompt, string confirmationString, MessageBoxIcon icon, ConfirmationMessageLayout layout)
		{
			return (DialogResult)userNotification.ShowConfirmation(message, caption, confirmationPrompt, confirmationString, (ZMessageBoxIcon)icon, layout);
		}

		public static DialogResult ShowConfirmationWithNotifications(this IUserNotification userNotification, ConfirmationDialogDescriptor descriptor)
		{
			return (DialogResult)userNotification.ShowConfirmationWithNotifications(descriptor);
		}

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public static DialogResult ShowRepeatableConfirmation(this IUserNotification userNotification, string message, string caption, string confirmationString, MessageBoxIcon icon, MessageBoxButtons buttons, out bool repeatAnswerForEntireSession)
		{
			return (DialogResult)userNotification.ShowRepeatableConfirmation(message, caption, confirmationString, (ZMessageBoxIcon)icon, (ZMessageBoxButtons)buttons, out repeatAnswerForEntireSession);
		}

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public static DialogResult ShowRepeatableConfirmation(this IUserNotification userNotification, MultilingualString message, string caption, string confirmationString, MessageBoxIcon icon, MessageBoxButtons buttons, out bool repeatAnswerForEntireSession)
		{
			return (DialogResult)userNotification.ShowRepeatableConfirmation(message, caption, confirmationString, (ZMessageBoxIcon)icon, (ZMessageBoxButtons)buttons, out repeatAnswerForEntireSession);
		}

		public static DialogResult ShowOrDefault(this IUserNotification userNotification, DialogDefaultContext context, string message)
		{
			return (DialogResult)userNotification.ShowOrDefault(context, message);
		}

		public static DialogResult ShowOrDefault(this IUserNotification userNotification, DialogDefaultContext context, Func<KUserControl> createUserControl)
		{
#if DEBUG
			if (userNotification is UnitTestUserNotification userTestUserNotification)
			{
				return userTestUserNotification.ShowOrDefault(context, createUserControl);
			}
#endif
			if (userNotification is UserNotification guiUserNotificaiton)
			{
				return guiUserNotificaiton.DefaultHandler.ShowOrDefault(context, createUserControl);
			}

			throw new NotSupportedException("Dialogs asking for user input are not supported on unattended processes.");
		}

		public static DialogResult ShowOrDefault<TDefault>(this IUserNotification userNotification, DialogDefaultContext context, ref TDefault dataSource, Func<TDefault, KUserControl> createUserControl, Func<KUserControl, TDefault> getObjectToSerialize = null)
		{
#if DEBUG
			if (userNotification is UnitTestUserNotification userTestUserNotification)
			{
				return userTestUserNotification.ShowOrDefault(context, ref dataSource, createUserControl, getObjectToSerialize);
			}
#endif
			if (userNotification is UserNotification guiUserNotificaiton)
			{
				return guiUserNotificaiton.DefaultHandler.ShowOrDefault(context, ref dataSource, (dataSourceForControl) => createUserControl(dataSourceForControl), getObjectToSerialize);
			}

			throw new NotSupportedException("Dialogs asking for user input are not supported on unattended processes.");
		}

		public static DialogResult ShowOrDefault<TDefault>(this IUserNotification userNotification, DialogDefaultContext context, ref TDefault dataSource, Func<TDefault, KUserControl> createUserControl, ISerializer<TDefault> serializer, Func<KUserControl, TDefault> getObjectToSerialize = null)
		{
#if DEBUG
			if (userNotification is UnitTestUserNotification userTestUserNotification)
			{
				return userTestUserNotification.ShowOrDefault(context, ref dataSource, createUserControl, serializer, getObjectToSerialize);
			}
#endif
			if (userNotification is UserNotification guiUserNotificaiton)
			{
				return guiUserNotificaiton.DefaultHandler.ShowOrDefault(context, ref dataSource, dataSourceForControl => createUserControl(dataSourceForControl), serializer, getObjectToSerialize);
			}

			throw new NotSupportedException("Dialogs asking for user input are not supported on unattended processes.");
		}

		public static string QueryUserResponse(this IUserNotification userNotification, string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{
			return userNotification.QueryUserResponse(message, caption, (ZMessageBoxButtons)buttons, (ZMessageBoxIcon)icon, (ZMessageBoxDefaultButton)defaultButton);
		}
	}
}
