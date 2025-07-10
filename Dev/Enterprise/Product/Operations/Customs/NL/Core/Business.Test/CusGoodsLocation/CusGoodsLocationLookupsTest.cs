using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class CusGoodsLocationLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestFacilitiesList()
	{
		var lookups = GetLookup(MessageTypeList.Codes.Import, nameof(CusEntryInstruction));
		var helper = new Customs.Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Netherlands, parent: eunZZZ);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "FAC");
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "NL000001", "NL000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "NL000002", "NL000002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "NL000003", "NL000003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "NL000004", "NL000004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

		Factory.Save();

		CombineAssertions(() =>
		{
			((ZZRefCusCodeListCombinedCollection)lookups.UnlocodeList).Load();
			var codes = lookups.UnlocodeList;
			AssertContainsExactElementsInAnyOrder("Elements", new ZString[] { "NL000001", "NL000002", "NL000003", "NL000004" }, ((ZZRefCusCodeListCombinedCollection)lookups.UnlocodeList).Select(x => x.ZZD_Code));
			AssertSame("Cached", codes, lookups.UnlocodeList);
		});
	}

	public void TestQualifierList_Import_Declaration()
	{
		AssertQualifierList(MessageTypeList.Codes.Import, nameof(JobDeclaration), "T");
	}

	public void TestQualifierList_Export_Declaration()
	{
		AssertQualifierList(MessageTypeList.Codes.Export, nameof(JobDeclaration), "T");
	}

	public void TestQualifierList_MiscellaneousCustoms_Declaration()
	{
		AssertQualifierList(MessageTypeList.Codes.MiscellaneousCustoms, nameof(JobDeclaration), "T, U, V, W, X, Y, Z");
	}

	public void TestQualifierList_Import_CusEntryInstruction()
	{
		AssertQualifierList(MessageTypeList.Codes.Import, nameof(CusEntryInstruction), "T");
	}

	public void TestQualifierList_Export_CusEntryInstruction()
	{
		AssertQualifierList(MessageTypeList.Codes.Export, nameof(CusEntryInstruction), "T");
	}

	public void TestQualifierList_MiscellaneousCustoms_CusEntryInstruction()
	{
		AssertQualifierList(MessageTypeList.Codes.MiscellaneousCustoms, nameof(CusEntryInstruction), "T, U, V, W, X, Y, Z");
	}

	public void AssertQualifierList(ZString messageType, ZString parent, ZString expectedList)
	{
		var lookups = GetLookup(messageType, parent);
		var list = lookups.QualifierList;
		CombineAssertions(() =>
		{
			AssertEquals("List", expectedList, list.CodesAsString);
			AssertSame("Cached", list, lookups.QualifierList);
		});
	}

	CusGoodsLocationLookups GetLookup(ZString messageType, ZString parent)
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = messageType;
		switch (parent)
		{
			case nameof(JobDeclaration):
				var cusGoodsLocation = (CusGoodsLocation)declaration.GoodsLocation;
				return cusGoodsLocation.Lookups;
			default:
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				cusGoodsLocation = instruction.GoodsLocation;
				return cusGoodsLocation.Lookups;
		}
	}
}
