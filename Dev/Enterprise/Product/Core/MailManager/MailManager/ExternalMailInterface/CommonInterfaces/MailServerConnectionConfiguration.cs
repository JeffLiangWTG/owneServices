namespace Enterprise.MailManager.ExternalMailInterface
{
	public class MailServerConnectionConfiguration
	{
		public MailServerConnectionConfiguration(string server, int port, string secureConnectionType)
			: this(server, port, SecureConnectionTypeLookup.FromRegistryValue(secureConnectionType))
		{
		}

		public MailServerConnectionConfiguration(string server, int port, SecureConnectionTypes secureConnectionType)
			: this(server, port)
		{
			SecureConnectionType = secureConnectionType;
		}

		MailServerConnectionConfiguration(string server, int port)
		{
			Server = server;
			Port = port;
		}

		public string Server { get; }
		public int Port { get; }
		public SecureConnectionTypes SecureConnectionType { get; }
	}
}
