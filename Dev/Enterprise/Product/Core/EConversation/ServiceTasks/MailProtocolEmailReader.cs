using System.Collections.Generic;
using System.Text;
using Enterprise.MailManager.ExternalMailInterface.CommonInterfaces;

namespace Enterprise.EConversation.ServiceTasks
{
	public sealed class MailProtocolEmailReader : IEmailReader
	{
		public event IEmailReader.EmailDownloadedDelegate EmailDownloaded;

		public MailProtocolEmailReader(string server, string mailbox, IMailProtocol protocol)
		{
			Server = server;
			Mailbox = mailbox;
			this.protocol = protocol;
		}

		readonly IMailProtocol protocol;
		bool isOpen;

		IMailProtocol MailProtocol
		{
			get
			{
				if (!isOpen)
				{
					protocol.Open();
					isOpen = true;
				}
				return protocol;
			}
		}

		public string Server { get; }

		public string Mailbox { get; }

		public void Start()
		{
			var continueDownloading = true;

			foreach (var id in MailProtocol.GetAllMessageIds())
			{
				var email = Encoding.UTF8.GetString(MailProtocol.GetMessageById(id));
				EmailDownloaded?.Invoke(id, email, ref continueDownloading);

				if (!continueDownloading)
				{
					return;
				}
			}
		}

		public Email GetRawEmailFromString(string eml)
		{
			return new Email(Encoding.UTF8.GetBytes(eml));
		}

		public void DeleteProcessedMails(IEnumerable<string> processedIds)
		{
			foreach (var id in processedIds)
			{
				MailProtocol.DeleteMessageById(id);
			}
		}

		public void Dispose()
		{
			protocol?.Dispose();
		}
	}
}
