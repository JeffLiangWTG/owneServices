using System;
using System.Web.Security.AntiXss;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class ProfileTest : TestCaseWithFactory
	{
		public void TestPageLoad_PopulatePersonalInfo()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "<svg/onload=alert(document.domain)>";

			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "<svg/onload=alert(document.domain)>";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();

			Page.AppInstance.SiteUser.Login(org.OH_Code, "newuser@cargowise.com", "1234");
			Page.DoPageLoad();

			AssertEquals(AntiXssEncoder.HtmlEncode(newContact.OC_ContactName, false), Page.UserNameLabel_Expose.Text);
			AssertEquals(AntiXssEncoder.HtmlEncode(org.OH_FullName, false), Page.CompanyNameLabel_Expose.Text);
			AssertEquals("newuser@cargowise.com", Page.EmailLabel_Expose.Text);
			AssertNullOrEmpty(Page.PersonalEmailLabel_Expose.Text);
			Assert(!Page.PersonalEmailLabel_Expose.Visible);
			Assert(!Page.ChangePersonalEmailButton_Expose.Visible);

			newContact.Person.PER_EmailAddress = "newuser@gmail.com";
			Factory.Save();
			newContact.Person.Reload();
			Page.AppInstance.SiteUser.Logout();
			Page.AppInstance.SiteUser.Login(org.OH_Code, "newuser@cargowise.com", "1234");
			Page.DoPageLoad();

			AssertEquals("newuser@gmail.com", Page.PersonalEmailLabel_Expose.Text);
			AssertEquals("newuser@gmail.com", Page.RegisteredPersonalEmail_Expose.Value);
			AssertEquals("display:none", Page.PersonalEmailTextBox_Expose.Attributes["style"]);
			Assert(!Page.RegisterPersonalEmailButton_Expose.Visible);
		}

		public void TestPageLoad_ShouldHideRelatedAccountsIfNoPersonPassword()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "MEHMEH";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();
			Page.AppInstance.SiteUser.Login(org.OH_Code, "newuser@cargowise.com", "1234");
			Page.DoPageLoad();
			Assert(!Page.TabContainer_Expose.Visible);
			newContact.Person.SetHashedPassword("1234");
			Factory.Save();
			newContact.Person.Reload();
			page = GetPageForTest();
			Page.AppInstance.SiteUser.Logout();
			Page.AppInstance.SiteUser.Login(org.OH_Code, "newuser@cargowise.com", "1234");
			Page.DoPageLoad();
			Assert(Page.TabContainer_Expose.Visible);
		}

		public void TestSavePersonalEmail()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			Factory.Save();
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();
			Page.AppInstance.SiteUser.Login(org.OH_Code, "newuser@cargowise.com", "1234");
			Page.SaveEmailButton_OnClick();
			AssertEquals("The personal email address cannot be empty.", Page.MessageLabel_Expose.Text);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			Page.PersonalEmailTextBox_Expose.Text = "newuser";
			AssertEquals("The personal email address cannot be empty.", Page.MessageLabel_Expose.Text);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			Page.PersonalEmailTextBox_Expose.Text = "newuser@gmail.com";
			Page.SaveEmailButton_OnClick();
			AssertEquals("An email was sent to newuser@gmail.com to confirm personal email.", Page.MessageLabel_Expose.Text);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains("/Admin/RegisterPersonalEmail.aspx?RegisterKey=", email.Body);
			AssertEquals("Personal Email Confirmation", email.Subject);
			AssertEquals("newuser@gmail.com", email.Recipients[0].Email);
			AssertEquals("display:none", Page.SaveEmailButton_Expose.Attributes["style"]);
			AssertEquals("display:none", Page.PersonalEmailTextBox_Expose.Attributes["style"]);
			Assert(Page.EmailLabel_Expose.Visible);
			Assert(Page.RegisterPersonalEmailButton_Expose.Visible);
		}

		public void TestChangePersonalEmail()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();
			newContact.Person.PER_EmailAddress = "newuser@gmail.com";
			Factory.Save();
			Page.AppInstance.SiteUser.Login(org.OH_Code, "newuser@cargowise.com", "1234");
			Page.ChangePersonalEmailButton_OnClick();
			Assert(Page.PersonalEmailTextBox_Expose.Visible);
			Assert(!Page.PersonalEmailLabel_Expose.Visible);
			AssertEquals("display:inline", Page.SaveEmailButton_Expose.Attributes["style"]);
			Assert(!Page.ChangePersonalEmailButton_Expose.Visible);
		}

		public void TestRegisterPersonalEmail()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			Factory.Save();
			Page.AppInstance.SiteUser.LoginSupportForTest(org.OH_Code);
			Page.RegisterPersonalEmailButton_OnClick();
			Assert(Page.PersonalEmailTextBox_Expose.Visible);
			Assert(!Page.PersonalEmailLabel_Expose.Visible);
			AssertEquals("display:inline", Page.SaveEmailButton_Expose.Attributes["style"]);
			Assert(!Page.RegisterPersonalEmailButton_Expose.Visible);
		}

		public void TestSwitchPage()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "MEHMEH";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();
			newContact.Person.SetHashedPassword("1234");
			Factory.Save();
			Page.AppInstance.SiteUser.Login(org.OH_Code, "newuser@cargowise.com", "1234");
			Page.DoPageLoad();
			Assert("Precondition", Page.TabContainer_Expose.Visible);
			Assert("Precondition", Page.TabPersonalEmail_Expose.Visible);
			Page.SwitchPage_Click();
			Assert("Should now show user accounts", !Page.TabPersonalEmail_Expose.Visible);
			Assert("Should now show user accounts", Page.TabUserAccounts_Expose.Visible);
			Assert("Should now enable personal email button", Page.ButtonPersonalEmail_Expose.Enabled);
			Assert("Should now disable user accounts button", !Page.ButtonUserAccounts_Expose.Enabled);
			Page.SwitchPage_Click();
			Assert("Should now show personal email", Page.TabPersonalEmail_Expose.Visible);
			Assert("Should now show personal email", !Page.TabUserAccounts_Expose.Visible);
			Assert("Should now disable personal email button", !Page.ButtonPersonalEmail_Expose.Enabled);
			Assert("Should now enable user accounts button", Page.ButtonUserAccounts_Expose.Enabled);
		}

		public void TestSwitchPage_ConfirmationCancelClick()
		{
			SetupForConfirmationPrompt();
			Page.SwitchPage_Click();
			Assert("Should be unchanged", !Page.TabPersonalEmail_Expose.Visible);
			Assert("Should be unchanged", Page.TabUserAccounts_Expose.Visible);
			Assert("Should be unchanged", Page.ButtonPersonalEmail_Expose.Enabled);
			Assert("Should be unchanged", !Page.ButtonUserAccounts_Expose.Enabled);
			AssertEquals("Should update css class of confirmation div", "CiModalOn", Page.ConfirmationDiv_Expose.CssClass);
			Page.ConfirmationCancel_Click();
			Assert("Should be unchanged", !Page.TabPersonalEmail_Expose.Visible);
			Assert("Should be unchanged", Page.TabUserAccounts_Expose.Visible);
			Assert("Should be unchanged", Page.ButtonPersonalEmail_Expose.Enabled);
			Assert("Should be unchanged", !Page.ButtonUserAccounts_Expose.Enabled);
			AssertEquals("Should update css class of confirmation div", "CiModalOff", Page.ConfirmationDiv_Expose.CssClass);
		}

		public void TestSwitchPage_ConfirmationNoClick()
		{
			SetupForConfirmationPrompt();
			Page.SwitchPage_Click();
			Assert("Should be unchanged", !Page.TabPersonalEmail_Expose.Visible);
			Assert("Should be unchanged", Page.TabUserAccounts_Expose.Visible);
			Assert("Should be unchanged", Page.ButtonPersonalEmail_Expose.Enabled);
			Assert("Should be unchanged", !Page.ButtonUserAccounts_Expose.Enabled);
			AssertEquals("Should update css class of confirmation div", "CiModalOn", Page.ConfirmationDiv_Expose.CssClass);
			Page.ConfirmationNo_Click();
			Assert("Should show personal email", Page.TabPersonalEmail_Expose.Visible);
			Assert("Should show personal email", !Page.TabUserAccounts_Expose.Visible);
			Assert("Should be disabled", !Page.ButtonPersonalEmail_Expose.Enabled);
			Assert("Should be enabled", Page.ButtonUserAccounts_Expose.Enabled);
			AssertEquals("Should update css class of confirmation div", "CiModalOff", Page.ConfirmationDiv_Expose.CssClass);
		}

		public void TestSwitchPage_ConfirmationYesClick()
		{
			SetupForConfirmationPrompt();
			Page.SwitchPage_Click();
			Assert("Should be unchanged", !Page.TabPersonalEmail_Expose.Visible);
			Assert("Should be unchanged", Page.TabUserAccounts_Expose.Visible);
			Assert("Should be unchanged", Page.ButtonPersonalEmail_Expose.Enabled);
			Assert("Should be unchanged", !Page.ButtonUserAccounts_Expose.Enabled);
			AssertEquals("Should update css class of confirmation div", "CiModalOn", Page.ConfirmationDiv_Expose.CssClass);
			Page.ConfirmationYes_Click();
			Assert("Should show personal email", Page.TabPersonalEmail_Expose.Visible);
			Assert("Should show personal email", !Page.TabUserAccounts_Expose.Visible);
			Assert("Should be disabled", !Page.ButtonPersonalEmail_Expose.Enabled);
			Assert("Should be enabled", Page.ButtonUserAccounts_Expose.Enabled);
			AssertEquals("Should update css class of confirmation div", "CiModalOff", Page.ConfirmationDiv_Expose.CssClass);
		}

		public void TestSaveEmailButton_OnClick_ShouldNotThrowNREIfEnvironmentVariablesAreNull()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			Factory.Save();
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();
			Page.AppInstance.SiteUser.Login(org.OH_Code, "newuser@cargowise.com", "1234");
			Page.PersonalEmailTextBox_Expose.Text = "newuser@gmail.com";
			Env.ClearUserContext();
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			Page.SaveEmailButton_OnClick();
			AssertEquals("An email was sent to newuser@gmail.com to confirm personal email.", Page.MessageLabel_Expose.Text);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertContains("/Admin/RegisterPersonalEmail.aspx?RegisterKey=", email.Body);
			AssertEquals("Personal Email Confirmation", email.Subject);
			AssertEquals("newuser@gmail.com", email.Recipients[0].Email);
			AssertEquals("display:none", Page.SaveEmailButton_Expose.Attributes["style"]);
			AssertEquals("display:none", Page.PersonalEmailTextBox_Expose.Attributes["style"]);
			Assert(Page.EmailLabel_Expose.Visible);
			Assert(Page.RegisterPersonalEmailButton_Expose.Visible);
		}

		void SetupForConfirmationPrompt()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "MEHMEH";
			org.OH_FullName = "MEHMEH";
			var newContact = org.Contacts.AddNew();
			newContact.OC_ContactName = "NewUser";
			newContact.OC_Email = "newuser@cargowise.com";
			newContact.OC_WebAccessEnabled = true;
			newContact.SetHashedPassword("1234");
			Factory.Save();
			newContact.Person.SetHashedPassword("1234");
			Factory.Save();
			Page.AppInstance.SiteUser.Login(org.OH_Code, "newuser@cargowise.com", "1234");
			Page.DoPageLoad();
			Assert("Precondition", Page.TabContainer_Expose.Visible);
			Assert("Precondition", Page.TabPersonalEmail_Expose.Visible);
			Page.SwitchPage_Click();
			Assert("Precondition: Should now show user accounts", !Page.TabPersonalEmail_Expose.Visible);
			Assert("Precondition: Should now show user accounts", Page.TabUserAccounts_Expose.Visible);
			Assert("Precondition: Should now enable personal email button", Page.ButtonPersonalEmail_Expose.Enabled);
			Assert("Precondition: Should now disable user accounts button", !Page.ButtonUserAccounts_Expose.Enabled);
			Page.UserAccountRelationship_Expose.SetHasChanges(true);
		}

		protected override void SetUp()
		{
			base.SetUp();
			EDIDataRegistry.Instance.MyAccountHostingSiteRootUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
		}

		ProfileForTest GetPageForTest()
		{
			var page = new ProfileForTest();
			page.UserNameLabel_Expose = new ZTextLabel();
			page.CompanyNameLabel_Expose = new ZTextLabel();
			page.EmailLabel_Expose = new ZTextLabel();
			page.PersonalEmailLabel_Expose = new ZTextLabel();
			page.PersonalEmailTextBox_Expose = new ZTextBox();
			page.ChangePersonalEmailButton_Expose = new ZButton();
			page.RegisterPersonalEmailButton_Expose = new ZButton();
			page.SaveEmailButton_Expose = new ZButton();
			page.RegisteredPersonalEmail_Expose = new HiddenField();
			page.Footer_Expose = new HtmlGenericControl();
			page.MessageLabel_Expose = new ZTextLabel();
			page.TabContainer_Expose = new HtmlGenericControl();
			page.TabPersonalEmail_Expose = new Panel();
			page.TabUserAccounts_Expose = new Panel();
			page.ButtonUserAccounts_Expose = new Button();
			page.ButtonPersonalEmail_Expose = new Button();
			page.UserAccountRelationship_Expose = new UserAccountRelationshipControlForTest();
			page.ConfirmationDiv_Expose = new Panel();
			return page;
		}

		ProfileForTest Page => page ?? (page = GetPageForTest());
		ProfileForTest page;
		class ProfileForTest : Profile
		{
			public void DoPageLoad()
			{
				base.Page_Load(this, EventArgs.Empty);
			}

			public void RegisterPersonalEmailButton_OnClick()
			{
				base.RegisterPersonalEmailButton_OnClick(this, EventArgs.Empty);
			}

			public void ChangePersonalEmailButton_OnClick()
			{
				base.ChangePersonalEmailButton_OnClick(this, EventArgs.Empty);
			}

			public void SaveEmailButton_OnClick()
			{
				base.SaveEmailButton_OnClick(this, EventArgs.Empty);
			}

			public void SwitchPage_Click()
			{
				SwitchPage_Click(this, EventArgs.Empty);
			}

			public void ConfirmationYes_Click()
			{
				ConfirmationYes_Click(this, EventArgs.Empty);
			}

			public void ConfirmationNo_Click()
			{
				ConfirmationNo_Click(this, EventArgs.Empty);
			}

			public void ConfirmationCancel_Click()
			{
				ConfirmationCancel_Click(this, EventArgs.Empty);
			}

			public ZTextLabel UserNameLabel_Expose { get => base.UserNameLabel; set => base.UserNameLabel = value; }

			public ZTextLabel CompanyNameLabel_Expose { get => base.CompanyNameLabel; set => base.CompanyNameLabel = value; }

			public ZTextLabel EmailLabel_Expose { get => base.EmailLabel; set => base.EmailLabel = value; }

			public ZTextLabel PersonalEmailLabel_Expose { get => base.PersonalEmailLabel; set => base.PersonalEmailLabel = value; }

			public ZTextBox PersonalEmailTextBox_Expose { get => base.PersonalEmailTextBox; set => base.PersonalEmailTextBox = value; }

			public ZButton ChangePersonalEmailButton_Expose { get => base.ChangePersonalEmailButton; set => base.ChangePersonalEmailButton = value; }

			public ZButton RegisterPersonalEmailButton_Expose { get => base.RegisterPersonalEmailButton; set => base.RegisterPersonalEmailButton = value; }

			public ZButton SaveEmailButton_Expose { get => base.SaveEmailButton; set => base.SaveEmailButton = value; }

			public HiddenField RegisteredPersonalEmail_Expose { get => base.RegisteredPersonalEmail; set => base.RegisteredPersonalEmail = value; }

			public HtmlGenericControl Footer_Expose { get => base.footer; set => base.footer = value; }

			public ZTextLabel MessageLabel_Expose { get => base.MessageLabel; set => base.MessageLabel = value; }

			public HtmlGenericControl TabContainer_Expose { get => TabContainer; set => TabContainer = value; }

			public Panel TabPersonalEmail_Expose { get => TabPersonalEmail; set => TabPersonalEmail = value; }

			public Panel TabUserAccounts_Expose { get => TabUserAccounts; set => TabUserAccounts = value; }

			public Button ButtonUserAccounts_Expose { get => ButtonUserAccounts; set => ButtonUserAccounts = value; }

			public Button ButtonPersonalEmail_Expose { get => ButtonPersonalEmail; set => ButtonPersonalEmail = value; }

			public UserAccountRelationshipControlForTest UserAccountRelationship_Expose { get => (UserAccountRelationshipControlForTest)UserAccountRelationship; set => UserAccountRelationship = value; }

			public Panel ConfirmationDiv_Expose { get => ConfirmationDiv; set => ConfirmationDiv = value; }

			protected override ZGlobal GetNewTestGlobal()
			{
				GlobalForTest result = new GlobalForTest();
				result.OnCustomSessionStart();
				return result;
			}
		}

		class GlobalForTest : Global
		{
			public void OnCustomSessionStart()
			{
				base.OnCustomSessionStart(this, EventArgs.Empty);
			}
		}

		class UserAccountRelationshipControlForTest : UserAccountRelationshipControl
		{
			public override bool HasChanges => hasChanges;
			bool hasChanges;
			public void SetHasChanges(bool hasChanges)
			{
				this.hasChanges = hasChanges;
			}

			public override void SaveChangesButton_Click(object sender, EventArgs e)
			{
				SetHasChanges(false);
			}
		}
	}
}
