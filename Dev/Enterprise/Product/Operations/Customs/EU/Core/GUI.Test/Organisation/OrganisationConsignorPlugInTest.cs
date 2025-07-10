using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class OrganisationConsignorPlugInTest : ZPlugInGenericTest
	{
		public void TestUseFR3FiscalRepresentationCheckBox()
		{
			using (var plugIn = new OrganisationConsignorPlugIn(organisation))
			{
				var controlCollection = plugIn.UserControl.Controls.Find("zCheckBox14UseIndirectRepresentationForExporter", true);
				AssertEquals("A control named zCheckBox14UseIndirectRepresentationForExporter should exist.", 1, controlCollection.Length);
			}
		}

		public void TestName()
		{
			using (var plugIn = new OrganisationConsignorPlugIn(organisation))
			{
				AssertEquals("Customs Defaults", plugIn.Name);
			}
		}

		public void TestType()
		{
			using (var plugIn = new OrganisationConsignorPlugIn(organisation))
			{
				AssertType<OrganisationConsignorPlugInUserControl>(plugIn.UserControl);
			}
		}

		protected override ZPlugIn GetPlugInToTest() => new OrganisationConsignorPlugIn(organisation);

		protected override void SetUp()
		{
			base.SetUp();
			organisation = Factory.NewWithValidTestData<OrgHeader>();

			countryData = Factory.New<OrgCountryData>();
			countryData.OV_OH_OrgHeader = organisation.PK;
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Latvia;
		}
		OrgHeader organisation;
		OrgCountryData countryData;
	}
}
