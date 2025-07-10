using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business.Testing
{
	class CusEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCEI_SubStyleAndLookups()
		{
			var dec = Factory.New<JobDeclaration>();
			var instruction = Factory.New<CusEntryInstruction>();
			instruction.CEI_JE = dec.PK;
			instruction.CEI_SubStyle = "X";
			AssertNotNull(instruction.Lookups.EntrySubStyleList);
			AssertType<CodeDescriptionPairList>(instruction.Lookups.EntrySubStyleList);
		}

		public void TestJustificationContactDetailOrganisations()
		{
			var dec = Factory.New<JobDeclaration>();
			var instruction = dec.CustomsEntryInstructions.AddNew();
			AssertNotNull(instruction.Lookups.JustificationContactDetailOrganisations);
			AssertType<OrgHeaderCollection>(instruction.Lookups.JustificationContactDetailOrganisations);
		}

		public void TestAdditionalInformationOptions()
		{
			var dec = Factory.New<JobDeclaration>();
			var instruction = dec.CustomsEntryInstructions.AddNew();
			var list = instruction.Lookups.AdditionalInformationOptions;
			AssertSame(Factory.GetCachedValue<AdditionalInformationOptions>(), list);
			AssertEquals("1, 2, 3, 4", list.CodesAsString);
		}

		public void TestBillTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			var instruction = dec.CustomsEntryInstructions.AddNew();
			var lookups = instruction.Lookups;
			var list = lookups.BillTypeList;
			AssertType<BillTypeList>(list);
			AssertSame(list, lookups.BillTypeList);
			AssertContainsExactElementsInAnyOrder(
				new string[]
				{
					BillTypeList.Codes.AWB,
					BillTypeList.Codes.Barcode,
					BillTypeList.Codes.CRT,
					BillTypeList.Codes.DSIC,
					BillTypeList.Codes.HAWB,
					BillTypeList.Codes.HBL,
					BillTypeList.Codes.HRWB,
					BillTypeList.Codes.RWB,
					BillTypeList.Codes.TIFDTA,
					BillTypeList.Codes.UCR
				}, lookups.BillTypeList.GetAllCodes());
		}

		public void TestSpecialClearanceList()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			var list = instruction.Lookups.SpecialCustomsClearanceList;
			AssertSame(Factory.GetCachedValue<SpecialCustomsClearanceList>(), list);
			AssertEquals("2001, 2002, 2003", list.CodesAsString);
		}

		public void TestLegalDocumentList()
		{
			var instruction = Factory.New<CusEntryInstruction>();
			var list = instruction.Lookups.LegalDocumentList;
			AssertSame(Factory.GetCachedValue<LegalDocumentList>(), list);
			AssertEquals("NFE, SNF", list.CodesAsString);
		}

		public void TestDetailWithoutLegalDocList()
		{
			var oInstruction = Factory.New<CusEntryInstruction>();
			var list = oInstruction.Lookups.DetailWithoutLegalDocList;
			AssertSame(Factory.GetCachedValue<DetailWithoutLegalDocList>(), list);
			AssertEquals("3001, 3002, 3003, 3004, 3005, 3006, 3007, 3008, 3009, 3010, 3011, 3012, 3013, 3014, 3015, 3016, 3017, 3018, 3019, 3020, 3021, 3022, 3023, 3024, 3025, 3026", list.CodesAsString);
		}

		public void TestAFRMMMethodOfCalculationList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			ReferenceTestDataHelper.CreateAfrmmTaxes(Factory);

			var instruction = Factory.New<CusEntryInstruction>();
			var list = instruction.Lookups.AFRMMMethodOfCalculationList;
			AssertEquals("FMM1, FMM4", list.CodesAsString);
			AssertNotContains("TUSM", list.CodesAsString);
		}
	}
}
