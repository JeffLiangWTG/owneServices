using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Web;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions.Authentication;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.Utilities.Exceptions;
using Enterprise.ZClientWebCargoWiseEDI.OIDC;
using NLog;
using WTG.OpenIDConnect.Token;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class OIDCLoginComplete : BasePage
	{
		protected override bool PageRequiresLogin(Uri url)
		{
			return false;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (CheckUserAndIDPData(out var idpResponse, out var userCookie))
			{
				var tokenUri = OIDCLoginHelper.GetOIDCTokenUri(
					idpResponse.Code,
					new Dictionary<string, string>()
					{
						{ OIDCLoginHelper.Constants.VerifierKey, userCookie.Verifier }
					},
					Logger,
					CancellationToken.None);

				using (var message = new HttpRequestMessage(HttpMethod.Get, tokenUri))
				using (var client = HttpClientFactory.Create())
				{
					var result = client.SendAsync(message).Result;
					var resultString = result.Content.ReadAsStringAsync().Result;
					var queryResult = JsonDocument.Parse(resultString);
					if (result.IsSuccessStatusCode)
					{
						if (queryResult.RootElement.TryGetProperty("id_token", out var idToken))
						{
							try
							{
								var jwtToken = TokenValidator.ValidateIdentityToken(
									SystemDataRegistry.Instance.OIDCConfig.Value.AuthorityURL,
									EDIDataRegistry.Instance.MyAccountQueryOIDCClientID.Value,
									idToken.GetString(),
									ConfigurationHelper.ConfigurationManagerCache,
									new TokenValidationLogger(new BaseLogger(GetType())), CancellationToken.None).Result;
								if (jwtToken != null)
								{
									var nonceClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Nonce)?.Value;
									if (string.IsNullOrEmpty(nonceClaim) || nonceClaim != userCookie.Nonce)
									{
										Response.Redirect(GetLoginUri(userCookie.State, OIDCLoginHelper.ErrorList.NonceDoesNotMatchCookie));
										return;
									}
									else
									{
										FindUserAndRedirect(jwtToken, userCookie);
										return;
									}
								}
								else
								{
									var error = $"MyAccount OIDC | ValidateIdentityToken returned null object | {resultString}";
									Logger.AddLog(LogLevel.Error, error, ((int)HttpStatusCode.BadRequest));
									Response.Redirect(GetLoginUri(idpResponse.State, OIDCLoginHelper.ErrorList.NullJWTSecurityToken));
								}
							}
							catch (ValidateTokenException ex)
							{
								var userMessage = OIDCLoginHelper.TokenValidationExceptionMessageProcessor(resultString, ex, Logger);
								Response.Redirect(GetLoginUri(idpResponse.State, userMessage));
							}
						}
						else
						{
							var errorMsg = $"MyAccount OIDC | IDP didn't issue id token | {resultString}";
							Logger.AddLog(LogLevel.Error, errorMsg, ((int)HttpStatusCode.BadRequest));
							Response.Redirect(GetLoginUri(idpResponse.State, OIDCLoginHelper.ErrorList.MissingIDToken));
						}
					}
					else
					{
						var errorResult = queryResult.RootElement.TryGetProperty(OIDCLoginHelper.Constants.ErrorKey, out var error);
						var errorDescriptionResult = queryResult.RootElement.TryGetProperty(OIDCLoginHelper.Constants.ErrorDescriptionKey, out var errorDescription);
						if (errorResult && errorDescriptionResult)
						{
							var userErrorMessage = OIDCLoginHelper.IDPErrorMessageProcessor(error.GetString(), errorDescription.GetString(), Logger);
							Response.Redirect(GetLoginUri(idpResponse.State, userErrorMessage));
						}
						else
						{
							var errorMsg = $"MyAccount OIDC | Cannot get token | {result.Content.ReadAsStringAsync().Result}";
							Logger.AddLog(LogLevel.Error, errorMsg, ((int)HttpStatusCode.BadRequest));
							Response.Redirect(GetLoginUri());
						}
					}
				}
			}
		}

		bool CheckUserAndIDPData(out IDPResponseData idpResponse, out OIDCAuthUserCookieData userCookie)
		{
			idpResponse = null;
			userCookie = null;
			if (!OIDCLoginHelper.IsOIDCReady())
			{
				Response.StatusCode = (int)HttpStatusCode.BadRequest;
				return false;
			}

			idpResponse = new IDPResponseData(Request.QueryString);
			if (idpResponse.HasError)
			{
				var userErrorMessage = OIDCLoginHelper.IDPErrorMessageProcessor(idpResponse.Error, idpResponse.ErrorDescription, Logger);
				Response.Redirect(GetLoginUri(string.Empty, userErrorMessage));
				return false;
			}

			if (string.IsNullOrEmpty(idpResponse.Code))
			{
				Response.Redirect(GetLoginUri(string.Empty, OIDCLoginHelper.ErrorList.MissingAuthorizationCode));
				return false;
			}

			userCookie = OIDCAuthUserCookieData.Deserialize(Request.Cookies[OIDCLoginHelper.Constants.OIDCCookieName]?.Value);
			if (!userCookie.IsCookieDecodingSuccessful)
			{
				var errorMsg = $"MyAccount OIDC | The cookie is invalid | Cookie: {Request.Cookies[OIDCLoginHelper.Constants.OIDCCookieName]?.Value} | Message: {userCookie.DeserializingMessage}";
				Logger.AddLog(LogLevel.Error, errorMsg, ((int)HttpStatusCode.BadRequest));
				Response.Redirect(GetLoginUri(string.Empty, OIDCLoginHelper.ErrorList.SessionIsInvalid));
				return false;
			}

			if (string.IsNullOrEmpty(userCookie.Verifier) || string.IsNullOrEmpty(userCookie.Nonce))
			{
				Response.Redirect(GetLoginUri(string.Empty, OIDCLoginHelper.ErrorList.MissingVerifierOrNonce));
				return false;
			}

			return true;
		}

		void FindUserAndRedirect(JwtSecurityToken token, OIDCAuthUserCookieData userCookie)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var userInfo = OIDCUserLogin.VerifyUserByOidcClaims(SystemDataRegistry.Instance.OIDCConfig.Value, token.Claims);
				if (userInfo.IsOK && userInfo.User is EDIGlbStaff staff)
				{
					HttpContext.Current.Session[OIDCLoginHelper.Constants.OIDCSessionKey] = true;
					var oidcOrgList = EDIDataRegistry.Instance.RedirectedOrganisations.Value;
					var contacts = OIDCLoginHelper.GetAllContactsViaUserEmailAndPersonPK(Factory, userCookie.EmailAddress, staff.GS_PER, userCookie.OrganisationCode).
						Where(x => oidcOrgList.Contains(x.OC_OH.ToGuid()));
					if (contacts.Count() == 1)
					{
						var returnUrl = this.GetLiteViewModeReturnUrl(userCookie.State);
						var uri = new MyAccountLoginRouter(new Uri(returnUrl, UriKind.RelativeOrAbsolute), contacts.Single()).GetRoutingUrl();
						Response.Redirect(uri.IsAbsoluteUri ? uri.AbsolutePath : uri.OriginalString);
					}
					else if (!contacts.Any())
					{
						HttpContext.Current.Session[OIDCLoginHelper.Constants.OIDCSessionKey] = false;
						Response.Redirect(GetLoginUri(userCookie.State, OIDCLoginHelper.ErrorList.NoUser));
					}
					else
					{
						Response.Redirect(GetChooseCompanyPageUri(contacts, userCookie.State, false));
					}
				}
				else
				{
					Response.Redirect(GetLoginUri(userCookie.State, userInfo.FailureMessage));
				}
			}
		}

		protected virtual void ReportErrorCore(string reportKey, string reportMessage)
		{
			(new WebExceptionReporter()).ReportWebException(null, reportKey, reportMessage);
		}

		protected NLogWrapper Logger
		{
			get
			{
				if (logger == null)
				{
					logger = new NLogWrapper(GetType());
				}

				return logger;
			}

			set { logger = value; }
		}
		NLogWrapper logger;

		protected virtual bool IsAuthenticated => Request.IsAuthenticated;

		protected override bool ShouldSetupSessionOnLoad => true;

		string GetLoginUri(string userQuery = "", string userMessage = "")
		{
			var uri = "~/Login/LoginV2.aspx";
			var query = new QueryString();
			if (!string.IsNullOrEmpty(userQuery))
			{
				query.Add("ReturnUrl", userQuery);
			}

			if (!string.IsNullOrEmpty(userMessage))
			{
				var secureQueryString = new SecureQueryString { { "message", userMessage } };
				query.Add("data", secureQueryString.ToString());
			}

			var queryString = query.ToString();
			uri += string.IsNullOrEmpty(queryString) ? string.Empty : $"?{queryString}";
			return uri;
		}

		string GetChooseCompanyPageUri(IEnumerable<OrgContact> contacts, string userQuery = "", bool rememberMe = false)
		{
			var accessControl = new TokenizedAccessControl();
			var tokenInfo = new AccessTokenInfo(rememberMe + ":" + string.Join(",", contacts.Select(x => x.PK)), contacts.FirstOrDefault().PK.ToGuid(), OrgContactSchema.Constants.Prefix);
			var chooseCompanyToken = accessControl.CreateLimitedToken(AccessTokenTypes.LoginRouterMultiContactIdentity, tokenInfo, TimeSpan.FromMinutes(15), 1);
			var passwordSecureQueryString = new SecureQueryString
			{
				{ LoginRouter.IdentityTokenQueryStringKey, chooseCompanyToken },
				{ LoginRouter.OriginalUrlQueryStringKey, userQuery },
			};

			var uri = "~/Login/ChooseCompany.aspx";
			var query = new QueryString();
			var encodedQueryString = WebUtility.UrlEncode(passwordSecureQueryString.ToString());
			query.Add(SecureQueryString.QueryStringKey, encodedQueryString);
			uri += "?" + query.ToString();

			Action<WebUser, LoginManager> switchCompanyFunc = (siteUser, loginMan) =>
			{
				if (siteUser == null || loginMan == null)
				{
					return;
				}

				var org = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, loginMan.CompanyCode);
				if (org != null && EDIDataRegistry.Instance.RedirectedOrganisations.Value.Contains(org.PK.ToGuid()))
				{
					siteUser.Login(loginMan.CompanyCode, loginMan.UserName, Enterprise.ZArchitecture.Environment.User.WebTransientPassword, loginMan.LoginHash, false);
				}
				else
				{
					HttpContext.Current.Response.Redirect("~/Login/LogoutLite.aspx");
				}
			};

			HttpContext.Current.Session[OIDCLoginHelper.Constants.SwitchCompanySessionKey] = switchCompanyFunc;

			return uri;
		}
	}
}
