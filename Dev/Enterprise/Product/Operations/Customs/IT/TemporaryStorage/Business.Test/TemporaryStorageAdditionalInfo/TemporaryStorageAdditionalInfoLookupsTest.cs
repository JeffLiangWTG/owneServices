using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStorageAdditionalInfoLookups))]
sealed class TemporaryStorageAdditionalInfoLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCodeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy);
		helper.CreateCusCodeType("ADDIN", "ADDIN", Core.Constants.CountryCodes.Italy);
		helper.CreateCusCodeType("AI44R", "AI44R", Core.Constants.CountryCodes.Italy);
		helper.CreateCusCodeType("AR44T", "AR44T", Core.Constants.CountryCodes.Italy);
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, "ADDIN", "code1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, "AI44R", "code2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, "AR44T", "code3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();
		var packedItem = bill.PackedItems.AddNew();
		var addInfo = packedItem.AdditionalInfos.AddNew();

		addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		var codeList = addInfo.Lookups.CodeList as ZZRefCusCodeListCombinedCollection;
		codeList.Load();
		AssertEquals(1, codeList.Count);
		AssertContainsExactElementsInAnyOrder(new ZString[] { "code1" }, codeList.Select(x => x.ZZD_Code));

		addInfo.CSI_SubType = "BLA";
		AssertEquals(0, addInfo.Lookups.CodeList.Count);
	}

	public void TestSubTypeList()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var bill = header.Bills.AddNew();
		var packedItem = bill.PackedItems.AddNew();
		var addInfo = packedItem.AdditionalInfos.AddNew();

		AssertEquals("List should contain only 'INF' value for 'Kind' field in TempStorage AddInfo", AdditionalInfoSubTypeList.Codes.AdditionalInformation, addInfo.Lookups.SubTypeList.CodesAsString);
	}
}

