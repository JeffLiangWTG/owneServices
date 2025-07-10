using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.IE;
using Enterprise.ZArchitecture.Environment;
using GlbCompanyWrapper = Enterprise.Customs.IE.Business.GlbCompanyWrapper;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	class ExtensionsTest : TestCaseWithFactory
	{
		public void TestIsTrueOrFalse()
		{
			var trueString = "1";
			Assert(trueString.IsTrueOrFalse());

			var falseString = "0";
			Assert(!falseString.IsTrueOrFalse());
		}

		public void TestTryParseToDate()
		{
			var dateString8 = "20240228";
			dateString8.TryParseToDate(out var result8);
			AssertEquals("8 digit date string", new ZDateTime(2024, 02, 28), result8);

			var dateString12 = "202402282359";
			dateString12.TryParseToDate(out var result12);
			AssertEquals("12 digit date string", new ZDateTime(2024, 02, 28, 23, 59, 0), result12);

			var dateString15 = "202402282359UTC";
			dateString15.TryParseToDate(out var result15);
			AssertEquals("15 digit date string", new ZDateTime(2024, 02, 28, 23, 59, 0), result15);
		}

		public void TestGetCredentialPK()
		{
			var company = Factory.New<GlbCompany>();
			var wrapper = (IIEGlbCompanyWrapper)GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company);
			AssertEquals("Empty", ZGuid.Empty, company.GetCredentialPK());
			var credential = wrapper.GetGlbExternalPasswordOrCreateNew();
			AssertEquals("GetCredentialPK", credential.PK, company.GetCredentialPK());
		}

		public void TestHasInvalidCredentialOrSetInvalidIfCredentialHasExpired_Company()
		{
			GlbCompany company = null;
			AssertEquals("NULL", true, company.HasInvalidCredentialOrSetInvalidIfCredentialHasExpired(false));
			(company, _) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(company);
			AssertEquals("Not in database", true, company.HasInvalidCredentialOrSetInvalidIfCredentialHasExpired(false));
			Factory.Save();
			AssertEquals("Saved", false, company.HasInvalidCredentialOrSetInvalidIfCredentialHasExpired(false));
			companyCredential.GP_ExpiryDate = ZDateTime.UtcNow.AddDays(-1);
			Factory.Save();
			AssertEquals("Pre: GP_PasswordStatus", PasswordStatusList.Codes.Valid, companyCredential.GP_PasswordStatus);
			AssertEquals("Saved", true, company.HasInvalidCredentialOrSetInvalidIfCredentialHasExpired(false));
			companyCredential.Reload();
			AssertEquals("No Saved: GP_PasswordStatus", PasswordStatusList.Codes.Valid, companyCredential.GP_PasswordStatus);
			AssertEquals("Saved", true, company.HasInvalidCredentialOrSetInvalidIfCredentialHasExpired(true));
			companyCredential.Reload();
			AssertEquals("Saved: GP_PasswordStatus", PasswordStatusList.Codes.Invalid, companyCredential.GP_PasswordStatus);
		}

		public void TestHasInvalidCredentialOrSetInvalidIfCredentialHasExpired_Credential()
		{
			(var company, _) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			GlbCompanyCredential credential = null;
			AssertEquals("NULL", true, credential.HasInvalidCredentialOrSetInvalidIfCredentialHasExpired(false));
			credential = InterchangeProcessorTestHelper.CreateValidCredential(company);
			AssertEquals("Valid", false, credential.HasInvalidCredentialOrSetInvalidIfCredentialHasExpired(false));
			credential.GP_ExpiryDate = ZDateTime.UtcNow.AddDays(-1);
			AssertEquals("Pre: GP_PasswordStatus", PasswordStatusList.Codes.Valid, credential.GP_PasswordStatus);
			AssertEquals("Saved", true, credential.HasInvalidCredentialOrSetInvalidIfCredentialHasExpired(false));
			AssertEquals("Post: GP_PasswordStatus", PasswordStatusList.Codes.Invalid, credential.GP_PasswordStatus);
			AssertEquals("Not Saved", true, credential.HasChanges);
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			AssertEquals("Saved", true, credential.HasInvalidCredentialOrSetInvalidIfCredentialHasExpired(true));
			AssertEquals("Saved: GP_PasswordStatus", PasswordStatusList.Codes.Invalid, credential.GP_PasswordStatus);
			AssertEquals("Saved", false, credential.HasChanges);
			credential.GP_PasswordStatus = "!@#";
			AssertEquals("Saved", true, credential.HasInvalidCredentialOrSetInvalidIfCredentialHasExpired(true));
			AssertEquals("Saved: GP_PasswordStatus", "!@#", credential.GP_PasswordStatus);
			AssertEquals("Saved", true, credential.HasChanges);
		}

		public void TestCheckValidROSCredentialAndSetInvalidIfCredentialHasExpired()
		{
			GlbCompany company = null;
			AssertEquals("NULL", true, company.CheckValidROSCredentialAndSetInvalidIfCredentialHasExpired());
			(company, _) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals("Not valid", false, company.CheckValidROSCredentialAndSetInvalidIfCredentialHasExpired());
			AssertEquals("ShowError", "Cannot send message as Company (CIE) doesn't have a Revenue ROS certificate uploaded or credential added. Please see eLearning 'Set up CargoWise for Customs Messaging in Ireland' on My Account for assistance.", UnitTestUserNotification.Instance.LastMessage.Text);
			var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(company);
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals("Valid", true, company.CheckValidROSCredentialAndSetInvalidIfCredentialHasExpired());
			AssertEquals("UnitTestUserNotification.Instance.LastMessage.WasNone", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			companyCredential.GP_ExpiryDate = ZDateTime.UtcNow.AddDays(-1);
			Factory.Save();
			AssertEquals("Pre: GP_PasswordStatus", PasswordStatusList.Codes.Valid, companyCredential.GP_PasswordStatus);
			AssertEquals("Invalid", false, company.CheckValidROSCredentialAndSetInvalidIfCredentialHasExpired());
			AssertEquals("ShowError", "Cannot send message as Company (CIE) doesn't have a Revenue ROS certificate uploaded or credential added. Please see eLearning 'Set up CargoWise for Customs Messaging in Ireland' on My Account for assistance.", UnitTestUserNotification.Instance.LastMessage.Text);
			companyCredential.Reload();
			AssertEquals("Saved: GP_PasswordStatus", PasswordStatusList.Codes.Invalid, companyCredential.GP_PasswordStatus);
		}
	}
}
