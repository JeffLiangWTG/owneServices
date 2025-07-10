using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class NctsDepartureMovementHeaderPhase5ValidationDeciderTest : TestCase
{
	public void TestIsRuleB1806Active()
	{
		AssertEquals(false, validationDecider.IsRuleB1806Active);
	}

	public void TestIsRuleB1836Active()
	{
		AssertEquals(true, validationDecider.IsRuleB1836Active);
	}

	public void TestIsRuleB1848_1Active()
	{
		AssertEquals(false, validationDecider.IsRuleB1848_1Active);
	}

	public void TestIsRuleB1838Active()
	{
		AssertEquals(true, validationDecider.IsRuleB1838Active);
	}

	public void TestIsRuleB1848Active()
	{
		AssertEquals(false, validationDecider.IsRuleB1848Active);
	}

	public void TestIsRuleB1850Active()
	{
		AssertEquals(true, validationDecider.IsRuleB1850Active);
	}

	public void TestIsRuleB1858Active()
	{
		AssertEquals(false, validationDecider.IsRuleB1858Active);
	}

	public void TestIsRuleB1858_1Active()
	{
		AssertEquals(false, validationDecider.IsRuleB1858_1Active);
	}

	public void TestIsRuleB1889Active()
	{
		AssertEquals(false, validationDecider.IsRuleB1889Active);
	}

	public void TestIsRuleB1891_1Active()
	{
		AssertEquals(true, validationDecider.IsRuleB1891_1Active);
	}

	public void TestIsRuleB1892Active()
	{
		AssertEquals(true, validationDecider.IsRuleB1892Active);
	}

	public void TestIsRuleB1893Active()
	{
		AssertEquals(true, validationDecider.IsRuleB1893Active);
	}

	public void TestIsRuleB1893_1Active()
	{
		AssertEquals(false, validationDecider.IsRuleB1893_1Active);
	}

	public void TestIsRuleB1893_2Active()
	{
		AssertEquals(false, validationDecider.IsRuleB1893_2Active);
	}

	public void TestIsRuleB1897Active()
	{
		AssertEquals(false, validationDecider.IsRuleB1897Active);
	}

	public void TestIsRuleB1922Active()
	{
		AssertEquals(false, validationDecider.IsRuleB1922Active);
	}

	public void TestIsRuleB2101Active()
	{
		AssertEquals(false, validationDecider.IsRuleB2101Active);
	}

	public void TestIsRuleBR5410Active()
	{
		AssertEquals(false, validationDecider.IsRuleBR5410Active);
	}

	public void TestIsRuleC0030Active()
	{
		AssertEquals(ruleConfiguration.IsRuleC0030Active, validationDecider.IsRuleC0030Active);
	}

	public void TestIsRuleC0101_1Active()
	{
		AssertEquals(true, validationDecider.IsRuleC0101_1Active);
	}

	public void TestIsRuleC0191Active()
	{
		AssertEquals(true, validationDecider.IsRuleC0191Active);
	}

	public void TestsRuleC0343Active()
	{
		AssertEquals(true, validationDecider.IsRuleC0343Active);
	}

	public void TestsRuleC0343_2Active()
	{
		AssertEquals(false, validationDecider.IsRuleC0343_2Active);
	}

	public void TestsRuleC0191_1Active()
	{
		AssertEquals(false, validationDecider.IsRuleC0191_1Active);
	}

	public void TestsRuleC0191_2Active()
	{
		AssertEquals(false, validationDecider.IsRuleC0191_2Active);
	}

	public void TestIsRuleC035Active()
	{
		AssertEquals(true, validationDecider.IsRuleC035Active);
	}

	public void TestsRuleC0337_2Active()
	{
		AssertEquals(false, validationDecider.IsRuleC0337_2Active);
	}

	public void TestIsRuleC0387Active()
	{
		AssertEquals(true, validationDecider.IsRuleC0387Active);
	}

	public void TestIsRuleC0403Active()
	{
		AssertEquals(true, validationDecider.IsRuleC0403Active);
	}

	public void TestIsRuleC0403_1Active()
	{
		AssertEquals(false, validationDecider.IsRuleC0403_1Active);
	}

	public void TestIsRuleC0531Active()
	{
		AssertEquals(true, validationDecider.IsRuleC0531Active);
	}

	public void TestIsRuleC0586Active()
	{
		AssertEquals(true, validationDecider.IsRuleC0586Active);
	}

	public void TestIsRuleC0599Active()
	{
		AssertEquals(true, validationDecider.IsRuleC0599Active);
	}

	public void TestIsRuleC0599_1Active()
	{
		AssertEquals(false, validationDecider.IsRuleC0599_1Active);
	}

	public void TestIsRuleC0599_2Active()
	{
		AssertEquals(false, validationDecider.IsRuleC0599_2Active);
	}

	public void TestIsRuleC0710Active()
	{
		AssertEquals(false, validationDecider.IsRuleC0710Active);
	}

	public void TestIsRuleC0806Active()
	{
		AssertEquals(true, validationDecider.IsRuleC0806Active);
	}

	public void TestIsRuleC0839_1Active()
	{
		AssertEquals(false, validationDecider.IsRuleC0839_1Active);
	}

	public void TestIsRuleC0909Active()
	{
		AssertEquals(true, validationDecider.IsRuleC0909Active);
	}

	public void TestIsRuleC191Active()
	{
		AssertEquals(false, validationDecider.IsRuleC191Active);
	}

	public void TestIsRuleC547Active()
	{
		AssertEquals(true, validationDecider.IsRuleC547Active);
	}

	public void TestIsRuleC589Active()
	{
		AssertEquals(true, validationDecider.IsRuleC589Active);
	}

	public void TestIsRuleC599Active()
	{
		AssertEquals(false, validationDecider.IsRuleC599Active);
	}

	public void TestIsRuleCN839Active()
	{
		AssertEquals(false, validationDecider.IsRuleCN839Active);
	}

	public void TestIsRuleE1103Active()
	{
		AssertEquals(true, validationDecider.IsRuleE1103Active);
	}

	public void TestIsRuleE1109Active()
	{
		AssertEquals(true, validationDecider.IsRuleE1109Active);
	}

	public void TestIsRuleE1301Active()
	{
		AssertEquals(false, validationDecider.IsRuleE1301Active);
	}

	public void TestIsRuleG0114Active()
	{
		AssertEquals(true, validationDecider.IsRuleG0114Active);
	}

	public void TestIsRuleIsRuleG0789_1Active()
	{
		AssertEquals(false, validationDecider.IsRuleG0789_1Active);
	}

	public void TestIsRuleN0002Active()
	{
		AssertEquals(false, validationDecider.IsRuleN0002Active);
	}

	public void TestIsRuleNR0007Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0007Active);
	}

	public void TestIsRuleNR0018Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0018Active);
	}

	public void TestIsRuleNR0031Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0031Active);
	}

	public void TestIsRuleNR0035Active()
	{
		AssertEquals(expected: false, validationDecider.IsRuleNR0035Active);
	}

	public void TestIsRuleNR0036Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0036Active);
	}

	public void TestIsRuleNR0037Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0037Active);
	}

	public void TestIsRuleNR0038Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0038Active);
	}

	public void TestIsRuleNR0039Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0039Active);
	}

	public void TestIsRuleNR0056Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0056Active);
	}

	public void TestIsRuleNR0070Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0070Active);
	}

	public void TestIsRuleNR0072Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0072Active);
	}

	public void TestIsRuleNR0073Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0073Active);
	}

	public void TestIsRuleNR0088Active()
	{
		AssertEquals(false, validationDecider.IsRuleNR0088Active);
	}

	public void TestIsRuleR0473Active()
	{
		AssertEquals(true, validationDecider.IsRuleR0473Active);
	}

	public void TestIsRuleR0473_1Active()
	{
		AssertEquals(false, validationDecider.IsRuleR0473_1Active);
	}

	public void TestIsRuleR0909Active()
	{
		AssertEquals(true, validationDecider.IsRuleR0909Active);
	}

	public void TestIsRuleR902Active()
	{
		AssertEquals(true, validationDecider.IsRuleR902Active);
	}

	public void TestIsRuleR903Active()
	{
		AssertEquals(true, validationDecider.IsRuleR903Active);
	}

	public void TestIsRuleR911Active()
	{
		AssertEquals(true, validationDecider.IsRuleR911Active);
	}

	public void TestIsRuleR0911Active()
	{
		AssertEquals(false, validationDecider.IsRuleR0911Active);
	}

	public void TestIsRulePLR0601Active()
	{
		AssertEquals(ruleConfiguration.IsRulePLR0601Active, validationDecider.IsRulePLR0601Active);
	}

	public void TestIsRuleR0350Active()
	{
		AssertEquals(ruleConfiguration.IsRuleR0350Active, validationDecider.IsRuleR0350Active);
	}

	public void TestIsRuleR0789Active()
	{
		AssertEquals(true, validationDecider.IsRuleR0789Active);
	}

	public void TestIsRuleR0789_1Active()
	{
		AssertEquals(false, validationDecider.IsRuleR0789_1Active);
	}

	public void TestIsRuleR0850Active()
	{
		AssertEquals(ruleConfiguration.IsRuleR0850Active, validationDecider.IsRuleR0850Active);
	}

	public void TestIsRuleR0900_4Active()
	{
		AssertEquals(false, validationDecider.IsRuleR0900_4Active);
	}

	public void TestIsRuleR0990Active()
	{
		AssertEquals(false, validationDecider.IsRuleR0990Active);
	}

	public void TestIsRuleR0994Active()
	{
		AssertEquals(false, validationDecider.IsRuleR0994Active);
	}

	public void TestIsRuleR0994_1Active()
	{
		AssertEquals(false, validationDecider.IsRuleR0994_1Active);
	}

	public void TestIsRuleR0601_1Active()
	{
		AssertEquals(false, validationDecider.IsRuleR0601_1Active);
	}

	public void TestIsRuleR0020Active()
	{
		AssertEquals(true, validationDecider.IsRuleR0020Active);
	}

	public void TestIsRuleR0020_1Active()
	{
		AssertEquals(false, validationDecider.IsRuleR0020_1Active);
	}

	public void TestIsInBondEntryTypeListValidationActive()
	{
		AssertEquals(true, validationDecider.IsInBondEntryTypeListValidationActive);
	}

	public void TestIsRuleTR0017Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0017Active);
	}

	public void TestIsRuleTR0048Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0048Active);
	}

	public void TestIsRuleTR0049Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0049Active);
	}

	public void TestIsRuleTR0050Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0050Active);
	}

	public void TestIsRuleTR0051Active()
	{
		AssertEquals(false, validationDecider.IsRuleTR0051Active);
	}

	public void TestIsRuleTR0053Active()
	{
		AssertEquals(false, validationDecider.IsRuleTR0053Active);
	}

	public void TestIsRuleTR0054Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0054Active);
	}

	public void TestIsRuleTR0086Active()
	{
		AssertEquals(expected: true, validationDecider.IsRuleTR0086Active);
	}

	public void TestIsRuleTR0092Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0092Active);
	}

	protected override void SetUp()
	{
		base.SetUp();
		validationDecider = new NctsDepartureMovementHeaderPhase5ValidationDecider();
		ruleConfiguration = new ValidationRuleConfiguration();
	}

	ValidationRuleConfiguration ruleConfiguration;
	NctsDepartureMovementHeaderPhase5ValidationDecider validationDecider;
}
