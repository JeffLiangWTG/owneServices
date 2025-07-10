using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(CusGuaranteeHeader))]
sealed class CusGuaranteeHeaderTest : EU.Business.Testing.CusGuaranteeHeaderAbstractTest
{
	public void TestCountrySpecificInstruction()
	{
		var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
		AssertType<GuaranteeCountrySpecificInstruction>("CountrySpecificInstruction Type", guaranteeHeader.CountrySpecificInstruction);
	}
}
