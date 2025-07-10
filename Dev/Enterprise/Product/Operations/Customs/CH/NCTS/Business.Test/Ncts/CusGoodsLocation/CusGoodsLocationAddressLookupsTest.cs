using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class CusGoodsLocationAddressLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestAuthorisationNumberList()
	{
		var location = Factory.New<CusGoodsLocation>();
		var locationAddress = location.Address;
		var lookups = locationAddress.Lookups;

		var orgHeaderPK = Factory.New<OrgHeader>().PK;
		locationAddress.IdentificationHolderPK = orgHeaderPK;
		locationAddress.E2_GovRegNumType = CH.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar;
		locationAddress.E2_GovRegNum = "123456";

		CombineAssertions(() =>
		{
			var numberList = (CusAuthorisationHeaderCollection)lookups.AuthorisationNumberList;
			AssertType<CusAuthorisationHeaderCollectionFiltered>(numberList);
			var sql = numberList.CompleteFilter.LiteralTextSqlFormatted;

			AssertContains("CPH_IsActive = 1", sql);
			AssertContains("CPH_ApplicationCode = 'AUT'", sql);
			AssertContains("CPH_RN_NKCountryCode = 'CH'", sql);
			AssertContains("CPH_Type = 'ALP'", sql);
			AssertContains($"CPH_OH_PermitHolder = '{orgHeaderPK}'", sql);

			var authorisationTypeFilterBO = numberList.FilterBusinessObjectDefaults[CusAuthorisationHeaderCollection.FilterConstants.AuthorisationType + ":Property"];
			AssertEquals("Value for AuthorisationType", CH.Business.CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar, authorisationTypeFilterBO.Value);
			AssertEquals("IsRemovable for AuthorisationType", false, authorisationTypeFilterBO.IsRemovable);

			var authorisationNumberFilterBO = numberList.FilterBusinessObjectDefaults[CusAuthorisationHeaderCollection.FilterConstants.AuthorisationNumber + ":Property"];
			AssertEquals("Value for AuthorisationNumber", "123456", authorisationNumberFilterBO.Value);
			AssertEquals("IsRemovable for AuthorisationNumber", true, authorisationNumberFilterBO.IsRemovable);

			var authorisationHolderFilterBO = numberList.FilterBusinessObjectDefaults[CusAuthorisationHeaderCollection.FilterConstants.AuthorisationHolder + ":Property"];
			AssertEquals("Value for AuthorisationHolder", orgHeaderPK, authorisationHolderFilterBO.Value);
			AssertEquals("IsRemovable for AuthorisationHolder", false, authorisationHolderFilterBO.IsRemovable);
		});
	}
}
