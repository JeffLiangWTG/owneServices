using Enterprise.EConversation.ServiceTasks;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core.Lists;
using Moq;
using NUnit.Framework;

namespace Enterprise.EConversation.Testing
{
	class EmailReaderFactoryTest : TestCase
	{
		public void TestUserPasswordAuth()
		{
			var settings = new Mock<IMailboxSettings>();
			settings.Setup(x => x.MailRetrievalProtocol).Returns(MailRetrievalProtocols.POP3);
			settings.Setup(x => x.Server).Returns("TestServer");
			settings.Setup(x => x.Port).Returns(110);
			settings.Setup(x => x.UserName).Returns("TestUser");

			using var reader = new EmailReaderFactory().Create(settings.Object, new LoggerForTest());

			AssertType<MailProtocolEmailReader>(reader);
			AssertEquals(settings.Object.Server, reader.Server);
			AssertEquals(settings.Object.UserName, reader.Mailbox);
		}

		public void TestOAuth()
		{
			var settings = new Mock<IOAuth2MailboxSettings>();
			settings.Setup(x => x.MailRetrievalProtocol).Returns(MailRetrievalProtocols.POP3);
			settings.Setup(x => x.Server).Returns("TestServer");
			settings.Setup(x => x.UseOAuth2).Returns(true);

			using var reader = new EmailReaderFactory().Create(settings.Object, new LoggerForTest());

			AssertType<MailDownloaderEmailReader>(reader);
		}

		public void TestGraphApi()
		{
			var settings = new Mock<IOAuth2MailboxSettings>();
			settings.Setup(x => x.MailRetrievalProtocol).Returns(MailRetrievalProtocols.POP3);
			settings.Setup(x => x.Server).Returns("TestServer");
			settings.Setup(x => x.UseOAuth2).Returns(true);
			settings.Setup(x => x.UseGraphApi).Returns(true);

			using var reader = new EmailReaderFactory().Create(settings.Object, new LoggerForTest());

			AssertType<MailDownloaderEmailReader>(reader);
		}
	}
}
