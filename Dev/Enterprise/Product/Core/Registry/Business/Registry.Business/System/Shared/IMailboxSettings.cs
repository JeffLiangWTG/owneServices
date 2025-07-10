using System.Collections.Generic;

namespace Enterprise.Registry.Business
{
	public interface IMailboxSettings
	{
		public string Server { get; }

		public int Port { get; }

		public string MailRetrievalProtocol { get; }

		public string UserName { get; }

		public string Password { get; }

		public string SecureConnectionType { get; }

		public bool IsValid(out ICollection<string> errorMessages);
	}
}
