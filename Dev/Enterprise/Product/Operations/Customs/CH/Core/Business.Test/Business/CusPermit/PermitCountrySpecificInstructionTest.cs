using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

public class PermitCountrySpecificInstructionTest : TestCaseWithFactory
{
	public void TestIsQtyValIndicatorMandatory()
	{
		var instruction = PermitCountrySpecificInstruction.GetByCountryCode(Factory, Core.Constants.CountryCodes.Switzerland);
		AssertEquals(false, instruction.IsQtyValIndicatorMandatory);
	}

	public void TestGetTypeList()
	{
		RefCusCodeTestHelper.CreatePermitAuthorityCodeList(Factory);

		var instruction = PermitCountrySpecificInstruction.GetByCountryCode(Factory, Core.Constants.CountryCodes.Switzerland);
		PermitTypeList typeList = instruction.GetTypeList();

		CombineAssertions(() =>
		{
			AssertEquals(true, typeList.ContainsCode(RefCusCodeTestHelper.ValidPermitAuthorityCode));
			AssertEquals(false, typeList.ContainsCode(RefCusCodeTestHelper.InvalidPermitAuthorityCode));
		});
	}

	public void TestTypeListSortOrder()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		const string codeType = "PRMAU";
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Switzerland);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType);
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Permit, codeType, codeType, Core.Constants.CountryCodes.Switzerland);

		foreach (var code in new string[] { "1", "11", "2", "22", "3", "33", "4", "44", "5", "55", "6", "66", "7", "77", "8", "88", "9", "99" })
		{
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Switzerland, codeType, code, code, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}
		Factory.Save();

		var instruction = PermitCountrySpecificInstruction.GetByCountryCode(Factory, Core.Constants.CountryCodes.Switzerland);
		PermitTypeList typeList = instruction.GetTypeList();
		AssertEquals("1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 22, 33, 44, 55, 66, 77, 88, 99", typeList.CodesAsString);
	}
}
