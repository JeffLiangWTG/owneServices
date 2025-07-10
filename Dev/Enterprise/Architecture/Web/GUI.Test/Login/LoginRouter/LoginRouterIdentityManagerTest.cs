using CargoWise.Common;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.Login.Testing
{
	[TestedType(typeof(LoginRouterIdentityManager))]
	public class LoginRouterIdentityManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGenerateToken_NullAccount()
		{
			AssertNoExceptionThrown(delegate
			{ LoginRouterIdentityManager.GenerateToken(null); });
			AssertEquals(string.Empty, LoginRouterIdentityManager.GenerateToken(null));
		}

		[TestDate(2020, 01, 01)]
		public void TestGenerateToken()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			AssertEquals("Should consume successfully", true, accessControl.TryConsume(token, AccessTokenTypes.LoginRouterIdentity, out var accessToken));
			AssertEquals("Contact should be parent", contact.PK, accessToken.ParentId);
			AssertEquals("Contact should be parent", OrgContactSchema.Constants.Prefix, accessToken.ParentTableCode);
		}

		public void TestGenerateTokenShouldBeContactSpecific()
		{
			var contact1 = Factory.NewWithValidTestData<OrgContact>();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			var token1 = LoginRouterIdentityManager.GenerateToken(contact1);
			var token2 = LoginRouterIdentityManager.GenerateToken(contact2);
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			AssertEquals("Should consume successfully", true, accessControl.TryConsume(token1, AccessTokenTypes.LoginRouterIdentity, out var accessToken1));
			AssertEquals("Should consume successfully", true, accessControl.TryConsume(token2, AccessTokenTypes.LoginRouterIdentity, out var accessToken2));
			AssertEquals("Contact should be parent", contact1.PK, accessToken1.ParentId);
			AssertEquals("Contact should be parent", contact2.PK, accessToken2.ParentId);
			AssertEquals("Contact should be parent", OrgContactSchema.Constants.Prefix, accessToken1.ParentTableCode);
			AssertEquals("Contact should be parent", OrgContactSchema.Constants.Prefix, accessToken2.ParentTableCode);
			AssertEquals(string.Empty, accessToken1.Scope);
			AssertEquals(string.Empty, accessToken2.Scope);
		}

		[TestDate(2020, 01, 01)]
		public void TestGenerateTokenIfAlreadyExists()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var token1 = LoginRouterIdentityManager.GenerateToken(contact);
			TestDateAttribute.AddMinutes(10);
			var token2 = LoginRouterIdentityManager.GenerateToken(contact);
			TestDateAttribute.AddMinutes(10);
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			AssertEquals("Should consume successfully since new token has a new expiry", true, accessControl.TryPeek(token2, AccessTokenTypes.LoginRouterIdentity, out var accessToken));
			AssertEquals("Contact should be parent", contact.PK, accessToken.ParentId);
			AssertEquals("Contact should be parent", OrgContactSchema.Constants.Prefix, accessToken.ParentTableCode);
			AssertEquals(string.Empty, accessToken.Scope);
		}

		[TestDate(2020, 01, 01)]
		[TestUtcOffset(-10, 0, 0)]
		public void TestGenerateTokenIfAlreadyExistsNonUtc()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			var tokenInfo = new AccessTokenInfo(string.Empty, contact.PK.ToGuid(), OrgContactSchema.Constants.Prefix);
			accessControl.TryCreate("12345", AccessTokenTypes.LoginRouterIdentity, false, ZDateTime.UtcNow.ToDateTime(), 1, tokenInfo);
			TestDateAttribute.AddMinutes(30);
			LoginRouterIdentityManager.GenerateToken(contact);
			AssertEquals("Should not try to consume existing token since it's been more than 15 minutes since expiry", 0, ErrorReporter.TotalErrorCount);
		}

		[TestDate(2020, 01, 01)]
		public void TestGenerateTokenIfAlreadyExistsAndExpired()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var token1 = LoginRouterIdentityManager.GenerateToken(contact);
			TestDateAttribute.AddMinutes(30);
			var token2 = LoginRouterIdentityManager.GenerateToken(contact);
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			AssertNotEquals(string.Empty, token2);
			AssertEquals("Should consume successfully since new token has a new expiry", true, accessControl.TryPeek(token2, AccessTokenTypes.LoginRouterIdentity, out var accessToken));
			AssertEquals("Contact should be parent", contact.PK, accessToken.ParentId);
			AssertEquals("Contact should be parent", OrgContactSchema.Constants.Prefix, accessToken.ParentTableCode);
			AssertEquals(string.Empty, accessToken.Scope);
		}

		public virtual void TestPopulatePropertiesFromToken()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var token = LoginRouterIdentityManager.GenerateToken(contact);

			var manager = new LoginRouterIdentityManager(Factory);
			manager.PopulatePropertiesFromToken(token);
			AssertEquals("manager should populate contact from token", contact.PK, manager.Contact.PK);
		}

		public void TestConsumeToken()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			AssertEquals("Precondition: Token should be generated", true, accessControl.TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out _));

			var manager = new LoginRouterIdentityManager(Factory);
			manager.PopulatePropertiesFromToken(token);
			AssertEquals(true, manager.ConsumeToken());
			AssertEquals("Token should have been consumed", false, accessControl.TryPeek(token, AccessTokenTypes.LoginRouterIdentity, out _));
		}

		public virtual void TestIsValidID()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var manager = new LoginRouterIdentityManager(Factory);
			AssertEquals(false, manager.IsValidID());
			manager.PopulatePropertiesFromToken(token);
			AssertEquals(true, manager.IsValidID());
		}

		public void TestNotConsumeExistingTokens()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			var existingToken1 = LoginRouterIdentityManager.GenerateToken(contact);
			var existingToken2 = LoginRouterIdentityManager.GenerateToken(contact);
			LoginRouterIdentityManager.GenerateToken(contact);
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			AssertEquals("Generate new token shouldn't consume exisiting tokens", true, accessControl.TryPeek(existingToken1, AccessTokenTypes.LoginRouterIdentity, out _));
			AssertEquals("Generate new token shouldn't consume exisiting tokens", true, accessControl.TryPeek(existingToken2, AccessTokenTypes.LoginRouterIdentity, out _));
		}
	}
}
