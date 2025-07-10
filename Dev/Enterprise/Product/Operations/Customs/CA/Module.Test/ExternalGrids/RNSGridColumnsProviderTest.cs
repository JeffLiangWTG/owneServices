using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class RNSGridColumnsProviderTest : CAShipmentGridColumnsProviderTest
	{
		public void TestAddColumns()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var parentConsol = Factory.NewWithValidTestData<CFSLoadListConsol>();

				using (var form = new MockConsolForm(parentConsol))
				{
					form.Show();

					var rNSGroupName = new ResourceStringData("", "Release Notifications (RNS)");
					AssertColumn(form.Grid, "RNSReleaseStatus", rNSGroupName, "RNS Release Status", false);
					AssertColumn(form.Grid, "RNSReleaseDate", rNSGroupName, "RNS Release Date", false);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var parentConsol = Factory.NewWithValidTestData<CFSLoadListConsol>();

				using (var form = new MockConsolForm(parentConsol))
				{
					form.Show();

					AssertNull("Column RNSReleaseStatus should not be added", form.Grid.GetColumnStyle("RNSReleaseStatus"));
					AssertNull("Column RNSReleaseDate should not be added", form.Grid.GetColumnStyle("RNSReleaseDate"));
				}
			}
		}

		public void TestLoadUserLayoutSettings()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var parentConsol = Factory.NewWithValidTestData<CFSLoadListConsol>();

				using (var form = new MockConsolForm(parentConsol))
				{
					form.Show();

					var rNSGroupName = new ResourceStringData("", "Release Notifications (RNS)");
					AssertColumn(form.Grid, "RNSReleaseStatus", rNSGroupName, "RNS Release Status", false);
					AssertColumn(form.Grid, "RNSReleaseDate", rNSGroupName, "RNS Release Date", false);

					form.Grid.SetAllColumnsVisible(true);
					form.Grid.Columns.HasLayoutChanged = true;
					form.Grid.SaveUserLayoutSettings();
				}

				using (var form = new MockConsolForm(parentConsol))
				{
					form.Show();

					var rNSGroupName = new ResourceStringData("", "Release Notifications (RNS)");
					AssertColumn(form.Grid, "RNSReleaseStatus", rNSGroupName, "RNS Release Status", true);
					AssertColumn(form.Grid, "RNSReleaseDate", rNSGroupName, "RNS Release Date", true);
				}
			}
		}
	}
}
