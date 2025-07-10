using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Enterprise.Environment;
using Enterprise.MailManager.ExternalMailInterface.CommonInterfaces;
using Enterprise.MailManager.ExternalMailInterface.POP3;
using Enterprise.MailManager.MailFilters.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Lists;
using Enterprise.ZArchitecture.Environment;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Pop3;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	public class MailDownloaderTest : TransactionedTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			SimpleMailServerForTest.SetUp<SimpleSmtpServer>();
			SimpleMailServerForTest.SetUp<SimplePop3Server>();
		}

		protected override void TearDown()
		{
			base.TearDown();
			SimpleMailServerForTest.TearDown();
		}

		public void TestUseMailKitPOP3AndIMAPProtocolLogging()
		{
			using var mailDownloader = new MailDownloaderForTest(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, MailTestHelpers.Username1, MailTestHelpers.Password1, MailRetrievalProtocols.POP3, nameof(SecureConnectionTypes.SSL));
			var log = new StringBuilder();
			mailDownloader.LogMessage += (type, message) => { log.Append($"Type: {type}, Message: {message}"); };

			Env.Registry.RawRegistry.UseOAuth2ForIncoming.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, OAuth2TypeList.Codes.Ms365);
			Env.Registry.RawRegistry.UseMailKitPOP3AndIMAPProtocolLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var mailClient = mailDownloader.GetMailProtocolTestMailProtocol() as Pop3Client;
			var protocolLogger = mailClient.ProtocolLogger;
			AssertEquals(typeof(ProtocolLogger), protocolLogger.GetType());

			Env.Registry.RawRegistry.UseMailKitPOP3AndIMAPProtocolLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			mailClient = mailDownloader.GetMailProtocolTestMailProtocol() as Pop3Client;
			protocolLogger = mailClient.ProtocolLogger;
			AssertEquals(typeof(NullProtocolLogger), protocolLogger.GetType());
		}

		public void TestLogProgressOfDownloading()
		{
			var log = new StringBuilder();
			using var mailDownloader = new MailDownloaderForTest(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, MailTestHelpers.Username1, MailTestHelpers.Password1, MailRetrievalProtocols.POP3, nameof(SecureConnectionTypes.SSL));
			mailDownloader.MessageIds = new List<string> { "Id1", "Id2", "Id3", "Id4" };
			mailDownloader.LogMessage += (type, message) => { log.Append($"Type: {type}, Message: {message}"); };

			AssertNoExceptionThrown(() => mailDownloader.DownloadFromServer());
			AssertContains("Downloading 4 emails from mail server at 127.0.0.1", log.ToString());
			AssertEquals(4, Regex.Matches(log.ToString(), "1 email downloaded in this batch").Count);

			log.Clear();
			mailDownloader.MessageIds.Clear();

			for (var i = 0; i < 9; i++)
			{
				mailDownloader.MessageIds.Add(i.ToString());
			}

			mailDownloader.SetUpProtocolMessageCount(mailDownloader.MessageIds.Count);

			AssertNoExceptionThrown(() => mailDownloader.DownloadFromServer());
			AssertContains("Downloading 9 emails from mail server at 127.0.0.1", log.ToString());
			AssertEquals(9, Regex.Matches(log.ToString(), "1 email downloaded in this batch").Count);

			log.Clear();
			mailDownloader.MessageIds.Clear();

			for (var i = 0; i < 1314; i++)
			{
				mailDownloader.MessageIds.Add(i.ToString());
			}

			mailDownloader.SetUpProtocolMessageCount(mailDownloader.MessageIds.Count);

			AssertNoExceptionThrown(() => mailDownloader.DownloadFromServer());
			AssertContains("Downloading 1314 emails from mail server at 127.0.0.1", log.ToString());
			AssertEquals(5, Regex.Matches(log.ToString(), "262 emails downloaded in this batch").Count);
			AssertContains("4 emails downloaded in this batch", log.ToString());

			log.Clear();
			mailDownloader.MessageIds.Clear();

			for (var i = 0; i < 5000; i++)
			{
				mailDownloader.MessageIds.Add(i.ToString());
			}

			mailDownloader.SetUpProtocolMessageCount(mailDownloader.MessageIds.Count);

			AssertNoExceptionThrown(() => mailDownloader.DownloadFromServer());
			AssertContains("Downloading 5000 emails from mail server at 127.0.0.1", log.ToString());
			AssertEquals(5, Regex.Matches(log.ToString(), "1000 emails downloaded in this batch").Count);

			log.Clear();
			mailDownloader.MessageIds.Clear();

			for (var i = 0; i < 7800; i++)
			{
				mailDownloader.MessageIds.Add(i.ToString());
			}

			mailDownloader.SetUpProtocolMessageCount(mailDownloader.MessageIds.Count);

			AssertNoExceptionThrown(() => mailDownloader.DownloadFromServer());
			AssertContains("Downloading 7800 emails from mail server at 127.0.0.1", log.ToString());
			AssertEquals(7, Regex.Matches(log.ToString(), "1000 emails downloaded in this batch").Count);
			AssertContains("800 emails downloaded in this batch", log.ToString());
		}
		public void TestEmailHasNoRecipientsExceptionIsHandled()
		{
			var log = new StringBuilder();
			using var mailDownloader = new MailDownloaderForTest(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, MailTestHelpers.Username1, MailTestHelpers.Password1, MailRetrievalProtocols.POP3, nameof(SecureConnectionTypes.SSL));
			mailDownloader.MessageIds = new List<string>() { "Id1" };
			mailDownloader.LogMessage += (type, message) => { log.Append($"Type: {type}, Message: {message}"); };
			mailDownloader.EmailDownloaded += (string id, ref string email, ref bool downloading) =>
			{
				throw new EmailHasNoRecipientsException("EmailHasNoRecipientsException message for test");
			};

			AssertNoExceptionThrown(() => mailDownloader.DownloadFromServer());
			AssertContains("EmailHasNoRecipientsException message for test", log.ToString());
		}

		public void TestInvalidOperationExceptionIsHandled()
		{
			var log = new StringBuilder();
			using var mailDownloader = new MailDownloaderForTest(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, MailTestHelpers.Username1, MailTestHelpers.Password1, MailRetrievalProtocols.POP3, nameof(SecureConnectionTypes.SSL));
			mailDownloader.MessageIds = new List<string>() { "Id1" };
			mailDownloader.LogMessage += (type, message) => { log.Append($"Type: {type}, Message: {message}"); };
			mailDownloader.EmailDownloaded += (string id, ref string email, ref bool downloading) =>
			{
				throw new InvalidOperationException("Email address is longer than max length. headerFrom:_Gift Card<team_VZygspRPJjlh9PvFQbOcmQm2Hr5m18xvIC4nJyQl5rlhoAapT1JrAD4ZuXCtcb6oiI9Sj6hfP4uRxWLaNYBW2T9vFTYtjLlBxg6XaktwxnXYl8lRumqsE3nVeRJ7@cvs.com>, maxLength:128");
			};

			AssertNoExceptionThrown(() => mailDownloader.DownloadFromServer());
			AssertContains("Email address is longer than max length.", log.ToString());
		}

		public void TestShouldNotDownloadingWhenCannotGetMessageSize()
		{
			using (var stream = new MemoryStream())
			{
				var log = new StringBuilder();
				using var mailDownloader = new MailDownloaderForTest2(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, MailTestHelpers.Username1, MailTestHelpers.Password1, MailRetrievalProtocols.POP3, nameof(SecureConnectionTypes.SSL), stream);
				mailDownloader.LogMessage += (type, message) => { log.Append($"Type: {type}, Message: {message}"); };
				mailDownloader.DownloadFromServer();

				AssertEquals("Should be called 1 time", 1, mailDownloader.CallCounterForGetMessageUid);
			}
		}

		public void TestServiceNotConnectedExceptionDoesNotStopDownloading()
		{
			var log = new StringBuilder();
			using var mailDownloader = new MailDownloaderForTest(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, MailTestHelpers.Username1, MailTestHelpers.Password1, MailRetrievalProtocols.POP3, nameof(SecureConnectionTypes.SSL));
			mailDownloader.LogMessage += (type, message) => { log.Append($"Type: {type}, Message: {message}"); };
			mailDownloader.EmailDownloaded += (string id, ref string email, ref bool downloading) =>
			{
				((IMailDownloader)mailDownloader).DeleteMessage(id);
			};
			mailDownloader.DownloadFromServer();
			AssertEquals("Will throw ServiceNotConnectedException when processing Id1, but this shouldn't block processing Id2, Id3 & Id4", 3, mailDownloader.DownloadedMessages.Count);
			AssertCollectionContains("Id2", mailDownloader.DownloadedMessages);
			AssertCollectionContains("Id3", mailDownloader.DownloadedMessages);
			AssertCollectionContains("Id4", mailDownloader.DownloadedMessages);
		}

		public void TestServiceNotConnectedExceptionIsCaught()
		{
			var log = new StringBuilder();
			var configuration = new MailServerConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, "alpha", "6Jo%q8dz", MailRetrievalProtocols.POP3, nameof(SecureConnectionTypes.None));
			using var mailDownloader = new MailDownloader(configuration);
			mailDownloader.LogMessage += (type, message) => { log.Append($"Type: {type}, Message: {message}"); };

			mailDownloader.DownloaderClosing += ThrowPop3ServiceNotConnectedException;
			mailDownloader.DownloadFromServer();
			var exceptionMessage = log.ToString();
			AssertContains("Type: Error, Message: Error downloading email from mail server at 127.0.0.1:110 -", exceptionMessage);

			void ThrowPop3ServiceNotConnectedException(long messageCount) => throw new ServiceNotConnectedException("The POP3 server has unexpectedly disconnected.", new InvalidOperationException("Inner exception message."));
		}

		public void TestProtocolExceptionIsHandledForEmailDownloaded()
		{
			var log = new StringBuilder();
			using var mailDownloader = new MailDownloaderForTest(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, MailTestHelpers.Username1, MailTestHelpers.Password1, MailRetrievalProtocols.POP3, nameof(SecureConnectionTypes.SSL));
			mailDownloader.LogMessage += (type, message) => { log.Append($"Type: {type}, Message: {message}"); };
			mailDownloader.EmailDownloaded += (string uniqueId, ref string email, ref bool continueDownloading) => throw new Pop3ProtocolException("The POP3 server has unexpectedly disconnected.");
			mailDownloader.DownloadFromServer();
			AssertContains("The POP3 server has unexpectedly disconnected.", log.ToString());
		}

		public void TestProtocolExceptionIsHandled()
		{
			var log = new StringBuilder();
			var configuration = new MailServerConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, "alpha", "6Jo%q8dz", MailRetrievalProtocols.POP3, nameof(SecureConnectionTypes.None));
			using var mailDownloader = new MailDownloader(configuration);
			mailDownloader.LogMessage += (type, message) => { log.Append($"Type: {type}, Message: {message}"); };

			mailDownloader.DownloaderClosing += ThrowPop3ProtocolException;
			mailDownloader.DownloadFromServer();
			AssertContains("The POP3 server has unexpectedly disconnected.", log.ToString());
			mailDownloader.DownloaderClosing -= ThrowPop3ProtocolException;

			log.Clear();
			mailDownloader.DownloaderClosing += ThrowImapProtocolException;
			mailDownloader.DownloadFromServer();
			AssertContains("The IMAP server has unexpectedly disconnected.", log.ToString());

			void ThrowPop3ProtocolException(long messageCount) => throw new Pop3ProtocolException("The POP3 server has unexpectedly disconnected.");
			void ThrowImapProtocolException(long messageCount) => throw new ImapProtocolException("The IMAP server has unexpectedly disconnected.");
		}

		public void TestAuthenticationExceptionIsHandled()
		{
			var configuration = new MailServerConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, "alpha", "wrong password", MailRetrievalProtocols.POP3, nameof(SecureConnectionTypes.None));
			using var mailDownloader = new MailDownloader(configuration);
			AssertNoExceptionThrown(() => mailDownloader.DownloadFromServer());
		}

		public void TestDownloadFromServerWithLoginApopWithoutTimeStamp()
		{
			var configuration = new MailServerConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, "alpha", "6Jo%q8dz", MailRetrievalProtocols.POP3, nameof(SecureConnectionTypes.None));
			using var mailDownloader = new MailDownloader(configuration);

			AssertNoExceptionThrown(() => mailDownloader.DownloadFromServer());
		}

		public void TestDownloadFromServerWithEmptyMailboxUserName()
		{
			var configuration = new MailServerConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, string.Empty, "PasswordTest", MailRetrievalProtocols.POP3, nameof(SecureConnectionTypes.None));
			using var mailDownloader = new MailDownloader(configuration);

			AssertNoExceptionThrown(() => mailDownloader.DownloadFromServer());
		}

		public void TestDownloadFromServerWithTlsSecureConnectionType()
		{
			var configuration = new MailServerConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, "alpha", "6Jo%q8dz", MailRetrievalProtocols.POP3, nameof(SecureConnectionTypes.TLS));
			using var mailDownloader = new MailDownloader(configuration);

			AssertNoExceptionThrown(() => mailDownloader.DownloadFromServer());
		}

		public void TestTimeoutExceptionIsHandled()
		{
			var log = new StringBuilder();
			var mailDownloader = new MailDownloaderForTest3(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, MailTestHelpers.Username1, MailTestHelpers.Password1, MailRetrievalProtocols.POP3, nameof(SecureConnectionTypes.SSL));
			mailDownloader.LogMessage += (type, message) => { log.Append($"Type: {type}, Message: {message}"); };

			AssertNoExceptionThrown(() => mailDownloader.DownloadFromServer());
			AssertNoExceptionThrown(() => mailDownloader.Dispose());
			AssertContains("The operation has timed out.", log.ToString());
		}

		public void TestMailKitAuthenticationExceptionIsHandled()
		{
			var log = new StringBuilder();
			using var mailDownloader = new MailDownloaderForTestingMailKitAuthenticationException(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, MailTestHelpers.Username1, MailTestHelpers.Password1, MailRetrievalProtocols.POP3, nameof(SecureConnectionTypes.SSL));
			mailDownloader.LogMessage += (type, message) => { log.Append($"Type: {type}, Message: {message}"); };

			AssertNoExceptionThrown(() => mailDownloader.DownloadFromServer());
			AssertContains("Type: Information, Message: Downloading 1 email from mail server at 127.0.0.1Type: Error, Message: Error downloading email from mail server at 127.0.0.1:110 - Authentication failed.", log.ToString());
		}

		class MailDownloaderForTestingMailKitAuthenticationException : MailDownloader
		{
			public MailDownloaderForTestingMailKitAuthenticationException(string server, int port, string userName, string password, string protocol, string secureConnection)
				: base(new MailServerConfiguration(server, port, userName, password, protocol, secureConnection))
			{
			}

			Mock<IMailProtocol> protocol;
			protected override IMailProtocol GetMailProtocol()
			{
				if (protocol == null)
				{
					protocol = new Mock<IMailProtocol>();
					protocol.Setup(p => p.GetAllMessageIds()).Returns(new List<string>() { "Id1" });
					protocol.Setup(p => p.MessageCount).Returns(1);
					protocol.Setup(p => p.GetMessageSizeById(It.IsAny<string>())).Returns(1);
					protocol.Setup(p => p.GetMessageById(It.IsAny<string>())).Throws(new MailKit.Security.AuthenticationException());
				}
				return protocol.Object;
			}
		}

		class MailDownloaderForTest : MailDownloader
		{
			public MailDownloaderForTest(string server, int port, string userName, string password, string protocol, string secureConnection)
				: base(new MailServerConfiguration(server, port, userName, password, protocol, secureConnection))
			{
			}

			Mock<IMailProtocol> protocolMock;

			public IMailProtocol GetMailProtocolTestMailProtocol() => base.GetMailProtocol();

			protected override IMailProtocol GetMailProtocol()
			{
				if (protocolMock == null)
				{
					protocolMock = new Mock<IMailProtocol>();
					protocolMock.Setup(p => p.GetAllMessageIds()).Returns(MessageIds);
					protocolMock.Setup(p => p.MessageCount).Returns(MessageIds.Count);
					protocolMock.Setup(p => p.GetMessageSizeById(It.IsAny<string>())).Returns(1);
					protocolMock.Setup(p => p.GetMessageById(It.IsAny<string>())).Returns(new byte[1]);
					protocolMock.Setup(p => p.DeleteMessageById(It.Is<string>(d => MessageIds.Contains(d)))).Callback<string>(x =>
					{
						var id = x;
						if (!connected) // Will throw ServiceNotConnectedException when processing Id1
						{
							throw new ServiceNotConnectedException("The Client is not connected.");
						}
						DownloadedMessages.Add(id);
					});
					protocolMock.Setup(p => p.ReOpenIfNeeded()).Callback(() =>
					{
						connected = true;
					});
				}

				return protocolMock.Object;
			}

			bool connected;

			internal List<string> MessageIds { get; set; } = new() { "Id1", "Id2", "Id3", "Id4" };

			internal List<string> DownloadedMessages { get; } = new();

			internal void SetUpProtocolMessageCount(int count)
			{
				protocolMock.Setup(p => p.MessageCount).Returns(count);
			}
		}

		class MailDownloaderForTest2 : MailDownloader
		{
			public MailDownloaderForTest2(string server, int port, string userName, string password, string protocol, string secureConnection, Stream downloadStream)
				: base(new MailServerConfiguration(server, port, userName, password, protocol, secureConnection))
			{
				DownloadStream = downloadStream;
			}
			Stream DownloadStream { get; }

			IMailProtocol protocol;
			protected override IMailProtocol GetMailProtocol()
			{
				if (protocol == null)
				{
					var mailServerConfiguration = new MailServerConnectionConfiguration(MailTestHelpers.LocalServer, MailTestHelpers.Pop3Port, SecureConnectionTypes.SSL);
					var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(MailTestHelpers.Username1, MailTestHelpers.Password1);

					var mockRepo = new MockRepository(MockBehavior.Strict);
					var pop3 = mockRepo.Create<MailKitPop3ForDownloadTest>(mailServerConfiguration, userPasswordAuthConfiguration, (Action<string, Exception, string>)ReportIncomingCommandException);
					pop3.Setup(p => p.IsAuthenticated).Returns(true);
					pop3.Setup(p => p.IsConnected).Returns(true);
					pop3.Setup(p => p.Count).Returns(1);
					pop3.Setup(p => p.GetMessageUid(It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns("test")
						.Callback<int, CancellationToken>((x, y) => CallCounterForGetMessageUid++);
					pop3.Setup(p => p.GetMessageSize(It.IsAny<int>(), It.IsAny<CancellationToken>())).Throws(new Pop3CommandException("POP3 server did not respond with a +OK response to the LIST command.", "Error"));
					pop3.Setup(p => p.GetStream(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>(), It.IsAny<ITransferProgress>())).Returns(DownloadStream);
					pop3.Protected().Setup("Dispose", ItExpr.IsAny<bool>());
					protocol = pop3.Object;
				}
				return protocol;
			}

			public int CallCounterForGetMessageUid { get; set; }
		}

		class MailDownloaderForTest3 : MailDownloader
		{
			public MailDownloaderForTest3(string server, int port, string userName, string password, string protocol, string secureConnection)
				: base(new MailServerConfiguration(server, port, userName, password, protocol, secureConnection))
			{
			}

			Mock<IMailProtocol> protocol;
			protected override IMailProtocol GetMailProtocol()
			{
				if (protocol == null)
				{
					protocol = new Mock<IMailProtocol>();
					protocol.Setup(p => p.GetAllMessageIds()).Returns(new List<string>() { "Id1" });
					protocol.Setup(p => p.MessageCount).Returns(1);
					protocol.Setup(p => p.GetMessageSizeById(It.IsAny<string>())).Returns(1);
					protocol.Setup(p => p.GetMessageById(It.IsAny<string>())).Returns(new byte[1]);
					protocol.Setup(p => p.Dispose()).Throws(new TimeoutException("The operation has timed out."));
				}
				return protocol.Object;
			}
		}

		public class MailKitPop3ForDownloadTest : MailKitPop3, IMailProtocol, IDisposable
		{
			public MailKitPop3ForDownloadTest(
				MailServerConnectionConfiguration mailServerConfiguration,
				UserPasswordAuthConfiguration userPasswordAuthConfiguration,
				Action<string, Exception, string> reportErrorAction = null)
				: base(mailServerConfiguration, userPasswordAuthConfiguration, reportErrorAction)
			{
			}

			void IMailProtocol.Open()
			{
			}

			void IMailProtocol.Close()
			{
			}

			void IDisposable.Dispose()
			{
				base.Dispose();
			}
		}
	}
}
