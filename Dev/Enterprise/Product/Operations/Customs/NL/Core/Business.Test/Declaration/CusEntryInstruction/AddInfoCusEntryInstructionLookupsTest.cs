using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.NL.Business.Declaration.Testing
{
	public class AddInfoCusEntryInstructionLookupsTest : EU.Business.Declaration.Testing.AddInfoCusEntryInstructionLookupsTest
	{
		public void TestTransNatureList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Netherlands, "Netherlands", eun);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "AAA", "AAA DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TranNature, "BBB", "AAA DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();

			AssertArrayEqualsByElements("Should match the expected list for NL.", new[] { "AAA" }, cusEntryInstruction.AddInfoLookups.TransNatureList.GetAllCodes());
		}
	}
}
