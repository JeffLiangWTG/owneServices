using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class CusEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCEI_SubStyleAndLookups()
		{
			var dec = Factory.New<JobDeclaration>();
			var instruction = Factory.New<CusEntryInstruction>();

			instruction.CEI_JE = dec.PK;
			instruction.CEI_SubStyle = "X";

			AssertNotNull(instruction.Lookups.EntrySubStyleList);
			AssertType(GetExpectedEntrySubStyleListType(), instruction.Lookups.EntrySubStyleList);
		}

		protected virtual Type GetExpectedEntrySubStyleListType() => typeof(CodeDescriptionPairList);

		public void TestEntrySubStyleList()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			AssertEquals(GetExpectedEntrySubStyleListCodesAsStringWithoutDeclaration(), instruction.Lookups.EntrySubStyleList.CodesAsString);
			AssertSame(instruction.Lookups.EntrySubStyleList, instruction.Lookups.EntrySubStyleList);

			var dec = Factory.New<JobDeclaration>();
			instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = dec.PK;
			AssertType(TypeOfDeclarationOfInstruction, instruction.JobDeclaration);

			AssertEquals(GetExpectedEntrySubStyleListCodesAsString(), instruction.Lookups.EntrySubStyleList.CodesAsString);
			AssertSame(instruction.Lookups.EntrySubStyleList, instruction.Lookups.EntrySubStyleList);

			dec.JE_MessageType = MessageTypeList.Codes.Import;
			AssertSame("Should have cached value on lookups.EntrySubStyleList - Import ", instruction.Lookups.EntrySubStyleList, instruction.Lookups.EntrySubStyleList);

			dec.JE_MessageType = MessageTypeList.Codes.Export;
			AssertSame("Should have cached value on lookups.EntrySubStyleList - Export", instruction.Lookups.EntrySubStyleList, instruction.Lookups.EntrySubStyleList);
		}

		protected virtual ZString GetExpectedEntrySubStyleListCodesAsStringWithoutDeclaration() => ZString.Empty;

		protected virtual ZString GetExpectedEntrySubStyleListCodesAsString() => "A, B, C, D, E, F, U, V, X, Y, Z";

		protected virtual Type TypeOfDeclarationOfInstruction => typeof(JobDeclaration);

		public void TestDeclarationTypeListNoDeclaration()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			AssertEquals(string.Empty, instruction.Lookups.DeclarationTypeList.CodesAsString);
			AssertSame(instruction.Lookups.DeclarationTypeList, instruction.Lookups.DeclarationTypeList);
		}

		public void TestDeclarationTypeListWithDeclaration()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "111", "One", "IMP", group: "AAA,BBB");
			helper.CreateRefCusProcedure(currentCountry, "A", "22", "22", "222", "Two", "IMP", group: "BBB,CCC");
			helper.CreateRefCusProcedure(currentCountry, "B", "33", "33", "333", "Three", "IMP", group: "AAA");
			helper.CreateRefCusProcedure(currentCountry, "A", "44", "44", "444", "Four", "EXP", group: "XXX");
			helper.CreateRefCusProcedure(currentCountry, "A", "55", "55", "555", "Five", "EXP", group: "XXX;YYY,ZZZ");
			helper.CreateRefCusProcedure("ZA", "B", "33", "33", "333", "Three", "IMP", group: "IFW");
			helper.CreateRefCusProcedure("ZA", "A", "55", "55", "555", "Five", "EXP", group: "ESD");

			var declaration = Factory.New<JobDeclaration>();
			var cei = declaration.CustomsEntryInstructions.AddNew();
			declaration.JE_MessageType = "IMP";
			AssertEquals(3, cei.Lookups.DeclarationTypeList.Count);
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			AssertEquals("AAA, BBB, CCC", instruction.Lookups.DeclarationTypeList.CodesAsString);
			AssertSame(instruction.Lookups.DeclarationTypeList, instruction.Lookups.DeclarationTypeList);
		}

		public void TestDeclarationTypeLookup()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;  // Latvia for base EU tests.
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(currentCountry, "A", "11", "11", "111", "One", "IMP", group: "AAA,BBB");
			helper.CreateRefCusProcedure(currentCountry, "A", "22", "22", "222", "Two", "IMP", group: "BBB,CCC");
			helper.CreateRefCusProcedure(currentCountry, "B", "33", "33", "333", "Three", "IMP", group: "AAA");
			helper.CreateRefCusProcedure(currentCountry, "A", "44", "44", "444", "Four", "EXP", group: "XXX");
			helper.CreateRefCusProcedure(currentCountry, "A", "55", "55", "555", "Five", "EXP", group: "XXX;YYY,ZZZ");
			helper.CreateRefCusProcedure("ZA", "B", "33", "33", "333", "Three", "IMP", group: "IFW");
			helper.CreateRefCusProcedure("ZA", "A", "55", "55", "555", "Five", "EXP", group: "ESD");

			var dec = Factory.New<JobDeclaration>();
			var cei = dec.CustomsEntryInstructions.AddNew();
			dec.JE_MessageType = "IMP";
			AssertEquals(3, cei.Lookups.DeclarationTypeList.Count);
			AssertEquals(GetExpectedDescriptionForDeclarationType("AAA"), cei.Lookups.DeclarationTypeList.GetDescriptionFromCode("AAA"));
			AssertEquals(GetExpectedDescriptionForDeclarationType("BBB"), cei.Lookups.DeclarationTypeList.GetDescriptionFromCode("BBB"));
			AssertEquals(GetExpectedDescriptionForDeclarationType("CCC"), cei.Lookups.DeclarationTypeList.GetDescriptionFromCode("CCC"));
			dec.JE_MessageType = "EXP";
			AssertEquals(3, cei.Lookups.DeclarationTypeList.Count);
			AssertEquals(GetExpectedDescriptionForDeclarationType("XXX"), cei.Lookups.DeclarationTypeList.GetDescriptionFromCode("XXX"));
			AssertEquals(GetExpectedDescriptionForDeclarationType("YYY"), cei.Lookups.DeclarationTypeList.GetDescriptionFromCode("YYY"));
			AssertEquals(GetExpectedDescriptionForDeclarationType("ZZZ"), cei.Lookups.DeclarationTypeList.GetDescriptionFromCode("ZZZ"));
		}

		protected virtual string GetExpectedDescriptionForDeclarationType(string code)
		{
			return code;
		}

		public void TestStyleList()
		{
			var dec = Factory.New<JobDeclaration>();
			var cei = dec.CustomsEntryInstructions.AddNew();
			var ceiLookups = cei.Lookups;
			AssertSame("The 'StyleList' override is required such that the descriptions can be defaulted from the code (base functionality)", ceiLookups.DeclarationTypeList, ceiLookups.StyleList);
		}
	}
}

