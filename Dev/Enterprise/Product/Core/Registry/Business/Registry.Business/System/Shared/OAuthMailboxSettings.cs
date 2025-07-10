using System;
using System.Collections.Generic;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MailManager.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class OAuthMailboxSettings : MailboxSettings, IOAuth2MailboxSettings
	{
		public OAuthMailboxSettings(string namePrefix, MultilingualString mailBoxCategory, Func<string, CreateMailBoxItemDelegate, IRegistryItem> getMailBoxItem, RegistryOptions registryOptions = RegistryOptions.Default)
			: base(namePrefix, mailBoxCategory, getMailBoxItem, registryOptions)
		{
			m365Category = RegistryItemSet.CombineCategories(
				RegistryItemSet.CombineCategories(mailBoxCategory, ResString.GetMultilingualString("OAuth 2.0", "OAuth 2.0")),
				ResString.GetMultilingualString("a54d3ac4-5b8a-49c2-b381-ee8a73baf5a4", "Microsoft 365"));
			gMailCategory = RegistryItemSet.CombineCategories(
				RegistryItemSet.CombineCategories(mailBoxCategory, ResString.GetMultilingualString("OAuth 2.0", "OAuth 2.0")),
				ResString.GetMultilingualString("c4420765-45a6-4d88-a522-a4f88f380b76", "Google Mail"));
		}

		readonly MultilingualString m365Category;
		readonly MultilingualString gMailCategory;

		#region Registry items

		IRegistryItem OAuth2TypeRegistry
		{
			get
			{
				return getMailBoxItem(NamePrefix + "OAuth2Type",
					delegate
					{
						return new CodePairRegistryItem(
							name: NamePrefix + "OAuth2Type",
							category: mailBoxCategory,
							caption: ResString.GetMultilingualString("0348bb6d-5065-42f7-bc27-5d229ab59eda", "{0} authentication type", RawDataRegistry.oAuth),
							hint: ResString.GetMultilingualString("9d39077d-1fe7-4d00-9486-568b7b0a0dd5", "When a type is selected, {0} authentication will be used.", RawDataRegistry.oAuth),
							lookUpList: new CodeDescriptionPairListProvider(() => new OAuth2TypeList()),
							storage: RegistryStorageFlags.System,
							registryOptions);
					});
			}
		}

		IRegistryItem Ms365OAuth2TenantIdRegistry
		{
			get
			{
				return getMailBoxItem(NamePrefix + "Ms365OAuth2TenantId", () =>
				{
					var result = new StringRegistryItem(
						NamePrefix + "Ms365OAuth2TenantId",
						m365Category,
						ResString.GetMultilingualString("a478fef0-8bd9-4e8b-a6a3-228c99356009", "Tenant ID"),
						ResString.GetMultilingualString("d742b408-70bf-4e11-a3a5-bb5f817d74c4", "The Tenant ID is used to identify the organization when authenticating the user. If your account type is Single tenant, enter in the Tenant ID. When left blank, the common authority will be used."),
						new StringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.Password | TextEditorType.Guid),
						RegistryStorageFlags.System,
						registryOptions | RegistryOptions.IsValueMandatory | RegistryOptions.IsPasswordVisibleForControllerUser,
						string.Empty);

					return result;
				});
			}
		}

		IRegistryItem Ms365ApplicationIdRegistry
		{
			get
			{
				return getMailBoxItem(NamePrefix + "Ms365ApplicationId", () =>
				{
					var result = new StringRegistryItem(
						NamePrefix + "Ms365ApplicationId",
						m365Category,
						ResString.GetMultilingualString("de839e21-b207-4e9c-b7ee-1b2c3397cef5", "Application ID"),
						ResString.GetMultilingualString("a8124c76-e66b-4703-9147-50113ec671ee", @"This is the Application ID registered in the Azure platform. It should be a unique identifier like '{0}'.

Important: You are required to register your own Application ID under App registrations blade in Azure AD, then enter the ID in this registry item before using the {1} authentication.", "acc8304d-88d3-4caa-8452-2be8061d7fba", RawDataRegistry.oAuth),
						new StringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.Password | TextEditorType.Guid),
						RegistryStorageFlags.System,
						registryOptions | RegistryOptions.IsValueMandatory | RegistryOptions.IsPasswordVisibleForControllerUser,
						string.Empty);
					return result;
				});
			}
		}

		IRegistryItem Ms365AppSecretRegistry
		{
			get
			{
				return getMailBoxItem(NamePrefix + "Ms365AppSecret", () =>
				{
					var result = new StringRegistryItem(
						NamePrefix + "Ms365AppSecret",
						m365Category,
						ResString.GetMultilingualString("a0099470-e956-46c8-9f33-a06bba940910", "Client Secret"),
						ResString.GetMultilingualString("6c950f3c-b3b6-493e-b12f-e8033ed8cdf0", @"This is the Client Secret that has been registered in the Azure platform.  The Client Secret is the value in the Value column of Certificates & secrets settings in Azure.  For added security, the Client Secret will be encrypted when the Registry is saved.

Important: Please consider the expiry period of the Client Secret in the Certificates & secrets page of your App registration in Azure as an expired Client Secret can lead to authentication issues.  Please refer to the Registering App in Microsoft Azure Technical Guide at https://myaccount.cargowise.com/Home/CargoWise/TechnicalGuides.aspx for further details on registering an App in Azure."),
						new StringRegistryDataType(true),
						new TextRegistryEditorInfo(TextEditorType.Password),
						RegistryStorageFlags.System,
						registryOptions | RegistryOptions.IsValueMandatory | RegistryOptions.IsPasswordVisibleForControllerUser,
						string.Empty)
					{
						OnUpdateAction = (_, _, _, _) => Ms365OAuth2AppToken.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null)
					};
					return result;
				});
			}
		}

		IRegistryItem UseGraphApiRegistry
		{
			get
			{
				return getMailBoxItem(NamePrefix + "UseGraphApi", () =>
					new RawDataRegistry.PhysicalServerRegistryItem(
						NamePrefix + "UseGraphApi",
						m365Category,
						ResString.GetMultilingualString("065ed747-63a8-48e3-911e-6a51f89097be", "Use Graph API"),
						ResString.GetMultilingualString("d75c83b3-1162-4623-bf8e-c72ad3134616",
							"When enabled, Microsoft Graph API will be used."),
						RegistryDataTypes.BoolType,
						registryOptions,
						false));
			}
		}

		IRegistryItem Ms365OAuth2TokenRegistry
		{
			get
			{
				return getMailBoxItem(NamePrefix + "Ms365OAuth2Token", () =>
				{
					var useFallback = UseFallback;
					return new Ms365OAuth2TokenRegistryItem(
						NamePrefix + "Ms365OAuth2Token",
						m365Category,
						ResString.GetMultilingualString("903df599-cfa6-4da5-a0cc-d5bffb9e9ec9", "{0} Access Token",
							RawDataRegistry.oAuth),
						ResString.GetMultilingualString("e3868f96-f12b-4b47-8f66-2253bd30f98c",
							@"This setting stores the {0} Access Token that the Mail Service Task will use during authentication.

Click on Grant Permissions to generate and store the {0} Access Token. If a token has already been generated, clicking on Grant Permissions will renew it. Click on the Clear button to clear the cached token.",
							RawDataRegistry.oAuth),
						EmailType.Incoming,
						useFallback ? RawDataRegistry.Instance.Ms365OAuth2TenantId : (StringRegistryItem)Ms365OAuth2TenantIdRegistry,
						useFallback ? RawDataRegistry.Instance.Ms365ApplicationIdForIncoming : (StringRegistryItem)Ms365ApplicationIdRegistry,
						useFallback ? RawDataRegistry.Instance.UseGraphApiForIncoming : new BooleanRegistryItem(UseGraphApiRegistry),
						RegistryStorageFlags.System,
						registryOptions
					);
				});
			}
		}

		IRegistryItem Ms365OAuth2AppToken
		{
			get
			{
				return getMailBoxItem(NamePrefix + "Ms365OAuth2AppToken", () =>
					new BinaryRegistryItem(NamePrefix + "Ms365OAuth2AppToken", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsHidden));
			}
		}

		IRegistryItem GmailDelegatedMailRegistry
		{
			get
			{
				return getMailBoxItem(NamePrefix + "GmailDelegatedMail", () =>
				{
					var result = new StringRegistryItem(
						NamePrefix + "GmailDelegatedMail",
						gMailCategory,
						ResString.GetMultilingualString("8e3e1a85-2e05-4fb2-8759-45cb86deccd3", "User Email"),
						ResString.GetMultilingualString("c20d9f29-69ad-45d8-b64f-7d8bc2f4d466", "Input the mailbox which you want to access, it will be delegated by service account."),
						new StringRegistryDataType(true),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.System,
						 registryOptions | RegistryOptions.IsValueMandatory | RegistryOptions.IsPasswordVisibleForControllerUser,
						string.Empty);
					return result;
				});
			}
		}

		IRegistryItem GmailServiceAccountKeyRegistry
		{
			get
			{
				return getMailBoxItem(NamePrefix + "GmailServiceAccountKey", () =>
				{
					var result = new GmailOAuth2JsonFileRegistryItem(
						NamePrefix + "GmailServiceAccountKey",
						gMailCategory,
						ResString.GetMultilingualString("7670598c-e156-445a-8155-3cbaa5af0f01", "Service Account Key"),
						ResString.GetMultilingualString("e1fd3a60-5e70-49e7-8232-075999a3a9ae", "Please choose key file (*.json). The key file can be created under the service account."),
						RegistryStorageFlags.System,
						registryOptions);
					return result;
				});
			}
		}

		#endregion

		#region Properties

		new bool UseFallback => string.IsNullOrWhiteSpace(Ms365OAuth2TenantIdRegistry.Value.ToString()) || string.IsNullOrWhiteSpace(Ms365ApplicationIdRegistry.Value.ToString());

		public string OAuth2Type
		{
			get
			{
				return base.UseFallback ? Env.Registry.UseOAuth2ForIncoming : (string)OAuth2TypeRegistry.Value;
			}
#if DEBUG
			set { OAuth2TypeRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool UseOAuth2 => !string.IsNullOrEmpty(OAuth2Type);

		public string TenantId
		{
			get
			{
				var tenantId = Ms365OAuth2TenantIdRegistry.Value.ToString();
				return !string.IsNullOrWhiteSpace(tenantId) ? tenantId : Env.Registry.Ms365OAuth2TenantId;
			}
#if DEBUG
			set { Ms365OAuth2TenantIdRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string ApplicationId
		{
			get
			{
				var appId = Ms365ApplicationIdRegistry.Value.ToString();
				return !string.IsNullOrWhiteSpace(appId) ? appId : Env.Registry.Ms365ApplicationIdForIncoming;
			}
#if DEBUG
			set { Ms365ApplicationIdRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public string AppSecret
		{
			get
			{
				var appSecret = Ms365AppSecretRegistry.Value.ToString();
				return !string.IsNullOrWhiteSpace(appSecret) ? appSecret : Env.Registry.Ms365AppSecretForIncoming;
			}
		}

		public bool UseGraphApi
		{
			get
			{
				var appId = Ms365ApplicationIdRegistry.Value.ToString();
				return !string.IsNullOrWhiteSpace(appId) ? (bool)UseGraphApiRegistry.Value : Env.Registry.UseGraphApiForIncoming;
			}
		}

		public string DelegatedMail
		{
			get
			{
				var delegateMail = GmailDelegatedMailRegistry.Value.ToString();
				return !string.IsNullOrWhiteSpace(delegateMail) ? delegateMail : Env.Registry.GmailDelegatedMailForIncoming;
			}
#if DEBUG
			set { GmailDelegatedMailRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public GmailOAuth2JsonFile ServiceAccountKey
		{
			get
			{
				return (GmailOAuth2JsonFile)GmailServiceAccountKeyRegistry.Value;
			}
#if DEBUG
			set { GmailServiceAccountKeyRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Translation not needed")]
		public override bool IsValid(out ICollection<string> errorMessages)
		{
			errorMessages = new List<string>();

			switch (OAuth2Type)
			{
				case "":
				{
					return base.IsValid(out errorMessages);
				}
				case OAuth2TypeList.Codes.Ms365:
					{
						if (string.IsNullOrWhiteSpace(TenantId))
						{
							errorMessages.Add("Tenant ID is not configured");
						}

						if (string.IsNullOrWhiteSpace(ApplicationId))
						{
							errorMessages.Add("Application ID is not configured");
						}

						return errorMessages.Count == 0;
					}
				case OAuth2TypeList.Codes.GMail:
					{
						if (string.IsNullOrWhiteSpace(DelegatedMail))
						{
							errorMessages.Add("Delegate Mail is not configured");
						}

						if (string.IsNullOrWhiteSpace(ServiceAccountKey.JsonText))
						{
							errorMessages.Add("Service Account Key is not configured");
						}

						return errorMessages.Count == 0;
					}
				default:
					{
						errorMessages.Add("Invalid OAuth type: " + OAuth2Type);
						return false;
					}
			}
		}

		public override IEnumerable<IRegistryItem> GetAllItems()
		{
			return
			[
				IMAPSecureConnectionTypeRegistry,
				MailboxPasswordRegistry,
				MailboxUserNameRegistry,
				MailServerRegistry,
				MailServerPortRegistry,
				POP3SecureConnectionTypeRegistry,
				MailRetrievalProtocolRegistry,
				OAuth2TypeRegistry,
				Ms365OAuth2TenantIdRegistry,
				Ms365ApplicationIdRegistry,
				Ms365AppSecretRegistry,
				UseGraphApiRegistry,
				Ms365OAuth2TokenRegistry,
				Ms365OAuth2AppToken,
				GmailDelegatedMailRegistry,
				GmailServiceAccountKeyRegistry
			];
		}

		public Ms365OAuth2Token GetMs365UserToken()
		{
			return (Ms365OAuth2Token)Ms365OAuth2TokenRegistry.Value;
		}

		public byte[] GetMs365AppToken()
		{
			return (byte[])Ms365OAuth2AppToken.Value;
		}

		public void SaveMs365OAuth2Token(bool isUserToken, byte[] bytes)
		{
			if (bytes == null)
			{
				return;
			}

			if (isUserToken)
			{
				var token = GetMs365UserToken();
				token.Token = bytes;
				Ms365OAuth2TokenRegistry.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, token);
			}
			else
			{
				Ms365OAuth2AppToken.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, bytes);
			}
		}
	}
}
