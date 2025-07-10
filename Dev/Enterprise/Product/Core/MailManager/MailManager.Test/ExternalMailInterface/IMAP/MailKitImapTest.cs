using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Enterprise.MailManager.ExternalMailInterface.CommonInterfaces;
using Enterprise.MailManager.ExternalMailInterface.Testing;
using Enterprise.MailManager.MailFilters.Testing;
using Enterprise.ZArchitecture.Core.Lists;
using Enterprise.ZArchitecture.Environment;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using Moq;
using NUnit.Framework;

namespace Enterprise.MailManager.ExternalMailInterface.IMAP.Testing
{
	sealed class MailKitImapTest : InboundMailServiceTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			SimpleMailServerForTest.SetUp<SimpleSmtpServer>();
			SimpleMailServerForTest.SetUp<SimpleImapServer>();
		}

		protected override void TearDown()
		{
			base.TearDown();
			SimpleMailServerForTest.TearDown();
		}

		public void TestDeleteMessageByNumberNoExceptionWhenMessagesNoLongerExist()
		{
			var mockInbox = new Mock<IMailFolder>();

			mockInbox.Setup(x => x.IsOpen).Returns(true);
			mockInbox.Setup(x => x.Expunge(It.IsAny<System.Threading.CancellationToken>()));

			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.ImapPort, SecureConnectionTypes.None);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, string.Empty);
			using (var imap = new TestMailKitImapForMockMailFolder(mockInbox, mailServerConfiguration, userPasswordAuthConfiguration))
			{
				imap.deleteAction = x => throw new ImapCommandException(ImapCommandResponse.No, "NO", "The specified message set is invalid");
				imap.batchDeleteAction = l => throw new ImapCommandException(ImapCommandResponse.No, "NO", "Some of the requested messages no longer exist");
				AssertNoExceptionThrown(() => imap.DeleteMessageByNumber(new List<long> { 1, 2 }));
			}
		}

		public void TestImapCommandExceptionIsHandledForClose()
		{
			var log = new StringBuilder();
			using var mailDownloader = new MailDownloader(MailTestHelpers.GetConfiguration(MailRetrievalProtocols.IMAP));
			mailDownloader.LogMessage += (type, message) => { log.Append($"Type: {type}, Message: {message}"); };

			var mockInbox = new Mock<IMailFolder>();
			mockInbox.Setup(m => m.Expunge(It.IsAny<System.Threading.CancellationToken>()))
					 .Throws(new ImapCommandException(ImapCommandResponse.No, "The IMAP server replied to the 'EXPUNGE' command with a 'NO' response: EXPUNGE failed."));

			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.ImapPort, SecureConnectionTypes.None);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, MailTestHelpers.Password1);
			var imap = new TestMailKitImapForMockMailFolder(mockInbox, mailServerConfiguration, userPasswordAuthConfiguration, mailDownloader.ReportIncomingCommandException);
			AssertNoExceptionThrown(() => imap.Dispose());
			AssertContains($@"Type: Warning, Message: Error expunge email from server '{MailTestHelpers.LocalServer}', this error is returned from the server side, it's better to check with your server provider, see below for details.
Response Type: No
Response Text: The IMAP server replied to the 'EXPUNGE' command with a 'NO' response: EXPUNGE failed.", log.ToString());

			mockInbox.VerifyAll();
		}

		public void TestNoMessageNotFoundExceptionWhenGetMessageById()
		{
			var log = new StringBuilder();
			using var mailDownloader = new MailDownloader(MailServerConfiguration.Default);
			mailDownloader.LogMessage += (type, message) => { log.Append($"Type: {type}, Message: {message}"); };

			var mockInbox = new Mock<IMailFolder>();
			mockInbox.Setup(m => m.GetStream(It.IsAny<UniqueId>(), It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken>(), It.IsAny<ITransferProgress>()))
					 .Throws(new MessageNotFoundException("The IMAP server did not return the requested stream."));

			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.ImapPort, SecureConnectionTypes.None);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, string.Empty);
			using (var imap = new TestMailKitImapForMockMailFolder(mockInbox, mailServerConfiguration, userPasswordAuthConfiguration, mailDownloader.ReportIncomingCommandException))
			{
				AssertNoExceptionThrown(() => imap.GetMessageById("blah blah"));
				AssertNullOrEmpty("Should have no logs", log.ToString());
			}

			mockInbox.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestNoFolderNotOpenExceptionThrownWhenDispose()
		{
			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.ImapPort, SecureConnectionTypes.None);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, MailTestHelpers.Password1);
			using (var imap = new MailKitImap(mailServerConfiguration, userPasswordAuthConfiguration))
			{
				MailTestHelpers.SetServerCertificateValidationCallback(imap);
				imap.Open();

				imap.Inbox.Close(true);
			}
		}

		public void TestNoExceptionThrownWhenDoingAdditionalAuthentication()
		{
			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.ImapPort, SecureConnectionTypes.None);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, MailTestHelpers.Password1);
			using (var imap = new MailKitImap(mailServerConfiguration, userPasswordAuthConfiguration))
			{
				MailTestHelpers.SetServerCertificateValidationCallback(imap);
				imap.Open();

				AssertNoExceptionThrown(() => imap.MailServiceHelper.TryHardConnectAndAuthenticate());
			}
		}

		public void TestAuthenticationExceptionThrownWhenAuthenticateFailed()
		{
			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.ImapPort, SecureConnectionTypes.None);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, "Wrong Password");
			using (var imap = new MailKitImap(mailServerConfiguration, userPasswordAuthConfiguration))
			{
				MailTestHelpers.SetServerCertificateValidationCallback(imap);

				try
				{
					imap.Open();
				}
				catch (FailedToAuthenticateException failedToAuthenticateException)
				{
					AssertType<AuthenticationException>(failedToAuthenticateException.InnerException);
				}
			}
		}

		public void TestNoNotConnectedExceptionThrown()
		{
			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.ImapPort, SecureConnectionTypes.None);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, MailTestHelpers.Password1);
			using (var imap = new MailKitImap(mailServerConfiguration, userPasswordAuthConfiguration))
			{
				MailTestHelpers.SetServerCertificateValidationCallback(imap);
				MailTestHelpers.SendEmailWithTLS();
				imap.Open();
				var messageIds = imap.GetAllMessageIds();
				AssertGreaterThan("Everything goes normal", messageIds.Count, 0);

				imap.messageIds.Clear();
				imap.Disconnect(false);
				messageIds = imap.GetAllMessageIds();
				AssertEquals(
					"The connection drop is requested, so we don't reconnect, and no exception thrown when calling FillAndCheckMessageIdsIfNeeded()",
					0, messageIds.Count);
			}

			using (var imap = new MailKitImap(mailServerConfiguration, userPasswordAuthConfiguration))
			{
				MailTestHelpers.SetServerCertificateValidationCallback(imap);
				MailTestHelpers.SendEmailWithTLS();

				imap.Open();
				imap.MailServiceHelper.AlwaysReconnectWhenDisconnected = true;
				imap.Disconnect(false);
				var messageIds = imap.GetAllMessageIds();
				AssertGreaterThan(
					"Simulate the case that the connection drop is unexpected, we will reconnect, and the result will be greater than 0",
					messageIds.Count, 0);
			}
		}

		public void TestExpungeShouldBeCalledWhenAuthenticated()
		{
			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.ImapPort, SecureConnectionTypes.None);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, "WrongPassword");
			using (var imap = new MailKitImap(mailServerConfiguration, userPasswordAuthConfiguration))
			{
				AssertExceptionThrown<FailedToAuthenticateException>(() => imap.Open());
			}
		}

		public void TestMessageIsExpunged()
		{
			string firstMessageId = null;
			string lastMessageId = null;
			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.ImapPort, SecureConnectionTypes.SSL);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, MailTestHelpers.Password1);
			using (var imap = new MailKitImap(mailServerConfiguration, userPasswordAuthConfiguration))
			{
				MailTestHelpers.SetServerCertificateValidationCallback(imap);
				imap.Open();

				var messageIds = MailTestHelpers.EnsureMailCountAboveZeroAndReturnMailIDs(imap);
				var count = messageIds.Count();

				if (count > 0)
				{
					firstMessageId = messageIds.First();
					lastMessageId = messageIds.Last();
					AssertNotNull(imap.GetMessageById(firstMessageId));
					AssertNotNull(imap.GetMessageById(lastMessageId));
					imap.DeleteMessageById(firstMessageId);
					imap.DeleteMessageById(lastMessageId);
					AssertNull(imap.GetMessageById(firstMessageId));
					AssertNull(imap.GetMessageById(lastMessageId));
				}
				else
				{
					AssertCountEqualsToZero();
				}
			}

			using (var imap = new MailKitImap(mailServerConfiguration, userPasswordAuthConfiguration))
			{
				MailTestHelpers.SetServerCertificateValidationCallback(imap);
				imap.Open();

				var messageIds = MailTestHelpers.EnsureMailCountAboveZeroAndReturnMailIDs(imap);

				if (!string.IsNullOrEmpty(firstMessageId) && !string.IsNullOrEmpty(lastMessageId))
				{
					AssertCollectionNotContains(firstMessageId, messageIds);
					AssertCollectionNotContains(lastMessageId, messageIds);
				}
			}
		}

		public void TestGetMessageByMessageNumberIndexShouldBe1Based()
		{
			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.ImapPort, SecureConnectionTypes.None);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, MailTestHelpers.Password1);
			using (var imap = new MailKitImap(mailServerConfiguration, userPasswordAuthConfiguration))
			{
				MailTestHelpers.SetServerCertificateValidationCallback(imap);
				imap.Open();

				var messageIds = MailTestHelpers.EnsureMailCountAboveZeroAndReturnMailIDs(imap);
				var count = messageIds.Count();
				if (count > 0)
				{
					AssertNotNull(imap.GetMessageByMessageNumber(count));
				}
				else
				{
					AssertCountEqualsToZero();
				}
			}
		}

		public void TestMessageSizeOfDeletedMailIsZero()
		{
			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.ImapPort, SecureConnectionTypes.SSL);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, MailTestHelpers.Password1);
			using (var imap = new MailKitImap(mailServerConfiguration, userPasswordAuthConfiguration))
			{
				MailTestHelpers.SetServerCertificateValidationCallback(imap);
				imap.Open();

				var messageIds = MailTestHelpers.EnsureMailCountAboveZeroAndReturnMailIDs(imap);
				if (messageIds.Any())
				{
					var deletedId = messageIds.First();

					// remove all messageIds except the selected one.
					var selected = imap.messageIds[deletedId];
					imap.messageIds.Clear();
					imap.messageIds[deletedId] = selected;

					imap.DeleteMessageById(deletedId);
					AssertEquals("Should return 0", 0, imap.GetMessageSizeById(deletedId));
				}
				else
				{
					AssertCountEqualsToZero();
				}
			}
		}

		public void TestImapCommandExceptionIsHandledForDeleteMessageById()
		{
			var log = new StringBuilder();
			using var mailDownloader = new MailDownloader(MailTestHelpers.GetConfiguration(MailRetrievalProtocols.IMAP));
			mailDownloader.LogMessage += (type, message) => { log.Append($"Type: {type}, Message: {message}"); };

			var mockInbox = new Mock<IMailFolder>();
			mockInbox.Setup(m => m.Store(It.IsAny<UniqueId>(), It.IsAny<IStoreFlagsRequest>(), It.IsAny<System.Threading.CancellationToken>()))
					 .Throws(new ImapCommandException(ImapCommandResponse.No, "STORE SAME_CMD_OVERFLOW"));

			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.ImapPort, SecureConnectionTypes.None);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, string.Empty);
			using (var imap = new TestMailKitImapForMockMailFolder(mockInbox, mailServerConfiguration, userPasswordAuthConfiguration, mailDownloader.ReportIncomingCommandException))
			{
				AssertNoExceptionThrown(() => imap.DeleteMessageById("blah blah"));
				AssertContains($@"Type: Warning, Message: Error deleting email with UniqueId 'blah blah' from server '{MailTestHelpers.LocalServer}', this error is returned from the server side, it's better to check with your server provider, see below for details.
Response Type: No
Response Text: STORE SAME_CMD_OVERFLOW", log.ToString());
			}

			mockInbox.VerifyAll();
		}

		public void TestSetMailServiceHelperOAuthFields()
		{
			var ms365OAuth2Token = new Ms365OAuth2Token() { Identifier = "id", Token = Guid.NewGuid().ToByteArray(), User = "user1" };
			Action<byte[]> action = DummyAction;
			var ms365OAuth2TenantId = Guid.NewGuid().ToString();
			var ms365ApplicationIdForIncoming = Guid.NewGuid().ToString();

			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.ImapPort, SecureConnectionTypes.None);
			var oAuth2Configuration = new Ms365OAuth2Configuration(
				tenantId: ms365OAuth2TenantId,
				applicationId: ms365ApplicationIdForIncoming,
				permissionType: Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_OutLook,
				cachedToken: ms365OAuth2Token.Token,
				tokenSaveAction: DummyAction,
				identifier: ms365OAuth2Token.Identifier);

			using (var imap = new MailKitImap(mailServerConfiguration, oAuth2Configuration))
			{
				AssertEquals(oAuth2Configuration, GetValue(imap.MailServiceHelper, "oAuth2Configuration"));
			}

			void DummyAction(byte[] message)
			{
				return;
			}
		}

		public void TestFinalizeNoExceptionThrowWhenDisposed()
		{
			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.ImapPort, SecureConnectionTypes.SSL);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, MailTestHelpers.Password1);

			var imap = new MailKitImapForTestFinalize(mailServerConfiguration, userPasswordAuthConfiguration);
			MailTestHelpers.SetServerCertificateValidationCallback(imap);
			imap.Open();

			imap.Dispose();
			AssertNoExceptionThrown(() => imap.Dispose());
			AssertNoExceptionThrown(() => imap.FinalizeForTest());
		}

		public class MailKitImapForTestFinalize : MailKitImap
		{
			public MailKitImapForTestFinalize(MailServerConnectionConfiguration config, UserPasswordAuthConfiguration auth) : base(config, auth)
			{
			}

			public void FinalizeForTest()
			{
				Dispose(false);
			}
		}

		static object GetValue(MailServiceHelper mailServiceHelper, string fieldName)
		{
			var field = typeof(MailServiceHelper).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
			var value = field.GetValue(mailServiceHelper);

			return value;
		}

		void AssertCountEqualsToZero()
		{
			Assert("Mail items are not available on shared test mail account - skip this test.", true);
		}

		protected override IMailProtocol GetMailProtocol2()
		{
			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.ImapPort, SecureConnectionTypes.None);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username2, MailTestHelpers.Password2);
			return new MailKitImap(mailServerConfiguration, userPasswordAuthConfiguration);
		}
	}
}
