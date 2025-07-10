using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class CommonPreviousDocumentArrivalValidationDeciderTest : TestCase
{
	public void TestIsRuleG0321Active() => AssertEquals(false, validationDecider.IsRuleG0321Active);

	public void TestIsRuleTR0030_1Active() => AssertEquals(false, validationDecider.IsRuleTR0030_1Active);

	protected override void SetUp()
	{
		validationDecider = new();
	}

	CommonPreviousDocumentArrivalValidationDecider validationDecider;
}
