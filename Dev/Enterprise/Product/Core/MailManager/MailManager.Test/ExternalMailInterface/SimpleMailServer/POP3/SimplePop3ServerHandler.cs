using System.Net.Sockets;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	public class SimplePop3ServerHandler : SimpleMailServerHandler
	{
		string _expectedPassword;

		public SimplePop3ServerHandler(TcpClient client)
		{
			this.client = client;
		}

		public override bool IsQuitCommand(string message)
		{
			return message.StartsWith("QUIT");
		}

		public override void SayGoodBye()
		{
			Write("+OK Bye-bye.");
		}

		public override void HandleCommand(string message)
		{
			if (message.StartsWith("CAPA"))
			{
				Write("+OK Capability list follows");
				Write("USER");
				Write("PASS");
				Write("STAT");
				Write("TOP");
				Write("UIDL");
				Write(".");
			}
			else if (message.StartsWith("USER"))
			{
				var user = SubFirstWhiteSpaceAndGet(message);
				if (!SimpleMailServer.UserPasswordPairs.ContainsKey(user))
				{
					_expectedPassword = string.Empty;
					Write("-ERR Invalid user");
				}
				else
				{
					_expectedPassword = SimpleMailServer.UserPasswordPairs[user];
					Write("+OK Welcome!");
				}
			}
			else if (message.StartsWith("PASS"))
			{
				var password = SubFirstWhiteSpaceAndGet(message);
				if (!password.Equals(_expectedPassword))
				{
					_expectedPassword = string.Empty;
					Write("-ERR Invalid Logon");
				}
				else
				{
					Write("+OK Valid Logon.");
				}
			}
			else if (message.StartsWith("STAT"))
			{
				var (count, total) = SimpleSmtpServer.GetEmailCount();
				Write($"+OK {count} {total}");
			}
			else if (message.StartsWith("STLS"))
			{
				Write("+OK Begin TLS negotiation now");
			}
			else if (message.StartsWith("RETR"))
			{
				var uid = SplitAndGet(message, 1, out bool isEmpty);
				var email = SimpleSmtpServer.GetOneEmail(int.Parse(uid), out var isError);
				if (!isError)
				{
					Write($"+OK {email.Data.Length} octets");
					var lines = email.Data.Split('\r');
					foreach (var line in lines)
					{
						Write(line);
					}
					Write(".");
				}
				else
				{
					Write($"-ERR Message {uid} expunged.");
				}
			}
			else if (message.StartsWith("DELE"))
			{
				HandleDeleCommand(message);
			}
			else if (message.StartsWith("TOP"))
			{
				Write("+OK");
			}
			else if (message.StartsWith("UIDL"))
			{
				if (message.Trim().Equals("UIDL"))
				{
					Write("+OK Unique-IDs follow");
					var emailDictionary = SimpleSmtpServer.GetEmailDictionary();
					foreach (var email in emailDictionary)
					{
						Write($"{email.Key} {email.Value.MessageId}");
					}
					Write(".");
				}
				else
				{
					var uid = SplitAndGet(message, 1, out bool isEmpty);
					Write($"+OK {uid} {SimpleSmtpServer.GetOneEmail(int.Parse(uid), out var isError).MessageId}");
				}
			}
			else if (message.StartsWith("NOOP"))
			{
				Write("+OK");
			}
			else if (message.StartsWith("RSET"))
			{
				Write("+OK");
			}
		}

		public virtual void HandleDeleCommand(string message)
		{
			var uid = SplitAndGet(message, 1, out bool isEmpty);
			if (uid.Trim().EndsWith("CAPA"))
			{
				uid = uid.Substring(0, 1);
			}
			SimpleSmtpServer.DeleteEmail(int.Parse(uid), out var isError);
			if (!isError)
			{
				Write($"+OK message {uid} deleted");
			}
			else
			{
				Write($"+OK message {uid} already deleted");
			}
		}
	}
}
