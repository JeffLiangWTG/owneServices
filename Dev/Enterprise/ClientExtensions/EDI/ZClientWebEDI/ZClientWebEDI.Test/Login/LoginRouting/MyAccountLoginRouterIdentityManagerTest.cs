using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.Login;
using NUnit.Framework;
using EdiCustomerUserAccount = Enterprise.Client.EDI.UserManagement.Business.EdiCustomerUserAccount;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(MyAccountLoginRouterIdentityManager))]
	public class MyAccountLoginRouterIdentityManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGenerateTokenUserAccount()
		{
			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			var token = MyAccountLoginRouterIdentityManager.GenerateToken(userAccount);
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			AssertEquals("Should consume successfully", true, accessControl.TryConsume(token, AccessTokenTypes.LoginRouterIdentity, out var accessToken));
			AssertEquals("User account should be parent", userAccount.PK, accessToken.ParentId);
			AssertEquals("User account should be parent", EdiCustomerUserAccountSchema.Constants.Prefix, accessToken.ParentTableCode);
			AssertEquals(string.Empty, accessToken.Scope);
		}

		[TestDate(2020, 01, 01)]
		public void TestGenerateTokenUserAccountIfAlreadyExists()
		{
			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			var token1 = MyAccountLoginRouterIdentityManager.GenerateToken(userAccount);
			TestDateAttribute.AddMinutes(10);
			var token2 = MyAccountLoginRouterIdentityManager.GenerateToken(userAccount);
			TestDateAttribute.AddMinutes(10);
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			AssertEquals("Should not have been consumed", true, accessControl.TryPeek(token1, AccessTokenTypes.LoginRouterIdentity, out _));
			AssertEquals("Should consume successfully since new token has a new expiry", true, accessControl.TryPeek(token2, AccessTokenTypes.LoginRouterIdentity, out var accessToken));
			AssertEquals("Contact should be parent", userAccount.PK, accessToken.ParentId);
			AssertEquals("Contact should be parent", EdiCustomerUserAccountSchema.Constants.Prefix, accessToken.ParentTableCode);
			AssertEquals(string.Empty, accessToken.Scope);
		}

		[TestDate(2020, 01, 01)]
		[TestUtcOffset(-10, 0, 0)]
		public void TestGenerateTokenIfAlreadyExistsNonUtc()
		{
			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			var tokenInfo = new AccessTokenInfo(string.Empty, userAccount.PK.ToGuid(), EdiCustomerUserAccountSchema.Constants.Prefix);
			accessControl.TryCreate("12345", AccessTokenTypes.LoginRouterIdentity, false, ZDateTime.UtcNow.ToDateTime(), 1, tokenInfo);
			TestDateAttribute.AddMinutes(30);
			MyAccountLoginRouterIdentityManager.GenerateToken(userAccount);
			var existingUserAccountTokenQuery = new ZQuery(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.LoginRouterIdentity);
			existingUserAccountTokenQuery.AddToFilter(StmAccessTokenSchema.SAT_ParentId, userAccount.PK);
			existingUserAccountTokenQuery.AddToFilter(StmAccessTokenSchema.SAT_ParentTableCode, EdiCustomerUserAccountSchema.Constants.Prefix);
			var existingTokensCount = Factory.GetDatabaseCount(typeof(StmAccessToken), existingUserAccountTokenQuery);
			AssertEquals("Should not try to consume existing token since it's been more than 15 minutes since expiry", 2, existingTokensCount);
		}

		public void TestPopulatePropertiesFromToken()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount.EUA_OC_WebAccessContact = contact.PK;
			var token = MyAccountLoginRouterIdentityManager.GenerateToken(userAccount);
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			AssertEquals("Precondition: Token should be generated", true, accessControl.TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out _));
			var manager = new MyAccountLoginRouterIdentityManager(Factory);
			manager.PopulatePropertiesFromToken(token);
			AssertEquals("manager should populate user account from token", userAccount.PK, manager.UserAccount.PK);
			AssertEquals("manager should populate contact from user account", contact.PK, manager.Contact.PK);
			manager.ConsumeToken();
			AssertEquals("Token should have been consumed", false, accessControl.TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out _));
		}

		public void TestIsValidID()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			var userAccount2 = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount2.EUA_OC_WebAccessContact = contact.PK;
			Factory.Save();
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var token2 = MyAccountLoginRouterIdentityManager.GenerateToken(userAccount);
			var token3 = MyAccountLoginRouterIdentityManager.GenerateToken(userAccount2);
			var manager = new MyAccountLoginRouterIdentityManager(Factory);
			var manager2 = new MyAccountLoginRouterIdentityManager(Factory);
			var manager3 = new MyAccountLoginRouterIdentityManager(Factory);
			AssertEquals(false, manager.IsValidID());
			manager.PopulatePropertiesFromToken(token);
			manager2.PopulatePropertiesFromToken(token2);
			manager3.PopulatePropertiesFromToken(token3);
			AssertEquals(true, manager.IsValidID());
			AssertEquals(true, manager2.IsValidID());
			AssertEquals(true, manager3.IsValidID());
		}

		public void TestGenerateToken_NotConsumeExistingTokens()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var userAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			userAccount.EUA_OC_WebAccessContact = contact.PK;
			Factory.Save();
			var existingToken1 = MyAccountLoginRouterIdentityManager.GenerateToken(userAccount);
			var existingToken2 = MyAccountLoginRouterIdentityManager.GenerateToken(userAccount);
			MyAccountLoginRouterIdentityManager.GenerateToken(userAccount);
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			AssertEquals("Generate new token shouldn't consume exisiting tokens", true, accessControl.TryPeek(existingToken1, AccessTokenTypes.LoginRouterIdentity, out _));
			AssertEquals("Generate new token shouldn't consume exisiting tokens", true, accessControl.TryPeek(existingToken2, AccessTokenTypes.LoginRouterIdentity, out _));
		}
	}
}
