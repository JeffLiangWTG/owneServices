using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class TermsAndConditionsTest : TestCaseWithFactory
	{
		public void TestAccept_Decline_NeedSignWebContract()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Contact 1";
			contact.OC_Email = "newuser@cargowise.com";
			contact.OC_WebAccessEnabled = true;
			Factory.Save();
			var orgTemplate = new NotificationEmailTemplate();
			orgTemplate.EmailSubject = "Org 001";
			orgTemplate.EmailBody = "Org 111";
			EDIDataRegistry.Instance.MyAccountTermsAndConditionsContent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, orgTemplate);
			var contactTemplate = new NotificationEmailTemplate();
			contactTemplate.EmailSubject = "Contact 002";
			contactTemplate.EmailBody = "Contact 222";
			EDIDataRegistry.Instance.MyAccountContactTermsAndConditionsContent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, contactTemplate);
			var emailTemplate = new NotificationEmailTemplate();
			emailTemplate.EmailSubject = "Email 003";
			emailTemplate.EmailBody = "Email 333";
			EDIDataRegistry.Instance.MyAccountTermsAndConditionsNotificationEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emailTemplate);
			EDIDataRegistry.Instance.MyAccountTermsAndConditionsNotificationEmailSenderName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Admin");
			EDIDataRegistry.Instance.MyAccountTermsAndConditionsNotificationEmailSenderAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "admin@cw1.com");
			var page = GetTestPage(true, contact);
			AssertEquals(true, ((MyAccountWebContract)page.DataSource).NeedSignWebContract);
			page.Decline();
			page = GetTestPage(true, contact);
			AssertEquals(true, ((MyAccountWebContract)page.DataSource).NeedSignWebContract);
			page.Accept();
			page = GetTestPage(true, contact);
			AssertEquals(true, ((MyAccountWebContract)page.DataSource).NeedSignWebContract);
			page.Decline();
			page = GetTestPage(true, contact);
			AssertEquals(true, ((MyAccountWebContract)page.DataSource).NeedSignWebContract);
			page.Accept();
			page = GetTestPage(true, contact);
			AssertEquals(false, ((MyAccountWebContract)page.DataSource).NeedSignWebContract);
			var orgLogs = string.Join("\r\n", org.Logs.Find(new ZQuery()).Where(x => x.SL_SE_NKEvent == "CTA").Select(x => $"{x.SL_Reference}").OrderBy(x => x));
			var contactLogs = string.Join("\r\n", contact.Logs.Find(new ZQuery()).Where(x => x.SL_SE_NKEvent == "CTA").Select(x => $"{x.SL_Reference}").OrderBy(x => x));
			CombineAssertions(() =>
			{
				AssertEquals(@"Web Contract Rejected (Org 001): Contact 1[newuser@cargowise.com]
Web Contract Rejected (Org 001): From 1.2.3.4
Web Contract Signed (Org 001): Contact 1[newuser@cargowise.com]
Web Contract Signed (Org 001): From 1.2.3.4", orgLogs);
				AssertEquals(@"Web Contract Rejected (Contact 002): Contact 1[newuser@cargowise.com]
Web Contract Rejected (Contact 002): From 1.2.3.4
Web Contract Signed (Contact 002): Contact 1[newuser@cargowise.com]
Web Contract Signed (Contact 002): From 1.2.3.4", contactLogs);
			});
		}

		public void TestOnLoad_OrgHeaderWebContractMessage()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "Mehhhh";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Contact 1";
			contact.OC_Email = "newuser@cargowise.com";
			contact.OC_WebAccessEnabled = true;
			Factory.Save();
			var orgTemplate = new NotificationEmailTemplate();
			orgTemplate.EmailSubject = "Org 001";
			orgTemplate.EmailBody = "Org 111";
			EDIDataRegistry.Instance.MyAccountTermsAndConditionsContent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, orgTemplate);
			var contactTemplate = new NotificationEmailTemplate();
			contactTemplate.EmailSubject = "Contact 002";
			contactTemplate.EmailBody = "Contact 222";
			EDIDataRegistry.Instance.MyAccountContactTermsAndConditionsContent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, contactTemplate);
			var emailTemplate = new NotificationEmailTemplate();
			emailTemplate.EmailSubject = "Email 003";
			emailTemplate.EmailBody = "Email 333";
			EDIDataRegistry.Instance.MyAccountTermsAndConditionsNotificationEmailTemplate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emailTemplate);
			EDIDataRegistry.Instance.MyAccountTermsAndConditionsNotificationEmailSenderName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Admin");
			EDIDataRegistry.Instance.MyAccountTermsAndConditionsNotificationEmailSenderAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "admin@cw1.com");
			var page = GetTestPage(true, contact);
			AssertEquals("Precondition", true, ((MyAccountWebContract)page.DataSource).NeedSignWebContract);
			AssertEquals("Precondition", typeof(EDIOrgHeaderWebContract), page.DataSource.GetType());
			page.OnLoad();
			AssertEquals("Should show org level message", FormattableString.Invariant($"I hereby certify that I am authorised to agree to this Agreement on behalf of {org.OH_FullName}, and {org.OH_FullName} agrees to be bound by the terms and conditions of this Agreement."), page.AuthorisedUserCheckBoxExposed.Text);
		}

		public void TestInvalidQueryString()
		{
			var page = GetTestPage(false);
			page.OnLoad();
			AssertEquals(false, page.AcknowledgeWrapperExposed.Visible);
			AssertEquals("The link which directed you to this page was invalid. Try logging in again and contact your system administrator if this issue persists.", page.ErrorMessageExposed.Text);
		}

		public void TestDecline_NotLoggedIn()
		{
			var page = GetTestPage(false);
			AssertNoExceptionThrown(delegate
			{
				page.Decline();
			});
		}

		TermsAndConditionsForTest GetTestPage(bool login = true, OrgContact contact = null)
		{
			var page = new TermsAndConditionsForTest();
			var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(HttpContext) }, null);
			method.Invoke(page, new object[] { HttpContext.Current });
			if (login)
			{
				var token = LoginRouterIdentityManager.GenerateToken(contact);
				var secureQueryString = new SecureQueryString { { LoginRouter.IdentityTokenQueryStringKey, token } };
				var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
				page.Request.QueryString.Remove(LoginRouter.QueryStringKey);
				page.Request.QueryString.Add(LoginRouter.QueryStringKey, encodedQueryString);
			}

			page.LoadOrCreateDataSource();
			return page;
		}

		class TermsAndConditionsForTest : TermsAndConditions
		{
			public TermsAndConditionsForTest()
			{
				AuthorisedUserCheckBox = new CheckBox();
				AcknowledgeWrapper = new HtmlGenericControl();
				WebContractContentHolder = new HtmlGenericControl();
				ContentChangedMessage = new ZTextLabel();
				ErrorMessage = new ZTextLabel();
			}

			public new void LoadOrCreateDataSource() => base.LoadOrCreateDataSource();
			public void OnLoad() => base.OnLoad(null);
			public Label ErrorMessageExposed => ErrorMessage;
			public HtmlGenericControl AcknowledgeWrapperExposed => AcknowledgeWrapper;
			public CheckBox AuthorisedUserCheckBoxExposed => AuthorisedUserCheckBox;
			protected override ZGlobal GetNewTestGlobal()
			{
				var result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}

			public void Accept()
			{
				AuthorisedUserCheckBox.Checked = true;
				try
				{
					AcceptButton_Click(null, null);
				}
				catch (HttpException)
				{
				}
			}

			public void Decline()
			{
				AuthorisedUserCheckBox.Checked = false;
				try
				{
					DeclineButton_Click(null, null);
				}
				catch (HttpException)
				{
				}
			}

			protected override string GetRequestIpAddress() => "1.2.3.4";
		}

		class GlobalForTest : Global
		{
			public void OnCustomSessionStart()
			{
				base.OnCustomSessionStart(this, EventArgs.Empty);
			}
		}
	}
}
