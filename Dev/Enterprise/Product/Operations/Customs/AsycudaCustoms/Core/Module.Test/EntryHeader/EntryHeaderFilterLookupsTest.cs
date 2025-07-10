using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.Module.Testing
{
	class EntryHeaderFilterLookupsTest : TestCaseWithFactory
	{
		public void TestEntryInstructionStyleList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGroupingCode = GlbCompany.CurrentCompany.Country.Code; //"Botswana"
			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, "ZZ");
			helper.CreateNewOrGetExistingDataGrouping(dataGroupingCode, dataGroupingCode, parentDataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Namibia, "Namibia", parentDataGrouping);
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty;
			helper.CreateNewOrGetExistingCusCodeType(codeType, "Entry style");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, codeType, "ESD", "ensty1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(dataGroupingCode, codeType, "AAA", "AAA dec", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(dataGroupingCode, codeType, "BBB", "BBB dec", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(dataGroupingCode, codeType, "XXX", "XXX dec", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(dataGroupingCode, codeType, "YYY", "YYY dec", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			helper.CreateRefCusProcedure(dataGroupingCode, "A", "11", "11", "111", "One", "IMP", group: "AAA,BBB");
			helper.CreateRefCusProcedure(dataGroupingCode, "A", "22", "22", "222", "Two", "IMP", group: "BBB,CCC");
			helper.CreateRefCusProcedure(dataGroupingCode, "B", "33", "33", "333", "Three", "IMP", group: "AAA");
			helper.CreateRefCusProcedure(dataGroupingCode, "A", "44", "44", "444", "Four", "EXP", group: "XXX");
			helper.CreateRefCusProcedure(dataGroupingCode, "A", "55", "55", "555", "Five", "EXP", group: "XXX;YYY,ZZZ");
			helper.CreateRefCusProcedure("NA", "B", "33", "33", "333", "Three", "IMP", group: "IFW");
			helper.CreateRefCusProcedure("NA", "A", "55", "55", "555", "Five", "EXP", group: "ESD");
			Factory.Save();
			var filterBizObj = new EntryHeaderFilterBusinessObject();
			var entryStyleList = filterBizObj.Lookups.EntryInstructionStyleList;
			AssertEquals("AAA, BBB, CCC, XXX, YYY, ZZZ", entryStyleList.CodesAsString);
			AssertEquals("AAA Description", "AAA dec", entryStyleList.GetDescriptionFromCode("AAA"));
			AssertEquals("BBB Description", "BBB dec", entryStyleList.GetDescriptionFromCode("BBB"));
			AssertEquals("CCC Description", "", entryStyleList.GetDescriptionFromCode("CCC"));
			AssertEquals("XXX Description", "XXX dec", entryStyleList.GetDescriptionFromCode("XXX"));
			AssertEquals("YYY Description", "YYY dec", entryStyleList.GetDescriptionFromCode("YYY"));
			AssertEquals("ZZZ Description", "", entryStyleList.GetDescriptionFromCode("ZZZ"));

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Namibia))
			{
				var filterBizObj2 = new EntryHeaderFilterBusinessObject();
				var entryStyleList2 = filterBizObj2.Lookups.EntryInstructionStyleList;
				AssertEquals("ESD, IFW", entryStyleList2.CodesAsString);
				AssertEquals("ESD Description", "ensty1", entryStyleList2.GetDescriptionFromCode("ESD"));
				AssertEquals("IFW Description", "", entryStyleList2.GetDescriptionFromCode("IFW"));
			}
		}
	}
}
