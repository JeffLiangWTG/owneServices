using System;
using CargoWise.Common;
using Enterprise.MailManager.ExternalMailInterface.CommonInterfaces;
using Enterprise.MailManager.ExternalMailInterface.IMAP;
using Enterprise.MailManager.ExternalMailInterface.POP3;
using Enterprise.ZArchitecture.Core.Lists;
using MailKit;

namespace Enterprise.MailManager.ExternalMailInterface
{
	public class MailProtocolFactory
	{
		public IMailProtocol GetMailProtocol(
			string mailRetrievalProtocol,
			MailServerConnectionConfiguration mailServerConfiguration,
			UserPasswordAuthConfiguration userPasswordAuthConfiguration,
			LogMessageHandler logMessageHandler = null,
			Action<string, Exception, string> reportErrorAction = null)
		{
			Argument.NotNullOrEmpty(mailServerConfiguration?.Server, nameof(mailRetrievalProtocol));

			ProtocolLogger logger = null;
			if (logMessageHandler != null)
			{
				logger = new ProtocolLogger(new LoggerStream(logMessageHandler));
			}

			return mailRetrievalProtocol switch
			{
				MailRetrievalProtocols.POP3 => new MailKitPop3(mailServerConfiguration, userPasswordAuthConfiguration, reportErrorAction, logger),
				MailRetrievalProtocols.IMAP => new MailKitImap(mailServerConfiguration, userPasswordAuthConfiguration, reportErrorAction, logger),
				_ => throw new ArgumentException("Unknown mail protocol was requested:" + mailRetrievalProtocol)
			};
		}

		public IMailProtocol GetMailProtocol(
			string mailRetrievalProtocol,
			MailServerConnectionConfiguration mailServerConfiguration,
			IOAuth2Configuration oAuth2Configuration,
			LogMessageHandler logMessageHandler = null,
			Action<string, Exception, string> reportErrorAction = null
			)
		{
			Argument.NotNullOrEmpty(mailServerConfiguration?.Server, nameof(mailRetrievalProtocol));

			ProtocolLogger logger = null;
			if (logMessageHandler != null)
			{
				logger = new ProtocolLogger(new LoggerStream(logMessageHandler));
			}

			return mailRetrievalProtocol switch
			{
				MailRetrievalProtocols.POP3 => new MailKitPop3(mailServerConfiguration, oAuth2Configuration, reportErrorAction, logger),
				MailRetrievalProtocols.IMAP => new MailKitImap(mailServerConfiguration, oAuth2Configuration, reportErrorAction, logger),
				_ => throw new ArgumentException("Unknown mail protocol was requested:" + mailRetrievalProtocol)
			};
		}
	}
}
