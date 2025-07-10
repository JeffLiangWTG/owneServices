using System;
using Enterprise.Environment;
using Enterprise.MailManager.ExternalMailInterface.IMAP;
using Enterprise.MailManager.ExternalMailInterface.POP3;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Lists;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.MailManager.ExternalMailInterface.Testing
{
	public class MailDownloaderConfigurationTest : TestCase
	{
		public void TestDefaultSettings()
		{
			var config = MailServerConfiguration.Default;
			AssertEquals(Env.Registry.MailServer, config.Server);
			AssertEquals(Env.Registry.MailServerPort, config.Port);
			AssertEquals(Env.Registry.MailboxUserName, config.UserName);
			AssertEquals(Env.Registry.MailRetrievalProtocol, config.Protocol);
			AssertEquals(Env.Registry.UseOAuth2ForIncoming, config.OAuth2Type);
			AssertEquals(Env.Registry.Ms365OAuth2TenantId, config.TenantId);
			AssertEquals(Env.Registry.Ms365ApplicationIdForIncoming, config.ApplicationId);
			AssertEquals(Env.Registry.GmailDelegatedMailForIncoming, config.DelegatedMail);
			AssertEquals(Env.Registry.GmailServiceAccountKeyForIncoming.FileName, config.ServiceAccountKey.FileName);
			AssertEquals(Env.Registry.GmailServiceAccountKeyForIncoming.JsonText, config.ServiceAccountKey.JsonText);

			var expectedConnectionType = Env.Registry.MailRetrievalProtocol == MailRetrievalProtocols.POP3
				? Env.Registry.POP3SecureConnection
				: Env.Registry.IMAPSecureConnection;
			AssertEquals(expectedConnectionType, config.SecureConnectionType);
		}

		public void TestMailboxSettingsConstructor()
		{
			var mailboxSettings = new Mock<IMailboxSettings>();
			mailboxSettings.Setup(x => x.Server).Returns("Server");
			mailboxSettings.Setup(x => x.Port).Returns(123);
			mailboxSettings.Setup(x => x.UserName).Returns("User");
			mailboxSettings.Setup(x => x.Password).Returns("Password");
			mailboxSettings.Setup(x => x.MailRetrievalProtocol).Returns(MailRetrievalProtocols.POP3);
			mailboxSettings.Setup(x => x.SecureConnectionType).Returns(ZArchitecture.Core.SecureConnectionTypes.None);

			var config = new MailServerConfiguration(mailboxSettings.Object);
			AssertEquals("Server", config.Server);
			AssertEquals(123, config.Port);
			AssertEquals("User", config.UserName);
			AssertEquals(MailRetrievalProtocols.POP3, config.Protocol);
			AssertEquals(ZArchitecture.Core.SecureConnectionTypes.None, config.SecureConnectionType);
			AssertEquals(false, config.UseOAuth2);

			var protocol = config.GetMailProtocol();
			AssertEquals(typeof(MailKitPop3), protocol.GetType());
		}

		public void TestOAuth2MailboxSettingsConstructor()
		{
			var mailboxSettings = new Mock<IOAuth2MailboxSettings>();
			mailboxSettings.Setup(x => x.Server).Returns("Server");
			mailboxSettings.Setup(x => x.Port).Returns(123);
			mailboxSettings.Setup(x => x.UserName).Returns("User");
			mailboxSettings.Setup(x => x.Password).Returns("Password");
			mailboxSettings.Setup(x => x.MailRetrievalProtocol).Returns(MailRetrievalProtocols.POP3);
			mailboxSettings.Setup(x => x.SecureConnectionType).Returns(ZArchitecture.Core.SecureConnectionTypes.None);
			mailboxSettings.Setup(x => x.OAuth2Type).Returns(OAuth2TypeList.Codes.Ms365);
			mailboxSettings.Setup(x => x.UseGraphApi).Returns(false);
			mailboxSettings.Setup(x => x.TenantId).Returns("Tenant");
			mailboxSettings.Setup(x => x.ApplicationId).Returns("Application");
			mailboxSettings.Setup(x => x.GetMs365UserToken()).Returns(new Ms365OAuth2Token());

			var config = new MailServerConfiguration(mailboxSettings.Object);
			AssertEquals("Server", config.Server);
			AssertEquals(123, config.Port);
			AssertEquals("User", config.UserName);
			AssertEquals(MailRetrievalProtocols.POP3, config.Protocol);
			AssertEquals(ZArchitecture.Core.SecureConnectionTypes.None, config.SecureConnectionType);
			AssertEquals(true, config.UseOAuth2);
			AssertEquals("Tenant", config.TenantId);
			AssertEquals("Application", config.ApplicationId);

			var protocol = config.GetMailProtocol();
			AssertEquals(typeof(MailKitPop3), protocol.GetType());
		}

		public void TestPop3MailProtocol()
		{
			var config = new MailServerConfiguration("Server", 123, "User", "Password", MailRetrievalProtocols.POP3, ZArchitecture.Core.SecureConnectionTypes.TLS);
			AssertEquals("Server", config.Server);
			AssertEquals(123, config.Port);
			AssertEquals("User", config.UserName);
			AssertEquals(MailRetrievalProtocols.POP3, config.Protocol);
			AssertEquals(ZArchitecture.Core.SecureConnectionTypes.TLS, config.SecureConnectionType);
			AssertEquals(false, config.UseOAuth2);

			var protocol = config.GetMailProtocol();
			AssertEquals(typeof(MailKitPop3), protocol.GetType());
		}

		public void TestImapMailProtocol()
		{
			var config = new MailServerConfiguration("Server", 123, "User", "Password", MailRetrievalProtocols.IMAP, ZArchitecture.Core.SecureConnectionTypes.SSL);
			AssertEquals("Server", config.Server);
			AssertEquals(123, config.Port);
			AssertEquals("User", config.UserName);
			AssertEquals(MailRetrievalProtocols.IMAP, config.Protocol);
			AssertEquals(ZArchitecture.Core.SecureConnectionTypes.SSL, config.SecureConnectionType);
			AssertEquals(false, config.UseOAuth2);

			var protocol = config.GetMailProtocol();
			AssertEquals(typeof(MailKitImap), protocol.GetType());
		}

		public void TestPop3MailProtocol_OAuth()
		{
			var token = new Ms365OAuth2Token { Identifier = "TokenIdentifier", Token = [4, 5, 6], User = "TokenUser" };
			var appToken = new byte[] { 1, 2, 3 };
			var config = new MailServerConfiguration(
				"Server",
				123,
				"User",
				"Password",
				MailRetrievalProtocols.POP3,
				ZArchitecture.Core.SecureConnectionTypes.SSL,
				OAuth2TypeList.Codes.Ms365,
				"Tenant",
				"Application",
				"AppSecret",
				() => token,
				() => appToken,
				mockSaveTokenAction.Object,
				false,
				null,
				null);
			AssertEquals("Server", config.Server);
			AssertEquals(123, config.Port);
			AssertEquals("User", config.UserName);
			AssertEquals(MailRetrievalProtocols.POP3, config.Protocol);
			AssertEquals(ZArchitecture.Core.SecureConnectionTypes.SSL, config.SecureConnectionType);
			AssertEquals(true, config.UseOAuth2);
			AssertEquals("Tenant", config.TenantId);
			AssertEquals("Application", config.ApplicationId);

			var protocol = config.GetMailProtocol();
			AssertEquals(typeof(MailKitPop3), protocol.GetType());

			var oAuthConfig = (Ms365OAuth2Configuration)config.GetOAuth2Configuration();
			AssertEquals("Tenant", oAuthConfig.TenantId);
			AssertEquals("Application", oAuthConfig.ApplicationId);
			AssertEquals(Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_OutLook, oAuthConfig.PermissionType);
			AssertEquals(token.Token, oAuthConfig.CachedToken);
			AssertEquals(token.Identifier, oAuthConfig.Identifier);
			AssertEquals(null, oAuthConfig.ClientSecret);
			AssertEquals(false, oAuthConfig.ShouldAcquireTokenInteractive);

			var testToken = new byte[] { 9, 9, 9 };
			oAuthConfig.TokenSaveAction(testToken);
			mockSaveTokenAction.Verify(x => x(true, testToken), Times.Once);
		}

		public void TestImapMailProtocol_OAuth()
		{
			var token = new Ms365OAuth2Token();
			var config = new MailServerConfiguration(
				"Server",
				123,
				"User",
				"Password",
				MailRetrievalProtocols.IMAP,
				ZArchitecture.Core.SecureConnectionTypes.TLS,
				OAuth2TypeList.Codes.Ms365,
				"Tenant",
				"Application",
				"AppSecret",
				() => token,
				null,
				mockSaveTokenAction.Object,
				false,
				null,
				null);
			AssertEquals("Server", config.Server);
			AssertEquals(123, config.Port);
			AssertEquals("User", config.UserName);
			AssertEquals(MailRetrievalProtocols.IMAP, config.Protocol);
			AssertEquals(ZArchitecture.Core.SecureConnectionTypes.TLS, config.SecureConnectionType);
			AssertEquals(true, config.UseOAuth2);
			AssertEquals("Tenant", config.TenantId);
			AssertEquals("Application", config.ApplicationId);

			var protocol = config.GetMailProtocol();
			AssertEquals(typeof(MailKitImap), protocol.GetType());

			var oAuthConfig = (Ms365OAuth2Configuration)config.GetOAuth2Configuration();
			AssertEquals("Tenant", oAuthConfig.TenantId);
			AssertEquals("Application", oAuthConfig.ApplicationId);
			AssertEquals(Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_OutLook, oAuthConfig.PermissionType);
			AssertEquals(token.Token, oAuthConfig.CachedToken);
			AssertEquals(token.Identifier, oAuthConfig.Identifier);
			AssertEquals(null, oAuthConfig.ClientSecret);
			AssertEquals(false, oAuthConfig.ShouldAcquireTokenInteractive);

			var testToken = new byte[] { 9, 9, 9 };
			oAuthConfig.TokenSaveAction(testToken);
			mockSaveTokenAction.Verify(x => x(true, testToken), Times.Once);
		}

		public void TestOAuthConfig_GraphAPI_UserToken()
		{
			var token = new Ms365OAuth2Token { Identifier = "TokenIdentifier", Token = [4, 5, 6], User = "TokenUser" };
			var config = new MailServerConfiguration(
				"Server",
				123,
				"User",
				"Password",
				MailRetrievalProtocols.IMAP,
				ZArchitecture.Core.SecureConnectionTypes.SSL,
				OAuth2TypeList.Codes.Ms365,
				"Tenant",
				"Application",
				"AppSecret",
				() => token,
				() => null,
				mockSaveTokenAction.Object,
				true,
				null,
				null);

			var delegateConfig = (Ms365OAuth2Configuration)config.GetOAuth2Configuration();
			AssertEquals("Tenant", delegateConfig.TenantId);
			AssertEquals("Application", delegateConfig.ApplicationId);
			AssertEquals(Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_GraphAPI, delegateConfig.PermissionType);
			AssertEquals(token.Token, delegateConfig.CachedToken);
			AssertEquals(token.Identifier, delegateConfig.Identifier);
			AssertEquals(null, delegateConfig.ClientSecret);
			AssertEquals(false, delegateConfig.ShouldAcquireTokenInteractive);

			var testToken1 = new byte[] { 9, 9, 9 };
			delegateConfig.TokenSaveAction(testToken1);
			mockSaveTokenAction.Verify(x => x(true, testToken1), Times.Once);
			mockSaveTokenAction.Reset();
		}

		public void TestOAuthConfig_GraphAPI_AppToken()
		{
			var appToken = new byte[] { 2, 3, 4 };
			var config = new MailServerConfiguration(
				"Server",
				123,
				"User",
				"Password",
				MailRetrievalProtocols.IMAP,
				ZArchitecture.Core.SecureConnectionTypes.SSL,
				OAuth2TypeList.Codes.Ms365,
				"Tenant",
				"Application",
				"AppSecret",
				() => null,
				() => appToken,
				mockSaveTokenAction.Object,
				true,
				null,
				null);

			var appConfig = (Ms365OAuth2Configuration)config.GetOAuth2Configuration();
			AssertEquals("Tenant", appConfig.TenantId);
			AssertEquals("Application", appConfig.ApplicationId);
			AssertEquals(Ms365OAuth2Configuration.Ms365OAuth2PermissionType.ApplicationPermission_GraphAPI, appConfig.PermissionType);
			AssertEquals(appToken, appConfig.CachedToken);
			AssertEquals(null, appConfig.Identifier);
			AssertEquals("AppSecret", appConfig.ClientSecret);
			AssertEquals(false, appConfig.ShouldAcquireTokenInteractive);

			var testToken2 = new byte[] { 9, 9, 9 };
			appConfig.TokenSaveAction(testToken2);
			mockSaveTokenAction.Verify(x => x(false, testToken2), Times.Once);
			mockSaveTokenAction.Reset();
		}

		public void TestGetMailProtocolWithMissingConfig()
		{
			var config = new MailServerConfiguration(string.Empty, -123, "", "", MailRetrievalProtocols.POP3, ZArchitecture.Core.SecureConnectionTypes.SSL);
			AssertExceptionThrown<InvalidOperationException>(
				"Failed to create mail protocol due to missing configuration values: Server, Port, UserName",
				() => config.GetMailProtocol());
		}

		protected override void SetUp()
		{
			base.SetUp();
			mockSaveTokenAction = new Mock<MailServerConfiguration.SaveTokenAction>();
		}

		Mock<MailServerConfiguration.SaveTokenAction> mockSaveTokenAction;
	}
}
