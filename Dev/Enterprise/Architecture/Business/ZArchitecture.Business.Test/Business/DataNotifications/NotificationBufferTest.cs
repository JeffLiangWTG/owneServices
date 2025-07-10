using System;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class NotificationBufferTest : TestCaseWithFactory
	{
		#region Test SendEmail

		public void TestSendEmailWithoutGroupDefaultingToAllUsers()
		{
			GuidRegistryItem regItem = new GuidRegistryItem("Test Registry Item", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.System, Guid.Empty);
			NotificationBuffer emailEvents = new NotificationBuffer();
			NotificationBuffer testEvents = new NotificationBuffer();

			BusinessObjectFactory factory = new BusinessObjectFactory();
			IGlbStaff dummyStaff = factory.New<IGlbStaff>();
			dummyStaff.GS_LoginName = "~test1";
			dummyStaff.GS_Code = "~t1";
			dummyStaff.GS_EmailAddress = "a@b.c";
			IGlbGroup dummyGroup = factory.New<IGlbGroup>();
			dummyGroup.Staff.Add((BusinessObject)dummyStaff);
			factory.Save();

			testEvents.SendEmail(dummyGroup.PK.ToGuid(), regItem, "Test Email", "My Header Rules\r\n", "My Footer Rules\r\n", emailEvents);
			AssertEquals(
				"Should be no errors sending because the data import error notification group doesn't exist so it defaults to 'All Users'",
				false, emailEvents.HasErrors);
		}

		public void TestSendEmailWithoutGroup()
		{
			NotificationBuffer innerNotify = new NotificationBuffer();
			NotificationBuffer buffer = new NotificationBuffer(innerNotify);
			AssertEquals(false, innerNotify.HasErrors);
			buffer.SendEmail("test", "nongroup");
			AssertEquals(
				"Should be errors sending because the data import error notification group doesn't exist",
				true, innerNotify.ContainsNotificationType(ErrorType.EmailNotifyGroupNotExist));
		}

		public void TestSendEmailWithoutUserToSendEmailTo()
		{
			IGlbGroup testEmailGroup = SetupEmailNotificationGroup();

			NotificationBuffer innerNotify = new NotificationBuffer();
			NotificationBuffer buffer = new NotificationBuffer(innerNotify);
			buffer.SendEmail("test", testEmailGroup.GG_Code);
			AssertEquals("Should be errors sending because no staff exists", true, innerNotify.ContainsNotificationType(ErrorType.ErrorSendingEmail));
		}

		public void TestSendEmailWithAttachments()
		{
			IGlbGroup testEmailGroup = SetupEmailNotificationGroup();
			IGlbStaff staff = Factory.New<IGlbStaff>();
			testEmailGroup.Staff.Add((BusinessObject)staff);
			staff.GS_FullName = "Bob Smith";
			staff.GS_EmailAddress = "bob@smith";
			Factory.Save();

			NotificationBuffer innerNotify = new NotificationBuffer();
			NotificationBuffer buffer = new NotificationBuffer(innerNotify);
			buffer.Notify(new InfoNotification("Hi People"));

			AttachmentDefCollection attachments = new AttachmentDefCollection();
			string subject = "Test Subject";
			buffer.SendEmail(testEmailGroup.PK.ToGuid(), null, subject, "", "", innerNotify, EnvProxy.Instance.OutgoingMailManager, Factory, attachments);
			EmailDef email = EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("0 attachments sent", 0, email.Attachments.Count);
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();

			AttachmentDef attachment1 = new AttachmentDef("TestDisplayName1", AttachmentDef.StringToByteArray("Hello World Today"));
			AttachmentDef attachment2 = new AttachmentDef("TestDisplayName2", AttachmentDef.StringToByteArray("Hello World Tomorrow"));
			attachments.Add(attachment1);
			attachments.Add(attachment2);

			buffer.SendEmail(testEmailGroup.PK.ToGuid(), null, subject, "", "", innerNotify, EnvProxy.Instance.OutgoingMailManager, Factory, attachments);
			email = EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("2 attachments sent", 2, email.Attachments.Count);
		}

		public void TestSendEmailWithOnlyASubjectAndNotificationGroupCode()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			NotificationBuffer innerNotify = new NotificationBuffer();
			NotificationBuffer buffer = new NotificationBuffer(innerNotify);
			IGlbGroup notificationGroup = SetupEmailNotificationGroup();
			AssertNotNull(notificationGroup);

			IGlbStaff staff = factory.New<IGlbStaff>();
			notificationGroup.Staff.Add((BusinessObject)staff);
			staff.GS_Code = "ZAC";
			staff.GS_EmailAddress = "user@test.com";

			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
			factory.Save();

			buffer.SendEmail("Test Subject", notificationGroup.GG_Code);
			AssertEquals("Number of emails sent should be 1", 1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
		}

		[TestDate(2005, 1, 2)]
		public void TestEmailBody_WithDefaultHeaderFooter()
		{
			NotificationBufferForTest notify = new NotificationBufferForTest();
			notify.Notify(new ErrorNotification(ErrorType.Error, "Message"));
			AssertMultilineASCIIEquals("Correct EmailBody with default header/footer", @$"
Data Notifications; Generated 02-Jan-05 00:00:00

Error: Message

{Enterprise.Core.Constants.ProductName} Data Notifications Process
".Trim(), notify.EmailBody);
		}

		[TestDate(2005, 1, 2)]
		public void TestEmailBody_WithAlternateHeaderFooter()
		{
			NotificationBufferForTest notify = new WithAlternateEmailBodyHeaderFooter();
			notify.Notify(new ErrorNotification(ErrorType.Error, "Message"));
			AssertMultilineASCIIEquals("Correct EmailBody with default header/footer", @"
Alternate Header

Error: Message

Alternate Footer
".Trim(), notify.EmailBody);
		}

		#endregion

#if NETFRAMEWORK
		public void TestInitializeLifetimeService()
		{
			NotificationBuffer buffer = new NotificationBuffer();
			AssertEquals("When remoting across an AppDomain, with a lease the object will expire after 5 minutes.", null, buffer.InitializeLifetimeService());
		}
#endif

		public void TestGenerateEmailBody()
		{
			NotificationBufferForTest buffer = new NotificationBufferForTest();
			Assert("Should at least have some header or footer in the email", buffer.EmailBody.Trim().Length > 0);

			buffer.Notify(new InfoNotification("some info"));
			Assert("includes info", buffer.EmailBody.IndexOf("some info") != -1);

			buffer.Notify(new ErrorNotification(ErrorType.MissingHeader, "some error"));
			Assert("includes error type message", buffer.EmailBody.IndexOf("header") != -1);
			Assert("includes error message", buffer.EmailBody.IndexOf("some error") != -1);
		}

		public void TestAsString()
		{
			NotificationBuffer buffer = new NotificationBuffer();

			buffer.Notify(new ErrorNotification(ErrorType.Error, "ErrorMessage"));
			buffer.Notify(new WarningNotification(WarningType.MaxLengthExceeded, "Field XYZ has a max length of 20"));
			buffer.Notify(new BOErrorNotification("BOError"));
			buffer.Notify(new BOWarningNotification("BOWarning"));
			buffer.Notify(new InfoNotification("Info"));

			string expectedString = "Error: ErrorMessage\r\n" +
				"Warning: Maximum length of this field has been exceeded (Field XYZ has a max length of 20)\r\n" +
				"Error: BOError\r\n" +
				"Warning: BOWarning\r\n" +
				"Info\r\n";

			AssertEquals("As String", expectedString, buffer.AsString);
		}

		public void TestAsStringDoesntIncludeBlankNotifications()
		{
			NotificationBuffer buffer = new NotificationBuffer();

			buffer.Notify(new ErrorNotification(ErrorType.Error, "ErrorMessage"));
			buffer.Notify(new InfoNotification(""));
			buffer.Notify(new BOErrorNotification("BOError"));

			string expectedString =
								"Error: ErrorMessage\r\n" +
				"Error: BOError\r\n";

			AssertEquals("As String", expectedString, buffer.AsString);
			Enterprise.ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestAsStringWithNewlineNotification()
		{
			NotificationBuffer buffer = new NotificationBuffer();

			buffer.Notify(new ErrorNotification(ErrorType.Error, "ErrorMessage"));
			buffer.Notify(new NewlineNotification());
			buffer.Notify(new BOErrorNotification("BOError"));

			string expectedString =
				"Error: ErrorMessage\r\n" +
				"\r\n" +
				"Error: BOError\r\n";

			AssertEquals("As String", expectedString, buffer.AsString);
		}

		public void TestGetEventsByType()
		{
			NotificationBuffer buffer = new NotificationBuffer();
			InfoNotification event1 = new InfoNotification("this is info");
			WarningNotification event2 = new WarningNotification(WarningType.Warning, "warning1");
			WarningNotification event3 = new WarningNotification(WarningType.Warning, "warning2");
			VerboseInfoNotification event4 = new VerboseInfoNotification("verbose 1");
			ErrorNotification event5 = new ErrorNotification(ErrorType.Error, "error");
			ErrorNotification event6 = new ErrorNotification(ErrorType.Error, "error2");
			ErrorNotification event7 = new ErrorNotification(ErrorType.DataTypeConversionError, "error2");

			buffer.Notify(event1);
			buffer.Notify(event2);
			buffer.Notify(event3);
			buffer.Notify(event4);
			buffer.Notify(event5);
			buffer.Notify(event6);
			buffer.Notify(event7);

			INotification[] result = buffer.GetEventsByType(NotificationSubscriberType.Info);
			AssertEquals(1, result.Length);
			AssertEquals(event1, result[0]);

			result = buffer.GetEventsByType(WarningType.Warning);
			AssertEquals(2, result.Length);
			AssertEquals(event2, result[0]);
			AssertEquals(event3, result[1]);

			result = buffer.GetEventsByType(NotificationSubscriberType.VerboseInfo);
			AssertEquals(1, result.Length);
			AssertEquals(event4, result[0]);

			result = buffer.GetEventsByType(ErrorType.Error);
			AssertEquals(2, result.Length);
			AssertEquals(event5, result[0]);
			AssertEquals(event6, result[1]);

			result = buffer.GetEventsByType(ErrorType.DataTypeConversionError);
			AssertEquals(1, result.Length);
			AssertEquals(event7, result[0]);

			result = buffer.GetEventsByType(ErrorType.Error, WarningType.Warning);
			AssertEquals(4, result.Length);
			AssertEquals(event2, result[0]);
			AssertEquals(event3, result[1]);
			AssertEquals(event5, result[2]);
			AssertEquals(event6, result[3]);
		}

		public void TestHasErrors()
		{
			NotificationBuffer notify = new NotificationBuffer();
			notify.Notify(new WarningNotification(""));
			AssertEquals("Warnings aren't errors", false, notify.HasErrors);

			notify.Notify(new ErrorNotification(ErrorType.RequiredFieldEmpty, ""));
			AssertEquals("Should have errors now", true, notify.HasErrors);

			notify.Clear();
			AssertEquals("No errors", false, notify.HasErrors);

			notify.AddError("xxx");
			AssertEquals("Should have errors after AddError()", true, notify.HasErrors);
		}

		public void TestHasWarnings()
		{
			NotificationBuffer notify = new NotificationBuffer();
			notify.Notify(new ErrorNotification(ErrorType.RequiredFieldEmpty, ""));
			AssertEquals("Errors aren't warnings", false, notify.HasWarnings);

			notify.Notify(new WarningNotification(""));
			AssertEquals("Should have warnings now", true, notify.HasWarnings);

			notify.Clear();
			AssertEquals("No warnings", false, notify.HasWarnings);

			notify.AddWarning("xxx");
			AssertEquals("Should have warnings after AddWarning()", true, notify.HasWarnings);
		}

		public void TestHasErrorsNotIncluding()
		{
			NotificationBuffer notify = new NotificationBuffer();
			notify.Notify(new ErrorNotification(ErrorType.RequiredFieldEmpty, ""));
			AssertEquals("Should have any errors, not including RequiredFieldEmpty", false, notify.HasErrorsNotIncluding(ErrorType.RequiredFieldEmpty));

			notify.Notify(new ErrorNotification(ErrorType.UnknownCode, ""));
			AssertEquals("Has errors other than RequiredFieldEmpty", true, notify.HasErrorsNotIncluding(ErrorType.RequiredFieldEmpty));
		}

		public void TestClear()
		{
			NotificationBuffer notify = new NotificationBuffer();
			notify.Notify(new ErrorNotification(ErrorType.Error));
			AssertEquals("Notification has 1 event", 1, notify.Events.Length);

			notify.Clear();

			AssertEquals("Notification has No events", 0, notify.Events.Length);
			Assert("Should have no errors because there should be 0 notification types", !notify.HasErrors);
			Assert("Should have no warnings because there should be 0 notification types", !notify.HasWarnings);
		}

		#region TestSendEmailWithFactorySaveException

		public void TestSendEmailWithFactoryZSaveException()
		{
			ZDataException dataException = new ZDataException(new Exception(), new System.Data.DataTable().NewRow(), Db.Connection);
			ZSaveException saveException = new ZSaveException(dataException, new BusinessObjectFactory());
			TestSendEmailWithFactorySaveException(saveException);
		}

		public void TestSendEmailWithFactorySqlException()
		{
			TestSendEmailWithFactorySaveException(CreateSqlException());
		}

		SqlException CreateSqlException()
		{
			SqlException result = null;
			try
			{
#pragma warning disable CW1116 // Use CargoWise.Data.Db.Connection
				new SqlConnection("database=\"throw an exception, I dare u\"").Open(); // This is a test so new db connection does not matter
#pragma warning restore CW1116 // Use CargoWise.Data.Db.Connection
			}
			catch (SqlException ex)
			{
				result = ex;
			}
			return result;
		}

		void TestSendEmailWithFactorySaveException(Exception exception)
		{
			IGlbGroup testEmailGroup = SetupEmailNotificationGroup();
			BusinessObjectFactoryWithSaveException factory = new BusinessObjectFactoryWithSaveException(exception);

			NotificationBuffer innerNotify = new NotificationBuffer();
			NotificationBuffer buffer = new NotificationBuffer(innerNotify);
			buffer.SendEmail(testEmailGroup.PK.ToGuid(), null, "subject", "body header", "body footer", buffer, EnvProxy.Instance.OutgoingMailManager, factory);
			AssertEquals("Should be errors because an exception was thrown", true, innerNotify.ContainsNotificationType(ErrorType.ErrorSendingEmail));
		}

		#endregion

		#region TestQueryUserCallsInnerNotify

		public void TestQueryUserCallsInnerNotify()
		{
			NotificationSubscriberForQueryUserTest innerNotify = new NotificationSubscriberForQueryUserTest();
			NotificationBuffer wrappingNotify = new NotificationBuffer(innerNotify);
			QueryUserYesNoEventArgs e = new QueryUserYesNoEventArgs("", true);

			((INotificationSubscriberQueryUser)wrappingNotify).QueryUser(e);
			AssertEquals("Should have passed the QueryUserEventArgs to the inner notify", true, e == innerNotify.LastQueryUserEventArgs);
		}

		#endregion

		#region Implementation

		IGlbGroup SetupEmailNotificationGroup()
		{
			string testGroupCode = "tst";
			IGlbGroup group = Factory.LoadTop1<IGlbGroup>(
				new ZQuery(GlbGroupSchema.GG_Code, SQLComparisonOperator.Equal, testGroupCode));
			if (group != null)
			{
				((EnterpriseBusinessObject)group).Delete();
			}
			IGlbGroup newNotificationsGroup = Factory.New<IGlbGroup>();
			newNotificationsGroup.GG_Code = testGroupCode;
			Factory.Save();

			return newNotificationsGroup;
		}

		#endregion
	}
}
