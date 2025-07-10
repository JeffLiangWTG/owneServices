using System;
using CargoWise.Data;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZClientWebCargoWiseEDI.WebApi.Controllers.TrustedMessaging.Base;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount.Interfaces;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public abstract class UserAgreementBaseController<TUserAgreementRequest, TGetAcceptancesRequest, TLicenceObject> : TrustedControllerWithLicenceDatabase<TLicenceObject>
		where TUserAgreementRequest : TLicenceObject, IUserAgreementInfo
		where TGetAcceptancesRequest : TLicenceObject, IGetAcceptancesInfo
		where TLicenceObject : ITrustedInfo
	{
		public const string AgreementIsNotAllowedOnlineAcceptance = "Online license agreement acceptance is not currently supported for your organization. Please contact the WiseTech Global License Management Team via eRequest for more information";

		public UserAgreementBaseController() : base()
		{
		}

		public UserAgreementBaseController(NLogWrapper logger) : base(logger)
		{
		}

		protected void GetRequiredUserAgreementCore(ITrustedContext<UserAgreementResponseData> context, TUserAgreementRequest requestInfo)
		{
			if (!context.Success)
			{
				return;
			}

			var database = GetLicenceDatabase(context, requestInfo);
			if (database == null)
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Authorization_ActionNotPermitted, "Access is not granted.");
				return;
			}

			var requestDataValidation = GetUserAgreementRequestDataValidation(requestInfo);
			requestDataValidation.ValidateGetRequestData();

			if (requestDataValidation.ErrorMessageBuilder.Messages.Count > 0)
			{
				context.Messages = requestDataValidation.ErrorMessageBuilder;
				return;
			}

			var responseData = new UserAgreementResponseData();
			var userAccount = GetUserAccountFromUserID(Factory, database, requestInfo.UserId);
			var currentAgreementInfo = GetCurrentAgreementSafe(database, requestInfo.UserAgreementType, requestInfo.UserCountry);
			var currentAgreement = currentAgreementInfo.Agreement;

			var agreementRequired = currentAgreement != null && !HasAcknowledgedAgreement(currentAgreement, userAccount, database);
			if (!currentAgreementInfo.AllowOnlineAcceptance && agreementRequired)
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Authorization_ActionNotPermitted, AgreementIsNotAllowedOnlineAcceptance);
				return;
			}

			if (agreementRequired)
			{
				var parser = new DocumentParser<EdiUserAgreementWrapper, DocEdiUserAgreement>(database.Factory);
				var wrapper = new EdiUserAgreementWrapper()
				{
					UserAgreement = currentAgreement,
					UserInfo = requestInfo,
					Database = database,
					GetAgreementDocsURL = GetDocLinkUrl(database, currentAgreement),
				};
				responseData.Required = true;
				responseData.Title = parser.Parse(wrapper, currentAgreement.ERA_Title);
				responseData.Content = parser.Parse(wrapper, currentAgreement.ERA_Content);
				responseData.VersionNumber = currentAgreement.ERA_VersionNumber;
				responseData.MinorVersionNumber = currentAgreement.ERA_MinorVersion;
				responseData.Variant = currentAgreement.ERA_VariantCode;
				responseData.Level = currentAgreement.Level;
			}
			else
			{
				responseData.Required = false;
				responseData.Title = string.Empty;
				responseData.Content = string.Empty;
				responseData.VersionNumber = 0;
				responseData.MinorVersionNumber = 0;
				responseData.Variant = string.Empty;
				responseData.Level = string.Empty;
			}

			context.ResponseInfo = responseData;
		}

		string GetDocLinkUrl(LicenceDatabase database, EdiUserAgreement currentAgreement)
		{
			if (currentAgreement == null || currentAgreement.ERA_Type != EdiUserAgreementTypes.Codes.CargoWiseNext)
			{
				return string.Empty;
			}

			var eDoc = UserAgreementHelper.GetLatestMLAEDoc(currentAgreement);
			if (eDoc == null)
			{
				return string.Empty;
			}

			var tokenQuery = new ZQuery();
			tokenQuery.AddToFilter(StmAccessTokenSchema.SAT_ParentId, database.LicEnterprise.PK);
			tokenQuery.AddToFilter(StmAccessTokenSchema.SAT_ParentTableCode, LicenceEnterpriseSchema.Constants.Prefix);
			tokenQuery.AddToFilter(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.MyAccountUserAgreement);
			var existingToken = new BusinessObjectFactory().LoadTop1<StmAccessToken>(tokenQuery);

			var verifyToken = string.Empty;
			if (existingToken != null)
			{
				verifyToken = existingToken.SAT_Token;
			}
			else
			{
				var verifyEmailToken = new UserAgreementQueryToken()
				{
					Type = UserAgreementTokenTypes.Accept,
					AgreementType = currentAgreement.ERA_Type
				};
				var tokenControl = (ITokenizedAccessControl)new TokenizedAccessControl();
				var verifyTokenInfo = new AccessTokenInfo(verifyEmailToken.ToJson(), database.LicEnterprise.PK.ToGuid(), database.LicEnterprise.TablePrefix);
				using (Db.DisposableActionForDbConnection())
				{
					var user = WebAppEnvironment.GetWebUserFromDatabase();
					using (Env.SetTemporaryUserContext(new UserContext(user, DataRegistry.Instance.WebBranch, DataRegistry.Instance.WebDepartment)))
					{
						verifyToken = tokenControl.CreateLimitedToken(AccessTokenTypes.MyAccountUserAgreement, verifyTokenInfo, TimeSpan.FromDays(90));
					}
				}
			}

			return EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value.Trim('/') + "/admin/" + LinkHelper.GetHandlerUrl(currentAgreement.PK, eDoc.UniqueKey.ToGuid(), verifyToken);
		}

		protected virtual UserAgreementRequestDataValidation GetUserAgreementRequestDataValidation(TUserAgreementRequest requestInfo)
		{
			return new UserAgreementRequestDataValidation(requestInfo);
		}

		protected void GetRequiredEnterpriseUserAgreementUrlCore(ITrustedContext<EnterpriseAgreementResponseData> context, IEnterpriseAgreementInfo requestInfo)
		{
			if (!context.Success)
			{
				return;
			}

			var database = GetLicenceDatabase(context, requestInfo);

			if (database == null)
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Authorization_ActionNotPermitted, "Access is not granted.");
				return;
			}

			var responseData = new EnterpriseAgreementResponseData();
			var currentAgreementInfo = GetCurrentAgreementSafe(database, requestInfo.UserAgreementType, string.Empty);
			var currentAgreement = currentAgreementInfo.Agreement;

			var agreementRequired = currentAgreement != null && !currentAgreement.HasAcknowledgedCorporateAgreement(database);
			if (!currentAgreementInfo.AllowOnlineAcceptance && agreementRequired)
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Authorization_ActionNotPermitted, AgreementIsNotAllowedOnlineAcceptance);
				return;
			}

			if (agreementRequired)
			{
				responseData.Required = true;
				responseData.Url = UserAgreementUrlProvider.GetMyAccountUserAgreementUrlForDatabase(database, "UserAgreementController", requestInfo.UserAgreementType, requestInfo.ShouldSendAgreementCopy);
			}
			else
			{
				responseData.Required = false;
				responseData.Url = string.Empty;
			}

			context.ResponseInfo = responseData;
		}

		protected abstract LicenceDatabase GetLicenceDatabase(ITrustedContext context, IEnterpriseAgreementInfo info);

		UserAgreementEDocRequestHelper LinkHelper
		{
			get { return linkHelper ?? (linkHelper = new UserAgreementEDocRequestHelper()); }
		}
		UserAgreementEDocRequestHelper linkHelper;

		protected void AcknowledgeUserAgreementCore(ITrustedContext<bool> context, TUserAgreementRequest requestInfo)
		{
			if (!context.Success)
			{
				return;
			}

			var database = GetLicenceDatabase(context, requestInfo);
			if (database == null)
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Authorization_ActionNotPermitted, "Access is not granted.");
				return;
			}

			var requestDataValidation = GetUserAgreementSubmissionRequestDataValidation(requestInfo);
			requestDataValidation.ValidateSubmissionRequestData();
			if (requestDataValidation.ErrorMessageBuilder.Messages.Count > 0)
			{
				context.Messages = requestDataValidation.ErrorMessageBuilder;
				return;
			}

			var agreementInfo = GetCurrentAgreementSafe(database, requestInfo.UserAgreementType, requestInfo.UserCountry);

			if (!agreementInfo.AllowOnlineAcceptance)
			{
				context.Messages = new ErrorMessages(ErrorCodes.Codes.Authorization_ActionNotPermitted, AgreementIsNotAllowedOnlineAcceptance);
				return;
			}

			var agreement = agreementInfo?.Agreement;
			EdiCustomerUserAccount userAccount = null;
			if (!string.IsNullOrEmpty(requestInfo.UserId))
			{
				userAccount = GetUserAccountFromUserID(Factory, database, requestInfo.UserId);
				requestDataValidation.CheckUserAgreementShouldExistAndNotBeAccepted(agreement, userAccount);

				if (userAccount == null)
				{
					userAccount = EdiCustomerUserAccount.CreateNewUserAccount(Factory, database, requestInfo.UserId, requestInfo.FullName, requestInfo.Email, requestInfo.UserCountry);
				}
			}

			if (requestDataValidation.ErrorMessageBuilder.Messages.Count > 0)
			{
				context.Messages = requestDataValidation.ErrorMessageBuilder;
				return;
			}

			var hasDatabaseLevelAssignment = agreementInfo.Assignment != null && agreementInfo.Assignment.EAE_ParentTableCode.EqualsIgnoringCase(LicenceDatabaseSchema.Constants.Prefix);
			UserAgreementHelper.AddAgreementLogToAccount(Factory, agreement, requestInfo, userAccount, database, hasDatabaseLevelAssignment ? null : database?.LicEnterprise);
			Factory.Save();

			if (requestInfo.ShouldSendAgreementCopy && !string.IsNullOrWhiteSpace(requestInfo.Email))
			{
				UserAgreementHelper.SendAgreementCopy(database.Factory, new EdiUserAgreementWrapper()
				{
					UserAgreement = agreement,
					UserInfo = requestInfo,
					Database = database,
				});
			}

			context.ResponseInfo = true;
		}

		protected virtual UserAgreementSubmissionRequestDataValidation GetUserAgreementSubmissionRequestDataValidation(TUserAgreementRequest requestInfo)
		{
			return new UserAgreementSubmissionRequestDataValidation(requestInfo);
		}

		bool HasAcknowledgedAgreement(EdiUserAgreement userAgreement, EdiCustomerUserAccount userAccount, LicenceDatabase database)
		{
			return userAgreement.Level.EqualsIgnoringCase(EdiUserAgreementLevelList.Codes.Corporate) ?
					userAgreement.HasAcknowledgedCorporateAgreement(database) :
					userAccount != null ?
						userAccount.HasAcknowledgedUserAgreement(userAgreement) :
						userAgreement.HasBeenAcceptedByOrganisation(database.WebAccessOrg);
		}

		EdiUserAgreementInfo GetCurrentAgreementSafe(LicenceDatabase database, string agreementType, string countryCode)
		{
			return EdiUserAgreement.GetCurrentAgreement(Factory, database, agreementType, countryCode);
		}

		protected void GetAcceptancesCore(ITrustedContext<GetAcceptancesResponse> context, TGetAcceptancesRequest requestInfo)
		{
			var factory = new BusinessObjectFactory
			{
				RefreshEnabled = false,
				NameForDebugging = "GetAcceptancesCore",
			};

			var database = GetLicenceDatabase(context, requestInfo);
			var enterprise = database?.LicEnterprise;
			if (enterprise == null)
			{
				return;
			}
			var query = new ZDBOnlyQuery(typeof(EdiUserAgreementAcceptanceLog));
			query.AddToFilter(EdiUserAgreementAcceptanceLogSchema.EUL_Type, requestInfo.UserAgreementType);
			var parentSubQuery = new ZDBOnlySubQuery(typeof(EdiUserAgreementAcceptanceLog), EdiUserAgreementAcceptanceLogSchema.PK);
			parentSubQuery.AddToFilter(JoinCondition.Or, EdiUserAgreementAcceptanceLogSchema.EUL_LE, enterprise.PK);
			parentSubQuery.AddToFilter(JoinCondition.Or, EdiUserAgreementAcceptanceLogSchema.EUL_LD, database.PK);
			query.AddSubQuery(parentSubQuery, JoinCondition.And);
			var acceptanceLogs = factory.Load<EdiUserAgreementAcceptanceLog>(query);

			var response = new GetAcceptancesResponse();

			foreach (var log in acceptanceLogs)
			{
				if (log.UserAgreement != null)
				{
					var info = new LicenseAcceptanceResponse();
					info.AcceptedByEmail = log.EUL_AcceptedByEmail;
					info.AcceptedIPAddresss = log.EUL_AcceptedByIPAddress;
					info.AcceptedByName = log.EUL_AcceptedByName;
					info.AcceptedTimeUtc = log.EUL_AcceptanceTimeUtc.ToDateTime();
					info.Content = log.UserAgreement.ERA_Content;
					info.Title = log.UserAgreement.ERA_Title;
					info.EffectiveStartUtc = log.UserAgreement.ERA_EffectiveTimeUtc.ToDateTime();
					info.MajorVersion = log.EUL_MajorVersion;
					info.MinorVersion = log.EUL_MinorVersion;
					info.Variant = log.EUL_VariantCode;
					info.Type = log.EUL_Type;

					response.Acceptances.Add(info);
				}
			}

			context.ResponseInfo = response;
			context.Success = true;
		}

		#region User Account

		EdiCustomerUserAccount GetUserAccountFromUserID(BusinessObjectFactory factory, LicenceDatabase licenceDatabase, string userID)
		{
			var userAccountQuery = new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, userID);
			userAccountQuery.AddToFilter(EdiCustomerUserAccountSchema.EUA_LD, licenceDatabase.PK);
			return factory.LoadTop1<EdiCustomerUserAccount>(userAccountQuery);
		}

		#endregion User Account
	}
}
