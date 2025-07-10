using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Controllers.TrustedMessaging.Base;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Models.PortalAuth;
using Newtonsoft.Json;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Interfaces;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public abstract class LoginServiceBaseController<TRequest> : TrustedControllerWithLicenceDatabase<TRequest> where TRequest : ITrustedUserInfo
	{
		public LoginServiceBaseController() : base()
		{
		}

		public LoginServiceBaseController(NLogWrapper logger) : base(logger)
		{
		}

		#region Auto Login Url

		protected void GetAutoLoginUrlCore(ITrustedContext<AutoLoginResponse> context, ITrustedAutoLoginInfo requestInfo)
		{
			if (!context.Success)
			{
				return;
			}

			var database = GetLicenceDatabase(context, (TRequest)requestInfo);
			if (database == null || !database.LD_IsActive || !database.LD_AllowAutoLogin)
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Authorization_ActionNotPermitted, "Auto login to MyAccount is disabled on this system.");
				return;
			}

			var requestDataValidation = new UserAccountCreationRequestDataValidation(requestInfo);
			requestDataValidation.Validate();

			if (requestDataValidation.ErrorMessageBuilder.Messages.Count > 0)
			{
				context.Messages = requestDataValidation.ErrorMessageBuilder;
				return;
			}

			try
			{
				var userAccount = EdiCustomerUserAccount.GetUserAccountForWebAccess(Factory, database, requestInfo.UserId);

				if (UserAgreementRequestValidationHelper.CanBypassUserAccountCollectionAgreement(requestInfo.Product))
				{
					if (userAccount == null)
					{
						userAccount = EdiCustomerUserAccount.CreateNewUserAccount(Factory, database, requestInfo.UserId, requestInfo.FullName, requestInfo.Email, requestInfo.UserCountry);
					}
				}
				else
				{
					var myaccountAgreement = EdiUserAgreement.GetCurrentAgreement(Factory, null, EdiUserAgreementTypes.Codes.UserAccountCollection, requestInfo.UserCountry)?.Agreement;
					if (myaccountAgreement != null)
					{
						if (userAccount != null)
						{
							if (!userAccount.HasAcknowledgedUserAgreement(myaccountAgreement))
							{
								context.Messages = new ErrorMessages(ErrorCodes.Codes.Authorization_AgreementNotAcknowledged, ErrorCodes.Descriptions.Authorization_AgreementNotAcknowledged);
								return;
							}
						}
						else
						{
							if (myaccountAgreement.HasBeenAcceptedByOrganisation(database.WebAccessOrg))
							{
								userAccount = EdiCustomerUserAccount.CreateNewUserAccount(Factory, database, requestInfo.UserId, requestInfo.FullName, requestInfo.Email, requestInfo.UserCountry ?? string.Empty);
							}
						}
					}

					if (userAccount == null)
					{
						context.Messages = new ErrorMessages(ErrorCodes.Codes.Authorization_ActionNotPermitted, "Access to MyAccount is not granted.");
						return;
					}
				}

				var importer = new WebRequestContactImporter(Factory, database);
				var contact = importer.ImportFromTrustedUserInfo(requestInfo).Item2;
				importer.SaveIfNeeded();

				if (userAccount.EUA_IsEmailVerificationRequired)
				{
					var router = new MyAccountLoginRouter(null, userAccount);
					var redirectUrl = router.GetRoutingUrl();
					context.ResponseInfo = new AutoLoginResponse(redirectUrl);
					return;
				}
				else if (contact == null || (userAccount.EUA_ContactRelationshipStatus != ContactRelationshipStatusList.Codes.DistinctEmailRequired && !contact.OC_Email.IsEmpty && !contact.OC_WebAccessEnabled))
				{
					context.Messages = new ErrorMessages(ErrorCodes.Codes.Authorization_ActionNotPermitted, "Access to MyAccount is not granted.");
					return;
				}

				var autoLoginUrl = BuildAutoLoginUrl(userAccount, requestInfo.ReturnUrl, context.Product, (requestInfo as TrustedAutoLoginInfo)?.SystemId);
				context.ResponseInfo = new AutoLoginResponse(autoLoginUrl);
				return;
			}
			catch (ZSaveException ex)
			{
				ErrorReporter.ReportOnce("LoginServiceController.GetAutoLoginUrl(): Contact import failed", ex);
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Server_Error, "Internal Server Error.");
				return;
			}
		}

		Uri BuildAutoLoginUrl(EdiCustomerUserAccount userAccount, Uri returnUrl, string productCode, string databaseNumber)
		{
			var contact = userAccount.WebAccessContact;
			var accessControl = new TokenizedAccessControl();

			string scopeUrl;
			if (returnUrl == null)
			{
				scopeUrl = EDIDataRegistry.Instance.MyAccountIndexPage.Value;

				var productUrl = EDIDataRegistry.Instance.MyAccountHostingSiteLandingPageUrl.Value;
				if (!string.IsNullOrEmpty(scopeUrl) && !string.IsNullOrEmpty(productCode) && productUrl.Any())
				{
					var url = productUrl.Cast<MyAccountHostingSiteLandingPageUrl>().FirstOrDefault(a => a.ProductCode == productCode);
					if (url != null)
					{
						scopeUrl = url.LandingPageAbsoluteUrl;
					}
				}
			}
			else
			{
				scopeUrl = (returnUrl.IsAbsoluteUri ? returnUrl.AbsoluteUri : Uri.EscapeUriString(returnUrl.ToString()));
			}

			var scope = new AutoLoginTokenScope
			{
				OrgCode = contact.OrgCode,
				DatabaseNumber = databaseNumber,
				ReturnUrl = scopeUrl
			};
			var serializedScope = AutoLoginHelper.SerializeToXml(scope);

			var accessToken = accessControl.CreateLimitedToken(AccessTokenTypes.MyAccountAutoLogin, new AccessTokenInfo(serializedScope, userAccount.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix), TimeSpan.FromMinutes(5), maxUses: 1);

			var basePath = EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value;
			var path = basePath.TrimEnd('/') + "/Login/AutoLogin.aspx?token=" + accessToken;

			var builder = new UriBuilder(path) { Port = -1 };
			return builder.Uri;
		}

		#endregion Auto Login Url

		#region Update User Account

		protected void UpdateUserAccountCore(ITrustedContext<bool> context, IUpdateUserInfo requestInfo)
		{
			if (!context.Success)
			{
				return;
			}

			var database = GetLicenceDatabase(context, (TRequest)requestInfo);
			if (database == null || !database.LD_IsActive || !database.LD_AllowAutoLogin)
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Authorization_ActionNotPermitted, "Access to MyAccount is not granted.");
				return;
			}

			if (string.IsNullOrWhiteSpace(requestInfo.UserId))
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_MissingRequiredField, UserAccountCreationRequestDataValidation.UserIDEmptyErrorMessage);
				return;
			}

			if (requestInfo.UserId.Length > EdiCustomerUserAccountSchema.EUA_UserID.MaxLength)
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_InvalidValue, UserAccountCreationRequestDataValidation.UserIDMaxLengthErrorMessage);
				return;
			}

			if (requestInfo.IsActive == null)
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_MissingRequiredField, Res.GetString("84d04a51-db28-469b-8507-fe88eb2c28df", "{0} is not specified.", IUpdateUserInfo.Schema.IsActive));
				return;
			}

			var userAccountQuery = new ZQuery(EdiCustomerUserAccountSchema.EUA_LD, database.PK);
			userAccountQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_UserID, requestInfo.UserId);
			var userAccount = Factory.LoadTop1<EdiCustomerUserAccount>(userAccountQuery);

			if (userAccount == null)
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_InvalidValue, "User Id is not found.");
				return;
			}

			if (userAccount.EUA_IsActive != requestInfo.IsActive.Value)
			{
				userAccount.EUA_IsActive = requestInfo.IsActive.Value;
				var importer = new WebRequestContactImporter(Factory, database);
				importer.ImportFromUserAccount(userAccount);
				importer.SaveIfNeeded();
			}

			context.ResponseInfo = true;
		}

		#endregion Update User Account

		#region OAuth

		protected void GetOAuthLoginUrlCore(TrustedContext<AuthenticationTokenInfo, OAuthLoginResponse> context)
		{
			if (!context.Success)
			{
				return;
			}

			var database = context.TrustedSystem?.FindTenantDatabaseByTrustedInfo(context.RequestInfo);
			if (database == null || !database.LD_IsActive || !database.LD_AllowAutoLogin)
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Authorization_ActionNotPermitted, "Auto login to MyAccount is disabled on this system.");
				return;
			}

			var info = context.RequestInfo;
			if (!Uri.TryCreate(info.RedirectUriString, UriKind.Absolute, out var redirectUri))
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Validation_InvalidValue, "Invalid redirect_uri value.");
				return;
			}

			try
			{
				var userAccount = EdiCustomerUserAccount.GetUserAccountForWebAccess(Factory, database, info.UserId);
				if (userAccount == null)
				{
					context.Messages = new ErrorMessages(ErrorCodes.Codes.Authorization_ActionNotPermitted, "Auto login to MyAccount is disabled on this system.");
					return;
				}

				var myaccountAgreement = EdiUserAgreement.GetCurrentAgreement(Factory, null, EdiUserAgreementTypes.Codes.UserAccountCollection, info.UserCountry)?.Agreement;
				if (myaccountAgreement != null && !userAccount.HasAcknowledgedUserAgreement(myaccountAgreement))
				{
					context.Messages = new ErrorMessages(ErrorCodes.Codes.Authorization_AgreementNotAcknowledged, ErrorCodes.Descriptions.Authorization_AgreementNotAcknowledged);
					return;
				}

				var importer = new WebRequestContactImporter(Factory, database);
				var contact = importer.ImportFromUserAccount(userAccount).Item2;
				importer.SaveIfNeeded();

				if (userAccount.EUA_IsEmailVerificationRequired)
				{
					var router = new MyAccountLoginRouter(null, userAccount);
					var redirectUrl = router.GetRoutingUrl();
					context.ResponseInfo = new OAuthLoginResponse() { RedirectUrl = redirectUrl };
					return;
				}
				else if (contact == null || !contact.OC_WebAccessEnabled)
				{
					context.Messages = new ErrorMessages(ErrorCodes.Codes.Authorization_ActionNotPermitted, "Access to MyAccount is not granted.");
					return;
				}

				ITokenizedAccessControl control = new TokenizedAccessControl();
				var extraClaims = JsonConvert.SerializeObject(info.Claims);
				var accessToken = control.CreateLimitedToken(AccessTokenTypes.MyAccountAutoLogin, new AccessTokenInfo(extraClaims, userAccount.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix), TimeSpan.FromMinutes(5), maxUses: 1);
				var autoLoginUrl = AppendKeyValuePairsToUri(redirectUri, new[] { new KeyValuePair<string, string>("token", accessToken) });
				context.ResponseInfo = new OAuthLoginResponse() { RedirectUrl = autoLoginUrl, Token = accessToken };
				return;
			}
			catch (ZSaveException ex)
			{
				ErrorReporter.ReportOnce("LoginServiceController.GetOAuthLoginUrl(): Contact import failed", ex);
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Server_Error, "Internal Server Error.");
				return;
			}
		}

		static Uri AppendKeyValuePairsToUri(Uri uri, IEnumerable<KeyValuePair<string, string>> values)
		{
			var builder = new UriBuilder(uri);
			var parameters = UriExtensions.ParseQueryString(uri);

			foreach (var kvp in values)
			{
				parameters.Add(kvp.Key, kvp.Value);
			}

			var keyValuePairs = new List<string>();
			foreach (var key in parameters.AllKeys)
			{
				foreach (var value in parameters.GetValues(key))
				{
					keyValuePairs.Add(FormattableString.Invariant($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value ?? string.Empty)}"));
				}
			}

			var newQueryString = string.Join("&", keyValuePairs);
			builder.Query = newQueryString;
			var newUri = builder.Uri;

			return newUri;
		}

		#endregion OAuth
	}
}
