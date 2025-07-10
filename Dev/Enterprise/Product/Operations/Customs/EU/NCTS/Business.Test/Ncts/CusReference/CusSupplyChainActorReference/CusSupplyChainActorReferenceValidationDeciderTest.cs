using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class CusSupplyChainActorReferenceValidationDeciderTest : TestCase
{
	public void TestIsRuleR0840Active() => AssertEquals(true, validationDecider.IsRuleR0840Active);

	protected override void SetUp()
	{
		base.SetUp();
		validationDecider = new CusSupplyChainActorReferenceValidationDecider();
	}

	CusSupplyChainActorReferenceValidationDecider validationDecider;
}
