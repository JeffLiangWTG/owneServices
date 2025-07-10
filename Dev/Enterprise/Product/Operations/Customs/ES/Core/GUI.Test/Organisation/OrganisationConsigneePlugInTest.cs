using Enterprise.Customs.ES.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.Customs.ES.GUI.Testing
{
	public class OrganisationConsigneePlugInTest : ZPlugInGenericTest
	{
		public void TestPlugInMembers()
		{
			var countryData = Factory.New<OrgCountryData>();
			countryData.OV_OH_OrgHeader = organisation.PK;
			countryData.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Spain;
			countryData.OV_OA_ApprovedLocation = organisation.MainAddress.PK;

			var countryData1 = Factory.New<OrgCountryData>();
			countryData1.OV_OH_OrgHeader = organisation.PK;
			countryData1.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.Spain;

			Factory.Save();

			using (var plugIn = new OrganisationConsigneePlugIn(organisation))
			{
				AssertEquals("ES Deferral", plugIn.Name);
				AssertEquals(typeof(OrganisationConsigneePlugInUserControl), plugIn.UserControl.GetType());

				AssertNotNull(plugIn.BusinessEntity);

				var addinfo = (ESOrgImpAddInfo)plugIn.BusinessEntity;
				addinfo.ZO_VATDeferment = true;
				addinfo.ZO_Retailer = true;
				addinfo.ZO_MethodOfPayment = "A";
				addinfo.ZO_MethodOfPaymentCan = "S";

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
