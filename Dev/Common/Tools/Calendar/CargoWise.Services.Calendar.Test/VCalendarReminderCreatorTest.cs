using System;
using System.Text;
using CargoWise.BrandManager;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.Services.Calendar.Testing
{
	sealed class VCalendarReminderCreatorTest : TransactionedTestCase
	{
		[ExpectNoExceptions]
		public void TestEmailFailedExceptionDoesNotBlowUp()
		{
			ReminderBase rem = new ReminderForTesting("hello", DateTimeKind.Local, new DateTime(2006, 11, 11), new DateTime(2006, 11, 11), "d", "d");
			VCalendarReminderCreator vCal = new VCalendarReminderCreator(rem);
			vCal.CreateAppointment();
		}

		public void TestCreateReminder()
		{
			ReminderBase reminder = new ReminderForTesting("MyOwnID", DateTimeKind.Local, new DateTime(2005, 11, 26, 11, 13, 0), new TimeSpan(2, 0, 0), "This is my test subject with a really nice and long length that's over 70 chars so it should get split.", "Some Body\r\nAnother Line of the body");
			reminder.Location = "99 Nowhere st";
			reminder.AlarmPeriod = new TimeSpan(2, 30, 0);

			reminder.Recipients.Add("Zubin Appoo", "zappoo@zip.com.au");
			reminder.Recipients.Add("Someone Else", "noone@iknow.com");
			reminder.Recipients.Add("Last Person", "hello@there.com");
			reminder.Recipients.Add(Env.CurrentUser.FullName, new EmailDef(Env.CurrentUser.PK).FromAddress);

			VCalendarReminderCreator creator = new VCalendarReminderCreator(reminder);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			creator.CreateAppointment();

			AssertEquals("1 email created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			EmailDef sentEmail = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("From is current user", Env.CurrentUser.FullName, sentEmail.FromDisplayName);
			AssertEquals("From is current user", "Default@edi.com.au", sentEmail.FromAddress);

			AssertEquals(4, sentEmail.Recipients.Count);

			AssertEquals("Correct recipients", "zappoo@zip.com.au", sentEmail.Recipients[0].Email);
			AssertEquals("Correct recipients", "noone@iknow.com", sentEmail.Recipients[1].Email);
			AssertEquals("Correct recipients", "hello@there.com", sentEmail.Recipients[2].Email);

			AssertEquals("Correct ContentType", EmailContentTypes.Calendar, sentEmail.ContentType);

			string expectedVCalMessage = string.Format("BEGIN:VCALENDAR\r\n" +
"METHOD:REQUEST\r\n" +
"PRODID:{0}\r\n" +
"VERSION:2.0\r\n" +
"BEGIN:VEVENT\r\n" +
"DTSTAMP:{1}\r\n" +
"DTSTART:{2}\r\n" +
"SUMMARY:This is my test subject with a really nice and long length that's o\r\n" +
" ver 70 chars so it should get split.\r\n" +
"UID:MyOwnID\r\n" +
"SEQUENCE:123\r\n" +
"ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=TRUE;CN=\"zappoo@zi\r\n" +
" p.com.au\":MAILTO:zappoo@zip.com.au\r\n" +
"ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=TRUE;CN=\"noone@ikn\r\n" +
" ow.com\":MAILTO:noone@iknow.com\r\n" +
"ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=TRUE;CN=\"hello@the\r\n" +
" re.com\":MAILTO:hello@there.com\r\n" +
"ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=FALSE;CN=\"Default@\r\n" +
" edi.com.au\":MAILTO:Default@edi.com.au\r\n" +
"ORGANIZER;CN=\"{3}\":MAILTO:Default@edi.com.au\r\n" +
"LOCATION:99 Nowhere st\r\n" +
"DTEND:{4}\r\n" +
"DESCRIPTION:Some Body\\NAnother Line of the body\r\n" +
"BEGIN:VALARM\r\n" +
"ACTION:DISPLAY\r\n" +
"DESCRIPTION:REMINDER\r\n" +
"TRIGGER;RELATED=START:-PT02H30M00S\r\n" +
"END:VALARM\r\n" +
"TRANSP:OPAQUE\r\n" +
"END:VEVENT\r\n" +
"END:VCALENDAR",

				BrandingFactory.Instance.ProductName,
				GetAndCheckDateStamp(sentEmail.Body),
				reminder.UTCDateFrom.ToString("yyyyMMdd\\THHmmss\\Z"),
				BrandingFactory.Instance.ProductSupportName,
				reminder.UTCDateTo.ToString("yyyyMMdd\\THHmmss\\Z"));

			AssertEquals("Correct Body - vCalendar Message", expectedVCalMessage, sentEmail.Body);
			AssertEquals("Correct Subject should be displayed", sentEmail.Subject, reminder.Subject);
		}

		public void TestCreateReminder_HtmlFormat()
		{
			ReminderBase reminder = new ReminderForTesting("GoGoGo", DateTimeKind.Local, new DateTime(2005, 11, 26, 11, 13, 0), new DateTime(2005, 11, 26, 13, 13, 0), "This is my test subject with a really nice and long length that's over 70 chars so it should get split.", "Some Body\r\nAnother Line of the body", "<HTML><HEAD><TITILE></TITLE></HEAD><BODY>Some Body<BR />Another Line of the body</BODY></HTML>");
			reminder.Location = "A13 Some Ave";
			reminder.AlarmPeriod = new TimeSpan(2, 30, 0);

			reminder.Recipients.Add("Samuel", "Samuel@yoyo.com.au");
			reminder.Recipients.Add(Env.CurrentUser.FullName, new EmailDef(Env.CurrentUser.PK).FromAddress);

			VCalendarReminderCreator creator = new VCalendarReminderCreator(reminder);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			creator.CreateAppointment();

			AssertEquals("1 email created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			EmailDef sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("From is current user", Env.CurrentUser.FullName, sentEmail.FromDisplayName);
			AssertEquals("From is current user", "Default@edi.com.au", sentEmail.FromAddress);

			AssertEquals(2, sentEmail.Recipients.Count);
			AssertEquals("Correct recipients", "Samuel@yoyo.com.au", sentEmail.Recipients[0].Email);
			AssertEquals("Correct ContentType", EmailContentTypes.Calendar, sentEmail.ContentType);

			string expectedVCalMessage = string.Format("BEGIN:VCALENDAR\r\n" +
"METHOD:REQUEST\r\n" +
"PRODID:{0}\r\n" +
"VERSION:2.0\r\n" +
"BEGIN:VEVENT\r\n" +
"DTSTAMP:{1}\r\n" +
"DTSTART:{2}\r\n" +
"SUMMARY:This is my test subject with a really nice and long length that's o\r\n" +
" ver 70 chars so it should get split.\r\n" +
"UID:GoGoGo\r\n" +
"SEQUENCE:123\r\n" +
"ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=TRUE;CN=\"Samuel@yo\r\n" +
" yo.com.au\":MAILTO:Samuel@yoyo.com.au\r\n" +
"ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=FALSE;CN=\"Default@\r\n" +
" edi.com.au\":MAILTO:Default@edi.com.au\r\n" +
"ORGANIZER;CN=\"{3}\":MAILTO:Default@edi.com.au\r\n" +
"LOCATION:A13 Some Ave\r\n" +
"DTEND:{4}\r\n" +
"DESCRIPTION:Some Body\\NAnother Line of the body\r\n" +
"X-ALT-DESC;FMTTYPE=text/html:<HTML><HEAD><TITILE></TITLE></HEAD><BODY>Some \r\n" +
" Body<BR />Another Line of the body</BODY></HTML>\r\n" +
"BEGIN:VALARM\r\n" +
"ACTION:DISPLAY\r\n" +
"DESCRIPTION:REMINDER\r\n" +
"TRIGGER;RELATED=START:-PT02H30M00S\r\n" +
"END:VALARM\r\n" +
"TRANSP:OPAQUE\r\n" +
"END:VEVENT\r\n" +
"END:VCALENDAR",

				BrandingFactory.Instance.ProductName,
				GetAndCheckDateStamp(sentEmail.Body),
				reminder.UTCDateFrom.ToString("yyyyMMdd\\THHmmss\\Z"),
				BrandingFactory.Instance.ProductSupportName,
				reminder.UTCDateTo.ToString("yyyyMMdd\\THHmmss\\Z"));

			AssertEquals("Correct Body - vCalendar Message", expectedVCalMessage, sentEmail.Body);
			AssertEquals("Correct Subject should be displayed", sentEmail.Subject, reminder.Subject);
		}

		public void TestCreateReminderForCancellation()
		{
			ReminderBase reminder = new ReminderForTesting("MyOwnID", DateTimeKind.Local, new DateTime(2005, 11, 26, 11, 13, 0), new TimeSpan(2, 0, 0), "This is my test subject with a really nice and long length that's over 70 chars so it should get split.", "Some Body\r\nAnother Line of the body");
			reminder.Location = "99 Nowhere st";
			reminder.AlarmPeriod = new TimeSpan(2, 30, 0);

			reminder.Recipients.Add("Zubin Appoo", "zappoo@zip.com.au");
			reminder.Recipients.Add("Someone Else", "noone@iknow.com");
			reminder.Recipients.Add("Last Person", "hello@there.com");

			reminder.ReminderType = ReminderType.Cancellation;

			VCalendarReminderCreator creator = new VCalendarReminderCreator(reminder);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			creator.CreateAppointment();

			AssertEquals("1 email created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef sentEmail = Env.OutgoingMailManager.EmailsCreated[0];

			string expectedVCalMessage = string.Format("BEGIN:VCALENDAR\r\n" +
"METHOD:CANCEL\r\n" +
"PRODID:{0}\r\n" +
"VERSION:2.0\r\n" +
"BEGIN:VEVENT\r\n" +
"DTSTAMP:{1}\r\n" +
"DTSTART:{2}\r\n" +
"SUMMARY:This is my test subject with a really nice and long length that's o\r\n" +
" ver 70 chars so it should get split.\r\n" +
"UID:MyOwnID\r\n" +
"SEQUENCE:123\r\n" +
"ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=TRUE;CN=\"zappoo@zi\r\n" +
" p.com.au\":MAILTO:zappoo@zip.com.au\r\n" +
"ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=TRUE;CN=\"noone@ikn\r\n" +
" ow.com\":MAILTO:noone@iknow.com\r\n" +
"ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=TRUE;CN=\"hello@the\r\n" +
" re.com\":MAILTO:hello@there.com\r\n" +
"ORGANIZER;CN=\"{3}\":MAILTO:Default@edi.com.au\r\n" +
"LOCATION:99 Nowhere st\r\n" +
"DTEND:{4}\r\n" +
"DESCRIPTION:Some Body\\NAnother Line of the body\r\n" +
"BEGIN:VALARM\r\n" +
"ACTION:DISPLAY\r\n" +
"DESCRIPTION:REMINDER\r\n" +
"TRIGGER;RELATED=START:-PT02H30M00S\r\n" +
"END:VALARM\r\n" +
"TRANSP:OPAQUE\r\n" +
"STATUS:CANCELLED\r\n" +
"END:VEVENT\r\n" +
"END:VCALENDAR",

				BrandingFactory.Instance.ProductName,
				GetAndCheckDateStamp(sentEmail.Body),
				reminder.UTCDateFrom.ToString("yyyyMMdd\\THHmmss\\Z"),
				BrandingFactory.Instance.ProductSupportName,
				reminder.UTCDateTo.ToString("yyyyMMdd\\THHmmss\\Z"));

			AssertEquals("Correct Body - vCalendar Cancellation Message", expectedVCalMessage, sentEmail.Body);
			AssertEquals("Correct Subject should be displayed", sentEmail.Subject, reminder.Subject);
		}

		public void TestCreateReminderNoRecipients()
		{
			ReminderBase reminder = new ReminderForTesting("MyOwnID", DateTimeKind.Local, new DateTime(2005, 11, 26, 11, 13, 0), new TimeSpan(2, 0, 0), "", "");
			reminder.Location = "99 Nowhere st";
			reminder.AlarmPeriod = new TimeSpan(2, 30, 0);

			VCalendarReminderCreator creator = new VCalendarReminderCreator(reminder);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			creator.CreateAppointment();
			AssertEquals("No emails created as no recipients", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			using (Env.CurrentUser.SetUserEmailAddressInTESTINGOnly("test@example.com"))
			{
				creator.CreateAppointment();
				AssertEquals("1 email as no recipients, but current user has an email address", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				EmailDef sentEmail = Env.OutgoingMailManager.EmailsCreated[0];

				Assert(sentEmail.Body.Contains("ORGANIZER;CN=\"CargoWise Support\":MAILTO:Default@edi.com.au\r\n"));
			}
		}

		public void TestCreateAppointmentWithAttachment()
		{
			var reminder = new ReminderForTesting("ReminderAttachment", DateTimeKind.Local, new DateTime(2005, 11, 26, 11, 13, 0), new TimeSpan(2, 0, 0), "", "");
			reminder.Location = "secret location";
			reminder.AlarmPeriod = new TimeSpan(2, 30, 0);
			reminder.Recipients.Add("Someone Else", "noone@iknow.com");

			var attachmentDef = new AttachmentDef("attachmentData.erf", Encoding.ASCII.GetBytes("Attachment Data"));
			var attachmentDef2 = new AttachmentDef("attachmentData2.erf", Encoding.ASCII.GetBytes("Attachment Data 2"));

			reminder.Attachments.Add(attachmentDef);
			reminder.Attachments.Add(attachmentDef2);

			var creator = new VCalendarReminderCreator(reminder);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			creator.CreateAppointment();

			AssertEquals("1 email created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];

			AssertContainsExactElementsInAnyOrder(new[] { attachmentDef, attachmentDef2 }, sentEmail.Attachments);
		}

		public void TestCreateAppointment_RecipientsAreAllOrganisers()
		{
			EnvProxy.Instance.Registry.CalendarInvitationSolutionForOrganisers = true;
			Guid staffPK = Guid.NewGuid();
			InsertStaff(staffPK, "Jenny Nguyen", "jenny.nguyen@wtgmailtest.com");

			using (Env.SetTemporaryUserContext(staffPK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				ReminderBase reminder = new ReminderForTesting(
					"TooManyOrganisers",
					DateTimeKind.Local,
					new DateTime(2013, 8, 30, 11, 10, 0),
					new TimeSpan(8, 8, 8),
					"Subject",
					"Follow up reminder regarding a quotation");
				reminder.Location = "1 Doody Street";
				reminder.AlarmPeriod = new TimeSpan(1, 0, 0);
				reminder.Recipients.Add(Env.CurrentUser.FullName, Env.CurrentUser.EmailAddress);
				reminder.Recipients.Add(Env.CurrentUser.FullName, Env.CurrentUser.EmailAddress);

				VCalendarReminderCreator creator = new VCalendarReminderCreator(reminder);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				AssertNoExceptionThrown("Reminder is sent successfully", () => creator.CreateAppointment());
			}
		}

		public void TestCreateAppointment_OtherRecipients()
		{
			Guid staffPK = Guid.NewGuid();
			InsertStaff(staffPK, "Jenny Nguyen", "jenny.nguyen@wtgmailtest.com");

			using (Env.SetTemporaryUserContext(staffPK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				// Test data
				ReminderBase reminderToManyRecipients = GetReminderForTesting_LotusNotesRecipients(true, true);
				VCalendarReminderCreator creatorForReminderToManyRecipients = new VCalendarReminderCreator(reminderToManyRecipients);

				ReminderBase reminderToOneRecipientWhosTheOrganiser = GetReminderForTesting_LotusNotesRecipients(true, false);
				VCalendarReminderCreator creatorForReminderToOneRecipientWhosTheOrganiser = new VCalendarReminderCreator(reminderToOneRecipientWhosTheOrganiser);

				ReminderBase reminderToOneRecipientWhosNOTTheOrganiser = GetReminderForTesting_LotusNotesRecipients(false, false);
				VCalendarReminderCreator creatorForReminderToOneRecipientWhosNOTTheOrganiser = new VCalendarReminderCreator(reminderToOneRecipientWhosNOTTheOrganiser);

				EnvProxy.Instance.Registry.CalendarInvitationSolutionForOrganisers = true;

				#region reminderToManyRecipients - Lotus Users

				Env.OutgoingMailManager.EmailsCreated.Clear();
				creatorForReminderToManyRecipients.CreateAppointment();

				AssertEquals("2 emails created - one email for the organiser and one email for the other recipients", 2, Env.OutgoingMailManager.EmailsCreated.Count);

				EmailDef emailSentToOrganiser = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Pre-condition: one email is sent exclusively to the organiser", 1, emailSentToOrganiser.Recipients.Count);
				AssertEquals("From is current company", Env.CurrentCompany.Name, emailSentToOrganiser.FromDisplayName);
				AssertEquals("From email is system email", "Default@edi.com.au", emailSentToOrganiser.FromAddress);
				AssertEquals("Correct ContentType", EmailContentTypes.Calendar, emailSentToOrganiser.ContentType);
				AssertEquals("Correct recipient - organiser/current user", Env.CurrentUser.EmailAddress, emailSentToOrganiser.Recipients[0].Email);
				AssertEquals("Correct body of email sent to the organiser - vCalendar message",
					GetExpectedVCalMessage_LotusNotesRecipients(
						emailSentToOrganiser,
						reminderToManyRecipients,
						Env.CurrentCompany.Name,
						Env.Instance.Registry.EnterpriseMailboxEmailAddress,
						true,
						true),
					emailSentToOrganiser.Body);

				EmailDef emailSentToAttendees = Env.OutgoingMailManager.EmailsCreated[1];
				AssertEquals("Pre-condition: one email is sent to the other recipients", 2, emailSentToAttendees.Recipients.Count);
				AssertEquals("From is current user", Env.CurrentUser.FullName, emailSentToAttendees.FromDisplayName);
				AssertEquals("From is current user", Env.CurrentUser.EmailAddress, emailSentToAttendees.FromAddress);
				AssertEquals("Correct ContentType", EmailContentTypes.Calendar, emailSentToAttendees.ContentType);
				AssertEquals("Correct recipient - non-organiser 1", "richard.smith@lotusnotes.com", emailSentToAttendees.Recipients[0].Email);
				AssertEquals("Correct recipient - non-organiser 2", "francisco.lorenzo@lotusnotes.com", emailSentToAttendees.Recipients[1].Email);
				AssertEquals("Correct body of email sent to non-organiser recipients - vCalendar message",
					GetExpectedVCalMessage_LotusNotesRecipients(
						emailSentToAttendees,
						reminderToManyRecipients,
						Env.CurrentUser.FullName,
						Env.CurrentUser.EmailAddress,
						true,
						true),
					emailSentToAttendees.Body);

				#endregion

				#region reminderToOneRecipientWhosTheOrganiser - Lotus Users

				Env.OutgoingMailManager.EmailsCreated.Clear();
				creatorForReminderToOneRecipientWhosTheOrganiser.CreateAppointment();

				AssertEquals("1 email created - one email for the organiser who's the only recipient", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				EmailDef emailSentToOneRecipientWhosTheOrganiser = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Pre-condition: email is sent to the organiser who's the only recipient", 1, emailSentToOneRecipientWhosTheOrganiser.Recipients.Count);
				AssertEquals("From is current company", Env.CurrentCompany.Name, emailSentToOneRecipientWhosTheOrganiser.FromDisplayName);
				AssertEquals("From email is system email", "Default@edi.com.au", emailSentToOneRecipientWhosTheOrganiser.FromAddress);
				AssertEquals("Correct ContentType", EmailContentTypes.Calendar, emailSentToOneRecipientWhosTheOrganiser.ContentType);
				AssertEquals("Correct recipient - organiser/current user", Env.CurrentUser.EmailAddress, emailSentToOneRecipientWhosTheOrganiser.Recipients[0].Email);
				AssertEquals("Correct body of email sent to the organiser who's the only recipient - vCalendar message",
					GetExpectedVCalMessage_LotusNotesRecipients(
						emailSentToOneRecipientWhosTheOrganiser,
						reminderToOneRecipientWhosTheOrganiser,
						Env.CurrentCompany.Name,
						Env.Instance.Registry.EnterpriseMailboxEmailAddress,
						false,
						true),
					emailSentToOneRecipientWhosTheOrganiser.Body);

				#endregion

				#region reminderToOneRecipientWhosNOTTheOrganiser - Lotus Users

				Env.OutgoingMailManager.EmailsCreated.Clear();
				creatorForReminderToOneRecipientWhosNOTTheOrganiser.CreateAppointment();

				AssertEquals("1 email created - one email for the one recipient who's not the organiser", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef emailSentToOneRecipientWhosNOTTheOrganiser = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Pre-condition: email is sent to the one recipient who's not the organiser", 1, emailSentToOneRecipientWhosNOTTheOrganiser.Recipients.Count);
				AssertEquals("From is current user", Env.CurrentUser.FullName, emailSentToOneRecipientWhosNOTTheOrganiser.FromDisplayName);
				AssertEquals("From is current user", Env.CurrentUser.EmailAddress, emailSentToOneRecipientWhosNOTTheOrganiser.FromAddress);
				AssertEquals("Correct ContentType", EmailContentTypes.Calendar, emailSentToOneRecipientWhosNOTTheOrganiser.ContentType);
				AssertEquals("Correct recipient - recipient who's not the organiser/current user", "richard.smith@lotusnotes.com", emailSentToOneRecipientWhosNOTTheOrganiser.Recipients[0].Email);
				AssertEquals("Correct body of email sent to the recipient who's not the organiser/current user - vCalendar message",
					GetExpectedVCalMessage_LotusNotesRecipients(
						emailSentToOneRecipientWhosNOTTheOrganiser,
						reminderToOneRecipientWhosNOTTheOrganiser,
						Env.CurrentUser.FullName,
						Env.CurrentUser.EmailAddress,
						false,
						false),
					emailSentToOneRecipientWhosNOTTheOrganiser.Body);

				#endregion

				EnvProxy.Instance.Registry.CalendarInvitationSolutionForOrganisers = false;

				#region reminderToManyRecipients - Non-Lotus users

				Env.OutgoingMailManager.EmailsCreated.Clear();
				creatorForReminderToManyRecipients.CreateAppointment();

				AssertEquals("1 email created - sent to everyone", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				EmailDef emailSentToEveryone = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Pre-condition: same email is sent to everyone", 3, emailSentToEveryone.Recipients.Count);
				AssertEquals("From is current user", Env.CurrentUser.FullName, emailSentToEveryone.FromDisplayName);
				AssertEquals("From is current user", Env.CurrentUser.EmailAddress, emailSentToEveryone.FromAddress);
				AssertEquals("Correct ContentType", EmailContentTypes.Calendar, emailSentToEveryone.ContentType);
				AssertEquals("Correct recipient - organiser/current user", Env.CurrentUser.EmailAddress, emailSentToEveryone.Recipients[0].Email);
				AssertEquals("Correct recipient - non-organiser 1", "richard.smith@lotusnotes.com", emailSentToEveryone.Recipients[1].Email);
				AssertEquals("Correct recipient - non-organiser 2", "francisco.lorenzo@lotusnotes.com", emailSentToEveryone.Recipients[2].Email);

				AssertEquals("Correct body of email sent to everyone - vCalendar message",
					GetExpectedVCalMessage_LotusNotesRecipients(
						emailSentToEveryone,
						reminderToManyRecipients,
						Env.CurrentUser.FullName,
						Env.CurrentUser.EmailAddress,
						true,
						true),
					emailSentToEveryone.Body);

				#endregion

				#region reminderToOneRecipientWhosTheOrganiser - Non-Lotus users

				Env.OutgoingMailManager.EmailsCreated.Clear();
				creatorForReminderToOneRecipientWhosTheOrganiser.CreateAppointment();

				AssertEquals("1 email created - one email for the organiser who's the only recipient", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				emailSentToOneRecipientWhosTheOrganiser = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Pre-condition: email is sent to the organiser who's the only recipient", 1, emailSentToOneRecipientWhosTheOrganiser.Recipients.Count);
				AssertEquals("From is current user", Env.CurrentUser.FullName, emailSentToOneRecipientWhosTheOrganiser.FromDisplayName);
				AssertEquals("From is current user", Env.CurrentUser.EmailAddress, emailSentToOneRecipientWhosTheOrganiser.FromAddress);
				AssertEquals("Correct ContentType", EmailContentTypes.Calendar, emailSentToOneRecipientWhosTheOrganiser.ContentType);
				AssertEquals("Correct recipient - organiser/current user", Env.CurrentUser.EmailAddress, emailSentToOneRecipientWhosTheOrganiser.Recipients[0].Email);
				AssertEquals("Correct body of email sent to the organiser who's the only recipient - vCalendar message",
					GetExpectedVCalMessage_LotusNotesRecipients(
						emailSentToOneRecipientWhosTheOrganiser,
						reminderToOneRecipientWhosTheOrganiser,
						Env.CurrentUser.FullName,
						Env.CurrentUser.EmailAddress,
						false,
						true),
					emailSentToOneRecipientWhosTheOrganiser.Body);

				#endregion

				#region reminderToOneRecipientWhosNOTTheOrganiser - Non-Lotus Users

				Env.OutgoingMailManager.EmailsCreated.Clear();
				creatorForReminderToOneRecipientWhosNOTTheOrganiser.CreateAppointment();

				AssertEquals("1 email created - one email for the one recipient who's not the organiser", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				emailSentToOneRecipientWhosNOTTheOrganiser = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Pre-condition: email is sent to the one recipient who's not the organiser", 1, emailSentToOneRecipientWhosNOTTheOrganiser.Recipients.Count);
				AssertEquals("From is current user", Env.CurrentUser.FullName, emailSentToOneRecipientWhosNOTTheOrganiser.FromDisplayName);
				AssertEquals("From is current user", Env.CurrentUser.EmailAddress, emailSentToOneRecipientWhosNOTTheOrganiser.FromAddress);
				AssertEquals("Correct ContentType", EmailContentTypes.Calendar, emailSentToOneRecipientWhosNOTTheOrganiser.ContentType);
				AssertEquals("Correct recipient - recipient who's not the organiser/current user", "richard.smith@lotusnotes.com", emailSentToOneRecipientWhosNOTTheOrganiser.Recipients[0].Email);
				AssertEquals("Correct body of email sent to the recipient who's not the organiser/current user - vCalendar message",
					GetExpectedVCalMessage_LotusNotesRecipients(
						emailSentToOneRecipientWhosNOTTheOrganiser,
						reminderToOneRecipientWhosNOTTheOrganiser,
						Env.CurrentUser.FullName,
						Env.CurrentUser.EmailAddress,
						false,
						false),
					emailSentToOneRecipientWhosNOTTheOrganiser.Body);

				#endregion
			}
		}

		public void TestCreateAppointment_OrganiserNameAndEmail()
		{
			EnvProxy.Instance.Registry.CalendarInvitationSolutionForOrganisers = true;
			var staffPK = Guid.NewGuid();
			InsertStaff(staffPK, "Test User", "test.user@abc.net");

			using (Env.SetTemporaryUserContext(staffPK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				ReminderBase reminder = new ReminderForTesting(
					"SendToSelf",
					DateTimeKind.Local,
					new DateTime(2018, 11, 1, 16, 0, 0),
					new TimeSpan(0, 30, 0),
					"Subject",
					"Follow up")
				{
					Location = "1 Doody Street",
					AlarmPeriod = new TimeSpan(0, 15, 0)
				};

				Env.OutgoingMailManager.EmailsCreated.Clear();
				var creator = new VCalendarReminderCreator(reminder);
				creator.CreateAppointment();

				var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
				var expectedOrganiserText = $@"ORGANIZER;CN=""{Env.CurrentCompany.Name}"":MAILTO:{Env.Instance.Registry.EnterpriseMailboxEmailAddress}";
				AssertContains(expectedOrganiserText, sentEmail.Body);
				AssertEquals(Env.CurrentCompany.Name, sentEmail.FromDisplayName);
				AssertEquals(Env.Instance.Registry.EnterpriseMailboxEmailAddress, sentEmail.FromAddress);
			}
		}

		[TestDate(2018, 8, 16, 16, 12, 35)]
		public void TestCreateAppointment_NoInvalidTime()
		{
			var reminder = new ReminderForTesting("MyOwnID", DateTimeKind.Local, ZDateTime.Empty, ZDateTime.Empty, "This is my test subject", "Some Body")
			{
				Location = "99 Nowhere st",
				AlarmPeriod = new TimeSpan(2, 30, 0)
			};
			reminder.Recipients.Add("Someone Else", "noone@iknow.com");

			Env.OutgoingMailManager.EmailsCreated.Clear();
			var creator = new VCalendarReminderCreator(reminder);
			creator.CreateAppointment();

			var expectedVCalMessage =
$@"BEGIN:VCALENDAR
METHOD:REQUEST
PRODID:{BrandingFactory.Instance.ProductName}
VERSION:2.0
BEGIN:VEVENT
DTSTAMP:20180816T161235Z
DTSTART:20180816T161235Z
SUMMARY:This is my test subject
UID:MyOwnID
SEQUENCE:123
ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=TRUE;CN=""noone@ikn
 ow.com"":MAILTO:noone@iknow.com
ORGANIZER;CN=""{BrandingFactory.Instance.ProductSupportName}"":MAILTO:Default@edi.com.au
LOCATION:99 Nowhere st
DTEND:20180816T161235Z
DESCRIPTION:Some Body
BEGIN:VALARM
ACTION:DISPLAY
DESCRIPTION:REMINDER
TRIGGER;RELATED=START:-PT02H30M00S
END:VALARM
TRANSP:OPAQUE
END:VEVENT
END:VCALENDAR";

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Correct Body - vCalendar Message", expectedVCalMessage, sentEmail.Body);
		}

		#region Implementation

		void InsertStaff(Guid pk, string fullname, string email)
		{
			var personPk = Guid.NewGuid();
			string personQuery = string.Format("insert into {0} ({1}, {2}) values (@PK, @PER_FullName)",
				GlbPersonSchema.Constants.TableName, GlbPersonSchema.PK.Name, GlbPersonSchema.PER_FullName.Name);
			using (var cmd = TestConnection.Command(personQuery))
			{
				cmd.AddParameterBasedOnDbColumn("@PK", personPk, GlbPersonSchema.PK);
				cmd.AddParameterBasedOnDbColumn("@PER_FullName", "some name", GlbPersonSchema.PER_FullName);
				cmd.ExecuteNonQuery();
			}

			string query = string.Format("INSERT INTO {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}) VALUES (@PK, @GS_Code, @GS_FullName, @GS_EmailAddress, @GS_PER, @GS_SystemCreateTimeUtc, @GS_SystemCreateUser, @GS_SystemLastEditTimeUtc, @GS_SystemLastEditUser)",
					GlbStaffSchema.Constants.TableName,
					GlbStaffSchema.PK.Name,
					GlbStaffSchema.GS_Code.Name,
					GlbStaffSchema.GS_FullName.Name,
					GlbStaffSchema.GS_EmailAddress.Name,
					GlbStaffSchema.GS_PER.Name,
					GlbStaffSchema.Constants.GS_SystemCreateTimeUtc,
					GlbStaffSchema.Constants.GS_SystemCreateUser,
					GlbStaffSchema.Constants.GS_SystemLastEditTimeUtc,
					GlbStaffSchema.Constants.GS_SystemLastEditUser);

			using (DbCommand command = TestConnection.Command(query))
			{
				command.AddParameterBasedOnDbColumn("@PK", pk, GlbStaffSchema.PK);
				command.AddParameterBasedOnDbColumn("@GS_Code", fullname.Substring(0, 3), GlbStaffSchema.GS_Code);
				command.AddParameterBasedOnDbColumn("@GS_FullName", fullname, GlbStaffSchema.GS_FullName);
				command.AddParameterBasedOnDbColumn("@GS_EmailAddress", email, GlbStaffSchema.GS_EmailAddress);
				command.AddParameterBasedOnDbColumn("@GS_PER", personPk, GlbStaffSchema.GS_PER);
				command.AddParameterBasedOnDbColumn("@GS_SystemCreateTimeUtc", DateTime.UtcNow, GlbStaffSchema.GS_SystemCreateTimeUtc);
				command.AddParameterBasedOnDbColumn("@GS_SystemCreateUser", "E", GlbStaffSchema.GS_SystemCreateUser);
				command.AddParameterBasedOnDbColumn("@GS_SystemLastEditTimeUtc", DateTime.UtcNow, GlbStaffSchema.GS_SystemLastEditTimeUtc);
				command.AddParameterBasedOnDbColumn("@GS_SystemLastEditUser", "E", GlbStaffSchema.GS_SystemLastEditUser);
				command.ExecuteNonQuery();
			}
		}

		ReminderBase GetReminderForTesting_LotusNotesRecipients(bool hasOrganiser, bool hasManyRecipients)
		{
			ReminderBase reminder = new ReminderForTesting(
					"ReverseLotusID",
					DateTimeKind.Local,
					new DateTime(2013, 8, 30, 11, 10, 0),
					new TimeSpan(8, 8, 8),
					"This is a placeholder subject that I hope the organiser that's on Lotus Notes can receive.",
					"Some-body I used to know\r\nRandom new line");
			reminder.Location = "1 Doody Street";
			reminder.AlarmPeriod = new TimeSpan(1, 0, 0);

			if (hasOrganiser)
			{
				reminder.Recipients.Add(Env.CurrentUser.FullName, Env.CurrentUser.EmailAddress);
			}

			if (hasManyRecipients)
			{
				reminder.Recipients.Add("Richard Smith", "richard.smith@lotusnotes.com");
				reminder.Recipients.Add("Francisco Lorenzo", "francisco.lorenzo@lotusnotes.com");
			}
			else if (!hasOrganiser)
			{
				reminder.Recipients.Add("Richard Smith", "richard.smith@lotusnotes.com");
			}

			return reminder;
		}

		string GetExpectedVCalMessage_LotusNotesRecipients(EmailDef email, ReminderBase reminder, string organiserName, string organiserEmail, bool hasManyRecipients, bool hasOrganiserAsRecipient)
		{
			string manyRecipients = @"ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=FALSE;CN=""jenny.ng
 uyen@wtgmailtest.com"":MAILTO:jenny.nguyen@wtgmailtest.com
ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=TRUE;CN=""richard.s
 mith@lotusnotes.com"":MAILTO:richard.smith@lotusnotes.com
ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=TRUE;CN=""francisco
 .lorenzo@lotusnotes.com"":MAILTO:francisco.lorenzo@lotusnotes.com";

			string oneRecipientAsOrganiser = @"ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=FALSE;CN=""jenny.ng
 uyen@wtgmailtest.com"":MAILTO:jenny.nguyen@wtgmailtest.com";

			string oneRecipientAsNonOrganiser = @"ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=TRUE;CN=""richard.s
 mith@lotusnotes.com"":MAILTO:richard.smith@lotusnotes.com";

			var receipientsAsText = hasManyRecipients ? manyRecipients : (hasOrganiserAsRecipient ? oneRecipientAsOrganiser : oneRecipientAsNonOrganiser);

			return $@"BEGIN:VCALENDAR
METHOD:REQUEST
PRODID:{BrandingFactory.Instance.ProductName}
VERSION:2.0
BEGIN:VEVENT
DTSTAMP:{GetAndCheckDateStamp(email.Body)}
DTSTART:{reminder.UTCDateFrom.ToString("yyyyMMdd\\THHmmss\\Z")}
SUMMARY:This is a placeholder subject that I hope the organiser that's on L
 otus Notes can receive.
UID:ReverseLotusID
SEQUENCE:123
{receipientsAsText}
ORGANIZER;CN=""{organiserName}"":MAILTO:{organiserEmail}
LOCATION:1 Doody Street
DTEND:{reminder.UTCDateTo.ToString("yyyyMMdd\\THHmmss\\Z")}
DESCRIPTION:Some-body I used to know\NRandom new line
BEGIN:VALARM
ACTION:DISPLAY
DESCRIPTION:REMINDER
TRIGGER;RELATED=START:-PT01H00M00S
END:VALARM
TRANSP:OPAQUE
END:VEVENT
END:VCALENDAR";
		}

		string GetAndCheckDateStamp(string vCalMessage)
		{
			string result = "";

			int dateStampStartIndex = vCalMessage.IndexOf("DTSTAMP:");
			if (dateStampStartIndex != -1)
			{
				dateStampStartIndex += 8;
				int dateStampEndIndex = vCalMessage.IndexOf("\r\n", dateStampStartIndex);

				if (dateStampEndIndex > dateStampStartIndex)
				{
					result = vCalMessage.Substring(dateStampStartIndex, dateStampEndIndex - dateStampStartIndex);
				}
			}

			ZDateTime resultTime;
			Assert("Date Time stamp should be parseable", ZDateTime.TryParseExact(result, out resultTime, "yyyyMMdd\\THHmmss\\Z"));
			Assert("Date Time stamp should not be empty", !resultTime.IsEmpty);
			return result;
		}

		#endregion
	}
}
