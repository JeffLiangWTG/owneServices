using System;
using System.Collections.Generic;

namespace Enterprise.MailManager.ExternalMailInterface.CommonInterfaces
{
	public interface IMailProtocol : IDisposable
	{
		void Open();
		ICollection<string> GetAllMessageIds();
		byte[] GetMessageById(string id);
		void DeleteMessageById(string id);
		long GetMessageSizeById(string id);
		void Close();
		long MessageCount { get; }

		byte[] GetMessageByMessageNumber(long messageNumber);
		void DeleteMessageByNumber(List<long> messageNumbers);

		void ReOpenIfNeeded();
	}
}
