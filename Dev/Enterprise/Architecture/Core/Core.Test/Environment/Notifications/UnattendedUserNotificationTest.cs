using System;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	public class UnattendedUserNotificationTest : TransactionedTestCase
	{
		UnattendedUserNotification Notification;

		protected override void SetUp()
		{
			base.SetUp();
			Notification = new UnattendedUserNotification();
		}

		public void TestEnsureShowDevExceptionGoesThroughSqlHandling()
		{
			var error = CargoWise.Data.Testing.SqlExceptionBuilder.CreateSqlError(4060, 1, 1, "", "server unav", "", 3);
			var collection = CargoWise.Data.Testing.SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = CargoWise.Data.Testing.SqlExceptionBuilder.CreateSqlException(collection);
			Notification.ShowDeveloperException(exception);
			AssertEquals(0, ExceptionReporter.Instance.TotalReportCount);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestIsInteractive()
		{
			AssertEquals("Is Interactive", false, Notification.IsInteractive);
		}

		public void TestShowDeveloperExceptionMessageException()
		{
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
			ClearStmErrorReports();
			Notification.ShowDeveloperException("A message", new IndexOutOfRangeException("Inner message"));
			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			Assert("exception we put in earlier was not found", ExceptionReporterTestListener.Instance[0].ToString().IndexOf("Inner message") >= 0);
			AssertEquals(0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestShowDeveloperException()
		{
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
			ClearStmErrorReports();

			Notification.ShowDeveloperException(new InvalidProgramException("oh dear"));

			Assert("subject", ExceptionReporterTestListener.Instance[0].ToString().IndexOf("oh dear") >= 0);
			AssertEquals(0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		[ExpectNoExceptions]
		public void TestShowMessage()
		{
			Notification.Show("no-op");
		}

		[ExpectNoExceptions]
		public void TestShowINotification()
		{
			Notification.Show(new Notification(CargoWise.ComponentModel.NotificationType.Information, "no-op"));
		}

		public void TestShowErrorMessageBeforeLogin()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext(null))
			{
				AssertNull(EnvProxy.Instance.CurrentCompany);
				Notification.ShowError("ARGH");
				AssertEquals(0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		public void TestShowErrorMessageBeforeLogin_ToPostmaster()
		{
			using (EnvProxy.Instance.SetTemporaryUserContext(null))
			{
				SetupNotificationEmail();
				AssertNull(EnvProxy.Instance.CurrentCompany);
				AssertEquals(0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
				Notification.ShowError("ARGH", "", true);
				AssertEquals(1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		public void TestShowErrorMessage()
		{
			SetupNotificationEmail();
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
			Notification.ShowError("This is the error");
			EmailDef mail = GetEmailSavedToBeSent();
			AssertEquals("Mail subject", UnattendedUserNotification.ErrorSubject, mail.Subject);
			Assert("Mail body: Error text", mail.Body.IndexOf("This is the error") >= 0);
		}

		public void TestShowErrorToEmail()
		{
			SetupNotificationEmail();
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
			Notification.ShowErrorToEmail("This is the error", "a@b.com");
			EmailDef mail = GetEmailSavedToBeSent();
			AssertEquals("Mail subject", UnattendedUserNotification.ErrorSubject, mail.Subject);
			Assert("Mail body: Error text", mail.Body.IndexOf("This is the error") >= 0);
			AssertEquals(mail.Recipients[0].Email, "a@b.com");
		}

		public void TestShowErrorToEmailGroup()
		{
			SetupNotificationEmail();
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
			var groupPK = RegistryConstants.GroupPKs.Notification;
			Notification.ShowErrorToEmailGroup("This is the error", groupPK);
			EmailDef mail = GetEmailSavedToBeSent();
			AssertEquals("Mail subject", UnattendedUserNotification.ErrorSubject, mail.Subject);
			Assert("Mail body: Error text", mail.Body.IndexOf("This is the error") >= 0);
			AssertEquals(mail.Recipients[0].Email, "example@example");
		}

		public void TestShowErrorSendsToAllEmailsIfNecessary()
		{
			SetupNotificationEmailForNonPostMaster();
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
			Notification.ShowError("This is the error");
			EmailDef mail = GetEmailSavedToBeSent();
			AssertEquals("Mail subject", UnattendedUserNotification.ErrorSubject, mail.Subject);
			Assert("Mail body: Error text", mail.Body.IndexOf("This is the error") >= 0);
			Assert("Mail body: warning message", mail.Body.IndexOf("(The Company Notification Group is not configured properly and is empty, so this email was sent out to a wider range of users instead.)") >= 0);
			AssertEquals(1, mail.Recipients.Count);
			AssertEquals("c@a.com", mail.Recipients[0].Email);
		}

		public void TestInfrastructureErrorNotificationsAreSentToHostingNotifications()
		{
			var originalHostedLocation = EnvProxy.HostedLocation;
			EnvProxy.SetHostedLocationForTest("SYD");
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();

			BusinessObjectFactory factory = new BusinessObjectFactory();
			IGlbGroup infrastructureGroup = factory.New<IGlbGroup>();
			infrastructureGroup.GG_Code = "SUP";
			factory.Save();
			Db.Connection.ExecuteNonQuery("UPDATE dbo.GlbStaff SET GS_EmailAddress = 'a@b.com', GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate() WHERE GS_CODE = 'E'");
			EnvProxy.Instance.Registry.RawRegistry.InfrastructureErrorsNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, infrastructureGroup.PK.ToGuid());
			Guid eGuid = (Guid)Db.Connection.ExecuteScalar("SELECT GS_PK from dbo.GlbStaff where GS_CODE = 'E'");
			Db.Connection.ExecuteNonQuery(string.Format("INSERT into dbo.GlbGroupLink(GK_PK, GK_GG, GK_GS) VALUES('{0}', '{1}', '{2}')", Guid.NewGuid(), infrastructureGroup.PK, eGuid));

			Guid groupGuid = EnvProxy.Instance.Registry.InfrastructureErrorsNotificationGroup;
			AssertEquals(infrastructureGroup.PK, groupGuid);

			AssertEquals(0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			Notification.ShowInfrastructureError("Message", "Caption");
			AssertEquals(1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);

			EmailDef mail = GetEmailSavedToBeSent();

			AssertEquals("Email subject", mail.Subject, UnattendedUserNotification.ErrorSubject + ": " + "Caption");
			Assert("Email body", mail.Body.IndexOf("Message") >= 0);

			var groupEmails = new EmailGroupUtility().GetHostedNotificationsEmailOverride();

			AssertEquals("Number of recipients", 1, groupEmails.Count);
			AssertEquals("Number of recipients", 1, mail.Recipients.Count);
			AssertEquals(mail.Recipients[0].Email, groupEmails[0]);
			AssertEquals("Hosting.Notifications@wisetechglobal.com", mail.Recipients[0].Email);

			EnvProxy.SetHostedLocationForTest(originalHostedLocation);
		}

		public void TestShowInfrastructureError()
		{
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();

			BusinessObjectFactory factory = new BusinessObjectFactory();
			IGlbGroup infrastructureGroup = factory.New<IGlbGroup>();
			infrastructureGroup.GG_Code = "SUP";
			factory.Save();
			Db.Connection.ExecuteNonQuery("UPDATE dbo.GlbStaff SET GS_EmailAddress = 'a@b.com', GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate() WHERE GS_CODE = 'E'");
			EnvProxy.Instance.Registry.RawRegistry.InfrastructureErrorsNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, infrastructureGroup.PK.ToGuid());
			Guid eGuid = (Guid)Db.Connection.ExecuteScalar("SELECT GS_PK from dbo.GlbStaff where GS_CODE = 'E'");
			Db.Connection.ExecuteNonQuery(string.Format("INSERT into dbo.GlbGroupLink(GK_PK, GK_GG, GK_GS) VALUES('{0}', '{1}', '{2}')", Guid.NewGuid(), infrastructureGroup.PK, eGuid));

			Guid groupGuid = EnvProxy.Instance.Registry.InfrastructureErrorsNotificationGroup;
			AssertEquals(infrastructureGroup.PK, groupGuid);

			AssertEquals(0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			Notification.ShowInfrastructureError("Message", "Caption");
			AssertEquals(1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);

			EmailDef mail = GetEmailSavedToBeSent();

			AssertEquals("Email subject", mail.Subject, UnattendedUserNotification.ErrorSubject + ": " + "Caption");
			Assert("Email body", mail.Body.IndexOf("Message") >= 0);

			var groupEmails = new EmailGroupUtility().GetGroupEmailCollection(groupGuid, false);

			AssertEquals("Number of recipients", 1, groupEmails.Count);
			AssertEquals("Number of recipients", 1, mail.Recipients.Count);
			AssertEquals(mail.Recipients[0].Email, groupEmails[0]);
			AssertEquals("a@b.com", mail.Recipients[0].Email);
		}

		[ExpectException(typeof(EmailHasNoRecipientsException))]
		public void TestShowErrorMessage_ExceptionHandlingShouldThrowFurther()
		{
			ResetPostMasterEmailAddress();
			SetupNotificationEmailToWithoutEmailGroup();
			ResetAdminStaffEmailAddress();
			ResetAllStaffEmailAddress();

			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
			Notification.ShowError("This is the error");
		}

		public void TestShowErrorMessageCaption()
		{
			SetupNotificationEmail();
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
			Notification.ShowError("This is the error", "A caption");
			EmailDef mail = GetEmailSavedToBeSent();
			AssertEquals("Mail subject", UnattendedUserNotification.ErrorSubject + ": A caption", mail.Subject);
			Assert("Mail body: Error text", mail.Body.IndexOf("This is the error") >= 0);
		}

		public void TestShowErrorMessageCaptionToPostMaster()
		{
			SetupNotificationEmail();
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
			Notification.ShowError("This is the error", "A caption", true);
			EmailDef mail = GetEmailSavedToBeSent();
			AssertEquals("Mail subject", UnattendedUserNotification.ErrorSubject + ": A caption", mail.Subject);
			Assert("Mail body: Error text", mail.Body.IndexOf("This is the error") >= 0);
		}

		public void TestShowErrorMessageContainsSoftwareVersion()
		{
			SetupNotificationEmail();
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
			Notification.ShowError("This is the error", "A caption", true);
			EmailDef mail = GetEmailSavedToBeSent();
			AssertEquals("Mail subject", UnattendedUserNotification.ErrorSubject + ": A caption", mail.Subject);
			Assert("Mail body: Error text", mail.Body.IndexOf($"{Enterprise.Core.Constants.ProductName} Version") >= 0);
		}

		public void TestShowErrorMessageContainsAdditionalInfo()
		{
			SetupNotificationEmail();
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
			Notification.ShowError("This is the error");
			EmailDef mail = GetEmailSavedToBeSent();
			AssertContains("Mail body should contain \"Machine Name\" section", $"Machine Name: {System.Environment.MachineName}", mail.Body, true);
			AssertContains("Mail body should contain \"User Domain\\Name\" section", $"User Domain\\Name: {System.Environment.UserDomainName}\\{System.Environment.UserName}", mail.Body, true);
			AssertContains("Mail body should contain \"Interactive Session\" section", $"Interactive Session: {(System.Environment.UserInteractive ? "Yes" : "No")}", mail.Body, true);
			AssertContains("Mail body should contain \"Database Server\\Instance\" section", $"Database Server\\Instance: {Db.Connection.ServerNameReportedByDatabase}", mail.Body, true);
			AssertContains("Mail body should contain \"Database Name\" section", $"Database Name: {Db.DatabaseName}", mail.Body, true);
		}

		public void TestShowErrorMessageAttachesAttachment()
		{
			SetupNotificationEmail();
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
			AttachmentDef attachment = new AttachmentDef("attachment", System.Text.Encoding.ASCII.GetBytes("someString"));
			Notification.ShowError("This is the error", "A caption", true, attachment);
			EmailDef mail = GetEmailSavedToBeSent();
			AssertEquals("Mail subject", UnattendedUserNotification.ErrorSubject + ": A caption", mail.Subject);
			Assert("Mail body: Error text", mail.Body.IndexOf("This is the error") >= 0);
			AssertEquals("Attachments' amount", 1, mail.Attachments.Count);
			AssertEquals("attachmentName", "attachment", mail.Attachments[0].DisplayName);
			AssertEquals("attachmentData", System.Text.Encoding.ASCII.GetBytes("someString"), mail.Attachments[0].Data);
		}

		public void TestShowWarningMessage()
		{
			SetupNotificationEmail();
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
			Notification.ShowWarning("This is the error");
			EmailDef mail = GetEmailSavedToBeSent();
			AssertEquals("Mail subject", UnattendedUserNotification.WarningSubject, mail.Subject);
			Assert("Mail body: Error text", mail.Body.IndexOf("This is the error") >= 0);
		}

		public void TestShowWarningToEmail()
		{
			SetupNotificationEmail();
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
			Notification.ShowWarningToEmail("This is the error", "a@b.com");
			EmailDef mail = GetEmailSavedToBeSent();
			AssertEquals("Mail subject", UnattendedUserNotification.WarningSubject, mail.Subject);
			Assert("Mail body: Error text", mail.Body.IndexOf("This is the error") >= 0);
			AssertEquals(mail.Recipients[0].Email, "a@b.com");
		}

		public void TestShowWarningToEmailGroup()
		{
			SetupNotificationEmail();
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
			var groupPK = RegistryConstants.GroupPKs.Notification;
			Notification.ShowWarningToEmailGroup("This is the error", groupPK);
			EmailDef mail = GetEmailSavedToBeSent();
			AssertEquals("Mail subject", UnattendedUserNotification.WarningSubject, mail.Subject);
			Assert("Mail body: Error text", mail.Body.IndexOf("This is the error") >= 0);
			AssertEquals(mail.Recipients[0].Email, "example@example");
		}

		public void TestShowWarningMessageCaption()
		{
			SetupNotificationEmail();
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
			Notification.ShowWarning("This is the error", "A caption");
			EmailDef mail = GetEmailSavedToBeSent();
			AssertEquals("Mail subject", UnattendedUserNotification.WarningSubject + ": A caption", mail.Subject);
			Assert("Mail body: Error text", mail.Body.IndexOf("This is the error") >= 0);
		}

		public void TestShowWarningMessageCaptionToPostMaster()
		{
			SetupNotificationEmail();
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
			Notification.ShowWarning("This is the error", "A caption", true);
			EmailDef mail = GetEmailSavedToBeSent();
			AssertEquals("Mail subject", UnattendedUserNotification.WarningSubject + ": A caption", mail.Subject);
			Assert("Mail body: Error text", mail.Body.IndexOf("This is the error") >= 0);
		}

		[TestDate(2006, 01, 01, 01, 01, 01)]
		public void TestShowErrorOnceADayToPostMaster()
		{
			SetupNotificationEmail();
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();

			Notification.ShowErrorOnceADay("hello", "message", "subject", true);
			AssertEquals(1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			Assert("subject line should contain the subject of the error", EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0].Subject.Contains("Batch Server Error: subject"));

			Notification.ShowErrorOnceADay("hello", "message", "subject", true);
			AssertEquals("Should not have shown another error yet, must wait a day", 1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(24.01);
			Notification.ShowErrorOnceADay("hello", "message", "another subject, same key", true);
			AssertEquals("Should have sent another notification because the old one expired", 2, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			Assert("subject line should contain the subject of the error", EnvProxy.Instance.OutgoingMailManager.EmailsCreated[1].Subject.Contains("Batch Server Error: another subject, same key"));
		}

		[ExpectNoExceptions]
		public void TestShowInformationMessage()
		{
			Notification.ShowInformation("");
		}

		[ExpectNoExceptions]
		public void TestShowInformationMessageCaption()
		{
			Notification.ShowInformation("", "");
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestShowMessageCaptionButtonsResult()
		{
			Notification.Show("", "", new ZMessageBoxButtons(), new ZDialogResult());
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestShowMessageCaptionButtonsIconResult()
		{
			Notification.Show("", "", new ZMessageBoxButtons(), new ZMessageBoxIcon(), new ZDialogResult());
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestShowMessageCaptionButtonsIcon()
		{
			Notification.Show("", "", new ZMessageBoxButtons(), new ZMessageBoxIcon());
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestShowConfirmation()
		{
			Notification.ShowConfirmation("", "", "", new ZMessageBoxIcon());
		}

		protected void SetupNotificationEmail()
		{
			Db.Connection.ExecuteNonQuery("UPDATE dbo.GlbStaff SET GS_EmailAddress = 'example@example', GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate() WHERE GS_CODE = 'PM'");
		}

		protected void SetupNotificationEmailForNonPostMaster()
		{
			var person = Guid.NewGuid();
			Db.Connection.ExecuteNonQuery($"insert into dbo.GlbPerson (PER_PK, PER_FullName, PER_SystemCreateTimeUtc, PER_SystemCreateUser, PER_SystemLastEditTimeUtc, PER_SystemLastEditUser) values ('{person}', 'aaa', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			Db.Connection.ExecuteNonQuery("UPDATE dbo.GlbStaff SET GS_EmailAddress = 'c@a.com', GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate() WHERE GS_CODE = 'E'");
			Db.Connection.ExecuteNonQuery($"UPDATE dbo.GlbStaff SET GS_IsSystemAccount = 0, GS_PER = '{person}', GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate() WHERE GS_CODE = 'E'");
		}

		#region Stuff for handling Exceptions in SendNotificationEmail

		protected void ResetPostMasterEmailAddress()
		{
			Db.Connection.ExecuteNonQuery("UPDATE dbo.GlbStaff SET GS_EmailAddress = '', GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate() WHERE GS_CODE = 'PM'");
		}

		protected void ResetAdminStaffEmailAddress()
		{
			Db.Connection.ExecuteNonQuery("UPDATE dbo.GlbStaff SET GS_EmailAddress = '', GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate() WHERE GS_IsController = 1 AND GS_IsSystemAccount = 0");
		}

		protected void ResetAllStaffEmailAddress()
		{
			Db.Connection.ExecuteNonQuery("UPDATE dbo.GlbStaff SET GS_EmailAddress = '', GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate() WHERE GS_IsSystemAccount = 0");
		}

		protected void SetupNotificationEmailToWithoutEmailGroup()
		{
			int groupCount = Convert.ToInt32(Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM dbo.GlbGroup WHERE GG_CODE = 'WITHOUTEMAIL'"));

			if (groupCount <= 0)
			{
				Db.Connection.ExecuteNonQuery("INSERT INTO dbo.GlbGroup (GG_PK, GG_Code, GG_Desc, GG_IsSystemDefined, GG_IsActive, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser) VALUES (NEWID(), 'WITHOUTEMAIL', 'Without Email Group', 0, 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			}

			Guid groupPK = (Guid)Db.Connection.ExecuteScalar("SELECT GG_PK FROM dbo.GlbGroup WHERE GG_CODE = 'WITHOUTEMAIL'");

			EmailGroupUtility emailGroupUtility = new EmailGroupUtility();
			emailGroupUtility.SetNotificationGroup(groupPK);
		}

		protected void SetupAdminStaffEmailAddress()
		{
			DbCommand command = Db.Connection.Command("SELECT COUNT(*) FROM dbo.GlbStaff WHERE GS_CODE = 'ADM'");
			int adminCount = Convert.ToInt32(command.ExecuteScalar());

			if (adminCount <= 0)
			{
				Db.Connection.ExecuteNonQuery(@"INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_IsController, GS_IsSystemAccount, GS_IsActive, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES 
																 (NEWID(), 'ADM', 'Admin', 1, 0, 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			}
			Db.Connection.ExecuteNonQuery("UPDATE dbo.GlbStaff SET GS_EmailAddress = 'admin@somecompany.com', GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate() WHERE GS_CODE= 'ADM'");
		}

		protected void SetupAllStaffEmailAddress()
		{
			DbCommand command = Db.Connection.Command("SELECT COUNT(*) FROM dbo.GlbStaff WHERE GS_CODE = 'A.K'");
			int adminCount = Convert.ToInt32(command.ExecuteScalar());

			if (adminCount <= 0)
			{
				Db.Connection.ExecuteNonQuery(@"INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_IsController, GS_IsSystemAccount, GS_IsActive, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES 
																 (NEWID(), 'A.K', 'Alexander', 0, 0, 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			}
			Db.Connection.ExecuteNonQuery("UPDATE dbo.GlbStaff SET GS_EmailAddress = 'alexander.korotun@cargowise.com', GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate() WHERE GS_CODE= 'A.K'");
		}

		#endregion

		EmailDef GetEmailSavedToBeSent()
		{
			AssertEquals("Email count", 1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			return EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0];
		}

		void ClearStmErrorReports()
		{
			using (var cmd = Db.Connection.Command(string.Format("DELETE [{0}]", StmErrorReportSchema.Constants.TableName)))
			{
				cmd.ExecuteNonQuery();
			}
		}
	}
}
