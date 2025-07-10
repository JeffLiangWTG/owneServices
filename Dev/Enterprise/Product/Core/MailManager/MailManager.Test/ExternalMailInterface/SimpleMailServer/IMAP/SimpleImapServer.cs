using System.Net.Sockets;
using Enterprise.MailManager.MailFilters.Testing;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	public class SimpleImapServer : SimpleMailServer
	{
		public override ushort Port => MailTestHelpers.ImapPort;

		public override SimpleMailServerHandler GetMailHandler(TcpClient client)
		{
			return new SimpleImapServerHandler(client);
		}

		public override void Greeting(SimpleMailServerHandler handler)
		{
			handler.Write("* OK Fake IMAP Server Ready");
		}
	}
}
