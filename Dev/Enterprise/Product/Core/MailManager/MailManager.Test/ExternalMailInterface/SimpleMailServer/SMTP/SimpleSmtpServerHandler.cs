using System.Net.Sockets;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	public class SimpleSmtpServerHandler : SimpleMailServerHandler
	{
		public SimpleSmtpServerHandler(TcpClient client)
		{
			this.client = client;
		}

		public override bool IsQuitCommand(string message)
		{
			return message.StartsWith("QUIT");
		}

		public override void SayGoodBye()
		{
			Write("221 OK.");
		}

		public override void HandleCommand(string message)
		{
			if (message.StartsWith("EHLO"))
			{
				Write("250-wisetechglobal.com Hello");
				Write("250 AUTH LOGIN PLAIN CRAM-MD5");
			}
			else if (message.StartsWith("AUTH PLAIN"))
			{
				Write("235 2.7.0 Authentication successful");
			}
			else if (message.StartsWith("AUTH LOGIN"))
			{
				Write("235 2.7.0 Authentication successful");
			}
			else if (message.StartsWith("AUTH CRAM-MD5"))
			{
				Write("235 2.7.0 Authentication successful");
			}
			else if (message.StartsWith("RCPT TO"))
			{
				Write("250 OK");
			}
			else if (message.StartsWith("MAIL FROM"))
			{
				Write("250 OK");
			}
			else if (message.StartsWith("DATA"))
			{
				Write("354 Start mail input; end with <CR><LF>.<CR><LF>");
			}
			else if (message.StartsWith("From: "))
			{
				SimpleSmtpServer.AddEmail(message);
				Write("250 OK");
			}
		}
	}
}
