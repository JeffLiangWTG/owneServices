using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using CusAuthorizationHeaderTypeList = Enterprise.Customs.IT.Business.CusAuthorizationHeaderTypeList;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(CusGoodsLocation))]
sealed class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
{
	public void TestLookup()
	{
		AssertType<CusGoodsLocationLookups>("Lookups", goodsLocation.Lookups);
	}

	public void TestCGL_AdditionalIdentifierCaption()
	{
		var data = DataBoundResourceStrings.GetDataForProperty(goodsLocation.CGL_AdditionalIdentifierInfo);
		AssertNotNull("Resource String Data", data);
		AssertEquals("Caption", "Place Code", data.Caption);
	}

	public void TestCGL_AdditionalIdentifier_UpdateValueIfOnlyOneValueIsPresent()
	{
		var holderPK = SetupAuthorisationHeadersAndRules();
		goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.AuthorizationNumber;
		goodsLocation.Address.IdentificationHolderPK = holderPK;
		goodsLocation.Address.AuthorisationNumber = "ALE1";
		AssertEquals("When Authorization Number (ALE1) having more than one LOC Rule Values", ZString.Empty, goodsLocation.CGL_AdditionalIdentifier);

		goodsLocation.Address.AuthorisationNumber = ZString.Empty;
		movementHeader.CustomsOffices.AddNew("DEP", "IT014199");
		goodsLocation.Address.AuthorisationNumber = "ALE1";
		AssertEquals("When Authorization Number ALE1 having only one LOC Rule Value", "90808F", goodsLocation.CGL_AdditionalIdentifier);

		movementHeader.CustomsOffices.RemoveAndDeleteAll();
		goodsLocation.Address.AuthorisationNumber = "ALE2";
		AssertEquals("When Authorization Number (ALE2) having more than one LOC Rule Values", ZString.Empty, goodsLocation.CGL_AdditionalIdentifier);

		goodsLocation.Address.AuthorisationNumber = ZString.Empty;
		movementHeader.CustomsOffices.AddNew("DEP", "IT117199");
		goodsLocation.Address.AuthorisationNumber = "ALE2";
		AssertEquals("When Authorization Number ALE2 having only one LOC Rule Value", "80815M", goodsLocation.CGL_AdditionalIdentifier);

		goodsLocation.Address.IdentificationHolderPK = ZGuid.Empty;
		AssertEquals("When IdentificationHolderPK is empty", ZString.Empty, goodsLocation.CGL_AdditionalIdentifier);
	}

	public void TestAdditionalIdentifierDescription()
	{
		const string description = "ALE1 - ALE";
		const string code = "T001";
		const string officeCode = "IT010101";

		var holder = Factory.NewWithValidTestData<OrgHeader>();
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory,
				CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport, holder.PK, "ALE1")
			.AddLocRule(code, officeCode);

		goodsLocation.Address.IdentificationHolderPK = holder.PK;
		goodsLocation.Address.AuthorisationNumber = "";
		goodsLocation.CGL_AdditionalIdentifier = code;

		CombineAssertions(() =>
		{
			AssertEquals("Description matches on valid additional identifier", description, goodsLocation.AdditionalIdentifierDescription);
			goodsLocation.CGL_AdditionalIdentifier = "InvalidCode";
			AssertEquals("Description is empty on invalid additional identifier", ZString.Empty, goodsLocation.AdditionalIdentifierDescription);
		});
	}

	public void TestAddressType()
	{
		AssertType<CusGoodsLocationAddress>(goodsLocation.Address);
	}

	public void TestGoodsLocationAddressOverride()
	{
		CombineAssertions("CGL_Qualifier and E2_AddressOverride combine test", () =>
		{
			var address = goodsLocation.Address;
			AssertEquals("When CGL_Qualifier is empty, E2_AddressOverride", true, address.E2_AddressOverride);

			goodsLocation.CGL_Qualifier = "Z";
			AssertEquals("When CGL_Qualifier = Z, E2_AddressOverride", false, address.E2_AddressOverride);

			goodsLocation.CGL_Qualifier = "Y";
			AssertEquals("When CGL_Qualifier = Y, E2_AddressOverride", true, address.E2_AddressOverride);
		});
	}

	public void TestClearAddressFields()
	{
		var orgAddress = Factory.New<OrgAddress>();
		var address = goodsLocation.Address;
		address.E2_OA_Address = orgAddress.PK;

		goodsLocation.CGL_Qualifier = "Z";
		AssertEquals("When CGL_Qualifier = Z, E2_AddressOverride", false, address.E2_AddressOverride);
		AssertEquals("OrganisationPK", orgAddress.OA_OH, address.OrganisationPK);

		CombineAssertions("Qualifier change calls ClearAddressFields", () =>
		{
			goodsLocation.CGL_Qualifier = "Y";
			AssertEquals("When CGL_Qualifier = Y, E2_AddressOverride", true, address.E2_AddressOverride);
			AssertEquals("OrganisationPK", ZGuid.Empty, address.OrganisationPK);
		});
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

	protected override BusinessObject GetNewBusinessObject() => goodsLocation;

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		goodsLocation = nctsHeader.MovementHeader.GoodsLocation;
		movementHeader = nctsHeader.MovementHeader;
	}

	CusGoodsLocation goodsLocation;
	NctsHeader nctsHeader;
	NctsDepartureMovementHeader movementHeader;

	ZGuid SetupAuthorisationHeadersAndRules()
	{
		var holder = Factory.NewWithValidTestData<OrgHeader>();
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport, holder.PK, "ALE1")
			.AddLocRule("90808F", "IT014199")
			.AddLocRule("90815M", "IT017199");

		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport, holder.PK, "ALE2")
			.AddLocRule("80808F", "IT114199")
			.AddLocRule("80815M", "IT117199");

		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.SelfAssessment, holder.PK, "SAS1");
		return holder.PK;
	}
}
