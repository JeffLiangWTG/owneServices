using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class GuaranteeCountrySpecificInstructionTest : TestCaseWithFactory
{
	public void TestGetTypeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "Guarantee Types");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "ZZZ", "Description for ZZZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();
		var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
		var typeList = guaranteeHeader.CountrySpecificInstruction.GetTypeList(Core.Constants.CountryCodes.Belgium);
		AssertContainsExactElementsInAnyOrder("Type List (NCTS is enabled)", new ZString[] { "COD", "IMP", "TRA", "TST", "ZZZ" }, typeList.GetAllCodes());
	}

	public void TestGetSubTypeList_IMP()
	{
		AssertGetSubTypeList(EUGuaranteeTypeList.Codes.IMP, expectedSubTypeList);
	}

	public void TestGetSubTypeList_COD()
	{
		AssertGetSubTypeList(EUGuaranteeTypeList.Codes.COD, expectedSubTypeList);
	}

	public void TestGetSubTypeList_TRA()
	{
		CreateCusCodeList("EUN", "CL251", expectedSubTypeList_TRA);
		Factory.Save();
		AssertGetSubTypeList(EUGuaranteeTypeList.Codes.TRA, expectedSubTypeList_TRA);
	}

	void AssertGetSubTypeList(string code, ZString[] expectedList)
	{
		var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
		var subTypeList = guaranteeHeader.CountrySpecificInstruction.GetSubTypeList(code);
		AssertContainsExactElementsInAnyOrder($"Sub Type List for {code}", expectedList, subTypeList.GetAllCodes());
	}

	void CreateCusCodeList(ZString dataGroupingCode, ZString codeType, IEnumerable<ZString> codes)
	{
		var currentCountry = Core.Constants.CountryCodes.Belgium;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var dataGrouping = helper.CreateNewOrGetExistingDataGrouping(dataGroupingCode);
		helper.CreateNewOrGetExistingDataGrouping(currentCountry, parent: dataGrouping);
		foreach (var code in codes)
		{
			helper.CreateCusCodeList(dataGroupingCode, codeType, code, ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
		}
	}

	readonly ZString[] expectedSubTypeList = new ZString[] { "0", "1", "2", "3", "5", "8", "C", "I", "R" };
	readonly ZString[] expectedSubTypeList_TRA = new ZString[] { "X", "Y", "Z" };
}
