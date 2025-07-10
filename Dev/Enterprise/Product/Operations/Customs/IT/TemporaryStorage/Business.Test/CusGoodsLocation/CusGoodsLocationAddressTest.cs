using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using CusAuthorizationHeaderTypeList = Enterprise.Customs.IT.Business.CusAuthorizationHeaderTypeList;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusGoodsLocationAddress))]
sealed class CusGoodsLocationAddressTest : EnterpriseBusinessObjectTestCase
{
	public void TestLookups()
	{
		var goodsLocationAddress = Factory.New<CusGoodsLocationAddress>();
		AssertType<CusGoodsLocationAddressLookups>(goodsLocationAddress.Lookups);
	}

	public void TestValidation()
	{
		var goodsLocationAddress = Factory.New<CusGoodsLocationAddress>();
		AssertType<CusGoodsLocationAddressValidation>(goodsLocationAddress.Validation);
	}

	public void TestAuthorisationNumberReadOnly()
	{
		var goodsLocationAddress = GoodsLocationAddress;
		var goodsLocation = goodsLocationAddress.GoodsLocation;
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
		AssertEquals("When CGL_Qualifier is Y", false, goodsLocationAddress.AuthorisationNumberInfo.ReadOnly);

		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.EoriNumber;
		AssertEquals("When CGL_Qualifier is not Y", true, goodsLocationAddress.AuthorisationNumberInfo.ReadOnly);
	}

	public void TestDefaultingAuthorisationNumber()
	{
		var holderWithMultipleAuth = Factory.NewWithValidTestData<OrgHeader>();
		var holderWithOnlyOneAuth = Factory.NewWithValidTestData<OrgHeader>();

		_ = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, holderWithMultipleAuth.PK, "TST11");
		_ = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, holderWithMultipleAuth.PK, "TST12");
		_ = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, holderWithOnlyOneAuth.PK, "TST21");
		Factory.Save();

		var goodsLocationAddress = GoodsLocationAddress;
		var goodsLocation = goodsLocationAddress.GoodsLocation;
		goodsLocationAddress.IdentificationHolderPK = holderWithMultipleAuth.PK;
		AssertEquals("When Holder has multiple authorisations, AuthorisationNumber", "", goodsLocationAddress.AuthorisationNumber);

		goodsLocationAddress.IdentificationHolderPK = holderWithOnlyOneAuth.PK;
		AssertEquals("When Holder has only one authorisations, AuthorisationNumber", "TST21", goodsLocationAddress.AuthorisationNumber);
	}

	public void TestDefaultingAdditionalIdentifier()
	{
		var holder = Factory.NewWithValidTestData<OrgHeader>();

		_ = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, holder.PK, "TST1")
			.AddLocRule("90808F")
			.AddLocRule("90815M");
		_ = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.TemporaryStorage, holder.PK, "TST2")
			.AddLocRule("92345R");

		var goodsLocationAddress = GoodsLocationAddress;
		goodsLocationAddress.IdentificationHolderPK = holder.PK;

		var goodsLocation = goodsLocationAddress.GoodsLocation;
		goodsLocationAddress.AuthorisationNumber = "TST1";
		AssertEquals("When Authorisation has multiple locations, AdditionalIdentifier", "", goodsLocation.AdditionalIdentifier);

		goodsLocationAddress.AuthorisationNumber = "TST2";
		AssertEquals("When Authorisation has only one location, AdditionalIdentifier", "92345R", goodsLocation.AdditionalIdentifier);
	}

	CusGoodsLocationAddress GoodsLocationAddress
	{
		get
		{
			if (fGoodsLocationAddress == null)
			{
				var header = Factory.New<TemporaryStorageHeader>();
				var goodsLocation = header.GoodsLocation;
				fGoodsLocationAddress = goodsLocation.Address;
			}

			return fGoodsLocationAddress;
		}
	}
	CusGoodsLocationAddress fGoodsLocationAddress;
}
