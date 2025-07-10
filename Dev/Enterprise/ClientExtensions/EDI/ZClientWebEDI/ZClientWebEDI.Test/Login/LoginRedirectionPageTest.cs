using System;
using System.Collections.Specialized;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class LoginRedirectionPageTest : TestCaseWithFactory
	{
		public void TestValidContactsAndPassword()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.SupersedeWebAccess();
			var contactPassword = "1234";
			contact.SetHashedPassword(contactPassword);
			Factory.Save();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_PER = contact.OC_PER;
			contact2.OC_WebAccessEnabled = true;
			var webUserAdminManager = new WebUserAdminManager(contact);
			var message = webUserAdminManager.SetMasterPassword("Th1sIsMyPassword!", "Th1sIsMyPassword!", PasswordInstructionType.Set);
			AssertEquals("Precondition", webUserAdminManager.PasswordChangeSuccess, message);
			Factory.Save();
			Assert("Precondition", contact.Person.HasPassword);
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			TestPage.RequestQueryString_Exposed.Remove(LoginRouter.QueryStringKey);
			TestPage.RequestQueryString_Exposed.Add(LoginRouter.QueryStringKey, encodedQueryString);
			TestPage.OnLoadForTest();
			Assert("Precondition", TestPage.HelperForTest.HasValidContacts);
			Assert("Login Instructions should be visible", TestPage.ContactsBoxForTest.Visible);
			Assert("Error message should not be visible", !TestPage.ErrorMessageForTest.Visible);
		}

		public void TestInvalidQueryString()
		{
			TestPage.OnLoadForTest();
			AssertEquals("Precondition", null, TestPage.RequestQueryString_Exposed[LoginRouter.QueryStringKey]);
			Assert("Precondition", !TestPage.HelperForTest.HasValidContacts);
			Assert("Login Instructions should not be visible", !TestPage.ContactsBoxForTest.Visible);
			Assert("Error message should be visible", TestPage.ErrorMessageForTest.Visible);
			AssertEquals("Error message should be set", "You have been redirected here incorrectly. Please attempt login again and if this issue is recurring, contact your system administrator.", TestPage.ErrorMessageForTest.Text);
		}

		public void TestContactInTokenHasNoPersonPassword()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.SupersedeWebAccess();
			var contactPassword = "1234";
			contact.SetHashedPassword(contactPassword);
			Factory.Save();
			var contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.OC_PER = contact.OC_PER;
			contact2.OC_WebAccessEnabled = true;
			Factory.Save();
			Assert("Precondition", contact.Person.PER_PasswordHash.IsEmpty);
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			TestPage.RequestQueryString_Exposed.Remove(LoginRouter.QueryStringKey);
			TestPage.RequestQueryString_Exposed.Add(LoginRouter.QueryStringKey, encodedQueryString);
			TestPage.OnLoadForTest();
			Assert("Login Instructions should not be visible", !TestPage.ContactsBoxForTest.Visible);
			Assert("Error message should be visible", TestPage.ErrorMessageForTest.Visible);
			AssertEquals("Error message should be set", "You have been redirected here incorrectly. Please attempt login again and if this issue is recurring, contact your system administrator.", TestPage.ErrorMessageForTest.Text);
		}

		public void TestHasPasswordButNoLoginContacts()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_WebAccessEnabled = true;
			contact.SupersedeWebAccess();
			var contactPassword = "1234";
			contact.SetHashedPassword(contactPassword);
			Factory.Save();
			var webUserAdminManager = new WebUserAdminManager(contact);
			var message = webUserAdminManager.SetMasterPassword("Th1sIsMyPassword!", "Th1sIsMyPassword!", PasswordInstructionType.Set);
			AssertEquals("Precondition", webUserAdminManager.PasswordChangeSuccess, message);
			Factory.Save();
			Assert("Precondition", contact.Person.HasPassword);
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, token } };
			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			TestPage.RequestQueryString_Exposed.Remove(LoginRouter.QueryStringKey);
			TestPage.RequestQueryString_Exposed.Add(LoginRouter.QueryStringKey, encodedQueryString);
			TestPage.OnLoadForTest();
			Assert("Precondition", !TestPage.HelperForTest.HasValidContacts);
			Assert("Login Instructions should not be visible", !TestPage.ContactsBoxForTest.Visible);
			Assert("Error message should be visible", TestPage.ErrorMessageForTest.Visible);
			AssertEquals("Error message should be set", "You have been redirected here incorrectly. Please attempt login again and if this issue is recurring, contact your system administrator.", TestPage.ErrorMessageForTest.Text);
		}

		#region Implementation
		LoginRedirectionForTest TestPage
		{
			get
			{
				if (testPage == null)
				{
					testPage = GetNewControl();
				}

				return testPage;
			}
		}

		LoginRedirectionForTest testPage;
		LoginRedirectionForTest GetNewControl()
		{
			var testPage = new LoginRedirectionForTest();
			var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
			method.Invoke(testPage, new object[] { HttpContext.Current });
			return testPage;
		}

		#region Class for Test
		public class LoginRedirectionForTest : LoginRedirection
		{
			public LoginRedirectionForTest()
			{
				ContactsBox = new HtmlGenericControl();
				LoginContactRepeater = new ZRepeater { BindTo = "ContactsForLogin" };
				Controls.Add(LoginContactRepeater);
				ErrorMessage = new ZTextLabel();
			}

			public void OnLoadForTest()
			{
				OnLoad(EventArgs.Empty);
			}

			public NameValueCollection RequestQueryString_Exposed => RequestQueryString;
			public OrgContactSupersededHelper HelperForTest => Helper;
			public HtmlGenericControl ContactsBoxForTest => ContactsBox;
			public ZTextLabel ErrorMessageForTest => ErrorMessage;
			public void ContinueButton_ClickExposed()
			{
				ContinueButton_Click(null, EventArgs.Empty);
			}

			protected override ZGlobal GetNewTestGlobal()
			{
				var result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}

			class GlobalForTest : Global
			{
				public void OnCustomSessionStart()
				{
					base.OnCustomSessionStart(this, EventArgs.Empty);
				}
			}
		}
		#endregion
		#endregion
	}
}
