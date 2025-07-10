using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsPreviousDocumentPhase5LookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCodeList_WhenBothItAndEunCusCodesPresent()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		CreatePreviousDocumentCusCodeType(helper);

		var codeListPIT2 = CreateNewOrGetExistingCusCodeList("PIT2", "IT");
		var codeListPIT1 = CreateNewOrGetExistingCusCodeList("PIT1", "IT");
		var codeListPEU2 = CreateNewOrGetExistingCusCodeList("PEU2", "EUN");
		var codeListPEU1 = CreateNewOrGetExistingCusCodeList("PEU1", "EUN");
		Factory.Save();

		AssertType<ZZRefCusCodeListCombinedCollection>("List Type", lookups.CodeList);
		var typeCodesList = (ZZRefCusCodeListCombinedCollection)lookups.CodeList;
		var completeFilter = typeCodesList.CompleteFilter;
		AssertEquals("When only EUN CusCodes present, CodeList count", 4, lookups.CodeList.Count);
		CombineAssertions(() =>
		{
			AssertType<ZZRefCusCodeListCombinedCollection>("List Type", typeCodesList);
			AssertSame("Cached", typeCodesList, lookups.CodeList);
			AssertEquals("PEU1 Matched EUN", true, Factory.Load<ZZRefCusCodeListCombined>(codeListPEU1.PK).MatchesFilter(completeFilter));
			AssertEquals("PEU2 Matched EUN", true, Factory.Load<ZZRefCusCodeListCombined>(codeListPEU2.PK).MatchesFilter(completeFilter));
			AssertEquals("PIT1 Matched IT", true, Factory.Load<ZZRefCusCodeListCombined>(codeListPIT1.PK).MatchesFilter(completeFilter));
			AssertEquals("PIT2 Matched IT", true, Factory.Load<ZZRefCusCodeListCombined>(codeListPIT2.PK).MatchesFilter(completeFilter));
			AssertEquals("List type ordered by code", "PEU1, PEU2, PIT1, PIT2", string.Join(", ", typeCodesList.Select(x => x.ZZD_Code)));
		});

		RefCusCodeList CreateNewOrGetExistingCusCodeList(string code, string dataGrouping)
		{
			var codeList = helper.CreateNewOrGetExistingCusCodeList(dataGrouping,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS,
			code,
			"Description",
			ZDateTime.MinSmallDateTimeValue,
			ZDateTime.MaxSmallDateTimeUtc);
			helper.CreateCusCodeListAttribute(codeList.PK, RefCusCodeListAttributeTypes.Codes.Level, "Item");

			return codeList;
		}
	}

	public void TestPackTypeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeType("UNPKG", "Packagings", "UNE");
		helper.CreateCusCodeList("UNE", "UNPKG", "BOX", "Box", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeList("UNE", "UNPKG", "CNT", "Container", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		CombineAssertions(() =>
		{
			var packageTypeList = lookups.PackTypeList;
			AssertEquals("PackTypeList Count", 2, packageTypeList.Count);
			AssertEquals("PackTypeList ContainsCode 'BOX'", true, packageTypeList.ContainsCode("BOX"));
			AssertEquals("PackTypeList ContainsCode 'CNT'", true, packageTypeList.ContainsCode("CNT"));
		});
	}

	public void TestUnitOfQuantityList_GoodsItemLevel()
	{
		SetupUnitOfQuantityTestData();

		var unitOfQuantityList = lookups.UnitOfQuantityList;
		unitOfQuantityList.Sort();
		AssertEquals("When Type field <> N337, UnitOfQuantityList should contain all values", "KGM - Kilogram\r\nKGMG - Kilogram Gross\r\nMTQ - Cubic meter\r\nTNE - Tonne", unitOfQuantityList.ElementsAsString);

		previousDocument.CSI_Code = "N337";
		unitOfQuantityList = lookups.UnitOfQuantityList;
		AssertEquals("When Type field = N337 at goods item level, UnitOfQuantityList should only contain KGM", "KGM - Kilogram", unitOfQuantityList.ElementsAsString);
	}

	public void TestUnitOfQuantityList_HeaderLevel()
	{
		SetupUnitOfQuantityTestData();
		var nctsHeaderPreviousDocument = nctsHeader.PreviousDocuments.AddNew();
		nctsHeaderPreviousDocument.CSI_Code = "N337";

		var unitOfQuantityList = nctsHeaderPreviousDocument.Lookups.UnitOfQuantityList;

		AssertEquals("At header level, UnitOfQuantityList should be empty", string.Empty, unitOfQuantityList.ElementsAsString);
	}

	void SetupUnitOfQuantityTestData()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");

		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KGMG", "Kilogram Gross", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "MTQ", "Cubic meter", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "TNE", "Tonne", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "KGM", "Kilogram", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeUtc);

		Factory.Save();
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		previousDocument = goodsItem.PreviousDocuments.AddNew();
		lookups = new NctsPreviousDocumentPhase5Lookups(previousDocument);
	}

	NctsHeader nctsHeader;
	NctsPreviousDocument previousDocument;
	NctsPreviousDocumentPhase5Lookups lookups;

	void CreatePreviousDocumentCusCodeType(UniversalReferenceTestDataHelper helper)
	{
		var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: euGrouping);
		helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS, "Previous Document Of NCTS", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
	}
}
