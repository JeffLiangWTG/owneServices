using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class AddInfoCusEntryInstructionLookupsTest : EU.Business.Declaration.Testing.AddInfoCusEntryInstructionLookupsTest
	{
		public void TestTransNatureList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France", eun);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "Tran Nature");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "IncoTerm Key");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("IsImport", "Is For Import", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "DIE");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("IsExport", "Is For Export", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "DIE");

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "000", "000 DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "AAA", "AAA DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "CCC", "CCC DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var importCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "TRN01", "TRN01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			importCode.Attributes.AddNew("IsImport", Core.Constants.BooleanTrueString);

			var exportCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "TRN02", "TRN02", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			exportCode.Attributes.AddNew("IsExport", Core.Constants.BooleanTrueString);

			var omniDirectionalCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "TRN03", "TRN032", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			omniDirectionalCode.Attributes.AddNew("IsImport", Core.Constants.BooleanTrueString);
			omniDirectionalCode.Attributes.AddNew("IsExport", Core.Constants.BooleanTrueString);
			Factory.Save();

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			var actualList = cusEntryInstruction.AddInfoLookups.TransNatureList.GetAllCodes();
			AssertArrayEqualsByElements("Should match the expected list for FR.", new[] { "AAA" }, actualList);

			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;

			var list = cusEntryInstruction.AddInfoLookups.TransNatureList;
			CombineAssertions("List should contain the Tran Natures with attributes IsImport ='Y' when declaration direction is Import.", () =>
			{
				AssertEquals(2, list.Count);
				Assert(list.ContainsCode("TRN01"));
				Assert(list.ContainsCode("TRN03"));
			});

			declaration.JE_MessageType = "EXP";
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			list = cusEntryInstruction.AddInfoLookups.TransNatureList;
			CombineAssertions("List should contain the Tran Natures with attributes IsExport ='Y' when declaration direction is Export.", () =>
			{
				AssertEquals(2, list.Count);
				Assert(list.ContainsCode("TRN02"));
				Assert(list.ContainsCode("TRN03"));
			});
		}

		public void TestValuationBypassCodeList()
		{
			CombineAssertions(() =>
			{
				var valuationBypassCodeList = lookups.ValuationBypassCodeList;
				AssertEquals("Values", "A, B, C, D, E, F, G, H, I, J, K, L, M", valuationBypassCodeList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<ValuationBypassCodeList>(), valuationBypassCodeList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			lookups = cusEntryInstruction.AddInfoLookups;
		}

		JobDeclaration declaration;
		CusEntryInstruction cusEntryInstruction;
		AddInfoCusEntryInstructionLookups lookups;
	}
}
