using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using ZClientEDI.GUI.Licencing;

namespace Enterprise.Client.EDI.Licencing.GUI.Testing
{
	public class OrganisationWizardFilterControlTest : TestCaseWithFactory
	{
		public void TestFilteredGridColumns()
		{
			Factory.NewWithValidTestData<EDIOrgHeader>();
			Factory.NewWithValidTestData<EDIOrgHeader>();

			var wizard = new LicenceDatabaseRegistrationWizard(Factory);

			using (var form = new ZForm())
			using (var filterControl = new OrganisationWizardFilterControlForTest(wizard, new OrganisationWizardFilterBusinessObject()))
			{
				form.Controls.Add(filterControl);
				form.Show();

				var columnNames = filterControl.FilteredGrid_Exposed.Columns.Select(x => x.ColumnName);
				AssertCollectionContains(OrgHeaderSchema.OH_Code.Name, columnNames);
				AssertCollectionContains(OrgHeaderSchema.OH_FullName.Name, columnNames);
				AssertCollectionContains(OrgHeaderSchema.OH_Category.Name, columnNames);
				AssertCollectionContains(OrgHeaderSchema.OH_IsActive.Name, columnNames);
				AssertCollectionContains("LicenceEnterpriseID", columnNames);
				AssertCollectionContains("LicenceEnterpriseCode", columnNames);
				AssertCollectionContains("CityName", columnNames);
				AssertCollectionContains("LicenceDatabaseCount", columnNames);
			}
		}

		public void TestGridShowsRecords()
		{
			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_FullName = "TestWI001";
			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.OH_FullName = "TestWI002";
			Factory.Save();

			var filterBizo = new OrganisationWizardFilterBusinessObject();
			var fullNameFilter = (ModuleTextFilter)filterBizo["Name"];
			fullNameFilter.Property = "TestWI00";
			fullNameFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			fullNameFilter.IsActive = true;

			var wizard = new LicenceDatabaseRegistrationWizard(Factory);
			using (var form = new ZForm())
			using (var filterControl = new OrganisationWizardFilterControlForTest(wizard, filterBizo))
			{
				form.Controls.Add(filterControl);
				form.Show();

				filterControl.FirePerformSearch();

				Assert(filterControl.GetToolStripRecordsFoundLabelText().Contains("2 records"));
			}
		}

		class OrganisationWizardFilterControlForTest : OrganisationWizardFilterControl
		{
			public OrganisationWizardFilterControlForTest(LicenceDatabaseRegistrationWizard wizard, FilterStripBusinessObject filterBusinessObject)
				: base(wizard, filterBusinessObject)
			{
			}

			public ZDisplayGrid FilteredGrid_Exposed => FilteredGrid;
		}
	}
}
