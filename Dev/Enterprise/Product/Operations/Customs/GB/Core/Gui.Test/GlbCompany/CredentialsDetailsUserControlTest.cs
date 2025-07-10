using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.GB.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Testing
{
	[TestedType(typeof(CredentialsDetailsUserControl))]
	sealed class CredentialsDetailsUserControlTest : BasherTest
	{
		public void TestAuthorisationFormByAuthoriseMenu_Click()
		{
			SetupData();
			using (var form = new GlbCompanyForm(company))
			{
				form.Show();
				var companyTabControl = form.Controls.Find("CompanyTabControl", true).First() as ZTemplateTabControl;
				var companyCredentialsPlugIn = (CompanyCredentialsPlugin)companyTabControl.PlugIns.GetPlugIn(ControllerIDs.CompanyCredentialsPlugIn);
				companyTabControl.SelectedTab = companyCredentialsPlugIn.TabPage;

				var credentialsUserControl = companyCredentialsPlugIn.UserControl.Controls.Find("CredentialsDetailsUserControl", true).First() as CredentialsDetailsUserControl;
				var menuItem = credentialsUserControl.AccUserGrid.ContextMenu.MenuItems.FindByText("Authorize");
				credentialsUserControl.AccUserGrid.SelectSingleElement(password2);

				menuItem.PerformClick();
				var authorisationForm = ZFormModaliser.LastFormShownDialogForTest as CredentialAuthorisationForm;
				AssertNotNull("CredentialAuthorisationForm should have been shwon.", authorisationForm);
				AssertEquals("Requesting authorization for Badge: 2 - EORI: A", authorisationForm.label2.Text);
			}
		}

		public void TestSaveDataFirstPopupByAuthoriseMenu_Click()
		{
			SetupData();
			using (var form = new GlbCompanyForm(company))
			{
				form.Show();
				company.GC_Name = "COM1";
				var companyTabControl = form.Controls.Find("CompanyTabControl", true).First() as ZTemplateTabControl;
				var companyCredentialsPlugIn = (CompanyCredentialsPlugin)companyTabControl.PlugIns.GetPlugIn(ControllerIDs.CompanyCredentialsPlugIn);
				companyTabControl.SelectedTab = companyCredentialsPlugIn.TabPage;

				var credentialsUserControl = companyCredentialsPlugIn.UserControl.Controls.Find("CredentialsDetailsUserControl", true).First() as CredentialsDetailsUserControl;
				var contextMenuItem = credentialsUserControl.AccUserGrid.ContextMenu.MenuItems.FindByText("Authorize");
				credentialsUserControl.AccUserGrid.SelectSingleElement(password2);
				contextMenuItem.PerformClick();
				AssertEquals("You need to save first. Would you like to save now and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessages();
				contextMenuItem.PerformClick();
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);

				company.GC_Name = "COM2";
				var mainMenuItem = form.Menu.MenuItems.FindByText("Authorize").MenuItems.FindByText("Authorize Credentials");
				UnitTestUserNotification.Instance.ClearMessages();
				mainMenuItem.PerformClick();
				AssertEquals("You need to save first. Would you like to save now and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessages();
				mainMenuItem.PerformClick();
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void SetupData()
		{
			company = Factory.NewWithValidTestData<MasterFiles.Business.GlbCompany>();
			company.GC_Code = "CK1";

			password1 = Factory.NewWithValidTestData<GlbExternalPassword_GB>();
			password1.GP_GC = company.PK;
			password1.GP_UserID = "A.1";

			password2 = Factory.NewWithValidTestData<GlbExternalPassword_GB>();
			password2.GP_GC = company.PK;
			password2.GP_UserID = "A.2";

			password3 = Factory.NewWithValidTestData<GlbExternalPassword_GB>();
			password3.GP_GC = company.PK;
			password3.GP_UserID = "A.3";
			password3.GP_GC = company.PK;
			Factory.Save();
		}

		public override Form GetFormToBash()
		{
			var form = new ZForm();
			form.CaptionRenderingEnabled = true;

			userControl = new CredentialsDetailsUserControl();
			userControl.Dock = DockStyle.Fill;

			form.Controls.Add(userControl);
			var provider = GlbCompanyWrapperProvider.GetProvider(Core.Constants.CountryCodes.UnitedKingdom);
			form.SetDataBinding(provider.GetWrapper(company), "");

			return form;
		}

		CredentialsDetailsUserControl userControl;

		MasterFiles.Business.GlbCompany company;
		GlbExternalPassword_GB password1;
		GlbExternalPassword_GB password2;
		GlbExternalPassword_GB password3;
	}
}
