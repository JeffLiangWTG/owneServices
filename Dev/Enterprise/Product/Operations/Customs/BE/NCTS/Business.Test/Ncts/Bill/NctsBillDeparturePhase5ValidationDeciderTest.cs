using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

sealed class NctsBillDeparturePhase5ValidationDeciderTest : TestCase
{
	public void TestIsRuleB1895_1Active()
	{
		AssertEquals(false, validationDecider.IsRuleB1895_1Active);
	}

	public void TestIsRuleB1896Active()
	{
		AssertEquals(false, validationDecider.IsRuleB1896Active);
	}

	public void TestIsRuleB1964Active()
	{
		AssertEquals(false, validationDecider.IsRuleB1964Active);
	}

	public void TestIsRuleC0001_4Active()
	{
		AssertEquals(false, validationDecider.IsRuleC0001_4Active);
	}

	public void TestIsRuleC0001_6Active()
	{
		AssertEquals(false, validationDecider.IsRuleC0001_6Active);
	}

	public void TestIsRuleC0001_7Active()
	{
		AssertEquals(false, validationDecider.IsRuleC0001_7Active);
	}

	public void TestIsRuleC0343_2Active()
	{
		AssertEquals(false, validationDecider.IsRuleC0343_2Active);
	}

	public void TestIsRuleC0502Active()
	{
		AssertEquals(true, validationDecider.IsRuleC0502Active);
	}

	public void TestIsRuleE1301Active()
	{
		AssertEquals(false, validationDecider.IsRuleE1301Active);
	}

	public void TestIsRuleG0001_1Active()
	{
		AssertEquals(false, validationDecider.IsRuleG0001_1Active);
	}

	public void TestIsRuleG0026_1Active()
	{
		AssertEquals(false, validationDecider.IsRuleG0026_1Active);
	}

	public void TestIsRuleC0909Active()
	{
		AssertEquals(true, validationDecider.IsRuleC0909Active);
	}

	public void TestIsRuleN0002Active()
	{
		AssertEquals(false, validationDecider.IsRuleN0002Active);
	}

	public void TestIsRuleNR0068Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0068Active);
	}

	public void TestIsRuleNR0069Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0069Active);
	}

	public void TestIsRuleNR0078Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0078Active);
	}

	public void TestIsRuleNR0079Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0079Active);
	}

	public void TestIsRuleR0221Active()
	{
		AssertEquals(true, validationDecider.IsRuleR0221Active);
	}

	public void TestIsRuleR0364()
	{
		AssertEquals(expected: true, validationDecider.IsRuleR0364Active);
	}

	public void TestIsRuleR0474Active()
	{
		AssertEquals(false, validationDecider.IsRuleR0474Active);
	}

	public void TestIsRuleR0474_1Active()
	{
		AssertEquals(false, validationDecider.IsRuleR0474_1Active);
	}

	public void TestIsRuleR0506Active()
	{
		AssertEquals(true, validationDecider.IsRuleR0506Active);
	}

	public void TestIsRuleR0983Active()
	{
		AssertEquals(true, validationDecider.IsRuleR0983Active);
	}

	public void TestIsRuleR0983_1Active()
	{
		AssertEquals(false, validationDecider.IsRuleR0983_1Active);
	}

	public void TestIsRuleTR0057Active()
	{
		AssertEquals(expected: true, validationDecider.IsRuleTR0057Active);
	}

	public void TestIsRuleTR0058Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0058Active);
	}

	public void TestIsRuleTR0059Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0059Active);
	}

	public void TestIsRuleTR0077Active()
	{
		AssertEquals(false, validationDecider.IsRuleTR0077Active);
	}

	public void TestIsRuleTR0078Active()
	{
		AssertEquals(false, validationDecider.IsRuleTR0078Active);
	}

	public void TestIsRuleTR0094Active()
	{
		AssertEquals(false, validationDecider.IsRuleTR0094Active);
	}

	protected override void SetUp()
	{
	base.SetUp();
	validationDecider = new NctsBillDeparturePhase5ValidationDecider();
	}

	NctsBillDeparturePhase5ValidationDecider validationDecider;
}
