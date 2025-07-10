using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(CusGoodsLocationAddress))]
sealed class CusGoodsLocationAddressTest : EnterpriseBusinessObjectTestCase
{
	public void TestLookups()
	{
		var goodsLocationAddress = Factory.New<CusGoodsLocationAddress>();
		AssertType<CusGoodsLocationAddressLookups>("Lookups", goodsLocationAddress.Lookups);
	}

	public void TestValidation()
	{
		var cusGoodsLocationAddress = Factory.New<CusGoodsLocationAddress>();
		AssertType<CusGoodsLocationAddressValidation>("Validation", cusGoodsLocationAddress.Validation);
	}

	public void TestDefaultingAuthorisationNumber()
	{
		var holderWithMultipleAuth = Factory.NewWithValidTestData<OrgHeader>();
		var holderWithOnlyOneAuth = Factory.NewWithValidTestData<OrgHeader>();

		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, "ALE", holderWithMultipleAuth.PK, "ALE1");
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, "ALI", holderWithMultipleAuth.PK, "ALE1");
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, "ALE", holderWithOnlyOneAuth.PK, "ALE2");
		Factory.Save();

		goodsLocation.Address.IdentificationHolderPK = holderWithMultipleAuth.PK;
		AssertEquals("When Holder has multiple authorisations, AuthorisationNumber", "", goodsLocation.Address.AuthorisationNumber);

		goodsLocation.Address.IdentificationHolderPK = holderWithOnlyOneAuth.PK;
		AssertEquals("When Holder has only one authorisations, AuthorisationNumber", "ALE2", goodsLocation.Address.AuthorisationNumber);
	}

	public void TestDefaultingAdditionalIdentifier()
	{
		var holder = Factory.NewWithValidTestData<OrgHeader>();

		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, "ALE", holder.PK, "ALE1")
			.AddLocRule("90808F")
			.AddLocRule("90815M");
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, "ALI", holder.PK, "ALI1")
			.AddLocRule("92345R");

		goodsLocation.Address.IdentificationHolderPK = holder.PK;

		goodsLocation.Address.AuthorisationNumber = "ALE1";
		AssertEquals("When Authorisation has multiple locations, AdditionalIdentifier", "", goodsLocation.CGL_AdditionalIdentifier);

		goodsLocation.Address.AuthorisationNumber = "ALI1";
		AssertEquals("When Authorisation has only one location, AdditionalIdentifier", "92345R", goodsLocation.CGL_AdditionalIdentifier);
	}

	public void TestE2_City_MaxLength()
	{
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_City = new ZString('0', 40);
		goodsLocationAddress.E2_OA_Address = orgAddress.PK;

		CombineAssertions("E2_City", () =>
		{
			AssertEquals("MaxLength", 35, goodsLocationAddress.E2_CityInfo.MaxLength);
			AssertEquals("When more than 35 char City assigned to OrgAddress", new ZString('0', 35), goodsLocationAddress.E2_City);
		});
	}

	public void TestE2_ValidationStatusIsSetToManuallyVerifiedWhenAddressIsOverride()
	{
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_Address1 = "MG Road";
		orgAddress.OA_Address2 = "4th Cross";
		goodsLocationAddress.E2_OA_Address = orgAddress.PK;

		AssertEquals("[PRE-CONDITION] E2_AddressOverride", false, goodsLocationAddress.E2_AddressOverride);

		goodsLocationAddress.E2_AddressOverride = true;
		AssertEquals("When Address is override, E2_ValidationStatus", AddressValidationStatus.ManuallyVerified, goodsLocationAddress.E2_ValidationStatus);

		goodsLocationAddress.E2_AddressOverride = false;
		AssertEquals("When Address is not override, E2_ValidationStatus matches linked address OA_ValidationStatus", orgAddress.OA_ValidationStatus, goodsLocationAddress.E2_ValidationStatus);
	}

	public void TestIgnoreValidationStatusError()
	{
		AssertEquals("IgnoreValidationStatusError", true, goodsLocationAddress.IgnoreValidationStatusError);
	}

	public void TestE2_Address1AndE2_Address2_Truncate()
	{
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.Address1 = new ZString('1', 50);
		orgAddress.Address2 = new ZString('2', 50);

		goodsLocationAddress.E2_OA_Address = orgAddress.PK;
		var expectedAddress = new ZString('1', 50) + new ZString('2', 20);
		AssertEquals("When Address1 + Address2 more than max length", expectedAddress, goodsLocationAddress.E2_Address1AndE2_Address2);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		goodsLocation = (CusGoodsLocation)declaration.GoodsLocation;
		goodsLocation.CGL_LocationUse = "DEP";
		goodsLocationAddress = goodsLocation.Address;
	}

	JobDeclaration declaration;
	CusGoodsLocation goodsLocation;
	CusGoodsLocationAddress goodsLocationAddress;

	protected override BusinessObject GetNewBusinessObject() => goodsLocationAddress;

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => goodsLocationAddress;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => goodsLocationAddress;

	protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		return jobDeclaration;
	}
}
