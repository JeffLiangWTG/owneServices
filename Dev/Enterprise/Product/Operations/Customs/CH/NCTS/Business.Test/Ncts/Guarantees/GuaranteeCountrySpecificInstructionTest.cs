using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

class GuaranteeCountrySpecificInstructionTest : TestCaseWithFactory
{
	public void TestTypeList()
	{
		AssertEquals(GuaranteeTypeList.Codes.TRA, GuaranteeCountrySpecificInstruction.GetTypeList(Core.Constants.CountryCodes.Switzerland).CodesAsString);
	}

	public void TestSubTypeList() => CombineAssertions(() =>
	{
		new RefDataTestHelper(Factory).CreateNctsBondTypeList();

		AssertEquals("For Type=TRA", "0, 1, 2, 3", GuaranteeCountrySpecificInstruction.GetSubTypeList(GuaranteeTypeList.Codes.TRA).CodesAsString);
		AssertEquals("For Type<>TRA", 0, GuaranteeCountrySpecificInstruction.GetSubTypeList("XXX").Count);
	});

	GuaranteeCountrySpecificInstruction GuaranteeCountrySpecificInstruction => guaranteeCountrySpecificInstruction ?? (guaranteeCountrySpecificInstruction = new GuaranteeCountrySpecificInstruction(Factory));
	GuaranteeCountrySpecificInstruction guaranteeCountrySpecificInstruction;
}
