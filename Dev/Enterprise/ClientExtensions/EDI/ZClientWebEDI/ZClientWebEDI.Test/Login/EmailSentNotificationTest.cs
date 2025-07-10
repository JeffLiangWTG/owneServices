using System;
using System.Reflection;
using System.Web;
using System.Web.UI;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class EmailSentNotificationTest : ZPageLifeCycleTest
	{
		[ExpectNoExceptions]
		public void TestPageLoad_InvalidRequest()
		{
			var page1 = GetPageForTest();
			page1.DoPageLoad();
		}

		public void TestPageLoad_EmailVerificationSentSuccessfully()
		{
			var secureQueryString = new SecureQueryString();
			secureQueryString.Add(UserEmailVerificationRoutingDescriptor.EmailVerificationSentUrlKey, new ZBool(true).ToString());
			secureQueryString.Add(UserEmailVerificationRoutingDescriptor.EmailVerificationAddressUrlKey, "user@cw1.com");
			var page1 = GetPageForTest();
			page1.Request.QueryString.Add(SecureQueryString.QueryStringKey, secureQueryString.ToString());
			page1.DoPageLoad();
			AssertEquals("Text should reflect email being sent", "Account authentication is required to login from this system. Follow the link in the verification email sent to user@cw1.com to confirm this is a valid account.", page1.MessageLabel_Exposed.Text);
		}

		public void TestPageLoad_EmailVerificationSentUnsuccessfully()
		{
			var secureQueryString = new SecureQueryString();
			secureQueryString.Add(UserEmailVerificationRoutingDescriptor.EmailVerificationSentUrlKey, new ZBool(false).ToString());
			secureQueryString.Add(UserEmailVerificationRoutingDescriptor.EmailVerificationAddressUrlKey, "user@cw1.com");
			var page1 = GetPageForTest();
			page1.Request.QueryString.Add(SecureQueryString.QueryStringKey, secureQueryString.ToString());
			page1.DoPageLoad();
			AssertEquals("Text should reflect email failing to be sent", "Account authentication is required to login from this system, but the verification email failed to send to address: user@cw1.com. Please attempt to login again and contact your system administrator if this issue persists.", page1.MessageLabel_Exposed.Text);
		}

		public void TestPageLoad_EmailVerificationSentNoEmail()
		{
			var secureQueryString = new SecureQueryString();
			secureQueryString.Add(UserEmailVerificationRoutingDescriptor.EmailVerificationSentUrlKey, new ZBool(false).ToString());
			secureQueryString.Add(UserEmailVerificationRoutingDescriptor.EmailVerificationAddressUrlKey, string.Empty);
			var page1 = GetPageForTest();
			page1.Request.QueryString.Add(SecureQueryString.QueryStringKey, secureQueryString.ToString());
			page1.DoPageLoad();
			AssertEquals("Text should reflect email address absence", "Account authentication is required to login from this system, but the account used for login has no email. Please enter an email address for the account before attempting login again.", page1.MessageLabel_Exposed.Text);
		}

		public void TestPageLoad_ShouldDisplayEmailVerificationSentMessageSent()
		{
			var secureQueryString = new SecureQueryString();
			secureQueryString.Add(UserEmailVerificationRoutingDescriptor.EmailVerificationSentUrlKey, new ZBool(true).ToString());
			var page = GetPageForTest();
			page.Request.QueryString.Add(SecureQueryString.QueryStringKey, secureQueryString.ToString());
			page.DoPageLoad();
			AssertEquals(true, page.ShouldDisplayEmailVerificationSentMessage);
			AssertEquals(true, page.EmailVerificationSentSuccessfully);
		}

		public void TestPageLoad_ShouldDisplayEmailVerificationSentMessageFailed()
		{
			var secureQueryString = new SecureQueryString();
			secureQueryString.Add(UserEmailVerificationRoutingDescriptor.EmailVerificationSentUrlKey, new ZBool(false).ToString());
			var page = GetPageForTest();
			page.Request.QueryString.Add(SecureQueryString.QueryStringKey, secureQueryString.ToString());
			page.DoPageLoad();
			AssertEquals(true, page.ShouldDisplayEmailVerificationSentMessage);
			AssertEquals(false, page.EmailVerificationSentSuccessfully);
		}

		public void TestPageLoad_ShouldDisplayEmailVerificationSentMessageBadData()
		{
			var secureQueryString = new SecureQueryString();
			secureQueryString.Add(UserEmailVerificationRoutingDescriptor.EmailVerificationSentUrlKey, "blah");
			var page = GetPageForTest();
			page.Request.QueryString.Add(SecureQueryString.QueryStringKey, secureQueryString.ToString());
			page.DoPageLoad();
			AssertEquals(false, page.ShouldDisplayEmailVerificationSentMessage);
			AssertEquals(false, page.EmailVerificationSentSuccessfully);
		}

		EmailSentNotificationForTest GetPageForTest()
		{
			var page = new EmailSentNotificationForTest();
			MethodInfo method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
			method.Invoke(page, new object[] { HttpContext.Current });
			page.AppInstance.SiteUser.Login("MEHMEH", "newuser@cargowise.com", CWSupportLoginToken.TokenForTest);
			return page;
		}

		protected override ZPage GetNewPage()
		{
			return GetPageForTest();
		}
	}
}
