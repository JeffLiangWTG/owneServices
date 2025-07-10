using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class EntryInstructionStyleListHelperTest : TestCaseWithFactory
	{
		public void TestEntryInstructionStyleListForDataGrouping()
		{
			SetupRefData();
			AssertEquals(0, EntryInstructionStyleListHelper.EntryInstructionStyleListForDataGrouping(Factory, "").Count);
			var entryStyleList = EntryInstructionStyleListHelper.EntryInstructionStyleListForDataGrouping(Factory, GlbCompany.CurrentCompany.Country.Code);
			AssertEquals("AAA, BBB, CCC, XXX, YYY, ZZZ", entryStyleList.CodesAsString);
			AssertEquals("AAA Description", "AAA dec", entryStyleList.GetDescriptionFromCode("AAA"));
			AssertEquals("BBB Description", "BBB dec", entryStyleList.GetDescriptionFromCode("BBB"));
			AssertEquals("CCC Description", "", entryStyleList.GetDescriptionFromCode("CCC"));
			AssertEquals("XXX Description", "XXX dec", entryStyleList.GetDescriptionFromCode("XXX"));
			AssertEquals("YYY Description", "YYY dec", entryStyleList.GetDescriptionFromCode("YYY"));
			AssertEquals("ZZZ Description", "", entryStyleList.GetDescriptionFromCode("ZZZ"));
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Namibia))
			{
				var entryStyleList2 = EntryInstructionStyleListHelper.EntryInstructionStyleListForDataGrouping(Factory, Core.Constants.CountryCodes.Namibia);
				AssertEquals("ESD, IFW", entryStyleList2.CodesAsString);
				AssertEquals("ESD Description", "ensty1", entryStyleList2.GetDescriptionFromCode("ESD"));
				AssertEquals("IFW Description", "", entryStyleList2.GetDescriptionFromCode("IFW"));
			}
		}

		public void TestEntryInstructionStyleListForDataGroupingAndShipmentType()
		{
			SetupRefData();
			var dataGroupingCode = GlbCompany.CurrentCompany.Country.Code; //"Botswana"
			AssertEquals(0, EntryInstructionStyleListHelper.EntryInstructionStyleListForDataGroupingAndShipmentType(Factory, "", "").Count);
			AssertEquals(0, EntryInstructionStyleListHelper.EntryInstructionStyleListForDataGroupingAndShipmentType(Factory, "", "IMP").Count);
			AssertEquals(0, EntryInstructionStyleListHelper.EntryInstructionStyleListForDataGroupingAndShipmentType(Factory, dataGroupingCode, "").Count);
			var entryStyleList = EntryInstructionStyleListHelper.EntryInstructionStyleListForDataGroupingAndShipmentType(Factory, dataGroupingCode, "IMP");
			AssertEquals("EntryStyleList.Count", 3, entryStyleList.Count);
			AssertEquals("AAA, BBB, CCC", entryStyleList.CodesAsString);
			AssertEquals("AAA Description", "AAA dec", entryStyleList.GetDescriptionFromCode("AAA"));
			AssertEquals("BBB Description", "BBB dec", entryStyleList.GetDescriptionFromCode("BBB"));
			AssertEquals("CCC Description", "", entryStyleList.GetDescriptionFromCode("CCC"));
			entryStyleList = EntryInstructionStyleListHelper.EntryInstructionStyleListForDataGroupingAndShipmentType(Factory, dataGroupingCode, "EXP");
			AssertEquals("EntryStyleList.Count", 3, entryStyleList.Count);
			AssertEquals("XXX, YYY, ZZZ", entryStyleList.CodesAsString);
			AssertEquals("XXX Description", "XXX dec", entryStyleList.GetDescriptionFromCode("XXX"));
			AssertEquals("YYY Description", "YYY dec", entryStyleList.GetDescriptionFromCode("YYY"));
			AssertEquals("ZZZ Description", "", entryStyleList.GetDescriptionFromCode("ZZZ"));
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Namibia))
			{
				var entryStyleList2 = EntryInstructionStyleListHelper.EntryInstructionStyleListForDataGroupingAndShipmentType(Factory, Core.Constants.CountryCodes.Namibia, "EXP");
				AssertEquals("EntryStyleList.Count", 1, entryStyleList2.Count);
				Assert("EntryStyleList", entryStyleList2.ContainsCode("ESD"));
				AssertEquals("ZZZ Description", "ensty1", entryStyleList2.GetDescriptionFromCode("ESD"));
			}
		}

		void SetupRefData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGroupingCode = GlbCompany.CurrentCompany.Country.Code; //"Botswana"
			var parentDataGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, "ZZ");
			helper.CreateNewOrGetExistingDataGrouping(dataGroupingCode, dataGroupingCode, parentDataGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Namibia, "Namibia", parentDataGrouping);
			var codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensty;
			helper.CreateNewOrGetExistingCusCodeType(codeType, "Entry style");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", codeType, "ESD", "ensty1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
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
		}
	}
}
