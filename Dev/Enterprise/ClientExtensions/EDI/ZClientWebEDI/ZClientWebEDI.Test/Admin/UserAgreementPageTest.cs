using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI.HtmlControls;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Client.EDI.UserManagement.Business.Testing;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;
using NUnit.Framework;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	[TestedType(typeof(UserAgreement))]
	public class UserAgreementPageTest : ZPageTestCase
	{
		public void TestInit_InvalidData()
		{
			HttpContext.Current.Request.QueryString.Add("data", "invalid");
			testPage.OnInitForTest();

			AssertEquals(302, HttpContext.Current.Response.StatusCode);

			var url = HttpContext.Current.Response.RedirectLocation;
			AssertContains("Notification.aspx", url);

			var query = NotificationAspxTest.LoadDataFromUr(url);
			AssertEquals(UserAgreement.Constants.NotificationTitle, query["Title"]);
			AssertEquals(UserAgreement.Constants.UrlExpiredMessage, query["Message"]);
		}

		public void TestOnInit_DeletededToken()
		{
			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			contact.OC_Email = "123@123.COM";
			var url = GetAgreementUrlFromCampaign(contact, EdiUserAgreementTypes.Codes.CargoWiseNext, false, out var campaignItem);

			var tokenObj = Factory.LoadTop1<StmAccessToken>(new ZQuery(StmAccessTokenSchema.SAT_Token, WebUtility.UrlDecode(url.Split('=')[1])));
			AssertNotNull(tokenObj);

			tokenObj.Delete();
			Factory.Save();

			HttpContext.Current.Request.QueryString.Add("data", url.Split('=')[1]);
			testPage.OnInitForTest();

			AssertContains("Page should redirect user to message page", nameof(Notification), HttpContext.Current.Response.RedirectLocation);
			var query = NotificationAspxTest.LoadDataFromUr(HttpContext.Current.Response.RedirectLocation);
			AssertEquals(UserAgreement.Constants.NotificationTitle, query["Title"]);
			AssertEquals(UserAgreement.Constants.UrlExpiredMessage, query["Message"]);
		}

		public void TestOnInit_ExpiredToken_NotFromCampaign()
		{
			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			contact.OC_Email = "123@123.COM";
			var url = GetAgreementUrlFromCampaign(contact, EdiUserAgreementTypes.Codes.CargoWiseNext, false, out var campaignItem);

			var tokenObj = Factory.LoadTop1<StmAccessToken>(new ZQuery(StmAccessTokenSchema.SAT_Token, WebUtility.UrlDecode(url.Split('=')[1])));
			tokenObj.SAT_ExpiresAt = ZDateTime.UtcNow.AddDays(-1);
			tokenObj.SAT_ParentTableCode = "XXX";
			Factory.Save();

			AssertNotNull(tokenObj);
			var tokenControl = (ITokenizedAccessControl)new TokenizedAccessControl();
			Assert("Should be an expiration token", !tokenControl.TryPeek(WebUtility.UrlDecode(url.Split('=')[1]), AccessTokenTypes.MyAccountUserAgreement, out var tokenInfo));

			HttpContext.Current.Request.QueryString.Add("data", url.Split('=')[1]);
			testPage.OnInitForTest();
			AssertContains("Page should redirect user to message page", nameof(Notification), HttpContext.Current.Response.RedirectLocation);
			var query = NotificationAspxTest.LoadDataFromUr(HttpContext.Current.Response.RedirectLocation);
			AssertEquals(UserAgreement.Constants.NotificationTitle, query["Title"]);
			AssertEquals(UserAgreement.Constants.UrlExpiredMessage, query["Message"]);
		}

		public void TestOnInit_ExpiredToken_CampaignSentUrlAlready()
		{
			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			contact.OC_Email = "123@123.COM";
			var url = GetAgreementUrlFromCampaign(contact, EdiUserAgreementTypes.Codes.CargoWiseNext, false, out var campaignItem);
			var tokenObj = Factory.LoadTop1<StmAccessToken>(new ZQuery(StmAccessTokenSchema.SAT_Token, WebUtility.UrlDecode(url.Split('=')[1])));
			tokenObj.SAT_ExpiresAt = ZDateTime.UtcNow.AddDays(-1);
			Factory.Save();

			campaignItem.G8_LastSentTimeUtc = ZDateTime.UtcNow.AddDays(-(UserAgreementUrlProvider.TokenKeepsAliveDays / 2));
			Factory.Save();

			AssertNotNull(tokenObj);
			var tokenControl = (ITokenizedAccessControl)new TokenizedAccessControl();
			Assert("Should be an expiration token", !tokenControl.TryPeek(WebUtility.UrlDecode(url.Split('=')[1]), AccessTokenTypes.MyAccountUserAgreement, out var tokenInfo));

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			HttpContext.Current.Request.QueryString.Add("data", url.Split('=')[1]);
			testPage.OnInitForTest();
			AssertContains("Page should redirect user to message page", nameof(Notification), HttpContext.Current.Response.RedirectLocation);
			var query = NotificationAspxTest.LoadDataFromUr(HttpContext.Current.Response.RedirectLocation);
			AssertEquals(UserAgreement.Constants.NotificationTitle, query["Title"]);
			AssertEquals(UserAgreement.Constants.NotificationResent, query["Message"]);
			AssertEquals(nameof(Notification.NotificationType.Success), query["NotificationType"]);

			AssertEquals("Should not create a new email", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestOnInit_ExpiredToken_ShouldResend()
		{
			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			contact.OC_Email = "123@123.COM";
			var url = GetAgreementUrlFromCampaign(contact, EdiUserAgreementTypes.Codes.CargoWiseNext, false, out var campaignItem);
			var decodedToken = WebUtility.UrlDecode(url.Split('=')[1]);

			campaignItem.G8_LastSentTimeUtc = ZDateTime.UtcNow.AddDays(-(UserAgreementUrlProvider.TokenKeepsAliveDays + 5));

			var tokenObj = Factory.LoadTop1<StmAccessToken>(new ZQuery(StmAccessTokenSchema.SAT_Token, decodedToken));
			tokenObj.SAT_ExpiresAt = ZDateTime.UtcNow.AddDays(-5);
			Factory.Save();

			AssertNotNull(tokenObj);
			var tokenControl = (ITokenizedAccessControl)new TokenizedAccessControl();
			Assert("Should be an expiration token", !tokenControl.TryPeek(decodedToken, AccessTokenTypes.MyAccountUserAgreement, out var tokenInfo));

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			HttpContext.Current.Request.QueryString.Add("data", url.Split('=')[1]);
			testPage.OnInitForTest();

			AssertContains("Page should redirect user to message page", nameof(Notification), HttpContext.Current.Response.RedirectLocation);
			var query = NotificationAspxTest.LoadDataFromUr(HttpContext.Current.Response.RedirectLocation);
			AssertEquals(UserAgreement.Constants.NotificationTitle, query["Title"]);
			AssertEquals(UserAgreement.Constants.NotificationResent, query["Message"]);
			AssertEquals(nameof(Notification.NotificationType.Success), query["NotificationType"]);

			AssertNull("token should be deleted", Factory.LoadTop1<StmAccessToken>(new ZQuery(StmAccessTokenSchema.SAT_Token, decodedToken)));

			AssertEquals("Should not create a new email", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var emailToken = Env.OutgoingMailManager.EmailsCreated[0].Body.Split('#')[1].Split('=')[1];
			Assert(tokenControl.TryPeek(emailToken, AccessTokenTypes.MyAccountUserAgreement, out var tokenInfo2));
			var queryToken = UserAgreementQueryToken.FromJson(tokenInfo2.Scope);
			AssertEquals("Campaign", queryToken.Source);
			AssertEquals(UserAgreementTokenTypes.Verify, queryToken.Type);
			AssertEquals(contact.PK, queryToken.FromContact);
		}

		public void TestOnInit_ValidToken_Contact()
		{
			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			Factory.Save();

			var token = GetToken(contact, null, UserAgreementTokenTypes.Verify, EdiUserAgreementTypes.Codes.CargoWiseNext, "123@123.com", "Name", "Title", true);
			HttpContext.Current.Request.QueryString.Add("data", token);
			testPage.OnInitForTest();

			AssertNotContains(nameof(Notification), HttpContext.Current.Response.RedirectLocation);
			AssertEquals("source", queryToken.Source);
			AssertEquals(UserAgreementTokenTypes.Verify, queryToken.Type);
			AssertEquals(contact.PK, queryToken.FromContact);
			Assert(queryToken.Enterprise.IsEmpty);
			AssertEquals(EdiUserAgreementTypes.Codes.CargoWiseNext, queryToken.AgreementType);
			AssertEquals("Name", queryToken.RecipientName);
			AssertEquals("Title", queryToken.RecipientJobTitle);
			AssertEquals("123@123.com", queryToken.RecipientEmail);
		}

		public void TestOnInit_ValidToken_Enterprise()
		{
			UserAgreementHelperTest.SetupLicenceEnterprise(Factory, out var enterprise, out var _, out var _);
			Factory.Save();

			var token = GetToken(null, enterprise, UserAgreementTokenTypes.Verify, EdiUserAgreementTypes.Codes.CargoWiseNext, "123@123.com", "Name", "Title", true);
			HttpContext.Current.Request.QueryString.Add("data", token);
			testPage.OnInitForTest();

			AssertNotContains(nameof(Notification), HttpContext.Current.Response.RedirectLocation);
			AssertEquals("source", queryToken.Source);
			AssertEquals(UserAgreementTokenTypes.Verify, queryToken.Type);
			AssertEquals(ZGuid.Empty, queryToken.FromContact);
			AssertEquals(enterprise.PK, queryToken.Enterprise);
			AssertEquals(EdiUserAgreementTypes.Codes.CargoWiseNext, queryToken.AgreementType);
			AssertEquals("Name", queryToken.RecipientName);
			AssertEquals("Title", queryToken.RecipientJobTitle);
			AssertEquals("123@123.com", queryToken.RecipientEmail);
		}

		public void TestOnInit_AcceptTokenShouldHaveEmail()
		{
			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			Factory.Save();

			var token = GetToken(contact, null, UserAgreementTokenTypes.Accept, EdiUserAgreementTypes.Codes.CargoWiseNext, string.Empty, "Name", "Title", true);
			HttpContext.Current.Request.QueryString.Add("data", token);
			testPage.OnInitForTest();

			AssertContains("Page should redirect user to message page", nameof(Notification), HttpContext.Current.Response.RedirectLocation);
			var query = NotificationAspxTest.LoadDataFromUr(HttpContext.Current.Response.RedirectLocation);
			AssertEquals(UserAgreement.Constants.NotificationTitle, query["Title"]);
			AssertEquals(UserAgreement.Constants.UrlExpiredMessage, query["Message"]);

			AssertEquals(UserAgreementTokenTypes.Accept, queryToken.Type);
			AssertNullOrEmpty(queryToken.RecipientEmail);

			AssertEquals("source", queryToken.Source);
			AssertEquals(contact.PK, queryToken.FromContact);
			Assert(queryToken.Enterprise.IsEmpty);
			AssertEquals(EdiUserAgreementTypes.Codes.CargoWiseNext, queryToken.AgreementType);
			AssertEquals("Name", queryToken.RecipientName);
			AssertEquals("Title", queryToken.RecipientJobTitle);
		}

		public void TestDataSource()
		{
			AssertNull(queryToken);
			AssertNull(tokenEnterprise);

			AssertNull(testPage.GetNewDataSourceForTest());
			AssertNull("DataSource(tokenEnterprise) should be null when missing query token", tokenEnterprise);
			AssertNull("`fromContact` should be null when missing query token", fromContact);
			AssertNull("`agreementInfo` should be null when missing query token", agreementInfo);
			AssertNull("`currentUserAgreementResponse` should be null when missing query token", currentUserAgreementResponse);

			var agreement = UserAgreementHelperTest.SetupAgreement(TestConnection, Factory, EdiUserAgreementTypes.Codes.CargoWiseNext, String.Empty, ZDateTime.UtcNow.AddMonths(-1));
			UserAgreementHelperTest.SetupLicenceEnterprise(Factory, out var enterprise, out var database, out var trustedSystem);
			UserAgreementHelperTest.AttachAgreementToEnterprise(agreement, enterprise);
			SetPageField("queryToken", new UserAgreementQueryToken
			{
				Source = "source",
				Type = UserAgreementTokenTypes.Verify,
				FromContact = ZGuid.NewZGuid(),
				Enterprise = enterprise.PK,
				AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext,
				RecipientName = "Name",
				RecipientJobTitle = "Title",
				RecipientEmail = "email@email.com"
			});

			AssertNotNull(queryToken);
			AssertNotNull(testPage.GetNewDataSourceForTest());
			AssertEquals(enterprise.PK, tokenEnterprise.PK);
			AssertNull("`fromContact` should be null when fromContact is invalid", fromContact);
			AssertEquals(agreement.ERA_Title, currentUserAgreementResponse.Title);
			Assert(currentUserAgreementResponse.AllowOnlineClickthrough);

			AssertEquals(EdiUserAgreementTypes.Codes.CargoWiseNext, agreementInfo.UserAgreementType);
			AssertEquals(enterprise.Organisation.OH_FullName, agreementInfo.ClientAgreementOrgFullName);
			AssertEquals(enterprise.Organisation.CountryCode, agreementInfo.UserCountry);
			AssertEquals("The user info should be from token because contact is null", "email@email.com", agreementInfo.Email);
			AssertEquals("The user info should be from token because contact is null", "Name", agreementInfo.FullName);
			AssertEquals("The user info should be from token because contact is null", "Title", agreementInfo.JobTitle);

			var contact = enterprise.Organisation.Contacts.AddNew();
			contact.OC_Email = "123@123.com";
			contact.OC_ContactName = "I DONT KNOW";
			contact.OC_Title = "WORLD BOSS";
			Factory.Save();

			SetPageField("queryToken", new UserAgreementQueryToken
			{
				Source = "source",
				Type = UserAgreementTokenTypes.Verify,
				FromContact = contact.PK,
				Enterprise = ZGuid.Empty,
				AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext,
				RecipientName = "Name",
				RecipientJobTitle = "Title",
				RecipientEmail = ""
			});
			AssertNotNull(queryToken);
			AssertNotNull(testPage.GetNewDataSourceForTest());
			AssertEquals(enterprise.PK, tokenEnterprise.PK);
			AssertEquals(contact.PK, fromContact.PK);
			AssertEquals(agreement.ERA_Title, currentUserAgreementResponse.Title);

			AssertEquals(EdiUserAgreementTypes.Codes.CargoWiseNext, agreementInfo.UserAgreementType);
			AssertEquals(enterprise.Organisation.OH_FullName, agreementInfo.ClientAgreementOrgFullName);
			AssertEquals(enterprise.Organisation.CountryCode, agreementInfo.UserCountry);
			AssertEquals("The user info should be from contact because this is a verify token", contact.Email, agreementInfo.Email);
			AssertEquals("The user info should be from contact because this is a verify token", contact.OC_ContactName, agreementInfo.FullName);
			AssertEquals("The user info should be from contact because this is a verify token", contact.OC_Title, agreementInfo.JobTitle);

			SetPageField("queryToken", new UserAgreementQueryToken
			{
				Source = "source",
				Type = UserAgreementTokenTypes.Accept,
				FromContact = contact.PK,
				Enterprise = ZGuid.Empty,
				AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext,
				RecipientName = "Name",
				RecipientJobTitle = "Title",
				RecipientEmail = "1111@2222.com"
			});
			AssertNotNull(testPage.GetNewDataSourceForTest());
			AssertEquals("The user info should be from token because this is a verify token", "1111@2222.com", agreementInfo.Email);
			AssertEquals("The user info should be from token because this is a verify token", "Name", agreementInfo.FullName);
			AssertEquals("The user info should be from token because this is a verify token", "Title", agreementInfo.JobTitle);
		}

		public void TestDataSource_Database()
		{
			AssertNull(queryToken);
			AssertNull(tokenEnterprise);

			AssertNull(testPage.GetNewDataSourceForTest());
			AssertNull("DataSource(tokenEnterprise) should be null when missing query token", tokenEnterprise);
			AssertNull("`fromContact` should be null when missing query token", fromContact);
			AssertNull("`agreementInfo` should be null when missing query token", agreementInfo);
			AssertNull("`currentUserAgreementResponse` should be null when missing query token", currentUserAgreementResponse);

			var agreement = UserAgreementHelperTest.SetupAgreement(TestConnection, Factory, EdiUserAgreementTypes.Codes.CargoWiseNext, String.Empty, ZDateTime.UtcNow.AddMonths(-1));
			UserAgreementHelperTest.SetupLicenceEnterprise(Factory, out var enterprise, out var database, out var trustedSystem);
			UserAgreementHelperTest.AttachAgreementToEnterprise(agreement, enterprise);
			SetPageField("queryToken", new UserAgreementQueryToken
			{
				Source = "source",
				Type = UserAgreementTokenTypes.Verify,
				FromContact = ZGuid.NewZGuid(),
				Database = database.PK,
				AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext,
				RecipientName = "Name",
				RecipientJobTitle = "Title",
				RecipientEmail = "email@email.com"
			});

			AssertNotNull(queryToken);
			AssertNotNull(testPage.GetNewDataSourceForTest());
			AssertEquals("Enterprise should be retrieved from database since there are no assignments", enterprise.PK, tokenEnterprise.PK);
			AssertNull("Database should not be stored since there are no assignments", tokenDatabase);
			AssertNull("`fromContact` should be null when fromContact is invalid", fromContact);
			AssertEquals(agreement.ERA_Title, currentUserAgreementResponse.Title);
			Assert(currentUserAgreementResponse.AllowOnlineClickthrough);

			AssertEquals(EdiUserAgreementTypes.Codes.CargoWiseNext, agreementInfo.UserAgreementType);
			AssertEquals(enterprise.Organisation.OH_FullName, agreementInfo.ClientAgreementOrgFullName);
			AssertEquals(enterprise.Organisation.CountryCode, agreementInfo.UserCountry);
			AssertEquals("The user info should be from token because contact is null", "email@email.com", agreementInfo.Email);
			AssertEquals("The user info should be from token because contact is null", "Name", agreementInfo.FullName);
			AssertEquals("The user info should be from token because contact is null", "Title", agreementInfo.JobTitle);
		}

		public void TestDataSource_DatabaseLevelAgreement_FromContact()
		{
			EdiUserAgreementTest.DisableEffectiveDateTriggers(TestConnection);
			var agreement = Factory.New<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(-2);
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VariantDescription = "VA1";

			var database1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var database2 = Factory.NewWithValidTestData<LicenceDatabase>();
			var database3 = Factory.NewWithValidTestData<LicenceDatabase>();
			var database4 = Factory.NewWithValidTestData<LicenceDatabase>();
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			enterprise.LE_OH = org2.PK;
			database1.LD_LE = enterprise.PK;
			database2.LD_LE = enterprise.PK;
			database3.LD_LE = enterprise.PK;
			database4.LD_LE = enterprise.PK;
			Factory.Save();

			var assignment1 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment1.Parent = database1;
			assignment1.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment1.EAE_VariantCode = "VA1";
			assignment1.EAE_OH_ClientAgreementOrg = org1.PK;
			var assignment2 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment2.Parent = database2;
			assignment2.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment2.EAE_VariantCode = "VA1";
			assignment2.EAE_OH_ClientAgreementOrg = org1.PK;
			var assignment3 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment3.Parent = database3;
			assignment3.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment3.EAE_VariantCode = "VA1";
			assignment3.EAE_OH_ClientAgreementOrg = org2.PK;
			var assignment4 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment4.Parent = database4;
			assignment4.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment4.EAE_VariantCode = "VA1";
			assignment4.EAE_OH_ClientAgreementOrg = org1.PK;

			var contact = org1.Contacts.AddNew();
			contact.OC_Email = "123@123.com";
			contact.OC_ContactName = "I DONT KNOW";
			contact.OC_Title = "WORLD BOSS";

			var info = new EnterpriseUserAgreementInfo
			{
				UserAgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext,
				Product = ProductTypes.Codes.CargoWiseOne,
				Email = "alex@contact.com",
				FullName = "Alex",
				JobTitle = "Developer",
				ShouldSendAgreementCopy = false
			};
			UserAgreementHelper.AcknowledgeEnterpriseUserAgreement(database4, info);
			Factory.Save();

			AssertNull(queryToken);
			AssertNull(tokenEnterprise);

			AssertNull(testPage.GetNewDataSourceForTest());
			AssertNull("DataSource(tokenEnterprise) should be null when missing query token", tokenEnterprise);
			AssertNull("`fromContact` should be null when missing query token", fromContact);
			AssertNull("`agreementInfo` should be null when missing query token", agreementInfo);
			AssertNull("`currentUserAgreementResponse` should be null when missing query token", currentUserAgreementResponse);

			SetPageField("queryToken", new UserAgreementQueryToken
			{
				Source = "source",
				Type = UserAgreementTokenTypes.Verify,
				FromContact = contact.PK,
				Enterprise = ZGuid.Empty,
				AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext,
				RecipientName = "I DONT KNOW",
				RecipientJobTitle = "WORLD BOSS",
				RecipientEmail = "123@123.com"
			});

			AssertNotNull(queryToken);
			AssertNotNull(testPage.GetNewDataSourceForTest());
			AssertNull("Should not have an enterprise, since we're at database level", tokenEnterprise);
			AssertEquals(contact.PK, fromContact.PK);
			AssertEquals("Should have a client agreement org, since we're at database level", org1.PK, clientAgreementOrg.PK);
			AssertEquals("Should only contain databases with assignments for the agreement and the given client agreement org", 2, databases.Count());
			AssertNotNull("Should contain databases with assignments for the agreement and the given client agreement org", databases.FirstOrDefault(x => x.PK == database1.PK));
			AssertNotNull("Should contain databases with assignments for the agreement and the given client agreement org", databases.FirstOrDefault(x => x.PK == database2.PK));
			AssertNull("Should not contain databases with a different client agreement org", databases.FirstOrDefault(x => x.PK == database3.PK));
			AssertNull("Should not contain databases which have already accepted the agreement", databases.FirstOrDefault(x => x.PK == database4.PK));
			AssertEquals(agreement.ERA_Title, currentUserAgreementResponse.Title);
			Assert(currentUserAgreementResponse.AllowOnlineClickthrough);

			AssertEquals(EdiUserAgreementTypes.Codes.CargoWiseNext, agreementInfo.UserAgreementType);
			AssertEquals(clientAgreementOrg.OH_FullName, agreementInfo.ClientAgreementOrgFullName);
			AssertEquals(clientAgreementOrg.CountryCode, agreementInfo.UserCountry);
			AssertEquals("The user info should be from token because contact is null", "123@123.com", agreementInfo.Email);
			AssertEquals("The user info should be from token because contact is null", "I DONT KNOW", agreementInfo.FullName);
			AssertEquals("The user info should be from token because contact is null", "WORLD BOSS", agreementInfo.JobTitle);
		}

		public void TestDataSource_DatabaseLevelAgreement_DatabaseToken()
		{
			EdiUserAgreementTest.DisableEffectiveDateTriggers(TestConnection);
			var agreement = Factory.New<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(-2);
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VariantDescription = "VA1";

			var database1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var database2 = Factory.NewWithValidTestData<LicenceDatabase>();
			var database3 = Factory.NewWithValidTestData<LicenceDatabase>();
			var database4 = Factory.NewWithValidTestData<LicenceDatabase>();
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			enterprise.LE_OH = org2.PK;
			database1.LD_LE = enterprise.PK;
			database2.LD_LE = enterprise.PK;
			database3.LD_LE = enterprise.PK;
			database4.LD_LE = enterprise.PK;
			Factory.Save();

			var assignment1 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment1.Parent = database1;
			assignment1.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment1.EAE_VariantCode = "VA1";
			assignment1.EAE_OH_ClientAgreementOrg = org1.PK;
			var assignment2 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment2.Parent = database2;
			assignment2.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment2.EAE_VariantCode = "VA1";
			assignment2.EAE_OH_ClientAgreementOrg = org1.PK;
			var assignment3 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment3.Parent = database3;
			assignment3.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment3.EAE_VariantCode = "VA1";
			assignment3.EAE_OH_ClientAgreementOrg = org2.PK;
			var assignment4 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment4.Parent = database4;
			assignment4.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment4.EAE_VariantCode = "VA1";
			assignment4.EAE_OH_ClientAgreementOrg = org1.PK;

			var contact = org1.Contacts.AddNew();
			contact.OC_Email = "123@123.com";
			contact.OC_ContactName = "I DONT KNOW";
			contact.OC_Title = "WORLD BOSS";

			var info = new EnterpriseUserAgreementInfo
			{
				UserAgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext,
				Product = ProductTypes.Codes.CargoWiseOne,
				Email = "alex@contact.com",
				FullName = "Alex",
				JobTitle = "Developer",
				ShouldSendAgreementCopy = false
			};
			UserAgreementHelper.AcknowledgeEnterpriseUserAgreement(database4, info);
			Factory.Save();

			AssertNull(queryToken);
			AssertNull(tokenEnterprise);

			AssertNull(testPage.GetNewDataSourceForTest());
			AssertNull("DataSource(tokenEnterprise) should be null when missing query token", tokenEnterprise);
			AssertNull("`fromContact` should be null when missing query token", fromContact);
			AssertNull("`agreementInfo` should be null when missing query token", agreementInfo);
			AssertNull("`currentUserAgreementResponse` should be null when missing query token", currentUserAgreementResponse);

			SetPageField("queryToken", new UserAgreementQueryToken
			{
				Source = "source",
				Type = UserAgreementTokenTypes.Verify,
				FromContact = Guid.Empty,
				Database = database1.PK,
				AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext,
				RecipientName = "I DONT KNOW",
				RecipientJobTitle = "WORLD BOSS",
				RecipientEmail = "123@123.com"
			});

			AssertNotNull(queryToken);
			AssertNotNull(testPage.GetNewDataSourceForTest());
			AssertNull("Should not have an enterprise, since we're at database level", tokenEnterprise);
			AssertNull("Should not have a fromContact, since we're coming from db", fromContact);
			AssertEquals("Should have a client agreement org, since we're at database level", org1.PK, clientAgreementOrg.PK);
			AssertEquals("Database should be retrieved from token", database1.PK, tokenDatabase.PK);
			AssertEquals("Should contain databases with assignments for the agreement and client agreement orgs matching the passed in database", 2, databases.Count());
			AssertNotNull("Should contain specified database", databases.FirstOrDefault(x => x.PK == database1.PK));
			AssertNotNull("Should contain database since it has the same client agreement org", databases.FirstOrDefault(x => x.PK == database2.PK));
			AssertNull("Should not contain databases with a different client agreement org", databases.FirstOrDefault(x => x.PK == database3.PK));
			AssertNull("Should not contain databases which have already accepted the agreement", databases.FirstOrDefault(x => x.PK == database4.PK));
			AssertEquals(agreement.ERA_Title, currentUserAgreementResponse.Title);
			Assert(currentUserAgreementResponse.AllowOnlineClickthrough);

			AssertEquals(EdiUserAgreementTypes.Codes.CargoWiseNext, agreementInfo.UserAgreementType);
			AssertEquals(clientAgreementOrg.OH_FullName, agreementInfo.ClientAgreementOrgFullName);
			AssertEquals(clientAgreementOrg.CountryCode, agreementInfo.UserCountry);
			AssertEquals("The user info should be from token because contact is null", "123@123.com", agreementInfo.Email);
			AssertEquals("The user info should be from token because contact is null", "I DONT KNOW", agreementInfo.FullName);
			AssertEquals("The user info should be from token because contact is null", "WORLD BOSS", agreementInfo.JobTitle);
		}

		public void TestAgreementInfo()
		{
			var agreement = UserAgreementHelperTest.SetupAgreement(TestConnection, Factory, EdiUserAgreementTypes.Codes.CargoWiseNext, String.Empty, ZDateTime.UtcNow.AddMonths(-1));
			UserAgreementHelperTest.SetupLicenceEnterprise(Factory, out var enterprise, out _, out _);
			UserAgreementHelperTest.AttachAgreementToEnterprise(agreement, enterprise);

			enterprise.Organisation.MainAddress.Address1 = "72 O'Riordan Street";
			enterprise.Organisation.OH_FullName = "Org 1";
			enterprise.Organisation.OH_RL_NKClosestPort = "AUSYD";
			var cusCode = Factory.NewWithValidTestData<OrgCusCode>();
			cusCode.OK_CodeType = "ABN";
			cusCode.OK_RN_NKCodeCountry = "AU";
			cusCode.OK_CustomsRegNo = "123";
			cusCode.OK_OH = enterprise.Organisation.PK;
			var contact = enterprise.Organisation.Contacts.AddNew();
			contact.OC_Email = "123@123.com";
			contact.OC_ContactName = "I DONT KNOW";
			contact.OC_Title = "WORLD BOSS";
			Factory.Save();

			SetPageField("queryToken", new UserAgreementQueryToken
			{
				Source = "source",
				Type = UserAgreementTokenTypes.Verify,
				FromContact = contact.PK,
				Enterprise = ZGuid.Empty,
				AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext,
				RecipientName = "Name",
				RecipientJobTitle = "Title",
				RecipientEmail = ""
			});
			AssertNotNull(queryToken);
			AssertNotNull(testPage.GetNewDataSourceForTest());
			AssertEquals(enterprise.PK, tokenEnterprise.PK);
			AssertEquals(contact.PK, fromContact.PK);
			AssertEquals("72 O'Riordan Street, New South Wales, Australia", agreementInfo.ClientMainAddressInSingleLine);
			AssertEquals(enterprise.Organisation.OH_FullName, agreementInfo.ClientAgreementOrgFullName);
			AssertEquals("ABN 123", agreementInfo.ClientBusinessRegistration);
			AssertEquals(enterprise.Organisation.CountryCode, agreementInfo.UserCountry);
		}

		public void TestAgreementInfo_DatabaseLevelAgreement()
		{
			var agreement = Factory.New<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(2);
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VariantDescription = "VA1";

			var database1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var database2 = Factory.NewWithValidTestData<LicenceDatabase>();
			var database3 = Factory.NewWithValidTestData<LicenceDatabase>();
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			enterprise.LE_OH = org2.PK;
			database1.LD_LE = enterprise.PK;
			database2.LD_LE = enterprise.PK;
			database3.LD_LE = enterprise.PK;
			Factory.Save();

			var assignment1 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment1.Parent = database1;
			assignment1.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment1.EAE_VariantCode = "VA1";
			assignment1.EAE_OH_ClientAgreementOrg = org1.PK;
			var assignment2 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment2.Parent = database2;
			assignment2.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment2.EAE_VariantCode = "VA1";
			assignment2.EAE_OH_ClientAgreementOrg = org1.PK;
			var assignment3 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment3.Parent = database3;
			assignment3.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment3.EAE_VariantCode = "VA1";
			assignment3.EAE_OH_ClientAgreementOrg = org2.PK;

			org1.MainAddress.Address1 = "72 O'Riordan Street";
			org1.OH_FullName = "Org 1";
			org1.OH_RL_NKClosestPort = "AUSYD";
			var cusCode = Factory.NewWithValidTestData<OrgCusCode>();
			cusCode.OK_CodeType = "ABN";
			cusCode.OK_RN_NKCodeCountry = "AU";
			cusCode.OK_CustomsRegNo = "123";
			cusCode.OK_OH = org1.PK;

			var contact = org1.Contacts.AddNew();
			contact.OC_Email = "123@123.com";
			contact.OC_ContactName = "I DONT KNOW";
			contact.OC_Title = "WORLD BOSS";
			Factory.Save();

			SetPageField("queryToken", new UserAgreementQueryToken
			{
				Source = "source",
				Type = UserAgreementTokenTypes.Verify,
				FromContact = contact.PK,
				Enterprise = ZGuid.Empty,
				AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext,
				RecipientName = "Name",
				RecipientJobTitle = "Title",
				RecipientEmail = ""
			});
			AssertNotNull(queryToken);
			AssertNotNull(testPage.GetNewDataSourceForTest());
			AssertNull(tokenEnterprise);
			AssertEquals(contact.PK, fromContact.PK);
			AssertEquals("72 O'Riordan Street, New South Wales, Australia", agreementInfo.ClientMainAddressInSingleLine);
			AssertEquals(org1.OH_FullName, agreementInfo.ClientAgreementOrgFullName);
			AssertEquals("ABN 123", agreementInfo.ClientBusinessRegistration);
			AssertEquals(org1.CountryCode, agreementInfo.UserCountry);
		}

		public void TestAcceptButton_Click()
		{
			var agreement = UserAgreementHelperTest.SetupAgreement(TestConnection, Factory, EdiUserAgreementTypes.Codes.CargoWiseNext, String.Empty, ZDateTime.UtcNow.AddMonths(-1));
			UserAgreementHelperTest.SetupLicenceEnterprise(Factory, out var enterprise, out _, out _);
			UserAgreementHelperTest.AttachAgreementToEnterprise(agreement, enterprise);
			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			contact.OC_OH = enterprise.Organisation.PK;
			contact.OC_Email = "123@123.com";
			contact.OC_ContactName = "I DONT KNOW";
			contact.OC_Title = "WORLD BOSS";
			Factory.Save();

			var token = GetToken(contact, enterprise, UserAgreementTokenTypes.Accept, EdiUserAgreementTypes.Codes.CargoWiseNext, contact.OC_Email, contact.OC_ContactName, contact.OC_Title, true);
			HttpContext.Current.Request.QueryString.Add("data", token);
			testPage.OnInitForTest();
			AssertNotNull(testPage.GetNewDataSourceForTest());
			testPage.AcceptButton_ClickForTest();

			var acceptanceLogQuery = new ZQuery(EdiUserAgreementAcceptanceLogSchema.EUL_ERA, agreement.PK);
			acceptanceLogQuery.AddToFilter(EdiUserAgreementAcceptanceLogSchema.EUL_LE, enterprise.PK);
			var acceptanceLogs = Factory.Load<EdiUserAgreementAcceptanceLog>(new ZQuery(acceptanceLogQuery));
			AssertEquals("Should have added an acceptance log for the enterprise", 1, acceptanceLogs.Length);
		}

		public void TestAcceptButton_Click_DatabaseLevelAgreement()
		{
			EdiUserAgreementTest.DisableEffectiveDateTriggers(TestConnection);
			var agreement = Factory.New<EdiUserAgreement>();
			agreement.ERA_Type = EdiUserAgreementTypes.Codes.CargoWiseNext;
			agreement.ERA_EffectiveTimeUtc = ZDateTime.UtcNow.AddMonths(-2);
			agreement.ERA_VariantCode = "VA1";
			agreement.ERA_VariantDescription = "VA1";

			var database1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var database2 = Factory.NewWithValidTestData<LicenceDatabase>();
			var database3 = Factory.NewWithValidTestData<LicenceDatabase>();
			var enterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			enterprise.LE_OH = org2.PK;
			database1.LD_LE = enterprise.PK;
			database2.LD_LE = enterprise.PK;
			database3.LD_LE = enterprise.PK;
			Factory.Save();

			var assignment1 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment1.Parent = database1;
			assignment1.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment1.EAE_VariantCode = "VA1";
			assignment1.EAE_OH_ClientAgreementOrg = org1.PK;
			var assignment2 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment2.Parent = database2;
			assignment2.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment2.EAE_VariantCode = "VA1";
			assignment2.EAE_OH_ClientAgreementOrg = org1.PK;
			var assignment3 = Factory.NewWithValidTestData<EdiUserAgreementAssignment>();
			assignment3.Parent = database3;
			assignment3.EAE_AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext;
			assignment3.EAE_VariantCode = "VA1";
			assignment3.EAE_OH_ClientAgreementOrg = org2.PK;

			org1.MainAddress.Address1 = "72 O'Riordan Street";
			org1.OH_FullName = "Org 1";
			org1.OH_RL_NKClosestPort = "AUSYD";
			var cusCode = Factory.NewWithValidTestData<OrgCusCode>();
			cusCode.OK_CodeType = "ABN";
			cusCode.OK_RN_NKCodeCountry = "AU";
			cusCode.OK_CustomsRegNo = "123";
			cusCode.OK_OH = org1.PK;

			var contact = Factory.New<EDIOrgContact>();
			contact.OC_OH = org1.PK;
			contact.OC_Email = "123@123.com";
			contact.OC_ContactName = "I DONT KNOW";
			contact.OC_Title = "WORLD BOSS";
			Factory.Save();

			var token = GetToken(contact, null, UserAgreementTokenTypes.Accept, EdiUserAgreementTypes.Codes.CargoWiseNext, contact.OC_Email, contact.OC_ContactName, contact.OC_Title, true);
			HttpContext.Current.Request.QueryString.Add("data", token);
			testPage.OnInitForTest();
			AssertNotNull(testPage.GetNewDataSourceForTest());
			testPage.AcceptButton_ClickForTest();

			var acceptanceLogQuery = new ZQuery(EdiUserAgreementAcceptanceLogSchema.EUL_ERA, agreement.PK);
			var acceptanceLogs = Factory.Load<EdiUserAgreementAcceptanceLog>(new ZQuery(acceptanceLogQuery));
			AssertEquals("Should have added an acceptance log for each database assignment", 2, acceptanceLogs.Length);
			var db1AcceptanceLog = acceptanceLogs.FirstOrDefault(x => x.EUL_LD == database1.PK);
			var db2AcceptanceLog = acceptanceLogs.FirstOrDefault(x => x.EUL_LD == database2.PK);
			AssertNotNull("Should have added an acceptance log for each database assignment", db1AcceptanceLog);
			AssertNotNull("Should have added an acceptance log for each database assignment", db2AcceptanceLog);
			AssertEquals("Should not set LE for db level acceptances", true, db1AcceptanceLog.EUL_LE.IsEmpty);
			AssertEquals("Should not set LE for db level acceptances", true, db2AcceptanceLog.EUL_LE.IsEmpty);
			AssertNull("Should not have added an acceptance log for database without assignment", acceptanceLogs.FirstOrDefault(x => x.EUL_LD == database3.PK));
			AssertNull("Should not have added any acceptance logs with enteprise PK", acceptanceLogs.FirstOrDefault(x => x.EUL_LE == enterprise.PK));
		}

		public void TestMacrosForClickThroughContent()
		{
			var agreement = UserAgreementHelperTest.SetupAgreement(TestConnection, Factory, EdiUserAgreementTypes.Codes.CargoWiseNext, String.Empty, ZDateTime.UtcNow.AddMonths(-1));
			var doc1 = agreement.DocManagerInfo.AddFileOrDocument([1, 1, 1, 1], "TAX INVOICE - AUS00335939 - JUSINTHKG (30-Jun-23).pdf", "MLA");
			var doc2 = agreement.DocManagerInfo.AddFileOrDocument([1, 1, 1, 1], "TAX INVOICE - AUS00335939 - JUSINTHKG (30-Jun-23).pdf", "MLA");
			doc1.IsPublished = true;
			doc2.IsPublished = false;
			agreement.DocManagerInfo.Save();
			UserAgreementHelperTest.SetupLicenceEnterprise(Factory, out var enterprise, out var database, out var trustedSystem);
			UserAgreementHelperTest.AttachAgreementToEnterprise(agreement, enterprise);
			SetPageField("queryToken", new UserAgreementQueryToken
			{
				Source = "source",
				Type = UserAgreementTokenTypes.Verify,
				FromContact = ZGuid.NewZGuid(),
				Enterprise = enterprise.PK,
				AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext,
				RecipientName = "Name",
				RecipientJobTitle = "Title",
				RecipientEmail = "email@email.com"
			});

			AssertNotNull(queryToken);
			AssertNotNull(testPage.GetNewDataSourceForTest());
			AssertEquals(enterprise.PK, tokenEnterprise.PK);
			AssertNull("`fromContact` should be null when fromContact is invalid", fromContact);
			AssertEquals(agreement.ERA_Title, currentUserAgreementResponse.Title);
			Assert(currentUserAgreementResponse.Content.Contains("Enterprise User Agreement some org  Not on file   UserAgreementEDocRequestHandler.axd?qdata"));
		}

		public void TestOnLoadFailed_AllowOnlineClick()
		{
			var agreement = UserAgreementHelperTest.SetupAgreement(TestConnection, Factory, EdiUserAgreementTypes.Codes.CargoWiseNext, String.Empty, ZDateTime.UtcNow.AddMonths(-1));
			UserAgreementHelperTest.SetupLicenceEnterprise(Factory, out var enterprise, out var database, out var trustedSystem);
			var assignment = UserAgreementHelperTest.AttachAgreementToEnterprise(agreement, enterprise);
			assignment.EAE_AllowOnlineAcceptance = false;

			var contact = enterprise.Organisation.Contacts.AddNew();
			contact.OC_Email = "123@123.com";
			contact.OC_ContactName = "I DONT KNOW";
			contact.OC_Title = "WORLD BOSS";
			Factory.Save();

			SetPageField("queryToken", new UserAgreementQueryToken
			{
				Source = "source",
				Type = UserAgreementTokenTypes.Verify,
				FromContact = contact.PK,
				Enterprise = enterprise.PK,
				AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext,
				RecipientName = "Name",
				RecipientJobTitle = "Title",
				RecipientEmail = ""
			});
			testPage.OnLoadForTest();

			Assert(!currentUserAgreementResponse.AllowOnlineClickthrough);
			AssertContains("The user should be redirected to the message page", "my-account/Notification.aspx", HttpContext.Current.Response.RedirectLocation);
			var query = NotificationAspxTest.LoadDataFromUr(HttpContext.Current.Response.RedirectLocation);
			AssertEquals(UserAgreement.Constants.NotificationTitle, query["Title"]);
			AssertEquals(UserAgreement.Constants.CannotOnlineClickthrough, query["Message"]);
		}

		public void TestOnLoadSuccessfully_AllowOnlineClick()
		{
			var agreement = UserAgreementHelperTest.SetupAgreement(TestConnection, Factory, EdiUserAgreementTypes.Codes.CargoWiseNext, String.Empty, ZDateTime.UtcNow.AddMonths(-1));
			UserAgreementHelperTest.SetupLicenceEnterprise(Factory, out var enterprise, out var database, out var trustedSystem);
			var assignment = UserAgreementHelperTest.AttachAgreementToEnterprise(agreement, enterprise);
			assignment.EAE_AllowOnlineAcceptance = true;

			var contact = enterprise.Organisation.Contacts.AddNew();
			contact.OC_Email = "123@123.com";
			contact.OC_ContactName = "I DONT KNOW";
			contact.OC_Title = "WORLD BOSS";
			Factory.Save();

			SetPageField("queryToken", new UserAgreementQueryToken
			{
				Source = "source",
				Type = UserAgreementTokenTypes.Verify,
				FromContact = contact.PK,
				Enterprise = enterprise.PK,
				AgreementType = EdiUserAgreementTypes.Codes.CargoWiseNext,
				RecipientName = "Name",
				RecipientJobTitle = "Title",
				RecipientEmail = ""
			});
			testPage.OnLoadForTest();

			Assert(currentUserAgreementResponse.AllowOnlineClickthrough);
			AssertNullOrEmpty(HttpContext.Current.Response.RedirectLocation);
		}

		public void TestInvalidAgreementTypeShouldShowNotification()
		{
			var agreement = UserAgreementHelperTest.SetupAgreement(TestConnection, Factory, EdiUserAgreementTypes.Codes.CargoWiseNext, String.Empty, ZDateTime.UtcNow.AddMonths(-1));
			UserAgreementHelperTest.SetupLicenceEnterprise(Factory, out var enterprise, out var database, out var trustedSystem);
			var assignment = UserAgreementHelperTest.AttachAgreementToEnterprise(agreement, enterprise);
			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			Factory.Save();
			var token = GetToken(contact, enterprise, UserAgreementTokenTypes.Verify, "{CWN}", "Email", "Name", "Title", true);
			HttpContext.Current.Request.QueryString.Add("data", token);
			testPage.OnInitForTest();

			AssertContains("The user should be redirected to the message page", "my-account/Notification.aspx", HttpContext.Current.Response.RedirectLocation);
			var query = NotificationAspxTest.LoadDataFromUr(HttpContext.Current.Response.RedirectLocation);
			AssertEquals(UserAgreement.Constants.NotificationTitle, query["Title"]);
			AssertEquals(UserAgreement.Constants.AgreementTypeInvalid, query["Message"]);
		}

		string GetToken(EDIOrgContact contact, LicenceEnterprise enterprise, string tokenType, string agreementType, string email, string fullName, string jobTitle, bool sendCopy)
		{
			var tokenObj = new UserAgreementQueryToken
			{
				Source = "source",
				Type = tokenType,
				FromContact = contact?.PK ?? ZGuid.Empty,
				Enterprise = enterprise?.PK ?? ZGuid.Empty,
				AgreementType = agreementType,
				RecipientName = fullName,
				RecipientJobTitle = jobTitle,
				RecipientEmail = email,
				SendAgreementCopy = sendCopy,
			};

			var tokenControl = new TokenizedAccessControl();
			var verifyTokenInfo = new AccessTokenInfo(tokenObj.ToJson(), contact?.PK.ToGuid() ?? enterprise.PK.ToGuid(), contact?.TablePrefix ?? enterprise.TablePrefix);

			return tokenControl.CreateLimitedToken(AccessTokenTypes.MyAccountUserAgreement, verifyTokenInfo, TimeSpan.FromDays(90), maxUses: 1);
		}

		string GetAgreementUrlFromCampaign(EDIOrgContact contact, string agreementType, bool sendCopy, out GlbCompanyCampaignItem campaignItem)
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "123@123.com";
			staff.GS_Code = "111";
			Factory.Save();

			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			campaign.G0_Type = "PREAP";
			campaign.HtmlDocumentBlob = ZBlob.FromAscii($@"#(*GetMyAccountUserAgreementUrl({agreementType}, {sendCopy})*)#");
			campaign.G0_CampaignName = "uuuttt";
			campaign.G0_Category = "PRINT";
			campaign.G0_EstimatedStartedDate = ZDateTime.UtcNow.AddDays(-2);
			campaign.G0_GS_NKCampaignCoordinator = staff.GS_Code;
			campaign.G0_GS_NKCampaignManager = staff.GS_Code;
			campaign.G0_BroadcastVoteSurveyExam = "DRM";

			campaignItem = campaign.CampaignsItemsSent.AddNew();
			campaignItem.G8_RecipientTableCode = contact.TablePrefix;
			campaignItem.G8_RecipientID = contact.PK;
			Factory.Save();

			var sender = new GlbCompanyCampaignSender(campaignItem.CompanyCampaign, [campaignItem]);
			sender.ShouldContinueWithSending += (s, e) => true;

			sender.CheckAndSendCampaigns();

			var email = Env.OutgoingMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains("uuuttt"));

			return email.Body.Split('#')[1];
		}

		UserAgreementForTest testPage => Page as UserAgreementForTest;

		UserAgreementQueryToken queryToken => GetPageField<UserAgreementQueryToken>("queryToken");

		EDIOrgContact fromContact => GetPageManager().FromContact;
		LicenceEnterprise tokenEnterprise => GetPageManager().Enterprise;
		LicenceDatabase tokenDatabase => GetPageManager().Database;
		OrgHeader clientAgreementOrg => GetPageManager().ClientAgreementOrg;
		IEnumerable<LicenceDatabase> databases => GetPageManager().Databases;
		EnterpriseUserAgreementInfo agreementInfo => GetPageManager().AgreementInfo;
		UserAgreementResponseData currentUserAgreementResponse => GetPageManager().CurrentUserAgreementResponse;

		T GetPageField<T>(string name)
		{
			return (T)typeof(UserAgreement).GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(testPage);
		}

		UserAgreementManager GetPageManager()
		{
			return (UserAgreementManager)typeof(UserAgreement).GetField("userAgreementManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(testPage);
		}

		void SetPageField<T>(string name, T value)
		{
			typeof(UserAgreement).GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(testPage, value);
		}

		protected override ZPage GetNewZPage()
		{
			return new UserAgreementForTest();
		}

		class UserAgreementForTest : UserAgreement
		{
			public UserAgreementForTest()
			{
				VerifyEmailButton = new ZButton();
				AcceptButton = new ZButton();
				CancelButton = new ZButton();
				VerifyPanel = new HtmlGenericControl();
				AgreementContent = new System.Web.UI.WebControls.PlaceHolder();
				DatabaseList = new ZRepeater();
				EDocList = new ZRepeater();
				DocumentPanel = new HtmlGenericControl();
				JobTitleTextBox = new ZTextBox();
				FullNameTextBox = new ZTextBox();
				EmailAddressTextBox = new ZTextBox();
			}

			public void OnInitForTest() => OnInit(null);
			public void AcceptButton_ClickForTest() => AcceptButton_Click(null, EventArgs.Empty);

			public BusinessObject GetNewDataSourceForTest() => GetNewDataSource();

			public void OnLoadForTest() => OnLoad(null);

			public override void Dispose()
			{
				VerifyEmailButton.Dispose();
				AcceptButton.Dispose();
				CancelButton.Dispose();
				VerifyPanel.Dispose();
				base.Dispose();
			}
		}
	}
}
