using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;
using CusAuthorizationHeaderTypeList = Enterprise.Customs.IT.Business.CusAuthorizationHeaderTypeList;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

sealed class CusGoodsLocationAddressLookupsTest : TestCaseWithFactory
{
	public void TestAuthorisationNumberList()
	{
		var holderPK = Factory.NewWithValidTestData<OrgHeader>().PK;
		var holder2PK = Factory.NewWithValidTestData<OrgHeader>().PK;

		_ = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, holderPK, "TST1");
		_ = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, holderPK, "TST2");
		_ = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport, holderPK, "ALE1");
		_ = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, holderPK, "ALI1");
		_ = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, holder2PK, "TST3");

		var goodsLocationAddress = Factory.New<CusGoodsLocationAddress>();
		goodsLocationAddress.IdentificationHolderPK = holderPK;
		goodsLocationAddress.AuthorisationNumber = "TST1";

		var lookups = goodsLocationAddress.Lookups;

		var numberList = (CusAuthorisationHeaderCollectionFiltered)lookups.AuthorisationNumberList;

		CombineAssertions("CusAuthorisationHeaderCollectionFiltered", () =>
		{
			var holderFilterBO = numberList.FilterBusinessObjectDefaults[CusAuthorisationHeaderCollection.FilterConstants.AuthorisationHolder + ":Property"];
			AssertEquals("Value for AuthorisationHolder", holderPK, holderFilterBO.Value);
			AssertEquals("IsRemovable for AuthorisationHolder", false, holderFilterBO.IsRemovable);

			var numberFilterBO = numberList.FilterBusinessObjectDefaults[CusAuthorisationHeaderCollection.FilterConstants.AuthorisationNumber + ":Property"];
			AssertEquals("Value for AuthorisationNumber", "TST1", numberFilterBO.Value);
			AssertEquals("IsRemovable for AuthorisationNumber", true, numberFilterBO.IsRemovable);
		});

		AssertContainsExactElementsInAnyOrder("List codes", new ZString[] { "TST1", "TST2" }, numberList.Select(x => x.CPH_Number));
	}
}
