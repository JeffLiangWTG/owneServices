using System.Net.Sockets;
using Enterprise.MailManager.MailFilters.Testing;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	public class SimplePop3Server : SimpleMailServer
	{
		public override ushort Port => MailTestHelpers.Pop3Port;

		public override SimpleMailServerHandler GetMailHandler(TcpClient client)
		{
			return new SimplePop3ServerHandler(client);
		}

		public override void Greeting(SimpleMailServerHandler handler)
		{
			handler.Write("+OK Fake POP3 Server Ready");
		}
	}
}
