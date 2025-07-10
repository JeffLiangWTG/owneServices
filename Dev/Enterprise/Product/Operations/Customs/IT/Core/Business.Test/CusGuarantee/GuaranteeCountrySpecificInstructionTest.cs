using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class GuaranteeCountrySpecificInstructionTest : TestCaseWithFactory
{
	public void TestGetTypeList()
	{
		var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
		var typeList = guaranteeHeader.CountrySpecificInstruction.GetTypeList(Core.Constants.CountryCodes.Italy);
		AssertContainsExactElementsInAnyOrder("Type List Codes", new ZString[] { "COD", "IMP", "TRA" }, typeList.GetAllCodes());
	}

	public void TestGetSubTypeListForImportGuarantee()
	{
		var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
		var subTypeList = guaranteeHeader.CountrySpecificInstruction.GetSubTypeList(EUGuaranteeTypeList.Codes.IMP);
		AssertType<ImportGuaranteeSubTypeList>("SubTypeList Type", subTypeList);
	}

	public void TestGetSubTypeListForTransitGuarantee()
	{
		var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
		var subTypeList = guaranteeHeader.CountrySpecificInstruction.GetSubTypeList(EUGuaranteeTypeList.Codes.TRA);
		AssertType<EUNctsGuaranteeTypeList>("SubTypeList Type", subTypeList);
	}
}
