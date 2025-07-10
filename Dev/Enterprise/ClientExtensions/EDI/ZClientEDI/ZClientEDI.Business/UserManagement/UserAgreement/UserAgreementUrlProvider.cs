using System;
using System.Net;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json.Linq;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public static class UserAgreementUrlProvider
	{
		public const int TokenKeepsAliveDays = 7;

		public static string GetMyAccountUserAgreementUrlForDatabase(LicenceDatabase database, string source, string agreementType, bool sendAgreementCopy)
		{
			if (database == null)
			{
				return "!URL can only be generated for a database.!";
			}

			var tokenScopeObj = new JObject
			{
				[SourceKey] = source,
				[TypeKey] = UserAgreementTokenTypes.Verify,
				[DatabaseKey] = database.PK.ToString(),
				[AgreementTypeKey] = agreementType,
				[RecipientNameKey] = string.Empty,
				[RecipientJobTitleKey] = string.Empty,
				[RecipientEmailKey] = string.Empty,
				[SendAgreementCopyKey] = sendAgreementCopy
			};

			var tokenInfo = new AccessTokenInfo(tokenScopeObj.ToString(), database.PK.ToGuid(), LicenceDatabaseSchema.Constants.Prefix);
			return GetUrl(tokenInfo);
		}

		public static string GetMyAccountUserAgreementUrlFromContact(BusinessObject parent, OrgContact contact, string source, string agreementType, bool sendAgreementCopy)
		{
			if (contact == null)
			{
				return "RECIPIENT-IS-NOT-CONTACT";
			}
			if (!EdiUserAgreementTypesMapper.IsAgreementTypeValid(agreementType))
			{
				return "INVALID-AGREEMENT-TYPE";
			}

			var tokenScopeObj = new JObject
			{
				[SourceKey] = source,
				[TypeKey] = UserAgreementTokenTypes.Verify,
				[FromContactKey] = contact.PK.ToString(),
				[AgreementTypeKey] = agreementType,
				[SendAgreementCopyKey] = sendAgreementCopy
			};

			var tokenInfo = new AccessTokenInfo(tokenScopeObj.ToString(), parent.PK.ToGuid(), parent.TablePrefix);
			return GetUrl(tokenInfo);
		}

		static string GetUrl(AccessTokenInfo tokenInfo)
		{
			var token = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.MyAccountUserAgreement, tokenInfo, TimeSpan.FromDays(TokenKeepsAliveDays), maxUses: 1);
			return EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value.Trim('/') + $"/Admin/UserAgreement.aspx?data={WebUtility.UrlEncode(token)}";
		}

		public const string SourceKey = "Source";
		public const string TypeKey = "Type";
		public const string FromContactKey = "FromContact";
		public const string DatabaseKey = "Database";
		public const string AgreementTypeKey = "AgreementType";
		public const string RecipientNameKey = "RecipientName";
		public const string RecipientJobTitleKey = "RecipientJobTitle";
		public const string RecipientEmailKey = "RecipientEmail";
		public const string SendAgreementCopyKey = "SendAgreementCopy";
	}
}
