using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

public sealed class CommonPreviousDocumentArrivalValidationDeciderTest : TestCase
{
	public void TestIsRuleG0321Active() => AssertEquals(false, validationDecider.IsRuleG0321Active);

	public void TestIsRuleTR0030_1Active() => AssertEquals(true, validationDecider.IsRuleTR0030_1Active);

	protected override void SetUp()
	{
		validationDecider = new();
	}

	CommonPreviousDocumentArrivalValidationDecider validationDecider;
}
