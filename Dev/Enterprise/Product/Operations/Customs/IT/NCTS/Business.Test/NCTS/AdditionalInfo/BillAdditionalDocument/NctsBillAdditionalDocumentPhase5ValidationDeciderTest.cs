using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

sealed class NctsBillAdditionalDocumentPhase5ValidationDeciderTest : TestCase
{
	public void TestIsRuleE1301Active()
	{
		AssertEquals(true, validationDecider.IsRuleE1301Active);
	}

	public void TestIsRuleG0321Active()
	{
		AssertEquals(false, validationDecider.IsRuleG0321Active);
	}

	public void TestIsRuleR3062Active()
	{
		AssertEquals(true, validationDecider.IsRuleR3062Active);
	}

	public void TestIsRuleTR0031Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0031Active);
	}

	public void TestIsRuleTR0032Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0032Active);
	}

	public void TestIsRuleTR0033Active()
	{
		AssertEquals(true, validationDecider.IsRuleTR0033Active);
	}

	public void TestIsRuleTR0062Active()
	{
		AssertEquals(false, validationDecider.IsRuleTR0062Active);
	}

	protected override void SetUp()
	{
		base.SetUp();
		validationDecider = new NctsBillAdditionalDocumentPhase5ValidationDecider();
	}

	NctsBillAdditionalDocumentPhase5ValidationDecider validationDecider;
}
