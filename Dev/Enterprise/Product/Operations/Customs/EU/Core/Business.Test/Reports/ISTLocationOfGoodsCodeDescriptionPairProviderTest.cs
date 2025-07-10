using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Reports.Testing
{
	class ISTLocationOfGoodsCodeDescriptionPairProviderTest : TestCaseWithFactory
	{
		public void TestGetCodeDescriptionPairList()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "Org001";
				var addressA = org.Addresses.AddNew();
				addressA.Address1 = "AddressA1";
				var addressB = org.Addresses.AddNew();
				addressB.Address1 = "AddressB1";
				var authorisationHeaderProvider = (CusAuthorisationHeaderProvider)Customs.Business.CusAuthorisationHeaderProvider.GetByCountryCode(GlbCompany.CurrentCompany.Country.Code);
				addressA.SetupAuthorisationHeader(authorisationHeaderProvider.AuthorizedLocationCode).WithNumber("Paris");
				addressA.SetupAuthorisationHeader(authorisationHeaderProvider.AuthorizedLocationCode).WithNumber("Toulouse");
				addressA.SetupAuthorisationHeader(CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP).WithNumber("Marseille");
				addressB.SetupAuthorisationHeader(authorisationHeaderProvider.AuthorizedLocationCode).WithNumber("Lyon");
				Factory.Save();
				CombineAssertions(() =>
				{
					var provider = new ISTLocationOfGoodsCodeDescriptionPairProvider();
					var list = provider.GetCodeDescriptionPairList();
					AssertEquals(3, list.Count);
					list = provider.GetDependenceCodeDescriptionPairList(addressA.PK.ToString());
					AssertEquals(2, list.Count);
					AssertContainsExactElementsInAnyOrder(new string[] { "Paris", "Toulouse" }, list.GetAllCodes());
					list = provider.GetDependenceCodeDescriptionPairList(addressB.PK.ToString());
					AssertEquals(1, list.Count);
				});
			}
		}
	}
}
