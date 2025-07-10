using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

sealed class NctsHeaderDeparturePhase5ValidationDeciderTest : TestCase
{
	public void TestIsRuleB1823Active() => AssertEquals(false, validationDecider.IsRuleB1823Active);
	public void TestIsRuleC0001Active() => AssertEquals(false, validationDecider.IsRuleC0001Active);
	public void TestIsRuleC0001_1Active() => AssertEquals(false, validationDecider.IsRuleC0001_1Active);
	public void TestIsRuleC0001_4Active() => AssertEquals(false, validationDecider.IsRuleC0001_4Active);

	public void TestIsRuleC0001_6Active() => AssertEquals(false, validationDecider.IsRuleC0001_6Active);

	public void TestIsRuleC0050Active() => AssertEquals(false, validationDecider.IsRuleC0050Active);

	public void TestIsRuleTR0087Active() => AssertEquals(true, (validationDecider as IRuleTR0087Decider).IsActive);

	public void TestIsRuleG0001_1Active()
	{
		AssertEquals(false, validationDecider.IsRuleG0001_1Active);
	}

	public void TestIsRuleNR0068Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0068Active);
	}
	
	public void TestIsRuleNR0069Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0069Active);
	}

	public void TestIsRuleNR0071Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0071Active);
	}

	public void TestIsRuleNR0074Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0074Active);
	}

	public void TestIsRuleTR0079Active()
	{
		AssertEquals(expected: true, validationDecider.IsRuleTR0079Active);
	}

	protected override void SetUp()
	{
		base.SetUp();
		validationDecider = new NctsHeaderDeparturePhase5ValidationDecider();
	}

	NctsHeaderDeparturePhase5ValidationDecider validationDecider;
}
