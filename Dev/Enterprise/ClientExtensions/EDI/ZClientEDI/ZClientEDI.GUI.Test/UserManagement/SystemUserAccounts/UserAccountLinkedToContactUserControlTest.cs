using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.UserManagement.GUI.Testing
{
	public class UserAccountLinkedToContactUserControlTest : TestCaseWithFactory
	{
		public void TestFilteredGridColumns()
		{
			Factory.NewWithValidTestData<EdiCustomerUserAccount>();
			Factory.NewWithValidTestData<EdiCustomerUserAccount>();

			var wizard = new SystemUserAccountsWizard(Factory);

			using (var form = new ZForm())
			using (var filterControl = new UserAccountLinkedToContactUserControlForTest(wizard.EdiCustomerUserAccountCollection))
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
				AssertCollectionContains("ContactOrganisation+OH_Code", columnNames);
				AssertCollectionContains("ContactOrganisation+OH_FullName", columnNames);
			}
		}

		class UserAccountLinkedToContactUserControlForTest : UserAccountLinkedToContactUserControl
		{
			public UserAccountLinkedToContactUserControlForTest(EdiCustomerUserAccountCollection ediCustomerUserAccountCollection)
				: base(ediCustomerUserAccountCollection)
			{
			}

			public ZGrid FilteredGrid_Exposed => UserAccountGrid;
		}
	}
}
