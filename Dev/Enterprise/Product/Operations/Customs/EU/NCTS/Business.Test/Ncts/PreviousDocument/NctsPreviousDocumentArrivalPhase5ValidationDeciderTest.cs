using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

public sealed class NctsPreviousDocumentArrivalPhase5ValidationDeciderTest : TestCase
{
	public void TestIsRuleG0321Active()
	{
		AssertEquals(false, validationDecider.IsRuleG0321Active);
	}

	protected override void SetUp()
	{
		validationDecider = new();
	}

	NctsPreviousDocumentArrivalPhase5ValidationDecider validationDecider;
}
