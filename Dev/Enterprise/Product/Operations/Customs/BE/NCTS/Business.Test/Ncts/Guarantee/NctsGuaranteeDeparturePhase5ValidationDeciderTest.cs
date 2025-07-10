using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

sealed class NctsGuaranteeDeparturePhase5ValidationDeciderTest : TestCase
{
	public void TestIsRuleB1898_1ActiveForPW_RX_NKCurrency()
	{
		AssertEquals(false, validationDecider.IsRuleB1898_1ActiveForPW_RX_NKCurrency);
	}

	public void TestIsRuleB2101Active()
	{
		AssertEquals(false, validationDecider.IsRuleB2101Active);
	}

	public void TestIsRuleC0085Active()
	{
		AssertEquals(true, validationDecider.IsRuleC0085Active);
	}

	public void TestIsRuleC0085_1Active()
	{
		AssertEquals(false, validationDecider.IsRuleC0085_1Active);
	}

	public void TestIsRuleC0085_2Active()
	{
		AssertEquals(false, validationDecider.IsRuleC0085_2Active);
	}

	public void TestIsRuleC0086Active()
	{
		AssertEquals(true, validationDecider.IsRuleC0086Active);
	}

	public void TestIsRuleC0086_1Active()
	{
		AssertEquals(false, validationDecider.IsRuleC0086_1Active);
	}

	public void TestIsRuleC0130Active()
	{
		AssertEquals(true, validationDecider.IsRuleC0130Active);
	}

	public void TestIsRuleNR0005Active()
	{
		AssertEquals(true, validationDecider.IsRuleNR0005Active);
	}

	public void TestIsRuleNR0014Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0014Active);
	}

	public void TestIsRuleR0318Active()
	{
		AssertEquals(true, validationDecider.IsRuleR0318Active);
	}

	public void TestIsRuleNR0064Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0064Active);
	}

	public void TestIsRuleNR0065Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0065Active);
	}

	public void TestIsRuleR0900Active()
	{
		AssertEquals(true, validationDecider.IsRuleR0900Active);
	}

	public void TestIsRuleR0900_1Active()
	{
		AssertEquals(false, validationDecider.IsRuleR0900_1Active);
	}

	public void TestIsRuleR0900_2Active()
	{
		AssertEquals(false, validationDecider.IsRuleR0900_2Active);
	}

	public void TestIsRuleR0900_3Active()
	{
		AssertEquals(false, validationDecider.IsRuleR0900_3Active);
	}

	public void TestIsRuleTR0019Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0019Active);
	}

	public void TestIsRuleTR0065Active()
	{
		AssertEquals(false, validationDecider.IsRuleTR0065Active);
	}

	public void TestIsRuleTR0093Active()
	{
		AssertEquals(false, validationDecider.IsRuleTR0093Active);
	}

	public void TestIsRuleTR0096Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0096Active);
	}

	protected override void SetUp()
	{
		base.SetUp();
		validationDecider = new NctsGuaranteeDeparturePhase5ValidationDecider();
	}

	NctsGuaranteeDeparturePhase5ValidationDecider validationDecider;
}
