using System;
using System.Collections.Generic;

namespace Enterprise.EConversation.ServiceTasks
{
	public interface IEmailReader : IDisposable
	{
		public delegate void EmailDownloadedDelegate(string id, string email, ref bool continueDownloading);
		event EmailDownloadedDelegate EmailDownloaded;

		string Server { get; }

		string Mailbox { get; }

		void DeleteProcessedMails(IEnumerable<string> processedIds);

		Email GetRawEmailFromString(string emailText);

		void Start();
	}
}
