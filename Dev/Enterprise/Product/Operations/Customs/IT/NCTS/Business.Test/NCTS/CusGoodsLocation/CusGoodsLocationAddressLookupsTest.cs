using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;
using CusAuthorizationHeaderTypeList = Enterprise.Customs.IT.Business.CusAuthorizationHeaderTypeList;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class CusGoodsLocationAddressLookupsTest : TestCaseWithFactory
{
	public void TestAuthorisationNumberList()
	{
		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		var organisation2 = Factory.NewWithValidTestData<OrgHeader>();

		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport, organisation.PK, "ALE1");
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, organisation.PK, "ALI2");
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.SelfAssessment, organisation.PK, "SAS1");
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, organisation.PK, "CW1");
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, organisation2.PK, "ALI3");

		var goodsLocationAddress = Factory.New<CusGoodsLocationAddress>();
		var lookups = goodsLocationAddress.Lookups;

		var authList = lookups.AuthorisationNumberList;
		AssertEquals("AuthorisationNumberList is CusAuthorisationHeaderCollection?", true, authList is CusAuthorisationHeaderCollection);

		var authorisationHeaderCollection = authList as CusAuthorisationHeaderCollection;
		AssertEquals("AuthorisationNumberList count", 3, authorisationHeaderCollection.Count);
		var authNumbers = authorisationHeaderCollection.Select(x => x.CPH_Number);
		AssertContainsExactElementsInAnyOrder("Authorizations", new ZString[] { "ALE1", "ALI2", "ALI3" }, authorisationHeaderCollection.Select(x => x.CPH_Number));

		goodsLocationAddress.IdentificationHolderPK = organisation2.PK;
		AssertEquals("AuthorisationNumberList count", 1, lookups.AuthorisationNumberList.Count);
		AssertContainsExactElementsInAnyOrder("Authorizations", new ZString[] { "ALI3" }, lookups.AuthorisationNumberList.Cast<CusAuthorisationHeader>().Select(x => x.CPH_Number));
	}
}
