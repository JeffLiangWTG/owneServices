using System;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.GB.Business.GbConstants;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(GlbExternalPassword_GB))]
	public class GlbExternalPassword_GBTests : GlbExternalPasswordTest<GlbExternalPassword_GB>
	{
		GlbExternalPassword_GB password;

		protected override void SetUp()
		{
			base.SetUp();

			password = Factory.New<GlbExternalPassword_GB>();
			password.GP_GC = GlbCompany.CurrentCompany.PK;
			password.Badge = "ABC";
			password.EORI = "123";

			Factory.Save();
		}

		void SetUpRefData(bool useDescription)
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);

			var refSysConfTypeClientIdLive = refHelper.CreateRefSysConfigType("ClientLive", "WTG Client Id Live", "WTG Client Id (Live) used when authorising CW1 to act on user's behalf for certain HMRC applications");
			var refSysConfTypeClientIdTest = refHelper.CreateRefSysConfigType("ClientTest", "WTG Client Id Test", "WTG Client Id (Test) used when authorising CW1 to act on user's behalf for certain HMRC applications");
			var refSysConfTypeClientIdDev = refHelper.CreateRefSysConfigType("ClientDev", "WTG Client Id Dev", "WTG Client Id (Dev) used when authorising CW1 to act on user's behalf for certain HMRC applications");

			refHelper.CreateRefSysConfig(refSysConfTypeClientIdLive.ZRT_ConfigCode, "LiveClientID", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			refHelper.CreateRefSysConfig(refSysConfTypeClientIdTest.ZRT_ConfigCode, "TestClientID", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			refHelper.CreateRefSysConfig(refSysConfTypeClientIdDev.ZRT_ConfigCode, "DevClientID", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			var refSysConfTypeCallbackUrlLive = refHelper.CreateRefSysConfigType("ClbUrlLive", "WTG Callback Url Live", "WTG Callback Url (Live) used when authorising CW1 to act on user's behalf for certain HMRC applications");
			var refSysConfTypeCallbackUrlTest = refHelper.CreateRefSysConfigType("ClbUrlTest", "WTG Callback Url Test", "WTG Callback Url (Test) used when authorising CW1 to act on user's behalf for certain HMRC applications");
			refHelper.CreateRefSysConfig(refSysConfTypeCallbackUrlLive.ZRT_ConfigCode, "https://gbcds.wisegrid.net", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			refHelper.CreateRefSysConfig(refSysConfTypeCallbackUrlTest.ZRT_ConfigCode, "https://gbcds-test.wisegrid.net", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			var refSysConfTypePath = refHelper.CreateRefSysConfigType("AppUrlPath", "HMRC application authorisation URL (path)", "The path of the URL that CW1 will build and to ask users to authorise it to act on their behalf for certain HMRC applications");
			var refSysConfTypeHstLive = refHelper.CreateRefSysConfigType("AppHstLive", "HMRC application authorisation URL (live host)",
				"The host of the LIVE URL that CW1 will build and to ask users to authorise it to act on their behalf for certain HMRC applications");
			var refSysConfTypeHstTest = refHelper.CreateRefSysConfigType("AppHstTest", "HMRC application authorisation URL (test host)", "The host of the TEST URL that CW1 will build and to ask users to authorise it to act on their behalf for certain HMRC applications");
			refHelper.CreateRefSysConfig(refSysConfTypePath.ZRT_ConfigCode, "oauth/authorize?response_type=code&client_id=[WTGClientID]&scope=write:customs-declaration%20write:customs-inventory-linking-exports%20write:customs-declarations-information%20common-transit-convention-traders%20write:goods-movement-system%20write:import-control-system[MoreScopes]&state=[CredentialID]&redirect_uri=[CallbackUrl]", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			refHelper.CreateRefSysConfig(refSysConfTypeHstLive.ZRT_ConfigCode, "https://api.service.hmrc.gov.uk/", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			refHelper.CreateRefSysConfig(refSysConfTypeHstTest.ZRT_ConfigCode, "https://test-api.service.hmrc.gov.uk/", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			Factory.Save();
		}

		public void TestDoNotAllowPeriod()
		{
			GlbExternalPassword.Badge = "A.B";
			AssertEquals("AB", GlbExternalPassword.Badge);
			GlbExternalPassword.EORI = "C.D";
			AssertEquals("CD", GlbExternalPassword.EORI);
			AssertEquals("CD.AB", GlbExternalPassword.GP_UserID);
			GlbExternalPassword.GP_UserID = "X.Y";
			AssertEquals("Y", GlbExternalPassword.Badge);
			AssertEquals("X", GlbExternalPassword.EORI);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(ZGuid.Empty, GlbExternalPassword.GP_GC);
			AssertEquals(PasswordTypesList.Codes.CDS, GlbExternalPassword.GP_PasswordType);
			AssertEquals(PasswordStatusList.Codes.Invalid, GlbExternalPassword.Status);
			AssertEquals(false, GlbExternalPassword.IsTokenForAll);
			AssertEquals(false, GlbExternalPassword.IsTokenForCDS);
			AssertEquals(false, GlbExternalPassword.IsTokenForEMCS);
			AssertEquals(false, GlbExternalPassword.IsTokenForGVMS);
			AssertEquals(false, GlbExternalPassword.IsTokenForNCTS);
			AssertEquals(false, GlbExternalPassword.IsTokenForSnSGB);
		}

		public void TestIsTokenForAll()
		{
			GlbExternalPassword.IsTokenForAll = true;
			AssertEquals(true, GlbExternalPassword.IsTokenForCDS);
			AssertEquals(true, GlbExternalPassword.IsTokenForEMCS);
			AssertEquals(true, GlbExternalPassword.IsTokenForGVMS);
			AssertEquals(true, GlbExternalPassword.IsTokenForNCTS);
			AssertEquals(true, GlbExternalPassword.IsTokenForSnSGB);

			GlbExternalPassword.IsTokenForAll = false;
			AssertEquals(false, GlbExternalPassword.IsTokenForCDS);
			AssertEquals(false, GlbExternalPassword.IsTokenForEMCS);
			AssertEquals(false, GlbExternalPassword.IsTokenForGVMS);
			AssertEquals(false, GlbExternalPassword.IsTokenForNCTS);
			AssertEquals(false, GlbExternalPassword.IsTokenForSnSGB);

			GlbExternalPassword.IsTokenForCDS = true;
			AssertEquals(false, GlbExternalPassword.IsTokenForAll);
			GlbExternalPassword.IsTokenForEMCS = true;
			AssertEquals(false, GlbExternalPassword.IsTokenForAll);
			GlbExternalPassword.IsTokenForGVMS = true;
			AssertEquals(false, GlbExternalPassword.IsTokenForAll);
			GlbExternalPassword.IsTokenForNCTS = true;
			AssertEquals(false, GlbExternalPassword.IsTokenForAll);
			GlbExternalPassword.IsTokenForSnSGB = true;
			AssertEquals(true, GlbExternalPassword.IsTokenForAll);

			GlbExternalPassword.IsTokenForCDS = false;
			AssertEquals(false, GlbExternalPassword.IsTokenForAll);
		}

		public void TestPasswordStatusSetToInvalidWhenBadgeChanges()
		{
			GlbExternalPassword.GP_GC = GlbCompany.CurrentCompany.PK;
			GlbExternalPassword.Badge = "123";
			GlbExternalPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			GlbExternalPassword.Badge = "456";
			AssertEquals(PasswordStatusList.Codes.Invalid, GlbExternalPassword.Status);
		}

		public void TestPasswordStatusSetToInvalidWhenEORIChanges()
		{
			GlbExternalPassword.GP_GC = GlbCompany.CurrentCompany.PK;
			GlbExternalPassword.EORI = "123";
			GlbExternalPassword.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			GlbExternalPassword.EORI = "456";
			AssertEquals(PasswordStatusList.Codes.Invalid, GlbExternalPassword.Status);
		}

		public void TestPasswordExpiredDisplay()
		{
			GlbExternalPassword.Status = PasswordStatusList.Codes.Valid;
			GlbExternalPassword.StatusMessage = "Access Token - Access token refreshed OK";
			GlbExternalPassword.GP_ExpiryDate = ZDateTime.Now.AddDays(-1);

			AssertEquals(PasswordStatusList.Codes.Invalid, GlbExternalPassword.Status);
			AssertEquals(StatusDescriptions.ExpiredAccessToken, GlbExternalPassword.StatusMessage);
		}

		public void TestUrl()
		{
			SetUpRefData(false);
			var url = password.GetUrl();
			AssertEquals(ExpectedUrlTest, url);
			GBCustomsDataRegistry.Instance.CDSPilotMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			url = password.GetUrl();
			AssertEquals(ExpectedUrlDev, url);
		}

		public void TestUrlWithCDSRequestPathInDescription()
		{
			SetUpRefData(true);
			var url = password.GetUrl();
			AssertEquals(ExpectedUrlTest, url);
		}

		public void TestUrlIfNoZZData()
		{
			var url = password.GetUrl();
			AssertEquals(string.Empty, url);
		}

		const string ExpectedUrlTest = "https://test-api.service.hmrc.gov.uk/oauth/authorize?response_type=code&client_id=TestClientID&scope=write:customs-declaration%20write:customs-inventory-linking-exports%20write:customs-declarations-information%20common-transit-convention-traders%20write:goods-movement-system%20write:import-control-system&state=EDIDAT.123.ABC&redirect_uri=https://gbcds.wisegrid.net";
		const string ExpectedUrlDev = "https://test-api.service.hmrc.gov.uk/oauth/authorize?response_type=code&client_id=DevClientID&scope=write:customs-declaration%20write:customs-inventory-linking-exports%20write:customs-declarations-information%20common-transit-convention-traders%20write:goods-movement-system%20write:import-control-system&state=EDIDAT.123.ABC&redirect_uri=https://gbcds-test.wisegrid.net";
	}
}
