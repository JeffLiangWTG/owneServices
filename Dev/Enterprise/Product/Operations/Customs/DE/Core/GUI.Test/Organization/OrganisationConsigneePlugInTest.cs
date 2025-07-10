using Enterprise.Customs.DE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.Customs.DE.GUI.PlugIn.Testing
{
	public class OrganisationConsigneePlugInTest : ZPlugInGenericTest
	{
		public void TestPlugInMembers()
		{
			var countryData = Factory.New<OrgCountryData>();
			countryData.OV_OH_OrgHeader = organisation.PK;
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Germany;
			countryData.OV_OA_ApprovedLocation = organisation.MainAddress.PK;

			var countryData1 = Factory.New<OrgCountryData>();
			countryData1.OV_OH_OrgHeader = organisation.PK;
			countryData1.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Germany;

			Factory.Save();

			using (var plugIn = new OrganisationConsigneePlugIn(organisation))
			{
				AssertEquals("DE Deferral", plugIn.Name);
				AssertEquals(typeof(OrganisationConsigneePlugInUserControl), plugIn.UserControl.GetType());

				AssertNotNull(plugIn.BusinessEntity);

				var addinfo = (DEOrgImpAddInfo)plugIn.BusinessEntity;
				addinfo.ZO_VATClaimBack = "Y";

				Factory.Save();

				AssertEquals(false, countryData1.OV_ImportCustomsDefaultAddInfo.IsEmpty);
				AssertEquals(true, countryData.OV_ImportCustomsDefaultAddInfo.IsEmpty);
			}
		}

		protected override ZPlugIn GetPlugInToTest() => new OrganisationConsigneePlugIn(organisation);

		protected override void SetUp()
		{
			base.SetUp();
			organisation = Factory.NewWithValidTestData<OrgHeader>();
		}
		OrgHeader organisation;
	}
}
