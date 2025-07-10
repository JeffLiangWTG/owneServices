using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class OrganisationConsigneePlugInTest : ZPlugInGenericTest
	{
		public void TestUseFR3FiscalRepresentationCheckBox()
		{
			using (var plugIn = new OrganisationConsigneePlugIn(organisation))
			{
				var controlCollection = plugIn.UserControl.Controls.Find("zCheckBoxUseFR3FiscalRepresentation", true);
				AssertEquals("A control named zCheckBoxUseFR3FiscalRepresentation should exist.", 1, controlCollection.Length);
			}
		}

		public void TestName()
		{
			using (var plugIn = new OrganisationConsigneePlugIn(organisation))
			{
				AssertEquals("EU Deferral", plugIn.Name);
			}
		}

		public void TestType()
		{
			using (var plugIn = new OrganisationConsigneePlugIn(organisation))
			{
				AssertType<OrganisationConsigneePlugInUserControl>(plugIn.UserControl);
			}
		}

		public void TestEUOrgAddInfo()
		{
			using (var plugIn = new OrganisationConsigneePlugIn(organisation))
			{
				var addinfo = (EUOrgImpAddInfo)plugIn.BusinessEntity;
				addinfo.ZO_OtherDeferType = "Y";
				Factory.Save();
				AssertEquals(false, countryData.OV_CustomsEconomicGroupAddInfo.IsEmpty);
			}
		}

		protected override ZPlugIn GetPlugInToTest() => new OrganisationConsigneePlugIn(organisation);

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
