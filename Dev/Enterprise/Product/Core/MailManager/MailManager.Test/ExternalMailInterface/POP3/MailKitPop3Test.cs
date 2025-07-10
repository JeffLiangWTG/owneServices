using System;
using System.Linq;
using System.Text;
using System.Threading;
using Enterprise.MailManager.ExternalMailInterface.CommonInterfaces;
using Enterprise.MailManager.ExternalMailInterface.Testing;
using Enterprise.MailManager.MailFilters.Testing;
using Enterprise.ZArchitecture.Core.Lists;
using MailKit.Net.Pop3;
using MailKit.Security;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.MailManager.ExternalMailInterface.POP3.Testing
{
	sealed class MailKitPop3Test : InboundMailServiceTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			SimpleMailServerForTest.SetUp<SimpleSmtpServer>();
			SimpleMailServerForTest.SetUp<SimplePop3Server>();
			SimpleMailServerForTest.SetUp<ProlongedBlockingPop3Server>();
		}

		protected override void TearDown()
		{
			base.TearDown();
			SimpleMailServerForTest.TearDown();
		}

		public void TestShouldHandlePop3CommandExceptionWhenGetMessageSizeById()
		{
			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, SecureConnectionTypes.SSL);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, MailTestHelpers.Password1);

			var logs = new StringBuilder();
			var mockRepo = new MockRepository(MockBehavior.Strict);
			using var mailDownloader = GetMailDownloader(logs);
			var mockPop3 = mockRepo.Create<MailKitPop3>(mailServerConfiguration, userPasswordAuthConfiguration, (Action<string, Exception, string>)mailDownloader.ReportIncomingCommandException, null);
			mockPop3.Setup(p => p.GetMessageSize(It.IsAny<int>(), It.IsAny<CancellationToken>())).Throws(new Pop3CommandException("POP3 server did not respond with a +OK response to the LIST command.", "Error"));
			mockPop3.Setup(p => p.IsConnected).Returns(true);
			mockPop3.Protected().Setup("Dispose", ItExpr.IsAny<bool>());

			using (var pop3 = mockPop3.Object)
			{
				pop3.messageIds.Add("test", 0);

				var expectedLogs = $@"Type: Warning, Message: Error downloading email with UniqueId 'test' from server '{MailTestHelpers.LocalServer}', this error is returned from the server side, it's better to check with your server provider, see below for details.
Status Text: Error
Exception: MailKit.Net.Pop3.Pop3CommandException: POP3 server did not respond with a +OK response to the LIST command.";

				AssertNoExceptionThrown(() => pop3.GetMessageSizeById("test"));
				AssertContains(expectedLogs, logs.ToString());
			}
		}

		public void TestShouldHandlePop3CommandExceptionWhenFillAndCheckMessageIdsIfNeeded()
		{
			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, SecureConnectionTypes.SSL);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, MailTestHelpers.Password1);

			var logs = new StringBuilder();
			var mockRepo = new MockRepository(MockBehavior.Strict);
			using var mailDownloader = GetMailDownloader(logs);
			var mockPop3 = mockRepo.Create<MailKitPop3>(mailServerConfiguration, userPasswordAuthConfiguration, (Action<string, Exception, string>)mailDownloader.ReportIncomingCommandException, null);
			mockPop3.Setup(p => p.GetMessageUid(It.IsAny<int>(), It.IsAny<CancellationToken>())).Throws(new Pop3CommandException("POP3 server did not respond with a +OK response to the UIDL command.", "Error"));
			mockPop3.Setup(p => p.IsAuthenticated).Returns(true);
			mockPop3.Setup(p => p.IsConnected).Returns(true);
			mockPop3.Setup(p => p.Count).Returns(1);
			mockPop3.Protected().Setup("Dispose", ItExpr.IsAny<bool>());

			using (var pop3 = mockPop3.Object)
			{
				var expectedLogs = $@"Type: Warning, Message: Error downloading email from server '{MailTestHelpers.LocalServer}', this error is returned from the server side, it's better to check with your server provider, see below for details.
Status Text: Error
Exception: MailKit.Net.Pop3.Pop3CommandException: POP3 server did not respond with a +OK response to the UIDL command.";

				AssertNoExceptionThrown(() => pop3.GetAllMessageIds());
				AssertContains(expectedLogs, logs.ToString());
			}
		}

		public void TestShouldHandleNotSupportedExceptionWhenFillAndCheckMessageIdsIfNeeded()
		{
			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, SecureConnectionTypes.SSL);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, MailTestHelpers.Password1);

			var logs = new StringBuilder();
			var mockRepo = new MockRepository(MockBehavior.Strict);
			using var mailDownloader = GetMailDownloader(logs);
			var mockPop3 = mockRepo.Create<MailKitPop3>(mailServerConfiguration, userPasswordAuthConfiguration, (Action<string, Exception, string>)mailDownloader.ReportIncomingCommandException, null);
			mockPop3.Setup(p => p.GetMessageUid(It.IsAny<int>(), It.IsAny<CancellationToken>())).Throws(new NotSupportedException("The POP3 server does not support the UIDL extension."));
			mockPop3.Setup(p => p.IsAuthenticated).Returns(true);
			mockPop3.Setup(p => p.IsConnected).Returns(true);
			mockPop3.Setup(p => p.Count).Returns(1);
			mockPop3.Protected().Setup("Dispose", ItExpr.IsAny<bool>());

			using (var pop3 = mockPop3.Object)
			{
				var expectedLogs = $@"Type: Warning, Message: Error downloading email from server '{MailTestHelpers.LocalServer}', this error is returned from the server side, it's better to check with your server provider, see below for details.

Exception: System.NotSupportedException: The POP3 server does not support the UIDL extension.";

				AssertNoExceptionThrown(() => pop3.GetAllMessageIds());
				AssertContains(expectedLogs, logs.ToString());
			}
		}

		public void TestShouldHandlePop3CommandExceptionWhenDeleteMessageById()
		{
			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, SecureConnectionTypes.SSL);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, MailTestHelpers.Password1);

			var logs = new StringBuilder();
			var mockRepo = new MockRepository(MockBehavior.Strict);
			using var mailDownloader = GetMailDownloader(logs);
			var mockPop3 = mockRepo.Create<MailKitPop3>(mailServerConfiguration, userPasswordAuthConfiguration, (Action<string, Exception, string>)mailDownloader.ReportIncomingCommandException, null);
			mockPop3.Setup(p => p.DeleteMessage(It.IsAny<int>(), It.IsAny<CancellationToken>())).Throws(new Pop3CommandException("POP3 server did not respond with a +OK response to the DELE command.", "Error"));
			mockPop3.Setup(p => p.IsConnected).Returns(true);
			mockPop3.Protected().Setup("Dispose", ItExpr.IsAny<bool>());

			using (var pop3 = mockPop3.Object)
			{
				mockPop3.Object.messageIds.Add("test", 0);

				var expectedLogs = $@"Type: Warning, Message: Error deleting email with UniqueId 'test' from server '{MailTestHelpers.LocalServer}', this error is returned from the server side, it's better to check with your server provider, see below for details.
Status Text: Error
Exception: MailKit.Net.Pop3.Pop3CommandException: POP3 server did not respond with a +OK response to the DELE command.";

				AssertNoExceptionThrown(() => pop3.DeleteMessageById("test"));
				AssertContains(expectedLogs, logs.ToString());
			}
		}

		public void TestNoExceptionThrownWhenDoingAdditionalAuthentication()
		{
			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, SecureConnectionTypes.SSL);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, MailTestHelpers.Password1);
			using (var pop3 = new MailKitPop3(mailServerConfiguration, userPasswordAuthConfiguration))
			{
				MailTestHelpers.SetServerCertificateValidationCallback(pop3);
				pop3.Open();

				AssertNoExceptionThrown(() => pop3.MailServiceHelper.TryHardConnectAndAuthenticate());
			}
		}

		public void TestAuthenticationExceptionThrownWhenAuthenticateFailed()
		{
			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, SecureConnectionTypes.SSL);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, "Wrong Password");
			using (var pop3 = new MailKitPop3(mailServerConfiguration, userPasswordAuthConfiguration))
			{
				MailTestHelpers.SetServerCertificateValidationCallback(pop3);

				try
				{
					pop3.Open();
				}
				catch (FailedToAuthenticateException failedToAuthenticateException)
				{
					AssertType<AuthenticationException>(failedToAuthenticateException.InnerException);
				}
			}
		}

		[SnailTest]
		public void TestNoNotConnectedExceptionThrown()
		{
			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, SecureConnectionTypes.SSL);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, MailTestHelpers.Password1);
			using (var pop3 = new MailKitPop3(mailServerConfiguration, userPasswordAuthConfiguration))
			{
				MailTestHelpers.SetServerCertificateValidationCallback(pop3);
				MailTestHelpers.SendEmailWithTLS();
				pop3.Open();
				var messageIds = pop3.GetAllMessageIds();
				AssertGreaterThan("Everything goes normal", messageIds.Count, 0);

				pop3.messageIds.Clear();
				pop3.Disconnect(false);
				messageIds = pop3.GetAllMessageIds();
				AssertEquals(
					"The connection drop is requested, so we don't reconnect, and no exception thrown when calling FillAndCheckMessageIdsIfNeeded()",
					0, messageIds.Count);
			}

			using (var pop3 = new MailKitPop3(mailServerConfiguration, userPasswordAuthConfiguration))
			{
				MailTestHelpers.SetServerCertificateValidationCallback(pop3);
				MailTestHelpers.SendEmailWithTLS();
				pop3.Open();
				pop3.MailServiceHelper.AlwaysReconnectWhenDisconnected = true;
				pop3.Disconnect(false);
				var messageIds = pop3.GetAllMessageIds();
				AssertGreaterThan(
					"Simulate the case that the connection drop is unexpected, we will reconnect, and the result will be greater than 0",
					messageIds.Count, 0);
			}
		}

		public void TestGetMessageById()
		{
			var log = new StringBuilder();
			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, SecureConnectionTypes.SSL);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, MailTestHelpers.Password1);
			using var mailDownloader = GetMailDownloader(log);
			using (var pop3 = new MailKitPop3(mailServerConfiguration, userPasswordAuthConfiguration, mailDownloader.ReportIncomingCommandException))
			{
				MailTestHelpers.SetServerCertificateValidationCallback(pop3);
				MailTestHelpers.SendEmailWithTLS();

				pop3.Open();
				var messageIds = pop3.GetAllMessageIds();

				if (messageIds.Any())
				{
					var first = messageIds.First();
					var message = pop3.GetMessageById(first);
					if (message == null)
					{
						AssertPop3CommandException(first, log.ToString());
					}
					else
					{
						Assert("message is not null, means the test passed", true);
					}
				}
			}
		}

		public void TestMessageDeleted()
		{
			var log1 = new StringBuilder();
			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, SecureConnectionTypes.SSL);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, MailTestHelpers.Password1);
			using var mailDownloader1 = GetMailDownloader(log1);
			using (var pop3 = new MailKitPop3(mailServerConfiguration, userPasswordAuthConfiguration, mailDownloader1.ReportIncomingCommandException))
			{
				MailTestHelpers.SetServerCertificateValidationCallback(pop3);
				MailTestHelpers.SendEmailWithTLS();

				pop3.Open();

				AssertNull(pop3.GetMessageById("WhatEverNotExist"));

				var messageIds = pop3.GetAllMessageIds();
				if (messageIds.Any())
				{
					var first = messageIds.First();

					var log2 = new StringBuilder();
					using var mailDownloader2 = GetMailDownloader(log2);
					using (var anotherPop3 = new MailKitPop3(mailServerConfiguration, userPasswordAuthConfiguration, mailDownloader2.ReportIncomingCommandException))
					{
						MailTestHelpers.SetServerCertificateValidationCallback(anotherPop3);

						anotherPop3.Open();
						var message1 = pop3.GetMessageById(first);
						if (message1 == null)
						{
							AssertPop3CommandException(first, log1.ToString());
						}
						else
						{
							var message2 = anotherPop3.GetMessageById(first);
							if (message2 == null)
							{
								AssertPop3CommandException(first, log2.ToString());
							}
							else
							{
								pop3.DeleteMessageById(first);
								AssertNull(pop3.GetMessageById(first));

								pop3.Close();
								AssertNull(anotherPop3.GetMessageById(first));

								AssertPop3CommandException(first, log2.ToString());
							}
						}
					}
				}
			}
		}

		public void TestRetryDeleteMessageByID()
		{
			var log1 = new StringBuilder();
			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, SecureConnectionTypes.SSL);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, MailTestHelpers.Password1);
			using var mailDownloader = GetMailDownloader(log1);
			using (var pop3 = new MailKitPop3ForTest(mailServerConfiguration, userPasswordAuthConfiguration, mailDownloader.ReportIncomingCommandException))
			{
				MailTestHelpers.SetServerCertificateValidationCallback(pop3);
				pop3.Open();
				pop3.FillMessagesForTest();

				var messageIds = pop3.GetAllMessageIds();

				pop3.Messages.Remove(messageIds.Last());

				AssertNoExceptionThrown(() => pop3.DeleteMessageById(messageIds.Last()));
			}
		}

		public void TestRetryAuthenticateWhenThrowServiceNotAuthenticatedException()
		{
			var log1 = new StringBuilder();
			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, SecureConnectionTypes.SSL);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, MailTestHelpers.Password1);
			using var mailDownloader = GetMailDownloader(log1);
			using (var pop3 = new MailKitPop3ForTest(mailServerConfiguration, userPasswordAuthConfiguration, mailDownloader.ReportIncomingCommandException))
			{
				MailTestHelpers.SetServerCertificateValidationCallback(pop3);
				pop3.Open();
				pop3.FillMessagesForTest();

				var messageIds = pop3.GetAllMessageIds();
				var id = messageIds.Last();

				pop3.MockAuthenticateIsFalse_ForTest = true;
				AssertNoExceptionThrown(() => pop3.DeleteMessageById(id));

				var messageIds1 = pop3.GetAllMessageIds();
				AssertEquals(1, messageIds.Count - messageIds1.Count);
				Assert(!messageIds1.Contains(id));
				AssertEquals(true, pop3.IsReTryAuthenticate_ForTest);
			}
		}

		public void TestNoExceptionThrownWhenRetryAuthenticateAndCountOfMailsChanged()
		{
			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, SecureConnectionTypes.SSL);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, MailTestHelpers.Password1);
			SimpleSmtpServer.AddOneEmail("123");

			using (var pop3Client = new MailKitPop3(mailServerConfiguration, userPasswordAuthConfiguration))
			{
				pop3Client.MockAuthenticateIsFalse_ForTest = true;
				MailTestHelpers.SetServerCertificateValidationCallback(pop3Client);
				pop3Client.Open();

				var messageIds = pop3Client.GetAllMessageIds();
				AssertEquals("Mail server should have one mail", 1, messageIds.Count);

				SimpleSmtpServer.ClearAll();
				AssertEquals("Mail server should have no mail", 0, pop3Client.GetMessageCount());
				AssertNoExceptionThrown(() => pop3Client.DeleteMessageById("123"));
			}
		}

		public void TestDeleteMessageById_ServerInProlongedBlockingState_ClientCanCancelAfterTimeout()
		{
			var log1 = new StringBuilder();
			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.ProlongedBlockingPop3Port, SecureConnectionTypes.SSL);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, MailTestHelpers.Password1);
			using var mailDownloader1 = GetMailDownloader(log1);
			using (var pop3 = new MailKitPop3(mailServerConfiguration, userPasswordAuthConfiguration, mailDownloader1.ReportIncomingCommandException))
			{
				MailTestHelpers.SetServerCertificateValidationCallback(pop3);
				MailTestHelpers.SendEmailWithTLS();

				pop3.Open();

				AssertNull(pop3.GetMessageById("WhatEverNotExist"));

				var messageIds = pop3.GetAllMessageIds();
				var first = messageIds.First();
				AssertExceptionThrown<OperationCanceledException>("The operation was canceled.", () => pop3.DeleteMessageById(first));
			}
		}

		MailDownloader GetMailDownloader(StringBuilder log)
		{
			var mailDownloader = new MailDownloader(MailTestHelpers.GetConfiguration(MailRetrievalProtocols.POP3));
			mailDownloader.LogMessage += (type, message) => { log.Append($"Type: {type}, Message: {message}"); };
			return mailDownloader;
		}

		void AssertPop3CommandException(string uniqueId, string log)
		{
			AssertContains($@"Type: Warning, Message: Error downloading email with UniqueId '{uniqueId}' from server '{MailTestHelpers.LocalServer}', this error is returned from the server side, it's better to check with your server provider, see below for details.
Status Text: Message 1 expunged.
Exception: MailKit.Net.Pop3.Pop3CommandException: POP3 server did not respond with a +OK response to the RETR command.", log);
		}

		protected override IMailProtocol GetMailProtocol2()
		{
			var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, SecureConnectionTypes.SSL);
			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username2, MailTestHelpers.Password2);
			return new MailKitPop3(mailServerConfiguration, userPasswordAuthConfiguration);
		}
	}
}
