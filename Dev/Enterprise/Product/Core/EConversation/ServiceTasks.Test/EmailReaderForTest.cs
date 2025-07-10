using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Enterprise.EConversation.ServiceTasks;
using MailKit;

namespace Enterprise.EConversation.Testing
{
	public class EmailReaderForTest : IEmailReader
	{
		public const string TestServer = "TestServer";
		public const string TestMailbox = "TestMailbox";

		public bool ThrowServerException;
		public bool ThrowServerExceptionOnSecondEmail;
		public bool ThrowSocketException;
		public bool ThrowTimeoutException;
		public string ExceptionSource = nameof(MailKit);

		readonly List<string> debugEmails = new();

		public event IEmailReader.EmailDownloadedDelegate EmailDownloaded;

		public string Server => TestServer;

		public string Mailbox => TestMailbox;

		public void Start()
		{
			if (ThrowServerException)
			{
				var exception = new MessageNotFoundException("Message unavailable") { Source = ExceptionSource };
				throw exception;
			}

			if (ThrowSocketException)
			{
				var exception = new Exception("Error",
					new System.IO.IOException("Error", new SocketException(10053)))
				{ Source = ExceptionSource };
				throw exception;
			}

			if (ThrowTimeoutException)
			{
				var exception = new Exception("Error",
					new System.IO.IOException("Error", new SocketException(10060)))
				{ Source = ExceptionSource };
				throw exception;
			}

			var continueDownloading = true;

			for (var i = 1; i <= debugEmails.Count; i++)
			{
				if (ThrowServerExceptionOnSecondEmail && i == 2)
				{
					var exception = new ServiceNotConnectedException("Server disconnected") { Source = ExceptionSource };
					throw exception;
				}

				EmailDownloaded?.Invoke(i.ToString(), debugEmails[i - 1], ref continueDownloading);

				if (!continueDownloading)
				{
					return;
				}
			}
		}

		public Email GetRawEmailFromString(string eml)
		{
			return new EmailForTest(Encoding.UTF8.GetBytes(eml));
		}

		public void DeleteProcessedMails(IEnumerable<string> processedIds)
		{
			debugEmails?.Clear();
		}

		public void AddEmailBundle(params Email[] emails)
		{
			debugEmails.AddRange(emails.Select(e => e == null ? "" : e.GetEml()));
		}

		public void AddEmailBundle(params string[] emails)
		{
			debugEmails.AddRange(emails);
		}

		public void Dispose()
		{
		}
	}
}
