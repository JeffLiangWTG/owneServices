using System.Net.Sockets;
using CargoWise.Common;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	public class SimpleImapServerHandler : SimpleMailServerHandler
	{
		bool _waitingAuthenticate;

		public SimpleImapServerHandler(TcpClient client)
		{
			this.client = client;
		}

		string receiver;
		string command;

		public override bool IsQuitCommand(string message)
		{
			var commands = message.Split(' ');
			if (commands.Length >= 2)
			{
				receiver = commands[0];
				command = commands[1];
			}

			return commands.IsNullOrEmpty();
		}

		public override void HandleCommand(string message)
		{
			if (_waitingAuthenticate)
			{
				if (SimpleMailServer.UserPasswordBase64.Contains(message.Trim()))
				{
					Write(receiver + " OK authentication successful");
				}
				else
				{
					Write(receiver + " NO [AUTHENTICATIONFAILED] failed");
				}
				_waitingAuthenticate = false;
				return;
			}

			if (command.StartsWith("CAPABILITY"))
			{
				Write("* CAPABILITY IMAP4REV1 MAILBOX-REFERRALS LOGIN-REFERRALS AUTH=PLAIN");
				Write(receiver + " OK CAPABILITY Completed");
			}
			else if (command.StartsWith("AUTHENTICATE"))
			{
				var param = SplitAndGet(message, 2, out var isEmpty);
				if (param.Equals("PLAIN"))
				{
					_waitingAuthenticate = true;
					Write("+");
					return;
				}
			}
			else if (command.StartsWith("NOOP"))
			{
				Write("* OK - noop completed");
			}
			else if (command.StartsWith("LOGOUT"))
			{
				Write("* BYE logging out");
				Write($"{receiver} OK LOGOUT completed");
			}
			else if (command.StartsWith("LOGIN"))
			{
				var username = SplitAndGet(message, 2, out var isEmpty);
				var password = SplitAndGet(message, 3, out var isNull);
				if (password.StartsWith("\"") && password.EndsWith("\""))
				{
					password = password.Substring(1, password.Length - 2);
				}
				if (SimpleMailServer.UserPasswordPairs.ContainsKey(username) &&
					SimpleMailServer.UserPasswordPairs[username].Equals(password))
				{
					Write($"{receiver} OK - login completed, now in authenticated state");
				}
				else
				{
					Write($"{receiver} NO [AUTHENTICATIONFAILED] - login failed");
				}
			}
			else if (command.StartsWith("SELECT"))
			{
				var param = SplitAndGet(message, 2, out var isEmpty);
				if (param.Equals("INBOX"))
				{
					Write("* FLAGS (\\Answered \\Flagged \\Seen \\Deleted)");
					Write($"* {SimpleSmtpServer.GetEmailCount().count} EXISTS");
					Write($"{receiver} OK [READ-WRITE] select completed");
				}
			}
			else if (command.StartsWith("EXAMINE"))
			{
				Write($"{receiver} OK [READ-ONLY] EXAMINE completed");
			}
			else if (command.StartsWith("CREATE"))
			{
				Write("* OK - create completed");
			}
			else if (command.StartsWith("DELETE"))
			{
				Write($"{receiver} OK - delete completed");
			}
			else if (command.StartsWith("LIST"))
			{
				Write("* LIST () \".\" \"INBOX\" ");
				Write($"{receiver} OK LIST Completed");
			}
			else if (command.StartsWith("FETCH"))
			{
				var param = SplitAndGet(message, 2, out var isEmpty);
				if (!isEmpty && param.Equals("1:*"))
				{
					var emailDictionary = SimpleSmtpServer.GetEmailDictionary();
					foreach (var email in emailDictionary)
					{
						Write($"* {email.Key} FETCH (UID {email.Key} RFC822.SIZE {email.Value.Data.Length})");
					}
					Write($"{receiver} OK FETCH Completed");
				}
			}
			else if (command.StartsWith("EXPUNGE"))
			{
				SimpleSmtpServer.ClearAll();
				Write($"{receiver} OK EXPUNGE completed");
			}
			else if (command.StartsWith("STORE"))
			{
				Write($"{receiver} OK STORE completed");
			}
			else if (command.StartsWith("CLOSE"))
			{
				Write($"{receiver} OK CLOSE completed");
			}

			receiver = string.Empty;
			command = string.Empty;
		}
	}
}
