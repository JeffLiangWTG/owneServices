using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class UCC6ImportPackageValidationDeciderTest : TestCaseWithFactory
{
	public void TestIsRuleC0820ActiveForCW_MarksAndNos()
	{
		AssertEquals(false, decider.IsRuleC0820ActiveForCW_MarksAndNos);
	}

	protected override void SetUp()
	{
		base.SetUp();

		decider = new UCC6ImportPackageValidationDecider();
	}
	UCC6ImportPackageValidationDecider decider;
}
