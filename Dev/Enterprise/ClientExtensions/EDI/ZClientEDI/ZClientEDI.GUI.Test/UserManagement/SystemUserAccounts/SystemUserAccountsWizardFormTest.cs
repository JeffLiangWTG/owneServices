using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.UserManagement.GUI.Testing
{
	[TestedType(typeof(SystemUserAccountsWizardForm))]
	public class SystemUserAccountsWizardFormTest : ZFormBasherTest
	{
		public void TestAdditionalInformation()
		{
			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			contact.OC_ContactName = "Name 1";
			contact.OC_WebAccessEnabled = true;
			contact.OC_Email = "contact@email.com";
			contact.SetHashedPassword("thepassword");
			var ediCustomerUserAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			ediCustomerUserAccount.EUA_OC_WebAccessContact = contact.PK;
			ediCustomerUserAccount.EUA_Email = "email1@wisetech.com";
			ediCustomerUserAccount.Database.GetOrCreateTrustedSystem();

			var passwordLogContact1 = Factory.New<StmALog>();
			using (passwordLogContact1.LockForUpdatingKeyFieldsForTesting())
			{
				passwordLogContact1.SL_Table = OrgContactSchema.Constants.TableName;
				passwordLogContact1.SL_Parent = contact.PK;
				passwordLogContact1.SL_SE_NKEvent = AutoEvents.WebAccessPasswordChangedCode;
				passwordLogContact1.SL_EventTime = ZDate.Today.AddDays(-1);
			}
			Factory.Save();

			var wizard = new SystemUserAccountsWizard(Factory);
			using (var form = new SystemUserAccountsWizardFormForTest(wizard))
			{
				form.Show();

				form.CustomerUserAccountWizardFilterControl_Exposed.FirePerformSearch();
				form.CustomerUserAccountWizardFilterControl_Exposed.FilteredGrid.CurrentRowIndex = 0;

				var current = form.CustomerUserAccountWizardFilterControl_Exposed.Grid.ListManager.GetCurrent() as EdiCustomerUserAccount;
				AssertEquals(contact.PK, current.EUA_OC_WebAccessContact);

				var licenceDatabase = current.Database;

				AssertEquals(licenceDatabase.EnterpriseID, form.EnterpriseIdValueLabel_Exposed.Text);
				AssertEquals(licenceDatabase.EnterpriseCode, form.EnterpriseCodeValueLabel_Exposed.Text);
				AssertEquals(licenceDatabase.ProductCodeDescription, form.ProductValueLabel_Exposed.Text);
				AssertEquals(licenceDatabase.LD_ServerCode, form.ServerCodeValueLabel_Exposed.Text);
				AssertEquals(licenceDatabase.LD_DatabaseNumber.ToString(), form.DatabaseNumberValueLabel_Exposed.Text);
				AssertEquals(licenceDatabase.LD_TenantID, form.TenantIDValueLabel_Exposed.Text);
				AssertEquals(current.EUA_UserID, form.UserIDValueLabel_Exposed.Text);
				AssertEquals(current.EDIWebAccessContact.Name, form.UserNameValueLabel_Exposed.Text);
				AssertEquals("email1@wisetech.com", form.UserEmailValueLabel_Exposed.Text);
				AssertEquals("contact@email.com", form.ContactEmailValueLabel_Exposed.Text);
				AssertEquals(current.EUA_ContactRelationshipStatus, form.UserStatusValueLabel_Exposed.Text);
				AssertEquals(licenceDatabase.TrustedSystem.ETS_SystemID, form.SystemIdValueLabel_Exposed.Text);
				AssertEquals(true, form.WebAccessEnabledCheckBox_Exposed.Checked);
				AssertEquals(true, form.PasswordSetCheckBox_Exposed.Checked);
				AssertEquals(current.EDIWebAccessContact.GetPasswordChangedOrSentLog().PostedLocalBranchTime.ToLongTimeString(),
								form.InstructionsLastSentValueLabel_Exposed.Text);
			}
		}

		public void TestSendPasswordInstructionsButton()
		{
			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			contact.OC_ContactName = "Name 1";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("thepassword");
			var ediCustomerUserAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			ediCustomerUserAccount.EUA_OC_WebAccessContact = contact.PK;
			ediCustomerUserAccount.EUA_Email = "email1@wisetech.com";
			ediCustomerUserAccount.Database.GetOrCreateTrustedSystem();
			Factory.Save();

			var wizard = new SystemUserAccountsWizard(Factory);
			using (var form = new SystemUserAccountsWizardFormForTest(wizard))
			{
				form.Show();

				form.CustomerUserAccountWizardFilterControl_Exposed.FirePerformSearch();
				form.CustomerUserAccountWizardFilterControl_Exposed.FilteredGrid.CurrentRowIndex = 0;

				var current = form.CustomerUserAccountWizardFilterControl_Exposed.Grid.ListManager.GetCurrent() as EdiCustomerUserAccount;
				AssertEquals(contact.PK, current.EUA_OC_WebAccessContact);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.SendPasswordInstructionsButton_Exposed.PerformClick();

				AssertEquals("Msg shown about password being successfully sent", "An email was sent to this contact containing the password Instruction and URL.",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestGridSelectionChange_ClearFormData()
		{
			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			contact.OC_ContactName = "Name 1";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("thepassword");
			var ediCustomerUserAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			ediCustomerUserAccount.EUA_OC_WebAccessContact = contact.PK;
			ediCustomerUserAccount.EUA_Email = "email1@wisetech.com";
			ediCustomerUserAccount.Database.GetOrCreateTrustedSystem();

			Factory.Save();

			var wizard = new SystemUserAccountsWizard(Factory);
			using (var form = new SystemUserAccountsWizardFormForTest(wizard))
			{
				form.Show();

				EDISecurityCheckpoints.SystemUserAccountsEdit.IsAllowed = true;
				AssertEquals(false, form.WebAccessContactGuidFindBox_Exposed.Enabled);

				form.CustomerUserAccountWizardFilterControl_Exposed.FirePerformSearch();
				form.CustomerUserAccountWizardFilterControl_Exposed.FilteredGrid.CurrentRowIndex = 0;

				var current = form.CustomerUserAccountWizardFilterControl_Exposed.Grid.ListManager.GetCurrent() as EdiCustomerUserAccount;
				AssertEquals(contact.PK, current.EUA_OC_WebAccessContact);

				var licenceDatabase = current.Database;

				AssertEquals(licenceDatabase.EnterpriseID, form.EnterpriseIdValueLabel_Exposed.Text);
				AssertEquals(licenceDatabase.EnterpriseCode, form.EnterpriseCodeValueLabel_Exposed.Text);
				AssertEquals(licenceDatabase.ProductCodeDescription, form.ProductValueLabel_Exposed.Text);
				AssertEquals(licenceDatabase.LD_ServerCode, form.ServerCodeValueLabel_Exposed.Text);
				AssertEquals(licenceDatabase.LD_DatabaseNumber.ToString(), form.DatabaseNumberValueLabel_Exposed.Text);
				AssertEquals(licenceDatabase.LD_TenantID, form.TenantIDValueLabel_Exposed.Text);
				AssertEquals(current.EUA_UserID, form.UserIDValueLabel_Exposed.Text);
				AssertEquals(current.EDIWebAccessContact.Name, form.UserNameValueLabel_Exposed.Text);
				AssertEquals(current.EUA_Email, form.UserEmailValueLabel_Exposed.Text);
				AssertEquals(current.WebAccessContact.OC_Email, form.ContactEmailValueLabel_Exposed.Text);
				AssertEquals(current.EUA_ContactRelationshipStatus, form.UserStatusValueLabel_Exposed.Text);
				AssertEquals(contact.OC_ContactName.ToUpper(), form.WebAccessContactGuidFindBox_Exposed.CodeBox.Text);
				AssertEquals(true, form.WebAccessContactGuidFindBox_Exposed.Enabled);

				ediCustomerUserAccount.Delete();
				Factory.Save();

				form.CustomerUserAccountWizardFilterControl_Exposed.FirePerformSearch();

				AssertEquals(string.Empty, form.EnterpriseIdValueLabel_Exposed.Text);
				AssertEquals(string.Empty, form.EnterpriseCodeValueLabel_Exposed.Text);
				AssertEquals(string.Empty, form.ProductValueLabel_Exposed.Text);
				AssertEquals(string.Empty, form.ServerCodeValueLabel_Exposed.Text);
				AssertEquals(string.Empty, form.DatabaseNumberValueLabel_Exposed.Text);
				AssertEquals(string.Empty, form.TenantIDValueLabel_Exposed.Text);
				AssertEquals(string.Empty, form.UserIDValueLabel_Exposed.Text);
				AssertEquals(string.Empty, form.UserNameValueLabel_Exposed.Text);
				AssertEquals(string.Empty, form.UserEmailValueLabel_Exposed.Text);
				AssertEquals(string.Empty, form.UserStatusValueLabel_Exposed.Text);
				AssertEquals(string.Empty, form.WebAccessContactGuidFindBox_Exposed.CodeBox.Text);
				AssertEquals(string.Empty, form.ContactEmailValueLabel_Exposed.Text);
				AssertEquals(false, form.WebAccessContactGuidFindBox_Exposed.Enabled);
			}
		}

		public void TestButtonsUserSecurityCheck()
		{
			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			contact.OC_ContactName = "Name 1";
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("thepassword");
			var ediCustomerUserAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			ediCustomerUserAccount.EUA_OC_WebAccessContact = contact.PK;
			ediCustomerUserAccount.EUA_Email = "email1@wisetech.com";
			ediCustomerUserAccount.Database.GetOrCreateTrustedSystem();
			Factory.Save();

			EDISecurityCheckpoints.SystemUserAccountsEdit.IsAllowed = false;
			using (var form = new SystemUserAccountsWizardFormForTest(new SystemUserAccountsWizard(Factory)))
			{
				form.Show();

				form.CustomerUserAccountWizardFilterControl_Exposed.FirePerformSearch();
				form.CustomerUserAccountWizardFilterControl_Exposed.FilteredGrid.CurrentRowIndex = 0;

				var current = form.CustomerUserAccountWizardFilterControl_Exposed.Grid.ListManager.GetCurrent() as EdiCustomerUserAccount;
				current.EUA_OC_WebAccessContact = Factory.NewWithValidTestData<EDIOrgContact>().PK;

				AssertEquals(false, form.SaveStripButton_Exposed.Enabled);
				AssertEquals(false, form.SaveCloseStripButton_Exposed.Enabled);
				AssertEquals(false, form.CancelStripButton_Exposed.Enabled);
				AssertEquals(false, form.WebAccessContactGuidFindBox_Exposed.Enabled);
				AssertEquals(false, form.SendPasswordInstructionsButton_Exposed.Enabled);
			}
		}

		public void TestPerformSearchShouldReloadGridCollectionFromDatabase()
		{
			var contact = Factory.NewWithValidTestData<EDIOrgContact>();

			var ediCustomerUserAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			ediCustomerUserAccount.EUA_OC_WebAccessContact = contact.PK;
			ediCustomerUserAccount.EUA_FullName = "Lam Othwell";
			Factory.Save();

			var wizard = new SystemUserAccountsWizard(Factory);
			using (var form = new SystemUserAccountsWizardFormForTest(wizard))
			{
				form.Show();

				EDISecurityCheckpoints.SystemUserAccountsEdit.IsAllowed = true;
				AssertEquals(false, form.WebAccessContactGuidFindBox_Exposed.Enabled);

				form.CustomerUserAccountWizardFilterControl_Exposed.FirePerformSearch();
				form.CustomerUserAccountWizardFilterControl_Exposed.FilteredGrid.CurrentRowIndex = 0;
				var userLinkControl = form.userAccountLinkedToContactUserControl_Exposed as UserAccountLinkedToContactUserControlForTest;
				var result = userLinkControl.FilteredGrid_Exposed.ListManager.GetCurrent() as EdiCustomerUserAccount;

				AssertEquals("Lam Othwell", result.EUA_FullName);

				ediCustomerUserAccount.EUA_FullName = "Jacques Leng";
				form.CustomerUserAccountWizardFilterControl_Exposed.FirePerformSearch();
				form.CustomerUserAccountWizardFilterControl_Exposed.FilteredGrid.CurrentRowIndex = 0;
				userLinkControl = form.userAccountLinkedToContactUserControl_Exposed as UserAccountLinkedToContactUserControlForTest;
				result = userLinkControl.FilteredGrid_Exposed.ListManager.GetCurrent() as EdiCustomerUserAccount;

				AssertEquals("Jacques Leng", result.EUA_FullName);
			}
		}

		public void TestSaveButton_WhenChangesMadeInTheGridRow()
		{
			var contact = Factory.NewWithValidTestData<EDIOrgContact>();
			var contact2 = Factory.NewWithValidTestData<EDIOrgContact>();

			var ediCustomerUserAccount = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			ediCustomerUserAccount.EUA_OC_WebAccessContact = contact.PK;
			ediCustomerUserAccount.EUA_FullName = "Lam Othwell";
			Factory.Save();

			var wizard = new SystemUserAccountsWizard(Factory);
			using (var form = new SystemUserAccountsWizardFormForTest(wizard))
			{
				form.Show();

				form.CustomerUserAccountWizardFilterControl_Exposed.FirePerformSearch();
				form.CustomerUserAccountWizardFilterControl_Exposed.FilteredGrid.CurrentRowIndex = 0;

				var current = form.CustomerUserAccountWizardFilterControl_Exposed.Grid.ListManager.GetCurrent() as EdiCustomerUserAccount;
				AssertEquals(contact.PK, current.EUA_OC_WebAccessContact);

				current.EUA_OC_WebAccessContact = contact2.PK;

				form.SaveStripButton_Exposed.PerformClick();

				var savedAccount = wizard.EdiCustomerUserAccountCollection.Cast<EdiCustomerUserAccount>().FirstOrDefault(a => a.PK == current.PK);
				AssertNotNull(savedAccount);
				AssertEquals(contact2.PK, savedAccount.EUA_OC_WebAccessContact);
			}
		}

		#region Implementation

		public override void TestBashingForm()
		{
			Assert(true); //custom form
		}

		public override void TestMinimumSizeNotTooBig()
		{
			Assert(true); //custom form
		}

		protected override Form GetFormToBashCore()
		{
			var wizard = new SystemUserAccountsWizard(Factory);
			return new SystemUserAccountsWizardForm(wizard);
		}

		class SystemUserAccountsWizardFormForTest : SystemUserAccountsWizardForm
		{
			public SystemUserAccountsWizardFormForTest(SystemUserAccountsWizard systemUserAccountsWizard)
				: base(systemUserAccountsWizard)
			{
			}

			public override UserAccountLinkedToContactUserControl GetUserAccountLinkedToContactUserControl(EdiCustomerUserAccountCollection ediCustomerUserAccountCollection)
			{
				return new UserAccountLinkedToContactUserControlForTest(ediCustomerUserAccountCollection);
			}

			public CustomerUserAccountWizardFilterControl CustomerUserAccountWizardFilterControl_Exposed
						=> customerUserAccountWizardFilterControl;

			public UserAccountLinkedToContactUserControl userAccountLinkedToContactUserControl_Exposed
						=> userAccountLinkedToContactUserControl;

			public ZLabel EnterpriseIdValueLabel_Exposed => EnterpriseIdValueLabel;
			public ZLabel EnterpriseCodeValueLabel_Exposed => EnterpriseCodeValueLabel;
			public ZLabel ProductValueLabel_Exposed => ProductValueLabel;
			public ZLabel ServerCodeValueLabel_Exposed => ServerCodeValueLabel;
			public ZLabel DatabaseNumberValueLabel_Exposed => DatabaseNumberValueLabel;
			public ZLabel SystemIdValueLabel_Exposed => SystemIdValueLabel;
			public ZLabel TenantIDValueLabel_Exposed => TenantIDValueLabel;
			public ZLabel UserIDValueLabel_Exposed => UserIDValueLabel;
			public ZLabel UserNameValueLabel_Exposed => UserNameValueLabel;
			public ZLabel UserEmailValueLabel_Exposed => UserEmailValueLabel;
			public ZLabel ContactEmailValueLabel_Exposed => ContactEmailValueLabel;
			public ZLabel UserStatusValueLabel_Exposed => UserStatusValueLabel;
			public ZCheckBox WebAccessEnabledCheckBox_Exposed => WebAccessEnabledCheckBox;
			public ZCheckBox PasswordSetCheckBox_Exposed => PasswordSetCheckBox;
			public ZButton SendPasswordInstructionsButton_Exposed => SendPasswordInstructionsButton;
			public ZLabel InstructionsLastSentValueLabel_Exposed => InstructionsLastSentValueLabel;
			public ZGuidFindBox WebAccessContactGuidFindBox_Exposed => WebAccessContactGuidFindBox;
			public ZToolStripButton SaveStripButton_Exposed => SaveStripButton;
			public ZToolStripButton SaveCloseStripButton_Exposed => SaveCloseStripButton;
			public ZToolStripButton CancelStripButton_Exposed => CancelStripButton;
		}

		class UserAccountLinkedToContactUserControlForTest : UserAccountLinkedToContactUserControl
		{
			public UserAccountLinkedToContactUserControlForTest(EdiCustomerUserAccountCollection ediCustomerUserAccountCollection)
				: base(ediCustomerUserAccountCollection)
			{
			}

			public ZGrid FilteredGrid_Exposed => UserAccountGrid;
		}

		#endregion
	}
}
