using System;
using System.Linq;
using Enterprise.Environment;
using Enterprise.Registry.Business;
#if NETFRAMEWORK
using Enterprise.ZArchitecture.Core;
#endif
using MimeKit;

namespace Enterprise.MailManager.ExternalMailInterface
{
	public class SmtpConfiguration : MailServerConnectionConfiguration
	{
		public SmtpConfiguration(string server, int port, string secureConnectionType, string ehloDomain = null)
			: base(server, port, SecureConnectionTypeLookup.FromRegistryValue(secureConnectionType))
		{
			EhloDomain = ehloDomain ?? Env.Registry.SMTPEhloDomain;
		}

		public string EhloDomain { get; private set; }

		internal static (SmtpConfiguration smtpConfiguration, UserPasswordAuthConfiguration userPasswordAuthConfiguration, string senderAddress) GetSecondarySmtpConfiguration(string from)
		{
			SmtpConfiguration smtpConfiguration = null;
			UserPasswordAuthConfiguration userPasswordAuthConfiguration = null;
			string senderAddress = null;

			var secondarySMTPServer = GetSecondarySmtpServer(from);
			if (secondarySMTPServer != null)
			{
				smtpConfiguration = new SmtpConfiguration(
					server: secondarySMTPServer.SMTPServer,
					port: secondarySMTPServer.SMTPPort,
					secureConnectionType: secondarySMTPServer.SMTPSecureConnection,
					ehloDomain: Env.Registry.SMTPEhloDomain);

				userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(userName: secondarySMTPServer.SMTPUsername, password: secondarySMTPServer.SMTPPassword);
				senderAddress = secondarySMTPServer.SMTPSenderAddress;
			}

			return (smtpConfiguration, userPasswordAuthConfiguration, senderAddress);
		}

		public static SecondarySMTPServer GetSecondarySmtpServer(string from)
		{
			SecondarySMTPServer secondarySMTPServer = null;
			if (!string.IsNullOrEmpty(from))
			{
				var fromAddress = string.Empty;
				var domain = string.Empty;
				InternetAddressList.TryParse(from, out var fromAddresses);
				if (fromAddresses?.Count > 0)
				{
					var mailBoxAddress = fromAddresses.OfType<MailboxAddress>().FirstOrDefault();
					fromAddress = mailBoxAddress?.Address;
					domain = mailBoxAddress?.Domain.ToLower();
				}

				var secondarySMTPServers = PhysicalServerDataRegistry.Instance.SecondarySMTPServers.Value.OfType<SecondarySMTPServer>().ToList();
				if (!string.IsNullOrEmpty(fromAddress))
				{
					secondarySMTPServer = secondarySMTPServers.FirstOrDefault(s => fromAddress.Equals(s.SMTPSenderAddress, StringComparison.OrdinalIgnoreCase));
				}

				if (secondarySMTPServer == null && !string.IsNullOrEmpty(domain))
				{
					secondarySMTPServer = secondarySMTPServers.FirstOrDefault(s => $", {s.SupportedDomains},".Contains($", {domain},", StringComparison.OrdinalIgnoreCase));
				}
			}

			return secondarySMTPServer;
		}
	}
}
