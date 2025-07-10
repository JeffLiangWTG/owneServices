using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

public sealed class CommonPreviousDocumentValidationDeciderTest : TestCase
{
	public void TestIsRuleG0321Active() => AssertEquals(false, validationDecider.IsRuleG0321Active);

	public void TestIsRuleTR0030_1Active() => AssertEquals(true, validationDecider.IsRuleTR0030_1Active);

	public void TestIsRuleE1301Active() => AssertEquals(false, validationDecider.IsRuleE1301Active);

	protected override void SetUp()
	{
		validationDecider = new();
	}

	CommonPreviousDocumentValidationDecider validationDecider;
}
