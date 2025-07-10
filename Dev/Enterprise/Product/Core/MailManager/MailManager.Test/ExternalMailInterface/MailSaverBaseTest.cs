using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.ExternalMailInterface.CommonInterfaces;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Lists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	class MailSaverBaseTest : TestCaseWithFactory
	{
		public void TestPersistingEmail_ShouldNotSaveDuplicateEmail()
		{
			AssertEquals("Should be Pop3 test env.", MailRetrievalProtocols.POP3, Env.Registry.MailRetrievalProtocol);
			pop3Downloader.GenerateDummies = false;

			var uniqueId = "123abc_totally_guaranteed_unique";
			var email = "你好\r\n\r\nsome_raw_mime_string";
			var continueDownload = true;

			saver.PersistingEmailExposed(uniqueId, email, ref continueDownload);
			saver.FactoryForTest.Save();
			var newSaver = new TestMailSaver(pop3Downloader, logger);
			newSaver.PersistingEmailExposed(uniqueId, email, ref continueDownload);
			newSaver.FactoryForTest.Save();

			var mails = new BusinessObjectFactory().Load<MailItem>(new ZQuery(MailDBItemsSchema.MI_POP3UIDL, "123abc_totally_guaranteed_unique"));
			AssertEquals("Should be only 1 email created.", 1, mails.Length);
			AssertNullOrEmpty(ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestSubjectIsUnfoldedAndSet()
		{
			pop3Downloader.GenerateDummies = false;
			saver.Retrieve();
			Assert(pop3Downloader.DownloadWasCalled);
			var item = Factory.LoadTop1(typeof(MailItem), new ZQuery()) as MailItem;
			AssertEquals("Subject should be unfolded - ref. RFC 822 sec. 3.1.1, 3.1.3", "The quick brown fox jumps over the lazy dog", item.MI_Subject);
		}

		public void TestSubjectIsTruncated()
		{
			pop3Downloader.GenerateDummies = false;
			pop3Downloader.SampleMessage = "Subject: " + new string('x', MailDBItemsSchema.MI_Subject.MaxLength) + "junk junk\r\n\r\nbody";
			saver.Retrieve();
			Assert(pop3Downloader.DownloadWasCalled);
			var item = Factory.LoadTop1(typeof(MailItem), new ZQuery()) as MailItem;
			AssertEquals("Subject should be truncated silently. Any email with a machine-readable subject line should be well under the max length. For human-readable subject lines, it doesn't matter if it gets truncated", new string('x', MailDBItemsSchema.MI_Subject.MaxLength), item.MI_Subject);
		}

		[ExpectNoExceptions]
		public void TestEmailAddressIsInvalid()
		{
			pop3Downloader.GenerateDummies = false;
			pop3Downloader.SampleMessage = @"Subject: Test Email Address
From: test@example.com
To: TestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalid@example.com

body";
			pop3Downloader.MessageUniqueId = "test-message";

			var postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			postMasterGroup.Staff[0].GS_EmailAddress = "a@b.c";
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			try
			{
				saver.Retrieve();
				Assert(pop3Downloader.DownloadWasCalled);
				AssertEquals("This message should deleted from POP3 server", 1, pop3Downloader.DeleteCount);
				AssertEquals("One outgoing MailItem should be created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var expectedSubject = string.Format("Error while processing an incoming email addressed to {0}", Env.Registry.MailboxEmailAddress);
				AssertEquals("Created outgoing Email has an Error Notification Subject", expectedSubject, Env.OutgoingMailManager.EmailsCreated[0].Subject);
				var errorMessage = "The email address [<TestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalidTestEmailAddressInvalid@example.com>] exceeds the maximum allowable of 320.";
				Assert("Created outgoing Email has an Error Notification Body", Env.OutgoingMailManager.EmailsCreated[0].Body.Contains(errorMessage));
			}
			finally
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();
			}
		}

		public void TestAttachmentsAreParsed()
		{
			pop3Downloader.GenerateDummies = false;
			saver.Retrieve();
			Assert(pop3Downloader.DownloadWasCalled);
			AssertEquals(1, Factory.GetDatabaseCount(typeof(MailAttachment)));
		}

		public void TestDMIFilter()
		{
			pop3Downloader.GenerateDummies = false;
			pop3Downloader.SampleMessage = @"Subject: [DocManager SHP ACV SSYD54633273]
From: Eagle Vessel Schedule and Routing <1-stop@edi.net.au>
To: Eagle Vessel Schedule and Routing 2 <2-stop@edi.net.au> 

body";
			pop3Downloader.MessageUniqueId = "test-message";

			saver.Retrieve();
			Assert(pop3Downloader.DownloadWasCalled);
			AssertEquals("This message should deleted from POP3 server", 1, pop3Downloader.DeleteCount);
			var items = new BusinessObjectFactory().Load<MailItem>(new ZQuery());
			AssertEquals("One mail item", 1, items.Length);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "DMI" }, items.Select(x => x.MI_Application));
			AssertEquals("test-message", items[0].MI_POP3UIDL);
		}

		public void TestMultipleFiltersSatisfy()
		{
			pop3Downloader.GenerateDummies = false;
			pop3Downloader.SampleMessage = @"Subject: Enterprise Vessel Schedule
From: Eagle Vessel Schedule and Routing <1-stop@edi.net.au>
To: Eagle Vessel Schedule and Routing 2 <2-stop@edi.net.au> 

body";
			pop3Downloader.MessageUniqueId = "test-message";

			saver.Retrieve();
			Assert(pop3Downloader.DownloadWasCalled);
			AssertEquals("This message should deleted from POP3 server", 1, pop3Downloader.DeleteCount);
			var items = new BusinessObjectFactory().Load<MailItem>(new ZQuery());
			AssertEquals("Mail item cloned for each filter", 1, items.Length);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "MAP" }, items.Select(x => x.MI_Application));
			AssertEquals("test-message", items[0].MI_POP3UIDL);
		}

		public void TestDownloadRFC822Attachments()
		{
			pop3Downloader.GenerateDummies = false;
			pop3Downloader.SampleMessage = resourceRetriever.Value.GetString("Enterprise.MailManager.Test.ExternalMailInterface.TestFiles.messagerfc822.txt");
			saver.Retrieve();
			Assert(pop3Downloader.DownloadWasCalled);
			var items = new BusinessObjectFactory().Load<MailItem>(new ZQuery());
			AssertEquals("Mail items", 1, items.Length);
			AssertEquals("Attachments", 2, items[0].MailAttachments.Count);
			AssertContainsExactElementsInAnyOrder("Attachment file names",
					new ZString[] { "How do I check my super transactions.eml", "New Bitmap Image.jpg" },
					new[] { items[0].MailAttachments[0].MA_FileName, items[0].MailAttachments[1].MA_FileName });
		}

		[TestDate(2005, 11, 11, 11, 00, 00)]
		public void TestDownloadTwoSmallEmails()
		{
			pop3Downloader.MessageSize = 2000;
			pop3Downloader.MessageCount = 2;
			saver.Retrieve();
			Assert(pop3Downloader.DownloadWasCalled);
			// AssertEquals("Should use one factory for both messages", 1, saver.FactorySaveCount);
			AssertMailsAreInDatabase();
		}

		[TestDate(2005, 11, 11, 11, 00, 00)]
		public void TestDownloadTenBigEmails()
		{
			pop3Downloader.MessageSize = 50000;
			pop3Downloader.MessageCount = 10;
			saver.Retrieve();
			Assert(pop3Downloader.DownloadWasCalled);
			// AssertEquals("Should have used a second factory because total message size exceeded 400KB", 2, saver.FactorySaveCount);
			AssertMailsAreInDatabase();
			AssertEquals("All messages deleted from POP3 server", 10, pop3Downloader.DeleteCount);
		}

		public void TestGenerateDummyMessage()
		{
			pop3Downloader.MessageSize = 1000;
			var length = pop3Downloader.GenerateDummyMessage().Length;
			AssertEquals("Length", 1000, length);
		}

		public void TestNotifyWhenCouldNotSaveEmail()
		{
			pop3Downloader.GenerateDummies = false;
			pop3Downloader.UseSampleErrorMessage = 1;

			var postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			postMasterGroup.Staff[0].GS_EmailAddress = "a@b.c";
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			try
			{
				saver.Retrieve();
				Assert(pop3Downloader.DownloadWasCalled);
				AssertEquals("One outgoing MailItem should be created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				AssertEquals("Two MailItems should be saved", 2 * pop3Downloader.UseSampleErrorMessage, Factory.GetDatabaseCount(typeof(MailItem)));

				var expectedSubject = string.Format("Error while processing an incoming email addressed to {0}", Env.Registry.MailboxEmailAddress);
				AssertEquals("Created outgoing Email has an Error Notification Subject", expectedSubject, Env.OutgoingMailManager.EmailsCreated[0].Subject);
				Assert("Created outgoing Email has an Error Notification Body", Env.OutgoingMailManager.EmailsCreated[0].Body.Contains("Email batch processor reported an error while processing an incoming email."));
			}
			finally
			{
				Env.OutgoingMailManager.EmailsCreated.Clear();
			}
		}

		[ExpectNoExceptions]
		public void TestLongAttachmentFileName()
		{
			pop3Downloader.GenerateDummies = false;
			pop3Downloader.SampleMessage = resourceRetriever.Value.GetString("Enterprise.MailManager.Test.ExternalMailInterface.TestFiles.LongAttachmentFileName.eml");
			saver.Retrieve();
		}

		[ExpectNoExceptions]
		public void TestGreetingCardSpamEmail()
		{
			pop3Downloader.GenerateDummies = false;
			pop3Downloader.SampleMessage = resourceRetriever.Value.GetString("Enterprise.MailManager.Test.ExternalMailInterface.TestFiles.GreetingCardSpamEmail.eml");
			saver.Retrieve();
		}

		[ExpectNoExceptions]
		public void TestImageSpamEmail()
		{
			pop3Downloader.GenerateDummies = false;
			pop3Downloader.SampleMessage = resourceRetriever.Value.GetString("Enterprise.MailManager.Test.ExternalMailInterface.TestFiles.ImageSpamEmail.eml");
			saver.Retrieve();
		}

		[UseSnapshotProtection]
		public void TestSecondDownloadOfMessageWithSameUniqueidNotSaved()
		{
			pop3Downloader.GenerateDummies = false;
			pop3Downloader.SampleMessage = "Subject: TEST\r\n\r\nbody";
			pop3Downloader.MessageUniqueId = "duplicate-message";
			saver.Retrieve();
			pop3Downloader.SampleMessage = "Subject: TEST\r\n\r\nbody";
			pop3Downloader.MessageUniqueId = "another-message";
			saver.Retrieve();
			pop3Downloader.SampleMessage = "Subject: TEST\r\n\r\nbody";
			pop3Downloader.MessageUniqueId = "duplicate-message";
			saver.Retrieve();
			var item = Factory.Load<MailItem>(new ZQuery(MailDBItemsSchema.MI_Subject, "TEST"));
			AssertEquals(2, item.Length);
		}

		[UseSnapshotProtection]
		public void TestSecondDownloadOfMessageWithSameUniqueidNotSaved_BustedUniqueIDs()
		{
			pop3Downloader.GenerateDummies = false;
			pop3Downloader.SampleMessage = "Subject: TEST\r\n\r\nbody";
			pop3Downloader.MessageUniqueId = " duplicate-message ";
			saver.Retrieve();
			pop3Downloader.SampleMessage = "Subject: TEST\r\n\r\nbody";
			pop3Downloader.MessageUniqueId = " another-message ";
			saver.Retrieve();
			pop3Downloader.SampleMessage = "Subject: TEST\r\n\r\nbody";
			pop3Downloader.MessageUniqueId = "  ∞duplicate-message  ";
			saver.Retrieve();
			var item = Factory.Load<MailItem>(new ZQuery(MailDBItemsSchema.MI_Subject, "TEST"));
			AssertEquals(2, item.Length);
		}

		[UseSnapshotProtection]
		public void TestSecondDownloadOfMessageWithSameUniqueidIsSavedDifferentSubject()
		{
			pop3Downloader.GenerateDummies = false;
			pop3Downloader.SampleMessage = "Subject: TEST\r\n\r\nbody";
			pop3Downloader.MessageUniqueId = "duplicate-message";
			saver.Retrieve();
			pop3Downloader.SampleMessage = "Subject: TEST\r\n\r\nbody";
			pop3Downloader.MessageUniqueId = "another-message";
			saver.Retrieve();
			pop3Downloader.SampleMessage = "Subject: TEST2\r\n\r\nbody";
			pop3Downloader.MessageUniqueId = "duplicate-message";
			saver.Retrieve();
			var item = Factory.Load<MailItem>(new ZQuery(MailDBItemsSchema.MI_Subject, SQLComparisonOperator.StartsWith, "TEST"));
			AssertEquals(3, item.Length);
		}

		[UseSnapshotProtection]
		public void TestSecondDownloadOfMessageWithSameUniqueidDifferingOnlyByCase()
		{
			pop3Downloader.GenerateDummies = false;
			pop3Downloader.SampleMessage = "Subject: TEST\r\n\r\nbody";
			pop3Downloader.MessageUniqueId = "duplicate-message";
			saver.Retrieve();
			pop3Downloader.SampleMessage = "Subject: TEST\r\n\r\nbody";
			pop3Downloader.MessageUniqueId = "duplicate-Message";
			saver.Retrieve();
			var item = Factory.Load<MailItem>(new ZQuery(MailDBItemsSchema.MI_Subject, "TEST"));
			AssertEquals(2, item.Length);
		}

		public void TestReportWhen_MI_Application_NotCorrectlySet()
		{
			var newFact = NewFactory();
			newFact.RefreshEnabled = false;
			newFact.Save();

			var query = new ZQuery(MailDBItemsSchema.MI_Application, "STD");
			query.AddToFilter(MailDBItemsSchema.MI_Status, MailStatus.Queued);
			query.AddToFilter(MailDBItemsSchema.MI_Direction, MailDirection.Receive);
			newFact.Load<MailItem>(query);
			AssertEquals(1, ErrorReporter.TotalErrorCount);

			AssertContains("Mails shoud be categorised according to the code that will process it. That code should be saved in MI_Application field. Please ignore if it comes from FilterStrip.", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		[UseSnapshotProtection]
		public void TestPersistingEmail_CorrectUniqueIdAndEmail()
		{
			pop3Downloader.GenerateDummies = false;

			var uniqueId = "123abc_totally_guaranteed_unique";
			var email = "some_raw_mime_string";
			var continueDownload = true;

			saver.PersistingEmailExposed(uniqueId, email, ref continueDownload);
			saver.Retrieve();

			var mailItem = Factory.Load<MailItem>(new ZQuery()).FirstOrDefault();
			Assert(!string.IsNullOrEmpty(mailItem.MI_POP3UIDL));
		}

		[UseSnapshotProtection]
		public void TestPersistingEmail_CaseSensitiveUniqueId()
		{
			pop3Downloader.GenerateDummies = false;

			var uniqueId1 = "ABCD";
			var uniqueId2 = "abcd";
			var continueDownload = true;

			var email1 = "some_raw_mime_string";
			saver.PersistingEmailExposed(uniqueId1, email1, ref continueDownload);
			saver.Retrieve();

			var email2 = "some_raw_mime_string";
			saver.PersistingEmailExposed(uniqueId2, email2, ref continueDownload);
			saver.Retrieve();

			Assert(Factory.Load<MailItem>(new ZQuery()).Any());

			var mailItems = Factory.Load<MailItem>(new ZQuery(MailDBItemsSchema.MI_POP3UIDL, "ABCD"));
			AssertEquals(uniqueId1, mailItems[0].MI_POP3UIDL);
			AssertEquals(uniqueId2, mailItems[1].MI_POP3UIDL);
		}

		[UseSnapshotProtection]
		public void TestPersistingEmail_BlankUniqueId()
		{
			TestPersistingEmail_CommonUniqueId(string.Empty);
		}

		[UseSnapshotProtection]
		public void TestPersistingEmail_NullUniqueId()
		{
			TestPersistingEmail_CommonUniqueId(null);
		}

		void TestPersistingEmail_CommonUniqueId(string uniqueId)
		{
			pop3Downloader.GenerateDummies = false;

			var email = "some_raw_mime_string";
			var continueDownload = true;

			saver.PersistingEmailExposed(uniqueId, email, ref continueDownload);
			saver.Retrieve();

			var expectedUniqueId = TestMailSaver.GenerateUniqueIdForEmailExposed(email);
			var mailItem = Factory.Load<MailItem>(new ZQuery(MailDBItemsSchema.MI_POP3UIDL, expectedUniqueId)).FirstOrDefault();
			AssertEquals(expectedUniqueId, mailItem.MI_POP3UIDL);

			var logs = new List<(LogType, string, Exception)>
			{
				(LogType.Information, "Email has null or empty unique ID; generated a new one.\r\nUniqueId:hxI0rXN/TzmurmKLWOW9nJEQd0c=", null),
				(LogType.Debug, $"RawEmail:{email}", null),
				(LogType.Debug, "No mail filters found for email: \r\nUniqueId: From: Subject: AllRecipients:", null),
				(LogType.Debug, $"No mail filters found for email: \r\nUniqueId:{pop3Downloader.MessageUniqueId} From:\"David James\" <davidj@edi.com.au> Subject:The quick brown fox jumps over the lazy dog AllRecipients:<davidj@dj294.syd.edi>", null),
			};

			AssertLogsEqual(logs, logger);
		}

		[UseSnapshotProtection]
		public void TestPersistingEmail_BlankEmail()
		{
			TestPersistingEmail_CommonEmail(string.Empty);
		}

		[UseSnapshotProtection]
		public void TestPersistingEmail_NullEmail()
		{
			TestPersistingEmail_CommonEmail(null);
		}

		void TestPersistingEmail_CommonEmail(string email)
		{
			pop3Downloader.GenerateDummies = false;

			var uniqueId = "abcdef";
			var continueDownload = true;

			saver.PersistingEmailExposed(uniqueId, email, ref continueDownload);
			Factory.Save();

			Assert(!Factory.Load<MailItem>(new ZQuery()).Any());
			AssertLastLogEquals(new Tuple<LogType, string, Exception>(LogType.Information, "Email dropped: null or empty contents.\r\nUniqueId:abcdef", null), logger);
		}

		[ExpectNoExceptions]
		public void TestPersistingEmail_NullLogger()
		{
			saver = new TestMailSaver(pop3Downloader, null);
		}

		[UseSnapshotProtection]
		public void TestPersistingEmail_UniqueIdLongerThanDbColumnSize()
		{
			pop3Downloader.GenerateDummies = false;

			var uniqueId = new string('A', MailDBItemsSchema.MI_POP3UIDL.MaxLength + 5);
			var email = "this is test data";
			var continueDownload = true;

			saver.PersistingEmailExposed(uniqueId, email, ref continueDownload);
			saver.Retrieve();

			var expectedUniqueId = TestMailSaver.GenerateUniqueIdForEmailExposed(email);
			var mailItem = Factory.Load<MailItem>(new ZQuery(MailDBItemsSchema.MI_POP3UIDL, expectedUniqueId)).FirstOrDefault();
			AssertEquals(expectedUniqueId, mailItem.MI_POP3UIDL);

			var logs = new List<(LogType, string, Exception)>
			{
				(LogType.Information, $"Unique ID from mail server is too long; generated a new one.\r\nUniqueId:{expectedUniqueId}", null),
				(LogType.Debug, $"RawEmail:{email}", null),
				(LogType.Debug, "No mail filters found for email: \r\nUniqueId:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA From: Subject: AllRecipients:", null),
				(LogType.Debug, $"No mail filters found for email: \r\nUniqueId:{pop3Downloader.MessageUniqueId} From:\"David James\" <davidj@edi.com.au> Subject:The quick brown fox jumps over the lazy dog AllRecipients:<davidj@dj294.syd.edi>", null),
			};

			AssertLogsEqual(logs, logger);

			AssertEquals("Unique Id is wrong for some reason; did you change the hash algorithm?", expectedUniqueId, "88NRrKhFNSUDzGk+UMy8q4NgkXU=");
		}

		[UseSnapshotProtection]
		public void TestPersistingEmail_AddingDuplicateIsHandledCorrectly_TrySavingTwiceInARow()
		{
			pop3Downloader.GenerateDummies = false;

			var uniqueId = "123abc_totally_guaranteed_unique";
			var email = "some_raw_mime_string";
			var continueDownload = true;

			saver.PersistingEmailExposed(uniqueId, email, ref continueDownload);
			saver.Retrieve();

			if (logger != null)
			{
				var logs1 = new List<(LogType, string, Exception)>
				{
					(LogType.Debug, $"No mail filters found for email: \r\nUniqueId:{uniqueId} From: Subject: AllRecipients:", null),
					(LogType.Debug, $"No mail filters found for email: \r\nUniqueId:{pop3Downloader.MessageUniqueId} From:\"David James\" <davidj@edi.com.au> Subject:The quick brown fox jumps over the lazy dog AllRecipients:<davidj@dj294.syd.edi>", null),
				};

				AssertLogsEqual(logs1, logger);
				logger.Clear();
			}

			saver.PersistingEmailExposed(uniqueId, email, ref continueDownload);
			saver.Retrieve();

			var logs2 = new List<(LogType, string, Exception)>
			{
				(LogType.Information, $"Email dropped: already exists in database.\r\nUniqueId:{uniqueId}", null),
				(LogType.Debug, $"RawEmail:{email}", null),
				(LogType.Information, $"Email dropped: already exists in database.\r\nUniqueId:{pop3Downloader.MessageUniqueId}", null),
				(LogType.Debug, $"RawEmail:{pop3Downloader.SampleMessage}", null),
			};

			AssertLogsEqual(logs2, logger);

			var mailItem = Factory.Load<MailItem>(new ZQuery()).FirstOrDefault();
			Assert(!string.IsNullOrEmpty(mailItem.MI_POP3UIDL));
		}

		[UseSnapshotProtection]
		public void TestPersistingEmail_AddingDuplicateIsHandledCorrectly_TryToPutEmailIntoTransactionTwice()
		{
			pop3Downloader.GenerateDummies = false;

			var uniqueId = "123abc_totally_guaranteed_unique";
			var email = "some_raw_mime_string";
			var continueDownload = true;

			saver.PersistingEmailExposed(uniqueId, email, ref continueDownload);
			saver.PersistingEmailExposed(uniqueId, email, ref continueDownload);
			saver.Retrieve();

			var logs = new List<(LogType, string, Exception)>
			{
				(LogType.Debug, $"No mail filters found for email: \r\nUniqueId:{uniqueId} From: Subject: AllRecipients:", null),
				(LogType.Information, "Email dropped: already exists in database.\r\nUniqueId:123abc_totally_guaranteed_unique", null),
				(LogType.Debug, $"RawEmail:{email}", null),
				(LogType.Debug, $"No mail filters found for email: \r\nUniqueId:{pop3Downloader.MessageUniqueId} From:\"David James\" <davidj@edi.com.au> Subject:The quick brown fox jumps over the lazy dog AllRecipients:<davidj@dj294.syd.edi>", null),
			};

			AssertLogsEqual(logs, logger);

			var mailItem = Factory.Load<MailItem>(new ZQuery()).FirstOrDefault();
			Assert(!string.IsNullOrEmpty(mailItem.MI_POP3UIDL));
		}

		[UseSnapshotProtection]
		public void TestPersistingEmail_NoMailFilterAssigned()
		{
			pop3Downloader.GenerateDummies = false;

			var uniqueId = "123abc_totally_guaranteed_unique";
			var email = "some_raw_mime_string";
			var continueDownload = true;

			saver.PersistingEmailExposed(uniqueId, email, ref continueDownload);
			saver.PersistingEmailExposed(uniqueId, email, ref continueDownload);
			saver.Retrieve();

			var logs = new List<(LogType, string, Exception)>
			{
				(LogType.Debug, $"No mail filters found for email: \r\nUniqueId:{uniqueId} From: Subject: AllRecipients:", null),
				(LogType.Information, "Email dropped: already exists in database.\r\nUniqueId:123abc_totally_guaranteed_unique", null),
				(LogType.Debug, "RawEmail:some_raw_mime_string", null),
				(LogType.Debug, $"No mail filters found for email: \r\nUniqueId:{pop3Downloader.MessageUniqueId} From:\"David James\" <davidj@edi.com.au> Subject:The quick brown fox jumps over the lazy dog AllRecipients:<davidj@dj294.syd.edi>", null),
			};

			AssertLogsEqual(logs, logger);

			var mailItem = Factory.Load<MailItem>(new ZQuery()).FirstOrDefault();
			Assert(!string.IsNullOrEmpty(mailItem.MI_POP3UIDL));
		}

		[DeveloperOnlyTest]
		public void TestDownloaderMemoryThresholdIsSufficient()
		{
			pop3Downloader.MessageCount = 1;
			for (long i = 1; i <= 100; i += 5)
			{
				GC.Collect();
				var messageSize = (i * MailDownloader.Megabyte);
				pop3Downloader.MessageSize = (int)messageSize;

				try
				{
					saver.Retrieve();
				}
				catch (OutOfMemoryException)
				{
					AssertExceptionThrown(typeof(InsufficientMemoryException), delegate
					{
						using (MailDownloader.GetMemoryFailPoint(messageSize))
						{
							saver.Retrieve();
						}
					});
				}
			}

			MailDownloader.OverridableMemoryFailPointFactor.Value -= 1;

			AssertExceptionThrown("Pop3Downloader.MemoryFailPointFactor can be set to a lower value ", typeof(OutOfMemoryException), delegate
			{
				for (long i = 1; i <= 100; i += 5)
				{
					GC.Collect();
					var messageSize = (i * MailDownloader.Megabyte);
					pop3Downloader.MessageSize = (int)messageSize;

					try
					{
						saver.Retrieve();
					}
					catch (OutOfMemoryException)
					{
						try
						{
							using (MailDownloader.GetMemoryFailPoint(messageSize))
							{
								saver.Retrieve();
							}
						}
						catch (InsufficientMemoryException)
						{ }
					}
				}
			});
		}

		[ExpectNoExceptions]
		public void TestSaveDeadlock()
		{
			pop3Downloader.MessageSize = 800000;
			pop3Downloader.MessageCount = 2;
			saver.IsDeadlockExceptionThrown = true;
			saver.Retrieve();
			Assert(pop3Downloader.DownloadWasCalled);
			saver.IsDeadlockExceptionThrown = false;
		}

		public void TestGenerateUniqueIdForEmail_Blank()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => TestMailSaver.GenerateUniqueIdForEmailExposed(string.Empty));
		}

		public void TestGenerateUniqueIdForEmail_Null()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => TestMailSaver.GenerateUniqueIdForEmailExposed(null));
		}

		[ExpectNoExceptions]
		public void TestGenerateUniqueIdForEmail_Correct()
		{
			var corpus = "lorem ipsum";
			var result = TestMailSaver.GenerateUniqueIdForEmailExposed(corpus);
			AssertEquals("v7d1mmfa62VBBJC02Yu52n0eos4=", result);
		}

		[ExpectNoExceptions]
		public void TestGenerateUniqueIdForEmail_LongMessage()
		{
			using var downloader = new TestPop3Downloader();
			var result = TestMailSaver.GenerateUniqueIdForEmailExposed(downloader.SampleMessage);
			AssertEquals("QPiY86je5ora0lVJVUXo9peKHew=", result);
		}

		[ExpectNoExceptions]
		public void TestGenerateUniqueIdForEmail_DifferentAlgorithm()
		{
			var corpus = "lorem ipsum";
			var result = TestMailSaver.GenerateUniqueIdForEmailExposed(corpus, MD5.Create());
			AssertEquals("gKdR/eV3AoZAxBkADjPrpg==", result);
		}

		[ExpectNoExceptions]
		public void TestGenerateUniqueIdForEmail_TestDefaultHashDoesntExceedPOP3UIDLColumnLimit()
		{
			using var downloader = new TestPop3Downloader();
			var result = TestMailSaver.GenerateUniqueIdForEmailExposed(downloader.SampleMessage);
			var columnLength = MailDBItemsSchema.MI_POP3UIDL.MaxLength;

			AssertLessThan("If this assertion fails it means you have changed the default hashing algorithm and the new hash size does not fit into the size"
				+ " requirements for the MailItemsDB.MI_POP3UIDL column. You should update the column size if you need a bigger hash, and then update this"
				+ " test accordingly.",
				result.Length, columnLength);
		}

		[ExpectNoExceptions]
		public void TestPersistingEmail_DefaultHashAlgorithmHasChangedAndCodeIsUpdated()
		{
			using var downloader = new TestPop3Downloader();
			var corpus = downloader.SampleMessage;
			var expectedResult = TestMailSaver.GenerateUniqueIdForEmailExposed(corpus, SHA1.Create());
			var defaultResult = TestMailSaver.GenerateUniqueIdForEmailExposed(corpus, TestMailSaver.GetDefaultHashAlgorithm());

			AssertEquals(
				"If this assertion fails it means you have changed the default hashing algorithm and the hashes in the MailItemsDB.MI_POP3UIDL column are now"
				+ " invalid. You need to write a transformation to rehash them, and then update this test accordingly.",
				expectedResult, defaultResult);
		}

		void AssertMailsAreInDatabase()
		{
			var items = (MailItem[])new BusinessObjectFactory().Load(typeof(MailItem), new ZQuery());
			AssertEquals(pop3Downloader.MessageCount, (long)items.Length);
			foreach (var item in items)
			{
				AssertEquals("Status", "QUE", item.MI_Status);
				AssertEquals("ReceivedDateTime", TestDateAttribute.Date, item.MI_ReceivedDateTime);
				Assert("Body", item.MI_Body.StartsWith("xxx"));
			}
		}

		static void AssertLastLogEquals<T>(T expected, ITestLogger actual)
		{
			if (actual != null)
			{
				AssertEquals(expected, actual.GetLastMessage());
			}
		}

		static void AssertLogsEqual<T>(IList<T> expected, ITestLogger actual)
		{
			if (actual != null)
			{
				CombineAssertions(() =>
				{
					AssertEquals(expected.Count, actual.GetCount());
					var count = 0;
					foreach (var log in expected)
					{
						AssertEquals($"Log [{count}] failed\r\n", log, actual.GetMessage(count++).ToValueTuple());
					}
				});
			}
		}

		protected override void SetUp()
		{
			TestCaseHelper.ClearTable(MailDBItemsSchema.Constants.TableName);
			pop3Downloader = new TestPop3Downloader();
			logger = new TestLogger();
			saver = new TestMailSaver(pop3Downloader, logger);
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			pop3Downloader.Dispose();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		protected TestPop3Downloader pop3Downloader;
		protected TestMailSaver saver;
		protected TestLogger logger;
		protected Lazy<EmbeddedResourceRetriever> resourceRetriever;

		protected interface ITestLogger : ILogger
		{
			Tuple<LogType, string, Exception> GetLastMessage();
			Tuple<LogType, string, Exception> GetSecondToLastMessage();
			Tuple<LogType, string, Exception> GetMessage(int index);
			void Clear();
			int GetCount();
		}

		protected class TestLogger : ITestLogger
		{
			public void Clear()
			{
				logs.Clear();
			}

			public void Log(LogType type, string message)
			{
				logs.Add(new Tuple<LogType, string, Exception>(type, message, null));
			}

			public void Log(LogType type, string message, Exception ex)
			{
				logs.Add(new Tuple<LogType, string, Exception>(type, message, ex));
			}

			public Tuple<LogType, string, Exception> GetLastMessage()
			{
				return logs.LastOrDefault();
			}

			public Tuple<LogType, string, Exception> GetSecondToLastMessage()
			{
				return logs[logs.Count - 2];
			}

			public Tuple<LogType, string, Exception> GetMessage(int index)
			{
				return logs[index];
			}

			public bool HasLogs()
			{
				return logs.Any();
			}

			public int GetCount() => logs.Count;

			readonly List<Tuple<LogType, string, Exception>> logs = new List<Tuple<LogType, string, Exception>>();
		}

		protected class TestMailSaver : MailSaver
		{
			public TestMailSaver(IMailDownloader downloader, ILogger logger) : base(downloader, logger) { }

			public void PersistingEmailExposed(string uniqueId, string email, ref bool continueDownloading)
			{
				saver_PersistingEmail(uniqueId, email, ref continueDownloading);
			}

			public BusinessObjectFactory FactoryForTest => base.Factory;

			public static string GenerateUniqueIdForEmailExposed(string toEncode, HashAlgorithm hashAlgorithm)
			{
				return GenerateUniqueIdForEmail(toEncode, hashAlgorithm);
			}

			public static string GenerateUniqueIdForEmailExposed(string toEncode)
			{
				return GenerateUniqueIdForEmail(toEncode);
			}

			public static HashAlgorithm GetDefaultHashAlgorithm()
			{
				return DefaultHashAlgorithm.Value;
			}
		}

		protected class TestPop3Downloader : IMailDownloader
		{
			public event EmailDownloadedHandler EmailDownloaded;
			public event DownloaderClosingHandler DownloaderClosing;
			public event LogMessageHandler LogMessage { add { } remove { } }

			public long? MessageCount { get; set; }

			public void DownloadFromServer()
			{
				DownloadWasCalled = true;
				var dummy = true;
				const bool dummyDelete = true;

				if (GenerateDummies)
				{
					long i;
					for (i = 0; i < MessageCount; i++)
					{
						var message = GenerateDummyMessage();
						EmailDownloaded(Guid.NewGuid().ToString(), ref message, ref dummy);
					}
					DownloaderClosing(i);
				}
				else
				{
					if (UseSampleErrorMessage > 0)
					{
						try
						{
							EmailDownloaded(Guid.NewGuid().ToString(), ref sampleErrorMessage[UseSampleErrorMessage - 1], ref dummy);
							Assertion.Fail("An exception should be thrown");
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							Assertion.AssertEquals("Should delete Error message", true, dummyDelete);
						}
					}
					EmailDownloaded(MessageUniqueId, ref sampleMessage, ref dummy);
					DownloaderClosing(1);
				}
			}

			void IMailDownloader.DeleteMessage(string messageId)
			{
				++DeleteCount;
			}

			public string GenerateDummyMessage()
			{
				var result = new StringBuilder();
				result.Append(@"Subject: test email
From: test@example
To: test@example

");
				const int lineLengthIncludingNewLine = 78;
				var bodyLine = new string('x', lineLengthIncludingNewLine - System.Environment.NewLine.Length) + System.Environment.NewLine;
				var headerSize = result.Length;

				for (var i = 0; i <= (MessageSize - headerSize) / lineLengthIncludingNewLine; i++)
				{
					result.Append(bodyLine);
				}

				result.Length = MessageSize - System.Environment.NewLine.Length;
				result.Append(System.Environment.NewLine);
				return result.ToString();
			}

			public bool DownloadWasCalled;
			public bool GenerateDummies { get; set; } = true;
			public int MessageSize { get; set; }
			public int DeleteCount { get; set; }
			public int UseSampleErrorMessage { get; set; }

			public string SampleMessage
			{
				get => sampleMessage;
				set => sampleMessage = value;
			}

			public string MessageUniqueId
			{
				get => messageUniqueId ?? (messageUniqueId = Guid.NewGuid().ToString());
				set => messageUniqueId = value;
			}
			string messageUniqueId;

			#region Sample messages causing exception in Lesnikovsy.Mail

			//			string[] sampleErrorMessage = { 
			//@"Return-path: <>
			//Received: from wic001mr.server-mail.com ([203.147.134.177])
			//	by acsedi.edi.net.au (acsedi.edi.net.au [127.0.0.1])
			//	(MDaemon PRO v9.5.6)
			//	with ESMTP id 51-md50001402830.msg
			//	for <enterpriseproduction@acsedi.edi.net.au>; Thu, 22 Nov 2007 11:28:24 +1100
			//Received: from bne001mz.server-mail.com (bne001mz-int.server-mail.com [172.16.32.65])
			//	by wic001mr.server-mail.com (Postfix) with ESMTP id A140A28CF9
			//	for <enterpriseproduction@acsedi.edi.net.au>; Thu, 22 Nov 2007 10:28:18 +1000 (EST)
			//Date: Thu, 22 Nov 2007 10:28:19 +1000
			//From: Mail Delivery System <Mail Delivery System>
			//To: enterpriseproduction@acsedi.edi.net.au
			//Subject: Delivery status notification
			//MIME-Version: 1.0
			//Content-Type: multipart/report;
			//Message-Id: <20071122002818.A140A28CF9@wic001mr.server-mail.com>
			//X-Spam-Processed: acsedi.edi.net.au, Thu, 22 Nov 2007 11:28:24 +1100
			//	(not processed: spam filter heuristic analysis disabled)
			//X-MDRcpt-To: enterpriseproduction@acsedi.edi.net.au
			//X-Rcpt-To: enterpriseproduction@acsedi.edi.net.au
			//X-MDRemoteIP: 203.147.134.177
			//X-Return-Path: 
			//X-Envelope-From: 
			//X-MDaemon-Deliver-To: enterpriseproduction@acsedi.edi.net.au
			//
			//This is a multi-part message in MIME format.
			//" };

			readonly string[] sampleErrorMessage = {
@"
" };  //	The message only fails to save when there are no messages constructed. The new mail.dll passes blank Mime types and headers.

			#endregion

			#region Dummy message with attachment
			string sampleMessage = @"Return-path: <davidj@edi.com.au>
Received: from xch.syd.edi ([192.168.210.4])
	by dj294.syd.edi (dj294.syd.edi)
	(MDaemon.PRO.v8.1.3.T)
	with ESMTP id md50000009479.msg
	for <davidj@dj294.syd.edi>; Tue, 08 Nov 2005 14:51:04 +1100
X-MimeOLE: Produced By Microsoft Exchange V6.5.7226.0
Content-class: urn:content-classes:message
MIME-Version: 1.0
Content-Type: multipart/mixed;
	boundary=""----_=_NextPart_001_01C5E417.A38C0FAF""
Subject: The quick
 brown fox
 jumps over
 the lazy dog
Date: Tue, 8 Nov 2005 14:51:02 +1100
Message-ID: <75C0C1D33C6C24448A290AE2A5116B412B2913@xch.syd.edi>
X-MS-Has-Attach: yes
X-MS-TNEF-Correlator: 
Thread-Topic: attachment
Thread-Index: AcXkF6PAr3A4bw8zRBuRuY3mGyMSlg==
From: ""David James"" <davidj@edi.com.au>
To: <davidj@dj294.syd.edi>
X-Spam-Processed: dj294.syd.edi, Tue, 08 Nov 2005 14:51:04 +1100
	(not processed: recipient davidj@dj294.syd.edi in exclude file)
X-MDRcpt-To: davidj@dj294.syd.edi
X-Rcpt-To: davidj@dj294.syd.edi
X-MDRemoteIP: 192.168.210.4
X-Return-Path: davidj@edi.com.au
X-MDaemon-Deliver-To: davidj@dj294.syd.edi

This is a multi-part message in MIME format.

------_=_NextPart_001_01C5E417.A38C0FAF
Content-Type: text/plain;
	charset=""us-ascii""
Content-Transfer-Encoding: quoted-printable

Attached.

------_=_NextPart_001_01C5E417.A38C0FAF
Content-Type: text/plain;
	name=""x.txt""
Content-Transfer-Encoding: base64
Content-Description: x.txt
Content-Disposition: attachment;
	filename=""x.txt""

eA0K

------_=_NextPart_001_01C5E417.A38C0FAF--
";
			#endregion

			public void Dispose()
			{
			}
		}
	}
}
