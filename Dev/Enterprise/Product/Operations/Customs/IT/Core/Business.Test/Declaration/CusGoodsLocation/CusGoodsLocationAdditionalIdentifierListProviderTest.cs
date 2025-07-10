using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusGoodsLocationAdditionalIdentifierListProviderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when Factory is null", () => new CusGoodsLocationAdditionalIdentifierListProvider(factory: null, new Mock<ICusGoodsLocationWrapper>().Object));
		AssertExceptionThrown<ArgumentNullException>("Exception expected when GoodsLocationWrapper is null", () => new CusGoodsLocationAdditionalIdentifierListProvider(Factory, goodsLocationWrapper: null));
	}

	public void TestAdditionalIdentifierList_WhenIdentificationHolderIsEmpty()
	{
		SetuUpAuthorisationHeadersAndRules();

		goodsLocation.Address.IdentificationHolderPK = ZGuid.Empty;
		AssertEquals("When Organisation and AuthorisationNumber is Empty, AdditionalIdentifierList.Count", 0, goodsLocationLookups.AdditionalIdentifierList.Count);
	}

	public void TestAdditionalIdentifierList_WhenAuthorisationNumberIsEmpty()
	{
		var holderPk = SetuUpAuthorisationHeadersAndRules();
		goodsLocation.Address.IdentificationHolderPK = holderPk;
		goodsLocation.Address.AuthorisationNumber = "";

		AssertContainsExactElementsInAnyOrder("When Authorisation Number is empty, AdditionalIdentifierList", new string[] { "90808F", "90815M", "100970K" }, goodsLocationLookups.AdditionalIdentifierList.GetAllCodes());
	}

	public void TestGetCachedList_LocationAuthorisationTypes()
	{
		var holderPk = SetuUpAuthorisationHeadersAndRules();
		goodsLocation.Address.IdentificationHolderPK = holderPk;
		goodsLocation.Address.AuthorisationNumber = "";
		declaration.JE_CustomsOffice = "";

		var cachedList = new CusGoodsLocationAdditionalIdentifierListProvider(Factory, new CusGoodsLocationWrapper(goodsLocation)).GetCachedList(new ZString[] { CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport });
		AssertContainsExactElementsInAnyOrder("Authorisation Type is ALI", new[] { "100970K" }, cachedList.GetAllCodes());
	}

	public void TestAdditionalIdentifierList_FilteringByOfficeOfPresentation()
	{
		var holderPk = SetuUpAuthorisationHeadersAndRules();

		goodsLocation.Address.IdentificationHolderPK = holderPk;
		goodsLocation.Address.AuthorisationNumber = "ALE1";

		declaration.JE_CustomsOffice = "IT014199";
		AssertEquals("When Office of Presentation is filled, AdditionalIdentifierList count", 1, goodsLocationLookups.AdditionalIdentifierList.Count);

		declaration.JE_CustomsOffice = "";
		AssertContainsExactElementsInAnyOrder("When Office of Presentation is empty, AdditionalIdentifierList", new string[] { "90808F", "90815M" }, goodsLocationLookups.AdditionalIdentifierList.GetAllCodes());
	}

	public void TestAdditionalIdentifierList_Description()
	{
		var holderPk = SetuUpAuthorisationHeadersAndRules();

		goodsLocation.Address.IdentificationHolderPK = holderPk;
		goodsLocation.Address.AuthorisationNumber = "ALE1";
		declaration.JE_CustomsOffice = "IT014199";

		AssertEquals("[PRE-CONDITION] AdditionalIdentifierList count", 1, goodsLocationLookups.AdditionalIdentifierList.Count);
		var additionalIdentifier = goodsLocationLookups.AdditionalIdentifierList[0];
		CombineAssertions(() =>
		{
			AssertEquals("AdditionalIdentifierList Code", "90808F", additionalIdentifier.Code);
			AssertEquals("AdditionalIdentifierList Description", "ALE1 - ALE", additionalIdentifier.Description);
		});
	}

	ZGuid SetuUpAuthorisationHeadersAndRules()
	{
		var holder = Factory.NewWithValidTestData<OrgHeader>();
		var authWithCus = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForExport, holder.PK, "ALE1")
			.AddLocRule("90808F", "IT014199")
			.AddLocRule("90815M", "IT017199");

		var auth2 = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, holder.PK, "ALI1")
			.AddLocRule("100970K", "IT025000");
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, CusAuthorizationHeaderTypeList.Codes.SelfAssessment, holder.PK, "SAS1");

		return holder.PK;
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		goodsLocation = (CusGoodsLocation)declaration.GoodsLocation;
		goodsLocationLookups = goodsLocation.Lookups;
	}

	JobDeclaration declaration;
	CusGoodsLocation goodsLocation;
	CusGoodsLocationLookups goodsLocationLookups;
}
