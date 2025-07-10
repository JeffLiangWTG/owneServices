using System;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Definitions.Authentication;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	class RegisterPersonalEmailTest : ZPageLifeCycleTest
	{
		protected override ZPage GetNewPage()
		{
			return RegisterPersonalEmailPage;
		}

		public void TestPageLoad()
		{
			using (Env.Instance.SuppressSwitchContextCheck(ensureContextIsRestoredAfterSuppression: false))
			{
				AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, "invalidToken");
				AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, "");
				AssertPageLoad(AssertOnLoadComplete_WithValidToken, TestToken);
				AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, TestToken_Expired);
				AssertPageLoad(AssertOnLoadComplete_WithInvalidToken, TestToken_InvalidType);
			}
		}

		void AssertPageLoad(EventHandler onLOadCompleteHandler, string token)
		{
			var queryStringKey = "RegisterKey";
			HttpContext.Current.Request.QueryString.Remove(queryStringKey);
			HttpContext.Current.Request.QueryString.Add(queryStringKey, token);
			Env.ClearUserContext();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			this.AssertOnLoadComplete += onLOadCompleteHandler;
			try
			{
				RunPageLifeCycle();
			}
			finally
			{
				this.AssertOnLoadComplete -= onLOadCompleteHandler;
			}
		}

		void AssertOnLoadComplete_WithInvalidToken(object sender, EventArgs e)
		{
			var testPage = (RegisterPersonalEmailForTest)this.TestPage;
			Assert(!testPage.RegisterResultResultLabel_Expose.Visible);
			AssertEquals("The register link is invalid.", testPage.ErrorMessageLabel_Expose.Text);
			AssertEquals("User context should be unset", Guid.Empty, Env.CurrentUserPK);
			AssertEquals("User context should be unset", Guid.Empty, Env.CurrentBranchPK);
			AssertEquals("User context should be unset", Guid.Empty, Env.CurrentDepartmentPK);
		}

		void AssertOnLoadComplete_WithValidToken(object sender, EventArgs e)
		{
			var testPage = (RegisterPersonalEmailForTest)this.TestPage;
			AssertNullOrEmpty(testPage.ErrorMessageLabel_Expose.Text);
			Assert(testPage.RegisterResultResultLabel_Expose.Visible);
			AssertEquals("You have successfully registered your personal email.", testPage.ResultMessageLabel_Expose.Text);
			AssertNotEquals("Site user should not be null", null, testPage.SiteUser);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentUserPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentBranchPK);
			AssertNotEquals("User context should be set", Guid.Empty, Env.CurrentDepartmentPK);
			TestOrgContact.Person.Reload();
			TestOrgContact_WithSamePersonalEmail.Reload();
			TestOrgContact_WithSamePersonalEmail.Person.Reload();
			AssertEquals("testuser@gmail.com", TestOrgContact_WithSamePersonalEmail.Person.PER_EmailAddress);
			AssertEquals(TestOrgContact.OC_PER, TestOrgContact_WithSamePersonalEmail.OC_PER);
			testPage.OnLoad();
			AssertEquals("The register link is invalid.", testPage.ErrorMessageLabel_Expose.Text);
		}

		public void TestPageLoad_PersonMerge()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "Bob Smith";
			person.PER_EmailAddress = "bob.smith@gmail.com";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_ContactName = "Bob Smith";
			contact1.OC_Email = "bob.smith@test.com";
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_IsActive = true;

			var contact2 = orgHeader.Contacts.AddNew();
			contact2.OC_ContactName = "Bob Smith (1)";
			contact2.OC_Email = "bob.smith@test.com.au";
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_IsActive = true;

			var contact3 = orgHeader.Contacts.AddNew();
			contact3.OC_ContactName = "Alice Smith";
			contact3.OC_Email = "alice.smith@test.com";
			contact3.OC_WebAccessEnabled = true;
			contact3.OC_IsActive = true;

			Factory.Save();

			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			var token1 = accessControl.CreateLimitedToken(AccessTokenTypes.RegisterPersonalEmail, new AccessTokenInfo(person.PER_EmailAddress, contact1.PK.ToGuid(), "OC"), maxUses: 1);
			var token2 = accessControl.CreateLimitedToken(AccessTokenTypes.RegisterPersonalEmail, new AccessTokenInfo(person.PER_EmailAddress, contact2.PK.ToGuid(), "OC"), maxUses: 1);
			var token3 = accessControl.CreateLimitedToken(AccessTokenTypes.RegisterPersonalEmail, new AccessTokenInfo(person.PER_EmailAddress, contact3.PK.ToGuid(), "OC"), maxUses: 1);

			var testPage = (RegisterPersonalEmailForTest)TestPage;
			var queryStringKey = "RegisterKey";

			CombineAssertions(() =>
			{
				HttpContext.Current.Request.QueryString.Remove(queryStringKey);
				HttpContext.Current.Request.QueryString.Add(queryStringKey, token1);
				testPage.OnLoad();
				contact1.Reload();
				AssertEquals("Contact with same name and same personal email address is merged to existing person", contact1.OC_PER, person.PK);

				HttpContext.Current.Request.QueryString.Remove(queryStringKey);
				HttpContext.Current.Request.QueryString.Add(queryStringKey, token2);
				testPage.OnLoad();
				contact2.Reload();
				AssertEquals("Contact with same name without suffix and same personal email address is merged to existing person", contact2.OC_PER, person.PK);

				HttpContext.Current.Request.QueryString.Remove(queryStringKey);
				HttpContext.Current.Request.QueryString.Add(queryStringKey, token3);
				testPage.OnLoad();
				contact3.Reload();
				AssertNotEquals("Contact with different name and same personal email address is not merged to existing person", contact3.OC_PER, person.PK);
			});
		}

		class RegisterPersonalEmailForTest : RegisterPersonalEmail
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new Global();
			}

			protected override Uri RequestUrl => new Uri("http://www.test.com/MyAccount/Admin/RegisterPersonalEmail.aspx");
			public RegisterPersonalEmailForTest()
			{
				ErrorMessageLabel_Expose = new ZTextLabel();
				ResultMessageLabel_Expose = new ZTextLabel();
				RegisterResultResultLabel_Expose = new HtmlGenericControl();
				LogoImage = new HyperLink();
			}

			public ZTextLabel ErrorMessageLabel_Expose { get => ErrorMessage; private set => ErrorMessage = value; }

			public HtmlGenericControl RegisterResultResultLabel_Expose { get => RegisterResult; private set => RegisterResult = value; }

			public ZTextLabel ResultMessageLabel_Expose { get => ResultMessageLabel; private set => ResultMessageLabel = value; }

			public void OnLoad()
			{
				base.OnLoad(EventArgs.Empty);
			}
		}

		RegisterPersonalEmailForTest RegisterPersonalEmailPage
		{
			get
			{
				var testPage = new RegisterPersonalEmailForTest();
				var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
				method.Invoke(testPage, new object[] { HttpContext.Current });
				return testPage;
			}
		}

		OrgContact TestOrgContact;
		OrgContact TestOrgContact_WithSamePersonalEmail;
		string TestToken;
		string TestToken_Expired;
		string TestToken_InvalidType;
		protected override void SetUp()
		{
			base.SetUp();
			TestToken = "testToken";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			TestOrgContact = orgHeader.Contacts.AddNew();
			TestOrgContact.OC_ContactName = "testUser1";
			TestOrgContact.OC_Email = "testuser@cargowise.com";
			TestOrgContact.OC_WebAccessEnabled = true;
			TestOrgContact.OC_IsActive = true;
			TestOrgContact_WithSamePersonalEmail = orgHeader.Contacts.AddNew();
			TestOrgContact_WithSamePersonalEmail.OC_ContactName = "testUser1 (1)";
			Factory.Save();
			TestOrgContact.Person.PER_EmailAddress = "testuser@gmail.com";
			TestOrgContact.Person.PER_WebAccessEnabled = true;
			Factory.Save();
			ITokenizedAccessControl accessControl = new TokenizedAccessControl();
			TestToken = accessControl.CreateLimitedToken(AccessTokenTypes.RegisterPersonalEmail, new AccessTokenInfo("testuser@gmail.com", TestOrgContact_WithSamePersonalEmail.PK.ToGuid(), "OC"), maxUses: 1);
			TestToken_Expired = accessControl.CreateLimitedToken(AccessTokenTypes.RegisterPersonalEmail, new AccessTokenInfo("testuser@gmail.com", TestOrgContact_WithSamePersonalEmail.PK.ToGuid(), "OC"), TimeSpan.FromHours(-1), 1);
			TestToken_InvalidType = accessControl.CreateLimitedToken("INV", new AccessTokenInfo("", Guid.Empty, "INV"), maxUses: 1);
		}
	}
}
