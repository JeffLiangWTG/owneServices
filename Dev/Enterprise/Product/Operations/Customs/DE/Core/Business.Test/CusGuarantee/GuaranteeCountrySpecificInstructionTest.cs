using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.DE.Business.Testing
{
	class GuaranteeCountrySpecificInstructionTest : TestCaseWithFactory
	{
		public void TestGetSubTypeList()
		{
			CombineAssertions(() =>
			{
				var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
				var subTypeList = guaranteeHeader.CountrySpecificInstruction.GetSubTypeList(EUGuaranteeTypeList.Codes.TRA);
				AssertEquals("Guarantee Type is TRA", "0, 1, 2, 4, 6", subTypeList.CodesAsString);

				subTypeList = guaranteeHeader.CountrySpecificInstruction.GetSubTypeList("COM");
				AssertEquals("Guarantee Type isn't TRA", 0, subTypeList.Count);
			});
		}
	}
}
