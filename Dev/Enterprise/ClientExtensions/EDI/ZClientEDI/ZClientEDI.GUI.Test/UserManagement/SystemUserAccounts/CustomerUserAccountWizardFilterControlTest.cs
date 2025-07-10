using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using ZClientEDI.GUI.UserManagement;

namespace Enterprise.Client.EDI.UserManagement.GUI.Testing
{
	public class CustomerUserAccountWizardFilterControlTest : TestCaseWithFactory
	{
		public void TestFilteredGridColumns()
		{
			Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			Factory.NewWithValidTestData<EdiCustomerUserAccount>();

			var wizard = new SystemUserAccountsWizard(Factory);

			using (var form = new ZForm())
			using (var filterControl = new CustomerUserAccountWizardFilterControlForTest(wizard.EdiCustomerUserAccountCollection, new CustomerUserAccountWizardFilterBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();

				var columnNames = filterControl.FilteredGrid_Exposed.Columns.Select(x => x.ColumnName);
				AssertCollectionContains(EdiCustomerUserAccount.Schema.EUA_UserID, columnNames);
				AssertCollectionContains(EdiCustomerUserAccount.Schema.EUA_IsActive, columnNames);
				AssertCollectionContains(EdiCustomerUserAccount.Schema.EUA_FullName, columnNames);
				AssertCollectionContains(EdiCustomerUserAccount.Schema.EUA_Email, columnNames);
				AssertCollectionContains(EdiCustomerUserAccount.Schema.EUA_IsEmailVerificationRequired, columnNames);
				AssertCollectionContains(EdiCustomerUserAccount.Schema.EUA_ContactRelationshipStatus, columnNames);
				AssertCollectionContains("Database+LD_Product", columnNames);
				AssertCollectionContains("Database+LD_ServerCode", columnNames);
				AssertCollectionContains("Database+LD_TenantID", columnNames);
				AssertCollectionContains("AccountVerificationStatus", columnNames);
				AssertCollectionContains("DatabaseLicenceEnterprise+LE_EnterpriseID", columnNames);
				AssertCollectionContains("DatabaseLicenceEnterprise+LE_EnterpriseCode", columnNames);
				AssertCollectionContains("EDIWebAccessContact+OrganisationCode", columnNames);
				AssertCollectionContains("EDIWebAccessContact+WorkingAddressCompanyName", columnNames);
				AssertCollectionContains("EDIWebAccessContact+OC_Email", columnNames);
			}
		}

		public void TestRecordsFoundText()
		{
			Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			Factory.Save();

			var wizard = new SystemUserAccountsWizard(Factory);

			using (var form = new ZForm())
			using (var filterControl = new CustomerUserAccountWizardFilterControlForTest(wizard.EdiCustomerUserAccountCollection, new CustomerUserAccountWizardFilterBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();

				AssertEquals("", filterControl.ToolStripRecordsFoundLabel_Exposed.Text);

				filterControl.FirePerformSearch();
				AssertEquals("Found\r\n2 records", filterControl.ToolStripRecordsFoundLabel_Exposed.Text.Trim());
			}
		}

		public void TestContextMenu()
		{
			Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			Factory.NewWithValidTestData<EdiCustomerUserAccount>();

			var wizard = new SystemUserAccountsWizard(Factory);

			using (var form = new ZForm())
			using (var filterControl = new CustomerUserAccountWizardFilterControlForTest(wizard.EdiCustomerUserAccountCollection, new CustomerUserAccountWizardFilterBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();

				var menu = filterControl.FilteredGrid_Exposed.ContextMenu.MenuItems.FindByText("Amalgamate User ID's into same Contact/Person");
				menu.PerformClick();
				AssertEquals("Please select two rows", UnitTestUserNotification.Instance.LastMessage.Text);

				menu = filterControl.FilteredGrid_Exposed.ContextMenu.MenuItems.FindByText("Clear 'DER' (Distinct Email Required) Status");
				menu.PerformClick();
				AssertEquals("Please select at least one row", UnitTestUserNotification.Instance.LastMessage.Text);

				menu = filterControl.FilteredGrid_Exposed.ContextMenu.MenuItems.FindByText("Audit Data");
				menu.PerformClick();
				AssertEquals("Please select a row", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestAuditDataMenuItem()
		{
			Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			Factory.Save();

			var wizard = new SystemUserAccountsWizard(Factory);

			using (var form = new ZForm(wizard))
			using (var filterControl = new CustomerUserAccountWizardFilterControlForTest(wizard.EdiCustomerUserAccountCollection, new CustomerUserAccountWizardFilterBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();

				filterControl.FirePerformSearch();

				filterControl.FilteredGrid_Exposed.Select(0);

				var menu = filterControl.FilteredGrid_Exposed.ContextMenu.MenuItems.FindByText("Audit Data");
				menu.PerformClick();
				AssertEquals(true, ZFormModaliser.LastFormShownDialogForTest is SystemUserAccountsAuditForm);
				ZFormModaliser.LastFormShownDialogForTest?.Dispose();
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestPerformSearchShouldReloadGridCollectionFromDatabase()
		{
			var user = Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			Factory.RefreshEnabled = false;

			user.EUA_FullName = "Nahte Kiwan";
			Factory.Save();

			var wizard = new SystemUserAccountsWizard(Factory);

			using (var form = new ZForm())
			using (var filterControl = new CustomerUserAccountWizardFilterControlForTest(wizard.EdiCustomerUserAccountCollection, new CustomerUserAccountWizardFilterBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();

				filterControl.FirePerformSearch();
				filterControl.FilteredGrid_Exposed.CurrentRowIndex = 0;
				Assert(filterControl.GetToolStripRecordsFoundLabelText().Contains("1 record"));
				var result = filterControl.Grid.ListManager.GetCurrent() as EdiCustomerUserAccount;

				AssertEquals("Nahte Kiwan", result.EUA_FullName);

				user.EUA_FullName = "John Lexington";
				Factory.Save();

				filterControl.FirePerformSearch();
				filterControl.FilteredGrid_Exposed.CurrentRowIndex = 0;
				Assert(filterControl.GetToolStripRecordsFoundLabelText().Contains("1 record"));
				result = filterControl.Grid.ListManager.GetCurrent() as EdiCustomerUserAccount;
				AssertEquals("John Lexington", result.EUA_FullName);
			}
		}

		class CustomerUserAccountWizardFilterControlForTest : CustomerUserAccountWizardFilterControl
		{
			public CustomerUserAccountWizardFilterControlForTest(EdiCustomerUserAccountCollection ediCustomerUserAccountCollection, FilterStripBusinessObject filterBusinessObject)
				: base(ediCustomerUserAccountCollection, filterBusinessObject)
			{
			}

			public ZDisplayGrid FilteredGrid_Exposed => FilteredGrid;
			public ZLabel ToolStripRecordsFoundLabel_Exposed => ToolStripRecordsFoundLabel;
		}
	}
}
