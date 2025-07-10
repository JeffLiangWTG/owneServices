using CargoWise.Application;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MailManager.ExternalMailInterface
{
	internal class MailSenderProvider : IMailSenderProvider
	{
		public IMailSender GetSender(ILogger logger = null, string from = null)
		{
#if DEBUG
			if (Globals.IsTest)
			{
				var sender = ObjectFactory.Get<IMailSender>();
				if (sender != null)
				{
					return sender;
				}
			}
#endif

			var senderAddress = Env.Registry.MailboxEmailAddress;
			var (overridenSmtpConfiguration, overridenUserPasswordAuthConfiguration, overridenSenderAddress) = SmtpConfiguration.GetSecondarySmtpConfiguration(from);

			if (OAuth2AuthenticationManager.Ms365OutgoingEnabled && Env.Registry.UseGraphApiForOutgoing && overridenSmtpConfiguration == null)
			{
				return new GraphMailSender(GetOAuth2ConfigurationForGraphSender(), senderAddress, logger);
			}
			else
			{
				var useMailKitSMTPProtocolLogging = Env.Registry.UseVerboseProtocolLogging;

				MailKitMailSender sender = null;
				if (overridenSmtpConfiguration != null)
				{
					sender = new MailKitMailSender(
						smtpConfiguration: overridenSmtpConfiguration,
						userPasswordAuthConfiguration: overridenUserPasswordAuthConfiguration,
						senderAddress: overridenSenderAddress,
						logger: logger,
						useMailKitSMTPProtocolLogging: useMailKitSMTPProtocolLogging);
				}
				else
				{
					var smtpConfiguration = new SmtpConfiguration(
						server: Env.Registry.SMTPServer,
						port: Env.Registry.SMTPPort,
						secureConnectionType: Env.Registry.SMTPSecureConnection,
						ehloDomain: Env.Registry.SMTPEhloDomain);

					if (OAuth2AuthenticationManager.OutgoingEnabled)
					{
						sender = new MailKitMailSender(
							smtpConfiguration: smtpConfiguration,
							oAuth2Configuration: GetOAuth2ConfigurationForMailKitSender(),
							senderAddress: senderAddress,
							logger: logger,
							useMailKitSMTPProtocolLogging: useMailKitSMTPProtocolLogging);
					}
					else
					{
						var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(userName: Env.Registry.SMTPUsername, password: Env.Registry.SMTPPassword);
						sender = new MailKitMailSender(
							smtpConfiguration: smtpConfiguration,
							userPasswordAuthConfiguration: userPasswordAuthConfiguration,
							senderAddress: senderAddress,
							logger: logger,
							useMailKitSMTPProtocolLogging: useMailKitSMTPProtocolLogging);
					}
				}

				return sender;
			}
		}

		public ISmtpSender GetSmtpSender(SmtpConfiguration smtpConfiguration, UserPasswordAuthConfiguration userPasswordAuthConfiguration, string senderAddress = null, ILogger logger = null)
		{
#if DEBUG
			if (Globals.IsTest)
			{
				var sender = ObjectFactory.Get<ISmtpSender>();
				if (sender != null)
				{
					return sender;
				}
			}
#endif

			var useMailKitSMTPProtocolLogging = Env.Registry.UseVerboseProtocolLogging;
			return new MailKitMailSender(
				smtpConfiguration: smtpConfiguration,
				userPasswordAuthConfiguration: userPasswordAuthConfiguration,
				senderAddress: senderAddress,
				logger: logger,
				useMailKitSMTPProtocolLogging: useMailKitSMTPProtocolLogging);
		}

		IOAuth2Configuration GetOAuth2ConfigurationForMailKitSender()
		{
			return OAuth2AuthenticationManager.LoadOutgoingConfiguration();
		}

		Ms365OAuth2Configuration GetOAuth2ConfigurationForGraphSender()
		{
			var userToken = Env.Registry.Ms365OAuth2TokenForOutgoing;
			var useUserToken = userToken != null && GraphUtils.UseUserToken(userToken.Token);
			var permissionType = useUserToken ? Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_GraphAPI : Ms365OAuth2Configuration.Ms365OAuth2PermissionType.ApplicationPermission_GraphAPI;
			var cachedToken = useUserToken ? userToken.Token : Env.Registry.Ms365OAuth2AppTokenForOutgoing;
			var identifier = useUserToken ? userToken.Identifier : null;
			var clientSecret = useUserToken ? null : Env.Registry.Ms365AppSecretForOutgoing;

			return new Ms365OAuth2Configuration(
				tenantId: Env.Registry.Ms365OAuth2TenantId,
				applicationId: Env.Registry.Ms365ApplicationIdForOutgoing,
				permissionType: permissionType,
				cachedToken: cachedToken,
				tokenSaveAction: b => SaveRegisterToken(b, useUserToken),
				identifier: identifier,
				clientSecret: clientSecret);
		}

		void SaveRegisterToken(byte[] bytes, bool useUserToken)
		{
			if (bytes != null)
			{
				if (useUserToken)
				{
					var token = Env.Registry.Ms365OAuth2TokenForOutgoing;
					token.Token = bytes;
					Env.Registry.Ms365OAuth2TokenForOutgoing = token;
				}
				else
				{
					Env.Registry.Ms365OAuth2AppTokenForOutgoing = bytes;
				}
			}
		}
	}
}
