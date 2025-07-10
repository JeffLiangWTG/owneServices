using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.TemporaryStorage.Module.Testing;

sealed class UCC6TemporaryStorageFilterLookupsTest : TestCaseWithFactory
{
	public void TestPreviousDocumentsType()
	{
		var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
		_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", eun);
		_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
		_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, codeType, "9001", "9001 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, codeType, "9002", "9002 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, codeType, "9003", "9003 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, "9004", "9004 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

		Factory.Save();

		var lookups = new UCC6TemporaryStorageFilterLookups(new UCC6TemporaryStorageFilterStripBusinessObject());
		var codeList = lookups.PreviousDocumentsType;
		codeList.Load();

		AssertContainsExactElementsInAnyOrder("Only IT code list", new ZString[] { "9002", "9003" }, codeList.Select(x => x.ZZD_Code).ToArray());
	}
}
