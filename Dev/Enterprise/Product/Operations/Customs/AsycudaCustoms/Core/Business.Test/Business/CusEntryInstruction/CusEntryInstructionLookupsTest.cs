using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class CusEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestWeightUQList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertSame(Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight), instruction.Lookups.WeightUQList);
		}

		public void TestPortOfExitList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGroupingCode = GlbCompany.CurrentCompany.Country.Code; //"Botswana"
			var parentGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, "ParentGrouping");
			helper.CreateNewOrGetExistingDataGrouping(dataGroupingCode, "Botswana", parentGrouping);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsUQ");
			var cusCode = helper.CreateCusCodeList(dataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ARIA", "Ariamsvlei", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var cusCode1 = helper.CreateCusCodeList(dataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "LUDE", "Luderitz", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var cusCode2 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ZZxx", "ZZxxx", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var customsOfficeList = declaration.Lookups.CustomsOfficeList;
			var instruction = Factory.New<CusEntryInstruction>();
			var portOfExitList = instruction.Lookups.PortOfExitList;
			AssertEquals("PortOfExitList.Count = 0 if no declartion binded", 0, portOfExitList.Count);
			instruction.CEI_JE = declaration.PK;
			portOfExitList = instruction.Lookups.PortOfExitList;
			AssertEquals(customsOfficeList, portOfExitList);
			AssertEquals(2, portOfExitList.Count);
			AssertEquals("Ariamsvlei", portOfExitList.GetDescriptionFromCode(cusCode.ZZD_Code));
			AssertEquals("Luderitz", portOfExitList.GetDescriptionFromCode(cusCode1.ZZD_Code));
		}

		public void TestEntryStyleList()
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
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var instruction = Factory.New<CusEntryInstruction>();
			var entryStyleList = instruction.Lookups.StyleList;
			AssertEquals("EntryStyleList.Count = 0 if no declartion binded", 0, entryStyleList.Count);
			instruction.CEI_JE = declaration.PK;
			entryStyleList = instruction.Lookups.StyleList;
			AssertEquals("EntryStyleList.Count", 3, entryStyleList.Count);
			AssertEquals("AAA, BBB, CCC", entryStyleList.CodesAsString);
			AssertEquals("AAA Description", "AAA dec", entryStyleList.GetDescriptionFromCode("AAA"));
			AssertEquals("BBB Description", "BBB dec", entryStyleList.GetDescriptionFromCode("BBB"));
			AssertEquals("CCC Description", "", entryStyleList.GetDescriptionFromCode("CCC"));
			declaration.JE_MessageType = "EXP";
			entryStyleList = instruction.Lookups.StyleList;
			AssertEquals("EntryStyleList.Count", 3, entryStyleList.Count);
			AssertEquals("XXX, YYY, ZZZ", entryStyleList.CodesAsString);
			AssertEquals("XXX Description", "XXX dec", entryStyleList.GetDescriptionFromCode("XXX"));
			AssertEquals("YYY Description", "YYY dec", entryStyleList.GetDescriptionFromCode("YYY"));
			AssertEquals("ZZZ Description", "", entryStyleList.GetDescriptionFromCode("ZZZ"));
			declaration.JE_MessageType = "";
			entryStyleList = instruction.Lookups.StyleList;
			AssertEquals("EntryStyleList.Count", 0, entryStyleList.Count);
			var instructionWithoutDeclaration = Factory.New<CusEntryInstruction>();
			AssertEquals(string.Empty, instructionWithoutDeclaration.Lookups.StyleList.CodesAsString);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Namibia))
			{
				var declaration2 = Factory.New<JobDeclaration>();
				var instruction2 = declaration2.CustomsEntryInstructions.AddNew();
				var entryStyleList2 = instruction2.Lookups.StyleList;
				AssertEquals("EntryStyleList.Count", 1, entryStyleList2.Count);
				Assert("EntryStyleList", entryStyleList2.ContainsCode("ESD"));
				AssertEquals("ZZZ Description", "ensty1", entryStyleList2.GetDescriptionFromCode("ESD"));
			}
		}
	}
}
