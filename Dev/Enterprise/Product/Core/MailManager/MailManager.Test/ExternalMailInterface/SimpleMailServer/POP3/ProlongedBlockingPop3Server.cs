using System.Net.Sockets;
using Enterprise.MailManager.MailFilters.Testing;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	public class ProlongedBlockingPop3Server : SimpleMailServer
	{
		public override ushort Port => MailTestHelpers.ProlongedBlockingPop3Port;

		public override SimpleMailServerHandler GetMailHandler(TcpClient client)
		{
			return new ProlongedBlockingPop3ServerHandler(client);
		}

		public override void Greeting(SimpleMailServerHandler handler)
		{
			handler.Write("+OK Fake ProlongedBlockingPop3 Server Ready");
		}
	}
}
