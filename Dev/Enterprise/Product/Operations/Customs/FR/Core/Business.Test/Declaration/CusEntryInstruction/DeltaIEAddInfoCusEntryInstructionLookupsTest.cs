using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class DeltaIEAddInfoCusEntryInstructionLookupsTest : TestCaseWithFactory
	{
		public void TestTransNatureList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, "Delta IE", euGrouping);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "Tran Nature");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("IsImport", "Is For Import", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "DIE");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("IsExport", "Is For Export", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "DIE");

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "TRN00", "TRN00", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var importCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "TRN01", "TRN01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			importCode.Attributes.AddNew("IsImport", Core.Constants.BooleanTrueString);

			var exportCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "TRN02", "TRN02", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			exportCode.Attributes.AddNew("IsExport", Core.Constants.BooleanTrueString);

			var omniDirectionalCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.DeltaIE, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "TRN03", "TRN032", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			omniDirectionalCode.Attributes.AddNew("IsImport", Core.Constants.BooleanTrueString);
			omniDirectionalCode.Attributes.AddNew("IsExport", Core.Constants.BooleanTrueString);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var lookups = cusEntryInstruction.AddInfoLookups;

			var list = lookups.TransNatureList;
			CombineAssertions("List should contain the Tran Natures with attributes IsImport ='Y' when declaration direction is Import.", () =>
			{
				AssertEquals(2, list.Count);
				Assert(list.ContainsCode("TRN01"));
				Assert(list.ContainsCode("TRN03"));
			});

			declaration.JE_MessageType = "EXP";
			list = lookups.TransNatureList;
			CombineAssertions("List should contain the Tran Natures with attributes IsExport ='Y' when declaration direction is Export.", () =>
			{
				AssertEquals(2, list.Count);
				Assert(list.ContainsCode("TRN02"));
				Assert(list.ContainsCode("TRN03"));
			});
		}
	}
}

