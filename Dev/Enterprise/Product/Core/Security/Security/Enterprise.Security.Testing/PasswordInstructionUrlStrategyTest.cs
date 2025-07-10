using System;
using System.Linq;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security.Testing
{
	class PasswordInstructionUrlStrategyTest : TestCaseWithFactory
	{
		public void TestGenerateResetPasswordLink()
		{
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_Email = "contact@wisetechglobal.com";
			Factory.Save();

			AssertExceptionThrown<WebSiteUrlNotSetException>(() => strategy.GenerateUrl(orgContact, PasswordInstructionType.Reset, null));

			WebDataRegistry.Instance.WebTrackerUrl.SetValue(orgContact.CompanyPKForLogin, Guid.Empty, Guid.Empty, "http://localhost/webtracker");
			var resetPasswordLink = strategy.GenerateUrl(orgContact, PasswordInstructionType.Reset, null);
			AssertContains("http://localhost/webtracker/Admin/ResetMasterPassword.aspx?ResetKey=", resetPasswordLink);
		}

		public void TestGenerateSetPasswordLink()
		{
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_Email = "contact@wisetechglobal.com";
			Factory.Save();

			AssertExceptionThrown<WebSiteUrlNotSetException>(() => strategy.GenerateUrl(orgContact, PasswordInstructionType.Set, null));

			WebDataRegistry.Instance.WebTrackerUrl.SetValue(orgContact.CompanyPKForLogin, Guid.Empty, Guid.Empty, "http://localhost/webtracker");
			var setPasswordLink = strategy.GenerateUrl(orgContact, PasswordInstructionType.Set, null);
			AssertContains("http://localhost/webtracker/Admin/SetMasterPassword.aspx?SetKey=", setPasswordLink);
		}

		public void TestGeneratePasswordInstructionToken()
		{
			var relatedContact = Factory.NewWithValidTestData<OrgContact>();
			relatedContact.OC_Email = "contact@wisetechglobal.com";
			Factory.Save();
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_Email = "contact@wisetechglobal.com";
			orgContact.OC_PER = relatedContact.OC_PER;
			Factory.Save();

			WebDataRegistry.Instance.WebTrackerUrl.SetValue(orgContact.CompanyPKForLogin, Guid.Empty, Guid.Empty, "http://localhost/webtracker");

			var resetNoPasswordResetInfo = PasswordInstructionUrlStrategy.GeneratePasswordInstructionToken(orgContact, PasswordInstructionType.Reset, null);
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			accessControl.TryPeek(resetNoPasswordResetInfo, AccessTokenTypes.ResetPassword, out var accessTokenInfo);
			AssertEquals(orgContact.OC_Email, accessTokenInfo.Scope);

			var guidTest = Guid.NewGuid();
			var passwordInfo = new PasswordResetInfo()
			{
				OrgCode = "CODE123",
				ContactEmail = orgContact.OC_Email,
				EmailTemplateCompanyPk = guidTest.ToString()
			};
			var resetWithPasswordResetInfo = PasswordInstructionUrlStrategy.GeneratePasswordInstructionToken(orgContact, PasswordInstructionType.Reset, passwordInfo);
			accessControl.TryPeek(resetWithPasswordResetInfo, AccessTokenTypes.ResetPassword, out accessTokenInfo);
			var expectedResetScope = "{\"product\":null,\"contact_email\":\"contact@wisetechglobal.com\",\"org_code\":\"CODE123\",\"navigation_url\":null,\"email_template_company_pk\":\"" + guidTest + "\"}";
			AssertEquals(expectedResetScope, accessTokenInfo.Scope);
			AssertToken(() => PasswordInstructionUrlStrategy.GeneratePasswordInstructionToken(orgContact, PasswordInstructionType.Reset, passwordInfo), TimeSpan.FromHours(24));
			AssertToken(() => PasswordInstructionUrlStrategy.GeneratePasswordInstructionToken(orgContact, PasswordInstructionType.Reset, passwordInfo, TimeSpan.FromMinutes(5)), TimeSpan.FromMinutes(5));

			passwordInfo = new PasswordResetInfo()
			{
				Product = "CW1",
				OrgCode = "CODE456",
				ContactEmail = orgContact.OC_Email,
				EmailTemplateCompanyPk = guidTest.ToString()
			};
			var setWithPasswordResetInfo = PasswordInstructionUrlStrategy.GeneratePasswordInstructionToken(orgContact, PasswordInstructionType.Set, passwordInfo);
			accessControl.TryPeek(setWithPasswordResetInfo, AccessTokenTypes.SetPassword, out accessTokenInfo);
			var expectedSetScope = "{\"product\":\"CW1\",\"contact_email\":\"contact@wisetechglobal.com\",\"org_code\":\"CODE456\",\"navigation_url\":null,\"email_template_company_pk\":\"" + guidTest + "\"}";
			AssertEquals(expectedSetScope, accessTokenInfo.Scope);
			AssertToken(() => PasswordInstructionUrlStrategy.GeneratePasswordInstructionToken(orgContact, PasswordInstructionType.Set, passwordInfo), TimeSpan.FromHours(24));
			AssertToken(() => PasswordInstructionUrlStrategy.GeneratePasswordInstructionToken(orgContact, PasswordInstructionType.Set, passwordInfo, TimeSpan.FromMinutes(10)), TimeSpan.FromMinutes(10));
		}

		void AssertToken(Func<string> tokenCreator, TimeSpan tokenTime)
		{
			var utcNow = ZDateTime.UtcNow;
			var accessToken = Factory.Load<StmAccessToken>(new ZQuery(StmAccessTokenSchema.SAT_Token, tokenCreator())).Single();
			Assert(Math.Abs((accessToken.SAT_ExpiresAt - utcNow).TotalMinutes - tokenTime.TotalMinutes) < 0.1);
			AssertEquals(1, accessToken.SAT_RemainingUseCount);
		}

		readonly PasswordInstructionUrlStrategy strategy = new PasswordInstructionUrlStrategy();
	}
}
