using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.GB.Business.Testing
{
	class GuaranteeCountrySpecificInstructionTest : TestCaseWithFactory
	{
		public void TestGetTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "Guarantee Types");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "ZZZ", "Description for ZZZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var list = instruction.GetTypeList(Core.Constants.CountryCodes.UnitedKingdom);
			AssertCodeDescriptionPairList(list,
				("COD", "Ongoing/comprehensive guarantee"),
				("COM", "Comprehensive"),
				("GEN", "General"),
				("IMP", "Import"),
				("TRA", "Transit"),
				("ZZZ", "Description for ZZZ")
			);
		}

		protected override void SetUp()
		{
			base.SetUp();
			instruction = new GuaranteeCountrySpecificInstruction(Factory);
		}

		GuaranteeCountrySpecificInstruction instruction;
	}
}
