using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Testing
{
	[TestedType(typeof(CredentialAuthorisationForm))]
	sealed class CredentialAuthorisationFormTest : ZFormBasherTest
	{
		GlbExternalPassword_GB password;

		protected override Form GetFormToBashCore()
		{
			var externalPassword = Factory.NewWithValidTestData<GlbExternalPassword_GB>();
			var authorisationManager = new GlbExternalPasswordAuthorisationManager();

			var testForm = new CredentialAuthorisationForm(externalPassword, authorisationManager);
			return testForm;
		}

		protected override void SetUp()
		{
			base.SetUp();

			password = Factory.New<GlbExternalPassword_GB>();
			password.GP_GC = Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.PK;
			password.Badge = "ABC";
			password.EORI = "123";
		}

		void SetupRefData()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);

			var refSysConfTypeClientIdLive = refHelper.CreateRefSysConfigType("ClientLive", "WTG Client Id Live", "WTG Client Id (Live) used when authorising CW1 to act on user's behalf for certain HMRC applications");
			var refSysConfTypeClientIdTest = refHelper.CreateRefSysConfigType("ClientTest", "WTG Client Id Test", "WTG Client Id (Test) used when authorising CW1 to act on user's behalf for certain HMRC applications");
			refHelper.CreateRefSysConfig(refSysConfTypeClientIdLive.ZRT_ConfigCode, "LiveClientID", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			refHelper.CreateRefSysConfig(refSysConfTypeClientIdTest.ZRT_ConfigCode, "TestClientID", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			var refSysConfTypeCallbackUrlLive = refHelper.CreateRefSysConfigType("ClbUrlLive", "WTG Callback Url Live", "WTG Callback Url (Live) used when authorising CW1 to act on user's behalf for certain HMRC applications");
			var refSysConfTypeCallbackUrlTest = refHelper.CreateRefSysConfigType("ClbUrlTest", "WTG Callback Url Test", "WTG Callback Url (Test) used when authorising CW1 to act on user's behalf for certain HMRC applications");
			refHelper.CreateRefSysConfig(refSysConfTypeCallbackUrlLive.ZRT_ConfigCode, "https://gbcds.wisegrid.net", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			refHelper.CreateRefSysConfig(refSysConfTypeCallbackUrlTest.ZRT_ConfigCode, "https://gbcds-test.wisegrid.net", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			var refSysConfTypePath = refHelper.CreateRefSysConfigType("AppUrlPath", "HMRC application authorisation URL (path)", "The path of the URL that CW1 will build and to ask users to authorise it to act on their behalf for certain HMRC applications");
			var refSysConfTypeHstLive = refHelper.CreateRefSysConfigType("AppHstLive", "HMRC application authorisation URL (live host)",
				"The host of the LIVE URL that CW1 will build and to ask users to authorise it to act on their behalf for certain HMRC applications");
			var refSysConfTypeHstTest = refHelper.CreateRefSysConfigType("AppHstTest", "HMRC application authorisation URL (test host)", "The host of the TEST URL that CW1 will build and to ask users to authorise it to act on their behalf for certain HMRC applications");
			refHelper.CreateRefSysConfig(refSysConfTypePath.ZRT_ConfigCode, "oauth/authorize?response_type=code&client_id=[WTGClientID]&scope=write:customs-declaration%20write:customs-inventory-linking-exports%20write:customs-declarations-information%20common-transit-convention-traders%20write:goods-movement-system%20write:import-control-system[MoreScopes]&state=[CredentialID]&redirect_uri=[CallbackUrl]", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			refHelper.CreateRefSysConfig(refSysConfTypeHstLive.ZRT_ConfigCode, "https://api.service.hmrc.gov.uk/", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			refHelper.CreateRefSysConfig(refSysConfTypeHstTest.ZRT_ConfigCode, "https://test-api.service.hmrc.gov.uk/", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			Factory.Save();
		}

		public void TestForm()
		{
			SetupRefData();
			var authorisationManager = new GlbExternalPasswordAuthorisationManager();
			CredentialAuthorisationForm form;
			using (form = new CredentialAuthorisationForm(password, authorisationManager))
			{
				form.Show();
				AssertEquals("Requesting authorization for Badge: ABC - EORI: 123", form.label2.Text);
				Assert(!form.ButtonAuthorise.Enabled);
				Assert(!form.checkBox1.Checked);
				Assert(!form.checkBox2.Checked);
				Assert(!form.checkBox3.Checked);
				Assert(!form.checkBox4.Checked);

				form.checkBox1.Checked = true;
				form.checkBox2.Checked = true;
				form.checkBox3.Checked = true;
				form.checkBox4.Checked = true;
				Assert(form.ButtonAuthorise.Enabled);

				Assert(!authorisationManager.IsAuthorised);
				form.ButtonAuthorise.PerformClick();
				Assert(authorisationManager.IsAuthorised);
			}
		}

		public void TestFormCDSReferences()
		{
			SetupRefData();
			var authorisationManager = new GlbExternalPasswordAuthorisationManager();
			CredentialAuthorisationForm form;
			using (form = new CredentialAuthorisationForm(password, authorisationManager))
			{
				form.Show();
				AssertNotContains("CDS", form.checkBox2.Text);
				AssertNotContains("CDS", form.label1.Text);
			}
		}

		public void TestFormIsNotAuthorisedWhenFormClosed()
		{
			SetupRefData();
			var authorisationManager = new GlbExternalPasswordAuthorisationManager();
			CredentialAuthorisationForm form;
			using (form = new CredentialAuthorisationForm(password, authorisationManager))
			{
				AssertEquals("Requesting authorization for Badge: ABC - EORI: 123", form.label2.Text);
				Assert(!form.ButtonAuthorise.Enabled);
				Assert(!form.checkBox1.Checked);
				Assert(!form.checkBox2.Checked);
				Assert(!form.checkBox3.Checked);
				Assert(!form.checkBox4.Checked);

				form.checkBox1.Checked = true;
				form.checkBox2.Checked = true;
				form.checkBox3.Checked = true;
				form.checkBox4.Checked = true;
				Assert(form.ButtonAuthorise.Enabled);

				Assert(!authorisationManager.IsAuthorised);
				form.Close();
				Assert(!authorisationManager.IsAuthorised);
			}
		}

		public void TestWarningIsShownWhenClickingAuthoriseIfNoRefData()
		{
			var authorisationManager = new GlbExternalPasswordAuthorisationManager();
			CredentialAuthorisationForm form;
			using (form = new CredentialAuthorisationForm(password, authorisationManager))
			{
				form.Show();

				form.checkBox1.Checked = true;
				form.checkBox2.Checked = true;
				form.checkBox3.Checked = true;
				form.checkBox4.Checked = true;

				Assert(!authorisationManager.IsAuthorised);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.ButtonAuthorise.PerformClick();
				Assert(!authorisationManager.IsAuthorised);
				// Assert popup is shown to user
				AssertContains("Cannot authorize due to missing data!", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
