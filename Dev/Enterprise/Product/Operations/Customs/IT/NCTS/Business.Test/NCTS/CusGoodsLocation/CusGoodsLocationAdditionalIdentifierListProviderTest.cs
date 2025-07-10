using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class CusGoodsLocationAdditionalIdentifierListProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When Factory is null", () => new CusGoodsLocationAdditionalIdentifierListProvider(null, new Mock<ICusGoodsLocationWrapper>().Object));
		AssertExceptionThrown<ArgumentNullException>("When GoodsLocationWrapper is null", () => new CusGoodsLocationAdditionalIdentifierListProvider(Factory, goodsLocationWrapper: null));
	}

	public void TestAdditionalIdentifierList_WhenIdentificationHolderIsEmpty()
	{
		SetupAuthorisationHeadersAndRules();
		goodsLocation.Address.IdentificationHolderPK = ZGuid.Empty;
		AssertEquals("When Organization and Authorization Number is empty", 0, goodsLocationLookups.AdditionalIdentifierList.Count);
	}

	public void TestAdditionalIdentifierList_WhenAuthorisationNumberIsEmpty()
	{
		var holderPk = SetupAuthorisationHeadersAndRules();
		goodsLocation.Address.IdentificationHolderPK = holderPk;
		goodsLocation.Address.AuthorisationNumber = "";

		var expectedCodes = new[] { "90808F", "90815M", "80808A", "80815B", "100970K" };
		AssertContainsExactElementsInAnyOrder("When Authorization Number is empty", expectedCodes, goodsLocationLookups.AdditionalIdentifierList.GetAllCodes());
	}

	public void TestAdditionalIdentifierList_WhenCustomsOfficesWithPurposeDEPArePresent()
	{
		var holderPk = SetupAuthorisationHeadersAndRules();
		goodsLocation.Address.IdentificationHolderPK = holderPk;
		goodsLocation.Address.AuthorisationNumber = "ALE1";

		AddCustomsOffice("DEP", "IT014199");
		AddCustomsOffice("TRA", "IT017199");

		var expectedCodes = new[] { "90808F" };
		AssertContainsExactElementsInAnyOrder("When Authorization Number = ALE1 and One Customs Office with Purpose DEP is present", expectedCodes, goodsLocationLookups.AdditionalIdentifierList.GetAllCodes());

		movementHeader.CustomsOffices.RemoveAndDeleteAll();
		AddCustomsOffice("DEP", "IT114199");
		AddCustomsOffice("DEP", "IT317199");
		goodsLocation.Address.AuthorisationNumber = "ALE2";

		expectedCodes = new[] { "80808A", "80815B" };
		AssertContainsExactElementsInAnyOrder("When Authorization Number = ALE1 and More Customs Offices with Purpose DEP are present", expectedCodes, goodsLocationLookups.AdditionalIdentifierList.GetAllCodes());
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		movementHeader = nctsHeader.MovementHeader;
		goodsLocation = nctsHeader.MovementHeader.GoodsLocation;
		goodsLocationLookups = goodsLocation.Lookups;
	}

	CusGoodsLocationLookups goodsLocationLookups;
	CusGoodsLocation goodsLocation;
	NctsHeader nctsHeader;
	NctsDepartureMovementHeader movementHeader;

	void AddCustomsOffice(string purpose, string customsOfficeCode)
	{
		var cusOffice = movementHeader.CustomsOffices.AddNew();
		cusOffice.CY_Code = purpose;
		cusOffice.CY_Data = customsOfficeCode;
	}

	ZGuid SetupAuthorisationHeadersAndRules()
	{
		var holder = Factory.NewWithValidTestData<OrgHeader>();
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport, holder.PK, "ALE1")
			.AddLocRule("90808F", "IT014199")
			.AddLocRule("90815M", "IT017199");

		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, holder.PK, "ALE2")
			.AddLocRule("80808A", "IT114199")
			.AddLocRule("80815B", "IT317199");

		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, holder.PK, "ALI1")
			.AddLocRule("100970K", "IT025000");
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.SelfAssessment, holder.PK, "SAS1");

		return holder.PK;
	}
}
