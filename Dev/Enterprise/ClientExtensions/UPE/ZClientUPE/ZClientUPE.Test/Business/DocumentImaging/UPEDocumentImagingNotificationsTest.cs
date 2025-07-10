using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.UPE.DocumentImaging.Testing
{
	class UPEDocumentImagingNotificationsTest : TestCaseWithFactory
	{
		public void TestNotifyAlsoNotifiesInnerNotificationSubscriber()
		{
			NotificationBuffer innerNotifications = new NotificationBuffer();
			UPEDocumentImagingNotifications notifications = new UPEDocumentImagingNotifications(innerNotifications);
			notifications.Notify(new ErrorNotification(ErrorType.ErrorRemovingEmails));
			AssertEquals("Notifications should be passed into the inner INotifications", true, innerNotifications.ContainsNotificationType(ErrorType.ErrorRemovingEmails));
		}

		[TestDate(2006, 3, 21)]
		public void TestSendEmailIfRequired()
		{
			Notifications.Notify(new InfoNotification("Info1"));
			Notifications.Notify(new InfoNotification("Info2"));
			Notifications.Flush();
			fNotifications = null;
			Notifications.Notify(new InfoNotification("Info3"));
			Notifications.Notify(new InfoNotification("Info4"));
			Notifications.SendEmailIfRequired();
			AssertSentEmail("CargoWise One Document Imaging Notifications", @"CargoWise One Document Imaging Notifications; Generated 21-Mar-06 00:00:00

Info1
Info2
Info3
Info4

CargoWise One Document Imaging
");
			Notifications.Notify(new InfoNotification("Info5"));
			Notifications.Notify(new InfoNotification("Info6"));
			Notifications.Flush();
			fNotifications = null;
			Notifications.Notify(new InfoNotification("Info7"));
			Notifications.Notify(new InfoNotification("Info8"));
			Notifications.SendEmailIfRequired();
			AssertSentEmail("CargoWise One Document Imaging Notifications", @"CargoWise One Document Imaging Notifications; Generated 21-Mar-06 00:00:00

Info5
Info6
Info7
Info8

CargoWise One Document Imaging
");
		}

		public void TestSendEmailIfRequired_DontSendIfNoNotificationsRaised()
		{
			Notifications.SendEmailIfRequired();
			AssertEquals("No email should be sent if there are no notifications", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			Notifications.Notify(new InfoNotification("Info1"));
			Notifications.SendEmailIfRequired();
			AssertEquals("When notifications are raised an email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		#region Implementation
		void AssertSentEmail(ZString expectedSubject, ZString expectedBody)
		{
			EmailDef sentEmail = Env.OutgoingMailManager.EmailsCreated[Env.OutgoingMailManager.EmailsCreated.Count - 1];
			AssertEquals("Subject", expectedSubject, sentEmail.Subject);
			AssertEquals("Body", expectedBody, sentEmail.Body);
			AssertEquals("To", "bob@edi.com.au", sentEmail.Recipients[0]);
		}

		GlbGroup EmailNotificationGroup
		{
			get
			{
				if (fEmailNotificationGroup == null)
				{
					fEmailNotificationGroup = Factory.New<GlbGroup>();
					GlbStaff recipient = fEmailNotificationGroup.Staff.AddNew();
					recipient.GS_EmailAddress = "bob@edi.com.au";
					recipient.GS_Code = "ZAC";
				}

				return fEmailNotificationGroup;
			}
		}

		GlbGroup fEmailNotificationGroup;
		UPEDocumentImagingNotifications Notifications
		{
			get
			{
				if (fNotifications == null)
				{
					fNotifications = new UPEDocumentImagingNotifications();
				}

				return fNotifications;
			}
		}

		UPEDocumentImagingNotifications fNotifications;
		protected override void SetUp()
		{
			base.SetUp();
			EmailNotificationGroup.Factory.Save();
			UPEDataRegistry.Instance.DocumentImagingNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, EmailNotificationGroup.PK.ToGuid());
		}
		#endregion
	}
}
