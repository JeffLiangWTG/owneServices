using System;
using System.Security.Cryptography;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	public abstract class WebUserTestCase : TestCaseWithFactory
	{
		#region Setup

		protected abstract WebUser GetNewWebUser();

		protected abstract IContactable CreateNewContact(string username, string email, string password);

		protected virtual string AffiliationCode
		{
			get { return ""; }
		}

		protected virtual string AffiliationName
		{
			get { return ""; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupTestObjects();
			Factory.Save();
		}

		protected virtual void SetupTestObjects()
		{
			Helper = GetNewTestHelper();
			User = GetNewWebUser();
			Contact = CreateNewContact(UserName, Email, Password);
		}

		protected override void TearDown()
		{
			Helper.RestoreEnvironmentSettings();
			base.TearDown();
		}

		protected const string UserName = "User";
		protected const string Email = "user@user";
		protected const string Password = "password";

		protected ZWebTestHelper Helper;
		protected WebUser User;
		protected IContactable Contact;

		protected virtual ZWebTestHelper GetNewTestHelper()
		{
			return new ZWebTestHelper(Factory);
		}

		#endregion

		#region TestLoginAndLogout

		public void TestLoginAndLogOut()
		{
			User.Login("XXX", "XXX", "XXX");
			AssertNotLoggedIn(User);

			User.Login(AffiliationCode, Email, Password);
			AssertSuccessfulStandardLogin(User);

			User.Logout();
			AssertNotLoggedIn(User);
		}

		protected void AssertSuccessfulStandardLogin(WebUser user)
		{
			AssertSuccessfulLogin(user, UserName);
		}

		#endregion

		#region TestLoginForWebServices

		public void TestLoginForWebServices()
		{
			Assert(!User.IsLoggedIn);

			AssertEquals("WebServiceUsername should be empty by default", "", WebDataRegistry.Instance.WebServiceUsername.Value);
			AssertEquals("WebServicePassword should be empty by default", "", WebDataRegistry.Instance.WebServicePassword.Value);

			User.Login(AffiliationCode, "", "");
			AssertNotLoggedIn(User);

			try
			{
				WebDataRegistry.Instance.WebServiceUsername.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestWebServiceUsername");
				WebDataRegistry.Instance.WebServicePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TestWebServicePassword");

				User.Login(AffiliationCode, "TestWebServiceUsername", "");
				AssertEquals("User should not be logged in with blank password", false, User.IsLoggedIn);

				User.Login(AffiliationCode, "", "TestWebServicePassword");
				AssertEquals("User should not be logged in with blank username", false, User.IsLoggedIn);

				User.Login(AffiliationCode, "IncorrectUsername", "TestWebServicePassword");
				AssertEquals("User should not be logged in with incorrect username", false, User.IsLoggedIn);

				User.Login(AffiliationCode, "TestWebServiceUsername", "IncorrectPassword");
				AssertEquals("User should not be logged in with incorrect password", false, User.IsLoggedIn);

				User.Login(AffiliationCode, "TestWebServiceUsername", "TestWebServicePassword");
				AssertSuccessfulLogin(User, "WebServicesUser");
			}
			finally
			{
				WebDataRegistry.Instance.WebServiceUsername.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
				WebDataRegistry.Instance.WebServicePassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
				if (!string.IsNullOrEmpty(WebDataRegistry.Instance.WebServiceUsername.Value))
				{
					ErrorReporter.ReportOnce("WebServiceUsername should be reset to empty string");
				}
				if (!string.IsNullOrEmpty(WebDataRegistry.Instance.WebServicePassword.Value))
				{
					ErrorReporter.ReportOnce("WebServicePassword should be reset to empty string");
				}
			}
		}

		#endregion

		public void TestLoginForEDISupportAndIsSuperUser()
		{
			Assert(!User.IsLoggedIn);
			Assert(!User.IsSuperUser);

			User.Login(AffiliationCode, Environment.User.SupportUserName, CWSupportLoginToken.TokenForTest);
			AssertSuccessfulLogin(User, Environment.User.SupportUserName);
			Assert(User.IsSuperUser);
		}

		public abstract void TestAreSecurityRightsGranted();

		protected virtual void AssertNotLoggedIn(WebUser user)
		{
			Assert(!user.IsLoggedIn);
			AssertNull(user.LoggedInUser);
			AssertEquals("", user.AffiliationCode);
			AssertEquals("", user.AffiliationName);
			AssertEquals("", user.LoggedInUserName);
		}

		protected virtual void AssertSuccessfulLogin(WebUser user, string expectedUsername)
		{
			Assert(user.IsLoggedIn);
			AssertEquals(AffiliationCode, user.AffiliationCode);
			AssertEquals(expectedUsername, User.LoggedInUserName);
			AssertEquals(AffiliationName, User.AffiliationName);
		}

		public void TestLoginFailWithHash()
		{
			var hash = GetRandomBytes(32);

			var contactUsername = string.Empty;
			var mockLoginAttemptRecorder = new Mock<IOrgContactLoginAttemptRecorder>();
			mockLoginAttemptRecorder
				.Setup(x => x.RecordLoginAttempt(AffiliationCode, Email, hash))
				.Callback<string, string, byte[]>((companyCode, username, hash1) => contactUsername = username);
			ObjectFactory.Substitute(mockLoginAttemptRecorder.Object);

			new OrgContactWebUser().Login(AffiliationCode, Email, "wrongPassword", hash);

			mockLoginAttemptRecorder.Verify(x => x.RecordLoginAttempt(AffiliationCode, Email, hash));
			AssertEquals("Contact's login attempts recorded", Email, contactUsername);
		}

		static byte[] GetRandomBytes(int size)
		{
			using (var generator = RandomNumberGenerator.Create())
			{
				var buf = new byte[size];
				generator.GetNonZeroBytes(buf);
				return buf;
			}
		}
	}
}
