using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Licencing.GUI.Testing
{
	public class DatabaseDetailsWizardUserControlTest : TestCaseWithFactory
	{
		public void TestLicenceGridColumns()
		{
			var orgHeader = Factory.NewWithValidTestData<EDIOrgHeader>();
			orgHeader.CreateAndLoadLicenceForOrg();

			using (var form = new ZForm())
			using (var control = new DatabaseDetailsWizardUserControlForTest(orgHeader.LicCompany.LicDatabases))
			{
				form.Controls.Add(control);
				form.Show();

				var columnNames = control.LicenceGrid_Exposed.Columns.Select(x => x.ColumnName);
				AssertCollectionContains(LicenceDatabase.Schema.LD_Product, columnNames);
				AssertCollectionContains(LicenceDatabase.Schema.LD_LicenceType, columnNames);
				AssertCollectionContains(LicenceDatabase.Schema.LD_IsActive, columnNames);
				AssertCollectionContains(LicenceDatabase.Schema.LD_ServerCode, columnNames);
				AssertCollectionContains(LicenceDatabase.Schema.LD_DatabaseNumber, columnNames);
				AssertCollectionContains(LicenceDatabase.Schema.LD_TenantID, columnNames);
				AssertCollectionContains(LicenceDatabase.Schema.LD_HostedLocation, columnNames);
				AssertCollectionContains(LicenceDatabase.Schema.LD_ReleaseRing, columnNames);
				AssertCollectionContains("WebAccessOrg+OH_Code", columnNames);
				AssertCollectionContains("BillingModel", columnNames);
			}
		}

		class DatabaseDetailsWizardUserControlForTest : DatabaseDetailsWizardUserControl
		{
			public DatabaseDetailsWizardUserControlForTest(LicenceCompanyLicenceDatabaseCollection licenceDatabaseCollection)
				: base(licenceDatabaseCollection)
			{
			}

			public ZGrid LicenceGrid_Exposed => LicenceGrid;
		}
	}
}
