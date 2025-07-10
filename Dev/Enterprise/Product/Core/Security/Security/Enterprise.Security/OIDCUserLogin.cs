using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.RemoteDesktopServices;
using Enterprise.Security.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WTG.OpenIDConnect.Login;

namespace Enterprise.Security
{
	public static class OIDCUserLogin
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "protocol defined constants")]
		const string OpenIdScope = "openid";
		const string OfflineAccessScope = "offline_access";
		const int TimeoutSeconds = 90;

		static string DefaultDomainHint => SystemDataRegistry.Instance.DomainHint.Value;

		public static LoginAuthenticationInfo SingleSignOnLoginWithRefreshToken(IOIDCConfig oidcConfig, OIDCWebLauncher webLauncher, CancellationToken cancellationToken)
		{
			return LoginAndVerifyUser(oidcConfig, webLauncher, OIDCLoginRequestMessage.LoginPrompt.Default, new[] { OpenIdScope, OfflineAccessScope }, DefaultDomainHint, oidcLoginFactory: null, cancellationToken);
		}

		public static LoginAuthenticationInfo PromptUserLoginAndClearRefreshToken(IOIDCConfig oidcConfig, OIDCWebLauncher webLauncher, CancellationToken cancellationToken)
		{
			return LoginAndVerifyUser(oidcConfig, webLauncher, OIDCLoginRequestMessage.LoginPrompt.Login, new[] { OpenIdScope }, DefaultDomainHint, oidcLoginFactory: null, cancellationToken);
		}

		public static LoginAuthenticationInfo VerifyUserByOidcClaims(IOIDCConfig oidcConfig, IEnumerable<Claim> claims)
		{
			var loginClaimData = new OidcLoginClaimData(claims, oidcConfig);
			return VerifyUser(loginClaimData);
		}

		/// <summary>
		/// Verify the OIDC configuration within the domain.
		/// </summary>
		/// <returns><see cref="string.Empty"/> when the config is correct, otherwise a message describing the error.</returns>
		public static string VerifyOidcConfig(IOIDCConfig oidcConfig, OIDCWebLauncher webLauncher, string domainHint, CancellationToken cancellationToken)
		{
			return LoginAndVerifyClaim(oidcConfig, webLauncher, OIDCLoginRequestMessage.LoginPrompt.Login, new[] { OpenIdScope }, domainHint, oidcLoginFactory: null, cancellationToken);
		}

		static LoginAuthenticationInfo LoginAndVerifyUser(
			IOIDCConfig oidcConfig,
			OIDCWebLauncher webLauncher,
			OIDCLoginRequestMessage.LoginPrompt loginPrompt,
			string[] defaultScopes,
			string domainHint,
			OIDCLoginFactory oidcLoginFactory,
			CancellationToken cancellationToken)
		{
			try
			{
				var loginClaimData = OIDCLoginAndReturnClaims(oidcConfig, webLauncher, loginPrompt, defaultScopes, domainHint, oidcLoginFactory, cancellationToken);
				return VerifyUser(loginClaimData);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return LoginAuthenticationInfo.NewFailedLogin(ex.Message);
			}
		}

		static string LoginAndVerifyClaim(
			IOIDCConfig oidcConfig,
			OIDCWebLauncher webLauncher,
			OIDCLoginRequestMessage.LoginPrompt loginPrompt,
			string[] defaultScopes,
			string domainHint,
			OIDCLoginFactory oidcLoginFactory,
			CancellationToken cancellationToken)
		{
			try
			{
				var loginClaimData = OIDCLoginAndReturnClaims(oidcConfig, webLauncher, loginPrompt, defaultScopes, domainHint, oidcLoginFactory, cancellationToken);
				return loginClaimData.ErrorDescription ?? loginClaimData.Error ?? string.Empty;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return ex.Message;
			}
		}

		static OidcLoginClaimData OIDCLoginAndReturnClaims(
				IOIDCConfig oidcConfig,
				OIDCWebLauncher webLauncher,
				OIDCLoginRequestMessage.LoginPrompt loginPrompt,
				string[] defaultScopes,
				string domainHint,
				OIDCLoginFactory oidcLoginFactory,
				CancellationToken cancellationToken)
		{
			if (oidcConfig.AuthorityURL.IsEmpty)
			{
				return new OidcLoginClaimData(ResString.GetMultilingualString("1F155553-A823-41B0-8FCE-EEFB02E293D5", "Authority URL is empty."));
			}
			if (!Uri.TryCreate(oidcConfig.AuthorityURL, UriKind.Absolute, out var authorityUrl))
			{
				return new OidcLoginClaimData(ResString.GetMultilingualString("A50B894C-5661-4836-A4E3-CE16E35061D2", "Authority URL is invalid."));
			}

			var scopes = oidcConfig.Scopes.Select(x => x.ScopeName.ToString()).Concat(defaultScopes);

			var loginRequest = new OIDCLoginRequestMessage(
				authorityUrl,
				oidcConfig.ClientIdentifier,
				"",
				scopes.ToArray(),
				loginPrompt,
				MapRegistryServerToRequestServer(oidcConfig.OIDCServerType),
				SuccessMessage,
				FailedMessage,
				TimeoutSeconds,
				domainHint,
				Guid.NewGuid());

			var loginResponse = CallLoginServer(loginRequest, webLauncher, oidcLoginFactory, cancellationToken);

			if (loginResponse is null)
			{
				return new OidcLoginClaimData(ZTerminalService.ClientPluginApplicationNotInstalledError);
			}

			if (loginResponse.Error != OIDCLoginResponseMessage.ErrorType.None)
			{
				return GetError(loginResponse);
			}

			var validatedToken = LoginServerInstance.ValidateIdentityToken(loginRequest, loginResponse, cancellationToken);

			return new OidcLoginClaimData(validatedToken.Claims, oidcConfig);
		}

		static LoginAuthenticationInfo VerifyUser(OidcLoginClaimData oidcLoginClaimData)
		{
			if (oidcLoginClaimData.Error != null)
			{
				return LoginAuthenticationInfo.NewFailedLogin(oidcLoginClaimData.Error, oidcLoginClaimData.ErrorDescription);
			}

			var productReg = ObjectFactory.Get<IProductRegistration>();
			if (productReg.IsWiseTechGlobalInternalEDISystem())
			{
				var companyCode = oidcLoginClaimData.LoginClaims.FirstOrDefault(x => x.Type == "company_code")?.Value;
				if (companyCode == null || companyCode.ToUpper() != "WTG")
				{
					return LoginAuthenticationInfo.NewFailedLogin(Res.GetString("C575FCEF-A895-4F5E-A02D-21D1B3D017C4", "Please log in using a user account within the WiseTech Global tenancy."));
				}
			}

			using (Db.DisposableActionForDbConnection())
			{
				TryGetUserFromIdentityToken(oidcLoginClaimData.DbIdentifierToClaimMap, out var user, out var loginAuthentication);

				if (user != null)
				{
					var loginAuthenticationInfo = ValidateUser(user);

					if (user is IFactoryProvider factoryProvider)
					{
						factoryProvider.Factory.RelinquishThreadOwnership();
					}
					user.LoginToken = new LoginToken()
					{
						DummyTokenOriginalUserName = oidcLoginClaimData["original_user_name"],
						DummyTokenOriginalUserEmail = oidcLoginClaimData["original_user_email"],
					};

					return loginAuthenticationInfo;
				}

				return loginAuthentication;
			}
		}

		static OIDCLoginResponseMessage CallLoginServer(OIDCLoginRequestMessage loginRequest, OIDCWebLauncher webLauncher,
			OIDCLoginFactory oidcLoginFactory, CancellationToken cancellationToken)
		{
			if (IsRemote)
			{
				return LoginServerInstance.IsSupported
					? LoginServerInstance.LoginRemote(loginRequest, cancellationToken)
					: null;
			}
			return LoginServerInstance.LoginLocal(loginRequest, webLauncher, cancellationToken, oidcLoginFactory);
		}

		static bool IsRemote
		{
			get
			{
				var terminalService = ObjectFactory.Get<TerminalService>();
				return terminalService.IsWTSSession && terminalService.IsRemoteAppSession;
			}
		}

		static IOIDCLoginServer LoginServerInstance => ObjectFactory.Get<IOIDCLoginServer>();

		static OIDCLoginRequestMessage.OIDCServer MapRegistryServerToRequestServer(OIDCServerTypes serverType)
		{
			switch (serverType)
			{
				case OIDCServerTypes.Azure:
					return OIDCLoginRequestMessage.OIDCServer.Azure;
				case OIDCServerTypes.Okta:
					return OIDCLoginRequestMessage.OIDCServer.Okta;
				case OIDCServerTypes.OneLogin:
					return OIDCLoginRequestMessage.OIDCServer.OneLogin;
				case OIDCServerTypes.WiseTechIdP:
					return OIDCLoginRequestMessage.OIDCServer.WiseTechIdP;
				default:
					return OIDCLoginRequestMessage.OIDCServer.Generic;
			}
		}

		static OidcLoginClaimData GetError(OIDCLoginResponseMessage errorResponse)
		{
			switch (errorResponse.Error)
			{
				case OIDCLoginResponseMessage.ErrorType.Timeout:
					return new OidcLoginClaimData(ResString.GetMultilingualString("517DB0B8-5C28-4471-ABAD-B101BB07B884", "Session expired, please try again."));
				case OIDCLoginResponseMessage.ErrorType.OperationCanceled:
					return new OidcLoginClaimData(ResString.GetMultilingualString("F4FF0903-D2B0-492D-B567-FD29997125C0", "Authentication operation was canceled, please try again."));
				case OIDCLoginResponseMessage.ErrorType.LoginFailed:
					return new OidcLoginClaimData(ResString.GetMultilingualString("38630E55-8FCC-48F5-8B63-105C11CC0096", "Authentication failed, please try again or contact your system administrator."), errorResponse.ErrorDescription);
				case OIDCLoginResponseMessage.ErrorType.Exception:
					return new OidcLoginClaimData(ResString.GetMultilingualString("CAC67F7B-E9DD-4A92-B18F-6E9CD4B7F055", "A problem occurred while attempting to perform authentication, please try again or contact your system administrator."), errorResponse.ErrorDescription);
				default:
					return new OidcLoginClaimData(errorResponse.ErrorDescription);
			}
		}

		static void TryGetUserFromIdentityToken(Dictionary<string, string> matchingTokenClaims, out IUser user, out LoginAuthenticationInfo loginAuthenticationInfo)
		{
			user = null;
			loginAuthenticationInfo = null;

			var factory = new BusinessObjectFactory() { NameForDebugging = "TryGetUserFromIdentityToken" };
			foreach (var staffToValue in matchingTokenClaims)
			{
				try
				{
					var (schemaColumn, converter) = OidcLoginDatabaseSchemaHelper.GetWhitelistedSchemaColumnFromName(staffToValue.Key);
					var query = new ZDBOnlyQuery(typeof(GlbStaff));
					query.AddToFilter(schemaColumn, converter(staffToValue.Value));
					var loadedResults = factory.Load<GlbStaff>(query);
					if (loadedResults.Length == 1)
					{
						user = loadedResults[0];
						break;
					}

					if (loadedResults.Length > 1)
					{
						loginAuthenticationInfo = LoginAuthenticationInfo.NewFailedLogin(ResString.GetMultilingualString("BD64A2F9-95D6-461E-8F64-1E2F793D0F1B", "Multiple users found based on the matched key '{0}' and value '{1}'. Please contact your system administrator.", staffToValue.Key, staffToValue.Value));
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}

			if (user == null && loginAuthenticationInfo == null)
			{
				loginAuthenticationInfo = LoginAuthenticationInfo.NewFailedLogin(ResString.GetMultilingualString("D43CD1EB-B5B2-499F-B168-E676C616B5BC", "No user record could be found that matched the retrieved identity information. Please contact your system administrator."));
			}
		}

		static LoginAuthenticationInfo ValidateUser(IUser user)
		{
			if (user.IsResource)
			{
				return LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.UserNotFound, UserLoginValidation.LoginFailMsg);
			}

			if (!user.CanLogin)
			{
				return LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.UserNotFound, UserLoginValidation.CLUserLoginFailMsg);
			}

			if (user.IsDeviceOnly)
			{
				return LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.UserNotFound, UserLoginValidation.LoginIsForDeviceOnlyMsg);
			}

			if (user.IsWebUser && !Globals.IsWeb)
			{
				return LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.WebUserCannotLogin, UserLoginValidation.LoginFailMsg);
			}

			if (user.IsSysAdmin)
			{
				return LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.DisallowedSysAdminLogon, UserLoginValidation.InvalidSysadminLogonMsg);
			}

			if (!user.IsActive)
			{
				return LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.UserInactive, UserLoginValidation.InactiveLoginMsg);
			}

			if (user.IsLockedOut)
			{
				return LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.UserLockedOut, UserLoginValidation.LockedoutLoginMsg);
			}

			var restriction = UserLoginValidation.CheckIPAddressRestriction(user);
			if (!string.IsNullOrEmpty(restriction))
			{
				return LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.ClientIPAddressRestricted, restriction);
			}

			return LoginAuthenticationInfo.NewSuccessfulLogin(user);
		}

		static string SuccessMessage => ResString.GetMultilingualString("73B4140D-BC2A-4C42-B419-ECAE5AB88182", "You've been successfully authenticated. Please close this browser tab, as it is no longer required, and continue to the application.");
		static string FailedMessage => ResString.GetMultilingualString("2115A02E-4819-4C24-84DB-3391D86FB90E", "Oops, something went wrong. Please try again or contact your System Administrator.");
	}
}
