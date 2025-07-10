using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class GuidedDecisionMakingTabStatusTest : TestCaseWithFactory
	{
		public void TestEnum()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Completed", nameof(GuidedDecisionMakingTabStatus.Completed));
				AssertEquals("NotApplicable", nameof(GuidedDecisionMakingTabStatus.NotApplicable));
				AssertEquals("Incomplete", nameof(GuidedDecisionMakingTabStatus.Incomplete));
				AssertEquals("Current", nameof(GuidedDecisionMakingTabStatus.Current));
				AssertEquals("CompletedWithWarning", nameof(GuidedDecisionMakingTabStatus.CompletedWithWarning));
			});
		}
	}
}
