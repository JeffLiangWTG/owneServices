using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	sealed class UCC6ImportPackageValidationDeciderTest : TestCaseWithFactory
	{
		public void TestIsRuleC0820ActiveForCW_MarksAndNos()
		{
			AssertEquals(true, decider.IsRuleC0820ActiveForCW_MarksAndNos);
		}

		protected override void SetUp()
		{
			base.SetUp();

			decider = new UCC6ImportPackageValidationDecider();
		}
		UCC6ImportPackageValidationDecider decider;
	}
}
