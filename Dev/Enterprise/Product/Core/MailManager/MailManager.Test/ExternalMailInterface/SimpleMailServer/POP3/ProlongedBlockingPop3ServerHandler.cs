using System.Net.Sockets;
using System.Threading;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	public class ProlongedBlockingPop3ServerHandler : SimplePop3ServerHandler
	{
		public ProlongedBlockingPop3ServerHandler(TcpClient client) : base(client)
		{
		}

		public override void HandleDeleCommand(string message)
		{
			Thread.Sleep(Timeout.Infinite);
		}
	}
}
