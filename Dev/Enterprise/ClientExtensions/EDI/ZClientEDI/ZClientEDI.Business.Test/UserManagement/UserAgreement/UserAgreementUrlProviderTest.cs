using System;
using System.Net;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json.Linq;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class UserAgreementUrlProviderTest : TestCaseWithFactory
	{
		public void TestGetMyAccountUserAgreementUrlForDatabase()
		{
			var lic = BillingTestHelper.CreateLicence(Factory, "ENT");
			var db = lic.Database;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			db.LD_OH_WebAccessOrg = org.PK;

			var agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			AssertEquals(EdiUserAgreementLevelList.Codes.Corporate, agreement.Level);
			Factory.Save();

			using (EDIDataRegistry.Instance.MyAccountSiteRootUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://myaccount"))
			{
				var requestSource = "requestSource";
				var url = UserAgreementUrlProvider.GetMyAccountUserAgreementUrlForDatabase(db, requestSource, EdiUserAgreementTypes.Codes.CargoWiseNext, sendAgreementCopy: true);

				AssertStartsWith("Should generate link to UserAgreement.aspx", "https://myaccount/Admin/UserAgreement.aspx?data=", url);
				var indexOfPreToken = url.IndexOf('=');
				var token = WebUtility.UrlDecode(url.Substring(indexOfPreToken + 1));
				var tokenControl = (ITokenizedAccessControl)new TokenizedAccessControl();

				AssertEquals("Token should be valid", true, tokenControl.TryPeek(token, AccessTokenTypes.MyAccountUserAgreement, out var tokenInfo));
				var scope = tokenInfo.Scope;
				var tokenObj = Factory.LoadTop1<StmAccessToken>(new ZQuery(StmAccessTokenSchema.SAT_Token, token));
				AssertEquals("The Token should expire after seven days", 6, (tokenObj.SAT_ExpiresAt - ZDateTime.UtcNow).Days);

				var tokenScopeObj = JObject.Parse(scope);
				AssertEquals(requestSource, tokenScopeObj[UserAgreementUrlProvider.SourceKey].ToString());
				AssertEquals(UserAgreementTokenTypes.Verify, tokenScopeObj[UserAgreementUrlProvider.TypeKey].ToString());
				AssertEquals(db.PK.ToString(), tokenScopeObj[UserAgreementUrlProvider.DatabaseKey].ToString());
				AssertEquals(EdiUserAgreementTypes.Codes.CargoWiseNext, tokenScopeObj[UserAgreementUrlProvider.AgreementTypeKey].ToString());
				AssertEquals(string.Empty, tokenScopeObj[UserAgreementUrlProvider.RecipientNameKey].ToString());
				AssertEquals(string.Empty, tokenScopeObj[UserAgreementUrlProvider.RecipientJobTitleKey].ToString());
				AssertEquals(string.Empty, tokenScopeObj[UserAgreementUrlProvider.RecipientEmailKey].ToString());
				AssertEquals(true, tokenScopeObj[UserAgreementUrlProvider.SendAgreementCopyKey].ToObject<bool>());
			}
		}
	}
}
