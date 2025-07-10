using System;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MailManager.Integration;
using MailKit.Security;

namespace Enterprise.MailManager
{
	public class Ms365OAuth2Configuration : IOAuth2Configuration
	{
		public Ms365OAuth2Configuration(
			string tenantId,
			string applicationId,
			 Ms365OAuth2PermissionType permissionType,
			 byte[] cachedToken,
			 Action<byte[]> tokenSaveAction = null,
			 string identifier = null,
			 string clientSecret = null,
			 bool shouldAcquireTokenInteractive = false)
		{
			TenantId = tenantId;
			ApplicationId = applicationId;
			PermissionType = permissionType;
			CachedToken = cachedToken;
			TokenSaveAction = tokenSaveAction;
			Identifier = identifier;
			ClientSecret = clientSecret;
			ShouldAcquireTokenInteractive = shouldAcquireTokenInteractive;

			scopesLookUp = new Dictionary<Ms365OAuth2PermissionType, string[]>()
			{
				{
					Ms365OAuth2PermissionType.DelegatePermission_OutLook,
					new string[]
					{
						"offline_access",
						"https://outlook.office.com/IMAP.AccessAsUser.All",
						"https://outlook.office.com/POP.AccessAsUser.All",
						"https://outlook.office.com/SMTP.Send"
					}
				},
				{
					Ms365OAuth2PermissionType.DelegatePermission_GraphAPI,
					new string[] { "mail.send.shared", "mail.readwrite.shared" }
				},
				{
					Ms365OAuth2PermissionType.ApplicationPermission_GraphAPI,
					new string[] { "https://graph.microsoft.com/.default", }
				},
			};
		}

		public string TenantId { get; private set; }
		public string ApplicationId { get; private set; }
		public Ms365OAuth2PermissionType PermissionType { get; }
		public bool IsUserToken => PermissionType == Ms365OAuth2PermissionType.DelegatePermission_GraphAPI;
		public Action<byte[]> TokenSaveAction { get; private set; }
		public byte[] CachedToken { get; private set; }
		public string Identifier { get; private set; }
		public string ClientSecret { get; private set; }
		public bool ShouldAcquireTokenInteractive { get; set; }

		public string[] Scopes => scopesLookUp[PermissionType];

		public enum Ms365OAuth2PermissionType
		{
			/// <summary>
			/// SMTP, IMap, POP3
			/// </summary>
			DelegatePermission_OutLook,
			DelegatePermission_GraphAPI,
			ApplicationPermission_GraphAPI,
		}

		readonly Dictionary<Ms365OAuth2PermissionType, string[]> scopesLookUp;

		public SaslMechanismOAuth2 GetSaslMechanism()
		{
			var helper = ObjectFactory.Get<IMs365OAuth2AuthenticationHelper>(nameof(IMs365OAuth2AuthenticationHelper), this);
			var auth = helper.AcquireTokenAsync().Result;
			return new SaslMechanismOAuth2(auth.Account.Username, auth.AccessToken);
		}
	}
}
