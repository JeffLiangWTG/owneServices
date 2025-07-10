using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CusGuaranteeHeader))]
class CusGuaranteeHeaderTest : EU.Business.Testing.CusGuaranteeHeaderAbstractTest
{
	public void TestValidation()
	{
		AssertType<CusGuaranteeHeaderValidation>(guaranteeHeader.Validation);
	}

	public void TestCusGuaranteeRulesType()
	{
		AssertType(typeof(CusGuaranteeRuleCollection<CusGuaranteeRule>), guaranteeHeader.CusGuaranteeRules);
	}

	public void TestAdditionalAccessCodesType()
	{
		AssertType(typeof(CusGuaranteeRuleCollection<CusGuaranteeRule>), guaranteeHeader.AdditionalAccessCodes);
	}

	public void TestCountrySpecificInstruction()
	{
		AssertType<GuaranteeCountrySpecificInstruction>(guaranteeHeader.CountrySpecificInstruction);
	}

	protected override void SetUp()
	{
		base.SetUp();
		guaranteeHeader = Factory.New<CusGuaranteeHeader>();
	}
	CusGuaranteeHeader guaranteeHeader;
}
