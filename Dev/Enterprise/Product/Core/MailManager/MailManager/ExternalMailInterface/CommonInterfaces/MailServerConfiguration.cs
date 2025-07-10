using System;
using System.Collections.Generic;
using Enterprise.Environment;
using Enterprise.MailManager.ExternalMailInterface.CommonInterfaces;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Lists;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MailManager.ExternalMailInterface
{
	public class MailServerConfiguration : IMailServerConfiguration
	{
		public delegate void SaveTokenAction(bool isUserToken, byte[] bytes);

		public static MailServerConfiguration Default => new(
			Env.Registry.MailServer,
			Env.Registry.MailServerPort,
			Env.Registry.MailboxUserName,
			Env.Registry.MailboxPassword,
			Env.Registry.MailRetrievalProtocol,
			Env.Registry.MailRetrievalProtocol == MailRetrievalProtocols.POP3
				? Env.Registry.POP3SecureConnection
				: Env.Registry.IMAPSecureConnection,
			Env.Registry.UseOAuth2ForIncoming,
			Env.Registry.Ms365OAuth2TenantId,
			Env.Registry.Ms365ApplicationIdForIncoming,
			Env.Registry.Ms365AppSecretForIncoming,
			() => Env.Registry.Ms365OAuth2TokenForIncoming,
			() => Env.Registry.Ms365OAuth2AppTokenForIncoming,
			DefaultSaveTokenAction,
			Env.Registry.UseGraphApiForIncoming,
			Env.Registry.GmailDelegatedMailForIncoming,
			Env.Registry.GmailServiceAccountKeyForIncoming);

		public MailServerConfiguration(IMailboxSettings settings)
			: this(settings.Server, settings.Port, settings.UserName, settings.Password, settings.MailRetrievalProtocol, settings.SecureConnectionType, string.Empty, null, null, null, null, null, null, false, null, null)
		{
		}

		public MailServerConfiguration(IOAuth2MailboxSettings settings)
			: this(
				settings.Server,
				settings.Port,
				settings.UserName,
				settings.Password,
				settings.MailRetrievalProtocol,
				settings.SecureConnectionType,
				settings.OAuth2Type,
				settings.TenantId,
				settings.ApplicationId,
				settings.AppSecret,
				settings.GetMs365UserToken,
				settings.GetMs365AppToken,
				settings.SaveMs365OAuth2Token,
				settings.UseGraphApi,
				settings.DelegatedMail,
				settings.ServiceAccountKey)
		{
		}

		public MailServerConfiguration(string server, int port, string userName, string password, string protocol, string secureConnectionType)
			: this(server, port, userName, password, protocol, secureConnectionType, string.Empty, null, null, null, null, null, null, false, null, null)
		{
		}

		public MailServerConfiguration(
			string server,
			int port,
			string userName,
			string password,
			string protocol,
			string secureConnectionType,
			string oAuth2Type,
			string tenantId,
			string applicationId,
			string appSecret,
			Func<Ms365OAuth2Token> userTokenAccessor,
			Func<byte[]> appTokenAccessor,
			SaveTokenAction saveTokenAction,
			bool useGraphApi,
			string delegatedMail,
			GmailOAuth2JsonFile serviceAccountKey)
		{
			Server = server;
			Port = port;
			UserName = userName;
			Protocol = protocol;
			SecureConnectionType = secureConnectionType;
			OAuth2Type = oAuth2Type;
			UseOAuth2 = !string.IsNullOrEmpty(oAuth2Type);
			TenantId = tenantId;
			ApplicationId = applicationId;
			UseGraphApi = useGraphApi;
			DelegatedMail = delegatedMail;
			ServiceAccountKey = serviceAccountKey;

			this.password = password;
			this.appSecret = appSecret;
			this.userTokenAccessor = userTokenAccessor ?? (() => null);
			this.appTokenAccessor = appTokenAccessor;
			this.saveTokenAction = saveTokenAction;
		}

		public string Server { get; }

		public int Port { get; }

		public string Protocol { get; }

		public string UserName { get; }

		public string SecureConnectionType { get; }

		public string OAuth2Type { get; }

		public bool UseOAuth2 { get; }

		public string TenantId { get; }

		public string ApplicationId { get; }

		public bool UseGraphApi { get; }

		public string DelegatedMail { get; }

		public GmailOAuth2JsonFile ServiceAccountKey { get; }

		readonly string password;
		readonly string appSecret;
		readonly Func<Ms365OAuth2Token> userTokenAccessor;
		readonly Func<byte[]> appTokenAccessor;
		readonly SaveTokenAction saveTokenAction;

		public IMailProtocol GetMailProtocol(LogMessageHandler logMessageHandler = null, Action<string, Exception, string> reportErrorAction = null)
		{
			return UseOAuth2 ? GetOAuth2MailProtocol(logMessageHandler, reportErrorAction) : GetUserPasswordMailProtocol(logMessageHandler, reportErrorAction);
		}

		public IOAuth2Configuration GetOAuth2Configuration(LogMessageHandler logMessageHandler = null)
		{
			return OAuth2Type switch
			{
				OAuth2TypeList.Codes.Ms365 => GetMs365OAuth2Configuration(),
				OAuth2TypeList.Codes.GMail => GetGmailOAuth2Configuration(),
				_ => throw new InvalidOperationException($"{OAuth2Type} is not a valid OAuth 2.0 type")
			};
		}

		MailServerConnectionConfiguration GetMailServerConnectionConfiguration()
		{
			return new MailServerConnectionConfiguration(Server, Port, SecureConnectionType);
		}

		IMailProtocol GetUserPasswordMailProtocol(LogMessageHandler logMessageHandler = null, Action<string, Exception, string> reportErrorAction = null)
		{
			EnsureValidUserPasswordConfiguration();

			var userPasswordAuthConfiguration = new UserPasswordAuthConfiguration(UserName, password);
			return new MailProtocolFactory().GetMailProtocol(
				Protocol,
				GetMailServerConnectionConfiguration(),
				userPasswordAuthConfiguration,
				Env.Registry.UseMailKitPOP3AndIMAPProtocolLogging ? logMessageHandler : null,
				reportErrorAction);
		}

		IMailProtocol GetOAuth2MailProtocol(LogMessageHandler logMessageHandler = null, Action<string, Exception, string> reportErrorAction = null)
		{
			return new MailProtocolFactory().GetMailProtocol(
				Protocol,
				GetMailServerConnectionConfiguration(),
				GetOAuth2Configuration(),
				Env.Registry.UseMailKitPOP3AndIMAPProtocolLogging ? logMessageHandler : null,
				reportErrorAction);
		}

		Ms365OAuth2Configuration GetMs365OAuth2Configuration()
		{
			if (!UseGraphApi)
			{
				return GetMs365OAuth2ConfigurationForOutlook();
			}

			var userToken = userTokenAccessor();
			var permissionType = GraphUtils.UseUserToken(userToken?.Token)
				? Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_GraphAPI
				: Ms365OAuth2Configuration.Ms365OAuth2PermissionType.ApplicationPermission_GraphAPI;

			return GetMs365OAuth2ConfigurationForGraphApi(permissionType);
		}

		Ms365OAuth2Configuration GetMs365OAuth2ConfigurationForOutlook()
		{
			var userToken = userTokenAccessor();

			return new Ms365OAuth2Configuration(
				tenantId: TenantId,
				applicationId: ApplicationId,
				permissionType: Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_OutLook,
				cachedToken: userToken.Token,
				tokenSaveAction: bytes => saveTokenAction(true, bytes),
				identifier: userToken.Identifier);
		}

		Ms365OAuth2Configuration GetMs365OAuth2ConfigurationForGraphApi(Ms365OAuth2Configuration.Ms365OAuth2PermissionType permissionType)
		{
			var useUserToken = permissionType == Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_GraphAPI;
			var userToken = useUserToken ? userTokenAccessor() : null;
			var cachedToken = useUserToken ? userToken.Token : appTokenAccessor();
			var identifier = useUserToken ? userToken.Identifier : null;
			var clientSecret = useUserToken ? null : appSecret;

			var oAuth2Configuration = new Ms365OAuth2Configuration(
				tenantId: TenantId,
				applicationId: ApplicationId,
				permissionType: permissionType,
				cachedToken: cachedToken,
				tokenSaveAction: bytes => saveTokenAction(useUserToken, bytes),
				identifier: identifier,
				clientSecret: clientSecret);
			return oAuth2Configuration;
		}

		GmailOAuth2Configuration GetGmailOAuth2Configuration()
		{
			return new GmailOAuth2Configuration(DelegatedMail, ServiceAccountKey);
		}

		void EnsureValidUserPasswordConfiguration()
		{
			var offendingProperties = new List<string>();
			if (string.IsNullOrWhiteSpace(Server))
			{
				offendingProperties.Add(nameof(Server));
			}

			if (Port <= 0)
			{
				offendingProperties.Add(nameof(Port));
			}

			if (string.IsNullOrWhiteSpace(UserName))
			{
				offendingProperties.Add(nameof(UserName));
			}

			if (offendingProperties.Count > 0)
			{
				throw new InvalidOperationException(
					$"Failed to create mail protocol due to missing configuration values: {string.Join(", ", offendingProperties)}");
			}
		}

		static void DefaultSaveTokenAction(bool useUserToken, byte[] bytes)
		{
			if (bytes == null)
			{
				return;
			}

			if (useUserToken)
			{
				var token = Env.Registry.Ms365OAuth2TokenForIncoming;
				token.Token = bytes;
				Env.Registry.Ms365OAuth2TokenForIncoming = token;
			}
			else
			{
				Env.Registry.Ms365OAuth2AppTokenForIncoming = bytes;
			}
		}
	}
}
