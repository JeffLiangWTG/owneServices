using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using ZClientEDI.GUI.Licencing;

namespace Enterprise.Client.EDI.Licencing.GUI.Testing
{
	public class LicenceDatabaseRegistrationWizardFilterControlTest : TestCaseWithFactory
	{
		public void TestFilteredGridColumns()
		{
			Factory.NewWithValidTestData<LicenceDatabase>();
			Factory.NewWithValidTestData<LicenceDatabase>();

			var wizard = new LicenceDatabaseRegistrationWizard(Factory);

			using (var form = new ZForm())
			using (var filterControl = new LicenceDatabaseRegistrationWizardFilterControlForTest(wizard.LicenceDatabaseCollection, new LicenceDatabaseWizardFilterBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();

				var columnNames = filterControl.FilteredGrid_Exposed.Columns.Select(x => x.ColumnName);
				AssertCollectionContains(LicenceDatabase.Schema.LD_Product, columnNames);
				AssertCollectionContains(LicenceDatabase.Schema.LD_LicenceType, columnNames);
				AssertCollectionContains(LicenceDatabase.Schema.LD_ServerCode, columnNames);
				AssertCollectionContains(LicenceDatabase.Schema.LD_TenantID, columnNames);
				AssertCollectionContains(LicenceDatabase.Schema.LD_DatabaseNumber, columnNames);
				AssertCollectionContains(LicenceDatabase.Schema.LD_SystemCreateTimeUtc, columnNames);
				AssertCollectionContains(LicenceDatabase.Schema.LD_SystemCreateUser, columnNames);
				AssertCollectionContains(LicenceDatabase.Schema.LD_SystemLastEditTimeUtc, columnNames);
				AssertCollectionContains(LicenceDatabase.Schema.LD_SystemLastEditUser, columnNames);
				AssertCollectionContains("WebAccessOrg+OH_Code", columnNames);
				AssertCollectionContains("EnterpriseIDWithoutDefault", columnNames);
			}
		}

		public void TestSetMasterOrgs()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			var db1 = Factory.NewWithValidTestData<LicenceDatabase>();
			var db2 = Factory.NewWithValidTestData<LicenceDatabase>();
			var suggestion1 = db1.OrgSuggestionCollections.AddNew();
			suggestion1.LDS_OH = org1.PK;
			suggestion1.LDS_TotalScore = 200;
			var suggestion2 = db1.OrgSuggestionCollections.AddNew();
			suggestion2.LDS_OH = org2.PK;
			suggestion2.LDS_TotalScore = 240;
			var suggestion3 = db1.OrgSuggestionCollections.AddNew();
			suggestion3.LDS_OH = org3.PK;
			suggestion3.LDS_TotalScore = 150;
			Factory.Save();

			var wizard = new LicenceDatabaseRegistrationWizard(Factory);

			using (var form = new ZForm())
			using (var filterControl = new LicenceDatabaseRegistrationWizardFilterControlForTest(wizard.LicenceDatabaseCollection, new LicenceDatabaseWizardFilterBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();

				filterControl.FirePerformSearch();
				filterControl.FilteredGrid_Exposed.SelectAllElements();

				var menuItem = filterControl.FilteredGrid_Exposed.ContextMenu.MenuItems.FindByText("Set Master Orgs", false);
				menuItem.PerformClick();
				AssertEquals("1 Database(s) successfully updated. 1 Database(s) skipped.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(db1.LD_OH_WebAccessOrg, org2.PK);
				AssertEquals(true, db2.LD_OH_WebAccessOrg.IsEmpty);
			}
		}

		public void TestGridShowsRecords()
		{
			Factory.NewWithValidTestData<LicenceDatabase>();
			Factory.NewWithValidTestData<LicenceDatabase>();
			Factory.Save();

			var wizard = new LicenceDatabaseRegistrationWizard(Factory);

			using (var form = new ZForm())
			using (var filterControl = new LicenceDatabaseRegistrationWizardFilterControlForTest(wizard.LicenceDatabaseCollection, new LicenceDatabaseWizardFilterBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();

				filterControl.FirePerformSearch();
				Assert(filterControl.GetToolStripRecordsFoundLabelText().Contains("2 records"));
			}
		}

		class LicenceDatabaseRegistrationWizardFilterControlForTest : LicenceDatabaseRegistrationWizardFilterControl
		{
			public LicenceDatabaseRegistrationWizardFilterControlForTest(LicenceDatabaseNonDependentCollection licenceDatabaseCollection, FilterStripBusinessObject filterBusinessObject)
				: base(licenceDatabaseCollection, filterBusinessObject)
			{
			}

			public ZDisplayGrid FilteredGrid_Exposed => FilteredGrid;
		}
	}
}
