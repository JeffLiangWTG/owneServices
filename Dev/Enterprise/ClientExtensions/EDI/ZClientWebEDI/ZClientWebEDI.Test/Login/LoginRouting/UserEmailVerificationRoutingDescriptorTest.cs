using System;
using System.Web;
using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	class UserEmailVerificationRoutingDescriptorTest : TestCaseWithFactory
	{
		public void TestGetRoutingPageUrl()
		{
			var originalUrl = new Uri("http://test.com");
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_UserID = "AAA";
			user.EUA_LD = db.PK;
			user.EUA_Email = "user@test.org";
			Factory.Save();
			var descriptor = new UserEmailVerificationRoutingDescriptor(originalUrl, db, user);
			descriptor.RoutingAction();
			var url = descriptor.RoutingUrl;
			AssertContains("Routing page url", "/Login/EmailSentNotification.aspx", url.ToString());
			var values = HttpUtility.ParseQueryString(url.Query);
			var queryString = new SecureQueryString(values["qdata"]);
			AssertEquals(true, new ZBool(queryString[UserEmailVerificationRoutingDescriptor.EmailVerificationSentUrlKey]));
			AssertEquals("user@test.org", queryString[UserEmailVerificationRoutingDescriptor.EmailVerificationAddressUrlKey]);
		}

		public void TestIsRoutingRequired()
		{
			var originalUrl = new Uri("http://test.com");
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_LicenceType = DatabaseTypes.Codes.Test;
			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "TST";
			user.EUA_FullName = "Test User";
			user.EUA_IsEmailVerificationRequired = true;
			Factory.Save();
			var descriptor = new UserEmailVerificationRoutingDescriptor(originalUrl, db, user);
			AssertEquals("User is required to verify email", true, descriptor.IsRoutingRequired);
			user.EUA_IsEmailVerificationRequired = false;
			Factory.Save();
			descriptor = new UserEmailVerificationRoutingDescriptor(originalUrl, db, user);
			AssertEquals("User has verified email", false, descriptor.IsRoutingRequired);
			db.LD_LicenceType = DatabaseTypes.Codes.Production;
			user.EUA_IsEmailVerificationRequired = true;
			Factory.Save();
			descriptor = new UserEmailVerificationRoutingDescriptor(originalUrl, db, user);
			AssertEquals("User is required to verify email", true, descriptor.IsRoutingRequired);
		}

		public void TestSendUserAccountEmailVerification_NoEUA_Email()
		{
			var originalUrl = new Uri("https://google.com.au");
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "TST";
			user.EUA_Email = string.Empty;
			Factory.Save();
			var descriptor = new UserEmailVerificationRoutingDescriptor(originalUrl, db, user);
			descriptor.RoutingAction();
			var url = descriptor.RoutingUrl;
			var values = HttpUtility.ParseQueryString(url.Query);
			var queryString = new SecureQueryString(values["qdata"]);
			AssertEquals(false, new ZBool(queryString[UserEmailVerificationRoutingDescriptor.EmailVerificationSentUrlKey]));
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			var query = new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.VerifyEmailToken);
			var accessToken = Factory.LoadTop1<StmAccessToken>(query);
			AssertNull(accessToken);
		}

		public void TestSendUserAccountEmailVerification_EmptyReturnUrl()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			var user = Factory.New<EdiCustomerUserAccount>();
			user.EUA_LD = db.PK;
			user.EUA_UserID = "TST";
			user.EUA_Email = "a@g.com";
			Factory.Save();
			var descriptor = new UserEmailVerificationRoutingDescriptor(null, db, user);
			descriptor.RoutingAction();
			var url = descriptor.RoutingUrl;
			var values = HttpUtility.ParseQueryString(url.Query);
			var queryString = new SecureQueryString(values["qdata"]);
			AssertEquals("Should redirect to say verification was successful", true, new ZBool(queryString[UserEmailVerificationRoutingDescriptor.EmailVerificationSentUrlKey]));
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(user.EUA_Email, Env.OutgoingMailManager.EmailsCreated[0].Recipients[0].Email);
			var query = new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.VerifyEmailToken);
			var accessToken = Factory.LoadTop1<StmAccessToken>(query);
			AssertNotNull(accessToken);
			AssertEquals(user.PK, accessToken.SAT_ParentId);
			var payloadMarker = accessToken.SAT_Scope.IndexOf(':');
			AssertEquals(UserEmailVerificationRoutingDescriptor.VerifyTestUserAccountTokenKey, accessToken.SAT_Scope.SubstringSafe(0, payloadMarker));
			var originalRequestUrl = accessToken.SAT_Scope.SubstringSafe(payloadMarker + 1);
			Assert("ReturnUrl should be empty", originalRequestUrl.IsEmpty);
		}
	}
}
