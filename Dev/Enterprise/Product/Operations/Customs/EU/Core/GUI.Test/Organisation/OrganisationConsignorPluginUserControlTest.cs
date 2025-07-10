using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class OrganisationConsignorPluginUserControlTest : TestCaseWithFactory
	{
		public void TestUserControl()
		{
			using (var form = new ZForm())
			using (var testUserControl = new OrganisationConsignorPlugInUserControl())
			{
				form.Controls.Add(testUserControl);
				form.Show();

				var zCheckBox14UseIndirectRepresentationForExporter = (ZCheckBox)testUserControl.Controls.Find("zCheckBox14UseIndirectRepresentationForExporter", true)[0];
				AssertEquals("A zCheckBox14UseIndirectRepresentationForExporter field should show", true, zCheckBox14UseIndirectRepresentationForExporter.Visible);
			}
		}

		public void TestBindingSource()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				using (var form = new ZForm())
				using (var testUserControl = new OrganisationConsignorPlugInUserControl())
				{
					form.Controls.Add(testUserControl);
					form.Show();

					AssertEquals("Enterprise.Customs.FR.Business.MasterFiles.RegionOrgImpAddInfo", testUserControl.BindingSource.DataSourceType.FullName);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Netherlands))
			{
				using (var form = new ZForm())
				using (var testUserControl = new OrganisationConsignorPlugInUserControl())
				{
					form.Controls.Add(testUserControl);
					form.Show();

					AssertEquals("Enterprise.Customs.NL.Business.RegionOrgImpAddInfo", testUserControl.BindingSource.DataSourceType.FullName);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				using (var form = new ZForm())
				using (var testUserControl = new OrganisationConsignorPlugInUserControl())
				{
					form.Controls.Add(testUserControl);
					form.Show();

					AssertEquals("Enterprise.Customs.EU.Business.EUOrgImpAddInfo", testUserControl.BindingSource.DataSourceType.FullName);
				}
			}
		}
	}
}
