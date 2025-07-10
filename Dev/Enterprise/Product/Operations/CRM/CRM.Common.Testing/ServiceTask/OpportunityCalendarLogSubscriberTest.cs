using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.CRM.Common.Testing
{
	[TestedType(typeof(OpportunityCalendarLogSubscriber))]
	sealed class OpportunityCalendarLogSubscriberTest : LogSubscriberTest<OpportunityCalendarLogSubscriber>
	{
		public void TestCreateAppointment_EmailSendFailedExceptionIsHandled()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "a@bbb.com ";
			staff.GS_FullName = "President Alex";

			var staffRecipient = Factory.NewWithValidTestData<GlbStaff>();
			staffRecipient.GS_FullName = "Test Exception";
			staffRecipient.GS_EmailAddress = "test.exception@cargowise.com";

			Factory.Save();

			var opportunity = Factory.NewWithValidTestData<CrmOpportunity>();
			opportunity.COP_GS_NKSalesPerson = staffRecipient.GS_Code;
			opportunity.COP_NextFollowUp = new ZDateTimeOffset(2024, 7, 9, 13, 30, 00);

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var log = opportunity.Logs.AddNew();

				using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
				{
					log.SL_SE_NKEvent = AutoEvents.RecallDateUpdated.Code;
					log.SL_Reference = $"Opportunity {opportunity.COP_OpportunityID} Next Follow Up Date Updated|NEW=20240709T080000Z";
					log.SL_EventTime = ZDateTime.Now;
				}

				Factory.Save();

				var reminderMock = new Mock<Reminder>("Reminder ID", opportunity.PK, new ZString(opportunity.TableName), DateTimeKind.Local, ZDateTime.Now, ZDateTime.Now, "", "", "", null, new BusinessObjectFactory());
				reminderMock.Setup(r => r.CreateAppointment()).Throws(new EmailSendFailedException("Test exception"));

				var subscriber = new OpportunityCalendarLogSubscriber();
				var logger = new LoggerForTesting();
				subscriber.SetDefaultLogger(logger);

				var queuedLogMock = new Mock<IQueuedLog>();
				queuedLogMock.SetupGet(x => x.Factory).Returns(new BusinessObjectFactory());
				queuedLogMock.SetupGet(x => x.SJ_ALogReference).Returns(log.PK);

				AssertNoExceptionThrown(() => subscriber.CreateAppointment(queuedLogMock.Object, opportunity, (_, _) => reminderMock.Object));

				var eventList = logger.NotifiedEventList;
				AssertEquals("There should be 1 error logged", 1, eventList.Count);
				AssertEquals(@"[Opportunity Calendar Reminder Subscriber] Could not create appointment due to the following error:
Test exception", eventList[0]);
			}
		}

		public void TestProcessQueuedLogs_InviteCreationForOrgOpportunity()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "a@bbb.com ";
			staff.GS_FullName = "President Alex";

			var staffRecipient = Factory.NewWithValidTestData<GlbStaff>();
			staffRecipient.GS_FullName = "Zubin Appoo";
			staffRecipient.GS_EmailAddress = "zubin.appoo@cargowise.com";

			Factory.Save();

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_GS_NKPrimarySalesPerson = staffRecipient.GS_Code;
			opportunity.P8_RecallDate = new ZDateTime(2020, 1, 30, 18, 30, 00);

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Factory.Save();
			}

			Env.OutgoingMailManager.EmailsCreated.Clear();
			RunLogWalkerCycleForTest();

			AssertEquals("1 email created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("From is current user", staff.GS_FullName, sentEmail.FromDisplayName);
			AssertEquals("From is current user", staff.GS_EmailAddress, sentEmail.FromAddress);
			AssertEquals(1, sentEmail.Recipients.Count);

			AssertEquals("Correct recipients", "zubin.appoo@cargowise.com", sentEmail.Recipients[0].Email);
			AssertEquals("Correct ContentType", EmailContentTypes.Calendar, sentEmail.ContentType);
			AssertStartsWith("Correct vCalendar Message", "BEGIN:VCALENDAR", sentEmail.Body);
			AssertStartsWith("Correct Subject", "Recall due for Opportunity", sentEmail.Subject);
			AssertEquals("Correct meeting date", new ZDateTime(2020, 1, 30, 18, 30, 00), GetVCalendarMeetingDateTime(sentEmail.Body));
		}

		public void TestProcessQueuedLogs_InviteCancellationForOrgOpportunity()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "a@bbb.com ";
			staff.GS_FullName = "President Alex";

			var staffRecipient = Factory.NewWithValidTestData<GlbStaff>();
			staffRecipient.GS_FullName = "Zubin Appoo";
			staffRecipient.GS_EmailAddress = "zubin.appoo@cargowise.com";

			Factory.Save();

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_GS_NKPrimarySalesPerson = staffRecipient.GS_Code;
			opportunity.P8_RecallDate = new ZDateTime(2020, 1, 30, 18, 30, 00);

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Factory.Save();
			}

			RunLogWalkerCycleForTest();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			opportunity.P8_RecallDate = ZDateTime.Empty;
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Factory.Save();
			}

			RunLogWalkerCycleForTest();
			AssertEquals("1 email created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("From is current user", staff.GS_FullName, sentEmail.FromDisplayName);
			AssertEquals("From is current user", staff.GS_EmailAddress, sentEmail.FromAddress);
			AssertEquals(1, sentEmail.Recipients.Count);

			AssertEquals("Correct recipients", "zubin.appoo@cargowise.com", sentEmail.Recipients[0].Email);
			AssertEquals("Correct ContentType", EmailContentTypes.Calendar, sentEmail.ContentType);
			AssertStartsWith("Correct vCalendar Message", "BEGIN:VCALENDAR", sentEmail.Body);
			AssertStartsWith("Correct Subject", "Recall has been canceled", sentEmail.Subject);
			AssertEquals("Correct meeting date", new ZDateTime(2020, 1, 30, 18, 30, 00), GetVCalendarMeetingDateTime(sentEmail.Body));
		}

		public void TestProcessQueuedLogs_DeletedOpportunityForOrgOpportunity()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "a@bbb.com ";
			staff.GS_FullName = "President Alex";

			var staffRecipient = Factory.NewWithValidTestData<GlbStaff>();
			staffRecipient.GS_FullName = "Zubin Appoo";
			staffRecipient.GS_EmailAddress = "zubin.appoo@cargowise.com";

			Factory.Save();

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_GS_NKPrimarySalesPerson = staffRecipient.GS_Code;
			opportunity.P8_RecallDate = new ZDateTime(2020, 1, 30, 18, 30, 00);

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Factory.Save();
			}

			opportunity.Delete();
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			RunLogWalkerCycleForTest();

			AssertEquals("0 emails created", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		[TestUtcOffset(5, 30, 0)]
		public void TestProcessQueuedLogs_InviteCreationForCrmOpportunity()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "a@bbb.com ";
			staff.GS_FullName = "President Alex";

			var staffRecipient = Factory.NewWithValidTestData<GlbStaff>();
			staffRecipient.GS_FullName = "Zubin Appoo";
			staffRecipient.GS_EmailAddress = "zubin.appoo@cargowise.com";

			Factory.Save();

			var opportunity = Factory.NewWithValidTestData<CrmOpportunity>();
			opportunity.COP_GS_NKSalesPerson = staffRecipient.GS_Code;
			opportunity.COP_NextFollowUp = new ZDateTimeOffset(2024, 7, 9, 13, 30, 00);

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var log = opportunity.Logs.AddNew();

				using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
				{
					log.SL_SE_NKEvent = AutoEvents.RecallDateUpdated.Code;
					log.SL_Reference = $"Opportunity {opportunity.COP_OpportunityID} Next Follow Up Date Updated|NEW=20240709T080000Z";
					log.SL_EventTime = ZDateTime.Now;
				}

				Factory.Save();
			}

			Env.OutgoingMailManager.EmailsCreated.Clear();

			RunLogWalkerCycleForTest();

			AssertEquals("1 email created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("From is current user", staff.GS_FullName, sentEmail.FromDisplayName);
			AssertEquals("From is current user", staff.GS_EmailAddress, sentEmail.FromAddress);
			AssertEquals(1, sentEmail.Recipients.Count);

			AssertEquals("Correct recipients", "zubin.appoo@cargowise.com", sentEmail.Recipients[0].Email);
			AssertEquals("Correct ContentType", EmailContentTypes.Calendar, sentEmail.ContentType);
			AssertStartsWith("Correct vCalendar Message", "BEGIN:VCALENDAR", sentEmail.Body);
			AssertStartsWith("Correct Subject", "Next follow up for Opportunity", sentEmail.Subject);
			AssertEquals("Correct meeting date", new ZDateTime(2024, 7, 9, 8, 00, 00), GetVCalendarMeetingDateTime(sentEmail.Body));
		}

		[TestUtcOffset(5, 30, 0)]
		public void TestProcessQueuedLogs_InviteCancellationForCrmOpportunity()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "a@bbb.com ";
			staff.GS_FullName = "President Alex";

			var staffRecipient = Factory.NewWithValidTestData<GlbStaff>();
			staffRecipient.GS_FullName = "Zubin Appoo";
			staffRecipient.GS_EmailAddress = "zubin.appoo@cargowise.com";

			Factory.Save();

			var opportunity = Factory.NewWithValidTestData<CrmOpportunity>();
			opportunity.COP_GS_NKSalesPerson = staffRecipient.GS_Code;
			opportunity.COP_NextFollowUp = new ZDateTimeOffset(2024, 7, 9, 13, 30, 00);

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var log = opportunity.Logs.AddNew();

				using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
				{
					log.SL_SE_NKEvent = AutoEvents.RecallDateUpdated.Code;
					log.SL_Reference = $"Opportunity {opportunity.COP_OpportunityID} Next Follow Up Date Updated|NEW=20240709T080000Z";
					log.SL_EventTime = ZDateTime.Now;
				}

				Factory.Save();
			}

			RunLogWalkerCycleForTest();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			opportunity.COP_NextFollowUp = ZDateTimeOffset.Empty;
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var log = opportunity.Logs.AddNew();

				using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
				{
					log.SL_SE_NKEvent = AutoEvents.RecallDateUpdated.Code;
					log.SL_Reference = $"Opportunity {opportunity.COP_OpportunityID} Next Follow Up Date Updated|OLD=20240709T080000Z";
					log.SL_EventTime = ZDateTime.Now;
				}

				Factory.Save();
			}

			RunLogWalkerCycleForTest();
			AssertEquals("1 email created", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("From is current user", staff.GS_FullName, sentEmail.FromDisplayName);
			AssertEquals("From is current user", staff.GS_EmailAddress, sentEmail.FromAddress);
			AssertEquals(1, sentEmail.Recipients.Count);

			AssertEquals("Correct recipients", "zubin.appoo@cargowise.com", sentEmail.Recipients[0].Email);
			AssertEquals("Correct ContentType", EmailContentTypes.Calendar, sentEmail.ContentType);
			AssertStartsWith("Correct vCalendar Message", "BEGIN:VCALENDAR", sentEmail.Body);
			AssertStartsWith("Correct Subject", "Next follow up has been canceled for Opportunity", sentEmail.Subject);
			AssertEquals("Correct meeting date", new ZDateTime(2024, 7, 9, 8, 00, 00), GetVCalendarMeetingDateTime(sentEmail.Body));
		}

		public void TestProcessQueuedLogs_DeletedOpportunityForCrmOpportunity()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "a@bbb.com ";
			staff.GS_FullName = "President Alex";

			var staffRecipient = Factory.NewWithValidTestData<GlbStaff>();
			staffRecipient.GS_FullName = "Zubin Appoo";
			staffRecipient.GS_EmailAddress = "zubin.appoo@cargowise.com";

			Factory.Save();

			var opportunity = Factory.NewWithValidTestData<CrmOpportunity>();
			opportunity.COP_GS_NKSalesPerson = staffRecipient.GS_Code;
			opportunity.COP_NextFollowUp = new ZDateTimeOffset(2024, 7, 9, 13, 30, 00);

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var log = opportunity.Logs.AddNew();

				using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
				{
					log.SL_SE_NKEvent = AutoEvents.RecallDateUpdated.Code;
					log.SL_Reference = $"Opportunity {opportunity.COP_OpportunityID} Next Follow Up Date Updated|NEW=20240709T080000Z";
					log.SL_EventTime = ZDateTime.Now;
				}

				Factory.Save();
			}

			opportunity.Delete();
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			RunLogWalkerCycleForTest();

			AssertEquals("0 emails created", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		ZDateTime GetVCalendarMeetingDateTime(string vCalendar)
		{
			var meetingDateTime = ZDateTime.Empty;
			var dateTimeStart_StartIndex = vCalendar.IndexOf("DTSTART:");
			if (dateTimeStart_StartIndex != -1)
			{
				dateTimeStart_StartIndex += 8;
				var dateTimeStart_EndIndex = vCalendar.IndexOf("\r\n", dateTimeStart_StartIndex);

				if (dateTimeStart_EndIndex > dateTimeStart_StartIndex)
				{
					var dateAsString = vCalendar.Substring(dateTimeStart_StartIndex, dateTimeStart_EndIndex - dateTimeStart_StartIndex);
					_ = ZDateTime.TryParseExact(dateAsString, out meetingDateTime, "yyyyMMdd\\THHmmss\\Z");
				}
			}
			return meetingDateTime;
		}
	}
}
