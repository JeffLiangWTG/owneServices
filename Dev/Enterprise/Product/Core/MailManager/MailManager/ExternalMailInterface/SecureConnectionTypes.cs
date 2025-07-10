using MailKit.Security;

namespace Enterprise.MailManager.ExternalMailInterface
{
	public enum SecureConnectionTypes
	{
		None,
		SSL,
		TLS,
		Auto
	}

	public static class SecureSocketOptionsLookup
	{
		public static SecureSocketOptions FromSecureConnectionTypes(SecureConnectionTypes connectionTypes)
		{
			switch (connectionTypes)
			{
				case SecureConnectionTypes.SSL:
					return SecureSocketOptions.SslOnConnect;
				case SecureConnectionTypes.TLS:
					return SecureSocketOptions.StartTls;
				case SecureConnectionTypes.None:
					return SecureSocketOptions.None;
				case SecureConnectionTypes.Auto:
					return SecureSocketOptions.Auto;
				default:
					return SecureSocketOptions.Auto;
			}
		}
	}

	public static class SecureConnectionTypeLookup
	{
		public static SecureConnectionTypes FromRegistryValue(string value)
		{
			switch (value)
			{
				case Enterprise.ZArchitecture.Core.SecureConnectionTypes.SSL:
					return SecureConnectionTypes.SSL;
				case Enterprise.ZArchitecture.Core.SecureConnectionTypes.TLS:
					return SecureConnectionTypes.TLS;
				default:
					return SecureConnectionTypes.None;
			}
		}
	}
}
