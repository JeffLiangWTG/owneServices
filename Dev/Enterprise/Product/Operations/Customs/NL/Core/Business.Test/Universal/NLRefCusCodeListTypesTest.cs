using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class NLRefCusCodeListTypesTest : TestCaseWithFactory
{
	public void TestGetCustomsStatusList()
	{
		var codeList = Factory.New<ZZRefCusCodeListCombined>();
		codeList.ZZD_Code = "99";
		codeList.ZZD_Description = "Test NL RefCusCodeList";
		codeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Netherlands;
		codeList.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus;
		codeList.ZZD_StartDate = ZDateTime.Today.AddYears(-1);
		codeList.ZZD_EndDate = ZDateTime.Today.AddYears(1);
		Factory.Save();
		var list1 = NLRefCusCodeListTypes.GetCustomsStatusList(Factory);
		AssertEquals(1, list1.Count);
		AssertEquals("Test NL RefCusCodeList", list1.GetDescriptionFromCode("99"));
		var list2 = NLRefCusCodeListTypes.GetCustomsStatusList(Factory);
		AssertSame("IsCached", list1, list2);
	}
}
