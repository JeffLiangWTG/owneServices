using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(GuaranteeForEntryInstructionLookups))]
	sealed class GuaranteeForEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestIEBondTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "Guarantee Types");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "ZZZ", "Description for ZZZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "IMP", "Description for IMP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var guaranteeForEntryInstruction = Factory.New<GuaranteeForEntryInstruction>();
			var typeList = guaranteeForEntryInstruction.Lookups.IEBondTypeList;

			CombineAssertions(() =>
			{
				AssertEquals("List must contain 2 elements", 2, typeList.Count);
				Assert("List must cotain code IMP", typeList.ContainsCode("IMP"));
				Assert("List must cotain code ZZZ", typeList.ContainsCode("ZZZ"));
			});
		}

		public void TestEuropeanUnionCountryList()
		{
			var guaranteeForEntryInstruction = Factory.New<GuaranteeForEntryInstruction>();

			AssertEquals("All EU Countries and GB","AT, BE, BG, CY, CZ, DE, DK, EE, EL, ES, FI, FR, GB, HR, HU, IE, IT, LT, LU, LV, MT, NL, PL, PT, RO, SE, SI, SK", guaranteeForEntryInstruction.Lookups.EuropeanUnionCountryList.CodesAsString);
		}
	}
}
