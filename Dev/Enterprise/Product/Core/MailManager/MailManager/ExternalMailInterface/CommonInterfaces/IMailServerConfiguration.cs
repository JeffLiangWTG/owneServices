using System;

namespace Enterprise.MailManager.ExternalMailInterface.CommonInterfaces
{
	public interface IMailServerConfiguration
	{
		string Server { get; }

		int Port { get; }

		string Protocol { get; }

		string UserName { get; }

		IMailProtocol GetMailProtocol(LogMessageHandler logMessageHandler = null, Action<string, Exception, string> reportErrorAction = null);

		IOAuth2Configuration GetOAuth2Configuration(LogMessageHandler logMessageHandler = null);
	}
}
