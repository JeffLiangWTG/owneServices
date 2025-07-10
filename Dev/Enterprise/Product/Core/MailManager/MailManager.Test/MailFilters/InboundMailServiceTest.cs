using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using Enterprise.MailManager.ExternalMailInterface.CommonInterfaces;
using MailKit;
using MailKit.Security;
using NUnit.Framework;

namespace Enterprise.MailManager.MailFilters.Testing
{
	abstract class InboundMailServiceTest : TransactionedTestCase
	{
		[SnailTest]
		[ExpectNoExceptions]
		public void TestRealWorldDeleteMessageByNumber()
		{
			var messageList = new List<long>();
			MailTestHelpers.SendEmailWithTLS(MailTestHelpers.UserEmailAddress2);

			using (var protocol = GetMailProtocol2())
			{
				MailTestHelpers.SetServerCertificateValidationCallback(protocol as IMailService);
				protocol.Open();
				var messageCount = protocol.MessageCount;
				for (int i = 1; i <= messageCount; i++)
				{
					messageList.Add(i);
				}

				protocol.DeleteMessageByNumber(messageList);
			}

			using (var protocol = GetMailProtocol2())
			{
				MailTestHelpers.SetServerCertificateValidationCallback(protocol as IMailService);
				protocol.Open();
				protocol.DeleteMessageByNumber(messageList);
			}
		}

		internal void AssertCaughtExceptions(Exception exception)
		{
			AssertEquals(true, exception is AuthenticationException || exception is SocketException || exception is IOException);
		}

		protected abstract IMailProtocol GetMailProtocol2();
	}
}
