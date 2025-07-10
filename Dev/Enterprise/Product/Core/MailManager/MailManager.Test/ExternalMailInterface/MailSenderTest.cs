using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using MailManager;
using Moq;
using NUnit.Framework;
using Rnwood.SmtpServer.Verbs;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	sealed class MailSenderTest : TestCaseWithFactory
	{
		public void TestFailedToAuthenticateExceptionHandled()
		{
			var errors = new List<string>();
			var logger = new Mock<ILogger>();
			logger.Setup(l => l.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback(new Action<LogType, string>((x, y) => errors.Add($"{x} : {y}")));
			RegisterEmailSubjectToThrowException(ArrayOfThreeItems[0].MI_Subject, typeof(FailedToAuthenticateException));

			AssertExceptionThrown<FailedToAuthenticateException>(() => new MailSender().SendMail(ArrayOfThreeItems[0], logger.Object));
			AssertEquals("Error : Error sending email with subject 'SubjectA' : Email failure", errors[0]);
		}

		public void TestLoggerIsPassedToMailKitMailSender()
		{
			Globals.IsTest_ForTest.Value = false;
			using (new DisposableAction(() => Globals.IsTest_ForTest.ResetValue()))
			{
				var logger = new LoggerForTest();
				new MailSender().SendMail(ArrayOfThreeItems[0], logger);

				var message = logger.LogEntries.First();
				AssertEquals("MailKitMailSender constructed.", message);
			}
		}

		public void TestCategoryIsUpToDate()
		{
			var registryItemInCategory = RawDataRegistry.Instance.AllowEmailsToBeSentFromUsersAddress;
			var displayedCategory = new MailSender().ConfigurationRegistryPath.Replace(" -> ", "/");

			AssertStartsWith("We should be pointing to the right category - if items are moved please update this field", displayedCategory, registryItemInCategory.Category);
		}

		public void TestSendMailSuccessful()
		{
			new MailSender().SendMail(ArrayOfThreeItems);
			AssertEquals("Status", MailStatus.Sent, ArrayOfThreeItems[0].MI_Status);
			AssertEquals("Status", MailStatus.Sent, ArrayOfThreeItems[1].MI_Status);
			AssertEquals("Status", MailStatus.Sent, ArrayOfThreeItems[2].MI_Status);
		}

		public void TestSendMailSuccessful_ConcurrencyException()
		{
			ArrayOfThreeItems[0].Factory.Save();
			var secondFactory = new BusinessObjectFactory();
			Factory.RefreshEnabled = false;
			secondFactory.RefreshEnabled = false;
			var mailInFactory2 = secondFactory.Load<MailItem>(ArrayOfThreeItems[1].PK);
			mailInFactory2.MI_Status = MailStatus.Failed;
			var mail2InFactory2 = secondFactory.Load<MailItem>(ArrayOfThreeItems[2].PK);
			mail2InFactory2.MI_Status = MailStatus.Sent;
			secondFactory.Save();

			new MailSender().SendMail(ArrayOfThreeItems);
			AssertEquals("Status", MailStatus.Sent, ArrayOfThreeItems[0].MI_Status);
			AssertEquals("Status", MailStatus.Failed, ArrayOfThreeItems[1].MI_Status);
			AssertEquals("Status", MailStatus.Sent, ArrayOfThreeItems[2].MI_Status);
		}

		public void TestSendMailThrowsFailedToSendException()
		{
			RegisterEmailSubjectToThrowException(ArrayOfThreeItems[1].MI_Subject, typeof(FailedToSendMessageException));
			new MailSender().SendMail(ArrayOfThreeItems);
			AssertEquals("Status", MailStatus.Sent, ArrayOfThreeItems[0].MI_Status);
			AssertEquals("Status", MailStatus.Failed, ArrayOfThreeItems[1].MI_Status);
			AssertEquals("Status", MailStatus.Sent, ArrayOfThreeItems[2].MI_Status);
		}

		[ExpectException(typeof(FailedToConnectException))]
		public void TestSendMailThrowsFailedToAuthenticateException()
		{
			RegisterEmailSubjectToThrowException(ArrayOfThreeItems[0].MI_Subject, typeof(FailedToConnectException));
			try
			{
				new MailSender().SendMail(ArrayOfThreeItems);
			}
			finally
			{
				AssertEquals("Status", MailStatus.Failed, ArrayOfThreeItems[0].MI_Status);
				AssertEquals("Status", MailStatus.Queued, ArrayOfThreeItems[1].MI_Status);
				AssertEquals("Status", MailStatus.Queued, ArrayOfThreeItems[2].MI_Status);
			}
		}

		public void TestSendMailThrowsFailedToLogInException()
		{
			RegisterEmailSubjectToThrowException(ArrayOfThreeItems[2].MI_Subject, typeof(FailedToAuthenticateException));
			try
			{
				var logger = new Mock<ILogger>();
				logger.Setup(l => l.Log(It.IsAny<LogType>(), It.IsAny<string>()));
				AssertExceptionThrown<FailedToAuthenticateException>(() => new MailSender().SendMail(ArrayOfThreeItems, logger.Object));
			}
			finally
			{
				AssertEquals("Status", MailStatus.Sent, ArrayOfThreeItems[0].MI_Status);
				AssertEquals("Status", MailStatus.Sent, ArrayOfThreeItems[1].MI_Status);
				AssertEquals("Status", MailStatus.Queued, ArrayOfThreeItems[2].MI_Status);
			}
		}

		[ExpectException(typeof(FailedToConnectException))]
		public void TestSendMailLogFailedToConnectException()
		{
			RegisterEmailSubjectToThrowException(ArrayOfThreeItems[2].MI_Subject, typeof(FailedToConnectException));
			var errors = new List<string>();
			try
			{
				var logger = new Mock<ILogger>();
				logger.Setup(l => l.Log(It.IsAny<LogType>(), It.IsAny<string>())).Callback(new Action<LogType, string>((x, y) => errors.Add($"{x} : {y}")));
				new MailSender().SendMail(ArrayOfThreeItems, logger.Object);
			}
			finally
			{
				AssertContains("Error : Enterprise.MailManager.ExternalMailInterface.FailedToConnectException: Email failure", errors[0]);
				AssertEquals("Status", MailStatus.Sent, ArrayOfThreeItems[0].MI_Status);
				AssertEquals("Status", MailStatus.Sent, ArrayOfThreeItems[1].MI_Status);
				AssertEquals("Status", MailStatus.Failed, ArrayOfThreeItems[2].MI_Status);
			}
		}

		public void TestEmailsSent()
		{
			var sender = new MailSender();
			sender.SendMail(ArrayOfThreeItems);

			var results = sender.GetEmailSentResults();
			AssertEquals("Emails Sent", 3, results.Count);
			AssertEquals("Servers found", 1, results.GroupBy(k => k.SendingServer).Count());
		}

		public void TestEmailsFailedToBeSent()
		{
			RegisterEmailSubjectToThrowException(ArrayOfThreeItems[2].MI_Subject, typeof(FailedToSendMessageException));
			var sender = new MailSender();
			sender.SendMail(ArrayOfThreeItems);

			var results = sender.GetEmailSentResults();
			AssertEquals("Emails Failed To Be Sent", 1, results.Count(r => !r.IsSuccess));
		}

		public void TestEmailsFailedToBeSent_RateLimit()
		{
			subjectExceptionMessage = " 4.4.2 Message submission rate for this client has exceeded the configured limit ";
			RegisterEmailSubjectToThrowException(ArrayOfThreeItems[1].MI_Subject, typeof(FailedToSendMessageException));
			var sender = new MailSender();
			sender.SendMail(ArrayOfThreeItems);
			AssertEquals("Status", MailStatus.Sent, ArrayOfThreeItems[0].MI_Status);
			AssertEquals("Status", MailStatus.Queued, ArrayOfThreeItems[1].MI_Status);
			AssertEquals("Status", MailStatus.Sent, ArrayOfThreeItems[2].MI_Status);
			AssertEquals("Emails Failed To Be Sent", 0, sender.GetEmailSentResults().Count(e => !e.IsSuccess));
		}

		public void TestEmailsNotMarkAsFailed_ConcurrentConnections()
		{
			subjectExceptionMessage = " 4.3.2 Concurrent connections limit exceeded ";
			RegisterEmailSubjectToThrowException(ArrayOfThreeItems[1].MI_Subject, typeof(FailedToSendMessageException));
			var sender = new MailSender();
			sender.SendMail(ArrayOfThreeItems);
			AssertEquals("Status", MailStatus.Sent, ArrayOfThreeItems[0].MI_Status);
			AssertEquals("Status", MailStatus.Queued, ArrayOfThreeItems[1].MI_Status);
			AssertEquals("Status", MailStatus.Sent, ArrayOfThreeItems[2].MI_Status);
			AssertEquals("Emails Failed To Be Sent", 0, sender.GetEmailSentResults().Count(e => !e.IsSuccess));
		}

		public void TestEmailWithoutRecipientsFailed()
		{
			ArrayOfThreeItems[0].MailRecipients[0].Delete();
			AssertEquals("No Recipients", 0, ArrayOfThreeItems[0].MailRecipients.Count);

			new MailSender().SendMail(ArrayOfThreeItems);

			AssertEquals("QUE Email without recipients failed", MailStatus.Failed, ArrayOfThreeItems[0].MI_Status);
			AssertEquals("Sent", MailStatus.Sent, ArrayOfThreeItems[1].MI_Status);
			AssertEquals("Sent", MailStatus.Sent, ArrayOfThreeItems[2].MI_Status);
		}

		public void TestAllEmailWithEmptyRecipientsFailed_NoException()
		{
			//since we're using a stub, simulate the exception that would be thrown. System.ArgumentException: THere are no recipients. At least one recipient must be specified.
			RegisterEmailSubjectToThrowException(ArrayOfThreeItemsWithEmptyRecipients[0].MI_Subject, typeof(ArgumentException));
			RegisterEmailSubjectToThrowException(ArrayOfThreeItemsWithEmptyRecipients[1].MI_Subject, typeof(ArgumentException));
			RegisterEmailSubjectToThrowException(ArrayOfThreeItemsWithEmptyRecipients[2].MI_Subject, typeof(ArgumentException));
			new MailSender().SendMail(ArrayOfThreeItemsWithEmptyRecipients);

			AssertEquals("QUE Email without recipients failed", MailStatus.Failed, ArrayOfThreeItemsWithEmptyRecipients[0].MI_Status);
			AssertEquals("QUE Email without recipients failed", MailStatus.Failed, ArrayOfThreeItemsWithEmptyRecipients[1].MI_Status);
			AssertEquals("QUE Email without recipients failed", MailStatus.Failed, ArrayOfThreeItemsWithEmptyRecipients[2].MI_Status);
			ErrorReporter.Clear();
		}

		public void TestHandledExceptions()
		{
			RegisterEmailSubjectToThrowException(ArrayOfThreeItems[0].MI_Subject, typeof(FormatException));
			RegisterEmailSubjectToThrowException(ArrayOfThreeItems[1].MI_Subject, typeof(ArgumentException));
			ArrayOfThreeItems[1].AddRecipientForUserCommunication("𝓪@b.com");

			new MailSender().SendMail(ArrayOfThreeItems);
			AssertEquals("Status", MailStatus.Failed, ArrayOfThreeItems[0].MI_Status);
			AssertEquals("Status", MailStatus.Failed, ArrayOfThreeItems[1].MI_Status);
			AssertEquals("Status", MailStatus.Sent, ArrayOfThreeItems[2].MI_Status);
		}

		public void TestEmailWithInvalidRecipientsFailed()
		{
			var staffWithValidEmail = Factory.NewWithValidTestData<GlbStaff>();
			staffWithValidEmail.GS_EmailAddress = "alexander.korotun@cargowise.com";
			Factory.Save();

			ArrayOfThreeItems[0].MailRecipients[0].Delete();
			ArrayOfThreeItems[0].AddRecipientForUserCommunication("invalid_email_address", MailRecipient.RecipientTypes.TO);
			AssertEquals("One invalid recipient", 1, ArrayOfThreeItems[0].MailRecipients.Count);

			new MailSender().SendMail(ArrayOfThreeItems);

			AssertEquals("QUE Email with invalid email address failed", MailStatus.Failed, ArrayOfThreeItems[0].MI_Status);
			AssertEquals("Sent", MailStatus.Sent, ArrayOfThreeItems[1].MI_Status);
			AssertEquals("Sent", MailStatus.Sent, ArrayOfThreeItems[2].MI_Status);
		}

		public void TestEmailRecipientsWithMultipleEmailAddressConcatenated()
		{
			ArrayOfThreeItems[0].AddRecipientForUserCommunication("alexander.korotun@cargowise.com, alexander.korotun@wisetechglobal.com", MailRecipient.RecipientTypes.TO);
			ArrayOfThreeItems[1].AddRecipientForUserCommunication("alexander.korotun@cargowise.com,alexander.korotun@wisetechglobal.com", MailRecipient.RecipientTypes.TO);
			ArrayOfThreeItems[2].AddRecipientForUserCommunication("alexander.korotun@cargowise.com ,alexander.korotun@wisetechglobal.com", MailRecipient.RecipientTypes.TO);

			new MailSender().SendMail(ArrayOfThreeItems);
			AssertEquals("Emails with valid addresses sent", MailStatus.Sent, ArrayOfThreeItems[0].MI_Status);
			AssertEquals("Emails with valid addresses sent", MailStatus.Sent, ArrayOfThreeItems[1].MI_Status);
			AssertEquals("Emails with invalid addresses failed", MailStatus.Failed, ArrayOfThreeItems[2].MI_Status);
		}

		public void TestOneOutOfMemoryExceptionFailsMessage()
		{
			mailSenderForTestingHolder.Dispose();
			mailSenderForTestingHolder = null;
			using (
				ObjectFactory.Substitute<IMailSender>(new MailSenderForTesting((mi) =>
				{
					if (mi.MI_Subject == ArrayOfThreeItems[1].MI_Subject)
					{
						var x = "hi";
						while (true)
						{
							x = x + x;
						}
					}
				})))
			{
				try
				{
					AssertExceptionThrown<OutOfMemoryException>(() => new MailSender().SendMail(ArrayOfThreeItems));
				}
				finally
				{
					AssertEquals("Should be sent", MailStatus.Sent, ArrayOfThreeItems[0].MI_Status);
					AssertEquals("Still should be queued due to unhandled exception.", MailStatus.Queued, ArrayOfThreeItems[1].MI_Status);
					AssertEquals("Queued because not processed", MailStatus.Queued, ArrayOfThreeItems[2].MI_Status);
				}
			}
		}

		[ExpectExceptionMessage(typeof(Exception), "Email failure")]
		public void TestAllUnhandledExceptionThrows()
		{
			RegisterEmailSubjectToThrowException(ArrayOfThreeItems[0].MI_Subject, typeof(InvalidOperationException));
			RegisterEmailSubjectToThrowException(ArrayOfThreeItems[1].MI_Subject, typeof(InvalidOperationException));
			RegisterEmailSubjectToThrowException(ArrayOfThreeItems[2].MI_Subject, typeof(InvalidOperationException));
			try
			{
				new MailSender().SendMail(ArrayOfThreeItems);
			}
			finally
			{
				AssertEquals("Status", MailStatus.Queued, ArrayOfThreeItems[0].MI_Status);
				AssertEquals("Status", MailStatus.Queued, ArrayOfThreeItems[1].MI_Status);
				AssertEquals("Status", MailStatus.Queued, ArrayOfThreeItems[2].MI_Status);
			}
		}

		public void TestSendEmailWithBigAttachment()
		{
			var logger = new LoggerForTest();
			var connection = Db.Connection;
			var factory = new BusinessObjectFactory(connection);
			var mailItem = factory.NewWithValidTestData<MailItem>();
			const string errorMessage = "Enterprise.MailManager.ExternalMailInterface.FailedToSendMessageException: ";
			const string errorMessage2 = "Please verify with your email administrator that larger attachments are supported by the email server before adjusting this registry setting";

			var attachment = GenerateBigAttachment();
			attachment.MA_FileName = "attachment.xls";
			mailItem.MailAttachments.Add(attachment);
			mailItem.AddRecipientForUserCommunication("jimmy@gmail.com");
			mailItem.MI_Direction = DirectionList.Codes.Transmit;
			Assert("PRE: We want it to take the branch that involves a db hit within the try", mailItem.HasActiveRecipients());

			using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				new MailSender().SendMail(new List<MailItem> { mailItem }, logger);
				var logResult = logger.LogEntries.LastOrDefault();
				Assert("Expected log to contain: " + errorMessage + ", but got: " + logResult, logResult.Contains(errorMessage));
				Assert("Expected log to contain: " + errorMessage2 + ", but got: " + logResult, logResult.Contains(errorMessage2));
			}

			//999 999 999 MB when converted to Bytes will overflow int.MaxValue. This makes sure we cater for the max value this registry supports
			using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 999_999_999))
			{
				logger.ClearLog();
				new MailSender().SendMail(new List<MailItem> { mailItem }, logger);
				AssertEquals(0, logger.LogEntries.Count());
			}
		}

		public void TestSendEmailWithSmtp()
		{
			var logger = new LoggerForTest();
			var mailSenderForTesting = new MailSenderForTesting(mi => throw new FailedToSendMessageException("Failed!"), "test.server.com");

			var backupSender = ObjectFactory.Get<IMailSenderProvider>().GetSender();
			ObjectFactory.DisposeSubstitutions();
			var mailSender = ObjectFactory.Substitute<IMailSender>(mailSenderForTesting);

			new MailSender().SendMail(new List<MailItem> { ArrayOfThreeItems[0] }, logger);

			ObjectFactory.DisposeSubstitutions();
			ObjectFactory.Substitute(backupSender);

			AssertEquals("Status", MailStatus.Failed, ArrayOfThreeItems[0].MI_Status);
			Assert(logger.LogEntries.FirstOrDefault().StartsWith("Enterprise.MailManager.ExternalMailInterface.FailedToSendMessageException: Failed!"));
		}

		public void TestSendEmailThrowAuthenticationExceptionWithRootCertificateIsNotTrustedOnlyAddMessageToLog()
		{
			var logger = new LoggerForTest();
			var mailSenderForTesting = new MailSenderForTesting(mi => throw new FailedToConnectException("A certificate chain processed, but terminated in a root certificate which is not trusted by the trust provider."), "test.server.com");

			var backupSender = ObjectFactory.Get<IMailSenderProvider>().GetSender();
			ObjectFactory.DisposeSubstitutions();
			var mailSender = ObjectFactory.Substitute<IMailSender>(mailSenderForTesting);

			AssertNoExceptionThrown(() => new MailSender().SendMail(new List<MailItem> { ArrayOfThreeItems[0] }, logger));

			ObjectFactory.DisposeSubstitutions();
			ObjectFactory.Substitute(backupSender);

			AssertEquals("Status", MailStatus.Failed, ArrayOfThreeItems[0].MI_Status);
			Assert(logger.LogEntries.FirstOrDefault().Contains("A certificate chain processed, but terminated in a root certificate which is not trusted by the trust provider."));
		}

		public void TestSendEmailThrowAuthenticationExceptionWithNoRootCertificateIsNotTrustedWillAddToLogAndThrow()
		{
			var logger = new LoggerForTest();
			var mailSenderForTesting = new MailSenderForTesting(mi => throw new FailedToConnectException("Any other message.Not root certificate which is not trusted"), "test.server.com");

			var backupSender = ObjectFactory.Get<IMailSenderProvider>().GetSender();
			ObjectFactory.DisposeSubstitutions();
			var mailSender = ObjectFactory.Substitute<IMailSender>(mailSenderForTesting);

			AssertExceptionThrown<FailedToConnectException>(() => new MailSender().SendMail(new List<MailItem> { ArrayOfThreeItems[0] }, logger));

			ObjectFactory.DisposeSubstitutions();
			ObjectFactory.Substitute(backupSender);

			AssertEquals("Status", MailStatus.Failed, ArrayOfThreeItems[0].MI_Status);
			Assert(logger.LogEntries.FirstOrDefault().Contains("Any other message.Not root certificate which is not trusted"));
		}

		public void TestPartialRejectionIsLoggedAfterSending()
		{
			// Arrange
			const string failedRecipient = "failed@stone.com";
			const string failureMessage = "This is a test error.";
			var logger = new LoggerForTest();
			var mailSenderForTesting = new MailSenderForTesting(mi => { }, "test.server.com");
			mailSenderForTesting.SetRejectedRecipients(new[] { new RejectedRecipientInfo() { Address = failedRecipient, ErrorCode = 300, ErrorMessage = "This is a test error." } });
			var backupSender = ObjectFactory.Get<IMailSenderProvider>().GetSender();
			ObjectFactory.DisposeSubstitutions();
			var mailSender = ObjectFactory.Substitute<IMailSender>(mailSenderForTesting);
			// Act
			new MailSender().SendMail(new List<MailItem> { ArrayOfThreeItems[0] }, logger);
			ObjectFactory.DisposeSubstitutions();
			ObjectFactory.Substitute(backupSender);
			// Assert
			AssertEquals("Status", MailStatus.Sent, ArrayOfThreeItems[0].MI_Status);
			AssertContains("The rejected recipient's address should be logged", failedRecipient, logger.LogEntries.FirstOrDefault());
			AssertContains("The rejection error should be logged", failureMessage, logger.LogEntries.FirstOrDefault());
		}

		public void TestSendWhenRecipientNotAccepted()
		{
			DataRegistry.Instance.UseGraphApiForOutgoing = false;
			Env.Registry.MailboxEmailAddress = "rick@test.cargowise.com";
			Env.Registry.SMTPServer = "localhost";

			var mail = Factory.NewWithValidTestData<MailItem>();
			mail.MI_Direction = DirectionList.Codes.Transmit;
			mail.MI_From = "from@test.cargowise.com";
			mail.AddRecipientForUserCommunication("invalidTo@test.cargowise.com");
			mail.MI_Subject = "TestSendWhenRecipientNotAccepted";
			mail.MI_Body = "TestSendWhenRecipientNotAccepted";

			var extraProcessorVerbs = new Dictionary<string, IVerb>();
			extraProcessorVerbs.Add("RCPT", new RejectedRcptVerbForTest());
			var server = new SMTPServerSupportingCustomizeVerb(extraProcessorVerbs);
			server.Start();

			try
			{
				Globals.IsTest_ForTest.Value = false;
				using (new DisposableAction(() => Globals.IsTest_ForTest.ResetValue()))
				{
					var mailSender = new MailSender();
					var logger = new LoggerForTest();
					mailSender.SendMail(new[] { mail }, logger);
					var commonLogInfo = $@"SMTP Command Error occurring when sending email with subject 'TestSendWhenRecipientNotAccepted'.
MessageFromAddress: from@test.cargowise.com
MessageToAddress: invalidTo@test.cargowise.com
RejectedRecipients: Address:invalidTo@test.cargowise.com,ErrorCode:504,ErrorMessage:Recipient rejected
Error Message: No recipients were accepted.";

					AssertCollectionContains($"{commonLogInfo}\r\nWill try to deliver the email again using the system email address as the From sender:   From: rick@test.cargowise.com.", logger.LogEntries);
					AssertCollectionContains(commonLogInfo, logger.LogEntries);
				}
			}
			finally
			{
				server.Stop();
			}
		}

		#region Implementation

		public void RegisterEmailSubjectToThrowException(string subject, params Type[] exceptionTypes)
		{
			subjectExceptions.Add(subject, exceptionTypes);
		}

		protected override void SetUp()
		{
			base.SetUp();

			mailSenderForTestingHolder = ObjectFactory.Substitute<IMailSender>(new MailSenderForTesting((mi) =>
			{
				string subject = mi.MI_Subject;
				if (subjectExceptions.TryGetValue(subject, out IEnumerable<Type> exceptionTypes))
				{
					Exception ex = null;
					foreach (var exType in exceptionTypes.Reverse())
					{
						if (ex == null)
						{
							ex = (Exception)Activator.CreateInstance(exType, new object[] { subjectExceptionMessage ?? "Email failure" });
						}
						else
						{
							ex = (Exception)Activator.CreateInstance(exType, new object[] { subjectExceptionMessage ?? "Email failure", ex });
						}
					}
					if (ex != null)
					{
						throw ex;
					}
				}
			}));
		}

		protected override void TearDown()
		{
			mailSenderForTestingHolder?.Dispose();
			subjectExceptions.Clear();
			subjectExceptionMessage = null;
			base.TearDown();
		}

		MailItem[] ArrayOfThreeItems
		{
			get
			{
				if (arrayOfThreeItems == null)
				{
					arrayOfThreeItems = new MailItem[3];
					arrayOfThreeItems[0] = Factory.New<MailItem>();
					arrayOfThreeItems[1] = Factory.New<MailItem>();
					arrayOfThreeItems[2] = Factory.New<MailItem>();
					Initialise(arrayOfThreeItems[0]);
					Initialise(arrayOfThreeItems[1]);
					Initialise(arrayOfThreeItems[2]);

					arrayOfThreeItems[0].MI_Subject = "SubjectA";
					arrayOfThreeItems[1].MI_Subject = "SubjectB";
					arrayOfThreeItems[2].MI_Subject = "SubjectC";
				}
				return arrayOfThreeItems;
			}
		}
		MailItem[] arrayOfThreeItems;

		void Initialise(MailItem mi)
		{
			mi.MI_Direction = DirectionList.Codes.Receive;
			mi.MI_Status = MailStatus.Queued;
			mi.MI_SendDateTime = Env.Time.CurrentLocalDateTime;
			mi.MI_ReceivedDateTime = Env.Time.CurrentLocalDateTime;
			mi.AddRecipientForUserCommunication("To@test.com", MailRecipient.RecipientTypes.TO);
		}

		MailItem[] ArrayOfThreeItemsWithEmptyRecipients
		{
			get
			{
				if (arrayOfThreeItemsWithEmptyRecipients == null)
				{
					arrayOfThreeItemsWithEmptyRecipients = new MailItem[3];
					arrayOfThreeItemsWithEmptyRecipients[0] = Factory.New<MailItem>();
					arrayOfThreeItemsWithEmptyRecipients[1] = Factory.New<MailItem>();
					arrayOfThreeItemsWithEmptyRecipients[2] = Factory.New<MailItem>();
					InitialiseWithEmptyRecipient(arrayOfThreeItemsWithEmptyRecipients[0]);
					InitialiseWithEmptyRecipient(arrayOfThreeItemsWithEmptyRecipients[1]);
					InitialiseWithEmptyRecipient(arrayOfThreeItemsWithEmptyRecipients[2]);

					arrayOfThreeItemsWithEmptyRecipients[0].MI_Subject = "SubjectA";
					arrayOfThreeItemsWithEmptyRecipients[1].MI_Subject = "SubjectB";
					arrayOfThreeItemsWithEmptyRecipients[2].MI_Subject = "SubjectC";

					arrayOfThreeItemsWithEmptyRecipients[0].MI_Direction = DirectionList.Codes.Transmit;
					arrayOfThreeItemsWithEmptyRecipients[1].MI_Direction = DirectionList.Codes.Transmit;
					arrayOfThreeItemsWithEmptyRecipients[2].MI_Direction = DirectionList.Codes.Transmit;
				}
				return arrayOfThreeItemsWithEmptyRecipients;
			}
		}
		MailItem[] arrayOfThreeItemsWithEmptyRecipients;

		void InitialiseWithEmptyRecipient(MailItem mi)
		{
			mi.MI_Status = MailStatus.Queued;
			mi.MI_SendDateTime = Env.Time.CurrentLocalDateTime;
			mi.MI_ReceivedDateTime = Env.Time.CurrentLocalDateTime;
			mi.AddRecipientForUserCommunication("", MailRecipient.RecipientTypes.TO);
		}

		IDisposable mailSenderForTestingHolder;
		readonly Dictionary<string, IEnumerable<Type>> subjectExceptions = new Dictionary<string, IEnumerable<Type>>();
		string subjectExceptionMessage;

		MailAttachment GenerateBigAttachment()
		{
			var attachment = Factory.New<MailAttachment>();
			using (var inputStream = GenerateRandomStream(2100000))
			{
				var inputBytes = new byte[inputStream.Length];
				inputStream.Read(inputBytes, 0, inputBytes.Length);
				attachment.MA_Data = new ZBlob(inputBytes);
				return attachment;
			}
		}

		static Stream GenerateRandomStream(int sizeInBytes)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
			var random = new Random();
			var memoryStream = new MemoryStream();

			using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, 1024, true))
			{
				while (memoryStream.Length < sizeInBytes)
				{
					string randomString = new string(Enumerable.Repeat(chars, 1024)
						.Select(s => s[random.Next(s.Length)]).ToArray());

					writer.Write(randomString);
				}
			}

			memoryStream.Position = 0;
			return memoryStream;
		}

		#endregion
	}
}
