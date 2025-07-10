using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.GUI.Testing
{
	class EdiCommissionAgreementCustomizationTreeControlTest : TestCaseWithFactory
	{
		#region Add Countries Button
		public void TestAddCountryMenuItems()
		{
			var licEnterprise = Factory.New<LicenceEnterprise>();
			var database1 = licEnterprise.Databases.AddNew();
			database1.LD_LE = licEnterprise.PK;
			database1.LD_ServerCode = "111";
			database1.LD_LicenceType = "PRD";
			var database2 = licEnterprise.Databases.AddNew();
			database2.LD_LE = licEnterprise.PK;
			database2.LD_ServerCode = "222";
			database2.LD_LicenceType = "TST";
			var company = licEnterprise.Companies.AddNew();
			company.LC_LE = licEnterprise.PK;
			var org = Factory.New<EDIOrgHeader>();
			company.LC_OH = org.PK;
			var agreement = Factory.New<EdiCommissionAgreement>();
			agreement.CA0_OH_Customer = org.PK;
			var customization = agreement.GetOrCreateCustomization();
			using (var form = new ZForm(customization))
			using (var treeControl = new EdiCommissionAgreementCustomizationTreeControl())
			{
				form.Controls.Add(treeControl);
				form.Show();
				var bottomToolStrip = (ZToolStrip)form.Controls.Find("bottomToolStrip", true).Single();
				var addCountriesDropDownButton = bottomToolStrip.Items.Cast<ZToolStripDropDownButton>().Single();
				var addCountriesDropDownItems = addCountriesDropDownButton.DropDownItems.Cast<ZToolStripMenuItem>();
				AssertArrayEqualsByElements(new[] { "New Databases", "Existing Databases", }, addCountriesDropDownItems.Select(x => x.Text).ToArray());
				var existingDatabasesItem = addCountriesDropDownItems.Single(x => x.Text == "Existing Databases");
				var existingDatabasesDropDownItems = existingDatabasesItem.DropDownItems.Cast<ToolStripItem>().ToArray();
				AssertEquals("All", existingDatabasesDropDownItems[0].Text);
				AssertType(typeof(ToolStripSeparator), existingDatabasesDropDownItems[1]);
				AssertEquals("111 (PRD)", existingDatabasesDropDownItems[2].Text);
				AssertEquals("222 (TST)", existingDatabasesDropDownItems[3].Text);
			}
		}

		#endregion
		#region ReadOnly
		public void TestReadOnly()
		{
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			using (var form = new ZForm(customization))
			using (var control = new EdiCommissionAgreementCustomizationTreeControl())
			{
				form.DisplayMode = ZArchitecture.Core.ODisplayMode.Browse;
				form.Show();
				var resetToDefaultButton = (ZButton)control.Controls.Find("resetToDefaultButton", true)[0];
				AssertEquals(true, resetToDefaultButton.Visible);
				control.ReadOnly = true;
				AssertEquals(false, resetToDefaultButton.Visible);
			}
		}
		#endregion
	}
}
