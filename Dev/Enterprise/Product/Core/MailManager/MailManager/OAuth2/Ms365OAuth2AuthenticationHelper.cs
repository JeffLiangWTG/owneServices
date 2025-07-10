using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Collections;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.MailManager.Integration;
using Enterprise.ZArchitecture.Core;
using Microsoft.Identity.Client;

namespace Enterprise.MailManager
{
	public class Ms365OAuth2AuthenticationHelper : IMs365OAuth2AuthenticationHelper
	{
		public Ms365OAuth2AuthenticationHelper(Ms365OAuth2Configuration oAuth2Configuration)
		{
			Argument.NotNull(oAuth2Configuration, nameof(oAuth2Configuration));
			this.oAuth2Configuration = oAuth2Configuration;
			cachedToken = oAuth2Configuration.CachedToken;
		}

		async Task<AuthenticationResult> IMs365OAuth2AuthenticationHelper.AcquireTokenAsync(CancellationToken token)
		{
			var app = GetClientApp();
			if (app is IPublicClientApplication pca)
			{
				return await AcquireUserTokenAsync(pca, token);
			}
			else if (app is IConfidentialClientApplication cca)
			{
				return await AcquireAppTokenAsync(cca);
			}

			throw new InvalidOperationException($"Error occurs when building application. Only {nameof(IPublicClientApplication)} and {nameof(IConfidentialClientApplication)} are supported.");
		}

		IClientApplicationBase GetClientApp()
		{
			IClientApplicationBase clientApplication;
			switch (oAuth2Configuration.PermissionType)
			{
				case Ms365OAuth2Configuration.Ms365OAuth2PermissionType.ApplicationPermission_GraphAPI:
					var key = nameof(IConfidentialClientApplication) + Tenant + oAuth2Configuration.ApplicationId + oAuth2Configuration.ClientSecret;
					if (!ApplicationCache.TryGetValue(key, out clientApplication))
					{
						clientApplication = ConfidentialClientApplicationBuilder.Create(oAuth2Configuration.ApplicationId)
						.WithClientSecret(oAuth2Configuration.ClientSecret)
						.WithAuthority($"{Instance}{Tenant}")
						.Build();

						ApplicationCache.Add(key, clientApplication);
					}
					break;
				case Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_OutLook:
				case Ms365OAuth2Configuration.Ms365OAuth2PermissionType.DelegatePermission_GraphAPI:
					key = nameof(IPublicClientApplication) + Tenant + oAuth2Configuration.ApplicationId;
					if (!ApplicationCache.TryGetValue(key, out clientApplication))
					{
						clientApplication = PublicClientApplicationBuilder.Create(oAuth2Configuration.ApplicationId)
						.WithAuthority($"{Instance}{Tenant}")
						.WithDefaultRedirectUri().Build();

						ApplicationCache.Add(key, clientApplication);
					}
					break;
				default:
					throw new NotSupportedException($"Permission Type {oAuth2Configuration.PermissionType} is not supported yet.");
			}

			return clientApplication;
		}

		async Task<AuthenticationResult> AcquireUserTokenAsync(IPublicClientApplication app, CancellationToken token = default)
		{
			SetTokenCacheCallback(app.UserTokenCache);
			AuthenticationResult result = null;
			IAccount account = null;
			if (!string.IsNullOrEmpty(oAuth2Configuration.Identifier))
			{
				account = await app.GetAccountAsync(oAuth2Configuration.Identifier);
			}

			try
			{
				result = await app.AcquireTokenSilent(oAuth2Configuration.Scopes, account).ExecuteAsync(token);
			}
			catch (MsalUiRequiredException)
			{
				if (oAuth2Configuration.ShouldAcquireTokenInteractive)
				{
					try
					{
						var remoteAcquireTokenInteractiveServer = ObjectFactory.Get<IAcquireTokenInteractiveServer>();
						result = await remoteAcquireTokenInteractiveServer.AcquireByDeviceCodeAsync(oAuth2Configuration.Scopes, app, token);
					}
					catch (MsalClientException clientException) when (clientException.ErrorCode == MsalError.AuthenticationCanceledError)
					{
						// User canceled the authentication, do nothing.
					}
					catch (MsalServiceException serviceException) when (serviceException.ErrorCode == MsalError.AccessDenied || serviceException.ErrorCode == "consent_required")
					{
						//MsalError.AccessDenied  - user clicked the back button, do nothing.
						//consent_required - user clicked the Cancel button in the Consent Granting screen.
					}
					catch (OperationCanceledException)
					{
						// User canceled the authentication or it timed out, do nothing.
					}
				}
				else
				{
					throw;
				}
			}

			return result;
		}

		async Task<AuthenticationResult> AcquireAppTokenAsync(IConfidentialClientApplication app)
		{
			SetTokenCacheCallback(app.AppTokenCache);
			return await app.AcquireTokenForClient(oAuth2Configuration.Scopes).ExecuteAsync();
		}

		#region Token Cache

		void SetTokenCacheCallback(ITokenCache tokenCache)
		{
			tokenCache.SetBeforeAccess(BeforeAccessNotification);
			tokenCache.SetAfterAccess(AfterAccessNotification);
		}

		void BeforeAccessNotification(TokenCacheNotificationArgs args)
		{
			lock (ObjectLock)
			{
				args.TokenCache.DeserializeMsalV3(cachedToken);
			}
		}

		void AfterAccessNotification(TokenCacheNotificationArgs args)
		{
			if (args.HasStateChanged)
			{
				lock (ObjectLock)
				{
					cachedToken = args.TokenCache.SerializeMsalV3();
					oAuth2Configuration.TokenSaveAction?.Invoke(cachedToken);
				}
			}
		}

		#endregion

		internal string Tenant
		{
			get
			{
				var tenant = oAuth2Configuration.TenantId.Trim();

				return string.IsNullOrEmpty(tenant) ? (NoResString)"common" : tenant;
			}
		}

		byte[] cachedToken;
		readonly Ms365OAuth2Configuration oAuth2Configuration;
		static readonly object ObjectLock = new object();

		LRUCache<string, IClientApplicationBase> ApplicationCache
		{
			get
			{
				if (applicationCache == null)
				{
					// Application should be unique per TenentId + AppId + AppSecret.
					// I set the strongReferenceCount to 3 because currently we have at most 3 different instances of application on GUI and 1 instance for service task (OMS/IMS...).
					applicationCache = new LRUCache<string, IClientApplicationBase>(strongReferenceCount: 3);
				}
				return applicationCache;
			}
		}

		[ThreadStatic]
		static LRUCache<string, IClientApplicationBase> applicationCache;

		const string Instance = "https://login.microsoftonline.com/";

		public static AuthenticationResult GetOAuth2AuthenticationResult(IOAuth2Configuration configuration)
		{
			if (configuration is not Ms365OAuth2Configuration ms365Configuration)
			{
				throw new ArgumentException(
					$"Tried to call {nameof(Ms365OAuth2AuthenticationHelper)}.{nameof(GetOAuth2AuthenticationResult)} using configuration of type {configuration?.GetType().FullName}");
			}

			var helper = ObjectFactory.Get<IMs365OAuth2AuthenticationHelper>(nameof(IMs365OAuth2AuthenticationHelper), ms365Configuration);
			return helper.AcquireTokenAsync().Result;
		}
	}
}
