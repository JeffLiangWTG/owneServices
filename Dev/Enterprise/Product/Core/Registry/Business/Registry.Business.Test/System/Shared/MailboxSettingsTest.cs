using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Lists;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	public class MailboxSettingsTest : TransactionedTestCase
	{
		public void TestMailBoxSettings_Default()
		{
			var testMailBoxSettings = new MailboxSettings("TestPrefix", (NoResString)"Test Category", (x, y) => { return y(); });
			AssertMailbox(testMailBoxSettings, "TestPrefix", "Test Category", RegistryOptions.Default | RegistryOptions.PreserveTestValue);
		}

		public void TestMailBoxSettings_RegistryOptions()
		{
			var testMailBoxSettings = new MailboxSettings("TestPrefix", (NoResString)"Test Category", (x, y) => { return y(); }, RegistryOptions.IsOnlyForSupport);
			AssertMailbox(testMailBoxSettings, "TestPrefix", "Test Category", RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue);
		}

		public void TestIsValid()
		{
			var testMailBoxSettings = new MailboxSettings("TestPrefix", (NoResString)"Test Category", (x, y) => { return y(); });
			var originalMailServer = testMailBoxSettings.Server;
			var originalUserName = testMailBoxSettings.UserName;

			try
			{
				Assert(!testMailBoxSettings.IsValid(out var errorMessages));
				AssertEquals(2, errorMessages.Count);

				testMailBoxSettings.Server = "imap.mail.com";
				Assert(!testMailBoxSettings.IsValid(out errorMessages));
				AssertEquals(1, errorMessages.Count);

				testMailBoxSettings.UserName = "test@mail.com";
				Assert(testMailBoxSettings.IsValid(out errorMessages));
				AssertEquals(0, errorMessages.Count);
			}
			finally
			{
				testMailBoxSettings.Server = originalMailServer;
				testMailBoxSettings.UserName = originalUserName;
			}
		}

		public void TestIsValid_OAuth2_Ms365()
		{
			var testMailBoxSettings = new OAuthMailboxSettings("TestPrefix", (NoResString)"Test Category", (x, y) => { return y(); });
			var originalApplicationId = testMailBoxSettings.ApplicationId;
			var originalTenantId = testMailBoxSettings.TenantId;

			try
			{
				testMailBoxSettings.OAuth2Type = OAuth2TypeList.Codes.Ms365;
				testMailBoxSettings.Server = "outlook.com";
				Assert(!testMailBoxSettings.IsValid(out var errorMessages));
				AssertEquals(2, errorMessages.Count);

				testMailBoxSettings.ApplicationId = "App-ID";
				Assert(!testMailBoxSettings.IsValid(out errorMessages));
				AssertEquals(1, errorMessages.Count);

				testMailBoxSettings.TenantId = "Tenant-ID";
				Assert(testMailBoxSettings.IsValid(out errorMessages));
				AssertEquals(0, errorMessages.Count);
			}
			finally
			{
				testMailBoxSettings.ApplicationId = originalApplicationId;
				testMailBoxSettings.TenantId = originalTenantId;
			}
		}

		public void TestIsValid_OAuth2_Gmail()
		{
			var testMailBoxSettings = new OAuthMailboxSettings("TestPrefix", (NoResString)"Test Category", (x, y) => { return y(); });
			var originalDelegateMail = testMailBoxSettings.DelegatedMail;
			var originalServiceAccountKey = testMailBoxSettings.ServiceAccountKey;

			try
			{
				testMailBoxSettings.OAuth2Type = OAuth2TypeList.Codes.GMail;
				testMailBoxSettings.Server = "gmail.com";
				Assert(!testMailBoxSettings.IsValid(out var errorMessages));
				AssertEquals(2, errorMessages.Count);

				testMailBoxSettings.DelegatedMail = "info@example.com";
				Assert(!testMailBoxSettings.IsValid(out errorMessages));
				AssertEquals(1, errorMessages.Count);

				testMailBoxSettings.ServiceAccountKey = new GmailOAuth2JsonFile
				{
					FileName = "test.json",
					JsonText = "ServiceAccountKeyJson"
				};
				Assert(testMailBoxSettings.IsValid(out errorMessages));
				AssertEquals(0, errorMessages.Count);
			}
			finally
			{
				testMailBoxSettings.DelegatedMail = originalDelegateMail;
				testMailBoxSettings.ServiceAccountKey = originalServiceAccountKey;
			}
		}

		#region Implementation

		static void AssertMailRetrievalProtocol(MailboxSettings mailbox, string namePrefix, string category, RegistryOptions registryOptions)
		{
			var registryItem = mailbox.GetAllItems().Single(x => x.Name == namePrefix + "MailRetrievalProtocol");
			AssertEquals(category, registryItem.Category);
			AssertEquals("Mail Retrieval Protocol", registryItem.Caption);
			AssertEquals("Protocol used to retrieve incoming mail", registryItem.Hint);
			AssertEquals(RegistryStorageFlags.System, registryItem.Storage);
			AssertEquals(MailRetrievalProtocols.POP3, registryItem.DefaultValue);
			AssertEquals(registryOptions, registryItem.Options);
		}

		static void AssertMailServer(MailboxSettings mailbox, string namePrefix, string category, RegistryOptions registryOptions)
		{
			var registryItem = mailbox.GetAllItems().Single(x => x.Name == namePrefix + "MailServer");
			AssertEquals(category, registryItem.Category);
			AssertEquals("Mail Server", registryItem.Caption);
			AssertEquals("Mail Server", registryItem.Hint);
			AssertEquals("", registryItem.DefaultValue);
			AssertEquals(registryOptions, registryItem.Options);
		}

		static void AssertMailServerPort(MailboxSettings mailbox, string namePrefix, string category, RegistryOptions registryOptions)
		{
			var registryItem = mailbox.GetAllItems().Single(x => x.Name == namePrefix + "MailServerPort");
			AssertEquals(category, registryItem.Category);
			AssertEquals("Mail Server Port", registryItem.Caption);
			AssertEquals("Mail Server Port", registryItem.Hint);
			AssertEquals(110, registryItem.DefaultValue);
			AssertEquals(registryOptions, registryItem.Options);
		}

		static void AssertMailboxUserName(MailboxSettings mailbox, string namePrefix, string category, RegistryOptions registryOptions)
		{
			var registryItem = mailbox.GetAllItems().Single(x => x.Name == namePrefix + "MailboxUserName");
			AssertEquals(category, registryItem.Category);
			AssertEquals("Mailbox User Name", registryItem.Caption);
			AssertEquals("e.g. user@example.com.", registryItem.Hint);
			AssertEquals("", registryItem.DefaultValue);
			AssertEquals(registryOptions, registryItem.Options);
		}

		static void AssertMailboxPassword(MailboxSettings mailbox, string namePrefix, string category, RegistryOptions registryOptions)
		{
			var registryItem = mailbox.GetAllItems().Single(x => x.Name == namePrefix + "MailboxPassword");
			AssertEquals(category, registryItem.Category);
			AssertEquals("Mailbox Password", registryItem.Caption);
			AssertEquals("Mailbox Password", registryItem.Hint);
			AssertEquals("", registryItem.DefaultValue);
			AssertEquals(registryOptions, registryItem.Options);
		}

		static void AssertIMAPSecureConnectionType(MailboxSettings mailbox, string namePrefix, string category, RegistryOptions registryOptions)
		{
			var registryItem = mailbox.GetAllItems().Single(x => x.Name == namePrefix + "IMAPSecureConnection");
			AssertEquals(category + "/IMAP", registryItem.Category);
			AssertEquals("IMAP Server Secure Connection", registryItem.Caption);
			AssertEquals("The type of secure connection to use to connect to the IMAP mail server", registryItem.Hint);
			AssertEquals(RegistryStorageFlags.System, registryItem.Storage);
			AssertEquals(SecureConnectionTypes.TLS, registryItem.DefaultValue);
			AssertEquals(registryOptions, registryItem.Options);
		}

		static void AssertPOP3SecureConnectionType(MailboxSettings mailbox, string namePrefix, string category, RegistryOptions registryOptions)
		{
			var registryItem = mailbox.GetAllItems().Single(x => x.Name == namePrefix + "POP3SecureConnection");
			AssertEquals(category + "/POP3", registryItem.Category);
			AssertEquals("POP3 Server Secure Connection", registryItem.Caption);
			AssertEquals("The type of secure connection to use to connect to the POP3 mail server", registryItem.Hint);
			AssertEquals(RegistryStorageFlags.System, registryItem.Storage);
			AssertEquals(SecureConnectionTypes.None, registryItem.DefaultValue);
			AssertEquals(registryOptions, registryItem.Options);
		}

		static void AssertUseOAuth(MailboxSettings mailbox, string namePrefix, string category, RegistryOptions registryOptions)
		{
			var registryItem = mailbox.GetAllItems().Single(x => x.Name == namePrefix + "OAuth2Type");
			AssertEquals(category, registryItem.Category);
			AssertEquals("OAuth 2.0 authentication type", registryItem.Caption);
			AssertEquals("When a type is selected, OAuth 2.0 authentication will be used.", registryItem.Hint);
			AssertEquals(RegistryStorageFlags.System, registryItem.Storage);
			AssertEquals(string.Empty, registryItem.DefaultValue);
			AssertEquals(registryOptions, registryItem.Options);
		}

		static void AssertMs365OAuth2TenantId(MailboxSettings mailbox, string namePrefix, string category, RegistryOptions registryOptions)
		{
			var registryItem = mailbox.GetAllItems().Single(x => x.Name == namePrefix + "Ms365OAuth2TenantId");
			AssertEquals(category + "/OAuth 2.0/Microsoft 365", registryItem.Category);
			AssertEquals("Tenant ID", registryItem.Caption);
			AssertEquals("The Tenant ID is used to identify the organization when authenticating the user. If your account type is Single tenant, enter in the Tenant ID. When left blank, the common authority will be used.", registryItem.Hint);
			AssertEquals(RegistryStorageFlags.System, registryItem.Storage);
			AssertEquals("", registryItem.DefaultValue);
			AssertEquals(registryOptions | RegistryOptions.IsValueMandatory | RegistryOptions.IsPasswordVisibleForControllerUser, registryItem.Options);
		}

		static void AssertMs365ApplicationId(MailboxSettings mailbox, string namePrefix, string category, RegistryOptions registryOptions)
		{
			var registryItem = mailbox.GetAllItems().Single(x => x.Name == namePrefix + "Ms365ApplicationId");
			AssertEquals(category + "/OAuth 2.0/Microsoft 365", registryItem.Category);
			AssertEquals("Application ID", registryItem.Caption);
			AssertEquals("This is the Application ID registered in the Azure platform. It should be a unique identifier like 'acc8304d-88d3-4caa-8452-2be8061d7fba'.\r\n\r\nImportant: You are required to register your own Application ID under App registrations blade in Azure AD, then enter the ID in this registry item before using the OAuth 2.0 authentication.", registryItem.Hint);
			AssertEquals(RegistryStorageFlags.System, registryItem.Storage);
			AssertEquals("", registryItem.DefaultValue);
			AssertEquals(registryOptions | RegistryOptions.IsValueMandatory | RegistryOptions.IsPasswordVisibleForControllerUser, registryItem.Options);
		}

		static void AssertMs365AppSecret(MailboxSettings mailbox, string namePrefix, string category, RegistryOptions registryOptions)
		{
			var registryItem = mailbox.GetAllItems().Single(x => x.Name == namePrefix + "Ms365AppSecret");
			AssertEquals(category + "/OAuth 2.0/Microsoft 365", registryItem.Category);
			AssertEquals("Client Secret", registryItem.Caption);
			AssertEquals("This is the Client Secret that has been registered in the Azure platform.  The Client Secret is the value in the Value column of Certificates & secrets settings in Azure.  For added security, the Client Secret will be encrypted when the Registry is saved.\r\n\r\nImportant: Please consider the expiry period of the Client Secret in the Certificates & secrets page of your App registration in Azure as an expired Client Secret can lead to authentication issues.  Please refer to the Registering App in Microsoft Azure Technical Guide at https://myaccount.cargowise.com/Home/CargoWise/TechnicalGuides.aspx for further details on registering an App in Azure.", registryItem.Hint);
			AssertEquals(RegistryStorageFlags.System, registryItem.Storage);
			AssertEquals("", registryItem.DefaultValue);
			AssertEquals(registryOptions | RegistryOptions.IsValueMandatory | RegistryOptions.IsPasswordVisibleForControllerUser, registryItem.Options);
		}

		static void AssertUseGraphApi(MailboxSettings mailbox, string namePrefix, string category, RegistryOptions registryOptions)
		{
			var registryItem = mailbox.GetAllItems().Single(x => x.Name == namePrefix + "UseGraphApi");
			AssertEquals(category + "/OAuth 2.0/Microsoft 365", registryItem.Category);
			AssertEquals("Use Graph API", registryItem.Caption);
			AssertEquals("When enabled, Microsoft Graph API will be used.", registryItem.Hint);
			AssertEquals(RegistryStorageFlags.System, registryItem.Storage);
			AssertEquals(false, registryItem.DefaultValue);
			AssertEquals(registryOptions, registryItem.Options);
		}

		static void AssertMs365OAuth2Token(MailboxSettings mailbox, string namePrefix, string category, RegistryOptions registryOptions)
		{
			var registryItem = mailbox.GetAllItems().Single(x => x.Name == namePrefix + "Ms365OAuth2Token");
			AssertEquals(category + "/OAuth 2.0/Microsoft 365", registryItem.Category);
			AssertEquals("OAuth 2.0 Access Token", registryItem.Caption);
			AssertEquals("This setting stores the OAuth 2.0 Access Token that the Mail Service Task will use during authentication.\r\n\r\nClick on Grant Permissions to generate and store the OAuth 2.0 Access Token. If a token has already been generated, clicking on Grant Permissions will renew it. Click on the Clear button to clear the cached token.", registryItem.Hint);
			AssertEquals(RegistryStorageFlags.System, registryItem.Storage);
			AssertEquals(registryOptions, registryItem.Options);

			var defaultValue = registryItem.DefaultValue as Ms365OAuth2Token;
			AssertNotNull(defaultValue);
			AssertEquals(null, defaultValue.Identifier);
			AssertEquals(null, defaultValue.User);
			AssertEquals(null, defaultValue.Token);
		}

		static void AssertMs365OAuth2AppToken(MailboxSettings mailbox, string namePrefix)
		{
			var registryItem = mailbox.GetAllItems().Single(x => x.Name == namePrefix + "Ms365OAuth2AppToken");
			AssertEquals(string.Empty, registryItem.Category);
			AssertEquals(RegistryStorageFlags.System, registryItem.Storage);
			AssertEquals(null, registryItem.DefaultValue);
			AssertEquals(RegistryOptions.IsHidden, registryItem.Options);
		}
		static void AssertGmailDelegatedMail(OAuthMailboxSettings mailbox, string namePrefix, string category, RegistryOptions registryOptions)
		{
			var registryItem = mailbox.GetAllItems().Single(x => x.Name == namePrefix + "GmailDelegatedMail");
			AssertEquals(category + "/OAuth 2.0/Google Mail", registryItem.Category);
			AssertEquals(RegistryStorageFlags.System, registryItem.Storage);
			AssertEquals(string.Empty, registryItem.DefaultValue);
			AssertEquals(registryOptions | RegistryOptions.IsValueMandatory | RegistryOptions.IsPasswordVisibleForControllerUser, registryItem.Options);
		}

		static void AssertGmailServiceAccountKey(OAuthMailboxSettings mailbox, string namePrefix, string category, RegistryOptions registryOptions)
		{
			var registryItem = mailbox.GetAllItems().Single(x => x.Name == namePrefix + "GmailServiceAccountKey");
			AssertEquals(category + "/OAuth 2.0/Google Mail", registryItem.Category);
			AssertEquals(RegistryStorageFlags.System, registryItem.Storage);
			AssertEquals(registryOptions, registryItem.Options);

			var defaultValue = registryItem.DefaultValue as GmailOAuth2JsonFile;
			AssertEquals(null, defaultValue.FileName);
			AssertEquals(null, defaultValue.JsonText);
		}

		public static void AssertMailbox(MailboxSettings mailbox, string namePrefix, string category, RegistryOptions registryOptions)
		{
			AssertMailRetrievalProtocol(mailbox, namePrefix, category, registryOptions);
			AssertMailServer(mailbox, namePrefix, category, registryOptions);
			AssertMailServerPort(mailbox, namePrefix, category, registryOptions);
			AssertMailboxUserName(mailbox, namePrefix, category, registryOptions);
			AssertMailboxPassword(mailbox, namePrefix, category, registryOptions);
			AssertIMAPSecureConnectionType(mailbox, namePrefix, category, registryOptions);
			AssertPOP3SecureConnectionType(mailbox, namePrefix, category, registryOptions);
		}

		public static void AssertMailbox(OAuthMailboxSettings mailbox, string namePrefix, string category, RegistryOptions registryOptions)
		{
			AssertMailbox((MailboxSettings)mailbox, namePrefix, category, registryOptions);

			AssertUseOAuth(mailbox, namePrefix, category, registryOptions);
			AssertMs365OAuth2TenantId(mailbox, namePrefix, category, registryOptions);
			AssertMs365ApplicationId(mailbox, namePrefix, category, registryOptions);
			AssertMs365AppSecret(mailbox, namePrefix, category, registryOptions);
			AssertUseGraphApi(mailbox, namePrefix, category, registryOptions);
			AssertMs365OAuth2Token(mailbox, namePrefix, category, registryOptions);
			AssertMs365OAuth2AppToken(mailbox, namePrefix);
			AssertGmailDelegatedMail(mailbox, namePrefix, category, registryOptions);
			AssertGmailServiceAccountKey(mailbox, namePrefix, category, registryOptions);
		}

		#endregion
	}
}
