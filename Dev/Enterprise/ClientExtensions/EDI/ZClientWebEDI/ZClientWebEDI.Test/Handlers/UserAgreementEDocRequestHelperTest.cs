using System;
using System.Net;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[TestedType(typeof(UserAgreementEDocRequestHelper))]
	public class UserAgreementEDocRequestHelperTest : DataRequestHelperTestCase
	{
		public void TestGetHandlerUrl()
		{
			var factory = new BusinessObjectFactory();
			var agreement = factory.NewWithValidTestData<EdiUserAgreement>();
			var edoc = agreement.DocManagerInfo.AddFileOrDocument([1, 1, 1, 1], "TAX INVOICE - AUS00335939 - JUSINTHKG (30-Jun-23).pdf", "INV");
			var uniqueKey = edoc.UniqueKey;
			AssertEquals("Precondition", edoc, agreement.DocManagerInfo.AllEDocs.GetFromUniqueKey(uniqueKey.ToGuid()));
			agreement.DocManagerInfo.Save();

			var tokenInfo = new AccessTokenInfo(string.Empty, agreement.PK.ToGuid(), EdiUserAgreementSchema.Constants.TableName);
			var token = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.MyAccountUserAgreement, tokenInfo, TimeSpan.FromMinutes(15), maxUses: 1);

				var helper = new UserAgreementEDocRequestHelper();
			var url = helper.GetHandlerUrl(agreement.PK, uniqueKey.ToGuid(), token);

			var queryString = new SecureQueryString(WebUtility.UrlDecode(url.Substring(url.IndexOf("?qdata=") + "?qdata=".Length)));
			var success = ZGuid.TryParse(queryString.Get("Data"), out var queryStringPK);
			AssertEquals("Should be a GUID", true, success);
			AssertEquals("Should match the agreement", agreement.PK, queryStringPK);

			var tokenFromQueryString = queryString.Get(UserAgreementEDocRequestHelper.TokenKey);
			var tokenControl = (ITokenizedAccessControl)new TokenizedAccessControl();
			var isTokenValid = !string.IsNullOrEmpty(tokenFromQueryString);
			var tokenPeekSuccess = tokenControl.TryPeek(tokenFromQueryString, AccessTokenTypes.MyAccountUserAgreement, out _);
			AssertEquals("Token should be valid", true, tokenPeekSuccess);
		}

		#region Overrides

		protected override DataRequestHelper GetRequestHelper()
		{
			return new UserAgreementEDocRequestHelper();
		}

		protected override bool ExpectedEnableCache
		{
			get
			{
				return false;
			}
		}

		protected override bool ExpectedUseSecureQueryString
		{
			get
			{
				return true;
			}
		}

		protected override string ExpectedBaseUrl
		{
			get
			{
				return "UserAgreementEDocRequestHandler.axd";
			}
		}

		#endregion
	}
}
