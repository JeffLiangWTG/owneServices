using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Environment
{
	public sealed class UnattendedUserNotification : UserNotificationBase, IUserNotification, IUnattendedUserNotification
	{
		readonly int threadId;

		internal UnattendedUserNotification()
		{
			threadId = Thread.CurrentThread.ManagedThreadId;
		}

		public static UnattendedUserNotification Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new UnattendedUserNotification();
				}
				return instance;
			}
		}

		[ThreadStatic]
		public static UnattendedUserNotification instance;

		public override void ShowDeveloperException(string key, string message, Exception e)
		{
			ExceptionReporter.Instance.ReportDeveloperExceptionOrHandleSilently(key, message, e);
		}

		public bool IsInteractive
		{
			get { return false; }
		}

		public void ShowDeveloperException(Exception e)
		{
			ShowDeveloperException("", e);
		}

		#region Show Error

		public void ShowError(string message)
		{
			SendNotificationEmail(ErrorSubject, message, false, null);
		}

		public void ShowError(MultilingualString message)
		{
			SendNotificationEmail(ErrorSubject, message, false, null);
		}

		public override void ShowError(string message, string caption)
		{
			SendNotificationEmail(ErrorSubject + ": " + caption, message, false, null);
		}

		public void ShowError(MultilingualString message, string caption)
		{
			SendNotificationEmail(ErrorSubject + ": " + caption, message, false, null);
		}

		public bool IsShowingError { get { return false; } }

		public void ShowError(string message, string caption, bool sendToPostMaster)
		{
			SendNotificationEmail(ErrorSubject + ": " + caption, message, sendToPostMaster, null);
		}

		public void ShowError(MultilingualString message, string caption, bool sendToPostMaster)
		{
			SendNotificationEmail(ErrorSubject + ": " + caption, message, sendToPostMaster, null);
		}

		public void ShowError(string message, string caption, bool sendToPostMaster, AttachmentDef attachment)
		{
			SendNotificationEmail(ErrorSubject + ": " + caption, message, sendToPostMaster, attachment);
		}

		public void ShowError(MultilingualString message, string caption, bool sendToPostMaster, AttachmentDef attachment)
		{
			SendNotificationEmail(ErrorSubject + ": " + caption, message, sendToPostMaster, attachment);
		}

		public void ShowInfrastructureError(string message, string caption)
		{
			SendNotificationEmail(ErrorSubject + ": " + caption, message, true, null, EnvProxy.Instance.Registry.InfrastructureErrorsNotificationGroup);
		}

		public void ShowErrorOnceADay(string key, string message, string subject, bool sendToPostMaster, AttachmentDef attachment = null, Guid? overridePK = null, BusinessObjectFactory bizO = null)
		{
			DateTime currentLocalDateTime = EnvProxy.Instance.Time.CurrentLocalDateTime;
			var errorKeys = ShownOnceADayErrorKeysList;
			if (!errorKeys.ContainsKey(key) || (currentLocalDateTime >= errorKeys[key].AddDays(1)))
			{
				SendNotificationEmail(ErrorSubject + ": " + subject, message, sendToPostMaster, attachment, overridePK, bizO);
				errorKeys[key] = currentLocalDateTime;
			}
		}

		Dictionary<string, DateTime> ShownOnceADayErrorKeysList
		{
			get
			{
				if (threadId != Thread.CurrentThread.ManagedThreadId)
				{
					throw new InvalidOperationException("This class is not thread safe, should only obtain an instance of this class via the thread static Instance property.");
				}

				return shownOnceADayErrorKeysList;
			}
		}

		readonly Dictionary<string, DateTime> shownOnceADayErrorKeysList = new Dictionary<string, DateTime>();

		public void ShowErrorToEmail(string message, string email)
		{
			SendNotificationEmail(ErrorSubject, message, false, null, email);
		}

		public void ShowErrorToEmail(string message, string caption, string email)
		{
			SendNotificationEmail(ErrorSubject + ": " + caption, message, false, null, email);
		}

		public void ShowErrorToEmailGroup(string message, Guid emailGroupPK)
		{
			SendNotificationEmail(ErrorSubject, message, false, null, emailGroupPK);
		}

		public string[] ShownOnceADayErrorKeys
		{
			get { return new List<string>(ShownOnceADayErrorKeysList.Keys).ToArray(); }
		}

		public void ClearShownOnceADayErrorKeys()
		{
			ShownOnceADayErrorKeysList.Clear();
		}

		#endregion

		#region Show Warning

		public void ShowWarning(string message)
		{
			SendNotificationEmail(WarningSubject, message, false, null);
		}

		public void ShowWarning(MultilingualString message)
		{
			SendNotificationEmail(WarningSubject, message, false, null);
		}

		public void ShowWarning(string message, string caption)
		{
			SendNotificationEmail(WarningSubject + ": " + caption, message, false, null);
		}

		public void ShowWarning(MultilingualString message, string caption)
		{
			SendNotificationEmail(WarningSubject + ": " + caption, message, false, null);
		}

		public void ShowWarning(string message, string caption, bool sendToPostMaster)
		{
			SendNotificationEmail(WarningSubject + ": " + caption, message, sendToPostMaster, null);
		}

		public void ShowWarningToEmail(string message, string email)
		{
			SendNotificationEmail(WarningSubject, message, false, null, email);
		}

		public void ShowWarningToEmail(string message, string caption, string email)
		{
			SendNotificationEmail(WarningSubject + ": " + caption, message, false, null, email);
		}

		public void ShowWarningToEmailGroup(string message, Guid emailGroupPK)
		{
			SendNotificationEmail(WarningSubject, message, false, null, emailGroupPK);
		}

		#endregion

		#region Unsupported Show MessageBoxes

		public void Show(string message)
		{
		}

		public void Show(MultilingualString message)
		{
		}

		public void Show(INotification notification)
		{
		}

		public void ShowInformation(string message)
		{
		}

		public void ShowInformation(MultilingualString message)
		{
		}

		public void ShowInformation(string message, string caption)
		{
		}

		public void ShowInformation(MultilingualString message, string caption)
		{
		}

		public ZDialogResult Show(string message, string caption, ZMessageBoxButtons buttons, ZDialogResult defaultResult)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the batch processor.");
		}

		public ZDialogResult Show(MultilingualString message, string caption, ZMessageBoxButtons buttons, ZDialogResult defaultResult)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the batch processor.");
		}
		public ZDialogResult Show(string message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon, string button1Text, string button2Text)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the batch processor.");
		}

		public ZDialogResult Show(string message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon, ZDialogResult defaultResult, IComponent parent = null)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the batch processor.");
		}

		public ZDialogResult Show(MultilingualString message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon, ZDialogResult defaultResult, IComponent parent = null)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the batch processor.");
		}

		public ZDialogResult Show(string message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the batch processor.");
		}

		public ZDialogResult Show(MultilingualString message, string caption, ZMessageBoxButtons buttons, ZMessageBoxIcon icon)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the batch processor.");
		}

		public ZDialogResult ShowConfirmation(string message, string caption, string confirmationString, ZMessageBoxIcon icon)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the batch processor.");
		}

		public ZDialogResult ShowConfirmation(MultilingualString message, string caption, string confirmationString, ZMessageBoxIcon icon)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the batch processor.");
		}

		public ZDialogResult ShowConfirmation(string message, string caption, string confirmationString, ZMessageBoxIcon icon, ZMessageBoxButtons buttons)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the batch processor.");
		}

		public ZDialogResult ShowConfirmation(MultilingualString message, string caption, string confirmationString, ZMessageBoxIcon icon, ZMessageBoxButtons buttons)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the batch processor.");
		}

		public ZDialogResult ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString, ZMessageBoxIcon icon)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the batch processor.");
		}

		public ZDialogResult ShowConfirmation(MultilingualString message, string caption, string confirmationPrompt, string confirmationString, ZMessageBoxIcon icon)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the batch processor.");
		}

		public ZDialogResult ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString, ZMessageBoxIcon icon, ConfirmationMessageLayout layout)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the batch processor.");
		}

		public ZDialogResult ShowConfirmation(MultilingualString message, string caption, string confirmationPrompt, string confirmationString, ZMessageBoxIcon icon, ConfirmationMessageLayout layout)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on the batch processor.");
		}

		public ZDialogResult ShowRepeatableConfirmation(string message, string caption, string confirmationString, ZMessageBoxIcon icon, ZMessageBoxButtons buttons, out bool repeatAnswerForEntireSession)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on unattended processes.");
		}

		public ZDialogResult ShowRepeatableConfirmation(MultilingualString message, string caption, string confirmationString, ZMessageBoxIcon icon, ZMessageBoxButtons buttons, out bool repeatAnswerForEntireSession)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on unattended processes.");
		}

		public ZDialogResult ShowOrDefault(DialogDefaultContext context, string message)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on unattended processes.");
		}

		public ZDialogResult ShowConfirmationWithNotifications(ConfirmationDialogDescriptor descriptor)
		{
			throw new NotSupportedException("Dialogs asking for user input are not supported on unattended processes.");
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

		#endregion

		static string BatchServerMailSubjectPrefix
		{
			get { return Res.GetString("f7da7b0a-2210-444e-a43c-8d49d900314c", "{0} Batch Server", Constants.ProductName); }
		}

		static string WebServicerMailSubjectPrefix
		{
			get { return Res.GetString("281D732F-1312-4A1D-8A17-2BDC2DF5F6AD", "{0} Web Service", Constants.ProductName); }
		}

		internal static string ErrorSubject
		{
			get { return Res.GetString("6a1b4cf6-279f-4b89-88de-a26188e9708a", "{0} Error", Globals.IsWeb ? WebServicerMailSubjectPrefix : BatchServerMailSubjectPrefix); }
		}

		internal static string WarningSubject
		{
			get { return Res.GetString("714100db-82ed-4f97-a85b-e5617a2b2e82", "{0} Warning", Globals.IsWeb ? WebServicerMailSubjectPrefix : BatchServerMailSubjectPrefix); }
		}

		#region Sending Notifications

		void SendNotificationEmail(string subject, string message, bool sendToPostMaster, AttachmentDef attachment, string email)
		{
			IEnvironment env = EnvProxy.Instance;
			if (env.CurrentCompany != null || sendToPostMaster)
			{
				EmailDef mail = CreateNotificationEmail(subject, message, attachment);

				mail.AddRecipientForUserCommunication(email);
				env.OutgoingMailManager.CreateAndSave(mail);
			}
		}

		void SendNotificationEmail(string subject, string message, bool sendToPostMaster, AttachmentDef attachment, Guid? overridePK = null, BusinessObjectFactory bizO = null)
		{
			IEnvironment env = EnvProxy.Instance;
			if (env.CurrentCompany != null || sendToPostMaster)
			{
				EmailDef mail = CreateNotificationEmail(subject, message, attachment);

				Guid recipientPK = overridePK ?? (sendToPostMaster ? env.Registry.NotificationGroup(Guid.Empty, Guid.Empty, Guid.Empty) : env.Registry.NotificationGroup(env.CurrentCompany.PK, env.CurrentBranch.PK, env.CurrentDepartment.PK));
				EmailGroupUtility util = new EmailGroupUtility();
				var groupEmails = util.SendNotificationToDatabaseAdministrator(recipientPK, false);
				if (groupEmails.Count > 0)
				{
					mail.AddRecipientForUserCommunication(groupEmails);
				}
				else
				{
					mail.Body = mail.Body + "\r\n\r\n" + (EnvProxy.IsHostedWithCargowise ? Res.GetString("833a2544-6285-44aa-be77-4d139afcd830", "('Hosted Notifications Email Override' is not configured properly and is empty, so this email was sent out to a wider range of users instead.)")
						: Res.GetString("806f7bf8-8341-4769-8b3c-d773181ae9d1", "(The Company Notification Group is not configured properly and is empty, so this email was sent out to a wider range of users instead.)"));
					var adminEmails = util.GetStaffEmailCollection(true);
					mail.AddRecipientForUserCommunication(adminEmails);
				}
				env.OutgoingMailManager.CreateAndSave(mail, bizO);
			}
		}

		EmailDef CreateNotificationEmail(string subject, string message, AttachmentDef attachment)
		{
			EmailDef mail = new EmailDef();
			IEnvironment env = EnvProxy.Instance;
			mail.Subject = subject;
			mail.Body = Res.GetString("c645d5ae-028a-4329-937a-1844b01cb364", "{0}\r\n-- \r\n{1}\r\n-- \r\n[This message was automatically generated by {2}", message, GetDatabaseAndLicenceInfo(), Globals.IsWeb ? WebServicerMailSubjectPrefix : BatchServerMailSubjectPrefix)
				+ (string.IsNullOrEmpty(env.GlobalFormTopCaption) ? "" : " - " + env.GlobalFormTopCaption)
				+ "]\r\n"
			;

			if (attachment != null)
			{
				mail.Attachments.Add(attachment);
			}
			return mail;
		}

		#endregion

		string GetDatabaseAndLicenceInfo()
		{
			using (Db.DisposableActionForDbConnection())
			{
				var localMachineName = Res.GetString("5C953F8D-4F89-4D78-9807-B65D90EDC09B", "Machine Name: {0}", System.Environment.MachineName) + "\r\n";
				var userDomainAndName = Res.GetString("B8AB6A45-3866-4FCD-9E1D-3F3C16E36604", "User Domain\\Name: {0}\\{1}", System.Environment.UserDomainName, System.Environment.UserName) + "\r\n";
				var interactiveSession = Res.GetString("E704CD04-F7C9-4A81-B163-030ED046685C", "Interactive Session: {0}", System.Environment.UserInteractive ? Res.GetString("55d637ae-0af7-48f8-9e39-5d33f7ce014e", "Yes") : Res.GetString("be4df38f-220e-4c3b-af21-85b1e2816e24", "No")) + "\r\n\r\n";

				var dbServerInstanceInfo = Res.GetString("84500390-bf9f-4cb6-b0e3-97030adb81d4", "Database Server\\Instance: {0}", Db.Connection.ServerNameReportedByDatabase) + "\r\n";
				var dbNameInfo = Res.GetString("54da9406-97ae-4539-9214-ebc657d4b8ca", "Database Name: {0}", Db.DatabaseName) + "\r\n\r\n";

				var versionInfo = Res.GetString("BB5BC9D1-64A8-4D23-8EA9-12AAA83CEF62", "{0} Version: {1}", Constants.ProductName, ReleaseInfo.Instance.VersionNumber) + "\r\n";
				var licenceRegistrationInfo = EnvProxy.Instance.CurrentCompany.GetSummaryAndRegistrationText();

				var result = localMachineName + userDomainAndName + interactiveSession
						+ dbServerInstanceInfo + dbNameInfo
						+ versionInfo + licenceRegistrationInfo;
				return result;
			}
		}
	}
}
